# K-BOT — STATUS, slices 0060–0069

Moved verbatim out of `../KBOT_STATUS.md` (28.09.2026). The index there says what
each slice is; this file holds everything recorded about it: its registry row, its
«Current focus» notes and its «Open threads» notes.

---

## Slice 0060

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0060 | Reîmprospătare, selecție păstrată și fluxuri parțiale | GATA pe cod (`dotnet build KBot.sln` **0 erori**, cele 7 `MSB3825` PREEXISTENTE pe `.resx`; XML-ul tuturor celor 8 `.wfl` bine format) / **NICIO SUITĂ N-A FOST RULATĂ și nimic nu s-a comis** (cerere explicită: «nu testa. nu git»); **NIMIC NU S-A VĂZUT PE ECRAN** — `SelectieReceptiiForm` n-a fost desenată nici măcar cu `DrawToBitmap`; **NICIUN WORKFLOW NOU N-A RULAT** și nicio cerere n-a ajuns pe o bază vie | [0060](../SLICE-0060-reimprospatare-ceruta.md#slice-0060) | Cereri, etape, contracte, probe și limite în jurnal. |

### Current focus

- [Detalii și limitele reîmprospătării](../SLICE-0060-reimprospatare-ceruta.md#slice-0060); fără probe noi pe ecran, workflow sau bază.

---

## Slice 0061

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0061 | Selecție multiplă și grafice separate | GATA pe cod (`dotnet build KBot.sln` **0 erori, 0 avertismente**) / **NICIO SUITĂ N-A FOST RULATĂ și nimic nu s-a comis** (cerere explicită: «no tests. no git») / **văzut pe ecran** prin `DrawToBitmap` în șase stări, dintr-o gazdă de unică folosință; **niciun gest n-a fost făcut cu mâna** și **nimic nu s-a salvat pe o bază vie** | [0061](../SLICE-0061-asociere-selectie-multipla-si-grafice-separate.md#slice-0061) | Contractele, probele, reviziile și limitele în etapa legată. |
| 0061-02 | Arborele graficelor și graficul rezervărilor | GATA pe cod (`dotnet build src\KBot.App\KBot.App.vbproj` **0 erori, 0 avertismente**) / **NICIO SUITĂ N-A FOST RULATĂ și nimic nu s-a comis** (cerere explicită: «fără teste, fără git») / **văzut pe ecran** prin `DrawToBitmap` în patru stări, dintr-o gazdă de unică folosință; **niciun gest n-a fost făcut cu mâna** și **nimic nu s-a citit de pe o bază vie** | [0061-02](../SLICE-0061-asociere-selectie-multipla-si-grafice-separate.md#slice-0061-02) | Contractele, probele, reviziile și limitele în etapa legată. |

### Current focus

- [Detalii și restanțe 0061](../SLICE-0061-asociere-selectie-multipla-si-grafice-separate.md#slice-0061); starea probei rămâne în registru.

---

## Slice 0062

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0062 | Instantanee neașezate și refacere din istoric | Implementare consemnată; Python offline 535 trecute / 26 sărite, 0 eșuate; build soluție 0 erori, MSB3825 preexistent; fără bază vie sau ecran | [0062](../SLICE-0062-receptii-neasezate-si-refacere-din-istoric.md#slice-0062) | Navigare adăugată din jurnalul 15.09; rezultatul App.Tests nu era consemnat. |

---

## Slice 0063

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0063 | Ultimul utilizator și unitatea de login, fără parolă/token | Implementare consemnată; Common.Tests 94 trecute / 0 eșuate (9 noi); build soluție 0 erori; formular nedeschis pe ecran | [0063](../SLICE-0063-login-ultimul-utilizator.md#slice-0063) | Navigare adăugată din jurnalul 15.09; implicitele Debug și lipsa probelor UI rămân în jurnal. |

---

## Slice 0064

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0064 | Linii zero, salvarea și antete fără linii | GATA pe cod (PYTHON `.venv` suita întreagă **546 trecute / 26 sărite / 0 picate**, +11 teste față de 0063; `KBot.App` **0 erori, 0 avertismente**, doar comentariu schimbat) / **nimic n-a rulat pe MariaDB**, salvarea nu s-a apăsat pe ecran după fix; datele vechi se repară DOAR prin «Refacere din istoric» (0062), apăsat de operator | [0064](../SLICE-0062-receptii-neasezate-si-refacere-din-istoric.md#slice-0064) | Contractele, probele, reviziile și limitele în etapa legată. |

---

## Slice 0065

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0065 | Ultimul antet, totaluri și perechi unice | GATA pe cod (`KBot.App` **0 erori, 0 avertismente**; `KBot.App.Tests` compilează; PYTHON cele trei fișiere atinse **69 trecute / 1 sărit**, suita întreagă NErulată) / **fără teste, fără git** la cererea operatorului: nimic pe ecran, nimic pe MariaDB, testele VB vechi ale vederii și ale editorului NEactualizate (vor pica la rulare, vezi worklog §6), nimic comis | [0065](../SLICE-0062-receptii-neasezate-si-refacere-din-istoric.md#slice-0065) | Contractele, probele, reviziile și limitele în etapa legată. |

### Current focus

- [Detalii și restanțe 0065](../SLICE-0062-receptii-neasezate-si-refacere-din-istoric.md#slice-0065); starea probei rămâne în registru.

---

## Slice 0066

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0066 | Tabele cu măsuri DPI, API logic și banc | GATA pe cod (`dotnet build KBot.sln` **0 erori, 0 avertismente** în afara celor 7 `MSB3825` preexistente) / **NICIO SUITĂ N-A FOST RULATĂ și nimic nu s-a comis** (cerere explicită: «no git. not tests») / **bancul văzut pe ecran** prin `DrawToBitmap` în șase stări (150%, text 110%); **cele ~80 de tabele migrate NU s-au văzut** | [0066](../SLICE-0066-tabele-la-dpi-ca-arborele.md#slice-0066) | Reguli, măsurători, fișiere și limite în etapa legată. |
| 0066-02 | Autoscalare DPI pentru formulare și zoom uniform | GATA pe cod (`dotnet build KBot.sln` **0 erori, 0 avertismente** în afara celor 7 `MSB3825` preexistente) / **NICIO SUITĂ N-A FOST RULATĂ și nimic nu s-a comis** («no tests no git») / **văzut pe ecran** prin `DrawToBitmap`: bancurile 0062 și 0066 la 150% sub Automatic, Fix 100% (= cum arată la 100%) și Manual 1,25 (= 125%) — aceeași imagine la 2/3, 5/6 și 1 —, plus un formular autorat la 144; **formularele reale NU s-au deschis** | [0066-02](../SLICE-0066-tabele-la-dpi-ca-arborele.md#slice-0066-02) | Reguli, măsurători, fișiere și limite în etapa legată. |

### Current focus

- [Detalii și restanțe 0066](../SLICE-0066-tabele-la-dpi-ca-arborele.md#slice-0066); probele istorice rămân limitate la banc/Windows 11, după caz.

---

## Slice 0067

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0067 | Actualizarea automată — arhitectură inițială | GATA pe cod (`src\KBot.Updater` / `src\KBot.Api` / `src\KBot.App` **0 erori, 0 avertismente**; Python doar sintaxă) / **NICIUN TEST RULAT, NICIUN PROIECT DE TESTE CONSTRUIT** («NO FUCKING TESTS») / **nimic n-a rulat cap la cap**: niciun push, nicio descărcare, niciun updater pornit, nimic pe ecran; `publish-release.ps1` NErulat cu pasul nou; **`KbotForm` și `LoginForm` NEatinse** (cerere: după acceptare) | [0067](../SLICE-0067-actualizare-automata.md#slice-0067-baza) | Detaliile, probele și limitele sunt în etapa legată. |
| 0067-01 | Instalator, versiuni și registru | GATA pe cod (`KBot.iss` **compilat cu ISCC 6.7.1, zero erori**, pe un staging fictiv; `src\KBot.Updater` Release **0 erori, 0 avertismente**) / **niciun Setup și niciun updater rulat, nimic pe ecran**; teste scrise, nerulate; `publish-release.ps1` nerulat; comis | [0067-01](../SLICE-0067-actualizare-automata.md#slice-0067-01) | Detaliile, probele și limitele sunt în etapa legată. |
| 0067-02 | Note de versiune — procedură și istoric | GATA pe scripturi (cele trei scripturi se parsează fără erori; `ReleaseNotes.ps1` rulat o dată de mână: cererea pentru 1.1.0.7 = 36 de jurnale de lucru de la 1.1.0.6) / **`publish-release.ps1` și `push-update.ps1` NErulate cu pașii noi**; niciun fișier din `src\` atins; niciun test | [0067-02](../../release-notes/README.md#slice-0067-02) | Procedura, baseline-ul, probele și limitele sunt în README; stilul cu diacritice din jurnalul inițial a fost înlocuit ulterior de regula fără diacritice. |
| 0067-03 | «Caută actualizări» poate reinstala ultimul pachet și când versiunea nu e mai mare | GATA pe cod (`KBot.App` Debug **0 erori, 0 avertismente**) / **nerulat cap la cap, niciun test, nimic pe ecran**; `FileVersion` necrescut | [0067-03](../SLICE-0067-actualizare-automata.md#slice-0067-03) | Doar din butonul din Setări; la pornire neschimbat. |

### Current focus

- **0067-02:** [procedura și probele istorice](../../release-notes/README.md#slice-0067-02); publicarea cu pașii noi și verdictul din fereastra de actualizare nu sunt certificate prin consolidare.

- **0067 / 01:** [istoricul actualizării și instalatorului](../SLICE-0067-actualizare-automata.md) păstrează predarea, probele, amânarea butoanelor și revizia DisplayVersion/UAC. Niciun verdict nou de execuție.

---

## Slice 0068

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0068 | Ancora IDH și istoricul consumat o dată | GATA pe cod (`dotnet build src\KBot.App` **0 erori, 0 avertismente BC**; Python importat și exercitat de mână) / **NICIO SUITĂ RULATĂ SAU CONSTRUITĂ, nimic cap la cap, nimic pe ecran** | [0068](../SLICE-0062-receptii-neasezate-si-refacere-din-istoric.md#slice-0068) | Contractele, probele, reviziile și limitele în etapa legată. |

### Current focus

- [Detalii și restanțe 0068](../SLICE-0062-receptii-neasezate-si-refacere-din-istoric.md#slice-0068); starea probei rămâne în registru.

---

## Slice 0069

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0069 | Umbra ferestrelor și chenarul de rezervă | GATA pe cod (`dotnet build` pe `src\KBot.Controls`, `src\KBot.App`, `src\KBot.DevHarness`, `src\KBot.Forexe`, `src\KBot.Migrator` **0 erori, 0 avertismente** în afara `MSB3825` preexistent) / **verificat pe ecran pe Windows 11** (sondă `CopyFromScreen`: umbră reală + chenar de rezervă forțat) / **NICIUN TEST RULAT SAU CONSTRUIT; Windows 10 neatins** | [0069](../SLICE-0069-umbra-ferestrelor-fara-chenar.md#slice-0069) | Reguli, măsurători, fișiere și limite în etapa legată. |

### Current focus

- [Detalii și restanțe 0069](../SLICE-0069-umbra-ferestrelor-fara-chenar.md#slice-0069); probele istorice rămân limitate la banc/Windows 11, după caz.

---
