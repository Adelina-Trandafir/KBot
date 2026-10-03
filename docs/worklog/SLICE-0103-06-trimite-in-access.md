# SLICE-0103-06 — «Trimite în Access»: K-BOT budget / rectifications → the unit's Access file (operator request, 02.10.2026)

Branch: `SLICE-0100-Multithreading` (nothing committed).

## What changed and why

A button in K-BOT's «Clasificații bugetare» window that sends the K-BOT changes of the budget and the rectifications to Access, to
the correct database (the unit's, found through the registry).

- **`Views/Nomenclatoare/AccessBudgetSender.vb`** (new, `System.Data.OleDb` 8.0.0 added to `KBot.App`; ACE 64-bit only — operator,
  02.10.2026: users have 64-bit Access only):
  - `ResolveUnitFile`: `C:\AVACONT\cale.accdb` ▸ `cai.FullPath` of the classification's `IdUnitate` (relative to the registry folder);
  - the files open without a password first, then with the shared one;
  - `Send`: in one transaction `UPDATE Clasificatii SET Trim1..4 WHERE IDClsf = <IdClsfAcc>` with the budget version in force today, and
    every rectification of the year into `Rectificari` (found on IdClsf + Data + Document → updated, else inserted with Capitol..Alineat
    copied from the Access classification row). Nothing is deleted from Access.
- **`ClasificatiiForm`**: button «Trimite în Access» (enabled when a classification is chosen and nothing is unsaved), asks first,
  re-reads the SAVED budget from the server, then sends; the result and every failure are shown (KBotMessage → operator log).
- `Clasificatie` (Domain) now carries `IdUnitate` and `IdClsfAcc` (the tree route returns them).

## Files touched

`src/KBot.App/Views/Nomenclatoare/AccessBudgetSender.vb` (new), `ClasificatiiForm.vb` + `.Designer.vb`, `KBot.App.vbproj`,
`src/KBot.Domain/Nomenclatoare.vb`, `src/KBot.Api/ApiClient.Nomenclatoare.vb`, `PYTHON/routes/forexe/clasificatii_edit.py`.

## Test results

No tests written or run. `dotnet build src/KBot.App/KBot.App.vbproj` → 0 warnings, 0 errors. Never run against a real Access file.

## Left unverified / deferred

- **«din registru» was read as the AVACONT registry `cale.accdb` (`cai`)**, the same one the Migrator calls «registrul». If the operator
  meant the Windows registry (`HKCU\Software\VB and VBA Program Settings\AVACONT\<DC>`), the lookup in `ResolveUnitFile` changes; nothing
  in the export says that key holds a path. The registry path is the constant `AccessBudgetSender.DefaultRegistryPath`, not a setting.
- **Password (operator, 02.10.2026):** one password shared by all the Access files, hard-coded in `AccessBudgetSender` in XOR-obfuscated form
  (it only hides the text from a look at the binary; it is not protection). A file is opened without it first, then with it
  (`Jet OLEDB:Database Password`). If ACE rejects the password on an UNprotected file, the first attempt already covers that case.
  The password now lives in `KBot.Common/AccessFilePassword.vb` and the Migrator uses it too (operator, 02.10.2026): `AccessProvider.Open`
  tries the typed password (when any), then the shared one, then none. KBot.Common 1.5.11.0, KBot.Migrator 1.17.1.0.
- `dotnet build src/KBot.App` currently fails in `Update/AppUpdateService.vb` (`UpdateProgressForm` constructor now asks for `access`): that is
  someone else's in-progress edit of the update system, not part of this slice; the Migrator builds clean.
- Which budget goes to Access: the version in force today into `Clasificatii.Trim1..4` (Access keeps one budget row per classification).
- Access columns come from `docs/MAPARE_NOMENCLATOARE.md`, not from the live file; `Esinc` / `DTQ` are left to Access.
- Writing while the Access application has the file open exclusively fails; the message says so.
