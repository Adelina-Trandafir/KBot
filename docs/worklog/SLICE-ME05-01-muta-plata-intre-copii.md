# SLICE-ME05-01 - KBotAdmin, ADECHIT: move one payment from a child to another child (17.11.2026)

Family ME05 opened 17.11.2026 (ME01-ME04 taken). KBotAdmin is the owner's private tool: no help topic. `KBotAdmin.vbproj` builds clean
(0 errors, 0 warnings); `py_compile` clean on the two Python files. NEVER run against a database, never seen on screen, not sent to the server.

## What changed and why

Request (Adelina): in KBotAdmin, an option that takes a sum from one child (a payment - other payments) and gives it to another child; when
the month is closed it must recalculate `ss_buget`; reuse the existing functions, ask before writing new ones.
Confirmed by Adelina: the WHOLE payment moves (no partial amount). The new function was confirmed by her in the same exchange.

- **Server** (`PYTHON/routes/admin_cli/adechit_muta_plata.py`, new): `analyse` (read only) and `move` (writes). One payment row of `AD_Plati`
  (receipt TIP 2 or other payment TIP 1) changes `IDP`, `IDZ`, `IDS`; its receipt / document is untouched (number, date, text as issued).
  Same subunit, same month. Typed value `IDPL;CNP_tinta[;IDS]` (IDPL is unique in the database - FKs reference it alone).
- **Reused as is** (nothing re-implemented): `Repository` (+ `acquire_lock(0)` / `acquire_lock(subunit)`, optimistic `update`),
  `service.temporal` (month rule of the web app), `bank_targets.ensure_payment_row` (target child without a month row: zero-day row, plus his
  statement when the month is closed), `service.rewrite_snapshot` (rebuilds `AD_SS_Buget` of a closed month from the payments), the
  `admin_cli` command pattern (`COMMANDS`, `COMMAND_PARAMS`, `SINGLE_DB_COMMANDS`, one transaction, `_journal`).
- **Closed month**: `rewrite_snapshot` runs for the source child AND the target child. **Open month**: nothing stored, calculated at read time.
- **Refused, nothing written**: payment cancelled; payment tied to a bank line (`AD_EXTRASE_ASOCIERI` keeps its own child / payer / month);
  target = same child; target CNP not found or found twice in the subunit; payer not active / not the target's, or several active payers
  and no IDS given; month too old (`temporal`); statement row missing or doubled.
- - **Flow in the screen** (Adelina, 17.11.2026; first version asked for an IDPL, she typed two CNPs - children, not payments - so it was redone):
  box 1 = the child the money leaves (CNP 13 digits or child ID), box 2 = the child who receives it (same forms, optionally `;IDS` of the payer).
  «Rulează» on «Mutare plata intre copii» (`adechit_plata_lista`, read only) lists the payments of the first child into a choice list (`cmbPlata`, new row
  `tlyPlata` of `tlyMain`); choosing one runs the analysis (`adechit_plata_analiza`, hidden, writes nothing: who gives, who receives, payer, documents, month,
  what is rebuilt, or rows «NU SE POATE MUTA»); the button «Mută plata» (`btnMuta`, hidden command `adechit_plata_muta`) appears only when the analysis
  lists no «NU SE POATE MUTA» row, tied to the exact payment and child analysed; editing either box hides the list and the button.
- **Commands** in `admin_cli.py`: `adechit_plata_analiza` (read) and `adechit_plata_muta` (writes). `CommandsView.vb` + `.Designer.vb` changed.

## Files touched
`PYTHON/routes/admin_cli/adechit_muta_plata.py` (new), `PYTHON/routes/admin_cli/admin_cli.py`, `KBotAdmin/Views/CommandsView.vb`, `KBotAdmin/Views/CommandsView.Designer.vb`.

## Unverified / deferred
- Never run: not on a copy of a database, not on screen. First run: `adechit_copil` for the source child (read IDPL), then the analysis, then the move
  on a copy; compare `AD_SS_Buget` of both children before / after.
- `admin_cli` connection assumed autocommit-off-free before `start_transaction` (same as the other repair commands); not exercised.
- Receipts keep the explanation text issued to the first payer; the receipt is not reissued.
- Payments from a bank statement are refused on purpose; moving them would need the bank association rewritten (not asked).
- The layout of the two boxes (`tlyParam`), of the payment list row (`tlyPlata`, row 2 of `tlyMain`) and the collapse of the second box were never seen on screen (the two boxes were seen once by Adelina, before the list existed).
- `adechit_muta_plata.py` must be copied to the server by hand together with `admin_cli.py`.
