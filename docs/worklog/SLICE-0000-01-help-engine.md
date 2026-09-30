# SLICE-0000-01 — Help engine (F1, «?», help window, manual export)

Operator request, 30.09.2026: an interactive help system and a manual in three parts —
Part 1 Contabil (everyday workflows), Part 2 Opțiuni avansate, Part 3 Director. This first pass
is the engine only; content, screenshots and guided tours follow in 0000-02 onwards.

## What changed and why

- **One text source.** Help topics are Markdown files with a header block, under
  `src/KBot.App/HelpContent/`, shipped as `<AppDir>\Help\`. The help window and the exported
  manual both render them, so they cannot drift apart. Authoring rules in `HelpContent/README.md`.
- **F1 everywhere.** `HelpKeyFilter` (an application message filter) catches F1 in any K-BOT
  window. It is a filter, not per-form `HelpRequested`, because some custom controls eat keys.
- **«?» on every caption bar.** `KBotCaptionBar.ShowHelpButton` (default True, so every existing
  window gets it without designer edits). Drawn only once the app has installed help
  (`KBotHelp.IsAvailable`), so the DevHarness and the VS designer show no dead button. Slot
  order right→left: close, max, min, help, theme, options.
- **Seam.** `KBotHelp` / `IKBotHelpProvider` in KBot.Theming (Controls cannot reference App);
  `HelpService` in KBot.App implements it and is installed at the start of `RunShellWithLogin`,
  so help works from the login window on.
- **Finding the topic.** From the control that asked, up its parents; each step offers
  `Type.controlName` or `Type` (form / user control); the first key listed in a topic's
  `screens:` wins; nothing → start page. «?» uses the focused control of its window. Debug builds
  show the keys that were tried in the help window's bar, for whoever writes `screens:`.
- **Who reads what.** Director role → Part 3 only. Others → Part 1, plus Part 2 while
  «Activează opțiuni avansate» is on. The exported manual carries all three parts when advanced
  options are on, otherwise the visible ones.
- **Help window** (`HelpForm`): contents tree, search (all words, case- and diacritic-blind,
  title hits weigh more), Back/Forward, «Exportă manualul...», page in a `WebBrowser` with CSS
  from the active theme palette. `topic:` links are caught in `Navigating`; web links go to the
  default browser.
- **Pictures** are embedded as data; a picture not on disk yet shows a visible «Imagine lipsă»
  box. A link to a missing topic shows in red. Bad topic files are skipped and logged with the
  reason (unknown header key, duplicate id, missing parent).
- **Manual**: one self-contained HTML file (cover, contents, parts on new printed pages), opened
  in the default browser to print or save as PDF.
- New package: **Markdig 0.45.0** (KBot.App).

## Files touched

- `src/KBot.Theming/KBotHelp.vb` (new)
- `src/KBot.Controls/CaptionBar/KBotCaptionBar.HelpButton.vb` (new), `KBotCaptionBar.vb`,
  `src/KBot.Controls/KBot.Controls.vbproj` (nesting)
- `src/KBot.App/Help/HelpTopic.vb`, `HelpLibrary.vb`, `HelpHtml.vb`, `HelpService.vb`,
  `HelpForm.vb`, `HelpForm.Designer.vb` (new)
- `src/KBot.App/HelpContent/` (new): `README.md`, `contabil/index.md`, `contabil/autentificare.md`,
  `contabil/ajutor.md`, `avansat/index.md`, `director/index.md`, `img/.gitkeep`
- `src/KBot.App/KBot.App.vbproj` (Markdig, copy of HelpContent → Help), `src/KBot.App/Program.vb`
  (DI + install)

## Test results

- `dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors.
- Run on screen (Debug, DEMO unit), driven by UI Automation:
  - login window shows «?»; F1 in the password box opened the help at «Conectarea»
    (keys `LoginForm.txtPass › LoginForm.tlpBody › LoginForm`);
  - first attempt showed a BLANK page: the WebBrowser drops a `DocumentText` set before the window
    is shown. Fixed (`SetHtml` holds the page until `OnShown` / `DocumentCompleted`), re-run OK;
  - main window «?» opened «Despre K-BOT» (key `KbotForm`);
  - search «parola» found «Opțiuni avansate» and «Conectarea» (diacritic-blind).
- No unit tests written (house rule: none unless asked).

## Left unverified or deferred

- «Exportă manualul...» not clicked on screen (save dialog + browser); DirectorForm help not
  seen (no director login used); dark theme page colours not seen.
- «Înainte» caption sits a few pixels high in the flat button (cosmetic).
- Seen in passing, not help: `000_DEMO` has no `FX_NoteCAB_Corectii` table
  (`MainForm.RefreshUncorrelatedMarkAsync` logs error 1146 at every start).
- Next: 0000-02 Part 1 content + screenshots, 0000-03 guided tours, 0000-04 Part 2,
  0000-05 Part 3, plus the MF requirements for a new DDF (ORD left out until new ORD exists).
- FileVersion of KBot.App / KBot.Controls / KBot.Theming not bumped (`push-update.ps1` asks).
