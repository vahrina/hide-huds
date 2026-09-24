## what hides

| element | shows when |
|---|---|
| hotbar | switching slots, hotbar keys or scrolling |
| hearts / mana | not at full health or just got hit, fades in and out |
| minimap | any activity, always shown with the overlay map (`mapstyle 1`) |
| info / builder accessories, radial hotbars | any activity |
| calamity flight | flying or wing time not full |
| calamity stealth | rogue stealth not full |
| calamity rage / adrenaline | any activity |

opening the inventory, fullscreen map or scrolling brings everything back

full layer list in [ui_list.md](ui_list.md)

## building

1. clone into `Documents\My Games\Terraria\tModLoader\ModSources\HideHud`
2. start tModLoader once so it creates `ModSources\tModLoader.targets`
3. either build in game (workshop > develop mods > build) or run `dotnet build` in the mod folder

the `.tmod` ends up in `Documents\My Games\Terraria\tModLoader\Mods`, enable it in the mods menu
