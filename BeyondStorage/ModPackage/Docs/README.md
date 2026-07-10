# Beyond Storage 3 - Everything Is Your Inventory
I would appreciate it if you could [support me on Ko-fi](https://ko-fi.com/gazorper), as **hundreds of hours** go into making a mod like this, keeping it up to date with newer game versions, not to mention supporting it.

[![Support me on Ko-fi](https://raw.githubusercontent.com/superguru/7d2d_mod_BeyondStorage3/refs/heads/master/BeyondStorage/Media/gazorper-kofi.png)](https://ko-fi.com/gazorper)

Please go to <span style="color:#4c5fd7;">**#beyond-storage3**</span> on the <span style="color:#f46f30;">**Discord server**</span> for mod support.

Also see [Docs\README.md](https://github.com/superguru/7d2d_mod_BeyondStorage3/blob/master/BeyondStorage/ModPackage/Docs/README.md) in the mod package for the full documentation.

You should also look at the Media Gallery on the mod distribution site where you downloaded this mod from.

## 💾What to download

- V3: Use the main download for the latest version of 7 Days to Die v3.x (all builds)
- V2: For older versions of the game, like 7 Days v2.x, see Old Files section
 
## What can this do?
### Extended Inventory - Everything Is Your Inventory (aka inventory network)
Items that exist in any of these places are now added to your Backpack and Toolbelt, and become part of your **extended inventory**:
1. Drones
2. Collectors, such as Apiaries and Dew Collectors
3. Workstations like Campfire, Forge, Dew Collector, Apiary, Workbench, Chem Station, Cement Mixer, and so on
4. Containers like crates, and things like refrigerators, wall safes, lockers, etc. that you crafted and placed
5. Vehicles like Bicycle, 4x4, Minibike, Motorcycle, Gyrocopter, and so on

This is also the order in which items are consumed from, so Backpack and Toolbelt items are used first, just like in the base game, and after that Drones, then Collectors, etc.

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

### Pushing and Locked Slots
Locking a slot means that you are turning Off any bulk transfers from that slot, just like in the base game.
So if you always want to keep Wood in your Backpack, lock the slot it's in, and Smart Push will leave it alone.
![Locked Backpack Slots](https://raw.githubusercontent.com/superguru/7d2d_mod_BeyondStorage3/refs/heads/master/BeyondStorage/Media/locked_slots_backpack.jpg)
This is my Backpack top row from a current game. None of these items will be bulk moved out of the Backpack, whether using the game *Move All* or *Move and Fill* buttons, or using Smart Push.
The 62 Polymers is not locked, so they can be moved is if there is a storage somewhere that already has some Polymers in it.

### Smart Pull, aka Topping up your Load Out
If you lock some slots in your Backpack (Player Inventory), or in Drones, vehicles like Motorcycle, Gyrocopter, and so on, then you can use those as your **Load Out** slots.
Using the Smart Pull button will top up any items in those slots with any available items.
![Smart Pull Loadout to Bicycle](https://raw.githubusercontent.com/superguru/7d2d_mod_BeyondStorage3/refs/heads/master/BeyondStorage/Media/smart_pull_bicycle_loadout.jpg)
This is my trusty Bicycle. I always keep Coins, Repair Kits, Lock picks, Shotgun Ammo, and Arrows in there.
I never keep any of these items in my own inventory, so my Backpack is just for looted items and whatnot.
Before leaving the base, I just use Smart Pull to Top Up my Bicycle Load Out, and I have a fresh load of bullets and other needful things to go **On Mission**.

### Gotchas about Smart Pull
Items are only pulled from immovable storage like crates and other user created and placed containers.
So <span style="color:#ff2222;">**you can't pull in**</span> ammo from your Bicycle into your Drone, for instance.
Pulling in items will not overflow to empty slots, whether they're locked or not. All it does is fill up a slot with an existing item until the maximum stack size for that item is reached.
![Img](https://raw.githubusercontent.com/superguru/7d2d_mod_BeyondStorage3/refs/heads/master/BeyondStorage/Media/player_crafted_office_desk_example.jpg)
This is my office desk that I **crafted and placed myself**. It is also part of the extended inventory network. The same Wall Safe from the earlier example is just above it.

### On Mission, aka cool things you didn't know you could do
If you are out of range of your base, then you're classified to be **On Mission**.
In this situation, pushing from your Backpack (Player Inventory) will use your vehicles and drones that are nearby as the destination to bulk move items to if there are already items of that type in them.
![Bicycle Prefilled On Mission](https://raw.githubusercontent.com/superguru/7d2d_mod_BeyondStorage3/refs/heads/master/BeyondStorage/Media/bicycle_prefilled_on_mission.jpg)
Some random stuff I looted on the way to a job and then placed in the Bicycle storage.
When I do the job, I can Smart Push these kinds of items directly to the Bicycle from within the POI. No need to run out to the vehicle, unless it's a new type of item.
Don't waste your time making and dealing with dump chests!

## Console and Config

### ⌨️Console Commands (when you press F1)

|Command|Description|
|:----|:----|
| bshelp | List available commands with their descriptions |
| bsclearcache | Invalidates cache and reloads items from storages |
| bsreloadconfig | Reload the config as per the current config.json file (maybe you modified it in a text editor) |
| bssetconfig | Change the value of a config property |
| bsshowconfig | Displays the current active config settings |

### ❄️Config file

The mod can also be configured by editing `Mods/BeyondStorage3/config.json`

|Setting|Default|Description|
|:----|:----|:----|
| range | -1.0 | Distance in metres for Consume, Smart Push, or SmartPull. Less than 0 means everything loaded by the game, which is effectively around 200m. |
| consumeFromDrones | true | |
| consumeFromVehicles | true | |
| serverSyncConfig | true | Force clients to load the mod config settings from the server when a player connects |
| isDebug | false | Logs additional information that might be useful for troubleshooting problems. You can generally leave this turned off. |

Any setting not listed here is either old, or otherwise is for mod development purposes. It's best to leave them alone.

The mod will automatically convert older properties and values, removing invalid ones as needed, when it loads.

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

Please report any bugs or issues you find on [Discord at GAZ World](https://discord.gg/hAF5T4P9pE), and I'll investigate it when I find the time.

The people on Discord are quite helpful too.

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

