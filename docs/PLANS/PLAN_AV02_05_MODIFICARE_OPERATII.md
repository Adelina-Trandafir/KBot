# PLAN AV02-05 - Port of ModificareNC1280 («Modificare / Ștergere operații contabile»)

## Context

Stage 2 of PLAN_AVACONT lists «Modificare: operatii diverse (`ModificareNC1280`)». Operations are now posted in K-BOT
(TZ_NoteC / TZ_Documente / TZ_Oper / TZ_Debit / TZ_Credit, AV02-01..04 + Contari), but there is no window to SEE
them line by line or to delete one. Access has it: an unbound form with a continuous subform `MNotaC1`, header
buttons that sort (left click, multi-column) and filter (right click, popup `frmFiltru`), an info popup `MNotaC11`,
footer buttons Modifica / Sterge operatie / Sterge document / Sterge nota (hidden), and its own ribbon «MNC».
Full functional map of the Access side was made (controls, VBA, ribbon callbacks from
`Surse/RawExport/Modules/basRibbonCallbacks.bas.txt:1117-1309,1622-1645`, delete routines
`mdl_Salvare_NoteContabile_code.txt:743-865`); it becomes the reading sheet in phase 1.

Not in the exports (stays unverified): the `USysRibbons` row «MNC» (labels/layout), `[~SQL]` 40000-40003 (list SQL),
table `B`, the Oper -> Debit/Credit cascade in `baza2026.accdb`.

## Decisions (Adelina, 09.11.2026)

- Slice **AV02-05** (sub-slices AV02-05-NN if it splits).
- Folders: `src/KBot.App/Avacont/ModificareDate/Operatii/` (+ `Views/`), same in `KBot.Api` and `KBot.Domain`;
  `PYTHON/routes/avacont/modificare_date/operatii/`; `sql/AVACONT/ModificareDate/Operatii/` (only if DDL is needed).
- **Page in AvacontMain** (not modal); the «MNC» ribbon becomes the page's view group (`IViewRibbon`).
- **Reading sheet first**, confirmed by Adelina, THEN code.
- **Modify: NOT now.** The window does list + delete; «Modifică operație» comes with the port of the `Note` form.
- **Prints (Nota contabila x4, Fisa cont debit/credit): deferred to stage 6.** Ordonantare / Propunere / Angajament
  bugetar-individual-legal: ELIMINAT (decision 23.10, ALOP).
- **Flag columns:** CH (`TZ_Oper.Chelt`) and CF (`TZ_Oper.Cfpp`) as ticks; PA and FX out. **«Ord» = state of the
  ordonantare document**, not a tick: empty (no `TZ_Oper.IdOrdTbl`), «Generat» (row in `FX_ORD_TBL`), then the
  signature stage from `FX_ORD.Semnatura` (A / B / Ordonator). **New column «R» (on/off): the operation's invoice
  has a reception** (`EF_PrimiteOperatii` -> `plati.are_receptie`, `PYTHON/routes/avacont/introducere_date/operatii/plati.py:161`).
  To verify in the sheet: the exact role tokens in `FX_ORD.Semnatura` (`OrdInfo.vb:54` says «AB,CD,Ordonator»,
  DDF uses «A,B,Ordonator»).

## Answers to the reading sheet (Adelina, 10.11.2026) - these override the proposals below

- «Ord» and «R» show ICONS, not words. «Ord»: one icon per stage (generated, signed AB, signed CD, Ordonator), empty =
  no ordonantare. «R»: one icon when ALL invoices have a reception, a visibly different one when only SOME have, none otherwise.
- Several filters at once, one per column: YES, built into `KBotDataView` so that every grid can do it.
- Delete of an operation with an ordonantare generated: REFUSED. Partner balance in the panel includes the opening balance.
- List order and panel state remembered per user. Closing / salaries / Centralizator lists: later.
- Main button: NEW ribbon tab «Introducere date» with group «Introducere Date» (the existing «Operatii» button MOVES
  there from Configurari > «Date») and group «Modificare Date» (the new «Modificare operatii» button).
