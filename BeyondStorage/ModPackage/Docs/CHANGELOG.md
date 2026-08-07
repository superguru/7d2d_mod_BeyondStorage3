__🔬 WIP v3.1.5__
- ✅ Added Shift+Push information to the relevant button tooltips
- ✅ Improve smart push notification message content
- ♻ Fixed that eligible storages was sorted by type then closest, instead of just closest
- ♻ Fixed that pushing to allies' vehicles didn't work
- 🙋 Any bugs to report?

- 📜 TODO: Allow push from any loot window

```text
For game V3.1.x or later
- Push to Allies' Vehicles! (can turn off)
- Optional GEARS settings support
- Read the Changelog for more

Go to #beyond-storage3 on Discord for support. First read the Docs/README.md file or the mod web page
```

```text
🔬 Experimental
🚧 WIP 
🚀 Released

- ✅ Added 
-  Changed
- ♻ Fixed 
- ❌ Removed 
- 👉🏼 WIP: 
- 📜 TODO: 
- 💭 Considering: 
```

__🚀 Released v3.1.4__
- ✅ Added GEARS support for all mod settings
- ✅ Allow smart push to vehicles of allied players as well as your own, On by default
- ✅ Added new config setting `allowPushToAlliedVehicles` to control this behaviour, default is On
- ✅ Added Shift Push to bulk transfer any leftover items after normal push to nearby storages, whether they already have other items of that type or not
- 🚼 Renamed the consumeXXX config settings to includeXXXInNetwork, as that is what they really do. Localisation, source code and documentation need to be updated too.

📢 This mod version will only support game V3.1.0 and later
📍 See the pinned message in [![#beyond-storage3](https://img.shields.io/badge/%23beyond--storage3-4c5fd7)](https://discord.gg/hAF5T4P9pE) on [![Discord](https://img.shields.io/badge/%23Discord-f46f30)](https://discord.gg/hAF5T4P9pE) to find release sites for this update.

__🚀 Released v3.1.3__
- ✅ Use the localised name for Backpack rather than player name for messages about smart sorting functions
- ✅ Do not allow quest items to be moved during smart sorting operations (for future features)
- ✅ Improve messages about source and target storages for smart sorting
- ♻ Move Creative only mode button Clear inventory so it doesn't clash with Smart buttons

__🚀 Released v3.1.2 [Discord Secret Club exclusive]__
- 🪲 Don't bulk move quest items such as the White River Supplies during Smart Push to overflow storages (move to overflow hidden feature)
- ✅ Use the localised name for Backpack rather than player name for messages about smart sorting functions
- 🙋 Any bugs to report?
 
__🚀 Released v3.1.1__
- ❌ Remove the concept of On Mission. It was an attempt to auto-detect if vehicles should be pushed to, but it didn't work out there in the wild.
- ✅ Everything will now Smart Push to the **Load Out slots (locked slots)** of Vehicle and Drones first, and after that fill the surrounding storages
- ✅ When all your storages are filled up, then the remaining items are pushed to Vehicles and Drones that already have items of that type
- ✅ Support the stack size multiplier in Sandbox Options (game V3.1.0 feature)
- ✅ Update README with how multiplayer works, and update the documentation in general with the current mod features and behaviour
- ✅ Enable using items produced in community workstations, aka the working ones found in the wild
- ♻ Fixed Item Type counting in transfer smart report only counted the total items, not the different item types as well
- ♻ Remove Smart Pull button that accidentally showed up on Dropped Loot containers
- ♻ Implemented Smart Push button on Dropped Loot containers
- ❌ Remove **serverSyncConfig** config option. There is no reason to ever set it to False.
- ♻ Small bug fixes that no-one even noticed :)

*** PACKAGED aka MOD CHANGELOG.md EOF ***