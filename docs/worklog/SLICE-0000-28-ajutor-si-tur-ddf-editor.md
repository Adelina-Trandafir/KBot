# SLICE-0000-28 — the help for 0084/02 + 0094-02, and the guided tour of the DDF editor

Operator request (01.10.2026): record the Sumar changes in the last slice that touched Sumar
(0084/02), the DDF editor changes in the last slice that touched it (0094-02), and add the
**guided tour of the DDF editor window** — new, and part of slice 0000.

## What changed and why

| Topic / tour | What |
|--------------|------|
| `contabil.vederi.sumar` | new section «Asociază parteneri» (when the button shows, the window, «De adăugat», add only); `SumarPartnersForm` in `screens:`; keywords partener / asociere. Tag `0084-02, 0000-28` |
| `contabil.ddf.editor` | the «Partener asociat» row of the header table rewritten (main partner; several partners → page «Parteneri»); new section «Parteneri» (the right-hand page, «Principal», «Asociază», «Scoate din asociere», saved with the document, the main partner is the only one written on A and B); `DdfEditPartnersPage` and `DdfPartnersView` in `screens:`. Tags `0094-02, 0084-02, 0000-28` |
| `tur-ddf-editor` (**new**, 13 steps) | header, «Partener asociat», the main-partner combo, the pages, each page (Secțiunea A / B / Descriere / Fișiere / Parteneri, two steps for «Parteneri»), «Salvează documentul», «Renunță». `screens: DdfEditForm`, so the «?» popup of the editor window offers it; `topic: contabil.ddf.editor` gives the topic its «Tur ghidat» link |
| `tur-sumar` | new step «Asociază parteneri» on `SumarView.btnPartners` |

The pages are rung through the nav list (`part: item:<Key>`); a page is not opened by the tour, so the
«Parteneri» steps ring its entry and say what the page does. «Pagini › Secțiunea B» is skipped by the
engine while the entry is hidden (before the revision is sent).

Left out on purpose (HELP_SYSTEM §5): the table, the routes, how the list is stored.

## Files touched
`src/KBot.App/HelpContent/contabil/vederi/sumar.md`, `contabil/ddf/editor.md`,
`tours/tur-ddf-editor.md` (new), `tours/tur-sumar.md`, `docs/worklog/state/KBOT_STATUS_0000-0009.md`
(row, watermark), `src/KBot.App/HelpContent/help-version.txt` (`2026-10-01`, unchanged).

## Test results
- `tools\HelpCheck\Check-Help.ps1 -Coverage`: **«No errors.»**, 48 topics, **18 tours**, 57 capture
  tags. Coverage names only `RobotQueueForm`, `SetariIstoricView`, `UpdateOfferForm` — gaps that were
  there before (0098 and the update windows); the three new types of 0084/02 and 0094-02 are covered.
- `dotnet build src/KBot.App/KBot.App.vbproj`: **0 warnings, 0 errors**.
- The tour and the help were **not seen on screen**.

## Capturi de refăcut / noi
- `ddf-editor` (editor header now has a right-hand «Parteneri» entry) and `sumar` (a button next to the
  header) look different: re-shoot with «Refă». No new capture tag was added.

## De citit de operator
- `contabil.ddf.editor`, section «Parteneri»: written from the code; the page was not seen. In
  particular «Dacă debifezi «Partener asociat», partenerul principal dispare din listă» describes
  `DdfDraft.SyncHeaderPartner` (the extras stay).
- `tur-ddf-editor` step «Paginile documentului» says the right-hand page «nu face parte din revizia
  propriu-zisă» — wording to confirm.
