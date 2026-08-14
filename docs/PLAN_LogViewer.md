# PLAN — Log viewer (slice 0031)

Target file in the repo: `docs/PLAN_LogViewer.md`.
Worklogs: `SLICE-0031-01-log-core.md`, `-02-server-logs.md`, `-03-chipbar.md`, `-04-log-viewer.md`.

Slice number **0031** was assigned by the operator. Confirm it against `KBOT_STATUS.md`
("Next free slice number") before starting. If STATUS says something else, **stop and report**.

**Version 2.** Revised after operator review: rotation now keeps history instead of clearing, and
server-side logs are in scope with per-unit filtering enforced on the server.

> **Note added when the plan was filed (slice 0031-01).** The plan text below is the operator's
> version 2, stored verbatim so passes 02–04 can read it. Where pass 01 found the plan contradicted
> the real source, the deviation is recorded in `docs/worklog/SLICE-0031-01-log-core.md` §"Abateri
> de la plan" — it is NOT edited into the plan text here. Four matter for later passes:
> **(A)** the "exactly three terminal sinks" line of §1.4 does not exist in `KBOT_STATUS.md`;
> **(B)** the flat 30% parser probe of §5.5 misfires on block formats and was refined;
> **(C)** rotation in the `RunLogger` constructor is a no-op as `RunLogger` is called today;
> **(D)** `TreeLogger` writes nothing in the built solution — `Init` is called only from `Surse/`.

---

## 0. Mandatory reads before touching anything

`CODE_WORKFLOW.md` §1 applies in full. Read these verbatim first. Everything below was written from
project-knowledge search, not from an editor, so **every "currently reads X" claim must be
re-checked against the real file**.

VB.NET side:

- `docs/worklog/KBOT_STATUS.md`, `docs/worklog/CODE_WORKFLOW.md`
- `src/KBot.Common/GlobalErrorLog.vb`
- `src/KBot.Controls/Adobe/AdobeHostLog.vb`
- `src/KBot.Controls/Tree/TreeLogger.vb`
- `src/KBot.DevHarness/RunLogger.vb`, `DevHarnessForm.vb`, `DevHarnessForm.Designer.vb`
- `src/KBot.Theming/ThemePalette.vb`, `BuiltInSchemes.vb`, `ThemeManager.vb`
- `src/KBot.Controls/NavList/KBotNavList.vb`, `KBotNavItem.vb`, `KBotNavItemCollection.vb`
  (the new control in §7 copies this design deliberately — read it before writing it)
- `src/KBot.Controls/DataView/KBotDataView.vb` + `Events/KBotRowFormattingEventArgs.vb` +
  `Events/KBotCellFormattingEventArgs.vb`
- `src/KBot.App/Views/IstoricView.vb` (the filter-engine-plus-view pattern this slice reuses)
- `src/KBot.Api/ApiClient.vb` + `IApiClient.vb` (how a call is added; the `WithReauth` pattern)
- `src/KBot.Controls/README.md` (folder-per-control-family rule)

Python side:

- `main.py`, `utils/logger.py`, `config.py`
- `routes/auth/auth.py`, `routes/auth/guard.py`, `routes/auth/session_store.py`
- one existing blueprint end to end, e.g. `routes/forexe/tree.py`, for the house shape of a route

---

## 1. Corrections to the original brief

Not silent deviations. Each is here because reading the real source showed the instruction was
wrong, impossible, or incomplete.

### 1.1 No new palette slots. The colours already exist.

`ThemePalette.vb` already has `[Error]`, `Warning`, `Success`, `Text`, `TextDim`, `DisabledText`,
and all three built-in schemes fill them. Adding `LogError` beside `Error` would be a second name
for one colour in three schemes.

**Mapping only, zero changes to `KBot.Theming`:**

| Level | Palette slot |
|---|---|
| Error | `Palette.ErrorColor` |
| Warn | `Palette.WarningColor` |
| Info | `Palette.TextColor` (row default — no override painted) |
| Debug | `Palette.TextDimColor` |
| Trace | `Palette.DisabledTextColor` |
| Unknown | `Palette.TextDimColor` |

Confirm the accessor names against the real file. If one does not exist, **stop and report** — do
not invent it.

### 1.2 The 10 MB rule cannot live in the viewer.

A viewer enforces a size limit only when someone opens it, which is exactly when it does not
matter. The check runs in the writers, before every append. See §4.3.

### 1.3 Rotation keeps history. It does not clear. (Revised)

The first version of this plan cleared the file at 10 MB, as instructed, and flagged that this
destroys history. The operator rejected that.

`utils/logger.py` on the VPS already solves this, and has since before this slice existed:

```python
RotatingFileHandler('api_server.log', maxBytes=10*1024*1024, backupCount=5)
```

The client copies that policy exactly — **10 MB, five generations** — so there is one rule across
both halves of the system instead of two. See §4.3.

### 1.4 `TreeLogger` becomes terminal sinks 4 and 5 — a documented rule change.

