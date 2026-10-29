# SLICE-0034 — descărcarea FOREXE: bandă de subsol + consolă + descărcări din arbore

Felia adaugă controlul vizibil al robotului FOREXE. Înainte exista doar `btnSinc`, fără progres în UI, cu jurnal numai în fișier; descărcarea unui angajament afișa MsgBox «într-o felie viitoare».

Regula de fond a feliei: **tot ce se descarcă rămâne LOCAL** (memorie + JSON). Singura scriere pe server rămâne vechiul upsert al listei, mutat în meniul butonului de opțiuni.

## 1. Ce s-a construit

### `ForexeController` (`src/KBot.App/Forexe/ForexeController.vb`) — coordonatorul

Singleton DI și singur apelant `IForexeRunner`: ține sesiunea, progresul, starea, anularea, certificatul și rezultatele. Cele două suprafețe UI se abonează la `StateChanged` / `ProgressChanged` / `StatusChanged` și cheamă `ConnectAsync` / `DownloadListaAsync` / `DownloadNodeAsync` / `Cancel` / `ShowBrowserAsync`.

`MainForm` a pierdut `EnsureForexeSessionAsync`, `SelectCertificate`, `HasLiveForexeSession` și `UpdateForexeStatus` — toate au intrat aici.

### `WorkflowResultStore` (`…/Forexe/WorkflowResultStore.vb`) — depozitul local

