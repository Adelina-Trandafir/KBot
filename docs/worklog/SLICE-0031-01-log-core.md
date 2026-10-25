# SLICE-0031-01 / 04 — Nucleul și vizualizatorul jurnalelor

Reunire documentară D001-28. Nucleul 01 și fereastra 04 păstrează contractele, abaterile și probele. Controlul reutilizabil rămâne separat în [0031-03](SLICE-0031-03-chipbar.md). 03/04 au fost realizate înainte de 02; planul server per-unitate din 02 rămâne o specificație istorică nelivrată la acest reper, iar [0089](SLICE-0089-jurnale-server-per-utilizator.md) descrie ulterior alt flux, pe utilizator/sesiune. Proba vizuală confirmată separat de operator precede reparațiile de plasare din 04; acestea nu sunt declarate privite. Release 0/0 din 01 este urmat de raportarea celor patru MSB3825 din 03/04, fără ascunderea diferenței.

[0031-01](#slice-0031-01) · [0031-04](#slice-0031-04)

<a id="slice-0031-01"></a>

## SLICE-0031-01 — Nucleul de jurnale: căi, rotație, citire, analiză, filtrare

Prima trecere din felia 0031 (vizualizator de jurnale), după `docs/PLAN_LogViewer.md` versiunea 2. Trecerea asta e **fără UI**: tot ce se scrie aici stă în `KBot.Common` (net8.0, fără WinForms, fără Drawing) plus cablarea celor patru scriitori existenți.

**Poarta de intrare a planului a trecut:** `KBOT_STATUS.md` spunea, înainte de această felie, «Next free slice number: **0031**» — exact numărul atribuit de operator. Nu a fost nevoie de oprire.

---

### Ce s-a schimbat și de ce

#### 1. Un singur loc care spune unde stau jurnalele (`LogPaths`)

`GlobalErrorLog` și `AdobeHostLog` compuneau FIECARE, prin cod duplicat, aceeași cale (`AppContext.BaseDirectory` + `Logs`). `TreeLogger` compunea alta — lângă executabil, **în afara** folderului `Logs\`. Un vizualizator are nevoie de un singur director de citit, deci calea se calculează acum într-un singur loc. Pentru primii doi scriitori calea rezultată e **identică** cu cea de dinainte; s-a verificat în fișier înainte de editare, cum cere planul.

#### 2. Rotația păstrează istoric (`LogRotation`)

10 MB, cinci generații, exact politica pe care serverul o are deja prin `RotatingFileHandler` din `utils/logger.py` (verificat: `maxBytes=10*1024*1024, backupCount=5`) — o singură regulă peste ambele jumătăți ale sistemului. Doar redenumiri, niciodată citire-și-rescriere, deci costul nu depinde de mărimea fișierului. Consum maxim mărginit: 60 MB pe familie.

Se cheamă **înainte** de fiecare adăugare, în cei patru scriitori — nu în vizualizator: un vizualizator ar aplica limita doar când îl deschide cineva, adică exact când nu mai contează.

`Roll` **nu aruncă niciodată**. Orice eșec (fișier blocat, drepturi, disc plin) pleacă pe `Trace` și întoarce `False`, ca linia care a declanșat verificarea să se scrie oricum. O problemă de rotație nu are voie să coste linia care a provocat-o. Fixat cu test pe fișier blocat exclusiv.

Două refuzuri explicite, ambele testate: `backupCount < 1` și `maxBytes <= 0` **nu ating nimic**. Un parametru greșit nu are voie să devină o distrugere de istoric.

#### 3. `TreeLogger`: mutat în `Logs\`, și cele două înghițiri devin sinkuri terminale

Fișierul trece de la directorul executabilului la `LogPaths.EnsureLogsDirectory()`. Numele (`log_{treeId}.txt`) și parametrul opțional de cale rămân neschimbate.

Cele două `Catch`-uri goale (`Write`: «Eșec silențios la scriere»; `Init`: rezerva pe Temp) scriu acum motivul pe `Trace`. **Nu pot rearunca** — s-a verificat în cod, nu presupus: `AdvancedTreeControl` le cheamă din căi de desenare (`TooltipPopup.OnPaint`, `AdvancedTreeControl.Painting.DrawContent`), iar o excepție ieșită dintr-o scriere de jurnal în `OnPaint` omoară procesul. Deci același tratament ca `GlobalErrorLog` și `AdobeHostLog`: `Trace.WriteLine`, fără rearuncare. Diferența față de starea de dinainte nu e mică — înghițirea tăcută nu lăsa **nicio** urmă.

#### 4. Citire, analiză, filtrare (`KBot.Common\Logging\`)

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

### Abateri de la plan, cu motiv

Niciuna tăcută. Toate au ieșit din citirea fișierului real, cum cere §0.

#### A. «Exact trei sinkuri terminale» nu există în `KBOT_STATUS.md`

Planul §1.4 cere corectarea, în STATUS, a unei linii care ar spune că sinkurile terminale ne-rearuncătoare sunt **exact trei**. Linia aceea **nu e în `KBOT_STATUS.md`** — s-a căutat în tot fișierul și în tot `docs/`. Singura formulare a regulii stă în `CLAUDE.md:43` și nu numără nimic: «`GlobalErrorLog.Write` is itself a terminal sink (never throws) — do not wrap calls to it.»

Deci nu era ce corecta în STATUS. Nu s-a inventat o linie ca să fie apoi «corectată». Situația reală, scrisă aici și în STATUS: sinkurile terminale ne-rearuncătoare din soluție sunt acum **cinci** — `GlobalErrorLog.Write`, `AdobeHostLog.Write`, `HandleUiError` (scrierea în runlog), `TreeLogger.Write`, `TreeLogger.Init` — ultimele două adăugate de felia asta, documentate mai sus.

#### B. Proba de potrivire a analizorului: pragul de 30% nu se aplică formatelor pe blocuri

§5.5 cere ca analizorul ghicit să fie trecut peste primele 50 de linii neagole și, sub **30%** anteturi recunoscute, ghicirea să fie declarată greșită.

Regula asta, aplicată literal, **strică exact fișierul care contează cel mai mult în felia asta**. Un `harness_errors.log` perfect sănătos e un antet urmat de zeci de linii de stivă: un bloc cu 20 de cadre dă 1/21 ≈ **4,8%** anteturi și ar fi declarat mereu «ghicire greșită», căzând pe alegerea per linie — unde `RunLogParser` ar rupe blocul, fiindcă prima linie a lui `ex.ToString()` (`System.InvalidOperationException: …`) nu e indentată și ar deveni intrare separată. Adică fix opusul testului 2 din plan, care cere ca antetul plus cinci linii de stivă să dea **o singură** intrare.

Corecția: `ILogEntryParser` capătă `ExpectsHeaderOnEveryLine`. Pragul procentual se aplică doar formatelor cu o intrare pe linie; pentru formatele pe blocuri proba cere **măcar un** antet recunoscut. Asta prinde tot ce trebuia să prindă (testul 5 — conținut adobe într-un fișier numit `harness_errors.log` — trece: zero anteturi recunoscute) fără să declare greșit un fișier normal. Ambele cazuri sunt fixate cu test, al doilea ca regresie explicită.

#### C. Rotația în constructorul lui `RunLogger` e, azi, cod fără efect

Planul cere cablarea în constructor. S-a făcut. Dar ce face de fapt merită spus, nu ascuns: apelantul de azi (`DevHarnessForm.RunTestsAsync`) compune un nume **unic pe rulare**, cu milisecunde, și deschide cu `append:=False`. Fișierul nu există încă la momentul apelului, deci `Roll` întoarce `False` fără să facă nimic — **de fiecare dată**.

Nu s-a mutat pe cont propriu în `WriteLine` (ar fi o abatere tăcută, și ar costa un `FileInfo.Length` per linie). Rămâne în constructor fiindcă `RunLogger` primește o cale **arbitrară**: un apelant viitor care dă o cale fixă chiar e protejat. Pentru scenariul din plan — «o buclă scăpată de sub control într-un test» — garda asta **nu ajută**, fiindcă bucla scrie în fișierul deja deschis. Dacă operatorul vrea acoperirea aceea, e o decizie separată, cu un cost pe linie.

#### D. `TreeLogger` nu scrie nimic în aplicația construită

`TreeLogger.Init` e apelat dintr-un **singur** loc în tot depozitul: `Surse/SURSA_TREE/Tree.vb:80`. `Surse/` nu e în `KBot.sln` (verificat) și, după `CLAUDE.md`, e o captură de sursă importată, nu cod construit. În solutia construită nimeni nu cheamă `Init`, deci `_initialized` rămâne `False` și `Write` iese pe prima linie.

Cu alte cuvinte: fișierele `log_{treeId}.txt` din tabelul §3 al planului **nu există azi** în aplicația livrată. Mutarea în `Logs\`, rotația și sinkurile terminale sunt corecte și ieftine, dar pân' când cineva cablează `Init`, sunt cod pregătit, nu cod care rulează. Analizorul e testat pe formatul real oricum — dacă `Init` se cablează vreodată, e gata.

#### E. Fișierul de rulare al bancului nu poartă marcaj de timp pe linie

Tabelul §3 descrie fișierul ca «antet, linii per test, `EROARE […]`, sumar», fără să spună că **niciuna** dintre liniile per test nu are marcaj de timp. Singura dată din tot fișierul e pe linia `Data      : yyyy-MM-dd HH:mm:ss`. De asta `RunLogParser` analizează linia aceea ca marcaj: prin regula de moștenire din §5.5, toată rularea capătă o dată. Fără asta, o rulare întreagă ar fi dispărut sub orice filtru de interval. Fixat cu test.

#### F. Nivelurile pentru verdictele de test — decise după citirea sursei, nu ghicite

Planul cere să nu se ghicească. Marcajele reale (`AppendVerdict`, `HandleUiError`): `[PASSED]` → `Info`; `[SKIPPED]` → `Warn` (nu e eroare, dar un test care **nu** a rulat nu e o reușită); `[FAILED]` și `[ERROR]` → `Error`; `EROARE [sursă]:` → `Error`.

#### G. `InternalsVisibleTo` pentru ansamblul de teste

`LogEntry` își ține mutatoarele `Friend` (în afara ansamblului o intrare e imuabilă; singurul care o modifică e `LogFileLoader`). Testele trebuie totuși să compună intrări de probă, deci `KBot.Common` expune membrii `Friend` **doar** către `KBot.Common.Tests`.

---

### Fișiere atinse

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

**Teste noi — `tests/KBot.Common.Tests/`:** `LogParserTests.vb`, `LogRotationTests.vb`, `LogFilterTests.vb`.

---

### Rezultatele testelor

Numere **dintr-o rulare reală**, nu purtate din plan sau dintr-un worklog vechi.

`dotnet build KBot.sln`: **Debug** 0 erori / 4 avertismente; **Release** 0 erori / 0 avertismente. Cele 4 avertismente din Debug sunt `MSB3825` (BinaryFormatter) în `KBot.App`, pe `DdfView.resx`, `PlatiView.resx`, `ReceptiiView.resx`, `RezervariView.resx` — fișiere **neatinse** de felia asta. Preexistente, **zero avertismente noi**.

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

#### Suita NU era verde înainte de felia asta — 11 teste picate, preexistente

§12.2 din plan cere «suita completă verde». **Nu se poate raporta asta cinstit**, fiindcă suita pica deja la 11 teste înainte de prima linie scrisă aici. Mulțimea numelor picate e **identică** înainte și după (comparată cu `diff`, ieșire goală): nu s-a stricat și nu s-a reparat nimic.

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

Grupul DDF arată ca o singură cauză (eticheta de revizie completată cu spații vs. zerouri, care se vede în Domain, Api și App deodată). **Nu s-a atins nimic din ele** — sunt în afara feliei, iar repararea lor pe furiș ar ascunde o regresie a altcuiva. Intră la fire deschise în STATUS.

---

### Ce a rămas neverificat sau amânat

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

---

<a id="slice-0031-04"></a>

## SLICE-0031-04 — `LogViewerForm`, dialogul de golire, cablarea în banc ȘI în shell

A patra trecere din felia 0031, după `docs/PLAN_LogViewer.md` §8–§9, plus două lucruri cerute direct de operator care nu erau în plan: meniul butonului de opțiuni din bara de titlu a shell-ului și fereastra de pornire care înlocuiește dialogul Da/Nu.

Cererea, în cuvintele operatorului: «integrate the new logging system into main form. i want to see the log form on press on optionbutton in the capbar. it should draw a custom popup with (for now) one option - "Arată jurnal". for now anyone can use it, but i will want to be enabled/disabled by an internal switch.» și, după trecerea vizuală, «in the launcher i want you to remove the messagebox and replace it with a form, using the navlist to select what i want to run».

---

### Ce s-a schimbat și de ce

#### 1. `LogViewerForm` — fereastra propriu-zisă

`src/KBot.App/Views/`, `Inherits KBotShellForm`, toate controalele declarate în `.Designer.vb`. Fereastra **nu analizează nimic**: tot ce ține de citit și de înțeles un jurnal stă în nucleul pur din `KBot.Common\Logging` (trecerea 01) — `LogPaths`, `LogFileLoader`, `LogFilter`, `ServerClock`. Aici se leagă doar nucleul acela de controalele casei.

Trei hotărâri de fond:

- **Citirea nu stă niciodată pe firul UI.** Fișierele se citesc și se analizează pe un fir de fundal
  (un jurnal poate avea 5 MB), cu `BusyBar` aprins; **filtrarea e în memorie și sincronă** și nu
  recitește niciodată un fișier. De aceea căutarea are cronometrul de 250 ms cerut de plan — altfel
  fiecare literă ar reface filtrarea peste zeci de mii de intrări. O a doua selecție o **anulează** pe
  prima (`CancellationTokenSource`): operatorul a schimbat fișierul, răspunsul vechi n-are ce căuta în
  grilă.
- **Coloana `Ora` arată ora CORECTATĂ, panoul de detaliu arată blocul BRUT.** Cine citește o urmă de
  stivă trebuie să vadă exact ce s-a scris în fișier, nu o versiune ajutată de noi. Fixat cu test.
- **Culorile rândurilor vin din paletă**, prin `RowFormatting` (§1.1 din plan): `ErrorColor`,
  `WarningColor`, `TextDimColor`, `DisabledTextColor`. **`Info` nu se vopsește deloc** — e rândul
  obișnuit, iar a-l colora ar face din normal o excepție. `KBotDataView` nu a fost atins, iar instanța
  refolosită a argumentelor nu se reține.

Bara de stare spune tot ce ar putea altfel dispărea tăcut: câte intrări au intrat, câte se văd, cât s-a citit, dacă fereastra de citire a tăiat coada, decalajul ceasului de server și — obligatoriu — **câte intrări au fost excluse fiindcă n-au dată**, când e pus un capăt de interval.

#### 2. Serverul: cablat, opțional, și ONEST că încă nu există

Grupul «Server» apare doar dacă fereastra a primit un `IApiClient`; fără el (bancul de probă) nu apare deloc — un grup care nu poate funcționa nu trebuie să existe. Lista se cere **la cerere**, dintr-un rând anume, nu la deschidere.

**Rutele nu există încă**: sunt livrate de trecerea 0031-02, împreună cu `GetLogFilesAsync` / `GetLogTailAsync` de pe `ApiClient`. Astăzi `ApiClient.GetAsync(Of T)` e literalmente `Throw New NotImplementedException()`. Fereastra prinde exact excepția aia și scrie în `KBotNotice`: «Jurnalele de server nu sunt încă disponibile: ruta se livrează în trecerea 0031-02. Fișierele locale funcționează normal.» Nu o listă goală, nu un mesaj tehnic englezesc aruncat operatorului.

DTO-urile (`LogFilesResponse` / `LogTailResponse`) sunt **private în formular**, scrise după contractul din planul §6.4. Când vine 0031-02, ele se mută pe `ApiClient` ca metode tipizate și dispar de aici. Probele 19–20 din plan aparțin acelei treceri și **nu au fost făcute**.

#### 3. `LogClearDialog` — singurul drum distructiv

Doi pași, fiindcă nu există «înapoi»: întâi lista fiecărui fișier local cu mărimea și numărul de intrări, **nimic bifat** la deschidere; apoi o confirmare care NUMEȘTE fișierele și totalul.

- Fișierul ținut deschis de rularea curentă se **arată**, dar șters și nebifabil. Se întreabă
  EMPIRIC (o deschidere exclusivă de o clipă), nu după o listă de nume ținută pe de rost, care ar
  rămâne în urmă la primul scriitor nou.
- `File.Delete`; pe `IOException` fișierul se **golește** în loc să fie șters. Dacă nici asta nu merge,
  apare pe nume, cu motivul, iar restul continuă. Nimic nu eșuează tăcut.
- **Jurnalele de server nu se ating de aici, niciodată.**

#### 4. Cablarea în bancul de probă (plan §9), fără ca bancul să refere `KBot.App`

`LogViewerForm` trăiește în `KBot.App` — acolo îi e locul, ca să rămână și în Release. Dar `KBot.DevHarness` **nu referă** `KBot.App` (referința merge invers), deci proba vizuală nu putea pur și simplu să construiască fereastra.

Soluția e o punte tipizată: `ILogViewerLauncher` în `KBot.DevHarness\Abstractions\`, implementată de `LogViewerLauncher` în `KBot.App` (compilat `#If DEBUG`, ca și referința) și înregistrată în DI. Plus un buton «Jurnale» pe `DevHarnessForm`, pe același tipar cu `OpenMainFormAction`. Un test care nu găsește puntea întoarce **Skipped**, nu Failed: înseamnă doar că rulează într-o gazdă fără shell.

`LogViewerTest` (categoria `Controls/UI`) copiază `ThemeGalleryTest` exact: OK → Passed, Cancel → Failed, închis → Skipped.

#### 5. Meniul din bara de titlu + comutatorul intern (CERUT de operator)

`capBar.OptionButtonClick` desfășoară un `CustomPopup` cu un singur rând, «&Arată jurnal». Aceleași decizii ca meniul de teme al aceleiași bare: garda `CustomPopup.ClosedJustNow` (fără ea, al doilea clic ar redeschide meniul instantaneu), ancorare pe `OptionButtonBounds`, fereastra arătată **nemodal** și **nu** într-un `Using`.

Comutatorul intern e `FeatureSwitches.VizualizatorJurnaleActiv` (`KBot.Common`, modul nou) — **azi mereu `True`**, cum s-a cerut. Când devine `False`, meniul **nu se deschide deloc**: fiind singurul rând, un meniu cu el stins ar fi o fereastră goală agățată de buton. Locul din care se va citi cu adevărat (rol, manifest, configurare) se schimbă **în interiorul modulului**, fără să atingă apelanții — ăsta e tot rostul lui.

Fereastra e nemodală și una singură: dacă e deja deschisă, se aduce în față (ca `InternalInfoForm`).

#### 6. Fereastra de pornire înlocuiește `MessageBox`-ul (CERUT de operator)

`StartupLauncherForm`: `KBotCaptionBar` + `KBotNavList` cu trei porniri — **Aplicația** (autentificare → shell), **Banc de probă**, **Jurnale** (vizualizatorul singur, fără autentificare și fără shell). Un `MessageBox` cu două butoane putea purta exact două opțiuni, iar textul lor era «Da» și «Nu»; aici pornirile se citesc dintr-o privire, se adaugă altele fără să se schimbe nimic altceva, și fereastra e tematizată (dialogul de sistem nu era).

Prima pornire e selectată din start: cine apasă Enter fără să citească primește **aplicația**, nu bancul. Renunțarea lasă `Alegere` gol, ca un apelant care se uită doar la proprietate să nu pornească ultima pornire survolată. Dispecerul din `Program.RunLauncher` **aruncă** pe o cheie necunoscută — o pornire adăugată în listă și uitată în dispecer trebuie să se vadă imediat.

#### 7. Trecerea vizuală a operatorului, și ce a rupt pe drum

Operatorul a deschis fereastra în designerul VS, a confirmat că **funcționează**, și a rearanjat așezarea: panourile andocate au fost înlocuite cu `TableLayoutPanel`-uri (`tlyMain`, `tlyFilter`, `tlyFilterActual`, `tlyFooter`) — «i want tly instead of those annoying panels where i have no real control» — apoi a cerut punerea la punct a plasării. Trei defecte reale găsite și reparate:

1. **`btnGoleste` era DECLARAT, dar niciodată construit și niciodată adăugat undeva**, iar clauza
   `Handles` fusese scoasă din code-behind. Formularul compila, se deschidea, arăta bine — și singurul
   drum de ștergere pur și simplu **nu exista**. Reintrodus în `tlyFooter` (coloană nouă, lângă
   «Deschide dosarul»: amândouă lucrează cu fișierele, iar un buton distructiv n-are ce căuta lângă
   «Copiază»), cu `Handles` la loc. **Are acum test de regresie**: un câmp declarat nu e un control
   montat. E același tipar cu `LstValori_ItemCheck` din 0030 și cu `btnSortClear`.
2. **`tlyMain.SetColumnSpan(tlyFooter, 4)` pe un tabel cu DOUĂ coloane** → 2.
3. **`tlyFilter` avea trei rânduri de 60 + 80 + rest într-o celulă de 124 px**: suma depășea celula,
   iar un `TableLayoutPanel` nu strânge rândurile absolute ca să încapă, deci bara de jetoane era
   tăiată. Două rânduri acum (câmpuri fix, jetoane elastic), cu jetoanele pe `Dock = Fill`: bara își
   rupe rândurile după LĂȚIME, iar înălțimea i-o dă tabelul.

Au fost șterse și trei declarații moarte (`pnlCard`, `pnlActiuni`, `pnlStare`) — controale care nu mai există nicăieri în formular.

---

### Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.App/Views/LogViewerForm.vb` / `.Designer.vb` | **nou** — fereastra (designerul rearanjat de operator pe `TableLayoutPanel`, plasarea pusă la punct aici) |
| `src/KBot.App/Views/LogClearDialog.vb` / `.Designer.vb` | **nou** — golirea în doi pași |
| `src/KBot.App/StartupLauncherForm.vb` / `.Designer.vb` | **nou** — fereastra de pornire (Debug) |
| `src/KBot.App/LogViewerLauncher.vb` | **nou** — capătul App al punții către banc (`#If DEBUG`) |
| `src/KBot.App/Program.vb` | `RunLauncher` în locul `MessageBox`-ului; înregistrarea `LogViewerForm` + `ILogViewerLauncher`; cablarea butonului «Jurnale» |
| `src/KBot.App/MainForm.vb` | meniul butonului de opțiuni + deschiderea nemodală a vizualizatorului |
| `src/KBot.Common/FeatureSwitches.vb` | **nou** — comutatorul intern |
| `src/KBot.DevHarness/Abstractions/ILogViewerLauncher.vb` | **nou** — puntea |
| `src/KBot.DevHarness/Tests/LogViewerTest.vb` | **nou** — proba vizuală (`Controls/UI`) |
| `src/KBot.DevHarness/DevHarnessForm.vb` / `.Designer.vb` | butonul «Jurnale» + `OpenLogViewerAction` |
| `src/KBot.Controls/ChipBar/KBotChipBar.vb` | `ContainsChip` (varianta care nu aruncă), cerută de fereastră |
| `tests/KBot.App.Tests/LogViewerFormTests.vb` | **nou** — probele 21–24 din plan + regresia butonului nemontat |

---

### Rezultatele probelor

Numere REALE, dintr-o rulare:

- `dotnet build KBot.sln -c Debug`: **0 erori**, 4 avertismente `MSB3825` **preexistente** (cele patru
  `.resx` de la alte felii, neatinse).
- `dotnet build KBot.sln -c Release`: **0 erori**, aceleași 4 avertismente (vezi nota din
  `SLICE-0031-03` despre «Release 0/0» din trecerea 01).
- `KBot.App.Tests`: **163** teste, **156 verzi / 7 picate**. Cele 7 sunt **preexistente** (4 Istoric,
  2 DDF, 1 `MainFormNavItemsTests.Designer_WroteLiteralDiacritics_NotEscapes`).
- Suita completă: **11 picate**, aceleași ca înainte de felie — 3 `Domain` (DDF) + 1 `Api` (DDF) +
  7 `App`. Restul verzi: `Controls` 831, `DevHarness` 170, `Theming` 71, `Common` 58, `Xfa` 39,
  `LocalStore` 1.

Probele 21–24 din plan, acoperite: fereastra montează bara de jetoane, lista, grila (cele șase coloane în ordine, `Ora` înghețată, grilă read-only), panoul de detaliu și notificările; `RowFormatting` mapează fiecare nivel pe culoarea din `ThemeManager.Current.Palette` (**niciodată pe un literal**), iar `Info` rămâne nevopsit; selecția pune `Raw` **neconvertit** în panou; un apel de server picat scoate notificarea și **lasă intrările locale întregi**.

---

### Ce a rămas NEVERIFICAT sau amânat

1. **Trecerea 0031-02 (jurnalele de server) NU s-a făcut.** Rutele, `LOG_DIR`, filtrul per unitate,
   handler-ul catch-all, `gunicorn.conf.py`, metodele tipizate de pe `ApiClient` și probele 19–20 +
   25–31 rămân. Până atunci grupul «Server» al ferestrei există, se poate apăsa și **spune pe șleau** că
   ruta nu e livrată. `ApiServerParser` și `ServerClock` continuă să nu fi văzut date reale de server.
2. **Proba vizuală din banc (`LogViewerTest`) nu a fost RULATĂ de mine.** Operatorul a confirmat
   separat că fereastra funcționează și a rearanjat-o singur; ce **nu** a fost privit după reparațiile
   de plasare de la §7 e rezultatul lor pe ecran (jetoanele netăiate, butonul de golire la locul lui,
   subsolul cu cinci coloane).
3. **Fereastra nu a fost văzută în Dark / Modern.**
4. **`StartupLauncherForm` nu a fost văzută pe ecran deloc** — e scrisă, compilează, dispecerul e
   acoperit de tipuri, dar nimeni nu s-a uitat la ea.
5. **Căutarea nu pliază diacriticele** («sters» nu găsește «șters») — limitare cunoscută a lui
   `LogFilter`, moștenită din trecerea 01, nereparată aici pe tăcute.
6. **Fără urmărire în timp real** (`FileSystemWatcher`), conform planului. Cititorul e scris ca să
   poată primi asta ulterior.
7. **Jurnalele nginx rămân invizibile**, ca în plan.
8. `KBotChipBar` **nu a trecut prin designerul real VS ca instanță proaspăt pusă pe un formular** —
   vezi firul deschis din `SLICE-0031-03`.
