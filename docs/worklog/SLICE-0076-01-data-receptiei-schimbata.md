# SLICE-0076-01 - data unei receptii schimbata in browserul FOREXE din K-BOT

**Data:** 06.11.2026. **Cerere (operator):** cand un user schimba data unei receptii in browserul din aplicatie, receptia din K-BOT (`FX_Receptii_R`) sa stie ca i s-a schimbat data; un user a schimbat data in afara aplicatiei si la descarcarea istoricului receptia a fost luata drept alt rand in R, ceea ce a stricat lantul de receptii. Planul complet (0076-01..03): `C:\Users\Adelina Trandafir\.claude\plans\mellow-popping-sifakis.md`. Aceasta subfelie = doar data schimbata IN K-BOT. Stergerea = 0076-02, plasa pentru schimbari din afara = 0076-03 (nepornite).

## 1. Ce s-a schimbat si de ce

Pasul 4b potriveste `ListaReceptii` cu `FX_Receptii_R` dupa **zi** (F25/F30). Cu data schimbata pe site nu exista R pe ziua noua, deci se insera un R nou, iar cel vechi pastra lantul de instantanee. `ForexeWatch.js` stia deja ambele date (`rowDate` = la apasarea ochiului, `formDate` = la Salveaza), dar shell-ul folosea doar data noua.

- **Browser** (`ForexeWatch.js`): pe `receptie-modificare` se retine si identitatea randului (`rowSuma`, `rowPozitie`, `rowRang` - rangul printre randurile cu aceeasi data, in ordinea listei, ca `RangZi`) si pleaca in `data` a evenimentului `finished`.
- **Shell** (`KbotForm.ForexeWatch.vb`, `PastreazaSchimbareaDeData`): la `Finished`, cand `rowDate <> formDate`, faptul se scrie pe disc (`ObservatiiReceptiiStore`, `<Asociere>\<cod>.observatii`) **inainte** de coada robotului - descarcarea poate fi aruncata (alta operatiune in lucru) sau poate cadea, faptul ramane.
- **Livrare** (`IngestFlow.TrimiteObservatiileAsync`): inainte de orice ingest al angajamentului, ce e in asteptare se trimite la server si se scoate din fisier doar dupa raspuns. Se livreaza la ingest, nu la descarcare, pentru ca 4b ruleaza la ingest - acelasi moment, dar acopera toate cai (iconita nodului, operatiune capturata, reluare dupa cadere). 409 -> intrebare Da/Nu (salveaza oricum ca receptie noua / opreste); orice alta eroare -> ingestul se opreste, faptul ramane.
- **Server** (`routes/forexe/receptii_observate.py`, `POST /api/forexe/receptii/data-schimbata`): alege R-ul din ziua veche (`alege_receptia`: singura a zilei; singura cu valoarea; dupa `RangZi`), `UPDATE DataR = noua, RangZi = NULL`. `RangZi = NULL` il face «rand vechi» pentru `_potriveste_ziua` (potrivire pe valoare exacta), iar 4b il re-stampileaza. Idempotent (zi veche goala + exact un R cu aceeasi suma pe ziua noua -> 200 «deja»). Ambiguu / negasit -> 409, nu scrie nimic.
- **Client HTTP:** `IReceptiiObservateApi` + `ApiClient.ReceptiiObservate.vb`; POCO `ObservatieReceptie` (KBot.Domain); `ObservatiiReceptiiStore` (KBot.Common).

**Abatere de la plan, spusa pe fata:** (a) alegerea R-ului NU refoloseste `_potriveste_ziua` direct, ci o functie proprie mai stricta (`alege_receptia`): `_potriveste_ziua` intoarce mereu un candidat «vechi» chiar si cand valoarea nu se potriveste (acceptabil la descarcare, unde urmatoarea repara; aici o alegere gresita muta o data pe alta receptie, tacut). Foloseste in schimb `_R_CANDIDATI_SQL` din 4b, deci «ce receptii sunt in acea zi» are o singura definitie. (b) Ruta nu e apelata din shell inainte de descarcare, ci din `IngestFlow` inainte de ingest (vezi mai sus).

## 2. Fisiere atinse

- `PYTHON/routes/forexe/receptii_observate.py` (nou), `PYTHON/routes/forexe/__init__.py` (import + comentariu)
- `src/KBot.Forexe/Services/JavaScripts/ForexeWatch.js`
- `src/KBot.Domain/ObservatieReceptie.vb` (nou), `src/KBot.Common/ObservatiiReceptiiStore.vb` (nou)
- `src/KBot.Api/IReceptiiObservateApi.vb`, `src/KBot.Api/ApiClient.ReceptiiObservate.vb` (noi)
- `src/KBot.App/KBotForm/Robot/KbotForm.ForexeWatch.vb`, `src/KBot.App/KBotForm/Robot/IngestFlow.vb`
- `src/KBot.App/HelpContent/contabil/forexe/browser.md` (sectiunea «Schimbi data unei receptii», tag `0076-01`)

Nicio schimbare de schema, niciun DDL.

## 3. Verificare

- `dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: KBot.Domain, KBot.Common, KBot.Api, KBot.Forexe, KBot.App **compilate fara erori de compilator** (nicio eroare BC). Build-ul s-a incheiat cu exit 1 doar pentru ca `KBot.App` (pid 38420) rula si tinea blocate DLL-urile din `src\KBot.App\bin\Debug` (MSB3021/3027 la copiere) - de repetat dupa inchiderea aplicatiei.
- `py_compile` pe `receptii_observate.py`: OK. `node --check` pe `ForexeWatch.js`: OK.
- **Nimic nu a fost rulat**: nici K-BOT, nici un browser FOREXE, nici MariaDB, nicio suita (cerere permanenta: fara teste).

## 4. Nerezolvat / neverificat

- Cheile randului razuit de `scrapeTable` pe tabelul real (`Data`, `Suma`) sunt cele pe care le citesc deja `receptionDates()` si payload-ul `ListaReceptii`; `rowSuma` presupune cheia `Suma` - **neverificat pe pagina reala**. Daca lipseste, `SumaVeche` ajunge gol, iar serverul se bazeaza pe «singura receptie a zilei» / rang.
- Coloana `DataR` presupusa DATE/DATETIME (ca la `_R_INSERT_SQL`); update-ul scrie un `date`.
- Doua receptii din aceeasi zi cu aceeasi valoare si fara `RangZi` stampilat raman ambigue (409) - intentionat.
- Fisierul `.observatii` sta in folderul Asociere (configurabil); nu are folder propriu in `settings.json`.
- Ajutor: sectiune adaugata in `browser.md`; nu s-a rulat `Check-Help.ps1`.
- Server: `receptii_observate.py` + `__init__.py` trebuie puse pe server de operator; **`Register-DeploymentTask.ps1` nerulat** (abia dupa testul si confirmarea ei).
