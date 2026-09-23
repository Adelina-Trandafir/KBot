# SLICE 0076 — Editările din «Browser FOREXE»: recepția salvată, rezervările ținute în memorie, marcajele de id

**Data:** 23.09.2026
**Cererea operatorului (rezumat):**

1. Fluxul de recepții rulat după o editare în vederea «Browser FOREXE» **nu pornește de la
   început** (operatorul e deja pe tabul Recepții), citește **doar recepția editată** (identificată
   prin data ei, reținută la clicul pe ochiul rândului) sau **doar ultima** pentru o recepție
   nouă, apasă «Istoric» și citește istoricul **ca fluxul REVERSE**. Modificarea unei recepții
   trebuie și detectată (ochiul din fiecare rând).
2. Fluxul de rezervări: **fiecare editare ținută în memorie** (tot ce ia partea de rezervări din
   «Prelucrare Completa»), fără preambul, **întrebare după fiecare salvare** dacă s-a terminat;
   la «DA», memoria devine exact ce ar fi întors fluxul normal (ca să nu se mai citească o dată),
   plus istoricul citit ca REVERSE.
3. Marcajele: `(IDREV: x)` la sfârșitul motivului unei rezervări, `(IDRH: x; IDR: y)` în
   descrierea unei recepții, ca istoricul să spună direct cui îi aparține fiecare rând (rezervarea
   în `Descriere`, recepția în `Observatii`-le rândului de total).

Răspunsurile operatorului la cele trei întrebări puse înainte de scris:
**(a)** marcajele: *rezervare + scriere + citire* (serverul rezervă id-urile, pagina le scrie,
ingestia le folosește); **(b)** «șirul utilizabil» = ce întoarce fluxul normal de rezervări, ca să
nu se mai citească încă o dată; **(c)** fișiere `.wfl` **noi** pentru browser, iar cele vechi își
păstrează preambulul (iconițele din subsolul arborilor le folosesc cu browserul oriunde).

## Ce s-a schimbat și de ce

### 1. Două fluxuri noi, fără preambul (`Workflows\`)

- **`adlop - Receptie Editata.wfl`**: antetul (.well), tabul Recepții, `ScrapeTable` pe listă,
  apoi `ForEachVar` care deschide **numai** rândul-țintă: `{{RECEPTIE_TINTA}}` = `ULTIMA` (recepție
  nouă → `[[R.IsLast]]`) sau data `zz/ll/aaaa` (recepție editată → `[[R.Data]]`). Fiecare rând
  poartă în `collectFields` și **`Citita`** (1 / 0). Pe ramura necitită se golesc cele patru
  variabile, ca în fluxul complet (altfel rândul ar lua `Detaliu`-ul vecinului). Apoi tab0 →
  «Istoric» → citire **de la ultima pagină înapoi** până la `{{DATA_IESIRE}}` (exact
  `Istoric Angajament REVERSE.wfl`) → «Înapoi» → **înapoi pe tabul Recepții**.
- **`adlop - Rezervari Editate.wfl`**: antetul + istoricul REVERSE + «Înapoi». Indicatorii NU se
  citesc: vin din memorie (§3).
- **`DATA_IESIRE` fără istoric local** = `(?!)` (`WorkflowCatalog.DataIesireNiciuna`), un tipar
  care nu se potrivește cu nimic, deci se citește tot istoricul. Gol ar fi fost invers: `^` se
  potrivește cu orice rând, deci citirea s-ar fi oprit la primul.
- **`adlop - Receptii Angajament.wfl`**: preambulul comentat de operator (V.3) **pus la loc**, cu
  nota V.4 — fișierul e rulat și de iconița din subsolul arborelui de recepții, care se apasă cu
  browserul oriunde (decizia (c)).
- `WorkflowCatalog`: `ReceptieEditataFile`, `RezervariEditateFile`, `VarReceptieTinta`,
  `ReceptieTintaUltima`, `DataIesireNiciuna`. `JobBuilder.BuildReceptieEditata(cod, data?, ultimaData?)`,
  `BuildRezervariEditate(cod, ultimaData?)`, `PuneDataIesire`.

### 2. Pagina (`ForexeWatch.js`, secțiunile 12 și 13)

