# SLICE-0000-21 — Questions and ratings to the server

Plan: `docs/PLAN_help_assistant.md` § 0000-21 (+ the operator's decisions of 30.09.2026 at its end).

## What changed and why

Every question typed in the help (the «?» popup and the help window) is kept, with the results
shown, the hit used and a 1-5 star rating, so the later «step 4» decision (an embedding model or
not) can be made from real data. **Nothing about who or where** is stored or logged by our code.

### What a row holds (and what it does not)

`FX_AjutorIntrebari` in `AVACONT_COMUN` (name follows the FX_ tables there, checked against
`MariaDB_Schema/AVACONT_COMUN.sql`): `Qid` (random GUID made on the client per question, UNIQUE),
`Intrebare` (trimmed, ≤ 300), `Moment` (UTC), `Parte` (`contabil` / `contabil+avansat` /
`director`), `VersiuneApp`, `VersiuneAjutor` (the watermark date), `Rezultate` (top 5 hit ids
`topicId#section` joined by `|`, ids only), `Deschis` (hit used), `Actiune` (`open` / `tour`),
`Nota` (1-5 or NULL), `Loc` (`popup` / `fereastra`). Index on `Moment` only (+ the unique on `Qid`).
**Not stored:** user, e-mail, unit, DC, IP, machine, session token. Kept for good.

### Server (`PYTHON/`)

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

### Client (`KBot.App`, `KBot.Api`, `KBot.Controls`)

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

## Queries for the «step 4» gate (run by hand on the VPS)

```sql
USE AVACONT_COMUN;

-- 1. Questions with no click (nothing useful found, or not looked at)
SELECT Moment, Loc, Intrebare, Rezultate, Nota
FROM FX_AjutorIntrebari WHERE Deschis IS NULL ORDER BY Moment DESC;

-- 2. Rated 1 or 2
SELECT Moment, Loc, Intrebare, Deschis, Actiune, Nota
FROM FX_AjutorIntrebari WHERE Nota <= 2 ORDER BY Moment DESC;

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

## Files touched

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

## Test results

- `dotnet build src\KBot.App\KBot.App.vbproj` (Api, Controls, Theming with it): **0 warnings,
  0 errors**.
- `PYTHON\.venv\Scripts\python.exe -m py_compile routes/help_feedback.py main.py`: compiles
  (syntax only; no test written or run — operator's rule).
- `tools\HelpCheck\Check-Help.ps1 -Coverage -Map`: **No errors.** (coverage: `RobotQueueForm`, 0098).
- No app run, no server run.

## Left unverified or deferred — ORDER MATTERS on release

1. **Apply `sql/0000_21_fx_ajutor_intrebari.sql` on the VPS** (AVACONT_COMUN) and **deploy
   `routes/help_feedback.py` + `main.py`** (restart the service) **before** publishing a client
   with this slice. A client that ships first is harmless (the questions wait in the list and the
   send fails quietly, 404 / 500 in `harness_errors.log` every 3 minutes) but noisy.
2. Never run: the route against the real database, the waiting list on disk, the exit send.
3. The «settled after 3 s» rule for a new question is a guess; the data will show whether it
   splits questions too often.
4. `help_feedback.log` sits in the server's working directory next to `api_server.log`; it is
   not read by `/api/logs/*` (on purpose: those routes serve the caller's own lines).
