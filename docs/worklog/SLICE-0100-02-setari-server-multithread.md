# SLICE-0100-02 — `Setari` table: the server decides multi-thread (operator request, 01.10.2026)

Branch: `SLICE-0100-Multithreading`. Changes the decision of slice 0100 that multi-thread is an advanced option.

## What changed and why

The operator wants to switch multi-thread on and limit the tabs **from the server**, per database. So:

**Data.** A new table `Setari` on every database (`sql/0100_02_setari.sql` = DDL + default rows on `AVACONT_SURSA`, the
template new units are cloned from; `sql/0100_02_setari_toate_bazele.sql` = a block that walks
`AVACONT_COMUN.Unitati.DC` and creates the table + inserts the default rows in every existing unit database).
Columns: `Cheie` (PK, ASCII), `TextVizibil` (what the operator may read), `Valoare` varchar(255) (one column for int /
date / text), `Tip` enum('int','date','text'). Defaults: `Multithread = 0`, `Multithread_Max = 1`.
`INSERT IGNORE`: a value changed by hand survives a second run.

**Server.** `PYTHON/routes/setari.py` (registered in `main.py`): `GET /api/setari` (bearer) returns the rows of the
session's database, `Valoare` typed by `Tip` (a value that does not fit → null, logged). No table → 200 with no rows.

**Client.**
- `KBot.Common/ServerSettings.vb`: static holder, filled by `Apply`, `Changed` event; `MultithreadAllowed`
  (`Multithread <> 0`), `MultithreadMax` (≥ 1), generic `GetInt` / `GetDate` / `GetText`. No row = off.
- `KBot.Api/ISetariApi.vb` + `ApiClient.Setari.vb`: `GetServerSettingsAsync`.
- `KbotForm.Parallel.vb` `LoadServerSettingsAsync`, called after the periods at login (`KbotForm.vb`) and at a change
  of unit (`KbotForm.Units.vb`); a failure is logged and leaves everything off.
- `AppSettings.MultiThreadInEffect` = server allows AND the operator's switch (the advanced options no longer count);
  new `DownloadThreadsCeiling` = `min(10, Multithread_Max)`; `DownloadThreadsInEffect` is clamped to it.
- The «Descărcări multiple» group left «Setări › Aplicație»; new page `SetariMultithreadView` (nav key
  `multithread`, «Descărcări multiple»), shown by `SetariForm` only while `Multithread = 1` (follows
  `ServerSettings.Changed`; if it is on screen when the server turns off, hands over to «Aplicație»). Its main
  checkbox is enabled from `Multithread`, the tabs field is limited by `Multithread_Max`.

## Decisions taken (assumptions)
- The value is read from the database the operator logged in to (the unit), not from a global place.
- Read-only for K-BOT: changed on the server with an `UPDATE`, effective at the operator's next login / unit change.
- Defaults 0 / 1 as asked; the operator's saved `DownloadThreads` (default 3) is capped, not rewritten.

## Files touched
`sql/0100_02_setari.sql`, `sql/0100_02_setari_toate_bazele.sql`; `PYTHON/routes/setari.py`, `PYTHON/main.py`;
`src/KBot.Common/ServerSettings.vb` (new), `AppSettings.vb`; `src/KBot.Api/ISetariApi.vb`, `ApiClient.Setari.vb` (new);
`src/KBot.App/` `KbotForm.vb`, `KbotForm.Units.vb`, `KbotForm.Parallel.vb`, `Setari/SetariForm(.Designer).vb`,
`Setari/SetariAplicatieView(.Designer).vb`, `Setari/SetariMultithreadView(.Designer).vb` (new).

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. No tests written or run (operator rule), nothing run
on screen, SQL not run, server route not run.

## Unverified / deferred
- **Deploy order:** the DDL (both files) before `setari.py` reaches the VPS; until then the route answers «no rows» and
  multi-thread stays off everywhere. `0100_02_setari_toate_bazele.sql` uses `BEGIN NOT ATOMIC`, `PREPARE` and a cursor
  — never run; run it as root in HeidiSQL / `mysql`. Check its report line.
- Whether the provisioning job copies **rows** of `AVACONT_SURSA` into new units (it clones the tables) is unchecked;
  a new unit without the default rows reads as «off» — safe.
- The new page and the removed group were not seen on screen (designer hand-written).
- Help: done as 0000-36 (`SLICE-0000-36-ajutor-setari-multithread.md`); no screenshot taken (needs K-BOT on a unit with Multithread = 1).
