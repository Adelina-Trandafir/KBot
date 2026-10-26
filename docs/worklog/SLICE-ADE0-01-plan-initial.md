# SLICE-ADE0-01…05 — Planificare, mapare și reguli ADECHIT

Etape documentare din 07–08.10.2026, în ordinea lor, cu livrabilele și limitele inițiale păstrate. Confirmarea comună a rubricilor de nevalidat/amânat din ADE0-01…05: *Rezolvat: operatorul a confirmat, la 23.10.2026, că nu mai este nimic nevalidat sau deschis.* Planul inițial nu constituie singur dovada implementării ADE1–ADE9.

- [ADE0-01](#slice-ADE0-01)
- [ADE0-02](#slice-ADE0-02)
- [ADE0-03](#slice-ADE0-03)
- [ADE0-04](#slice-ADE0-04)
- [ADE0-05](#slice-ADE0-05)

## SLICE-ADE0-01 — planul inițial ADECHIT <a id="slice-ADE0-01"></a>

Data: 07.10.2026. Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026), numai documentație.

### Ce s-a schimbat și de ce

Am creat registrul separat ADECHIT, după structura KBOT_STATUS: index în rădăcină, detalii în fișier de stare pentru zece felii, worklog pentru intervenție, decizii fixe, subfelii și numere libere. Utilizatorul a cerut explicit numerotarea ADE0, ADE1 etc.

Planul include corecțiile utilizatorului: toate sistemele comune sunt reutilizate; editarea DataGrid în celule este critică din prima versiune; fără facturi, bonuri fiscale sau reduceri pentru absențe/frați; mdl_Situatie și interogările lui se transpun 1 la 1; cache-urile `_L` nu se migrează. Datele unității rămân în sistemul existent, fără pagină sau rută ADE pentru editarea lor. Unitati_Chitante este excepția explicită de la prefixul AD_, iar seria și numărul oferite în conversație sunt doar exemple.

Codex scrie și verifică local pe localhost:5050; utilizatorul publică prin AvacontPush și testează pe Linux/MariaDB. Acest flux prevalează asupra cerinței generice de push din CODE_WORKFLOW. Nu s-a făcut commit sau push în această intervenție.

### Fișiere atinse

- [ADECHIT_STATUS.md](../../ADECHIT_STATUS.md) — index, reguli, decizii și ordinea lucrărilor.
- [ADECHIT_STATUS_ADE0-ADE9.md](state/ADECHIT_STATUS_ADE0-ADE9.md) — planul detaliat, criterii de acceptare, dependențe și întrebări rămase.
- Acest worklog.

### Rezultatele verificărilor

- Citite regulile din KBOT_STATUS.md și CODE_WORKFLOW.md și un exemplu de fișier de stare.
- Planul confruntat cu sursele locale inspectate în conversație: exportul Access, mdl_Situatie și interogări, portalul Python/JS, autentificarea/conexiunile, loggerul, controalele, schema_sync, AvacontPush și DateUnitateForm/API-ul de date unitate.
- Verificate local existența țintelor linkurilor și consecvența numerotării planului.
- Fără schimbare de cod executabil; nu este necesară compilare sau rularea suitei. Preview-ul 5050 nu este încă implementat sau pornit.

### Neverificat sau amânat

- Niciun acces la server, MariaDB sau datele live; niciun SQL executat, niciun deploy.
- Nu este încă terminată maparea tuturor fluxurilor active; aceasta este ADE0-02.
- Lipsesc datele și rezultatele Access pentru paritate, schema live și matricea finală a drepturilor. Aceste dependențe sunt înregistrate în status.
- Planul nu reprezintă implementarea sau acceptarea funcțională a vreunei felii ADE1–ADE9.
- Fără schimbări vizibile în aplicație: nu există pagini de ajutor de actualizat acum.
- Nu a fost citit sau modificat niciun fișier Claude.md.

## SLICE-ADE0-02 — mapare Access → web și contractul calculelor <a id="slice-ADE0-02"></a>

Data: 07.10.2026. Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### Ce s-a schimbat și de ce

Am urmărit traseele din formulare/module și interogările motorului 2021 pentru a separa regulile care trebuie reproduse de cache-uri, funcții excluse și variante legacy. Inventarul clasifică 66 de tabele, 44 de formulare, 27 de interogări și 18 rapoarte. Contractul descrie formulele, ordinea fazelor, filtrele de anulare, tipurile/conversiile, snapshotul și cazurile necesare comparației Access/web. Nu constituie dovadă de paritate.

Utilizatorul a confirmat păstrarea încasărilor în lună închisă cu actualizarea situației salvate; prezența rămâne blocată. Am corectat formularea generală din plan. Codul identificat folosește CFGs/NUMAR drept următorul număr în Plati_chitante; seria/numărul ilustrative nu devin seed-uri. Neconcordanțele M01–M10 sunt explicite, inclusiv lipsa actualizării snapshotului în traseele de anulare găsite.

### Fișiere

- [Maparea fluxurilor](../../ADECHIT/MAPARE_ACCESS_WEB.md).
- [Contractul calculelor](../../ADECHIT/CONTRACT_CALCULE.md).
- [Inventarul obiectelor](../../ADECHIT/INVENTAR_OBIECTE.md).
- [Indexul ADECHIT](../../ADECHIT_STATUS.md).
- [Starea ADE0–ADE9](state/ADECHIT_STATUS_ADE0-ADE9.md).
- Acest worklog.

### Verificări efective

- Inspecție statică a surselor Access, relațiilor și interogărilor; fără rulare Access.
- Inventarul generat din numele fișierelor are clasificări explicite pentru 155 de obiecte; verificarea refuză un obiect lipsă din clasificare sau o clasificare duplicată.
- Verificare automată: 210 legături locale au ținte existente; fiecare dintre cele 155 de obiecte apare exact o dată în inventar; exact șapte tabele sunt excluse drept cache _L. Verificarea legăturilor confirmă fișierele/directoarele, nu ancorele interne Markdown.
- Reverificarea Salvare_Lunara confirmă 25 de câmpuri copiate în snapshot.
- Nu s-au rulat teste de aplicație: intervenția adaugă numai documentație.

### Neverificat sau amânat

- Rezultatele Access, conversiile ACE/ordinea evaluării, datele reale și rularea rapoartelor.
- M01–M10, traseul real de redeschidere, anularea în lună închisă și rapoartele legacy.
- Schema/API/editorul și previzualizarea 5050 rămân de implementat în feliile alocate.
- Nicio conexiune Linux/MariaDB, SQL executat, commit sau push. Utilizatorul publică prin AvacontPush și testează serverul. Modificările preexistente ale altor module nu au fost incluse.
- Nicio modificare vizuală în aplicație; nu există pagină de ajutor devenită neactualizată.

Următoarea lucrare planificată: ADE1-01, integrarea cu proiectul și sistemele comune.

## SLICE-ADE0-03 — documentarea tuturor feliilor ADECHIT <a id="slice-ADE0-03"></a>

Data: 08.10.2026. Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). Cerere: documentarea tuturor feliilor, cu oprire la final. Implementarea ADE1–ADE9 nu a fost începută.

### Ce s-a schimbat și de ce

Planul detaliază ADE0–ADE9 și toate cele 30 de subfelii alocate: dependențe, pași, livrabile, verificări și acceptare. Include sursele infrastructurii reutilizate, contractul editării în celule, modelul de date, drepturi, migrare, calcule, operații financiare, rapoarte și predarea către utilizator prin AvacontPush.

Un registru separat urmărește M01–M10 și D01–D10, cu feliile afectate și momentul necesar rezolvării. Nu inventează răspunsuri și nu blochează documentarea curentă. Indexul și fiecare secțiune de stare trimit către plan. ADE0-03 este alocată acestei intervenții; următoarea subfelie de analiză este ADE0-04, următoarea felie liberă ADE10.

### Fișiere

- [PLAN_IMPLEMENTARE](../../ADECHIT/PLAN_IMPLEMENTARE.md), nou.
- [DECIZII_DESCHISE](../../ADECHIT/DECIZII_DESCHISE.md), nou.
- [ADECHIT_STATUS](../../ADECHIT_STATUS.md), actualizat.
- [Stare ADE0–ADE9](state/ADECHIT_STATUS_ADE0-ADE9.md), actualizată.
- Acest worklog.

### Verificări efective

Inspecție locală a structurii și punctelor de integrare: Flask, portal, DataGrid, EventBus/ListenerTracker/registry, log, schema_sync, migrare și AvacontPush. Planul distinge sursele existente de căile/API-urile propuse și testele viitoare de cele efectuate. Verificarea automată a trecut: 10 felii, 30 de subfelii, fiecare cu secțiune unică în plan și rând unic în status; 90 de legături locale și 31 de ancore valide. Numerele următoare libere și stările separă documentarea de implementare.

### Neverificat sau amânat

Nu s-au rulat teste de aplicație, Access sau MariaDB: modificările sunt exclusiv documentare. Nu există schelet ADE nou, SQL nou, preview pornit, import, commit sau push. Configurările live, datele reale, rezultatele Access și deciziile M/D rămân necesare în feliile indicate. Nicio modificare vizuală de aplicație și niciun ajutor devenit neactualizat.

Lucrul se oprește la finalul documentării/verificării, conform cererii utilizatorului. ADE1-01 este doar următoarea lucrare planificată, nu este pornită automat.

## SLICE-ADE0-04 — adoptarea regulilor proiectului <a id="slice-ADE0-04"></a>

Data: 08.10.2026. Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### Ce s-a schimbat și de ce

La cererea explicită a utilizatorului, au fost adoptate cele 15 reguli din lista filtrată: 1, 2, 4, 5, 6, 9, 10, 11, 12, 17, 22, 28, 29, 30, 31. Numerotarea este păstrată. Regulile specifice VB.NET/WinForms nu au fost încorporate. Regulile adoptate sunt persistent accesibile din indexul citit înaintea lucrului ADECHIT.

### Fișiere

- [Reguli proiect](../../ADECHIT/REGULI_PROIECT.md), nou.
- [Index](../../ADECHIT_STATUS.md), trimitere obligatorie și stare actualizată.
- [Stare ADE0](state/ADECHIT_STATUS_ADE0-ADE9.md), subfelie și următor număr ADE0-05.
- [Plan](../../ADECHIT/PLAN_IMPLEMENTARE.md), actualizarea numerelor libere.
- Acest worklog.

### Verificări

Compararea regulilor salvate cu lista aprobată și verificarea legăturilor locale. Intervenție exclusiv documentară; nu necesită teste de aplicație.

### Neverificat sau amânat

Nu s-a auditat sau rescris codul existent pentru conformitate în această intervenție. Nu s-au modificat sursele Access, implementarea, configurările serverului sau fișiere Claude.md. Fără commit, push sau acces Linux/MariaDB.

## SLICE-ADE0-05 — sursa reală Access și procesele MSACCESS <a id="slice-ADE0-05"></a>

Data: 08.10.2026. Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### Ce s-a schimbat și de ce

Am salvat instrucțiunea utilizatorului: datele reale sunt în `C:\Users\Adelina Trandafir\source\repos\KBOT\ADECHIT\ACCESS_SOURCES\baza2020_PP.mdb`. Dacă rămâne un proces MSACCESS, nu încercăm să îl oprim; anunțăm clar utilizatorul să îl oprească el. Regula include procesele pornite pentru lucrare și orice metodă de oprire.

### Fișiere

- [Reguli](../../ADECHIT/REGULI_PROIECT.md).
- [Index](../../ADECHIT_STATUS.md).
- [Stare](state/ADECHIT_STATUS_ADE0-ADE9.md).
- [Plan](../../ADECHIT/PLAN_IMPLEMENTARE.md), următor număr ADE0-06.
- Acest worklog.

### Verificări

Memoria consemnează proba din intervenția precedentă: ACE 16.0 a deschis MDB în mod doar-citire; 24 tabele, Platitori 653, Prezenta 1214, Plati 380, SS_Buget 1214, CFGs 30. Procesul gazdă a ieșit ulterior cu eroare; aceasta nu dovedește existența unui proces MSACCESS rămas. Nu am reluat citirea MDB și nu am operat asupra proceselor în această intervenție.

### Neverificat sau amânat

Fără reconciliere completă, rezultate de calcul Access, modificări de date sau implementare. Fără oprire de proces, conexiune server, commit sau push. Modificări exclusiv documentare.
