# SLICE-VC08-20 — Sincronizare dată NIR/factură și inventar pe primitori

Data: 05.11.2026.

## Cerere și implementare

La schimbarea Datei NIR, Data facturii primește aceeași valoare în ciornă și în DatePicker, fără emiterea unui al doilea eveniment. Sincronizarea existentă Data facturii → Data NIR se păstrează. Numerotarea și validarea folosesc ambele date actualizate; refuzul restaurează datele, numărul/subnumărul și blocarea numerotării prin mecanismul existent. Numai familia NIR este afectată.

Data facturii comune deja legate rămâne blocată, conform regulii existente: sincronizarea Data NIR → Data facturii se aplică numai dacă data facturii este editabilă. Nu se modifică fluxul facturilor comune și nu se propagă schimbarea la alte NIR-uri.

Listele `OI_LISTA_INVENTAR` și `MF_LISTA_INVENTAR` grupează mai întâi după primitor, apoi după grupările existente ale machetei. Adaptarea se face în memorie, inclusiv pentru machetele salvate, înaintea sortării românești existente. Identitatea primitorului rămâne distinctă de eticheta sa; persoane diferite cu același nume nu se combină.

Fiecare primitor următor începe pe pagină nouă. Semnăturile configurate se adaugă după toate conturile, pozițiile și subtotalurile fiecărui primitor, inclusiv pentru ultimul. Totalul general al raportului, când există, se păstrează separat după listele semnate, fără să fie inclus în tabelul ultimului primitor. Nu se adaugă o întrerupere după ultimul primitor. Modelul comun transmite separările și semnăturile către listare/PDF și exportul Excel. Listele de magazie și inventarul total pe produse nu sunt schimbate.

## Fișiere proprii și motivul înregistrării

Motivul înregistrării: «Sincronizarea datei facturii editabile cu data NIR și listarea inventarului OI/MF cu pagină nouă și semnături pentru fiecare primitor.»

- `PYTHON/static/js/vercon/docs.js`: sincronizarea Datei NIR spre Data facturii editabilă.
- `PYTHON/routes/vercon/report_body.py`: grupare pe primitor, întreruperi de pagină, semnături individuale și total general separat.
- `PYTHON/routes/vercon/report_service.py`: aplicarea grupării înainte de sortarea și generarea raportului.
- `PYTHON/static/help/vercon.json`: comportamentul datelor NIR/factură și al inventarului pe primitori.
- `docs/worklog/SLICE-VC08-20-date-nir-inventar-primitori.md`: cerere, implementare, fișiere și limite.
- `VERCON/VERCON_STATUS.md`: stare și legătură la worklog.
- `implementare_vercon_v2.md`: intrare datată în indexul istoric.

## Stare și limite

**TESTAT DE ADELINA, ÎNREGISTRAT PENTRU SERVER.** La 05.11.2026, Adelina a confirmat explicit: «am testat. inregistreaza fisierele». Agentul a făcut numai analiză de proiectare pe surse și înregistrarea autorizată; fără teste, compilare, verificări automate sau de execuție, browser, HTTP, SQL, publicare ori Git. Rapoartele deja generate trebuie regenerate prin Listare pentru noua structură.

Toate cele șapte fișiere proprii enumerate mai sus, inclusiv ajutorul și documentația actualizată cu confirmarea testului, sunt înregistrate prin `tools/Register-DeploymentTask.ps1` pentru `SLICE-VC08-20`, conform confirmării explicite a Adelinei și AGENTS.md.
