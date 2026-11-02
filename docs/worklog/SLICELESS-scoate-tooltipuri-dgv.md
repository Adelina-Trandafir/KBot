# SLICELESS - no tooltip on a DGV itself (02.11.2026)

Operator rule: a `KBotDataView` never has a tooltip of its own, and its column headers have none either; the tooltips on CELLS stay (untouched).

## What was removed
Every `tips.SetToolTipHeader/Text(<grid>, ...)` and every `<column>.HeaderTooltip = ...` in the Designer files that declare a `KBotDataView`: 44 grid lines + 126 header lines in 20 files.
- KBotAdmin: RecordHistoryForm, RightsForm, and the views Audit, Backups, Diff, LocalBackup, LoginReset, MfImport, OnceQuery, PushFiles, Requests, Rights, Scripts, SourceQuery, Visits.
- KBot.App: Avacont PlanContSolduriView, SablonOperatiiView; Director DeSemnatRestransForm, DirectorForm.
- KBot.EFactura: PrimiteListaPage.
Cell tooltips (`KBotCellTooltip`, `CellFormatting`-driven) were not touched. Searched: every `.vb` outside `bin`, `obj`, `Surse`, `_reference`, `Backup`.

The exact lines that went (file, tab, line) are in [SLICELESS-scoate-tooltipuri-dgv.removed.txt](SLICELESS-scoate-tooltipuri-dgv.removed.txt), in case a header text is wanted back (it can serve as the cell tooltip text).

## Verified / unverified
- `dotnet build` of KBotAdmin and KBot.App: 0 warnings, 0 errors. A second scan finds no grid-level or header tooltip left.
- Nothing was opened on screen. Commented-out tooltip lines were left as they are.
- Not in the K-BOT help (the help does not describe grid tooltips).
