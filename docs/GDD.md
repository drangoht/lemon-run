# GDD -- Lemon Run

> **How to fill this document in**: the **`/write-the-gdd`** skill -- it conducts the interview
> section by section, in the order the decisions are taken, and works through the complete example of
> a small game.

**Source of truth for the design.** Every gameplay decision is recorded here *immediately*, with what
justifies it. The code says *how*; this document says **why**.

> When a conclusion is refuted, **keep it and mark it as such** rather than rewriting it: the
> reasoning that led to the mistake is worth as much as the correction, and it is what stops the same
> detour being taken twice.

WARNING: **this file stays a table of contents: ~150 lines, ceiling.** The detail of a system goes
into `docs/gdd/<system>.md` (section 4). It is re-read by every agent before every task: whatever is
added here is paid for on every subsequent task, including those that have nothing to do with it.

## 1. Pitch

**You run toward the horizon across three lanes with a pursuer at your back: every obstacle you hit
costs you the lead that separates you from it.**

Verb: run, dodge, collect. What opposes the player: a **visible** threat that gains ground on every
mistake -- not a timer, not randomness. A mistake is therefore *paid for* rather than instantly
fatal: the player sees what it cost, and the run goes on until the lead is gone.

## 2. The game loop

```
spawn on the middle lane, the pursuer three strides behind, speed creeping up
   ->  read the lane ahead: the obstacles, and the fruit laid out between them
   ->  a hit hands lead back to the pursuer; a fruit swallowed buys some of it back
   ->  speed rises, the reaction window narrows, hits start to chain
   ->  caught: score = distance run + fruit swallowed
   ->  restart, because the line you should have taken is the one you have just seen
```

**Lead is the only resource**, and the pillar of the game: collecting is not an activity bolted on
beside survival, it *is* how you survive. A player who plays safe in the empty lane starves and gets
caught -- which only holds if the fruit is laid where reaching it costs something (section 5).

WARNING: the last arrow is load-bearing. A death must be attributable to **one readable decision**
("I changed lane too late"), never to an obstacle that was impossible to see coming. Any rule that
breaks that attribution is to be discarded (section 7).

## 3. Controls

Three gestures, no more: **left lane, right lane, jump**. Every obstacle therefore has two possible
answers (go round it, or jump it) rather than one correct key -- a death stays "wrong line", never
"wrong button", which is what the pillar of section 2 demands.

| Action | Keyboard | Gamepad | Touch |
|---|---|---|---|
| Lane left | Left arrow | D-pad left / left stick | Swipe left |
| Lane right | Right arrow | D-pad right / left stick | Swipe right |
| Jump | Space **or** Up arrow | South button (A) | Swipe up |
| Pause | Escape | Start | Button, top right of the HUD |
| Restart once caught | Space | South button (A) | Tap anywhere |

**0.1 ships the keyboard column only.** The other two record a decision taken, not code that exists
-- they are written here so that nobody has to wonder later whether they were decided or forgotten.

WARNING: **an ability must announce its key in the game** (HUD, description, acquisition screen).
Invisible reads as non-existent.

WARNING: **AZERTY** keyboard: `Key.A` falls under the key marked Q. Avoid `A`, `Q`, `Z`, `W`, `M` for
global shortcuts. The arrow keys and Space are chosen precisely because they sit at the same physical
place on both layouts -- do not add a WASD alias without re-reading `docs/pitfalls/input.md`.

<!-- Still to be decided, at the moment the movement system is built (section 4), not before:
     - is a lane change instant, or does it travel (and can it be reversed mid-travel)?
     - is an input buffered while airborne, or dropped?
     Both are input-window questions: they are answered with a prototype in hand, not here. -->

## 4. Systems

<!-- WARNING: THIS SECTION IS AN INDEX, NOT CONTENT. One system = one docs/gdd/<system>.md file,
     one line here. It is the section that swells fastest (it reached 21 KB on a Snake), and
     everything written in it is re-read in full by every agent touching ANOTHER system.

     Each file says: what the system does, its values, and the measurement or observation that
     justifies them. The numeric values live in Assets/Scripts/Rules/, not here. -->

| System | File | In one sentence |
|---|---|---|
| <!-- Movement --> | <!-- [gdd/movement.md](gdd/movement.md) --> | <!-- what it does --> |

## 5. Progression and difficulty

<!--
Rules learned the hard way, to be respected unless there is a new reason:
- A difficulty step adds a NAMED RULE, not a multiplier. The player must be able to read it before
  starting and to understand why they lost.
- Before adding a constraint, check what it GIVES the player: a constraint that also hands out its
  own antidote hardens nothing.
- An optional lever is not a rule: a rule applies to every session.
- Never a wall of patience on a key encounter: more dangerous beats longer.
-->

**Already settled by section 2, before any curve**: the fruit is laid where reaching it *costs*
something -- in the lane an obstacle makes awkward, or behind a jump. Fruit on the free lane would
hand the player back the antidote to the very constraint being applied, and the loop would collapse
into "stay on the empty lane".

<!-- The steps themselves are written as they are built, one named rule at a time: it is the
     prototype that says when a run stops being readable, not imagination. -->

## 6. What has been measured

<!--
Point to docs/TEST_REPORT.md, and record the CONCLUSION here, not the raw data.

WARNING: an isolated session settles nothing: the variance between two sessions can reach a factor
of 2.4 before the tested setting even acts. A balancing verdict is taken on a paired bench, on the
sign test.
-->

## 7. What has been discarded, and why

<!-- The most useful list in the document: it avoids reopening the same debate ten times. -->

Decided during the design interview of 2026-09-19, **before any code** -- so these are reasoned
choices, not measurements. Whatever the prototype disproves comes back in here marked as refuted,
rather than being rewritten.

> **Pseudo-3D out of 2D sprites** (URP 2D, OutRun-style scaling). Discarded. It kept the template
> untouched and reused the sprite pipeline, but the entire depth would have been hand-coded (sort
> order, scale, hitboxes drifting away from the visual) and, decisively: a jump and a lane change
> both move the sprite across the screen -- **the two gestures would read the same**. Consequence:
> the project switches to URP 3D.
>
> **3D with modelled, animated assets.** Postponed, not discarded. A run cycle cannot be generated
> by script, and the project's `artist` agent produces sprites through Python generators: the
> project would become an asset project before being a gameplay project. Low-poly geometry built by
> `SceneBuilder` first; reopen once the loop holds up.
>
> **Lead rebuilding on its own while running clean.** Discarded: it splits the game into two loops
> -- survive on one side, score on the other -- and the fruit becomes decorative. The whole of
> section 2 hangs on the fruit being the *only* way back.
>
> **Three hits, no refund.** Discarded: with no way to buy lead back, the pursuer is a disguised
> life counter and the lead gauge stops saying anything while the run is going on.
>
> **Slide/duck as a third gesture.** Postponed. A high obstacle has only one right answer, which
> turns a death into "wrong button" rather than "wrong line" -- exactly what section 2 forbids. To
> be reopened only if the placement runs out of variety (section 5), and this entry re-read first.
>
> **Single rail, jump only** (Chrome Dino). Discarded: with no lanes there is no "safe lane versus
> paying lane" choice, and the fruit loses its reason to exist. Listed so that it is explicitly
> ruled out rather than quietly forgotten.
