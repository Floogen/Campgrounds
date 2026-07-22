# Tents

`Campgrounds.Framework.Models.Data.CampingTentData`

A tent is what the player pitches at a campsite. Every tent needs **four** sprites (one per facing) plus the buffs it grants while resting and how many campfire meals it allows to be cooked per trip.

```json title="content.json (abridged)"
{
  "Id": "{{ModId}}_CanvasTent",
  "DisplayName": "Canvas Tent",
  "Description": "Heavy and waterproof.",

  "TexturePath": "Mods/{{ModId}}/Tents",
  "GrayscaleTexturePath": "Mods/{{ModId}}/Tents_Grayscale",
  "PreviewOffset": { "X": 0, "Y": -8 },

  "NorthSprite": {
    "DisplayRectangle":  { "X": 0, "Y": 0, "Width": 48, "Height": 32 },
    "BoundaryRectangle": { "X": 0, "Y": 16, "Width": 48, "Height": 16 },
    "EntranceTile":      { "X": 16, "Y": 48 },
    "TileOffset":        { "X": 0, "Y": 0 },
    "ShadowRectangle":   { "X": 0, "Y": 128, "Width": 48, "Height": 12 },
    "ShadowOffset":      { "X": 0, "Y": 4 }
  },
  "EastSprite":  { "DisplayRectangle": { "X": 48, "Y": 0, "Width": 32, "Height": 48 }, "BoundaryRectangle": { "X": 48, "Y": 32, "Width": 32, "Height": 16 }, "EntranceTile": { "X": 32, "Y": 32 } },
  "SouthSprite": { "DisplayRectangle": { "X": 0, "Y": 48, "Width": 48, "Height": 32 }, "BoundaryRectangle": { "X": 0, "Y": 64, "Width": 48, "Height": 16 }, "EntranceTile": { "X": 16, "Y": 32 } },
  "WestSprite":  { "DisplayRectangle": { "X": 48, "Y": 48, "Width": 32, "Height": 48 }, "BoundaryRectangle": { "X": 48, "Y": 80, "Width": 32, "Height": 16 }, "EntranceTile": { "X": 0, "Y": 32 }, "FlipHorizontally": true },

  "RestingBuffs": [ { "Id": "food_2" } ],
  "NumberOfAllowedCampfireMeals": 2,

  "HideUntilUnlocked": false
}
```

## Properties

