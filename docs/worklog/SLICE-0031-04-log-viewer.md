# SLICE-0031-04 — `LogViewerForm`, dialogul de golire, cablarea în banc ȘI în shell

A patra trecere din felia 0031, după `docs/PLAN_LogViewer.md` §8–§9, plus două lucruri cerute direct
de operator care nu erau în plan: meniul butonului de opțiuni din bara de titlu a shell-ului și
fereastra de pornire care înlocuiește dialogul Da/Nu.

Cererea, în cuvintele operatorului: «integrate the new logging system into main form. i want to see
the log form on press on optionbutton in the capbar. it should draw a custom popup with (for now) one
option - "Arată jurnal". for now anyone can use it, but i will want to be enabled/disabled by an
internal switch.» și, după trecerea vizuală, «in the launcher i want you to remove the messagebox and
replace it with a form, using the navlist to select what i want to run».

---

## Ce s-a schimbat și de ce

### 1. `LogViewerForm` — fereastra propriu-zisă

`src/KBot.App/Views/`, `Inherits KBotShellForm`, toate controalele declarate în `.Designer.vb`.
Fereastra **nu analizează nimic**: tot ce ține de citit și de înțeles un jurnal stă în nucleul pur din
`KBot.Common\Logging` (trecerea 01) — `LogPaths`, `LogFileLoader`, `LogFilter`, `ServerClock`. Aici se
leagă doar nucleul acela de controalele casei.

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

Bara de stare spune tot ce ar putea altfel dispărea tăcut: câte intrări au intrat, câte se văd, cât
s-a citit, dacă fereastra de citire a tăiat coada, decalajul ceasului de server și — obligatoriu —
**câte intrări au fost excluse fiindcă n-au dată**, când e pus un capăt de interval.

### 2. Serverul: cablat, opțional, și ONEST că încă nu există

Grupul «Server» apare doar dacă fereastra a primit un `IApiClient`; fără el (bancul de probă) nu apare
deloc — un grup care nu poate funcționa nu trebuie să existe. Lista se cere **la cerere**, dintr-un
rând anume, nu la deschidere.

**Rutele nu există încă**: sunt livrate de trecerea 0031-02, împreună cu `GetLogFilesAsync` /
`GetLogTailAsync` de pe `ApiClient`. Astăzi `ApiClient.GetAsync(Of T)` e literalmente
`Throw New NotImplementedException()`. Fereastra prinde exact excepția aia și scrie în `KBotNotice`:
«Jurnalele de server nu sunt încă disponibile: ruta se livrează în trecerea 0031-02. Fișierele locale
funcționează normal.» Nu o listă goală, nu un mesaj tehnic englezesc aruncat operatorului.

DTO-urile (`LogFilesResponse` / `LogTailResponse`) sunt **private în formular**, scrise după contractul
din planul §6.4. Când vine 0031-02, ele se mută pe `ApiClient` ca metode tipizate și dispar de aici.
Probele 19–20 din plan aparțin acelei treceri și **nu au fost făcute**.

### 3. `LogClearDialog` — singurul drum distructiv

Doi pași, fiindcă nu există «înapoi»: întâi lista fiecărui fișier local cu mărimea și numărul de
intrări, **nimic bifat** la deschidere; apoi o confirmare care NUMEȘTE fișierele și totalul.

- Fișierul ținut deschis de rularea curentă se **arată**, dar șters și nebifabil. Se întreabă
  EMPIRIC (o deschidere exclusivă de o clipă), nu după o listă de nume ținută pe de rost, care ar
  rămâne în urmă la primul scriitor nou.
- `File.Delete`; pe `IOException` fișierul se **golește** în loc să fie șters. Dacă nici asta nu merge,
  apare pe nume, cu motivul, iar restul continuă. Nimic nu eșuează tăcut.
- **Jurnalele de server nu se ating de aici, niciodată.**

### 4. Cablarea în bancul de probă (plan §9), fără ca bancul să refere `KBot.App`

`LogViewerForm` trăiește în `KBot.App` — acolo îi e locul, ca să rămână și în Release. Dar
`KBot.DevHarness` **nu referă** `KBot.App` (referința merge invers), deci proba vizuală nu putea pur
și simplu să construiască fereastra.

