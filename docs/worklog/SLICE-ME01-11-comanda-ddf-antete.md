# SLICE-ME01-11 - KBotAdmin «Comenzi»: verificare documente de fundamentare (FX_DDF), «Explică» și «Repară codul» (02.11.2026)

Owner's tool only (KBotAdmin, never published). Read only: nothing here writes to a database.

## Why
AAB2SGAKRH4 (user log, 02.11.2026): the editor refused to add or save a revision because the code had two FX_DDF headers (IDDF 8 with 12 revisions and 22 ordinances, IDDF 9 with one revision, NumarRev 7, one SA line, one SB line and one reservation). Header 9 looked like a duplicate; deleting it would have deleted revision 69 and, through `FX_Rezervari.IDREV ... ON DELETE CASCADE`, its reservation. The fix was done by hand (move the revision under 8, then delete the empty header 9). This command finds the same family of problems and shows what a deletion would do, in two steps (operator's request).

## How it works
Step 1 - command «Verificare documente de fundamentare» (`ddf_antete`, one database only): a summary (count per kind of problem) and one line per case. Kinds: several headers under one code; `FX_Angajamente.IDDF` pointing at a document that is not the code's; the same `NumarRev` twice in one document; a revision carrying another code than its document.
Step 2 - when the summary has cases, the code box (the existing `txtParam`) and the button «Explică» appear, tied to this command and this database. The button runs `ddf_antete_explica` (hidden from the list) with the typed code and prints three blocks in plain words: what the database holds for the code (headers, revisions), what deleting each header would do (counts of what disappears, what blocks, read from the live foreign keys: CASCADE / RESTRICT), and a conclusion (blocked by ordinances; empty header that can go; header with revisions that must be moved first, whether the `NumarRev` values clash or fit a gap in the main header). When the conclusion ends with `REPARABIL`, the button «Repară codul» appears (tied to that code and database; typing another code hides it).
**Same-revision key (operator, 02.11.2026):** a revision of an extra header with the same `DataRev` AND the same total (`SUM(ValCur)` of its section A lines, rounded to 2 decimals - the application's own `TotalRevizie`; both must have at least one A line) as ANY revision of the main header is the same revision: it is deleted (with its A/B/PRT lines; its content goes to the Jurnal first), not moved, and its reservations (`FX_Rezervari.IDREV`) are re-pointed to the revision that stays (operator, 02.11.2026: a reservation exists only once - `FX_Rezervari.IDH` is unique - so deleting it with the revision, which the cascade would do, would lose it). The revision itself is deleted, not moved; the reservation's row goes to the Jurnal first. Refused (reason shown) if such a revision also holds attachments, a PDF or PDF signatures, which would be lost. Revisions without a match move as before (the `NumarRev` rules apply to them only). The explanation lists every revision of the extra header with its decision.
Step 3 - «Repară codul» runs `ddf_antete_repara` (hidden from the list; WRITES, one database, one code, confirmation first). In ONE transaction (READ COMMITTED, the code's FX_DDF and FX_Angajamente rows locked, eligibility re-checked under the lock): the revisions of the extra headers move under the header `FX_Angajamente.IDDF` points to (`FX_DDF_REV`, and `IDDF` of `_SA`/`_SB`/`_PRT`/`_ATT`; reservations, PDFs and signatures follow by `IDREV`), then every extra header left with nothing on it is deleted. It refuses (nothing changes, reasons shown) when: the angajament points at none of the headers, the headers have different DC, an extra header has ordinances / document partners / received-invoice links (they do not move with a revision) or a RESTRICT dependent, a revision has no `NumarRev`, or a `NumarRev` is already taken in the main header or in another header being moved. After the move it recounts everything left on the extra header and rolls back if anything remains. The Jurnal line (`ddf_antete_repara`) holds the moved IDREVs and the full values of the deleted headers, enough to put them back by hand. The result then shows the explanation again (now one header).

## Files
- `PYTHON/routes/admin_cli/ddf_headers.py` (new) - `check`, `explain`, `repair`, `_eligibility`.
- `PYTHON/routes/admin_cli/admin_cli.py` - the three commands, `COMMAND_PARAMS`, `SINGLE_DB_COMMANDS`.
- `KBotAdmin/Views/CommandsView.vb`, `CommandsView.Designer.vb` - `btnExplain`, `btnFix` (new 6th column of the bar), `ConfigureExplain`, `ConfigureFix`, `BtnExplain_Click`, `BtnFix_Click`, the expectation text, `HiddenCommands`.

## Test results
`dotnet build KBotAdmin\KBotAdmin.vbproj`: 0 errors, 0 warnings. The two Python files pass `py_compile`. No tests written or run (operator rule); the commands were NOT run against any database.

## Unverified / deferred
- **Server files must be pushed by hand** (`ddf_headers.py`, `admin_cli.py`); KBotAdmin asks the server for the new commands and does not list them until they are there.
- Never run: the SQL and the wording of the three blocks are unseen against the real data (AAB2SGAKRH4 on 038_SCGP is the case to try; expected: step 1 lists it as «antete multiple», the explanation says IDDF 9 holds one revision with a reservation and that NumarRev 7 fits the gap in IDDF 8 - or, after the by-hand repair, step 1 no longer lists it).
- Table and column names come from `MariaDB_Schema\000_DEMO.sql` (22.09.2026); a table missing on the target is skipped in the counts, a missing required column stops the command with a message.
- Not checked on purpose: `FX_Angajamente.IDDF` empty while documents exist (could be many old rows; would be noise).
- **The repair has never run.** Not even on a copy. Moving rows updates `DataModificare` (ON UPDATE) of the moved revisions and lines. Which header is the main one is decided by `FX_Angajamente.IDDF` alone; if that pointer is the wrong one, the repair would move the right document under the wrong header (the explanation shows `[principal]` first - read it before pressing).
- **Reservations of a dropped duplicate revision are re-pointed to the revision that stays** (confirmed by the operator, 02.11.2026: they are unique, deleting them would lose them). With a reservation moved, `Sumele` of the kept revision's lines and the reservation totals are not recomputed; the count and the row are in the Jurnal.
- A gap in `NumarRev` stays after a duplicate is dropped (no renumbering).
- "Same total" uses section A only (as the tree does); section B is not compared.
- AAB2SGAKRH4 may already have been repaired by hand (script of 02.11.2026): then step 1 no longer lists it and there is nothing to try it on. Try on a copy of the database first.
- `FX_DDF` rows are read with `SELECT *`; the deleted headers' values go into the Jurnal as Python dict text.
- Not in the K-BOT help (KBotAdmin is the owner's private tool). The prevention (a guard in `_scrie_graf` that refuses a second header for a code) is not done.

## Deploy
- Server: `PYTHON/routes/admin_cli/ddf_headers.py` (new) and `admin_cli.py` (also holds the ME01-08 `db-*` verbs). Both are run per command from SSH, so no service restart is expected (not confirmed here).
- Local: KBotAdmin must be republished (`publish-admin.ps1`, page «Scripturi») for `btnExplain` / `btnFix` to exist.
- Registered for Git on 02.11.2026 (`tools\Register-DeploymentTask.ps1 -Slice SLICE-ME01-11`) after Adelina validated the queries; publish from KBotAdmin, commit slice SLICE-ME01-11.
