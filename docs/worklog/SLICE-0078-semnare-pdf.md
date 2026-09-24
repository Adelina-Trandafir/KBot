# SLICE-0078 — Semnarea DDF / ORD în vizualizatorul Adobe din K-BOT (absoarbe felia 0021)

Data: 23.09.2026 · Plan: `C:\Users\Adelina Trandafir\.claude\plans\slice-0078-combined-with-hidden-pillow.md`
(aprobat de operator). Număr: **0078**, dat de operator («slice 0078 combined with 0021»). Felia
0021 (semnarea DDF, rezervată din 0020-05) e **absorbită** aici și nu mai are rând separat.

Decizii ale operatorului (23.09.2026), cerute înainte de cod:
- `Semnatura` = **numele rolurilor**, fără virgulă la coadă: DDF `A,B,Ordonator`, ORD `AB,CD,Ordonator`
  (aceleași nume ca `mdl_FX_Helpers.NumeSemnatar` din Access).
- **ORD intră** în felie (scrie `FX_ORD.Semnatura`, coloana există pe schema vie).
- **Stocarea pe bucăți comune** pe server intră în felie (pasul 05).
- Încărcarea eșuată **se păstrează local și se reîncearcă**.

---

## 1. Ce s-a schimbat și de ce

Cele cinci cerințe ale operatorului, în ordinea lor:

1. **Detectarea semnării în vizualizatorul din aplicație, pe mai multe câmpuri.** Nu există un
   eveniment Adobe pentru «s-a semnat» (fereastra găzduită e un proces străin, fără COM). Semnalul
   folosit e **salvarea**: orice semnătură obligă Adobe să salveze. După fiecare salvare, fișierul e
   citit (Adobe îl ține deschis — citire cu `FileShare.ReadWrite|Delete`, după ce lungimea s-a
   stabilizat) și iTextSharp întoarce **numele câmpurilor semnate**. Ce contează ca «nou» e
   mulțimea NUMELOR DE CÂMPURI, nu a rolurilor: rolul DDF «A» are câmpurile `SignatureField11..16`,
   deci a doua semnătură pe un câmp A schimbă PDF-ul fără să schimbe rolurile. O salvare fără
   câmpuri semnate noi (o simplă editare de formular) **nu** se încarcă.
2. **Capcana «Salvare ca».** Dialogul e mutat în afara ecranului în clipa în care apare, calea
   documentului afișat e scrisă în caseta de nume, **citită înapoi și comparată**, apoi se apasă
   «Salvare»; întrebarea «înlocuiți?» e acceptată DOAR dacă e deținută de dialogul pe care tocmai
   l-am apăsat. Orice îndoială (casetă negăsită, text citit diferit, dialog care nu se închide în
   20 s) **anulează** dialogul — Adobe nu aplică semnătura — și operatorul primește mesaj. Dialogul
   nu rămâne niciodată la operator. Capcana e pornită MEREU pe suprafața «Document» (DDF și ORD),
   chiar și fără sesiune de semnare: nicio salvare nu poate pleca în alt loc.
3. **Spațiul pe server.** Măsurat pe cele 16 PDF-uri semnate reale din `C:\AVACONT\FOREXE\PDF`:
   compresia unui fișier singur câștigă ~9% (fluxurile sunt deja comprimate), dar ~97% dintr-un DDF
   e identic cu orice alt DDF (fonturi, machetă, scripturi). Serverul taie PDF-ul în bucăți după
   conținut (medie ~4 KB) și ține fiecare bucată distinctă O SINGURĂ DATĂ per bază. Primul DDF
   costă ~250 KB, fiecare următor ~5 KB (8 DDF-uri: 288 KB în loc de 2,18 MB). Refacerea lipește
   bucățile la loc, deci fișierul iese **identic la octet** — semnătura rămâne validă — iar suma
   SHA-256 se verifică după refacere. Contractul de pe fir al feliei 0041 nu s-a schimbat.
4. **Serverul e singura sursă de adevăr.** O revizie/ordonanțare cu PDF semnat pe server nu mai
   arată nimic până când copia locală n-a fost comparată cu suma de pe server; la nepotrivire,
   copia e înlocuită cu originalul și operatorul primește mesajul «nu era identic… a fost înlocuit».
   Fișierul ales din pagina «Fișiere» care aparține unei revizii e deschis CA acea revizie (trece
   prin aceeași verificare). Generarea unui document nesemnat peste unul semnat cere confirmare.