- **`receptie-modificare` are reguli** (nu mai e TODO): pornește la ochiul unui rând din tabul
  Recepții (`li.tab1.active`, același ochi pe care îl apasă «Prelucrare Completa»), se salvează cu
  același «Salvează». La clic se reține **data rândului** (`rowDate`), la salvare **data din
  formular** (`formDate`, `input[name='data']`, normalizată la `zz/ll/aaaa`); evenimentul
  `finished` duce `data.dataReceptie` = data din formular, altfel cea a rândului.
- **Rezervarea**: la pornire se citește lista indicatorilor din tab0 (`rezBefore`) și, la ochi,
  codul rândului (`rezIndicator`); la clicul pe salvare se citește **tabelul de buget** de pe
  pagina de editare (exact tabelul pe care îl răzuiește fluxul complet în spatele ochiului); la
  confirmare, pe tab0, se citește **rândul indicatorului** (după cod; la «Adaugă» — rândul al cărui
  cod nu era în listă). `data = {indicator, indicatorCod, buget}`.
- Tabelele se citesc cu o **copie** a lui `ScrapeTableExtract.js` (`scrapeTable`), ca rândul să
  aibă exact cheile dintr-un pachet descărcat (`Indicator_ang`, …, `Col_12`).
- **Formular închis fără salvare** («Înapoi» al paginii, alt tab): la următoarea bătaie de 2 s
  operațiunea se încheie ca la «Renunță» (`checkAbandoned`, după ce formularul a fost văzut o dată).
  Altfel o recepție doar deschisă pentru privit ar fi blocat orice pornire ulterioară.
- **Marcajele**: clicul pe «Continuă» din modalul motivului / pe «Salvează» din formularul
  recepției e **ținut** (după gardianul salvărilor fără modificări, înaintea urmăririi), pagina
  cere marcajul prin `_kbotWatchCallback` (`event: 'marcaj'`), .NET răspunde cu
  `window._kbotWatch.setMarcaj(id, text, motiv)`, marcajul se pune la sfârșitul textului
  (**înlocuind** unul mai vechi — o recepție editată își păstrează descrierea) și clicul e
  **reluat**. Fără răspuns în 8 s, sau cu eroare, salvarea pleacă **fără** marcaj și consola spune
  de ce: munca operatorului nu e niciodată ținută ostatică.

### 3. .NET

- `ForexeWatchEvent.DetaliiJson` (obiectul `data` al paginii, text JSON); `WorkflowExecutor`
  tratează `marcaj` separat (nu e operațiune, nu ajunge în shell): `SetMarcajProvider`,
  `RaspundeLaMarcaj` (răspunde **întotdeauna**, cu marcajul sau cu motivul).
- `ForexeRunner` primește în constructor puntea `Func(tip, cod, ct) → marcaj` (ca la Excel);
  `Program.vb` o înregistrează peste `IMarcajApi` (nou, `KBot.Api`, implementat de `ApiClient`
  într-un fișier parțial — **ținut în afara lui `IApiClient`**, altfel toate cele zece falsuri
  din teste ar fi trebuit atinse).
- `ForexeController`: `DescarcaPartialAsync` primește acum o **fabrică de job** (rulată în
  interiorul porții, fiindcă citirea istoricului local folosește tokenul operației) și o
  **transformare** a pachetului. Noi: `DownloadReceptieEditataAsync(cod, data?, citesteIstoric)` și
  `DownloadRezervariEditateAsync(cod, randuri, citesteIstoric)`.
- `WorkflowResultStore`: `DoarReceptiileCitite` (rândurile cu `Citita = 1`, fără coloana
  `Citita`; lista brută tăiată după datele păstrate), `CuIndicatoriMemorati` (pune
  `TabelIndicatori_results` — rând + `BugetIndicator` imbricat — și `TabelIndicatori`, adică exact
  cele două tabele ale secțiunii 1 din «Rezervari Angajament»).
- **`KbotForm.ForexeWatch.vb`**:
  - recepție nouă → `DownloadReceptieEditataAsync(cod, Nothing)` (ultimul rând); recepție editată →
    cu data din pagină; fără dată → reîmprospătarea tuturor recepțiilor (mai lent, niciodată greșit),
    spus pe consolă;
  - rezervare → **nu se mai descarcă la fiecare salvare**: rândul se pune în `RezervariInLucru`
    (pe cod de angajament, un rând pe indicator, ultima salvare câștigă) și se întreabă
    «Ați terminat modificarea rezervărilor?». **NU** = se continuă. **DA** =
    `DownloadRezervariEditateAsync` + ingestia în două faze; memoria se golește doar după o
    descărcare reușită.

