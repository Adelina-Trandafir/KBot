# SLICE 0000-07 — coverage pass + finishing

**Date:** 30.09.2026. **Standing slice:** 0000 (help). The plan (01–06) was complete; the operator said
«go ahead with 0000-07» without naming it, so 07 = make F1 reach a topic from every operator window,
write what was missing, add the Part 2 tour.

## Coverage check

Every `Form` / `UserControl` in KBot.App (outside the harness and the help itself) was compared with
the `screens:` keys of the topics. 24 had no key. Sorted:

| Kind | Screens | Done |
|------|---------|------|
| Pages inside a keyed window (F1 walks up to the parent) | CabNoteReceiptPage, DdfFileBrowser, DdfFisierPreview, DdfValoriPage, ReaderHostPreview, XfaXmlPreview, OrdVizualizarePage, OrdDocumentPage, RegulaPaginaEditor | nothing needed |
| Debug / leftover | StartupLauncherForm (Debug launcher), PlaceholderView | left out |
| Separate windows without a topic | AlegereUnitateForm, GraficeAsociereForm, AsociereBenziForm, IstoricIntervalForm, LogClearDialog, UpdateProgressForm, InternalInfoForm | written, see below |
| ORD editor | OrdEditForm, OrdZiuaForm, OrdTextForm, OrdAtasamentePage, OrdBeneficiariPage, OrdDocumentePage | **left out: waiting on the operator** (ORD is excluded until new ORD exists) |

## What was written

- `contabil/forexe/descarcare.md`: «Grafice și benzi» (window + enlarged bands) and «Alegerea unității»
  (the save stops; «Alege unitatea», «Nu mă mai întreba...», «Renunță»). +2 captures.
- `contabil/forexe/browser.md`: «Ce face K-BOT după o salvare făcută în pagină» (slice 0073 watcher:
  new angajament / reception / reservation with the «Ați terminat...?» question; the «Istoric angajament»
  window, «Tot istoricul»; one takeover at a time). Only what K-BOT does after the save, not the clicks
  in the page. The old line «ce modifici aici de mână ocolește K-BOT» was wrong since 0073 and was
  replaced. +1 capture.
- `contabil/index.md`: the «regula de aur» now names the browser view as a road through K-BOT.
- `contabil/fereastra.md`: the ⓘ button → «Informații interne».
- `contabil/setari.md`: «Golirea jurnalelor» (two steps, cannot be undone, file in use, server logs
  untouched).
- NEW `contabil/actualizare.md`: the startup check (none / offered / mandatory / server down),
  «Caută actualizări» in Setări › Informații, the download window, updater keeps Logs and settings
  (settings live in %APPDATA%). +1 capture.
- NEW `tours/tur-avansat.md`: 7 steps across Setări (pages, «Activează opțiuni avansate», Aplicație
  tabs, FOREXE console, Pagina FOREXE, Temă, Căi fișiere).

## Checks
Static check: ids unique, every link / parent / tour topic resolves, capture ids unique, every screen
and tour target names a real type and control. Build App **0 warnings, 0 errors**. Nothing on screen.

## Not done
- «Înainte» caption a few pixels high (0000-01 note): both buttons are set up the same; the offset
  is the tall «Î» glyph. Not nudged blind.
- «Exportă manualul...» still not clicked on screen.
