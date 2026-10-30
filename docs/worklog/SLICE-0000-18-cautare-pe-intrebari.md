# SLICE-0000-18…22 — Istoric al asistentului de ajutor

Etapele din 30.09.2026 sunt păstrate integral, cu propunerea în [planul inițial](../PLAN_help_assistant.md) și întreținerea în [HELP_SYSTEM](../HELP_SYSTEM.md). Ordinea contează: 18 introduce pagina HTML de rezultate; 20 o înlocuiește cu lista comună ferestrei și meniului; 21 adaugă întrebări și stele, iar 22 încheie textul/documentarea. Dosarele meniului se deschid inline, față de submeniul propus inițial. Ghidul de întreținere și [istoricul tururilor/ferestrei](SLICE-0000-23-tururi-pe-butoane-istoric-capturi-estompate.md) consemnează și schimbări ulterioare (0000-27: tururi doar pentru fereastra al cărei «?» a fost apăsat; 0000-32: cartea continuă); etapele de mai jos nu sunt descrierea tuturor versiunilor ulterioare. SQL-ul este exemplu istoric, neexecutat în consolidare. DDL/ruta nerulate și probele nevăzute pe ecran rămân astfel la reperele înregistrate.

[0000-18](#slice-0000-18) · [0000-19](#slice-0000-19) · [0000-20](#slice-0000-20) · [0000-21](#slice-0000-21) · [0000-22](#slice-0000-22)

## SLICE-0000-18 — Search that understands a typed question (no model) <a id="slice-0000-18"></a>

Plan: `docs/PLAN_help_assistant.md` § 0000-18.

### What changed and why

`HelpLibrary.Search` formerly required every query word, matched whole topics and knew no endings, failing questions such as «cum trimit un DDF?». It now:

1. **Drops filler words** — one list, `HelpSearch.StopWords` (folded Romanian: cum, se, un, o, la, de, pot, sa, unde, ce, care, pentru, ...). Words of one letter are dropped too.
2. **Stems:** fold lowercase/no diacritics, one character per character; strip a common Romanian ending (`-urilor`, `-ilor`, `-ului`, `-ii`, `-ea`, `-a`, `-e`, `-i`...), take 5 letters. Trimit/trimiterea → trimi, plăți/plata → plat. Four-letter stem prefixes score half, matching semnez/semnare.
3. **Score:** matched words × 100 + weight (capped at 99). Per-word full stem/4-letter weights: title 10/5, keywords 6/3, heading 8/4, text 2/1. With ≥3 query words require at least half in a section; with 1–2, one match suffices.
4. **Sections:** preamble before first `## `, then each heading/text. Hit = topic, heading, ~170-character snippet around first match, anchor. Maximum 3/topic; additional hits must match their own section, not only the topic title.
5. `keywords:` is unchanged as the place for synonyms (content in 0000-19 / 0000-22).
6. **Load-time index:** `HelpSearch.Index`, from `HelpLibrary.Parse`, splits sections and stores folded/stemmed title/keywords/headings/text hash sets. Queries fold only their words and look up sets; reading-order tie breaks are computed once/part.

Result model: `HelpHit(TopicId, SectionAnchor, Title, SectionTitle, Snippet, Score)` + `Key` (`topicId#anchor`, the id the question log will store in 0000-21).

**Anchors:** `HelpSearch.AnchorFor(heading)` joins folded words (`pagina-forexe-certificatul-memorat`), unique per topic with -2/-3. `HelpHtml.AddSectionAnchors` assigns the same level-2 ids from heading text/order, without stored mapping. Printed manual omits ids to avoid cross-topic clashes.

**Help window:** `HelpForm.ShowTopic(id, sectionAnchor)`, page key `t:<id>#<anchor>` retained in history/Înapoi. `DocumentCompleted` scrolls; busy browser keeps the anchor in pending page. `HelpHtml.SearchPage` lists Topic › Section/snippet with `topic:<id>#<anchor>`; `topic:id#anchor` also works inside topics.

### Files touched

`src/KBot.App/Help/HelpSearch.vb` (new): `HelpHit`, `HelpTermSet`, `HelpQueryTerm`, `HelpSection`, `HelpSearch` (fold, words, stop words, stem, plain text, anchors, index, search, snippet); `src/KBot.App/Help/HelpTopic.vb`: `Sections`, `TitleTerms`, `KeywordTerms` (Friend); `src/KBot.App/Help/HelpLibrary.vb`: `Parse` indexes; `Search` returns `List(Of HelpHit)`; old `Fold` removed (only the old search used it); `src/KBot.App/Help/HelpHtml.vb`: `SearchPage(query, hits)`, heading anchors, `.hit` style, `topic:id#anchor` links; `src/KBot.App/Help/HelpForm.vb`: section in the page key, scroll to the section

### Test results

- `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors**.
- `tools\HelpCheck\Check-Help.ps1 -Coverage -Map`: **No errors.** Coverage lists `RobotQueueForm` (slice 0098, another thread's window; already noted in 0000-17).
- No tests, no app run (operator's rule).

### Left unverified or deferred

- The ranking was never run on real questions: the weights are a first guess. 0000-21 records every question and the hit opened, so they can be tuned from data.
- The scroll to a section relies on `HtmlElement.ScrollIntoView` in the WebBrowser control; not seen on screen.
- The help text describing the search is updated in 0000-22 (planned there, tag `0000-18`).

---

## SLICE-0000-19 — `open:` actions on topics <a id="slice-0000-19"></a>

Plan: `docs/PLAN_help_assistant.md` § 0000-19. Engine + content, no UI (the buttons are drawn by the «?» popup and the help window's search list in 0000-20).

### What changed and why

A topic could not name its screen, so a search result could only open the help page. Now:

1. **Header `open:`** uses capture goto values (`view:ddf`, `menu:clasificatii`, `setari:tema`). Following HELP_SYSTEM §6: `HelpLibrary.Parse` validates against `HelpLibrary.GotoPattern`, matching `$GotoPrefix`; model `HelpTopic.Open`; `Check-Help.ps1` adds `$HeaderKeys` open/value validation. `HelpContent/README.md` documents it and keywords after ending-insensitive search.
2. **Hit actions:** `HelpLibrary.Search` fills OpenTarget from topic open, TourId/TourTitle from the first tour in a VISIBLE part (`HelpLibrary.TourOf`). `HelpService.OpenButtonText(target)` uses new `IHelpCaptureNavigator.TargetCaption`: KbotForm reads view/menu text, strips `<b>`/«(!)», uses Setări for settings, giving «Deschide «Rezervări»»/«Deschide «Clasificații bugetare»»/«Deschide «Setări»»; no main window → «Deschide ecranul». `HelpService.OpenScreen(target, topicId, owner)` calls Navigate; the NavigateForCapture message for an unavailable view or absent main window is shown through KBotMessage.Show.
3. **Content**: `open:` on the 25 topics that describe one real screen (values taken from each topic's own capture `goto:`):
   - views: `contabil.forexe.browser` (browser), `contabil.vederi.sumar / istoric / rezervari / receptii / plati / extrase / ord`, `contabil.ord.generare` and `contabil.ord.editor` (ord), `contabil.ddf`, `contabil.ddf.editor / semnare / trimitere` (ddf), `contabil.ddf.rezervare` (rezervari), `contabil.notecab` (notecab);
   - menu: `contabil.ddf.nou` (angajament_nou), `contabil.nomenclatoare.clasificatii`, `contabil.nomenclatoare.parteneri`;
   - Setări: `contabil.setari` and `avansat.documente` (aplicatie), `avansat.pagina`, `avansat.tema`, `avansat.foldere`, `avansat.jurnale` (jurnal). Left without `open:` on purpose: chapter and concept pages (`contabil`, `contabil.forexe`, `contabil.vederi`, `contabil.asocieri*`, `contabil.liste`, `contabil.ddf.mf`...), the main window itself, and the director's part (his window has no `goto:` navigation). Obvious synonyms added to `keywords:` (0000-18 item 5): DDF «document de fundamentare, angajament bugetar»; semnarea «semnatura electronica, certificat digital, token»; Plăți «op, banca»; Recepții «factura, facturi»; Parteneri «beneficiari»; Clasificații «clasificatie bugetara, indicatori».
4. **Director**: hits only come from `VisibleParts()` (a director sees part 3 only) and `TourOf` takes the same parts, so nothing here bypasses it.

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
- `tools\HelpCheck\Check-Help.ps1 -Coverage -Map`: **No errors.** (coverage: `RobotQueueForm`, 0098, unchanged).
- No tests, no app run.

### Left unverified or deferred

- The buttons themselves: 0000-20. Nothing here is visible to the operator yet.
- `setari:jurnal` / `setari:pagina` / `setari:tema` / `setari:foldere` open pages that exist only with the advanced options on; their topics are in part 2, which is shown only then.

---

## SLICE-0000-20 — The «?» popup <a id="slice-0000-20"></a>

Plan: `docs/PLAN_help_assistant.md` § 0000-20.

### What changed and why

Caption «?» now opens a search/topic/tours popup under the button, replacing direct help-window opening. F1 still opens the focused-control topic. Popup and window share the search control.

1. **Seam.** `IKBotHelpProvider.ShowHelpMenu(origin, anchorScreenRect)` + `KBotHelp.RequestMenu` (KBot.Theming). `KBotCaptionBar.HelpButtonClicked` calls `RequestMenu` with the button's screen rectangle. `ShowHelp` / `KBotHelp.Request` (F1) stay as they were.
2. **Controls** (`KBot.Controls/Popup/`, `KBotHelpPopup.md`, CONTROLS row): reviewed CustomPopup (flat painted menu, no textbox/submenus) and KBotDropDownMenu (nonactivating, no search keyboard). Neither fits; new family reuses KBotTextField, KBotScrollBar, KBotToolTip.ShowAt, ThemeShapes, KBotThemedForm/UserControl:
   - `KBotHelpRow` + `IKBotHelpSearchSource` — the rows and the interface the application implements (the controls cannot reference KBot.App);
   - `KBotHelpList` — painted list: headers, hits (title › section, two lines of text, «Deschide «...»» / «Tur ghidat» buttons), the screen's topic, tours, folders of tours; own scroll bar; hover tooltips;
   - `KBotHelpSearchPanel` (UserControl, designer) — box + list; types → local search after 150 ms; Up / Down / Enter drive the list; Esc raised to the host;
   - `KBotHelpPopup` (Form/Designer): panel and «Deschide ajutorul complet (F1)» footer, below/above button by space; closes on Esc, outside click or row handoff deferred through BeginInvoke. All controls use IThemedControl/palette fields (no host-designer serialization), logical pixels scaled at layout, AutoScaleMode.Dpi 96/96.
3. **Tours** (`HelpPopupTours`): EnumWindows z-order, excluding minimized/disabled/help windows. Ownership requires a VISIBLE screen (form/control/selected main view) through `HelpTourRunner.FindInWindow`, split from FindTarget; hidden views do not qualify. Offer each tour once on its top eligible window, only VisibleParts. One window → direct list; several → caption folders, top open. Optional tour `screens:` (`HelpTour.Screens`, `$TourKeys`/check/README) serves `tur-avansat` (`screens: SetariForm`), whose topic names only a checkbox.
4. **Actions** (`HelpSearchSession` per popup/fereastra panel): hit → section via HelpService.ShowHit, transferring popup query/results through HelpForm.TakeOverSearch; Deschide → OpenScreen (19); Tur ghidat/tour row → StartTour; topic row → topic; full help → F1 target for that window. No hits gives a short note row.
5. **Window:** HelpForm.Designer replaces txtCauta with pnlCautare/KBotHelpSearchPanel. Empty: box above contents; results: whole left side; clear/Esc restores contents. Removes HelpHtml.SearchPage and s: HTML results pages in favour of the list. Updates start-page F1/«?»/search sentence.
6. **Tours, content**: five new tours for screens that had none — `tur-sumar`, `tur-istoric`, `tur-notecab`, `tur-clasificatii`, `tur-parteneri` (text only from what their topics already say; targets checked against the designers). Left without a tour on purpose: the DDF and ORD editors (operator's decision in 0000-07 / 0000-11), `BrowserView` (its buttons are in `tur-forexe`), `RobotQueueForm` (0098, has no topic yet).

### Files touched

- `src/KBot.Theming/KBotHelp.vb`
- `src/KBot.Controls/CaptionBar/KBotCaptionBar.HelpButton.vb`
- `src/KBot.Controls/Popup/KBotHelpRow.vb`, `KBotHelpList.vb`, `KBotHelpSearchPanel.vb`, `KBotHelpSearchPanel.Designer.vb`, `KBotHelpPopup.vb`, `KBotHelpPopup.Designer.vb`, `KBotHelpPopup.md` (new); `src/KBot.Controls/CONTROLS.md`
- `src/KBot.App/Help/HelpService.vb` (`ShowHelpMenu`, `PopupHomeRows`, `ShowHit`, `MainWindow`, `ShowHelpForKeys`, `TopicForKeys`)
- `src/KBot.App/Help/HelpSearchSession.vb`, `HelpPopupTours.vb` (new)
- `src/KBot.App/Help/HelpTour.vb` (`Screens`), `HelpTourRunner.vb` (`FindInWindow`)
- `src/KBot.App/Help/HelpForm.vb`, `HelpForm.Designer.vb`, `HelpHtml.vb`
- `src/KBot.App/HelpContent/README.md`; `tours/tur-avansat.md`; new `tours/tur-sumar.md`, `tur-istoric.md`, `tur-notecab.md`, `tur-clasificatii.md`, `tur-parteneri.md`
- `tools/HelpCheck/Check-Help.ps1`

### Test results

- `dotnet build src\KBot.App\KBot.App.vbproj` (builds Theming and Controls too): **0 warnings, 0 errors**.
- `tools\HelpCheck\Check-Help.ps1 -Coverage -Map`: **No errors.** 48 topics, 17 tours, 54 capture tags; coverage `RobotQueueForm` (0098) as before.
- No tests, no app run: the popup was never seen on screen.

### Left unverified or deferred

- Nothing here was rendered: placement under the button, the list's row heights at 125 / 150 %, the folder toggle, the tooltips, the deferred close when a tour bubble or a message takes the focus. The operator should open «?» on the main window (one window, tours listed) and with Setări open over it (two folders).
- The popup's stars and the question log: 0000-21. The help text for the popup: 0000-22.
- Folders open inline in the list rather than as a cascading submenu (simpler keyboard, no second activating window); say if a real submenu is wanted.

---

## SLICE-0000-21 — Questions and ratings to the server <a id="slice-0000-21"></a>

Plan: `docs/PLAN_help_assistant.md` § 0000-21 (+ the operator's decisions of 30.09.2026 at its end).

### What changed and why

Every question typed in the help (the «?» popup and the help window) is kept, with the results shown, the hit used and a 1-5 star rating, so the later «step 4» decision (an embedding model or not) can be made from real data. **Nothing about who or where** is stored or logged by our code.

#### What a row holds (and what it does not)

`FX_AjutorIntrebari` in `AVACONT_COMUN` (name follows the FX_ tables there, checked against `MariaDB_Schema/AVACONT_COMUN.sql`): `Qid` (random GUID made on the client per question, UNIQUE), `Intrebare` (trimmed, ≤ 300), `Moment` (UTC), `Parte` (`contabil` / `contabil+avansat` / `director`), `VersiuneApp`, `VersiuneAjutor` (the watermark date), `Rezultate` (top 5 hit ids `topicId#section` joined by `|`, ids only), `Deschis` (hit used), `Actiune` (`open` / `tour`), `Nota` (1-5 or NULL), `Loc` (`popup` / `fereastra`). Index on `Moment` only (+ the unique on `Qid`). **Not stored:** user, e-mail, unit, DC, IP, machine, session token. Kept for good.

#### Server (`PYTHON/`)

- `sql/0000_21_fx_ajutor_intrebari.sql` — the DDL. The service account already has INSERT / UPDATE on `AVACONT_COMUN.*` (`sql/0076_avacont_drepturi.sql`), no GRANT.
- `routes/help_feedback.py` — `POST /api/help/feedback`, registered in `main.py`. Bearer guard (`require_session`, not the X-Api-Key one). Body `{"rows": [...]}`, at most 50 rows / 64 KB (413 / 400 otherwise). Every field validated (GUID form, text length, UTC form, parts, version forms, ≤ 5 hit ids of the id pattern, action / rating / place from their small sets); a bad row is counted in `rejected` and skipped, the rest saved in ONE `executemany` upsert (`INSERT ... ON DUPLICATE KEY UPDATE`, idempotent by `Qid`). Answer `{"saved", "rejected"}`.
- **Logging.** The root logger's handlers stamp every line with the caller's IP and session tag (`utils/logger.py`), so the route: (1) has its own logger `help_feedback` → `help_feedback.log`, `propagate = False`, format without IP / tag, lines with counts and error TYPES / errno only (a driver message can quote a value); (2) opens its own connection instead of `get_kbot_connection`, which logs a failed connect through the root logger; (3) drops `g.session` / `g.session_token` as soon as the guard has passed, so no stray line (a library's) can carry the user. The guard's own `AUTH_401` lines on a bad token are unchanged (they carry a token prefix, no question) and the VPS access log is out of scope (operator).

#### Client (`KBot.App`, `KBot.Api`, `KBot.Controls`)

- **Waiting list** (`HelpQuestionLog`): one small JSON file per question, named by its id, in `%APPDATA%\AVACONT\KBot\HelpOutbox\` (per user, survives updates). Written through a temp file + rename, so a crash leaves a whole file; still there after an offline start. A later change (click, rating) rewrites the same file. Chosen over SQLite: nothing to open or migrate, one file = one row, and the upsert on the server makes a resend harmless.
- **Sending:** Task.Run, oldest-first batches ≤50 at start (earlier offline backlog), every 3min, at 10 waiting, and Application.ApplicationExit (≤3s); only with login token. Delete only after server 200 and unchanged per-id version; remove unreadable files to avoid blockage. GlobalErrorLog records failure type/HTTP status, never text. POST uses ServerGate, waiting while the robot runs. IHelpFeedbackApi/HelpFeedbackRow/HelpFeedbackResult and ApiClient.HelpFeedback.vb are registered in Program.vb like IMarcajApi.
- **Question** (`HelpSearchSession.TrackQuestion`): box text ≥2 characters; continued typing keeps it. Persist before a used-hit action (id/open/tour), on rating, clear or popup/window close. Changed text after click/rating creates a new question; so does a change after a 3s pause, keeping the previous unclicked question. Popup hit transfers the same question through Adopt; rating in the window updates its row.
- **Rating** (`KBotHelpStars`, new): «A fost util răspunsul?» with 5 stars below popup/window results, only with box text. One rating/question; same star clears to NULL, another replaces it.
- **No personal data**: the box's grey hint is now «Scrie o întrebare (fără nume sau date personale)...» and its tooltip says the questions are sent without the name, to improve the help.
- **Help version:** one-line watermark `2026-09-30` in HelpContent/help-version.txt, loaded as HelpVersion by HelpLibrary.Load. Malformed file is reported/sent empty; watermark updates must also update it (HELP_SYSTEM, 22).

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

`sql/0000_21_fx_ajutor_intrebari.sql` (new); `PYTHON/routes/help_feedback.py` (new), `PYTHON/main.py` (import + register); `src/KBot.Api/IHelpFeedbackApi.vb`, `src/KBot.Api/ApiClient.HelpFeedback.vb` (new); `src/KBot.App/Program.vb` (DI), `src/KBot.App/Help/HelpQuestionLog.vb` (new: `HelpQuestion`, `HelpQuestionLog`), `HelpSearchSession.vb`, `HelpService.vb` (ctor, `SaveQuestion`, start / exit sends), `HelpForm.vb` (session made in the constructor, `TakeOverSearch` adopts), `HelpLibrary.vb` (`HelpVersion`); `src/KBot.App/HelpContent/help-version.txt` (new); `src/KBot.Controls/Popup/KBotHelpStars.vb` (new), `KBotHelpRow.vb` (`CurrentRating`), `KBotHelpSearchPanel.vb` / `.Designer.vb` (stars, hint), `KBotHelpPopup.md`; Rule 0 sweep in files touched: English comments instead of Romanian captions with diacritics in `HelpService.vb`, `HelpForm.vb`, `KbotForm.HelpCapture.vb`, `IHelpCaptureNavigator.vb`.

### Test results

- `dotnet build src\KBot.App\KBot.App.vbproj` (Api, Controls, Theming with it): **0 warnings, 0 errors**.
- `PYTHON\.venv\Scripts\python.exe -m py_compile routes/help_feedback.py main.py`: compiles (syntax only; no test written or run — operator's rule).
- `tools\HelpCheck\Check-Help.ps1 -Coverage -Map`: **No errors.** (coverage: `RobotQueueForm`, 0098).
- No app run, no server run.

### Left unverified or deferred — ORDER MATTERS on release

1. **Apply `sql/0000_21_fx_ajutor_intrebari.sql` on the VPS** (AVACONT_COMUN) and **deploy `routes/help_feedback.py` + `main.py`** (restart the service) **before** publishing a client with this slice. A client that ships first is harmless (the questions wait in the list and the send fails quietly, 404 / 500 in `harness_errors.log` every 3 minutes) but noisy.
2. Never run: the route against the real database, the waiting list on disk, the exit send.
3. The «settled after 3 s» rule for a new question is a guess; the data will show whether it splits questions too often.
4. `help_feedback.log` sits in the server's working directory next to `api_server.log`; it is not read by `/api/logs/*` (on purpose: those routes serve the caller's own lines).

---

## SLICE-0000-22 — Help content, docs and record for the help assistant <a id="slice-0000-22"></a>

Plan: `docs/PLAN_help_assistant.md` § 0000-22. Closes 0000-18…0000-21.

### What changed and why

#### Help text (Romanian, «tu», users only)

- `contabil/ajutor.md` («Cum folosești ajutorul») rewritten: F1 vs «?»; **Meniul «?»** (search box, «Pe ecranul acesta», «Tururi ghidate» with one folder per window, «Deschide ajutorul complet (F1)», how it closes); **Cum pui o întrebare** (own words, filler words / diacritics / endings do not matter, results as you type, page › section + a few words, click / arrows + Enter open the section, what to do when nothing is found) + the one honest sentence: questions and ratings are sent without the name to improve the help, no personal data in the box; **Butoanele unui rezultat** («Deschide «...»», what happens when the view is not available, «Tur ghidat»); **A fost util răspunsul?** (stars 1-5, same star takes it back, rating in the help window after reading); **Tururile ghidate** (where they start); **Fereastra de ajutor** (the same search box, results in place of the contents, Esc). Each section tagged with the sub-slice that decided it (`0000-18` … `0000-21`, plus the older `0000-01` / `0000-04`). `screens:` gains `KBotHelpPopup` (F1 inside the popup opens this page); keywords for the typed words (intrebare, meniul ajutor, tur ghidat, stele, nota, util).
- `director/index.md`: the note about «?» / F1 says what the popup holds, that the director can rate an answer, and the same sentence about sending without the name (tag `0081-06, 0000-20, 0000-21`); `screens:` + `KBotHelpPopup`.
- `tours/tur-fereastra.md` step «Bara de titlu»: «?» opens a help menu (question box, the screen's page, the tours); F1 opens the page directly (tag + `0000-20`).
- `tours/tur-asocieri.md` first step: start the tour from «?» in the Asocieri window's caption bar (or F1, then the page's tour link) (tag + `0000-20`).
- Nothing about the inner workings (waiting list, batches, server, weights) is in the help.

#### Docs

- `docs/HELP_SYSTEM.md`: §1 (F1 vs the «?» popup, the search, the question log), §2 (popup controls, `help-version.txt`, table + route; engine files `HelpSearch`, `HelpSearchSession`, `HelpPopupTours`, `HelpQuestionLog`), §3 (the popup's topic row and tours), §4 procedure (`open:` for a new screen, `keywords:` for new words, tours with their own `screens:`, the watermark date also in `help-version.txt`), §6 (last header / tour keys added), new §7 «Search and the question log (maintainer side)».
- `src/KBot.App/HelpContent/README.md`: `open:` (0000-19), `keywords`, tour `screens:` and how the popup picks tours (0000-20) — written in those sub-slices, checked here.
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

`src/KBot.App/HelpContent/contabil/ajutor.md`, `director/index.md`, `tours/tur-fereastra.md`, `tours/tur-asocieri.md`; `docs/HELP_SYSTEM.md`, `docs/PLAN_help_assistant.md`; `docs/worklog/state/KBOT_STATUS_0000-0009.md`, `docs/worklog/KBOT_STATUS.md`

### Test results

- `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors**.
- `tools\HelpCheck\Check-Help.ps1 -Coverage -Map`: **No errors.** 48 topics, 17 tours, 57 capture tags. Coverage lists `RobotQueueForm` (slice 0098, another thread; already in «Ajutor de actualizat»).
- No tests, no app run.

### De citit de operator

- The whole `contabil.ajutor` page and the director's note: the popup, the stars and the sending have not been seen on screen; the text follows the code.
- «Dacă vederea nu există pentru angajamentul selectat, K-BOT îți spune» relies on the existing message of `NavigateForCapture` («Vederea «…» nu e disponibilă pentru angajamentul selectat…»).
- The sentence on sending without the name is in two places (`contabil.ajutor`, `director`) and in the search box's tooltip.

### Left open (not part of this plan)

- `RobotQueueForm` (0098) still has no topic; the 0098 and «Reanalizează rezervările» notes in «Ajutor de actualizat» are untouched.
- The DDL + route must be on the VPS before a client with 0000-21 is published (0000-21 worklog).
