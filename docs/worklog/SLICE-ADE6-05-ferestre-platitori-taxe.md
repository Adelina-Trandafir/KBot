# SLICE-ADE6-05 — ferestre Plătitori și Taxe

## Ce s-a schimbat și de ce

Din antet se deschid două ferestre modale. Fundalul este inert, Escape și clicul pe
fundal nu închid fereastra. Editarea folosește DataGrid comun. Plătitori afișează copiii
și persoanele asociate copilului selectat; se pot edita copiii și edita/adăuga persoane.
Taxe permite editare și rânduri noi. Câmpurile Da/Nu sunt editabile ca text validat.
«Salvează și închide» confirmă inclusiv celula încă deschisă și trimite toate modificările
printr-o comandă atomică, autorizată catalog, cu versiuni și idempotency key.
La eroare fereastra și editările rămân; reîncercarea aceluiași corp refolosește cheia.
Fără modificări, butonul închide fără scriere. Dacă încărcarea eșuează, se permite
închiderea ferestrei goale. Navigarea cu editări nesalvate cere confirmarea browserului.
Activarea taxei dezactivează vechea taxă prin serviciul existent; nu rescrie IDV istoric.
Ajutorul din pagină a fost actualizat cu referința ADE6-05.

## Fișiere atinse

- PYTHON/static/adechit.html
- PYTHON/static/css/adechit.css
- PYTHON/static/js/adechit/app.js și catalogs.js (nou)
- PYTHON/routes/adechit/__init__.py și service.py
- PYTHON/tests/test_adechit.py
- ADECHIT_STATUS.md și docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md

## Verificări efective

- API: 11 teste trecute (pytest, mediul virtual al proiectului), inclusiv rollback
  integral la conflict/validare, idempotency, drepturi și păstrarea taxei istorice.
- Sintaxa JS verificată cu Node; git diff --check pentru fișierele lucrării.
- Browser Edge izolat, localhost:5050: Escape/fundal păstrează fereastra, editarea nu
  scrie înainte de salvare, salvarea confirmă celula curentă, persoana nouă persistă.
- Browser: refuz HTTP 409 simulat păstrează fereastra și draftul; reîncercarea salvează.
  Tema întunecată și ferestrele pe telefon (390 px) au fost inspectate din capturi.
- Preview-ul Python pornit dimineață a fost repornit pentru a încărca ruta nouă;
  baza locală a fost păstrată. Datele din proba browser sunt fictive.
- Prima rulare pytest a eșuat la crearea directorului temporar în sandbox; reluată cu
  --basetemp artifacts/ade6-05-pytest, toate testele au trecut.

## Neverificat sau amânat

Nu s-au executat SQL remote, publicare, validare MariaDB sau probe Access.
Nu se schimbă schema bazei. Transferurile/plecările și configurarea unității nu fac parte
din această lucrare. Browserul poate închide un tab după confirmare; fereastra modală
nu poate garanta păstrarea datelor după închiderea browserului.
Modificările sunt locale, necomise; fișierele ADE aveau deja modificări ample necomise.

## Predare pentru server

Publicați cele șase fișiere de implementare enumerate mai sus prin AvacontPush,
împreună cu lucrările ADE anterioare necesare. Nu există script SQL nou pentru ADE6-05.
După restartul API, verificați deschiderea ambelor ferestre, editarea/adăugarea unui
plătitor și a unei taxe, salvarea, reîncărcarea și păstrarea editărilor la conflict.
