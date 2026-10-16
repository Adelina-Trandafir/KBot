# SLICE-KE05 - The robot adds a reception in FOREXE ("Receptie Noua") (16.10.2026)

## What changed and why
Operator request (16.10.2026): link the received e-invoices to receptions and, later, to payments; plan in `docs/KE_PLAN.md`.

- New workflow `Workflows/Creare/adlop - Receptie Noua.wfl`: the reception part of the old "Rezervare si Receptie" (tab Receptii > Adauga > type, date, description, the value of each indicator row > Save), WITHOUT the reservation steps, with the angajament opened by its code like "Incarca Rezervare", the indicator row matched like there (`01A- ` prefix off, letters and digits only), the save click marked `commits="true"` (a dry run stops before it) and, at the end, the text of the site's success element `.alert.alert-success` in `MesajSalvare` (empty when it did not show).
- `WorkflowCatalog` (file names, variables), `JobBuilder.BuildReceptieNoua` / `BuildReceptieModificata`, `KBot.Forexe.vbproj` (copy to output).
- The shell hands its robot to the e-invoice views: `IKeRobot` / `KeRobot.Current` (`KBot.EFactura`), implemented in `KbotForm.KeRobot.vb`. A save waits its turn in the robot queue, runs the workflow, then ALWAYS reads the angajament back (`DownloadReceptieEditataAsync` for the reception's date) and takes it into K-BOT (`DuLaIngestieAsync`). The save is confirmed by the success element; ONLY when it did not show, the read-back is also the check: the reception is saved if the K-BOT marker is in the description of a reception of the package.

## Files touched
`src/KBot.Forexe/Workflows/Creare/adlop - Receptie Noua.wfl`, `WorkflowCatalog.vb`, `JobBuilder.vb`, `KBot.Forexe.vbproj`, `src/KBot.EFactura/Views/Primite/IKeRobot.vb`, `src/KBot.Domain/EFacturaKeRobot.vb`, `src/KBot.App/KbotForm.KeRobot.vb`, `KbotForm.ForexeWatch.vb` (wiring).

## Test results
`dotnet build` of `KBot.Api`, `KBot.EFactura`, `KBot.Forexe` and `KBot.App`: 0 errors, 0 warnings; `py_compile` of the changed Python files: ok; the two new workflows parse as XML. **Nothing was run against a live database, the live site or on screen, and no test was written or run** (operator rule: no tests).

## Unverified / deferred
- **The workflow never ran on the real site.** Every selector is copied from a flow that exists (the old "Rezervare si Receptie", "Incarca Rezervare", "Receptie Editata"); the format of the value typed in the indicator field is assumed the same as the reservation one ("1.380,00"); whether a confirmation modal follows "Salveaza" on the reception form is not known. First run: on a test angajament, with the dry run (`DryRunMode`) on, then off.
- The two pictures the ALOP guide asks for a reception (p.21-22) are taken by the hand-made path only (`IaCapturileReceptieiAsync`); the robot takes one `Poza_Receptie` screenshot that is NOT filed. Deferred.
- The list of receptions is not paged by the flow (like `Receptie Editata`).
- `sql/KE_01_primite_receptii.sql` is NOT run on `AVACONT_SURSA` nor synced to the unit databases; the server is NOT deployed. Until the script runs, the received-invoice routes answer "Tabelele E-Factura ale unitatii lipsesc sau sunt vechi" (the new `CotaTVA` column is part of the sync insert).
- Nothing here was seen on screen: the forms and the view are written by hand in their designer files (never opened in the Visual Studio designer).
