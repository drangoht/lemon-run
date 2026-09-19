# Obstacles

What stands in the road, and what answers it. No fruit and no pursuer yet -- a hit is counted and
shown, but what it *costs* belongs to the lead system (GDD section 2).

> WARNING: none of the numbers below is measured. See `docs/TEST_REPORT.md` for what *has* been
> checked -- the rules work; whether they are well tuned nobody knows yet.

## The two kinds, and why exactly two

| Kind | Looks like | Answers |
|---|---|---|
| **Low** | orange, knee height | jump it **or** go round it |
| **Full** | crimson, taller than the runner | go round it -- nothing else |

A low obstacle is the interesting one: it leaves the player a **choice** rather than a key to
find. That is what section 3 of the GDD bought by keeping the gesture vocabulary to three, and it
is why a slide was turned down -- with one, a high obstacle would have had exactly one right
answer and a death would read as "wrong button" instead of "wrong line".

Full obstacles exist to make the lane itself matter. Without them, every row could be answered by
jumping and the three lanes would be decoration.

## The two guarantees

Both come straight from the pillar in section 2 -- a death must be attributable to one readable
decision -- and both are held by tests rather than by care.

**1. No row is ever impassable.** The opening is placed *first*, and the rest of the row is filled
in around it. Nothing is drawn then rejected: on a hard setting, rejection loops an unbounded
number of times and returns the least varied rows.

**2. The opening is always reachable.** It moves by at most one lane from the previous row's, so a
single lane change always answers it. An opening two lanes away is a death the player could not
avoid, and -- worse -- it looks exactly like one they could have, so they go looking for their own
mistake.

Checked over 200 seeds x 300 rows at the hardest possible setting (`tests/RowDrawTests.cs`).

## Spacing is a duration, not a distance

The gap between rows is `speed x ReactionSeconds`, floored. Fixed in units, the road would get
silently harder as the speed ramps from 12 to 26, and the difficulty would come from a number
nobody chose. Fixed in seconds, the reaction window stays what it was decided to be and any
tightening is a deliberate act (section 5).

The window must also cover the lane change itself, not just the decision: a window shorter than
`LaneChangeDuration` leaves the runner still travelling when it reaches the row, and the hit lands
on a decision that was taken correctly. A test walks the whole speed ramp to check that.

## The values

| Value | Set to | Where it comes from |
|---|---|---|
| Reaction window | 0.9 s | by eye |
| Minimum gap | 12 units | by eye, so the opening rows are not glued together |
| Blocked lanes | 55 % | by eye |
| Full rather than low | 40 % | by eye |
| Low clearance | 0.45 unit | the height of a low obstacle, so the two always agree |
| Course seed | 20260919 | arbitrary; same seed, same road |

The seed is not a detail: a balancing pass can only compare two settings on the **same** obstacle
course, which is what section 6 asks for.

## The decisions taken, and why

**Collision is computed, not physical.** At top speed a frame covers nearly half a unit, and a
thin trigger is exactly what a fast object walks through without touching. The rule is decided in
`ObstacleRules.Hits` and tested with no engine; what is tested is the **crossing** of the row,
never "am I level with it".

**Mid lane change, the runner counts as being on the nearest lane.** Halfway through it is on no
lane at all, and an obstacle met right then still has to resolve one way or the other. Rounding to
the nearest centre matches what the player sees.

**An obstacle does not stop the runner.** It is run through. Stopping would end the run, and the
loop says a mistake is *paid for*, not fatal.

## Still open

- **A hit costs nothing yet** beyond a counter and a flash. The lead it hands back to the pursuer
  is the next system, and it is what turns this into a game.
- **Difficulty is flat**: the blocked and full percentages never move. A difficulty step must add
  a *named rule*, not a multiplier (section 5) -- nothing is designed there yet.
- **Nothing is measured.** No human has played a single run.

## Where it lives

| What | File |
|---|---|
| What a row allows, hit and crossing | `Assets/Scripts/Rules/ObstacleRules.cs` |
| Drawing rows around a guaranteed opening | `Assets/Scripts/Rules/RowDraw.cs` |
| Spacing as a reaction window | `Assets/Scripts/Rules/RowSpacing.cs` |
| Laying, recycling, hit detection | `Assets/Scripts/Gameplay/ObstacleField.cs` |

Tests: `tests/ObstacleRulesTests.cs`, `RowDrawTests.cs`, `RowSpacingTests.cs`.
