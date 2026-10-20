# 2026-10-19 — Arbori Intrări/Ieșiri: două niveluri și actualizare lazy locală

Stare: **SCRIS LOCAL**, nerulat și nepublicat.

Sub arborii documentelor apare bifa «Grupeaza pe luni», inclusiv în
Intrări și Ieșiri. Bonurile PTC folosesc același control, cu eticheta
înlocuită. Modul inițial este cel pe luni, ca la PTC.

- Bifat: L0 = Luna în română / Anul; L1 = Data / NrDoc, valoarea în dreapta.
- Debifat: L0 = Furnizor / Gestionar; L1 = Data / NrDoc, valoarea în dreapta.

Nu se mai adaugă nivel de lună sub furnizor sau gestionar. Documentele
se ordonează descrescător după dată/număr, lunile descrescător, iar
furnizorii/gestionarii alfabetic în română. Numărul NIR include subnumărul.
Eticheta se trunchiază cu ellipsis, iar tooltip-ul păstrează textul complet.
Marcajele pentru anulare și retur fără bon sursă rămân vizibile în etichetă.
Totalurile lunare existente ale PTC sunt păstrate.

NIR păstrează încărcarea lazy: se citesc numai rădăcinile lunilor sau
furnizorilor, apoi documentele rădăcinii deschise. Endpoint-ul existent
acceptă filtrarea după furnizor sau lună; fiecare SELECT rămâne limitat
la subunitatea, gestiunea și exercițiul active. Citirea combinată
furnizor+lună rămâne disponibilă pentru apelanții vechi.

Schimbarea modului păstrează documentul selectat și deschide rădăcina lui
în noul mod. Bifa este dezactivată cât timp se pregătește gruparea;
la eroare revine la valoarea anterioară. Gruparea nu se schimbă în editare.
Ramurile deschise pot coexista: deschiderea uneia nu le închide pe celelalte.

Controlul TreeView are acum randare incrementală opțională, activată
pentru arborii VERCON. Reconciliază elementele după id, păstrând obiectele
DOM ale rândurilor și subarborilor nemodificați. Expand/collapse și
înlocuirea placeholder-ului lazy actualizează conținutul schimbat; indicii
de navigare se sincronizează fără înlocuirea rândurilor vecine.
`setData(..., {preserveExpanded: true})` păstrează expansiunea înainte
de randare. Adaptorul VERCON nu mai golește containerul la refresh;
se păstrează și scroll-ul și selecția. Căutarea și starea bifelor rămân
gestionate de control. Alte aplicații păstrează randarea existentă până
la activarea explicită a opțiunii `incrementalRender`.

Fișiere:

- `PYTHON/static/js/vercon/document-tree-order.js`, `oi-tree-order.js`,
  `receipt-tree.js`, `docs.js`, `ui.js`;
- `PYTHON/static/js/components/treeview/treeview-patch.js`,
  `treeview-render.js`, `treeview-search.js`, `treeview-checkable.js`;
- `PYTHON/static/css/vercon-document-tree.css`, `vercon.css`;
- `PYTHON/routes/vercon/receipt_tree.py`, `doc_routes.py`,
  `receipt_invoice_view.py`;
- acest jurnal și `implementare_vercon_v2.md`.

Numai analiză de design a surselor. Nu s-au rulat teste, build-uri sau
verificări automate/vizuale, conform AGENTS.md. Nu s-au executat
interogări MariaDB și nu s-au modificat scheme. Necesită publicare
Python/JS/CSS și repornirea serviciului pentru modificările Python.
