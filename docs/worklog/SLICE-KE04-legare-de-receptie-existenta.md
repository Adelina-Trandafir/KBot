# SLICE-KE04/06/07/10 — Legarea, crearea și modificarea recepției din facturi

Etapele inițiale păstrează separat cele trei moduri ale ferestrei și extinderea la mai multe facturi. Robotul și corecția formatelor/erorilor sunt în [KE05/09](SLICE-KE05-robotul-si-receptie-noua-in-forexe.md). KE10 introduce câte o fereastră pe factură; [KE11](SLICE-KE12-receptii-pagina-in-ribbon.md#slice-KE11) o înlocuiește ulterior cu o fereastră comună, apoi KE12 cu pagina din ribbon. Incertitudinea „încarcă revizia manuală” din KE10 rămâne consemnată la etapa ei.

- [KE04](#slice-KE04)
- [KE06](#slice-KE06)
- [KE07](#slice-KE07)
- [KE10](#slice-KE10)

## SLICE-KE04 - Link an invoice to an existing reception (16.10.2026) <a id="slice-KE04"></a>

### What changed and why
Operator request (16.10.2026): link the received e-invoices to receptions and, later, to payments; plan in `docs/KE_PLAN.md`.

- Window `KeLegaForm` (one window, three modes). Mode "existenta": the receptions of the angajament with their value, their rest and an editable "De alocat" per classification; the proposed amount is the smaller of the invoice rest and the reception rest (sign aware), which the accountant may lower for a partial payment.
- Server: `receptii_angajament` (indicators + receptions, value per classification from the last counting snapshot, what invoices already cover, rest), `verifica` (the indicator exists, the sum fits the rest of the invoice on that classification, same sign; a warning when the receptions would pass the legal commitment) and `leaga` (an existing reception must have the rest, else 409 `REST_RECEPTIE` with the advice to modify or make a new one). Nothing changes in FOREXE.

### Files touched
`src/KBot.EFactura/Forms/KeLegaForm.vb` + designer, `PYTHON/routes/efactura/primite_ke.py` (`receptii_angajament`, `verifica`, `leaga`), `PrimiteView.Receptii.vb` (`LinkAsync`).

### Test results
See the [shared initial validation checkpoint](SLICE-KE01-modelul-legaturii-si-calculele.md#checkpoint-16-10) for KE01…KE07 (16.10.2026).

### Unverified / deferred
- How the rest of a reception behaves when a modified reception grows after links were written on it was reasoned, not tried.
- SQL/server prerequisites: [initial checkpoint](SLICE-KE01-modelul-legaturii-si-calculele.md#checkpoint-16-10).
- Hand-written forms and unseen UI: the same initial checkpoint applies.

## SLICE-KE06 - "Receptie noua" from an invoice (16.10.2026) <a id="slice-KE06"></a>

### What changed and why
- The flow of the button: the window (`KeLegaForm`, mode "noua": type Avans / Partial / Final, default "Final" like the site; date, default the invoice date; description, default "plata factura nr/data"; the value of each classification, default the rest of the invoice) > the server check (`verifica`: indicator exists, amounts fit; warnings asked as a question) > a K-BOT marker `(IDRH: n; IDR: m)` is reserved through the existing route and written at the end of the description > the robot adds the reception > ONLY when it is saved the links are written with those numbers (`EF_PrimiteReceptii.IDRH/IDR`), so the download that follows knows whose reception it is.
- If the save cannot be confirmed (no success element and the read-back does not find the marker) nothing is linked and the accountant is told to look in FOREXE before trying again.

- **Optional one-reception-per-day (operator, 16.10.2026).** Setting `AppSettings.KeBlockSameDayReception`, OFF by default, in «Setari > Aplicatie» (`chkKeZi`). When on, `KeLegaForm` (mode "noua") refuses the date when the angajament already has a reception on it and tells the accountant to change that one or pick another date. Why off by default: a reception K-BOT writes carries its own numbers (the marker `(IDRH; IDR)`), so the day does not identify it; the check is a client-side wish, not a server rule. Only receptions K-BOT already knows are checked (a reception made on the site since the last download is not seen).

### Files touched
`src/KBot.EFactura/Views/Primite/PrimiteView.Receptii.vb` (`NewReceptionAsync`, `ReserveMarkerAsync`), `Forms/KeLegaForm.vb`, `src/KBot.Common/AppSettings.vb`, `src/KBot.App/Setari/SetariAplicatieView(.Designer).vb`; help `contabil/efactura/index.md` (tag KE06).

### Test results
See the [shared initial validation checkpoint](SLICE-KE01-modelul-legaturii-si-calculele.md#checkpoint-16-10) for KE01…KE07 (16.10.2026).

### Unverified / deferred
- The reserved marker lives 2 days (`FX_NumberLock`): a reception saved and read much later may lose it. The flow reads back at once.
- The link is written after the robot and not in the same transaction: if only the link write fails the reception exists in FOREXE and the accountant must link it with "Leaga de receptie...".
- SQL/server prerequisites: [initial checkpoint](SLICE-KE01-modelul-legaturii-si-calculele.md#checkpoint-16-10).
- Hand-written forms and unseen UI: the same initial checkpoint applies.

## SLICE-KE07 - "Modifica receptia": finding the right reception and raising it (16.10.2026) <a id="slice-KE07"></a>

### What changed and why
- New `Workflows/Creare/adlop - Receptie Modificata.wfl`: candidates match DATE/TOTAL from the list (Tip, Data, Suma, eye; description is inside the form). Each is opened; when a K-BOT marker is known, its snapshot-number pattern must match the description (no colon, because `IfVar` splits on it). A mismatch returns with browser Back and tries the next; with no marker, the first candidate wins. None found → `GASITA = 0`, nothing saved. The form matches «Adauga» per operator: indicator values are rewritten and the new marker appended to the existing description.
- Server: `receptii_angajament` also answers `Idrhs` (the snapshot numbers of each reception); `verifica` for `modificata` gives the old and the new value of each row; `leaga` records the links on the reception (`IDRR`).

### Files touched
`src/KBot.Forexe/Workflows/Creare/adlop - Receptie Modificata.wfl`, `PYTHON/routes/efactura/primite_ke.py`, `PrimiteView.Receptii.vb` (`ChangeReceptionAsync`, `MarkerRegex`), `KeLegaForm.vb`, `ApiClient.EFactura.Ke.vb`.

### Test results
See the [shared initial validation checkpoint](SLICE-KE01-modelul-legaturii-si-calculele.md#checkpoint-16-10) for KE01…KE07 (16.10.2026).

### Unverified / deferred
- **The workflow never ran on the real site**, same as KE05; additionally the eye selector and the "Modifica receptie" heading are copied from `Receptie Editata.wfl`.
- Two receptions of the same day with the same total and no marker cannot be told apart by the site's list: the first one is taken. (The plan's `RangZi` tie-break was NOT built, on the operator's word of 16.10.2026: what K-BOT writes carries the marker `(IDRH; IDR)`, which names the reception whatever its day; the tie-break would only help an old reception without a marker. The download already stamps `RangZi` on `FX_Receptii_R`, so it can be added later if it turns out to be needed.)
- SQL/server prerequisites: [initial checkpoint](SLICE-KE01-modelul-legaturii-si-calculele.md#checkpoint-16-10).
- Hand-written forms and unseen UI: the same initial checkpoint applies.

## SLICE-KE10 - ONE new reception for several received invoices (18.10.2026) <a id="slice-KE10"></a>

### What changed and why
Operator request: link several invoices to one new reception. In the list shown by the root «Toate facturile» (ticks) a new button **«Recepție nouă»** (left of «Listează») works on the ticked invoices:
1. one `KeLegaForm` per invoice (the first asks angajament, type, date, description; the next ones are tied to them with `KeLegaForm.FixeazaReceptiaNoua` and ask only classifications and amounts);
2. each invoice's classifications are saved if changed and the server checks it (`verifica`); any problem stops everything, before FOREXE;
3. the rows of all the checks are added up per indicator (`SumRows`) and the robot saves ONE reception with ONE marker `(IDRH; IDR)`;
4. every invoice gets its link with the same IDRH/IDR (`AddKeLegaturiAsync`); a link that fails is reported, the others go on; the list is read again.
The server is unchanged (no unique key on `EF_PrimiteReceptii.IDRH`; `leaga` is per invoice).

### Files touched
`KBot.EFactura/Views/Primite/PrimiteView.ReceptieMultipla.vb` (new), `PrimiteListaPage.vb`, `PrimiteListaPage.Designer.vb`, `KBot.EFactura/Forms/KeLegaForm.vb`; help: `HelpContent/contabil/efactura/index.md` (tag `KE10`, `0000-99`).

### Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Not run, not seen on screen.

### Unverified / deferred
- The button's room next to the status label at a narrow width; the window texts of invoices 2..n.
- «Revizie adaugata manual nu se poate incarca» (the second request): NOT done, waiting for the operator's answer on what «incarca» means there.
