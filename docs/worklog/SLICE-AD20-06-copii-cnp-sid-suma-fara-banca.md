# SLICE-AD20-06 — CNP complet, SID și suma propusă la asocierea extraselor

Data: 05.11.2026.

## Cerere și implementare

În **Asociere extrase cu copii**, lista Copii primește mai mult spațiu din lățimea listei Plătitori. CNP-ul are minimum 145 px, iar coloana nouă **SID (lei)** minimum 95 px, cu tip numeric, aliniere la dreapta și afișare în format românesc. Grila Plătitori nu mai include coloana **Banca**; identificarea plătitorului și fluxul existent nu sunt modificate.

SID-ul este citit pentru **Luna înregistrării**, independent de luna extrasului și de grupa din situația istorică. Copiii rămân cei ai grupei curente din catalog, ca în asocierea existentă. O rută nouă de citire întoarce acești copii cu SID-ul lunii, în subunitatea autentificată. Pentru lunile închise, se folosesc SID-urile salvate când există. Pentru lunile deschise și copiii fără situație salvată, se reutilizează motorul `calculations.calculate`, cu istoricul și reporturile existente. Copiii fără prezență primesc numai în memorie un rând de calcul cu zero zile/valoare; nu se inserează prezență, situație sau plată la citire. Situațiile/prezențele duplicate și SID-ul indisponibil sunt raportate, fără a le înlocui tacit cu zero.

La selectarea copilului, direct în grilă sau prin căutare, **Suma pentru copil (lei)** primește SID-ul pozitiv. Dacă SID este zero, primește întreaga sumă a extrasului selectat. Valoarea rămâne editabilă și nu se plafonează la suma extrasului sau la restul nerepartizat. Când copilul este ales înaintea extrasului, selectarea ulterioară a extrasului completează propunerea corespunzătoare. După adăugarea copilului în repartizare, rămâne propunerea existentă a restului până la alegerea următorului copil. Controalele existente pentru copii repetați, total și sume pozitive întregi sunt păstrate.

SID-urile se recitesc la încărcarea grupei, la căutare/selectare, reîncărcare și schimbarea lunii. Fiecare rând păstrează identitatea lunii SID-ului, iar butoanele de adăugare/salvare nu folosesc un copil încărcat pentru altă lună. Nu se schimbă formulele motorului, datele istorice, alte grile sau schema MariaDB.

## Fișiere proprii și motivul înregistrării

Motivul înregistrării: «Lărgirea listei Copii pentru CNP complet și SID-ul lunii, completarea sumei din SID sau întreaga încasare când SID este zero și eliminarea coloanei Banca din Plătitori.»

- `PYTHON/routes/adechit/bank_children.py` (nou): copiii grupei cu SID-ul lunii înregistrării, numai citire.
- `PYTHON/routes/adechit/__init__.py`: ruta pentru copiii și SID-urile lunii.
- `PYTHON/static/js/adechit/bank-associations.js`: coloanele CNP/SID, eliminarea Banca, încărcarea lunară și propunerea sumei.
- `PYTHON/static/css/adechit-bank-associations.css`: lărgirea Copii din spațiul Plătitori.
- `PYTHON/static/help/adechit.json`: folosirea SID-ului și propunerea sumei.
- `ADECHIT/UTILIZARE_WEB.md`: ghidul cu referința AD20-06.
- `ADECHIT_STATUS.md`: starea curentă în index.
- `docs/worklog/state/ADECHIT_STATUS_ADE20-ADE29.md`: registrul detaliat și următoarea subfelie liberă.
- `docs/worklog/SLICE-AD20-06-copii-cnp-sid-suma-fara-banca.md`: cerere, implementare, fișiere și limite.

## Stare și limite

**TESTAT ȘI VALIDAT DE ADELINA — VERSIUNEA FINALĂ, ÎNREGISTRAT PENTRU SERVER.**

La 05.11.2026, Adelina a confirmat explicit: «am verificat si validat: inregistreaza fisierele». Validarea privește versiunea finală cumulată AD20-01–13, cu toate corecțiile ulterioare; descrierile intermediare din workloguri păstrează istoricul cererilor. Confirmarea nu detaliază separat acoperirea fiecărui scenariu enumerat anterior.

Toate cele 9 fișiere proprii enumerate mai sus, inclusiv ajutorul și documentația, sunt înregistrate pentru SLICE-AD20-06 prin tools/Register-DeploymentTask.ps1, cu motivul acestei intervenții și versiunile finale curente. Agentul nu a executat teste automate sau vizuale, compilare, browser, SQL, SSH, publicare, restart ori Git.

Scenarii consemnate anterior pentru operator: CNP de 13 cifre și SID complet vizibile la dimensiunea/zoomul folosit; SID numeric și sortare; lună deschisă/închisă; copil fără prezență și copil mutat; selecție directă/prin căutare; SID pozitiv/zero; copil ales înainte/după extras; schimbarea lunii; sumă editată manual, repartizare pe mai mulți copii și refuzul depășirii totalului; Plătitori fără Banca. Se publică împreună modulul Python nou, ruta, JS/CSS și ajutorul. Fără SQL de aplicat.

Captura 13 din ajutor este anterioară modificării și rămâne de refăcut cu blur puternic pe datele sensibile. Nu s-au creat sau modificat capturi.
