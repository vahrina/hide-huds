import argparse
import os
import re
import shutil
import subprocess
import sys

ROOT = os.path.dirname(os.path.abspath(__file__))
PROJECT = os.path.join(ROOT, "Dungeons")
UPROJECT = os.path.join(PROJECT, "Dungeons.uproject")
COOKED = os.path.join(PROJECT, "Saved", "Cooked", "Windows", "Dungeons", "Content", "Mods")
BUILD = os.path.join(ROOT, "Build")
RETOC = os.path.join(ROOT, "Tools", "retoc.exe")
REPAK = os.path.join(ROOT, "Tools", "repak.exe")
DEFAULT_ENGINE = r"C:\Program Files\Epic Games\UE_5.6"
STEAM = r"C:\Program Files (x86)\Steam"
XBOX_GAME = r"C:\XboxGames\Minecraft Dungeons II\Content"


def fail(message):
    print(f"Error: {message}")
    sys.exit(1)


def paks(game):
    return os.path.join(game, "Dungeons", "Content", "Paks")


def find_game():
    vdf = os.path.join(STEAM, "steamapps", "libraryfolders.vdf")
    libraries = re.findall(r'"path"\s+"([^"]+)"', open(vdf, encoding="utf-8").read()) if os.path.exists(vdf) else []
    candidates = [os.path.join(p.replace("\\\\", "\\"), "steamapps", "common", "Minecraft Dungeons II") for p in [STEAM] + libraries] + [XBOX_GAME]
    return next((g for g in candidates if os.path.isdir(paks(g))), None)


def cook(editor, mod, unversioned):
    args = [editor, UPROJECT, "-run=cook", "-targetplatform=Windows", f"-cookdir={os.path.join(PROJECT, 'Content', 'Mods', mod)}", "-unattended", "-nop4", "-nosplash"]
    if unversioned:
        args.append("-unversioned")
    print(f"Cooking {mod} ({'unversioned' if unversioned else 'versioned'})")
    result = subprocess.run(args, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True, errors="replace")
    if result.returncode != 0:
        print("\n".join(line for line in result.stdout.splitlines() if "Error" in line))
        fail("the cook failed, see Dungeons/Saved/Logs/Dungeons.log")


def main():
    parser = argparse.ArgumentParser(description="Cook and pack a Blueprint Loader mod for Minecraft Dungeons II")
    parser.add_argument("mod", help="the mod folder name inside Content/Mods")
    parser.add_argument("--install", action="store_true", help="copy the built mod into the game's ~mods folder")
    parser.add_argument("--engine", default=DEFAULT_ENGINE, help="the Unreal Engine 5.6.1 install folder")
    parser.add_argument("--game", help="the Minecraft Dungeons II install folder")
    args = parser.parse_args()

    mod = args.mod
    editor = os.path.join(args.engine, "Engine", "Binaries", "Win64", "UnrealEditor-Cmd.exe")
    if not os.path.exists(editor):
        fail(f"Unreal Engine 5.6.1 was not found at {args.engine}, pass its folder with --engine")
    for tool in (RETOC, REPAK):
        if not os.path.exists(tool):
            fail(f"{os.path.basename(tool)} is missing from the Tools folder, see the README")
    if not os.path.exists(os.path.join(PROJECT, "Content", "Mods", mod, "ModActor.uasset")):
        fail(f"Content/Mods/{mod}/ModActor was not found")

    cook(editor, mod, False)
    header_root = os.path.join(BUILD, "Temp", mod, "Header")
    shutil.rmtree(header_root, ignore_errors=True)
    header = os.path.join(header_root, "Dungeons", "Content", "Mods", mod, "ModActor.uasset")
    os.makedirs(os.path.dirname(header))
    shutil.copy2(os.path.join(COOKED, mod, "ModActor.uasset"), header)

    cook(editor, mod, True)
    staging = os.path.join(BUILD, "Temp", mod, "Staging")
    shutil.rmtree(staging, ignore_errors=True)
    shutil.copytree(os.path.join(COOKED, mod), os.path.join(staging, "Dungeons", "Content", "Mods", mod))

    out_dir = os.path.join(BUILD, mod)
    shutil.rmtree(out_dir, ignore_errors=True)
    os.makedirs(out_dir)
    stem = os.path.join(out_dir, f"{mod}_P")
    print("Packing")
    subprocess.run([RETOC, "to-zen", "--version", "UE5_6", staging, stem + ".utoc"], check=True, stdout=subprocess.DEVNULL)
    os.remove(stem + ".pak")
    subprocess.run([REPAK, "pack", "-q", "--version", "V11", header_root, stem + ".pak"], check=True)
    print(f"Built {out_dir}")

    if args.install:
        game = args.game or find_game()
        if not game or not os.path.isdir(paks(game)):
            fail("Minecraft Dungeons II was not found, pass its folder with --game")
        mods = os.path.join(paks(game), "~mods", mod)
        os.makedirs(mods, exist_ok=True)
        for ext in (".pak", ".utoc", ".ucas"):
            shutil.copy2(stem + ext, mods)
        print(f"Installed to {mods}")


if __name__ == "__main__":
    main()
