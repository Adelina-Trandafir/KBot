# SLICE-VC06-32 — Număr NIR zecimal automat și blocat după schimbarea datei

Data: 03.11.2026.

## Regula explicită a utilizatorului

Utilizatorul corectează VC06-31: când se modifică Data NIR sau Data facturii, numărul NIR trebuie recalculat automat. Dacă nu există un NIR după data aleasă, numărul se lasă neschimbat. Dacă există, se atribuie următorul zecimal disponibil, se afișează caseta zecimală și se dezactivează atât Număr NIR, cât și zecimalele.

Recalcularea nu mai depinde de opțiunea intermediară „număr introdus manual”; se aplică la adăugare și la schimbarea datelor unui NIR existent. La revenirea la o dată fără NIR ulterior, valorile curente se păstrează și câmpurile redevin editabile. Un formular nou cu număr gol necesită în continuare o propunere inițială. Sincronizarea Data NIR după Data facturii, cerută anterior, rămâne activă; poziția se stabilește după Data NIR rezultată.

## Implementare

- `PYTHON/routes/vercon/nir_number.py` (nou): regula NIR este separată de numerotarea FCS. Se exclude documentul curent la editare și se caută NIR-uri cu dată strict ulterioară, în seria anuală a subunității/gestiunii. FCS nu declanșează singur blocarea, dar numerele sale, ca și numerele anulate, ocupă poziții în seria comună și nu se reutilizează.
- Când există NIR ulterior, calculul atribuie o fracție nenulă după ultima poziție anterioară, de exemplu 26,1 → 26,2. Dacă intervalul este mai îngust, poate folosi sutimi, fără să depășească poziția următoare. Lipsa unei poziții admise este refuzată explicit. Nu se introduce NIR 0 și nu se depășesc două zecimale.
- Fără NIR ulterior, perechea număr/subnumăr furnizată rămâne neschimbată; sunt verificate forma și unicitatea. Nu se aplică limita de dată dedusă exclusiv din ordinea numerică, deoarece aceasta ar contrazice păstrarea numărului cerută de utilizator. Numai un număr gol primește propunerea inițială maxim + 1.
- `receipt_number.py`: citirea seriei include marcajul Storno, pentru a distinge NIR de FCS. Algoritmul FCS nu este schimbat în acest slice.
- `doc_routes.py`, `receipt_routes.py`, `receipt_distribution.py`: propunerea și validarea NIR folosesc noul contract; validarea recunoaște și schimbarea datei facturii. Repartizarea pe destinații verifică regula separat în fiecare serie. Afișarea existentă „auto” pentru destinații multiple este păstrată.
- `receipts.py`: crearea și editarea datelor/numărului unui NIR obișnuit reverifică și atribuie perechea sub blocarea gestiunii. Numărul calculat la editare nu este suprascris cu cel anterior. Auditul editării include numărul și subnumărul înainte/după. O editare economică fără schimbarea datelor sau numărului nu renumerotează documentul. Stocul inițial folosește traseul său existent.
- `static/js/vercon/document-number.js`: pentru NIR, transmite numărul curent și aplică rezultatul obligatoriu când există NIR ulterior. Fără NIR ulterior, nu schimbă valorile existente, inclusiv la schimbarea datei. Starea `nirNumberLocked` controlează blocarea.
- `static/js/vercon/document-subnumber.js`: afișează caseta zecimală în cazul automat și dezactivează ambele TextBox-uri prin API-ul `setEnabled`. Starea se păstrează după redesenare; când nu mai există NIR ulterior se reactivează câmpurile. Interacțiunile manuale sunt ignorate cât timp câmpurile sunt blocate.
- `static/js/vercon/docs.js`: fiecare schimbare de dată cere recalcularea și pentru NIR salvat ori cu număr manual. La refuzul validării se restaurează datele, perechea număr/subnumăr și blocarea anterioară. Trecerea la stoc inițial elimină blocarea specifică inserării NIR.
- Ajutorul și README-ul rapoartelor descriu regula finală și înlocuiesc explicația intermediară VC06-31.

Limita intervenției: numerotarea NIR la schimbarea datelor și coerența ecran/validare/salvare. Protecțiile stocului, limitele exercițiului, dependențele documentelor, identificarea facturilor și regulile de acces nu sunt relaxate. Nu se modifică FCS, BC/OI sau scheme MariaDB și nu se renumerotează alte documente.

## Fișiere proprii și motiv pentru înregistrare după confirmare

Motiv: `Recalcularea obligatorie a numarului NIR dupa schimbarea datei cand exista NIR ulterior, cu zecimal automat si campuri dezactivate; pastrarea numarului daca nu exista NIR ulterior.`

