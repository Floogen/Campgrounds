# A Complete Example

This page walks through a **fully working pack** (*Cindersap Forest Campground*), from the `manifest.json` to the last map patch, so you can
see how the pieces fit together and copy the shape.

The pack is the framework's own reference content, so what you see here is the intended, idiomatic way
to build one.

!!! tip "Read alongside the pack"
    If you have the example pack itself, keep it open next to this page. This walks the same files in
    load order. The pack has the full, unabridged JSON (with the literal IDs in place of the
    `{{ModId}}_` placeholders used here).

---

## The shape of the pack

```
[CP] Cindersap Forest Campground/
├── manifest.json
├── content.json                    the entry point (wires everything together)
├── i18n/
│   └── default.json                every DisplayName / dialogue string
└── assets/
    ├── maps/                        the campground map, visitor-site maps, tilesheets
    └── campgrounds/
        ├── campgroundPreview.png
        ├── tents/       campingTents.json + tent textures
        ├── foods/       campfireFoods.json + icon sheet
        ├── villagers/   campingVillagers.json
        ├── visitors/    parkVisitors.json
        └── shops/       shops.json
```

The pack keeps each content type in its own file and stitches them together from `content.json` with
`Include`. This is optional and not a requirement. It helps keep the structure organized by doing so, however.

---

## 1. The manifest

```json title="manifest.json"
{
  "Name": "[CP] Cindersap Forest Campground",
  "Author": "PeacefulEnd",
  "Version": "1.0.0",
  "UniqueID": "PeacefulEnd.Campgrounds.CindersapForestCampground",
  "MinimumApiVersion": "4.1.10",
  "UpdateKeys": [ "Nexus:????" ],
  "Dependencies": [
    { "UniqueID": "PeacefulEnd.Campgrounds.Core", "IsRequired": true, "MinimumVersion": "1.0.0" },
    { "UniqueID": "PeacefulEnd.Campgrounds.ContentPatcher", "IsRequired": true, "MinimumVersion": "1.0.0" }
  ],
  "ContentPackFor": { "UniqueID": "Pathoschild.ContentPatcher" }
}
```

