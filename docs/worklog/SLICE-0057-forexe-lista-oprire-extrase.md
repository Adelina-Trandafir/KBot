# SLICE-0057 — FOREXE: lista pe butonul din dreapta, oprirea pe «nu există în listă», extrasele SNM

Trei lucruri cerute de operator, în aceeași trecere. Primele două sunt reparații; al treilea
este o portare care nu mai fusese făcută deloc.

---

## 1. Descărcarea listei „rămânea în aer"

**Ce era.** Subsolul arborelui din `MainForm` are două iconițe. În designer, cea din STÂNGA
purta imaginea `database` și eticheta «Sursa datelor: unitatea și perioada…», iar cea din
DREAPTA purta imaginea de refresh și eticheta «Actualizează lista de angajamente din FOREXE».
Handler-ul însă era legat pe cea din **stânga** (`Tree_FooterLeftIconClicked`), iar cea din
dreapta nu avea niciun handler. Deci: butonul care spunea că actualizează lista nu făcea
nimic, iar celălalt descărca lista și o salva doar local — `DownloadListaAsync` întorcea
rândurile mapate, iar shell-ul le arunca. Nimic nu ajungea nici în arbore, nici pe server.

**Ce s-a făcut.** Alegerea operatorului (2026-09-08): **dreapta = lista, stânga = extrasele**.

- `KBOT.Designer.vb`: iconița din stânga devine `credit_card` cu eticheta extraselor; cea din
  dreapta își păstrează imaginea de refresh, cu eticheta rescrisă ca să spună regula nouă.
- `Tree_FooterRightIconClicked` (nou) face fluxul complet: descarcă lista prin coordonator,
  o trimite pe server, reîncarcă arborele **doar dacă s-a scris ceva**, apoi spune cifrele.

**Regula cerută — «dacă ang există, îl lasă în pace; dacă nu, îl adaugă».** Ruta de upsert
face de la felia 0008 `ON DUPLICATE KEY UPDATE Descriere, Stare`, adică rescria descrierea
fiecărui angajament la fiecare apăsare. S-a adăugat comutatorul `doar_noi` pe
`POST /api/forexe/angajamente/upsert`:

- absent sau `false` ▸ exact comportamentul de până acum (clienții vechi nu se schimbă);
- `true` ▸ codurile existente se citesc ÎNTÂI, în aceeași tranzacție, și **nu se ating deloc**;
  se inserează numai ce lipsește, cu `ON DUPLICATE KEY UPDATE CodAngajament = CodAngajament`
  ca plasă. Răspunsul poartă `inserate` / `existente`, cifre exacte — `rowcount` nu se poate
  desface înapoi sub `ON DUPLICATE KEY` (1 per insert, 2 per update).

Cifrele vin de la SERVER, nu se numără în client: arborele arată o singură perioadă (un an +
un SS), deci un angajament al altei perioade i-ar părea nou.

**Ce înseamnă „gol" (asumat, nu confirmat).** «Îl trimite la server gol (doar el ca denumire)»
s-a citit ca *doar antetul angajamentului* — `Cod` + `Descriere` + `Stare`, așa cum le dă lista
FOREXE — fără indicatori / recepții / plăți. Descrierea reală se trimite pentru că altfel nodul
din arbore ar apărea cu codul drept titlu (`PopulateTree` folosește `Descriere` ca titlu).
Descărcarea întreagă rămâne iconița din dreapta NODULUI.

---

## 2. Oprirea pe «Angajamentul … nu există în listă!»

**Ce era.** `adlop - Prelucrare Completa.wfl:43-44` (și varianta REVERSE) fac, când căutarea
nu întoarce nimic:

```xml
<Log message="EROARE: Angajamentul {{COD_ANGAJAMENT}} nu există în listă!" level="error" />
<Exit message="Angajamentul {{COD_ANGAJAMENT}} nu a fost găsit. Opresc execuția." />
```

`WorkflowExecutor.ExecuteAsync` prindea `WorkflowExitException` ca oprire **grațioasă** și nu
o mai spunea nimănui. `ForexeRunner.RunJobAsync` construia imediat după
`JobResult{Success = True}`, cu zero tabele. Shell-ul ducea pachetul gol mai departe, la
`DuLaIngestieAsync`, deci serverul Python primea o prelucrare fără nimic în ea. Operatorul
vedea eroarea în consolă și, în paralel, o cerere spre server care nu avea ce să facă.

**Ce s-a făcut.**

- `WorkflowExecutor` ține minte mesajul: câmpul `_exitMessage`, resetat la începutul fiecărui
  `ExecuteAsync` (o oprire a rulării precedente nu are voie să treacă drept a acesteia) și
  expus ca `ExitMessage`. Excepția tot NU iese din executor — un `<Exit>` e o oprire voită, nu
  o cădere, iar browserul trebuie să rămână deschis în urma ei.
- `ForexeRunner.RunJobAsync` citește `ExitMessage` imediat după execuție și, dacă e plin,
  întoarce `Failed(mesaj)`. La fel în `RunAsync` (Conectare) — «Conectare» n-are `<Exit>` azi,
  dar cele două căi nu au voie să răspundă diferit dacă i se adaugă unul.
- `ForexeController` capătă `LastFailure`: **de ce** s-a întors goală ultima intenție. Se
  golește la începutul fiecărei intenții și rămâne GOL când operatorul însuși a renunțat
  (a închis dialogul de certificat) — shell-ul arată casetă doar când e plin.
- `MainForm.ShowForexeFailure` pune motivul în fața operatorului.

