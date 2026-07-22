# Villagers

`Campgrounds.Framework.Models.Data.VillagerData`

A villager entry gives one NPC opinions about camping: which tent they bring, which campgrounds they like and what they say about the trip. This is the model that makes inviting Abigail feel different
from inviting Lewis.

```json title="content.json (abridged)"
{
  "Id": "Abigail",
  "TentId": "{{ModId}}_PurpleTent",

  "LikedCampgrounds":    [ "{{ModId}}_RiverClearing", "{{ModId}}_OldMine" ],
  "DislikedCampgrounds": [ "{{ModId}}_SunnyMeadow" ],

  "InviteDialogueAccepted": [ "Camping? Yes. Absolutely yes." ],
  "InviteDialogueRejected": [ "I can't tonight — Dad needs me at the shop." ],

  "LikedDialogueDayOf":    [ "This place is amazing. Look at those rocks!" ],
  "NeutralDialogueDayOf":  [ "It's nice out here. Quiet." ],
  "DislikedDialogueDayOf": [ "It's... very sunny. Great." ],

  "LikedDialogueDayAfter":    [ "I'm still thinking about that clearing." ],
  "NeutralDialogueDayAfter":  [ "Thanks for taking me camping." ],
  "DislikedDialogueDayAfter": [ "I got a sunburn. Worth it, I guess." ],

  "RequirementsCondition": "PLAYER_FRIENDSHIP_POINTS Current Abigail 1000"
}
```

## Properties

### Identity

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Id` <span class="req">required</span> | `string` | — | The NPC's **internal name** — `Abigail`, `Sebastian`, `Leo`. Not their display name, not a name you invent and **not** `{{ModId}}`-prefixed. Doubles as the Content Patcher entry key. |
| `TentId` <span class="opt">optional</span> | `string` | — | The [tent](tents.md) this villager brings and pitches for themselves. Points at a `CampingTentData.Id`, so it *is* prefixed. Falls back to the starter tent if unset or unresolved. |

!!! note "Internal names, not display names"
    Internal names are what the game uses in `Data/Characters` and in dialogue files. They differ
    from display names in translated games and for a handful of NPCs in English too. When in doubt,
    check the [wiki's NPC list](https://stardewvalleywiki.com/Modding:NPC_data).

### Campground preferences

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `LikedCampgrounds` <span class="opt">optional</span> | `List<string>` | empty list | `CampgroundData.Id` values this villager enjoys. Selects the **liked** dialogue lines. |
| `DislikedCampgrounds` <span class="opt">optional</span> | `List<string>` | empty list | `CampgroundData.Id` values this villager dislikes. Selects the **disliked** dialogue lines. |

A campground in neither list is **neutral**. That's the default. You only need to list the sites
this villager has a real opinion about.

!!! note "If an Id is in both lists, liked wins"
    `LikedCampgrounds` is checked before `DislikedCampgrounds`, so a campground in both is treated as
    liked. Nothing validates against the overlap (it just resolves in liked's favour). Best to keep
    the lists disjoint anyway, for your own clarity.

### Invitation dialogue

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `InviteDialogueAccepted` <span class="opt">optional</span> | `List<string>` | empty list | Lines used when the villager accepts an invitation. |
| `InviteDialogueRejected` <span class="opt">optional</span> | `List<string>` | empty list | Lines used when they turn one down. |
| `InviteDialogueCooldown` <span class="opt">optional</span> | `List<string>` | empty list | Lines used when the villager is asked while still on their invite [cooldown](#invite-cooldown). When empty, the framework falls back to the general `dialogues.general.inviteOnCooldown` translation. |

### Trip dialogue

Six lists, forming a 3×2 matrix of **sentiment** × **when**:

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `LikedDialogueDayOf` <span class="opt">optional</span> | `List<string>` | empty list | Said at a liked campground, during the trip. |
| `NeutralDialogueDayOf` <span class="opt">optional</span> | `List<string>` | empty list | Said at a campground in neither list, during the trip. |
| `DislikedDialogueDayOf` <span class="opt">optional</span> | `List<string>` | empty list | Said at a disliked campground, during the trip. |
| `LikedDialogueDayAfter` <span class="opt">optional</span> | `List<string>` | empty list | Said the following day, after a liked campground. |
| `NeutralDialogueDayAfter` <span class="opt">optional</span> | `List<string>` | empty list | Said the following day, after a neutral campground. |
| `DislikedDialogueDayAfter` <span class="opt">optional</span> | `List<string>` | empty list | Said the following day, after a disliked campground. |

### Which line gets used

```
                    ┌─────────────┬───────────────┬────────────────┐
                    │   Liked     │    Neutral    │   Disliked     │
                    │ campground  │  campground   │  campground    │
