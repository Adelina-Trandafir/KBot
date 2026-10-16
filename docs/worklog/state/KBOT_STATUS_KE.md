# K-BOT — STATUS, slice family KE (received invoices <-> receptions <-> payments)

Plan: [`docs/KE_PLAN.md`](../../KE_PLAN.md). KE is a family inside K-BOT, like 00EF (it joins E-Factura with Receptii and, later, Plati). Numbering KE01, KE02, ...
Operator request 16.10.2026. Everything below is code and text only: **the SQL was not run, the server not deployed, nothing run against the
live site or seen on screen, no test written or run.**

## Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| KE01 | Data model, amounts with VAT, server routes, client contract | GATA pe cod (build curat, SQL nerulat, server nedeployat) | `SLICE-KE01-modelul-legaturii-si-calculele.md` | `EF_PrimiteReceptii`, `EF_PrimiteLinii.IdClsf` / `CotaTVA` (`sql/KE_01_primite_receptii.sql`); `primite_ke.py`, `primite_ke_routes.py`; `EFacturaKe.vb`, `ApiClient.EFactura.Ke.vb`. Every write: role `Contabil`, checked on the server. |
| KE01-02 | Diagnostic: everything about one angajament in one table | GATA pe cod, **SQL nerulat** | `SLICE-KE01-02-fisa-angajament.md` | `sql/KE_01_02_fisa_angajament.sql`: `AVACONT_COMUN.FX_FisaAngajament(db, cod)`; `DIAG` rows first (indicator vs Clasificatii, invoice line vs indicator, reception value per classification, links), then every table as JSON. |
| KE02 | The angajament and the classification of each line | GATA pe cod | `SLICE-KE02-angajament-si-clasificatii-pe-linii.md` | `KeClasificariForm`, `KeAngajamentForm`; two new columns in «Continut». |
| KE03 | The «Receptii» view of an invoice | GATA pe cod | `SLICE-KE03-vederea-receptii.md` | `PrimiteReceptiiPage`, `PrimiteView.Receptii.vb`. |
| KE04 | Link to an existing reception | GATA pe cod | `SLICE-KE04-legare-de-receptie-existenta.md` | `KeLegaForm` (mode «existenta»), `verifica` / `leaga`. |
| KE05 | The robot adds a reception («Receptie Noua») | GATA pe cod, **workflow nerulat pe site** | `SLICE-KE05-robotul-si-receptie-noua-in-forexe.md` | `Receptie Noua.wfl`, `IKeRobot`, `KbotForm.KeRobot.vb`; confirmation by `.alert.alert-success`, read-back as the check only when it is missing. |
| KE06 | «Receptie noua» from an invoice | GATA pe cod, nerulat | `SLICE-KE06-receptie-noua-din-factura.md` | marker `(IDRH; IDR)` reserved and written in the description; the links are written only after the save is confirmed. |
| KE07 | «Modifica receptia» | GATA pe cod, **workflow nerulat pe site** | `SLICE-KE07-modificarea-unei-receptii.md` | `Receptie Modificata.wfl`: candidates by date + total, checked by the marker; nothing is saved when none matches. |
| KE08 | Payments and payment orders | **NEPORNIT** | — | The plan leaves it for after KE01-KE07 (the payment will tie to the reception, `FX_Receptii.SumaPlatita` already exists). |

## Current focus

- **KE01..KE07 are in code (16.10.2026).** To see them work, in this order: (1) run `sql/KE_01_primite_receptii.sql` on `AVACONT_SURSA` and sync it to the unit
  databases, deploy the server; (2) log in as a `Contabil`, open an invoice of a supplier that has a DDF, «Receptii» > «Clasificatii pe linii...» >
  «Leaga de receptie...» (touches nothing in FOREXE); (3) only then, on a TEST angajament and with the dry run on, «Receptie noua...» and
  «Modifica receptia...» (the two workflows were never run on the real site).
- Decisions of the operator (16.10.2026), all in the plan: amounts WITH VAT; storno = a normal reception with a negative sign; the classification
  comes from `FX_Indicatori` and must exist before the reception (else a new reservation revision first); one supplier may have several angajamente
  (the accountant may pick any); default reception type «Final»; only the role `Contabil` writes; no «draft» step.

## Open threads

- The two workflows (`Receptie Noua`, `Receptie Modificata`) must be run once on a test angajament: selectors, the value format of the indicator
  field (assumed «1.380,00»), a possible confirmation after «Salveaza», the success element.
- Reception pictures for the ALOP file (guide p.21-22) are not taken by the robot (only by the hand-made path).
- Two receptions of one day with the same total and no K-BOT marker cannot be told apart by the site's list (the first is taken); the `RangZi` tie-break of the plan was not built (operator 16.10.2026: the marker `(IDRH; IDR)` identifies what K-BOT writes, the day does not matter; `RangZi` is already stamped by the download if it is ever needed for old receptions).
- KE06: «one reception per day on an angajament» is an OPTIONAL setting (`KeBlockSameDayReception`, off by default, «Setari > Aplicatie»), checked in `KeLegaForm`.
- KE08 (payments, payment orders).
- **Ajutor**: `0000-83` covers the text; capture `efactura-receptii` to take, `efactura-cumparare` to redo.
