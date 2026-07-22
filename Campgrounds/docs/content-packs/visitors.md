# Visitors

`Campgrounds.Framework.Models.Data.Visitors.VisitorData`

A visitor is someone who turns up at **Cindersap Park** (the campground hub the framework adds) on their own schedule. Unlike a [villager](villagers.md), who the player invites along on a trip, a
visitor shows up by themselves.

The data asset is `Data/PeacefulEnd_Campgrounds/ParkVisitors`.

Visitors come in two flavours:

- **[Standard](#standard-visitors)**: name some NPCs and let the framework place them at the park's
  built-in campsite. Can fill **any** of the three sites.
- **[Advanced](#advanced-visitors)**: patch the map yourself and control the scene precisely. Pinned
  to **one specific** site.

```json title="content.json (abridged)"
{
  "Id": "{{ModId}}_WeekendHiker",

  "PreferredDays": [ "Saturday", "Sunday" ],
  "PreferredDates": [
    { "Day": 24, "Season": "Winter" }
  ],

  "StandardVisitorSettings": {
    "Visitors": [ "Linus", "Marnie" ]
  },

  "DaysRequiredBetweenVisits": 7,
  "RequirementsCondition": "PLAYER_FRIENDSHIP_POINTS Current Linus 250"
}
```

## Properties

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Id` <span class="req">required</span> | `string` | — | Unique identifier for this visitor entry. Not checked by validation, but Content Patcher uses it as the entry key. Prefix it with `{{ModId}}`. |
| `PreferredDates` <span class="opt">optional</span> | `List<`[`DateModel`](../reference/shared-models.md#datemodel)`>` | `null` | Specific calendar dates this visitor may appear. |
| `PreferredDays` <span class="opt">optional</span> | `List<DayOfWeek>` | `null` | Days of the week this visitor may appear. |
| `StandardVisitorSettings` <span class="req">one of</span> | [`StandardVisitorSettings`](#standard-visitors) | `null` | Simple NPC placement. Mutually exclusive with `AdvancedVisitorSettings`. |
| `AdvancedVisitorSettings` <span class="req">one of</span> | [`MapVisitorSettings`](#advanced-visitors) | `null` | Map-patch placement. Mutually exclusive with `StandardVisitorSettings`. |
| `DaysRequiredBetweenVisits` <span class="opt">optional</span> | `int?` | `7` | Minimum days before this visitor can return. Must be `>= 0`. |
| `RequirementsCondition` <span class="opt">optional</span> | `string` | — | A [Game State Query](https://stardewvalleywiki.com/Modding:Game_state_queries) gating the visit entirely. |

!!! danger "Exactly one settings block"
    Supplying **neither** `StandardVisitorSettings` nor `AdvancedVisitorSettings` fails validation.
    So does supplying **both**.

---

## Scheduling

### When a visitor can appear

`PreferredDates` and `PreferredDays` has different logic depending how which properties you supply:

| You supply | Behaviour |
| --- | --- |
| **Neither** | Visitor can appear on **any** day (subject to `RequirementsCondition` and `DaysRequiredBetweenVisits`). |
| **`PreferredDays` only** | Today's day of the week must be in the list. |
| **`PreferredDates` only** | Today must match one of the dates. |
| **Both** | **OR**, today matches if it's in *either* list. |

Supplying both does **not** narrow the schedule, it widens it:

```json title="Appears every Saturday AND on Winter 24 whatever day that falls on"
{
  "PreferredDays": [ "Saturday" ],
  "PreferredDates": [ { "Day": 24, "Season": "Winter" } ]
}
```

If you want "only on Saturdays in winter", express it with `PreferredDates` alone, or with a
`RequirementsCondition` of `SEASON winter` plus `PreferredDays: [ "Saturday" ]`.

`RequirementsCondition` is checked **first** and separately. It's an AND against everything else: a
failing condition means no visit, regardless of the date lists.

### Repeat visits

| `DaysRequiredBetweenVisits` | Meaning |
| --- | --- |
| omitted | Defaults to **7** days. |
| `0` | No cooldown, may appear on consecutive days. |
| `n` | At least `n` days between visits. |

Negative values fail validation.

The cooldown starts the day the visitor is *selected* and is stored per `VisitorData.Id` in the
player's save. Selecting a visitor also removes them from the day's pool, so **one visitor can fill
at most one site per day**. Only [repaired sites](#visitor-spots) select, so a visitor never has its
cooldown burned by a site the player can't reach yet.

!!! tip "Cooldown and schedule interact"
    A visitor with `PreferredDays: [ "Saturday" ]` and the default 7-day cooldown appears *every*
    Saturday (the cooldown expires exactly as the next one arrives). Set `DaysRequiredBetweenVisits`
    to `14` for a biweekly visitor.

!!! warning "A matching `PreferredDates` entry ignores the cooldown"
    The cooldown is only applied to visitors who **don't** have a `PreferredDates` match today. If
    today is one of your `PreferredDates`, the visitor is eligible no matter how recently they were
    here.

    This is almost certainly what you want (a birthday or festival visitor should show up on the day regardless), but it means `DaysRequiredBetweenVisits` cannot be used to suppress a `PreferredDates` entry. **`PreferredDays` gets no such exemption**, so it *is* subject to the cooldown.

    If you want a date-specific visitor to be skippable, gate it with `RequirementsCondition` instead.

### How a visitor is chosen

The framework fills the park's **repaired** sites each morning, **in a fixed order: `SW`, then `NW`,
then `SE`**. Each site picks one visitor from the eligible pool and that visitor is removed from the
pool before the next site picks. Sites the player hasn't repaired are skipped and take no part.

For each site, candidates are considered in four tiers and the **first tier with any match wins**:

| Tier | Candidates |
| --- | --- |
| 1 | **Advanced** visitors pinned to *this* site, with a `PreferredDates` match today. |
| 2 | Any visitor eligible for this site, with a `PreferredDates` match today. |
| 3 | Any visitor eligible for this site, with a `PreferredDays` match today. |
| 4 | Any remaining visitor eligible for this site. |

"Eligible for this site" means: a standard visitor (they fit anywhere), or an advanced visitor whose
`RequiredSpot` is this site.

Within a tier the choice is **random** among everyone who passes
[`CanVisitToday`](#when-a-visitor-can-appear), though visitors with `PreferredDates` are still tried
before those with `PreferredDays` and those before everyone else.

Two consequences to design around:

- **`SW` gets first pick** (of the repaired sites, at least). A visitor eligible for several sites
  lands in the earliest repaired one more often, because it chooses first and removes them from the
  pool. If you want something to reliably appear at a particular site, make it an advanced visitor
  with a `RequiredSpot`.
- **Tier 1 is the only guarantee in the system.** An advanced visitor pinned to a site with a
  `PreferredDates` match today beats every other candidate for that site. That's the tool for "this
  character is *definitely* here on Winter 24".

---

## Standard visitors

`Campgrounds.Framework.Models.Data.Visitors.StandardVisitorSettings`

Name the NPCs and let the framework place them at the park's built-in campsite.

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Visitors` <span class="req">required</span> | `List<string>` | empty list | NPC **internal names**. Must contain at least one entry. |

```json
{
  "StandardVisitorSettings": {
    "Visitors": [ "Linus", "Marnie", "Robin" ]
  }
}
```

**Everyone in the list arrives together**. They're placed on the site's `IsVisitorSpawn` tiles, shuffled, one NPC per tile.

They're taken **in the order you list them**, so if there are fewer spawn tiles than visitors, the
ones at the end of the list miss out. Put the NPC who matters most first.

!!! warning "You're limited by the map's spawn tiles"
    If the site has fewer `IsVisitorSpawn` tiles than you list visitors, the extras simply don't
    appear. The park's built-in sites have a fixed number of spawn points (5 by default), so any past that will be omitted. Keep groups small, or use an
    [advanced visitor](#advanced-visitors) and bring a map with as many
    [`SpawnVisitorName`](../reference/framework.md#back-layer) tiles as you need.

!!! tip "A name that doesn't resolve is skipped, not fatal"
    If an entry in `Visitors` doesn't match a real NPC, the framework logs a warning naming both the
    NPC and your `VisitorData.Id`, then moves on to the next name (the spawn tile goes to whoever comes after it rather than being wasted). Search your SMAPI log for `Unable to find the NPC` to
    catch typos.

### Standard visitors are site-agnostic

A standard visitor has no `RequiredSpot` and can fill **any** of the three sites, whichever one gets to them first. You don't choose and you can't rely on it being the same site twice.

When a standard visitor occupies a site, the framework patches the park's **active** campsite map
over that site's area, so the camp itself looks the same wherever they land. Only the NPCs change.

If the site matters to your scene, you want an [advanced visitor](#advanced-visitors).

## Advanced visitors

`Campgrounds.Framework.Models.Data.Visitors.MapVisitorSettings`

The full-control path: claim one of the park's three visitor sites and patch your own content into
it (an NPC's camp, a merchant's stall, a parked cart, etc.)

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `RequiredSpot` <span class="req">required</span> | [`VisitorSpots`](../reference/enums.md#visitorspots) | — | Which of Cindersap Park's three visitor sites this needs, by its cardinal position in the park: `SW`, `NW` or `SE`. |
| `MapPatches` <span class="req">required</span> | `List<`[`MapPatchModel`](../reference/shared-models.md#mappatchmodel)`>` | `null` | Map patches applied when this visitor arrives. Must contain at least one. |

```json
{
  "AdvancedVisitorSettings": {
    "RequiredSpot": "SW",
    "MapPatches": [
      {
        "MapPath": "Mods/{{ModId}}/Maps/HikerCamp",
        "FromArea": { "X": 0, "Y": 0, "Width": 8, "Height": 8 },
        "ToArea":   { "X": 4, "Y": 20, "Width": 8, "Height": 8 },
        "PatchMode": "Overlay"
      }
    ]
  }
}
```

### Visitor spots

Cindersap Park has three visitor sites, named for where they sit in the park:

| `RequiredSpot` | Site | Repaired via |
| --- | --- | --- |
| `SW` | South-west | `PeacefulEnd.Campgrounds_RepairVisitorSite 1` |
| `NW` | North-west | `PeacefulEnd.Campgrounds_RepairVisitorSite 2` |
| `SE` | South-east | `PeacefulEnd.Campgrounds_RepairVisitorSite 3` |

A visitor claims exactly one, so two visitors needing the same site can't appear on the same day.

Each site starts **overgrown** and the park map keeps its overgrown tiles patched over any site the
player hasn't repaired. So on a fresh save no advanced visitor has anywhere to go and the sites
unlock in whatever order the player can afford (the [repair costs](../reference/framework.md#repair-costs) change from site 1 to site 3).

!!! tip "Spread your visitors across sites"
    If every visitor in your pack requires `SW`, only one can ever appear at a time and none can
    until the player repairs that particular site. Distributing them across all three lets the park
    feel busier.

When two visitors are eligible for the same site, the choice is random within the winning
[tier](#how-a-visitor-is-chosen) (there's no "first declared wins"), so you can't out-order another
pack. Both standard and advanced visitors occupy a site. Standard visitors aren't free extras.

**Unrepaired sites are skipped entirely.** They don't select a visitor, don't consume one from the
day's pool and don't burn anyone's [cooldown](#repeat-visits). On a fresh save with nothing repaired,
your visitors wait rather than cycling invisibly.

One consequence remains to design around: a visitor pinned to `SE` needs the park's
[priciest repair](../reference/framework.md#repair-costs) before it can appear at all. If your pack
has one headline visitor, `SW` is the kindest home for it.

### Map patches

Each entry in `MapPatches` copies an area from your map asset onto the park map. See
[`MapPatchModel`](../reference/shared-models.md#mappatchmodel) for the property details.

Patches in the list are applied **in order**, so a later patch can draw over an earlier one, useful for layering a base camp then its details.

Patches are only applied when the site is **repaired** *and* this visitor was selected for it today.
The park map is invalidated each morning, so your patch goes on and comes off cleanly.

### Spawning NPCs

An advanced visitor does **not** get NPCs placed for it. `Visitors` is a standard-only field and
the framework's placement step exits early for advanced visitors.

Instead, put [`SpawnVisitorName`](../reference/framework.md#back-layer) tile properties on the `Back`
layer of the map you patch in. Each one spawns that NPC on that tile, facing
`VisitorFacingDirection` (default `North`):

| Tile property | Value |
| --- | --- |
| `SpawnVisitorName` | `Linus` |
| `VisitorFacingDirection` | `South` |

This is more work than `StandardVisitorSettings`, but it's also strictly more capable. You place each
NPC exactly where you want them, facing where you want, as many as you like.

---

## Validation rules

The framework rejects a `VisitorData` when:

| Condition | Message |
| --- | --- |
| Both settings blocks are `null` | `Must give value for StandardVisitorSettings or AdvancedVisitorSettings` |
| Both settings blocks are supplied | `Values have been given for StandardVisitorSettings AND AdvancedVisitorSettings. You can only specify one!` |
| `StandardVisitorSettings` is invalid | `Error with the given StandardVisitorSettings: {reason}` |
| `AdvancedVisitorSettings` is invalid | `Error with the given AdvancedVisitorSettings: {reason}` |
| `DaysRequiredBetweenVisits` < 0 | `DaysRequiredBetweenVisits must be greater or equal to 0!` |

For `StandardVisitorSettings`:

| Condition | Message |
| --- | --- |
| `Visitors` is `null` or empty | `Missing the "Visitors" property!` |

For `AdvancedVisitorSettings` (`MapVisitorSettings`):

| Condition | Message |
| --- | --- |
| `RequiredSpot` is missing | `Missing the "RequiredSpot" property!` |
| `MapPatches` is `null` or empty | `Missing the "MapPatches" property!` |
| A patch is invalid | `Error with MapPatch #{n}: {reason}` |

Patch numbering in the error is **1-based**, so `MapPatch #1` is the first entry in your list.

---