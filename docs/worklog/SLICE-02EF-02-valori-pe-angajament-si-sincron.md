# SLICE-02EF-02 - Reception view: columns per angajament, editable «Valoare», both grids in step (06.11.2026)

## Request
Operator's mock-up of «Recepții <> EFactura»: per-angajament Rezervări / Recepții / Disponibil; «Recepții» renamed «Total recepții» (the whole classification); «Valoare» editable; «Rămas» = Disponibil of the ANGAJAMENT - Valoare; a value above the angajament's Disponibil is BLOCKED. The window is gone (KE21); the grid of amounts lives on in `KeReceptiiView`.

## Final state
- `gridAloc`: «Total recepții», then per ANGAJAMENT (as the Sumar view defines them, on the day of the reception) **Rezervări** = `SUM(FX_Rezervari.R_Valoare)` (undated), **Recepții** = `SUM(FX_Receptii.DIF)` before the day, **Disponibil** = Rezervări - Recepții. They come with `disponibil()` (`RezervariAng`, `ReceptiiAng`); `FX_Indicatori.Angajament_Legal` / `.Receptii` are not used (downloads never refresh `Receptii`).
- «Rămas» = angajament Disponibil - Valoare, may not go below 0: refused at typing and stops the button (`AngajamentProblem`). Not for «Leagă» to an existing reception, not for a storno.
- The header edit of «Recepționată» and the spreading of «Valoare» over the lines were replaced by KE20-01 / KE20-04; `GroupHeaderEdit` stays in `KBotDataView` unused.

Files: `src/KBot.Api/ApiClient.EFactura.Ke.vb`, `src/KBot.Domain/EFacturaKe.vb`, server `PYTHON/routes/efactura/primite_ke.py` `disponibil()` (not deployed by me; an older server leaves the three columns empty and the block off). Help 0000-155.

Unverified: «la zi» was read as «on the day of the reception»; Rezervări is the undated total; the real figures were not looked at. A classification that is not an indicator of the angajament has no per-angajament figures and no block.