### 4. Serverul

- **`routes/forexe/marcaj.py`** (nou) — `POST /api/forexe/marcaj/rezerva` `{tip, cod}`:
  - **rezervarea unui număr = avansarea contorului AUTO_INCREMENT** al tabelei: un rând-substitut
    inserat și șters în aceeași tranzacție. InnoDB nu mai dă niciodată acel număr, deci un
    `INSERT` ulterior care îl numește explicit nu se poate ciocni cu unul automat. Numărul se
    notează și în `FX_NumberLock` (Tip `IDREV` / `IDRH` / `IDR`, codul angajamentului, cine, până
    când) — doar evidență;
  - `rezervare` → **același IDREV** cât timp lacătul angajamentului e viu (o sesiune de editare a
    rezervărilor = o revizie DDF), TTL 30 de zile, prelungit la fiecare cerere; `receptie` → o
    pereche nouă la fiecare salvare, TTL 2 zile;
  - `id_marcaj_utilizabil` (id-ul nu e deja în tabelă ȘI e sub contor — deci chiar a fost dat de
    contor), `consuma_lacatul`, `idrev_tinut`.
- `prelucrare_helpers`: `compune_marcaj`, `extract_marcaj` (ultimul marcaj din text),
  `fara_marcaj` (un text **fără** marcaj se întoarce neatins).
- Ingestia (`prelucrare_pasi.py`):
  - **3b**: `(IDREV: n)` din `Descriere` (sau `Observatii`) câștigă în fața vechiului `(REV:nn)`;
    se scrie în `FX_Istoric.IDREV` chiar dacă revizia nu există încă (coloana n-are cheie
    străină); 3c/3d scriu deja NULL + avertisment în `FX_Rezervari` cât timp lipsește;
  - **4a**: antetul cu `(IDRH: n; IDR: m)` naște `FX_Receptii_H` **cu IDRH = n**, iar prima lui
    linie `FX_Receptii` cu **IDR = m** (restul liniilor pe contor); un marcaj nefolosibil e ignorat
    și spus în jurnal; `Descriere` se scrie fără marcaj;
  - **4b**: marcajul iese din `DescriereReceptie` înainte de hash și de `FX_Receptii_R.Descriere`.
- **Salvarea DDF** (`ddf_edit.py`): o revizie **nouă** a unui angajament care are un IDREV ținut
  se naște **cu acel IDREV** (lacătul se consumă), iar pasul nou **8.1b** leagă rezervările
  nelegate ale căror rânduri de istoric poartă acel IDREV.

### 5. O reparație găsită pe drum (felia 0060)

`WorkflowResultStore.FaraReceptiileSarite` tăia doar `ListaReceptii`, dar serverul citește
**`ListaReceptii_results`** — deci recepțiile nebifate ajungeau totuși la pasul 4b cu `Detaliu`
gol, adică exact recepția pe jumătate actualizată pe care tăietura trebuia s-o împiedice. Acum se
taie ambele tabele.

## Fișiere atinse

- `src/KBot.Forexe/Workflows/adlop - Receptie Editata.wfl` (nou), `adlop - Rezervari Editate.wfl`
  (nou), `adlop - Receptii Angajament.wfl` (preambul pus la loc), `KBot.Forexe.vbproj` (copiere
  + 1.0.12 ▸ **1.0.13**), `WorkflowCatalog.vb`, `JobBuilder.vb`, `ForexeRunner.vb`,
  `Executor/WorkflowExecutor.Watch.vb`, `Models/ForexeWatchEvent.vb`,
  `Services/JavaScripts/ForexeWatch.js`
- `src/KBot.Api/IMarcajApi.vb` (nou), `ApiClient.Marcaj.vb` (nou), `KBot.Api.vbproj`
  (1.0.6 ▸ **1.0.7**)
