# SLICE-VC14-05 — Număr de factură dublu (avertizare) și asocierea eFacturilor cu facturi storno

12.11.2026 — La cererea Adelinei, două lucruri în VERCON.

## 1. Avertizare la număr de factură dublu (nu blochează)

- **Regula:** același furnizor (cod fiscal normalizat, nu denumirea) + același număr de factură, ignorând majusculele și spațiile; data facturii nu se compară. Se caută în aceeași gestiune, printre NIR și FCS neanulate. Un NIR cu factură comună mai multor NIR (`FacturaId`) nu se avertizează (cazul normal).
- **La introducere (NIR și FCS):** pagina întreabă serverul (`GET /api/vercon/doc/<familie>/invoice-twins`) când se schimbă **Număr factură** sau **Furnizor**, și încă o dată înainte de salvare (pentru valori care n-au fost tastate, ex. din eFactură). Răspunsul apare prin sistemul de mesaje (`messages.js`, nivel «Atenție»); salvarea nu e oprită. O aceeași valoare avertizează o singură dată pe ciornă. La FCS furnizorul se ia din NIR-ul stornat.
- **La asocierea cu eFactura:** `detail()` întoarce `twins` (alte documente ale furnizorului cu numărul eFacturii, fără NIR-ul curent și fără cele deja legate de aceeași eFactură); fereastra arată avertizarea, iar asocierea fără fereastră (automată) o arată ca mesaj global. Nu blochează.

## 2. eFacturi asociate și cu facturi storno (FCS)

Ambele forme: **notă de credit** (Semn −1, cantități pozitive) și **factură normală cu cantitate negativă** (furnizorii care nu fac notă de credit).

- Un rând de eFactură **ia marfa înapoi** când e pe o notă de credit sau are cantitate negativă (ambele împreună = rând normal). Rândurile storno se asociază liniilor unei FCS (`VC_IntrariStorno`), celelalte liniilor unui NIR; fereastra unei FCS vede numai rândurile storno, a unui NIR numai pe celelalte; o încercare greșită prin API dă `EF_SIGN`.
- Bugetele lucrează pe **valoarea absolută** a cantității rândului (`abs(Cant)`), la asociere, la `progress`, la `progress_states` (factura e «preluată» când fiecare rând, pozitiv sau negativ, e folosit complet).
- `shared.allocations`: legătura e valabilă și când rândul vizat e al unei FCS (alt tabel de linii).
- O FCS legată de o eFactură se blochează la modificare ca un NIR; meniul ei ⋮ are Asociază / Înlocuiește / Elimină / Afișează eFactura (document `storno` primește `efactura`, `has_efactura`, `edit_locked`).
- **Trimiterea:** nota de credit se trimite acum din K-BOT ca orice factură (fosta decizie D3 înlocuită); lista «Adaugare NIR» a Administratorului nu o oferă (nu pornește un NIR din ea), lista de asociere da; elementele listei au `credit_note`.
- Contabilul vede și numerele FCS-urilor pe factură (cu `Storno`, numărul fără semn).

## Fișiere

- `PYTHON/routes/vercon/invoice_twins.py` (nou), `doc_routes.py`
- `PYTHON/routes/vercon/efactura_association.py`, `efactura_queue.py`, `efactura_receipts.py`, `documents.py`
- `PYTHON/utils/vercon_efactura.py`
- `PYTHON/routes/efactura/primite_vercon.py`
- `PYTHON/static/js/vercon/invoice-twins.js` (nou), `docs.js`, `efactura-association.js`, `efactura-actions.js`, `efactura-auto-association.js`
- `PYTHON/static/help/vercon.json` (ajutor VERCON), `src/KBot.App/HelpContent/contabil/efactura/index.md` (ajutor K-BOT: notele de credit se trimit; tag `VC14-05`)

## Verificat / neverificat

- Verificat: `py_compile` curat pe fișierele Python atinse, importul modulelor merge, `node --check` curat pe JS, JSON-ul de ajutor se parsează.
- **Nerulat:** nimic pe o bază reală și nimic văzut pe ecran; nu s-a deschis fereastra de asociere cu o FCS și nu s-a trimis o notă de credit. Nepublicat.
- De urmărit la prima rulare: (a) interogarea `VALID` din `allocations` (acum cu două `LEFT JOIN`-uri pe liniile NIR / FCS); (b) rândurile eFacturii cu cantitate 0 nu pot fi asociate; (c) un NIR și o FCS ale aceleiași facturi: facturile cu rânduri mixte ajung «preluate» doar când ambele părți sunt asociate; (d) ajutorul K-BOT: `Check-Help.ps1` raportează erori vechi în alte tutoriale, nu am verificat dacă acceptă tag-ul `VC14-05`.
- Deschis: sub-slice-ul de ajutor `0000-NN` nu a fost numerotat (tag-ul din ajutor poartă `VC14-05`).
