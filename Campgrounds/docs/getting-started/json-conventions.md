# JSON Conventions
## Tokens work everywhere

Because these are ordinary Content Patcher patches, [tokens](https://github.com/Pathoschild/StardewMods/tree/develop/ContentPatcher#readme)
work in every entry key and value:

```json
{
  "Id": "{{ModId}}_RiverClearing",
  "Description": "{{i18n: riverClearing.description}}",
  "TravelTimeInHours": "{{TravelTime}}"
}
```

`{{i18n:}}` lets you translate `DisplayName` and `Description` from your pack's `i18n` folder. Setting this up early is recommended.

`{{Random: {{Range: 20, 40}} }}` produces a random value at load (the example pack uses it for a visitor's `DaysRequiredBetweenVisits` and for shop stock and prices).

## Positions: `Vector2`

Properties typed `Vector2` (`PlayerSpawnTile`, `TileOffset`, `PreviewOffset`, `ShadowOffset`,
`EntranceTile`, `Tile`) accept an object (see [tiles vs pixels](#tiles-versus-pixels) for which unit
each uses):

```json
{ "PlayerSpawnTile": { "X": 12, "Y": 18 } }
```

A string shorthand also parses:

```json
{ "PlayerSpawnTile": "12, 18" }
```

Both work. The object form is more verbose but harder to get wrong and it's what the examples on
this site use throughout.

## Areas: `Rectangle`

Properties typed `Rectangle` (`DisplayRectangle`, `BoundaryRectangle`, `ShadowRectangle`,
`SourceRectangle`, `FromArea`, `ToArea`) take four values: the top-left corner, then the size.

```json
{ "DisplayRectangle": { "X": 0, "Y": 0, "Width": 32, "Height": 48 } }
```

!!! warning "`Width`/`Height` are a size, not a second corner"
    A `32×48` sprite starting at `(0, 0)` is `Width: 32, Height: 48` and **not** `X: 32, Y: 48`.

## Tiles versus pixels

This is the distinction that causes the most confusion, so here it is explicitly:

| Kind of property | Unit | Examples |
| --- | --- | --- |
| Tile positions | **Tiles** (1 tile = 16 px) | `PlayerSpawnTile`, `GuestSpawnTile`, `Tile` |
| Sprite areas | **Pixels** on your texture | `DisplayRectangle`, `BoundaryRectangle`, `ShadowRectangle`, `SourceRectangle` |
| Pixel offsets | **Pixels** | `EntranceTile`, `TileOffset`, `ShadowOffset`, `PreviewOffset` |
| Sizes | **Tiles** | `MaxTentTileSize` |

A tent whose `DisplayRectangle` is `48×32` pixels occupies **3×2 tiles**. The framework derives
tile footprint by dividing the display rectangle by 16. See
[`GetTileSize`](../content-packs/tents.md#tile-footprint).

## Optional properties

Any property marked <span class="opt">optional</span> in these docs can simply be left out of your
JSON.

Please note:

- **Omitted is not the same as zero.** `SpecificYear` omitted means "any year". `SpecificYear: 0`
  is an invalid year and will fail validation.
- **Omitted is not the same as empty.** For `VisitorData`, omitting `PreferredDays` means "any day".
  Providing `[]` means the list exists but matches nothing.
- **Defaults are applied by the model, if not by you.** `PreviewTextureScale` is `4` if you don't set
  it.

## Enums

Enum-typed properties take the member name as a string and are case-sensitive unless the framework
says otherwise:

```json
{
  "Direction": "North",
  "RequiredSpot": "SW",
  "PatchMode": "Overlay"
}
```

See the [Enums reference](../reference/enums.md) for every valid value.

## Days of the week

`VisitorData.PreferredDays` is a list of `DayOfWeek` values, so the week starts on **Sunday**:

```json
{ "PreferredDays": [ "Saturday", "Sunday" ] }
```

Valid names: `Sunday`, `Monday`, `Tuesday`, `Wednesday`, `Thursday`, `Friday`, `Saturday`.

## Seasons

`DateModel.Season` takes a vanilla `Season` value: `Spring`, `Summer`, `Fall`, `Winter`.

## Asset paths are asset names

`TexturePath`, `PreviewTexturePath`, `GrayscaleTexturePath` and `MapPath` take **game asset names**,
not file paths into your pack:

```json
{ "TexturePath": "Mods/{{ModId}}/Tents" }     // correct
{ "TexturePath": "assets/tents.png" }         // wrong
```

`Load` your file into an asset name first. See
[Textures and maps](data-assets.md#textures-and-maps).

## Game State Queries

`UnlockCondition` and `RequirementsCondition` are vanilla
[Game State Queries](https://stardewvalleywiki.com/Modding:Game_state_queries) (the same syntax Content Patcher and the game's own data files use).

```json
{ "UnlockCondition": "PLAYER_HAS_MAIL Current guildMember" }
{ "UnlockCondition": "SEASON summer, PLAYER_FRIENDSHIP_POINTS Current Abigail 1250" }
```

A comma-separated list is an **AND** statement (every condition must pass). Leaving the property empty or
omitting it entirely means "always true".

## Buffs

[`CampfireFoodData.Buffs`](../content-packs/campfire-foods.md#buffs) and [`CampingTentData.RestingBuffs`](../content-packs/tents.md#gameplay) are lists of [`BuffData`](../reference/shared-models.md#buffdata), each naming a vanilla or custom buff `Id` (matching a key in the `Data/Buffs` asset) with an optional `Duration`:

```json
{ "Buffs": [ { "Id": "food_2" }, { "Id": "drink_1", "Duration": 300000 } ] }
```

`Duration` is in milliseconds. Omit it (or use `0`) to inherit the tent's default duration, or use `-2` for a buff that lasts the whole day. An Id the game doesn't recognise won't be caught by validation. It fails the moment the buff is applied, so test your meals in-game.