Soluția e o punte tipizată: `ILogViewerLauncher` în `KBot.DevHarness\Abstractions\`, implementată de
`LogViewerLauncher` în `KBot.App` (compilat `#If DEBUG`, ca și referința) și înregistrată în DI. Plus
un buton «Jurnale» pe `DevHarnessForm`, pe același tipar cu `OpenMainFormAction`. Un test care nu
găsește puntea întoarce **Skipped**, nu Failed: înseamnă doar că rulează într-o gazdă fără shell.

`LogViewerTest` (categoria `Controls/UI`) copiază `ThemeGalleryTest` exact: OK → Passed, Cancel →
Failed, închis → Skipped.

### 5. Meniul din bara de titlu + comutatorul intern (CERUT de operator)

`capBar.OptionButtonClick` desfășoară un `CustomPopup` cu un singur rând, «&Arată jurnal». Aceleași
decizii ca meniul de teme al aceleiași bare: garda `CustomPopup.ClosedJustNow` (fără ea, al doilea
clic ar redeschide meniul instantaneu), ancorare pe `OptionButtonBounds`, fereastra arătată **nemodal**
și **nu** într-un `Using`.

Comutatorul intern e `FeatureSwitches.VizualizatorJurnaleActiv` (`KBot.Common`, modul nou) — **azi
mereu `True`**, cum s-a cerut. Când devine `False`, meniul **nu se deschide deloc**: fiind singurul
rând, un meniu cu el stins ar fi o fereastră goală agățată de buton. Locul din care se va citi cu
adevărat (rol, manifest, configurare) se schimbă **în interiorul modulului**, fără să atingă apelanții
— ăsta e tot rostul lui.

Fereastra e nemodală și una singură: dacă e deja deschisă, se aduce în față (ca `InternalInfoForm`).

### 6. Fereastra de pornire înlocuiește `MessageBox`-ul (CERUT de operator)

`StartupLauncherForm`: `KBotCaptionBar` + `KBotNavList` cu trei porniri — **Aplicația**
(autentificare → shell), **Banc de probă**, **Jurnale** (vizualizatorul singur, fără autentificare și
fără shell). Un `MessageBox` cu două butoane putea purta exact două opțiuni, iar textul lor era «Da»
și «Nu»; aici pornirile se citesc dintr-o privire, se adaugă altele fără să se schimbe nimic altceva,
și fereastra e tematizată (dialogul de sistem nu era).

Prima pornire e selectată din start: cine apasă Enter fără să citească primește **aplicația**, nu
bancul. Renunțarea lasă `Alegere` gol, ca un apelant care se uită doar la proprietate să nu pornească
ultima pornire survolată. Dispecerul din `Program.RunLauncher` **aruncă** pe o cheie necunoscută — o
pornire adăugată în listă și uitată în dispecer trebuie să se vadă imediat.

### 7. Trecerea vizuală a operatorului, și ce a rupt pe drum

Operatorul a deschis fereastra în designerul VS, a confirmat că **funcționează**, și a rearanjat
așezarea: panourile andocate au fost înlocuite cu `TableLayoutPanel`-uri (`tlyMain`, `tlyFilter`,
`tlyFilterActual`, `tlyFooter`) — «i want tly instead of those annoying panels where i have no real
control» — apoi a cerut punerea la punct a plasării. Trei defecte reale găsite și reparate:

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

Au fost șterse și trei declarații moarte (`pnlCard`, `pnlActiuni`, `pnlStare`) — controale care nu mai
există nicăieri în formular.

---

## Fișiere atinse

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

## Rezultatele probelor

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

Probele 21–24 din plan, acoperite: fereastra montează bara de jetoane, lista, grila (cele șase coloane
în ordine, `Ora` înghețată, grilă read-only), panoul de detaliu și notificările; `RowFormatting` mapează
fiecare nivel pe culoarea din `ThemeManager.Current.Palette` (**niciodată pe un literal**), iar `Info`
rămâne nevopsit; selecția pune `Raw` **neconvertit** în panou; un apel de server picat scoate
notificarea și **lasă intrările locale întregi**.

---

## Ce a rămas NEVERIFICAT sau amânat

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
