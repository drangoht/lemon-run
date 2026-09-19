# Test report -- Lemon Run

A **cumulative** file. Each session adds a section **at the top** (most recent first), dated, with
the tested version.

> **Never rewrite a past section.** If an old conclusion is refuted, add the refutation and **mark
> the old one as such**: the reasoning that led to the mistake is worth as much as the correction.
> This file is what avoids re-reporting a known bug and redoing a test already settled.

## Session of 2026-09-19 - v1.0-de54e10+ - the pursuer

**Scope**: the chase loop end to end -- lead draining on hits, gauge, catch, restart. **Not**
tested: whether five hits is the right number, whether the pursuer is framed well, whether the
gauge warns in time. None of that is a measurement.

**Method**: `tools/drive_game.py`, no input at all, seeded course. A second pass with
`StartLead 10` / `BlockedPercent 25` through `tuning.json`, to stretch the run and sample the
gauge part-way instead of during the burst.

### What works

- **Five hits end a run**, matching the arithmetic asserted in `LeadTests`.
- **The gauge empties** as the lead falls, and the pursuer visibly closes in -- by the end it
  occludes the runner, which is what being caught should look like.
- **The catch**: runner frozen, dim panel, `CAUGHT / 250 m / Space to run again`.
- **Space starts a new run**: hits back to 0, lead back to 3.00 s, gauge full.
- **The course is deterministic**: two untouched runs on the same seed both ended at **250.1 m**.
  That is the property a balancing pass needs -- two settings compared on the same road.

### Two framing defects, both found by looking and neither reported anywhere

1. With the camera 7 units behind the runner, a pursuer drawn 6 units back was **not visible at
   all**: on the ground, that far back, it falls under the view cone.
2. Widening the band then put it *near the lens*, so at **full lead** -- the safest moment of the
   run -- it filled the screen. The reading was inverted: safe looked like doom.

Camera pulled back to 12 units so the band has room in front of it. Recorded in
`docs/pitfalls/urp-rendering.md`.

### Not verified

The gauge is supposed to redden as it empties. It never came back red on a capture: the colour
only turns in the last quarter, and the five hits arrive in a burst of a second or two. **Coded,
not seen.**

## Session of 2026-09-19 - v1.0-c7595df+

**Scope**: the obstacles -- that they appear, that they are told apart, that a hit registers, and
that a jump clears a low one. **Not** tested: how it feels, the difficulty, the spacing. Nothing
here is a balancing verdict.

**Method**: `tools/drive_game.py`, seed `4242`, `BlockedPercent 100` / `FullPercent 0` (every
blocked lane jumpable), rows packed by `ReactionSeconds 0.35` / `MinimumRowGap 6` so that a 30 s
run crosses dozens of them rather than four. Runs paired on the same seed, therefore on the same
road. Settings passed through `tuning.json`, no rebuild.

### What works

- **Obstacles appear and are told apart**: low ones orange, full ones crimson, readable well
  before arrival.
- **A hit registers and shows**: the counter climbs and the runner flashes -- caught mid-flash on
  `docs/run-jumping.png`.
- **`tuning.json` is applied without recompiling**: `FullPercent 0` emptied the road of every
  crimson obstacle on the next launch.
- **A jump clears a low obstacle.** Same road, same 30 s, ~565 units:

  | Run | Hits |
  |---|---|
  | No input at all | **49** |
  | Jumping, cadence 0.60 s | 28 |
  | Jumping, cadence 0.62 s | **10** |

  The middle row is the interesting one: it is not a weaker effect, it is the *injection* being
  wrong. The arc lasts 0.62 s and a second jump in mid-air is refused, so a 0.60 s cadence has
  every other press rejected and leaves the runner grounded some 48 % of the time. Matching the
  cadence to the arc gives the 80 % drop the model predicts. **A driving scenario whose cadence
  beats against a game duration measures the beat, not the game.**

### Not a measurement

The numbers above say the *rule* works. None of them says the spacing, the speed or the jump are
right: no session has been played by a human, and every value in `RunnerTuning` is still set by
eye.

<!-- Template for a session, to be copied at the TOP of the file:

## Session of YYYY-MM-DD - v<version>-<sha>

**Scope**: what was tested, and what was not.
**Method**: commands used (tools/drive_game.py ...), options, seed.

### What works
- ...

### [BUG-XXX] Short title
Severity: Blocker / Major / Minor / Cosmetic
Context: (screen, version, options)
Reproduction: (precise steps, seed if applicable)
Observed / Expected:
Hypothesis: (probable cause if obvious)
Assigned to: developer | game-designer

### Feel
What measurement cannot say. It is the only source on that point, and it has already been right
against the bench.

-->
