# Plan: help assistant (search, "open it for me", tours, questions to the server)

Hand-over plan, written 30.09.2026. **Built the same day as 0000-18…0000-22** (worklogs
`docs/worklog/SLICE-0000-18-…` to `SLICE-0000-22-…`; what differs from this text is written there,
e.g. the tour folders open inline in the list). All of it is help work, so every
piece is a sub-slice `0000-NN` (next free number: `0000-18`). No generative model. No hosted-model
option (that is deliberately out of scope for now).

## Read first
- `CLAUDE.md`, `docs/worklog/CODE_WORKFLOW.md`, `docs/HELP_SYSTEM.md` (the update procedure and the
  writing rules), `src/KBot.App/HelpContent/README.md` (header / tag syntax).
- Standing rules that bite here: rule 0 (no diacritics in code), English code / Romanian only in
  operator-visible strings, no test code or test runs unless the operator asks, **no git writes**,
  `KBotMessage.Show` for messages, `KBotToolTip` for tooltips, try/catch policy, DPI + designer
  rules for every new control, help is for users only (no inner workings).
- Build only what you touch: `dotnet build src\KBot.App\KBot.App.vbproj` (0 warnings, 0 errors), then
  `tools\HelpCheck\Check-Help.ps1 -Coverage -Map` -> «No errors.».

## What already exists (do not rebuild)
- `HelpLibrary.Search` (`src/KBot.App/Help/HelpLibrary.vb`): ignores diacritics, but EVERY word must
  be found in the topic, whole topics only, no word endings. A real question ("cum trimit un DDF?")
  finds nothing.
- `HelpService.Navigate(target, topicId)` + `KbotForm.NavigateForCapture` (`src/KBot.App/KbotForm.HelpCapture.vb`):
  already opens `view:<key>`, `menu:<key>`, `setari:<page>`, `help` / `help:<id>`. Today only captures
  and tour steps use it; a topic cannot carry a target.
- Tours (`HelpContent/tours/*.md`, `HelpTour.vb`, `HelpTourRunner.vb`): header has `topic:`, and a
  topic has `screens:`. So tour -> screens is derivable through the tour's topic.
- «?» on every caption bar: `KBotCaptionBar.HelpButtonClicked` -> `KBotHelp.Request(Me)` ->
  `IKBotHelpProvider.ShowHelp(origin)` (`src/KBot.Theming/KBotHelp.vb`) -> opens the help window.
  F1 goes to the same place and must keep doing so.
- `HelpService.VisibleParts()` already decides who sees contabil / avansat / director. Everything
  below (search hits, actions, tours) must respect it.

---

## 0000-18 — Search that understands a question (no model)
In `HelpLibrary` (and the results page in `HelpHtml`):
1. Drop filler words (Romanian: cum, se, un, o, la, de, pot, sa, unde, ce, etc. — keep the list in
   one place).
2. Match on word stems (folded, endings trimmed, e.g. prefix of the first ~5 letters) so «trimit»
   meets «trimiterea».
3. Score by how many words match, not "all words must match". Title > keywords > body, as now.
4. Search per SECTION (each `## ` heading), not only per topic. A hit = topic + section heading +
   short snippet + an anchor so the page opens at that section.
5. `keywords:` stays the place for synonyms and old Access names; add the obvious ones while doing
   content.
6. Keep it fast and allocation-light: fold and split each section once when the library loads.
Result model (used by the popup, the help window and the question log): ordered list of
`HelpHit(topicId, sectionAnchor, title, snippet, score)`.

## 0000-19 — `open:` actions on topics
1. New topic header key `open:` with the same values as a capture's `goto:` (`view:ddf`,
   `menu:clasificatii`, `setari:tema`). Follow `HELP_SYSTEM.md` §6 "New header key": `HelpLibrary.Parse`,
   `HelpContent/README.md`, `$HeaderKeys` in `Check-Help.ps1`, and make the checker validate the value
   against the same pattern as `goto:` (`$GotoPrefix`).
