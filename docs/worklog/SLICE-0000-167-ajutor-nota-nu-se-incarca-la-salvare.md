# SLICE-0000-167 - help: the correction note is not uploaded into CAB at save (07.11.2026)

## What changed and why
Slice 0088-09 removed the upload question after saving a note. `contabil/notecab.md`, «Cum faci nota», step 3 now says the note is not uploaded at save (no signatures yet), is signed in «Note corecție» and sent with «Trimite în CAB». Slice tag of the section: `0088-09, 0000-167`.

## Files touched
- `src/KBot.App/HelpContent/contabil/notecab.md`

## Test results
- `Check-Help.ps1` not run.

## Left unverified / deferred
- Not seen on screen. The `operatiuni-necorelate` capture is unaffected (the window did not change visually).
