# SLICE-KE09 - New reception: amounts and date as the site wants them, and the site's error box (18.10.2026)

## What changed and why
Operator request after a failed «Receptie noua» (log `Log_20261018_124414.txt`, 13:07): FOREXE answered the save with the red box
`CAB-0031 - Datele introduse nu sunt valide`, the robot waited 8 s for the green box, then K-BOT went on to read back and timed out.
- **Amount.** The log shows the value typed as `302.50`. `DdfSendInputs` built its number format with `New CultureInfo("ro-RO")`, which
  honours the operator's Windows overrides (a «.» decimal separator). It now uses `DdfSendInputs.CulturaSite`, a ro-RO culture with
  `useUserOverride = False` and the separators set by hand («,» decimal, «.» thousands). `FormatSuma` / `ParseSuma` and the number reading of
  `KeLegaForm` use it.
- **Date.** The date typed in the reception form is now `zz.ll.aaaa`, written with quoted literal dots (`WorkflowCatalog.DataReceptieFormularFormat`
  = `dd'.'MM'.'yyyy`, invariant). The old `DataReceptieFormat` (`dd/MM/yyyy`) stays for MATCHING the site's list («Data» cell) and the history.
- **Error box.** `Receptie Noua.wfl` and `Receptie Modificata.wfl` wait for either `.alert.alert-success` or `.alert.alert-error`; the error text
  (`.alert.alert-error strong`) goes to `MesajEroare` (`WorkflowCatalog.VarMesajEroare`). `KbotForm.KeRobot.vb` returns it to the operator as
  «FOREXE a respins recepția și nu s-a salvat nimic: ...» and does NOT read back or link anything.

## Files touched
`KBot.Domain/DdfSendInputs.vb`, `KBot.Forexe/WorkflowCatalog.vb`, `KBot.Forexe/JobBuilder.vb`, `KBot.Forexe/Workflows/Creare/adlop - Receptie Noua.wfl`,
`.../adlop - Receptie Modificata.wfl`, `KBot.App/KbotForm.KeRobot.vb`, `KBot.EFactura/Forms/KeLegaForm.vb`.

## Flow analysis against the logs (what is right, what was not)
- Recorded manual flow (`Rez+Rec_energetic_1_indicator.wfl`) = reservation (value `176408,35`, reason, «Continua») then reception (type Final, date by picker,
  description, value `6949,89`, save). Both automatic flows follow the same steps; the manual one types the value WITHOUT thousand separators.
- Reservation (13:06): `Rezervare` saved, K-BOT marker `(IDREV: 456)` written in the reason, history read: correct.
- New reception (13:07): the form was filled, the save clicked, then NO success box in 8 s; `ReceptieEditata` then found no «Recepții» tab and timed out
  (15 s) - consequence of the rejected save, now stopped before it.
- Not proven by the logs: whether the date format or the amount caused CAB-0031 (the log does not print the date; the amount was `302.50`).

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run on the site, no tests (operator rule).

## Unverified / deferred
- The workflows were NOT run against FOREXE. The combined selector `.alert.alert-success, .alert.alert-error` and the nested `IfExists` are assumed to be
  supported by the executor like the single selectors; first run on a TEST angajament with the dry run.
- Whether the site accepts `zz.ll.aaaa` in `input[name='data']` is the operator's word (not seen in a log).
- Field-level errors under the box («Verificati erorile pe fiecare camp») are not read.
