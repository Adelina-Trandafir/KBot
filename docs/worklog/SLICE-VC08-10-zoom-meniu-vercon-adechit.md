# 2026-10-19 — Zoom comun în meniurile VERCON și ADECHIT

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

Butonul de zoom este montat o singură dată în bara aplicației, lângă Ajutor. Ambele aplicații folosesc același control din `PYTHON/static/js/components/menu/menu-zoom.js`, cu CSS alăturat. Paginile și ferestrele individuale nu instanțiază controale de zoom.

Sliderul acoperă 75–150%, cu valori intermediare și puncte magnetice la 90%, 100%, 110% și 125%. Procentele respective sunt și butoane de selecție directă. În timpul tragerii se actualizează indicatorul; redimensionarea se aplică la încheierea gestului. Din tastatură se pot alege toate procentele întregi, fără blocare în punctele snap.

Nivelul se aplică rădăcinii documentului și se memorează în `localStorage`, cheia comună `kbot-application-zoom`: rămâne activ la schimbarea paginilor, redeschiderea aplicației și trecerea între cele două aplicații. Evenimentul storage sincronizează filele deschise. În lipsa accesului la storage, nivelul curent funcționează în pagină.

`PYTHON/static/js/utils/layout-geometry.js` convertește coordonatele de ecran în pixeli de layout pentru meniuri, ComboBox, ComboTree, TreeView, DatePicker, ModalFrame și DataGrid. Sunt adaptate și poziționările specifice ADECHIT și cardurilor de context VERCON. Lățimile și înălțimile raportate la viewport din CSS păstrează proporțiile existente folosind variabilele controlului comun; fallback-ul păstrează comportamentul pe paginile fără zoom. Imprimarea resetează zoom-ul interfeței la 100%.

Fișiere:

- Control: `PYTHON/static/js/components/menu/menu-zoom.js` și `.css`; `PYTHON/static/js/utils/layout-geometry.js`.
- Integrare: `PYTHON/static/js/vercon/app.js`, `adechit/app.js`, `vercon/receipt-context.js`; stylesheet-urile de intrare `PYTHON/static/css/vercon.css` și `adechit.css`.
- Geometrie: modulele comune din `js/components/menu`, `combobox`, `combotree`, `treeview`, `datepicker`, `modal-frame` și `js/dgv`.
- Viewport CSS: `adechit-{annual,bank-associations,catalog-forms,catalogs, controls,export,help,layout,payment-notice-config}.css`, `vercon-{base,history,preview,rapoarte,receptie,workspace}.css`, `modal-frame.css`, `confirm-box.css`, `report-preview.css`, `dgv.css`, `combobox.css`, `combotree/combotree.css`.
- Jurnal: acest fișier și `implementare_vercon_v2.md`.

Nu s-au rulat teste, build-uri sau verificări automate/vizuale, conform AGENTS.md. Numai analiză de design a surselor. Nu s-a accesat MariaDB și nu s-au modificat scheme. Necesită publicarea fișierelor JS și CSS.
