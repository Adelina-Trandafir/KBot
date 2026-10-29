# SLICE-0089 / 01 — Jurnalele serverului pe utilizator și sesiune

Reunire documentară D001-25. Etapa inițială aduce marcarea și citirea jurnalelor; 01 le condiționează ulterior de opțiunile avansate. Exportul pe zile deschise, selecția sesiunilor, rotațiile și limitele sunt păstrate. «done» din registru rămâne un statut istoric, fără a certifica rularea pe VPS sau aspectul paginii.

[0089-jurnale](#slice-0089-baza) · [0089-01](#slice-0089-01)

## SLICE 0089 — Server journals in K-BOT, per user and per session <a id="slice-0089-baza"></a>

**Date:** 28.09.2026. **Request (operator):** wire the Flask `api_server.log` and `forexe_timing.log` into the K-BOT journal page (`SetariJurnalView`, combo `CmbTipJurnal`), tied to the current user; show only the user's **last 3 sessions**, with an option for more; split the server journal into Error / Warning / Info.

**Operator corrections during the slice (same day):**
1. `api_server.log` lines without `[forexe` are ignored — they belong to the old Access/VBA path;
2. the match is `[forexe` followed by `]` or `.` — some lines are just `[forexe]`;
3. split the log: the old VBA path writes its own file, `api_server_vba.log`.
4. the day bands of the grid show only the date (not «Ora: …»), and every day starts collapsed;
5. «Exportă» exports only the expanded days; none expanded = nothing exported (29.09.2026).

### The problem

`api_server.log` has IP and usually unit, but no user; `[forexe.ord_edit] genereaza: …` and tracebacks have neither. Operators can share an IP, so existing lines cannot identify the user and must be tagged at write time. Timing blocks usually have `dc=` / `user=`, but no login/session identifier.

### What changed

#### Server (PYTHON)
- `utils/logger.py`: new `SessionTagFilter` on both root handlers. Every line written inside an authenticated request gets `{s=<token8> u=<user> dc=<db>} ` right after the IP (`%(session_tag)s` in the format; empty outside a session). Only the first 8 characters of the token, the same prefix the `AUTH_*` lines already log. `SERVER_LOG_PATH` / `SERVER_LOG_BACKUPS` constants shared with the reader.
- `utils/logger.py`, split (correction 3): a second rotating handler writes `api_server_vba.log` (10 MB x 5, same format). `LegacySplitFilter` sends each line to exactly one file: a request with `X-Api-Key` and no `Authorization` (the test `require_session_or_api_key` already uses) → `api_server_vba.log`; everything else — K-BOT, `AUTH_LOGIN` / `AUTH_401`, startup, lines outside a request → `api_server.log`. The console still gets every line. Checked: every route of the legacy blueprints (admin, clasificatii, ddf, ftp, mfp, migrare, nomenclatoare, ord, parteneri, salarii, tools, wfls) is behind a key guard, so the old client always sends the key. Also fixed: the file handler was opened even when the root logger already had handlers.
- `utils/timing.py`: `log_path()` + `LOG_BACKUPS` (shared with the reader); `_session_notes()` puts `session=<token8> user= dc=` first on every block header, from `g.session` (`@timed` always sits under `@require_session`, checked in angajamente/extrase/prelucrare).
- `routes/logs.py` (new, `logs_bp`, registered in `main.py`), both `@require_session`:
  - `GET /api/logs/server?sessions=N` — the caller's `[forexe]` / `[forexe.xxx]` lines (plus their traceback lines) from `api_server.log` + `.1`..`.5`, session mark removed;
  - `GET /api/logs/timing?sessions=N` — the caller's blocks from `forexe_timing.log` + rotations, `====` separators removed.
  - The user comes from the session, never from the query. N defaults to 3, clamped 1..50. "Last N sessions" = the N logins with the most recent activity. Files are read newest first; once N sessions are known, older files are read only while they still contain them. Answer capped at 4 M characters (oldest dropped, `truncated: true`).
  - Body: `text`, `truncated`, `sessions` (`session`, `dc`, `first`, `last`, `entries`, `errors`, `warnings`, newest first), `sessions_requested`, `server_time`.

#### Client
- `KBot.Api/ApiClient.GetAsync(Of T)`: implemented (was `NotImplementedException`); bearer GET, case-insensitive JSON, non-2xx → `ApiException` with the server's `error`. No interface change, so no test double had to change. FileVersion 1.0.13.0 → 1.0.14.0.
- `KBot.Common/Logging/Parsers/ForexeTimingParser.vb` (new): one entry per timing block; level from the HTTP status (2xx Info, 4xx Warn, 5xx / `EXC` Error); source = route label; server time with `ServerClock` correction. `LogFileLoader`: parser registered before `AdobeHost`, file name `forexe_timing.log` → it. `LogFileLoader.vb` comments swept to English (rule 0). FileVersion 1.5.5.0 → 1.5.6.0.
- `SetariJurnalView`:
  - `CmbTipJurnal`: «Jurnale locale» / «Server FOREXE» / «Timpi FOREXE» (the server choices only when an `IApiClient` is set);
  - new `cmbSesiuni` (3 / 5 / 10 / 20 / 50 sesiuni), enabled only for server journals;
  - the server text goes through `LogFileLoader.LoadText` under `api_server.log` / `forexe_timing.log`, so the existing level chips do the Error / Warning / Info split;
  - status line adds «3 sesiuni: 28.09 09:09–10:51 (014_SCSV), …»;
  - failures and "nothing of yours yet" go into `noticeGol`; «Deschide dosarul» / «Golește» disabled for server journals;
  - the old `/api/logs/files` + `/api/logs/tail` code removed (those routes never existed); `noticeServer` removed from the designer (it sat in the hidden file panel). Test hooks `DebugAduListaServerAsync` / `DebugNoticeServerAfisat` kept, now on the new path.
  - Filter row re-laid for the extra column (144 dpi designer numbers).
  - Day grouping (correction 4): `HeaderCaptionFormat` / `FooterCaptionFormat` = `{1}`, replacing `{0}: {1} ({2})` (`{0}` = «Ora»); `CollapsedByDefault = True`. Since that flag collapses each day only on first encounter, `IncarcaSelectiaAsync(strangeZilele)` also calls `grila.CollapseAllGroups()` when file/kind/session count changes. Refresh or page activation passes False to retain open days. Applies to local and server journals.
  - Export (correction 5): `btnExporta` takes only the rows that pass the filters AND sit in an expanded day, through the new public `KBotDataView.IsRowShown(rowIndex)` (a public form of the existing `RowIsOnScreen`). No day expanded = no dialog, status line «Nimic de exportat: deschideți zilele pe care vreți să le exportați.». Tooltip updated. «Copiază» unchanged (selected row, or every filtered row when nothing is selected).
- `KBot.Controls`: `KBotDataView.IsRowShown` added (+ `KBotDataView.md`). FileVersion 1.54.0.0 → 1.55.0.0.

### Files touched
`PYTHON/utils/logger.py`, `PYTHON/utils/timing.py`, `PYTHON/routes/logs.py` (new), `PYTHON/main.py`; `src/KBot.Api/ApiClient.vb`, `src/KBot.Api/KBot.Api.vbproj`; `src/KBot.Common/Logging/LogFileLoader.vb`, `src/KBot.Common/Logging/Parsers/ForexeTimingParser.vb` (new), `src/KBot.Common/KBot.Common.vbproj`; `src/KBot.App/Setari/SetariJurnalView.vb`, `src/KBot.App/Setari/SetariJurnalView.Designer.vb`; `src/KBot.Controls/DataView/KBotDataView.Grouping.vb`, `src/KBot.Controls/DataView/KBotDataView.md`, `src/KBot.Controls/KBot.Controls.vbproj`

### Test results
- `dotnet build src/KBot.App/KBot.App.vbproj`: 0 errors, 0 warnings.
- Python: `py_compile` of the four files OK.
- No tests written or run (standing rule); the page was not opened on screen.

### Unverified / deferred
- **Nothing before the deploy shows up.** Lines written before this server version have no session mark, so the server journals start empty and fill as the operator works.
- After the deploy, VBA lines are only in `api_server_vba.log`; greps on the VPS must include it. The old `api_server.log` rotations still hold mixed lines until they rotate out.
- `api_server.log` format change: any other reader of that file (grep scripts, the operator's own tools) sees the extra `{s=… u=… dc=…}` field after the IP. The K-BOT parser is unaffected (the server strips the mark before sending).
- Never run against the VPS; the route, the tag and the page were checked by reading only.
- The existing `LogViewerFormTests` select rows by index (`DebugSelecteazaRand`); with the days now collapsed those rows are inside collapsed groups. Not run (standing rule) -- if one fails, that is the reason.
- Label widths in the filter row (64 / 76 px at 144 dpi) not checked on screen.
- Sessions are keyed by the 8-character token prefix; two live tokens of one user sharing a prefix would merge (1 in 64^8).

---

## SLICE-0089-01 — server journals only with «Opțiuni avansate» (operator request, 01.10.2026) <a id="slice-0089-01"></a>

### What changed and why
Operator: the server journals are tied to the advanced options switch; with it off the user must not see the combo that picks the journal type.

`SetariJurnalView`:
- `PotrivesteTipuriCuClientul`: «Server FOREXE» / «Timpi FOREXE» are listed only with an `IApiClient` AND `AppSettings.AdvancedOptions`; otherwise only «Jurnale locale» exists.
- `ArataCombinatiileDeTip` (called from `Activated`): `CmbTipJurnal` and `cmbSesiuni` are hidden when the server journals are off, and the first two columns of `tlyFilterActual` collapse to 0 so no gap is left. The designer widths are remembered the first time they are collapsed and restored when the combos come back. It is only called from `Activated` (not the constructor) so the widths are read after the DPI scaling.
- The switch lives on another settings page, so it can only change while the journal page is away; activation re-reads it. Switching it off while a server journal was selected falls back to «Jurnale locale» and reloads.

### Assumption
Read as a requirement («the server journals are connected to the advanced options checkbox, if false the combo is not visible»): the code did not have this link before. «Jurnale locale» stays available without the switch.

### Files
`src/KBot.App/Setari/SetariJurnalView.vb`; help `avansat/jurnale.md` (see 0000-35).

### Test results
`dotnet build src\KBot.App`: no compile errors. Nothing run on screen; the collapsed columns were not seen.

## Repere unice din registrul 0089

Înaintea folosirii erau cerute pe VPS utils/logger.py, utils/timing.py, routes/logs.py și main.py, cu repornire; nicio executare nu este făcută aici. Ajutorul pentru condiția avansată era `avansat.jurnale`, 0000-35.
