# SLICE-AD20-05 — Panou de înregistrare de sus în dreapta și butoane duble în footer

Data: 05.11.2026.

## Cerere și implementare

În **Asociere extrase cu copii**, panoul cu datele ce urmează a fi înregistrate este mutat din rândul listelor Grupe/Copii/Plătitori într-o coloană proprie în dreapta. Pe calculator începe la nivelul părții de sus a tabelului de extrase, după rândul căutării extrasului, și se întinde până deasupra footerului. Tabelul de extrase este restrâns cu lățimea coloanei panoului. Coloana are 260 px, respectiv 230 px în adaptarea intermediară, și derulare proprie dacă este necesar.

Structura workspace-ului este o grilă explicită: perioade în stânga sus, căutare și extrase în centru sus, căutare copil/liste în stânga jos și datele de înregistrare în dreapta. Identificatorii controalelor și clasele folosite la schimbarea modului sunt păstrate. În modul automat, căutarea copilului și listele se ascund împreună, iar luna înregistrării rămâne în dreapta tabelului de revizuire. Pe ecrane de maximum 980 px se păstrează adaptarea cu panouri succesive.

Toate butoanele `.btn` din footerul acestei ferestre, inclusiv comutarea modului, au înălțimea minimă de **56 px**, față de **28 px** anterior (+100%). Footerul își rezervă spațiul corespunzător și permite în continuare trecerea butoanelor pe mai multe rânduri. Nu se măresc butoanele altor ferestre sau cele din interiorul panoului de repartizare.

Intervenția este numai de structură și CSS; logica asocierii, selecției copiilor, repartizării și salvării nu este modificată. Căutarea copilului rămâne numai în grupele vizibile, cu eticheta în stânga și selectarea textului la intrare.

## Fișiere proprii și motivul înregistrării

Motivul înregistrării: «Mutarea panoului de înregistrare în dreapta de la partea de sus a tabelului de extrase și dublarea înălțimii butoanelor din footerul asocierii.»

- `PYTHON/static/adechit.html`: panoul de înregistrare separat de liste și structura grilei.
- `PYTHON/static/css/adechit-bank-associations.css`: poziționarea coloanelor, adaptările și butoanele de 56 px.
- `PYTHON/static/help/adechit.json`: descrierea poziției panoului și a butoanelor.
- `ADECHIT/UTILIZARE_WEB.md`: ghidul cu referința AD20-05.
- `ADECHIT_STATUS.md`: starea curentă în index.
- `docs/worklog/state/ADECHIT_STATUS_ADE20-ADE29.md`: registrul detaliat și următoarea subfelie liberă.
- `docs/worklog/SLICE-AD20-05-panou-inregistrare-dreapta-butoane-duble.md`: cerere, implementare, fișiere și limite.

## Stare și limite

**TESTAT ȘI VALIDAT DE ADELINA — VERSIUNEA FINALĂ, ÎNREGISTRAT PENTRU SERVER.**

La 05.11.2026, Adelina a confirmat explicit: «am verificat si validat: inregistreaza fisierele». Validarea privește versiunea finală cumulată AD20-01–13, cu toate corecțiile ulterioare; descrierile intermediare din workloguri păstrează istoricul cererilor. Confirmarea nu detaliază separat acoperirea fiecărui scenariu enumerat anterior.

Toate cele 7 fișiere proprii enumerate mai sus, inclusiv ajutorul și documentația, sunt înregistrate pentru SLICE-AD20-05 prin tools/Register-DeploymentTask.ps1, cu motivul acestei intervenții și versiunile finale curente. Agentul nu a executat teste automate sau vizuale, compilare, browser, SQL, SSH, publicare, restart ori Git.

Scenarii consemnate anterior pentru operator: alinierea panoului cu partea de sus a tabelului de extrase, înălțimea disponibilă și derularea, modul automat/manual, căutarea copilului și listele, butoanele din footer la dimensiuni diferite de ecran și zoom. Nu s-au creat sau modificat capturi; capturile 13/14 din ajutor rămân de refăcut cu blur puternic pe datele sensibile. Nu există SQL de aplicat.
