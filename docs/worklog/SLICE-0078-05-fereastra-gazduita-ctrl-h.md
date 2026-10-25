# SLICE 0078-05 — Fereastra găzduită fără poziționare / ascundere; Ctrl+H; proba ordinii salvării

Jurnal regrupat în D001-05 (24–25.10.2026): intervenția inițială 0078-05 și intervențiile 0078-07/08/09/11/12/13/14 sunt păstrate mai jos, cu identificator, dată, fișiere, probe și limite proprii. Relatările sunt istorice: 0078-09 adoptă în final «Nu» la închidere; 0078-13 modifică recunoașterea confirmării din 0078-12 fără să dovedească ipoteza proprietarului; rescrierea ActiveX ulterioară rămâne în jurnalul separat 0078-15. Nu se declară rerulări sau publicări prin această regrupare.

Repere: [05](#slice-0078-05), [07](#slice-0078-07), [08](#slice-0078-08), [09](#slice-0078-09), [11](#slice-0078-11), [12](#slice-0078-12), [13](#slice-0078-13), [14](#slice-0078-14).

<a id="slice-0078-05"></a>

Cererea operatorului, 29.09.2026, după proba 0078-04:

1. Confirmat: cu motorul «Fereastră găzduită» erorile JS («GeneralErrorOperation failed») la
   deschiderea unui DDF semnat NU apar. Deci cauza e motorul ActiveX (AcroPDF), nu documentul.
2. Problemă nouă: la semnarea în fereastra găzduită apare ÎNTÂI «Salvare ca», abia apoi se
   salvează efectiv (după PIN-ul tokenului). Teama: documentul semnat nu ajunge pe server.
   Cerere: o probă pe banc pentru asta.
3. Tot ce ține de POZIȚIONAREA și ASCUNDEREA barelor Adobe în fereastra găzduită se scoate: strică
   Adobe-ul operatorului (nu revine după închiderea K-BOT). În locul lor: Ctrl+H (mod citire,
   care ascunde barele implicit).

## 1. Fereastra găzduită — ce s-a scos, ce a intrat

`AdobeReaderHost` (KBot.Controls) rescris pe calea livrată:

- **Scos:** profilele măsurate (decupare sus 152 / dreapta 230, deplasare dx −130), pornirea cu
  `/A "toolbar=0&navpanes=0"`, detectarea generației UI + repornirea «pentru profil»,
  `Mode` / `ReapplyProfile` / `CurrentChoice` / `LastDetection`, supraveghetorul insignei
  plutitoare (`AdobePopupWatcher`, `PopupWatchEnabled`).
- **Acum:** fereastra umple exact panoul (0,0,lățime,înălțime). Linia de comandă: `/n /s "fișier"`
  (`/n` pe «Automat» = DA — proces al K-BOT, nu instanța operatorului; «Nu» îl scoate).
- **Ctrl+H** (`ArmReadMode`): un cronometru de 300 ms, max. 30 s după încorporare, trimite o
  singură dată Ctrl+H (SendInput) când: panoul e pe ecran; `AVPageView` e vizibilă și are mărime;
  nu e o rafală de mesaje de script (`AdobeSaveTrap.InScriptBurst`, reverificat la
  `ScriptBurstEnded`); fereastra K-BOT e activă și în prim-plan; după `SetFocus` pe pagină,
  focusul e chiar în fereastra Adobe. Altfel scrie în jurnal de ce așteaptă, iar la 30 s spune
  operatorului să apese Ctrl+H. Modul citire e o stare a documentului deschis: nu intră în
  preferințele Adobe.
- `IsSaving` (nou): o «Salvare ca» apăsată de capcană nu s-a terminat încă.

«Setări» ▸ «Opțiuni fereastră găzduită…»: rămân doar «Instanță nouă Adobe (/n)» și «La schimbarea
documentului». Scoase: «Mod vizualizator Adobe» și «Ascunde fereastra plutitoare». Valorile
stocate (`AdobeViewerMode` în kbot_paths.json, `AdobePopupWatch` în app_settings.json) rămân în
fișiere, necitite.

**Nu s-a atins:** motorul ActiveX (calea veche cu colapsare / ascundere / antet și «ActiveX — mod
citire»), bancul de măsurat profile din DevHarness (`AdobeReaderHarnessForm`) și clasele pe care
doar el le mai folosește (`AdobeViewerProfile(s)`, `AdobeHostGeometry`, `AdobePopupWatcher`,
`AdobeUiDetector`, `AdobeCreationHook` rămâne opțiune de captură).

## 2. Proba ordinii salvării (bancul «Semnare PDF»)

DevHarness ▸ «Adobe/PDF» ▸ «Semnare PDF — capcana «Salvare ca», încărcare pe server, recitire»:

- **«Forțează fereastra găzduită»** (bifată implicit): vizualizatorul bancului folosește fereastra
  găzduită oricare ar fi setarea (`ReaderHostPreview.ForcedEngine`; setarea nu se scrie).
- **«Doar simulează încărcarea»**: o `PdfSigningSession` ADEVĂRATĂ pe `RecordingPdfApi` (un
  `IApiClient` făcut cu `DispatchProxy` care doar înregistrează încărcarea). Fără server, fără
  autentificare. Linia «ÎNCĂRCARE SIMULATĂ» spune ce s-ar fi trimis (octeți, roluri, câmpurile
  noi) și la câte ms după închiderea dialogului.
- **Cronologia pe disc** (`PdfSaveTimeline`, liniile «[Disc]»): orice eveniment din folderul
  documentului (inclusiv fișierele temporare ale Adobe), orice schimbare de mărime / oră de
  scriere a documentului, toate cu «+N ms după închiderea dialogului», și semnăturile când
  fișierul s-a oprit din schimbat. La închiderea dialogului se scrie dacă fișierul era încă
  NESCHIMBAT.

Pași: deschide PDF-ul (ex. `BANC_DDF_42_A.pdf`) cu «Doar simulează încărcarea» bifat ▸ semnează
▸ urmărește în jurnal: «SALVAT de capcană (dialogul s-a închis)» ▸ «[Disc] Fișier: … octeți» ▸
«[Disc] Fișierul s-a oprit din schimbat … N câmpuri semnate» ▸ «ÎNCĂRCARE SIMULATĂ … +N ms».

## 3. Ce spune codul (ipoteză, nerulat)

`PdfSigningSession` are două semnale: `NotifySaved` (dialogul închis) și un FileSystemWatcher pe
fișier. La dialog închis fișierul e încă cel vechi (sumă = serverul ▸ nimic de făcut); când Adobe
scrie fișierul semnat, watcher-ul pornește o a doua verificare ▸ încărcare. Deci ordinea «dialog
întâi, scriere după» ar trebui să fie acoperită — proba de mai sus o confirmă sau nu.

**Riscul real:** `Detach()` așteaptă doar cât dialogul e deschis (`IsBusy`). Dacă operatorul
schimbă documentul / revizia DUPĂ ce dialogul s-a închis dar ÎNAINTE ca Adobe să scrie (PIN-ul
tokenului!), sesiunea se închide și Adobe e oprit (`KillProcess`) ▸ semnătura se poate pierde.
Bancul avertizează la eliberare dacă `IsSaving`; cronologia arată cât durează fereastra de risc.

## Fișiere

- `src/KBot.Controls/Adobe/AdobeReaderHost.vb` — rescris (vezi §1).
- `src/KBot.Controls/Adobe/AdobeNativeMethods.vb` — `GetAncestor` + `GA_ROOT`.
- `src/KBot.Controls/Adobe/AdobeHostSettings.vb` — fără comutatorul insignei.
- `src/KBot.Controls/Adobe/AdobeViewerSettings.vb` — eticheta «Automat (Da)» pentru `/n`.
- `src/KBot.App/Views/Ddf/ReaderHostPreview.vb` — fără profil / insignă / `ReapplySettings`;
  `ForcedEngine`, `IsSaving`.
- `src/KBot.App/Views/Ddf/DdfFisierPreview.vb` — fără profil.
- `src/KBot.App/Setari/AdobeGazduireForm.vb` + `.Designer.vb` — două setări în loc de patru.
- `src/KBot.App/Setari/SetariAplicatieView.vb` + `.Designer.vb` — texte; `AdobeModeItem` scos.
- `src/KBot.App/HarnessTests/PdfSigningHarnessForm.vb` + `.Designer.vb` — cele două bife,
  cronologia, încărcarea simulată.
- NOI: `src/KBot.App/HarnessTests/PdfSaveTimeline.vb`, `src/KBot.App/HarnessTests/RecordingPdfApi.vb`.
- FileVersion: Controls 1.53 ▸ **1.54** (App rămâne 1.1.0.2, deja crescut în 0078-04, nelivrat).

Build `src\KBot.App` (cu Controls, Xfa, DevHarness): **0 erori, 0 avertismente**. Nimic rulat.
Teste rămase în urmă (nerulate): `AdobeReaderHostTests` nu mai verifică profilul (nu are ce);
`DdfViewTests.AdobeModeCombo_*` căutau deja un `cboAdobeMod` pe pagina DDF care nu mai există
din 0072-01.

## 4. Proba 29.09.2026 09:04–09:12 (bancul Secțiunea B, fereastră găzduită) — și ce a arătat

A → B → Ordonator semnate, fără PIN (09:04–09:06) și cu tokenul cu PIN (09:09–09:12); la
redeschidere toate trei cu integritate OK. **Dar** citirea de după «Salvare ca» a găsit de fiecare
dată câmpul nou cu semnătura GOALĂ («can't decode PKCS7SignedData», semnatar gol, fără dată), iar
la PIN citirea a venit chiar ÎNAINTE de «Token Logon» (09:11:12.190 vs 09:11:12.373). Adobe scrie
fișierul de DOUĂ ori: întâi cu câmpul semnat + un loc gol pentru semnătură, apoi, după PIN,
semnătura în acel loc, pe loc (aceeași mărime).

**Defect găsit:** `PdfSignatures.Read` numără câmpul după NUME, deci locul gol = «semnat».
`PdfSigningSession` ar fi încărcat fișierul pe jumătate scris, iar la a doua scriere cheia
câmpurilor nu se mai schimba ▸ fișierul bun NU ar fi ajuns niciodată pe server.
**Reparat:** sesiunea încarcă doar când toate semnăturile se decodează și se verifică
(`UnfinishedSignatures` ▸ `XfaSignedDocument.CheckSignatures`); altfel reverifică la 1 s, max.
180 s (timp pentru PIN), apoi mesaj «Semnați din nou» fără încărcare. Bancul Secțiunea B așteaptă
la fel și spune în cât timp s-a terminat semnătura.

**Ctrl+H lent:** ~4,5 s la fiecare deschidere în «fereastra K-BOT nu e în prim-plan». Acum, când
prim-planul e al unui proces Adobe, K-BOT îl ia înapoi (`SetForegroundWindow`) și trimite; altfel
jurnalul spune a cui e fereastra din prim-plan. Cronometrul: 300 ▸ 150 ms. Nerulat.

## 5. NOTAFD.xml după inserarea Secțiunii B

Operatorul, 29.09.2026: atașamentul NOTAFD.xml din document are tot `___________` / `___`
(făcut de «Valideaza» pe B provizorie; după inserare nu mai validează nimeni). Verificat în
`BANC_DDF_41_B_091049.pdf`: obiectul 142 (`/Filespec` 141, `text/plain`, Flate), rândul
`rowT_ang_ctrl_ang` cu `cod_angajament="___________" indicator_angajament="___"`.

`XfaSignedDocument.FillIncremental` (acum Function, întoarce o linie pentru jurnal) înlocuiește în
ACEEAȘI revizie incrementală și fluxul atașamentului: `DdfNotafdSync.SyncSectionB` ia fiecare rând
completat din `SubformSectiuneaB/Table3/Row1` (în ordine; rândul gol al machetei ignorat) și scrie
în `rowT_ang_ctrl_ang` corespunzător: `cod_angajament`=Cell1, `indicator_angajament`=Cell2,
`program`=Cell3, `cod_SSI`=Cell4, sumele Cell5..Cell10 (doar celulele cu valoare); plus
`ckbx_secta_inreg_ctrl_ang`=CheckBox9. Număr de rânduri diferit ▸ atașamentul NU se atinge (și se
spune). Fluxul: `/DL`, `/Params` (`Size`, `CheckSum` MD5, `ModDate`) refăcute; citit înapoi și
comparat (`VerifyEmbeddedFile`). Octeții semnați rămân neatinși. Nerulat.
⚠ De văzut în panoul de semnături Adobe cum apare schimbarea atașamentului după semnătura A (e
aceeași schimbare pe care ar face-o «Valideaza» apăsat din nou).

## 6. Schimbarea de după semnătură (postSign) — două variante, pe bancul Secțiunea B

Proba 09:18–09:21: după semnătură formularul (scripturile `postSign`) mai schimbă documentul;
Adobe întreabă la închidere dacă salvează; fără «Da» următoarea semnătură nu se mai activează.
Sesiunea încărca doar fișierul de la semnătură, fără acea schimbare. Două variante, ambele OPRITE
implicit în `PdfSigningSession` (producția neschimbată), alese pe banc din «După semnătură»:

- **A — `UploadSaveAfterSignature`:** o salvare ulterioară fără semnătură nouă se încarcă și ea,
  doar dacă e o ADĂUGARE la octeții încărcați ultima dată și toate semnăturile se verifică. Pe banc
  sesiunea rămâne vie 90 s după eliberarea documentului, ca să prindă «Salvați modificările? ▸ Da».
- **B — `SaveAfterSignature`:** când semnătura e scrisă, sesiunea cere salvarea
  (`ReaderHostPreview.RequestSave` ▸ `AdobeReaderHost.RequestSave`: Ctrl+S prin aceeași coadă ca
  Ctrl+H) și așteaptă max. 20 s schimbarea fișierului, apoi încarcă o singură dată. Doar fereastra
  găzduită.

Bancul are acum o sesiune REALĂ pe `RecordingPdfApi` (liniile «ÎNCĂRCARE SIMULATĂ» + semnăturile
octeților trimiși). Nume noi la fiecare pas: `BANC_DDF_<idrev>_A_HHmmss.pdf`, «Deschide…» copiază
în `<nume>_HHmmss.pdf`, inserarea ▸ `…_B_HHmmss.pdf` (`_2`, `_3` la aceeași secundă). Nerulat.

### 6.1 Proba 09:53–09:55 (token cu PIN): B merge, A scoasă

Varianta B: la A, B și Ordonator, Ctrl+S a plecat, fișierul s-a salvat în ~1,7 s, iar încărcarea
simulată conține revizia de după semnătură (A: «1/2»; B: «2/3», 384.883 octeți). Varianta A lăsa
documentul nesalvat ▸ SCOASĂ. **B e acum comportamentul de producție:** `ReaderHostPreview.Signing`
pune `SaveAfterSignature = RequestSave` pe orice sesiune primită (DDF, ORD, NC — fereastra găzduită;
pe ActiveX `RequestSave` răspunde False și se încarcă ca înainte). Așteptarea: 8 s după ce Ctrl+S a
plecat (`SaveKeysSent` ▸ `NotifySaveKeysSent`; după ULTIMA semnătură formularul nu mai are ce
debloca, deci fișierul poate rămâne neschimbat), 60 s doar când tastele n-au putut fi trimise
(mesajul «Salvare după semnătură» cere Ctrl+S de mână). Pe banc: «După semnătură» = «K-BOT salvează
singur» (implicit) sau «Nimic», pentru comparație. Butonul «Golește + închide Adobe» (jurnalul
rămâne): golește vizualizatorul și, după confirmare, închide toate procesele Adobe.

## 7. Oglinda pe server a încărcărilor bancului (doar 000_DEMO)

`sql/0078_05_kbot_banc_pdf.sql` ▸ tabela `KBOT_BANC_PDF`, DOAR în `000_DEMO`, fără legături cu
alte tabele: un rând NOU la fiecare încărcare (Tip, IdDoc, NumeFisier, Pas, Semnatura, Semnaturi
JSON, Statie JSON, ShaPrecedent, Sha256 recalculat pe server, Dimensiune, Operator, Continut).
Ruta `PUT /api/forexe/banc/pdf/<tip>/<iddoc>?nume=&pas=` (`routes/forexe/pdf_banc.py`): 403 pe orice
altă bază; aceleași reguli de fir ca PDF-urile reale (octeți bruți, `X-Sha256` verificat), fără
verificare de concurență. Client: `ApiClient.UploadBancPdfAsync` (nu e pe `IApiClient`). Bancul
Secțiunea B: bifa «Trimite și în tabela de probă (000_DEMO)» trimite fiecare încărcare simulată și
compară suma primită înapoi. FileVersion Api 1.0.14 ▸ **1.0.15**. ⚠ DDL de rulat pe `000_DEMO` +
ruta copiată pe VPS. Nerulat.
Citirea înapoi: `GET /api/forexe/banc/pdf` (rândurile, cele mai noi întâi, max. 500, fără conținut)
și `GET /api/forexe/banc/pdf/<id>` (octeții, `ETag` = suma, reverificată de client ca la PDF-urile
reale); tot doar pe 000_DEMO. `ApiClient.ListBancPdfAsync` / `DownloadBancPdfAsync`, DTO
`BancPdfRow` / `BancPdfList`. Bancul: «Încarcă PDF de pe server…» ▸ `BancPdfPickerForm` (listă cu
Id, Primit, Tip, IdDoc, Fișier, Roluri, Octeți, Pas, Operator) ▸ fișierul ales scris în
`<nume>_srv<Id>_HHmmss.pdf` și deschis.

## 8. Trecerea pe fereastra găzduită la actualizare + Adobe pe tot ecranul la închidere

**Acțiune unică** (`src/KBot.App/OneTimeActions.vb`, apelată din `Program.Main` imediat după
validarea folderelor, înaintea oricărei ferestre): pe fiecare mașină, o singură dată, motorul PDF
trece pe «Fereastra» (oricare ar fi fost: ActiveX / ActiveXCitire) și «/n» pe «Auto» (= /n) —
setările cu care a mers bancul pe 29.09.2026. Marcajul `0078-05-fereastra-gazduita` se scrie în
`kbot_paths.json` (`KBotPaths.AppliedOneTimeActions`; fișierul e păstrat de updater), deci o alegere
ulterioară a operatorului în Setări rămâne. Scrierea eșuată ▸ schimbarea ține doar sesiunea și se
reia la pornirea următoare. Urma: o linie în `adobe_preview.log` («Acțiune unică …»). Modul de
închidere (A/B, per utilizator) NU e atins — bancul a mers pe A, implicitul.

**Adobe pe tot ecranul la închidere** — opțiune, DEBIFATĂ implicit: Setări ▸ «Fereastră găzduită
Adobe» ▸ «La închiderea K-BOT: Readu fereastra Adobe la dimensiunea ecranului»
(`AppSettings.AdobeRestoreScreenOnExit` ▸ `AdobeHostOptions.RestoreScreenSizeOnExit`, prin
`AdobeHostSettings.ApplyTo`, deci de la documentul următor). Când e bifată, ultima eliberare —
închiderea ferestrei K-BOT care ține panoul (`FormClosing`, ne-anulat) sau `Dispose` al gazdei —
trece prin `AdobeScreenRelease`: ascunde fereastra, îi pune înapoi stilul de dinainte de găzduire,
o scoate din panou, o așază pe zona de lucru a monitorului ei, o MAXIMIZEAZĂ (se vede o clipă) și îi
trimite WM_CLOSE; 3 s de răgaz, apoi procesul e oprit doar dacă l-a pornit K-BOT; o fereastră
străină care nu se închide (de ex. Adobe întreabă de salvare) rămâne pe ecran. Schimbarea de
document NU e afectată (acolo rămâne regula: fereastra nu se dă înapoi niciodată). NEVERIFICAT: dacă
Adobe ține minte dimensiunea după o închidere așa — de testat de operator. FileVersion Common 1.5.6
▸ **1.5.7**. Nerulat.

---

<a id="slice-0078-07"></a>

## SLICE 0078-07 — Fereastra Adobe găzduită care se mărește singură

Data: 29.09.2026. Pe PC-ul unui client (KBot 1.1.0.4, `adobe_preview.log` + captura arborelui de
ferestre, lipite de operator): K-BOT a pus fereastra Adobe la mărimea panoului (15:25:44.829
«MUTAT 0,0 1920x1020 -> 0,0 654x586», a doua trecere «NESCHIMBAT»), a trimis Ctrl+H (15:25:45.455),
iar în captură: panoul `0x000D07B4` = **807×772**, fereastra `AcrobatSDIWindow` `0x00050846` =
**1919×1079** — tăiată, fără partea dreaptă și de jos. Număr: **0078-07** (propus).

**Verificat din captură:** panoul a crescut după așezare (654×586 ▸ 807×772; controalele ascunse
«Se încarcă documentul…» au rămas la mărimea veche). **Nu se știe** dacă fereastra Adobe a ajuns
la ecran întreg înainte sau după ce K-BOT a readus-o la noua mărime a panoului: redimensionarea la
schimbarea panoului nu scria nimic în jurnal. **Ipoteză:** fereastra venea MAXIMIZATĂ din Adobe-ul
operatorului (predată unei instanțe existente, «NU a fost creată de K-BOT»), iar K-BOT nu scoate
marcajul «maximizată» — Adobe se reface la ecran întreg la o reașezare proprie (probabil Ctrl+H).

### 1. Ce s-a schimbat

- **`AdobeSizeWatcher` (NOU, KBot.Controls/Adobe):** cârlig WinEvent `EVENT_OBJECT_LOCATIONCHANGE`
  pe procesul Adobe (în afara procesului — nimic injectat în Adobe), filtrat la fereastra găzduită;
  o rafală de evenimente = un singur `Changed`, la 100 ms după ultimul.
- **`AdobeReaderHost`:** după a doua trecere pornește urmărirea. Când Adobe își schimbă singur
  fereastra și ea nu mai e cât panoul: linia «Adobe și-a schimbat singur fereastra: <dreptunghi>
  (panoul: …; marcată «maximizată»: da/nu)», apoi o readuce (linia «Poziție (readusă la panou)»).
  Dacă Adobe o schimbă de peste 5 ori în 3 s, K-BOT se oprește (mesaj în jurnal) până la
  următoarea redimensionare a panoului. Oprit la `Detach`, la eliberarea pe ecran întreg, la `Dispose`.
- **Redimensionarea panoului** (`Relayout`) scrie acum în jurnal când fereastra NU a ajuns la
  mărimea cerută («Poziție (panoul s-a redimensionat)»); când a ajuns, tace (fără zgomot la tragerea
  separatorului).
- Stilul ferestrei Adobe NU se schimbă (marcajul «maximizată» doar se citește, pentru jurnal):
  regula 0078-05 — nimic din ce ar ține minte Adobe-ul operatorului.
- **Ctrl+2 după Ctrl+H** (cererea operatorului, 29.09.2026): `ArmReadMode` pune în coadă Ctrl+H și
  apoi Ctrl+2 — aceleași condiții, același focus, trimise în același lot, în ordinea asta; în jurnal
  «Ctrl+2 trimis documentului.». În Acrobat, Ctrl+2 = potrivire la lățime (din cunoștințe generale,
  neverificat pe versiunea clientului). Dacă nu poate fi trimis: «Apăsați Ctrl+2 în document…».
- (din 0078-06 §5) `ReaderHostPreview.pnlHost_SizeChanged`: `_host?.Relayout()`.

### 2. Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.Controls/Adobe/AdobeSizeWatcher.vb` | **NOU** |
| `src/KBot.Controls/Adobe/AdobeReaderHost.vb` | urmărirea, `OnAdobeResized`, `Fill(…, onlyWhenWrong)`, `Relayout` cu jurnal |
| `src/KBot.Controls/Adobe/AdobeNativeMethods.vb` | `WS_MAXIMIZE` |
| `src/KBot.App/Views/Ddf/ReaderHostPreview.vb` | `_host?.Relayout()` |

FileVersion: Controls 1.55 (necomis, posibil deja în 1.1.0.4) ▸ **1.56**. App: 1.1.0.4 e livrat ▸
de crescut la publicare (`push-update.ps1` întreabă).

### 3. Rezultatele testelor

`dotnet build src\KBot.App\KBot.App.vbproj` (cu Controls): **0 erori, 0 avertismente**. Nimic rulat.

### 4. Rămas NEVERIFICAT

1. Nerulat. Dacă ipoteza e bună, jurnalul următor de pe acel PC arată, după «Ctrl+H trimis», linia
   «Adobe și-a schimbat singur fereastra: 0,0 1919x1079 … «maximizată»: da» și readucerea.
2. Dacă Adobe se luptă (linia «de N ori în 3 s»), readucerea nu ajunge: rămâne varianta scoaterii
   marcajului «maximizată» la încorporare, cu riscul ca Adobe-ul operatorului să țină minte fereastra
   mică (de hotărât de operator).
3. Ctrl+2 se trimite o singură dată, la deschidere: după o readucere la panou (sau o redimensionare
   a panoului) Adobe poate păstra zoom-ul vechi, nepotrivit la noua lățime.
4. Readucerea poate retrimite Adobe-ului o reașezare care anulează modul citire (Ctrl+H) — de văzut.

> 30.09.2026: `AdobeSizeWatcher` a fost redenumit `AdobeWindowWatcher` și lărgit în 0078-08 (vezi [0078-08](#slice-0078-08)).

---

<a id="slice-0078-08"></a>

## SLICE 0078-08 — Arborele blocat cât Adobe încă deschide documentul

Data: 30.09.2026. Cererea operatorului: în orice arbore care, la clic, deschide un PDF, un «debounce»
— de preferat NU un cronometru, ci un semnal că Adobe a terminat de procesat. Cât e activ, clicul pe
alt rând nu e permis: un al doilea document trimis la Adobe cât primul încă se încarcă dă erori
Adobe aleatorii și lasă procese Adobe fantomă. Număr: **0078-08** (propus).

### 1. Ce s-a schimbat

#### 1.1 Semnalul «Adobe a terminat» (`AdobeReaderHost.DocumentReady`)

Ridicat O DATĂ pe document găzduit, când toate sunt adevărate:
- pagina e așezată (`AVPageView` cu marcaj propriu de vizibil și mărime — merge și cu panoul ascuns,
  când operatorul e pe altă pagină);
- nu rulează o rafală de mesaje de script (`AdobeSaveTrap.InScriptBurst`);
- Ctrl+H / Ctrl+2 au plecat sau s-a renunțat la ele (doar cât panoul e pe ecran — ascuns, tastele
  așteaptă panoul, documentul e deja gata);
- **nicio fereastră a procesului Adobe nu s-a mai arătat / ascuns / mișcat de 250 ms**.

Ultimul punct e semnalul venit de la Adobe: cârligul WinEvent din `AdobeWindowWatcher` (fost
`AdobeSizeWatcher`, 0078-07, redenumit; acum ascultă `EVENT_OBJECT_SHOW` … `EVENT_OBJECT_LOCATIONCHANGE`
pe tot procesul, doar ferestre întregi) ridică `Quiet` când o rafală de evenimente s-a încheiat.
Singurul cronometru e cel care strânge rafala (250 ms fără evenimente), nu o așteptare ghicită.
«Poke» după găzduire (Adobe poate fi deja gata — atunci n-ar mai veni niciun eveniment), după
trimiterea tastelor și după o rafală de script.

**Plasă de siguranță:** dacă Adobe nu termină în 60 s (un mesaj de eroare, fereastra închisă),
`DocumentReady` pleacă oricum, cu linia «Document gata în Adobe (ATENȚIE: Adobe nu a terminat în
60 s…)». Fără ea, un document care nu se termină ar ține arborii blocați. În jurnal, calea normală:
«Document gata în Adobe (pagina așezată, ferestrele Adobe nu s-au mai schimbat de 250 ms).»

#### 1.2 Poarta comună (`AdobeOpenGate`, KBot.Controls/Adobe)

Una pe aplicație: clicul care trimite un document nou poate veni din mai mulți arbori. Proprietarii:
- `ReaderHostPreview` (paginile «Document» DDF / ORD / Note CAB, recepția, bancurile): intră la
  trimiterea documentului spre Adobe; iese la `DocumentReady`, la eșecul găzduirii, la fișier lipsă,
  la `Clear`, la eliberare; pe motorul ActiveX iese când încărcarea (care așteaptă așezarea) se întoarce;
- `DdfFisierPreview` (pagina «Fișiere»): steagul lui «ocupat» (care deja bloca lista) ține acum până
  la `DocumentReady` pentru PDF-uri și intră și în poartă.

#### 1.3 Arborii

`AdvancedTreeControl.SelectionLocked` (NOU, doar la rulare, nu se scrie în designer): apăsarea pe un
rând, dublu-clicul și tastele de navigare sunt ignorate; eliberarea unei apăsări refuzate e ignorată
și ea; cursorul = `AppStarting`. Derularea, lupa, capul și subsolul merg. Legați de poartă
(`AdobeOpenGate.LockWhileOpening`) după `InitializeComponent`: arborele principal (`KbotForm`),
`DdfView`, `OrdView`, `NoteCabView`.

#### 1.4 Ajutorul

`contabil.liste` — secțiune nouă «Cât se deschide un document» (eticheta `0078-08`), trecută ca
**0000-16**.

### 2. Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.Controls/Adobe/AdobeWindowWatcher.vb` | **NOU** (înlocuiește `AdobeSizeWatcher.vb`, șters) |
| `src/KBot.Controls/Adobe/AdobeOpenGate.vb` | **NOU** |
| `src/KBot.Controls/Adobe/AdobeReaderHost.vb` | `DocumentReady`, `IsDocumentReady`, `OnAdobeQuiet`, termenul de 60 s, `FindPageView` |
| `src/KBot.Controls/Tree/AdvancedTreeControl.Lock.vb` | **NOU** — `SelectionLocked` |
| `src/KBot.Controls/Tree/AdvancedTreeControl.Overrides.vb`, `.Keyboard.vb` | refuzul clicurilor / tastelor |
| `src/KBot.App/Views/Ddf/ReaderHostPreview.vb` | intrare / ieșire din poartă |
| `src/KBot.App/Views/Ddf/DdfFisierPreview.vb` | «ocupat» până la `DocumentReady` + poarta |
| `src/KBot.App/KbotForm.vb`, `Views/DdfView.vb`, `Views/OrdView.vb`, `Views/NoteCabView.vb` | `LockWhileOpening(tree)` |
| `src/KBot.App/HelpContent/contabil/liste.md` | secțiunea nouă (0000-16) |

FileVersion: Controls 1.56 ▸ **1.57**. App: de crescut la publicare (`push-update.ps1`).

### 3. Rezultatele testelor

`dotnet build src\KBot.App\KBot.App.vbproj`: **0 erori, 0 avertismente**. `Check-Help.ps1 -Coverage`:
«No errors.» (acoperirea arată `RobotQueueForm` — din 0098, deja în lista «Ajutor de actualizat»).
Nimic rulat.

### 4. Rămas NEVERIFICAT

1. Nerulat. De văzut în `adobe_preview.log` linia «Document gata în Adobe (…)» și cât durează de la
   «Pornesc Adobe»; dacă apare des varianta de 60 s, semnalul nu se prinde pe acel PC.
2. **250 ms de liniște** e o presupunere: dacă Adobe face o pauză mai lungă la mijlocul încărcării
   (formulare XFA mari, scripturi), arborele se poate debloca puțin prea devreme.
3. Arborele principal se blochează și el; o schimbare de angajament în timpul încărcării nu mai e
   posibilă — voit (tot ea detașa documentul pe jumătate încărcat).
4. Motorul ActiveX: se deblochează când se întoarce încărcarea, nu după aceeași liniște.
5. Procesele fantomă: poarta oprește cauza cerută (al doilea document în timpul încărcării); dacă
   mai apar, de trimis `adobe_preview.log` de pe PC-ul respectiv.

---

<a id="slice-0078-09"></a>

## SLICE 0078-09 — Întrebarea Adobe «salvați modificările?» la închiderea documentului

Data: 01.10.2026. Cererea operatorului: când vizualizatorul se închide (sau operatorul dă clic în altă parte),
documentul se închide, dar uneori rămâne nesalvat și apare «Do you want to save changes to '…PDF' before
closing?» (Yes / No / Cancel). Propunere: prinde fereastra și închide-o, sau trimite Ctrl+S înainte de închidere.
Apoi: «the trap works only when the user clicks the save button». Număr: **0078-09** (propus).

### 1. Ce s-a schimbat și de ce

**Cauza (din cod, nu din jurnal).** Capcana `AdobeSaveTrap` reacționează doar la dialogul «Salvare ca». Iar
`AdobeReaderHost.Detach`, `AdobeReaderHost.ReleaseAtScreenSize` și `AcroPdfSurface.Clear` o opreau ÎNAINTE de a
închide fereastra / controlul. Întrebarea de la închidere (nici «Salvare ca», nici alertă de script) apărea deci
fără nimeni care să o urmărească și rămânea pe ecran. La modul de închidere (A) cu proces pornit de K-BOT nu apare
(procesul e oprit); apare când fereastra e a unui Adobe pe care nu l-a pornit K-BOT (închisă doar cu `WM_CLOSE`) și la
motorul ActiveX.

**Alegerea: Ctrl+S nu, răspuns pe dialog da.** Ctrl+S cere focus și prim-plan (în jurnalele din 29.09 s-a pierdut
chiar și așa) și nu merge cât panoul e ascuns, adică exact la «clic în altă parte». Un mesaj către dialog nu cere nimic.
Răspunsul e **«Da»** (salvează); «Salvare ca» care poate urma trece prin capcană ca de obicei, deci fișierul se
întoarce pe propria cale.

- `AdobeSaveTrap.BeginClose` / `ClosePoll`: capcana rămâne pornită cât se închide documentul (una oprită e reluată);
  întrebarea e recunoscută (`AdobeSaveDialogFilter.IsSaveOnClosePrompt`: butoane Da + Nu, sau TaskDialog cu titlu
  «Adobe…»; dacă textul se citește, trebuie să conțină numele documentului) și primește `WM_COMMAND IDYES`
  (`TDM_CLICK_BUTTON` pentru TaskDialog), o singură dată pe fereastră, mutată în afara ecranului.
  Doar cât `_closing` e activ și doar pentru procesele urmărite, deci documentele proprii ale operatorului nu sunt atinse.
- O salvare făcută la închidere NU ridică `Saved` (nu e semnătură; următorul document poate fi deja cel urmărit de sesiune).
- `AdobeWindowTeardown.Run` și `AdobeScreenRelease.Run`: parametru opțional `whileClosing` apelat la fiecare pas al
  așteptării; cât spune că o salvare e în curs, răgazul pornește din nou (cel mult 15 s), ca să nu fie oprit procesul
  în mijlocul salvării.
- `AdobeReaderHost.Detach` / `ReleaseAtScreenSize`: `BeginClose` înainte, `Stop` în `Finally`.
- `AcroPdfSurface.Clear` și înlocuirea unui document în același control: `BeginClose` + `PumpClosePrompt`
  (minimum 300 ms, mai mult cât o salvare rulează).
- `AdobeNativeMethods.IDNO`; `AdobeDialogFacts.HasNoButton` / `Text`.
- Controls 1.59 ▸ 1.60.

### 2. Fișiere atinse

`src/KBot.Controls/Adobe/`: `AdobeSaveTrap.vb`, `AdobeSaveDialogFilter.vb`, `AdobeWindowTeardown.vb`,
`AdobeScreenRelease.vb`, `AdobeReaderHost.vb`, `AcroPdfSurface.vb`, `AdobeNativeMethods.vb`; `KBot.Controls.vbproj`.

### 3. Rezultate

`dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: **0 erori, 0 avertismente**. Nimic rulat, nicio suită
(cerere permanentă), nimic pe ecran, nimic comis.

### 4. Neverificat / amânat

- **Clasa ferestrei întrebării.** Presupusă `#32770` cu butoane cu id 6/7 (MessageBox standard; captura arată așa).
  Dacă e o fereastră proprie Adobe, capcana nu o vede: jurnalul `adobe_preview.log` nu va avea linia
  «Întrebarea Adobe la închidere acceptată cu «Da»», iar întrebarea va apărea ca înainte.
- **«Da» scrie fișierul.** Dacă Adobe salvează pe loc, fără «Salvare ca», nu apare nicio linie de capcană în afară de a
  noastră. Dacă operatorul preferă «Nu» (renunță la modificări), schimbarea e într-un singur loc: `AnswerClosePrompt`.
- **ActiveX:** nu se știe dacă distrugerea controlului AcroPDF întreabă; dacă `LoadFile` blochează pe întrebare, firul UI
  nu poate răspunde (capcana rulează pe el). Cele 300 ms de așteptare pe fiecare schimbare de document sunt o presupunere.
- Ajutor: nicio pagină de ajutor nu vorbea despre această întrebare; nimic de actualizat (dispare, nu apare ceva nou).

### 5. A doua trecere (01.10.2026, după prima rulare)

Jurnalul `temp_logs\jurnal_20261001_173437.log` (17:33:05): capcana a văzut întrebarea — `#32770`, proces urmărit (7220),
titlu «Adobe Acrobat», textul citit — dar **`butonDa=False butonNu=False`**: nu are butoane cu id 6 / 7. Primul filtru
cerea acele butoane, deci a lăsat-o în pace («alt dialog Adobe (lăsat în pace)»). Corectat:
- `IsSaveOnClosePrompt`: titlu «Adobe…» + text care numește documentul închis = destul (butoanele nu mai contează).
- `AnswerClosePrompt`: butonul «Da» se caută după text (`IsYesCaption`: Yes/Da/Ja/Oui/Si/Sim), nu după id; apăsat în
  trei feluri, câte unul pe încercare (WM_COMMAND/BN_CLICKED pe buton ▸ activare + BM_CLICK ▸ BM_CLICK postat),
  maximum 3 încercări; niciodată WM_CLOSE (ar fi «Cancel»). Dialogul NU se mai mută în afara ecranului: dacă nimic nu
  merge, operatorul îl vede și răspunde. La prima vedere, jurnalul scrie TOȚI copiii dialogului (clasă, id, text) — dacă
  nici acum nu merge, se vede din ce e făcut.
- Build: `KBot.Controls` 0 erori / 0 avertismente; `KBot.App` nu s-a putut copia (aplicația rula sub Visual Studio) —
  de reconstruit cu aplicația oprită. Tot nerulat.

### 6. A treia trecere (01.10.2026): «Nu», nu «Da»

Cu «Da» răspunsul a mers, dar Adobe a încercat să scrie fișierul, ținut încă deschis de vizualizatorul care se închidea, și a
arătat «The file may be read-only, or another user may have it open. Please save the document with a different name or in a
different folder.» (OK). Cererea operatorului: «better click NO instead of yes». `AnswerClosePrompt` apasă acum butonul
**«Nu»** (`IsNoCaption`: No/Nu/Nein/Non/Nao; rezerve `IDNO` / `TDM_CLICK_BUTTON IDNO`); nimic nu se mai salvează, deci nu mai
urmează nici «Salvare ca», nici mesajul de fișier ocupat. Așteptarea după răspuns scade de la 4 s la 1 s
(`CloseAnswerWaitMs`). Modificările făcute în document și nesalvate se pierd la închidere — semnăturile au propria salvare
(capcana «Salvare ca» + Ctrl+S după semnătură), deci nu depind de această întrebare. Build `KBot.Controls`: vezi mai jos.

---

<a id="slice-0078-11"></a>

## SLICE-0078-11 - Adobe processes K-BOT started are ended when K-BOT closes (operator request, 03.10.2026)

Operator: some Adobe processes stay in the Task Manager as ghosts after the application is closed. Proposed fix: keep the
PID of every Adobe process K-BOT generates and, on closing, send WM_CLOSE (unsure it works on ghosts) or kill by id.

### What changed and why

- New `src/KBot.Controls/Adobe/AdobeProcessRegistry.vb`:
  - `Track(pid)` remembers a process with its START TIME (a pid reused later by something else is not mistaken for it).
  - `TrackFamily(parents)` / `Sweep()`: an Adobe-named process (`Acrobat`, `AcroRd32`) is "ours" only if its parent is K-BOT
    or a process already ours, repeated until nothing new (broker -> renderer chain). An Acrobat the operator opened by hand
    has another parent and is NEVER touched.
  - `Shutdown()`: final sweep, then WM_CLOSE (posted, not sent) to every top-level window of our processes, wait up to
    1.5 s for them to leave, then `Process.Kill(entireProcessTree:=True)` for the survivors - the kill is what handles a
    ghost, which has no window to answer WM_CLOSE. Never throws (terminal, runs while the application goes away).
  - Everything is written to `adobe_preview.log` (`AdobeHostLog`): the processes found, WM_CLOSE count, who left, who was
    killed, and the Adobe processes LEFT ALONE with their parent pid - the evidence for the next ghost if it has another parent.
- Where processes get remembered: `ProcessAdobeLauncher.Start` (`IAdobeLauncher.vb`, every Adobe K-BOT starts itself) and
  `AcroPdfSurface.OwnerPids` (the ActiveX viewer's broker/renderer family, on every save-trap sweep).
- Where it runs: `Program.RunShellWithLogin` - `Application.Run(shell)` is now in a `Try/Finally` whose Finally calls
  `AdobeProcessRegistry.Shutdown()`, after the shell is closed and disposed (so the ActiveX control is gone first), before
  the logout.
- FileVersion: `KBot.Controls` 1.60 -> 1.61. (`KBot.App` already bumped in the same uncommitted work.)

### Files touched

`src/KBot.Controls/Adobe/AdobeProcessRegistry.vb` (new), `IAdobeLauncher.vb`, `AcroPdfSurface.vb`,
`src/KBot.Controls/KBot.Controls.vbproj`, `src/KBot.App/Program.vb`.

### Test results

`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, no ghost provoked (operator rule: no tests).

### Left unverified / deferred

- **Whether the ghosts descend from K-BOT is an assumption.** The existing `OwnerPids` code says the broker's parent is
  K-BOT (slice 0078 evidence); if a ghost was started another way (e.g. by COM through svchost) the registry will not see it.
  The Shutdown log then lists it under «left alone» with its parent - read that line after the next ghost.
- WM_CLOSE on a hung ghost does nothing by itself; the kill after the 1.5 s wait is the part that works there.
- Only a NORMAL close is covered: a crash or a kill of K-BOT from the Task Manager skips `Shutdown` (a list of pids kept
  on disk for the next start would cover it - not built, not asked).
- The DevHarness exit path (`RunHarness`) is not covered; it has its own Adobe clean-up.
- A document Adobe is still asking about («save changes?») is killed after the wait instead of answered; at this point
  the documents are already closed by the viewers.
- No help / NOUTATI change (operator order; nothing the operator sees).

---

<a id="slice-0078-12"></a>

## SLICE-0078-12 - Hosted window with an OLDER Adobe: script-error monitor + its own bench (operator request, 06.10.2026)

Operator: what the hosted-window engine does today is correct for Reader 2024 and newer. On the local Acrobat Pro 2020
(Acrobat DC 19.12, cracked install) it works less well, and the JS errors that used to show only on the ActiveX engine
show there too. So the JS-error monitoring must exist on the hosted-window engine as well, and a NEW harness form is
needed to test only this old-Adobe side.

### What the real log said (read before changing anything)

`src/KBot.App/bin/Debug/net8.0-windows/Logs/adobe_preview.log`, 05.10.2026 20:05-20:58, `Acrobat.exe` 19.12.20035.332343,
engine «Fereastră găzduită», DDF 185:

- The script-window handling of `AdobeSaveTrap` was ALREADY running on the hosted window (the trap is shared by both
  engines): 12 x «GeneralErrorOperation failed.» each closed by `PressOk` method 0 (WM_COMMAND) at 20:48:06-08; the
  «JavaScript Debugger» console («The JavaScript Debugger is not enabled…») hidden at 20:43, 20:53, 20:56; the form's own
  messages («Nu sunt erori la sectiunea A», «Validarea s-a terminat cu succes!…») left to the operator; `Ctrl+H, Ctrl+2:
  aștept — mesaje de script în curs` until the burst ended.
- What was missing is MONITORING, not detection: those facts were only text lines among others in `adobe_preview.log`.
  No count per document, nothing structured for a bench to show, no trace (the trap's trace was ActiveX-only), no line that
  says what the document gave.

### What changed and why

- **`AdobeSaveTrap.ScriptAlertSeen`** (new event, `Action(Of AdobeScriptAlert)`): raised once per window and outcome from
  `DismissScriptNoise`: error pressed (first press), message left to the operator, error stuck after every press, error
  hidden, console hidden, console left alone. Additive: no decision of the trap changed; a listener that throws is logged
  and ignored (`NotifyScript`).
- **`AdobeScriptAlert`** (new, POCO + `AdobeScriptAlertAction`): time, handle, title, text, action, matching pattern, presses;
  `IsError` / `IsConsole` / `KindLabel` / `ActionLabel` / `Describe()`.
- **`AdobeScriptMonitor`** (new): the tally for ONE document (`ErrorCount`, `StuckCount`, `MessageCount`, `ConsoleCount`,
  `Alerts`, `Summary()`, `Recorded` event, `Reset()`). A stuck alert always follows its first press, so it is part of
  `ErrorCount`, not an addition (said in the summary: «…; 1 nu s-a închis»).
- **`AdobeReaderHost`**: owns a monitor (`ScriptMonitor`), forwards `ScriptAlertSeen`, resets it at every document, and writes
  ONE line, `Mesaje de script Adobe la deschidere: …` (always, also «niciun mesaj de script») when the document is ready and
  `… până la eliberarea documentului: …` at release only when more arrived since. Also writes `Adobe: <exe> <version> —
  ANTERIOR lui 2024 / linia 2024 sau mai nouă — <path>` at every launch (`AdobeProductInfo`) and exposes it as `AdobeProduct`.
- **Trace for the hosted window**: `AdobeReaderHost` sets `_saveTrap.Traced = True`, opens an `AcroPdfTraceLog` recording per
  document (source `AdobeReaderHost`, trap lines as `SaveTrap#n`), mirrors every working-log line as `REPORT:`, dumps the
  hosted window tree right after hosting and when the document is ready, and closes the recording at ready / failure /
  release. **All of it is a no-op unless `AcroPdfTraceLog.SwitchedOn`** (in memory, off at every start), which in the
  application is reachable only through the Setări checkbox that is hidden while the hosted window is the engine - so the
  application's behaviour on Reader 2024+ is unchanged apart from the two extra working-log lines above.
- **`AdobeProductInfo`** (new): file version of the resolved Adobe exe; `IsBefore2024` = file-version major < 24 (the operator's
  line; a number read, not behaviour probed).
- **New bench `OldAdobeHostHarnessForm`** (+ Designer, + `OldAdobeHostHarnessTest`, DevHarness ▸ «Adobe/PDF» ▸ «Fereastră
  găzduită — Adobe vechi (< 2024): monitor erori JS»; Debug only): drives `AdobeReaderHost` directly - no signing session,
  no server, no print watch. Same preface as the application before a document (classic-interface option, standard Save As
  preference, operator's «instanță nouă» and detach settings). Always on a copy (`Temp\PDF\Banc\ADOBE_VECHI_<name>`).
  Shows: the Adobe found and which side of 2024; hosted-after / ready-after milliseconds; one row per script window
  (colour: stuck = error, closed error = warning, console = dim) and the running tally; the live working log; switches for
  Ctrl+H/Ctrl+2, the trap (Save As + script alerts are one object) and the detailed trace; the REAL «Mesaje de script
  Adobe…» list dialog; «Arborele ferestrelor Adobe» (hosted tree + every top-level window of the Adobe processes, to read a
  dialog that stays on screen); «Copiază raportul» (Adobe, switches, timings, tally, each window, last 250 log lines).
- Settings window NOT touched (see deferred).
- FileVersion: `KBot.Controls` 1.62 ▸ **1.63**, `KBot.App` 1.1.1.8 ▸ **1.1.1.9**.
- Help / NOUTATI: nothing to update - nothing the operator sees changes (the bench is Debug-only, the rest is log lines).

### Files touched

`src/KBot.Controls/Adobe/`: `AdobeScriptAlert.vb` (new), `AdobeScriptMonitor.vb` (new), `AdobeProductInfo.vb` (new),
`AdobeSaveTrap.vb`, `AdobeReaderHost.vb`, `AcroPdfTraceLog.vb` (doc comment), `AdobeReaderHost.md`;
`src/KBot.App/HarnessTests/`: `OldAdobeHostHarnessForm.vb`, `OldAdobeHostHarnessForm.Designer.vb`, `OldAdobeHostHarnessTest.vb`
(all new); `tests/KBot.Controls.Tests/`: `AdobeScriptMonitorTests.vb`, `AdobeProductInfoTests.vb` (new);
`src/KBot.Controls/KBot.Controls.vbproj`, `src/KBot.App/KBot.App.vbproj` (FileVersion).

### Test results

- `dotnet build src\KBot.Controls`, `dotnet build src\KBot.App`, `dotnet build tests\KBot.Controls.Tests`: 0 errors, 0 warnings.
- Tests written (the tally, the alert kinds, the 2024 line / version parsing) and compiled, **not run** (operator rule: no tests).
- **The bench was not run**: nothing seen on screen, no PDF opened in it, the Acrobat 19.12 on this machine not driven.

### Findings from the same log, NOT fixed (outside this request)

1. **The «Replace existing file?» box is not recognised on Acrobat 19.12 (hosted window).** At 20:57:44 the trap pressed Save in
   «Save As PDF» and the box «Save As» (`butonDa=True butonNu=True`, text «…The file already exists. Replace existing file?»)
   was classified `Other` / «lăsat în pace», never answered; the trap then timed out (`ANULAT … nu s-a închis după apăsarea…`)
   and the «Save As PDF» dialog reappeared 44+ times, each cancelled («a reapărut după 3 apăsări»). `Classify` accepts a
   confirm only when `facts.OwnerWindow = awaitingConfirmFor` (the Save As we pressed): **hypothesis** - on this Acrobat the box is
   owned by another window. The `Other` log line does not print the owner; with the trace on, the bench's
   `Handle … kind=Other … owner=0x…` line does. Earlier runs (20:43, 20:44) closed the same way «fără confirmare de
   suprascriere» and the document still saved (cause unknown: operator pressed Yes?).
2. Dialogs only the older Acrobat raises, all left alone by the trap: «Appearance Integrity Report», «Token Logon», «The
   document could not be signed.» (20:43:55 after a name that could not be pinned: `ANULAT: Numele din dialog … nu a putut fi fixat`).
3. First window found after 8.7 s on a cold Acrobat 19.12 (20:43:07) - the 30 s search budget absorbs it.

### Left unverified / deferred

- Nothing run. Whether the monitor rows and the tally match what Acrobat 19.12 really shows is unproven until the bench is
  used with a DDF; the text of `Mesaje de script Adobe la deschidere:` is the first thing to compare with the 20:48 run (expect
  «12 erori de script…»).
- **Settings**: the «Mesaje de script Adobe…» button and the trace checkbox in Setări ▸ Documente are still visible only
  while an ActiveX engine is chosen, although the same list drives the hosted window's trap. Making them visible for the
  hosted window changes a screen and needs the help updated (0000-NN); asked for nothing here - the bench has both.
- Two traps that watch the SAME Adobe process (Acrobat DC ignores «/n»: the DDF and the ORD page both hosting) share the script
  windows: the trap that meets a window first reports it, so an alert can be attributed to the other page's host.
- `AdobeProductInfo.IsBefore2024` is a version number, not a behavioural test (Acrobat 2020 reports 20.x, the local 19.12 is
  a DC 2019 build).
- Not committed (operator did not ask); the worktree also holds unrelated uncommitted work.

---

<a id="slice-0078-13"></a>

## SLICE-0078-13 - «Replace existing file?» on the older Acrobat; F8 in the hosted window (operator request, 06.10.2026)

Scope: hosted-window engine only (old AND new Adobe share one code path). NOTHING on the ActiveX engines.

### What changed and why

- **Replace-existing-file confirm.** `adobe_preview.log` 05.10 20:57:44 (Acrobat 19.12): the «Save As» box with Yes/No and the text
  «…The file already exists. Replace existing file?» was classified `Other` and left alone, because `AdobeSaveDialogFilter.Classify`
  accepted a confirm only when its owner was the Save As we pressed. Save As then timed out and reappeared 44+ times.
  Now, while a Save As of ours is pending, a Yes/No box of the watched process whose text is the replace question
  (`IsReplaceQuestion`: «already exists» / «Replace existing» / Romanian equivalents) is a `ConfirmOverwrite` too; the owner test
  stays. Any other Yes/No question is still left alone. The answer is the existing `HandleConfirm` (WM_COMMAND IDYES).
  The ownership hypothesis of 0078-12 was therefore not needed to fix it, and is still unproven.
- **F8** (no Ctrl) is queued with Read Mode in `AdobeReaderHost.ArmReadMode`: order Ctrl+H, F8, Ctrl+2 (F8 before the zoom-to-width,
  so the width is taken as the right pane goes). `HideRightPaneEnabled` (default True) switches it off; the older-Adobe bench
  ticks it with the Ctrl+H box. Same conditions, retries and log lines as the other keys («F8 trimis documentului», manual hint on failure).
  ASSUMPTION: F8 toggles the right-hand toolbar in this Acrobat (operator's observation); on a document where that pane is already hidden it would show it.
- Controls 1.63 -> **1.64** (App not touched). Help: the toolbar the operator sees changes -> **help topic not yet updated** (see below).

### Files touched
`AdobeSaveDialogFilter.vb`, `AdobeReaderHost.vb`, `OldAdobeHostHarnessForm(.Designer).vb`, `KBot.Controls.vbproj`,
`tests/KBot.Controls.Tests/AdobeSaveDialogFilterTests.vb` (one new test).

### Test results
Builds of `KBot.App` and `KBot.Controls.Tests`: 0 errors, 0 warnings. Test written, not run (operator rule). Nothing run on screen.

### Unverified / deferred
- Nothing seen: that the confirm is now answered on Acrobat 19.12 and on the new Reader, and that F8 hides the right pane in both.
- If F8 acts differently after Ctrl+H (Read Mode), the order or a per-version choice may be needed.
- Help (0000-NN) for the hidden right pane: not done.

---

<a id="slice-0078-14"></a>

## SLICE-0078-14 - Advice at start when the installed Adobe is older than 2025 (operator request, 06.10.2026)

### What changed and why

Field finding, 06.10.2026: on the operator's PC (Acrobat Pro 2020, cracked) the form's `calculate` scripts of the DDF
(`Table1/Row1/Cell7`, `Table1/FooterRow/Cell7`) raise «Property 'rawValue' cannot be set because doing so would violate this
document's permissions settings» after signing. Same template signed by hand in Acrobat, no K-BOT: same error, not always.
The same signed file opened in Reader 2025 on a VM: clean, signature valid. The same file with the free Reader 26.2 on the SAME
PC: no error. So the cause is the Adobe on the PC, not the template and not K-BOT (K-BOT does not write `Cell7` in
Section A at all: `DdfXmlBuilder` writes `Cell1`…`Cell6`).

K-BOT now tells the operator at start when the installed Adobe is older than 2025.

- `AdobeAdvice.ShowIfDue` (KBot.App): resolves the Adobe path (`AdobeWindowHosting.ResolveAdobePath`), reads it
  (`AdobeProductInfo.Read`) and, unless `IsRecommended`, shows a `KBotMessage` Yes/No box (default No): what was found, the
  recommendation (free Adobe Acrobat Reader 2025 or newer) and «Vrei să nu mai primești acest avertisment?». **Yes** writes
  `AppSettings.ShowAdobeAdvice = False`. Nothing installed: no advice. Called in `MainForm_Load`, before the initial tour.
- `AdobeProductInfo.IsRecommended` = version known AND major >= 25 (`RecommendedReaderMajor`). **Judged by version only**: a first
  version of this change also required the file to be `AcroRd32.exe`; wrong, the free Reader since 2024 is `Acrobat.exe`
  with description «Adobe Acrobat» and Windows name «Adobe Acrobat (64-bit)» - identical to Pro (checked on the operator's PC).
  Free and paid cannot be told apart from the executable.
- `AppSettings.ShowAdobeAdvice` (default True; DTO + clone mapping).
- «Setări ▸ Aplicație ▸ Generale» (group «Fereastra principală»): new switch «Avertizează dacă Adobe e mai vechi de 2025»
  (`chkAvertismentAdobe`) turns it back on; row added to the table (rows below shifted by one).
- Help: `contabil/setari.md`, section «Pagina «Aplicație»: pornirea K-BOT» + group table, tag `0078-14`; the Setări capture is marked
  to redo.

### Files touched
`src/KBot.App/AdobeAdvice.vb` (new), `KbotForm.vb`, `Setari/SetariAplicatieView.vb`, `Setari/SetariAplicatieView.Designer.vb`,
`src/KBot.Common/AppSettings.vb`, `src/KBot.Controls/Adobe/AdobeProductInfo.vb`, `src/KBot.App/HelpContent/contabil/setari.md`.

### Test results
No tests written or run (operator rule). `KBot.App` build: only file-lock copy errors while K-BOT was running (no compile error
in the last run); the first build of this change was clean (0 errors, 0 warnings) before the version-only correction.
`Check-Help.ps1 -Coverage`: 15 errors, all in `tutorials/ordonantare-din-plata.md` (sections without slice tag), none from this change.

### Unverified / deferred
- **Build not re-confirmed clean after the version-only correction** (K-BOT was running and locked the DLLs): close K-BOT and build again.
- Assumption: the free Reader 2025 has file-version major 25 (2024 = 24, 2026 = 26.2 seen on the operator's PC). Not seen for 25.
- The advice window was seen once on screen (operator's screenshot, first version of the text); the new text not seen.
- Help recorded here with the `0078-14` tag, not as a `0000-NN` sub-slice; the watermark in `help-version.txt` was NOT moved.
- Controls version (FileVersion) not bumped; App/Common not bumped.
- Release notes (`NOUTATI.md`) not written: done at publish time.

### Capturi de refăcut
`setari` (new switch on the «Aplicație» page).
