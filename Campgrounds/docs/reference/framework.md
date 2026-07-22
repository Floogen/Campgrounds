# Framework Reference

Everything the framework registers with SMAPI and the game: console commands for testing, item queries for shops, unlock keys for progression, Content Patcher tokens for pack authors, tile actions for mappers and the maps it adds.

All strings on this page are exact unless marked otherwise.

---

## Console commands

Type these into the SMAPI console while the game is running.

| Command | Arguments | Description |
| --- | --- | --- |
| `campgrounds_startcamp` | `<CAMPGROUND_DATA_ID> [TENT_DATA_ID] [GUEST_NAME]` | Starts a camping trip immediately. The fastest way to test a campground without playing to it. |
| `campgrounds_addrations` | `<AMOUNT>` | Adds Camp Rations to the player. Negative values subtract, but see [the note on overspending](#spending-rations). |
| `campgrounds_setcampingtotaldays` | `<TOTAL_DAYS>` | Sets the total nights camped, stored in the vanilla stats system under `PeacefulEnd.Campgrounds_TotalNightsGoneCamping`. |
| `campgrounds_opententmenu` | *(none)* | Opens the `TentListMenu` directly. |

```
campgrounds_startcamp YourName.MyPack_RiverClearing YourName.MyPack_CanvasTent Abigail
```

!!! tip "Pair with Content Patcher's commands"
    `patch reload YourName.MyPack` re-applies your pack without restarting. The framework picks up the invalidated data automatically. Then `campgrounds_startcamp` to look at the result. That loop
    takes seconds.

    `patch summary` tells you whether your patch applied at all. Check it first when nothing seems
    to happen.

---

## Item queries

Use these anywhere the game accepts an [item query](https://stardewvalleywiki.com/Modding:Item_queries), most usefully in [shop data](https://stardewvalleywiki.com/Modding:Shops), so players can buy their way into your content.

| Query | Argument | Returns |
| --- | --- | --- |
| `(O)PeacefulEnd.Campgrounds.Items.CampsiteMap` | `<CAMPGROUND_DATA_ID>` | A campsite map that unlocks that campground. |
| `(O)PeacefulEnd.Campgrounds.Items.CampfireRecipe` | `<CAMPFIRE_FOOD_DATA_ID>` | A recipe that unlocks that campfire food. |
| `(O)PeacefulEnd.Campgrounds.Items.TentSchematic` | `<CAMPING_TENT_DATA_ID>` | A schematic that unlocks that tent. |
| `PeacefulEnd.Campgrounds.Currency.CampRations.Id` | *(none)* | A Camp Rations item. |
| `PeacefulEnd.Campgrounds_UNKNOWN_COOKING_RECIPES` | *(none)* | The day's unknown cooking recipes. Used by the visitor shops. |

Selling a campsite map in a shop:

```json
{
  "Action": "EditData",
  "Target": "Data/Shops",
  "TargetField": [ "SomeShopId", "Items" ],
  "Entries": {
    "{{ModId}}_RiverClearingMap": {
      "Id": "{{ModId}}_RiverClearingMap",
      "ItemId": "(O)PeacefulEnd.Campgrounds.Items.CampsiteMap {{ModId}}_RiverClearing",
      "Price": 5000
    }
  }
}
```

!!! tip "Sell them in the built-in camp shop"
    The framework's own shop, `PeacefulEnd.Campgrounds.Shops.CampShop`, is an ordinary
    [`Data/Shops`](https://stardewvalleywiki.com/Modding:Shops) entry, so you can `EditData` it to stock your campsite maps, tent schematics and recipes right at the caretaker's counter, using the item queries below. That puts your unlockables where players already look for camping supplies.

!!! danger "The argument is not optional"
    The first three queries share their name with the item they create, which is a neat trick but has
    a sharp edge: writing the bare `"ItemId": "(O)PeacefulEnd.Campgrounds.Items.CampsiteMap"` resolves
    as a **query with no arguments**, not as a plain item. That fails and logs a shop error.

    Always pass the Id. And pass a *real* one. All three queries look the argument up in loaded data
    and return an error result if it doesn't resolve, so a typo shows up as a shop error rather than
    silently stocking a broken item. The message names the Id you gave:

    ```
    No campground found with the ID "{your Id}".
    No campfire food found with the ID "{your Id}".
    No camping tent found with the ID "{your Id}".
    ```

### What these items actually do

They aren't keepsakes. The moment one enters the player's inventory the framework consumes it, plays
the "hold above head" animation with a discovery message and sets a **world state flag**, the same pattern the game uses for special items. The item is then removed.

Which means: the item is a delivery mechanism for an unlock flag and the flag is what your content
should actually check.

---

## Unlock keys

When one of the items above is picked up, the framework sets a world state flag built from the data
Id:

| Item | Flag set |
| --- | --- |
| Campsite map | `MAP_UNLOCKED_CAMPGROUND:<CAMPGROUND_DATA_ID>` |
| Campfire recipe | `RECIPE_UNLOCKED_CAMPFIRE_FOOD:<CAMPFIRE_FOOD_DATA_ID>` |
| Tent schematic | `SCHEMATIC_UNLOCKED_TENT:<CAMPING_TENT_DATA_ID>` |

Flags are set **everywhere** (for all players), not per player.

### The buy-to-unlock pattern

This is the main progression loop the framework is built around and nothing in the models hints at it. Check the flag with the vanilla
[`WORLD_STATE_ID`](https://stardewvalleywiki.com/Modding:Game_state_queries) query in your entry's
`UnlockCondition`:

```json
{
  "Id": "{{ModId}}_RiverClearing",
  "UnlockCondition": "WORLD_STATE_ID MAP_UNLOCKED_CAMPGROUND:{{ModId}}_RiverClearing",
  "UnlockHint": "Maybe someone in town sells maps.",
  "PlayerSpawnTile": { "X": 12, "Y": 18 },
  "GuestSpawnTile": { "X": 14, "Y": 18 }
}
```

Sell the matching map in a shop and the campground unlocks when the player buys it. The same shape
works for tents (`SCHEMATIC_UNLOCKED_TENT:`) and foods (`RECIPE_UNLOCKED_CAMPFIRE_FOOD:`).

!!! tip "The Id appears four times, keep them identical"
    The `Entries` key, the entry's `Id`, the item query argument and the `UnlockCondition` flag all embed the same Id. Using `{{ModId}}` in each keeps them in sync.

    **Casing is forgiving where it's looked up, strict where it's compared.** The item query and
    event command match your Id case-insensitively and the flag they set always uses the canonical
    casing from your entry's `Id`, so `campsitemap yourname.mypack_riverclearing` still unlocks
    `YourName.MyPack_RiverClearing`. But `WORLD_STATE_ID` compares the flag as plain text, so the Id
    in your `UnlockCondition` must match your entry's `Id` exactly, casing included.

    In practice both live in the same file and both use `{{ModId}}`, so they agree by construction.
    It helps to know which half is which if you ever hand-write one of them.

!!! note "Don't put spaces in your Ids"
    Game state query arguments are space-delimited, so a campground Id containing a space would split
    the flag across two arguments and never match. Ids shouldn't have spaces anyway. This is one more reason.

---

## Currency

| Constant | Value | Purpose |
| --- | --- | --- |
| Currency / item query ID | `PeacefulEnd.Campgrounds.Currency.CampRations.Id` | Registered both as the `specialCurrencyDisplay` key and as an item query. |
| Balance storage key | `PeacefulEnd.Campgrounds.Currency.CampRations.Balance` | Where the balance lives in `Game1.player.modData`. |

The balance is clamped at a minimum of 0. Spending more than the player has fails rather than going negative.

### The currency icon

The Camp Rations icon is drawn from a patchable asset:

```
Data/PeacefulEnd.Campgrounds/Campgrounds/Textures/CurrencyIcons
```

Camp Rations use the `16×16` sprite at `(0, 0)`. You can retexture it with an `EditImage` patch:

```json
{
  "Action": "EditImage",
  "Target": "Data/PeacefulEnd.Campgrounds/Campgrounds/Textures/CurrencyIcons",
  "FromFile": "assets/rations_icon.png",
  "ToArea": { "X": 0, "Y": 0, "Width": 16, "Height": 16 }
}
```

You can also read a player's ration balance from a Game State Query without any framework support,
since it's stored in `modData`. But mind the comparison:

```
PLAYER_MOD_DATA Current PeacefulEnd.Campgrounds.Currency.CampRations.Balance 25
```

!!! warning "This is an exact string match, not a threshold"
    The balance is stored as the integer's text form (`_campRations.ToString()`) and
    [`PLAYER_MOD_DATA`](https://stardewvalleywiki.com/Modding:Game_state_queries) compares it as a
    **string**. The query above matches a balance of *exactly* `25`, not "25 or more". There's no
    built-in way to express "at least N rations" as a GSQ against this key. If you need a threshold, gate on something else (a purchased [unlock item](#unlock-keys), the [nights-camped stat](#stats)) instead.

    Inside the framework's own menus the balance is compared numerically, so affordability checks
    there behave as you'd expect. This caveat is only about GSQs reading the raw stored value.

---

## Stats

The framework records several stats in the game's vanilla stats system. Each is a plain integer you can read from a Game State Query or display through a [Content Patcher token](#content-patcher-tokens). The first three are cumulative tallies. The last three are live counts of how much content is currently unlocked.

| Stat ID | Meaning |
| --- | --- |
| `PeacefulEnd.Campgrounds_TotalNightsGoneCamping` | Total nights the player has spent camping. |
| `PeacefulEnd.Campgrounds_TotalGuestsInvited` | Total guests brought on overnight trips. Counted per guest when a trip is completed, so re-inviting the same guest on a later trip adds to it again. Inviting a guest without camping overnight doesn't count. |
| `PeacefulEnd.Campgrounds_TotalCampMealsMade` | Total meals cooked at the campfire across all trips. Cooking happens once per trip but a single session can produce several meals at once. Each meal made adds to the total. |
| `PeacefulEnd.Campgrounds_TotalCampsitesUnlocked` | How many campgrounds are currently unlocked. |
| `PeacefulEnd.Campgrounds_TotalTentsUnlocked` | How many tent schematics are currently unlocked. |
| `PeacefulEnd.Campgrounds_TotalRecipesUnlocked` | How many campfire recipes are currently unlocked. |

Because these are vanilla stats, you can gate content on them from a Game State Query with
[`PLAYER_STAT`](https://stardewvalleywiki.com/Modding:Game_state_queries), for example unlocking a campground only after the player has camped a few times:

```json
{ "UnlockCondition": "PLAYER_STAT Current PeacefulEnd.Campgrounds_TotalNightsGoneCamping 5" }
```

---

## Content Patcher tokens

The framework registers six [custom Content Patcher tokens](https://github.com/Pathoschild/StardewMods/blob/stable/ContentPatcher/docs/extensibility.md), one for each [stat](#stats) above. Use them to drop the current value straight into any text field.

| Token | Value |
| --- | --- |
| `PeacefulEnd.Campgrounds/TotalNightsGoneCamping` | Nights the player has spent camping. |
| `PeacefulEnd.Campgrounds/TotalGuestsInvited` | Guests brought on completed overnight trips (repeat invites count). |
| `PeacefulEnd.Campgrounds/TotalCampMealsMade` | Meals cooked at the campfire across all trips. |
| `PeacefulEnd.Campgrounds/TotalCampsitesUnlocked` | Campgrounds currently unlocked. |
| `PeacefulEnd.Campgrounds/TotalTentsUnlocked` | Tent schematics currently unlocked. |
| `PeacefulEnd.Campgrounds/TotalRecipesUnlocked` | Campfire recipes currently unlocked. |

Each token is prefixed with the framework's mod ID (`PeacefulEnd.Campgrounds/`) and resolves to the stat's current value as a number. Any pack that already lists Campgrounds as a dependency can use them with no extra setup:

```json
{
  "Action": "EditData",
  "Target": "Characters/Dialogue/Robin",
  "Entries": {
    "Mon": "You've camped {{PeacefulEnd.Campgrounds/TotalNightsGoneCamping}} nights now? Sounds like you've earned a rest."
  }
}
```

!!! tip "Token to display, `PLAYER_STAT` to gate"
    These tokens are for *showing* a number in text. To gate content on a threshold (such as only after ten nights), use the [`PLAYER_STAT`](#stats) Game State Query instead.

!!! note "Availability and freshness"
    The tokens return nothing on the title screen (before a save loads), so any patch relying on them is inactive there. Their value refreshes on Content Patcher's context updates, not continuously. For nights, guests and meals that makes no difference, since those only change on a day change. The three unlock counts are the exception: the framework recomputes them about once a second, so their real value can change mid-day the moment the player unlocks something, but a token displaying one won't catch up until the next context update. If you need an unlock count the instant it changes, read the stat via C#.

---

## Player modData keys

| Key | Contents |
| --- | --- |
| `PeacefulEnd.Campgrounds.Currency.CampRations.Balance` | The player's Camp Rations balance. |
| `Campgrounds.NextVisit.Cooldown.Id` | A JSON object mapping each `VisitorData.Id` to the day its [visit cooldown](../content-packs/visitors.md#repeat-visits) expires, as a days-since-start number. Written on save, read each morning. |
| `Campgrounds.NextInvite.Cooldown.<VillagerName>` | The day a villager's [invite cooldown](../content-packs/villagers.md#invite-cooldown) expires, as a days-since-start number. One key per villager, written when a trip is taken. |

---

## The intro event

Three of the tile actions below refuse to do anything until the player has seen:

```
PeacefulEnd.Campgrounds.Events.CampingIntro
```

Until then they show a "talk to Clem" message instead. If you're testing and the camp list won't
open, this is why.

Check it yourself with `PLAYER_HAS_SEEN_EVENT Current PeacefulEnd.Campgrounds.Events.CampingIntro`.

---

## Tile actions

For **map authors**. Set as the `Action` tile property on the `Buildings` layer.

| Action | Arguments | Description |
| --- | --- | --- |
| `PeacefulEnd.Campgrounds_CampingSiteList` | *(none)* | Opens `CampListMenu`, where the player chooses a campground. Requires the intro event. |
| `PeacefulEnd.Campgrounds_CarRepair` | *(none)* | Offers the garage car repair. Requires the intro event, plus the materials below. |
| `PeacefulEnd.Campgrounds_RepairVisitorSite` | `<SITE_ID>` | Offers to repair visitor site `1`, `2` or `3`. Requires the intro event, plus the materials below. |
| `PeacefulEnd.Campgrounds_CampShopCounter` | *(none)* | Opens the camp shop counter, a dialogue offering supplies, the tent catalogue or leaving. The supplies option opens the shop `PeacefulEnd.Campgrounds.Shops.CampShop`, run by the caretaker `PeacefulEnd.Campgrounds.Characters.Caretaker`. |

```
PeacefulEnd.Campgrounds_RepairVisitorSite 1
```

!!! warning "An invalid site ID does nothing, quietly"
    `_RepairVisitorSite` switches on `1`, `2` and `3`. Any other number falls through the switch and
    the action returns success with no dialogue, no repair and no error. If your repair tile does
    nothing when interacted with, check the number.

### Repair costs

Each repair is only *offered* if the player is carrying the materials:

| Repair | Materials |
| --- | --- |
| Garage car | 5× Battery Pack `(O)787`, 10× Iron Bar `(O)335`, 100× Wood `(O)388` |
| Visitor site 1 | 2× Iron Bar `(O)335`, 100× Wood `(O)388` |
| Visitor site 2 | 5× Iron Bar `(O)335`, 25× Hardwood `(O)709`, 200× Wood `(O)388` |
| Visitor site 3 | 5× Pine Tar `(O)726`, 15× Iron Bar `(O)335`, 50× Hardwood `(O)709`, 200× Stone `(O)390` |

Otherwise the player gets a "missing items" message. Note site 3 wants **Stone**, not Wood, unlike
the other two.

## Touch actions

Set as the `TouchAction` tile property.

| Action | Description |
| --- | --- |
| `PeacefulEnd.Campgrounds_ParkClosed` | Shows the "park pathway closed" message. |
| `PeacefulEnd.Campgrounds_CampingExit` | Ends the camping trip. Offers to head back to the park, return to the farm or cancel. |

### The leave-early warning

`_CampingExit` has an extra confirmation step that protects the player from wasting a meal.

Cooking a campfire meal doesn't apply its buffs immediately. The buffs are **cached and applied the next morning** (they're the reward for staying the night). So if the player has cooked but then tries
to leave the same day, the framework shows a "leave without camping?" Yes/No prompt first:

- **Yes**: the cached meal is discarded (`ClearBuffs`) and the player exits. They cooked, but they
  leave before the buff pays out, so they lose it.
- **No**: the player stays at the campsite.

If the player *hasn't* cooked yet, there's nothing to lose, so this prompt is skipped and they go
straight to the exit menu below.

### The exit menu

Once past that check (or straight away, if nothing was cooked), the player chooses:

| Option | Result |
| --- | --- |
| Head back to the park | Ends the trip and warps to Cindersap Park. |
| Return to the farm | Ends the trip and warps to the farmhouse front door. |
| Cancel | Stays at the campsite. |

---

## Map tile properties

For **map authors** and for anyone writing an [advanced visitor](../content-packs/visitors.md#advanced-visitors), because this is how those spawn their NPCs.

The framework scans the park map every morning and reads these properties.

### Back layer

| Property | Value | Purpose |
| --- | --- | --- |
| `IsVisitorSpawn` | *(any, presence is what counts)* | Marks a spawn point for a [standard visitor](../content-packs/visitors.md#standard-visitors). Must be paired with `VisitorSpot`. |
| `VisitorSpot` | `SW`, `NW` or `SE` | Which site this spawn point belongs to. Parsed case-insensitively. |
| `SpawnVisitorName` | An NPC's internal name | Spawns that NPC on this tile, unconditionally. |
| `VisitorFacingDirection` | `North`, `East`, `South`, `West` | Which way the spawned NPC faces. Case-insensitive, **defaults to `North`** if absent or unparseable. |

`IsVisitorSpawn` + `VisitorSpot` and `SpawnVisitorName` are two separate mechanisms:

- **`IsVisitorSpawn` + `VisitorSpot`** is a *slot*. It's only used if a standard visitor is active at
  that site. The framework decides who fills it. Place several per site (they're shuffled) and the
  visitor is placed on one of them at random.
- **`SpawnVisitorName`** is a *named placement*. It spawns that specific NPC every day, regardless of
  which visitor was selected or whether any site is repaired. This is how an advanced visitor's
  patched map puts its own characters on the map.

!!! warning "A bad `VisitorSpot` value silently degrades"
    The two mechanisms are checked as `if`/`else if` on the same tile. A tile with `IsVisitorSpawn`
    whose `VisitorSpot` doesn't parse falls through to the `SpawnVisitorName` branch and if that's absent too, the tile does nothing at all. No warning is logged. If a spawn point is being ignored,
    check the spelling of `VisitorSpot` first.

Spawned NPCs are **frozen** in place for the day. Any NPC in the park that isn't spawned this way is
returned to its normal schedule each morning, except the park's caretaker,
`PeacefulEnd.Campgrounds.Characters.Caretaker`, which is left alone.

### Campground maps

The tile properties above are read on the **park** map. A separate set is read on each **campground**
map (the map a visitor travels *to*) to place the tents and campfire. All are on the `Back` layer,
and **all three positions are mandatory**: a campground map missing any of them fails setup, logs a warning and the trip is aborted.

| Property | Value | Purpose |
| --- | --- | --- |
| `IsCampingSpot` | *(any, presence counts)* | Marks where a tent is pitched. Read twice (see `IsForGuest`). |
| `IsForGuest` | `T` on the guest's tent tile, absent on the player's | Distinguishes the two `IsCampingSpot` tiles. |
| `CampingDirection` | [`Direction`](enums.md#direction) | Which way that tent faces. Defaults to `South` if absent or unparseable. |
| `IsCookingSpot` | *(any, presence counts)* | Marks the campfire / cooking spot. |

So a campground map needs **three** tagged tiles: a player `IsCampingSpot`, a guest `IsCampingSpot` with `IsForGuest` = `T` and an `IsCookingSpot`.

!!! danger "The guest tent tile is required even for solo sites"
    Setup checks for the player tile, the guest tile and the cooking tile in that order and bails
    on the first one missing. **The guest tile is required whether or not a guest ever comes**. Omit
    it and the site won't load even when the player travels alone. The exact warnings are:

    ```
    The campgrounds map with name {id} is missing the player's tent spot (IsCampingSpot tile property on Back layer)
    The campgrounds map with name {id} is missing the guest's tent spot (IsCampingSpot and IsForGuest tile property on Back layer)
    The campgrounds map with name {id} is missing a cooking spot (IsCookingSpot tile property on Back layer)
    ```

    If a campground silently refuses to load, check the SMAPI log for one of these three.

!!! note "This is what `CampgroundMapDetails` is"
    The framework reads these tiles into a `CampgroundMapDetails` object at runtime. That model isn't something you write in JSON. See [the reference note](index.md#campgroundmapdetails).

### Buildings layer

| Property | Value | Purpose |
| --- | --- | --- |
| `IsCampfire` | *(any, presence is what counts)* | Adds a campfire light source on this tile. |
| `HasSmoke` | *(any, presence is what counts)* | Emits a smoke puff roughly once a second. **Only read on a tile that also has `IsCampfire`.** |

The light is a sconce-type source with radius `2.5` and colour `(0, 80, 160)`. Smoke puffs are offset
`+24` px right and `-16` px up from the tile's top-left corner, so they rise from roughly the middle
of the tile.

!!! tip "`HasSmoke` without `IsCampfire` does nothing"
    `HasSmoke` is only checked inside the `IsCampfire` branch. A smoking chimney with no campfire
    beneath it needs both properties on the same tile.

Both are rescanned each morning after the map is invalidated, so a campfire your visitor's map patch
brings in will light up on the day that visitor arrives.

---

## Event commands

For use in [event scripts](https://stardewvalleywiki.com/Modding:Event_data).

| Command | Arguments | Description |
| --- | --- | --- |
| `PeacefulEnd.Campgrounds_GiveRations` | `<AMOUNT>` | Changes the player's Camp Rations balance by `<AMOUNT>`. |
| `PeacefulEnd.Campgrounds_GiveCampsiteMap` | `<CAMPGROUND_DATA_ID>` | Unlocks that campground, with the discovery animation and message. |
| `PeacefulEnd.Campgrounds_GiveCampfireRecipe` | `<CAMPFIRE_FOOD_DATA_ID>` | Unlocks that campfire recipe, with the discovery animation and message. |
| `PeacefulEnd.Campgrounds_GiveTentSchematic` | `<CAMPING_TENT_DATA_ID>` | Unlocks that tent schematic, with the discovery animation and message. |

```
PeacefulEnd.Campgrounds_GiveRations 25
PeacefulEnd.Campgrounds_GiveCampsiteMap YourName.MyPack_RiverClearing
PeacefulEnd.Campgrounds_GiveCampfireRecipe YourName.MyPack_ToastedMarshmallow
PeacefulEnd.Campgrounds_GiveTentSchematic YourName.MyPack_CanvasTent
```

A missing argument, a malformed one or a campground Id that doesn't match any loaded data logs an error and skips the command. The event carries on from the next line.

### `_GiveCampsiteMap` doesn't give an item

Despite the name, this command never puts a campsite map in the player's inventory. It does what
picking one up *would* do, minus the item: sets the
[`MAP_UNLOCKED_CAMPGROUND:<id>`](#unlock-keys) world state flag, plays the hold-above-head animation and shows the discovery message.

So it's the event-scripting equivalent of the item. Reach for it when a character *hands* the player
a campsite in a cutscene and for the [item query](#item-queries) when they buy one.

The Id is matched **case-insensitively**, consistent with `campgrounds_startcamp` and the item
queries. An Id that matches nothing is reported as:

```
No campground found with the ID "{your Id}".
```

The command is then skipped rather than half-applied. The player gets no unlock and no message and
the event continues.

`_GiveCampfireRecipe` and `_GiveTentSchematic` behave the same way for their content, setting the
matching [`RECIPE_UNLOCKED_CAMPFIRE_FOOD:<id>`](#unlock-keys) or [`SCHEMATIC_UNLOCKED_TENT:<id>`](#unlock-keys)
world state flag with the same animation and message and likewise never hand over an item. To place an
actual item in a cutscene, spawn the [item](#items-and-tools) instead.

---

## Spending rations

`campgrounds_addrations` and `PeacefulEnd.Campgrounds_GiveRations` both go through the same balance
change and both accept negative amounts.

!!! warning "An overspend is rejected, not clamped"
    If a subtraction would take the balance below zero, the framework **rejects the whole change** and
    leaves the balance untouched. It does not clamp to 0 and it does not partially spend.

    So `PeacefulEnd.Campgrounds_GiveRations -100` against a balance of 50 leaves the player with 50,
    not 0. The event command reports this and skips:

    ```
    Unable to change the player's Camp Rations balance by -100, as their current balance is 50.
    ```

    The console command doesn't report it. It's a testing tool, so check the balance yourself if a
    `campgrounds_addrations` with a negative number appears to do nothing.

If an event charges the player rations, gate it on the balance rather than assuming the charge
landed. The balance is readable from a Game State Query via
[`PLAYER_MOD_DATA`](#player-moddata-keys), subject to the caveat noted there.

---

## Items and tools

| Id | Description |
| --- | --- |
| `(O)PeacefulEnd.Campgrounds.Items.CampsiteMap` | Campsite map. Consumed on pickup, sets `MAP_UNLOCKED_CAMPGROUND:`. |
| `(O)PeacefulEnd.Campgrounds.Items.CampfireRecipe` | Campfire recipe. Consumed on pickup, sets `RECIPE_UNLOCKED_CAMPFIRE_FOOD:`. |
| `(O)PeacefulEnd.Campgrounds.Items.TentSchematic` | Tent schematic. Consumed on pickup, sets `SCHEMATIC_UNLOCKED_TENT:`. |
| `PeacefulEnd.Campgrounds.Tools.WalkieTalkie` | Walkie-talkie tool. Using it asks whether to start the cutscene showing who's at the park. The prompt differs before and after 12:00. |

Each of the three unlock items carries its target Id in `modData` under
`Campgrounds.Items.CampsiteMap.Data.Id`, `Campgrounds.Items.CampfireRecipe.Data.Id` or `Campgrounds.Items.TentSchematic.Data.Id` respectively. The item's displayed name and description are
generated from that Id at runtime, so a map for your campground reads as your campground's name
without you defining an item for it.

---

## Maps and locations

| Asset / location | Description |
| --- | --- |
| `Maps/PeacefulEnd.Campgrounds.ContentPatcher_CindersapPark` | The park map. Edit this to change the park itself. |
| `Maps/PeacefulEnd.Campgrounds.ContentPatcher_CindersapParkOvergrown` | The overgrown source map, patched over any unrepaired visitor site. |
| `Maps/PeacefulEnd.Campgrounds.ContentPatcher_CindersapParkActive` | The active park map. |
| `PeacefulEnd.Campgrounds.ContentPatcher_CindersapParkGarage` | The garage location, where the car is repaired. |

### The park's visitor sites

The framework edits the park map on load, patching overgrown tiles over each visitor site the player
hasn't repaired. The areas it uses, in tile coordinates:

| Site ID | [`RequiredSpot`](enums.md#visitorspots) | Area patched when unrepaired | Position |
| --- | --- | --- | --- |
| `1` | `SW` | `X: 0, Y: 25, Width: 22, Height: 26` | Bottom-left |
| `2` | `NW` | `X: 0, Y: 0, Width: 35, Height: 25` | Top-left |
| `3` | `SE` | `X: 40, Y: 0, Width: 29, Height: 51` | Right-hand side |

The site ID is what `PeacefulEnd.Campgrounds_RepairVisitorSite` takes as its argument. The `RequiredSpot` column is what an [advanced visitor](../content-packs/visitors.md#advanced-visitors) writes to claim the same site. Repairing a site sets the world state flag
`CINDERSAP_PARK_VISITOR_SITE_<id>`, so you can gate content on a site being open with, for example,
`WORLD_STATE_ID CINDERSAP_PARK_VISITOR_SITE_1` for the `SW` site.

### How a site's map is assembled

Each site's appearance is the result of up to two patches layered onto the base park map:

| Site state | What's patched over it |
| --- | --- |
| Not repaired | The **overgrown** map, with `Replace`. |
| Repaired, no visitor available today | Nothing (the base park map shows through). |
| Repaired, [standard visitor](../content-packs/visitors.md#standard-visitors) today | The **active** map, with `Replace`. |
| Repaired, [advanced visitor](../content-packs/visitors.md#advanced-visitors) today | That visitor's own `MapPatches`, each with its own `PatchMode`. |

The park map is invalidated every morning, so this is re-evaluated daily.

That's the practical difference between the two visitor types: a standard visitor borrows the park's
built-in campsite, an advanced visitor brings its own.

!!! note "Overgrowth patches use `Replace`"
    The overgrowth is applied with [`PatchMapMode.Replace`](enums.md#patchmapmode), so it clears the
    site completely. Anything your pack adds to an unrepaired site's area on the base park map is
    wiped until the player repairs it. Patch the site through
    [`MapVisitorSettings`](../content-packs/visitors.md#advanced-visitors) rather than editing the
    park map directly.

---

## Other behaviour to know

- **Bushes at the park entrance are removed on save load.** The framework clears `Bush` large terrain
  features in the `Forest` location within `X: 8, Y: 0, Width: 5, Height: 12` and logs how many it
  removed. Only bushes are removed, so anything else you place there survives.
- **The garage car is repaired on save load** if the player has already earned it.
