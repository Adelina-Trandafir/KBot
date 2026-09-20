# SLICE 0067-01 — Instalatorul Inno: detectează versiunea instalată, permite doar actualizări, actualizează cu regulile sistemului de actualizare

**Data:** 20.09.2026
**Cererea operatorului:** «the inno installer must detect if a version of kbot is already installed
and if so, only allow upgrades. if the version being installed is older than what's on the pc, it
shouldn't allow the install. also, when upgrading, use logic from the update system
(`src\KBot.App\Update\`)»

## Presupuneri declarate

- **Numărul feliei: 0067-01** — instalatorul Inno n-are rând propriu în STATUS (a intrat cu
  commit-ul `a15cc7c`, fără felie); cererea e o a doua trecere peste sistemul de actualizare (0067),
  al cărui contract îl copiază. «Next free» rămâne 0073.
- **Aceeași versiune ≠ actualizare.** Cererea spune «only allow upgrades»; interactiv, operatorul
  e totuși ÎNTREBAT dacă rescrie aceeași versiune (o reparație e un lucru legitim, iar un instalator
  refăcut fără bump ar fi altfel de neinstalat); în mod silențios (`/SILENT`, `/VERYSILENT`)
  regula strictă se aplică și aceeași versiune e refuzată.
- **Versiunea care contează e `FileVersion` din `KBot.App.exe` instalat**, nu `DisplayVersion`
  din registru: `KBot.Updater` rescrie fișierele fără să atingă «Programe și caracteristici», deci
  registrul rămâne în urmă după prima actualizare automată. Registrul e doar rezervă, pentru un
  folder înregistrat al cărui exe a dispărut.
- **Versiune instalată care nu se poate citi** (exe fără resursă de versiune ȘI registru gol) =
  «nu e instalat» → instalare proaspătă peste folder. Un build real K-BOT poartă întotdeauna
  `FileVersion`, deci cazul e teoretic; se scrie în jurnalul Setup-ului.

## Ce s-a schimbat și de ce

Un singur fișier: `tools\KBotInstaller\KBot.iss`.

### 1. Detectarea instalării existente (`DetectInstalled`)

Ordinea: (a) exe-ul din folderul înregistrat (`InstallLocation` din cheia de dezinstalare
`HKLM\…\Uninstall\{AppId}_is1`, vederea pe 64 de biți, apoi pe 32); (b) exe-ul din `C:\KBOT`
(instalările de dinaintea instalatorului Inno — SFX-ul — n-au cheie în registru; **exact cazul
acestui PC**: nicio cheie, `C:\KBOT\KBot.App.exe` = 1.0.19.0); (c) `DisplayVersion` din registru.
Sursa («exe» / «registry») se scrie în jurnalul Setup-ului.

### 2. Compararea — oglinda lui `UpdatePolicy`

`TryParseVersion` taie «a.b.c.d» în cel mult patru părți numerice și **umple lipsurile cu 0**
(`UpdatePolicy.Normalize`: altfel «1.0.31» < «1.0.31.0»), apoi `PackVersionComponents` +
`ComparePackedVersion` (Inno 6.1+; scriptul cere 6.5+). Pachetul mai vechi → **refuzat**
(`InitializeSetup` întoarce False, caseta spune versiunile și folderul; drumul spre versiunea veche =
dezinstalare din «Programe și caracteristici», spus pe nume). Egal → întrebare (implicit «Nu»).
Mai nou → actualizare. `AppVersion` care nu se parsează = `RaiseException` (defect de build, nu
de operator).

### 3. Actualizarea cu regulile lui `UpdateApplier`

| Regula updater-ului | În instalator |
|---|---|
| `Logs\` nu se atinge niciodată (`PreservedFolders`) | `Excludes: "\Logs\*"` pe `[Files]`; folderul vine gol din `[Dirs]` (`_keep.txt` din staging nu se mai copiază) |
| Se scriu doar fișierele din pachet; nimic din folder nu se șterge (`Asociere\`, `WorkflowResults\`, `Extrase\`, `kbot_paths.json` supraviețuiesc) | comportamentul implicit Inno (fără `[InstallDelete]`), acum scris în comentariu ca regulă; `[UninstallDelete]` pentru `Logs\` rămâne — e dezinstalare, nu actualizare |
| Ținta e folderul aplicației, fără întrebare | `DisableDirPage=auto` (pagina de folder doar la prima instalare) + `DirEdit` fixat pe folderul găsit în `InitializeWizard` (acoperă și cazul fără registru) |
| Așteaptă închiderea aplicației | `CloseApplications=yes` (era deja) |
| Textul ofertei spune ce urmează | `WelcomeLabel2` rescris la actualizare: versiunea instalată → cea nouă, «se înlocuiesc doar fișierele din pachet, jurnalele (Logs\) și datele locale rămân neatinse, nimic nu se șterge» |

`AppId` a devenit `#define MyAppId` ca `[Code]` să citească aceeași cheie de dezinstalare pe care o
scrie `[Setup]`; `DefaultDirName` la fel (`MyDefaultDir`).

**Capcană ISPP găsită la compilare:** o linie de cod Pascal care ÎNCEPE cu `#13#10` e citită de
preprocesor ca directivă («Unknown preprocessor directive»); `#13#10` stă mereu în continuarea unei
linii.

## Fișiere atinse

- `tools/KBotInstaller/KBot.iss` — antet, `MyAppId`/`MyDefaultDir`, `DisableDirPage=auto`,
  `[Dirs]`, `Excludes`, secțiunea `[Code]` (`TryParseVersion`, `ExeVersionIn`, `ReadRegString`,
  `DetectInstalled`, `CheckInstalledVersion`, `InitializeSetup` rescris, `InitializeWizard` nou).
- `docs/worklog/SLICE-0067-01-instalator-doar-actualizari.md` (acesta), `docs/worklog/KBOT_STATUS.md`.

## Rezultatele testelor

- **Compilare ISCC (Inno Setup 6.7.1) reușită** pe un folder de staging fictiv (3 fișiere +
  `Logs\_keep.txt`), `/DAppVersion=1.0.31.0`: zero erori; lista de compresie confirmă că
  `Logs\_keep.txt` NU intră în pachet. Exe-ul rezultat s-a șters cu folderul temporar.
- `publish-release.ps1` NErulat (nu s-a construit niciun pachet real).
- Niciun Setup **nu s-a rulat** pe acest PC (ar fi scris în `C:\KBOT`).

## Neverificat / amânat

- **Nimic văzut pe ecran**: nici refuzul de downgrade, nici întrebarea de reinstalare, nici textul
  de bun venit al actualizării, nici sărirea paginii de folder. Primul pachet real de verificat:
  (1) instalare peste `C:\KBOT` 1.0.19.0 fără registru → trebuie să spună «1.0.19.0 → 1.0.31.0»,
  CU pagina de folder (fără cheie în registru `auto` o arată, preumplută cu `C:\KBOT`); (2) apoi
  un Setup mai vechi peste → refuz, iar de data asta fără pagina de folder (cheia există);
  (3) același → întrebare.
- **Registrul rămâne în urmă după actualizările automate** (`DisplayVersion` = ultima instalare
  prin Setup): instalatorul nu depinde de el, dar «Programe și caracteristici» minte. Dacă se
  vrea corect, `KBot.Updater` ar trebui să scrie `DisplayVersion` — cere HKLM, deci drepturi de
  administrator pe care updater-ul le cere doar când folderul nu e scriibil; de hotărât.
- **`SuppressibleMsgBox` în mod silențios** întoarce implicitul: downgrade = refuz (bine), aceeași
  versiune = refuz (regula strictă). Nevăzut.
- Componenta `migrare` deselectată la actualizare NU șterge `Migrare\` vechi (regula «nimic nu se
  șterge»); nu se semnalează operatorului.
