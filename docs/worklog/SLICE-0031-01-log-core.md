# SLICE-0031-01 — Nucleul de jurnale: căi, rotație, citire, analiză, filtrare

Prima trecere din felia 0031 (vizualizator de jurnale), după `docs/PLAN_LogViewer.md` versiunea 2.
Trecerea asta e **fără UI**: tot ce se scrie aici stă în `KBot.Common` (net8.0, fără WinForms,
fără Drawing) plus cablarea celor patru scriitori existenți.

**Poarta de intrare a planului a trecut:** `KBOT_STATUS.md` spunea, înainte de această felie,
«Next free slice number: **0031**» — exact numărul atribuit de operator. Nu a fost nevoie de oprire.

---

## Ce s-a schimbat și de ce

### 1. Un singur loc care spune unde stau jurnalele (`LogPaths`)

`GlobalErrorLog` și `AdobeHostLog` compuneau FIECARE, prin cod duplicat, aceeași cale
(`AppContext.BaseDirectory` + `Logs`). `TreeLogger` compunea alta — lângă executabil, **în afara**
folderului `Logs\`. Un vizualizator are nevoie de un singur director de citit, deci calea se
calculează acum într-un singur loc. Pentru primii doi scriitori calea rezultată e **identică** cu
cea de dinainte; s-a verificat în fișier înainte de editare, cum cere planul.

### 2. Rotația păstrează istoric (`LogRotation`)

10 MB, cinci generații, exact politica pe care serverul o are deja prin `RotatingFileHandler` din
`utils/logger.py` (verificat: `maxBytes=10*1024*1024, backupCount=5`) — o singură regulă peste
ambele jumătăți ale sistemului. Doar redenumiri, niciodată citire-și-rescriere, deci costul nu
depinde de mărimea fișierului. Consum maxim mărginit: 60 MB pe familie.

Se cheamă **înainte** de fiecare adăugare, în cei patru scriitori — nu în vizualizator: un
vizualizator ar aplica limita doar când îl deschide cineva, adică exact când nu mai contează.

`Roll` **nu aruncă niciodată**. Orice eșec (fișier blocat, drepturi, disc plin) pleacă pe `Trace` și
întoarce `False`, ca linia care a declanșat verificarea să se scrie oricum. O problemă de rotație nu
are voie să coste linia care a provocat-o. Fixat cu test pe fișier blocat exclusiv.

Două refuzuri explicite, ambele testate: `backupCount < 1` și `maxBytes <= 0` **nu ating nimic**.
Un parametru greșit nu are voie să devină o distrugere de istoric.

### 3. `TreeLogger`: mutat în `Logs\`, și cele două înghițiri devin sinkuri terminale

Fișierul trece de la directorul executabilului la `LogPaths.EnsureLogsDirectory()`. Numele
(`log_{treeId}.txt`) și parametrul opțional de cale rămân neschimbate.

Cele două `Catch`-uri goale (`Write`: «Eșec silențios la scriere»; `Init`: rezerva pe Temp) scriu
acum motivul pe `Trace`. **Nu pot rearunca** — s-a verificat în cod, nu presupus:
`AdvancedTreeControl` le cheamă din căi de desenare (`TooltipPopup.OnPaint`,
`AdvancedTreeControl.Painting.DrawContent`), iar o excepție ieșită dintr-o scriere de jurnal în
`OnPaint` omoară procesul. Deci același tratament ca `GlobalErrorLog` și `AdobeHostLog`:
`Trace.WriteLine`, fără rearuncare. Diferența față de starea de dinainte nu e mică — înghițirea
tăcută nu lăsa **nicio** urmă.

### 4. Citire, analiză, filtrare (`KBot.Common\Logging\`)

* `LogFileReader` — `FileShare.ReadWrite` (obligatoriu: `RunLogger` își ține fișierul deschis cu
  `AutoFlush` cât rulează bancul; e cazul obișnuit, nu unul marginal), ultimii 5 MB prin `Seek`,
  prima linie parțială aruncată, BOM sărit.
* Șase analizoare în spatele lui `ILogEntryParser`, fiecare scris **după scriitorul real**, nu după
  tabelul planului.
* `LogFileLoader` — ghicește analizorul după numele fișierului, **verifică ghicirea**, coase liniile
  de continuare în blocuri, moștenește marcajele lipsă și raportează câte au moștenit și câte au
  rămas fără dată.
* `LogFilter` — clasă pură, aceeași formă ca `IstoricFilter` (citit înainte de scriere): patru axe
  în ȘI, mulțime goală = nimic, intervalul comparat pe marcajul **corectat**, iar excluderile fără
  dată se **numără** și se întorc.
* `ServerClock` — decalajul față de ceasul serverului, cu regula din §6.6: se aplică **doar**
  liniilor care nu poartă decalaj propriu.

---

## Abateri de la plan, cu motiv

Niciuna tăcută. Toate au ieșit din citirea fișierului real, cum cere §0.

### A. «Exact trei sinkuri terminale» nu există în `KBOT_STATUS.md`

Planul §1.4 cere corectarea, în STATUS, a unei linii care ar spune că sinkurile terminale
ne-rearuncătoare sunt **exact trei**. Linia aceea **nu e în `KBOT_STATUS.md`** — s-a căutat în tot
fișierul și în tot `docs/`. Singura formulare a regulii stă în `CLAUDE.md:43` și nu numără nimic:
«`GlobalErrorLog.Write` is itself a terminal sink (never throws) — do not wrap calls to it.»

Deci nu era ce corecta în STATUS. Nu s-a inventat o linie ca să fie apoi «corectată». Situația
reală, scrisă aici și în STATUS: sinkurile terminale ne-rearuncătoare din soluție sunt acum
**cinci** — `GlobalErrorLog.Write`, `AdobeHostLog.Write`, `HandleUiError` (scrierea în runlog),
`TreeLogger.Write`, `TreeLogger.Init` — ultimele două adăugate de felia asta, documentate mai sus.

### B. Proba de potrivire a analizorului: pragul de 30% nu se aplică formatelor pe blocuri

§5.5 cere ca analizorul ghicit să fie trecut peste primele 50 de linii neagole și, sub **30%**
anteturi recunoscute, ghicirea să fie declarată greșită.

Regula asta, aplicată literal, **strică exact fișierul care contează cel mai mult în felia asta**.
Un `harness_errors.log` perfect sănătos e un antet urmat de zeci de linii de stivă: un bloc cu 20 de
cadre dă 1/21 ≈ **4,8%** anteturi și ar fi declarat mereu «ghicire greșită», căzând pe alegerea per
linie — unde `RunLogParser` ar rupe blocul, fiindcă prima linie a lui `ex.ToString()`
(`System.InvalidOperationException: …`) nu e indentată și ar deveni intrare separată. Adică fix
opusul testului 2 din plan, care cere ca antetul plus cinci linii de stivă să dea **o singură**
intrare.

Corecția: `ILogEntryParser` capătă `ExpectsHeaderOnEveryLine`. Pragul procentual se aplică doar
formatelor cu o intrare pe linie; pentru formatele pe blocuri proba cere **măcar un** antet
recunoscut. Asta prinde tot ce trebuia să prindă (testul 5 — conținut adobe într-un fișier numit
`harness_errors.log` — trece: zero anteturi recunoscute) fără să declare greșit un fișier normal.
Ambele cazuri sunt fixate cu test, al doilea ca regresie explicită.

### C. Rotația în constructorul lui `RunLogger` e, azi, cod fără efect

Planul cere cablarea în constructor. S-a făcut. Dar ce face de fapt merită spus, nu ascuns:
apelantul de azi (`DevHarnessForm.RunTestsAsync`) compune un nume **unic pe rulare**, cu
milisecunde, și deschide cu `append:=False`. Fișierul nu există încă la momentul apelului, deci
`Roll` întoarce `False` fără să facă nimic — **de fiecare dată**.

Nu s-a mutat pe cont propriu în `WriteLine` (ar fi o abatere tăcută, și ar costa un `FileInfo.Length`
per linie). Rămâne în constructor fiindcă `RunLogger` primește o cale **arbitrară**: un apelant
viitor care dă o cale fixă chiar e protejat. Pentru scenariul din plan — «o buclă scăpată de sub
control într-un test» — garda asta **nu ajută**, fiindcă bucla scrie în fișierul deja deschis.
Dacă operatorul vrea acoperirea aceea, e o decizie separată, cu un cost pe linie.

### D. `TreeLogger` nu scrie nimic în aplicația construită

`TreeLogger.Init` e apelat dintr-un **singur** loc în tot depozitul: `Surse/SURSA_TREE/Tree.vb:80`.
`Surse/` nu e în `KBot.sln` (verificat) și, după `CLAUDE.md`, e o captură de sursă importată, nu cod
construit. În solutia construită nimeni nu cheamă `Init`, deci `_initialized` rămâne `False` și
`Write` iese pe prima linie.

Cu alte cuvinte: fișierele `log_{treeId}.txt` din tabelul §3 al planului **nu există azi** în
aplicația livrată. Mutarea în `Logs\`, rotația și sinkurile terminale sunt corecte și ieftine, dar
pân' când cineva cablează `Init`, sunt cod pregătit, nu cod care rulează. Analizorul e testat pe
formatul real oricum — dacă `Init` se cablează vreodată, e gata.

### E. Fișierul de rulare al bancului nu poartă marcaj de timp pe linie

Tabelul §3 descrie fișierul ca «antet, linii per test, `EROARE […]`, sumar», fără să spună că
**niciuna** dintre liniile per test nu are marcaj de timp. Singura dată din tot fișierul e pe linia
`Data      : yyyy-MM-dd HH:mm:ss`. De asta `RunLogParser` analizează linia aceea ca marcaj: prin
regula de moștenire din §5.5, toată rularea capătă o dată. Fără asta, o rulare întreagă ar fi
dispărut sub orice filtru de interval. Fixat cu test.

### F. Nivelurile pentru verdictele de test — decise după citirea sursei, nu ghicite

Planul cere să nu se ghicească. Marcajele reale (`AppendVerdict`, `HandleUiError`):
`[PASSED]` → `Info`; `[SKIPPED]` → `Warn` (nu e eroare, dar un test care **nu** a rulat nu e o
reușită); `[FAILED]` și `[ERROR]` → `Error`; `EROARE [sursă]:` → `Error`.

### G. `InternalsVisibleTo` pentru ansamblul de teste

`LogEntry` își ține mutatoarele `Friend` (în afara ansamblului o intrare e imuabilă; singurul care o
modifică e `LogFileLoader`). Testele trebuie totuși să compună intrări de probă, deci
`KBot.Common` expune membrii `Friend` **doar** către `KBot.Common.Tests`.

---

## Fișiere atinse

**Noi — `src/KBot.Common/Logging/`:**

| Fișier | Rol |
|---|---|
| `LogPaths.vb` | unde stau jurnalele, într-un singur loc |
| `LogRotation.vb` | 10 MB × 5 generații, doar redenumiri, nu aruncă niciodată |
| `KBotLogLevel.vb` | `KBotLogLevel` + `LogOrigin` |
| `LogEntry.vb` | intrarea = un BLOC, imuabilă în afara ansamblului |
| `LogFileReader.vb` | coadă de 5 MB, `FileShare.ReadWrite`, BOM, linie parțială |
| `ILogEntryParser.vb` | interfața + `ExpectsHeaderOnEveryLine` (abaterea B) |
| `LogFileLoader.vb` | alegerea analizorului + proba + continuare + moștenire |
| `LogFilter.vb` | filtru pur, patru axe, numără excluderile fără dată |
| `ServerClock.vb` | decalajul față de ceasul serverului |
| `AssemblyInfo.Logging.vb` | `InternalsVisibleTo("KBot.Common.Tests")` |
| `Parsers/HarnessErrorParser.vb` | `==== ts  [sursă] ====` + stiva ca o intrare |
| `Parsers/AdobeHostParser.vb` | `ts  mesaj` |
| `Parsers/TreeLoggerParser.vb` | `[oră] [durată] [NIVEL] [sursă] mesaj`, data din fișier |
| `Parsers/RunLogParser.vb` | marcajele reale din `DevHarnessForm` |
| `Parsers/ApiServerParser.vb` | forma ISO nouă **și** cea veche |
| `Parsers/FallbackParser.vb` | plasa de siguranță; nu eșuează niciodată |

**Modificate (cablarea scriitorilor):**

| Fișier | Ce |
|---|---|
| `src/KBot.Common/GlobalErrorLog.vb` | `LogPaths` + `LogRotation.Roll` înainte de adăugare; const `FileNameOnly` |
| `src/KBot.Controls/Adobe/AdobeHostLog.vb` | idem |
| `src/KBot.Controls/Tree/TreeLogger.vb` | mutat în `Logs\`, rotație, cele două sinkuri terminale |
| `src/KBot.DevHarness/RunLogger.vb` | `Imports KBot.Common` + `Roll` în constructor (vezi abaterea C) |

**Teste noi — `tests/KBot.Common.Tests/`:** `LogParserTests.vb`, `LogRotationTests.vb`,
`LogFilterTests.vb`.

---

## Rezultatele testelor

Numere **dintr-o rulare reală**, nu purtate din plan sau dintr-un worklog vechi.

`dotnet build KBot.sln`: **Debug** 0 erori / 4 avertismente; **Release** 0 erori / 0 avertismente.
Cele 4 avertismente din Debug sunt `MSB3825` (BinaryFormatter) în `KBot.App`, pe
`DdfView.resx`, `PlatiView.resx`, `ReceptiiView.resx`, `RezervariView.resx` — fișiere **neatinse** de
felia asta. Preexistente, **zero avertismente noi**.

`dotnet test KBot.sln -c Debug`, suita completă:

| Proiect | Înainte | După |
|---|---|---|
| KBot.Common.Tests | 14 / 14 | **58 / 58** |
| KBot.Controls.Tests | 814 / 814 | 814 / 814 |
| KBot.DevHarness.Tests | 170 / 170 | 170 / 170 |
| KBot.Theming.Tests | 71 / 71 | 71 / 71 |
| KBot.Xfa.Tests | 39 / 39 | 39 / 39 |
| KBot.LocalStore.Tests | 1 / 1 | 1 / 1 |
| KBot.Api.Tests | 67 / 68 (**1 picat**) | 67 / 68 (**1 picat**) |
| KBot.Domain.Tests | 14 / 17 (**3 picate**) | 14 / 17 (**3 picate**) |
| KBot.App.Tests | 149 / 156 (**7 picate**) | 149 / 156 (**7 picate**) |
| **Total** | **1339 / 1350** | **1383 / 1394** |

**+44 teste noi, toate verzi.**

### Suita NU era verde înainte de felia asta — 11 teste picate, preexistente

§12.2 din plan cere «suita completă verde». **Nu se poate raporta asta cinstit**, fiindcă suita
pica deja la 11 teste înainte de prima linie scrisă aici. Mulțimea numelor picate e **identică**
înainte și după (comparată cu `diff`, ieșire goală): nu s-a stricat și nu s-a reparat nimic.

Toate 11 sunt în zone fără legătură cu jurnalele — etichete de revizie DDF și vederea Istoric:

* `KBot.Domain.Tests.DdfInfoTests` — `EtichetaRevizie_PadsWithSpaces_NotZeroes`,
  `EtichetaRevizie_HandlesTwoAndThreeDigits`, `EtichetaRevizie_MissingDate_LeavesDatePartEmpty`
* `KBot.Api.Tests.ApiClientTests.GetDdf_FormatsRevisionLabel_WithSpacePadding_Not_Zeroes`
* `KBot.App.Tests.DdfViewTests` — `Leaf_CaptionPadsRevisionNumberWithSpaces`,
  `AdobeSettings_LiveOnTheDocumentPage_NotOnValues`
* `KBot.App.Tests.IstoricViewTests` — `Tree_HasTwoLevels_MonthsThenDays`,
  `GridColumns_FiveNoValueColumns_NoTotals`,
  `DetailPane_FollowsSelection_DescriereAndNonZeroValues`,
  `NewContext_RebuildsTheTree_AndAnEmptyResponseClearsIt`
* `KBot.App.Tests.MainFormNavItemsTests.Designer_WroteLiteralDiacritics_NotEscapes`

Grupul DDF arată ca o singură cauză (eticheta de revizie completată cu spații vs. zerouri, care se
vede în Domain, Api și App deodată). **Nu s-a atins nimic din ele** — sunt în afara feliei, iar
repararea lor pe furiș ar ascunde o regresie a altcuiva. Intră la fire deschise în STATUS.

---

## Ce a rămas neverificat sau amânat

1. **Nimic din felia asta nu a fost văzut pe ecran** — nu există UI în trecerea asta. Verdictul
   vizual vine abia la 0031-04.
2. **`TreeLogger` nu rulează în aplicația construită** (abaterea D). Mutarea în `Logs\` e corectă,
   dar neobservabilă până când cineva cablează `Init`.
3. **Rotația din `RunLogger` nu are efect azi** (abaterea C).
4. **Căutarea nu pliază diacriticele** — «sters» nu găsește «șters». Limitare din plan, păstrată
   deliberat și **fixată cu test în ambele sensuri**, ca să nu fie «reparată» din greșeală.
5. **Ora fără dată a lui `TreeLogger`**: data unei intrări e dedusă din `LastWriteTime`-ul
   fișierului, nu înregistrată. Peste miezul nopții orele chiar merg înapoi, pe aceeași dată — ales
   în locul unei ghiciri, și fixat cu test ca purtare declarată.
6. **`ApiServerParser` nu a văzut niciodată un fișier real de server.** Forma veche e scrisă după
   `utils/logger.py` citit în depozit; forma nouă, după §6.2 din plan — care abia la 0031-02 devine
   cod. Prima confruntare cu un fișier adevărat e în trecerea următoare.
7. **`ServerClock` nu a primit niciodată un `server_time` real** — câmpul apare abia la 0031-02.
   Aritmetica e testată; integrarea, nu.
8. **Cele 11 teste picate preexistente** rămân picate.
