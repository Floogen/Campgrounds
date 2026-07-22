# Content Packs

Five content types, each with its own JSON model. Every page in this section lists the model's
properties, their types, defaults and the rules the framework checks at load time.

<div class="grid cards" markdown>

-   **[Campgrounds](campgrounds.md)** (`CampgroundData`)

    The destination itself. Spawn tiles, travel time, tent size limits, dialogue overrides, etc.

-   **[Tents](tents.md)** (`CampingTentData`)

    Four directional sprites, resting buffs, meal allowance.

-   **[Campfire Foods](campfire-foods.md)** (`CampfireFoodData`)

    Meals made with Camp Rations, granting custom buffs.

-   **[Villagers](villagers.md)** (`VillagerData`)

    NPC preferences and the dialogue they use when invited.

-   **[Visitors](visitors.md)** (`VisitorData`)

    Who can appear at the Cindersap Forest Park.

</div>

---

## How to read the property tables

| Column | Meaning |
| --- | --- |
| **Property** | The exact JSON key.
| **Type** | The underlying C# type. See [JSON Conventions](../getting-started/json-conventions.md) for how each is written. |
| **Default** | What you get if you omit the property. `—` means the type's default (`null`, `0`, `false`). |

Properties are tagged <span class="req">required</span> when the model's validation rejects the
entry without them and <span class="opt">optional</span> otherwise.

---

## Unlockable content

`CampgroundData`, `CampingTentData` and `CampfireFoodData` share an identical trio of properties.
Rather than repeat it on three pages, here it is once:

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `UnlockCondition` <span class="opt">optional</span> | `string` | — | A [Game State Query](https://stardewvalleywiki.com/Modding:Game_state_queries). Empty or omitted means always unlocked. |
| `UnlockHint` <span class="opt">optional</span> | `string` | — | Text shown to the player while the content is still locked, hinting at how to unlock it (usually when hovered).<br><br>Ignored when `HideUntilUnlocked` is `true`. |
| `HideUntilUnlocked` <span class="opt">optional</span> | `bool` | `false` | If `true`, the entry is hidden from the menu entirely until unlocked and `UnlockHint` is ignored. |

### Choosing between hint and hide

```json title="Visible but locked "
{
  "UnlockCondition": "PLAYER_HAS_MAIL Current beachBridgeFixed",
  "UnlockHint": "Something about the bridge to the east...",
  "HideUntilUnlocked": false
}
```

```json title="Invisible until fixed"
{
  "UnlockCondition": "PLAYER_HAS_MAIL Current beachBridgeFixed",
  "HideUntilUnlocked": true
}
```

### Unlocking with a purchasable item

This is the framework's main progression loop and it isn't visible from the models alone.

Each of the three unlockable types has a matching **item** the framework can create (a campsite map, a campfire recipe, a tent schematic). When the player picks one up (or is gifted via event), it is consumed and sets a world
state flag named after your entry's `Id`.

Check that flag in `UnlockCondition`:

| Content | Item query | Flag to check |
| --- | --- | --- |
| Campground | `(O)PeacefulEnd.Campgrounds.Items.CampsiteMap <ID>` | `MAP_UNLOCKED_CAMPGROUND:<ID>` |
| Tent | `(O)PeacefulEnd.Campgrounds.Items.TentSchematic <ID>` | `SCHEMATIC_UNLOCKED_TENT:<ID>` |
| Campfire food | `(O)PeacefulEnd.Campgrounds.Items.CampfireRecipe <ID>` | `RECIPE_UNLOCKED_CAMPFIRE_FOOD:<ID>` |

```json title="the entry"
{
  "Id": "{{ModId}}_RiverClearing",
  "UnlockCondition": "WORLD_STATE_ID MAP_UNLOCKED_CAMPGROUND:{{ModId}}_RiverClearing",
  "UnlockHint": "Maybe someone in town sells maps."
}
```

```json title="the shop entry that unlocks it"
{
  "Id": "{{ModId}}_RiverClearingMap",
  "ItemId": "(O)PeacefulEnd.Campgrounds.Items.CampsiteMap {{ModId}}_RiverClearing",
  "Price": 5000
}
```

Full details, including the exact query strings, are in the
[Framework Reference](../reference/framework.md#item-queries).

### Notes

- **`UnlockCondition` is evaluated live**, not cached at load. A condition that becomes false again
  (a season check, say) re-locks the content.
- **Validation does not check your query.** A malformed GSQ isn't caught when the pack loads. It
  fails when it's evaluated. Make sure to test it!
- **Unlock flags are global, not per player.** They're set with `addWorldStateIDEverywhere`, so in
  multiplayer one player buying the map unlocks the campground for everyone.
- **`HideUntilUnlocked` works the same in each menu.** Each content type is filtered from *its own* menu:
  campgrounds from the camp list, tents from the tent list, foods from the campfire cooking menu.

### How locked entries appear

When `HideUntilUnlocked` is `false`, a locked entry stays visible but unusable and hovering it shows
its [`UnlockHint`](#unlockable-content):

| Menu | A locked-but-visible entry looks like |
| --- | --- |
| Campsite Menu | Present but not selectable, hover shows the `UnlockHint`. Unlocked sites sort ahead of locked ones. |
| Tent Menu | Present, not selectable, hover shows the hint. The starter tent always sorts first, then unlocked, then locked. |
| Campfire Cooking Menu | Present, not selectable, with its name shown as `???`. The `UnlockHint` appears as hover text. |

When `HideUntilUnlocked` is `true`, the entry is filtered out of its menu entirely until `UnlockCondition` passes.

---

## Cross-references between content types

Content types point at each other by `Id`. The graph is small:

| From | Property | Points at |
| --- | --- | --- |
| `VillagerData` | `TentId` | A `CampingTentData.Id` |
| `VillagerData` | `LikedCampgrounds`, `DislikedCampgrounds` | `CampgroundData.Id` values |
| `CampgroundData` | `DialogueOverrides[].Id` | An NPC's internal name |
| `VisitorData` | `StandardVisitorSettings.Visitors[]` | NPC internal names |
| `VisitorTile` | `SpawnVisitorName` | An NPC's internal name |