2. A hit whose topic has `open:` gets a button «Deschide <screen>» that calls `HelpService.Navigate`.
   A hit whose topic has a tour also gets «Tur ghidat». Unavailable view -> the existing message
   from `NavigateForCapture` is shown (through `KBotMessage.Show`).
3. Content: add `open:` to every topic that has a real screen (about 20).
4. Director login sees only part 3; `VisibleParts` already enforces it — do not bypass it.

## 0000-20 — The «?» popup
Goal: clicking «?» opens a custom popup (not the help window) with: a search bar, the topic of the
current screen, and the tours. F1 keeps opening the help window on the topic of the focused control.
1. **Seam.** `KBotCaptionBar.HelpButtonClicked` currently calls `KBotHelp.Request(Me)`. Add a second
   seam in `KBotHelp` / `IKBotHelpProvider` (for example `ShowHelpMenu(origin, anchorScreenRect)`);
   KBot.Controls cannot reference KBot.App, so the popup is fed through an interface implemented by
   `HelpService`. F1 and the old `ShowHelp` path stay.
2. **Control.** New popup in `KBot.Controls/Popup/` (look at the existing popup family first and reuse
   it), a search box (reuse `KBot.Controls/TextField/`), a result list, a tours area, and a last line
   «Deschide ajutorul complet». It owns child controls, so it implements `IThemedControl`; zero
   hardcoded colours; DPI and designer rules from `CLAUDE.md`; tooltips via `KBotToolTip`.
   Typing searches LOCALLY (instant, works offline); Enter / click on a hit opens the help window on
   that section; the hit's buttons (0000-19) are in the list. Esc or click-away closes it.
