# Pitfalls -- Input


**WARNING: `KeyCode` and `Key` designate a POSITION on a QWERTY keyboard**, never the printed
character. On an AZERTY keyboard, `Key.A` / `Key.D` / `Key.W` place the controls under the keys marked
**Q / D / Z**. That is the intended result, not a bug. Corollary: avoid `A`, `Q`, `Z`, `W`, `M` for
global shortcuts -- prefer `Tab`, `R`, the digits or the arrow keys, whose position is common to both
layouts. **This pitfall was only discovered by injecting real keys.**

**WARNING: `InputSystemUIInputModule` and not `StandaloneInputModule`.** With the Input System package
active, the old module receives nothing: the UI simply stops responding, with no error.

**WARNING: the very first key after taking focus is lost**, on the Windows build as in the browser.
Always send one for nothing before measuring anything.

**WARNING: the Input System package can be INSTALLED and yet INACTIVE, and then the game answers no
key at all.** `ProjectSettings/ProjectSettings.asset` carries `activeInputHandler`: `0` is the old
Input Manager, `1` the package, `2` both. The template shipped with **0**. Everything else looks
right -- the package is there, `using UnityEngine.InputSystem` compiles, the build succeeds, the
game runs -- but `Keyboard.current` is **null**, so every read returns nothing. Cost here: a full
debugging pass on the runner, on the key injection and on the window focus, all three of which were
fine. Symptom to recognise: the game is alive and animating, and *nothing whatsoever* responds.

**WARNING: never `return` quietly on a null `Keyboard.current`.** That is what turned the pitfall
above into an invisible one: the guard reads as prudence ("no keyboard plugged in") while in
practice it means the input handler is misconfigured. `Runner.ReadInput()` now reports it once,
as an error.
