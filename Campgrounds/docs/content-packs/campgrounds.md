# Campgrounds

`Campgrounds.Framework.Models.Data.CampgroundData`

A campground is a destination the player can travel to. It defines where the player and their guest
arrive, how long the trip takes, what size tents fit and how the site is presented in the camp list
menu.

```json title="content.json (abridged)"
{
  "Id": "{{ModId}}_RiverClearing",
  "Description": "A quiet bend in the river, sheltered by old willows.",
  "PreviewTexturePath": "Mods/{{ModId}}/RiverClearingPreview",
  "PreviewTextureScale": 4.0,

  "PlayerSpawnTile": { "X": 12, "Y": 18 },
  "GuestSpawnTile": { "X": 14, "Y": 18 },
  "MaxTentTileSize": { "Width": 4, "Height": 3 },

  "TravelScreenText": "You follow the river north until the road runs out.",
  "RequireVehicle": false,
  "TravelTimeInHours": 1,
  "ForceForageRefreshOnVisit": true,

  "UnlockCondition": "PLAYER_HAS_MAIL Current riverAccess",
  "UnlockHint": "The old logging road might go somewhere.",
  "HideUntilUnlocked": false
}
```

## Properties

### Identity and presentation

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Id` <span class="req">required</span> | `string` | — | Unique identifier. **Must be the name of a game location you register** (see below). Doubles as the Content Patcher entry key and is what `campgrounds_startcamp` and `VillagerData.LikedCampgrounds` take. Matched **case-insensitively**. |
| `Description` <span class="opt">optional</span> | `string` | — | Flavour text describing the site, shown in the camp list menu. |
| `PreviewTexturePath` <span class="opt">optional</span> | `string` | *(default preview)* | **Asset name** of the preview image shown when browsing campgrounds. If omitted, the framework's `Data/PeacefulEnd.Campgrounds/Campgrounds/Textures/Default_Preview` is used. |
| `PreviewTextureScale` <span class="opt">optional</span> | `float` | `4.0` | Scale the preview is drawn at. `4.0` matches the game's usual pixel scale. Use `2.0` for a preview drawn at double resolution. |

!!! danger "A campground's `Id` is a location name: you must add the location yourself"
    Unlike the other content types, a campground isn't self-contained. The framework travels the
    player to the site by calling the game's `getLocationFromName` with your `Id`, so **the `Id` has
    to match a real, loaded `GameLocation`** (one you register through vanilla [`Data/Locations`](https://stardewvalleywiki.com/Modding:Location_data), with its own map).

    A `CampgroundData` whose `Id` doesn't resolve to a location will sit in the menu and then fail to
    travel anywhere.

    The campground's **display name** (shown in menus and travel messages) comes from that
    location's `DisplayName` in `Data/Locations`, not from anything on `CampgroundData`.

### Spawning

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `PlayerSpawnTile` <span class="req">required</span> | `Vector2` | — | Tile the player arrives on. |
| `GuestSpawnTile` <span class="req">required</span> | `Vector2` | — | Tile an invited villager arrives on. |
| `MaxTentTileSize` <span class="opt">optional</span> | [`SizeModel`](../reference/shared-models.md#sizemodel) | — | Largest tent footprint the site accepts, in **tiles**. Omit for no limit. |

Both spawn tiles are <span class="req">required</span> even though they're nullable in the model. Validation rejects the campground if either is missing.

### Travel

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `TravelScreenText` <span class="opt">optional</span> | `string` | — | Text shown on the travel transition screen while the player is on their way. |
| `RequireVehicle` <span class="opt">optional</span> | `bool` | `false` | If `true`, the player can only travel here once the **garage car is repaired**. |
| `TravelTimeInHours` <span class="opt">optional</span> | `int` | `0` | In-game hours the journey consumes. Must be `0`–`15`. |
| `ForceForageRefreshOnVisit` <span class="opt">optional</span> | `bool` | `false` | If `true`, forage on the map is cleared and respawned each morning. |

!!! warning "`TravelTimeInHours` is capped below 16"
    Validation rejects anything greater or equal to `16`. `0` means instant travel.

    The hours are applied by advancing the clock. The camp list menu **won't let the player travel** if arrival would land at or after **midnight**. Plan the number against when you expect players to set out.

!!! note "`RequireVehicle` gates on the garage car"
    When `true`, the camp list menu blocks travel to this site until the player has repaired Clem's garage car. Until then, selecting the site shows a "vehicle
    required" message instead of traveling.

    The menu also shows a "requires vehicle" info line for the site, drawn in **red** while the car
    is still unrepaired, so the player can see the requirement before they try.

!!! note "What `ForceForageRefreshOnVisit` actually does"
    Each morning, the framework removes every forage object on the campground map, resets its spawn counter and runs the game's normal object-spawn pass again. The effect is a site whose forage is
    freshly rolled every day rather than accumulating or persisting.

    Leave it `false` for a site where forage should behave like anywhere else in the world. Set it
    `true` for a "wild" site you want to feel freshly grown on each trip. It only touches objects the
    game considers forage, so items placed items and terrain features are untouched.

### Dialogue overrides

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `DialogueOverrides` <span class="opt">optional</span> | `List<`[`VisitorDialogueOverride`](#visitordialogueoverride)`>` | empty list | Replaces the standard `VillagerData` dialogue for matching NPCs **at this campground specifically**. |

This is how you give a site its own character. If Abigail says something particular whenever she's brought to *specific* campsite, it goes here rather than in her `VillagerData`.

```json
{
  "DialogueOverrides": [
    {
      "Id": "Abigail",
      "DialogueDayOfOverride": [
        "The river's is nice here. I love it.",
        "Something something, dialogue."
      ],
      "DialogueDayAfterOverride": [
        "I slept so peacefully with the sound of the river!"
      ]
    }
  ]
}
```

### Unlocking

`UnlockCondition`, `UnlockHint` and `HideUntilUnlocked` behave identically across all unlockable content (see [Unlockable content](index.md#unlockable-content)).

---

## VisitorDialogueOverride

`Campgrounds.Framework.Models.Data.Visitors.VisitorDialogueOverride`

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Id` <span class="req">required</span> | `string` | — | The NPC's **internal name** (`Abigail`, `Sebastian`), not their display name. Matched case-insensitively. |
| `DialogueDayOfOverride` <span class="opt">optional</span> | `List<string>` | empty list | Replaces the day-of dialogue, liked, neutral **and** disliked alike. |
| `DialogueDayAfterOverride` <span class="opt">optional</span> | `List<string>` | empty list | Replaces the day-after dialogue, liked, neutral and disliked alike. |

