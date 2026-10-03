# SLICE-0104 — Componentele Access separate de aplicația principală; pachet «cu Access» / «fără Access»

Cererea operatorului (02.10.2026): Migratorul și comunicarea cu Access interesează DOAR clienții care folosesc și
aplicația Access (VBA). Pachetul pentru clienții noi trebuie să le lipsească; cel pentru clienții cu Access le păstrează.

## What changed and why

**1. Separarea (arhitectură).**
- **Migrator** — era deja un exe separat (`src/KBot.Migrator`, publicat în `Migrare\`; `KBot.App` nu îl cheamă nicăieri).
  Nu s-a schimbat codul lui; pachetul fără Access pur și simplu nu-l mai conține.
- **Comunicarea cu Access** — singurul cod din `KBot.App` care vorbea cu fișierele Access era
  `AccessBudgetSender` (felia 0103-06, OLE DB / ACE) + referința la pachetul `System.Data.OleDb`. A fost mutat într-un
  proiect nou, **`src/KBot.Access`** (`KBot.Access.dll`). `KBot.App` NU îl referențiază: îl găsește la rulare prin
  `KBot.Common.AccessBridge` (`IAccessBridge` + încărcare într-un `AssemblyLoadContext` propriu, dependențele OLE DB se
  rezolvă din `KBot.Access.deps.json`, iar toate `KBot.*` vin din aplicație ca tipul interfeței să fie același).
  Un pachet fără fișierul `KBot.Access.dll` nu are pod, deci nu are cod Access.
  `KBot.App.vbproj`: `ProjectReference ... ReferenceOutputAssembly="false"` (ordinea build-ului) + un target care copiază
  componenta lângă exe doar pentru rulări de dezvoltare; proprietatea `KBotAudience=NonAccess` o scoate cu totul.
- `AccessSendResult` s-a mutat în `KBot.Common/AccessBridge.vb` (interfața îl întoarce).

**2. Întrebarea «access / non-access» în `publish-release.ps1` și `push-update.ps1`.**
- Prima întrebare a scriptului, ÎNAINTE de cea de semnare: «(A)ccess / (N)on-access», Enter = Access; parametru
  `-Audience Ask|Access|NonAccess`. `push-update.ps1` o pune el (și o trimite mai departe), ca să meargă și cu `-SkipBuild`.
- Access = `Migrare\` + `KBot.Access.dll` (+ pachetul OLE DB lângă exe); non-access = nimic din ele, cu o gardă care
  oprește build-ul dacă s-a strecurat ceva. Fișierele poartă sufixul `_noaccess` (`KBot_Release_<stamp>_noaccess.zip`,
  `KBot_Setup_<stamp>_noaccess.exe`, pe server `KBot_<ver>_noaccess.zip`).
- `KBot.iss`: `/DWithAccess=1|0` (implicit 1) scoate componenta «migrare», fișierele, comanda rapidă și rândul din textul
  de întâmpinare.
- `push-update.ps1` refuză un zip care nu e de felul declarat (un pachet cu Access nu poate ieși ca «non_access»).

**3. Clasificarea la rulare + rândul `Access` din setările fiecărei baze.**
- `Setari.Access` (`1` = clientul are aplicația Access, `0` sau fără rând = nu), citit prin `/api/setari` după login →
  `ServerSettings.AccessEnabled`. `AccessFeature.Enabled` = rândul ȘI componenta instalată. Butonul «Trimite în Access»
  (`ClasificatiiForm`) apare doar dacă `AccessFeature.Enabled`; handlerul verifică încă o dată.
- `sql/0104_setari_access.sql` (șablonul `AVACONT_SURSA`, valoare `0`) și `sql/0104_setari_access_toate_bazele.sql`
  (bazele existente, implicit **`1`**: toate unitățile de azi au primit pachetul cu Migrator, deci sunt clienții Access;
  variabila `v_access` din script se schimbă dacă nu e așa). Unitățile NOI nu primesc rândul (provizionarea copiază doar
  structura tabelelor): fără rând = `0` = non-access, valoarea dorită pentru un client nou.

**4. Endpoint nou: `POST /api/access/client-type`** (`PYTHON/routes/access.py`), public (se cheamă înainte de login).
- Corp `{ "email": "..." }` (în corp, nu în adresă) → `{ "access": 0|1 }`. 1 dacă ORICE unitate pe care o poate deschide
  e-mailul are `Setari.Access = '1'`. E-mail necunoscut → `0`, nu eroare (nu spune unui străin ce adrese există).
- **Injecție:** e-mailul se validează cu un șablon strict (≤254 caractere) și merge DOAR ca parametru legat
  (`%s`); numele bazelor (identificatori, nu pot fi parametri) vin din propriul tabel, trec printr-un șablon `[A-Za-z0-9_]{1,64}`
  și se pun între ghilimele inverse; unul care nu se potrivește e sărit și logat. Limită 30 cereri / minut / IP (429).
- Clientul: `IAccessTypeApi` / `AccessTypeApi` (KBot.Api), `ClientProfile` (KBot.Common), `AppUpdateService.ResolveAccessAsync`.
  Se cheamă (a) la pornire, cu e-mailul ultimei autentificări, înainte de verificarea de actualizări; (b) în `LoginForm`, la 0,7 s după
  ultima tastă din caseta de e-mail, doar dacă arată a e-mail (nevid), și încă o dată la «Continuă» dacă nu apucase.

**5. Verificarea de actualizări respectă `access`.**
- Nu rulează până nu se știe tipul clientului (`ClientProfile.AccessKnown`). Fără e-mail reținut, verificarea de la pornire
  se AMÂNĂ: `LoginForm` o rulează o singură dată, imediat ce serverul a răspuns pentru e-mailul tastat
  (`UpdateCheckPending`; dacă pornește actualizatorul, fereastra se închide cu `ExitForUpdate` și `Program` iese).
- `GET /api/update/latest?access=0|1` și `GET /api/update/download?access=0|1` (lipsă = `1`: clienții vechi sunt toți cu Access;
  altă valoare = 400 `ACCESS_PARAM_INVALID`). Clientul cere pachetul de felul lui, și la descărcare.
- **`latest.json`** are acum două blocuri: `{ "access": {...}, "non_access": {...} }`. Forma veche (blocul la rădăcină) se
  citește în continuare ca blocul «access», deci serverul nou merge cu un `latest.json` vechi; primul push o rescrie în forma
  nouă. `push-update.ps1` rescrie doar blocul felului său și pune înapoi, neschimbat, pe celălalt (citit prin API).
- `IUpdateApi` păstrează cele două metode vechi (fără felul pachetului = pachetul cu Access) și primește două supraîncărcări cu `access`.

## Files touched

PYTHON: `routes/access.py` (nou), `routes/update.py`, `main.py`.
SQL: `sql/0104_setari_access.sql`, `sql/0104_setari_access_toate_bazele.sql` (noi).
KBot.Common: `AccessBridge.vb` (nou: `IAccessBridge`, `AccessSendResult`, `AccessBridge`, `AccessFeature`), `ClientProfile.vb` (nou),
`ServerSettings.vb` (`Access`), `KBot.Common.vbproj` (FileVersion).
KBot.Api: `IAccessTypeApi.vb`, `AccessTypeApi.vb` (noi), `IUpdateApi.vb`, `UpdateApi.vb`.
KBot.Access (proiect nou, în `KBot.sln`): `KBot.Access.vbproj`, `AccessBridgeImpl.vb`, `AccessBudgetSender.vb` (mutat din `KBot.App/Views/Nomenclatoare/`).
KBot.App: `KBot.App.vbproj`, `Program.vb`, `LoginForm.vb` + `.Designer.vb` (`tmrAccess`), `Update/AppUpdateService.vb`,
`Update/UpdateProgressForm.vb`, `Setari/SetariInfoView.vb`, `Views/Nomenclatoare/ClasificatiiForm.vb`.
Scripturi: `publish-release.ps1`, `push-update.ps1`, `tools/KBotInstaller/KBot.iss`.

## Test results (verificat vs presupus)

- **Verificat:** `dotnet build src/KBot.App` (Debug) — 0 erori, 0 avertismente; `dotnet build KBot.sln` — toate proiectele din `src/` curate;
  singurele erori sunt cele DEJA existente din `tests/KBot.App.Tests` (`capturiApi`, `JobRequest`), fără legătură cu felia.
- **Verificat (sondă fără ferestre):** `AccessBridge` găsește `KBot.Access.dll`, interfața se potrivește (același tip `KBot.Common`),
  dependența OLE DB se rezolvă din folderul componentei (ACE 16 a fost atins pe un fișier fals → «Unrecognized database format»),
  `AccessFeature.Enabled` e `False` fără rând și `True` cu `Access = 1`.
- **Verificat:** `dotnet publish` al aplicației cu `KBotAudience=NonAccess` nu conține `KBot.Access.*`, `System.Data.OleDb.dll`, `Migrare`;
  `dotnet publish` al `KBot.Access` produce `KBot.Access.dll`, `.deps.json`, `System.Data.OleDb.dll` + dependențele lui (ce copiază scriptul).
- **Verificat:** Python (`py_compile -W error`) pe `access.py`, `update.py`, `main.py`; scripturile `.ps1` trec parserul PowerShell.
- **Nu s-a rulat:** niciun script de publicare, niciun test (`pytest`/`dotnet test`), nicio cerere către server, nicio fereastră K-BOT.

## Left unverified / deferred

- `publish-release.ps1` / `push-update.ps1` nerulate cap-coadă (nici cu Access, nici fără); compilarea `KBot.iss` cu `/DWithAccess=0` nerulată.
- `routes/access.py` nerulat nici offline; testele existente de `update` (`tests/test_update_routes.py`) nerulate (forma veche e păstrată
  în `read_latest(update_dir)`, cu `access=True` implicit). Teste noi: nescrise (regula proiectului: nu se scriu teste nesolicitate).
- Pe server: **DDL înainte de cod** — `0104_setari_access_toate_bazele.sql` (valoare implicită `1` pentru bazele existente: de confirmat)
  și `0104_setari_access.sql`; apoi `routes/access.py`, `routes/update.py`, `main.py`; abia apoi un push cu noul `push-update.ps1`.
  Până se pune pe server noul `update.py`, clienții noi primesc 400/404 de la `?access=`; clienții vechi merg ca înainte.
- Un client nou care nu apucă să afle tipul (server oprit) nu verifică actualizările la pornire; le verifică la tastarea e-mailului,
  sau manual din «Setări» (care mai încearcă o dată).
- **Ajutorul și «Noutăți»:** nimic despre sistemul Access nu intră în ajutor (`HelpContent/`) și nici în `docs/release-notes/NOUTATI.md`
  (cerința operatorului, 02.10.2026). Pasajul și eticheta `0104` puse inițial în `clasificatii.md` au fost scoase. Rămân acolo, ale feliei
  0103-04 / 0103-06 (altă sesiune), rândul «Trimite în Access» și secțiunea lui: de hotărât de operator dacă pleacă și ele.
- `AccessFilePassword` (fișierul altei felii, `KBot.Common`) a rămas în `Common` fiindcă îl folosește și Migratorul; dacă pachetul fără Access
  ar trebui să nu conțină nici măcar parola ofuscată, mutarea lui e un pas separat.
- Lucru în paralel în același arbore (felia 0103-03..06): `AccessBudgetSender.vb` (netrimis în git) a fost MUTAT, nu copiat, în `KBot.Access`;
  cine mai are fișierul deschis la vechea cale trebuie să-l redeschidă.

---

# SLICE-0104-02 — pagina «Access» în Setări; calea către `cale.accdb` din setări (02.10.2026)

## What changed and why

1. **Pagină nouă «Access» în fereastra Setări** (`Setari/SetariAccessView`, cheia `access`). Apare în lista din stânga doar cât
   `AccessFeature.Enabled` (= `Setari.Access = 1` pe unitatea conectată ȘI componenta `KBot.Access.dll` instalată; urmează
   `ServerSettings.Changed`, ca pagina «Descărcări multiple»). Conține:
   - caseta **«Calea către «cale.accdb»»** (+ «Alege…» și «Reîncarcă»); se salvează la Enter / la ieșirea din câmp; câmp gol = calea implicită;
   - o **grilă** cu rândurile din tabelul `cai` al fișierului, pentru **anul** (`SessionContext.An`) și **sursa** (`SessionContext.SectorSursa`) alese în K-BOT,
     filtrate pe coloanele `AnDate` și `SURSA` (confirmat de operator că `AnDate` e coloana anului; VBA-ul vechi filtra tot `cai.AnDate = globANL()`).
     Coloanele: IdUnitate, DC, NumeUnitate, SURSA, AnDate, FullPath, CaleForexe, AlteDetalii (cele pe care le citește deja Migratorul).
2. **`DefaultRegistryPath` nu mai e constantă.** `AccessBudgetSender.DefaultRegistryPath` și `IAccessBridge.DefaultRegistryPath` au dispărut;
   calea e `AppSettings.AccessRegistryPath` (în `app_settings.json`), cu valoarea inițială `AppSettings.AccessRegistryPathDefault = C:\AVACONT\cale.accdb`.
   «Trimite în Access» (`ClasificatiiForm`) o ia de acolo.
3. **`IAccessBridge.ReadRegistry(registryPath, an, sursa) As DataTable`** (implementat în `KBot.Access`): citește tot `cai` și filtrează în cod
   (registrul ține anul ca text; un parametru tipat ar ghici). Citirea merge pe fundal; o eroare se spune pe pagină, nu se aruncă.

## Files touched
`KBot.Common/AppSettings.vb`, `KBot.Common/AccessBridge.vb`; `KBot.Access/AccessBudgetSender.vb`, `AccessBridgeImpl.vb`;
`KBot.App/Setari/SetariAccessView.vb` + `.Designer.vb` (noi), `SetariForm.vb` + `.Designer.vb`; `KBot.App/Views/Nomenclatoare/ClasificatiiForm.vb`.
Ajutor / `NOUTATI.md`: neatinse, din cerința operatorului.

## Test results
- **Verificat:** `dotnet build src/KBot.App` (Debug, trage și `KBot.Access`) — 0 erori, 0 avertismente.
- **Nu s-a rulat / nu s-a văzut pe ecran:** pagina (aspect, încadrare, comportamentul la schimbarea anului / sursei), citirea reală din `C:\AVACONT\cale.accdb`, nicio fereastră K-BOT, niciun test.

## Left unverified / deferred
- **Formatul sursei:** presupun că `SessionContext.SectorSursa` are aceeași formă ca `cai.SURSA` (`01A` / `02A` / `02E`), comparat fără diferență de litere. Dacă nu, grila iese goală și pagina o spune.
- Pagina nu se actualizează singură când se schimbă anul / sursa cât timp e deschisă: «Reîncarcă» sau reintrarea pe pagină.
- Coloanele grilei sunt cele opt cunoscute; o coloană în plus din `cai` nu se vede (grilele se declară în designer).
- Un pachet «fără Access» nu are pagina deloc (nu are componenta); `AppSettings.AccessRegistryPath` rămâne totuși în `app_settings.json`, nefolosit.

## 0104-02 finding — crash when K-BOT exits after the Access driver was used (03.10.2026)

- **Symptom:** after a window that reads or writes an Access file was used (Setări ▸ Access; «Trimite în Access» does the same), closing K-BOT ends with
  an access violation (`0xC0000005`, exit code `-1073741819`). Windows logs `Application Error` 1000 / WER 1001 (faulting module `mso98win32client.dll`,
  offset `0x8288f`) and runs the post-mortem debugger if one is registered (it was WinDbg on this PC). Nothing reaches `harness_errors.log`: the crash is native.
- **Evidence (cdb launched on `KBot.App.exe`, log `cdb_exit_094605.log`, 03.10.2026):** the stack is Windows' own process shutdown
  (`RtlExitUserProcess` → `LdrShutdownProcess` → the detach of `mso98win32client.dll` → C++ static destructors → `Mso::Floodgate::FloodgateClientInitializer`
  destructor → read of address `0x4de1`). No `KBot.*` or managed frame is in it. `ACEOLEDB.DLL` / `ACECORE.DLL` / `mso98win32client.dll` are loaded in the process
  once the Access driver is used (Office Click-to-Run copy, `Program Files\Microsoft Office\root\...`).
- **Reading:** a fault inside Office's own shutdown code, triggered by the Office driver being loaded into K-BOT. Not caused by a K-BOT change; K-BOT only started loading
  the driver with slice 0103-06 / this slice's page. The user loses nothing (the process is exiting), but every such exit is reported as a crash.
- **Not known:** whether a client with the standalone Access Database Engine (not Click-to-Run) crashes the same way.
- **Option proposed:** end the process with `TerminateProcess` after K-BOT has finished its own shutdown, only when the Access driver was used (skips the Office detach).
  Not implemented yet.

## 0104-02 — the exit workaround and the test form (03.10.2026, after the finding above)

- **`KBot.Common/FastExit.vb`** (new): `OfficeDriverLoaded()` (is `ACEOLEDB.DLL` or `mso98win32client.dll` in this process), `TerminateNow()`
  (`TerminateProcess(GetCurrentProcess(), 0)` -- the libraries are never asked to unload), `TerminateIfOfficeDriverLoaded()` (never throws).
  `Program.Main` calls the last one from its `Finally`, after `SingleInstance.Release()`, so every normal exit (shell, login cancelled, probe form, bench)
  goes through it. A process that never loaded the driver exits as before. Exit code 0.
- **`KBot.DevHarness/AccessProbeForm`** (new, Debug only, `KBotThemedForm`): path box (starts as `AppSettings.AccessRegistryPath`), year and source boxes
  (both empty = every row), «Încarcă și citește» -> `IAccessBridge.ReadRegistry` -> the rows of `cai` as text, a line saying whether the Office driver is loaded,
  and «Închide direct» -> `FastExit.TerminateNow()` straight from the button. Reached from the Debug start chooser: «Probă Access — citește cale.accdb, închide direct»
  (`StartupLauncherForm.KEY_ACCESS`, dispatched in `Program.RunLauncher`).
- **`ReadRegistry`**: a year <= 0 or an empty source now means «any» (the settings page still always passes both).
- Versions: KBot.Common 1.5.12.0, KBot.Access 1.0.1.0, KBot.DevHarness 1.0.30.0 (KBot.App left for the push script's own question).
- **Verified:** `dotnet build src/KBot.App` -- 0 errors, 0 warnings. **Not run:** the form, the exit workaround (does the crash really disappear?), any test.
- **Not covered:** `KBot.Migrator` is a separate exe that also uses the Office driver; it would exit with the same crash and does not call `FastExit`.
- **Verified 03.10.2026** (operator ran the steps, K-BOT launched under cdb; logs `cdb_fix_095609.log`, `cdb_x_095820.log`): after the probe form read `cale.accdb`
  (`ACEOLEDB.DLL` and `mso98win32client.dll` loaded in the process) the process ended with no access violation and no second-chance exception, and Windows logged no
  crash -- once closed with «Închide direct», once with the window's X (the `Program.Main` path). Before the workaround every such exit crashed.
  Also verified the same way (`cdb_full_100120.log`): the full application, login, «Setări ▸ Access» opened (driver loaded), window closed -- no access violation, no crash event,
  nothing new in `harness_errors.log`.
  `KBot.Migrator` now calls `FastExit` too (`Program.vb`, `Finally`); built, **not run**. «Trimite în Access» not exercised.
