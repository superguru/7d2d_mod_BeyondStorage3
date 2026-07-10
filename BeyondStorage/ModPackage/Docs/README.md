# Beyond Storage 3 - Everything Is Your Inventory
I would appreciate it if you could [support me on Ko-fi](https://ko-fi.com/gazorper)
Hundreds of hours go into making a mod like this, not to mention supporting it.
[![Support me on Ko-fi](https://raw.githubusercontent.com/superguru/7d2d_mod_BeyondStorage3/refs/heads/master/BeyondStorage/Media/gazorper-kofi.png)](https://ko-fi.com/gazorper)

Please go to <span style="color:#4c5fd7;">**#beyond-storage3**</span> on the <span style="color:#f46f30;">**Discord server**</span> for mod support.

Also see [Docs\README.md](https://github.com/superguru/7d2d_mod_BeyondStorage3/blob/master/BeyondStorage/ModPackage/Docs/README.md) in the mod package for the full documentation.

You should also look at the Media Gallery on the mod distribution site where you downloaded this mod from.

## What can this do?
### Extended Inventory - Everything Is Your Inventory (aka inventory network)
Items that exist in any of these places are now part of the your **extended inventory**:
- Player backpack
- Storage crates
- Containers like refrigerators, wall safes, lockers, etc. that you crafted and placed
- Workstation outputs, such as items produced in Campfire, Forge, Dew Collector, Apiary, Workbench, Chem Station, Cement Mixer, and so on
- Vehicle storage like Bicycle, 4x4, Minibike, Motorcycle, Gyrocopter, and so on
- Drone storage
### Consume from the extended inventory
Paint, pick locks, upgrade blocks, refuel equipment and vehicles, repair blocks, pay traders and vending machines, and of course craft things. 
The Backpack items are used first, just like in the base game, and after that this is the order of storages where items are Consumed from:
1. Drones
2. Collectors, such as Apiaries and Dew Collectors
3. Workstations
4. Containers
5. Vehicles

Items from the source slots with the least items are used first.
You can turn off Consume from specific blocks, like from a Wooden Crate or a Locker using the radial menu.
![Consume On/Off radial](https://raw.githubusercontent.com/superguru/7d2d_mod_BeyondStorage3/refs/heads/master/BeyondStorage/Media/consume_radial_on_off_side_by_side.jpg)
### Smart sorting
#### Push to surrounding storage
You can bulk move all items from any storage to the surrounding storage, providing there is already an item of that type present in that storage.
As long as there are items left to move, the destination slots with the most items will be filled first until all slots across all target storages are filled up to maximum stack size for each item.
Then empty slots will be used, until there is no more space anywhere.
![Smart Push from Campfire](https://raw.githubusercontent.com/superguru/7d2d_mod_BeyondStorage3/refs/heads/master/BeyondStorage/Media/smart_push_from_campfire.jpg)
If you use the Smart Push function from this Campfire, then the 6 Bacon and Eggs will be bulk moved to the Wooden Crate that already has other Bacon and Eggs stored in it.
The 16 Water will also by moved to any crates or other player storage if there is already Water in them.
This works from anywhere and anything where you see the Smart Push button.
Yes, Vehicles and Drones too. Also Apiaries and Dew Collectors. Even crates! Everything is included.

#### Pushing and Locked Slots
Locking a slot means that you are turning Off any bulk transfers from that slot, just like in the base game.
So if you always want to keep Wood in your Backpack, lock the slot it's in, and Smart Push will leave it alone.
![Locked Backpack Slots](https://raw.githubusercontent.com/superguru/7d2d_mod_BeyondStorage3/refs/heads/master/BeyondStorage/Media/locked_slots_backpack.jpg)
This is my Backpack top row from a current game. None of these items will be bulk moved out of the Backpack, whether using the game *Move All* or *Move and Fill* buttons, or using Smart Push.
The 62 Polymers is not locked, so they can be moved is if there is a storage somewhere that already has some Polymers in it.

#### Smart Pull, aka Topping up your Load Out
If you lock some slots in your Backpack (Player Inventory), or in Drones, vehicles like Motorcycle, Gyrocopter, and so on, then you can use those as your **Load Out** slots.
Using the Smart Pull button will top up any items in those slots with any available items.
![Smart Pull Loadout to Bicycle](https://raw.githubusercontent.com/superguru/7d2d_mod_BeyondStorage3/refs/heads/master/BeyondStorage/Media/smart_pull_bicycle_loadout.jpg)
This is my trusty Bicycle. I always keep Coins, Repair Kits, Lock picks, Shotgun Ammo, and Arrows in there.
I never keep any of these items in my own inventory, so my Backpack is just for looted items and whatnot.
Before leaving the base, I just use Smart Pull to Top Up my Bicycle Load Out, and I have a fresh load of bullets and other needful things to go **On Mission**.

#### Gotchas about Smart Pull
Items are only pulled from immovable storage like crates and other user created and placed containers.
So <span style="color:#ff2222;">**you can't pull in**</span> ammo from your Bicycle into your Drone, for instance.
Pulling in items will not overflow to empty slots, whether they're locked or not. All it does is fill up a slot with an existing item until the maximum stack size for that item is reached.

#### On Mission, aka cool things you didn't know you could do
If you are out of range of your base, then you're classified to be **On Mission**.
In this situation, pushing from your Backpack (Player Inventory) will use your vehicles and drones that are nearby as the destination to bulk move items to if there are already items of that type in them.
