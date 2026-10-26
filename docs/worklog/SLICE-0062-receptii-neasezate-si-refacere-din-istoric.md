# SLICE-0062/64/65/68 — Istoric recepții: refacere, linii, totaluri și ancore

Patru jurnale din 15–18.09.2026 sunt reunite integral. Ordinea contează: 0062 arată și reface ce lipsește după IDH; 0064 păstrează liniile zero și, în 0064-02, exclude antetele fără linii în afara ștergerii; 0065 repară calculul DIF după editor și citește ultimul antet în vedere; 0068 adaugă ancora pentru istoric absent din payload și consumul o singură dată. Afirmațiile inițiale că 4a nu verifică IDH, că nu sunt văzute toate rândurile migrate ori că un antet fără linii trebuie afișat se citesc la data etapei, cu corecțiile ulterioare. Ruta admin cu cheia veche este istoricul lui 0062; [planul de retragere ulterior](../PLAN_Securitate_API.md) și excepția sa limitată nu sunt înlocuite de acest jurnal. Selecția/graficele rămân în [0061](SLICE-0061-asociere-selectie-multipla-si-grafice-separate.md), iar regulile numerotate în [fundament](../FUNDAMENT_Asociere_Receptii.md). Probele sunt istorice, nu rerulate.

[0062-receptii](#slice-0062) · [0064-linii](#slice-0064) · [0065-receptia](#slice-0065) · [0068-ancora](#slice-0068)

<a id="slice-0062"></a>

## SLICE 0062 — Anteturile fără recepție se VĂD, și refacerea din istoric a ce lipsește din `FX_Receptii_H` / `FX_Receptii`

**Data:** 15.09.2026 **Cerute de operator** (două propoziții):

1. «`ReceptiiH` își pierde `IDRH` și `IDRR` în Access; la migrare rândurile ajung pe MariaDB, dar `Receptii_H` n-are `IDRR` în niciun câmp, așa că serverul spune că NU sunt recepții de încărcat — dar sunt, doar că nu-s legate de `Receptii_R` (`IDRR IS NULL`).»
2. «Am nevoie de o cale, în vederea Recepții sau în `IstoricView`, să refac toate rândurile care lipsesc din `Receptii` sau `Receptii_H`, după `IDH`.»

S-a ales vederea Recepții pentru amândouă: acolo e simptomul (lista goală) și tot acolo sunt deja editorul de legături (antet) și reîmprospătarea (subsol dreapta). `IstoricView` rămâne read-only, cum e declarată.

---

### 1. De ce dispăreau (cererea 1)

`GET /api/forexe/receptii` pornea din `FX_Receptii_R` cu `INNER JOIN FX_Receptii_H ON H.IDRR = R.IDRR` — portul literal al lui `qFX_MAIN_REC_TREE`. Un antet cu `IDRR NULL` nu se potrivește cu nicio recepție, deci cădea din rezultat; când TOATE anteturile sunt așa, răspunsul e `receptii: []` și vederea spune «Angajamentul nu are recepții». Serverul nu mințea despre ce a găsit — mințea interogarea, prin ce alegea să nu caute.

**Schimbarea:** rădăcina interogării e acum `FX_Receptii_H`, cu `LEFT JOIN FX_Receptii_R` și `WHERE H.CodAngajament = %s`. Un antet neașezat vine cu toate câmpurile de recepție `NULL` (`idrr: null`, `data_r: null`, `suma_antet: 0.0`, steagurile `false`). Restul rămâne: `LEFT JOIN FX_Receptii`, `LEFT JOIN FX_Indicatori`, subinterogările scalare pe `Clasificatii`, aceeași ordine (`R.NRCRT, R.DataR, H.NrCrt, H.DataH, Rc.IDR` — NULL-urile lui R vin primele în MariaDB, dar clientul oricum grupează). O recepție FĂRĂ niciun antet nu apare — nici în Access nu apărea.

**Pe fir:** `GetReceptieRow.idrr` devine `Integer?`; `ReceptieRow.Idrr` rămâne `Integer`, cu **0 = neașezat** — aceeași convenție pe care o are deja `AsociereInfo` (`idrr = 0` = «pe nicio recepție»). Un `null` JSON într-un `Integer` nenulabil ar fi picat deserializarea întregului răspuns, deci schimbarea DTO-ului nu e cosmetică.

**În arbore (`ReceptiiView.BuildTree`):** rândurile se despart în așezate (`Idrr > 0`) și neașezate. Lunile și tooltip-urile de reconciliere se calculează DOAR peste cele așezate — un antet fără `DataR` n-are lună și n-are ce căuta într-un cumul. Neașezatele merg într-un dosar nou, **ultimul**, «Instantanee neașezate», cu totalul = `Sum(Total)` pe anteturi distincte și un nod per antet (`h_{IDRH}`: `DataH`, descrierea antetului, `Total`; iconița după semnul valorii, ca la recepții). Click pe dosar sau pe un antet umple grila cu liniile lor, ca orice nod. Tooltip-ul dosarului spune câte sunt și că se așază din editorul de legături (iconița din antetul arborelui), care le vede deja: `citeste_instantanee` din `asociere.py` citea DE LA ÎNCEPUT toate anteturile angajamentului, cu `idrr = 0` pentru cele fără recepție.

### 2. Refacerea din istoric (cererea 2)

#### 2.1 Ce e sursa de adevăr

`FX_Istoric` nu pierde nimic: fiecare antet și fiecare linie poartă `IDH = FX_Istoric.ID` (verificat în `TABLES/FX_Receptii_H.md` și `TABLES/FX_Receptii.md`: coloana `IDH` și relația `FX_IstoricFX_Receptii_H` / `FX_IstoricFX_Receptii`). Iar algoritmul care construiește H și liniile din istoric există deja, de două ori: în Access (`mdl_FX_Istoric.FX_Istoric_Populeaza_ Receptii`, liniile 529–652 din export) și portat în `prelucrare_pasi.step4a_populeaza_receptii` (felia 0048-03). Amândouă merg peste rândurile de istoric de recepție în ordinea `ID`, strâng liniile (`Val_Receptie <> 0`) într-un tampon și le varsă sub primul rând cu `(activ:true)` în `Observatii`, care e antetul (`IDH` = ID-ul lui, `DataH = DataFX`, `Total = Val_Receptie`, `Descriere` = textul dintre «Receptie: » și virgulă).

#### 2.2 Ruta nouă: `POST /api/forexe/receptii/refacere`

`routes/forexe/receptii_refacere.py`, `refa_receptii(cursor, cod, aplica)`. Aceeași plimbare ca 4a, cu trei deosebiri, toate deliberate:

* **Peste rândurile `Prelucrat = 1`**, nu `= 0`. Cele neprelucrate aparțin următoarei descărcări; 4a le va construi atunci și NU verifică `IDH`-ul, deci dacă le-am construi și aici ar ieși două H pentru același rând de istoric.
* **Scrie numai ce lipsește.** Antet: dacă există deja un H cu acel `IDH`, i se refolosește `IDRH`-ul; altfel se inserează cu `_H_INSERT_SQL` din 4a (`IDRR NULL`, `NrCrt` continuă de la maxim, `EsteStergere` din descriere). Linie: lipsește după `IDH` ▸ `_REC_INSERT_SQL`; există dar `IDRH IS NULL` (orfană) ▸ `UPDATE FX_Receptii SET IDRH = %s` — relegată, nu dublată; există și e legată ▸ nu se atinge.
* **Nu ridică pentru un indicator lipsă.** 4a ridică `ValueError` (o ingestie nu are voie să continue cu o linie fără indicator). La refacere, o asemenea linie ar opri tot angajamentul; se sare, se numără în `linii_sarite` și se spune în `avertismente`.

Trei lucruri se **semnalează** fără să se înghită: liniile rămase fără antet la sfârșit (tamponul nevidat), liniile sărite, și anteturile existente care au linii cu `IDH NULL` (la ele nu se poate judeca ce lipsește, deci liniile lor nu se ating și se spune de ce).

**DIF-urile.** O linie adăugată sau relegată sub un antet care e DEJA pe o recepție îi schimbă lanțul: `step4d_calculeaza_dif(cursor, cod, idrr)` se cheamă pentru fiecare `IDRR` atins (`receptii_recalculate` în răspuns). Un antet nou n-are `IDRR`, deci n-are lanț — îl va avea când operatorul îl așază, iar așezarea (`recalculeaza_final`) își face socoteala ei.

**Două moduri, un singur cod.** `aplica = false` (implicit) e o PROBĂ: numără, nu scrie (`rollback`). `aplica = true` scrie într-o singură tranzacție. Cheile răspunsului sunt ASCII, identice în ambele moduri: `antete_lipsa, linii_lipsa, linii_orfane, antete_scrise, linii_scrise, linii_relegate, linii_fara_antet, linii_sarite, receptii_recalculate, avertismente, aplicat, cod`.

#### 2.2b Toată baza dintr-un foc

A doua cerere a operatorului din aceeași zi: «trebuie să le pot face pe toate deodată, pentru o bază». Același endpoint, corp `{ "toate": true, "aplica": … }` (exclusiv cu `cod`): `refa_toate` ia toate `CodAngajament`-urile cu rânduri de istoric de recepție prelucrate și cheamă `refa_receptii` pentru fiecare, **fiecare în tranzacția lui** — un angajament care cade (FK, indicator) intră în `erori` cu mesajul lui și nu trage după el nici ce s-a scris înainte, nici ce vine după. Răspunsul: `angajamente`, `cu_lipsuri`, `totaluri` (suma pe cele opt chei numărate) și `detalii` DOAR pentru angajamentele la care s-a găsit ceva de făcut sau de spus. Fișier de cereri pentru VS Code (REST Client): `PYTHON/requests/receptii_refacere.http` — login, probă pe un cod, aplicare pe un cod, probă pe toată baza, aplicare pe toată baza. Parola se cere la prompt, nu stă în fișier.

#### 2.2c Fără sesiune, cu cheia de administrare

«Nu pot cu utilizatorul admin, să dau doar numele bazei?» — ba da: `POST /api/admin/receptii/refacere`, în `routes/admin.py`, sub `require_api_key` (`X-Api-Key` = `API_KEY` din `config.py`), cu `db_name` în corp (validat de `_validate_db_name`), plus `cod` sau `toate`, și `aplica`. Același nucleu (`citeste_cererea` + `executa_refacerea`, scoase în `receptii_refacere.py` ca ambele rute să le împartă), același răspuns, plus `db_name` ecou. **Nu trece prin `get_db_connection`** ca restul rutelor de administrare: tabelele `FX_` sunt pe serverul K-BOT (`get_kbot_connection`). Importul e local în handler — `angajamente.py` importă deja `_validate_db_name` din `admin.py`, un import la vârf închidea un cerc. Regula «bearer ≠ X-Api-Key, nu se amestecă» rămâne: sunt două rute, fiecare cu o singură pază.

`PYTHON/requests/refacere.ps1` (curl.exe): `-Db 000_DEMO [-Apply] [-Cod …]` pe calea admin (cheia la prompt mascat / `-ApiKey` / `KBOT_API_KEY`), sau `-User … -Dc …` pe calea cu login. Scrie răspunsul brut lângă script și îl rezumă în consolă.

#### 2.3 Clientul

* `IApiClient.RebuildReceptiiAsync(cod, apply, ct)` ▸ `ReceptiiRebuildResult` (POCO nou în `KBot.Domain`, cu `NothingToDo`). DTO-urile `PostReceptiiRefacereRequest/Response` în `UpsertAngajamenteRequest.vb`, lângă celelalte.
* `ReceptiiView`: al treilea callback opțional, `rebuildMissing As Action(Of String)`, legat de **iconița din STÂNGA subsolului** arborelui (`FooterLeftIcon = database`, slot liber până acum — butonul de strângere nu e pornit în vederea asta, deci nu-i ia colțul). Fără callback iconița se stinge, ca celelalte două.
* `MainForm.RebuildReceptiiFromIstoric(cod)`: proba ▸ dacă nu lipsește nimic, o spune și se oprește ▸ altfel întreabă cu cifrele reale (câte anteturi, câte linii, câte orfane) și cu avertizarea că anteturile refăcute vor apărea în dosarul «Instantanee neașezate» ▸ la Da, scrierea ▸ raportul (scrise / relegate / recepții recalculate / semnalări) ▸ `LoadTreeAsync(pastreazaSelectia:=True)`, ca după editorul de legături: steagurile `Are*` ale nodului se pot schimba, iar reîncărcarea împinge singură contextul nou în vedere. Trăiește în shell din motivul obișnuit: plasa de re-autentificare pe o formă de răspuns proprie, iar `WithReauth` e privat acolo.

#### 2.4 Drumul operatorului

Recepții ▸ angajamentul ▸ iconița din stânga subsolului ▸ Da ▸ anteturile apar în «Instantanee neașezate» ▸ iconița din antetul arborelui (editorul de legături) ▸ se așază pe recepțiile lor ▸ la salvare, DIF-urile se recalculează și arborele se reîncarcă.

### Fișiere atinse

**Nou**
- `PYTHON/routes/forexe/receptii_refacere.py`
- `PYTHON/tests/test_forexe_receptii_refacere.py` (15 teste, offline, `FakeCursor`)
- `PYTHON/requests/receptii_refacere.http` (cererile pentru VS Code REST Client)
- `PYTHON/requests/refacere.ps1` (curl.exe: login sau cheie admin ▸ probă / aplicare)
- `PYTHON/tests/test_admin_receptii_refacere.py` (6 teste, offline, Flask test client + `executa_refacerea` înlocuit)
- `src/KBot.Domain/ReceptiiRebuildResult.vb`
- `docs/worklog/SLICE-0062-receptii-neasezate-si-refacere-din-istoric.md` (acest fișier)

**Modificat**
- `PYTHON/routes/forexe/receptii.py` — rădăcina H + LEFT JOIN R; nota din antet
- `PYTHON/routes/forexe/__init__.py` — înregistrarea rutei
- `PYTHON/routes/admin.py` — `POST /api/admin/receptii/refacere` (X-Api-Key + `db_name`)
- `src/KBot.Api/IApiClient.vb`, `ApiClient.vb` — `RebuildReceptiiAsync`; `Idrr = If(r.idrr, 0)`
- `src/KBot.Api/UpsertAngajamenteRequest.vb` — `idrr As Integer?`; cele două DTO-uri
- `src/KBot.Api/KBot.Api.vbproj` — FileVersion 1.0.5.0 ▸ 1.0.6.0
- `src/KBot.Domain/ReceptiiInfo.vb` — documentarea convenției `Idrr = 0`
- `src/KBot.Domain/KBot.Domain.vbproj` — FileVersion 1.2.0.0 ▸ 1.2.1.0
- `src/KBot.App/Views/ReceptiiView.vb` / `.Designer.vb` — dosarul neașezatelor, iconița și handler-ul din stânga subsolului, `UNPLACED_KEY`
- `src/KBot.App/KbotForm.vb` — `RebuildReceptiiFromIstoric`, `WarningsParagraph`, cablarea
- `src/KBot.App/KBot.App.vbproj` — FileVersion 1.0.29.0 ▸ 1.0.30.0
- `tests/KBot.App.Tests/ReceptiiViewTests.vb` — 5 teste noi (dosar, click, doar-neașezate, iconița pornită/stinsă, click ▸ shell); stub-ul `RebuildReceptiiAsync` în toate cele 9 implementări false ale lui `IApiClient` din `tests/KBot.App.Tests`

Restul arborelui de lucru (`KbotForm.Designer.vb`, `KBot.Migrator/*`, `KBot.Theming/*`, `FormFit*`, `Surse/SURSA_XFA_WRITTER/` etc.) era deja modificat/neurmărit la începutul sesiunii — **nu e al acestei felii**.

### Rezultate

- `PYTHON`: `pytest tests` — **535 trecute / 26 sărite (toate host-only / env), 0 eșuate** (21 noi). **Niciun test n-a atins serverul sau vreo bază**: cursoare false și `executa_refacerea` înlocuit.
- `dotnet build KBot.sln --no-incremental` — **0 erori**; avertismentele `MSB3825` (`.resx` / `BinaryFormatter`) sunt PREEXISTENTE.
- `dotnet test tests/KBot.Common.Tests` — 94 trecute (vezi și felia 0063).
- `dotnet test tests/KBot.App.Tests` — vezi §«Ce NU s-a făcut» dacă rezultatul lipsește de aici.

### Ce NU s-a făcut, și trebuie spus

- **Nimic nu a rulat pe o bază vie.** Interogarea H-rădăcină, ruta de refacere și recalcularea DIF n-au atins MariaDB; `test_forexe_receptii.py` (host-only) nu s-a putut rula aici. Că `Prelucrat = 1` acoperă chiar rândurile migrate din Access e **dedus** din `FX_Istoric_Actualizeaza_Rezolvat` (setează `Prelucrat = True` după prelucrare), nu observat pe datele operatorului.
- **Nimic nu s-a văzut pe ecran.** Dosarul «Instantanee neașezate», iconița `database` din stânga subsolului și cele trei casete ale shell-ului sunt verificate doar prin teste headless / prin compilare.
- **Nu s-a atins `IstoricView`** (varianta a doua din cerere): decizie de plasare, nu lipsă.
- **Liniile legate de un H GREȘIT** (există după `IDH`, `IDRH` nenul, dar alt antet decât cel dedus din istoric) nu se ating și nu se semnalează. N-a fost în cerere; ar fi o judecată peste o legătură pe care cineva a făcut-o.
- **Anteturile duplicate pe același `IDH`** (dacă o migrare repetată le-a produs) nu se detectează: `h_dupa_idh` reține ultimul citit.
- **Nimic nu s-a comis** — arborele de lucru conține WIP străin de felia asta; comiterea selectivă e a operatorului.

---

<a id="slice-0064"></a>

## SLICE 0064 — Liniile cu zero ale unui instantaneu sunt linii și se păstrează (F31)

**Data:** 17.09.2026 **Cerut de operator:** «Unele H au indicatori cu valoare zero. Nu e o eroare — înseamnă că nu s-a schimbat nimic la acel indicator. Când iau un H neașezat din coș, văd doar rândul care are valoare, și asta declanșează «Instantaneul pierde indicatorii…», deci nu-l pot pune unde trebuie. Ori luăm din MariaDB și indicatorii cu 0, ori nu mai folosim mesajul. Aș merge pe prima.»

S-a mers pe prima.

---

### 1. De unde venea refuzul

`MotivulRefuzului` din `AsociereForm` (și perechea lui de pe server, `valideaza_plasarile` din `prelucrare_asociere.py`) aplică F16: de-a lungul lanțului ordonat după `DataH`, mulțimea indicatorilor unui instantaneu trebuie s-o conțină pe a celui dinainte. Mulțimile se iau din `FX_Receptii` — liniile instantaneului.

Numai că liniile cu zero **nu ajungeau niciodată în `FX_Receptii`**. Pasul 4a al ingestiei (`prelucrare_pasi.step4a_populeaza_receptii`) e portul fidel al lui `FX_Istoric_Populeaza_ Receptii` din Access (`mdl_FX_Istoric.md`, linia 623: `ElseIf CDbl(rcHis!Val_Receptie) <> 0`), care păstra doar rândurile cu valoare. Un indicator care nu s-a mișcat într-o salvare (site-ul îl trimite cu `Suma receptie: 0 RON`) dispărea deci din instantaneu, deși recepția îl are în continuare în `RHR` (pasul 4b scrie TOATE rândurile din `Detaliu`, și pe cele cu zero). Rezultatul: primul instantaneu al lanțului numea {AAB, AA2}, următorul — cu AA2 la zero — numea doar {AAB}, și F16 spunea, corect după datele pe care le vedea, că AA2 «a dispărut». Mesajul însuși zice «un indicator poate cădea la zero, dar nu poate dispărea» — regula presupunea exact liniile pe care ingestia le arunca.

Nu era un defect al vetoului, ci al datelor de sub el. Vetoul rămâne.

### 2. Ce s-a schimbat

**`PYTHON/routes/forexe/prelucrare_helpers.py`** — `este_linie_receptie(rand)` (nou): un rând de istoric care nu e antet e LINIE dacă are `Val_Receptie <> 0` **sau** numește un indicator (`CodAI` nevid). Un rând cu zero fără indicator se sare, ca înainte; un rând cu valoare al cărui indicator lipsește din `FX_Indicatori` ridică în continuare aceeași eroare. Criteriul trăiește o singură dată, fiindcă îl folosesc două plimbări care trebuie să fie de acord:

* **`prelucrare_pasi.step4a_populeaza_receptii`** — `elif este_linie_receptie(r)` în locul lui `elif float(r["Val_Receptie"] or 0) != 0`. Linia cu zero se scrie cu `Valoare = 0`, `ValoareOrig = 0`, hash-ul calculat pe 0, `TipIntern` după aceeași regulă.
* **`receptii_refacere.refa_receptii`** (felia 0062) — aceeași înlocuire. Prin asta refacerea devine și **umplerea înapoi** pentru F31: liniile cu zero pe care ingestiile de dinainte le-au aruncat sunt «lipsă după `IDH`», se inserează sub antetul lor existent, iar `DIF`/`DIFC` ale lanțului atins se recalculează (`step4d_calculeaza_dif`).

**`docs/FUNDAMENT_Asociere_Receptii.md`** — **F31** adăugat, cu sursa VBA și motivul.

**`src/KBot.App/Forexe/AsociereForm.vb`** — doar comentariul lui `MotivulRefuzului`: spune de unde vin mulțimile, că zerourile sunt acum acolo, și că nu are voie să apară un filtru pe `Valoare`. Nicio linie de cod VB schimbată.

### 3. Ce se schimbă în rest, prin construcție

* **`SUM(DIF)` coboară acum odată cu indicatorul.** Înainte, un indicator cu 100 în H1 și 0 în H2 lăsa în `FX_Receptii` doar linia lui H1 (`DIF = 100`); acum H2 are linia cu 0 și `DIF = -100`, deci `Sumar.TotalReceptii` și `qFX_ORD_REC_ANT` (care adună `DIF`) dau valoarea reală. Access avea același defect — nu se copiază.
* **Recepțiile reconstituite** (F26) își primesc `RHR` din ultimul instantaneu cu linii, deci vor avea și rânduri cu 0 — exact cum le are o recepție descărcată de pe site.
* **F15** compară liniile cu zerourile deja scoase pe amândouă laturile (`_valori_pe_indicator`, felia 0058), deci nu vede nicio diferență.
* **Vederea Recepții** arată un rând per linie `FX_Receptii`: sub un antet vor apărea și liniile cu 0. E adevărul instantaneului; dacă operatorul le vrea ascunse, e un filtru de afișare, nu unul de ingestie.
* Pasul 4c (trecerea automată) potrivește doar pe `Total` — neatins.

### 4. Fișiere atinse

| Fișier | Ce |
|---|---|
| `PYTHON/routes/forexe/prelucrare_helpers.py` | `este_linie_receptie` (nou) |
| `PYTHON/routes/forexe/prelucrare_pasi.py` | 4a folosește criteriul; docstring F31 |
| `PYTHON/routes/forexe/receptii_refacere.py` | refacerea folosește criteriul; docstring |
| `PYTHON/tests/test_forexe_prelucrare_pasi.py` | +2 teste (zero cu indicator = linie; zero fără indicator = sărit) |
| `PYTHON/tests/test_forexe_receptii_refacere.py` | +1 test (linia cu zero aruncată de o ingestie veche se pune înapoi sub antet, cu DIF recalculat) |
| `docs/FUNDAMENT_Asociere_Receptii.md` | F31 |
| `src/KBot.App/Forexe/AsociereForm.vb` | comentariu la `MotivulRefuzului` |

### 5. Rezultatele testelor

* PYTHON (`.venv`): suita întreagă **538 trecute / 26 sărite / 0 picate** (de la 535/26; +3 teste noi). Cele patru fișiere direct atinse: 138 trecute.
* `dotnet build src\KBot.App\KBot.App.vbproj`: **0 erori, 0 avertismente** (schimbare de comentariu, dar s-a compilat).
* Suitele VB.NET nu s-au rulat — nicio linie de cod VB nu s-a schimbat.

### 6. Ce rămâne neverificat / de făcut de operator

* **Nimic n-a rulat pe MariaDB.** Că site-ul trimite într-adevăr rândul cu `Suma receptie: 0 RON` pentru indicatorul nemișcat e afirmația operatorului (F31 `OPERATOR`), nu un rând citit de mine din `FX_Istoric`. Dacă rândul cu zero lipsește din istoric, atunci nu are de unde veni și nici refacerea nu-l poate inventa.
* **Datele deja ingerate NU se repară singure.** Pentru angajamentele descărcate înainte de azi, operatorul trebuie să apese **«Refacere din istoric»** (vederea Recepții, felia 0062) — pe angajamentul cu pricina sau pe toată baza; proba (`aplica = false`) arată întâi câte linii ar adăuga. Abia după aceea `MotivulRefuzului` vede și indicatorii cu zero pe lanțurile vechi.
* Zerourile în vederea Recepții nu s-au văzut pe ecran.
* Rândurile 0062 și 0063 lipsesc încă din registrul `KBOT_STATUS.md` (worklog-urile lor există, necomise); s-a adăugat doar rândul 0064 și s-a mutat numărul liber la 0065.

---

<a id="slice-0064-02"></a>

## 0064-02 — Corrective follow-up: salvarea din editor cădea cu «'rand_istoric'»; antetele fără linii se ignoră peste tot (F32)

**Data:** 17.09.2026 (aceeași zi) **Raportat de operator:** «În AsociereForm, la salvare: `ApiException: Eroare la salvarea asocierii: 'rand_istoric'`. Se întâmplă la ORICE salvare, nu doar pe anumite rânduri. Separat: am descoperit în FX_Istoric rânduri care au doar rândul-total (fără niciun indicator). Sunt erori din vechea aplicație Access și trebuie ignorate complet, în AsociereForm și în orice alt calcul.»

Două defecte, fără legătură între ele, ambele confirmate pe cod.

### 1. Căderea la salvare — de unde venea

`'rand_istoric'` este un `KeyError` din Python, ajuns la operator prin ramura `except Exception` a lui `post_asociere` (500, `Eroare la salvarea asocierii: {e}`).

Editorul de oricând (`routes/forexe/asociere.py`) ancorează instantaneele pe `idrh` — nu există sarcină utilă, deci nu există indice de rând (scris în antetul fișierului). `normalizeaza_comenzi` pune totuși pe fiecare COMANDĂ aliasul `rand_istoric = idrh`, «ca să putem refolosi neschimbate funcțiile din prelucrare_asociere». Dar `citeste_instantanee` din același fișier NU punea aliasul pe INSTANTANEE. Felia 0058 (jurnalul `asociere.log`) a adăugat în `valideaza_plasarile` un `journal.table(...)` peste lanț care citește `i["rand_istoric"]` — iar lista de argumente se evaluează chiar dacă jurnalul e stins. `valideaza_plasarile` rulează la fiecare salvare care atinge un lanț cu cel puțin un instantaneu rămas, deci **orice salvare** cădea acolo. `materializeaza_reconstituite` (reconstituirile, F26) cheiază la fel și ar fi căzut și ea.

**Fix:** `citeste_instantanee` din `asociere.py` pune `"rand_istoric": idrh` pe fiecare instantaneu, exact ca pe comenzi. Un rând, plus docstring. Clientul VB citește GET-ul prin `GetAsociereInstantaneu`, care nu are câmpul, deci îl ignoră — nimic de schimbat în VB.

### 2. Antetele fără linii — F32

În istoric, un instantaneu = rândurile-linie (câte unul per indicator) urmate de rândul-antet (`(activ:true)`, cu totalul). Pasul 4a acumulează liniile într-un tampon și le varsă sub antet când sosește. Un antet fără nicio linie înaintea lui producea un `FX_Receptii_H` cu zero rânduri în `FX_Receptii`. Vederea Recepții îl arăta deliberat (LEFT JOIN, «un antet FĂRĂ linii nu are voie să DISPARĂ din arbore», ca în Access); editorul îl arăta; F16 îl vedea cu mulțimea de indicatori VIDĂ în mijlocul lanțului (deci «indicatorii dispar» pe un rând pe care operatorul nu l-a atins); `DIFH` îl lua ca `precedent`; `recalculeaza_final` îl putea face `Final`; trecerea automată 4c îl putea potrivi pe `Total`.

Singurul antet fără linii care E instantaneu: rândul de ștergere (F21). Rămâne.

**Criteriul, o singură dată** (`prelucrare_helpers.py`):
* `is_header_only_snapshot(este_stergere, line_rows_seen)` — pentru cele două plimbări peste istoric. Numără RÂNDURILE-LINIE văzute de la antetul precedent, nu liniile construite: o linie cu indicator necunoscut în `FX_Indicatori` (refacerea o sare și o spune) tot face antetul un instantaneu adevărat, ca ancora să rămână și linia să se poată atașa la o refacere ulterioară.
* `SNAPSHOT_COUNTS_SQL` — predicatul de citire: `(COALESCE(H.EsteStergere,0) <> 0 OR EXISTS (SELECT 1 FROM FX_Receptii L WHERE L.IDRH = H.IDRH))`, cu tabelul aliasat `H`.

**Unde se aplică:**

| Fișier | Ce |
|---|---|
| `prelucrare_pasi.step4a_populeaza_receptii` | antetul fără linii (și nu ștergere) NU se inserează; `logger.info` per antet + total; docstring F32. Rândul de istoric primește oricum `Prelucrat = 1` la pasul 7, deci nu se recitește |
| `prelucrare_pasi._DIF_H_SQL` (4d) | filtru F32 — nu mai e `precedent` pentru instantaneul real de după el |
| `receptii_refacere.refa_receptii` | nu se inserează; dacă există deja în bază rămâne (o ștergere ar trebui să ajungă la `FX_ORD.IDRH`) dar se numește în `avertismente` cu IDRH-ul; docstring |
| `prelucrare_asociere._INSTANTANEE_SQL`, `_TOATE_INSTANTANEELE_SQL` | tabloul de hotărât și contextul propunerii |
| `prelucrare_asociere._H_LANT_SQL` (`recalculeaza_final`) | antetul gol nu poate fi «ultimul» |
| `prelucrare_asociere._H_MEMBRI_LANT_SQL` (nou, în locul SELECT-ului inline din `aplica_decizii`) | membrii deja așezați ai lanțului pe care îi judecă F15/F16 |
| `asociere._INSTANTANEE_SQL` | editorul de oricând; o comandă care ar numi un antet gol cade în `verifica_blocajele` cu «nu există pe acest angajament» |
| `receptii._SQL_RECEPTII` | vederea Recepții; comentariul despre LEFT JOIN rescris: LEFT JOIN-ul rămâne PENTRU rândul de ștergere |
| `tree._SELECT` (`AreReceptii`) | un angajament cu numai antete goale nu aprinde steagul |
| `docs/FUNDAMENT_Asociere_Receptii.md` | **F32** + linia de revizie din antet (F31 și F32) |

**Neatinse, deliberat:** `_AMPRENTA_SQL` (`hc`/`hm`/`hn`) — amprenta e un hash comparat între GET și POST ale aceluiași cod, deci filtrul nu-i schimbă rostul; `_BLOCAJE_SQL` — un antet gol niciodată returnat nu poate fi comandat; `_R_DEMARCHEAZA_SQL` — privește doar rândurile de ștergere; `ord_edit._SQL_REC_ANT` și `_DIF_R_SQL` — pornesc din LINII, deci antetele goale nu contribuiau oricum. Nicio linie VB schimbată.

### 3. Teste

* `test_forexe_asociere.py` (+2): `citeste_instantanee` din editor poartă `rand_istoric == idrh` și trece prin `valideaza_plasarile`; **cap la cap** — lanț de DOUĂ instantanee, se desprinde unul, POST-ul ajunge la commit (200). Amândouă **verificate că pică fără fix** (aliasul scos temporar: `KeyError: 'rand_istoric'`, respectiv 500 — exact eroarea operatorului). Cu un singur instantaneu lanțul rezultat e gol și defectul nu se vedea — de-asta două. Falsul `FakeConnection` a învățat `UPDATE`, `_H_LANT_SQL` și `_RECONSTITUITE_SQL`; dispecerizarea pe prefix deosebește acum `_BLOCAJE_SQL` de `_INSTANTANEE_SQL` (ambele încep cu `SELECT H.IDRH`) după ` AS ord_h`.
* `test_forexe_prelucrare_pasi.py` (+4): antetul fără linii nu produce H; antetul gol nu fură liniile antetului următor și nu consumă `NrCrt`; criteriul scutește ștergerea; **paza**: toate cele opt citiri ale lui `FX_Receptii_H` conțin `SNAPSHOT_COUNTS_SQL`. Două teste vechi aveau ca fixtură incidentală chiar un antet fără linii — au primit o linie; cel cu «zero fără indicator» asertează acum că singura linie scrisă e cea reală.
* `test_forexe_receptii_refacere.py` (+2): antetul gol nici nu se inserează, nici nu se numără «lipsă», și e numit în avertismente; unul deja în bază rămâne neatins și e numit cu IDRH. Cheia moartă `SELECT IDRH, Total FROM FX_Receptii_H` din falsul cursor actualizată la forma nouă (dispecerul cade pe `[]` pentru prefixe necunoscute, deci era inertă, dar mințea).

PYTHON (`.venv`, `KBOT_ASOCIERE=0`): suita întreagă **546 trecute / 26 sărite / 0 picate** (de la 538/26; +8 teste).

### 4. Ce rămâne neverificat

* **Nimic n-a rulat pe MariaDB.** Că `EXISTS (SELECT 1 FROM FX_Receptii L WHERE L.IDRH = H.IDRH)` e ieftin pe volumul real nu s-a măsurat; `FX_Receptii.IDRH` e FK, deci ar trebui indexat.
* **Salvarea din AsociereForm nu s-a apăsat pe ecran** după fix; drumul e acoperit cap la cap doar de testul cu falsul de conexiune.
* **Antetele goale deja în bază rămân în `FX_Receptii_H`**, nevăzute. Nu se șterg: un `DELETE` ar trebui să se uite la `FX_ORD.IDRH` și la orice altceva îi ține cheia. Refacerea le numește în avertismente, ca operatorul să știe că sunt.
* **Instantaneele ingerate înainte de F31 ale căror singure linii erau zerouri** arată, în `FX_Receptii`, exact ca un antet gol și sunt ascunse de același filtru până la «Refacere din istoric» — reparația pe care 0064 o cerea deja. Până n-o apasă operatorul, pe angajamentele vechi pot lipsi din editor instantanee reale «fără schimbare».

---

<a id="slice-0065"></a>

## SLICE-0065 — Recepția valorează ultimul ei antet; rădăcina «Toate recepțiile»; DIF după salvarea din editor; perechile unice se așază singure

**Data:** 17.09.2026 **Cerere (operator, aceeași zi), pe vederea Recepții și pe editorul de legături:**

> 0. să aibă un rând-rădăcină «Toate Recepțiile»;
> 1. la o recepție selectată să arate DOAR ultimii indicatori — nu totalul;
> 2. să arate totalul DOAR al ultimului H al fiecărui R;
> 3. tooltip-ul fiecărui nod (R) să arate și câmpul Descriere;
> 4. totalul recepțiilor pe ultimul R din JSON iese greșit: ar trebui 29.645, tooltip-ul și
>    OrdonantareForm arată 25.410 (lipsește una);
> 5. la deschiderea AsociereForm, dacă se găsesc valori unice pentru R și H, să le asocieze
>    singur — «ia-i din muncă».
>
> Apoi: **fără teste, fără git.**

Numărul feliei: 0065 = «Next free» din STATUS. Presupunere, nu confirmare — se poate renumerota.

### 1. Cauza lui (4): pasul 4d nu rula după salvarea din editor

În JSON-ul operatorului (`AAB2KRCC64T`, `027_SCGM`), antetul `IDRH 242` (recepția `IDRR 187`) are `DIFH NULL`, iar liniile lui 476/477 au `DIF NULL`. Celelalte șase recepții au `DIFH = 4.235`. Tooltip-ul din Recepții însuma `DIFH` pe anteturi (`qFX_MAIN_REC_TT_DIFH`), iar `qFX_ORD_REC_ANT` (ordonanțarea) însumează `DIF` pe linii: `NULL` -> 0 pe amândouă drumurile, deci 6 × 4.235 = 25.410.

De ce e NULL: `DIFH`/`DIF` nu vin de pe site, le calculăm noi (pasul 4d, `step4d_calculeaza_dif`) — F20 retras spune exact asta. Ingestia îl rulează după 4c (`_pas4d_pe_receptiile_atinse`) și refacerea din 0062 după ce a scris. **Editorul de oricând (`routes/forexe/asociere.py`, `aplica_comenzi`) nu îl rula deloc**: recalcula `Final`/`Partial` și F28, dar nu diferențele. Orice instantaneu așezat de acolo (sau o recepție pornită de acolo, 0059 — `IDRR 187` are `Incarcat NULL`, semnul unui rând scris de editor) rămânea invizibil pentru ambele totaluri.

**Fix:** `aplica_comenzi` cheamă `step4d_calculeaza_dif(cursor, cod, idrr)` pentru fiecare lanț atins (`de_recalculat`), după `recalculeaza_final`. Datele deja stricate se repară de la sine la următoarea salvare din editor pe acel angajament, la orice «Reîmprospătare» (ingestia rulează 4d pe TOATE recepțiile angajamentului) sau la «Refacere din istoric» pe lanțurile atinse.

### 2. Vederea Recepții — o recepție VALOREAZĂ ultimul ei instantaneu

Lanțul unei recepții = instantaneele ei în ordinea `DataH`, fiecare cu valoarea ÎNTREGII recepții la acel moment (F3). Ce e adevărat acum e ULTIMUL antet. Vederea le calcula altfel: totalul nodului R era `SumaAntet` al rândului R, grila aduna `Valoare` peste TOATE anteturile lanțului (deci `AAB` = 4.235 + 0 pe `IDRR 180`, când ultimul antet spune 0), iar tooltip-ul aduna `DIFH`-uri.

`ReceptiiView.vb` acum:

| Cerere | Ce s-a făcut |
|---|---|
| 0 | Rădăcina `ROOT_KEY = "all"`, «Toate recepțiile ~~~ total», bold, deschisă; lunile sub ea; dosarul «Instantanee neașezate» rămâne la nivelul de sus, după ea. Tooltip de reconciliere pe rădăcină: numărul recepțiilor, cumul întreg vs. TOATE plățile. |
| 1 | `LiniileDeAratat`: pe orice nod, liniile ultimului antet CU linii al fiecărei recepții (`LiniileUltimuluiAntet`), apoi gruparea pe clasificație. Pe un nod R = exact indicatorii ultimului antet. Rândul-total sintetic rămâne scos (2026-08-13). |
| 2 | `ValoareaReceptiei` = `Total` al ultimului antet (`UltimulAntet`: `DataH` desc, `IDRH` desc, fără `StersH`). Eticheta R, iconița ▲/▼, totalul lunii și al rădăcinii (`TotalReceptii`) pornesc toate de aici; `SumaAntet` nu mai e citit de arbore. |
| 3 | Rândul «Descriere» în tooltip-ul de recepție: `R.Descriere` (nou pe fir, `descriere_r`), altfel `Descriere` al ultimului antet. |
| 4 | Cumulul «Recepții cumulate» = suma rulantă a `ValoareaReceptiei`, nu a `DIFH` (`SumDistinctAntetDifh` scos). Pe lanț complet cele două coincid; pe `DIFH NULL` doar a doua e adevărată — 29.645 pe JSON-ul operatorului. `Difh` rămâne pe `ReceptieRow`, necitit de vedere. |

Rândul de ștergere (F21) e ultimul antet al unei recepții șterse și nu are linii: totalul lui e valoarea recepției la ștergere (deci cumulul nu se schimbă față de înainte), liniile se iau din ultimul antet care are, iar eticheta R primește «[ștearsă]» (`este_stergere` nou pe fir). F22 («ascunsă din arbore») rămâne neimplementată ca înainte — nu s-a cerut aici.

Pe fir (`receptii.py`): `descriere_r` (`R.Descriere`) și `este_stergere` (`COALESCE(H.EsteStergere,0)`); `GetReceptieRow`, `ApiClient.GetReceptiiAsync`, `ReceptieRow` (`DescriereR`, `EsteStergere`) le poartă.

### 3. AsociereForm — perechile cu valoare unică se așază singure

`AsazaPerechileUnice`, chemată în `ReincarcaAsync` după citirea tabloului și înainte de `Reconstruieste`, DOAR în `ModAsociere.Oricand` (în propunere sugestiile le face serverul, 4c):

* candidații din coș: neașezate, neblocate, nemarcate «fără schimbare», nu rânduri de ștergere;
* recepțiile: cele neșterse pe site;
* valoarea rotunjită la ban; se ia o pereche NUMAI când valoarea apare pe un singur instantaneu din coș ȘI pe o singură recepție (`SumaAntet`). Valoarea nu e cheie (F5) — de aceea unicitatea pe amândouă laturile, nu potrivirea: șapte recepții de 4.235 cu două instantanee de 4.235 nu se ating;
* vetourile F14/F16 se aplică prin `MotivulRefuzului`, ca la o tragere;
* rezultatul e o MUTARE LOCALĂ (`_pozitie`), nesalvată: intră în `Comenzi()`, se vede în arbore, se poate trage înapoi. Banda spune câte s-au așezat (`TextAsezareAutomata`) și cere verificarea.

### 4. Fișiere atinse

| Fișier | Ce |
|---|---|
| `PYTHON/routes/forexe/asociere.py` | import `step4d_calculeaza_dif`; 4d pe fiecare lanț atins în `aplica_comenzi` |
| `PYTHON/routes/forexe/receptii.py` | `descriere_r`, `este_stergere`; docstring |
| `PYTHON/tests/test_forexe_asociere.py` | falsul răspunde la `_DIF_H_SQL`/`_DIF_R_SQL`; `test_a_save_recomputes_dif_on_every_touched_chain` (pică fără fix — scris ÎNAINTE de «fără teste», lăsat) |
| `PYTHON/tests/test_forexe_receptii.py` | cheile noi în `ROW_KEYS` + două asserturi (host-only, sare aici) |
| `src/KBot.Api/UpsertAngajamenteRequest.vb`, `ApiClient.vb` | DTO + mapare |
| `src/KBot.Domain/ReceptiiInfo.vb` | `DescriereR`, `EsteStergere`; antetul fișierului |
| `src/KBot.App/Views/ReceptiiView.vb` | rădăcina, ultimul antet, grila, tooltip-urile, `[ștearsă]` |
| `src/KBot.App/Forexe/AsociereForm.vb` | `AsazaPerechileUnice`, `TextAsezareAutomata`, apelul din `ReincarcaAsync` |

Neatinse deliberat: `ord_edit._SQL_REC_ANT` (rămâne `SUM(DIF)`, ca în Access — cu 4d rulat corect cifrele lui sunt bune); `Difh` pe `ReceptieRow` (pe fir, necitit); `KBotDataView`.

### 5. Verificări

* `dotnet build src\KBot.App\KBot.App.vbproj`: **0 erori, 0 avertismente** (cele MSB3825 din `.resx` sunt de dinainte). `tests\KBot.App.Tests` **compilează**.
* PYTHON (`.venv`, `KBOT_ASOCIERE=0`): `test_forexe_asociere.py`, `test_forexe_receptii.py`, `test_forexe_prelucrare_pasi.py` — **69 trecute / 1 sărit**. Suita întreagă NU a rulat («fără teste»).

### 6. Neverificat / amânat

* **Nimic pe ecran, nimic pe MariaDB.** Arborele pe trei niveluri, rândul «Descriere» din tooltip și banda «așezate automat» nu s-au văzut nici prin `DrawToBitmap`.
* **Testele VB din `ReceptiiViewTests.vb` NU s-au actualizat** (cerere «fără teste»): compilează, dar cele care numără `t.Items` (acum 1 rădăcină + dosar) sau așteaptă cumulul din `DIFH` (Februarie: 3.480,43 -> 6.344,55 cu regula ultimului antet) vor pica la rulare. Tot așa `AsociereFormTests.vb`: `StandardStare` are H31 = 200 unic față de R2 = 200 unic, deci se așază singur la încărcare — `FaraNiciOSchimbare_NuPleacaNicioComanda` și `Incarcarea_AsazaFiecareInstantaneuUndeIlAreServerul` vor pica; `StareCuNeasezatInaintePlatii` la fel (H31 = 250 unic, R2 = 250 unic). Fixturile trebuie mutate pe valori nepotrivite, cu teste noi pentru așezarea automată — de făcut într-o rundă cu teste.
* Datele operatorului: `IDRH 242` rămâne cu `DIFH NULL` până la prima salvare din editor pe angajament, o reîmprospătare sau o refacere; vederea Recepții e corectă și așa (nu mai citește `DIFH`), ordonanțarea NU (citește `DIF`).
* Nimic comis («fără git»).

---

<a id="slice-0068"></a>

## SLICE 0068 — Ancora pe id-ul de istoric (F34) și rândul de istoric consumat o singură dată (F33)

**Data:** 18.09.2026 **Cererea operatorului:** «something is happening and not all receptiiH are getting associated with receptiiR on a new download … it did the 25.08.2026 receptie but it didn't do the 27.07.2026 … if i manually open the asociere form, it settles the problem automatically. what's going on?» — apoi «go ahead with both changes».

### Ce s-a găsit

Descărcarea `AAB2KPRT2EB` (workflow `PrelucrareCompletaReverse`) a salvat cu avertismentul «1 instantanee neasociate nu au rândul lor de istoric în această descărcare și nu pot fi rezolvate acum». Două cauze, una peste alta:

1. **Ancora F24 nu acoperă rândurile vechi sub fluxul REVERSE.** `citeste_instantanee` punea un instantaneu neașezat în tabloul de decis DOAR dacă `IDH`-ul lui era în `TabelIstoric` din sarcina utilă (indicele rândului e ancora deciziei). Dar `adlop - Prelucrare Completa Reverse.wfl` merge înapoi prin paginile de istoric și se oprește la `DATA_IESIRE` (`exitIfCellEquals`, `startFromLast` în `WorkflowExecutor.Actions.ScrapeTable.vb`), deci `TabelIstoric` e o DIFERENȚĂ, nu istoricul întreg — presupunerea din docstring-ul pasului 3a («FOREXE trimite istoricul intreg la fiecare descarcare») era adevărată doar la prima descărcare. Orice instantaneu rămas neașezat dintr-o rulare mai veche era, prin construcție, de nerezolvat din descărcare; editorul de oricând îl vedea fiindcă ancorează pe `IDRH`.
2. **Un rând de istoric cu `Prelucrat = 0` care nu a fost inserat de rularea curentă nu era marcat niciodată.** Interogarea operatorului a arătat rândul `IDH 5786` (recepția 27.07.2026) cu `Prelucrat = 0`, restul 1 — venit pe altă cale (migrare). Pasul 4a îl citea la fiecare descărcare și îi năștea un `FX_Receptii_H` nou, neașezat (`FX_Receptii_H: 2` în mesaj, cu un singur antet în payload), iar cauza 1 îl făcea de neașezat din descărcare.

### Ce s-a schimbat și de ce

#### Server (`PYTHON/routes/forexe/`)

- **`prelucrare_pasi.py` — F33.** `step4a_populeaza_receptii` întoarce acum `(antete, ids_consumate)`: nu mai scrie un antet al cărui `IDH` are deja `FX_Receptii_H` (`_H_EXISTA_IDH_SQL`; liniile din tampon se aruncă, nu se toarnă a doua oară) și raportează fiecare rând consumat (antete scrise, antete sărite F32/F33, liniile turnate sub un antet). Liniile rămase fără antet la sfârșit NU sunt consumate. `step7_actualizeaza_rezolvat` primește `ids_noi ∪ ids_consumate` (`prelucrare.py`). Docstring-ul pasului 3a corectat.
- **`prelucrare_asociere.py` — F34.** `ancora(x)` / `ancora_text(a)`: numele unui instantaneu e `("rand", indice)` când rândul de istoric e în payload, altfel `("idh", FX_Istoric.ID)`. `citeste_instantanee` nu mai lasă afară rândurile fără indice — le pune cu `rand_istoric = None` și `idh`; rămân afară (numărate, avertizate) doar cele fără `IDH` și dublurile pe același `IDH` (resturi F33). `normalizeaza_decizii` cere exact una dintre `rand_istoric` și `idh` (`null` = absent). `verifica_acoperirea`, `materializeaza_reconstituite`, `aplica_decizii` cheiază pe ancoră; jurnalul arată «rândul 3» / «istoric 5786». Membrii de lanț deja legați nu mai poartă `rand_istoric = -1`, ci `None`. `MOTIV_FARA_ISTORIC` rescris pentru cele două cazuri rămase.
- **`prelucrare.py`.** Propunerea trimite pe fiecare instantaneu `rand_istoric` (sau `null`), `idh` și `idrh` (id-ul fazei întâi — DOAR cheia dicționarelor formularului, nu se trimite înapoi).
- Editorul de oricând (`asociere.py`) NU s-a schimbat: aliasul `rand_istoric = idrh` dă aceeași ancoră pe ambele laturi.

#### Client

- **`KBot.Domain/AsociereInfo.vb`.** `AncoraAsociere.Cheie/Text`; `InstantaneuPropus.RandIstoric` devine `Integer?`, plus `Idh`, `Idrh`, `Ancora()`; `DecizieAsociere.RandIstoric` devine `Integer?`, plus `Idh As Integer?`, `Ancora()`; `AsociereDosar.EsteComplet` compară ancore.
- **`KBot.Domain/AsociereStare.vb`.** `InstantaneuLegat.RandIstoric As Integer?`. `DinPropunere`: cheia rândurilor de decis e `IDRH`-ul propunerii (pozitiv, unic), contextul rămâne `-IDRH`; ancora călătorește separat pe `RandIstoric` / `Idh`. Până acum cheia ERA indicele, ceea ce nu lăsa loc pentru un al doilea fel de ancoră.
- **`KBot.Api/UpsertAngajamenteRequest.vb`, `ApiClient.vb`.** DTO-urile poartă `rand_istoric?` / `idh?` (decizie) și `rand_istoric?` / `idh?` / `idrh?` (propunere); `CatreFir` trimite exact una dintre ele (`_jsonFaraNull` omite pe cea nepusă).
- **`KBot.App/Forexe/AsociereForm.vb`.** `DeciziiDin` citește ancora din `inst.RandIstoric` / `inst.Idh`, nu din cheie; mesajul «nu are nicio hotărâre» numește ancora.

#### Documente

- `docs/FUNDAMENT_Asociere_Receptii.md`: F33 și F34 adăugate, antetul revizuit.

### Fișiere atinse

`PYTHON/routes/forexe/prelucrare_pasi.py`, `prelucrare_asociere.py`, `prelucrare.py`; `PYTHON/tests/test_forexe_prelucrare_pasi.py`, `test_forexe_prelucrare_asociere.py`, `test_forexe_prelucrare_route.py`; `src/KBot.Domain/AsociereInfo.vb`, `AsociereStare.vb`; `src/KBot.Api/UpsertAngajamenteRequest.vb`, `ApiClient.vb`; `src/KBot.App/Forexe/AsociereForm.vb`; `tests/KBot.App.Tests/AsociereDeciziiTests.vb`, `tests/KBot.Domain.Tests/AsociereStareDinPropunereTests.vb`, `tests/KBot.Api.Tests/AsociereApiClientTests.vb`; `docs/FUNDAMENT_Asociere_Receptii.md`; `docs/worklog/KBOT_STATUS.md`; acest fișier.

### Teste

- **Scrise, nu rulate** (regula casei: nicio suită nu se rulează și nu se construiește). Python: 3 teste noi pe 4a (F33: antet cu instantaneu existent, tamponul nu se scurge în antetul următor, liniile fără antet nu se consumă) + cele existente adaptate la tuplul întors; 8 teste noi pe ancoră (`ancora`, forma deciziei, acoperire prin `idh`, `citeste_instantanee` pe cazul real 5786/6253, dubluri, fără IDH). VB: `AsociereDeciziiTests` (+2), `AsociereStareDinPropunereTests` (cheie ≠ ancoră, +1), `AsociereApiClientTests` (+1, decizia a 5-a prin `idh`).
- `dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: **0 erori, 0 avertismente BC** (doar cele 7 `MSB3825` preexistente). Modulele Python s-au importat și `ancora` / `normalizeaza_decizii` / `verifica_acoperirea` s-au exercitat de mână pe cazul 5786.

### Neverificat / amânat

- Proiectele de test VB nu s-au construit (regulă); modificările din ele sunt mecanice (`.Value`, câmpuri noi în obiecte), dar necompilate aici.
- Nimic rulat cap la cap și nimic pe ecran. Prima descărcare reală după felia asta ar trebui să arate, pe `AAB2KPRT2EB`: `FX_Receptii_H: 0` (antetul 5786 sărit F33), rândul 5786 marcat `Prelucrat = 1`, și niciun avertisment despre istoric.
- **Dublurile deja născute** înainte de felie (două `FX_Receptii_H` pe același `IDH`, dacă există în alte angajamente) rămân în bază; ingestia le lasă afară cu avertisment, editorul le vede. O curățare ar trebui să atingă `FX_ORD.IDRH` — nu s-a scris.
- Un dosar de asociere (`AsociereStore`) salvat cu forma veche a propunerii (fără `idrh`/`idh`) ar da chei 0 la `DinPropunere`; amprenta îl respinge oricum la salvare.
- Liniile de recepție inserate în rularea curentă fără antet sunt în continuare marcate `Prelucrat = 1` prin `ids_noi` (purtare veche, contrazisă de comentariul din 4a); nu s-a atins, fiindcă REVERSE nu desparte niciodată o secundă.

## Repere unice din registru

- La 17.09, registrul și jurnalul 0064 semnalau lipsa rândurilor 0062/0063. Consolidarea adaugă navigarea 0062 din jurnalul existent; afirmația veche rămâne reper istoric, fără a inventa rezultate pentru App.Tests sau o probă MariaDB.
