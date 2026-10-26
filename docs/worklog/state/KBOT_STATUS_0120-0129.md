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
| 0121-01 | Schema: legături și indicatori | SCRIS (`sql/P121_01_parteneri_angajamente.sql`), NERULAT: the operator runs it on AVACONT_SURSA + sync | [0121-01](../SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md#slice-0121-01) |
| 0121-02 | Migrator: conversie și raport | CONSTRUIT (build 0 warn/0 err), NEPORNIT, netestat (operator: nu se poate testa acum) | [0121-02](../SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md#slice-0121-02) |
| 0121-03 | Fereastra Parteneri: noile legături | CONSTRUIT (build 0 warn/0 err, py_compile), NEPORNIT, netestat; ajutor 0000-113 | [0121-03](../SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md#slice-0121-03) |
| 0121-04 | Asociere DDF/furnizor: hook-uri | CONSTRUIT (py_compile), NEPORNIT, netestat; route `POST /api/forexe/parteneri/angajament` for 0121-05 | [0121-04](../SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md#slice-0121-04) |
| 0121-05 | Recepții: alegere persistentă pe furnizor | CONSTRUIT (build 0 warn/0 err, py_compile), NEPORNIT, netestat; ajutor 0000-114 | [0121-05](../SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md#slice-0121-05) |
| 0121-07 | Copie unică din vechile coduri | SCRIS, NERULAT (operatorul o ruleaza din «Interogari unice», dupa P121_01 + sync + fisierele Python) | [0121-07](../SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md#slice-0121-07) |
| 0121-06 | Închidere condiționată de probe și operator | NEPORNIT | [0121-06](../SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md#slice-0121-06) |

### Open threads — 0121

- [Instalare, probele Migratorului și conținutul vechi nevăzut](../SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md): schema/interogările nu au fost rulate la checkpoint, iar 02…05 nu au fost pornite. 07 completează ruta de preluare a codurilor vechi; 06 așteaptă rezultatul, ultima unitate și cuvântul operatorului.

---

## Slice 0122 — Partner only by CodFiscal, no IdPartener/CodPartener (ANALYSIS)

Operator, 20.10.2026: end the dependence of `FX_DDF_REV_SA`, `FX_DDF_REV_SB`, `FX_ORD_TBL` and `VC_NIR` on `Parteneri.IdPartener` (outside the Access migration), so `Parteneri` can later hold one row per CodFiscal. The next update is mandatory, so old clients are not a constraint. Full analysis, sub-slices and open decisions: [SLICE-0122-parteneri-fara-idpartener.md](../SLICE-0122-parteneri-fara-idpartener.md).

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0122 | Plan: CodFiscal în CodPartener, fără dependența de id | PLAN decided by the operator, 20.10.2026 | [0122](../SLICE-0122-parteneri-fara-idpartener.md#slice-0122-plan) |
| 0122-01 | Conversia datelor și corecția 01b | DDL on all databases; queries `0122_01` + `0122_01b` (`sql/0122_01b_interogare_unica.sql`) + checks (`sql/0122_01_verificari.sql`) run CLEAN on ONE database (23.10.2026), the others still to be done | [0122-01](../SLICE-0122-parteneri-fara-idpartener.md#slice-0122-01) |
| 0122-02 | Server DDF/ORD: CodFiscal normalizat | CONSTRUIT (py_compile), NEPORNIT, netestat; client unchanged | [0122-02](../SLICE-0122-parteneri-fara-idpartener.md#slice-0122-02) |
| 0122-03 | Cititori și protecția la ștergere | CONSTRUIT (py_compile), NEPORNIT, netestat; the old by-id test stays until the keys are dropped | [0122-03](../SLICE-0122-parteneri-fara-idpartener.md#slice-0122-03) |
| 0122-08 | Ascunderea partenerilor nefolosiți, cu previzualizare | SCRIS, NERULAT; nothing is deleted, reversible; preview first | [0122-08](../SLICE-0122-parteneri-fara-idpartener.md#slice-0122-08) |

---

## Slice 0123 — VERCON and ADECHIT reports in K-BOT.app (native pages)

Operator, 21.10.2026: bring the reports of the VERCON and ADECHIT sites into K-BOT as pages that look like the application (not a hosted web page), in two new ribbon tabs, VERCON and ADECHIT, each with a «Rapoarte» button; VERCON also holds «Trimite la Vercon». The reports stay computed by the server (same code as the site); K-BOT chooses the parameters, shows the PDF and saves the Excel. Decisions: ADECHIT first, VERCON with full parity afterwards; nothing may stay on the server (the snapshot system is kept but now deletes what is expired and releases a report when its viewer closes).

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0123-01 | Tabs VERCON (with «Trimite la Vercon», moved) and ADECHIT; bearer bridge for the ADECHIT read routes; snapshot sweep + release (shared by ADECHIT and VERCON); page «ADECHIT Rapoarte» (six reports, PDF in Adobe surface, Excel); help 0000-119 | GATA pe cod (KBot.App build 0 warnings / 0 errors, py_compile ok); **server NEDEPLOYAT, nimic rulat, pagina nevazuta pe ecran** | [SLICE-0123-01](../SLICE-0123-01-adechit-rapoarte.md) |
| 0123-03 | Corective dupa primul test (22.10.2026): bara de incarcare, subunitatea o singura data in antet (server: `report_identity.py`), Ctrl+2 o data dupa afisare (`FitWidthOnly`), cautarea copilului porneste si singura | GATA pe cod (build 0/0, py_compile ok); **server NEDEPLOYAT, nimic rulat**; cauza listei «Copil» gri NEDOVEDITA | [SLICE-0123-03](../SLICE-0123-03-corective-test-adechit-rapoarte.md) |
| 0123-04 | Ctrl+H / Ctrl+2 ale controlului Adobe (fereastra gazduita) hotarate de ferestrele Adobe: Ctrl+H doar cand panourile se vad, Ctrl+2 doar dupa ce Ctrl+H a prins si pagina s-a asezat; `FitWidthOnly` din 0123-03 scos | GATA pe cod (build 0/0), **nimic rulat**; numele/starea ferestrei panoului in mod citire NECAPTURATE | [SLICE-0123-04](../SLICE-0123-04-ctrl-h-dupa-ferestre.md) |
| 0123-05 | Filele VERCON si ADECHIT apar doar daca unitatea are date in tabelele VC_ / AD_ (randuri calculate `AdechitDate` / `VerconDate` in `GET /api/setari`; `ServerSettings`; `AvacontMain.ApplyAppTabs`) | GATA pe cod (build 0/0, py_compile ok); **server NEDEPLOYAT (setari.py), nimic rulat**; ce inseamna «are date» de confirmat | [SLICE-0123-05](../SLICE-0123-05-file-doar-cu-date.md) |
| 0123-06 | Arborele: textul din dreapta (dupa ~~~) care nu incape dupa cel din stanga nu mai dispare, se taie cu «...» la capatul drept (plan de conturi); `KBot.Controls` 1.69.0.0 | GATA pe cod (build 0/0); **nevazut pe ecran** | [SLICE-0123-06](../SLICE-0123-06-arbore-text-dreapta-cu-puncte.md) |
| 0123-02 | VERCON «Rapoarte» (23.10.2026): bridge `routes/vercon/kbot_bridge.py` + release route; page `VerconRapoarteForm` (Subunitate / Gestiune / An, tree combo of periods with months, dates, options, report tree, selection tree with lazy branches, Listare PDF, Export Excel); `AdvancedTreeControl.CompleteLazyLoad` / `SearchStarting`; help `contabil/vercon-rapoarte.md` | GATA pe cod (KBot.App build 0 warnings / 0 errors, py_compile ok); **server NEDEPLOYAT, nimic rulat sau vazut**; lazy tree and tick-an-unread-branch NEEXERSATE | [SLICE-0123-02](../SLICE-0123-02-vercon-rapoarte.md) |

### Open threads — 0123

- The server files of 0123-01 and 0123-02 (lists in the worklogs) are for the operator to publish; the bridges assume the K-BOT user name is the e-mail of the AD / VR roles.
- Other VERCON server files + the site's `reports.js` / `report-result.js` were being changed in the same tree by someone else during 0123-02 (OI reports rework, `tree_group`); the page copies the site's report-tree / selection rules as of 23.10.2026 and must follow if they change.
- Excel exports all columns (no column picker yet). VERCON «Rapoarte»: the period tree combo opens all years (the site opens the latest only).
- Ajutor de actualizat: none (done in 0000-119); ribbon captures to redo are listed in the 0000-119 worklog.
