# SLICE-0000-02 — Help capture tool («Capturi pentru ajutor»)

Operator request, 30.09.2026: the operator takes the help screenshots through K-BOT itself (also
on a client's PC, for the FOREXE flow); Claude only writes text with capture tags.

## What changed and why

- **Capture tag** (one line in a topic): `<!-- capture: <id> | caption: .. | goto: .. | prepare: .. -->`.
  The tag IS the picture: rendered as `img/<id>.png`, or «Imagine lipsă: <caption>» until taken.
  Parsed by `HelpCapture`; `HelpLibrary.Captures` lists all tags of all parts; bad tags / duplicate
  ids are logged and skipped. Format documented in `HelpContent/README.md`.
- **Setting** `AppSettings.HelpCaptureMode` + checkbox «Mod capturi pentru ajutor» in
  Setări › Aplicație, visible only with advanced options; honoured only while those are on.
  Not described in the help.
- **Entry points**: «Meniu › Capturi pentru ajutor» (hidden item, shown on menu opening when the
  mode is on) and «Capturi...» in the help window's bar.
- **HelpCaptureForm**: KBotDataView of every capture (part, page, caption, prepare, state) with a
  «Fă poza» / «Refă» button per row, filter «Doar cele care lipsesc», «Reîncarcă», «Deschide dosarul».
- **Flow**: window hides → `goto` (`view:` / `menu:` / `setari:` via `IHelpCaptureNavigator`
  implemented by KbotForm; `help` / `help:<id>`) → floating `HelpCapturePromptForm` with the
  prepare text and any note (view off for the selected angajament, unknown target...) →
  «Capturează» → `HelpCaptureOverlay` freezes the monitor under the cursor: drag = rectangle,
  click = window under cursor, Ctrl+click = control under cursor, Esc/right click = cancel.
- **Saving** (`HelpCaptureStore`): always `<AppDir>\Help\img\<id>.png`; also
  `src\KBot.App\HelpContent\img\` when running from a repository build. On a client PC the files
  stay in `C:\KBOT\Help\img\` (updater never deletes unshipped files) and are copied back by hand.
- Sample tags on three start pages (conectare-fereastra, fereastra-principala, setari-aplicatie).

## Files touched

- New: `src/KBot.App/Help/HelpCapture.vb`, `HelpCaptureStore.vb`, `HelpCaptureNative.vb`,
  `HelpCaptureOverlay.vb`, `HelpCapturePromptForm(.Designer).vb`, `HelpCaptureForm(.Designer).vb`,
  `IHelpCaptureNavigator.vb`, `src/KBot.App/KbotForm.HelpCapture.vb`, this worklog.
- Edited: `HelpLibrary.vb`, `HelpHtml.vb`, `HelpService.vb`, `HelpForm(.Designer).vb`,
  `KbotForm.Designer.vb` (2 menu items), `KbotForm.Nomenclatoare.vb` (menu case),
  `Setari/SetariAplicatieView(.Designer).vb`, `KBot.Common/AppSettings.vb`, `KBot.App.vbproj`,
  `HelpContent/README.md`, `HelpContent/{contabil,avansat}/*.md`.
- Renumbering 0097 → 0000 (operator): code comments, worklog 0000-01, status files.

## Test results

- Build KBot.App: 0 warnings, 0 errors. The operator confirmed the checkbox is there.
- The capture flow itself (list, prompt, overlay, save) was NOT run by Claude — the operator runs it.

## Left unverified or deferred

- Whole capture flow on screen; multi-monitor with different DPI (overlay covers only the monitor
  under the cursor); `ReloadLibrary` redraws the open help page but not its contents tree.
- «Capturi...» keeps its 110px column in the help bar when hidden (empty gap).
- The `Color.Black` dimming mask in the overlay is deliberate (it shades a photo, not UI).
- FileVersions not bumped.

## Addendum (same day) — «Încarcă»

Operator: some pictures cannot be taken from inside K-BOT. Each row now has a second button,
«Încarcă»: pick a PNG / JPG / BMP from disk; it is copied into a fresh bitmap (source file not
left locked) and saved as `<id>.png` to the same folders as a capture. Build: 0 warnings, 0 errors.
Files: `HelpCaptureForm.vb`, `HelpCaptureForm.Designer.vb` (column `incarca`).
