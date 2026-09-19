# Pitfalls -- PowerShell (build and release scripts)


**WARNING: NEVER test `$?` after a native executable in PowerShell 5.1.** `git`, Unity and Butler
write their progress on **stderr even when all is well**, which sets `$?` to `$false` while the exit
code is 0. The release script announced "git push failed" at **every successful release**. Only
`$LASTEXITCODE` is authoritative.

**WARNING: `$ErrorActionPreference = 'Stop'` is a trap in a build script**, for the same reason: the
slightest progress line on stderr aborts the script.

**WARNING: `$LASTEXITCODE` is not authoritative after calling a *.ps1* either -- it is not set at
all.** `& script.ps1` leaves the variable exactly as it was; only a native executable or an
explicit `exit` writes it. `build.ps1` called `exit 1` on every failure path and nothing at all on
success, so a caller reading `$LASTEXITCODE` got a **stale** value -- and in a fresh session that
value is `$null`, which `-ne 0` evaluates to **TRUE**. `release_itch.ps1` therefore declared the
build failed immediately after printing `web build OK`, on the very first release the project ever
attempted. Symptom to recognise: a script that announces success and failure in consecutive lines.
Fix: **every script called by another ends with an explicit `exit 0`.**

**WARNING: `$null -ne 0` is TRUE.** Any `if ($LASTEXITCODE -ne 0)` guarding a command that may not
have run, or that is not native, fires on nothing at all. When the variable may be unset, test it
as `if ($null -ne $LASTEXITCODE -and $LASTEXITCODE -ne 0)` -- or make sure it is always set, which
is the better fix.

**WARNING: a release script that can only be tried by publishing is never tested anywhere but in
production.** Hence `-DryRun`, which goes as far as the distribution folder and stops before any
visible effect. It is what caught the bug above, before it cost a broken release.