5. **`Semnatura` în `FX_DDF_REV` / `FX_ORD`**: rolurile, în ordinea canonică, scrise **în aceeași
   tranzacție** cu PDF-ul (antetul nou `X-Semnatura` pe `PUT /api/forexe/{ddf|ord}/pdf/<id>`).
   Nu poate exista PDF stocat fără roluri sau roluri fără PDF.

Plasa pentru încărcarea eșuată: copia semnată + `.json` în `<AppDir>\PdfDeIncarcat\`. Cât timp
există, vederea arată ACEA copie și reîncearcă încărcarea la deschidere; la pornire (după login)
se reîncearcă toate. Fără ea, regula de la punctul 4 ar suprascrie la următoarea deschidere
semnătura cu versiunea mai veche de pe server. Un 409 (altcineva a semnat între timp) marchează
copia «în conflict»: nu se reîncearcă automat, rămâne în folder pentru decizia operatorului.

---

## 2. Fișiere atinse

### Server (`PYTHON/`) — pașii 01 și 05
| Fișier | Ce |
|---|---|
| `routes/forexe/pdf.py` | `X-Semnatura` (validat pe lista rolurilor familiei, rescris în ordine canonică; absent = coloana neatinsă) + `UPDATE Semnatura` în aceeași tranzacție. Stocare pe bucăți când baza are DDL-ul 0078 (probă per cerere în `information_schema`; altfel fișier întreg, ca în 0041). Descărcarea servește `Continut` când există, altfel reface din bucăți și **reverifică suma** (nepotrivire = 500, niciodată octeți greșiți). Răspunsul PUT poartă și `semnatura`. |
| `routes/forexe/ord.py` | `o.Semnatura` -> câmpul `semnatura` pe fiecare ordonanțare. |
| `utils/pdf_chunks.py` | **NOU**, pur: tăiere după conținut (gear hash, 1 KB / ~4 KB / 64 KB, tabel fix derivat din SHA-256), listă împachetată, zlib per bucată, refacere cu verificare per bucată. |
| `scripts/pdf_chunks_cleanup.py` | **NOU**, rulat de mână: șterge bucățile nefolosite mai vechi de o oră; `--convert` transformă rândurile vechi (fișier întreg) în liste de bucăți, verificând suma fiecăruia; `--dry-run`. |
| `tests/test_pdf_chunks.py` | **NOU** — scris, **nerulat**. |

### DDL
| Fișier | Ce |
|---|---|
| `sql/0078_fx_pdf_bucati.sql` | **NOU**: `FX_PDF_BUCATI` (`Sha256 binary(32)` PK, `Dimensiune`, `Continut mediumblob` zlib, `DataCreare`) + `Bucati mediumblob NULL` pe `FX_DDF_PDF`/`FX_ORD_PDF`, `Continut` devine NULL-abil, CHECK «exact una din două forme». Formele verificate pe `MariaDB_Schema/000_DEMO.sql` și `AVACONT_SURSA.sql`. |

### Client
| Fișier | Ce |
|---|---|
| `KBot.Api/IApiClient.vb`, `ApiClient.vb` | `UploadDdfPdfAsync` / `UploadOrdPdfAsync` primesc `semnatura` (-> `X-Semnatura`). ORD: `semnatura` mapat. |
| `KBot.Api/PdfDownloadResult.vb` | `PutPdfResponse.semnatura`. |
| `KBot.Api/UpsertAngajamenteRequest.vb`, `KBot.Domain/OrdInfo.vb`, `DdfInfo.vb` | `OrdHeaderRow.Semnatura` (nou), comentariul lui `RevizieRow.Semnatura` adus la zi. |
| `KBot.Controls/Adobe/AdobeSaveTrap.vb` | **NOU** — capcana (cârlig WinEvent per proces + măturare la 200 ms; clicuri POSTATE, nu trimise). |
| `KBot.Controls/Adobe/AdobeSaveDialogFilter.vb` | **NOU**, pur — ce e dialogul (Salvare ca / confirmare deținută de el / altul) + `SamePath`. |
| `KBot.Controls/Adobe/AdobePrefs.vb` | **NOU** — `bToggleCustomSaveExperience = 1` (dialogul standard Windows în loc de ecranul Adobe cu cloud). |
| `KBot.Controls/Adobe/AdobeNativeMethods.vb` | `EnumChildWindows`, `GetDlgCtrlID`, `SendMessageTimeout` (text), `GW_OWNER`, `WM_SETTEXT/GETTEXT/COMMAND`, `TDM_CLICK_BUTTON`, `ID*`. |
| `KBot.Controls/Adobe/AdobeReaderHost.vb` + `.md` | `SaveTrapEnabled`, `HostedPath`, evenimentele `DocumentSaved` / `SaveTrapFailed`; `Detach` așteaptă până la 5 s o salvare apăsată înainte să închidă Adobe. Regula «nu semna cât e găzduit» înlocuită. |
| `KBot.Xfa/PdfSignatures.vb` | **NOU** — `PdfSignatures.Read(bytes, docType)` -> `PdfSignatureInfo` (câmpuri, mască, `RoleNames`, `ToSemnatura`, `FieldKey`, neclasificate). `AdobeUtils.ClassifySigner` devine `Friend` (aceleași reguli, nu o a doua copie). |
| `KBot.App/Views/PdfSigningSession.vb` | **NOU** — sesiunea unui document afișat: semnale (capcană + FileSystemWatcher), așteptarea stabilizării, citirea, decizia, încărcarea, copia în cache, rezultatul. |
| `KBot.App/Views/PendingPdfUploads.vb` | **NOU** — `<AppDir>\PdfDeIncarcat\` (salvare, citire, conflict, încărcare, reîncercare generală). |
| `KBot.App/Views/SignedPdfFiles.vb` | **NOU** — citire partajată, așteptarea stabilizării, scriere în cache prin `.part`. |
| `KBot.App/Views/SigningMessages.vb` | **NOU** — mesajele operatorului (toate prin `KBotMessage`). |
| `KBot.App/Views/PdfCache.vb` | `PdfCacheResult.LocalReplaced`. |
| `KBot.App/Views/DdfView.vb`, `OrdView.vb` | Sesiunea per document, semnatul nu se arată până la verificare, reîncercarea copiei păstrate, mesajul de înlocuire, confirmarea regenerării, (DDF) fișierul din listă deschis ca revizie. |
| `KBot.App/Views/Ddf/ReaderHostPreview.vb` | Capcana mereu pornită pe AMBELE motoare, `Signing`, preferința Adobe scrisă o dată per proces, evenimentele `DocumentSaved` / `SaveCancelled` (pentru banc). |
| `KBot.Controls/Adobe/AcroPdfSurface.vb` | Capcana «Salvare ca» și pe motorul ActiveX: `SaveTrapEnabled`, `LoadedPath`, `DocumentSaved` / `SaveTrapFailed`, `OwnerPids` (procesele care dețin ferestrele Adobe `AVL_*` din control). `Clear` și o încărcare nouă așteaptă o salvare apăsată. |
| `KBot.Controls/Adobe/AdobeSaveTrap.vb` | `PidSource`: procesele se recitesc la fiecare verificare (la ActiveX procesul Adobe nu e cunoscut dinainte și poate apărea după încărcare). |
| `KBot.Controls/Adobe/AdobeHostLog.vb` | Evenimentul `LineWritten` — fiecare linie, pe loc, pentru jurnalul live al bancului. |
| `KBot.App/HarnessTests/PdfSigningHarnessForm.vb` (+ `.Designer.vb`), `PdfSigningHarnessTest.vb` | **NOU** — bancul de semnare (DevHarness → «Adobe/PDF»), vezi §5. |
| `KBot.App/Views/Ddf/DdfPageContext.vb`, `Ord/OrdPageContext.vb`, `Ddf/DdfDocumentPage.vb`, `Ord/OrdDocumentPage.vb` | Sesiunea călătorește prin context până la previzualizare. |
| `KBot.App/Program.vb` | Reîncercarea copiilor păstrate după afișarea shell-ului. |
| `tests/KBot.App.Tests/*` (9 dubluri `IApiClient`) | Parametrul `semnatura` adăugat. |
| `tests/KBot.Controls.Tests/AdobeSaveDialogFilterTests.vb`, `tests/KBot.Xfa.Tests/PdfSignatureInfoTests.vb` | **NOI** — scrise, **nerulate**. |

FileVersion: `KBot.Controls` 1.51.0.0 ▸ **1.52.0.0**, `KBot.Xfa` 1.2.0.0 ▸ **1.3.0.0**,
`KBot.Api` 1.0.7.0 ▸ **1.0.8.0**, `KBot.Domain` 1.2.1.0 ▸ **1.2.2.0**, `KBot.App` 1.0.37.2 ▸ **1.0.38.0**.

---

## 3. Rezultatele testelor

- `dotnet build src\KBot.App\KBot.App.vbproj --no-incremental` (construiește și Controls, Xfa, Api,
  Domain, Common, Theming): **0 erori, 0 avertismente**. `KBot.Api` și `KBot.Controls` construite și
  separat: 0 / 0.
- Python: `ast.parse` curat pe `pdf.py`, `ord.py`, `pdf_chunks.py`.
- Tăietorul de bucăți verificat **în memorie** pe cele 8 DDF-uri reale (fără bază, fără server):
  refacere identică la octet pe toate 8; primul 251.634 octeți noi, următoarele 5.236–6.442; ~55 ms
  per fișier.
- **Nicio suită rulată sau construită** (regula casei: fără `dotnet test` / `pytest` / build de
  proiecte de test). Testele noi și dublurile modificate NU au fost compilate.

---

## 4. Rămas NEVERIFICAT / amânat

1. **Nimic nu a rulat pe ecran.** Capcana n-a întâlnit niciodată un dialog Adobe real. Structura
   dialogului (id-urile 1148/1001 ale casetei de nume, butonul IDOK, confirmarea TaskDialog vs.
   casetă clasică) e cea documentată de Windows, **nu** cea observată pe Adobe-ul operatorului.
   Capcana scrie în `adobe_preview.log` fiecare dialog pe care îl vede (cu clasă, proces, titlu și
   ce a găsit în el) — prima rulare dă dovada.
2. **`bToggleCustomSaveExperience`** — nume luat din firele comunității Adobe, neverificat pe
   versiunea instalată. Dacă e greșit, Acrobat DC arată propriul ecran «Salvare ca» (nu `#32770`),
   capcana îl loghează ca «alt dialog» și NU îl poate completa. De confirmat pe ecran.
3. **Numele reale ale câmpurilor de semnătură DDF** nu sunt în depozit (`ddf_demo.xml` e o copie a
   machetei ORD). Maparea pe roluri folosește regulile existente din `ClassifySigner`; un câmp
   nerecunoscut e logat ca «fără rol cunoscut», PDF-ul se încarcă oricum, dar rolul lui nu ajunge
   în `Semnatura`. De notat numele reale de pe primul DDF semnat.
4. **Motorul ActiveX** are aceeași capcană (cererea operatorului din 23.09.2026: «mergem până la
   capăt cu ActiveX»; motorul îl alege DOAR operatorul, K-BOT nu-l schimbă și nu mai scrie niciun
   avertisment). Neverificat: dacă AcroPDF desenează documentul dintr-un proces Adobe separat sau
   din procesul K-BOT. Capcana urmărește procesele care dețin ferestrele `AVL_*` din control
   (jurnalul spune «proces Adobe nou urmărit N (proces «X»)»). Dacă e procesul K-BOT însuși, un
   dialog de fișiere al K-BOT deschis cât timp un document e afișat ar fi prins și el — de văzut
   pe banc. Aceeași limită ca la fereastra găzduită: Adobe e mono-instanță, deci un «Salvare ca»
   din alt Acrobat deschis de operator în același proces ar fi redirecționat.

   **Măsurat pe banc, 23.09.2026 (prima rulare ActiveX):**
   - Semnarea se aplică în MEMORIE: documentul arată «Digitally signed by AD.CREDIT», dar fișierul
     de pe disc rămâne neschimbat (aceeași sumă) și Acrobat NU cere salvarea. Salvarea vine abia
     când operatorul apasă dischetă din bara Adobe.
   - Atunci apare un «Save As» `#32770` standard (caseta de nume = ComboBox > Edit, butonul
     «&Save»), cu numele propus `C__Users_..._BANC_DDF_41.pdf` în `Downloads`.
   - Dialogul aparține procesului BROKER (`Acrobat.exe /o /eo /l /b /ac /id <pid K-BOT>`, părinte =
     K-BOT), iar ferestrele `AVL_*` din control aparțin copilului RENDERER (`--type=renderer`).
     Capcana urmărea doar renderer-ul, deci dialogul a trecut nevăzut (și nelogat). Reparat:
     `OwnerPids` întoarce toată familia; dialogurile Adobe din procese neurmărite se loghează.
   - A doua rulare: dialogul a fost văzut, dar caseta de nume NU (`numeFișier=False`). Măsurat pe un
     «Save As» real (Windows 11): `DirectUIHWND > FloatNotifySink > ComboBox (id 0) > Edit (id 1001)`
     -- id-ul e pe Edit, nu pe ComboBox. Bara de adresă are tot un Edit în ComboBox, dar cu id 41477.
     Reparat în `FindFileNameEdit`.
5. **Proprietarul dialogului**: fereastra Adobe e copil al panoului nostru, deci dialogul modal
   poate avea ca proprietar forma K-BOT. Filtrul se uită la PROCESUL dialogului (PID-ul găzduit +
   cel pornit), nu la proprietar, tocmai de aceea — dar comportamentul real n-a fost văzut.
6. **DDL-ul 0078** nu a rulat pe nicio bază. Ordinea e liberă (ruta sondează), dar până nu rulează
   pe o bază, acea bază stochează mai departe fișiere întregi.
7. **401 la încărcare** nu trece prin `WithReauth` (plasa e tipizată pe `DdfInfo`/`OrdInfo`):
   o sesiune expirată duce copia în `PdfDeIncarcat`, de unde pleacă la următoarea deschidere sau
   pornire. Acceptat, notat.
8. **Plan vs. realizare:** planul cerea extinderea `INativeWindows` pentru apelurile noi; nu s-a
   făcut — decizia e în filtrul pur (testabil), iar apelurile Win32 stau în capcană, ca în
   `AdobePopupWatcher`. Un singur jurnal de felie în loc de cinci.

## 5. De făcut de operator înainte să funcționeze

1. VPS: copiat `routes/forexe/pdf.py`, `routes/forexe/ord.py`, `utils/pdf_chunks.py`,
   `scripts/pdf_chunks_cleanup.py`; repornit gunicorn.
2. Rulat `sql/0078_fx_pdf_bucati.sql` pe `AVACONT_SURSA` și pe fiecare bază de unitate.
3. Opțional: `pdf_chunks_cleanup.py --convert --dry-run`, apoi fără `--dry-run`, pentru rândurile
   din 0041.
4. Pe ecran (lista de verificare): generat un DDF, semnat câmpul A — dialogul «Salvare ca» NU se
   vede, fișierul din `TempPdf` e suprascris, `adobe_preview.log` arată dialogul prins; pe server
   rândul apare cu `Semnatura = A`; semnat B — a doua încărcare, `A,B`; modificat de mână fișierul
   din cache și redeschis — mesajul apare și se descarcă originalul; fără rețea — copia ajunge în
   `PdfDeIncarcat\` și pleacă la repornire; aceiași pași pe ORD; al doilea DDF adaugă ~5 KB în
   `FX_PDF_BUCATI`, iar descărcarea are aceeași sumă.

---

## 5. Bancul de semnare (DevHarness → «Adobe/PDF»)

Cerut de operator pe 23.09.2026, după prima rulare pe motorul ActiveX. Pornire Debug ▸ «Nu»
(bancul de probă) ▸ categoria «Adobe/PDF» ▸ «Semnare PDF — capcana «Salvare ca», încărcare pe
server, recitire». Marcat distructiv: cu bifa «Încarcă pe server» o semnătură ajunge REAL pe server
pentru id-ul scris și scrie `Semnatura`.

Folosește piesele REALE ale paginilor DDF / ORD (`ReaderHostPreview`, `PdfSigningSession`,
rutele PDF), cu motorul din «Setări» (afișat, niciodată schimbat). Lucrează mereu pe o copie în
`TempPdf\Banc\` (golit la fiecare pornire). În dreapta, `adobe_preview.log` în timp real.

Pași:
1. «Autentificare…» (aceeași fereastră de login), tip DDF/ORD, id (IDREV / IDORDP).
2. «Deschide un PDF local…» (copie) sau «Deschide copia de pe server».
3. Semnați în Adobe. Jurnalul arată dialogul prins, textul scris, «Salvare» apăsat, confirmarea,
   apoi citirea bancului: câmpurile semnate, `Semnatura` calculată, suma.
4. Cu bifa pusă: rezultatul încărcării (sha nou, roluri) + mesajul operatorului.
5. «Compară cu serverul»: descarcă, citește semnăturile ambelor copii, IDENTIC / DIFERIT.
6. «Deschide copia de pe server» = recitirea: copia de pe server înlocuiește copia de lucru.
Fără bifă (sau fără id / login) se testează doar capcana, nimic nu pleacă pe server.

## 8. Reparații după primul test în aplicație (23.09.2026, seara)

**Cauza celor două simptome aleatorii** («nu spune niciodată salvat» / «spune salvat, dar nu e pe
server»), citită în `adobe_preview.log`: vederea DDF și vederea ORD își țin fiecare vizualizatorul,
deci **două capcane rulau odată**, iar toate controalele AcroPDF ale procesului sunt servite de
**același** broker Acrobat. Ambele capcane prindeau același «Save As» și își scriau pe rând calea.
Exemplu 16:35:51: DDF 47 semnat a ajuns în `ORD_NR_1_AAB2EF2MCP4.PDF` și a fost încărcat ca
**ORD 1** (roluri «AB», sha 98b7f992) — verificat: fișierul are machetă DDF.

**De reparat pe server de operator (000_DEMO):** ORD 1 are pe server PDF-ul lui DDF 47.

```sql
DELETE FROM FX_ORD_PDF WHERE IDORDP = 1;
UPDATE FX_ORD SET Semnatura = NULL WHERE IDORDP = 1;
-- doar dacă 0079 era deja aplicat:
DELETE FROM FX_PDF_SEMNATURI WHERE IDORDP = 1;
```

**Reparat în cod:**

| Ce | Unde |
|---|---|
| Un «Save As» are **un singur proprietar**: capcana al cărei document e numele propus de Adobe; altfel cea al cărei vizualizator e pe ecran; altfel nimeni — dialogul se anulează. | `AdobeSaveTrap` (`Claims`, `Arbitrate`, `ChooseOwner`), `AdobeSaveDialogFilter.NameMatches`, `AcroPdfSurface.IsOnScreen` |
| «Salvat» doar când dialogul s-a **închis** (sau stă ascuns ≥ 1 s). Înainte, prima clipire (dialogul se ascunde o clipă la apăsare) ridica «salvat» înainte ca Adobe să fi scris. Dacă reapare fără «replace?» se apasă din nou (max. 3). | `AdobeSaveTrap.CheckPending` |
| Confirmarea «replace?» se apasă o singură dată. | `HandleConfirm` |
| Mesajele de script («Warning: JavaScript Window») primesc **OK prin mesaj** (`SendMessageTimeout WM_COMMAND`, nu taste); consola «JavaScript Debugger» primește **Cancel**, apoi e ascunsă dacă rămâne. Textul lor merge în jurnal. O casetă cu mai multe butoane e o întrebare și rămâne operatorului; o casetă de alertă nu se ascunde niciodată (modală ascunsă = Adobe blocat). | `AdobeSaveTrap.DismissScriptNoise`, `AdobeNativeMethods.SendWithTimeout` |
| Vizualizator gol («după 10 încercări … niciun arbore de panouri»): controlul se recreează și fișierul se reîncarcă **o dată**; jurnalul spune câte ferestre / vederi Adobe avea controlul. | `AcroPdfSurface.ShowDocumentAsync`, `DescribeTree` |
| Folderele PDF implicite: `C:\KBOT\Temp\PDF` (generate, golit la pornire), `...\PDF\DDF\` și `...\PDF\ORD\` (copiile de pe server — golite și ele la pornire, fiindcă `Wipe` e recursiv; serverul e sursa). Vechiul implicit `C:\AVACONT\FOREXE\PDF\...` scris în `kbot_paths.json` se citește ca noul implicit. | `KBotPaths`, `SetariFoldere` |
| Fără PDF pe server → **niciodată** un fișier local: doar ce a produs «Generează» în sesiunea asta, altfel butonul de generare. | `DdfView` / `OrdView.BuildCurrentContext` |

Rămâne: pagina «Fișiere» deschide în continuare un fișier ales explicit din listă (acum doar din
copiile locale de sub `C:\KBOT\Temp\PDF`).

FileVersion: Common 1.5.2.0 → **1.5.3.0** (celelalte proiecte deja crescute pentru 0078).
Build `KBot.App` (cu tot lanțul): **0 erori, 0 avertismente**. Nimic rulat pe ecran.

### 8.1 Același seară — după al doilea test

- Jurnalul a arătat că mesajele de script ERAU prinse, dar «apăsat butonul 0»: în alerta Adobe
  toate butoanele (bifă, OK, fără nume, Cancel) stau într-un GroupBox și au id **0**, deci un
  `WM_COMMAND` după id nu apasă nimic. Acum butonul «OK» se caută după TEXT și se apasă el însuși
  (`BM_CLICK`, trimis cu `SendMessageTimeout`); reapare → încă o apăsare după 700 ms, max. 3, apoi
  ascuns. Consola «JavaScript Debugger» și cadrul «Warning: JavaScript Window» fără butoane se
  ascund (`SW_HIDE`), la FIECARE apariție (Adobe arată consola din nou la următoarea eroare).
- Reîncărcarea o dată a vizualizatorului gol a funcționat în ambele cazuri din jurnal (19:05, 19:08:
  «0 ferestre, 0 vederi Adobe» → «a doua încărcare a reușit»).
- `AVDocumentHeaderView` (banda de 102 px de deasupra paginii) se ascunde, iar fereastra de sub
  ea urcă la locul ei și crește cu aceeași înălțime; refăcut la 400 ms după orice redimensionare
  (Adobe își refă aranjarea). `AcroPdfSurface.HideDocumentHeader`.
- Build `KBot.App`: 0 erori, 0 avertismente. Nimic rulat pe ecran.

### 8.2 Timerele se opresc când vizualizatorul nu e pe ecran

Cererea operatorului: toate timerele se opresc cât vizualizatorul nu e pe ecran și repornesc când
revine. `AcroPdfSurface` urmărește `VisibleChanged` / `ParentChanged` ale panoului (se propagă și
de la părinți: altă vedere, altă pagină) și `Resize` al formei (minimizare). Nu e pe ecran →
`AdobeSaveTrap.Pause()` (timerul de 200 ms oprit, cârligele WinEvent scoase; ținta și procesele
păstrate) și timerul de reaplicare a antetului oprit. Revine → `Resume()` (cârlige + timer) și o
reaplicare a antetului. O salvare deja apăsată e urmărită până la capăt, apoi timerul se oprește.
O capcană suspendată nu poate fi aleasă proprietarul unui «Save As» — deci cu DDF și ORD deschise,
dialogul ajunge la singura capcană activă, cea de pe ecran.

### 8.3 Ritm rapid la erori de script; antetul căutat corect

- Din momentul în care apare un mesaj de script, capcana verifică la **100 ms**
  (`AdobeSaveTrap.ScriptBurstIntervalMs`); după **3000 ms** fără alt mesaj
  (`ScriptBurstQuietMs`) revine la 200 ms (`SweepIntervalMs`). Ambele trecute în jurnal.
- Antetul nu se ascundea: jurnalul nu avea nicio linie despre el, deci nu era găsit. Căutarea
  folosea `Walk` (adâncime 14) o singură dată, imediat după colapsare. Acum caută în TOT arborele
  (`EnumChildWindows`), pe timerul de reaplicare: până la 15 încercări la 400 ms după încărcare
  (antetul poate apărea abia după randarea paginii), 3 după o redimensionare / revenire pe ecran;
  la final, dacă nu l-a găsit, o spune în jurnal cu numărul de ferestre din control.
- Build `KBot.App`: 0 erori, 0 avertismente. Nimic rulat pe ecran.