`KBOT_STATUS.md` says there are **exactly three** permitted non-rethrowing terminal sinks.
`TreeLogger.Write` has a bare `Catch` commented "Eșec silențios la scriere", and `TreeLogger.Init`
swallows its Temp fallback. Both violate the no-swallowed-exceptions rule as written.

They cannot be made to rethrow: `AdvancedTreeControl` calls them from paint and layout paths, and a
throw out of a log write inside `OnPaint` takes the process down. Fix them the way `GlobalErrorLog`
and `AdobeHostLog` already work — `Trace.WriteLine`, no rethrow — and update the "exactly three"
line in `KBOT_STATUS.md` **in the same commit**. Do not leave STATUS saying three while the code
has five.

### 1.5 Server logs are in scope, and the Python logger is redesigned. (Revised)

Original scope excluded them. The operator wants every error visible, filtered per unit **on the
server**. That is not a viewer feature — it is a change to how the API logs. See §6.

---

## 2. Scope

**In:**

- A themed log viewer form: file list, level chips, text search, date range, grid, detail pane,
  copy / export / open folder / clear.
- One new control, `KBotChipBar`.
- A pure, WinForms-free core in `KBot.Common`: paths, rotation, reading, parsing, filtering.
- Rotation with five generations at 10 MB, enforced in every writer.
- `TreeLogger` moved into the same `Logs\` folder as everything else.
- **Server side:** per-unit log files, a DC-aware logging filter, a catch-all error handler so
  uncaught exceptions reach the log file, Gunicorn's own output redirected into files, two read
  endpoints, and a server clock reading.

**Out — do not build, do not scaffold:**

- **nginx logs.** They live under `/var/log/nginx`, owned by root or `www-data`, and reading them
  needs a group membership or an ACL on the directory — a change to the box, not to app code. Record
  in the worklog what it would take; do not do it here.
- Live tailing (`FileSystemWatcher` / polling). The reader is written so it can be added; nothing
  more.
- A `MainForm` nav entry. The form opens from DevHarness in this slice and must not depend on
  anything DevHarness-only, so wiring it into the shell later is a two-line change.
- Deleting logs as a side effect of anything. The only deletion path is the operator pressing clear
  and confirming.
- Writing to server logs from the client. The endpoints are read-only.

---

## 3. The client log files, as they actually are

Verify each format against its writer before writing a parser. Do not trust this table alone.

| File | Writer | Format |
|---|---|---|
| `<AppDir>\Logs\harness_errors.log` | `GlobalErrorLog.Write` | `==== yyyy-MM-dd HH:mm:ss.fff  [source] ====` then the full `ex.ToString()` over many lines, then a blank line. UTF-8 with BOM, appended. |
| `<AppDir>\Logs\test_{yyyyMMdd_HHmmss_fff}.log` | `RunLogger` | one file per harness run: header, per-test lines, `EROARE [source]: <ex.ToString()>`, summary. UTF-8 with BOM, `AutoFlush`, **held open while a run is in progress**. |
| `<AppDir>\Logs\adobe_preview.log` | `AdobeHostLog.Write` | `yyyy-MM-dd HH:mm:ss.fff  message`, one line per entry. |
| `<AppDir>\Logs\test_adobe_rhp.log` | slice 0023 probe | timestamped, appended. Confirm the prefix in the 0023 source. |
| `<BaseDirectory>\log_{treeId}.txt` | `TreeLogger` | `[HH:mm:ss.fff] [12.345s] [LEVEL] [source] message`. **Time only, no date.** Moves to `Logs\` in this slice. |

Three timestamp shapes and one multi-line format. That is the actual problem; the grid is the easy
half.

---

## 4. `KBot.Common` — writer-side changes

### 4.1 `LogPaths.vb` (new)

```
Public Module LogPaths
    Public Function LogsDirectory() As String        ' Path.Combine(AppContext.BaseDirectory, "Logs")
    Public Function EnsureLogsDirectory() As String
    Public Function Combine(fileName As String) As String
End Module
```

One place that answers "where do logs live". `GlobalErrorLog`, `AdobeHostLog`, `RunLogger` and
`TreeLogger` all call it. No behaviour change for the first three — confirm the path they compute
today is identical before editing.

### 4.2 `TreeLogger`

- Write to `LogPaths.EnsureLogsDirectory()`, not `AppDomain.CurrentDomain.BaseDirectory`.
  Filename `log_{treeId}.txt` unchanged. Keep the existing optional path override if callers use it.
- Keep the Temp fallback in `Init`, but write the reason to `Trace` instead of swallowing.
- Terminal sinks per §1.4.

### 4.3 `LogRotation.vb` (new) — history, not destruction

```
Public Module LogRotation
    Public Const MaxBytes As Long = 10L * 1024L * 1024L
    Public Const BackupCount As Integer = 5
    Public Function Roll(filePath As String, maxBytes As Long, backupCount As Integer) As Boolean
