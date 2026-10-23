# SLICE-VC06-17 — eFactura: ComboBox custom și încărcare automată — 23.10.2026

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

Selectorul nativ select a fost înlocuit cu Combobox-ul comun, configurat
dropDownStyle=simple, cu etichetă accesibilă și dropdown în dialog. Poate
afișa toate cele 500 de opțiuni oferite de API. Cheile de sortare sunt
explicit furnizorul, data și numărul facturii ca text, independent de ID.

Eliminat butonul «Deschide factura». După încărcarea listei se selectează și
se citește automat prima factură potrivită. API-ul listei acceptă filtrul
opțional partner, rezolvat la CUI-ul normalizat al partenerului NIR; se
păstrează filtrul de dată. Numărul nu este criteriu de asociere. Facturile
preluate integral prin fluxul vechi, care nu pot fi convertite în alocări,
nu sunt oferite pentru acest dialog.

Selectarea altei opțiuni în ComboBox încarcă automat pozițiile. Se confirmă
celula în editare și se protejează ciorna nesalvată; dacă utilizatorul
renunță la schimbare sau editul este invalid, ComboBox revine la factura
curentă. În timpul scrierii ori al unui răspuns neconfirmat selectorul
rămâne blocat. O citire eșuată se poate relua prin reselectarea facturii.
Controlul și dropdownul sunt distruse la închiderea dialogului.

Butoanele sunt acum «🔗 Asociaza», apoi «💡 Propune», cu descrierile lungi
în title. Bara conține numai ComboBox-ul și aceste două acțiuni.

Fișiere: PYTHON/static/js/vercon/efactura-association.js;
PYTHON/routes/vercon/efactura_queue.py;
PYTHON/static/css/vercon-efactura.css; PYTHON/static/help/vercon.json;
implementare_vercon_v2.md; acest jurnal.

Fără DDL nou sau execuție SQL. Publicare: Python/JS/CSS/help împreună,
repornirea serviciului web și reîncărcarea paginii.
