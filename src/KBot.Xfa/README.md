# KBot.Xfa

Librărie **în proces** pentru completarea + semnarea formularelor XFA (DDF/ORD), portată din
exe-ul `XFA_WRITTER` (Access îl lansa ca `.exe` separat prin `WScript.Shell.Run` și citea
codul de ieșire). Aici K-BOT o apelează direct — **fără proces extern**.

## API public — `XfaWriter`

- `XfaWriter.Genereaza(xmlPath, outputPdfPath, docType, [deschidePdf])` — descarcă macheta,
  completează XFA din XML, embedează atașamentele base64. Fără Adobe. `deschidePdf:=True`
  deschide PDF-ul cu aplicația implicită (`Process.Start`).
- `XfaWriter.GenereazaSiSemneaza(xmlPath, outputPdfPath, docType)` — ca mai sus, apoi deschide
  Adobe și **așteaptă** (fără timeout) semnarea + închiderea; întoarce masca semnatarilor.
- `XfaWriter.Semneaza(outputPdfPath, docType)` — mod „doar semnare" pe un PDF deja generat.
- `XfaWriter.ExtrageAtasamente(xmlPath, ByRef cleanXmlPath)` — separă nodul `<Attachments>`.

`docType` acceptat: **DDF**, **ORD**. Rezultatul e un `XfaResult` (înlocuiește codurile de
ieșire ale exe-ului): `Reusit`, `Semnat`, `Masca`, `SemnatAB/CD/Ordonator`, `CaleOutputPdf`,
plus `CodIesireLegacy` (0 / 2 / 11..17) pentru compatibilitate.

> ⚠️ Modurile cu semnare **blochează** (deschid Adobe, `WaitForExit`). Din UI, rulează-le pe
> un thread de fundal (`Await Task.Run(...)`) ca să nu îngheți fereastra.

## Componente

- `AdobeUtils` — motorul iTextSharp 5 (`PdfReader`/`PdfStamper`/`XfaForm`): modifică XFA din
  XML, embedează atașamente, deschide Adobe, calculează masca semnatarilor din registry
  (handler `.pdf` → validare că e Adobe).
- `TemplateDownloader` — cache DDF/ORD de pe API-ul **legacy** (`Configs`, `X-API-KEY`).
- `AttachmentModel`, `XfaResult`, `Configs`, `XfaLog` (trasare în `C:\AVACONT\logs`).

Erorile de la granițe se loghează în `GlobalErrorLog` (`KBot.Common`) și se rearuncă.
Trasarea operațională detaliată rămâne în `XfaLog`.

## Rulare din K-BOT

Verificare offline a logicii pure (separare atașamente + mască semnatari): DevHarness →
categoria **XFA** (`XfaWriterHarnessTest`, doar Debug). Generarea completă a PDF-ului cere
macheta de pe serverul legacy; semnarea cere Adobe ca handler implicit `.pdf`.

Machete iTextSharp: pachet **.NET Framework** folosit pe net8.0-windows (NU1701 e așteptat,
rulează corect — aceeași versiune ca exe-ul original).
