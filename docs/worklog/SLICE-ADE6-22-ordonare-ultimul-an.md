# SLICE-ADE6-22 — Ordonare după denumire și ultimul an deschis

09.10.2026. SCRIS LOCAL, fără teste automate sau vizuale, conform preferinței
utilizatorului din REGULI_PROIECT.md.

## Ce s-a schimbat și de ce

Ordonarea inițială a tuturor listelor cu nume/denumire este alfabetică în română:
grupele din arbore și combobox, copiii din Prezență și Plătitori, persoanele
asociate, educatorii și taxele după Explicație. Grilele folosesc sortBy din
DataGrid comun, păstrând ordonarea și după editare sau adăugare. Documentele
se afișează după Explicație, cu rândul nou la sfârșit. Comboboxul lunilor este
ordonat după denumire; arborele LUNA / ANUL rămâne cronologic descrescător.

La încărcarea paginii se cer numai lunile celui mai recent an și se deschide
ramura lui. Ceilalți ani rămân închiși și se încarcă lazy. Deschiderea anului
nu selectează luna sau grupa și nu descarcă date despre copii. Restul fluxului
lazy din ADE6-21 se păstrează.

## Fișiere atinse

- PYTHON/static/js/adechit/app.js, payers.js, catalogs.js.
- PYTHON/static/adechit.html: ajutor actualizat.
- PYTHON/tests/adechit_lazy.test.mjs: adaptarea simulării DataGrid și a
  așteptărilor existente la ultimul an deschis. Testele nu au fost rulate.
- ADECHIT/UTILIZARE_WEB.md, ADECHIT_STATUS.md și registrul ADE0–ADE9.

## Verificări și predare

Citire/revizie a codului și contractului sortBy; fără execuție de teste,
server sau browser. Probele ADE6-21 sunt istorice și nu validează această
subfelie. Utilizatorul publică cele trei module JS și adechit.html prin
AvacontPush și verifică pe PC/mobil ordonarea și anul deschis.

Fără schimbare API, DDL, commit, push sau restart executat din chat.
Următoarea subfelie liberă: ADE6-23.
