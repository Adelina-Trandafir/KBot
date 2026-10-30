# SLICE-0010-08 - KBotDataView: Esc cancels the edit of the cell, a second Esc cancels the row - in every table (operator, 29.10.2026)

## What changed and why
Operator: in the tables, Esc cancels the edit in the current box; a second Esc in a row cancels the row - a new row is deleted and an empty one appears, or it is emptied, whichever is simpler.
Then: **on every table where the operator edits, and this is the default of the grid**; another logic only when the operator asks for it.

**Grid (`KBot.Controls/DataView`)**
- Esc in an open editor cancels the edit of the cell (as before) and **arms** a second Esc (`_escArmed`).
- `EscapeCancelsRow` (Boolean, **default True**, `K-BOT` category): off = only the cell is cancelled.
- The **baseline** of a row (`KBotDataRow`, Friend `CaptureBaseline` / `TakeBaseline`): a copy of its cells taken right before the operator's first write to it (text/combo commit, check box, option
  button, group tick); dropped by `MarkClean` / `ClearDirty` and by `ClearRows`. A row the host created and marked dirty by hand (the «+» rows) gets its baseline at the first write too.
- The second Esc belongs to the grid only while the current row HAS a baseline (`IsInputKey`): if the operator changed nothing in the row, Esc goes on to the form, so a dialog still closes on it.
- Second Esc: `RowCancelRequested` (`KBotRowCancelEventArgs` = column, row, `Handled`) goes to the host first; when it is not handled the grid runs `RevertRow` (public): the cells go back to the baseline,
  `CellValueChanged` is raised for each cell that changes (so a host that mirrors cells into a model stays right), the row is clean again.
- The arming is dropped by anything else: another key, a mouse press, a change of the current cell, the next `BeginEdit`.

**Hosts with their own handler** (a row the operator just made is REMOVED; the rest use the default revert)
- «Sabloane» (`SablonOperatiiView`): type A - the line goes (a new empty row stays at the end); type M - the line is emptied and keeps its form.
- «Sabloane salarii»: the three line tables - a line with a new (negative) id goes; an existing one goes back.
- «Facturi» lines (`FacturiForm.Linii`): a line that came from the spare row goes; a line of the invoice goes back; the spare row does nothing.
- DDF section A (`DdfEditSectiuneaAPage`): a line without a key (`IdSecA = 0`) goes without a question; a saved line is left as it is (its classification feeds other cells, so they are not put back one by one).
- Clasificatii venituri and Clasificatii buget (buget + rectificari), Jurnale, Parteneri «Coduri»: a row without an id (added with «+») goes; the others go back.
- Every other editable table (Parteneri balances, Conturi E-Factura, Plan de conturi balances, Ord pages, DDF other pages, Setari, Forexe forms ...) uses the default: a row added by the operator is **emptied** to
  the values it had when created (the host created it with defaults), an existing one goes back to what it was.

**«Sabloane» also**: a «✕» column on the type-M table (empties the line, the form stays; type A still removes it); «Salveaza» 25 % shorter (190 -> 143 at the 144-dpi designer; `AutoSize` off, side padding 3).

## Files touched
`KBot.Controls/DataView/KBotDataRow.vb`, `KBotDataView.Editing.vb`, `KBotDataView.Input.vb`, `KBotDataView.CheckBoxes.vb`, `KBotDataView.vb`, `Events/KBotRowCancelEventArgs.vb` (new);
`KBot.App`: `SablonOperatiiView` (+ Designer), `SablonSalariiView`, `DdfEditSectiuneaAPage`, `ClasificatiiVenituriView`, `ClasificatiiBugetView`, `JurnaleForm`, `ParteneriForm`; `KBot.EFactura/FacturiForm.Linii.vb`;
help `HelpContent/contabil/liste.md` (new section), `nomenclatoare/sabloane.md` (0000-139, 0000-140); `docs/release-notes/NOUTATI.md`.

## Test results
`dotnet build src/KBot.App` (builds KBot.Controls and KBot.EFactura): 0 warnings, 0 errors. No tests run (operator rule); nothing seen on screen.

## Unverified / deferred
- Nothing seen on screen: the Esc sequence on any table, the 143-px «Salveaza», the «✕» on the type-M table.
- The default revert re-raises `CellValueChanged` with the OLD values: every host's handler runs once per changed cell. Read for the Ord pages, DDF and Facturi (they write `e.NewValue` into their model); not
  read for Setari, Forexe forms, Plan de conturi balances, E-Factura Ke*/Primite lists - they get the default untested.
- A row marked dirty by hand and edited again after a save without `ClearDirty` would go back to the state before that save; every table that saves calls `ClearDirty` or loads the rows again (checked by grep, not by use).
- The baseline is the state at the FIRST operator write: if the host changes cells by itself in between (computed columns), those go back too, which is meant.
- Unchanged behavior to know: a row the operator did not change leaves Esc to the form (a page closes on the second Esc as before).
