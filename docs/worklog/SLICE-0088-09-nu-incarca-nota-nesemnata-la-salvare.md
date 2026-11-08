# SLICE-0088-09 - the saved correction note is no longer offered for upload into CAB (07.11.2026)

## What changed and why
Operator: when a correction note is saved it must NOT be offered straight away for upload into FOREXE, because it has no signatures yet.
- Checked first (verified in code): the gate did NOT exist. `CabNoteForm.btnSalveaza_Click` ran, after the save and the PDF, a loop calling the shell's `UploadCabNoteAsync` for every note, i.e. the question «Vrei să încarci NOTA DE CORECȚIE în CAB?» (with only a warning when the PDF was unsigned).
- Now the window saves the notes + the unsigned PDFs and closes. The upload stays where the signed document is: view «Note corecție», ribbon button «Trimite în CAB» (`NoteCabView.UploadAsync`), unchanged.
- The `uploadToCab` parameter and field are removed from `CabNoteForm` (nothing uses them any more); the two callers in `KbotForm.CabNotes.vb` no longer pass `AddressOf UploadCabNoteAsync`.

## Files touched
- `src/KBot.App/KBotForm/Views/NoteCab/CabNoteForm.vb`
- `src/KBot.App/KBotForm/Operations/KbotForm.CabNotes.vb`
- Help: `src/KBot.App/HelpContent/contabil/notecab.md` (see SLICE-0000-167)
- Status: `state/KBOT_STATUS_0080-0089.md` (row 0088-09), `state/KBOT_STATUS_0000-0009.md` (row 0000-167)

## Test results
- `dotnet build src/KBot.App`: 0 errors, 0 warnings. Nothing run, no tests (operator's rule).

## Left unverified / deferred
- Not run, not seen on screen.
- «Trimite în CAB» in the view still only WARNS when the document is unsigned («documentul NU este semnat; FOREXE îl poate refuza») and lets the operator go on. Whether to block it until the note is signed was not asked; left as is.