- Access cascades Oper -> Debit/Credit (operator); K-BOT is RESTRICT, so the server deletes the lines explicitly.

## Phase 1 - reading sheet + plan in the repo (no code)

1. Copy this plan to `docs/PLANS/PLAN_AV02_05_MODIFICARE_OPERATII.md` (rule 08.11.2026).
2. Write `docs/access-map/avacont/modificare-nc1280.md`: every element with PORT / ELIMINAT / DE DECIS, plain words.
   Proposed labels (Adelina confirms or changes):
   - PORT: the list (NC, Doc, Data = document date, Clasificatie, Fel, Jurnal, Explicatie, Cont debit[.P partener],
     Cont credit, Suma, CH, CF, Ord, R, row icon by `TZ_Documente.Fel`); sort by header click (asc -> desc -> off,
     several columns); filter by header right click (values / contains / date range / amount range); letter keys
     O C E F J L N D S open the filter of their column, Enter/arrows/Shift+arrows; «Curăță» (sort + filter);
     «Ordonare implicită»; info panel toggle (document, tip, factură plătită integral/parțial, ordin de plată,
     facturile plătite de o plată, sold partener la data documentului + anual); «Șterge operație»; «Șterge document»;
     Excel export of the list AS SHOWN (filter + sort applied - fixes the Access bug); default order NC, Data, Doc.
   - ELIMINAT: Modifica (now), Sterge nota (hidden in Access, «am eliminat aceasta optiune»), FX/AL/Cont8067, PA,
     Ord/Prop/Ang prints, the startup «#EROARE» integrity check (server keys make it impossible), `Filtru_b_Click`
     (missing form), `btn_MNC_S2` (does nothing), `cbxSterg_NC`, the dead footer boxes.
   - DE DECIS: list views OA2 «operatii de inchidere» (stage 7), OA3 «salarii» (stage 3), OA4 «Centralizator»
     (SQL not exported) - proposal: not now; several filters at once (Access: one) - proposal: one per column, combined;
     setting MNC_T0 (sort order) and MNC_C6 (panel at start) - proposal: remembered per user in AppSettings until the
     Setari «Avacont» page exists; DelNC tick - not needed if «Sterge nota» is out; delete of an operation that has an
     ordonantare generated (`IdOrdTbl` set) - proposal: refused; where the main ribbon button goes (proposal:
     Configurari > «Date», next to «Operații», key `modificare_operatii`, text «Modificare operații»).
   - Delete rules (server, fixing the Access gaps): refused when the date is in a closed period
     (`postare.data_ultimei_inchideri` + previous year after January as `EInchis`), when `Fel='I'`, when it is the
     only operation of a closing note (CCV), when the invoice is paid (`TZ_PlatiFacturi.IdEvidentiere`), when the
     payment order is locked (`plati.motiv_blocare`: OPME / extras); a payment that pays invoices asks first with the
     list of invoices that become unpaid. «Șterge document» applies the SAME checks to each operation (Access did not).
     Every delete is recorded through the existing server action log (`PYTHON/routes/actiuni.py` map), not an Access-style `Actiuni` row.
3. Stop; Adelina confirms the sheet.

## Phase 2 - code (after confirmation; adjusted to what she changes)

**Server** - `PYTHON/routes/avacont/modificare_date/{__init__.py, operatii/__init__.py, operatii/lista.py,
operatii/stergere.py}`, imported at the end of `PYTHON/routes/avacont/__init__.py` like `op9`; pattern of
`introducere_date/operatii/op9.py` (`require_session`, `get_kbot_connection(g.session.db_name)`, `json_utf8`, `rollback`).
- `GET /api/avacont/modificare-operatii/lista?idsubunitate=&an=` - one row per `TZ_Oper` (join Documente, NoteC,
  Clasificatii, TZ_Jurnale, TZ_Debit/TZ_Credit + TZ_PlanCont + Parteneri, `FX_ORD_TBL`/`FX_ORD` for «Ord»,
  `EF_PrimiteOperatii` for «R», `TZ_OPuri` / `TZ_PlatiFacturi` aggregated - no duplicate rows like `qMNC2025`);
  each row carries `blocat` + `motiv` so the buttons can dim with a tooltip. COUNT rows around every join.
