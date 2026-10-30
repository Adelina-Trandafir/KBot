# SLICE-KE02/03 și 00EF-39 — Clasificațiile și vederea Recepții

KE02/KE03 păstrează interfața inițială din 16.10 și corecția observată de operator. În 17.10, 00EF-39 mută clasificațiile în fereastra de legare și scoate fereastra separată. Clasificațiile salvate înaintea verificării rămân salvate chiar dacă legătura este refuzată; anularea ferestrei nu salvează. [KE11](SLICE-KE12-receptii-pagina-in-ribbon.md#slice-KE11) și [KE12](SLICE-KE12-receptii-pagina-in-ribbon.md) schimbă ulterior interfața, fără a rescrie aceste checkpointuri.

- [KE02](#slice-KE02)
- [KE03](#slice-KE03)
- [00EF-39](#slice-00EF-39)

## SLICE-KE02 - The angajament and the classification of each invoice line (16.10.2026) <a id="slice-KE02"></a>

### What changed and why
Operator request (16.10.2026): link the received e-invoices to receptions and, later, to payments; plan in `docs/KE_PLAN.md`.

- Window `KeClasificariForm`: the accountant chooses the angajament (the ones of the supplier's DDFs, or "Alt angajament..." from the list of all) and, for each line, the classification from the angajament's INDICATORS (`FX_Indicatori`): the law says the indicator must exist before the reception. "Ca la prima linie" copies the first choice. Saved on the server (`PUT .../ke/clasificatii`).
- Window `KeAngajamentForm` (picker with server-side search, like `AlegeDdfForm`): used when the supplier has no DDF or the invoice goes to another angajament. A supplier with several angajamente: every angajament of its DDFs is in the list.
- The lines grid of the "Continut" view got two read-only columns: "Valoare cu TVA" and "Clasificatie".
- A classification that is not an indicator of the angajament is refused with a message that names the missing step (a new reservation revision through the DDF), by the dialog and by the server check.

### Files touched
`src/KBot.EFactura/Forms/KeClasificariForm.vb` + designer, `KeAngajamentForm.vb` + designer, `Views/Primite/PrimiteLiniiPage.Designer.vb`, `PYTHON/routes/efactura/primite_ke.py` (`angajamente`, `seteaza_clasificatii`).

### Test results
See the [shared initial validation checkpoint](SLICE-KE01-modelul-legaturii-si-calculele.md#checkpoint-16-10) for KE01…KE07 (16.10.2026).

### Unverified / deferred
- The grid combo column is fed with `Column(key).ComboItems` (the way `FacturiForm.Linii` does it); how it behaves with a long list was not seen.
- SQL/server prerequisites: [initial checkpoint](SLICE-KE01-modelul-legaturii-si-calculele.md#checkpoint-16-10).
- Hand-written forms and unseen UI: the same initial checkpoint applies.

### Corrective 16.10.2026 (found on screen by the operator)
- The angajament chosen last (`_keCod`) was kept across invoices: the window of another invoice opened on a wrong angajament and offered ITS indicators (e.g. 65.10.04.02.20.01.30 instead of 65.02.04.02.20.01.04). Now it is cleared when another invoice is shown (`ShowInvoiceAsync`), and an already linked invoice starts on the angajament of its first link (`ApplyKe`).
- `KeClasificariForm.ApplyIndicators`: a cell holding a choice from the previous angajament goes back to "(nealeasa)" when the angajament changes (it used to look chosen and save as not chosen).

## SLICE-KE03 - The "Receptii" view of a received invoice (16.10.2026) <a id="slice-KE03"></a>

### What changed and why
- New page `PrimiteReceptiiPage` and its place in the bar of views of `PrimiteView` (after "Mesaje"), shown once an invoice is chosen, in every host of `PrimiteView` (the "Facturi primite" window and the "E-Factura" view of the DDF).
- It shows the total with VAT, what is allocated, the rest and the state, the table of links (angajament, classification, reception, kind, sum) and the buttons: "Clasificatii pe linii...", "Leaga de receptie...", "Receptie noua...", "Modifica receptia...", "Scoate legatura". For a role other than `Contabil` the buttons are hidden and a line says why.
- `PrimiteView.Receptii.vb` (partial) reads the state right after the invoice (own call `GetKeAsync`, a failure becomes a line in the view) and fills the two new columns of the lines grid.

### Files touched
`src/KBot.EFactura/Views/Primite/PrimiteReceptiiPage.vb` + designer, `PrimiteView.Receptii.vb`, `PrimiteView.vb`, `PrimiteView.Designer.vb`.

### Test results
See the [shared initial validation checkpoint](SLICE-KE01-modelul-legaturii-si-calculele.md#checkpoint-16-10) for KE01…KE07 (16.10.2026).

### Unverified / deferred
- The nav item reuses the `invoice` icon (no new image was made).
- SQL/server prerequisites: [initial checkpoint](SLICE-KE01-modelul-legaturii-si-calculele.md#checkpoint-16-10).
- Hand-written forms and unseen UI: the same initial checkpoint applies.

## SLICE-00EF-39 - Received invoices: the classifications of the lines move into «Leaga de receptie» (17.10.2026) <a id="slice-00EF-39"></a>

### What changed and why
Operator request: KeLegaForm «Leaga de receptie…» absorbs KeClasificariForm «Clasificatii pe linii», removing the old window/button from shared PrimiteView: both PrimiteForm «Cumparare» and KbotMain E-Factura hosts.

`KeLegaForm` now has, under the angajament row, a grid of the invoice lines (Nr, Denumire, Valoare cu TVA, **Clasificatie** as a combo). Everything the old window did is there:
- Choices: chosen angajament INDICATORS. A preexisting non-indicator classification remains visible as «nu e indicator al angajamentului»; switching angajament resets choices from the previous list to «(nealeasa)». Cells accept list values only; Ca la prima linie copies the first choice to all. Before opening any angajament, existing line classifications remain.
- NEW local De alocat calculation replaces the server answer from saved classifications: per chosen classification, sum VAT-inclusive line values minus existing EFacturaKeStare.Legaturi allocations. Recomputed on each classification change, shared by existing/new/modified reception modes.
- OK returns Clasificari (all choices)/ClasificariSchimbate, without writing. PrimiteView.LinkAsync saves changes via SetKeClasificatiiAsync, ApplyKe applies returned state, then the link check/flow runs against saved classifications.
- Removed: `KeClasificariForm` (+ designer), `PrimiteReceptiiPage.btnClasificatii` and its event `ClassificationsRequested`, `PrimiteView.PgReceptii_ClassificationsRequested`. The four remaining buttons of the page share the width. The note under the summary now says the classification is chosen at «Leaga de receptie».

Classification save occurs on Leaga/Creeaza/Modifica in the link window, replacing separate Salveaza. A later link refusal leaves those choices saved; Cancel saves nothing.

### Files touched
`src/KBot.EFactura/Forms/KeLegaForm.vb`, `KeLegaForm.Designer.vb` (grid `gridLinii`, `btnToate`, `lblSep2`; the window is 120 px taller), `src/KBot.EFactura/Views/Primite/PrimiteView.Receptii.vb`, `PrimiteReceptiiPage.vb`, `PrimiteReceptiiPage.Designer.vb`, `src/KBot.EFactura/KBot.EFactura.vbproj` (FileVersion 1.2.0.0 -> 1.3.0.0). Deleted: `Forms/KeClasificariForm.vb`, `Forms/KeClasificariForm.Designer.vb`. Help: see SLICE-0000-95.

### Test results
`dotnet build src/KBot.EFactura` and `dotnet build src/KBot.App`: 0 warnings, 0 errors. Nothing run, nothing seen on screen; no test (operator's rule).

### Unverified / deferred
- The new grid and the 120 px of extra height were never seen; the designer coordinates are written by hand and the dock order (Top, Top, Top, Top, Fill) is what makes the layout, so open the window once in the designer / on screen.
- `KBotDataView` combo column + `CellValueChanged` on a combo commit: the event is assumed to fire with `ColumnKey = "clsf"` when a combo value is committed (the old window never listened to it); if it does not, the allocation grid only follows after «Ca la prima linie» or a new angajament.
- The old plan (KE02) said the classification is chosen automatically when the angajament has a single indicator; the old window never did that and this one does not either.
- The KE02/KE03 sections above describe the old window (history, left as written).
