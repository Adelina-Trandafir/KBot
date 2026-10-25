# SLICE-0000-18…22 — Istoric al asistentului de ajutor

Etapele din 30.09.2026 sunt păstrate integral, cu propunerea în [planul inițial](../PLAN_help_assistant.md) și întreținerea în [HELP_SYSTEM](../HELP_SYSTEM.md). Ordinea contează: 18 introduce pagina HTML de rezultate; 20 o înlocuiește cu lista comună ferestrei și meniului; 21 adaugă întrebări și stele, iar 22 încheie textul/documentarea. Dosarele meniului se deschid inline, față de submeniul propus inițial. Ghidul de întreținere și [istoricul tururilor/ferestrei](SLICE-0000-23-tururi-pe-butoane-istoric-capturi-estompate.md) consemnează și schimbări ulterioare (0000-27: tururi doar pentru fereastra al cărei «?» a fost apăsat; 0000-32: cartea continuă); etapele de mai jos nu sunt descrierea tuturor versiunilor ulterioare. SQL-ul este exemplu istoric, neexecutat în consolidare. DDL/ruta nerulate și probele nevăzute pe ecran rămân astfel la reperele înregistrate.

[0000-18](#slice-0000-18) · [0000-19](#slice-0000-19) · [0000-20](#slice-0000-20) · [0000-21](#slice-0000-21) · [0000-22](#slice-0000-22)

<a id="slice-0000-18"></a>

## SLICE-0000-18 — Search that understands a typed question (no model)

Plan: `docs/PLAN_help_assistant.md` § 0000-18.

### What changed and why

The old `HelpLibrary.Search` needed EVERY word of the query in a topic, matched whole topics only and knew no word endings, so a real question («cum trimit un DDF?») found nothing. The search now:

1. **Drops filler words** — one list, `HelpSearch.StopWords` (folded Romanian: cum, se, un, o,
   la, de, pot, sa, unde, ce, care, pentru, ...). Words of one letter are dropped too.
2. **Matches stems** — folded (lower case, no diacritics, one character for one character),
   a common Romanian ending dropped (`-urilor`, `-ilor`, `-ului`, `-ii`, `-ea`, `-a`, `-e`,
   `-i`...), then cut to 5 letters: «trimit» and «trimiterea» both become «trimi», «plăți» and
   «plata» both «plat». A second, looser key (the first 4 letters of the stem) scores half, so
   «semnez» still meets «semnare».
3. **Scores by how many words match**, not «all or nothing»: score = words matched × 100 +
   weight (capped at 99). Weights per word: title 10 / 5, keywords 6 / 3, section heading 8 / 4,
   section text 2 / 1 (full stem / first four letters). With three words or more a section must
   hold at least half of them; with one or two, one is enough.
4. **Searches per SECTION** — the text before the first `## ` and each `## ` heading with its
   text. A hit = topic + section heading + a snippet (~170 characters around the first match)
   + the section's anchor. A topic gives at most 3 hits, and its extra hits only for sections
   that match on their own (not just through the topic title).
5. `keywords:` is unchanged as the place for synonyms (content in 0000-19 / 0000-22).
6. **Done once at load**: `HelpSearch.Index` (called from `HelpLibrary.Parse`) splits each
   topic into sections, folds and stems title, keywords, headings and text into hash sets. A
   query only folds its own words and looks them up. The reading order used to break ties is
   worked out once per part.

Result model: `HelpHit(TopicId, SectionAnchor, Title, SectionTitle, Snippet, Score)` + `Key` (`topicId#anchor`, the id the question log will store in 0000-21).

**Anchors.** `HelpSearch.AnchorFor(heading)` = folded words of the heading joined by dashes (`pagina-forexe-certificatul-memorat`), unique inside the topic (`-2`, `-3`). The topic page gives every level-2 heading the same id (`HelpHtml.AddSectionAnchors`, from the heading's text, same order), so both sides agree without storing anything. The printed manual gets no section ids (they would clash between topics).

**Help window.** `HelpForm.ShowTopic(id, sectionAnchor)`; page keys may be `t:<id>#<anchor>` (history and «Înapoi» keep the section); the section is scrolled into view when the page has loaded (`DocumentCompleted`), carried with the pending page when the browser is busy. The results page (`HelpHtml.SearchPage`) lists one entry per section: «Topic › Section» + snippet, link `topic:<id>#<anchor>`. `topic:id#anchor` links inside topics work too.

### Files touched

- `src/KBot.App/Help/HelpSearch.vb` (new): `HelpHit`, `HelpTermSet`, `HelpQueryTerm`,
  `HelpSection`, `HelpSearch` (fold, words, stop words, stem, plain text, anchors, index, search,
  snippet)
- `src/KBot.App/Help/HelpTopic.vb`: `Sections`, `TitleTerms`, `KeywordTerms` (Friend)
- `src/KBot.App/Help/HelpLibrary.vb`: `Parse` indexes; `Search` returns `List(Of HelpHit)`;
  old `Fold` removed (only the old search used it)
- `src/KBot.App/Help/HelpHtml.vb`: `SearchPage(query, hits)`, heading anchors, `.hit` style,
  `topic:id#anchor` links
- `src/KBot.App/Help/HelpForm.vb`: section in the page key, scroll to the section

### Test results

- `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors**.
- `tools\HelpCheck\Check-Help.ps1 -Coverage -Map`: **No errors.** Coverage lists
  `RobotQueueForm` (slice 0098, another thread's window; already noted in 0000-17).
- No tests, no app run (operator's rule).

### Left unverified or deferred

- The ranking was never run on real questions: the weights are a first guess. 0000-21 records
  every question and the hit opened, so they can be tuned from data.
- The scroll to a section relies on `HtmlElement.ScrollIntoView` in the WebBrowser control;
  not seen on screen.
- The help text describing the search is updated in 0000-22 (planned there, tag `0000-18`).

---

<a id="slice-0000-19"></a>

## SLICE-0000-19 — `open:` actions on topics

Plan: `docs/PLAN_help_assistant.md` § 0000-19. Engine + content, no UI (the buttons are drawn by the «?» popup and the help window's search list in 0000-20).

### What changed and why

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

### Files touched

- `src/KBot.App/Help/HelpTopic.vb` (`Open`)
- `src/KBot.App/Help/HelpLibrary.vb` (`open` key, `GotoPattern`, hit actions, `TourOf`)
- `src/KBot.App/Help/HelpSearch.vb` (`HelpHit.OpenTarget / TourId / TourTitle`)
- `src/KBot.App/Help/HelpService.vb` (`OpenButtonText`, `OpenScreen`)
- `src/KBot.App/Help/IHelpCaptureNavigator.vb` (`TargetCaption`)
- `src/KBot.App/KbotForm.HelpCapture.vb` (`TargetCaption`)
- `tools/HelpCheck/Check-Help.ps1` (`open` key + value check)
- `src/KBot.App/HelpContent/README.md`
- 25 topic headers under `src/KBot.App/HelpContent/contabil/` and `avansat/` (listed above)

### Test results

- `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors**.
- `tools\HelpCheck\Check-Help.ps1 -Coverage -Map`: **No errors.** (coverage: `RobotQueueForm`,
  0098, unchanged).
- No tests, no app run.

### Left unverified or deferred

- The buttons themselves: 0000-20. Nothing here is visible to the operator yet.
- `setari:jurnal` / `setari:pagina` / `setari:tema` / `setari:foldere` open pages that exist
  only with the advanced options on; their topics are in part 2, which is shown only then.

---

<a id="slice-0000-20"></a>

## SLICE-0000-20 — The «?» popup

Plan: `docs/PLAN_help_assistant.md` § 0000-20.

### What changed and why

The «?» of every caption bar used to open the help window, exactly like F1. It now opens a popup under the button with a search box, the topic of the current screen and the guided tours of the windows on screen. **F1 is unchanged**: it still opens the help window on the topic of the focused control. The help window's search box is the same control as the popup's.

1. **Seam.** `IKBotHelpProvider.ShowHelpMenu(origin, anchorScreenRect)` + `KBotHelp.RequestMenu`
   (KBot.Theming). `KBotCaptionBar.HelpButtonClicked` calls `RequestMenu` with the button's screen
   rectangle. `ShowHelp` / `KBotHelp.Request` (F1) stay as they were.
2. **Controls** (`KBot.Controls/Popup/`, doc `KBotHelpPopup.md`, row in `CONTROLS.md`). The existing
   popups were looked at first: `CustomPopup` is a flat painted menu with no text box and no
   submenus, `KBotDropDownMenu` never activates (so no keyboard for a search box). Neither fits,
   so a small family was added next to them, reusing `KBotTextField` (search box),
   `KBotScrollBar`, `KBotToolTip.ShowAt`, `ThemeShapes`, `KBotThemedForm` /
   `KBotThemedUserControl`:
   - `KBotHelpRow` + `IKBotHelpSearchSource` — the rows and the interface the application
     implements (the controls cannot reference KBot.App);
   - `KBotHelpList` — painted list: headers, hits (title › section, two lines of text,
     «Deschide «...»» / «Tur ghidat» buttons), the screen's topic, tours, folders of tours; own
     scroll bar; hover tooltips;
   - `KBotHelpSearchPanel` (UserControl, designer) — box + list; types → local search after
     150 ms; Up / Down / Enter drive the list; Esc raised to the host;
   - `KBotHelpPopup` (Form, designer) — the panel + last line «Deschide ajutorul complet (F1)»;
     placed under the button (above it when there is no room); closes on Esc, click elsewhere
     and when a row hands over (deferred with `BeginInvoke`).
   All `IThemedControl`, colours only from the palette (kept in fields, so a host designer has
   nothing to serialize), logical px scaled at layout, `AutoScaleMode.Dpi` 96/96.
3. **Tours in the popup** (`HelpPopupTours`): the application windows in z-order (`EnumWindows`),
   skipping minimized, disabled (behind a modal dialog) and the help's own windows. A tour
   belongs to a window when one of its screens is VISIBLE in it (`HelpTourRunner.FindInWindow`,
   split out of `FindTarget`): the form, a control, or the selected view of the main window (the
   other views are hidden). A tour is offered once, on the top window it belongs to; only parts in
   `VisibleParts()`. One window with tours → listed directly; several → one folder per window,
   titled with its caption, the top one open.
   New optional tour header key `screens:` (`HelpTour.Screens`, `$TourKeys` + key check in
   `Check-Help.ps1`, README) — used by `tur-avansat` (`screens: SetariForm`), whose topic lists only
   a checkbox.
4. **Rows and actions** (`HelpSearchSession`, one per panel, `popup` / `fereastra`): a hit →
   the help window at its section (`HelpService.ShowHit`); from the popup the window's search list
   takes over the question and its results (`HelpForm.TakeOverSearch`). «Deschide «...»» →
   `HelpService.OpenScreen` (0000-19); «Tur ghidat» / a tour row → `StartTour`; the topic row → the
   topic; «Deschide ajutorul complet» → what F1 would open for that window. Nothing found → a
   short note row.
5. **Help window**: `txtCauta` replaced by `pnlCautare` (`KBotHelpSearchPanel`) in
   `HelpForm.Designer.vb`. Folded to its box over the contents while empty; with results it takes
   the whole left side (the contents come back when the box is emptied or on Esc). The HTML
   results page (`HelpHtml.SearchPage`, the `s:` pages) is gone: the list is the results page now.
   The start page sentence about F1 / «?» / the search box updated.
6. **Tours, content**: five new tours for screens that had none — `tur-sumar`, `tur-istoric`,
   `tur-notecab`, `tur-clasificatii`, `tur-parteneri` (text only from what their topics already
   say; targets checked against the designers). Left without a tour on purpose: the DDF and ORD
   editors (operator's decision in 0000-07 / 0000-11), `BrowserView` (its buttons are in
   `tur-forexe`), `RobotQueueForm` (0098, has no topic yet).

### Files touched

- `src/KBot.Theming/KBotHelp.vb`
- `src/KBot.Controls/CaptionBar/KBotCaptionBar.HelpButton.vb`
- `src/KBot.Controls/Popup/KBotHelpRow.vb`, `KBotHelpList.vb`, `KBotHelpSearchPanel.vb`,
  `KBotHelpSearchPanel.Designer.vb`, `KBotHelpPopup.vb`, `KBotHelpPopup.Designer.vb`,
  `KBotHelpPopup.md` (new); `src/KBot.Controls/CONTROLS.md`
- `src/KBot.App/Help/HelpService.vb` (`ShowHelpMenu`, `PopupHomeRows`, `ShowHit`, `MainWindow`,
  `ShowHelpForKeys`, `TopicForKeys`)
- `src/KBot.App/Help/HelpSearchSession.vb`, `HelpPopupTours.vb` (new)
- `src/KBot.App/Help/HelpTour.vb` (`Screens`), `HelpTourRunner.vb` (`FindInWindow`)
- `src/KBot.App/Help/HelpForm.vb`, `HelpForm.Designer.vb`, `HelpHtml.vb`
- `src/KBot.App/HelpContent/README.md`; `tours/tur-avansat.md`; new `tours/tur-sumar.md`,
  `tur-istoric.md`, `tur-notecab.md`, `tur-clasificatii.md`, `tur-parteneri.md`
- `tools/HelpCheck/Check-Help.ps1`

### Test results

- `dotnet build src\KBot.App\KBot.App.vbproj` (builds Theming and Controls too): **0 warnings,
  0 errors**.
- `tools\HelpCheck\Check-Help.ps1 -Coverage -Map`: **No errors.** 48 topics, 17 tours, 54 capture
  tags; coverage `RobotQueueForm` (0098) as before.
- No tests, no app run: the popup was never seen on screen.

### Left unverified or deferred

- Nothing here was rendered: placement under the button, the list's row heights at 125 / 150 %,
  the folder toggle, the tooltips, the deferred close when a tour bubble or a message takes the
  focus. The operator should open «?» on the main window (one window, tours listed) and with
  Setări open over it (two folders).
- The popup's stars and the question log: 0000-21. The help text for the popup: 0000-22.
- Folders open inline in the list rather than as a cascading submenu (simpler keyboard, no
  second activating window); say if a real submenu is wanted.

---

<a id="slice-0000-21"></a>

## SLICE-0000-21 — Questions and ratings to the server

Plan: `docs/PLAN_help_assistant.md` § 0000-21 (+ the operator's decisions of 30.09.2026 at its end).

### What changed and why

Every question typed in the help (the «?» popup and the help window) is kept, with the results shown, the hit used and a 1-5 star rating, so the later «step 4» decision (an embedding model or not) can be made from real data. **Nothing about who or where** is stored or logged by our code.

#### What a row holds (and what it does not)

`FX_AjutorIntrebari` in `AVACONT_COMUN` (name follows the FX_ tables there, checked against `MariaDB_Schema/AVACONT_COMUN.sql`): `Qid` (random GUID made on the client per question, UNIQUE), `Intrebare` (trimmed, ≤ 300), `Moment` (UTC), `Parte` (`contabil` / `contabil+avansat` / `director`), `VersiuneApp`, `VersiuneAjutor` (the watermark date), `Rezultate` (top 5 hit ids `topicId#section` joined by `|`, ids only), `Deschis` (hit used), `Actiune` (`open` / `tour`), `Nota` (1-5 or NULL), `Loc` (`popup` / `fereastra`). Index on `Moment` only (+ the unique on `Qid`). **Not stored:** user, e-mail, unit, DC, IP, machine, session token. Kept for good.

#### Server (`PYTHON/`)

- `sql/0000_21_fx_ajutor_intrebari.sql` — the DDL. The service account already has INSERT /
  UPDATE on `AVACONT_COMUN.*` (`sql/0076_avacont_drepturi.sql`), no GRANT.
- `routes/help_feedback.py` — `POST /api/help/feedback`, registered in `main.py`. Bearer guard
  (`require_session`, not the X-Api-Key one). Body `{"rows": [...]}`, at most 50 rows / 64 KB
  (413 / 400 otherwise). Every field validated (GUID form, text length, UTC form, parts, version
  forms, ≤ 5 hit ids of the id pattern, action / rating / place from their small sets); a bad row
  is counted in `rejected` and skipped, the rest saved in ONE `executemany` upsert
  (`INSERT ... ON DUPLICATE KEY UPDATE`, idempotent by `Qid`). Answer `{"saved", "rejected"}`.
- **Logging.** The root logger's handlers stamp every line with the caller's IP and session tag
  (`utils/logger.py`), so the route: (1) has its own logger `help_feedback` → `help_feedback.log`,
  `propagate = False`, format without IP / tag, lines with counts and error TYPES / errno only
  (a driver message can quote a value); (2) opens its own connection instead of
  `get_kbot_connection`, which logs a failed connect through the root logger; (3) drops
  `g.session` / `g.session_token` as soon as the guard has passed, so no stray line (a library's)
  can carry the user. The guard's own `AUTH_401` lines on a bad token are unchanged (they carry
  a token prefix, no question) and the VPS access log is out of scope (operator).

#### Client (`KBot.App`, `KBot.Api`, `KBot.Controls`)

- **Waiting list** (`HelpQuestionLog`): one small JSON file per question, named by its id, in
  `%APPDATA%\AVACONT\KBot\HelpOutbox\` (per user, survives updates). Written through a temp file
  + rename, so a crash leaves a whole file; still there after an offline start. A later change
  (click, rating) rewrites the same file. Chosen over SQLite: nothing to open or migrate, one
  file = one row, and the upsert on the server makes a resend harmless.
- **Sending**: in the background (`Task.Run`), batches of ≤ 50, oldest first: at start
  (what an earlier offline run left), every 3 minutes (timer), as soon as 10 wait, and at exit
  (`Application.ApplicationExit`, 3 seconds at most). Only while logged in (a token). A file is
  deleted only after the server's 200, and only if the question did not change while on the
  wire (a version counter per id). An unreadable file is removed (it would block the list).
  Failures go to `GlobalErrorLog` as type / HTTP status only — never the text. The POST passes
  through `ServerGate` like any write: while the robot runs it waits, which a background send can
  afford. `IHelpFeedbackApi` / `HelpFeedbackRow` / `HelpFeedbackResult` + `ApiClient.HelpFeedback.vb`,
  registered in `Program.vb` like `IMarcajApi`.
- **What counts as a question** (`HelpSearchSession.TrackQuestion`): the text in the box (≥ 2
  characters). Typing on = the same question. It is written to the waiting list when a hit is
  used (hit id + `open` / `tour` for a button — saved BEFORE the action runs), when it is rated,
  and when the box is emptied or the popup / window closes. After a click or a rating, a changed
  text is a new question; so is a text changed after a pause of 3 seconds (the old one is kept,
  with no click). A hit opened from the popup carries the SAME question into the help window
  (`Adopt`), so a rating there updates the popup's row.
- **Rating** (`KBotHelpStars`, new): «A fost util răspunsul?» + five stars under the results, in
  the popup and in the help window's search list; shown only while the box has text; one rating
  per question, a click on the same star takes it back (NULL), a new one replaces it.
- **No personal data**: the box's grey hint is now «Scrie o întrebare (fără nume sau date
  personale)...» and its tooltip says the questions are sent without the name, to improve the
  help.
- **Help version**: `HelpContent/help-version.txt` (one line, the watermark date `2026-09-30`),
  read by `HelpLibrary.Load` into `HelpVersion`; a malformed file is reported and the version is
  sent empty. Moving the watermark now also means updating this file (`HELP_SYSTEM.md`, 0000-22).

### Queries for the «step 4» gate (run by hand on the VPS)

```sql
USE AVACONT_COMUN;

-- 1. Questions with no click (nothing useful found, or not looked at) SELECT Moment, Loc, Intrebare, Rezultate, Nota FROM FX_AjutorIntrebari WHERE Deschis IS NULL ORDER BY Moment DESC;

-- 2. Rated 1 or 2 SELECT Moment, Loc, Intrebare, Deschis, Actiune, Nota FROM FX_AjutorIntrebari WHERE Nota <= 2 ORDER BY Moment DESC;

-- 3. The most repeated questions
SELECT LOWER(Intrebare) AS Intrebare, COUNT(*) AS De_cate_ori,
       SUM(Deschis IS NOT NULL) AS Cu_clic, ROUND(AVG(Nota), 1) AS Nota_medie
FROM FX_AjutorIntrebari GROUP BY LOWER(Intrebare) ORDER BY De_cate_ori DESC LIMIT 50;

-- 4. The gate: «answered well» = rated 4-5, or the FIRST hit was the one used. Target ~85 %.
SELECT COUNT(*) AS Total,
       SUM(COALESCE(Nota, 0) >= 4
           OR (Deschis IS NOT NULL AND Deschis = SUBSTRING_INDEX(Rezultate, '|', 1))) AS Bine,
       ROUND(100 * SUM(COALESCE(Nota, 0) >= 4
           OR (Deschis IS NOT NULL AND Deschis = SUBSTRING_INDEX(Rezultate, '|', 1))) / COUNT(*), 1) AS Procent
FROM FX_AjutorIntrebari;
```

### Files touched

- `sql/0000_21_fx_ajutor_intrebari.sql` (new)
- `PYTHON/routes/help_feedback.py` (new), `PYTHON/main.py` (import + register)
- `src/KBot.Api/IHelpFeedbackApi.vb`, `src/KBot.Api/ApiClient.HelpFeedback.vb` (new)
- `src/KBot.App/Program.vb` (DI), `src/KBot.App/Help/HelpQuestionLog.vb` (new: `HelpQuestion`,
  `HelpQuestionLog`), `HelpSearchSession.vb`, `HelpService.vb` (ctor, `SaveQuestion`, start /
  exit sends), `HelpForm.vb` (session made in the constructor, `TakeOverSearch` adopts),
  `HelpLibrary.vb` (`HelpVersion`)
- `src/KBot.App/HelpContent/help-version.txt` (new)
- `src/KBot.Controls/Popup/KBotHelpStars.vb` (new), `KBotHelpRow.vb` (`CurrentRating`),
  `KBotHelpSearchPanel.vb` / `.Designer.vb` (stars, hint), `KBotHelpPopup.md`
- Rule 0 sweep in files touched: English comments instead of Romanian captions with diacritics in
  `HelpService.vb`, `HelpForm.vb`, `KbotForm.HelpCapture.vb`, `IHelpCaptureNavigator.vb`.

### Test results

- `dotnet build src\KBot.App\KBot.App.vbproj` (Api, Controls, Theming with it): **0 warnings,
  0 errors**.
- `PYTHON\.venv\Scripts\python.exe -m py_compile routes/help_feedback.py main.py`: compiles
  (syntax only; no test written or run — operator's rule).
- `tools\HelpCheck\Check-Help.ps1 -Coverage -Map`: **No errors.** (coverage: `RobotQueueForm`, 0098).
- No app run, no server run.

### Left unverified or deferred — ORDER MATTERS on release

1. **Apply `sql/0000_21_fx_ajutor_intrebari.sql` on the VPS** (AVACONT_COMUN) and **deploy
   `routes/help_feedback.py` + `main.py`** (restart the service) **before** publishing a client
   with this slice. A client that ships first is harmless (the questions wait in the list and the
   send fails quietly, 404 / 500 in `harness_errors.log` every 3 minutes) but noisy.
2. Never run: the route against the real database, the waiting list on disk, the exit send.
3. The «settled after 3 s» rule for a new question is a guess; the data will show whether it
   splits questions too often.
4. `help_feedback.log` sits in the server's working directory next to `api_server.log`; it is
   not read by `/api/logs/*` (on purpose: those routes serve the caller's own lines).

---

<a id="slice-0000-22"></a>

## SLICE-0000-22 — Help content, docs and record for the help assistant

Plan: `docs/PLAN_help_assistant.md` § 0000-22. Closes 0000-18…0000-21.

### What changed and why

#### Help text (Romanian, «tu», users only)

- `contabil/ajutor.md` («Cum folosești ajutorul») rewritten: F1 vs «?»; **Meniul «?»** (search
  box, «Pe ecranul acesta», «Tururi ghidate» with one folder per window, «Deschide ajutorul
  complet (F1)», how it closes); **Cum pui o întrebare** (own words, filler words / diacritics /
  endings do not matter, results as you type, page › section + a few words, click / arrows +
  Enter open the section, what to do when nothing is found) + the one honest sentence: questions
  and ratings are sent without the name to improve the help, no personal data in the box;
  **Butoanele unui rezultat** («Deschide «...»», what happens when the view is not available,
  «Tur ghidat»); **A fost util răspunsul?** (stars 1-5, same star takes it back, rating in the help
  window after reading); **Tururile ghidate** (where they start); **Fereastra de ajutor** (the
  same search box, results in place of the contents, Esc). Each section tagged with the sub-slice
  that decided it (`0000-18` … `0000-21`, plus the older `0000-01` / `0000-04`). `screens:` gains
  `KBotHelpPopup` (F1 inside the popup opens this page); keywords for the typed words
  (intrebare, meniul ajutor, tur ghidat, stele, nota, util).
- `director/index.md`: the note about «?» / F1 says what the popup holds, that the director can
  rate an answer, and the same sentence about sending without the name (tag `0081-06, 0000-20,
  0000-21`); `screens:` + `KBotHelpPopup`.
- `tours/tur-fereastra.md` step «Bara de titlu»: «?» opens a help menu (question box, the
  screen's page, the tours); F1 opens the page directly (tag + `0000-20`).
- `tours/tur-asocieri.md` first step: start the tour from «?» in the Asocieri window's caption bar
  (or F1, then the page's tour link) (tag + `0000-20`).
- Nothing about the inner workings (waiting list, batches, server, weights) is in the help.

#### Docs

- `docs/HELP_SYSTEM.md`: §1 (F1 vs the «?» popup, the search, the question log), §2 (popup
  controls, `help-version.txt`, table + route; engine files `HelpSearch`, `HelpSearchSession`,
  `HelpPopupTours`, `HelpQuestionLog`), §3 (the popup's topic row and tours), §4 procedure
  (`open:` for a new screen, `keywords:` for new words, tours with their own `screens:`, the
  watermark date also in `help-version.txt`), §6 (last header / tour keys added), new §7
  «Search and the question log (maintainer side)».
- `src/KBot.App/HelpContent/README.md`: `open:` (0000-19), `keywords`, tour `screens:` and how the
  popup picks tours (0000-20) — written in those sub-slices, checked here.
- `tools/HelpCheck/Check-Help.ps1`: `open` and tour `screens` (0000-19 / 0000-20), checked here.
- `docs/PLAN_help_assistant.md`: header says it is built and where the differences are written.

#### Record

Worklogs `SLICE-0000-18…22`, rows in `state/KBOT_STATUS_0000-0009.md`, index line «0000-01…22 GATA», watermark moved («… plus asistentul de ajutor (0000-18…0000-22)», `help-version.txt` = `2026-09-30`), open threads for the VPS order and the screen check.

### Captures

New capture tags (the operator shoots them, never Claude):

- `ajutor-meniu` — the «?» popup on the main window (Rezervări selected), empty box.
- `ajutor-cautare` — the popup with «cum trimit un DDF»: results, buttons, stars.
- `ajutor-fereastra-cautare` — the help window with «cum semnez» in the search box (`goto: help`).

The popup closes when the focus leaves it, so the capture tool's own «Fă poza» cannot freeze it: the `prepare:` text says to take it with Win+Shift+S and load it with «Încarcă». To re-shoot: none (no existing picture shows the old search box).

### Files touched

- `src/KBot.App/HelpContent/contabil/ajutor.md`, `director/index.md`, `tours/tur-fereastra.md`,
  `tours/tur-asocieri.md`
- `docs/HELP_SYSTEM.md`, `docs/PLAN_help_assistant.md`
- `docs/worklog/state/KBOT_STATUS_0000-0009.md`, `docs/worklog/KBOT_STATUS.md`

### Test results

- `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors**.
- `tools\HelpCheck\Check-Help.ps1 -Coverage -Map`: **No errors.** 48 topics, 17 tours, 57 capture
  tags. Coverage lists `RobotQueueForm` (slice 0098, another thread; already in «Ajutor de
  actualizat»).
- No tests, no app run.

### De citit de operator

- The whole `contabil.ajutor` page and the director's note: the popup, the stars and the sending
  have not been seen on screen; the text follows the code.
- «Dacă vederea nu există pentru angajamentul selectat, K-BOT îți spune» relies on the existing
  message of `NavigateForCapture` («Vederea «…» nu e disponibilă pentru angajamentul selectat…»).
- The sentence on sending without the name is in two places (`contabil.ajutor`, `director`) and
  in the search box's tooltip.

### Left open (not part of this plan)

- `RobotQueueForm` (0098) still has no topic; the 0098 and «Reanalizează rezervările» notes in
  «Ajutor de actualizat» are untouched.
- The DDL + route must be on the VPS before a client with 0000-21 is published (0000-21 worklog).
