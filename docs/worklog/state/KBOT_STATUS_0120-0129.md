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

---

## Slice 0123 — VERCON and ADECHIT reports in K-BOT.app (native pages)

Operator, 21.10.2026: bring the reports of the VERCON and ADECHIT sites into K-BOT as pages that look like the application (not a hosted web page), in two new ribbon tabs, VERCON and ADECHIT, each with a «Rapoarte» button; VERCON also holds «Trimite la Vercon». The reports stay computed by the server (same code as the site); K-BOT chooses the parameters, shows the PDF and saves the Excel. Decisions: ADECHIT first, VERCON with full parity afterwards; nothing may stay on the server (the snapshot system is kept but now deletes what is expired and releases a report when its viewer closes).

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0123-01 | Tabs VERCON (with «Trimite la Vercon», moved) and ADECHIT; bearer bridge for the ADECHIT read routes; snapshot sweep + release (shared by ADECHIT and VERCON); page «ADECHIT Rapoarte» (six reports, PDF in Adobe surface, Excel); help 0000-119 | GATA pe cod (KBot.App build 0 warnings / 0 errors, py_compile ok); **server NEDEPLOYAT, nimic rulat, pagina nevazuta pe ecran** | [SLICE-0123-01](../SLICE-0123-01-adechit-rapoarte.md) |
| 0123-03 | Corective dupa primul test (22.10.2026): bara de incarcare, subunitatea o singura data in antet (server: `report_identity.py`), Ctrl+2 o data dupa afisare (`FitWidthOnly`), cautarea copilului porneste si singura | GATA pe cod (build 0/0, py_compile ok); **server NEDEPLOYAT, nimic rulat**; cauza listei «Copil» gri NEDOVEDITA | [SLICE-0123-03](../SLICE-0123-03-corective-test-adechit-rapoarte.md) |
| 0123-04 | Ctrl+H / Ctrl+2 ale controlului Adobe (fereastra gazduita) hotarate de ferestrele Adobe: Ctrl+H doar cand panourile se vad, Ctrl+2 doar dupa ce Ctrl+H a prins si pagina s-a asezat; `FitWidthOnly` din 0123-03 scos | GATA pe cod (build 0/0), **nimic rulat**; numele/starea ferestrei panoului in mod citire NECAPTURATE | [SLICE-0123-04](../SLICE-0123-04-ctrl-h-dupa-ferestre.md) |
| 0123-02 | VERCON «Rapoarte» (23.10.2026): bridge `routes/vercon/kbot_bridge.py` + release route; page `VerconRapoarteForm` (Subunitate / Gestiune / An, tree combo of periods with months, dates, options, report tree, selection tree with lazy branches, Listare PDF, Export Excel); `AdvancedTreeControl.CompleteLazyLoad` / `SearchStarting`; help `contabil/vercon-rapoarte.md` | GATA pe cod (KBot.App build 0 warnings / 0 errors, py_compile ok); **server NEDEPLOYAT, nimic rulat sau vazut**; lazy tree and tick-an-unread-branch NEEXERSATE | [SLICE-0123-02](../SLICE-0123-02-vercon-rapoarte.md) |

### Open threads — 0123

- The server files of 0123-01 and 0123-02 (lists in the worklogs) are for the operator to publish; the bridges assume the K-BOT user name is the e-mail of the AD / VR roles.
- Other VERCON server files + the site's `reports.js` / `report-result.js` were being changed in the same tree by someone else during 0123-02 (OI reports rework, `tree_group`); the page copies the site's report-tree / selection rules as of 23.10.2026 and must follow if they change.
- Excel exports all columns (no column picker yet). VERCON «Rapoarte»: the period tree combo opens all years (the site opens the latest only).
- Ajutor de actualizat: none (done in 0000-119); ribbon captures to redo are listed in the 0000-119 worklog.
