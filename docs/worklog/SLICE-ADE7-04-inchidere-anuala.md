# ADE7-04 — Închidere anuală august

09.10.2026. Implementare locală, nepublicată.

## Ce s-a schimbat și de ce

Închiderea lui august necesită acum planul anual. Conform cererii utilizatorului,
fereastra există exclusiv pe desktop: TreeView custom în stânga, DataGrid custom
cu copiii în dreapta și DataGrid custom cu educatorii dedesubt. Prima coloană
selectează copiii, Ctrl/Shift permit selecție multiplă, iar drag-and-drop pe
arbore sau comboboxul custom mută selecția în grupa destinație. Plecat este o
coloană separată. Se pot păstra/muta toți copiii sau se pot muta individual.

DataGrid primește opțional multiSelect, selectionCheckbox și dragRows;
utilizările existente păstrează selecția simplă. Intervalele folosesc ordinea
filtrată/sortată a grilei, inclusiv rândurile virtualizate.

Grupele noi se creează numai la confirmarea planului și necesită nume unic,
cel puțin un educator și cel puțin un copil care rămâne. Copiii pleacă la
31 august, iar mutările și noii educatori se aplică la 1 septembrie.
Situația din august se salvează înaintea modificării cataloagelor. Tot fluxul
rulează în tranzacția și blocarea existente; eșecul anulează toate scrierile.
Idempotency-Key păstrează aceeași cheie la reîncercarea aceluiași plan.
Serverul verifică versiunile copiilor, grupelor, lunii și istoricului educatorilor,
precum și drepturile close/catalog/transfer. Nu s-au schimbat formulele financiare.

## Fișiere

- `PYTHON/routes/adechit/annual.py`, `service.py`, `__init__.py`.
- `PYTHON/static/js/adechit/annual.js`, `app.js`.
- `PYTHON/static/js/dgv/datagrid.js`, `selection.js`.
- `PYTHON/static/adechit.html`, `PYTHON/static/css/adechit.css`.
- `PYTHON/tests/test_adechit_annual.py`, `dgv_selection.test.mjs`, stubul nou în `adechit_lazy.test.mjs`.
- Ajutorul de utilizare și registrul ADE7 actualizate în aceeași intervenție.

Fișierele deja modificate în workspace au fost păstrate; nu s-a făcut commit/push.

## Verificări efective

25 teste API trecute (18 existente și 7 pentru august), cu SQLite de probă.
Acoperă plan obligatoriu, mutare individuală/lot, grupă nouă, plecare cu sold,
snapshot cu numele vechi, perioade educatori, idempotency, versiuni și rollback
după eșecul creării lunii următoare. 5 teste JS trecute (3 lazy și 2 selecție).
Prima încercare pytest a eșuat la crearea directorului temporar din sandbox;
rularea cu basetemp în artifacts a trecut.

Aceste probe au fost pornite înainte de citirea regulii care lasă testarea
utilizatorului. Nu s-au mai rulat teste după identificarea ei. Ultimele ajustări
de mesaje, selecție programatică și ordonare nu sunt retestate.
O tentativă de deschidere a preview-ului 5057 în browserul integrat a primit
ERR_CONNECTION_TIMED_OUT; nu s-a realizat probă vizuală. Serverul local de probă
a fost oprit. Nu există rezultate de test UI/server revendicate.

## Neverificat sau amânat

- Aspectul și interacțiunea drag-and-drop în browser, navigarea repetată și tema.
- Validarea reală MariaDB și publicarea prin AvacontPush, care aparțin utilizatorului.
- AD_03 și catalogul importat sunt prerequisite existente; nu se adaugă DDL nou.
- Redeschiderea lunii păstrează comportamentul existent: anulează situațiile și
  luna următoare, dar nu revine automat asupra modificărilor cataloagelor anuale.
  Semantica unei anulări complete a reorganizării anuale rămâne de stabilit.
- Dacă se schimbă educatorii unei grupe cu perioade programate după 1 septembrie,
  închiderea refuză operația și cere corectarea perioadelor în catalog; nu șterge
  tacit programările ulterioare.
