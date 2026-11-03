# SLICE-VC06-37 — Comisie implicită; nota contabilului în fereastra CPV; clic pe altă celulă cu lista deschisă

Data: 03.11.2026. Testat de Adelina în aplicație: merge.

## Cerere
1. Dacă există o singură comisie, să fie aleasă implicit la adăugarea unui NIR.
2. După ce scrii un produs într-un NIR și dai clic în Cantitate (sau în denumirea din factură), apărea «Rând incomplet» (Cantitate, Preț, TVA % lipsă), și la NIR-uri normale.
3. Fereastra CPV a unui produs dintr-un NIR din eFactura: casetele de căutare mai mici, una lângă alta, iar în dreapta nota contabilului (copiabilă selectiv, fără autoselectare).

## Ce s-a schimbat
- `PYTHON/static/js/vercon/docs.js`, `add()`: `header.IdComisie` = singura comisie din `lookups.commissions` (NIR).
- Nota în CPV: `docs.js` → `receipt-product-entry.js` (`note`) → `cpv-picker.js` + `PYTHON/static/css/vercon-cpv.css`. Cu `note` ≠ `null` (NIR din eFactura) căutările au coloane 1:1:2 (25% / 25% / 50%), iar în dreapta e «Notă contabil» într-un textarea doar-citire (text de rezervă când contabilul n-a scris nimic). Se aplică și ferestrei de clasificare MF. `efactura-intake.js` păstrează `draft.efactura_note`.
- Punctul 2, `PYTHON/static/js/dgv/editing.js`, `_onFocusOut`: cât timp un editor listă/arbore are lista deschisă, un strat transparent prinde clicurile; clicul pe altă celulă a aceleiași grile nimerește stratul, focusul trece pe pagină și grila judeca rândul ca și cum operatorul ar fi plecat. Acum celula se salvează, dar rândul se judecă la trecerea reală pe alt rând sau la salvarea documentului.
- Ajutor: `PYTHON/static/help/vercon.json`, secțiunea `nir-efactura` (tag VC06-37).

## Capcane / neacoperit
- Cu arborele deschis, primul clic pe Cantitate închide doar lista (o cere strat); celula cere al doilea clic, ca la orice listă derulantă.
- Cauza punctului 2 nu se vedea în grila izolată, ci doar în pagina reală (jurnal din consolă); o grilă de probă nu o reproduce.

## Teste
Niciun test automat. Verificat de Adelina în aplicație (publicare debugging). Fereastra CPV verificată și pe o pagină de probă (coloane 233/233/467 px pe 960 px). Fișierele pentru server sunt înregistrate (SLICE-VC06-37).
