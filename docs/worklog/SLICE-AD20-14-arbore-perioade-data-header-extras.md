# SLICE-AD20-14 — Arborele perioadelor după data din antetul extrasului

Data: 05.11.2026.

## Cerere și implementare

În **Asociere extrase cu copii**, arborele LUNA / ANUL grupează încasările după `FX_Extrase_F.DataExtras`, provenită din atributul XML `Data_extras`, aceeași sursă afișată în DGV prin AD20-13.

`available_periods` citește anii/lunile prin legăturile existente operațiune → cont/antet → fișier, păstrând selecția încasărilor pozitive fără debit și ordinea cronologică descrescătoare. `available` aplică filtrul lunii și intervalele opționale de date asupra aceleiași date de antet. Limita finală a intervalului este inclusivă pentru întreaga zi. Manualul și previzualizarea automată folosesc același furnizor de încasări, deci aceeași filtrare.

Exemplul `Data_extras="04-Nov-2026"` aparține ramurii **Noiembrie 2026**, indiferent de DataBanca sau DataDoc ale operațiunii. Operațiunile fără dată de antet legată nu generează ramuri cu o dată substituită. Contractul arborelui și cheile an/lună rămân aceleași, fără modificarea controlului comun sau a JavaScript-ului arborelui.

Nu se modifică importul, data documentelor salvate, numerotarea pe anul calendaristic în curs, sumele, evidența asocierilor sau schema MariaDB. AD20-13 rămâne istoricul schimbării coloanelor; această intervenție adaugă gruparea/filtrarea după antet.

## Fișiere proprii și motivul înregistrării

Motivul înregistrării: «Gruparea arborelui LUNA/ANUL și filtrarea încasărilor din asociere după Data_extras din antetul XML, în acord cu data afișată în DGV.»

- `PYTHON/routes/adechit/bank_associations.py`: perioadele arborelui și filtrele după data antetului.
- `PYTHON/static/help/adechit.json`: explicarea grupării și filtrării după Data_extras.
- `ADECHIT/UTILIZARE_WEB.md`: ghidul actualizat cu referința AD20-14.
- `ADECHIT_STATUS.md`: starea curentă în index, păstrând validarea AD20-01–13 separat.
- `docs/worklog/state/ADECHIT_STATUS_ADE20-ADE29.md`: registrul detaliat și următoarea subfelie liberă.
- `docs/worklog/SLICE-AD20-14-arbore-perioade-data-header-extras.md`: cerere, modificare, fișiere și limite.

## Stare și limite

**VALIDAT DE ADELINA, ÎNREGISTRAT PENTRU SERVER.** La 05.11.2026, Adelina a confirmat explicit «validat. inregistreaza-l». Toate cele șase fișiere proprii enumerate mai sus, inclusiv ajutorul și documentația actualizată cu validarea, sunt înregistrate pentru SLICE-AD20-14 prin `tools/Register-DeploymentTask.ps1`. Agentul nu a rulat teste, compilare, verificări automate sau vizuale, browser, HTTP, SQL, SSH, publicare ori Git.

Scenarii consemnate anterior pentru operator: antet și operațiune în luni/ani diferiți, ramura corectă în arbore, lista manuală și previzualizarea automată filtrate după antet, ordinea anilor/lunilor și interval cu zi finală inclusivă. Confirmarea validării nu detaliază acoperirea fiecărui scenariu. Fără SQL de aplicat sau capturi noi. Captura 13 din ajutor rămâne de refăcut cu blur puternic.
