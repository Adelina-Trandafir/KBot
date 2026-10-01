# SLICE-0000-29 — help for the header menu rows «Jurnal activitate» / «Configurare K-BOT» (sliceless code change)

Operator request, 01.10.2026. The code change has no slice number (recorded in
`state/KBOT_STATUS_SLICELESS.md`); this file is its help pass.

## What changed and why

Code (no slice):
- The caption bar of the main window no longer has the options («Setări») button
  (`KbotForm.Designer.vb`: `ShowOptionsButton`, option image and tint removed).
- `KbotForm.Chrome.vb`: the popup menu of that button (`CapBar_OptionButtonClick`,
  `MeniuOptiuni_ItemClicked`, the `OPT_*` keys) is gone. `ShowLog()` and the new `ShowSettings()`
  stay, each with its own Try/Catch and operator message.
- `KbotForm.Nomenclatoare.vb` (`MenuNou_ItemClicked`): the keys `jurnal` -> `ShowLog()` (Setari
  window on the «Jurnal» page) and `setari` -> `ShowSettings()`. The rows already existed in the
  designer (`KBotMenuItem12/13`), they were not wired.
- `KbotForm.HelpCapture.vb` (`MenuNou_Opening`): the «Jurnal activitate» row follows
  `FeatureSwitches.VizualizatorJurnaleActiv` at every opening (the old rule: the log row only when
  the operator's switch is on). Before, with the switch off the button opened Setari directly; now
  the row is hidden.
- Operator-visible text that named the old row: the checkbox in Setari > Aplicatie now reads
  «Rândul «Jurnal activitate» în meniul MENIU al ferestrei principale» (+ tooltip + the confirmation
  line). Stale comments/XML docs updated.

Help (tag `0000-29`, `fara-felie`):
- `tours/tur-fereastra.md`: step «Bara de titlu › Setări» removed; step «Butonul MENIU» names the two
  new rows.
- `contabil/fereastra.md` (section MENIU): two new rows.
- `contabil/setari.md` (intro) and `avansat/index.md` (step 1): «MENIU › Configurare K-BOT» instead of
  «the cog in the title bar».
- `HelpContent/README.md`: note that the menu keys `jurnal`/`setari` exist but captures use `setari:<page>`.

## Files touched

- `src/KBot.App/KbotForm.Designer.vb`, `KbotForm.Chrome.vb`, `KbotForm.Nomenclatoare.vb`,
  `KbotForm.HelpCapture.vb`, `KbotForm.Console.vb`, `KbotForm.TreeOptions.vb` (comments)
- `src/KBot.App/Setari/SetariAplicatieView.Designer.vb`, `SetariAplicatieView.vb`, `SetariForm.vb` (comment),
  `src/KBot.App/Views/LogViewerForm.vb` (comment), `src/KBot.Common/AppSettings.vb`, `FeatureSwitches.vb` (comments)
- Help: files listed above; `docs/worklog/state/KBOT_STATUS_*.md`, `KBOT_STATUS.md`.

## Test results

- `dotnet build src/KBot.App/KBot.App.vbproj`: see the note at the end (0 warnings, 0 errors).
- `tools/HelpCheck/Check-Help.ps1 -Coverage`: see the note at the end.
- No tests run, nothing seen on screen (operator rules).

## Unverified / deferred

- Not seen on screen: the caption bar without the button, the menu rows opening Setari / the Jurnal page.
- Capturi de refăcut: any capture showing the main caption bar with the cog (e.g. `fereastra-zone`) — the
  cog is gone; re-shoot with «Refă» in the capture list.
- `FeatureSwitches.VizualizatorJurnaleActiv` off: the «Jurnal» PAGE of Setari is still reachable from the
  settings window; only the menu row is hidden (same as before for the old menu row).
- `SincronizeazaAsync` stays unreachable from the shell (unchanged).
