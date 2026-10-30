# SLICE DIR1-03 - the director signs ONLY in the «Ordonator» field (30.10.2026) - CLOSED

## What it does
A signature a director adds in any other field is NOT uploaded; an error says so, and K-BOT then puts the document back as the server has it.

- `PdfSigningSession.AllowedNewRoles` (Nothing = no limit), set by `DdfView`, `OrdView` and `NoteCabView` from `DirectorMode.SigningRoles` (= «Ordonator») when the session is a director's (`NoteCabView` takes an optional `k_session`, passed by `KbotForm.CreateView`). Notes have roles S1/S2, so a director cannot sign them.
- `CheckOnceAsync`: right after «the signed fields changed», the signatures added since the baseline (`SignatureRecords.Added`) are tested; another or unknown role ends the check with `PdfSigningStatus.Refused`. Nothing is uploaded; `SigningMessages.ShowOutcome` shows the sentence with the error icon (it says what happened, not what to do). It runs on Adobe's placeholder, before the PIN.
- After the message is closed, `RestoreAfterRefusalAsync` (in the three views) ends the session, lets the document go (Adobe closes it), `SigningRestore.DeleteLocalCopiesAsync` deletes the watched file and its cache copy (retried ~5 s while Adobe holds it) and the view loads the document again, which downloads the original. No file is left, so the «nu era identic» message does not appear, and every later attempt is judged afresh.
- Every exit of the check writes a «Verificare după salvare: ...» line in `adobe_preview.log` (file unreadable / identical to the server copy / no signature / same fields / fields now).

## Traps
- Without the restore, the file kept the refused field, so a second save had the same field set as `_refusedKey` and stayed silent.
- A «Verificare ... identic cu cel de pe server» line right after an upload is normal (the watcher sees K-BOT's own cache write).

## Files touched
`src/KBot.App/Views/{SigningRestore.vb (new),PdfSigningSession.vb,SigningMessages.vb,DdfView.vb,OrdView.vb,NoteCabView.vb}`, `KbotForm.Views.vb`, `Director/DirectorMode.vb`. Help: 0000-141 (`director.semnare`).

## Test results
Build 0 errors, 0 warnings. Real test, operator, 30.10.2026, `adobe_preview.log`: a signature in B → «Semnătură în câmp nepermis (B) — documentul NU se încarcă» (12:42, 12:43, 12:51); at 12:51 the refusal was followed by «fișierul local șters, se descarcă originalul», a new session, then a signature in «Ordonator» → «Document semnat încărcat: DDF 185, roluri «A,B,Ordonator»». `harness_errors.log` had no new entry. The operator confirmed the messages and the behaviour.

## Unverified / deferred
- Only the DDF was tried; ORD and a note go through the same code (`AllowedNewRoles`, `RestoreAfterRefusalAsync`) but were not signed by a director.
- If Adobe keeps the file locked for more than ~5 s the copy stays and the next open replaces it (with the «nu era identic» message).
- Client-side only: the server does not refuse a director token on write routes (see DIR1-01).
