# SLICE-0019 — XFA_WRITTER → `KBot.Xfa` (librărie în proces, nu exe separat)

Portarea exe-ului `XFA_WRITTER` (din `Surse/SURSA_XFA_WRITTER`) în `src/KBot.Xfa` ca **librărie
apelată în proces**. Access lansa exe-ul prin `WScript.Shell.Run(cmd, 1, True)` și citea codul de
ieșire; K-BOT îl apelează acum direct, în proces — cerința operatorului: „exact ce face proiectul
din surse, dar din K-BOT, NU un exe separat ca în Access”.

`KBot.Xfa` era doar un ciot (vbproj + README) deja referit de `KBot.App`. Acum e o librărie reală.

## API public — `XfaWriter` (înlocuiește `Program.Main` + parsarea CLI)

- `Genereaza(xmlPath, outputPdfPath, docType, [deschidePdf])` — descarcă macheta (după tip),
  completează XFA din XML, embedează atașamentele base64. Fără Adobe. `deschidePdf:=True` deschide
  PDF-ul cu aplicația implicită (`Process.Start`, `UseShellExecute=True`) — echivalentul din README.
- `GenereazaSiSemneaza(xmlPath, outputPdfPath, docType)` — ca mai sus, apoi deschide Adobe și
  **așteaptă** (fără timeout, `WaitForExit`) semnarea + închiderea; întoarce masca semnatarilor.
- `Semneaza(outputPdfPath, docType)` — mod „doar semnare” pe un PDF deja generat.
- `ExtrageAtasamente(xmlPath, ByRef cleanXmlPath)` — separă nodul `<Attachments>` (public fiindcă
  e util și la verificare/testare izolată).

`docType` ∈ {DDF, ORD}. Rezultatul e un **`XfaResult`** (înlocuiește codurile de ieșire ale exe-ului):
`Reusit`, `Semnat`, `Masca`, `SemnatAB/CD/Ordonator`, `CaleOutputPdf`, plus `CodIesireLegacy`
(0 = generat fără semnare, 2 = nesemnat, 11..17 = semnat = 10 + mască) pentru compatibilitate/telemetrie.

> ⚠️ Modurile cu semnare **blochează** (deschid Adobe, `WaitForExit`). Din UI trebuie rulate pe un
> thread de fundal (`Await Task.Run(...)`) ca să nu înghețe fereastra.

## Componente portate

- **`AdobeUtils`** — motorul iTextSharp 5 (`PdfReader`/`PdfStamper`/`XfaForm`/`AcroFields`): modifică
  XFA din XML (recursiv; tabele = șterge `Row1`-urile din DOM și le reconstruiește din XML;
  subformulare repetabile `SubformInf` clonate când XML are mai multe instanțe decât macheta),
  embedează atașamente, deschide Adobe și calculează masca semnatarilor din handler-ul `.pdf` din
  registry (UserChoice → HKCR), cu validare că exe-ul e Adobe (nume `Acrobat.exe`/`AcroRd32.exe` sau
  `CompanyName` conține „Adobe”). Expus în plus `MascaSemnatari(fieldNames, docType)` — bucla de
  clasificare, utilă și fără PDF.
- **`TemplateDownloader`** — cache DDF/ORD de pe API-ul **legacy** (`Configs`:
  `adcredit.avatarsoft.ro:5008` + `X-API-KEY`, `c:\avacont\cache`). HEAD întâi (skip re-download dacă
  e în cache), apoi GET complet. E API-ul vechi FOREXE/VBA, separat de API-ul K-BOT bearer.
- **`AttachmentModel`**, **`XfaResult`**, **`Configs`**, **`XfaLog`** (trasare în `C:\AVACONT\logs`).

## Adaptări la regulile casei

- **Fără bloc `Namespace`** — `Namespace AdobeUtilsNS` scos; tipurile stau în namespace-ul implicit
  `KBot.Xfa`.
- **`Logger` → modul `XfaLog`** — fără `AllocConsole`/`Console.WriteLine` (rula ca exe CLI acolo);
  trasarea operațională detaliată rămâne, în `C:\AVACONT\logs`, un fișier per rulare. Sink terminal:
  nu aruncă niciodată.
- **`GlobalErrorLog` la granițe** — metodele de graniță (`XfaWriter.*`, `AdobeUtils` publice,
  `TemplateDownloader.GetTemplatePath`) loghează în `GlobalErrorLog` (`KBot.Common`) și rearuncă;
  internele deja logau în `XfaLog`. `KBot.Xfa` referă acum `KBot.Common` — **fără ciclu** (Common
  referă doar Domain).
