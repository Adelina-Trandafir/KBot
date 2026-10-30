# SLICELESS - KBotAdmin «Import MF din Access» (27.10.2026)

Owner's tool only (KBotAdmin, never published). The active fixed assets of an old `mf.mdb` (the pre-VERCON register) are brought into an MF gestiune of one unit. The old register had no holders, so the assets enter the gestiune only; the holder comes later with a handover voucher (bon de predare) made in VERCON.

## How it works
Page «Import MF din Access» (ribbon Publicare > VERCON, key `mf-import`):
1. **«Alege fișier...» / «Citește mijloacele fixe»** - `MfAccess` copies the file to a temporary name and reads table `Mijloace Fixe` where `STERS = False` (ACE provider, as VRC.Migrator). Nothing else of the old file is read: no depreciation history (`Amortizari`, `Luni Amortizare`), no exits (`Mijloace Fixe Iesiri`), no `Clasificari`/`Conturi`. VERCON computes depreciation at display time.
2. **«Citește subunitățile»** - verb `vercon-mf-targets` for the unit ticked in the tree: its subunits, its MF gestiuni (`VC_Gestiuni.Type = 'MF'`) and the warehouse accounts of each (`VC_Masini.TIP` in `MFM`/`MAGAZIE`). The operator picks subunit, gestiune and account.
3. **«Verifică»** - verb `vercon-mf-check`, writes nothing. Per ticked row: the classification (Access `Categorie`) must exist in `AVACONT_COMUN.VC_ClasificariMF` (a trailing dot is ignored) and DNF must lie inside its limits (years x 12, the same rule as the receipt page); the inventory number must not already exist in the gestiune or repeat in the list; value, DNF, DF, date valid; the gestiune must have an active VAT rate of 0.
4. **«Importă rândurile bifate»** - verb `vercon-mf-import`: checks again, then writes through `receipts.create_nir` (the code of the VERCON receipt page), one `STOC_INITIAL` document per asset dated on its Access `DataIntrare`, in one transaction (all or nothing). Refused if another user holds the gestiune lock. One `VC_Audit` row `mf.import` lists the codes.

## Field mapping
| Access | VERCON |
|---|---|
| `CodProdus` | inventory number (`VC_ProduseP.COD`) |
| `DenumireProdus` | `VC_Produse.DenumireP` (UM = `buc`, one product per asset) |
| `Categorie` | `VC_Produse.CC` (checked in `VC_ClasificariMF`) |
| `ValoareIntrare` | entry value, VAT rate 0 so it is kept as is (`PUTVA` = value) |
| `DataIntrare` | `VC_ProduseM.DataIntrare` and the date of the document |
| `DNF`, `DF` | `VC_ProduseM.DNF`, `DF` (DF is the time used BEFORE the first handover; in the old file DF is 0 at entry) |
| account | chosen by the operator (warehouse account of the gestiune) |

## Files touched
- `PYTHON/routes/admin_cli/vercon_mf_import.py` (new) + 2 lines in `admin_cli.py` (register).
- `KBotAdmin/Server/MfAccess.vb`, `KBotAdmin/Views/MfImportView.vb` + `.Designer.vb` (new); `MainForm.vb`, `MainForm.Designer.vb` (button + page); `KBotAdmin.vbproj` (`System.Data.OleDb` 8.0.0).

## Verified / unverified
- Verified: `dotnet build KBotAdmin\KBotAdmin.vbproj` clean (0 warnings, 0 errors); the Python modules compile. The Access file was read once (on a copy) from PowerShell: 6 assets in the table, 4 active (`STERS = False`), all unit 27.
- **NOT run:** the page was never opened, the server verbs never ran against MariaDB, nothing was imported. The server file must be pushed by hand (Publicare) before use.
- **Assumptions to confirm on the first run:** `create_nir` accepts a `STOC_INITIAL` document with a new product built in the same body (as the page does); `VC_TVA` has a row with TVA = 0 in the target gestiune; the 4 classifications from the old file exist in `VC_ClasificariMF` with DNF limits that contain the old DNF (108/120/60/90 months) - otherwise «Verifică» refuses those rows with the reason, and they cannot be imported by this tool.
- Not done on purpose: no help entry (admin tool, not in the K-BOT help), no tests (operator rule).