3. **Tours in the popup.**
   - Collect the visible application windows in z-order (top first). Ignore: minimized, not
     `Enabled` (a window behind a modal dialog), the help window itself.
   - For each, find the tours whose topic's `screens:` match the window's type name or one of its
     controls that is visible; for the main window also the selected view (`navViews.SelectedKey`),
     so the Rezervari view offers `tur-rezervari`. Filter by `VisibleParts`.
   - **One window has tours** -> list them directly. **More than one window has tours** -> one folder
     (sub-menu) per window, titled with that window's caption, each holding that window's tours.
   - A tour starts through the existing runner (`HelpTourRunner`); its step `goto:` / `target:` work
     as today. If the topic mapping is not enough to tell which window a tour belongs to, add an
     optional `screens:` key to the tour header (same values as a topic's) — same §6 procedure.
   - Make sure every view / form that should have a tour really has one (`-Map` shows the tree);
     missing ones are content work in this sub-slice.
4. The popup must also exist in the help window (see next): the same search control/logic, not a copy.

## 0000-21 — Questions and ratings to the server
Design rules (operator, 30.09.2026): every question goes to the server; **nothing about who or where
is recorded — not in the table and not in any log**; the answer and a 1-5 star rating are saved so
the later model decision (embedding model, "step 4") can be made from real data; do not burden the
server.

**What is stored** (one row per question, keyed by a random id made on the client — a fresh GUID per
question, not derived from the user, the unit, the PC or the session):
`qid`, question text (trimmed, max ~300 chars), UTC time, part that was visible (contabil / avansat /
director), app version, help version (the watermark date), the ordered top hits that were shown
(`topicId#section`, at most 5 — ids only, never the text), which hit was opened (if any), which
action button was used (open / tour), rating 1-5 or NULL, where it was asked («popup» / «fereastra»).
**Not stored:** user id, email, unit id, DC, IP, machine name, session token.

**Client (`KBot.App`)**
1. After a search the question is written to a small local outbox (SQLite via `KBot.LocalStore`, or
   a small file — pick what fits; it must survive a crash and an offline start). The UI never waits.
2. A background flush sends the outbox in **batches** (e.g. on idle every few minutes, on app close,
   and when it holds N rows), retries later on failure, deletes rows only after the server said OK.
   Fire-and-forget; a failure is logged to `GlobalErrorLog` WITHOUT the question text.
3. Rating: five stars under the results, in the popup and on the help window's results page, text
   «A fost util răspunsul?». One rating per `qid`; changing it re-sends the row (upsert). Also record
   which hit was opened — it is free signal.
4. The box tells the operator not to type personal data (short hint in the search box) — free text
   is the one place a name or CUI could leak in.

**Server (`PYTHON/`)**
1. One new table (DDL in `sql/` and applied on the VPS BEFORE the client ships; check
   `MariaDB_Schema/` for the real schema and which database holds shared tables — ask if the folder
   is missing). `qid` is UNIQUE; keep it narrow; index by time only.
2. One route, e.g. `POST /help/feedback`, in a new `PYTHON/routes/` file, registered like the others.
   Body = list of rows (cap ~50 rows / ~64 KB), **idempotent upsert by `qid`**, validate every field
   (lengths, rating 1-5, known ids only as strings). Bearer auth is fine (`require_session`, not the
   X-Api-Key guard) but the route must NOT read the user / unit into the row or into any log line.
3. **Logging:** the route's own code must not log the user, unit, IP or question text. The VPS
   access log is out of scope (see "Server access log" below).
4. A plain SQL query (documented in the worklog) that lists: questions with no click, rating <= 2,
   and top repeated questions. That is what the later "step 4" gate reads (target: ~85% of questions
   answered well = rated >= 4 or first hit opened).
5. Retention: rows are kept for good (operator's decision); table in `AVACONT_COMUN`.

## 0000-22 — Help content, docs, record
1. Write the help for what the operator now sees (Romanian, «tu», users only): the «?» popup, asking a
   question, the buttons on a result, the star rating, how to start a tour from the popup. One honest
   sentence that questions written there are sent anonymously to improve the help (no mention of the
   inner workings). Every new / changed section carries its `<!-- slice: ... -->` tag
   (`0000-18`…`0000-21`).
2. Update `HelpContent/README.md` (`open:`, optional tour `screens:`), `docs/HELP_SYSTEM.md` (new
   files, the popup, the question log — maintainer side), `Check-Help.ps1`.
3. Record as `docs/worklog/SLICE-0000-NN-<slug>.md` per sub-slice, rows in
   `state/KBOT_STATUS_0000-0009.md`, update the index line in `KBOT_STATUS.md`, move the watermark.
   Captures of the new popup: add capture tags with ids and list them under «Capturi de refăcut» —
   the operator shoots them, never Claude.

## Order and dependencies
0000-18 and 0000-19 first (engine, no UI). 0000-20 needs both. 0000-21 needs 0000-18's hit model and
0000-20's UI for the stars; the server table + route can be built in parallel. 0000-22 closes.
Each sub-slice ends with a clean build + `Check-Help.ps1`, no app runs, no tests, no git writes.

## Later, not part of this plan (gated on the data from 0000-21)
"Step 4": a small multilingual embedding model (~100-150 MB, CPU, ONNX Runtime, vectors computed at
build time) — only if the ratings / click data show plain search is below target. Before that,
check how `push-update.ps1` and the updater package files so the model is not re-sent with each update.

## Decided by the operator (30.09.2026)
- The table lives in `AVACONT_COMUN`. Table name: the instance's choice, following the naming there.
- Rows are kept for good (no purge job for now).
- The "outbox" is just a waiting list on the client PC (questions are written there first and sent in
  the background). Its storage (small file vs SQLite) and flush cadence are the instance's technical
  choice: take the simplest that survives a crash and an offline start. Do not ask the operator.

## Server access log: not a concern for now (operator, 30.09.2026)
The web server's own request log (nginx / Flask console) may keep showing which PC called this route.
The operator accepts that for now. Do NOT check or change the VPS logging, and do not stop for it.
The rule that still holds is in OUR code and table: no user, unit, IP or machine in the row, and none
in any log line our code writes (`GlobalErrorLog` on the client, Flask app logging in the route).
Nothing to ask the operator.
