# SLICE-00EF-66 - Legatura factura primita - operatie contabila: o singura cheie; E-Factura se muta in AVC.Migrator (09.11.2026)

Cererea operatorului: pe «Contari» operatiile facturii apareau gresit (alta operatie sau niciuna). Cauza era in date: `EF_PrimiteOperatii.IdOperatie` avea doua intelesuri (numarul din Access sau `TZ_Oper.IdOper`). Reparatia e in scriere, nu in citire: coloana are un singur inteles, iar ecranul face un join simplu. Apoi, cererea ca migrarea E-Factura sa treaca din KBot.Migrator in AVC.Migrator, cu «doar e-facturi», cu legaturile `EFT_O` rescrise complet la fiecare rulare.

## What changed and why
- **`sql/00EF_66_primite_operatii_id_access.sql`**: coloana noua `EF_PrimiteOperatii.IdOperatieAccess` (numarul Access ca data); `IdOperatie` = `TZ_Oper.IdOper` (0 = operatia nu e in `TZ_Oper`). De rulat pe `AVACONT_SURSA` si propagat prin sincronizarea schemei.
- **Citire** (`PYTHON/routes/avacont/introducere_date/operatii/plati.py`, `PYTHON/routes/efactura/primite_contari.py`): un singur join `o.IdOper = e.IdOperatie` pentru ecranul «Contari» (`existente`) si pentru restul de plata (`evidentieri`); fara ghicire. O legatura fara operatie apare cu textul «Legatura facturii nu are operatie contabila in TZ_Oper» (camp nou `Gasita` in `EFacturaContariExistenta`).
- **«Preia din Access»** (`PYTHON/routes/efactura/primite_access.py`): scrie numarul Access in `IdOperatieAccess`; `IdOperatie` se pune imediat daca operatia e deja in `TZ_Oper`, altfel 0.
- **`EfImporter`** (`src/KBot.Migrator/EFactura/`): poate scrie pe conexiunea si tranzactia primite de la AVC.Migrator (`EfImportOptions.ExternalConnection` / `ExternalTransaction`); `EFT_O`: legaturile venite din Access se sterg si se citesc din nou la fiecare rulare, apoi `IdOperatie` se completeaza din `TZ_Oper` (numar Access + unitate); cele facute in K-BOT raman.
- **AVC.Migrator**: pasul nou «E-Factura» (`AvcEFacturaStep`, ultimul din catalog; bifat singur = «doar e-facturi»); fisierul `ef_<an>.accdb` din `<dosarul lui cale.accdb>\EFACTURA\` sau din `EfReceivedFile` din `avc-migrator-settings.json`; pasul «Operatii» pune `IdOperatie` pentru toate legaturile cu numar Access, la fiecare rulare, in orice ordine fata de importul facturilor.
- **KBot.Migrator**: fila «E-Factura» scoasa (`MigratorForm.EFactura.vb` sters, designerul curatat); `EfImporter` ramane ca biblioteca.
- **KBotAdmin** (ME01-29): comanda «Golire date contabile» goleste si `EF_PrimiteOperatii`.

## Files touched
`sql/00EF_66_primite_operatii_id_access.sql` (nou); `PYTHON/routes/avacont/introducere_date/operatii/plati.py`, `PYTHON/routes/efactura/primite_contari.py`, `primite_access.py`; `src/KBot.Domain/EFacturaContari.vb`, `src/KBot.EFactura/Views/FacturiCumparare/FacturiCumparareContarePage.vb`; `src/KBot.Migrator/EFactura/EfImporter.vb`, `EfModels.vb`, `MigratorForm.vb`, `MigratorForm.Designer.vb`; `src/AVC.Migrator/` (`AvcEFacturaStep.vb` nou, `AvcCatalog.vb`, `AvcContext.vb`, `AvcSettings.vb`, `AvcStep.vb`, `AvcOperatiiStep.vb`, `Views/AvcMigratorForm.vb`, `README.md`).

## Test results
`dotnet build -c Release` pe AVC.Migrator, KBot.Migrator, KBot.App: 0 erori. `py_compile` pe fisierele Python. Nimic rulat pe date.

## Unverified / deferred
- Pasul «E-Factura», golirea si migrarea de la zero nu au rulat; cele trei facturi din probe (PHBTR.0090710, PHBTR.0090711, AVA_1017) de redeschis dupa migrare.
- Fisierul real `ef_2026.accdb` trebuie sa fie la calea derivata (sau in setari); `EF_2025.accdb` din repo e demo.
- Sincronizarea repetata (actualizari/stergeri in Access) nu se face: fluxul e «golire, migrare de la zero».
- Ajutor: niciunul (KBot.Migrator si AVC.Migrator nu sunt pentru clienti).