- `PYTHON/routes/vercon/nir_number.py`
- `PYTHON/routes/vercon/receipt_number.py`
- `PYTHON/routes/vercon/doc_routes.py`
- `PYTHON/routes/vercon/receipt_routes.py`
- `PYTHON/routes/vercon/receipt_distribution.py`
- `PYTHON/routes/vercon/receipts.py`
- `PYTHON/static/js/vercon/document-number.js`
- `PYTHON/static/js/vercon/document-subnumber.js`
- `PYTHON/static/js/vercon/docs.js`
- `PYTHON/static/help/vercon.json`
- `PYTHON/reports/vercon/README.md`
- `docs/worklog/SLICE-VC06-32-nir-zecimal-automat-blocat-dupa-data.md`
- `docs/worklog/SLICE-VC06-31-inserare-nir-numar-dupa-data.md`
- `VERCON/VERCON_STATUS.md`
- `implementare_vercon_v2.md`

## Stare

**IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT, NEÎNREGISTRAT PENTRU GIT.** Analiză de design din surse, fără teste, compilare, verificări automate de execuție, browser, HTTP, SQL, SSH, publicare, restart, Git sau script de înregistrare. Fișierele noi și versiunile modificate necesită confirmarea explicită pentru înregistrare conform AGENTS.md; confirmările anterioare nu acoperă această corecție ulterioară.

## Mesaje și înregistrare pentru publicare — 03.11.2026

La cererea explicită „scrie și mesajele pentru publicare pe server prin SSH”, au fost înregistrate cu succes cele 11 fișiere proprii pentru server din VC06-32 și cele 9 fișiere proprii comune cu etapa VC06-31. Sunt 20 de înregistrări pentru 11 fișiere distincte, cu SHA-256 ale versiunilor curente. Documentația din afara arborelui PYTHON nu a fost inclusă, conform limitării anterioare a utilizatorului la fișierele care ajung pe server. Fișierele pentru server nu au fost modificate după înregistrare.

Mesajul derivat pentru lotul VC06-31/32:

```text
SLICE-VC06-31/32

- Corectarea propunerii și salvării numărului și subnumărului pentru inserarea NIR între documente existente.
- Recalcularea numărului NIR la schimbarea datei: zecimal automat cu ambele câmpuri dezactivate dacă există NIR ulterior; păstrarea numărului în lipsa unui NIR ulterior.
```

Stare: **ÎNREGISTRAT PENTRU PUBLICARE LA CERERE EXPLICITĂ; FĂRĂ TRANSFER, CONEXIUNE SSH SAU GIT EXECUTAT.** KBOTADMIN construiește mesajul efectiv din înregistrările fișierelor selectate; un lot care include și alte slice-uri va include și motivele lor. Această înregistrare nu declară teste executate și nu publică fișierele.

## Reînregistrarea versiunilor rămase necomise pe server — 03.11.2026

Utilizatorul cere ca fișierele proprii rămase în Changes pe server să aibă slice și informații pentru operare din KBOTADMIN. S-au citit prin SSH numai amprentele celor 10 fișiere de aplicație din captură, evidența `.kbotadmin-deployment/debug.json` și starea Git limitată la aceste căi. Cele 10 versiuni locale coincid exact cu cele de pe server. Jurnalul `api_server.log.1` nu a fost inclus.

La momentul citirii, evidența debugging era goală, iar înregistrările locale VC06-31/32 nu mai existau. Starea corespunde fluxului normal de publicare, care salvează versiunile anterioare și consumă metadatele lotului, apoi încarcă versiunile curente. Fișierele din captură nu erau disponibile ca versiuni debugging pentru „Finalizează debugging”. Nu s-a fabricat și nu s-a modificat evidența debugging a serverului.

La cererea explicită s-au creat înregistrări noi prin scriptul proiectului: 8 pentru VC06-31 și 10 pentru VC06-32, adică 18 înregistrări pentru exact 10 fișiere distincte. Motivele sunt cele reale ale slice-urilor. Lista corespunde celor 11 fișiere pentru server ale VC06-32, cu excluderea `PYTHON/static/help/vercon.json`, care nu apărea între fișierele rămase din captură și a fost tratat separat în VC06-33.

KBOTADMIN poate reciti metadatele la **Scanează**, afișând fișierele chiar dacă versiunile locale/server coincid. Pentru acest caz, utilizatorul poate selecta cele 10 fișiere și folosi **Trimite**, cu **Publicare debugging debifată**: fluxul normal va salva în Git versiunile deja existente pe server înainte de încărcarea aceluiași conținut. Repornirea nu este necesară pentru această repetare a aceluiași conținut. Nu s-a executat commit, transfer de fișiere de aplicație sau restart în etapa de pregătire. Nu s-a executat Git local, test sau verificare funcțională a aplicației.
