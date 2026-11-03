# SLICE-VC06-38 — Fereastra «Produs din eFactura» (NIR din eFactura)

Data: 03.11.2026. Testat de Adelina în aplicație: merge.

## Cerere
La alegerea «FOLOSEȘTE DENUMIREA DIN EFACTURA» pe o linie de NIR din eFactura să apară o fereastră mică: denumirea din factură editabilă, pe mai multe rânduri; UM din factură editabilă; CPV din factură needitabil dacă există, altfel tastat sau ales ca în Produse; mesajele contabilului (pe mai multe rânduri). Două butoane standard: «Renunță» stânga, «Salvează» dreapta. La salvare se revine în NIR cu datele pe rândul respectiv. Elementul trebuie să fie mereu primul în listă.

## Ce s-a schimbat
- Modul nou `PYTHON/static/js/vercon/efactura-line-dialog.js` (`efacturaLineDialog`): fereastră `ModalFrame` (ca fereastra CPV, deci selectorul CPV se deschide peste ea), cu clasele `.vc-efi` pentru subsol/butoane (`statusBar`: Renunță start, Salvează end, `is-primary`). Răspunde `{ name, um, cpv }` sau `null` (Renunță / Esc / ×).
  - Denumire: textarea 5 rânduri, max 255. UM: max 50, obligatorie. Notă contabil: textarea doar-citire (`draft.efactura_note`).
  - CPV din factură: câmp doar-citire. Fără CPV în factură: câmp text + «Alege…» (deschide `mountCpvPicker`, `onPick` completează câmpul). Codul tastat se verifică la «Salvează» cu `GET /api/vercon/cpv/<cod>`; gol → `confirmBox` «Produsul NU va avea CPV».
- `docs.js`: `onCellSave` deschide fereastra înainte de orice modificare a liniei; la «Renunță» întoarce linia neschimbată. La «Salvează» folosește `{ newName, um, cpv }` din fereastră. Marcaj `line._cpvDeclined` (CPV gol confirmat). Elementul din listă are `pinned: true`.
- `receipt-product-entry.js`, `finishProduct`: cu `_cpvDeclined` nu mai deschide fereastra CPV a doua oară.
- Câmp nou pe nodurile arborelor, `pinned: true` (frunză de nivel 1 care se poate alege, mereu prima, vizibilă și la căutare): `combotree.js` (`canSelectComboItem`), `treeview-render.js` (ordinea), `treeview-search.js` (`filterTreeForSearch`). Fără el elementul, de nivel 1, nu putea fi ales, era sortat ultimul și dispărea la căutare.
- CSS: `PYTHON/static/css/vercon-efactura.css` (`.vc-efl*`).
- Ajutor: `PYTHON/static/help/vercon.json`, secțiunea `nir-efactura` (tag VC06-38).

## Capcane / neacoperit
- Valorile editate nu se scriu înapoi în coloana «Din factura primită» (rămâne ce a scris furnizorul).
- Dacă există deja un produs cu aceeași denumire, se folosește acela (UM/CPV din fereastră nu îl schimbă).
- `docs.js` depășește ~1000 de linii (peste 1090); logica nouă e în modul separat, dar fișierul merită împărțit.

## Teste
Niciun test automat. Verificat de Adelina în aplicație (publicare debugging). Fișierele pentru server sunt înregistrate (SLICE-VC06-38).