- Comentarii/șiruri românești.

## Două capcane rezolvate

1. **`Option Strict On` rupe `PdfStamper(reader, os, "\0", True)`** — literalul `"\0"` (String) nu se
   poate da parametrului `Char`. Pe Option Strict Off (exe-ul) se convertea la primul caracter =
   backslash; intenția era însă NUL. Folosit `ChrW(0)` — sentinela iTextSharp „păstrează versiunea
   machetei” (`PdfStamperImp`: `pdfVersion == 0` ⇒ ține versiunea reader-ului). Corectează un bug
   latent, nu doar compilează.
2. **Shadowing VB case-insensitive** — Sub `DeschidePdf` vs parametrul `deschidePdf` colidau
   (VB e case-insensitive), deci `DeschidePdf(path)` se lega de Boolean → `BC30454 Expression is not
   a method`. Sub redenumit `DeschideCuAplicatiaImplicita`. Aceeași capcană ca la 0010
   (`ProposedValue`/`HeaderText`) — vezi memoria `vbnet-case-insensitive-shadowing`.

## Cum se rulează din K-BOT

- **DevHarness → categoria „XFA”** (`XfaWriterHarnessTest` în `src/KBot.App/HarnessTests`, doar
  `#If DEBUG`, descoperit prin scanarea assembly-ului de intrare — DevHarness NU referă `KBot.Xfa`).
  Rulează DOAR logica pură offline: separarea atașamentelor base64 + clasificarea măștii ORD/DDF.
- **`tests/KBot.Xfa.Tests`** (xUnit, `net8.0-windows`) — **39 teste**, adăugate acestei felii:
  `ExtrageAtasamenteTests` (fără/cu Attachments, base64 invalid ignorat, nume/date lipsă),
  `MascaSemnatariTests` (subform + fallback numeric ORD/DDF, AB înaintea lui A, combinare pe biți,
  `Nothing`/gol, idempotență), `XfaResultTests` (mapările de cod 0/2/11..17, clamp pe mască negativă,
  boolurile de conveniență).

## Fișiere atinse

- `src/KBot.Xfa/` — **nou:** `AdobeUtils.vb`, `XfaWriter.vb`, `XfaResult.vb`, `TemplateDownloader.vb`,
  `AttachmentModel.vb`, `Config.vb`, `XfaLog.vb`; `README.md` rescris.
- `src/KBot.Xfa/KBot.Xfa.vbproj` — pachet `iTextSharp 5.5.13.3`, ProjectReference `KBot.Common`,
  `FileVersion` 1.0.0.0 → **1.1.0.0**.
- `src/KBot.App/HarnessTests/XfaWriterHarnessTest.vb` — **nou** (categoria „XFA”, Debug-only).
- `tests/KBot.Xfa.Tests/` — **nou** proiect xUnit (39 teste); adăugat în `KBot.sln`.
- `docs/worklog/KBOT_STATUS.md` — rândul slice 0019, „Next free slice number” → 0020.

## Rezultate teste

- `dotnet build KBot.sln` — **0 errors** (doar `NU1701` pe iTextSharp/BouncyCastle — pachete .NET
  Framework pe net8.0-windows; rulează corect, aceeași versiune ca exe-ul original).
- `dotnet test tests/KBot.Xfa.Tests` — **39 passed / 0 failed**.
- `dotnet test KBot.sln` — suita completă rămâne verde (fără regresie).

## Rămas neverificat / amânat

- **Niciun PDF real produs sau inspectat.** Generarea completă cere macheta de pe serverul legacy
  (`adcredit.avatarsoft.ro:5008`, cheie X-API-KEY); nerulată live.
- **Semnarea cere Adobe ca handler implicit `.pdf`** — `WaitForSignature` refuză explicit dacă
  handler-ul nu e Adobe. Neverificată pe o mașină reală.
- **Verificat doar logica pură** (separare atașamente + mască) prin xUnit + harness. Motorul XFA
  propriu-zis (`ModifyXfaFromXml`, clonarea `SubformInf`, embed atașamente) NU e acoperit de teste
  automate — cere o machetă PDF reală.
- **Vederile editor DDF/ORD rămân amânate** (sunt `PlaceholderView`). Ele vor apela `XfaWriter` când
  se implementează — abia atunci apare un consumator de producție (azi consumatorul e doar harness-ul).
- `Configs` e hardcodat (URL legacy + cheie + `c:\avacont\cache`), fidel exe-ului. Dacă se dorește
  configurabil prin setările K-BOT, e o felie separată.
