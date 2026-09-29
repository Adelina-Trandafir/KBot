# SLICE 0078-05 — Fereastra găzduită fără poziționare / ascundere; Ctrl+H; proba ordinii salvării

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
