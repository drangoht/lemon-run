# The fruit

What buys the lead back. With this, the loop of GDD section 2 closes: collecting is not an
activity beside survival, it **is** how you survive.

## The rule the whole system exists for

**A fruit is never laid on the lane that is already free.**

That is section 5, and it is the one thing `FruitPlacement` enforces. A fruit on the opening hands
the player back the antidote to the very constraint being applied: the run collapses into "stay on
the empty lane and be fed for it", and the pillar of section 2 stops meaning anything.

So a fruit only ever sits on a lane blocked **low**, floating at jump height. Taking it means
leaving the opening, entering a blocked lane, and clearing it with a jump.

Not on a full-height lane either -- that one cannot be entered at all, and a reward nobody can
reach is not a choice, it is decoration. Checked over 300 seeds x 200 rows
(`tests/FruitPlacementTests.cs`).

## What it costs to be greedy

Leaving the opening is not free, and the price is not the jump -- it is **position**. The next
row's opening is drawn within one lane of *this* row's opening. A player who went sideways for a
fruit is no longer on it, so the next opening can be two lanes away and out of reach.

That is deliberate, and it is the risk/reward the loop is built on: **the safe line is always
survivable; deviating for fruit is a gamble.** It stays inside the pillar of section 2 because
choosing to leave the opening is itself a readable decision, taken in full view of the next row.

## The values

| Value | Set to | Where it comes from |
|---|---|---|
| Bought back per fruit | 0.35 s | half a hit: **two fruit undo one mistake** |
| Ceiling it can buy up to | 4.5 s | above the 3.0 s start, or early fruit buys nothing |
| Chance a jumpable lane carries one | 55 % | by eye |
| Height it floats at | 1.05 unit | by eye, inside the jump arc |
| Reach | 0.75 unit | wide on purpose, see below |
| Points per fruit | 10 | by eye |

**Half a hit, not a whole one.** A fruit that paid for an entire mistake would make the greedy
line strictly better than the careful one, and there would be no choice left to make.

**The reach is a band, not a point.** The decision the player makes is the *lane*, taken a second
earlier and in full view. Asking them to also land the apex on a given centimetre would move the
decision into the jump, where nothing is readable. A test asserts that a jump which merely clears
the obstacle underneath also takes the fruit.

**Fruit counts twice**: it buys lead, and it scores points. Saying so out loud is the point -- a
fruit worth no points would still be worth taking, but only defensively, and the game would become
about surviving rather than about the choice.

## Measured

Two paired runs on the same seeded road, every blocked lane low and carrying fruit
(`docs/TEST_REPORT.md`):

| Run | Distance | Fruit | Hits | Score |
|---|---|---|---|---|
| No input | 182 m | 0 | 5 | 182 |
| Jumping throughout | 278 m | 3 | 6 | 308 |

Three fruit bought back 1.05 s on a 3.0 s lead, which is exactly one extra hit survived --
6 instead of 5. The loop does what it says.

WARNING: that is a check of the *rule*, not of the balance. The scenario stayed on one lane and
jumped; it never made the choice the system is built around -- leaving the opening for a fruit.
Nobody has played that.

## Still open

- **Nothing rewards a clean run.** With fruit only on blocked lanes, a player who never deviates
  starves by design. Whether that is too harsh needs playing.
- **The greedy trap is unmeasured**: how often deviating leaves the next opening out of reach is
  a number nobody has looked at, and it decides whether the gamble is fair or a punishment.
- **A taken fruit still says very little.** It vanishes; no sound, no flash on the gauge. One
  fruit moves the drawn pursuer by about 0.4 unit out of a 5.1-unit band -- roughly 8 % -- which
  at that distance is near the edge of what the eye catches. The gauge is the honest channel and
  it is small too. This is the likeliest reason the gain read as "nothing happened" even once
  BUG-001 was fixed, and it wants a real cue, not a bigger number.

## Where it lives

| What | File |
|---|---|
| Which lane may carry a fruit, and the reach | `Assets/Scripts/Rules/FruitPlacement.cs` |
| Distance plus fruit | `Assets/Scripts/Rules/Score.cs` |
| What it buys back | `Assets/Scripts/Rules/Lead.cs` (`AfterGain`) |
| Placing and collecting | `Assets/Scripts/Gameplay/ObstacleField.cs` |

Tests: `tests/FruitPlacementTests.cs`, `tests/ScoreTests.cs`.
