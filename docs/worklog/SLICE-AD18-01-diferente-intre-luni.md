# SLICE-AD18-01 — Diferențe între luni consecutive în ADECHIT

04.11.2026. Cerință: raport nou care compară exclusiv SID/SIC din luna curentă cu SFD/SFC din luna anterioară și afișează numai diferențele, cu rândul complet curent deasupra celui anterior. Adelina a clarificat explicit că rândurile lipsă înseamnă copiii prezenți într-o singură lună: «Da, lipsesc din una dintre luni».

## Implementare

Raportul `month-comparison`, nume stabil pentru lățimile Excel `ADECHIT_DIFERENTE_LUNI`, folosește aceleași rute de generare, snapshot, drept de citire, subunitate și export PDF/Excel ca rapoartele ADECHIT existente. Nu introduce tabele, DDL, operații de scriere în evidență sau calcule financiare alternative. Modulul nou `month_comparison.py` reutilizează `situation_rows` și coloanele/formatarea situației lunare.

Luna selectată este luna curentă a comparației. Luna anterioară se caută după anul și luna calendaristică, în aceeași subunitate, cu trecerea decembrie–ianuarie. Nu se scade IDL și nu se sare peste o lună absentă. Dacă luna anterioară nu există, raportul refuză generarea cu mesaj care indică perioada lipsă. Situațiile lunilor închise sunt citite din SS_Buget, iar cele ale lunilor deschise sunt calculate prin fluxul existent, fără rescriere.

Rândurile celor două luni sunt asociate după IDP, independent de grupa istorică, nume sau CNP. Nu se ignoră în tăcere duplicatele sau identificatorii lipsă: raportul refuză datele care nu pot forma o pereche unică. Perechea este inclusă exclusiv dacă SID curent diferă numeric de SFD anterior, SIC curent diferă numeric de SFC anterior sau rândul lipsește din una dintre luni. Comparația folosește Decimal înaintea formatării și tratează soldurile NULL ca zero. Diferențele din celelalte coloane nu includ singure perechea. Filtrul hide_zero nu este folosit; lipsa unui rând rămâne relevantă chiar când rândul existent conține numai zero.

Tabelul A4 landscape include toate coloanele situației lunare, plus Luna, Grupa și Diferență / lipsă. Fiecare pereche are același număr pe două rânduri consecutive, curent apoi anterior. Rândul absent este marcat LIPSĂ, păstrează numele/CNP-ul ca repere de identificare și are celule numerice goale. Nu sunt fabricate sume pentru luna absentă. Antetul numără perechile cu solduri diferite și rândurile lipsă în fiecare lună; nu se fac totaluri care ar aduna cele două perioade.

Filtrarea după grupă se aplică după asocierea ambelor luni complete: se păstrează perechea dacă oricare dintre rânduri aparține grupei. Astfel, un transfer nu devine o lipsă falsă. Toate grupele poate fi combinat cu Separă grupe, caz în care perechea rămâne în grupa curentă sau, dacă rândul curent lipsește, în grupa anterioară. Perechile și grupele sunt ordonate alfabetic cu chei explicite în limba română, fără sortare după ID. O grupă care nu mai este disponibilă în selectorul lunii curente poate fi inclusă prin Toate grupele; selectorul existent nu a fost extins.

Web și K-BOT au butonul «Diferențe între luni», cu aceleași alegeri lună/grupă/toate/separat. În K-BOT, butonul nou este declarat în Designer într-un KBotTableLayoutPanel; panoul de parametri permite derularea pentru a păstra accesul la comenzi când înălțimea nu ajunge. Ajutorul textual al ambelor interfețe și ghidul de utilizare sunt actualizate.

## Fișiere proprii create/modificate

Server și web:

- `PYTHON/routes/adechit/month_comparison.py` — nou, selectarea și macheta comparației.
- `PYTHON/routes/adechit/reports.py` — înregistrarea generatorului și numelui Excel.
- `PYTHON/static/adechit.html` — butonul raportului.
- `PYTHON/static/js/adechit/reports.js` — transmiterea selecției către generatorul comun.
- `PYTHON/static/help/adechit.json` — instrucțiunile raportului nou.

K-BOT:

- `src/KBot.Domain/Adechit.vb` — constanta tipului nou de raport.
- `src/KBot.App/Views/Adechit/AdechitRapoarteForm.vb` — disponibilitate și generare.
- `src/KBot.App/Views/Adechit/AdechitRapoarteForm.Designer.vb` — butonul și rândul de comenzi, derularea parametrilor.
- `src/KBot.App/HelpContent/contabil/adechit.md` — instrucțiuni și cuvinte de căutare.

Documentație:

- `ADECHIT/UTILIZARE_WEB.md`.
- `ADECHIT_STATUS.md`.
- `docs/worklog/state/ADECHIT_STATUS_ADE10-ADE19.md`.
- `docs/worklog/SLICE-AD18-01-diferente-intre-luni.md` — acest fișier.

Motiv concret pentru înregistrarea tuturor celor 13 fișiere: «Raport ADECHIT de diferențe SID/SIC curent față de SFD/SFC anterior și rânduri lipsă între luni consecutive, cu perechi complete, acces web/K-BOT, PDF/Excel și documentație.»

## Stare și limite

SCRIS LOCAL, NETESTAT, NEPUBLICAT. S-au citit sursele și schema locală relevantă din `MariaDB_Schema/000_DEMO.sql` și s-a analizat designul. Conform instrucțiunilor Adelinei nu s-au rulat teste, compilări, verificări automate, probe vizuale sau generări PDF/Excel. Nu s-au executat SQL, conexiuni la server, publicare, restart sau Git.

Pentru proba Adelinei: solduri egale cu alte coloane diferite (nu trebuie afișate), diferență numai SID/SFD, diferență numai SIC/SFC, rând lipsă în fiecare lună inclusiv cu valori zero, transfer între grupe, toate/separat, luni deschise/închise, decembrie–ianuarie, lună anterioară absentă, lipsa diferențelor, PDF/Excel și accesul din ambele interfețe. Macheta și paginarea nu au fost văzute. Publicarea trebuie să includă modulul Python nou; K-BOT necesită compilare și distribuire de către operator. Fără scripturi SQL.

## Capturi de refăcut

`PYTHON/static/help/images/adechit/19-panou-rapoarte.jpg` nu include butonul nou și rămâne de refăcut, cu blur puternic pe datele sensibile. Nu s-au creat ori modificat capturi. Nu există o captură K-BOT în topicul actual.

## Înregistrare și închidere

Adelina a autorizat înregistrarea la 04.11.2026 prin «inregisreaza daca trebuie» și a semnalat apoi fișierul propriu month_comparison.py rămas FĂRĂ GIT. Prima înregistrare din chat a inclus numai AD19-02; se completează acum separat AD18-01 pentru toate cele 13 fișiere enumerate, cu motivul raportului de diferențe. Confirmarea nu reprezintă testare sau publicare. Rezultatul efectiv al scriptului este raportat în chat și în registrul imuabil; închiderea din perspectiva registrului depinde de succesul scriptului.
