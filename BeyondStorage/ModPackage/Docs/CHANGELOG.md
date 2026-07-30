__Released v3.1.1__
- ❌ Remove the concept of On Mission. It was an attempt to auto-detect if vehicles should be pushed to, but it didn't work out there in the wild.
- ✅ Everything will now Smart Push to the **Load Out slots (locked slots)** of Vehicle and Drones first, and after that fill the surrounding storages
- ✅ When all your storages are filled up, then the remaining items are pushed to Vehicles and Drones that already have items of that type
- ✅ Support the stack size multiplier in Sandbox Options (game V3.1.0 feature)
- ✅ Update README with how multiplayer works, and update the documentation in general with the current mod features and behaviour
- ✅ Enable using items produced in community workstations, aka the working ones found in the wild
- 🪲 Fixed Item Type counting in transfer smart report only counted the total items, not the different item types as well
- 🪲 Remove Smart Pull button that accidentally showed up on Dropped Loot containers
- 🪲 Implemented Smart Push button on Dropped Loot containers
- ❌️ Remove **serverSyncConfig** config option. There is no reason to ever set it to False.
- 🪲 Small bug fixes that no-one even noticed :)

📢 This mod version will only support game V3.1.0 and later
📍 See the pinned message for the release sites for this update.