Memorie (ultima listă mapată + ultimul rezultat per cod) și JSON cu marcaj de timp în `<AppDir>\WorkflowResults\`. Diacritice LITERALE: `JavaScriptEncoder.Create(...LatinExtendedA...)`, același tipar ca `ThemeOverrideStore`.

### Cele două suprafețe

`ForexeFooterView` (UserControl, `IThemedControl`) în `pnlStatus`: Conectare, pastila de conexiune, progres, certificat, ultima linie de stare, buton de extindere. `ForexeConsoleForm` (`KBotShellForm`, redimensionabilă): jurnalul complet, progres, stare, Anulează, Arată browserul, Deschide jurnalul.

## 2. Ce s-a CITIT din fișierele reale (nu s-a presupus)

### Cele două `.wfl` de prelucrare

Lipseau la începutul feliei; le-a pus operatorul după raportul de la pasul 0.

| | `adlop - Prelucrare Completa.wfl` | `… Reverse.wfl` |
|---|---|---|
| Versiune | V.5 — 13/08/2026 | V.4 — 26/03/2026 |
| Variabile | `{{COD_ANGAJAMENT}}` | `{{COD_ANGAJAMENT}}` + `{{DATA_IESIRE}}` |
| Timeout-uri | 15s | 5s |

**Planul presupunea UN tabel `saveTo`. Sunt CINCI, plus scalari** — cea mai mare corecție a feliei:

- tabele: `TabelIndicatori`, `BugetIndicator`, `ListaReceptii`, `Detaliu`, `TabelIstoric`
- scalari (`<Read saveTo>`): `CodAngajament`, `DataAngajament`, `DataInceputDerulare`, `UltimaModificare`, `DescriereAngajament`, `StareAngajament`, plus per-recepție `TipReceptie`, `DescriereReceptie`, `CodIndicator`

Sinkul salvează întreg `JobResult`: `Tables` structurat și numai cheile din `Data` care nu au devenit tabele. Astfel tabelul nu apare și structurat, și ca JSON brut.

Cele două fișiere diferă **doar** la `ScrapeTable`-ul final de istoric (Reverse: `startFromLast="true"`, `lastPageSelector="a.last"`, `prevPageSelector="a.prev"`, `exitIfCellEquals="Timp:~:^{{DATA_IESIRE}}"`). Secțiunile 0–2 se re-citesc INTEGRAL în ambele — reverse-ul **nu e incremental** decât pentru istoric.

⚠️ `{{DATA_IESIRE}}` **nu e documentat** în antetul propriu al fișierului Reverse (acolo scrie doar `COD_ANGAJAMENT`). ⚠️ `collectFields` pentru recepții **diferă între cele două fișiere**: varianta completă omite `TipReceptie` și `CodIndicator` (comentate), Reverse le include. Nu s-a atins.

### Înainte vs. înapoi — confirmat din Access, fără presupuneri

`MODULES/mdl_FX_Tasks_Send.md`, `FX_Angajament_InfoComplete`:

```vba
lastDate = DMax("DataFX", "FX_Istoric", "CodAngajament='" & CodAngajament & "'")
If Not IsNull(lastDate) Then … Reverse … AddVariable "DATA_IESIRE", Format(lastDate, "DD\/MM\/YYYY HH\:MM\:SS")
Else … varianta completă
```

Deci formatul `dd/MM/yyyy HH:mm:ss` este **citit**, nu ghicit (planul îl dădea ca fallback), și se scrie cu `InvariantCulture`: valoarea intră într-o expresie regulată comparată cu textul din pagină, deci un separator schimbat de Windows ar rupe oprirea.

Echivalentul K-BOT al lui `DMax`: `IApiClient.GetIstoricAsync(cod, ct)` → `IstoricInfo.Randuri` → `MAX(DataFx)`, chemat prin `WithReauth` ca orice altă citire a shell-ului.

⚠️ **Caveat păstrat din plan:** fiindcă descărcările noi rămân locale, istoricul local reflectă doar ce era deja în bază — calea REVERSE se aprinde numai pentru coduri care au deja istoric în DB. E comportament corect, dar rar exercitat până când vor exista upsert-uri.

## 3. `KBot.Controls` — `FooterLeftIconClicked` (adăugare, nu schimbare)

`FooterLeftIcon` se desena, dar nu avea dreptunghi, hover sau eveniment, contrar planului. Completarea oglindește iconița din dreapta:

- `FooterLeftIconClicked` în `.Events.vb`
- `ComputeFooterLeftIconRect` + `FooterLeftIconRect` în `.Footer.vb` (aceeași funcție pură pentru desen și hit-test — o a doua formulă ar fi o iconiță care se desenează unde nu se apasă)
- `_footerLeftIconHover` + `FooterLeftIconHoverColor` (cu `ShouldSerialize`/`Reset`) în `.ButtonHover.vb`

Pur aditiv: niciun arbore existent nu-și schimbă comportamentul (fără iconiță setată, totul e gol). Desenul iconiței trece acum prin dreptunghiul calculat, deci `midY` a rămas fără folosință și a fost șters (altfel: avertisment de variabilă nefolosită).

Când iconița se vede, butonul de strângere **nu** e pe stânga (`ShowFooterLeftIcon`), deci începutul benzii îi aparține — de aici `PaddingTreeStart` în geometrie.

## 4. `MainForm`

- `pnlStatus`: `lblProgram`, `lblForexe` și `btnSinc` ȘTERSE; `forexeFooter` andocat `Fill`, `lblOperator` păstrat (`Left`). **CodProgram nu mai are etichetă** — rămâne pe sesiune, de unde îl citește `JobBuilder`.
- **Strângerea dispare numai din shell:** șterse `tree_CollapsedChanged`, `ClampSplitter`, două câmpuri splitter și valorile designer `FooterCollapseButtonPosition`, `FooterCollapse*Image`, `MinimumCollapsedWidth`. `FooterCollapseButton` nu fusese activat, deci colțul stâng era deja liber. Arborii vederilor păstrează strângerea.
- `tree.FooterLeftIcon` = `database` (nu există glifă de descărcare în resurse; iconița de refresh era deja luată de subsolul-dreapta) → `DownloadListaAsync`.
- `tree.RightIconClicked` (exista deja, ducea la `MsgBox`-ul `RefreshAngajament`) → `DownloadNodeAsync`.
- Meniul butonului de opțiuni are rândul nou «&Sincronizare (server)» = descărcare prin coordonator
  + `UpsertAngajamenteAsync` prin `WithReauth`, cu ambele gărzi păstrate.
- Constructorul a primit al șaselea parametru (`ForexeController`); `Program.vb` îl înregistrează singleton.

## 5. Jurnalul — hotărârea de la §10 al planului

`RichTextBoxLogger` cere `RichTextBox`-ul la construcție și **nu expune nici buffer public, nici eveniment pe linie** (bufferul e privat). Deci varianta «oglindește un RichTextBox ascuns» nu era disponibilă fără a modifica logger-ul.

Consola se creează o singură dată în `MainForm_Load` (`EnsureConsole`), înainte de logger, apoi stă ascunsă. `OnFormClosing` ascunde fără distrugere, păstrând valid controlul logger-ului. Fișierul `<AppDir>\Logs` se scrie indiferent de afișarea consolei.

`ForexeRunner.OnExecutorStatus` transmite starea, anterior doar logată, și prin noul `IForexeRunner.StatusUpdated`. Abonatul care aruncă este izolat de robot. `HasLiveSession` și `ShowBrowserAsync` intră și ele în interfață, eliminând `DirectCast` din gazde.

## 6. Fișiere atinse

**Noi:** `src/KBot.App/Forexe/` — `ForexeController.vb`, `WorkflowResultStore.vb`, `ForexeFooterView.vb` + `.Designer.vb`, `ForexeConsoleForm.vb` + `.Designer.vb`. **Modificate:** `MainForm.vb`, `MainForm.Designer.vb`, `Program.vb`; `AdvancedTreeControl.{Events, Footer,ButtonHover,Properties}.vb`; `IForexeRunner.vb`, `ForexeRunner.vb`, `JobBuilder.vb`, `WorkflowCatalog.vb`; `tests/KBot.App.Tests/MainFormNavItemsTests.vb` (doar al șaselea `Nothing` în constructor); `FileVersion` bumpat în `KBot.App` (1.0.17.0), `KBot.Controls` (1.24.0.0), `KBot.Forexe` (1.0.2.0).

Cele două `.wfl` noi **nu cer nicio schimbare de proiect**: `publish-debug.ps1` §5 copiază toate `*.wfl` prin glob. (Ca și până acum, un simplu `dotnet build` NU le pune în `bin` — doar publish-ul.)

## 7. Probe

Build `KBot.sln` Debug **0 erori / 0 avertismente**; Release **0 erori / 5 avertismente `MSB3825` preexistente** (`DdfView.resx`, `OrdView.resx` — fișiere neatinse de felie). `Controls.Tests` **831 trecute, 0 picate**. `App.Tests` **155 trecute / 6 picate** — mulțime **IDENTICĂ** cu baza: verificat prin `git stash` pe arborele curat, aceleași 6 eșecuri, aceleași nume. **Fără teste noi** (politica standing).

## 8. Trecerea operatorului peste bandă (după prima livrare)

Trei cereri, plus consecințele lor.

### `KBotProgressBar` — control nou (`src/KBot.Controls/Progress/KBotProgressBar.vb`)

`System.Windows.Forms.ProgressBar` e desenat de Windows, deci **tema nu-l poate atinge**: rămânea verde pe schemă întunecată. Controlul nou e fratele DETERMINAT al lui `KBotBusyBar` (acela e indeterminatul) și îi copiază deciziile: același `SetStyle`, același `KBotDesignTime.IsDesignTime` în `OnPaint` (fără log din procesul designerului), aceeași formă de `ApplyTheme`.

Regulile casei respectate cap-coadă: `BarColor` / `TrackColor` / `PercentTextColor` au `Color.Empty` = «din temă» cu perechea `ShouldSerialize`/`Reset` fiecare, iar `Font` — care **nu poate purta `<DefaultValue>`** (atributul cere o constantă) — are perechea lui plus steagul «operatorul l-a scris», ca designerul să nu-l pinuiască în fiecare gazdă.

`Value` **prinde** valorile din afara intervalului în loc să arunce: sursa e un `IProgress(Of Integer)` venit de pe firul robotului, iar o excepție acolo ar opri descărcarea din cauza unei bare de progres. Umplerea se decupează la forma șinei, ca marginea rotunjită să rămână rotunjită și la 100%.

⚠️ **Capcana IThemedControl, a treia oară.** `ForexeFooterView` este ea însăși `IThemedControl`, deci `ThemeManager` **nu recurge în copiii ei** — bara ar fi rămas pe culorile implicite la fiecare comutare. `ApplyTheme` al benzii o împinge explicit mai departe. (În consolă nu e nevoie: acolo părintele e un `Panel` obișnuit, deci traversarea ajunge singură la ea.)

### Vizibilitate condiționată în bandă

`pbProgress` se vede **doar cât rulează ceva**; `lblCert` **doar după ce s-a ales un certificat** (fără el n-are ce spune, iar un «Certificat: —» permanent e zgomot). Amândouă sunt andocate la stânga, deci ascunse nu lasă gol în urmă — banda se strânge singură.

Steagurile pornesc din **constructor, nu din designer**: pe suprafața de proiectare cele două rămân vizibile (altfel operatorul n-ar mai avea ce apuca), iar la rulare pornesc ascunse.

### Consola a primit același control

Consola folosește același control tematizat pentru același progres; vizibilitatea rămâne permanentă, fiind fereastră dedicată.

**Observație:** `ForexeFooterView.Designer.vb` și `ForexeConsoleForm.Designer.vb` fuseseră între timp deschise în designerul VS și **re-serializate** (dimensiuni, margini, `AutoScaleDimensions`) — adică dus-întorsul din §DoD 14 s-a făcut și **au supraviețuit**. Schimbările de aici sunt peste forma re-serializată de designer.

## ⚠️ Deschis / neverificat

1. **Nimic nu a atins FOREXE-ul viu.** Niciun workflow rulat, nicio sesiune deschisă, niciun certificat ales. Toate cele patru intenții ale coordonatorului sunt verzi doar la compilare.
2. **Nicio suprafață randată pe ecran.** Banda de subsol și consola nu au fost privite în nicio schemă; dus-întorsul prin designerul VS (`ForexeFooterView`, `ForexeConsoleForm`, `MainForm` cu `pnlStatus` schimbat și subsolul arborelui schimbat) e al operatorului — e riscul standing de la 0025/0027.
3. **JSON-ul nu s-a scris niciodată pe un disc real** — nici folderul `WorkflowResults`, nici diacriticele în fișier.
4. **Nu există mapper de ingestie** pentru «Prelucrare Completa»: rezultatul se păstrează BRUT, deliberat. Operatorul se uită întâi la coloanele reale; maparea e o felie ulterioară.
5. **Calea REVERSE e rar exercitată** cât timp descărcările rămân locale (vezi §2).
6. `FooterLeftIconClicked` n-a fost apăsat niciodată pe ecran — geometria și survolarea se judecă privind, ca la orice iconiță de bandă. 6b. **`KBotProgressBar` n-a fost privit niciodată pe ecran** — colțurile rotunjite, decuparea umplerii la 100%, contrastul accentului pe cele patru scheme și felul în care arată la înălțimea autorată în bandă (52px, mult peste cei 18 impliciți) se judecă doar privind. Nici dus-întorsul lui prin designerul VS nu s-a făcut, și **are** perechi `ShouldSerialize`/`Reset` de probat exact ca la 0025/0027. **Fără teste** (politica standing) — deci `Value`-ul care prinde, `Fraction` și `ShouldSerialize*` sunt neacoperite.
7. Iconița subsolului e `database`, aleasă din ce exista; nu e o glifă de descărcare — de schimbat din designer când există una.
8. `collectFields` diferit între cele două `.wfl` (§2) — nereparat, nu e al feliei.
