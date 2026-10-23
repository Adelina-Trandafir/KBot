# NIR: asociere în popup și grupare inițială pe furnizori — 23.10.2026

«Asociază eFactura…» este disponibilă în meniul ⋮ al fiecărui NIR de
cumpărare din arbore; butonul din footer a fost eliminat. Comanda deschide
întâi NIR-ul asupra căruia s-a invocat meniul, apoi asocierea acelui document.
Rămâne dezactivată fără drept de scriere, pentru documente anulate, în
editare sau cu operații neconfirmate. Stocul inițial nu oferă asociere.

Arborele NIR pornește cu «Grupeaza pe luni» debifat și încarcă rădăcinile
furnizorilor în ordine alfabetică românească. Parametrul initialByDate al
documentTreeOrder primește false numai pentru NIR. Bifa poate fi activată
ulterior; alegerea curentă se păstrează la reîncărcările arborelui în același
formular. Celelalte familii își păstrează configurația inițială.

Fișiere: PYTHON/static/js/vercon/{docs,document-tree-order}.js;
PYTHON/static/help/vercon.json; implementare_vercon_v2.md;
docs/worklog/VERCON-2026-10-23-efactura-asociere-nir-partiala.md;
acest jurnal.

Stare: SCRIS LOCAL, NEPUBLICAT. Numai analiză de design pe surse. Fără
teste, verificări automate/de sintaxă, browser sau MariaDB, conform
instrucțiunilor proiectului. Nu necesită SQL nou. Publicare: fișierele JS
și helpul împreună, apoi reîncărcarea paginii VERCON.
