# Shared Models

Small models reused across content types. All live in `Campgrounds.Framework.Models.Common` unless
noted.

---

## BuffData

`Campgrounds.Framework.Models.Data.BuffData`

A single buff plus an optional length. Used by [`CampfireFoodData.Buffs`](../content-packs/campfire-foods.md#buffs) and [`CampingTentData.RestingBuffs`](../content-packs/tents.md#gameplay).

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Id` <span class="req">required</span> | `string` | — | The buff Id, matching a key in the vanilla `Data/Buffs` asset or one added by another mod. Passed to the game's `Buff` constructor. |
| `Duration` <span class="opt">optional</span> | `int` | `0` | Buff length in **milliseconds**. `0` inherits the default duration for where the buff is used (the tent's [`FoodBuffDuration`](../content-packs/tents.md#gameplay) for meals, [`RestingBuffDuration`](../content-packs/tents.md#gameplay) for resting buffs).<br><br>Use `-2` for the buff to last the whole day. |

```json
{ "Id": "food_2", "Duration": 300000 }
```

The default duration constant is `420000` (7 minutes). An unrecognised `Id` is not validated, it fails when the buff is applied, so test every meal in-game.

---

## DateModel

`Campgrounds.Framework.Models.Common.DateModel`

A calendar date, with everything except the day optional. Used by
[`VisitorData.PreferredDates`](../content-packs/visitors.md#scheduling).

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Day` <span class="req">required</span> | `int` | `0` | Day of the month. Must be **1–28**. |
| `Season` <span class="opt">optional</span> | `Season?` | `null` | `Spring`, `Summer`, `Fall` or `Winter`. Omit to match the day in **every** season. |
| `SpecificYear` <span class="opt">optional</span> | `int?` | `null` | Match one exact year. Must be `1` or greater. |
| `YearRange` <span class="opt">optional</span> | [`NumberRange`](#numberrange) | `null` | Match a span of years. Omit `Max` for "this year onward". |

### How matching works

A date matches today when **every** supplied property matches. Omitted properties are skipped, so
each one you add narrows the match:

```json title="The 14th of every season, every year"
{ "Day": 14 }
```

```json title="Winter 14, every year"
{ "Day": 14, "Season": "Winter" }
```

```json title="Winter 14 of year 2 only"
{ "Day": 14, "Season": "Winter", "SpecificYear": 2 }
```

```json title="Winter 14, from year 3 onward"
{ "Day": 14, "Season": "Winter", "YearRange": { "Min": 3 } }
```

```json title="Winter 14, years 2 through 5"
{ "Day": 14, "Season": "Winter", "YearRange": { "Min": 2, "Max": 5 } }
```

!!! warning "`SpecificYear` and `YearRange` are ANDed, not ORed"
    Supplying both means the year must satisfy **both**, which is almost always a mistake and can easily be unsatisfiable. `SpecificYear: 2` with `YearRange: { "Min": 5 }` matches no year at all,
    and nothing warns you. Use one or the other.

### The Stardew Valley Calendar

A Stardew Valley season is 28 days.

### Validation rules

| Condition | Message |
| --- | --- |
| `Day` < 1 or > 28 | `The "Day" property must be between 1 and 28 (inclusive).` |
| `SpecificYear` <= 0 | `The "SpecificYear" property must be greater than 0.` |
| `YearRange` is invalid | `Error with the given YearRange: {reason}` |
| `YearRange.Min` < 0 | `The "YearRange.Min" property must be >= 0.` |
| `YearRange.Max` < 0 | `The "YearRange.Max" property must be >= 0.` |


---

## NumberRange

`Campgrounds.Framework.Models.Common.NumberRange`

An inclusive range with an optional upper bound.

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Min` <span class="opt">optional</span> | `int` | `0` | Lower bound, inclusive. |
| `Max` <span class="opt">optional</span> | `int?` | `null` | Upper bound, inclusive. Omit for unbounded. |

```json title="3 and up, no ceiling"
{ "Min": 3 }
```

```json title="2 through 5 inclusive"
{ "Min": 2, "Max": 5 }
```

Both bounds are **inclusive**: `{ "Min": 2, "Max": 5 }` matches 2, 3, 4 and 5.

### Validation rules

| Condition | Message |
| --- | --- |
| `Max` is supplied and `Min` > `Max` | `The "Max" property must be greater than "Min" if given.` |

`Min == Max` is valid and matches exactly that one value.

---

## SizeModel

`Campgrounds.Framework.Models.Common.SizeModel`

A width/height pair in **tiles**. Used by
[`CampgroundData.MaxTentTileSize`](../content-packs/campgrounds.md#spawning) and returned by
[`CampingTentData.GetTileSize`](../content-packs/tents.md#tile-footprint).

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Width` <span class="req">required</span> | `int` | `0` | Width in tiles. Must be greater than 0. |
| `Height` <span class="req">required</span> | `int` | `0` | Height in tiles. Must be greater than 0. |

```json
{ "Width": 4, "Height": 3 }
```

### Validation rules

| Condition | Message |
| --- | --- |
| `Width` <= 0 | `The "Width" property must be greater than 0.` |
| `Height` <= 0 | `The "Height" property must be greater than 0.` |

Both default to `0`, which fails validation, so wherever a `SizeModel` is used, both properties are effectively required.

!!! tip "For 'no limit', omit the whole property"
    `{ "Width": 0, "Height": 0 }` is rejected. If you want `MaxTentTileSize` to mean "any tent fits",
    omit `MaxTentTileSize` entirely rather than zeroing it.

---

## DirectionalSpriteModel

`Campgrounds.Framework.Models.Common.DirectionalSpriteModel`

Documented in full on the [Tents](../content-packs/tents.md#directionalspritemodel) page, since
that's the only model that uses it.

Summary:

| Property | Type | Unit |
| --- | --- | --- |
| `DisplayRectangle` | `Rectangle` | pixels |
| `BoundaryRectangle` | `Rectangle` | pixels |
| `ShadowRectangle` | `Rectangle?` | pixels |
| `EntranceTile` | `Vector2` | pixels |
| `TileOffset` | `Vector2` | pixels |
| `ShadowOffset` | `Vector2` | pixels |
| `FlipHorizontally` | `bool` | — |
| `FlipVertically` | `bool` | — |

---

## MapPatchModel

`Campgrounds.Framework.Models.Common.MapPatchModel`

Copies part of one map onto another. Used by
[`MapVisitorSettings.MapPatches`](../content-packs/visitors.md#advanced-visitors).

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `MapPath` <span class="req">required</span> | `string` | — | **Asset name** of the source map, e.g. `Mods/{{ModId}}/Maps/HikerCamp`. Loaded through the content pipeline, so it is *not* a path into your pack folder. |
| `FromArea` <span class="opt">optional</span> | `Rectangle?` | `null` | Area of the **source** map to copy, in tiles. Omit to copy the whole map. |
| `ToArea` <span class="opt">optional</span> | `Rectangle?` | `null` | Area of the **target** map to paste into, in tiles. Omit to paste at the origin. |
| `PatchMode` <span class="opt">optional</span> | [`PatchMapMode`](enums.md#patchmapmode) | `Overlay` | How the source is combined with the target. |

```json
{
  "MapPath": "Mods/{{ModId}}/Maps/HikerCamp",
  "FromArea": { "X": 0, "Y": 0, "Width": 8, "Height": 8 },
  "ToArea":   { "X": 4, "Y": 20, "Width": 8, "Height": 8 },
  "PatchMode": "Overlay"
}
```

### Notes

- `FromArea` and `ToArea` are in **tiles**, not pixels, unlike the sprite rectangles elsewhere in the framework. This is the one place `Rectangle` means tiles.
- The two areas should be the **same size**. Both are passed straight to SMAPI's `PatchMap`, so
  mismatched sizes behave however SMAPI handles them rather than being caught here.
- `Overlay` is the default, so tiles that are empty in your source leave the target's tiles intact.
  That's usually what an additive visitor patch wants. Use `Replace` when you need to clear an area
  out entirely. Note this default differs from Content Patcher's `EditMap`, which defaults to
  `ReplaceByLayer`.

### Validation rules

| Condition | Message |
| --- | --- |
| `MapPath` is missing or empty | `The "MapPath" property must be given.` |

`PatchMode` is SMAPI's [`PatchMapMode`](enums.md#patchmapmode): `Overlay` (the default here), `ReplaceByLayer` or `Replace`.