Toate cele patru workflow-uri care au `<Exit>` îl folosesc pentru același lucru — «ce am fost
trimis să aduc nu e acolo, mă opresc» — deci schimbarea e corectă pentru toate.

---

## 3. Extrasele de cont (SNM) — portare completă

Nu exista nimic în K-BOT: doar sursa veche, `Surse/SURSA_FOREXE/Services/ForexeSNM.vb` (302
linii Playwright + Newtonsoft) și apelantul ei din `KBOT_IPC`. Pe server nu exista nicio rută
care să bage extrase în `FX_Extrase_F` / `FX_Extrase_H` / `FX_Extrase`.

### Împărțirea muncii — aceeași ca în Access

Robotul descarcă PDF-urile și desface XML-ul din ele; **serverul** citește XML-ul și scrie
tabelele. Exact ce făcea sistemul vechi: robotul împingea pe pipe trei câmpuri
(`PdfFisier`, `DataFisier`, `XmlContent`), iar `mdl_FX_Extrase.FX_Extrase_Prelucrare` făcea
toată prelucrarea. Citirea XML-ului cere nomenclatoare (`Clasificatii`, `Parteneri`, `Unitati`,
`FX_Indicatori`) la care clientul nu are acces.

### Partea de robot

- `KBot.Forexe/Services/ForexeSNM.vb` — nou. Nu e un workflow și nu are `.wfl`: cutia de
  mesaje FOREXE răspunde pe JSON (`ForexeSNM/messages/loadAll.do`), iar PDF-urile vin de pe a
  doua adresă (`downloadFile.do`). Ambele prin `page.EvaluateAsync` + `fetch(credentials:
  'include')`, deci pe cookie-ul sesiunii pe care operatorul a autentificat-o deja.
  Față de original: `CancellationToken` real (butonul «Anulează» din consolă funcționează),
  eșecurile ies în sus în loc să fie înghițite, iar rezultatul e o listă de POCO-uri
  (`ExtrasDescarcat`), nu `JArray`.
- `IForexeRunner.DescarcaExtraseAsync` + implementarea din `ForexeRunner`: trece prin runner
  fiindcă runner-ul e singurul care ține o pagină vie, și ca descărcarea să intre în istoricul
  de lucrări ca oricare alta. **Aruncă** la eșec (n-are `JobResult` de întors).
- `ForexeController.DownloadExtraseAsync`: cere data ultimului extras importat (punct de
  oprire al paginării), rulează robotul, raportează progresul pe ambele suprafețe.
- `KBotPaths.FolderExtrase` + cheia de setare `Extrase`.

### Partea de server — `PYTHON/routes/forexe/extrase.py` (nou)

- `POST /api/forexe/extrase/import` — portul lui `FX_Extrase_Prelucrare`. Trei niveluri:
  `/extras` ▸ `FX_Extrase_F`, `cont_ext` ▸ `FX_Extrase_H`, `cont_misc` ▸ `FX_Extrase`.
  Totul într-o singură tranzacție, ca în original.
- `GET /api/forexe/extrase/ultima` — `MAX(FX_Extrase_F.DataExtras)`.

Portate ca atare: `FX_ParseDataExtraseF` (inclusiv numele de lună RO+EN),
`ParseXmlDate_YYYYMMDD`, `ParseContMiscNodeToDict`, `Extrase_H_Add` (sursa din primele 3
caractere ale contului, `ClsfSal` din poziția 4, cele opt solduri) și `Extrase_Add` (referința
din explicații, `CodAI` direct sau prin clasificație, partenerul după `CodFiscal`).

---

## Ce s-a descoperit pe drum (și nu s-a inventat)

- **`FX_Extrase.HASH` e GOL pe toate rândurile scrise de Access.** `Extrase_Add` calcula
  hash-ul, căuta după el cu `FindFirst`… și apoi **nu îl scria** în `rsEx!HASH`. Deci
  deduplicarea rândurilor de extras nu a funcționat niciodată în sistemul vechi. Se vede în
  `FX_System_Export/TABLES/FX_Extrase.md`, unde coloana e goală. Aici hash-ul SE SCRIE, dar
  deciziile rămân pe **cheia naturală** (D9, felia 0048) — coloana are nevoie de o rulare
  completă înainte să se sprijine cineva pe ea.
- **Cheia „IBAN" din hash-ul de operațiune intra mereu goală.** Se alimenta din `vIbanPlat`,
  o variabilă care nu e atribuită NICIODATĂ în acea procedură (IBAN-ul plătitorului stă în
  `vPlatitorIBAN`). S-a reprodus ca atare: un hash „reparat" ar fi un șir nou.
- **`FX_Extrase.DataDoc` e TEXT, nu dată**, și poartă formatul scurt al mașinii românești
  (`30.12.2025` — confirmat în exportul tabelei). Se scrie în același format.
- **`CStr(<Double>)` din VBA taie la 15 cifre semnificative.** `100 * Round(0.07, 2)` dă
  `7.000000000000001` în virgulă mobilă; VBA scrie `7`, Python `str()` scrie tot. Din 600 de
  valori de bani încercate, **48** ies diferit. `_cstr_double_vba` reproduce tăietura.
  `_vba_cstr` din `prelucrare_helpers` NU s-a atins: forma lui actuală e deja în date.

---

## Fișiere atinse

**Nou**
- `src/KBot.Forexe/Services/ForexeSNM.vb`, `src/KBot.Forexe/Models/ExtrasDescarcat.vb`
- `src/KBot.Domain/AngajamenteAdaugate.vb`, `ExtrasPentruImport.vb`, `ImportExtraseRezultat.vb`
- `PYTHON/routes/forexe/extrase.py`
- `tests/KBot.Api.Tests/ExtraseApiClientTests.vb`
- `tests/KBot.App.Tests/ForexeControllerFailureTests.vb`
- `PYTHON/tests/test_forexe_extrase.py`
- `docs/worklog/SLICE-0057-forexe-lista-oprire-extrase.md`

