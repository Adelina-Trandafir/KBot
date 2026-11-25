# SLICE-ME04-03 - KBotAdmin, comenzi > FOREXE: «Verificare extrase lipsa» + «Repara» (16.11.2026)

Same family as ME04-01/02 (KBotAdmin, owner's private tool). `KBotAdmin.vbproj` builds clean (0 errors, 0 warnings); `py_compile -W error`
clean on `extrase_lipsa.py` and `admin_cli.py`. Never run against a database, never seen on screen.

## What changed and why

Trigger: `TZ521102968922` was missing from `FX_Extrase` although the statement XML holds it. Cause: two payments of one payer, same day, same
document number, same amount, differing only by the reference; the import before AD21-02 had no reference in its dedup key and dropped the
second one. Files imported before that fix are short of operations that their XML (kept in `FX_Extrase_F.XML`) still holds.

Request: a command in `comenzi\FOREXE`, on one database: step 1 reads the XML of `FX_Extrase_F` again and finds what is missing in
`FX_Extrase_H` and `FX_Extrase`; step 2, repair, brings the missing ones.

- **Server** (`PYTHON/routes/admin_cli/extrase_lipsa.py`, new): `check` (read only) and `repair` (writes). Both walk every `FX_Extrase_F` row with
  the SAME rules as the import (`routes/forexe/extrase.py`), imported lazily: a header is missing when the file has no `FX_Extrase_H` row for the
  same `Cont`; an operation is missing when its natural key (DataDoc, NrDoc, platitor_cui, suma_debit, suma_credit, Referinta) is not in
  `FX_Extrase` at all (global, like the import; a key repeated inside the XML counts once). `repair` writes with the import's own
  `_scrie_antet` / `_scrie_operatiune`, so unit, classification, partner and HASH come out as the import would have set them. One transaction
  (InnoDB required), then `leaga_automat_protejat` as the import does, one Jurnal line (`extrase_lipsa_repara`) with the file ids and the key of every
  operation written, then the check is shown again inside the same transaction. `FX_Extrase_F` is never written; nothing already present is touched.
  Files without XML or with invalid XML are listed, not touched.
- **Registration** (`admin_cli.py`): `extrase_lipsa` (read only) and `extrase_lipsa_repara` (writes); both single-database.
- **Client** (`CommandsView.vb`): `extrase_lipsa` is listed under FOREXE; `extrase_lipsa_repara` is hidden and runs from the «Repară» button that
  appears after the check when the summary shows `reparabile` > 0 (same path as «Clasificatii dublate»); confirmation text names what is written.
  The expectation text for the command was added.

## Files touched
- `PYTHON/routes/admin_cli/extrase_lipsa.py` (new)
- `PYTHON/routes/admin_cli/admin_cli.py`
- `KBotAdmin/Views/CommandsView.vb`
- this file, `state/KBOT_STATUS_ME04.md`, index line in `KBOT_STATUS.md`

## Unverified / deferred
- NEVER RUN. The two server files must be copied by hand to the server (rule: no deploy by me).
- Not run on the dependency side either: `repair` imports `routes.forexe.extrase` (loads the whole FOREXE package) the way `receptii_lanturi.py`
  already does with other `routes.forexe` modules; whether it loads in the CLI process on the server is unproven.
- A header that already exists keeps its stored `IdUnitate` / `IdClsf`; the new operations of that header take them from it (like their siblings).
- No lock against a robot import running at the same moment: both could write the same operation. Run the repair when no download is going.
- `/ssh` registration only after Adelina confirms a test.
- Help: none (KBotAdmin is the owner's private tool).
