# K-BOT — STATUS, slices 0120–0129

Everything recorded about each slice. The index in `../KBOT_STATUS.md` says what each slice is.

---

## Slice 0120 — DDF editor: the action row beside «Salvează documentul»

Part of the new main window recorded on 13.10.2026 (slices 0113–0120; decisions and the shared open threads in
[KBOT_STATUS_0110-0119.md](KBOT_STATUS_0110-0119.md), «Slices 0113–0120»).

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0120 | While page «Secțiunea A» is open, its row «Adaugă rând / Șterge rândul / state» moves into the editor's bottom row, beside «Salvează documentul» (`DdfEditSectiuneaAPage.AttachActionsTo` / `DetachActions`, `DdfEditForm.ActivatePage`); with 0113 «Renunță» is hidden (ribbon «Închide» / Esc; numbers released on close as before) | GATA pe cod (build of the merge 0/0), seen only in `KBOT_TEST`; help 0000-74 | [SLICE-0120](../SLICE-0120-ddf-randul-de-jos.md) |

### Open threads — 0120

- Tour `tur-ddf-editor` has a step on `DdfEditForm.btnRenunta` (now hidden): proposal in the 0000-74 worklog; tours
  not changed without the operator's OK.

---

## Slice 0121 — Supplier <-> angajament links by CodFiscal (PLAN)

Operator, 19.10.2026 (page «Recepții», KE16): an angajament picked for a supplier belongs to the supplier; the durable link is keyed by the normalized CodFiscal, not by the per-unit `IdPartener` / `CodPartener`. `Parteneri` is not altered. Full plan, decisions and facts read: [SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md](../SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md).

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0121-01 | Schema: two new tables (link by CodFiscal+CodAng; indicators by CodAI with ContBancar); `Parteneri` / `Parteneri_Coduri` untouched | SCRIS (`sql/P121_01_parteneri_angajamente.sql`), NERULAT: the operator runs it on AVACONT_SURSA + sync | [SLICE-0121-01-schema.md](../SLICE-0121-01-schema.md) |
| 0121-02 | Migrator: `ParteneriAng` -> new tables by CodFiscal, with the report (no CodFiscal, cross-unit conflicts, CodAI missing, duplicates) | CONSTRUIT (build 0 warn/0 err), NEPORNIT, netestat (operator: nu se poate testa acum) | [SLICE-0121-02-migrator.md](../SLICE-0121-02-migrator.md) |
| 0121-03 | «Parteneri» window + `parteneri_edit.py` on the new tables | CONSTRUIT (build 0 warn/0 err, py_compile), NEPORNIT, netestat; ajutor 0000-113 | [SLICE-0121-03-fereastra-parteneri.md](../SLICE-0121-03-fereastra-parteneri.md) |
| 0121-04 | Hooks: partner <-> DDF / angajament association fills the link and indicators if missing | CONSTRUIT (py_compile), NEPORNIT, netestat; route `POST /api/forexe/parteneri/angajament` for 0121-05 | [SLICE-0121-04-carlige-asociere.md](../SLICE-0121-04-carlige-asociere.md) |
| 0121-05 | Page «Recepții»: supplier's angajamente from the new tables, the pick saved, `_added` retired | CONSTRUIT (build 0 warn/0 err, py_compile), NEPORNIT, netestat; ajutor 0000-114 | [SLICE-0121-05-pagina-receptii.md](../SLICE-0121-05-pagina-receptii.md) |
| 0121-07 | One-time query KBotAdmin: copy the old `Parteneri_Coduri` into the new tables (`sql/0121_07_interogare_unica.sql`) | SCRIS, NERULAT (operatorul o ruleaza din «Interogari unice», dupa P121_01 + sync + fisierele Python) | [SLICE-0121-07-copiere-parteneri-coduri.md](../SLICE-0121-07-copiere-parteneri-coduri.md) |
| 0121-06 | Close: `Parteneri_Coduri` unread, dropped only at the operator's word | NEPORNIT | the plan file |

### Open threads — 0121

- 0121-01: DDL written (`sql/P121_01_parteneri_angajamente.sql`), not run. Operator: run on AVACONT_SURSA, sync to the units (KBotAdmin), refresh `MariaDB_Schema/`. No code uses the tables yet.
- 0121-03: never run against a database; until the Migrator (0121-02) fills the new tables the window shows empty codes. Server files (`partner_links.py`, `parteneri_edit.py`) go to the VPS by the operator.
- Operator, 19.10.2026: the Migrator part (0121-02) cannot be tested now - built only (Migrator FileVersion 1.21.0.0), never run: Access column names from `qFX_Ang_Part`, `LAST_INSERT_ID` fallback and the report figures to be checked on the first real «Verifică».
- The real content of `Parteneri_Coduri` (is `CodAng` filled?) not seen: run the check queries at the end of `sql/0121_07_interogare_unica.sql` before the run. 0121-06 waits for 0121-07 and the last unit's Migrator run.
- 0121-05: the picks of the page are durable now (supplier link by tax code); never run. A supplier whose invoices carry no tax code cannot have a pick saved.

---

## Slice 0122 — Partner only by CodFiscal, no IdPartener/CodPartener (ANALYSIS)

Operator, 20.10.2026: end the dependence of `FX_DDF_REV_SA`, `FX_DDF_REV_SB`, `FX_ORD_TBL` and `VC_NIR` on `Parteneri.IdPartener` (outside the Access migration), so `Parteneri` can later hold one row per CodFiscal. The next update is mandatory, so old clients are not a constraint. Full analysis, sub-slices and open decisions: [SLICE-0122-parteneri-fara-idpartener.md](../SLICE-0122-parteneri-fara-idpartener.md).

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0122 | Analysis of the IdPartener/CodPartener dependencies; plan 0122-01..07 (CodFiscal goes into `CodPartener`, `VC_NIR` gets the column) | PLAN decided by the operator, 20.10.2026 | [SLICE-0122](../SLICE-0122-parteneri-fara-idpartener.md) |
| 0122-01 | Column `VC_NIR.CodPartener` (`sql/P122_01_vc_nir_codpartener.sql`) + one-time query that puts the normalized CodFiscal in `CodPartener` on SA, SB, ORD_TBL, VC_NIR (`sql/0122_01_interogare_unica.sql`) | SCRIS, NERULAT; the operator runs the DDL, the sync, then the query from KBotAdmin | [SLICE-0122-01](../SLICE-0122-01-codfiscal-in-codpartener.md) |
