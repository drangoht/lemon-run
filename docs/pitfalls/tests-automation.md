# Pitfalls -- Headless tests and game driving


See the **`/verify-in-game`** skill for the full procedure. The pitfalls, in short:

- **Focus is THE blocking point**: out of focus, Unity receives no key and no mouse movement -- the
  test lies silently. `SetForegroundWindow` alone fails from a non-interactive shell; only a **real
  click** gives focus legitimately.
- **`keybd_event` must carry the scan code** (Unity reads the raw input), and **arrow keys require
  `KEYEVENTF_EXTENDEDKEY`** -- without it, their scan code is the numeric keypad's and the key is
  silently lost.
- **`SetCursorPos` puts nothing into the input stream**: use `SendInput` with
  `MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE`, coordinates normalised over 0..65535.
- **An instant press only tests `wasPressedThisFrame`**: anything requiring a hold needs a real hold.
  Concluding "the arrow keys do not work" from an instant press is wrong.
- **The Unity splash lasts ~2 s**, and **the Windows firewall opens a modal alert on the first launch
  of every new exe path** -- it steals focus and greys out the window.
- **Do not hard-code the position of the elements you aim at**: an overhaul moves them, and the clicks
  land in the void with no error at all.
- **PlayerPrefs are persistent**: driving an option with N presses gives a result *relative* to the
  previous session.
- **Pixel analysis thresholds**: two false conclusions in a row (a centroid contaminated by a piece of
  scenery, a count of light pixels that was counting the HUD text). Frame outside the HUD, exclude
  every known element by its hue, **then look at the image**.
- **What is pure geometry cannot be proven by playing**: a throwaway file in `Assets/Editor` called
  with `-executeMethod` logs the bounds to within two pixels. That is what caught a zone three times
  too wide whose formula read perfectly.
- **Never look up the game window by its TITLE.** `find_window` matched any visible window whose
  title *contained* "Lemon Run" and kept the first one. An editor showing a file called
  `new-game.ps1 -Name "Lemon Run"` in its tab carries that string in its window title -- so
  `tools/build.ps1 -Run` framed **the editor**, and `docs/check.png` came back a flawless screenshot
  of VS Code. Every safeguard agreed: the window "existed", `GetForegroundWindow()` matched it (the
  editor really was in front), exit code 0. Reproduced with a Notepad on a `Lemon Run.txt`: it comes
  **before** the game in the `EnumWindows` order, so it wins. Match on the **owning executable**
  (`GetWindowThreadProcessId` + `QueryFullProcessImageNameW`), which cannot be borrowed by accident.
- **A capture that frames the wrong window is the worst failure of this tool**: it does not crash, it
  produces a plausible image, and whatever is concluded from it is wrong. Before trusting an odd
  capture, check the size printed by `drive_game.py`: the game is launched at 1280x720, so any rect
  far from ~1298x767 is not the game window.
- **A key bound to two actions makes a scenario measure something else.** Space is both the jump
  and the restart. A 26 s pass "jumping throughout" reported 108 m, 0 hits and 0 fruit: every
  catch had been followed by my own key restarting the run, and the capture showed a fresh one.
  Nothing was wrong with the game. Drive with a key bound to ONE action -- here the Up arrow,
  which only jumps -- or check the bindings table in GDD section 3 before building a scenario.
- **A driving cadence that beats against a game duration measures the beat, not the game.**
  Jumping every 0.60 s against a 0.62 s jump arc has every other press refused (no double jump),
  leaves the runner grounded ~48 % of the time, and turned an 80 % drop in hits into a 43 % one.
  The figure looked like a weak effect and was an artefact of the scenario. Before concluding
  anything from a repeated key, check the period against the durations the game is made of.
- **A value changed in `tuning.json` for one test stays there for every later run.** The file
  lives beside the binary and is only rewritten when it is missing, so a road packed dense for a
  measurement silently becomes the road of the next session. Put the defaults back, or delete it.
- **`prime()` presses Down then Up, and those are game actions.** The throwaway key that works
  around the lost-first-press is not neutral: Up is the jump. Every scenario driven through
  `--keys` therefore starts with a jump already under way, which shifts what the first capture
  shows. Read a capture with that in mind, or leave a beat before measuring.
- **A decoy window built with WinForms but no message pump is never enumerated**: a first attempt at
  testing the above with `$f.Show()` from a sleeping PowerShell produced a window that `EnumWindows`
  never saw -- so the test passed while proving nothing. Use a real application (Notepad) to
  reproduce a window-lookup defect.
