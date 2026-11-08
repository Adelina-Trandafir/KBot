# SLICE-04EF-01 — Plăți din Contări

## Protecția la salvarea unei plăți noi

Bifa **„Protectie contari Plati/Receptii”** din **Setări → Aplicație** controlează verificarea recepției asociate facturii în **FacturiCumparare → Contări**. Cu protecția dezactivată, lipsa recepției produce un avertisment și permite salvarea. Cu protecția activată, lipsa recepției blochează salvarea. Protecția pornește dezactivată în 2026. Adelina o poate activa manual mai devreme pentru utilizatorii aleși pentru probe.

Migratorul importă valorile istorice din Access ca atare, fără recalcularea lor și fără verificarea existenței recepției.

## Cerință pentru viitorul slice de închidere a anului

Regula de activare automată este în [stadiul portării Avacont, la „Închidere anuală”](state/KBOT_STATUS_AV.md#închidere-anuală).

## What was built (08.11.2026) — code compiled, NOT run, NOT tested

Decisions and the data model: [SLICE-AV02-02](SLICE-AV02-02-plati-op-setari.md) (DDL `AV02_02`, server rules, migrator step). The client side:

- **Page «Contări»** (`FacturiCumparareContarePage`, view `FacturiCumparareView.Contare`): the mode follows the templates held — evidence template only = `evidentiere`; both = `ambele`; the evidence template taken off by a click on it (payment template kept) = `plata`. «Salvează» no longer says "payment not registered yet": it sends `mod`, `protectie` (the tick), `dataOP` and `explicatieOP`. The column «Număr OP» is read only: it shows the PROVISIONAL number with a tooltip («Număr provizoriu...»); the message after the save names the REAL number and says when it differs from the one shown. A booked invoice shows its payments (number, note, date, explanation, value) in the payment grid.
- **Ribbon button «Șterge plata»** (`ef_contare_sterge_plata`, shell `AvacontMain`): visible while a booked invoice is only looked at, enabled for a payment whose order is not locked; the tooltip gives the reason when locked.
- **Setări › Aplicație**: the tick «Protectie contari Plati/Receptii» (`AppSettings.ContariProtectiePlati`, default off).
- **Help**: `contabil/efactura/index.md` (section «Contări») and `contabil/setari.md`, tags `04EF-01, 0000-175`.

Files: `src/KBot.Domain/EFacturaContari.vb`, `src/KBot.Api/IEFacturaApi.vb`, `ApiClient.EFactura.Contari.vb`, `src/KBot.EFactura/Views/FacturiCumparare/FacturiCumparareContarePage(.Designer).vb`, `FacturiCumparareView.Contare.vb`, `src/KBot.Common/AppSettings.vb`, `src/KBot.App/Setari/Views/SetariAplicatieView(.Designer).vb`, `src/KBot.App/Avacont/AvacontMain(.Designer).vb`.

**Build:** `dotnet build src/KBot.App/KBot.App.vbproj` succeeded, 0 warnings. **Nothing was seen on screen.**

**Not built (still open):** editing a payment other than delete-and-redo; (the pages `Operații diverse` and `Facturi și plăți` of the form `OperatiiSabloane` are built since: `SLICE-AV02-03` / `SLICE-AV02-04`); the generation of the OP document (PDF/XML export, `ContUnitate`, `bBIC`).