- `GET .../info/<id_oper>` - the info panel data.
- `GET .../stergere/<id_oper>/verifica` (refusal reason or the confirmation text + invoices) and
  `DELETE .../operatie/<id_oper>`, `DELETE .../document/<id_document>` - one transaction; reuse
  `plati.sterge_plata` for payments, unlink `EF_PrimiteOperatii` for evidences, delete Debit/Credit/Oper, then the
  empty document and the empty note (same order as `sterge_plata`, `plati.py:280-301`).
- No DDL expected; if one is needed it goes to `sql/AVACONT/ModificareDate/Operatii/AV02_05_*.sql` (utf8mb3, CreatedAt/UpdatedAt).

**Client**
- Domain: `src/KBot.Domain/Avacont/ModificareDate/Operatii/ModificareOperatiiInfo.vb` (row, info, check result).
- Api: `src/KBot.Api/Avacont/ModificareDate/Operatii/IModificareOperatiiApi.vb` + `ApiClient.ModificareOperatii.vb`
  (pattern `ApiClient.Operatii.vb`: lowercase wire classes, `SendNomenclatoareAsync`, `AvacontRoot`).
- App: `Avacont/ModificareDate/Operatii/KbotForm.ModificareOperatii.vb` (`DeschideModificareOperatii`, registered
  in `KbotForm.Machete.vb:RegisterMachete`, one instance like `KbotForm.PlanCont.vb:17-42`);
  `Views/ModificareOperatiiForm.vb` + `.Designer.vb` (all controls in the designer; `KBotDataView` grid, info panel at
  the bottom, `KBotNotice` for warnings) + partials `.Filtre.vb` (sort, filter popup, keys), `.Stergere.vb`,
  `.Info.vb`, `.Ribbon.vb` (`IViewRibbon`, model `PlanContForm.vb:348-389`), each with its `<Compile Update>` +
  `SubType Code` + `DependentUpon` in `KBot.App.vbproj`; follows `SubunitPicked` / year like `PlanContForm`.
- AvacontMain: main button in the Designer group «Date»; view buttons in `ribbon.RightGroup` (`Visible = False`),
  keys in `ViewGroupKeys` (`AvacontMain.vb:547`), `Case` in `Ribbon_ItemClicked` forwarding to `RunRibbonButton`.
- Excel: existing `ExcelJob` (KBot.Common) / `PlanContExcel.vb` pattern.
- House rules: no diacritics in code, `k_` prefix, Try/Catch + `GlobalErrorLog`, `KBotMessage.Show`, `KBotToolTip`,
  no grid tooltip on the DGV itself, DGV bands untouched, files <= ~700 lines, buttons Yes right / No left.

**Help + records**
- `src/KBot.App/HelpContent/contabil/nomenclatoare/modificare-operatii.md` (+ link in `index.md`), tags
  `<!-- slice: AV02-05 -->` and the 0000-NN help sub-slice; `tools\HelpCheck\Check-Help.ps1 -Coverage -Map`.
- Worklog `docs/worklog/SLICE-AV02-05-modificare-operatii.md`; row in `state/KBOT_STATUS_AV.md` (+ next free number
  AV02-06) and the AV line of `KBOT_STATUS.md`.

## Verification

- `dotnet build src\KBot.App\KBot.App.vbproj` (0 errors / 0 warnings); Python files byte-compiled with `PYTHON\.venv`.
- No tests, no runs, no git writes, no deploy (Adelina publishes the .py files and opens the window herself).
- Adelina's check: open «Modificare operații», compare a few notes with the same notes in Access on `baza2026.accdb`,
  try each delete refusal (closed month, paid invoice, locked OP, closing note) and one real delete.
