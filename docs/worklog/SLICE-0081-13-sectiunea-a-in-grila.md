# SLICE 0081-13 — Section A entered straight in the grid; no new angajament while reservations lack a DDF

**Date:** 23.10.2026. **Request (operator), «+» / «Angajament nou» and the DDF editor:**
1. If there are downloaded angajamente WITHOUT a DDF but WITH reservations, a new angajament cannot be created.
2. In the «Adauga DDF» editor the grid always has an empty row, not via «Adauga rand».
3. With «Adauga rand», after picking a classification the budget and the other values did not update in the grid — they must update straight in the editor's grid.
4. The «Clasificatie» cell is a combo tree with ONLY the classifications of the selected «Program».
5. «Sterge rand» is a button in the grid, like everywhere else.

## What changed

- **`KbotForm.Ddf.vb`:** `DeschideAngajamentNou` first calls the new `AngajamenteCuRezervariFaraDdf()`: the
  rows of the loaded tree (`_treeRows`) with `AreRezervari` and not `AreDDF`, hidden ones left out. Any
  found → a message naming up to 8 codes («Adaugati intai DDF-ul...») and the editor does not open.
- **`DdfEditSectiuneaAPage` (+ Designer):**
  - The buttons `btnAdauga` / `btnSterge` are gone; `tlyButoane` keeps only the status label. The
    line window `DdfEditLinieAForm` is no longer opened from here (the class stays in the project, unused).
  - **Permanent empty row** (`UmpleGrila` appends a row with `Tag = Nothing` unless the grid is locked).
    `Grd_CellFormatting` makes every cell of it read-only except «Clasificatie» (read-only too while a
    new angajament's header is incomplete), blanks and disables its delete button, and shows a hint.
  - **«Clasificatie» column** is visible again: a combo with `CommitOnSelect`, fed by
    `ActualizeazaListaClasificatii` with `KBotComboGroupedItem`s — only `ClasificatiileLibere` (the
    header program's SSs, minus the classifications other lines use), the ones already in the angajament
    first, the session's SS first; one folding header per «Sursa XX — din angajament / alte clasificatii».
    The list is rebuilt whenever the status line is (`ActualizeazaStarea`) and on every header edit
    (`SeteazaLipsurileAntetului`), so a program change reaches it.
  - **Picking** (`SchimbaClasificatia` / `AplicaClasificatia`, ported from `DdfEditLinieAForm.ScrieInLinie`):
    on the empty row it creates the line (joins `_draft.LiniiA`, a new empty row appears, the cursor goes
    to «Val. curenta»); on a line it changes the classification. Budget, receptions, available, previous
    value and remaining are written into the row at once. Typed text must name an item of the list
    (`ValideazaClasificatia`), else the cell is refused.
  - **«sterge» button column** (`✕`, like ClasificatiiForm / ParteneriForm): `Grd_ButtonClick` asks
    «Stergi randul?», removes the line and the grid row, raises `DraftModificat` (section B is rebuilt by
    the form). Hidden on a grid locked because the document comes from reservations.
  - A MANUAL angajament's list depends on its first line, so every add / change / delete refetches it
    (`ReincarcaListaManualaAsync`).
  - «Clsf» column is hidden (the code is inside «Clasificatie»); the cell label is now
    `code — denumire` with the code written as forexecab writes it.
- **Help (0000-NN, slice tag 0081-13):** `contabil/ddf/editor.md` (section «Sectiunea A» rewritten, capture
  `ddf-rand-nou` removed, `ddf-editor` capture marked for redo), `contabil/ddf/nou.md` (the block + redo
  mark), `tours/tur-ddf-editor.md`.

## Files touched

`src/KBot.App/KbotForm.Ddf.vb`, `src/KBot.App/DDF_EDIT/DdfEditSectiuneaAPage.vb`,
`src/KBot.App/DDF_EDIT/DdfEditSectiuneaAPage.Designer.vb`, `src/KBot.App/DDF_EDIT/DdfEditForm.vb` (two
comments), `src/KBot.App/HelpContent/contabil/ddf/editor.md`, `.../nou.md`, `.../tours/tur-ddf-editor.md`.

## Test results

- `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors**.
- `Check-Help.ps1`: no message about the three help files touched (the run exits 1 on messages about
  `tutorials/ordonantare-din-plata.md`, which this work did not touch).
- No test written or run (house rule); **not seen on screen**.

## Unverified / deferred

- Not run in the app: the grouped combo inside a grid cell (first use of `KBotComboGroupedItem` in
  `KBotDataView`), `CommitOnSelect` with those items, typed find in the cell, the cursor jump to
  «Val. curenta» after a pick, the placeholder text of the empty row, the dead button on it.
- «Din angajament» / «alte clasificatii» are one level of folding (the combo has no deeper tree); a real
  tree by chapter would need a new control — say so if wanted.
- Item 1 reads the tree that is loaded (current year / SS selection, hidden angajamente off). An
  angajament outside that selection is not seen. A server-side check would need a new route.
- `DdfEditLinieAForm` is now dead code, kept until the operator says to delete it.
- `NOUTATI.md` not written (release notes are written at release time).
