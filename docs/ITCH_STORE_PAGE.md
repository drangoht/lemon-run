# itch.io page -- Lemon Run

**The page text lives here**, and it is from here that it gets pasted onto itch. If the published
page is in another language, keep both files (`ITCH_STORE_PAGE_FR.md`) and **fix them together**:
otherwise one of the two lies, and nobody knows which.

WARNING: **this text must describe the game AS IT IS.** A page describing a feature removed two
versions earlier is the most common and the most expensive defect: the visitor notices the gap and
closes the tab. Re-read it at every release that changes something visible.

---

## Title

Lemon Run

## Tagline (one line, under the title)

Something is chasing you. Every fruit you swallow buys back the ground you lost.

## Description

You run down three lanes with a pursuer at your back. Hit something and it closes in; swallow a
fruit and it falls back.

The catch is where the fruit grows: **never on the lane that is clear.** Staying safe means going
hungry, and going hungry means being caught. Every row asks the same question, and you answer it
about once a second.

<!-- SCREENSHOT to insert here: docs/run-fruit.png shows it in one image -- the runner on the free
     middle lane, and the fruit floating above an obstacle in the lane beside it. -->

### Controls

| Action | Keyboard | Touch |
|---|---|---|
| Change lane | Left / Right arrow | -- not in 0.1 |
| Jump | Space or Up arrow | -- not in 0.1 |
| Run again once caught | Space | -- not in 0.1 |

WARNING: 0.1.0 is keyboard only. Say so on the page rather than letting a phone visitor find out:
the **Mobile friendly** box must stay unticked until touch controls exist.

### What is in 0.1.0

- Three lanes, a jump, and a road that never ends
- Obstacles of two kinds: the low ones you may jump *or* go round, the tall ones you must go round
- A pursuer, and a lead measured in seconds rather than lives
- A score: metres run plus fruit swallowed

It is a first playable version. There is no sound, the shapes are placeholder blocks, and none of
the tuning has been played enough to be called balanced.

### Credits

Font: Unity's built-in LegacyRuntime.
<!-- Nothing else yet: no sound, no music, no third-party asset. Fill this in as soon as there is
     one, with its licence, and check COMMERCIAL usage -- some free generation plans forbid it. -->

---

## Dashboard settings -- WARNING: they are in NO file of the repo

To be checked by hand after every publication; they were wrong for several versions on a previous
project.

- [ ] **Kind of project** = HTML (otherwise the web build gets downloaded instead of played)
- [ ] File ticked **"This file will be played in the browser"**
- [ ] **Mobile friendly** -- it alone decides what itch offers a visitor on a phone
- [ ] **Orientation** declared
- [ ] **Classification** tab: genre, tags, **player count**, multiplayer mode
- [ ] **Cover 630 x 500** -- the only image seen by visitors who do not open the page
