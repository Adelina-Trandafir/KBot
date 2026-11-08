# SLICE-AD20-13 — Data_extras în asociere și în Ultimele încărcări

Data: 05.11.2026.

## Cerere și implementare

La corecția explicită a Adelinei, data din DGV-ul de încasări al **Asocierii extraselor cu copii** este atributul **Data_extras** al antetului XML. Aceeași dată este adăugată în DGV-ul **Ultimele încărcări**, imediat lângă data/ora **Încărcat**.

Sursa existentă este `FX_Extrase_F.DataExtras`, unde importatorul păstrează atributul XML. În asociere se citesc legăturile `FX_Extrase.IDFXH → FX_Extrase_H.IDEXH → FX_Extrase_H.IDEXF → FX_Extrase_F.IDEXF`, iar în istoric se folosește `AD_EXTRASE_INCARCARI.IDEXF → FX_Extrase_F.IDEXF`. Ambele proiecții întorc `DATE(DataExtras)` ca dată calendaristică, cu titlul **Data extras**. Exemplul furnizat `Data_extras="04-Nov-2026"` corespunde datei **04.11.2026**.

Se folosesc legături LEFT JOIN, pentru a nu elimina operații sau încărcări fără antet legat. Nu se înlocuiește o dată lipsă cu DataBanca, DataDoc, data încărcării sau data dedusă din numele fișierului; Data extras rămâne goală. În istoric, și coloana Încărcat folosește valoarea calendaristică originală pentru sortare, păstrând formatarea existentă cu dată/oră.

Data extras este informație de afișare și nu intră în amprenta existentă a operațiunii bancare; salvarea recitește în continuare aceleași câmpuri sursă. Intervenția nu schimbă importul, datele documentelor generate, filtrele perioadelor, numerotarea sau schema MariaDB. Înlocuiește alegerea DataBanca pentru coloana afișată, descrisă istoric în AD20-02.

## Fișiere proprii și motivul înregistrării

Motivul înregistrării: «Afișarea atributului Data_extras din antetul XML în asocierea extraselor și lângă data încărcării în Ultimele încărcări.»

- `PYTHON/routes/adechit/bank_associations.py`: proiecția datei extrasului din antetul fișierului.
- `PYTHON/static/js/adechit/bank-associations.js`: coloana Data extras.
- `PYTHON/routes/adechit/extras_intake.py`: data extrasului în proiecția istoricului.
- `PYTHON/static/js/adechit/extracts-intake.js`: Data extras imediat după Încărcat, coloane de dată cu sortare cronologică.
- `PYTHON/static/help/adechit.json`: sursa și folosirea datei în cele două liste.
- `ADECHIT/UTILIZARE_WEB.md`: ghidul actualizat cu referința AD20-13.
- `ADECHIT_STATUS.md`: starea curentă în index.
- `docs/worklog/state/ADECHIT_STATUS_ADE20-ADE29.md`: registrul detaliat și următoarea subfelie liberă.
- `docs/worklog/SLICE-AD20-13-data-extras-asociere-istoric.md`: cerere, implementare, fișiere și limite.

## Stare și limite

**TESTAT ȘI VALIDAT DE ADELINA — VERSIUNEA FINALĂ, ÎNREGISTRAT PENTRU SERVER.**

La 05.11.2026, Adelina a confirmat explicit: «am verificat si validat: inregistreaza fisierele». Validarea privește versiunea finală cumulată AD20-01–13, cu toate corecțiile ulterioare; descrierile intermediare din workloguri păstrează istoricul cererilor. Confirmarea nu detaliază separat acoperirea fiecărui scenariu enumerat anterior.

Toate cele 9 fișiere proprii enumerate mai sus, inclusiv ajutorul și documentația, sunt înregistrate pentru SLICE-AD20-13 prin tools/Register-DeploymentTask.ps1, cu motivul acestei intervenții și versiunile finale curente. Agentul nu a executat teste automate sau vizuale, compilare, browser, SQL, SSH, publicare, restart ori Git.

Scenarii consemnate anterior pentru operator: un extras cu Data_extras diferită de DataBanca/DataDoc, aceeași dată în ambele liste, data/ora încărcării păstrate, sortare cronologică, încărcare fără antet legat și salvarea asocierii cu amprenta existentă. Se publică împreună cele două module Python și cele două module JS. Fără SQL de aplicat sau capturi noi. Capturile 12/13 din ajutor rămân de refăcut cu blur puternic pe datele sensibile.
