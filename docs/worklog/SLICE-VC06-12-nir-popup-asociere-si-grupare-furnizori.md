# SLICE-VC06-12 — NIR: asociere în popup și grupare inițială pe furnizori — 23.10.2026

«Asociază eFactura…» în meniul ⋮ al fiecărui NIR de cumpărare, fără buton în footer: deschide NIR-ul vizat, apoi asocierea lui. Dezactivată fără drept de scriere, pe documente anulate, în editare ori cu operații neconfirmate. Fără asociere la stoc inițial.

NIR pornește cu «Grupeaza pe luni» debifat și rădăcini de furnizori ordonate alfabetic românește: `initialByDate=false` în `documentTreeOrder` numai pentru NIR. Bifa se poate activa; alegerea persistă la reîncărcare în același formular. Celelalte familii păstrează configurația inițială.

Fișiere: PYTHON/static/js/vercon/{docs,document-tree-order}.js; PYTHON/static/help/vercon.json; implementare_vercon_v2.md; docs/worklog/VERCON-2026-10-23-efactura-asociere-nir-partiala.md; acest jurnal.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).