**Modificat**
- `src/KBot.App/KBOT.vb` (cele două handlere de subsol + `ImportaExtraseAsync` +
  `ShowForexeFailure`), `src/KBot.App/KBOT.Designer.vb` (iconițe + etichete)
- `src/KBot.App/Forexe/ForexeController.vb` (`LastFailure`, `DownloadExtraseAsync`)
- `src/KBot.Forexe/Executor/WorkflowExecutor.Core.vb` + `.Flow.vb` (`ExitMessage`)
- `src/KBot.Forexe/ForexeRunner.vb`, `IForexeRunner.vb`
- `src/KBot.Api/ApiClient.vb`, `IApiClient.vb`, `UpsertAngajamenteRequest.vb`
- `src/KBot.Common/SetariFoldere.vb`, `KBotPaths.vb`
- `PYTHON/routes/forexe/angajamente.py` (`doar_noi`), `PYTHON/routes/forexe/__init__.py`
- `PYTHON/tests/test_forexe_angajamente.py` (trei teste `doar_noi`)
- cele nouă `IApiClient` false din `tests/KBot.App.Tests` (metodele noi)
- `FileVersion`: `KBot.Common` 1.4→1.5, `KBot.Domain` 1.1→1.2, `KBot.Forexe` 1.0.8→1.0.9.
  `KBot.Api` (1.0.5) și `KBot.App` (1.0.28) erau deja urcate în ciclul acesta.

---

## Rezultatele testelor

| Suită | Rezultat |
|---|---|
| `dotnet build src/KBot.App` | **0 erori**, 7 avertismente — toate `MSB3825` pe `.resx`, PREEXISTENTE (niciun `.resx` atins) |
| `KBot.Api.Tests` | 103 trecute / **1 picat** — `GetDdf_FormatsRevisionLabel_…`, **PREEXISTENT** (verificat prin `git stash` pe HEAD) |
| `KBot.App.Tests` | 227 trecute / **13 picate** — aceleași 13 pe HEAD, verificat prin `git stash`; erau 222 trecute înainte, deci cele 5 noi trec |
| `KBot.Common.Tests` | 85 trecute, 0 picate |
| `KBot.Domain.Tests` | 28 trecute / **3 picate** — familia `DdfInfoTests.EtichetaRevizie_*`, aceeași zonă cu picarea preexistentă din `KBot.Api.Tests` |
| `PYTHON` (`.venv`, suita întreagă) | **509 trecute, 19 sărite, 0 picate** (înainte: 485 / 15) |

Cele trei teste `doar_noi` din `test_forexe_angajamente.py` au rulat **live pe `000_DEMO`**
(nu s-au sărit), deci ramura nouă a rutei e verificată pe bază reală, inclusiv faptul că
`Descriere`/`Stare` ale unui angajament existent rămân neatinse.

---

## Ce a rămas NEVERIFICAT sau amânat

1. **Nimic din felia asta nu s-a văzut pe ecran și nimic din SNM n-a rulat live.** Nu s-a
   deschis nicio sesiune FOREXE, nu s-a descărcat niciun extras, nu s-a apăsat niciun buton.
   Toate verdictele de mai sus sunt de compilare și de test.
2. **`DefaSS` / `DefaSSS` nu există în schema din depozit** (`MariaDB_Schema/000_DEMO.sql`).
   Ele traduc primele 3 caractere ale contului (`21E`, `23A`, `01A`, `82E`) în sursa-sector
   (`02A`, `02E`), iar fără ele nu se poate afla unitatea unui cont. `docs/MAPARE_NOMENCLATOARE.md`
   pomenește `AVACONT_COMUN.DefaSursaSector`, dar perechea `DefaSS`/`DefaSSS` nu apare nicăieri.
   Ruta le citește exact cu interogarea din Access, iar când tabela lipsește pune un
   **avertisment cu numele ei** în răspuns și lasă `IdUnitate` NULL. NU s-a ghicit o mapare
   alternativă. **De confirmat pe baza live** înainte de prima rulare adevărată.
3. ~~**`ClasificatiiV` nu există în schema din depozit** — aceeași tratare, `IdClsfV`
   rămâne NULL cu avertisment.~~ **REZOLVAT 09.09.2026, corecția 8:** tabela există
   acum, sub numele `Clasificatii_Venituri`, și e și migrată. Interogarea lui
   `FX_DicClsfV` s-a portat verbatim, cu `CONCAT_WS` în loc de `CONCAT` (coloanele
   sunt nulabile). Tot nerulată pe date reale.
4. **`gUnitati.IdUnitatePentru(Sursa)` e o DEDUCȚIE.** `clsUnitati` nu e în exportul Access
   (s-au exportat doar modulele). S-a tradus ca `SELECT IdUnitate FROM Unitati WHERE
   SursaSector = %s`, sprijinit pe datele din `FX_Extrase_H.md` (contul `21E…` ▸ IdUnitate 77,
   `01A…` ▸ 76). Consistent, dar **neconfirmat**.
5. **`FX_Extrase_H.IdClsf` primește `Clasificatii.IdClsfAcc`**, pe decizia blocată din STATUS
   («`FX_Indicatori.IdClsf` ține id-ul ACCESS»). Nu s-a verificat pe date reale pentru ACEASTĂ
   tabelă.
