# K-BOT — STATUS, slice family 03EF (e-invoices: ribbon of the received invoices and the «Contari» view)

Family 03EF (operator, 08.11.2026): numbering `03EF-01`, `03EF-02`, ... Worklogs `SLICE-03EF-nn-*.md`. Continues 01EF (the «Contari» page) and 00EF (the received invoices window): the ribbon takes over what the page carried, and the page is cleaned.

## Current focus
- **03EF-13 built (08.11.2026):** booked invoices shown in «Contari», default tree by supplier without operations, CPV / Um tooltips. Server files by hand-deploy only.
- **03EF-08..11 built (08.11.2026), after the operator tried 03EF-01..07:** «Exportă...», tick boxes on all invoices with one supplier at a time, «Salvează» only with every cell filled, separator fix in the ribbon. Nothing seen on screen.
- **03EF-01..07 built (08.11.2026), builds clean, nothing run, nothing seen on screen, no server file.** Ribbon: no period boxes, the synchronisation button opens the popup of «Actualizează»; «Contari» buttons in the view group (Adaugă | Salvează | Renunță | Salvează ca... | Ascunde...); the foot and the length section left the page; «Lungime» progress column; combo as wide as the list below.

## Registry

| Slice | Name | Status | Worklog | Notes |
|------:|------|--------|---------|-------|
| 03EF-01 | Ribbon «Sincronizează facturi primite»: the box «Între 1 și 60 de zile» and the combo «Perioada de adus» out; the click opens the popup of «Actualizează» | **INCHIS (08.11.2026)**: rulat si testat de operator | `SLICE-03EF-01-ribbon-sincronizare-fara-casete-cu-popup.md` | Help 0000-171. |
| 03EF-02 | «Contari»: «Adaugă operații contabile» \| «Salvează ca...» (ZIP / XML) \| «Ascunde...» in the ribbon | **INCHIS (08.11.2026)**: rulat si testat de operator | `SLICE-03EF-02-contari-butoane-in-ribbon.md` | **«Ascunde...» = arhiva (de confirmat).** Help 0000-171. |
| 03EF-03 | While booking, «Adaugă» is replaced by «Salvează» and «Renunță» | **INCHIS (08.11.2026)**: rulat si testat de operator | `SLICE-03EF-03-adauga-devine-salveaza-si-renunta.md` | Help 0000-171. |
| 03EF-04 | The foot of the page (buttons + HTML info) out; the info is the tooltip of «Salvează» | **INCHIS (08.11.2026)**: rulat si testat de operator | `SLICE-03EF-04-subsol-pagina-contari-scos.md` | Hint in the classification box when nothing is ticked. Help 0000-171. |
| 03EF-05 | The section with the length of the explanation out | **INCHIS (08.11.2026)**: rulat si testat de operator | `SLICE-03EF-05-sectiunea-lungimii-scoasa.md` | |
| 03EF-06 | «Lungime» progress column after «Explicație O.P.», live | **INCHIS (08.11.2026)**: rulat si testat de operator | `SLICE-03EF-06-coloana-lungime-in-plati.md` | One colour (the old bar changed colour). Help 0000-171. |
| 03EF-07 | The classification combo as wide as «Documente evidențiere» | **INCHIS (08.11.2026)**: rulat si testat de operator | `SLICE-03EF-07-combo-clasificatie-latimea-listei.md` | Layout fix, not in the help. |
| 03EF-08 | «Salvează ca...» renamed «Exportă...» | **INCHIS (08.11.2026)**: rulat si testat de operator | `SLICE-03EF-08-salveaza-ca-devine-exporta.md` | Help 0000-172. |
| 03EF-09 | While booking, every invoice has a tick box; only one supplier stays ticked (another supplier's tick replaces the old ones) | **INCHIS (08.11.2026)**: rulat si testat de operator | `SLICE-03EF-09-bife-la-toti-furnizorii-un-singur-furnizor.md` | Help 0000-172. |
| 03EF-10 | «Salvează» needs every cell of the grids filled | **INCHIS (08.11.2026)**: rulat si testat de operator | `SLICE-03EF-10-salvare-doar-cu-toate-celulele-completate.md` | NC and Număr OP must be typed. Help 0000-172. |
| 03EF-11 | Ribbon: the separator of the right group moved with the group (stray line over the left buttons) | **INCHIS (08.11.2026)**: rulat si testat de operator | `SLICE-03EF-11-separator-grup-dreapta-mutat-cu-grupul.md` | Bug in `KBotRibbon.ShiftGroup`. |
| 03EF-12 | Tree of the received invoices: month > day (dd.mm.yyyy) > invoice «Furnizor - Număr ~~~ Valoare» | **INCHIS (08.11.2026)**: rulat si testat de operator | `SLICE-03EF-12-arbore-pe-luni-zile-si-facturi.md` | The view of an angajament and the view «Recepții» keep their shape (to confirm). Help 0000-173. |

| 03EF-13 | «Contari» shows an invoice that already has operations (read only); the tree starts by supplier without operations; CPV / Um tooltips in «Continut» | **INCHIS (08.11.2026)**: rezolvat, confirmat de operator | `SLICE-03EF-13-contari-facturi-contate-arbore-implicit-sfaturi.md` | New route `contari/existente`, `explicatii` in the invoice detail. Payment rows depend on migrated data. Help 0000-174. |
| 03EF-14 | Tree options: «Afisare facturi cu operatii» next to «fara operatii», exclusive | **INCHIS (08.11.2026)**: rezolvat, confirmat de operator | `SLICE-03EF-14-filtre-cu-fara-operatii-exclusive.md` | Click on the ticked one unticks it (to confirm). Help tag 03EF-14 (with 0000-174). |

## Open threads
- «Ascunde...» is the archive of 01EF-03 by assumption; confirm with the operator.
- Nothing seen on screen: the group order and the separator in the view group, the placement of the two popups under the ribbon buttons, the hint in the disabled combo.

**Next free number: 03EF-15**
