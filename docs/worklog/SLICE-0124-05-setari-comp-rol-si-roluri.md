# SLICE-0124-05 — «Semnare DDF» in Setări, and the role «Imputernicit»

**Date:** 30.10.2026.

## What changed and why

- **`SetariSemnareDdfView` (new settings page, key `semnareddf`)**: grid row per compartment from unit documents/table, selected rather than typed; role combo (`(niciun rol)` = none). «Reîncarcă» / «Salvează», no writes before save; asks before closing unsaved. Controls in `.Designer.vb`; `SetariForm` wiring through `CreateView` and nav item.
- **`sql/0124_roluri_imputernicit.sql` (new, AVACONT_COMUN)**: KBotAdmin offers the roles of `Roluri` with `Sectiune = 'KB'`. `KB_SECRETAR` and `KB_ADMINISTRATOR` already existed; the script adds
  `KB_IMPUTERNICIT` (the accountant's operations) and brings the descriptions of Secretar / Administrator in line with the rule «see ONLY what is addressed to them».
  **The operator runs it** (like `AD10_03`); I did not.

## Files touched

`src/KBot.App/Setari/SetariSemnareDdfView.vb` + `.Designer.vb` (new), `SetariForm.vb`, `SetariForm.Designer.vb`; `sql/0124_roluri_imputernicit.sql` (new).

## Test results

`dotnet build src\KBot.App`: 0 errors. Nothing run on screen; the Designer of the new page was never opened in Visual Studio.

## Left unverified / deferred

- The combo column API (`Column(key).ComboItems`, `CellValueChanged`) is used as `JurnaleForm` and `SetariExtraseView` use it; not seen working here.
- The settings page is reachable by the accountant's shell only; a restricted role has no settings window and the server refuses the table's routes to them.
- The sync of sql/0124 to the units and the `Roluri` script on AVACONT_COMUN are the operator's.
