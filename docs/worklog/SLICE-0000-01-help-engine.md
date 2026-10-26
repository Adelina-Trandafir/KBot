# SLICE-0000-01…07 — Istoricul inițial al ajutorului K-BOT

Consolidare documentară D002-05: motorul, unealta de capturi, cele trei părți ale manualului, tururile și finisarea din 30.09.2026 sunt etapele aceluiași plan inițial. Fiecare corp păstrează probele, limitele și inventarul său. Numerotarea anunțată în 01 este propunerea inițială; ordinea executată este cea a titlurilor 01…07. Tooltipul nemodificat în 03 este corectat în 04, iar turul Părții 2 amânat în 05 este scris în 07. Nu rezultă confirmarea vizuală a etapelor nevăzute.

Registrul consemna pentru motor numărul inițial 0097-01, renumerotat la cererea operatorului. Regulile de captură și de documentare citate sunt istorice; nu înlocuiesc instrucțiunile actuale și nu implică modificarea ajutorului încorporat sau a capturilor.

[01](#slice-0000-01) · [02](#slice-0000-02) · [03](#slice-0000-03) · [04](#slice-0000-04) · [05](#slice-0000-05) · [06](#slice-0000-06) · [07](#slice-0000-07)

<a id="slice-0000-01"></a>

## SLICE-0000-01 — Help engine (F1, «?», help window, manual export)

Operator request, 30.09.2026: an interactive help system and a manual in three parts — Part 1 Contabil (everyday workflows), Part 2 Opțiuni avansate, Part 3 Director. This first pass is the engine only; content, screenshots and guided tours follow in 0000-02 onwards.

### What changed and why

- **One text source.** Help topics are Markdown files with a header block, under `src/KBot.App/HelpContent/`, shipped as `<AppDir>\Help\`. The help window and the exported manual both render them, so they cannot drift apart. Authoring rules in `HelpContent/README.md`.
- **F1 everywhere.** `HelpKeyFilter` (an application message filter) catches F1 in any K-BOT window. It is a filter, not per-form `HelpRequested`, because some custom controls eat keys.
- **«?» on every caption bar.** `KBotCaptionBar.ShowHelpButton` (default True, so every existing window gets it without designer edits). Drawn only once the app has installed help (`KBotHelp.IsAvailable`), so the DevHarness and the VS designer show no dead button. Slot order right→left: close, max, min, help, theme, options.
- **Seam.** `KBotHelp` / `IKBotHelpProvider` in KBot.Theming (Controls cannot reference App); `HelpService` in KBot.App implements it and is installed at the start of `RunShellWithLogin`, so help works from the login window on.
- **Finding the topic.** From the control that asked, up its parents; each step offers `Type.controlName` or `Type` (form / user control); the first key listed in a topic's `screens:` wins; nothing → start page. «?» uses the focused control of its window. Debug builds show the keys that were tried in the help window's bar, for whoever writes `screens:`.
- **Who reads what.** Director role → Part 3 only. Others → Part 1, plus Part 2 while «Activează opțiuni avansate» is on. The exported manual carries all three parts when advanced options are on, otherwise the visible ones.
- **Help window** (`HelpForm`): contents tree, search (all words, case- and diacritic-blind, title hits weigh more), Back/Forward, «Exportă manualul...», page in a `WebBrowser` with CSS from the active theme palette. `topic:` links are caught in `Navigating`; web links go to the default browser.
- **Pictures** are embedded as data; a picture not on disk yet shows a visible «Imagine lipsă» box. A link to a missing topic shows in red. Bad topic files are skipped and logged with the reason (unknown header key, duplicate id, missing parent).
- **Manual**: one self-contained HTML file (cover, contents, parts on new printed pages), opened in the default browser to print or save as PDF.
- New package: **Markdig 0.45.0** (KBot.App).

### Files touched

- `src/KBot.Theming/KBotHelp.vb` (new)
- `src/KBot.Controls/CaptionBar/KBotCaptionBar.HelpButton.vb` (new), `KBotCaptionBar.vb`, `src/KBot.Controls/KBot.Controls.vbproj` (nesting)
- `src/KBot.App/Help/HelpTopic.vb`, `HelpLibrary.vb`, `HelpHtml.vb`, `HelpService.vb`, `HelpForm.vb`, `HelpForm.Designer.vb` (new)
- `src/KBot.App/HelpContent/` (new): `README.md`, `contabil/index.md`, `contabil/autentificare.md`, `contabil/ajutor.md`, `avansat/index.md`, `director/index.md`, `img/.gitkeep`
- `src/KBot.App/KBot.App.vbproj` (Markdig, copy of HelpContent → Help), `src/KBot.App/Program.vb` (DI + install)

### Test results

- `dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors.
- Run on screen (Debug, DEMO unit), driven by UI Automation:
  - login window shows «?»; F1 in the password box opened the help at «Conectarea» (keys `LoginForm.txtPass › LoginForm.tlpBody › LoginForm`);
  - first attempt showed a BLANK page: the WebBrowser drops a `DocumentText` set before the window is shown. Fixed (`SetHtml` holds the page until `OnShown` / `DocumentCompleted`), re-run OK;
  - main window «?» opened «Despre K-BOT» (key `KbotForm`);
  - search «parola» found «Opțiuni avansate» and «Conectarea» (diacritic-blind).
- No unit tests written (house rule: none unless asked).

### Left unverified or deferred

- «Exportă manualul...» not clicked on screen (save dialog + browser); DirectorForm help not seen (no director login used); dark theme page colours not seen.
- «Înainte» caption sits a few pixels high in the flat button (cosmetic).
- Seen in passing, not help: `000_DEMO` has no `FX_NoteCAB_Corectii` table (`MainForm.RefreshUncorrelatedMarkAsync` logs error 1146 at every start).
- Next: 0000-02 Part 1 content + screenshots, 0000-03 guided tours, 0000-04 Part 2, 0000-05 Part 3, plus the MF requirements for a new DDF (ORD left out until new ORD exists).
- FileVersion of KBot.App / KBot.Controls / KBot.Theming not bumped (`push-update.ps1` asks).

<a id="slice-0000-02"></a>

## SLICE-0000-02 — Help capture tool («Capturi pentru ajutor»)

Operator request, 30.09.2026: the operator takes the help screenshots through K-BOT itself (also on a client's PC, for the FOREXE flow); Claude only writes text with capture tags.

### What changed and why

- **Capture tag** (one line in a topic): `<!-- capture: <id> | caption: .. | goto: .. | prepare: .. -->`. The tag IS the picture: rendered as `img/<id>.png`, or «Imagine lipsă: <caption>» until taken. Parsed by `HelpCapture`; `HelpLibrary.Captures` lists all tags of all parts; bad tags / duplicate ids are logged and skipped. Format documented in `HelpContent/README.md`.
- **Setting** `AppSettings.HelpCaptureMode` + checkbox «Mod capturi pentru ajutor» in Setări › Aplicație, visible only with advanced options; honoured only while those are on. Not described in the help.
- **Entry points**: «Meniu › Capturi pentru ajutor» (hidden item, shown on menu opening when the mode is on) and «Capturi...» in the help window's bar.
- **HelpCaptureForm**: KBotDataView of every capture (part, page, caption, prepare, state) with a «Fă poza» / «Refă» button per row, filter «Doar cele care lipsesc», «Reîncarcă», «Deschide dosarul».
- **Flow**: window hides → `goto` (`view:` / `menu:` / `setari:` via `IHelpCaptureNavigator` implemented by KbotForm; `help` / `help:<id>`) → floating `HelpCapturePromptForm` with the prepare text and any note (view off for the selected angajament, unknown target...) → «Capturează» → `HelpCaptureOverlay` freezes the monitor under the cursor: drag = rectangle, click = window under cursor, Ctrl+click = control under cursor, Esc/right click = cancel.
- **Saving** (`HelpCaptureStore`): always `<AppDir>\Help\img\<id>.png`; also `src\KBot.App\HelpContent\img\` when running from a repository build. On a client PC the files stay in `C:\KBOT\Help\img\` (updater never deletes unshipped files) and are copied back by hand.
- Sample tags on three start pages (conectare-fereastra, fereastra-principala, setari-aplicatie).

### Files touched

- New: `src/KBot.App/Help/HelpCapture.vb`, `HelpCaptureStore.vb`, `HelpCaptureNative.vb`, `HelpCaptureOverlay.vb`, `HelpCapturePromptForm(.Designer).vb`, `HelpCaptureForm(.Designer).vb`, `IHelpCaptureNavigator.vb`, `src/KBot.App/KbotForm.HelpCapture.vb`, this worklog.
- Edited: `HelpLibrary.vb`, `HelpHtml.vb`, `HelpService.vb`, `HelpForm(.Designer).vb`, `KbotForm.Designer.vb` (2 menu items), `KbotForm.Nomenclatoare.vb` (menu case), `Setari/SetariAplicatieView(.Designer).vb`, `KBot.Common/AppSettings.vb`, `KBot.App.vbproj`, `HelpContent/README.md`, `HelpContent/{contabil,avansat}/*.md`.
- Renumbering 0097 → 0000 (operator): code comments, worklog 0000-01, status files.

### Test results

- Build KBot.App: 0 warnings, 0 errors. The operator confirmed the checkbox is there.
- The capture flow itself (list, prompt, overlay, save) was NOT run by Claude — the operator runs it.

### Left unverified or deferred

- Whole capture flow on screen; multi-monitor with different DPI (overlay covers only the monitor under the cursor); `ReloadLibrary` redraws the open help page but not its contents tree.
- «Capturi...» keeps its 110px column in the help bar when hidden (empty gap).
- The `Color.Black` dimming mask in the overlay is deliberate (it shades a photo, not UI).
- FileVersions not bumped.

### Addendum (same day) — «Încarcă»

Operator: some pictures cannot be taken from inside K-BOT. Each row now has a second button, «Încarcă»: pick a PNG / JPG / BMP from disk; it is copied into a fresh bitmap (source file not left locked) and saved as `<id>.png` to the same folders as a capture. Build: 0 warnings, 0 errors. Files: `HelpCaptureForm.vb`, `HelpCaptureForm.Designer.vb` (column `incarca`).

<a id="slice-0000-03"></a>

## SLICE-0000-03 — Part 1 «Contabil»: the text, with capture tags

Operator request, 30.09.2026: write Part 1 from the slice .md files; no custom-control or browser internals; the FOREXE page as it is meant to work, not what is clicked on it; a section on what MF expects for a NEW DDF (ORD creation left out: it does not exist yet).

### What changed and why

31 Romanian topics under `src/KBot.App/HelpContent/contabil/` (+ README example), 30 capture tags for the operator to shoot through «Capturi pentru ajutor» (0000-02).

| Chapter | Topics |
|---------|--------|
| Despre K-BOT | overview + the golden rule; Conectarea; Fereastra principală; Cum folosești ajutorul |
| Legătura cu FOREXE | band, conectare (+ operațiuni necorectate), lista, descărcarea (+ Asocieri, recepții tăiate, reîmprospătări parțiale), Browser FOREXE, consola |
| Vederile | Sumar, Istoric, Rezervări, Recepții (+ asocieri oricând), Plăți, Extrase (view + window + modes + columns), Ordonanțare (view only) |
| Documentul de fundamentare | flow + the seven revision states + tree menu; **Ce cere MF la un DDF nou**; Angajament nou; Editorul; Adaugă rezervare / Definitivează / Derulează; Semnarea; Trimiterea |
| Other | Operațiuni necorelate + note CAB; Nomenclatoare (Clasificații, Parteneri); Setări și aspect |

Sources actually read: worklogs 0011-02, 0014, 0015-03, 0017-03/04, 0022-02, 0033, 0034, 0048-04, 0055, 0057, 0060, 0063, 0074, 0080-02/03, 0081-12, 0084, 0087, 0088, 0091, 0092, 0093, 0095, 0096; `docs/FUNDAMENT_DocumentFundamentare_CAB.md` (MF guide for OMF 1140/2025 — the MF section is built from it), `docs/HANDOFF_0081_DDF_Trimitere.md`, `docs/PLAN_DDF_Trimitere.md` (states + Rezervări menu table); operator strings from the designers (tooltips, captions) and `DdfView` / signing messages. State names and menu captions checked against the code, not only the plan.

`screens:` keys use the real form / view / control names, so F1 lands on these topics.

### Files touched

- New: `HelpContent/contabil/{fereastra,notecab,setari}.md`, `contabil/forexe/*` (6), `contabil/vederi/*` (8), `contabil/ddf/*` (7), `contabil/nomenclatoare/*` (3).
- Rewritten: `contabil/index.md`, `contabil/autentificare.md`.

### Test results

- Build KBot.App: 0 warnings, 0 errors; 33 topic files copied to `bin\...\Help\`.
- Static check (bash): no duplicate topic ids, every `parent:` and every `topic:` link resolves, no duplicate capture id. Not opened on screen (operator's rule).

### Left unverified or deferred

- **To be read by the operator** — written from the code/worklogs, not from how accountants work: especially «Asocieri» (what to decide in the basket), the Recepții footer LEFT icon (described as «reface din istoric recepțiile care lipsesc», from slice 0062's name only), the DDF «+» on a reservation leaf, the signing steps' button names inside the PDF («Validez antet și secțiunea A», «Validez secțiunea B», from the MF guide summary), and the Setări page list.
- **Found in passing:** the «Sursă/Sector» combo's tooltip says «Subperioada (SS) din anul ales» but the combo holds a sector/source (e.g. 02A). The help says sector/source; the tooltip in `KbotForm.Designer.vb` (`cboSs`) was NOT changed.
- ORD creation not described (does not exist yet); Part 2 and Part 3 not written (0000-05/06).
- Guided tours (0000-04) not started.

<a id="slice-0000-04"></a>

## SLICE-0000-04 — Guided tours (+ the «Sursă / sector» tooltip)

Operator, 30.09.2026: «fix the tooltip, then go ahead with 0000-04».

### What changed and why

- **Tooltip** (`KbotForm.Designer.vb`, `cboSs`): header «Sursă / sector», text «Sursa și sectorul (de exemplu 02A) din anul ales. / Schimbarea lor reîncarcă arborele și toate ecranele. / Ultima alegere se ține minte pentru data viitoare.» (was «Subperioada (SS)...», wrong — found in 0000-03).
- **Tour files**: `HelpContent/tours/*.md` — header (`id`, `title`, `part`, `topic`) + one `## ` per step with optional `target:` / `goto:`. Parsed by `HelpTour`; loaded by `HelpLibrary` (the topic loader skips the folder; bad files / duplicate ids / missing topic are logged). Format in README.
- **Engine**:
  - `HelpTourRunner` — per step: `HelpService.Navigate(goto)` (moved out of `HelpCaptureForm`, now shared by captures and tours), 400 ms for a lazily built view, finds the target among the open windows (`TypeName` / `TypeName.controlName`, first visible), rings it, places the bubble. One tour at a time. A missing target → centred bubble with a note; the tour goes on.
  - `HelpTourFrame` — top-most, click-through, never-activating hollow rectangle in the accent colour.
  - `HelpTourBubble` (Designer) — «Pasul n din m», title, text, note, «◄ Înapoi» / «Înainte ►» («Gata» on the last) / «Închide»; → / Enter / ← / Esc; sized to its text; placed right, left, below or above the target, clamped to the working area.
- **Entry points**: «▶ Tur ghidat: ...» at the top of the tour's topic page and a «Tururi ghidate» list on the help start page (`tour:` link scheme). The help window minimizes during a tour and comes back after. Tours are not in the exported manual.
- **Four tours** (Part 1): `tur-fereastra` (9 steps), `tur-forexe` (9), `tur-rezervari` (4), `tur-ddf` (6).

### Files touched

- New: `Help/HelpTour.vb`, `HelpTourRunner.vb`, `HelpTourFrame.vb`, `HelpTourBubble(.Designer).vb`, `HelpContent/tours/{tur-fereastra,tur-forexe,tur-rezervari,tur-ddf}.md`.
- Edited: `HelpLibrary.vb` (tours; `ParsePart` Friend), `HelpService.vb` (`Navigate`, `StartTour`), `HelpCaptureForm.vb` (uses `Navigate`), `HelpHtml.vb` (`TourScheme`, links, CSS), `HelpForm.vb` (`tour:` links), `HelpContent/README.md`, `KbotForm.Designer.vb` (tooltip).

### Test results

- Build KBot.App: 0 warnings, 0 errors; 4 tour files copied to `bin\...\Help\tours\`.
- Static check: all 21 distinct `target:` values name a real type and a control declared in its designer. Not run on screen (operator's rule).

### Left unverified or deferred

- On screen: ring placement at other DPIs / multi-monitor, bubble height with long texts, focus returning to K-BOT after «Gata».
- Painted parts (tree footer icons, caption-bar buttons, node icons) cannot be ringed on their own; the steps ring the whole control and say where to look.
- Tours for Part 2 and Part 3 come with those parts (0000-05 / 0000-06).

<a id="slice-0000-05"></a>

## SLICE-0000-05 — Part 2 «Opțiuni avansate»: the text, with capture tags

Operator, 30.09.2026: «go ahead with 0000-05».

### What changed and why

Six Romanian topics under `src/KBot.App/HelpContent/avansat/` (part `avansat`, shown only while «Activează opțiuni avansate» is on), 7 capture tags:

| Topic | Covers |
|-------|--------|
| `avansat` — Opțiuni avansate | how to switch them on (password), what appears (fila «Documente», «Pagina FOREXE», «Temă», «Căi fișiere»), the password is a guard not security, the manual then carries all parts |
| `avansat.documente` | PDF engine (ActiveX / Fereastră găzduită), the hosted-window dialog (/n, release A/B, full screen on exit), ActiveX trace + new control per document, «Mesaje de script Adobe» (list, Probă, Lista implicită), Excel ribbon (Macro / Fereastră) |
| `avansat.pagina` | FOREXE page CSS rules: fields, «Salvează și aplică», «Regulă nouă...» (element tree, framed in the page), «Implicite» (the five default rules); the documents' captures «pagina originală / așa cum se vede» |
| `avansat.tema` | scheme edit / save / restore; scaling Automat / Fix 100% / Manual + factor; «Windows întinde fereastra» (restart); text size, «Font din temă», window base |
| `avansat.foldere` | the six folders, defaults, empty = default, relative paths, effect at next start, startup refusal on a bad path |
| `avansat.jurnale` | the log files and what each holds, server logs on the Jurnal page, «Consola FOREXE detaliată», what to send when reporting a problem |

**Deliberately NOT described:** the help capture mode (operator's rule, 0000-02).

Sources: the designers' operator text of `SetariAplicatieView`, `SetariPaginaView`, `SetariTemaView`, `SetariFolder`, `AdobeGazduireForm`, `AdobeMesajeForm`, `RegulaPaginaForm`; `SetariForm.AdvancedPageKeys`; `KBot.Common/SetariFoldere.vb` (folder list); `docs/SETARI_UTILIZATOR.md` §6–7 (settings window, FOREXE page rules, document captures). §1–2 of that guide (Adobe clipping / moving profiles) were NOT used: they describe the old hosting that the operator has since ruled out.

`screens:` keys point F1 on the «Documente» tab's controls and on the advanced pages here.

### Files touched

- Rewritten: `HelpContent/avansat/index.md`.
- New: `HelpContent/avansat/{documente,pagina,tema,foldere,jurnale}.md`.

### Test results

- Build KBot.App: 0 warnings, 0 errors.
- Static check: no duplicate ids / capture ids, every parent and topic link resolves, every `Type.control` screen key names a control declared in that type's designer. Not run on screen.

### Left unverified or deferred

- To be read by the operator: whether the Adobe options should be described at all for clients, and the «Ce trimiți când ceri ajutor» advice.
- No guided tour for Part 2 (none asked for; the engine takes one file in `tours/` if wanted).
- Part 3 «Director» (0000-06) next.

<a id="slice-0000-06"></a>

## SLICE 0000-06 — Part 3 «Director»

**Date:** 30.09.2026. **Standing slice:** 0000 (help).

### What was written

Content only; no code changed.

- `HelpContent/director/index.md` (rewritten from the stub): what the director's window is, where the director's signature sits in the DDF flow (states «Semnat A și B — la director» → «Aprobat»), the window at a glance.
- `director/lista.md`: the list, its six columns, the hover text, «Reîncarcă lista» (the list does not refresh itself after a signature), the three failure cases («Nu s-au putut citi: ...», an error in place of the title, a document that does not appear).
- `director/unitati.md`: one session = one unit; a document of another unit → question → login window with that unit pre-selected → old session logged out; the two follow-up messages; the expired-session re-login on the same unit.
- `director/semnare.md`: read-only DDF view (the refusal sentence quoted), the signing steps on «Document PDF», what happens after, the same «if something goes wrong» list as the accountant's signing topic.
- `tours/tur-director.md`: 5 steps on `DirectorForm` (lblTitlu, lstDocumente, pnlDocument, lblStare, btnReincarca).

F1 keys: `DirectorForm`, `DirectorForm.capBar`, `.lstDocumente`, `.lblTitlu`, `.btnReincarca`, `.pnlDocument`, and the DDF view's page types. A director sees only Part 3, so the DDF page types here do not clash with the accountant's DDF topics.

Captures: `director-fereastra`, `director-lista`, `director-semnare`, with no `goto` (the capture list opens only from the accountant's main window, which cannot open the director's). Each `prepare` says to shoot on the director's PC and load the picture with «Încarcă».

### Source
`src/KBot.App/Director/DirectorForm.vb` + `.Designer.vb` (operator strings, control names), [0081-06 — Director](SLICE-0081-01-starea-reviziei.md#slice-0081-06), `KBot.Domain/DdfRevisionState.vb` (state labels), `contabil/ddf/semnare.md` (shared signing behaviour).

### To check (operator)
1. The signature field's name in the final PDF: the text says «câmpul semnăturii ordonatorului de credite». Is there a «Validez» button for the director too, as for A and B?
2. «Anunță administratorul K-BOT» — right wording for who the director calls?

### State
Build App **0 warnings, 0 errors**; the files reach `bin\...\Help\director\` and `Help\tours\`. Nothing seen on screen (no director login available here).

<a id="slice-0000-07"></a>

## SLICE 0000-07 — coverage pass + finishing

**Date:** 30.09.2026. **Standing slice:** 0000 (help). The plan (01–06) was complete; the operator said «go ahead with 0000-07» without naming it, so 07 = make F1 reach a topic from every operator window, write what was missing, add the Part 2 tour.

### Coverage check

Every `Form` / `UserControl` in KBot.App (outside the harness and the help itself) was compared with the `screens:` keys of the topics. 24 had no key. Sorted:

| Kind | Screens | Done |
|------|---------|------|
| Pages inside a keyed window (F1 walks up to the parent) | CabNoteReceiptPage, DdfFileBrowser, DdfFisierPreview, DdfValoriPage, ReaderHostPreview, XfaXmlPreview, OrdVizualizarePage, OrdDocumentPage, RegulaPaginaEditor | nothing needed |
| Debug / leftover | StartupLauncherForm (Debug launcher), PlaceholderView | left out |
| Separate windows without a topic | AlegereUnitateForm, GraficeAsociereForm, AsociereBenziForm, IstoricIntervalForm, LogClearDialog, UpdateProgressForm, InternalInfoForm | written, see below |
| ORD editor | OrdEditForm, OrdZiuaForm, OrdTextForm, OrdAtasamentePage, OrdBeneficiariPage, OrdDocumentePage | **left out: waiting on the operator** (ORD is excluded until new ORD exists) |

### What was written

- `contabil/forexe/descarcare.md`: «Grafice și benzi» (window + enlarged bands) and «Alegerea unității» (the save stops; «Alege unitatea», «Nu mă mai întreba...», «Renunță»). +2 captures.
- `contabil/forexe/browser.md`: «Ce face K-BOT după o salvare făcută în pagină» (slice 0073 watcher: new angajament / reception / reservation with the «Ați terminat...?» question; the «Istoric angajament» window, «Tot istoricul»; one takeover at a time). Only what K-BOT does after the save, not the clicks in the page. The old line «ce modifici aici de mână ocolește K-BOT» was wrong since 0073 and was replaced. +1 capture.
- `contabil/index.md`: the «regula de aur» now names the browser view as a road through K-BOT.
- `contabil/fereastra.md`: the ⓘ button → «Informații interne».
- `contabil/setari.md`: «Golirea jurnalelor» (two steps, cannot be undone, file in use, server logs untouched).
- NEW `contabil/actualizare.md`: the startup check (none / offered / mandatory / server down), «Caută actualizări» in Setări › Informații, the download window, updater keeps Logs and settings (settings live in %APPDATA%). +1 capture.
- NEW `tours/tur-avansat.md`: 7 steps across Setări (pages, «Activează opțiuni avansate», Aplicație tabs, FOREXE console, Pagina FOREXE, Temă, Căi fișiere).

### Checks
Static check: ids unique, every link / parent / tour topic resolves, capture ids unique, every screen and tour target names a real type and control. Build App **0 warnings, 0 errors**. Nothing on screen.

### Not done
- «Înainte» caption a few pixels high (0000-01 note): both buttons are set up the same; the offset is the tall «Î» glyph. Not nudged blind.
- «Exportă manualul...» still not clicked on screen.
