# Beyond Storage 3 - Everything Is Your Inventory
I would appreciate it if you could [support me on Ko-fi](https://ko-fi.com/gazorper), as **hundreds of hours** go into making a mod like this, keeping it up to date with newer game versions, not to mention supporting it.

[![Support me on Ko-fi](https://raw.githubusercontent.com/superguru/superguru/refs/heads/master/images/superguru-kofi-orange.png)](https://ko-fi.com/gazorper)

Please go to [![#beyond-storage3](https://img.shields.io/badge/%23beyond--storage3-4c5fd7)](https://discord.gg/hAF5T4P9pE) on [![Discord](https://img.shields.io/badge/%23Discord-f46f30)](https://discord.gg/hAF5T4P9pE) for mod support.

Also see [Docs\README.md](https://github.com/superguru/7d2d_mod_BeyondStorage3/blob/master/BeyondStorage/ModPackage/Docs/README.md) in the mod package for the full documentation.

You should also look at the Media Gallery on the mod distribution site where you downloaded this mod from.

## 💾What to download

- V3: Use the main download for the latest version of 7 Days to Die v3.x (all builds)
- V2: For older versions of the game, like 7 Days v2.x, see Old Files section
 
## What can this do?
There are three main functions:
- 1️⃣ <span style="text-decoration: underline;">Consume</span> - pay, paint, refuel, craft, upgrade, repair, reload
- Smart Sorting:
    - 2️⃣ <span style="text-decoration: underline;">Smart Push</span>
        - ✔️ Normal Mode: move items to surrounding containers that **already have** the item being moved - works for everything
        - 👆 Shift Push Mode: Hold Shift and click the Smart Push button to move items to storages that have open slots, even if they **don't already have** the item being moved
    - 3️⃣ <span style="text-decoration: underline;">Smart Pull</span> - top up your **Load Out slots (locked slots)** - works with backpack, vehicles, drones

This is explained more fully below.

### Extended Inventory - Everything Is Your Inventory (aka inventory network)
Items that exist in any of these places are now added to your Backpack and Toolbelt, and become part of your **extended inventory**:
1. Drones
2. Collectors, such as Apiaries, Dew Collectors, Chicken Coops, modded collectors
3. Workstations like Campfire, Forge, Dew Collector, Apiary, Workbench, Chem Station, Cement Mixer, and so on, including community workstations (working ones found in the wild), and modded workstations
4. Containers like crates, and player crafted things like refrigerators, wall safes, lockers, etc. that you crafted and placed. If your ally in a multi-player game crafted it, it's also player crafted storage.
5. Vehicles like Bicycle, 4x4, Minibike, Motorcycle, Gyrocopter, and so on. All modded vehicles also work.

This is the order in which items are Consumed from too, so Backpack and Toolbelt items are used first, just like in the base game, and after that Drones, then Collectors, etc.

![Player Crafted Wall Safe Consume Toggle](https://raw.githubusercontent.com/superguru/7d2d_mod_BeyondStorage3/refs/heads/master/BeyondStorage/Media/consume_toggle_on_player_wall_safe.jpg)
An example of a player crafted storage, in this case a Wall Safe.

### Consume from the extended inventory
Paint, pick locks, upgrade blocks, refuel equipment and vehicles, repair blocks, pay traders and vending machines, and of course craft things. 
![Consume from Useful Drone](https://raw.githubusercontent.com/superguru/7d2d_mod_BeyondStorage3/refs/heads/master/BeyondStorage/Media/consume_from_useful_drone.jpg)
Items from the source slots with the least items are used first.
You can turn off Consume for specific blocks, like for a Wooden Crate or a Locker using the radial menu.
![Consume On/Off radial](https://raw.githubusercontent.com/superguru/7d2d_mod_BeyondStorage3/refs/heads/master/BeyondStorage/Media/consume_radial_on_off_side_by_side.jpg)

### Smart Push to surrounding storage
You can bulk move all items from any storage to the surrounding storage, providing there is already an item of that type present in that storage.
As long as there are items left to move, the destination slots with the most items will be filled first until all slots across all target storages are filled up to maximum stack size for each item.
Then empty slots will be used, until there is no more space anywhere.
![Smart Push from Campfire](https://raw.githubusercontent.com/superguru/7d2d_mod_BeyondStorage3/refs/heads/master/BeyondStorage/Media/smart_push_from_campfire.jpg)
If you use the Smart Push function from this Campfire, then the 6 Bacon and Eggs will be bulk moved to the Wooden Crate that already has other Bacon and Eggs stored in it.
The 16 Water will also by moved to any crates or other player storage if there is already Water in them.
This works from anywhere and anything where you see the Smart Push button.
Yes, Vehicles and Drones too. Also Apiaries and Dew Collectors. Even crates! Everything is included.

### Smart Push Stages (aka Order Of Operations)

1\) Pushing items from your anywhere will first try to fill any **Load Out slots (locked slots)**, and then move anything that's left to the surrounding storages.

This should help with keeping your bullets, lock picks, repair kits, and bandages stocked up without too much other intervention.

2\) After this any container that can't move is tried, and items are sorted into the Crates, etc. per item type they already contain.

Nothing is bulk moved to workstation or collector outputs, because that makes no sense.

3\) Lastly, if there's still something to move, then any mobile storages are tried, also looking for places that already contain the item types that need to be moved, thereby sorting them. Mobile storages are things like vehicles, and drones.

> 👆 Shift Push Mode: There is a stage 4 if you use Shift Push (click the button while holding Shift):
> 4\) After the stages above, any remaining items (that there's room for somewhere) will be moved to any storages that have empty slots, whether already have items of that type or not.
> You can Shift Push from all stationary storages, including workstations.

This helps a lot when you are clearing a POI and you - and perhaps others that you're playing with - placed some dump chests. You can auto-sort items by bulk transferring them using Smart Push, which will fill up those chest, and then also any of your own vehicles and drones nearby that already have any of the remaining items that need to be moved.

### Pushing and Locked Slots
Locking a slot means that you are turning Off any bulk transfers from that slot, just like in the base game. Slot locking prevents items from being bulk moved **OUT**, not in.
So if you always want to keep Wood in your Backpack, lock the slot it's in, and Smart Push will leave it alone.
![Locked Backpack Slots](https://raw.githubusercontent.com/superguru/7d2d_mod_BeyondStorage3/refs/heads/master/BeyondStorage/Media/locked_slots_backpack.jpg)
This is my Backpack top row from a current game. None of these items will be bulk moved out of the Backpack, whether using the game *Move All* or *Move and Fill* buttons, or using Smart Push.
The 62 Polymers is not locked, so they can be moved is if there is a storage somewhere that already has some Polymers in it.

## Overflow from Smart Push
**Example 1:** You have 10 Water to push, and there is a Crate that has 9 Water and also an empty slot, then the result after bulk transferring the Water is that the Crate not has a slot with 10 Water and another slot with 9 Water. The extra water overflowed to that Crate's empty slot because it had room.

**Example 2:** You have 10 Water to push, and there are two crates with 2 Water and 3 Water respectively. After the smart push, the two crates will have 10 Water and 5 Water respectively.

**Example 3:** You have 20 Water to push, and there are two crates.
- Crate A has 2 Water, 1 empty slot. 
- Crate B has 3 Water, 1 empty slot.

After the smart push:
- Crate A has 10 Water, 5 Water <- 8 Water came in, filling the 3 Water slot, Then 5 was left to put in the empty slot
- Crate B has 10 Water, 1 empty slot <- 7 Water came in, and filled up the existing 3 Water slot

📢**Note:** These examples are for the default max stack size of 10 in Vanilla default settings. You can change the stack size by using the Sandbox Options, which will have the same logic, just with different max stack sizes for items.

### Smart Pull, aka Topping up your Load Out
If you lock some slots in your Backpack (Player Inventory), or in Drones, vehicles like Motorcycle, Gyrocopter, and so on, then you can use those as your **Load Out** slots.
Using the Smart Pull button will top up any items in those slots with any available items.
![Smart Pull Loadout to Bicycle](https://raw.githubusercontent.com/superguru/7d2d_mod_BeyondStorage3/refs/heads/master/BeyondStorage/Media/smart_pull_bicycle_loadout.jpg)
This is my trusty Bicycle. I always keep Coins, Repair Kits, Lock picks, Shotgun Ammo, and Arrows in there.
I never keep any of these items in my own inventory, so my Backpack is just for looted items and whatnot.
Before leaving the base, I just use Smart Pull to Top Up my Bicycle Load Out, and I have a fresh load of bullets and other needful things to go **On Mission**.

I also do a Smart Push from the Workbench to sort any produced ammo to, in this order:
1. My Bicycle (or any vehicles or drones that belong to me, closest first) - top up the **Load Out slots (locked slots)**
2. Stationary storage - Crates, player crafted storage, etc. with items matching the ones being transferred
3. Mobile storage - vehicles and drones again, but this time not limited to locked slots, but any slot that already contains the items being transferred

### Gotchas about Smart Pull
Items are only pulled from stationary storages like crates and other user created and placed containers. Never from vehicles or drones.
So <span style="color:#ff2222;">**you can't pull in**</span> ammo from your Bicycle into your Drone, for instance.
Pulling in items will not overflow to empty slots, whether they're locked or not. All it does is fill up a slot with an existing item until the maximum stack size for that item is reached.
![Img](https://raw.githubusercontent.com/superguru/7d2d_mod_BeyondStorage3/refs/heads/master/BeyondStorage/Media/player_crafted_office_desk_example.jpg)
This is my office desk that I **crafted and placed myself**. It is also part of the extended inventory network. The same Wall Safe from the earlier example is just above it.

### 🤼‍♀️Multiplayer Explained
Firstly, it's important to understand that no one player owns anything **except** Vehicles or Drones in the game.

So if you build a Crate and place it, then anyone of your Allies can access it. If you build a Wall Safe and place it, then the same rule applies.

When you read this documentation you will see it mentions "Player crafted storage", and it's important to understand this concept.

- Did you craft the Wooden Crate? Then that's a "Player crafted storage".
- Did your friend who is multiplaying with you and is an Ally craft the Wooden Crate? Then that's also a "Player crafted storage".

📢 Just because you personally crafted and placed a Wooden Crate, does not mean you own that crate. No-one owns it.

You can run over to your friend's base, or to a crate that they built and placed, and just open it and take what's inside, or perhaps put some items into the crate.

All of these storages are all part of you and anyone of your Allies' extended inventory, providing it's in **range**.

#### What about land claims?
Placing a Land Claim Block does not mean you claim **ownership** over anything.

This is just how the game works. 

Zombies don't spawn in range of a Land Claim Block, unless it's a Blood Moon.

Enemies - players who are not Allies - don't spawn there at any time.

Also, Land Claim Blocks prevent enemy damage to your Ally group's blocks.

#### So what can you do to prevent your friend from using items in "your" storage?
1. Do not ally with that player. Allies **share everything** except Vehicles and Drones.
2. Move the storage out of range of your Ally. But if your friend runs over to your storage, they would come in range of it, and then be able to use the items inside. This is just a sort of general suggestion.
3. You can turn Consume Off or On for a storage. This will mean anyone in your Ally group can still Smart Push or Pull to and from it, but no-one, including yourself, can Consume from it.

#### Locking your storage
You can lock your storage **with a PIN**. Then only someone with the PIN can open the storage physically, and the same goes for their extended inventory.

So lock it **with a PIN**, don't share the PIN, and then only you can Consume from it, and only you can Smart Push and Pull to and from it.

#### Short Summary about Multiplayer storage
In this game, there is no concept whatsoever of a particular Player owning a Crate or any other storage.

A Player can only own a Vehicle or a Drone.

## Console and Config

### ⌨️Console Commands (when you press F1)

```text
+----------------+--------------------------------------------------------------+
| Command        | Description                                                  |
+----------------+--------------------------------------------------------------+
| bshelp         | List available commands with their descriptions              |
+----------------+--------------------------------------------------------------+
| bsclearcache   | Invalidates cache and reloads items from storages            |
+----------------+--------------------------------------------------------------+
| bsreloadconfig | Reload the config as per the current config.json file        |
|                | (maybe you modified it in a text editor)                     |
+----------------+--------------------------------------------------------------+
| bssetconfig    | Change the value of a config property                        |
+----------------+--------------------------------------------------------------+
| bsshowconfig   | Displays the current active config settings                  |
+----------------+--------------------------------------------------------------+
```

### ❄️Config file
The mod can also be configured by editing `Mods/BeyondStorage3/config.json`

```text
+---------------------------+---------+------------------------------------------------------------+
| Setting                   | Default | Description                                                |
+---------------------------+---------+------------------------------------------------------------+
| includeDrones             | On      | Whether to include nearby Drones in storage network        |
+---------------------------+---------+------------------------------------------------------------+
| includeVehicles           | On      | Whether to include nearby Vehicles in storage network      |
+---------------------------+---------+------------------------------------------------------------+
| allowPushToAlliedVehicles | On      | Allow smart push to send items to nearby Vehicles          |
|                           |         | belonging to allies                                        |
|                           |         | Only works if includeVehicles is On                        |
+---------------------------+---------+------------------------------------------------------------+
| isDebug                   | Off     | Logs additional information that might be useful for       |
|                           |         | troubleshooting problems. You can generally leave this     |
|                           |         | turned off.                                                |
+---------------------------+---------+------------------------------------------------------------+
| range                     | 0. 0    | Range to include in storage network.                       |
|                           |         | 0.0 is everything in memory, max is 250m.                  |
|                           |         | The game usually holds up to 200m radius.                  |
+---------------------------+---------+------------------------------------------------------------+
*Off=false, On=True in config.json file.
```

**⚙︎⚙︎⚙︎ [Gears](https://www.nexusmods.com/7daystodie/mods/4017) settings support ⚙︎⚙︎⚙︎** (Optional)
All settings can be modified using the Gears interface.
This is available at the game loading screen, and also in-game when you press \<ESC\> and click the \[MODS\] button.

`Mods/BeyondStorage3/config.json` is the master source for config values, and changes to this file will override the settings saved in Gears. However Gears setting will always be synced when you access the Gears settings editing screen.

*Any setting not listed here is either old, or otherwise is for mod development purposes. It's best to leave them alone.*
*The mod will automatically convert older properties and values, removing invalid ones as needed, when it loads.*

#### 📝Notes:
1. In the config file, true means On and false means Off
2. Previous versions of the mod had other config options. Their usage was self-explanatory from their names.
3. Install the same version on **CLIENT AND SERVER** if you are hosting or participating in a multi-player game.

## ⚙️Installation
- Use a Mod Manager to install the mod, or 
- Unzip the contents of this mod into your 7 Days to Die Mods folder

This is a Harmony mod, so you need to [disable EAC on your computer](https://help.sparkedhost.com/en/article/how-to-disable-easy-anti-cheat-7-days-to-die-11jgs21/)

If applicable to your game, also [Disable EAC on your server](https://shockbyte.com/help/knowledgebase/articles/how-to-turn-off-easy-anti-cheat-eac-on-your-7-days-to-die-server)

Install the same version on **CLIENT AND SERVER** if you are hosting or participating in a multi-player game.

Please go to [![#beyond-storage3](https://img.shields.io/badge/%23beyond--storage3-4c5fd7)](https://discord.gg/hAF5T4P9pE) on [![Discord](https://img.shields.io/badge/%23Discord-f46f30)](https://discord.gg/hAF5T4P9pE) for mod support.

The people on Discord are quite helpful too.

## 🌐Translations
- 🇷🇺 Russian: [Beyond Storage 3_rus](https://www.nexusmods.com/7daystodie/mods/11244)

## 🧩Troubleshooting
Things don't seem to work? Try these steps first.

Check your mod list. Don't install other **"craft from container"** type mods at the same time as this one, as these types of mods are generally not compatible with each other.

Did you delete something you shouldn't have? [Stop deleting the Harmony Folder](https://7daystodiemods.com/stop-deleting-the-harmony-folder), aka **0_TFP_Harmony**

Fix many errors by [verifying the integrity of your game installation](https://help.steampowered.com/en/faqs/view/0C48-FCBD-DA71-93EB)

Install the same version on **CLIENT AND SERVER** if you are hosting or participating in a multi-player game.

## 📚Articles that might help
- [How To Disable Easy Anti Cheat - 7 Days To Die](https://help.sparkedhost.com/en/article/how-to-disable-easy-anti-cheat-7-days-to-die-11jgs21)
- [How to add Mods to your 7D2D game](https://www.nexusmods.com/7daystodie/articles/889)
- [Basic troubleshooting for mods](https://www.nexusmods.com/7daystodie/articles/787)

## 🏆Credits
- [Undead Legacy](https://www.snowbeegaming.com/undead-legacy) by Subquake for inspiring us all
- [aedenthorn](https://github.com/aedenthorn) for the original mod
- [unv-annihilator](https://github.com/unv-annihilator) for the 7 Days to Die v1 fork
- [superguru](https://github.com/superguru) for the 7 Days to Die v2 refactor
- [gazorper](https://next.nexusmods.com/profile/gazorper/mods) for the Beyond Storage 3 mod
- For v2.5.x mod series: Compatibility code copied (with permission) from Jakmeister999's Enhanced HUD (V2.3) mod
- The 7 Days to Die community for their support and contributions

## 🤝Mod compatibility list

### ✅ Considered compatible (V3):

I run these mods, among others, and so the development and testing of this mod means it works perfectly with the mods listed below:
- ✅ [AGF V3 Larger Backpacks 60-72-84-119](https://www.nexusmods.com/7daystodie/mods/1684)
- ✅ [AGF - V3 - HUD PLUS](https://www.nexusmods.com/7daystodie/mods/870)
- ✅ [Bdub's Vehicles (3.0)](https://www.nexusmods.com/7daystodie/mods/342)
- ✅ [CraftLink](https://www.nexusmods.com/7daystodie/mods/10970)
- ✅ [Endless Harvest](https://www.nexusmods.com/7daystodie/mods/7554)
- ✅ [IZY Classic for 7 Days To Die 3.0](https://www.nexusmods.com/7daystodie/mods/11059)
- ✅ IZY [Flatlander's Combat Sandbox for IZY Classic](https://www.nexusmods.com/7daystodie/mods/9980)
- ✅ [Modivination](https://www.nexusmods.com/7daystodie/mods/10952)
- ✅ [PROJECT Z FULL VERSION](https://www.nexusmods.com/7daystodie/mods/7786)
- ✅ [RAM - Random Affixes Mod (3.0)](https://www.nexusmods.com/7daystodie/mods/9567)
- ✅ [Ramos Crafted In](https://www.nexusmods.com/7daystodie/mods/8629)
- ✅ [Ramos Recipe Tracker](https://www.nexusmods.com/7daystodie/mods/8634)
- ✅ [Reclaim Storage Crates](https://www.nexusmods.com/7daystodie/mods/5418)
- ✅ [Vehicle Madness](https://7daystodiemods.com/mods/vehicle-madness-802517)

#### Common Dependency Libraries that are compatible
- ✅ [0-Quartz](https://www.nexusmods.com/7daystodie/mods/2409)
- ✅ [Gears](https://www.nexusmods.com/7daystodie/mods/4017) <- Beyond Storage supports settings changes using Gears

##### SCore (not really compatible)
For mods that use [0-SCore](https://www.nexusmods.com/7daystodie/mods/6176), you need to disable these Features:
- Remote Crafting
- Remote Repair/Upgrade
 
### ❌ Not considered compatible:

#### General

Don't use any remote crafting type of mods at the same time. 

They are generally not compatible with each other. 

Just pick one you like and use that.

### Overhaul Mods

Most overhaul mods will replace your Backpack, Workstations, and other windows in the game.

That will make the Smart Push and Pull buttons from Beyond Storage invisible, so a lot of the functionality will then be inaccessible.

#### Others (not compatible)

- ❌ [Advanced UI](https://www.nexusmods.com/7daystodie/mods/8289)
- ❌ [CATUI (for 7DTD 3.1)](https://www.nexusmods.com/7daystodie/mods/5248)

## License

This mod is licensed under the [Apache-2.0 License](https://www.apache.org/licenses/LICENSE-2.0.html). See the [LICENSE.txt](https://raw.githubusercontent.com/superguru/7d2d_mod_BeyondStorage3/refs/heads/master/LICENSE.txt) for details.

## ⌛History

[Undead Legacy](https://www.snowbeegaming.com/undead-legacy) by Subquake was almost certainly the first mod to introduce the "remote broadcasting" of items for crafting, as if you had a gigantic inventory that included your storage crates.

[Craft From Containers](https://www.nexusmods.com/7daystodie/mods/2196?tab=description) by aedenthorn implemented this for later versions of the game, as Undead Legacy was not available for post A21 at the time. The source code is on [GitHub](https://github.com/aedenthorn/7D2DMods)

[1.0 Beyond Storage](https://www.nexusmods.com/7daystodie/mods/5087) was refactored and updated for 7 Days to Die v1 by unv-annihilator and extended this idea, based on the code from aedenthorn. This brought the addition of vehicles to pull items from. The code quality and stability of this mod high. See [https://github.com/unv-annihilator/7D2D_Mods/tree/main](https://github.com/unv-annihilator/7D2D_Mods/tree/main) for that fork.

[Beyond Storage 2](https://github.com/superguru/7d2d_mod_BeyondStorage2) added pulling from all conceivable sources, like drones, workstation outputs, dew collectors. Also added all inventory operations like paying for items at a trader, painting, lock picking. The mod also fixes many notification bugs from the original game, and all operations are seamless. Pushing items from any place you can pull from, in order to auto-sort items, was also added.

## 🚅Why use this mod?

I would recommend Beyond Storage 3 over other "craft from container" type mods, due to speed, stability, functionality, and code quality.

The UI and game functionality integration is exceptional, and the mod is very stable and ⚡⚡⚡lightning⚡⚡⚡ fast.

*** PACKAGED aka MOD DOCUMENTATION PAGE README.md EOF ***