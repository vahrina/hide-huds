## what hides

each part of the hud hides on its own after a few seconds and comes back when something changes. health bar stays up while you're hurt

client side only, other players dont need it

| element | shows when | hides when idle and |
|---|---|---|
| health | health, shield, barrier, curse or void shield changes | at full health (barrier ignored) |
| money | gold, lunar or void coins change | always |
| items | an item stack changes | always |
| equipment | equipment or charges change | not recharging |
| level / xp | xp or level changes | always |
| buffs | a buff shows up, goes away or stacks change | always |
| skills | a skill is used or gets a charge back | all skills are full |
| objectives | objective text changes | always |
| allies | an ally takes damage or joins/leaves | all allies are full |
| boss bar | boss spawns or takes damage | always |
| timer / difficulty | stage or difficulty changes | always |

the version number in the top right is hidden for good

crosshair, damage numbers, chat, prompts and menus are never touched

to see everything, hold `LeftAlt` or open the scoreboard (both configurable)

## config

you may configure it in your desired mod manager: `BepInEx/config/com.vah.HideHud.cfg`

- `Enabled` turn the whole thing on or off
- `IdleSeconds` how long before stuff fades (default 3)
- `FadeInSeconds` / `FadeOutSeconds` fade speed (default 0.15 / 0.35)
- `PeekKey` hold to show everything (default `LeftAlt`), any [unity keycode](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/KeyCode.html), `None` to disable. combos also work: `LeftControl + A`
- `ShowAllOnScoreboard` show everything on the scoreboard (default on)
- `KeepBossBarWhileActive` keep boss bar up while a boss is alive (default off)
- `KeepDifficultyTimer` keep the run timer visible (default off)
- `KeepEquipmentWhileRecharging` keep equipment up until its ready (default on)
- `Widgets` turn hiding on or off per element, `Version` hides the version number

## dependencies

- [BepInExPack](https://thunderstore.io/package/bbepis/BepInExPack/)
- [HookGenPatcher](https://thunderstore.io/package/RiskofThunder/HookGenPatcher/)

## notes

- hud mods with their own canvas (HunkHUD, BetterHudLite) arent handled, but seem to work fine as-is from what i tested
- small mod elements inside vanilla hud parts fade along with them
- a charging teleporter keeps the objective panel up since the percent keeps changing

## building

```powershell
dotnet build -c Release
```

set `-p:RiskOfRain2Path=...` and `-p:MmhookPath=...` if your game or profile is somewhere else. dll ends up in `bin\Release`

move the compiled binary & `manifest.json` into your `BepInEx` directory, e.g. `%AppData%\Thunderstore Mod Manager\DataFolder\RiskOfRain2\profiles\<your profile>\BepInEx\plugins\HideHud`

the mod may not show up in the mod manager list, you can find it under the "Edit Config" tab regardless to adjust the values as desired