┌───────────────────┼─────────────┼───────────────┼────────────────┤
│ During the trip   │ LikedDay    │ NeutralDay    │ DislikedDay    │
│  (day of)         │   Of        │   Of          │   Of           │
├───────────────────┼─────────────┼───────────────┼────────────────┤
│ The next day      │ LikedDay    │ NeutralDay    │ DislikedDay    │
│  (day after)      │   After     │   After       │   After        │
└───────────────────┴─────────────┴───────────────┴────────────────┘
```

The campground's [`DialogueOverrides`](campgrounds.md#visitordialogueoverride) sit **on top** of this
matrix. If the campground overrides day-of dialogue for this NPC, the entire left-hand column of the
table above is bypassed and the override's lines are used instead, regardless of sentiment.

!!! tip "Every line in a list is shown, in order"
    A dialogue list isn't a pool to pick one from. The framework joins the whole list with the game's
    page-break token and shows the lines **in sequence**, one dialogue box after another. Two entries
    means a two-page conversation, not a random choice between them.

    ```json
    { "LikedDialogueDayOf": [ "This place is amazing.", "Look at those rocks!" ] }
    ```

    That's two boxes, always in that order. Write each list as the script you want spoken, not as
    interchangeable variants.

### Requirements

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `RequirementsCondition` <span class="opt">optional</span> | `string` | — | A [Game State Query](https://stardewvalleywiki.com/Modding:Game_state_queries) gating this entry. Empty or omitted means always available. |

Use it to require a friendship level before an NPC can be invited camping at all:

```json
{ "RequirementsCondition": "PLAYER_FRIENDSHIP_POINTS Current Abigail 1000" }
```

!!! bug "`HasRequirements()` returns `true` when the condition is empty"
    Reading the source: an empty `RequirementsCondition` returns `true`, meaning "no requirements, so
    this passes". The method name reads like it should answer *"does this have requirements?"*, but it
    actually answers *"are this entry's requirements met?"*. Mentioned only because it's confusing if
    you go source-diving. The behaviour is the sensible one.

### Invite cooldown

After a villager comes camping, they can't be invited again for **7 days**. The cooldown starts the day the trip is actually taken, not when the invitation is offered, so cancelling or overriding an invitation before setting off costs nothing.

Ask a villager while they're still on cooldown and they decline with their [`InviteDialogueCooldown`](#invitation-dialogue) lines, or the general `dialogues.general.inviteOnCooldown` translation when that list is empty.

### Camping friendship

Bringing a villager camping raises their friendship the morning after a night spent at the campsite, scaled by how they feel about the campground:

| Campground sentiment | Friendship |
| --- | --- |
| Liked | +80 |
| Neutral | +45 |
| Disliked | none |

Sentiment comes from the same [`LikedCampgrounds` and `DislikedCampgrounds`](#campground-preferences) lists that pick the trip dialogue, so a liked site both reads the liked lines and grants the larger gain. A villager with no entry of their own counts as neutral, so they earn the neutral amount.

---

## The generic villager

There's one reserved `Id` the framework treats specially:

```
PeacefulEnd.Campgrounds.Villagers.Generic
```

A `VillagerData` entry with this `Id` is the **fallback** used to fill in dialogue for any NPC whose
own entry leaves a list empty. It is the default dialogue script every villager falls back on.

The generic entry fills two gaps:

- **An NPC with no entry of their own** uses the generic dialogue throughout and is treated as
  neutral toward every campground.
- **An NPC whose entry leaves one list empty** falls back to the generic entry's line for *that*
  list, keeping the rest of their own dialogue. So you can write Abigail a custom liked-day-of line
  and let everything else come from the generic entry.

It has no `TentId`, `LikedCampgrounds` or `DislikedCampgrounds`, since those are per NPC. A fallback villager is always treated as neutral toward every campground.

---

## Validation rules

The framework rejects a `VillagerData` when:

| Condition | Message |
| --- | --- |
| `Id` is missing or empty | `Id needs to be set!` |

That's the only check. Every other property is optional, which means a villager entry consisting of
nothing but `{ "Id": "Abigail" }` is valid and does nothing.

!!! note "`TentId` is not validated, but it does fall back gracefully"
    A `TentId` pointing at a tent that doesn't exist won't fail at load. When the villager is brought
    camping, an unset or unresolved `TentId` falls back to the framework's **starter tent** rather
    than leaving them tentless, so a typo produces the default tent, not a crash or an empty spot.
    Check the spelling against the tent's `Id` (pack prefix included) if a villager shows up in the
    wrong tent.

---

## Patterns

### Minimal entry: a villager with no strong feelings

```json
{
  "Id": "Lewis",
  "NeutralDialogueDayOf":   [ "A fine spot. Very, uh... rustic." ],
  "NeutralDialogueDayAfter":[ "I slept surprisingly well, all things considered." ]
}
```

### Editing another pack's villager

Because `Id` is the NPC's name and Content Patcher keys list entries by `Id`, there can only ever be
**one entry per NPC** across every installed pack. Two packs both writing an `Entries` patch for
`Abigail` don't merge. The one applied later silently replaces the other, ordered by mod load order
and then patch order.

That makes vanilla NPCs shared territory. If you want to add to an existing Abigail entry rather than
clobber it, use `Fields` and gate it on the other pack being present:

```json
{
  "Action": "EditData",
  "Target": "Data/PeacefulEnd.Campgrounds/Villagers",
  "Fields": {
    "Abigail": {
      "TentId": "{{ModId}}_PurpleTent"
    }
  },
  "When": { "HasMod": "SomeoneElse.TheirPack" }
}
```

!!! warning "Use `TargetField` to append a list"
    Setting `LikedCampgrounds` through `Fields` overwrites the whole list, dropping whatever the
    other pack put there. If you need to add one campground to an existing list, use
    [`TargetField`](https://github.com/Pathoschild/StardewMods/tree/develop/ContentPatcher#readme) to drill into it and add
    your value as an entry:

    ```json
    {
      "Action": "EditData",
      "Target": "Data/PeacefulEnd.Campgrounds/Villagers",
      "TargetField": [ "Abigail", "LikedCampgrounds" ],
      "Entries": {
        "{{ModId}}_RiverClearing": "{{ModId}}_RiverClearing"
      }
    }
    ```