### Identity

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Id` <span class="req">required</span> | `string` | — | Unique identifier. Referenced by `VillagerData.TentId`. Prefix it with `{{ModId}}`. |
| `DisplayName` <span class="req">required</span> | `string` | — | Name shown to the player. |
| `Description` <span class="opt">optional</span> | `string` | — | Flavour text shown alongside the tent in the menu. |

### Textures

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `TexturePath` <span class="req">required</span> | `string` | — | **Asset name** of the tent's spritesheet, e.g. `Mods/{{ModId}}/Tents`. All four directional sprites are cut from this one image. |
| `GrayscaleTexturePath` <span class="opt">optional</span> | `string?` | — | **Asset name** of a desaturated copy of the spritesheet, used for the locked/unavailable state. Omit to let the framework handle it. |
| `PreviewOffset` <span class="opt">optional</span> | `Vector2` | `{ X: 0, Y: 0 }` | Pixel nudge applied when drawing the tent in a preview/menu context. Use it to centre a sprite that sits oddly in its rectangle. |

!!! tip "Grayscale sheets must line up"
    If you supply `GrayscaleTexturePath`, it has to use the **same layout** as `TexturePath`. The directional rectangles are applied to both images unchanged.

### Directional sprites

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `NorthSprite` <span class="req">required</span> | [`DirectionalSpriteModel`](#directionalspritemodel) | — | The tent as seen when facing north. |
| `EastSprite` <span class="req">required</span> | [`DirectionalSpriteModel`](#directionalspritemodel) | — | Facing east. |
| `SouthSprite` <span class="req">required</span> | [`DirectionalSpriteModel`](#directionalspritemodel) | — | Facing south. |
| `WestSprite` <span class="req">required</span> | [`DirectionalSpriteModel`](#directionalspritemodel) | — | Facing west. |

All four are required. There's no fallback. If you only want one look, point all four at the same
rectangle and use `FlipHorizontally` for the mirrored pair.

### Gameplay

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `RestingBuffs` <span class="opt">optional</span> | [`List<BuffData>`](../reference/shared-models.md#buffdata) | empty list | Buffs applied while the player rests in this tent. Each [`BuffData`](../reference/shared-models.md#buffdata) names a buff `Id` and an optional `Duration`. |
| `RestingBuffDuration` <span class="opt">optional</span> | `int` | `420000` | Default duration in milliseconds for this tent's resting buffs, used when a `RestingBuffs` entry gives no `Duration` of its own. `420000` is 7 minutes. |
| `FoodBuffDuration` <span class="opt">optional</span> | `int` | `420000` | Default duration in milliseconds for [campfire meal buffs](campfire-foods.md#buffs) cooked at this tent, used when a meal's [`Buff`](campfire-foods.md#buffs) gives no `Duration` of its own. `420000` is 7 minutes. |
| `NumberOfAllowedCampfireMeals` <span class="opt">optional</span> | `int` | `1` | How many different meals can be selected in one campfire cooking session. **Clamped to 1–5.** |

!!! warning "`NumberOfAllowedCampfireMeals` limits"
    The max value for `NumberOfAllowedCampfireMeals` is `5`, while the lowest is `1`.

    A tent allowing `3` lets the player cook up to three
    different foods at once. Their combined [buffs](campfire-foods.md#buffs) are all applied
    together.

!!! note "Buff durations"
    Both duration properties are the tent's **defaults** in milliseconds (`420000` is 7 minutes). A buff overrides the default for itself with its own [`BuffData.Duration`](../reference/shared-models.md#buffdata), where `-2` lasts the whole day. `RestingBuffDuration` covers the tent's own resting buffs and `FoodBuffDuration` covers the meals cooked at the campsite.

### Unlocking

`UnlockCondition`, `UnlockHint` and `HideUntilUnlocked` behave identically across all unlockable content (see [Unlockable content](index.md#unlockable-content)).

---

## DirectionalSpriteModel

`Campgrounds.Framework.Models.Common.DirectionalSpriteModel`

Describes how the tent looks and behaves in one specific facing.

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `DisplayRectangle` | `Rectangle` | `{0,0,0,0}` | The area on `TexturePath` to draw, in **pixels**. Also determines the tent's tile footprint. |
| `BoundaryRectangle` | `Rectangle` | `{0,0,0,0}` | The area that acts as solid collision, the part of the tent the player can't walk through. |
| `EntranceTile` | `Vector2` | `{0,0}` | The doorway's position, in **pixels** from the sprite's top-left corner. Divided by 16 internally to find the tile the player interacts with. |
| `TileOffset` | `Vector2` | `{0,0}` | Nudges the whole sprite when placed, in **pixels**. Despite the name it is *not* in tiles, `{ "Y": -8 }` shifts the tent up half a tile. |
| `FlipHorizontally` | `bool` | `false` | Mirror the sprite left-to-right when drawing. |
| `FlipVertically` | `bool` | `false` | Mirror the sprite top-to-bottom when drawing. |
| `ShadowRectangle` | `Rectangle?` | `null` | Optional shadow sprite area on the same texture. Omit for no shadow. |
| `ShadowOffset` | `Vector2` | `{0,0}` | Pixel offset applied to the shadow relative to the tent. |

### Display versus boundary

These two rectangles do different jobs and are easy to conflate:

- **`DisplayRectangle`** is everything you drew, including the tall canvas peak the player should
  be able to walk *behind*.
- **`BoundaryRectangle`** is the solid base, usually just the bottom row or two of the sprite.

A typical tent has a `BoundaryRectangle` that shares the display rectangle's `X` and `Width` but
covers only its lower portion:

```json
{
  "DisplayRectangle":  { "X": 0, "Y": 0,  "Width": 48, "Height": 32 },
  "BoundaryRectangle": { "X": 0, "Y": 16, "Width": 48, "Height": 16 }
}
```

Making `BoundaryRectangle` equal to `DisplayRectangle` gives you a tent the player can't stand
behind. It will look like they're blocked by thin air where the peak is.

!!! note "`BoundaryRectangle.X`/`Y` are offsets from the sprite's own origin"
    The framework positions collision by adding `BoundaryRectangle.X` and `.Y` (as pixels, divided by
    16 into tiles) to the **tent's top-left tile** (the same origin the sprite is drawn from). So the
    two rectangles share a coordinate space: a `BoundaryRectangle` of `{ "Y": 16 }` means "16 pixels
    down from the top of where this sprite is drawn", regardless of where `DisplayRectangle` sits on
    the texture.

    In practice this means you can read both rectangles off your spritesheet with the *same* ruler,
    as long as you measure `BoundaryRectangle` relative to the display sprite's top-left corner rather
    than the sheet's `(0, 0)`. The `X`/`Y` you want is the inset from the sprite, not the absolute
    texture position.

### Using sprite flips

East and west are usually mirror images. Draw one, reuse it:

```json
{
  "EastSprite": {
    "DisplayRectangle": { "X": 48, "Y": 0, "Width": 32, "Height": 48 },
    "FlipHorizontally": false
  },
  "WestSprite": {
    "DisplayRectangle": { "X": 48, "Y": 0, "Width": 32, "Height": 48 },
    "FlipHorizontally": true
  }
}
```

Remember to set `EntranceTile` for the flipped side too. A doorway at the sprite's left edge sits at
its right edge once flipped and the flip does **not** move the entrance for you. (In the real
example pack the sturdy tent's east sprite flips its west sprite but keeps its own `EntranceTile`.)

---

## Tile footprint

The framework never asks you for a tent's size in tiles. It derives it by dividing
`DisplayRectangle` by 16:

```
Width  = DisplayRectangle.Width  / 16
Height = DisplayRectangle.Height / 16
```

| `DisplayRectangle` | Footprint |
| --- | --- |
| `48 × 32` px | 3 × 2 tiles |
| `32 × 48` px | 2 × 3 tiles |
| `64 × 64` px | 4 × 4 tiles |
| `40 × 32` px | 2 × 2 tiles |

The footprint is per direction and it's what [`CampgroundData.MaxTentTileSize`](campgrounds.md#tent-fitting)
is compared against.

---

## Validation rules

The framework rejects a `CampingTentData` when:

| Condition | Message |
| --- | --- |
| `DisplayName` is missing or empty | `DisplayName needs to be given!` |
| `TexturePath` is missing or empty | `TexturePath needs to be given!` |
| `NorthSprite` is missing | `Missing NorthSprite!` |
| `EastSprite` is missing | `Missing EastSprite!` |
| `SouthSprite` is missing | `Missing SouthSprite!` |
| `WestSprite` is missing | `Missing WestSprite!` |

Checks run in that order and stop at the first failure. Fix one, re-run and you may find another
waiting behind it.
