# Campfire Foods

`Campgrounds.Framework.Models.Data.CampfireFoodData`

A campfire food is a meal the player cooks at their campsite. Meals cost **Camp Rations**, grant buffs and are limited per trip by the tent's
[`NumberOfAllowedCampfireMeals`](tents.md#gameplay).

```json title="content.json (abridged)"
{
  "Id": "{{ModId}}_ToastedMarshmallow",
  "DisplayName": "Toasted Marshmallow",
  "Description": "Golden on the outside, molten in the middle. Worth the burnt fingers.",
  "RationCost": 1,

  "TexturePath": "Mods/{{ModId}}/Foods",
  "SourceRectangle": { "X": 0, "Y": 0, "Width": 16, "Height": 16 },

  "Buffs": [ { "Id": "food_2" } ],

  "UnlockCondition": "PLAYER_FRIENDSHIP_POINTS Current Vincent 500",
  "UnlockHint": "Vincent has opinions about marshmallows.",
  "HideUntilUnlocked": false
}
```

## Properties

### Identity

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Id` <span class="req">required</span> | `string` | — | Unique identifier for this meal. Used as the Content Patcher entry key and by the campfire-recipe item queries. Prefix it with `{{ModId}}`. |
| `DisplayName` <span class="req">required</span> | `string` | — | Name shown on the campfire menu. |
| `Description` <span class="opt">optional</span> | `string` | — | Flavour text shown when the meal is highlighted. |

### Cost

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `RationCost` <span class="opt">optional</span> | `int` | `0` | Camp Rations the meal costs. Must be `>= 0`. |

Camp Rations are the framework's own currency (see [`Currency`](../reference/enums.md#currency)).
`RationCost: 0` is valid and makes the meal free.

At the campfire, the player picks meals up to the tent's
[`NumberOfAllowedCampfireMeals`](tents.md#gameplay) and the cost of each is deducted as it's
selected. You can't select a meal you can't afford and the combined cost of the session can't
exceed the rations you walked in with. Deselecting a meal, or closing the menu without cooking,
refunds what was spent. So rations are only truly consumed once the player commits to cooking.

### Appearance

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `TexturePath` <span class="req">required</span> | `string` | — | **Asset name** of the meal's icon sheet, e.g. `Mods/{{ModId}}/Foods`. |
| `SourceRectangle` <span class="req">required</span> | `Rectangle?` | — | The icon's area on that sheet, in **pixels**. |

Both are required. `TexturePath` is an **asset name**, not a path into your pack. `Load` your icon
sheet first and reference the name you loaded it as. See
[Textures and maps](../getting-started/data-assets.md#textures-and-maps).

Icons are conventionally 16×16, matching vanilla object sprites. You can pack several meals onto one
sheet and give each a different `SourceRectangle`:

```json
{ "TexturePath": "Mods/{{ModId}}/Foods", "SourceRectangle": { "X": 0,  "Y": 0, "Width": 16, "Height": 16 } }
{ "TexturePath": "Mods/{{ModId}}/Foods", "SourceRectangle": { "X": 16, "Y": 0, "Width": 16, "Height": 16 } }
```

### Buffs

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Buffs` <span class="opt">optional</span> | [`List<BuffData>`](../reference/shared-models.md#buffdata) | empty list | Buffs applied when the meal is eaten. Each [`BuffData`](../reference/shared-models.md#buffdata) names a buff `Id` and an optional `Duration`. |

IDs match the keys in the vanilla `Data/Buffs` asset, or any buff added by another mod. Multiple
buffs stack:

```json
{ "Buffs": [ { "Id": "food_2" }, { "Id": "drink_1" } ] }
```

!!! note "Meal buff duration"
    When a campfire meal's buffs are applied, the duration is the tent's [`FoodBuffDuration`](tents.md#gameplay) by default.

    A `Buff` overrides that default with its own [`Duration`](../reference/shared-models.md#buffdata), where `-2` lasts the whole day.

### Unlocking

`UnlockCondition`, `UnlockHint` and `HideUntilUnlocked` behave identically across all unlockable content (see [Unlockable content](index.md#unlockable-content)).

To sell a meal's recipe in a shop, pair an `UnlockCondition` with the framework's campfire recipe
item:

```json
{ "UnlockCondition": "WORLD_STATE_ID RECIPE_UNLOCKED_CAMPFIRE_FOOD:{{ModId}}_ToastedMarshmallow" }
```

See [the buy-to-unlock pattern](../reference/framework.md#the-buy-to-unlock-pattern).

---

## Validation rules

The framework rejects a `CampfireFoodData` when:

| Condition | Message |
| --- | --- |
| `DisplayName` is missing or empty | `DisplayName needs to be set!` |
| `RationCost` < 0 | `RationCost must be greater or equal to 0!` |
| `SourceRectangle` is missing | `Missing SourceRectangle!` |

---