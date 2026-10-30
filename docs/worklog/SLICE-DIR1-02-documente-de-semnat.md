# SLICE DIR1-02 - «Documente de semnat»: the list, as a ribbon button (30.10.2026) - CLOSED

## What it does
The list of slice 0081-06 (`DirectorForm`, `GET /api/forexe/ddf/director/de-semnat`) is the director's ribbon button «Documente de semnat».

- **Button** `director_de_semnat` (Designer, `Visible = False`, shown by `ApplyDirectorRibbon`); `KbotForm.Director.vb` opens `DirectorForm` modally after `PutAway` (its document view shares the one Acrobat broker). The window is `ShowInTaskbar = False` (it is not a second application). The tray application (TRAY-01) opens documents in the same window.
- **Current unit only, no «Unitate» column**: the server lists every director unit, the form filters on `_session.DbName`; another unit is chosen from the title bar (DIR1-01).
- **`KBotDataView`** (`lstDocumente`: cod / obiect / rev / data / total, read-only, `ColumnFillMode = SpecificColumn` on «Obiect», so the columns fill the panel). Selection is `CurrentRow` / `SelectionChanged`; `SelecteazaRevizia` sets `CurrentRowIndex`. The old per-row tooltip (`DescScurta`) was not carried over.
- **The document opens without the revisions tree**: `DdfView.ArboreAscuns` (= `split.Panel1Collapsed`) and `DdfView.ArataRevizia(info, idrev)`, which sets revision and context together, so there is ONE load and, with the tree hidden, the reselect path lands on the «Document PDF» page.

## Traps
- `SetContext` followed by `Reincarca` started two loads; the second, without a revision to land on, won and left the «Toate reviziile» node (tab «Documente» = print list).
- The PDF tab is called «Document PDF» (it is «Documente» only on a month node of the main shell).

## Files touched
`src/KBot.App/Director/DirectorForm.vb` + `.Designer.vb`, `KbotForm.Director.vb`, `AvacontMain.vb` + `.Designer.vb`, `Director/DirectorMode.vb`, `Views/DdfView.vb`. Help: 0000-141 (`director.lista`).

## Test results
Build 0 errors, 0 warnings. Seen and used by the operator on 30.10.2026 (list, document straight on its PDF page, no taskbar entry): «pare ok».

## Unverified / deferred
`StartPosition` of the window is `CenterScreen` (not centred on the main window). The per-row tooltip is gone.