6. **`AUTO_INCREMENT` pe cele trei chei primare** (`IDEXF`, `IDEXH`, `IDFXE`) — DDL-ul din
   depozit le arată `int(11) NOT NULL` simplu, dar rutele existente (`FX_Istoric`,
   `FX_Receptii_H`) folosesc `lastrowid`, deci baza live le are auto-increment. Ruta refuză
   ZGOMOTOS un `lastrowid = 0` în loc să lege rândurile copil de zero.
7. **Ruta de import n-a fost rulată niciodată** — testele ei de rută sunt host-only și s-au
   sărit aici (4 sărite). S-au rulat doar funcțiile pure (24 trecute).
8. **`SincronizeazaAsync`** (meniul de opțiuni) folosește în continuare upsert-ul plat, care
   REÎMPROSPĂTEAZĂ `Descriere`/`Stare`. E deliberat: butonul din subsol întreabă «ce e nou»,
   meniul întreabă «adu tot la zi». Dacă operatorul vrea o singură semantică, se decide separat.
9. **Folderul implicit al extraselor e `<AppDir>\Extrase`**, nu `C:\AVACONT\FOREXE\EXTRASE\` ca
   în Access. Motivul: `SetariFoldere.Valideaza` verifică la PORNIRE dreptul de scriere pe
   fiecare folder în care se scrie, iar o cale absolută sub `C:\AVACONT` ar putea opri lansarea
   aplicației pe o mașină care nu are acel drept — pentru o funcție pe care poate n-o folosește.
   Se poate muta din `settings.json`, iar calea folosită se scrie oricum per fișier în
   `FX_Extrase_F.CaleFisier`.
10. **Cele 13 + 1 + 3 picări preexistente** (familia `DdfInfoTests.EtichetaRevizie_*` /
    `DdfXfaParser` / `IstoricView` / `DdfView` / `XfaXmlPreview` /
    `MainFormNavItemsTests.Designer_WroteLiteralDiacritics_NotEscapes`) nu au fost atinse.
    Sunt de dinaintea acestei felii — verificat prin `git stash` pe HEAD. Cine reia zona să
    înceapă cu ele; în special ultima, care păzește chiar regula de diacritice a casei.

---

# CORECȚII 08.09.2026 (aceeași felie, nu una nouă)

Șapte lucruri semnalate de operator după prima rulare pe ecran a feliei. Nu e felie nouă:
toate ating cod scris mai sus sau imediat lângă el.

## 1. `DefaSS` / `DefaSSS` — există, dar erau căutate în baza greșită

Operatorul le-a creat în **`AVACONT_COMUN`**. Ruta le citea NECALIFICAT
(`SELECT SSS, SS FROM DefaSS INNER JOIN DefaSSS …`), iar conexiunea e deschisă pe baza
UNITĂȚII — deci MariaDB răspundea 1146 pe orice bază și importul cădea de fiecare dată pe
ramura «nomenclatorul lipsește»: antetele rămâneau fără unitate și fără clasificație, la
nesfârșit, oricâte tabele s-ar fi creat.

Numele sunt acum calificate cu `AVACONT_COMUN.`, exact ca `DefaClsfF` / `DefaArticol` din
`istoric.py` și `BIC` / `CAI` din `ord_edit.py`. Avertismentul (când chiar lipsesc) numește
acum baza, nu doar tabela.

**`ClasificatiiV` a rămas atunci NECALIFICAT** — nu se spusese pe ce bază stă, iar a o
califica după ureche ar fi fost o ghicire. **Răspunsul a venit a doua zi — vezi
corecția 8 de la coada fișierului: tabela s-a mutat, sub alt nume.**

> Verificarea pe baza vie NU s-a putut face de aici: `PYTHON/config.py` din depozit e mock,
> cu parolele redactate, iar serverul K-BOT refuză conexiunea (1045). Corectura e citită din
> cod și din convenția celorlalte rute, nu confirmată pe date.

## 2. Cele trei chei primare ale extraselor — răspunsul

**Da, transferul din Access continuă să meargă după ce devin `AUTO_INCREMENT`** — dar NU se
schimbă cu mâna, și mai ales nu înaintea migrării.

Ce s-a verificat, punct cu punct:

- **Migratorul scrie id-urile Access VERBATIM.** `TableMaps` duce familia Extrase cu
  `NameMatched`, fără să excludă cheia, deci `IDEXF` / `IDEXH` / `IDFXE` călătoresc ca
  valori explicite.
- **MariaDB acceptă o valoare explicită într-o coloană `AUTO_INCREMENT`** și, la `ALTER`,
  reașază contorul pe `MAX + 1`. Singurul caz în care ar INVENTA o cheie e un id `0`, `NULL`
  sau lipsă.
- **Cazul acela nu poate apărea aici.** `IDEXF` / `IDEXH` / `IDFXE` sunt AutoNumber în
  Access: `mdl_FX_Extrase.Extrase_F_Add` nu atribuie niciodată `rsExF!IDEXF`, ci îl CITEȘTE
  înapoi după `rsExF.Update` (`Extrase_F_Add = rsExF!IDEXF`). Exportul le arată «Long»
  fiindcă un AutoNumber Access CHIAR e Long — `FX_Istoric.ID`, care e deja pe lista
  convertitelor, apare la fel.
- **`ALTER` pe o cheie părinte de FK e deja bătătorit:** `FX_Istoric.ID` e referit de
  `FX_Plati`, `FX_Receptii` și `FX_Rezervari` și se convertește de la prima migrare.

**Ce s-a făcut în loc de o schimbare manuală.** Cele trei perechi au intrat în mașinăria care
există deja pentru asta:

- `AutoIncrementStep.Targets` (KBot.Migrator) — șapte perechi au devenit **zece**. Pasul
  rulează ULTIMUL, doar după un transfer încheiat cu COMMIT, și refuză schema de referință.
  Ordinea (creare din `AVACONT_SURSA` ▸ migrare ▸ verificare ▸ `ALTER`) e chiar garda: pe
  cheia simplă un rând fără id **cade**, pe cea auto ar primi tăcut o cheie inventată.
- `schema_common.EXEMPT_COLUMNS` (PYTHON) — aceleași zece. Fără asta, `schema_sync` ar vedea
  `AUTO_INCREMENT`-ul ca abatere față de `AVACONT_SURSA` și ar genera un `MODIFY` care îl
  scoate înapoi.
- `test_schema_sync_exempt_columns.py` — pinul (lista scrisă a doua oară, dinadins) și
  verificarea încrucișată cu fișierul `.vb`, actualizate. **10 trecute.**

**Motivul pentru care era oricum obligatoriu:** `routes/forexe/extrase.py` leagă rândurile
copil prin `cursor.lastrowid`, iar pe o cheie INT simplă acela e `0`. Ruta refuză zgomotos un
`0` în loc să lege rândurile de zero — deci pe o bază migrată importul de extrase **nu putea
rula deloc** înainte de conversia asta.

## 3. Pachet gol ▸ nu se mai atinge serverul

`MainForm.DuLaIngestieAsync` numără acum RÂNDURILE pachetului înainte de orice cerere și, la
zero, spune atât operatorului și se oprește. Se numără rândurile, nu tabelele: workflow-ul
întoarce cele cinci tabele și când sunt toate goale, deci `Tabele.Count` ar fi 5 pentru un
pachet fără nimic în el (aceeași socoteală ca `total` din `DownloadNodeAsync`).

## 4. Orice mesaj arătat operatorului intră și în jurnal

Regulă nouă a casei, cerută de operator. Nu s-a rezolvat cu o linie de `Write` lipită lângă
fiecare casetă — primul apel nou scris fără ea ar fi rupt regula în tăcere — ci făcând din
cele două lucruri UNUL:

- **`KBot.Common\Logging\OperatorLog.vb`** (nou) — scriitorul:
  `<AppDir>\Logs\mesaje_operator.log`, o intrare pe linie, sink terminal ca `GlobalErrorLog`
  (dacă nu se poate scrie, mesajul pleacă pe `Trace` — o casetă nu are voie să pice fiindcă
  n-a mers jurnalul).
- **`KBot.Common\Logging\Parsers\OperatorLogParser.vb`** (nou) + înregistrarea în
  `LogFileLoader` — fișierul se citește în vizualizatorul de jurnale, cu nivel și sursă.
  Analizorul stă ÎNAINTEA lui `AdobeHostParser`: amândouă încep cu același marcaj de timp
  urmat de două spații, iar cel permisiv ar fi înghițit liniile celuilalt.
- **`KBot.Theming\KBotMessage.vb`** (nou) — poarta unică. Se apelează exact ca
  `MessageBox.Show` / `MsgBox` și întoarce același lucru. Sursa (`NumeFișier.Metodă`) NU se
  scrie la apel: vine din `<CallerFilePath>` + `<CallerMemberName>`, deci e compusă la
  COMPILARE — nu se poate greși, nu costă nimic la rulare și nu depinde de o stivă care poate
  fi optimizată.
  În KBot.Theming fiindcă e stratul WinForms cel mai de jos: îl au deja toate proiectele care
  arată casete, deci regula intră fără nicio referință nouă și fără ciclu. Nu e un control.
- **Măturare: 34 de fișiere, toate cele ~190 de apeluri** din `src/` (fără `_reference/`).
  `KBot.App`, `KBot.Forexe`, `KBot.Migrator` și `KBot.DevHarness` au primit
  `<Import Include="KBot.Theming" />` la nivel de proiect, ca regula să fie disponibilă în
  ORICE fișier fără să depindă de lista de `Imports` a fiecăruia.

Trei lucruri ieșite la iveală în măturare:

1. **`KBotMessage` nu are supraîncărcare cu UN singur argument.** Ar fi ambiguă cu
   `Show(text, caption)` — parametrii de apelant sunt tot `String` și opționali. Regula VB
   care ar fi decis e prea subțire ca să sprijini pe ea fiecare casetă din aplicație.
2. **Două casete de depanare uitate în cod**, `AdvancedTreeControl.Popup.vb`:
   `MessageBox.Show("OnActivated fired")` și `("OnGotFocus fired")` — pe activarea și pe
   focusul popup-ului de tooltip al arborelui. Nu sunt mesaje pentru operator; **s-au șters**.
3. **O singură casetă NU trece prin poartă**, și e scris de ce chiar acolo:
   `WorkflowExecutor.Browser.vb` (promptul UAC pentru politica de certificate) folosește
   `MessageBox` din **WPF** (`Imports System.Windows`, proiectul are `UseWPF`), cu alte tipuri
   de argumente. KBot.Theming n-are WPF și n-are de ce să capete doar ca să acopere un apel.
   Regula e respectată cu mâna: `OperatorLog.Write` chiar înaintea afișării.

Efect colateral util: `LoginForm` chema `Write(...)` NECALIFICAT (mergea fiindcă
`GlobalErrorLog` era singurul modul cu `Write`). Cele două apeluri s-au calificat
— `GlobalErrorLog.Write` —, ca în restul soluției.

## 5 + 6. Variabilele executorului se adunau peste rulări — UN singur defect, două simptome

**Ce s-a văzut.** La reîmprospătarea unui angajament, pachetul pleca spre server cu scalarii
ca LISTE: `CodAngajament` = `["AAB3TTGSA3T","AAB4SN5DKFN","AAB4SN5DKFN"]` — primul cod fiind
al unui angajament descărcat cu totul altă dată. Serverul respingea, corect,
`DataAngajament` cu «Data invalidă (așteptat zz.ll.aaaa)». În același pachet apărea și
`ListaAngajamente` cu 39 de rânduri, rămasă de la o apăsare pe iconița de listă.

**De ce.** `WorkflowExecutor.SetVariable` **ADAUGĂ** într-o listă per nume, iar
`GetAllVariables` împachetează o listă cu mai multe valori într-un `JArray`. Sesiunea FOREXE
ține UN SINGUR executor pentru toate lucrările ei, iar `_variables` nu se golea niciodată:
`ExecuteAsync` reseta doar `_exitMessage` (felia 0057), nu și memoria. Trei apăsări = trei
valori. `PopulateResult` citește exact același dicționar, de-aia și tabelul altei lucrări
călătorea mai departe.

`KBOT_IPC` făcea curat — `ResetVariables()` după fiecare flux și `ClearAllVariables()` în
`Finally` (`_reference/KBOT_IPC.WorkFlow.vb:116,363`) — și golirea s-a pierdut la portare.

**Ce s-a făcut.** `ForexeRunner.RunJobAsync` cheamă `_executor.ClearAllVariables()` înaintea
fiecărei lucrări. Două alegeri, scrise și în cod:

- **ÎNAINTE, nu după:** o rulare care a murit cu excepție sau a fost anulată n-apucă să curețe
  după ea, iar lucrarea următoare tot cu memoria goală trebuie să pornească.
- **În runner, nu în `ExecuteAsync`:** parametrii JSON ai lucrării intră tot prin
  `SetVariable`, chiar înaintea execuției — o golire din executor i-ar șterge pe cei tocmai
  puși.

`RunAsync` (Conectare) nu are nevoie: își construiește executorul de la zero.

## 7. Un angajament selectat își arată datele chiar dacă n-a fost descărcat

Un angajament adus doar de iconița din dreapta subsolului are antet și atât — deci n-are
indicatori, deci `GET /api/forexe/sumar` nu întoarce antet, deci `SumarView` rămânea complet
goală: nici măcar codul pe care tocmai îl selectase operatorul.

`SumarView.SetContext` umple acum antetul din RÂNDUL DE ARBORE înainte de orice apel de
rețea — cod, descriere, stare, `DataCreare`, `DataDefinitivare`, încărcat/preluat, toate deja
pe `AngajamentTreeInfo` —, iar `LoadAsync` nu mai golește antetul nici când serverul n-are
nimic, nici pe calea de eroare: angajamentul selectat e tot acela.

Nimic nu se inventează: **`DataFX` e singurul câmp de antet pe care arborele nu-l are** și
rămâne gol până răspunde serverul. Starea goală a grilei spune acum «Angajamentul nu are
indicatori. Se arată doar datele din listă.»

## Fișiere atinse (corecții)

**Nou:** `src/KBot.Common/Logging/OperatorLog.vb`,
`src/KBot.Common/Logging/Parsers/OperatorLogParser.vb`, `src/KBot.Theming/KBotMessage.vb`

**Modificat:** `PYTHON/routes/forexe/extrase.py`,
`PYTHON/routes/schema_sync/schema_common.py`,
`PYTHON/tests/test_schema_sync_exempt_columns.py`,
`src/KBot.Migrator/MariaDb/AutoIncrementStep.vb`,
`src/KBot.Forexe/ForexeRunner.vb`, `src/KBot.App/KBOT.vb`,
`src/KBot.App/Views/SumarView.vb`, `src/KBot.App/LoginForm.vb`,
`src/KBot.Common/Logging/LogFileLoader.vb`,
`src/KBot.Controls/Tree/AdvancedTreeControl.Popup.vb`,
`src/KBot.Forexe/Executor/WorkflowExecutor.Browser.vb`,
plus cele 34 de fișiere ale măturării și cele patru `.vbproj` care au primit importul.

**FileVersion:** `KBot.Theming` 1.12→1.13, `KBot.Controls` 1.47→1.48, `KBot.Migrator`
1.7→1.8, `KBot.DevHarness` 1.0.26→1.0.27. `KBot.Common`, `KBot.Forexe`, `KBot.Api` și
`KBot.App` erau deja urcate în ciclul acesta.

## Rezultate

| Verificare | Rezultat |
|---|---|
| `dotnet build KBot.sln` | **0 erori**, 7 avertismente — toate `MSB3825` pe `.resx`, PREEXISTENTE (niciun `.resx` atins) |
| `test_schema_sync_exempt_columns.py` | **10 trecute** (pinul de zece perechi + verificarea încrucișată cu `.vb`) |
| Formatul `mesaje_operator.log` vs. analizorul lui | verificat pe trei linii-probă (cu titlu, fără titlu, multi-rând pliat) |

## Ce a rămas NEVERIFICAT

1. **Nimic nu s-a văzut pe ecran și nicio sesiune FOREXE n-a fost deschisă.** Verdictele sunt
   de compilare.
2. **Suita de teste NU s-a rulat** (cerut explicit). Fișierele de teste care ating zonele
   atinse aici au fost CITITE: niciunul nu fixează textul stării goale din `SumarView` și
   niciunul nu cheamă `MessageBox` direct. Rămâne totuși de rulat.
3. **Corectura de la punctul 1 nu s-a putut proba pe baza vie** — vezi nota de acolo.
4. ~~**`ClasificatiiV`** rămâne întrebarea deschisă a feliei.~~ **ÎNCHIS 09.09.2026** — vezi corecția 8 de mai jos.

---

## 8. `ClasificatiiV` ▸ `Clasificatii_Venituri` — tabela traversează, cu tot cu rectificări (09.09.2026)

Întrebarea deschisă a feliei era „pe ce bază stă `ClasificatiiV`". Răspunsul a venit în
trei trepte, ultima fiind cea care contează:

1. «ClasificatiiV is access only … now they reside in Clasificatii per database»;
2. «IdClsfV should never be written by python/vb.net!!! it's an access field ONLY!!!»;
3. **«actually i was wrong … IdClsfV is still needed. Each DB has now a
   Clasificatii_Venituri table which must be also ported»** — plus `RectificariV`, devenit
   `Clasificatii_Venituri_Rectificari`, și schema nouă în `sql/AVACONT_SURSA.sql`.

Ce e scris acum în cod e treapta 3. Treptele 1 și 2 nu au lăsat nimic în urmă.

### Ținta

```sql
CREATE TABLE `Clasificatii_Venituri`  (
  `IdClsfV`    int(11) UNSIGNED NOT NULL,   -- vezi mai jos: a intrat AUTO_INCREMENT
  `Capitol`    varchar(255) NULL,
  `SubCapitol` varchar(255) NULL,
  `Paragraf`   varchar(255) NULL,
  `Denumire`   varchar(255) NULL,
  `Trim1` .. `Trim4` int NULL DEFAULT 0,
  PRIMARY KEY (`IdClsfV`)
);
```

**Ținta e 1=1 cu tabela Access** (operatorul), deci interogarea lui `FX_DicClsfV` se
poartă verbatim și nu se reinventează nicio cheie. `Clasificatii_Venituri_Rectificari`
atârnă de ea printr-o cheie străină reală pe `IdClsfV`, și e la rândul ei 1=1 cu
`RectificariV`.

### Ruta (`extrase.py`)

`clsf_venituri_pentru` e la loc, acum pe `Clasificatii_Venituri`, iar `IdClsfV` e din nou
în `_H_INSERT_SQL`. Trei lucruri de reținut:

* **Fără filtru pe unitate.** Tabela n-are `IdUnitate` nici în Access, nici pe MariaDB:
  ține clasificațiile de venituri ale întregii DIRECȚII. E singurul nomenclator din set
  căutat așa, și e o proprietate a lui, nu o scăpare. (`FX_DicClsfV` chiar asta face —
  fără predicat, spre deosebire de `FX_DicClsf`.)
* **Necalificat**, spre deosebire de `DefaSS`/`DefaSSS`: e o tabelă PE BAZĂ, nu una comună.
* **`CONCAT_WS('', Capitol, SubCapitol, Paragraf)`, nu `CONCAT(...)`.** Cele trei coloane
  sunt nulabile, iar `CONCAT` întoarce NULL dacă orice argument e NULL — pe când `&` din
  Access trata Null ca șir gol. `CONCAT_WS` sare peste NULL-uri, deci se poartă ca `&`.
  Cu `CONCAT` simplu, orice rând cu un `Paragraf` gol ar fi dispărut tăcut din dicționar.

Paza pe 1146 rămâne, cu motiv nou: tabela e în `AVACONT_SURSA` abia din 09.09.2026, deci
lipsește din orice bază nesincronizată de atunci, iar avertismentul spune exact asta.

### Migratorul — două hărți noi

`ClasificatiiV` și `RectificariV` **au ieșit din `TableMaps.Excluded`**. Decizia D5 din
`MAPARE_NOMENCLATOARE.md` le parcase ca „nu în felia asta"; acum au țintă, deci motivul a
dispărut. `ParteneriSI` rămâne afară.

**Cheia Access călătorește NESCHIMBATĂ, și asta e toată diferența față de `Clasificatii`.**
Acolo, Regula 1 redenumește `IDClsf` ▸ `IdClsfAcc` și lasă MariaDB să-și pună propriul
`IDClsf`, fiindcă ținta le ține pe amândouă. Aici nu există a doua coloană în care să
parchezi id-ul Access, și **două lucruri arată deja spre el**: `FX_Extrase_H.IdClsfV`
(scris și de `Extrase_H_Add`, și de rută) și cheia străină a rectificărilor. Reașezarea
cheii le-ar rupe pe amândouă în tăcere.

#### Cheia a intrat `AUTO_INCREMENT` și a fost făcută simplă

Tabela a sosit în `AVACONT_SURSA` cu `IdClsfV` declarat `AUTO_INCREMENT`. Semnalat, fiindcă
asta sărea peste paza pe care celelalte zece chei o au: ele stau `INT` simplu în schema de
referință **tocmai ca** un id lipsă, NULL sau 0 să CRAPE la migrare în loc să primească
tăcut o cheie inventată (planul 3.1) — iar aici cheia inventată ar fi fost exact cea spre
care arată `FX_Extrase_H.IdClsfV`.

**Operatorul a făcut-o `INT UNSIGNED NOT NULL` pe server în aceeași zi.** Deci perechea a
intrat pe amândouă listele, care descriu o singură decizie și se schimbă împreună:

* `AutoIncrementStep.Targets` — 10 ▸ **11** perechi;
* `schema_sync.EXEMPT_COLUMNS` — 10 ▸ **11**, altfel sincronizarea ar vedea
  `AUTO_INCREMENT`-ul de după migrare ca abatere și ar genera un `MODIFY` care îl scoate;
* testul-piron `test_schema_sync_exempt_columns.py` — `test_exactly_ten_pairs` ▸
  `test_exactly_eleven_pairs`, plus lista `EXPECTED` și pinul care citește fișierul `.vb`.

Ordinea rămâne cea de întotdeauna: **creare din `AVACONT_SURSA` ▸ migrare ▸ verificare ▸
`ALTER`**. `ClasificatiiV.IdClsfV` e AutoNumber în Access, deci nu e niciodată 0 sau NULL,
iar `ALTER`-ul reașază contorul pe MAX+1 — argumentul de la corecția 2, neschimbat.

> ⚠ **`sql/AVACONT_SURSA.sql` din depozit arată încă `AUTO_INCREMENT` pe coloana aia.**
> Serverul a fost schimbat, dump-ul local nu. Nu l-am editat: e un dump, autoritatea lui e
> serverul, iar o retușare de mână l-ar face să pară regenerat când nu e. `AutoIncrementStep`
> nu citește niciunul din cele două fișiere — întreabă baza VIE și raportează
> «era deja AUTO_INCREMENT» când e cazul — deci codul e corect față de ambele stări.

Bucla nomenclatoarelor rulează un pas pe fișier de unitate, iar tabela n-are `IdUnitate` —
deci fișierul care are rânduri le scrie, iar celelalte găsesc tabela goală. Operatorul:
**un singur fișier Access are date acolo**. Upsert-ul face repetiția inofensivă, iar
`PRIMARY KEY (IdClsfV)` e cheie unică adevărată, deci — spre deosebire de `Clasificatii`
(D8) — harta asta **nu are nevoie de `InsertOnly`**.

La rectificări, **`ID`-ul Access NU e exclus** — și aici e o diferență față de sora ei,
`Clasificatii_Rectificari`, care îl aruncă. Aceea se sprijină pe `UNIQUE (IdClsf, Data,
Document)` ca să rămână idempotentă; tabela asta n-are așa ceva — `PRIMARY KEY (ID)` e
singurul index unic — deci aruncarea id-ului Access ar face MariaDB să bată unul nou la
FIECARE rulare și ar dubla toate rectificările. Purtat, upsert-ul are pe ce se potrivi.
Coloana rămâne `AUTO_INCREMENT` și n-are nevoie de nimic la final: InnoDB își împinge
contorul dincolo de o valoare explicită, deci nu e țintă pentru `AutoIncrementStep`.

**`DTQ` călătorește** — pe `Clasificatii_Rectificari` nu o face doar fiindcă ținta n-are
coloana; aici o are. E `DEFAULT NOW()` pe server, dar un rând migrat poartă valoarea din
Access: un `DEFAULT` se aplică doar coloanei pe care `INSERT`-ul o omite, iar acesta o
numește.

Fiindcă schema e 1=1 cu Access, **niciuna dintre cele două hărți nu mai are excluderi** —
`DTQ`/`Esinc` pe care le pusesem la prima scriere erau presupuneri despre coloane Access
care, în forma 1=1, nu există.

### Ce NU s-a putut verifica

**Coloanele Access ale lui `RectificariV` n-au fost citite niciodată** — tabela nu e în
`C:\AVACONT\FX_System_Export`, care exportă doar familia `FX_*`. „1=1 cu Access" e spusă de
operator, nu văzută de mine; maparea e o potrivire pe nume, iar jurnalul planului
(`LogPlan`) va spune la prima rulare ce s-a potrivit și ce s-a sărit. Nu s-a ghicit
nicio coloană.

Nimic nu s-a rulat pe date: niciun `.accdb` deschis, nicio migrare pornită, nicio bază vie
atinsă (`config.py` din depozit e mock). `MariaDB_Schema/000_DEMO.sql` e dump-ul vechi și
nu are cele două tabele — bazele migrate înainte de 09.09.2026 au nevoie de o sincronizare
cu `AVACONT_SURSA` înainte ca ruta să găsească `Clasificatii_Venituri`.

### Fișiere atinse

| Fișier | Ce |
|---|---|
| `PYTHON/routes/forexe/extrase.py` | `clsf_venituri_pentru` pe `Clasificatii_Venituri`, cu `CONCAT_WS`; `IdClsfV` înapoi în `_H_INSERT_SQL` și în `_scrie_antet`; antetul fișierului și comentariul lui `_ER_NO_SUCH_TABLE` |
| `src/KBot.Migrator/Transfer/TableMaps.vb` | `ClasificatiiV`/`RectificariV` scoase din `Excluded`; două hărți noi în `Nomenclators()`, ambele potrivire pură pe nume |
| `src/KBot.Migrator/MariaDb/AutoIncrementStep.vb` | `Clasificatii_Venituri.IdClsfV` — a unsprezecea pereche |
| `PYTHON/routes/schema_sync/schema_common.py` | aceeași pereche în `EXEMPT_COLUMNS` (10 ▸ 11) |
| `PYTHON/tests/test_schema_sync_exempt_columns.py` | pironul: `EXPECTED`, numărul, și pinul care citește `.vb` |
| `src/KBot.Domain/ImportExtraseRezultat.vb` | comentariul numește acum și `Clasificatii_Venituri` printre nomenclatoarele care pot lipsi |
| `tests/KBot.Api.Tests/ExtraseApiClientTests.vb` | textul de avertisment din răspunsul fals folosea numele vechi |

### Rezultate

`dotnet build KBot.sln` — **0 erori, 0 avertismente**. `KBot.Api.Tests`, filtrat pe
`Extrase` — 8 trecute. `test_forexe_extrase.py` — 24 trecute, 4 sărite.
`test_schema_sync_exempt_columns.py` — 10 trecute, inclusiv pinul care citește
`AutoIncrementStep.vb` și cere ca cele două liste să spună același lucru.
