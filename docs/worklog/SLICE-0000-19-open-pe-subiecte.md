# SLICE-0000-19 — `open:` actions on topics

Plan: `docs/PLAN_help_assistant.md` § 0000-19. Engine + content, no UI (the buttons are drawn by
the «?» popup and the help window's search list in 0000-20).

## What changed and why

A topic could not name its screen, so a search result could only open the help page. Now:

1. **New header key `open:`** with the same values as a capture's `goto:` (`view:ddf`,
   `menu:clasificatii`, `setari:tema`). Following `HELP_SYSTEM.md` §6 «New header key»:
   `HelpLibrary.Parse` reads it and rejects a value that does not fit
   `HelpLibrary.GotoPattern` (the same pattern as `$GotoPrefix`); `HelpTopic.Open`;
   `HelpContent/README.md` documents it (plus a short «keywords» section: what to put there now
   that the search ignores endings); `Check-Help.ps1` accepts `open` in `$HeaderKeys` and
   validates the value against `$GotoPrefix`.
2. **Hit actions.** `HelpLibrary.Search` fills, on every `HelpHit`, `OpenTarget` (the topic's
   `open:`) and `TourId` / `TourTitle` (the topic's first tour in a VISIBLE part,
   `HelpLibrary.TourOf`). `HelpService.OpenButtonText(target)` gives the button caption from
   what the main window really shows — «Deschide «Rezervări»», «Deschide «Clasificații
   bugetare»», «Deschide «Setări»» — through the new `IHelpCaptureNavigator.TargetCaption`
   (`KbotForm`: the view button's text, the menu row's text without `<b>` marks and «(!)»,
   «Setări» for a settings page); «Deschide ecranul» when the main window is not open.
   `HelpService.OpenScreen(target, topicId, owner)` goes there through the existing
   `Navigate`; the sentence `NavigateForCapture` returns for a view that is off for the selected
   angajament (or no main window) is shown with `KBotMessage.Show`.
3. **Content**: `open:` on the 25 topics that describe one real screen (values taken from each
   topic's own capture `goto:`):
   - views: `contabil.forexe.browser` (browser), `contabil.vederi.sumar / istoric / rezervari /
     receptii / plati / extrase / ord`, `contabil.ord.generare` and `contabil.ord.editor` (ord),
     `contabil.ddf`, `contabil.ddf.editor / semnare / trimitere` (ddf), `contabil.ddf.rezervare`
     (rezervari), `contabil.notecab` (notecab);
   - menu: `contabil.ddf.nou` (angajament_nou), `contabil.nomenclatoare.clasificatii`,
     `contabil.nomenclatoare.parteneri`;
   - Setări: `contabil.setari` and `avansat.documente` (aplicatie), `avansat.pagina`,
     `avansat.tema`, `avansat.foldere`, `avansat.jurnale` (jurnal).
   Left without `open:` on purpose: chapter and concept pages (`contabil`, `contabil.forexe`,
   `contabil.vederi`, `contabil.asocieri*`, `contabil.liste`, `contabil.ddf.mf`...), the main
   window itself, and the director's part (his window has no `goto:` navigation).
   Obvious synonyms added to `keywords:` (0000-18 item 5): DDF «document de fundamentare,
   angajament bugetar»; semnarea «semnatura electronica, certificat digital, token»; Plăți «op,
   banca»; Recepții «factura, facturi»; Parteneri «beneficiari»; Clasificații «clasificatie
   bugetara, indicatori».
4. **Director**: hits only come from `VisibleParts()` (a director sees part 3 only) and
   `TourOf` takes the same parts, so nothing here bypasses it.

## Files touched

- `src/KBot.App/Help/HelpTopic.vb` (`Open`)
- `src/KBot.App/Help/HelpLibrary.vb` (`open` key, `GotoPattern`, hit actions, `TourOf`)
- `src/KBot.App/Help/HelpSearch.vb` (`HelpHit.OpenTarget / TourId / TourTitle`)
- `src/KBot.App/Help/HelpService.vb` (`OpenButtonText`, `OpenScreen`)
- `src/KBot.App/Help/IHelpCaptureNavigator.vb` (`TargetCaption`)
- `src/KBot.App/KbotForm.HelpCapture.vb` (`TargetCaption`)
- `tools/HelpCheck/Check-Help.ps1` (`open` key + value check)
- `src/KBot.App/HelpContent/README.md`
- 25 topic headers under `src/KBot.App/HelpContent/contabil/` and `avansat/` (listed above)

## Test results

- `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors**.
- `tools\HelpCheck\Check-Help.ps1 -Coverage -Map`: **No errors.** (coverage: `RobotQueueForm`,
  0098, unchanged).
- No tests, no app run.

## Left unverified or deferred

- The buttons themselves: 0000-20. Nothing here is visible to the operator yet.
- `setari:jurnal` / `setari:pagina` / `setari:tema` / `setari:foldere` open pages that exist
  only with the advanced options on; their topics are in part 2, which is shown only then.
