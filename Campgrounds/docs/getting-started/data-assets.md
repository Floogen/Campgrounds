# Data Assets

Campgrounds exposes five data assets. Each one is a **list** of models and each one is the `Target` of
an `EditData` patch in your `content.json`.

| Asset | Contains | Documentation |
| --- | --- | --- |
| `Data/PeacefulEnd_Campgrounds/Campgrounds` | `List<CampgroundData>` | [Campgrounds](../content-packs/campgrounds.md) |
| `Data/PeacefulEnd_Campgrounds/CampingTents` | `List<CampingTentData>` | [Tents](../content-packs/tents.md) |
| `Data/PeacefulEnd_Campgrounds/CampfireFoods` | `List<CampfireFoodData>` | [Campfire Foods](../content-packs/campfire-foods.md) |
| `Data/PeacefulEnd_Campgrounds/Villagers` | `List<VillagerData>` | [Villagers](../content-packs/villagers.md) |
| `Data/PeacefulEnd_Campgrounds/ParkVisitors` | `List<VisitorData>` | [Visitors](../content-packs/visitors.md) |

## Ids and keys

These assets are **lists** (`[ ... ]`), not dictionaries. Lists have no keys of their own, so Content
Patcher assigns one: for a list of models, **the key is the model's `Id` field**.

That means the `Entries` key and the `Id` inside the entry must be the same string:

```json
{
  "Action": "EditData",
  "Target": "Data/PeacefulEnd_Campgrounds/CampingTents",
  "Entries": {
    "{{ModId}}_CanvasTent": {        // ← the key
      "Id": "{{ModId}}_CanvasTent",  // ← must match the key
      "DisplayName": "Canvas Tent"
    }
  }
}
```

!!! danger "Mismatched key and ID"
    If the key and the `Id` disagree, Content Patcher adds the entry under the key you wrote while
    the framework reads the `Id` inside it. Nothing errors (you just get an entry that other content can't reference and that `patch reload` will duplicate rather than replace). Keep them identical.

### Always prefix with `{{ModId}}`

`{{ModId}}` is a Content Patcher token that expands to your manifest's `UniqueID`.

`{{ModId}}_CanvasTent` becomes `YourName.MyPack_CanvasTent`. Use it for every ID **you define**.

When you **reference** an ID (`TentId`, `LikedCampgrounds` and so on), write the full prefixed
form, using `{{ModId}}` for your own content or the literal Id for someone else's:

```json
{
  "Id": "Abigail",
  "TentId": "{{ModId}}_PurpleTent",
  "LikedCampgrounds": [ "{{ModId}}_RiverClearing", "SomeoneElse.TheirPack_OldMine" ]
}
```

!!! note "Some Exceptions"
    Anything that references NPCs (such as `VillagerData.Id`), should use the NPC's internal name (`Abigail`).

    The same goes for `VisitorDialogueOverride.Id`, `StandardVisitorSettings.Visitors` and `VisitorTile.SpawnVisitorName`.

## Splitting across files with `Include`

A pack with several content types quickly outgrows a single `content.json`. Content Patcher's
`Include` action pulls in another JSON file so you can keep each content type in its own file:

```json title="content.json"
{
  "Format": "2.9.0",
  "Changes": [
    { "Action": "Include", "FromFile": "assets/campgrounds/tents/campingTents.json" },
    { "Action": "Include", "FromFile": "assets/campgrounds/foods/campfireFoods.json" },
    { "Action": "Include", "FromFile": "assets/campgrounds/villagers/campingVillagers.json" }
  ]
}
```

Each included file looks like a `content.json` in miniature:

```json title="assets/campgrounds/tents/campingTents.json"
{
  "Changes": [
    { "Action": "EditData", "Target": "Data/PeacefulEnd_Campgrounds/CampingTents", "Entries": { "...": "..." } },
    { "Action": "Load", "Target": "...", "FromFile": "..." }
  ]
}
```

This is exactly how the [example pack](complete-example.md) is organised and it's the
recommended structure for anything beyond a single campground.

## Adding, editing and removing

Standard Content Patcher semantics apply.

**Add**: use a key that doesn't exist yet. New entries go to the bottom of the list.

```json
"Entries": {
  "{{ModId}}_RiverClearing": { "Id": "{{ModId}}_RiverClearing", "..." : "..." }
}
```

**Edit one field of an existing entry**: use `Fields` rather than `Entries`, so you don't stomp on
other packs' edits:

```json
{
  "Action": "EditData",
  "Target": "Data/PeacefulEnd_Campgrounds/Campgrounds",
  "Fields": {
    "PeacefulEnd.Campgrounds_SomeExistingSite": {
      "TravelTimeInHours": 2
    }
  }
}
```

## Conditions

Because these are ordinary Content Patcher patches, `When` works as normal and it composes with
the framework's own [`UnlockCondition`](../content-packs/index.md#unlockable-content):

```json
{
  "Action": "EditData",
  "Target": "Data/PeacefulEnd_Campgrounds/Campgrounds",
  "Entries": { "{{ModId}}_WinterCamp": { "Id": "{{ModId}}_WinterCamp", "...": "..." } },
  "When": { "HasMod": "SomeoneElse.SnowPack" }
}
```

They both do different jobs:

| Use | For |
| --- | --- |
| `When` | Whether the entry **exists at all**. Mod compatibility, config toggles, translations. |
| `UnlockCondition` | Whether an existing entry is **available to the player**. Progression, friendship, mail flags. |

A campground gated with `When` simply isn't there (no hint, no locked entry in the menu and the player has no idea they're missing anything).

A campground gated with `UnlockCondition` can show [`UnlockHint`](../content-packs/index.md#choosing-between-hint-and-hide) to give tips on how to unlock it.
For progression (such as unlocking a campsite after an event or flag), you'll like want to use `UnlockCondition`.

## Textures and maps

Properties like `TexturePath`, `PreviewTexturePath` and `MapPath` are **game asset names**, not
paths into your content pack folder.

`Load` your file into an asset name, then reference that name:

```json title="content.json"
{
  "Format": "2.9.0",
  "Changes": [
    {
      "Action": "Load",
      "Target": "Mods/{{ModId}}/Tents",
      "FromFile": "assets/tents.png"
    },
    {
      "Action": "EditData",
      "Target": "Data/PeacefulEnd_Campgrounds/CampingTents",
      "Entries": {
        "{{ModId}}_CanvasTent": {
          "Id": "{{ModId}}_CanvasTent",
          "DisplayName": "Canvas Tent",
          "TexturePath": "Mods/{{ModId}}/Tents",
          "NorthSprite": { "DisplayRectangle": { "X": 0, "Y": 0, "Width": 48, "Height": 32 } }
        }
      }
    }
  ]
}
```

`Mods/{{ModId}}/...` is the common convention asset name, but any unique named path works.

!!! warning "`"TexturePath": "assets/tents.png"` will not work"
    A bare file path is not an asset name. The framework passes it to the content pipeline, which
    looks for `Content/assets/tents.png` and throws. If your tent is invisible or your log has a
    missing-asset error, this is usually why.

## Default preview texture

`CampgroundData.PreviewTexturePath` is an optional property. The framework registers a
fallback at:

```
Data/PeacefulEnd_Campgrounds/Campgrounds/Textures/Default_Preview
```

You can omit `PreviewTexturePath` and your campground will show the default preview instead. Specifying it will override the default preview.