The important part is the split between `ContentPackFor` and `Dependencies` (covered in full on the
[Getting Started](index.md#2-write-the-manifest) page), the pack is *for* Content Patcher and *depends
on* the framework's core and Content Patcher component.

---

## 2. content.json

`content.json` does five things, in order.

### It creates the campground's location

A campground **is a game location**. Before anything else, the pack registers one through vanilla
`Data/Locations`, keyed by the same Id the `CampgroundData` will use:

```json title="content.json (abridged)"
{
  "Action": "EditData",
  "Target": "Data/Locations",
  "Entries": {
    "{{ModId}}_CindersapForestCampground": {
      "DisplayName": "{{i18n:map.cindersapForestCampground.name}}",
      "CreateOnLoad": { "MapPath": "Maps/{{ModId}}_CindersapForestCampground" },
      "ExcludeFromNpcPathfinding": true,
      "Fish": [ /* ... */ ],
      "Forage": [ /* ... */ ],
      "MinDailyForageSpawn": 4,
      "MaxDailyForageSpawn": 6,
      "MaxSpawnedForageAtOnce": 6
    }
  }
}
```

Note where the campground's **display name** lives: on the *location*, not the `CampgroundData`. The
location also carries its own `Fish` and `Forage` tables (like any Stardew Valley location). Then the map itself is loaded:

```json
{ "Action": "Load", "Target": "Maps/{{ModId}}_CindersapForestCampground", "FromFile": "assets/maps/CindersapCampground.tmx" }
```

### It registers the campground with the framework

The `CampgroundData` entry appears and points back at that same location Id:

```json
{
  "Action": "EditData",
  "Target": "Data/PeacefulEnd.Campgrounds/Campgrounds",
  "Entries": {
    "{{ModId}}_CindersapForestCampground": {
      "Id": "{{ModId}}_CindersapForestCampground",
      "ForceForageRefreshOnVisit": true,
      "PreviewTexturePath": "Mods/{{ModId}}/CindersapForestCampgroundPreview",
      "PlayerSpawnTile": { "X": 25, "Y": 18 },
      "GuestSpawnTile": { "X": 17, "Y": 23 },
      "MaxTentTileSize": { "Width": 5, "Height": 5 },
      "Description": "A peaceful camping spot in Cindersap Forest.",
      "TravelTimeInHours": 0,
      "UnlockCondition": "WORLD_STATE_ID MAP_UNLOCKED_CAMPGROUND:{{ModId}}_CindersapForestCampground",
      "UnlockHint": "Unlocked after talking with the Cindersap Park Ranger."
    }
  }
}
```

That `UnlockCondition` is the [buy-to-unlock loop](../content-packs/index.md#unlocking-with-a-purchasable-item)
in action. It checks the exact flag a campsite map sets and the map that sets it is sold in the shop
(step 5).

### It includes the rest

The remaining content types are pulled in as separate files:

```json
{ "Action": "Include", "FromFile": "assets/campgrounds/tents/campingTents.json" },
{ "Action": "Include", "FromFile": "assets/campgrounds/foods/campfireFoods.json" },
{ "Action": "Include", "FromFile": "assets/campgrounds/villagers/campingVillagers.json" },
{ "Action": "Include", "FromFile": "assets/campgrounds/visitors/parkVisitors.json" },
{ "Action": "Include", "FromFile": "assets/campgrounds/shops/shops.json" }
```

An `Include` file is just a `content.json` fragment with its own `Changes` array. This is done for organizational purposes and to reduce the bloat of 'content.json'.

---

## 3. Tents: three tiers of progression

`campingTents.json` defines three tents that form a progression:

| Tent | Meals | Resting buff | How it's unlocked |
| --- | --- | --- | --- |
| Starter | 1 | none | Always available (it's the starter tent) |
| Standard | 1 | `LowXPBuff` | Buy the schematic |
| Sturdy | 2 | `LowXPBuff` | Buy the schematic **after camping 10 times** |

The Standard tent shows the full unlock shape:

```json
{
  "Id": "{{ModId}}_StandardTent",
  "DisplayName": "{{i18n:campingTents.standardTent.name}}",
  "NumberOfAllowedCampfireMeals": 1,
  "RestingBuffs": [ { "Id": "{{ModId}}_LowXPBuff" } ],
  "UnlockCondition": "WORLD_STATE_ID SCHEMATIC_UNLOCKED_TENT:{{ModId}}_StandardTent",
  "UnlockHint": "Purchase the schematic from the Cindersap Forest Park shop.",
  "TexturePath": "Mods/{{ModId}}/StandardTent",
  "GrayscaleTexturePath": "Mods/{{ModId}}/StandardTentGrayscale",
  "PreviewOffset": { "X": 0, "Y": -20 },
  "SouthSprite": { "...": "..." }
}
```

A few things the pack demonstrates that are easy to miss:

- **`EntranceTile` and offsets are pixels.** The starter tent's south entrance is
  `{ "X": 16, "Y": 48 }`, which is three tiles' worth of pixels (not tile `16`). See
  [the tents reference](../content-packs/tents.md#directionalspritemodel).
- **The sturdy tent halves its spritesheet with a flip.** Its `EastSprite` sets
  `"FlipHorizontally": true` and reuses the west sprite's `DisplayRectangle` and gives east its own
  `EntranceTile`, because the flip doesn't move the entrance.
- **Each tent loads its own textures** right after its `EditData`, so the whole tent (data +
  sprite) lives in one block.
- **The starter tent has no `UnlockCondition`.** It's the fallback every villager and every player
  starts with, so it's always available.

---

## 4. Foods, villagers, visitors

### Foods

Three meals, each `RationCost: 1` and all from one 16×16 icon sheet by `SourceRectangle`. Two are
free from the start (`"UnlockCondition": ""`) while one is bought as a recipe:

```json
{
  "Id": "{{ModId}}_SausageOnAStick",
  "RationCost": 1,
  "TexturePath": "Mods/{{ModId}}/DefaultCampfireFoods",
  "SourceRectangle": { "X": 32, "Y": 0, "Width": 16, "Height": 16 },
  "Buffs": [ { "Id": "{{ModId}}_LowStaminaBuff" } ],
  "UnlockCondition": "WORLD_STATE_ID RECIPE_UNLOCKED_CAMPFIRE_FOOD:{{ModId}}_SausageOnAStick",
  "UnlockHint": "{{i18n:foods.sausageOnAStick.unlockHint}}"
}
```

### Villagers

The pack writes an entry for Abigail and Harvey, specifically to set their liked / disliked campgrounds:

```json
{
  "Changes": [
    {
      "Action": "EditData",
      "Target": "Data/PeacefulEnd.Campgrounds/Villagers",
      "TargetField": [ "Abigail", "LikedCampgrounds" ],
      "Entries": {
        "PeacefulEnd.Campgrounds.ContentPatcher_MinesCampground": "PeacefulEnd.Campgrounds.ContentPatcher_MinesCampground"
      }
    },
    {
      "Action": "EditData",
      "Target": "Data/PeacefulEnd.Campgrounds/Villagers",
      "TargetField": [ "Harvey", "DislikedCampgrounds" ],
      "Entries": {
        "PeacefulEnd.Campgrounds.ContentPatcher_MinesCampground": "PeacefulEnd.Campgrounds.ContentPatcher_MinesCampground"
      }
    }
  ]
}
```

Note: `TargetField` should be used to append the lists (`LikedCampgrounds` and `DislikedCampgrounds`).

### Visitors

`parkVisitors.json` uses both visitor styles side by side. One standard visitor:

```json
{
  "Id": "{{ModId}}_Willy",
  "PreferredDates": [ { "Day": 8 } ],
  "StandardVisitorSettings": { "Visitors": [ "Willy" ] }
}
```
Three advanced visitors, one per park site (`NW`, `SE`, `SW`):

```json
{
  "Id": "{{ModId}}_TravelingMerchant",
  "PreferredDates": [ { "Day": 8 } ],
  "AdvancedVisitorSettings": {
    "RequiredSpot": "NW",
    "MapPatches": [
      {
        "MapPath": "Maps/{{ModId}}_ParkSpotNW_TravelingMerchant",
        "ToArea": { "X": 5, "Y": 5, "Width": 12, "Height": 12 },
        "PatchMode": "Overlay"
      }
    ]
  }
}
```

The advanced visitors show two techniques the reference pages only describe:

- **Token-driven cooldowns.** The `AbiEmiMaru` visitor sets
  `"DaysRequiredBetweenVisits": "{{Random: {{Range: 20, 40}} }}"`, which is a Content Patcher token (evaluated
  per visit), for a visit every 20–40 days.
- **Weather/festival gating with a negated query.** That same visitor uses
  `"RequirementsCondition": "!WEATHER PeacefulEnd.Campgrounds.ContentPatcher_CindersapPark Rain Storm GreenRain Festival Wedding"`. The leading `!` skips the visit on bad-weather or event days. Note the query is checked against the
  **park** location, not the campground.

Each advanced visitor spreads across a *different* [sites](../content-packs/visitors.md#visitor-spots), so all three could appear on the same day.

---

## 5. The shop

`shops.json` edits the framework's built-in camp shop by appending items to it:

```json
{
  "Action": "EditData",
  "Target": "Data/Shops",
  "TargetField": [ "PeacefulEnd.Campgrounds.Shops.CampShop", "Items" ],
  "Entries": {
    "{{ModId}}_TentSchematic_SturdyTent": {
      "Id": "{{ModId}}_TentSchematic_SturdyTent",
      "ItemId": "(O)PeacefulEnd.Campgrounds.Items.TentSchematic {{ModId}}_SturdyTent",
      "Price": 5000,
      "AvailableStock": 1,
      "AvailableStockLimit": "Player",
      "Condition": "!WORLD_STATE_ID SCHEMATIC_UNLOCKED_TENT:{{ModId}}_SturdyTent, PLAYER_STAT Current PeacefulEnd.Campgrounds_TotalNightsGoneCamping 10"
    }
  }
}
```

Some notes:

- The **`ItemId`** is the [tent-schematic item query](../reference/framework.md#item-queries) with the
  tent's Id as its argument. Buying it sets the `SCHEMATIC_UNLOCKED_TENT:` flag the tent's
  `UnlockCondition` checks.
- The **`Condition`** does two things: `!WORLD_STATE_ID ...` hides the item once bought (so it's a
  one-time purchase) and `PLAYER_STAT ... TotalNightsGoneCamping 10` hides it until the player has
  camped ten nights. See the [nights-camped stat](../reference/framework.md#stats).
- **`AvailableStockLimit: "Player"`** with `AvailableStock: 1` makes it once per player.

The pack sells a campsite map, a recipe and two tent schematics this way, then uses `MoveEntries` to
order them after the walkie-talkie in the shop list.

### A second shop for the recipe merchant

The visitor-run recipe shop shows the `UNKNOWN_COOKING_RECIPES` query put to use, with random stock
and a random price multiplier:

```json
{
  "Id": "{{ModId}}_RandomRecipes",
  "ItemId": "PeacefulEnd.Campgrounds_UNKNOWN_COOKING_RECIPES",
  "MaxItems": "{{Random: {{Range: 3, 5}} }}",
  "PriceModifiers": [ { "Modification": "Multiply", "Amount": "{{Random: {{Range: 2, 3}} }}" } ]
}
```

---