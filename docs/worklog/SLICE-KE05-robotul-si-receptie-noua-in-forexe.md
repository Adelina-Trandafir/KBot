# SLICE-KE05/09 — Robotul recepției și tratarea erorii FOREXE

KE05 este implementarea inițială; KE09 urmează incidentului din 18.10 și separă formatele site-ului de cele Windows. Regula inițială de recitire este amendată: la mesajul de eroare FOREXE nu se recitește și nu se leagă nimic. Logul nu dovedește dacă data sau suma a provocat CAB-0031; fluxurile corectate nu au fost rulate pe site.

- [KE05](#slice-KE05)
- [KE09](#slice-KE09)

## SLICE-KE05 - The robot adds a reception in FOREXE ("Receptie Noua") (16.10.2026) <a id="slice-KE05"></a>

### What changed and why
Operator request (16.10.2026): link the received e-invoices to receptions and, later, to payments; plan in `docs/KE_PLAN.md`.

- New workflow `Workflows/Creare/adlop - Receptie Noua.wfl`: the reception part of the old "Rezervare si Receptie" (tab Receptii > Adauga > type, date, description, the value of each indicator row > Save), WITHOUT the reservation steps, with the angajament opened by its code like "Incarca Rezervare", the indicator row matched like there (`01A- ` prefix off, letters and digits only), the save click marked `commits="true"` (a dry run stops before it) and, at the end, the text of the site's success element `.alert.alert-success` in `MesajSalvare` (empty when it did not show).
- `WorkflowCatalog` (file names, variables), `JobBuilder.BuildReceptieNoua` / `BuildReceptieModificata`, `KBot.Forexe.vbproj` (copy to output).
- The shell hands its robot to the e-invoice views: `IKeRobot` / `KeRobot.Current` (`KBot.EFactura`), implemented in `KbotForm.KeRobot.vb`. A save waits its turn in the robot queue, runs the workflow, then ALWAYS reads the angajament back (`DownloadReceptieEditataAsync` for the reception's date) and takes it into K-BOT (`DuLaIngestieAsync`). The save is confirmed by the success element; ONLY when it did not show, the read-back is also the check: the reception is saved if the K-BOT marker is in the description of a reception of the package.

### Files touched
`src/KBot.Forexe/Workflows/Creare/adlop - Receptie Noua.wfl`, `WorkflowCatalog.vb`, `JobBuilder.vb`, `KBot.Forexe.vbproj`, `src/KBot.EFactura/Views/Primite/IKeRobot.vb`, `src/KBot.Domain/EFacturaKeRobot.vb`, `src/KBot.App/KbotForm.KeRobot.vb`, `KbotForm.ForexeWatch.vb` (wiring).

### Test results
See the [shared initial validation checkpoint](SLICE-KE01-modelul-legaturii-si-calculele.md#checkpoint-16-10) for KE01…KE07 (16.10.2026).

### Unverified / deferred
- **The workflow never ran on the real site.** Every selector is copied from a flow that exists (the old "Rezervare si Receptie", "Incarca Rezervare", "Receptie Editata"); the format of the value typed in the indicator field is assumed the same as the reservation one ("1.380,00"); whether a confirmation modal follows "Salveaza" on the reception form is not known. First run: on a test angajament, with the dry run (`DryRunMode`) on, then off.
- The two pictures the ALOP guide asks for a reception (p.21-22) are taken by the hand-made path only (`IaCapturileReceptieiAsync`); the robot takes one `Poza_Receptie` screenshot that is NOT filed. Deferred.
- The list of receptions is not paged by the flow (like `Receptie Editata`).
- SQL/server prerequisites: [initial checkpoint](SLICE-KE01-modelul-legaturii-si-calculele.md#checkpoint-16-10).
- Hand-written forms and unseen UI: the same initial checkpoint applies.

## SLICE-KE09 - New reception: amounts and date as the site wants them, and the site's error box (18.10.2026) <a id="slice-KE09"></a>

### What changed and why
Operator request after a failed «Receptie noua» (log `Log_20261018_124414.txt`, 13:07): FOREXE answered the save with the red box `CAB-0031 - Datele introduse nu sunt valide`, the robot waited 8 s for the green box, then K-BOT went on to read back and timed out.
- **Amount.** The log shows the value typed as `302.50`. `DdfSendInputs` built its number format with `New CultureInfo("ro-RO")`, which honours the operator's Windows overrides (a «.» decimal separator). It now uses `DdfSendInputs.CulturaSite`, a ro-RO culture with `useUserOverride = False` and the separators set by hand («,» decimal, «.» thousands). `FormatSuma` / `ParseSuma` and the number reading of `KeLegaForm` use it.
- **Date.** The date typed in the reception form is now `zz.ll.aaaa`, written with quoted literal dots (`WorkflowCatalog.DataReceptieFormularFormat` = `dd'.'MM'.'yyyy`, invariant). The old `DataReceptieFormat` (`dd/MM/yyyy`) stays for MATCHING the site's list («Data» cell) and the history.
- **Error box.** `Receptie Noua.wfl` and `Receptie Modificata.wfl` wait for either `.alert.alert-success` or `.alert.alert-error`; the error text (`.alert.alert-error strong`) goes to `MesajEroare` (`WorkflowCatalog.VarMesajEroare`). `KbotForm.KeRobot.vb` returns it to the operator as «FOREXE a respins recepția și nu s-a salvat nimic: ...» and does NOT read back or link anything.

### Files touched
`KBot.Domain/DdfSendInputs.vb`, `KBot.Forexe/WorkflowCatalog.vb`, `KBot.Forexe/JobBuilder.vb`, `KBot.Forexe/Workflows/Creare/adlop - Receptie Noua.wfl`, `.../adlop - Receptie Modificata.wfl`, `KBot.App/KbotForm.KeRobot.vb`, `KBot.EFactura/Forms/KeLegaForm.vb`.

### Flow analysis against the logs (what is right, what was not)
- Recorded manual flow (`Rez+Rec_energetic_1_indicator.wfl`) = reservation (value `176408,35`, reason, «Continua») then reception (type Final, date by picker, description, value `6949,89`, save). Both automatic flows follow the same steps; the manual one types the value WITHOUT thousand separators.
- Reservation (13:06): `Rezervare` saved, K-BOT marker `(IDREV: 456)` written in the reason, history read: correct.
- New reception (13:07): the form was filled, the save clicked, then NO success box in 8 s; `ReceptieEditata` then found no «Recepții» tab and timed out (15 s) - consequence of the rejected save, now stopped before it.
- Not proven by the logs: whether the date format or the amount caused CAB-0031 (the log does not print the date; the amount was `302.50`).

### Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run on the site, no tests (operator rule).

### Unverified / deferred
- The workflows were NOT run against FOREXE. The combined selector `.alert.alert-success, .alert.alert-error` and the nested `IfExists` are assumed to be supported by the executor like the single selectors; first run on a TEST angajament with the dry run.
- Whether the site accepts `zz.ll.aaaa` in `input[name='data']` is the operator's word (not seen in a log).
- Field-level errors under the box («Verificati erorile pe fiecare camp») are not read.
