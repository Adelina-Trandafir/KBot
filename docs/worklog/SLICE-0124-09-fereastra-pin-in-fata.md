# SLICE-0124-09 — The token's PIN window is brought to the front

**Date:** 30.10.2026. Corrective found while running the 0124 bench; belongs to the Adobe signing in general, not only to the flow.

## What changed and why

Twice on 30.10.2026 the signature on section B looked like a hang in the bench: `adobe_preview.log` ends at the «Token Logon» window
(16:49:27 and 17:07:01), Acrobat showed `Responding = False` and its taskbar thumbnail was blank. The operator then saw that the PIN
window had been waiting **behind** the bench window (stopping debug left it on screen for a second; one monitor only). A process
waiting modally on the token's window shows as not responding in Windows, so nothing was frozen — the window was only unseen.

`AdobeSaveTrap` now brings the signature windows of a watched Adobe process to the front, **once per window** (`_fronted`): titles in
`SigningDialogTitles` = «Token Logon», «Signing», «Sign Document». It does not move, resize or write into them (the PIN is the
operator's alone) and logs the result of `SetForegroundWindow`, including «Windows a refuzat» when another process holds the foreground.

The garbled text «%replace_password_term%» seen once in that dialog is, most likely, the trap reading the dialog before it finished
initializing; not proven, not acted on.

## Files touched

- `src/KBot.Controls/Adobe/AdobeSaveTrap.vb` — `SigningDialogTitles`, `_fronted`, `BringSigningDialogToFront`, call in `Handle`.
- `src/KBot.Controls/KBot.Controls.vbproj` — `FileVersion` 1.73.0.0 → 1.74.0.0.
- `src/KBot.App/HelpContent/contabil/ddf/semnare.md` — one line (the PIN window comes to the front; if not seen, look in the taskbar), tag `0124-09`.

## Test results

`dotnet build src\KBot.App` (output redirected, the app was running): **0 errors**. `Check-Help.ps1`: no finding for `ddf\semnare.md`
(it lists older findings in other topics). Nothing run on screen.

## Left unverified / deferred

- Not seen working: whether `SetForegroundWindow` is allowed at that moment (Windows may refuse it to a process that is not in the
  foreground); the log line says which. If it is refused, the next step is a taskbar flash, not forcing the focus.
- The titles are the English ones measured on this PC («Token Logon», «Signing», «Sign Document»); a Romanian Adobe may title the signing
  window differently. The token's own window comes from the token software, not from Adobe.
- Help capture of the PIN window: none taken.

## Second pass (same day, 17:2x) — the first fix did not hold

The bench blocked again. `adobe_preview.log` shows the new code ran («adusă în față: da» for «Sign Document», «Signing» and «Token Logon»), yet the
PIN window never appeared; the garbled text («%replace_password_term%») came again exactly on the blocked ones (17:07:01, 17:17:58) and on
none of the windows that worked. So the window was not merely behind another one.

Read live from the machine while it was blocked (PowerShell, read-only): the «Token Logon» window (process 33444) was **not visible**
(`vis=False`) and its OWNER was the bench form (the hosted Adobe window is a child of K-BOT, so a dialog of Acrobat ends up owned by a K-BOT
window); the bench form and the harness window both reported **hung** (`IsHungAppWindow`); a second Acrobat process (45952, started in the
same second as 33444) was `Responding = False` with all its windows invisible. The operator's debugger pause stopped in
`AdobeSaveTrap.OnTick` (the UI thread), and closing the harness made the PIN window appear.

Cause hypothesis (NOT proven): `AdobeSaveTrap.Handle` called `GatherFacts` on every dialog on every 200 ms tick, and `GatherFacts` sends
`WM_GETTEXT` to each static of the dialog (2 s timeout each). While the token's window is busy with the token it does not answer, so the UI
thread spends almost all its time inside the tick, the window it owns (K-BOT) cannot serve it, and the dialog never finishes appearing.

Change: the signature's own windows (`SigningDialogTitles`, in a watched Adobe process) are now recognised by TITLE ALONE, before
`GatherFacts`; they are brought to the front once and are never read. The log line says «nu i s-a citit textul».

Still open: if it blocks again with this change, the cause is elsewhere (the cross-process parent/child link between the hosted Adobe window and
the bench form, or the token software itself). The evidence to ask for then is the debugger's call stack of the UI thread at the pause, with
native frames shown, and the same window listing as above.

## Result of the second pass

The operator's run of 17:28-17:31 (adobe_preview.log): A and B signed and uploaded with no block, the PIN window appearing each time; the director
step (17:31-17:32) also finished: 3 signatures, 1,352,000 bytes, uploaded identical, Adobe closed, local copies deleted. One run is not proof the block is gone (it was intermittent: 2 blocks in about 10 PIN windows
before); keep the call-stack evidence request above for the next time it blocks.
