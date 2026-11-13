# SLICE-ME03-01 - KBotAdmin, «Comenzi» > ADECHIT > «Informatii copil» (12.11.2026)

Slice given by Adelina (12.11.2026, `/slice SLICE-MEnn-mm`, resolved to ME03-01: ME01 and ME02 are taken).
Built clean (`KBotAdmin.vbproj`: 0 errors, 0 warnings) and `py_compile` clean on both Python files. Nothing run against a database.

## What changed and why

Request: in KBotAdmin, page «Comenzi», a new section «ADECHIT» whose first command is «Informatii copil». It asks for a CNP and, for the
database selected in the tree, shows everything that happened with that child in the whole database: payments, receipts, bank statements,
months - everything that points to the IDP got from the CNP.

- **Server** (`PYTHON/routes/admin_cli/adechit_copil.py`, new; registered in `admin_cli.py` as `adechit_copil`, with the typed value «CNP copil»;
  a command with a typed value already refuses `<TOATE BAZELE>`). Read only. The CNP finds the child row(s) of `AD_Platitori`
  (`TRIM(CNP) = CNP`; one row per subunit the child sits in, and the same subunit may hold the CNP twice, so ALL rows found are followed).
  Every other table is filtered with the `(SubunitId, IDP)` pair of those rows, as the foreign keys are, so an equal IDP of another subunit cannot leak in.
  Blocks, in this order: summary (rows per table) · the child (group, entry / exit, left) · his payers (`AD_Platitori_sub`; the portal
  access code is shown only as da / nu) · the portal account of the parents (`AD_PortalParents`) · group history (`AD_Platitori_Istoric`) ·
  months (`AD_Prezenta` with month, group, daily rate) · stored monthly statement (`AD_SS_Buget`) · payments and cash-ins (`AD_Plati`, TIP 2 =
  cash-in, with the receipt and the bank-statement rows of each) · receipts (`AD_Chitante`) · other documents (`AD_AlteDoc`) · returns
  (`AD_Retur`) · set-offs (`AD_Compensare`, the child as debit or credit, with the other child) · bank statement lines
  (`AD_EXTRASE_ASOCIERI` + `FX_Extrase`). Each list stops at 2000 rows and says so.
- **CNP not a child's**: if no child has it but a payer (`CNP_Platitor`) does, the answer names the payer and his children instead of «nothing found».
- **Not an ADECHIT database** (no `AD_` tables): refused with a readable sentence.
- **Client** (`KBotAdmin/Views/CommandsView.vb`): `CommandApps` puts `adechit_copil` under the existing `ADECHIT` root (the root was already in `AppRoots`
  but held no command). The typed-value box, the single-database rule and the multi-block output are the generic ones `profil_produs` already uses.

## Files touched
- `PYTHON/routes/admin_cli/adechit_copil.py` (new)
- `PYTHON/routes/admin_cli/admin_cli.py` - handler `_c_adechit_copil`, `COMMANDS`, `COMMAND_PARAMS`
- `KBotAdmin/Views/CommandsView.vb` - `AdechitChildName`, `CommandApps`
- `docs/worklog/SLICE-ME03-01-...md`, `docs/worklog/state/KBOT_STATUS_ME03.md`, index line in `docs/worklog/KBOT_STATUS.md`

## Unverified / deferred
- NEVER RUN: not against a database, not on screen. The SQL was written from `MariaDB_Schema/000_DEMO.sql` (tuple `IN (subquery)`,
  correlated `GROUP_CONCAT`, the `AD_PortalParents.CNP` collation conversion are the parts most worth a first look on the server).
- Which payments count as «plati» and which as «incasari» follows `ADECHIT/CONTRACT_CALCULE.md` (TIP 2 = cash-in, others = payments); NULL TIP is shown as «TIP ?».
- Amounts are printed as the database stores them (`AD_Plati.Plata` is an integer, `AD_Retur.Suma` a double).
- The Jurnal (`AVACONT_COMUN.Jurnal`) is not read: it keys on accounts, not on children.
- Never deployed (rule: the operator copies the server files by hand). `/ssh` registration of the two Python files only after Adelina confirms a test.
- Help: none (KBotAdmin is the owner's private tool).
