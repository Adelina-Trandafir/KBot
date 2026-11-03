# SLICE-VC06-35 — NIR din eFactura: doar mesajul contabilului; «FOLOSEȘTE DENUMIREA DIN EFACTURA»

Data: 03.11.2026.

## Cerere

1. La preluarea din eFactura să nu mai apară «Factura … din e-Factura este în ciornă: alegeți produsul fiecărui rând.»; să rămână DOAR mesajul de la contabil.
2. În arborele de produse, primul element = «FOLOSEȘTE DENUMIREA DIN EFACTURA»: completează automat denumirea și UM din factură și cere CPV (dacă factura nu îl are), ca la adăugarea unui rând nou.

## Ce s-a schimbat

- `PYTHON/static/js/vercon/docs.js`, `fromEfactura`: bara de stare arată doar `Notă contabil: …` (sau nimic). Au dispărut și avertismentele «Furnizorul nu este în lista de parteneri» și «conturi propuse în altă gestiune» (cerere: DOAR mesajul contabilului); `intake.apply` încă întoarce lista conturilor nebifate, neafișată acum.
- `docs.js`: coloana Produs a rândurilor din factură are primul nod `FOLOSEȘTE DENUMIREA DIN EFACTURA` (`EF_NAME_NODE`). Alegerea lui creează produs nou (`selectProduct` cu `newName`, `um`, `cpv`) și rulează imediat `finishProduct` (alegere cont dacă sunt mai multe, clasificare MF dacă e MF, CPV), apoi TVA/valori implicite; focusul trece pe Cantitate. Dacă linia facturii nu are UM, fluxul normal de produs nou cere UM (focus pe UM). Un produs existent cu aceeași denumire se refolosește (fără întrebări).
- `receipt-product-entry.js`: `selectProduct` primește `um` și `cpv`; `finishProduct` nu mai deschide alegerea CPV dacă produsul are deja unul din factură. Fluxul vechi (CPV null) neschimbat.
- `efactura-intake.js`: liniile păstrează `ef_um` și `ef_cpv`.
- `PYTHON/routes/vercon/efactura_queue.py`, `detail`: fiecare linie întoarce `cpv` = codul complet din nomenclatorul `AVACONT_COMUN.CPV` care începe cu cele 8 cifre din factură; `null` dacă factura nu are CPV sau nu există în nomenclator (atunci se cere alegerea).
- Ajutor: `PYTHON/static/help/vercon.json`, secțiunea `nir-efactura` (tag VC06-34, VC06-35).

## Teste

Testat de Adelina în aplicație: merge. Fișierele pentru server sunt înregistrate (SLICE-VC06-35). Niciun test automat.

## De știut

- UM se ia exact cum o scrie linia facturii (cod UBL ca `H87`, dacă așa îl salvează importul eFactura); nu există mapare pe text.
