# The pursuer and the lead

The opposition of the game (GDD section 1). A hit hands lead back; when there is none left the run
is over.

> *Was true while this file stood alone, and no longer is: nothing bought the lead back, so a run
> was a countdown of five mistakes -- the "three hits, no refund" design section 7 turned down by
> name. The fruit now buys it back (`gdd/fruit.md`), and the gauge says something again.*

## The lead is held in seconds

Not in units. Eight units of lead means something very different at twelve units per second and at
twenty-six: in distance, the threat would quietly change meaning all the way up the speed ramp.
"He is two seconds behind" says the same thing at the start of a run and at the end -- the same
reasoning that put the row spacing in seconds.

| Value | Set to | Where it comes from |
|---|---|---|
| Lead at the start, and its cap | 3.0 s | by eye |
| Cost of a hit | 0.7 s | by eye -- five mistakes end a run on no fruit at all |
| Bought back by a fruit | 0.35 s | half a hit; the reasoning is in `gdd/fruit.md` |
| Lockout before the restart key | 0.7 s | Space is also the jump: see below |

The gain is capped at the starting lead. Without a cap, a player ahead on fruit could bank an
untouchable lead and the opposition would simply stop existing for the rest of the session.

## What the player sees is not what the rules say

The pursuer is **drawn** inside a band of world units that keeps it framed, from 6.5 behind the
runner at full lead to 1.4 when caught. That is not the lead converted into distance, and the
decoupling is deliberate: the true gap would put it out of frame, and a threat nobody sees does
not exist whatever the gauge says.

The cost is real and has to be stated: **the distance on screen cannot be used to judge whether a
hit is survivable.** The bar at the top of the screen is the only honest reading, which is why
there is one.

WARNING: the band is bounded by the camera at both ends, and getting it wrong reports nothing.
Beyond the camera's setback the pursuer is not drawn at all. Close to it, the reading inverts: the
camera sits *behind* the runner, so "far behind the runner" is "near the lens", and a full lead --
the safest moment of the run -- fills the screen with the threat. Both were met here, in that
order. The camera was pulled from 7 back to 12 units so the band has room in front of it; move one
of the two and re-read the other.

## The catch

The runner freezes, the panel gives the distance run and names the key to run again. Naming it
matters: a player who does not know how to restart reads a finished game as a frozen one.

The restart reloads the scene rather than resetting each system by hand -- everything is built by
`SceneBuilder` and nothing is worth keeping between runs, so a reload cannot leave a stale value
behind, which hand-resetting would on the first system somebody forgets.

WARNING: the lockout is not politeness. Space is *also* the jump, so a player caught mid-jump is
holding the very key that restarts; with no delay the score flashes past unread and the run looks
as though it restarted on its own.

## Still open

- **The gauge reddens only in its last quarter**, and by then it is also short. Whether that warns
  in time is unknown: it has been coded, not seen -- no capture caught it red.
- **The pursuer is large in frame** even at full lead. That is a framing judgement and it needs
  eyes, not a measurement.
- **Nothing is measured.** Five hits per run is arithmetic, not balance.

## Where it lives

| What | File |
|---|---|
| Lead, hit, gain, gauge fraction, draw gap | `Assets/Scripts/Rules/Lead.cs` |
| The pursuer and its placement | `Assets/Scripts/Gameplay/Pursuer.cs` |
| End of run and restart | `Assets/Scripts/Gameplay/RunSession.cs` |
| The bar | `Assets/Scripts/UI/LeadGauge.cs` |
| The end panel | `Assets/Scripts/UI/RunOverLabel.cs` |

Tests: `tests/LeadTests.cs`.
