# Reference

The building blocks used across multiple content types.

<div class="grid cards" markdown>

-   **[Shared Models](shared-models.md)**

    `DateModel`, `NumberRange`, `SizeModel`, `DirectionalSpriteModel`, `MapPatchModel`.

-   **[Enums](enums.md)**

    Every valid string for every enum-typed property.

-   **[Framework Reference](framework.md)**

    Console commands, tile actions, item queries and the park's maps.

</div>

## Model index

| Model | Namespace | Used by |
| --- | --- | --- |
| `CampgroundData` | `Models.Data` | [Campgrounds](../content-packs/campgrounds.md) |
| `CampingTentData` | `Models.Data` | [Tents](../content-packs/tents.md) |
| `CampfireFoodData` | `Models.Data` | [Campfire Foods](../content-packs/campfire-foods.md) |
| `VillagerData` | `Models.Data` | [Villagers](../content-packs/villagers.md) |
| `VisitorData` | `Models.Data.Visitors` | [Visitors](../content-packs/visitors.md) |
| `StandardVisitorSettings` | `Models.Data.Visitors` | `VisitorData` |
| `MapVisitorSettings` | `Models.Data.Visitors` | `VisitorData` |
| `VisitorDialogueOverride` | `Models.Data.Visitors` | `CampgroundData` |
| `VisitorTile` | `Models.Data.Visitors` | *(see [note](../content-packs/visitors.md#visitortile))* |
| `DateModel` | `Models.Common` | `VisitorData` |
| `NumberRange` | `Models.Common` | `DateModel` |
| `SizeModel` | `Models.Common` | `CampgroundData`, `CampingTentData` |
| `DirectionalSpriteModel` | `Models.Common` | `CampingTentData` |
| `MapPatchModel` | `Models.Common` | `MapVisitorSettings` |
| `CampgroundMapDetails` | `Models.Game` | *(runtime, see [note](#campgroundmapdetails))* |

All models live under `Campgrounds.Framework.Models`.

---