- `src/KBot.App/Program.vb`, `Forexe/ForexeController.vb`, `Forexe/WorkflowResultStore.vb`,
  `KbotForm.ForexeWatch.vb`, `KBot.App.vbproj` (1.0.36.5 ▸ **1.0.37.0**)
- `PYTHON/routes/forexe/marcaj.py` (nou), `__init__.py`, `prelucrare_helpers.py`,
  `prelucrare_pasi.py`, `ddf_edit.py`
- `docs/worklog/KBOT_STATUS.md`, acest fișier

## Rezultatele testelor

- `dotnet build` pe `src\KBot.Api`, `src\KBot.Forexe`, `src\KBot.App` (`--no-incremental`),
  `src\KBot.DevHarness` (`--no-incremental`): **0 erori, 0 avertismente**. Cele două `.wfl` noi
  apar în `bin\...\Workflows\`.
- Toate cele 11 `.wfl` se parsează ca XML; `node --check ForexeWatch.js`: valid.
- Python: pachetul `routes.forexe` se importă cu `.venv`; `compune_marcaj` / `extract_marcaj` /
  `fara_marcaj` exersate de mână (`(IDREV: 112)`, `(IDRH: 631; IDR: 2570)`, un text fără marcaj
  rămâne identic, `(REV:3)` nu e luat drept marcaj, `extract_numar_rev` nu vede `(IDREV:`).
- **Nicio suită xUnit / pytest construită sau rulată** (cerere explicită: «no TESTS»). Interfețele
  pe care le implementează falsurile din teste (`IApiClient`, `IForexeRunner`) **nu s-au schimbat**.
- Regula 0: diacritice numai în textele pe care le citește operatorul (verificat pe diff).

## Neverificat / amânat

- **Nimic nu a atins CABWeb, un browser adevărat sau MariaDB.** De confirmat la prima sesiune:
  - că ochiul din tabul Recepții deschide «Modifică recepție» și că `input[name='data']` există
    în formular (altfel rămâne data rândului);
  - ordinea listei de recepții — «ultima» = **ultimul rând** al tabelului, cum a cerut operatorul;
    dacă FOREXE sortează altfel, o recepție nouă cu dată mai veche ar fi citită greșit;
  - că rândul de total al recepției are descrierea (cu marcaj) între `Receptie: ` și prima
    virgulă a `Observatii`-lor (cum o citește deja pasul 4a) și că FOREXE nu taie textul;
  - că motivul rezervării ajunge în `FX_Istoric.Descriere` (afirmația operatorului, nevăzută);
  - că reluarea clicului (`button.click()`) e acceptată de Wicket ca un clic obișnuit;
  - că tabelul de buget de pe pagina de editare e cel din fluxul complet (`table.table-bordered.table-hover.table-condensed`)
    și că `input[name^='tableContainer:']` NU apare și pe lista din tab0 (altfel închiderea fără
    salvare a unei rezervări nu se detectează — inofensiv, doar nu se curăță singură).
- **Drepturile contului serverului**: rezervarea șterge rânduri din `FX_DDF_REV`, `FX_Receptii_H`,
  `FX_Receptii` și citește `information_schema.TABLES`. Nevăzut dacă contul le are pe fiecare bază.
- **Persistența contorului**: se sprijină pe MariaDB ≥ 10.2.4 (contorul InnoDB supraviețuiește
  repornirii). Versiunea serverului e 10.11 după felia 0075-00 — deci în regulă, dar nescris nicăieri
  ca verificat.
- **Memoria rezervărilor trăiește cât shell-ul.** La închiderea K-BOT cu rezervări nepreluate ele
  rămân în FOREXE (și în istoric), dar nu în K-BOT până la o reîmprospătare a nodului. Nu se
  întreabă la închidere.
- O rezervare pe un **indicator nou** al cărui rând n-a putut fi citit din pagină: istoricul lui
  vine, dar serverul nu cunoaște indicatorul — se spune pe consolă să se descarce nodul.
- **Enter** într-o casetă de text a formularului de recepție sau a ferestrei «Motiv» este oprit (`onMarcajKey`): se salvează doar cu butonul, care pune marcajul. În casetele cu mai multe rânduri Enter face rând nou, ca înainte.
- Salvarea DDF leagă automat numai o revizie **nouă**; o revizie existentă editată nu consumă
  IDREV-ul ținut.
