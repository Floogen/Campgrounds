# Getting Started

Campgrounds content packs are **[Content Patcher](https://github.com/Pathoschild/StardewMods/tree/develop/ContentPatcher#readme)
packs**. The framework exposes five data assets and you add your content by writing `EditData` patches against them.

If you've ever added an item to `Data/Objects` with Content Patcher, then you already know how this works.

## What you need

- [SMAPI](https://smapi.io)
- [Content Patcher](https://www.nexusmods.com/stardewvalley/mods/1915)
- The Campgrounds framework
- A text editor

## 1. Create the folder

Make a new folder under `Mods`. A `[CP]` prefix is the Content Patcher convention:

```
Stardew Valley/
└── Mods/
    └── [CP] My First Campground/
        ├── manifest.json
        └── content.json
```

## 2. Write the manifest

Your pack is `ContentPackFor` **Content Patcher** and takes Campgrounds as a **dependency**:

```json title="manifest.json"
{
  "Name": "My First Campground",
  "Author": "Your Name",
  "Version": "1.0.0",
  "Description": "Adds a quiet clearing by the river.",
  "UniqueID": "YourName.MyFirstCampground",
  "UpdateKeys": [],
  "ContentPackFor": {
    "UniqueID": "Pathoschild.ContentPatcher",
    "MinimumVersion": "2.9.0"
  },
  "Dependencies": [
    {
      "UniqueID": "PeacefulEnd.Campgrounds.Core",
      "IsRequired": true,
      "MinimumVersion": "1.0.0"
    }
    {
      "UniqueID": "PeacefulEnd.Campgrounds.ContentPatcher",
      "IsRequired": true,
      "MinimumVersion": "1.0.0"
    }
  ]
}
```

!!! warning "`ContentPackFor` is Content Patcher, not Campgrounds"
    Campgrounds never receives your pack directly. Content Patcher does and it edits the assets Campgrounds reads.

## 3. Write the content.json

```json title="content.json"
{
  "Format": "2.9.0",
  "Changes": [
    {
      "Action": "EditData",
      "Target": "Data/PeacefulEnd_Campgrounds/Campgrounds",
      "Entries": {
        "{{ModId}}_RiverClearing": {
          "Id": "{{ModId}}_RiverClearing",
          "Description": "A quiet bend in the river.",
          "PlayerSpawnTile": { "X": 12, "Y": 18 },
          "GuestSpawnTile": { "X": 14, "Y": 18 },
          "TravelTimeInHours": 1,
          "TravelScreenText": "You follow the river north until the road runs out."
        }
      }
    }
  ]
}
```

Three things to notice:

- **`Target`** is the framework's data asset. There are [five of them](data-assets.md) available.
- **The `Entries` key must equal the entry's own `Id`.** These assets are *lists*, not dictionaries,
  and Content Patcher uses each model's `Id` field as its key. See [here for details](data-assets.md#ids-and-keys).
- **`{{ModId}}`** automatically translates to your manifest's `UniqueID`.

## 4. Test it

Launch through SMAPI. Content Patcher reports patch errors in the console and Campgrounds logs its
own validation failures separately.

!!! tip "Reload without restarting"
    Content Patcher's `patch reload YourName.MyFirstCampground` command re-applies your pack in place.
    Campgrounds watches for the resulting asset invalidation and reloads its data automatically, so
    your edit takes effect without a game restart.

    `patch summary` shows which of your patches applied and which were skipped and is usually the
    fastest way to find out why nothing happened.

## 5. Try it immediately

You don't have to play to the campsite to see whether it works. Jump straight there:

```
campgrounds_startcamp {{ModId}}_RiverClearing
```

See [Console Commands](../reference/framework.md#console-commands) for the rest.

## Next steps

- [Data Assets](data-assets.md) (the five targets and how list keys work)
- [JSON Conventions](json-conventions.md) (`Vector2`, `Rectangle`, asset paths and Game State Queries)
- [Campgrounds](../content-packs/campgrounds.md) (the full property list for your campsite)
