# Running and lanes

The first system: the runner goes forward on its own, changes lane, jumps. No obstacle, no fruit
and no pursuer yet -- this file covers only what moves.

> WARNING: **not one number below has been measured.** They are set by eye to give something
> playable to react to. Each is marked with where it came from; a value with no measurement behind
> it must never be quoted later as if it had one.

## What it does

| Gesture | Effect |
|---|---|
| Left / Right arrow | Moves one lane, and one only, whatever the magnitude of the input |
| Space or Up arrow | A fixed-duration jump arc |
| nothing | The runner goes forward regardless, faster and faster up to a ceiling |

Three lanes, numbered 0 to 2 from the left, starting on the middle one so that both sides cost the
same.

## The values, and what they rest on

| Value | Set to | Where it comes from |
|---|---|---|
| Lane width | 2 units | by eye, so that a change reads without the runner crossing the screen |
| Lane change | 0.14 s | by eye -- short enough not to feel like steering, long enough to see the path |
| Starting speed | 12 u/s | by eye |
| Top speed | 26 u/s | by eye |
| Ramp | 600 units | by eye; reached in about 35 s at the average speed |
| Jump peak | 1.6 units | by eye |
| Jump duration | 0.62 s | by eye |

All of them live in `RunnerTuning` and are re-read at every launch from a `tuning.json` written
next to the executable: changing one costs a relaunch, not a rebuild.

WARNING: **the lane width is the exception.** The road geometry bakes it at build time while the
runner reads it from the tuning, so changing it in `tuning.json` slides the runner off the painted
lanes -- with nothing to warn about it. That one needs a rebuild.

## The decisions taken, and why

**A lane change travels, it does not teleport.** At speed, a runner that jumps sideways gives no
reading of *where* it passed, and a collision then looks arbitrary. Smoothstep eased, so it reads
as a move rather than a jerk.

**A step into the edge is refused, never wrapped.** Wrapping would teleport the runner across the
whole road on a mistyped key, and a death would stop being attributable to a readable decision
(GDD section 2).

**The jump is an arc of fixed duration, not a physics simulation.** A jump whose length depends on
gravity, mass and the frame rate cannot be placed against an obstacle with any certainty. Here it
lasts exactly `JumpDuration` whatever happens, so an obstacle fits under it or does not -- a design
decision rather than a physics outcome.

**The speed ramp is flat after 600 units.** An endless ramp ends in a speed no reaction time can
follow, and the run then stops being lost on a decision. Where the ceiling belongs is a
measurement nobody has taken.

**The camera follows a lane change only halfway** (`LateralReach = 0.55`). Following it fully keeps
the runner dead centre and makes the change nearly invisible; not following at all pushes it to the
edge of the frame on the outer lanes.

**The road is dashed.** Not decoration: on a plain uniform strip, forward motion is invisible --
with nothing passing by, a runner at 12 units per second and one standing still look the same.

## Still open -- to be settled with the game in hand

- **A lane change already under way blocks the next one.** Buffering the second input instead would
  let a double change be typed ahead. Which is right is a question of feel, and it is answered by
  playing, not here.
- **An input while airborne is dropped.** Same question, same answer.
- **The refusal at the edge is silent.** A rule that cancels a player input and shows nothing reads
  as a dropped key. Something has to say it on screen -- nothing does yet.
- **Nothing is measured.** Every number in the table above is waiting for a play session.

## Where it lives

| What | File |
|---|---|
| Lane numbering, clamping, position | `Assets/Scripts/Rules/Lanes.cs` |
| Travel between lanes and its easing | `Assets/Scripts/Rules/LaneTravel.cs` |
| The jump arc | `Assets/Scripts/Rules/JumpArc.cs` |
| The speed ramp | `Assets/Scripts/Rules/RunPace.cs` |
| Every tunable value | `Assets/Scripts/Rules/RunnerTuning.cs` |
| Reading `tuning.json` | `Assets/Scripts/Core/TuningLoader.cs` |
| The runner itself | `Assets/Scripts/Gameplay/Runner.cs` |
| The camera | `Assets/Scripts/Gameplay/CameraRig.cs` |
| The endless road | `Assets/Scripts/Gameplay/GroundTreadmill.cs` |
| Debug readout (temporary) | `Assets/Scripts/UI/RunDebugLabel.cs` |

Tests: `tests/LanesTests.cs`, `LaneTravelTests.cs`, `JumpArcTests.cs`, `RunPaceTests.cs` --
27 in all, `dotnet test tests/LemonRun.Tests.csproj`, no engine required.
