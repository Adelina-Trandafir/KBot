# SLICE-ME01-13 - KBotAdmin «Comenzi»: verificare lanturi de receptii si «Repara receptiile» (02.11.2026)

Owner's tool only (KBotAdmin, never published). Same pattern as [ME01-11](SLICE-ME01-11-comanda-ddf-antete.md): a read-only check, then one button that writes for ONE code.

## Why
AAB2EBFXAAE (038_SCGP): the association guard refused every full download of the angajament («ultimul instantaneu nu are valoarea receptiei»), and its advice (move a snapshot) was right for one reception and wrong for the other. The FOREXE history in `FX_Istoric` showed two different causes:
- **Reception 58** («dobanda legala penalizatoare transa 4», AA2 3553) carried the values of reception 59 (AAB 11543), the other reception of the same day. Step 4b matched on the day only and took the first candidate for both rows (F30) until the same-day matching of 30.09.2026 (`_potriveste_ziua`). Its chain (snapshot 73) was right; the reception row was wrong.
- **Snapshots 75 and 76** (19213, «... si contrib handica neincadrati») sat on reception 60 instead of reception 61. Reception 60 and 61 values were right. No reception had 19213, so the automatic pass could not have placed them: placed by hand (or an older path), then frozen by the ordonantari.

Neither shows as an error anywhere else: a placement of an unplaced snapshot and the step 4b update of a reception are not judged against ordonantari (the freeze of ME/0048 protects only links already placed).

## What it does
**Step 1 - «Verificare lanturi de receptii»** (`receptii_lanturi`, one database, read only). A reception is open when its newest snapshot has another total, or another line the reception really carries (F15; a zero line is not a line). Open receptions are sorted:
1. **instantaneu pe alta receptie** - the snapshot's name (`FX_Receptii_H.Descriere`, the reception name FOREXE copies into the snapshot) is not the reception's name but is the name of exactly ONE other non-deleted reception of the angajament. Repairable when, after the move, both chains close and the reception keeps at least one snapshot.
2. **receptie rescrisa de gemena** - the reception's name equals its last snapshot's name, and another reception of the same `DataR`, a different name, the same total and the same non-zero lines has a chain that closes. Repairable when every indicator of the snapshot has exactly one `FX_Receptii_RHR` row on the reception.
3. **alt caz** - listed only, never repaired.
The summary gives cases and repairable cases per kind; the detail lists every case with what would be done. With repairable cases the code box and the button «Repara receptiile» appear (tied to this command and database).

**Step 2 - «Repara receptiile»** (`receptii_lanturi_repara`, hidden from the list; WRITES, one database, one code, confirmation first). ONE transaction (READ COMMITTED; the code's receptions, snapshots and ordonantare lines locked; the findings re-made under the lock; every `UPDATE` checks its row count):
- cause 1: `FX_Receptii_H.IDRR` of the moved snapshots; then `recalculeaza_final` (Final/Partial + HASH) and `step4d_calculeaza_dif` (DIFH/DIFHC, DIF/DIFC) on both chains - the same functions the association window uses;
- cause 2: `FX_Receptii_R.SumaAntet` and the `Valoare` of each `FX_Receptii_RHR` line set to what the last snapshot says (indicators the snapshot does not name go to 0). `TipReceptie`, `CreditBugetar` and `ValoareN` are left as they are;
- ordonantare lines (only after a move): the reception figure each line reads (`SUM(FX_Receptii.DIF)` per indicator up to the ordonantare day, the sum of `qFX_ORD_REC_ANT`) is taken before and after; a line whose stored `TotalReceptii` equals the figure read before gets `TotalReceptii` and `Ramas` moved by the difference (`Ramas = TotalReceptii - PlatiAnt - Valoare`). A line whose stored figure was another is NOT touched and is listed. The ordonantari are printed again, not generated again;
- the Jurnal line (`receptii_lanturi_repara`) holds what was done and the values before.
The result shows what was done, the ordonantare lines changed or left, and the chains still open for the code.

## Files
- `PYTHON/routes/admin_cli/receptii_lanturi.py` (new) - `_load`, `_findings`, `check`, `repair`.
- `PYTHON/routes/admin_cli/admin_cli.py` - the two commands, `COMMAND_PARAMS`, `SINGLE_DB_COMMANDS`.
- `KBotAdmin/Views/CommandsView.vb`, `CommandsView.Designer.vb` - `btnLanturi` (same cell as `btnRepair` / `btnExplain`), `ConfigureLanturi`, `BtnLanturi_Click`, the expectation text, `HiddenCommands`.
- `KBotAdmin/README.md`; `sql/corectie_AAB2EBFXAAE_receptii.sql` (the one-time script the command replaces; kept as reference, not to be run).

## Verification
`dotnet build KBotAdmin\KBotAdmin.vbproj`: 0 errors, 0 warnings; both Python files pass `py_compile`. No tests written or run (operator rule). Adelina ran the commands on the server on 02.11.2026 and confirmed them correct and complete.

## Limits
- The name match (`FX_Receptii_H.Descriere` = reception name) is the only identity used; receptions whose names repeat (monthly «salarii») give no case of kind 1, by design.
- `CreditBugetar` / `ValoareN` of a rewritten reception keep the twin's figures (the originals are not in the database).
- Ordonantare lines already printed or signed keep what is on paper; the stored figures change, the documents must be listed again.
- Server files go on the server by hand; KBotAdmin must be republished for `btnLanturi`.