### How overriding works

An override only takes effect when its list is **non-empty**. Concretely:

- If `DialogueDayOfOverride` has entries, it replaces `LikedDialogueDayOf`, `NeutralDialogueDayOf` **and** `DislikedDialogueDayOf` from the NPC's `VillagerData`.
- If it's empty or omitted, the `VillagerData` dialogue is used as normal.
- The same rule applies independently to `DialogueDayAfterOverride`.

!!! warning "Overrides are not sentiment-aware"
    You cannot override *only* the disliked day-of line. The override collapses all three sentiments
    into one list, so whatever you write here is said whether the villager loved the trip or hated
    it.

You can override day-of without touching day-after and vice versa. Supplying only
`DialogueDayOfOverride` leaves the day-after dialogue entirely to `VillagerData`.

---

## Validation rules

The framework rejects a `CampgroundData` when:

| Condition | Message |
| --- | --- |
| `Id` is missing or empty | `Missing the "Id" property!` |
| `PlayerSpawnTile` is missing | `Missing the "PlayerSpawnTile" property!` |
| `GuestSpawnTile` is missing | `Missing the "GuestSpawnTile" property!` |
| `TravelTimeInHours` < 0 | `The "TravelTimeInHours" can't be negative!` |
| `TravelTimeInHours` >= 16 | `The "TravelTimeInHours" can't be greater than 16!` |
| `MaxTentTileSize` is invalid | `Invalid MaxTentTileSize given!` |

!!! note
    `Description`, `PreviewTexturePath` and `TravelScreenText` are **not** validated. A campground with no preview image loads fine and falls back to the framework's default preview.

---

## Tent fitting

When the player picks a tent for this site, the framework compares the tent's footprint in the
chosen direction against `MaxTentTileSize`. A tent is rejected if **either** dimension exceeds the
limit:

```
tentSize = tent.GetTileSize(direction)
reject if tentSize.Width > MaxTentTileSize.Width
reject if tentSize.Height > MaxTentTileSize.Height
```

Two consequences to plan around:

- **The check is per direction.** A 4×2 tent facing north may be 2×4 facing east. With a
  `MaxTentTileSize` of `3×3`, that tent fits in *neither* direction. With `4×4` it fits in both. A
  limit of `4×2` would let it face north but not east.
- **Omitting `MaxTentTileSize` means no limit.** Any tent fits, including one large enough to
  overhang your map's clearing. If your site is tight, set the limit.
