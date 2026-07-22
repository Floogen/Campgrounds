# Enums

Every enum-typed property takes the member **name** as a string. Values below are exact. They're taken from the framework's own enum declarations.

```json
{ "Direction": "North", "RequiredSpot": "SW", "PatchMode": "Overlay" }
```

---

## Direction

`Campgrounds.Framework.Models.Enums.Direction`

Which way a tent or visitor faces.

| Value | Notes |
| --- | --- |
| `North` | The default for [`VisitorTile.Direction`](../content-packs/visitors.md#visitortile) and the `VisitorFacingDirection` tile property. |
| `East` | |
| `South` | The default for `CampgroundMapDetails`. |
| `West` | |

**Used by:** [`VisitorTile.Direction`](../content-packs/visitors.md#visitortile),
the `VisitorFacingDirection` [map tile property](framework.md#back-layer),
`CampgroundMapDetails.PlayerTentDirection`, `CampgroundMapDetails.GuestTentDirection` and `CampingTentData.GetTileSize(direction)`.

!!! note "Two different defaults"
    `VisitorFacingDirection` defaults to `North` when absent or unparseable. `CampgroundMapDetails` defaults to `South`.

---

## VisitorSpots

`Campgrounds.Framework.Models.Enums.VisitorSpots`

Which of the three visitor sites in **Cindersap Park** an
[advanced visitor](../content-packs/visitors.md#advanced-visitors) needs. The value is the site's
**cardinal position within the park**.

| Value | Site |
| --- | --- |
| `SW` | The south-west site. |
| `NW` | The north-west site. |
| `SE` | The south-east site. |

**Used by:** `MapVisitorSettings.RequiredSpot` and the `VisitorSpot`
[map tile property](framework.md#back-layer).

!!! warning "There is no `NE`"
    The park has three sites, not four. `NE` is not a valid site.

!!! note "Only advanced visitors name a spot"
    [Standard visitors](../content-packs/visitors.md#standard-visitors) have no `RequiredSpot` and can
    fill any of the three. Sites are filled in the order `SW`, `NW`, `SE`.

!!! note "Not to be confused with `Direction`"
    [`Direction`](#direction) is which way a tent or visitor *faces* and uses full names (`North`).
    `VisitorSpots` is *where in the park* a site sits and uses two-letter abbreviations (`NW`).

!!! note "Sites start overgrown"
    Each site is repaired by the player through the `PeacefulEnd.Campgrounds_RepairVisitorSite` tile
    action, which sets a world-state flag. Until then the park map keeps its overgrown tiles patched
    over that area. A visitor requiring an unrepaired site is simply
    [not selected](../content-packs/visitors.md#visitor-spots) that day (no visitor, no map patch and nothing logged).

---

## Currency

`Campgrounds.Framework.Models.Enums.Currency`

| Value | Notes |
| --- | --- |
| `CampRations` | The only currency. Spent on [campfire foods](../content-packs/campfire-foods.md). |

No content pack property currently takes a `Currency` value. `CampfireFoodData.RationCost` is a plain `int` and Camp Rations are implied. The enum exists for the framework's internal use and likely anticipates additional currencies later.

---

## CampDialogue

`Campgrounds.Framework.Models.Enums.CampDialogue`

Identifies which dialogue slot is being requested. **Not** written in content packs. It's how the framework selects between your [`VillagerData`](../content-packs/villagers.md#trip-dialogue) lists internally. Listed here so the mapping is legible.

| Value | Maps to `VillagerData` property |
| --- | --- |
| `InviteAccepted` | `InviteDialogueAccepted` |
| `InviteRejected` | `InviteDialogueRejected` |
| `InviteCooldown` | `InviteDialogueCooldown` |
| `LikedDayOf` | `LikedDialogueDayOf` |
| `NeutralDayOf` | `NeutralDialogueDayOf` |
| `DislikedDayOf` | `DislikedDialogueDayOf` |
| `LikedDayAfter` | `LikedDialogueDayAfter` |
| `NeutralDayAfter` | `NeutralDialogueDayAfter` |
| `DislikedDayAfter` | `DislikedDialogueDayAfter` |

The three `*DayOf` members are all served by `VisitorDialogueOverride.DialogueDayOfOverride` and
the three `*DayAfter` members by `DialogueDayAfterOverride`, which is why [overrides can't distinguish sentiment](../content-packs/campgrounds.md#how-overriding-works).

Note that `InviteAccepted`, `InviteRejected` and `InviteCooldown` have **no override path**. A campground cannot
change how a villager responds to an invitation.

`InviteCooldown` also differs in its fallback: when a villager's `InviteDialogueCooldown` list is empty, the framework uses the general `dialogues.general.inviteOnCooldown` translation rather than a per NPC line.

---

## PatchMapMode

`StardewModdingAPI.PatchMapMode`

How a [`MapPatchModel`](shared-models.md#mappatchmodel) merges its source map into the target. This
is SMAPI's own enum, the same one Content Patcher exposes as `PatchMode` on `EditMap` patches.

| Value | Effect |
| --- | --- |
| `Overlay` | Only non-empty source tiles are copied. Empty tiles leave the target's tile showing through. **This is the framework's default.** |
| `ReplaceByLayer` | All tiles in the target area are replaced, but only on layers that exist in the source map. Layers absent from the source are untouched. |
| `Replace` | All tiles in the target area are replaced, including with empty ones. Clears everything the source doesn't define. |

**Used by:** `MapPatchModel.PatchMode`.

### Choosing a mode

Think about what should happen to whatever is *already* on the map underneath your patch:

- **`Overlay`**: you're adding a tent and a campfire onto existing grass. The grass stays. This is
  the default because it's what an additive visitor patch almost always wants.
- **`ReplaceByLayer`**: you're redefining the ground and buildings but want to leave the `Paths`
  layer (spawn points, grass placement) alone.
- **`Replace`**: you're clearing an area completely, including anything another mod put there.

!!! note "The framework's default differs from Content Patcher's"
    `MapPatchModel.PatchMode` defaults to `Overlay`. Content Patcher's `EditMap` `PatchMode` defaults
    to `ReplaceByLayer`. If you're used to writing `EditMap` patches, the default here is not the same.

---

## DayOfWeek

Not a framework enum. This is .NET's `System.DayOfWeek`, used by
[`VisitorData.PreferredDays`](../content-packs/visitors.md#scheduling).

| Value |
| --- |
| `Sunday` |
| `Monday` |
| `Tuesday` |
| `Wednesday` |
| `Thursday` |
| `Friday` |
| `Saturday` |

---

## Season

Not a framework enum. This is the vanilla `StardewValley.Season`, used by
[`DateModel.Season`](shared-models.md#datemodel).

| Value |
| --- |
| `Spring` |
| `Summer` |
| `Fall` |
| `Winter` |