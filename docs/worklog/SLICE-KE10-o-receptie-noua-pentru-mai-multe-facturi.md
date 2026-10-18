# SLICE-KE10 - ONE new reception for several received invoices (18.10.2026)

## What changed and why
Operator request: link several invoices to one new reception. In the list shown by the root «Toate facturile» (ticks) a new button **«Recepție nouă»**
(left of «Listează») works on the ticked invoices:
1. one `KeLegaForm` per invoice (the first asks angajament, type, date, description; the next ones are tied to them with
   `KeLegaForm.FixeazaReceptiaNoua` and ask only classifications and amounts);
2. each invoice's classifications are saved if changed and the server checks it (`verifica`); any problem stops everything, before FOREXE;
3. the rows of all the checks are added up per indicator (`SumRows`) and the robot saves ONE reception with ONE marker `(IDRH; IDR)`;
4. every invoice gets its link with the same IDRH/IDR (`AddKeLegaturiAsync`); a link that fails is reported, the others go on; the list is read again.
The server is unchanged (no unique key on `EF_PrimiteReceptii.IDRH`; `leaga` is per invoice).

## Files touched
`KBot.EFactura/Views/Primite/PrimiteView.ReceptieMultipla.vb` (new), `PrimiteListaPage.vb`, `PrimiteListaPage.Designer.vb`, `KBot.EFactura/Forms/KeLegaForm.vb`;
help: `HelpContent/contabil/efactura/index.md` (tag `KE10`, `0000-99`).

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Not run, not seen on screen.

## Unverified / deferred
- The button's room next to the status label at a narrow width; the window texts of invoices 2..n.
- «Revizie adaugata manual nu se poate incarca» (the second request): NOT done, waiting for the operator's answer on what «incarca» means there.
