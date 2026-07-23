# Campgrounds

Campgrounds is a Stardew Valley framework that lets mod authors add the following:

- **Campgrounds** the player (and invited NPCs) can travel to
- **Tents** that give buffs or allow players to eat additional meals
- **Campfire meals** players can cook for buffs while camping
- **Visitors** who appear at the Cindersap Forest Park on specific or random dates
- **Custom villager responses** when camping, with custom dialogue for specific campsites 

Everything in this documentation describes the *content pack* side of the framework: the JSON
files you write, the properties they accept and the rules the framework enforces.

---

## Start here

<div class="grid cards" markdown>

-   **[Getting Started](getting-started/index.md)**

    Set up a Content Patcher pack and add your first campsite.

-   **[Content Packs](content-packs/index.md)**

    The full property reference for campgrounds, tents, foods, villagers and visitors.

-   **[Reference](reference/index.md)**

    Shared models, enums, validation errors and the framework's commands and tile actions.

</div>

---

## What you can add

| Content type | What it does | Reference |
| --- | --- | --- |
| **Campground** | A destination the player can travel to, with spawn tiles, travel cost and unlock rules. | [Campgrounds](content-packs/campgrounds.md) |
| **Camping Tent** | The tent the player (or villager) uses. Has four directional sprites and can include various buffs. | [Tents](content-packs/tents.md) |
| **Campfire Food** | A unique meal cooked while camping. Made with Camp Rations and grant custom buffs. | [Campfire Foods](content-packs/campfire-foods.md) |
| **Villager** | Campsite specific dialogue, as well as customizable location preferences. | [Villagers](content-packs/villagers.md) |
| **Visitor** | NPCs or custom map patches that apply to the Cindersap Forest Park on specific or random dates. | [Visitors](content-packs/visitors.md) |

---

## Concepts you'll meet everywhere

**Unlock Conditions.** Campgrounds, tents and foods all share the same three properties:

- `UnlockCondition`
- `UnlockHint`
- `HideUntilUnlocked`

See [Unlockable Content](content-packs/index.md#unlockable-content) for more details.

**Validation.** Every model validates itself when it loads. If a required property is missing or
out of range, the framework rejects that entry and logs the reason.


**Tiles, not pixels.** Positions like `PlayerSpawnTile` are given in *tiles* (16 pixels each).
Sprite rectangles like `DisplayRectangle` are given in *pixels* on your texture.

Mixing these up is an easy mistake (see [JSON Conventions](getting-started/json-conventions.md#tiles-versus-pixels)).


**Everything uses Content Patcher.** There is no custom pack format.

You write `EditData` patches
against [five data assets](getting-started/data-assets.md) and Content Patcher tokens, `When`
conditions and `patch reload` all work as they normally do.
