# SLICE 0000-59 — help for the K-BOT message window and the «send the error» button (operator, 09.10.2026)

## What changed and why
The operator asked that the message system (slices 0112 to 0112-04) be covered in the help too. New topic **`contabil.mesaje`** («Mesajele K-BOT», part contabil, order 92,
`screens: KBotMessageBoxForm`, so F1 on a message opens it):
- what is in the window (glyph, heading, buttons), where the answers stand («Da» / «OK» / «Reîncearcă» right; «Nu» / «Anulare» / «Renunță» left), Enter, Esc, when the X shows;
- **«Trimite eroarea»**: shown only on error messages, in the title bar; dimmed while it sends, gone after a successful send (an error cannot be sent twice), no message about the
  result either way (a button that stays means it did not go), needs a connection.
Left out on purpose (help is for users only): what the report holds, the table, the logs, the route. No capture: an error box only appears in a real situation.
Tags: `0112, 0112-03, 0112-04, 0000-59`. Watermark and `help-version.txt` moved to 2026-10-09. NOUTATI 1.1.2.1 got the «button dimmed / disappears, no message» line and the help line.

## Files touched
New: `src/KBot.App/HelpContent/contabil/mesaje.md`. Edited: `HelpContent/help-version.txt`, `docs/release-notes/NOUTATI.md`, `state/KBOT_STATUS_0000-0009.md`, `KBOT_STATUS.md`.

## Test results
`Check-Help.ps1 -Coverage`: the new topic has no error of its own (the file's slice tag is now known). The 21 other errors listed by the checker are the older tutorials without slice tags
(`tutorials\ordonantare-din-plata.md` and others) and were there before this task. Not run on screen.

## Left unverified / deferred
- Nothing seen on screen. The text describes the behaviour from the code: the dimming, the disappearing and the silence were written in this same session and never run.
- Captures that show a native Windows message box stay stale (listed under «Ajutor de actualizat»).
- Not committed (operator rule: no git writes).
