# SLICE-0000-34 — Help for slice 0100 + stale captures marked in red (operator request, 01.10.2026)

Help work for feature slice **0100** (multi-thread downloads, branch `SLICE-0100-Multithreading`). Recorded as 0000-34
because all help work is slice 0000. The sections carry `<!-- slice: 0100 -->` as the rules ask.

## What changed and why

**Help text.**
- New topic `contabil.forexe.descarcare-multipla` («Mai multe descărcări deodată»): how to switch it on (advanced
  options), the five options with their defaults, the «Actualizează angajamente» window and its «Actualizat» column,
  how a download runs (N at once, the rest in a FIFO line, one failure does not stop the others, nothing saved until
  all ended, saved one after another, one tab left open), the on-connect update, the new-angajamente question,
  «Actualizează implicit toate recepțiile».
- `contabil.setari`: the «Generale» tab grouped (new section «cum e grupată fila»); `avansat` (step 3 names the new
  group); `contabil.forexe.descarcare` (picker skipped option + pointer), `.lista` (the new question), `.coada`
  (one task in the queue).

**Capture mechanism (new).** A capture tag may carry `redo: yyyy-MM-dd HH:mm` + `why: <slice>: <what changed>`.
`HelpCapture.RedoSince/RedoWhy/NeedsRedo`; the capture list (`HelpCaptureForm`) paints a row RED (theme
`ErrorColor`) while its saved picture is older than the `redo:` moment, writes «· de refăcut» in the state column,
shows «DE REFĂCUT — why» above the preparation text and counts them in the summary line. Retaking or loading the
picture clears it by itself. `Check-Help.ps1` knows the two keys. Documented in `HelpContent/README.md` and
`docs/HELP_SYSTEM.md`.

## Captures to redo / new
- Red now (existing pictures, tagged `redo: 2026-10-01 22:48`): `setari`, `avansat-activare` (the «Aplicație» page was regrouped).
- New, missing (shown «lipsă»): `setari-descarcari-multiple`, `arbore-meniu-actualizare`, `actualizare-multipla`.

## Files touched
`src/KBot.App/Help/HelpCapture.vb`, `HelpCaptureForm.vb`; `tools/HelpCheck/Check-Help.ps1`;
`src/KBot.App/HelpContent/` `contabil/forexe/descarcare-multipla.md` (new), `contabil/setari.md`, `avansat/index.md`,
`contabil/forexe/descarcare.md`, `lista.md`, `coada.md`, `README.md`; `docs/HELP_SYSTEM.md`; status files.

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. `Check-Help.ps1 -Coverage`: the only errors left
are the slice tags `0100` (valid only once the fork with slice 0100 is merged — `KBOT_STATUS.md` on this branch has
no 0100 row) and the older `0077-3` tag in `contabil\fereastra.md`. Nothing run on screen.

## Unverified / deferred
- The red rows were not seen (no screen run). The `0100` tags validate only after the fork is merged into `master`.
- The tree's «Actualizat: …» tooltip line is not in the help (no capture shows a tooltip of the list).
