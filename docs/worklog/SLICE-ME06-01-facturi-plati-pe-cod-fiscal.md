# SLICE-ME06-01 - KBotAdmin, AVACONT: invoices and payments of one CodFiscal, per SS (19.11.2026)

Family ME06 opened 19.11.2026 (ME01-ME05 taken; first free number from `/slice SLICE-MEnn-mm`). KBotAdmin is the owner's private tool: no help topic.
`KBotAdmin.vbproj` builds clean (0 errors, 0 warnings); `py_compile` clean on the two Python files. NEVER run against a database, never seen on screen,
not sent to the server.

## What changed and why

Request (Adelina): «Facturi si plati» > «Facturi an curent» showed no invoice for a partner that surely had unpaid ones. She asked for a command in KBotAdmin
that, for a CodFiscal, shows per SS which invoices / payments the accounting operations hold, and (second message) a combo with the partners available on
the chosen database instead of a typed CodFiscal.

- **Server** (`PYTHON/routes/admin_cli/facturi_plati_cui.py`, new), read only:
  - `check`: finds every `Parteneri` row with that normalized CodFiscal (letters and digits, upper case, leading «RO» dropped: the rule of
    `partner_links.normalize_cui`, written once more here as a SQL expression so the module does not load the whole FOREXE package), then every `TZ_Oper` with a
    debit or credit line on one of those partners. Blocks: partners found; summary per subunit + year (SS); invoices; payments; other operations.
  - Same rule as the window (`facturi_plati.py::_facturi_deschise`): invoice = `Tip = 'E'` with a credit line of the partner; paid = `TZ_PlatiFacturi.Platit`
    summed by `IdEvidentiere`; rest = value - paid; column «in lista» = the rest is not zero (what «Facturi si plati» shows).
    Payment = `Tip = 'P'` with a debit line of the partner, with `TZ_OPuri.NrOP` when it has an order.
  - «Alte operatii ale partenerului» lists the operations of any other Tip: an invoice carrying another Tip (migrated from Access) is the likely reason a
    partner shows nothing in the window, and this block makes it visible instead of leaving it out.
  - `partners`: the choice list, one row per normalized CodFiscal (name, how many partner rows, the SS of the units they sit in); hidden partners and
    partners without CodFiscal are left out.
  - Lists cut at 2000 rows, with a note. Every sum is aggregated in a sub-select before the joins (no row multiplication).
- **Commands** in `admin_cli.py`: `facturi_plati_cui` (param label «CodFiscal», single database) and `facturi_plati_cui_parteneri` (hidden behind the combo,
  single database).
- **Screen** (`CommandsView.vb` + `.Designer.vb`): new row `tlyPartener` (index 3 of `tlyMain`, collapsed unless the command is chosen) with `cmbPartener`
  (`KBotComboBox`, `Editable` + `FindAsYouType`, so typing part of the name or of the CodFiscal narrows the list). For this command the typed box is hidden;
  the combo is filled from the selected database each time the command or the database changes; «Ruleaza» sends the chosen CodFiscal and refuses when
  nothing is chosen or the list belongs to another database. The command sits under AVACONT in the command list.

## Correction the same day (19.11.2026, from the first real output)

Adelina ran the command on `000_DEMO` for the CodFiscal 21015411: besides the invoices of AVATAR SOFT it listed `stat burse` / `stat salarii` of ISJ. Cause (read
from the data she pasted): the special partners `BURSE` (IdPartener 272, code 1013) and `FOND AFERENT PERSOANELOR CU HANDICAP` (243, code 023) carry in the CodFiscal
cell the code of the UNIT (21015411), not one of their own; grouping partners by CodFiscal therefore merged them with the supplier. Her rule: a CodFiscal equal to the code of
the unit links NOTHING; and such partners should all be of `Parteneri.Tip` 2 (internal: not a firm by CUI, not a person by CNP).
- `facturi_plati_cui.py`: the unit's code is read from `AVACONT_COMUN.Unitati_Date` (else `Unitati.CF`); the command refuses a CodFiscal equal to it; the partner
  lookup and the choice list leave out the partners with that code and the partners of Tip 2.
- Recorded as project information on request (19.11.2026): the table «`Parteneri`: ce înseamnă `Tip` și `CodFiscal`» in `CLAUDE.md` (section «Database schema»): Tip 1 + CUI = firmă furnizor,
  Tip 1 + CNP = persoană fizică (beneficiar Salarii, Burse, Transport...), Tip 2 = partener intern, CodFiscal ignorat.
- NOT changed (decision and slice are hers): the other places that match by CodFiscal still treat the unit's code as a key (the migrator's fallback
  `AvcOperatiiStep.vb` `ParteneriDupaCui`, `EF_` links, `Parteneri_Angajamente`, `partner_links.py`), and the existing special partners are not set to Tip 2.

## Unverified

- Nothing ran: not the SQL on a real database, not the combo on screen (does `FindAsYouType` narrow by CodFiscal as well as by name - the caption holds both).
- `REQUIRED` columns of `facturi_plati_cui.py` were taken from `MariaDB_Schema/000_DEMO.sql` and `sql/AVACONT/.../AV02_02_plati_op_setari.sql`.
- Payments of the previous Access year (`TZ_PlatiFacturi.IdEvidentiereAnterior`) are not counted in «platit», exactly as in the window.
- `CommandsView.vb` was over 1000 lines before this slice; it was split by responsibility afterwards in `SLICE-ME06-02` (the code described here now sits in `CommandsView.Partner.vb` and `CommandsView.Names.vb`).

## To do by hand

Copy `facturi_plati_cui.py` and `admin_cli.py` to the server (Adelina, by hand).