End Module
```

Called **before** every append. Same policy as Python's `RotatingFileHandler`, deliberately:

1. File missing, or `Length <= maxBytes` → return False, touch nothing.
2. Over the limit → delete `<file>.5`, rename `.4`→`.5`, `.3`→`.4`, `.2`→`.3`, `.1`→`.2`,
   `<file>`→`.1`, then let the caller create a fresh live file. Return True.
3. Renames only. Never read-and-rewrite — the cost must not depend on file size.
4. Any failure at any step → **do not throw, do not delete anything else**. Write the reason to
   `Trace` and return False, so the append that triggered the check still happens. A rotation
   problem must never cost the line that caused it.

Worst case per family is 60 MB, bounded and predictable.

Wire it into `GlobalErrorLog.Write`, `AdobeHostLog.Write`, `TreeLogger.Write` and the `RunLogger`
constructor. Per-run files rarely reach 10 MB, but a runaway loop in a test would, and the guard
costs one `FileInfo.Length`.

**Do not** put the check in the viewer. The viewer reports sizes; it does not enforce them.

---

## 5. `KBot.Common` — reading and parsing (no WinForms, no Drawing)

Everything here is pure and headless-testable.

### 5.1 `KBotLogLevel`

`Unknown, Trace, Debug, Info, Warn, Error` — in that order, so a minimum-level comparison is a
plain `>=` if it is ever wanted.

### 5.2 `LogEntry`

| Member | Type | Notes |
|---|---|---|
| `Timestamp` | `Date?` | `Nothing` when the format carries none |
| `Level` | `KBotLogLevel` | |
| `Source` | `String` | the `[source]` bracket, or the client IP for server entries |
| `Message` | `String` | first line, trimmed |
| `Raw` | `String` | the complete original block including continuation lines — what the detail pane shows |
| `FileName` | `String` | file name only |
| `LineNumber` | `Integer` | 1-based, within the loaded window |
| `Origin` | `LogOrigin` | `Client` or `Server` — drives the clock correction in §6.6 and the `Sursă` column |

Immutable after construction where practical.

### 5.3 `LogFileReader`

- Open with `FileShare.ReadWrite`. `RunLogger` holds its file open with `AutoFlush`; a reader that
  does not allow write-sharing fails on the current run's log. This is the common case, not an edge
  case.
- Read the **last 5 MB**: seek to `Max(0, Length - 5MB)`, discard through the first newline so a
  half-line never reaches the parser, skip a BOM at offset 0.
- Return the text plus `WasTruncated As Boolean` and `FileLengthBytes As Long`.
- UTF-8 decoding.

### 5.4 Parsers

```
Public Interface ILogEntryParser
    ReadOnly Property Name As String
    Function TryParseHeader(line As String, ByRef result As LogEntry) As Boolean
