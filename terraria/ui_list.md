# interface layers

| policy | meaning |
| --- | --- |
| ignore | never touch |
| idle | hide after idle delay |
| map | idle, except `mapstyle == 1` |
| resource | idle, but show/fade when hp not full or recently hit |
| hotbar | idle, but show on hotbar key / slot change / scroll |
| flight | idle, but show when flying or `wingtime` not full |
| stealth | show only while calamity rogue stealth is not full |

## vanilla

| layer | policy |
| --- | --- |
| vanilla: interface logic 1 | ignore |
| vanilla: mp player names | ignore |
| vanilla: emote bubbles | ignore |
| vanilla: entity markers | ignore |
| vanilla: smart cursor targets | ignore |
| vanilla: laser ruler | ignore |
| vanilla: ruler | ignore |
| vanilla: gamepad lock on | ignore |
| vanilla: tile grid option | ignore |
| vanilla: town npc house banners | ignore |
| vanilla: hide ui toggle | ignore |
| vanilla: wire selection | ignore |
| vanilla: capture manager check | ignore |
| vanilla: ingame options | ignore |
| vanilla: fancy ui | ignore |
| vanilla: achievement complete popups | ignore |
| vanilla: entity health bars | ignore |
| vanilla: invasion progress bars | ignore |
| vanilla: map / minimap | map |
| vanilla: diagnose net | ignore |
| vanilla: diagnose video | ignore |
| vanilla: sign tile bubble | ignore |
| vanilla: hair window | ignore |
| vanilla: dresser window | ignore |
| vanilla: npc / sign dialog | ignore |
| vanilla: interface logic 2 | ignore |
| vanilla: resource bars | resource |
| vanilla: interface logic 3 | ignore |
| vanilla: inventory | ignore |
| vanilla: info accessories bar | idle |
| vanilla: settings button | ignore |
| vanilla: hotbar | hotbar |
| vanilla: builder accessories bar | idle |
| vanilla: radial hotbars | idle |
| vanilla: mouse text | ignore |
| vanilla: player chat | ignore |
| vanilla: death text | ignore |
| vanilla: cursor | ignore |
| vanilla: debug stuff | ignore |
| vanilla: mouse item / npc head | ignore |
| vanilla: mouse over | ignore |
| vanilla: interact item icon | ignore |
| vanilla: interface logic 4 | ignore |

## calamity

| layer | policy |
| --- | --- |
| flight ui | flight |
| stealth ui | stealth |
| rage & adrenaline ui | idle |
