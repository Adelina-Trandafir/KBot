# SLICE-0078-14 - Advice at start when the installed Adobe is older than 2025 (operator request, 06.10.2026)

## What changed and why

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

## Files touched
`src/KBot.App/AdobeAdvice.vb` (new), `KbotForm.vb`, `Setari/SetariAplicatieView.vb`, `Setari/SetariAplicatieView.Designer.vb`,
`src/KBot.Common/AppSettings.vb`, `src/KBot.Controls/Adobe/AdobeProductInfo.vb`, `src/KBot.App/HelpContent/contabil/setari.md`.

## Test results
No tests written or run (operator rule). `KBot.App` build: only file-lock copy errors while K-BOT was running (no compile error
in the last run); the first build of this change was clean (0 errors, 0 warnings) before the version-only correction.
`Check-Help.ps1 -Coverage`: 15 errors, all in `tutorials/ordonantare-din-plata.md` (sections without slice tag), none from this change.

## Unverified / deferred
- **Build not re-confirmed clean after the version-only correction** (K-BOT was running and locked the DLLs): close K-BOT and build again.
- Assumption: the free Reader 2025 has file-version major 25 (2024 = 24, 2026 = 26.2 seen on the operator's PC). Not seen for 25.
- The advice window was seen once on screen (operator's screenshot, first version of the text); the new text not seen.
- Help recorded here with the `0078-14` tag, not as a `0000-NN` sub-slice; the watermark in `help-version.txt` was NOT moved.
- Controls version (FileVersion) not bumped; App/Common not bumped.
- Release notes (`NOUTATI.md`) not written: done at publish time.

## Capturi de refăcut
`setari` (new switch on the «Aplicație» page).