End Interface
```

One question only: does this line start a new entry, and if so what are its fields? Continuation is
the loader's job.

- `HarnessErrorParser` — the `==== ts  [source] ====` banner; level always `Error` (that file only
  ever receives exceptions); the `ex.ToString()` lines that follow are continuation.
- `AdobeHostParser` — `yyyy-MM-dd HH:mm:ss.fff  message`, level `Info`. Confirm the separator in
  `AdobeHostLog.Write`.
- `TreeLoggerParser` — `[HH:mm:ss.fff] [12.345s] [LEVEL] [source] message`. **Time only**: take the
  date from the file's `LastWriteTime`. If times run backwards across midnight, leave later entries
  on the same date rather than guessing, and say so in the worklog.
- `RunLogParser` — the harness run file. `EROARE [` starts an `Error` entry whose `ex.ToString()`
  follows as continuation. Read `DevHarnessForm.RunTestsAsync` for the real per-test result lines
  before deciding which map to `Warn` and which to `Info`. Do not guess at markers you have not seen.
- `ApiServerParser` — the server format defined in §6.2. Both the new ISO form and the legacy form,
  because the file on disk today is the legacy one.
- `FallbackParser` — every line is its own `Info` entry with no timestamp. Never fails.

### 5.5 `LogFileLoader` — parser selection and continuation

- Pick a parser by file-name pattern (`harness_errors.log`, `adobe_preview.log`, `test_*.log`,
  `log_*.txt`, `api_*.log`).
- **Then verify the guess.** Run it over the first 50 non-blank lines; if fewer than 30% are
  recognised as headers, fall back to trying every parser per line, and record which parser actually
  won. A silently wrong guess produces a viewer full of `Unknown` rows and no error — the failure
  mode that wastes an afternoon.
- Continuation: a line no parser recognises as a header is appended to the previous entry's `Raw`
  (and to `Message` only if `Message` is empty). With no previous entry it becomes its own entry,
  `Level = Unknown`, `Timestamp = Nothing`. Blank lines are continuation and never start an entry.
- Afterwards, entries with `Timestamp = Nothing` inherit the preceding entry's timestamp. Return
  how many inherited and how many are still `Nothing` — the filter needs both (§5.6).

### 5.6 `LogFilter`

Pure class, same shape as `IstoricFilter`. Read that file before writing this one.

Criteria: `Files As ISet(Of String)`, `Levels As ISet(Of KBotLogLevel)`, `Text As String`,
`From As Date?`, `To As Date?`.

- Text: `IndexOf(..., StringComparison.OrdinalIgnoreCase)` against `Raw`, so a search hits inside a
  stack trace. Empty text matches everything. **No diacritic folding** — "sters" will not find
  "șters". Record that as a known limitation.
- Empty `Levels` / `Files` = show nothing, and the chip bar guarantees at least one chip stays
  ticked (`MinimumRequiredChecked = 1`). Pin it with a test either way; a filter that silently
  empties the grid is a bug report waiting to happen.
- `From`/`To` inclusive, compared on the **corrected** timestamp (§6.6). An entry still holding
  `Nothing` is excluded when either bound is set, and the count of such exclusions is returned so
  the status bar can say `… · 3 intrări fără dată, excluse de filtrul de timp`. Silent exclusion is
  not acceptable.
- Returns kept entries plus counts. No side effects, no UI types.

---

## 6. Server side — per-unit logs (pass 0031-02)

### 6.1 The problem with what exists

`utils/logger.py` attaches a `RotatingFileHandler('api_server.log', ...)` to the **root** logger,
plus a `StreamHandler`. Three consequences:

1. **One flat file for every unit.** There is no way to hand an operator only their own lines.
2. **The path is relative**, so the file lands in whatever directory Gunicorn was started in.
   Code must confirm the real location on the VPS before anything else.
3. **Uncaught exceptions never reach the file.** Flask handles them and the traceback goes out
   through the console handler, which under Gunicorn is stderr, which systemd captures in the
   journal. The file that the viewer would read is missing precisely the crashes.

### 6.2 One file per unit, mirroring one database per unit

The project's own convention is one database per unit, keyed by `DC`. Logging follows it.

- `<LOG_DIR>/api_<DC>.log`, plus `.1`–`.5` archives, `maxBytes=10*1024*1024`, `backupCount=5`.
- `<LOG_DIR>/api_common.log` for records with no unit: startup, the route table, pre-auth failures,
  rate-limiter blocks.
- `LOG_DIR` becomes an **absolute** path in `config.py`, chosen by Code per §13.2. The existing
  `api_server.log` is left exactly where it is — not moved, not renamed, not deleted, not exposed
  (§13.3). The per-unit files start empty.

**Format** (both handlers), ISO 8601 with a real UTC offset so no guessing is ever needed:

```
%(asctime)s - %(levelname)s - %(dc)s - %(ip)s - %(message)s
2026-08-13T14:22:31.123+03:00 - ERROR - 000_DEMO - 86.120.4.11 - forexe_tree DB error: ...
```

Set the formatter's `default_time_format` / converter so `asctime` carries the offset. The `dc`
field is written even though the file is already per-unit: it survives a file being moved, renamed
or concatenated, and it makes `api_common.log` readable.

**Routing.** A `logging.Handler` subclass resolves the DC per record and dispatches to a lazily
created `RotatingFileHandler` per DC, cached in a dict. This is safe because the project enforces a
**single Gunicorn worker** — two processes appending to and rotating the same file is exactly the
race this avoids. If that guarantee ever changes, this design must be revisited; write that in the
worklog.

**Where the DC comes from**, in order:

1. an explicit `extra={"dc": ...}` on the log call — used by `auth.py`, which knows the target DC
   at login before any session exists;
2. `g.session.db_name`, when a session exists (confirm the attribute in `session_store.py`);
3. `"common"`.

A `DcFilter`, alongside the existing `RequestIPFilter`, sets `record.dc`. Both filters must be
attached to every handler, and both must tolerate no request context — `has_request_context()` is
already the pattern in `RequestIPFilter`; copy it.

### 6.3 The catch-all error handler (`main.py`)

```python
@app.errorhandler(Exception)
def _log_unhandled(err):
    logger.exception("UNHANDLED %s %s", request.method, request.path)
    return jsonify({"error": "Eroare internă. Consultați jurnalul serverului."}), 500
```

Literal diacritics, `ensure_ascii=False` already set globally. Re-raise or pass through
`HTTPException` unchanged so 404 and 405 keep their status codes — an errorhandler that turns every
404 into a 500 is a classic and is worse than the gap it fixes. Confirm the exact form against the
Flask version in use.

Without this, "all errors visible" is false, and the viewer would show a hole rather than an error.

### 6.4 The endpoints

Both `@require_session`. Both **derive the filename from `g.session.db_name`** — the caller never
supplies a name or a path. There is no allowlist to get wrong: an operator cannot address another
unit's file because no code path exists that would let them name one.

- `GET /api/logs/files` → `{"files": [{"name": "api_000_DEMO.log", "generation": 0,
  "size_bytes": 812345, "modified": "<ISO+offset>"}], "server_time": "<ISO+offset>"}`.
  Only the caller's unit. Generations 0–5.
- `GET /api/logs/tail?generation=<0..5>&max_bytes=<n>` → `{"text": "...", "truncated": true,
  "size_bytes": ..., "server_time": "<ISO+offset>"}`.
  `generation` is an int 0–5, validated; anything else is a 400 with a Romanian message.
  `max_bytes` is clamped to 5 MB server-side regardless of what the client asks for. Default 512 KB.
  Reads the last N bytes by seeking, never `read()` on the whole file.

`api_common.log` is **not** reachable through these endpoints — see the open question in §13. It
holds startup records and pre-auth failures across all units; a route error always has a session and
therefore lands in the unit's own file.

Client half: `GetLogFilesAsync` / `GetLogTailAsync` on `ApiClient` + `IApiClient`, following the
existing call pattern including `WithReauth`. Read `ApiClient.vb` first and copy the shape.

### 6.5 Gunicorn's own output

Everything above concerns records the Flask app produces. Gunicorn produces its own, and today they
go to stderr and end up only in the systemd journal: worker start-up and shutdown, worker crashes,
and — critically — any traceback raised **before** the app's logger exists, such as an import error
or a bad `config.py`. Those are the errors that matter most and are currently the hardest to see.

Create **`gunicorn.conf.py`** in the app root. Gunicorn reads `./gunicorn.conf.py` from its working
directory by default; verify that against the installed Gunicorn version rather than assuming, and
if the systemd unit needs `--config` added, say so in the worklog and give the exact line to change.

```python
errorlog   = "<LOG_DIR>/gunicorn_error.log"
accesslog  = "<LOG_DIR>/gunicorn_access.log"
loglevel   = "info"
capture_output = True     # worker stdout/stderr into errorlog
```

`capture_output = True` is the point of the exercise: it sends anything written straight to
stdout/stderr — a bare `print`, a traceback at import time — into the error log file instead of the
journal.

**Consequence that must be handled, not discovered later.** `utils/logger.py` currently attaches a
`StreamHandler` to the root logger. With `capture_output = True`, every line that handler writes is
duplicated into `gunicorn_error.log`, which means per-unit records land in a shared file. Under
Gunicorn, **do not attach the console handler**; keep it only when `main.py` is run directly for
development. Gunicorn sets `SERVER_SOFTWARE` in the environment — check for it, and confirm that is
true for the installed version before relying on it. If it is not, use an explicit environment
variable set in the systemd unit instead of guessing.

**Rotation.** Gunicorn does not rotate its own files. Add `/etc/logrotate.d/kbot`. logrotate is the
standard Ubuntu tool that renames a log file once a day (or once it passes a size), keeps a set
number of old copies, and deletes the rest — the same idea as `RotatingFileHandler`, run by the
system instead of by the application. It must send Gunicorn a `USR1` signal in the `postrotate`
step: that tells Gunicorn to close and reopen its log files, so it keeps writing to the new file
rather than to the renamed one. Match the app's policy — size 10 M, `rotate 5`, `missingok`,
`notifempty`, `compress` optional. Put the exact file contents in the worklog; it is not in the repo
and nobody will remember it in six months.

**Exposure.** These two files carry records from every unit and from before any request context
exists, so they cannot be filtered per unit without parsing a shared file — the leak-prone approach
this design deliberately rejected in §6.2. They are therefore treated like `api_common.log`
(§13.1): written, rotated, **not** exposed through the endpoints, read from the server when needed.

**Deployment.** This part is not a code change alone; it needs a service restart and a logrotate
file dropped on the box. It cannot be verified from the repo. List it as an explicit host step in
the worklog, with the exact commands, and mark it unverified until the operator has run it.

**Adjacent, deliberately not absorbed:** slice 0004 has an open item to enforce a single Gunicorn
worker through an `on_starting` guard, and `gunicorn.conf.py` is exactly where that guard belongs.
Do **not** silently fold another slice's work into this one. Either the operator says to do it here,
and both slice rows record that, or the worklog notes that the file now exists and the guard is a
two-line follow-up.

### 6.6 Clock correction

The server's time comes back on every one of these responses as `server_time`, and the login
response gains the same field. The client keeps `ServerClock.Offset`:

- computed at login as `server_time - DateTimeOffset.UtcNow`, and refreshed from each logs response
  so a three-hour-old session does not display against a stale offset;
- round-trip is not compensated (half the RTT is the error bound, sub-second on this link, and
  pretending to more precision than that would be dishonest);
- new server lines carry their own offset in the timestamp, so **the correction is a fallback for
  legacy lines only**. Use the parsed offset when the line has one; use `ServerClock.Offset` when it
  does not.

Display rule: the grid's `Ora` column shows **every** entry in client local time, server entries
converted. The detail pane shows the raw line **untouched**. The status bar states the offset when
it is non-zero: `ceas server: +03:00`.

---

## 7. `KBotChipBar` — the one new control (pass 0031-03)

Folder `src/KBot.Controls/ChipBar/`, per the README rule. Three files: `KBotChip.vb`,
`KBotChipCollection.vb`, `KBotChipBar.vb`.

**Copy `KBotNavList`'s decisions rather than reinventing them.** Read `KBotNavList.vb`,
`KBotNavItem.vb` and `KBotNavItemCollection.vb` first; this is their multi-select sibling and should
be recognisably the same code.

### 7.1 `KBotChip`

Parameterless constructor (the collection dialog needs it) plus `New(key, text)`. Properties, each
with `<Category("K-BOT")>` and a Romanian `<Description>`: `Key`, `Text`, `Checked`, `Count` (badge,
0 = hidden), `Enabled`, `Visible`, `AccentOverride As Color` (`Color.Empty` = use the scheme accent).

`AccentOverride` exists so the ERROR chip can be red and WARN amber. The **caller** passes
`Palette.ErrorColor` / `Palette.WarningColor`; the control never names a colour. Say that in the
property's description, and re-apply on theme change.

`Bounds` stays `Friend` — derived layout state, must not serialize, same reason as `KBotNavItem`.
`ToString()` returns something readable in the collection dialog: `err — "Erori" [✓]`.

### 7.2 `KBotChipCollection`

`Inherits Collection(Of KBotChip)`, `Friend Owner As KBotChipBar`. Override `InsertItem` / `SetItem`
/ `RemoveItem` / `ClearItems`: call the base, then `Owner?.InvalidateLayout()`. Reject `Nothing`.
**Do not validate keys here** — the dialog inserts an empty chip the moment Add is pressed.

### 7.3 `KBotChipBar`

- `<ToolboxItem(True)>`, `<DefaultProperty("Chips")>`, `<DefaultEvent("CheckedChanged")>`.
- `Inherits Control`, `Implements IThemedControl`, `Implements ISupportInitialize`.
- Same `SetStyle` set as `KBotNavList`: `UserPaint Or AllPaintingInWmPaint Or
  OptimizedDoubleBuffer Or ResizeRedraw Or Selectable`. `TabStop = True`.
- `Chips` with `<DesignerSerializationVisibility(Content)>`.
- Layout: horizontal flow, wraps to a second row. Chip width = `TextRenderer.MeasureText` + padding
  (+ badge). Heights and paddings DPI-scaled through `ThemeShapes.ScaleDpi`. Public `ChipHeight`,
  `ChipPadding`, `ChipSpacing`, `ChipCornerRadius`.
- Painting: unchecked = `ButtonBack` / `ButtonText` / `ButtonBorder`; hover = `ButtonHover`;
  checked = `AccentOverride` if set else `Accent`, text `AccentText`; disabled `DisabledText`.
  Cache brushes and pens, rebuild in `ApplyTheme` as `KBotDataView.RebuildThemeResources` does,
  dispose in `Dispose(disposing)`.
- API: `AddChip(key, text)`, `AddChip(key, text, checked)`, `SetChecked`, `IsChecked`,
  `SetChipEnabled`, `SetChipVisible`, `SetBadge`, `CheckedKeys As IReadOnlyList(Of String)`,
  `CheckAll`, `UncheckAll`.
- Errors, house rule, no silent no-ops: empty key, duplicate key, unknown key → `ArgumentException`
  with Romanian messages and literal diacritics.
- Events: `CheckedChanged(chipKey)` on **real change only**, plus `ChipClicked`. A programmatic
  `SetChecked` to the value already held raises nothing.
- Input: click toggles; `Left`/`Right` move focus, `Space` toggles; override `IsInputKey` for all
  three or the form eats them. Skip invisible and disabled chips.
- `MinimumRequiredChecked As Integer` (default 0). At 1, unticking the last checked chip is refused
  — a no-op with a brief flash, not an exception; this is a mouse gesture, not an API call.
- Every `OnPaint` / `OnKeyDown` / `OnMouseDown` body routes through `GlobalErrorLog.Write`, guarded
  by `KBotDesignTime.IsDesignTime(Me)`. Copy the exact pattern from `KBotNavList`.
- `BeginInit`/`EndInit`: suspend layout and key validation between them; validate and apply
  `Checked` at `EndInit`, skipping validation at design time and showing the 2px red border instead,
  as `KBotNavList` does.

---

## 8. `LogViewerForm` — `src/KBot.App/Views/` (pass 0031-04)

Inherits `KBotShellForm` (confirm the base class and its API; follow whatever structure it imposes
rather than fighting it). **All controls declared in `LogViewerForm.Designer.vb`** — house rule.

Layout, top to bottom:

1. `KBotCaptionBar` — title `Jurnale`.
2. Filter strip: `chipLevels As KBotChipBar` (`MinimumRequiredChecked = 1`), `txtSearch As
   KBotTextField`, `txtFrom` / `txtTo As KBotTextField` (`dd.MM.yyyy`), `btnRefresh`.
3. Body split: left `navFiles As KBotNavList` (vertical), right the grid.
4. Detail pane below the grid: read-only multiline `TextBox`, `Consolas`, showing `LogEntry.Raw`.
5. Action row: `btnCopy`, `btnExport`, `btnOpenFolder`, `btnClear`.
6. `KBotBusyBar` + status label.

Details:

- **`navFiles`** has two groups separated by a `KBotNavList` separator: `Local` (every `*.log`,
  `*.log.1`–`.5` and `log_*.txt` under `LogPaths.LogsDirectory()`, archives labelled `arhivă`) and
  `Server` (from `GET /api/logs/files`). A `Toate fișierele` item is pinned first and merges
  everything, sorted on corrected timestamps, entries without one kept in file order at the end.
  Badge = entry count. Within a group, newest modified first.
- **Server items load on demand**, when selected — not at form open. If the call fails, the server
  group shows the error in a `KBotNotice` and the local files keep working. A dead API must not take
  the viewer down with it.
- **Grid columns** (`KBotDataView`): `Ora` (frozen, `dd.MM HH:mm:ss.fff`), `Nivel`, `Sursă`
  (`local` / `server`), `Fișier`, `Detaliu` (the `[source]` bracket or client IP), `Mesaj` (last,
  fills). Row `Tag` holds the `LogEntry`.
- **Row colouring** through the existing `RowFormatting` event and the §1.1 mapping. Do not touch
  `KBotDataView`. The args instance is **reused** — the handler must not retain it.
- **Loading is off the UI thread.** `BusyBar` on, read/fetch and parse on a background thread,
  marshal back, `BeginUpdate`/`EndUpdate` around the fill. Filtering is in-memory and synchronous and
  must never re-read a file or re-issue a request.
- **Search debounce**: 250 ms timer, not `TextChanged` straight into a re-filter.
- **Status label**: `1.482 intrări · 96 afișate · 3,4 MB · ultimii 5 MB · ceas server +03:00 ·
  3 fără dată, excluse`.
- **`btnCopy`**: the selected row's `Raw`, or the whole filtered set when nothing is selected.
  **`btnExport`**: `SaveFileDialog`, filtered entries' `Raw`, UTF-8 with BOM.
  **`btnOpenFolder`**: `Process.Start` on `explorer.exe` with the logs directory.
- **Empty state**: no files, or nothing after filtering, gets a `KBotNotice`, not a blank grid.

### 8.1 `btnClear` — the destructive one

**Local files only.** Server logs are never deleted from the client; the endpoints are read-only and
stay that way.

A modal lists every local file with a checkbox, size and entry count, nothing ticked initially, then
a confirmation naming exactly what will go and its total size. Two steps, because there is no undo.

Per file: `File.Delete`; on `IOException` (a file held open — the running harness log) fall back to
truncating to zero. If both fail, report that file by name with the reason and continue with the
rest. **Nothing fails silently.** Summarise per file, refresh, reload the current selection.

The file the running process holds open is shown but **greyed and unticked**, labelled `în uz de
rularea curentă`.

---

## 9. DevHarness wiring

- A `Jurnale` button on `DevHarnessForm` (declared in the `.Designer.vb`) opening `LogViewerForm`
  non-modally.
- An `IHarnessTest` in `Controls/UI` that opens the viewer and asks for a human verdict — copy
  `ThemeGalleryTest` exactly (OK → Passed, Cancel → Failed, closed → Skipped).
- `LogViewerForm` lives in `KBot.App`, so Release keeps it and a `MainForm` nav entry later costs
  two lines. Confirm no DevHarness reference leaks into `KBot.App` in Release — the
  Debug-conditioned `ProjectReference` stays exactly as it is.

---

## 10. Tests

**Report real before/after counts from an actual run.** Do not carry numbers from this plan or an
older worklog; that has already gone wrong once (slice 0025 §0.3). .NET tests run on the client
machine after publish, results in `AppDir\Logs\test_*.log`.

`KBot.Common.Tests`:

1. Each parser: one real sample line per format → correct timestamp, level, source, message.
2. `HarnessErrorParser`: banner + five stack lines → **one** entry whose `Raw` holds all six.
3. An unrecognised leading line with no previous entry → its own `Unknown` entry, not a crash.
4. `TreeLoggerParser`: time-only stamp combined with the file date; the midnight case documented.
5. Parser mis-guess: adobe-format content in a file named `harness_errors.log` → low header-match
   rate detected, winning parser reported.
6. `ApiServerParser`: the ISO-with-offset form **and** the legacy form, both to the right level.
7. `LogFileReader`: oversized file → truncated flag, first partial line dropped, BOM skipped.
8. `LogFileReader` against a file held open by a live `StreamWriter` with `AutoFlush` → reads fine.
   This is the `RunLogger` case and the one a careless `File.ReadAllText` breaks.
9. `LogRotation`: over limit → `.1`…`.5` shift, oldest dropped, live file recreated empty, `True`;
   under limit → byte-identical, `False`; a locked file → `False`, no throw, no data lost;
   `backupCount` respected exactly (never a sixth generation).
10. `LogFilter`: each axis alone; two combined; empty-set semantics as decided; the
    timestamp-less exclusion count.
11. `ServerClock`: offset arithmetic; a line carrying its own offset ignores the clock offset; a
    legacy line uses it.

`KBot.Controls.Tests`:

12. `KBotChipCollection` mutation invalidates layout; `Nothing` rejected.
13. Duplicate / empty / unknown key → `ArgumentException` on each setter.
14. `CheckedKeys` reflects state; `CheckAll` / `UncheckAll`.
15. `CheckedChanged` fires once on real change, never on a redundant set.
16. `MinimumRequiredChecked = 1` refuses to untick the last chip.
17. `BeginInit`/`EndInit` round trip with chips added between them.
18. Keyboard: `Space` toggles the focused chip; `Left`/`Right` skip hidden and disabled chips.

`KBot.Api.Tests`:

19. `GetLogFilesAsync` / `GetLogTailAsync` deserialize the documented body, including `server_time`.
20. A 401 goes through `WithReauth` like every other call.

`KBot.App.Tests`:

21. `LogViewerForm` constructs and mounts chip bar, nav list, grid and detail pane.
22. The `RowFormatting` handler maps each level to the right palette colour — assert against
    `ThemeManager.Current.Palette`, never a literal.
23. Selecting a row puts that entry's `Raw` in the detail pane, **unconverted**.
24. A failing server call leaves the local files usable and surfaces a notice.

Python (`PYTHON/tests/test_logs.py`, host-only, skips off-host like the existing suites):

25. `DcFilter` sets `record.dc` from `g.session.db_name`, from an explicit `extra`, and falls back
    to `common` with no request context.
26. The routing handler opens one file per DC and reuses it on the second record.
27. `GET /api/logs/files` returns only the caller's unit's files; a session on another DC sees a
    different set. **This is the security test — do not skip it.**
28. `generation` outside 0–5, or non-numeric → 400 with a Romanian message.
29. `max_bytes` above the server cap is clamped, not honoured.
30. Both endpoints without a token → 401 with a reason code.
31. The catch-all handler writes a traceback to the unit's file and returns 500, while a 404 stays
    a 404.

---

## 11. Passes

| Pass | Contents | Worklog |
|---|---|---|
| 0031-01 | `LogPaths`, `LogRotation`, `LogFileReader`, parsers, `LogFileLoader`, `LogFilter`, `ServerClock`; the four writer changes; `TreeLogger` fixes; the STATUS terminal-sink correction; tests 1–11 | `SLICE-0031-01-log-core.md` |
| 0031-02 | Python: `LOG_DIR`, per-DC routing handler, `DcFilter`, ISO format, catch-all error handler, `gunicorn.conf.py` + logrotate, both endpoints; `ApiClient` half; tests 19–20, 25–31 | `SLICE-0031-02-server-logs.md` |
| 0031-03 | `KBotChipBar` + `KBotChip` + `KBotChipCollection`; tests 12–18 | `SLICE-0031-03-chipbar.md` |
| 0031-04 | `LogViewerForm`, clear dialog, DevHarness wiring, harness test; tests 21–24 | `SLICE-0031-04-log-viewer.md` |

Each pass commits and pushes on its own with its worklog and its `KBOT_STATUS.md` update. Do not
batch them. 0031-02 touches production API behaviour — deploy it deliberately, not as a side effect
of a client build.

---

## 12. Definition of done (per pass)

1. `dotnet build -c Debug` and `-c Release`: 0 errors, 0 new warnings, `Option Strict On`.
   Python: offline suite green or cleanly skipped, 0 fail/error.
2. Full .NET suite green; real counts reported.
3. Worklog at `docs/worklog/SLICE-0031-0X-….md` with the four mandatory sections: what changed and
   why · files touched · test results · anything left unverified or deferred.
4. `KBOT_STATUS.md` updated — slice row, next free number, and (pass 01) the terminal-sink count.
5. Code, worklog and STATUS committed together and pushed.
6. No swallowed exceptions introduced. The only new non-rethrowing sinks are the two in
   `TreeLogger`, and they are documented.

---

## 13. Operator decisions (settled — do not re-open)

1. **`api_common.log` is written but not exposed.** It is read later, from the server, by whatever
   means the operator chooses. No endpoint, no role check, no client surface in this slice. Write
   it, rotate it like the rest, and leave it alone.
2. **`LOG_DIR` is Code's call.** Confirm on the VPS where `api_server.log` lands today, pick a
   sensible absolute path (owned by the service account, outside the code tree so a deploy cannot
   wipe it), put it in `config.py`, and record the choice and the reasoning in the worklog.
3. **Start fresh.** The existing `api_server.log` and its `.1`–`.5` archives stay exactly where they
   are, untouched and unexposed. Nothing is migrated, renamed or deleted. The per-unit files begin
   empty. Say so in the worklog so nobody later mistakes the old file for a bug.

---

## 14. Known limitations — put these in the worklog, do not quietly fix them

- No visual verdict until the operator runs the harness test. Chip layout, wrapping, level colours
  in all three schemes, the detail pane: all unseen.
- `KBotChipBar` has not been round-tripped through the real Visual Studio designer. Slice 0025 hit
  exactly this wall; the programmatic `BeginInit`/`EndInit` test is the closest available proof and
  is not the same thing. Say so plainly.
- Search is not diacritic-insensitive.
- `TreeLogger`'s time-only stamps mean an entry's date is inferred from the file, not recorded.
- Per-unit routing is safe **because a single Gunicorn worker is enforced**. If that ever changes,
  two processes rotating one file is a real race.
- nginx logs remain invisible. Gunicorn output reaches files but is not exposed through the
  endpoints — it is read from the server, like `api_common.log`.
- Server log retention is 60 MB per unit. With many units, watch the disk.
