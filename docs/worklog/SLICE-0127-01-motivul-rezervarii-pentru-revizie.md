# SLICE-0127-01 - Motivul rezervarii din FOREXE, oferit la revizia > 0 (04.11.2026)

Operator, 04.11.2026: la salvarea unei modificari de rezervare (sau a unei rezervari noi) FOREXE cere un motiv; K-BOT sa-l pastreze
si sa-l ofere automat cand se genereaza o revizie > 0. **DOAR in modul Lucru din browserul din aplicatie** (vederea «Browser FOREXE»).
Numar de felie: ultimul liber (0127), cerut de operator. Raspunsurile ei: motivele se unesc cu «; » **doar** pentru acelasi cod
angajament si acelasi indicator; motivul mai lung de 255 se taie; **nu are incredere in ce e in FX_Istoric** pentru motiv.

## What changed and why
1. **Pagina FOREXE (`ForexeWatch.js`)**: la «Continua» din fereastra de motiv (clicul oprit deja pentru marcajul `(IDREV: n)`), textul scris de
   operator e citit INAINTE sa i se lipeasca marcajul, curatat (fara marcaj, spatii strânse) si tinut in starea operatiei (`rezMotiv`, in
   `sessionStorage`). Un motiv respins si trimis din nou in aceeasi operatie il suprascrie. Pleaca cu evenimentul «terminat» al rezervarii
   (`data.motiv`), alaturi de `indicatorCod`, care se citeste din tabel dupa salvare (si pentru un indicator NOU, prin diferenta de tabel).
   Nu se intampla cat lucreaza robotul (`suspended`), deci doar modul Lucru.
2. **Shell (`KbotForm.ForexeWatch.vb`)**: `PastreazaRezervareaAsync` apeleaza `PastreazaMotivulAsync` la FIECARE salvare (nu abia la «DA»), cu
   motivul si indicatorul din `DetaliiJson` (`MotivDin`). Un motiv care nu se poate pastra se spune pe consola si nu opreste nimic (salvarea e deja in FOREXE).
3. **API client**: `IMarcajApi.SalveazaMotivRezervareAsync` + `ApiClient.Marcaj.vb` -> `POST /api/forexe/marcaj/motiv`.
4. **Server**: `PYTHON/routes/forexe/marcaj_motiv.py` (modul nou). Ruta pastreaza motivul in tabelul nou `FX_RezervariMotiv` (`sql/0127_fx_rezervari_motiv.sql`),
   cheie unica `(IDREV, CodIndicator)`. IDREV il da **serverul** (numarul tinut in `FX_NumberLock` pentru angajament), niciodata clientul. A doua
   salvare a aceluiasi indicator in aceeasi sesiune: motivele se unesc cu «; » (nu se repeta unul identic). Fara numar tinut -> 409.
5. **Generarea reviziei (`ddf_edit.py`, `post_ddf_genereaza`)**, doar pentru revizia > 0: `propune_motiv` citeste motivele numarului tinut,
   doar pentru indicatorii liniilor reviziei (comparatie fara spatii/litere mari). Un motiv sau acelasi motiv la toti indicatorii -> textul simplu; indicatori
   diferiti cu motive diferite -> fiecare cu codul lui, separate prin « | » (nu se amesteca). Pentru `Desc_Scurta` se taie la 255 si apare avertisment
   in lista `avertismente`; `Desc_Lunga` / `Desc_Lunga_ANSI` primesc textul intreg. Fara motiv pastrat, ramane ca pana acum (tipul operatiei).
   Daca exista motive pentru numarul tinut dar pentru niciun indicator al reviziei, avertisment.
6. **Ajutor (0000-151)**: sectiune noua in `contabil/forexe/browser.md`, randul «Descriere scurta» in `contabil/ddf/editor.md`, textul din tutorialul
   `revizie-din-rezervare.md`; etichete `0127-01, 0000-151`.

## Files touched
`src/KBot.Forexe/Services/JavaScripts/ForexeWatch.js`, `src/KBot.Forexe/Models/ForexeWatchEvent.vb` (comentariu), `src/KBot.Api/IMarcajApi.vb`,
`src/KBot.Api/ApiClient.Marcaj.vb`, `src/KBot.App/KbotForm.ForexeWatch.vb`, `PYTHON/routes/forexe/marcaj_motiv.py` (nou), `PYTHON/routes/forexe/__init__.py`,
`PYTHON/routes/forexe/ddf_edit.py`, `sql/0127_fx_rezervari_motiv.sql` (nou), `src/KBot.App/HelpContent/{contabil/forexe/browser.md, contabil/ddf/editor.md, tutorials/revizie-din-rezervare.md}`.

## Test results
Nici un test scris sau rulat (regula proiectului). `ast.parse` pe cele trei fisiere Python: verde. `dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: 0 avertismente, 0 erori.
`Check-Help.ps1 -Coverage`: nu a trecut curat din motive vechi (sectiuni fara eticheta in `tutorials/ordonantare-din-plata.md`, control lipsa in `tutorials/parteneri.md`); ce tine de aceasta felie s-a reparat (etichetele `0127-01` / `0000-151` au rand in registru).

## Unverified / deferred
- **Nimic rulat**: nici in pagina FOREXE, nici pe server. **DDL-ul `0127` trebuie rulat de operator** (pe SURSA, apoi sincronizarea unitatilor); pana atunci ruta intoarce 500 cu mesaj clar si generarea reviziei ramane ca inainte (cu avertisment in jurnalul serverului).
- **Neverificat**: ca textul `Indicator_ang` citit din pagina e acelasi cu `FX_Rezervari.CodIndicator` (comparatia ignora spatiile si literele mari, dar nu alte diferente). Daca nu se potrivesc, revizia primeste avertismentul «motiv pastrat, dar pentru niciun indicator» si ramane tipul operatiei.
- **Neverificat**: ca fereastra de motiv apare in cadrul operatiei `rezervare` armate (altfel `state.op` nu e `rezervare` si motivul nu se pastreaza; se vede pe consola K-BOT ca lipseste «Motivul rezervarii ... e pastrat»).
- **Interpretare de confirmat**: motive diferite la indicatori diferiti ai aceleiasi revizii apar impreuna, cu codul indicatorului, separate prin « | » (descrierea scurta e un singur camp). Daca operatorul voia altceva, se schimba `text_motiv_revizie`.
- Motivele raman in `FX_RezervariMotiv` si dupa salvarea reviziei (ca evidenta); nu se sterg la stergerea reviziei.
- `ddf_edit.py` avea deja peste 2700 de linii (regula de ~1000); felia a adaugat ~20. Modulul nou e separat.
- Nimic in `NOUTATI.md` (se scrie la publicare). `help-version.txt` neatins.
- Ajutorul nu a fost vazut pe ecran; nicio captura noua sau de refacut.
