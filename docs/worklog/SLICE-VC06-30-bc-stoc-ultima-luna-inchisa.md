# SLICE-VC06-30 — Editarea BC pornește de la stocul ultimei luni închise

Data: 03.11.2026.

## Cerere și regulă confirmată

Utilizatorul raportează din nou `STOCK_NEGATIVE`, APA 19L, −4 la 19.05.2026, în jurnalul din 03.11.2026, cererile `14e7d2cfba764ba59f26e7d9fa3606de` și `3b0191701657495c8efcfe0e3f35a88e`. VC06-29 a corectat data efectivă de stoc, dar nu a modificat regula editării BC care controla întregul istoric.

Utilizatorul propune pornirea de la stocul ultimei luni închise și confirmă explicit că aceasta înseamnă **luna calendaristică imediat anterioară lunii active**. În cod există închidere de an, nu un marcaj separat de închidere lunară; nu se introduce un asemenea marcaj.

Pentru luna activă octombrie, soldul din 30 septembrie devine soldul inițial. Un deficit temporar din mai nu mai blochează editarea BC dacă soldul de pornire este nenegativ. Se verifică toate mișcările din octombrie și cele ulterioare. Luna activă rămâne ultima lună cu date potrivit VC06-28, independent de calendarul calculatorului.

Propunerea intermediară de comparare a deficitelor înainte/după pe întregul istoric nu a fost implementată; regula lunii anterioare confirmată de utilizator a înlocuit-o înainte de editarea codului.

## Schimbări

- `PYTHON/routes/vercon/consumption_timeline.py` (nou): citește registrul comun numai pentru variantele afectate, în contextul existent al subunității/gestiunii/exercițiului. Mișcările cu dată anterioară primei zile a lunii active formează soldul inițial; nu se verifică soldurile lor intermediare. Cantitățile sunt Decimal, cu precizie de calcul 60.
- Un sold inițial negativ este refuzat explicit, la ultima zi a lunii anterioare, fără anulare artificială ori reparare de date. Mișcările din luna activă și toate cele ulterioare se verifică prin agregarea comună pe zi, intrări înaintea ieșirilor. Un deficit în acest interval produce refuz cu produs, dată și începutul perioadei verificate.
- `PYTHON/routes/vercon/documents.py`: numai `edit_bc` apelează noul control după înlocuirea provizorie a bonului. Începutul lunii se derivă din data BC deja validată în luna activă, nu din data calculatorului. Controlul rulează în tranzacția existentă, sub blocarea gestiunii, înaintea recalculării listei și auditului; orice eroare păstrează rollback-ul întregii editări.
- Ajutorul și documentația de rapoarte explică soldul de pornire și intervalul validat.

Limita intervenției: numai validarea stocului la **editarea BC**. Crearea/anularea BC, NIR, FCS, retururile și celelalte operații nu sunt schimbate. Rapoartele istorice continuă să arate soldurile reale. Nu se stochează solduri noi și nu se modifică date sau scheme MariaDB. Contextul exercițiului și intrările de stoc inițial existente sunt păstrate; nu se adună artificial exercițiul precedent peste soldul inițial deja migrat.

## Fișiere proprii pentru înregistrare după confirmarea Adelinei

Slice: `SLICE-VC06-30`.

Motiv: `Validarea editării BC pornește de la soldul lunii anterioare lunii active, fără blocare pentru deficite temporare din lunile vechi.`

- `PYTHON/routes/vercon/consumption_timeline.py`
- `PYTHON/routes/vercon/documents.py`
- `PYTHON/static/help/vercon.json`
- `PYTHON/reports/vercon/README.md`
- `docs/worklog/SLICE-VC06-30-bc-stoc-ultima-luna-inchisa.md`
- `VERCON/VERCON_STATUS.md`
- `implementare_vercon_v2.md`

## Stare

**IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT; ÎNREGISTRAREA PENTRU GIT AȘTEAPTĂ CONFIRMAREA EXPLICITĂ A ADELINEI DUPĂ PROBĂ.** Analiză de design din surse și din jurnalul furnizat. Nu s-au rulat teste, compilare, verificări automate, browser, HTTP, SQL, conexiuni SSH, publicare, restart, Git local sau scriptul de înregistrare. Nu există confirmare de probă pentru codul nou. Lista și motivul de mai sus sunt pregătite; nu reprezintă înregistrări în `docs/deployment/pending`.

Actualizare 03.11.2026: utilizatorul a cerut explicit înregistrarea fișierelor care ajung pe server. `tools/Register-DeploymentTask.ps1` a înregistrat cu succes pentru VC06-30 cele patru fișiere proprii din PYTHON: `consumption_timeline.py`, `documents.py`, `static/help/vercon.json` și `reports/vercon/README.md`. Documentația din afara arborelui publicat nu a fost inclusă, conform cererii. Împreună cu VC06-28 și VC06-29 s-au creat 28 de înregistrări pentru 25 de fișiere distincte; fișierele comune păstrează motivele ambelor slice-uri. Nu s-au modificat fișierele pentru server după înregistrare. Stare: **ÎNREGISTRAT LA CERERE EXPLICITĂ; FĂRĂ PUBLICARE SAU PROBĂ EXECUTATĂ ÎN ACEASTĂ ETAPĂ.** Nu s-a executat Git local ori pe server.
