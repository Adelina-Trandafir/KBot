# SLICE 0070 — Browserul FOREXE nu mai apare niciodată singur pe ecran

**Data:** 19.09.2026
**Cererea operatorului:** «in the kbot.forexe we make the following changes: 1. the chromium
browser CAN NEVER be displayed standalone! this allows the user to click the close button, and
then the forexe flow is lost. so whenever the user wants to see the actual browser, it will open
in its own form. like the recorder does. actually, we will use the recorder form, but if it is
opened only for view, then all the recording options (the right panel) is disabled. also, this
means that the form wil auto dock the browser on load. 2. when the browser is hidden by button or
when the parent form is closed, the browser window must be auto-detached and hidden just like the
Ascunde/Arata browser does. this is a new slice. no git. no tests!»

## Ce s-a găsit

- «Arată browserul» din consolă (`ForexeController.ToggleBrowserAsync` → `ForexeRunner.ShowBrowserAsync`
  → `WorkflowExecutor.ShowBrowserWindowAsync`) punea fereastra Chromium LIBERĂ pe ecran, la
  (100,100), cu buton de închidere — și `AddHandler _page.Close` din executor confirmă ce urmează:
  «Browser-ul a fost inchis. Aplicatia se va inchide automat!».
- Recorderul (felia 0053) andochează REAL (`SetParent` în `pnlBrowser`, felia de andocare), dar
  «Detașează» și `FormClosing` dădeau fereastra înapoi pe desktop, tot liberă și tot vizibilă
  (`UndockBrowserAsync` repunea dreptunghiul dinaintea andocării sau 1400×900 la (100,100)).
- `JobRequest.ShowBrowser` (implicit False, nesetat nicăieri în App) putea cere un browser lansat
  la vedere, tot liber.

## Ce s-a schimbat și de ce

### `KBot.Forexe` — executorul

- **`Executor/WorkflowExecutor.Docking.vb`** — `UndockBrowserAsync` = **detașare + ascundere**: ia
  fereastra din panou, îi dă înapoi stilurile de ramă, dar o parchează la (-3000,0) ca fereastră-unealtă
  (`WS_EX_TOOLWINDOW`, fără `WS_EX_APPWINDOW`), exact starea de stealth de la lansare. Rămâne
  ARĂTATĂ în sensul Win32 (`SWP_SHOWWINDOW`, niciodată `SW_HIDE`) — o fereastră ascunsă nu poate
  găzdui dialogul de certificat (`ApplyStealthWindowStyle` spune de ce). `_isBrowserVisible = False`.
  Se completează sincron (un `SetParent` + un `SetWindowPos`), lucru scris în doc: `DetachExecutor`
  din recorder se bazează pe el. `_preDockBounds` și `DockSwShow` scoase — nu mai are cine să le
  folosească. **Eveniment nou `OnDockStateChanged(docked)`**, ridicat la sfârșitul andocării și al
  detașării: fără el, «Ascunde browserul» din consolă detașa browserul de sub recorder și recorderul
  rămânea cu butoanele aprinse pe dos. `RaiseBrowserAboveAsync` (ținea browserul LIBER deasupra
  recorderului în z-order) scos: nu mai există browser liber de ținut.
- **`Executor/WorkflowExecutor.Browser.vb`** — `ShowBrowserWindowAsync` **ȘTERS** (singura cale
  spre un browser liber vizibil). `HideBrowserWindowAsync`: andocat → `UndockBrowserAsync` (ascunderea
  E detașarea); liber → ca înainte, off-screen prin CDP. Locul de parcare e acum o singură pereche de
  constante (`StealthLeft/Top/Width/Height`) folosită și de `--window-position` la lansare.
- **`JobModels.vb`** — `JobRequest.ShowBrowser` **ȘTERS**; comentariul spune de ce (KBOT_IPC avea
  comutatorul, aici fereastra se naște întotdeauna ascunsă).

### `KBot.Forexe` — recorderul și runner-ul

- **`Helpers/RecorderForm.vb`** — **`ViewOnly`** (proprietate, se poate schimba oricând):
  `True` = titlul «K-BOT Browser FOREXE», `splitMain.Panel2.Enabled = False` (tot panoul din
  dreapta: andocare, înregistrare, opțiuni, pași, previzualizare), o înregistrare în curs e oprită
  (nu poate merge în spatele unui panou stins — operatorul n-ar mai putea-o opri); `False` = «K-BOT
  Recorder», totul aprins. **Andocare automată la `Shown`** (`EnsureDockedAsync`, publică — runner-ul
  o cheamă și pe un formular deja deschis, poate operatorul detașase între timp; gardată cu
  `_docking`, fiindcă `Shown` și runner-ul pot cere andocarea în aceeași clipă, iar verificarea
  `_isDocked` a executorului stă înaintea primului `Await`). Un eșec de andocare e SPUS operatorului
  (`KBotMessage`), nu lăsat ca panou gol. `AttachExecutor` cu ACELAȘI executor nu mai desprinde și
  reatașează (ar fi detașat browserul din fața operatorului). `DetachExecutor` detașează întâi
  browserul dacă e andocat aici (executorul urmează să moară la reconectare; un Chromium care moare
  copil al panoului lasă panoul cu un handle mort) și se abonează/dezabonează la `OnDockStateChanged`
  → `UpdateButtons` pe firul de UI. Timerul de resincronizare nu mai are ramura «neandocat»:
  browserul neandocat e în afara ecranului, n-are ce ține deasupra formularului.
- **`ForexeRunner.vb`** — `ShowBrowserAsync(owner)` deschide recorderul cu `ViewOnly = True`
  (formular nou → se andochează singur la `Shown`; formular deja deschis → `Activate` +
  `EnsureDockedAsync`). Modul se poate doar RIDICA: «Arată browserul» peste un recorder în care
  operatorul înregistrează îl lasă în modul de înregistrare; «Recorder» peste un formular doar
  pentru privit îi aprinde panoul (`ShowRecorderCore(owner, viewOnly)`, comun). `HideBrowserAsync`:
  ÎNTÂI `_executor.HideBrowserWindowAsync()` (detașează de oriunde), APOI închide formularul dacă e
  `ViewOnly` (nu mai are ce arăta); cel de înregistrare rămâne cu pașii lui și își stinge singur
  butoanele prin eveniment. `stealthMode:=True` întotdeauna. `DetachRecorder` (la moartea
  executorului) închide și el formularul doar-pentru-privit. **Eveniment nou
  `BrowserVisibilityChanged`** (din `OnDockStateChanged`), ca butonul din consolă să-și schimbe
  eticheta când operatorul închide fereastra cu X-ul, fără să fi apăsat nimic în consolă.
- **`IForexeRunner.vb`** — `ShowBrowserAsync(owner As IWin32Window)` (avea nevoie de proprietar
  pentru fereastră) + `Event BrowserVisibilityChanged As EventHandler`.

### `KBot.App`

- **`Forexe/ForexeController.vb`** — `ToggleBrowserAsync` / `ShowBrowserAsync` dau `Owner`
  runner-ului; se abonează la `BrowserVisibilityChanged` și îl retransmite ca `StateChanged`, deci
  `ForexeConsoleForm.ActualizeazaStarea` recitește eticheta «Arată/Ascunde browserul».
- **`Forexe/ForexeConsoleForm.Designer.vb`** — textul tooltip-ului spune ce face acum butonul
  («Deschide o fereastră K-BOT cu pagina … Închiderea ferestrei ascunde browserul la loc.»).

### Altele

- **`tests/KBot.App.Tests/ForexeControllerFailureTests.vb`** — `FakeRunner` urmează interfața
  (semnătura `ShowBrowserAsync(owner)`, evenimentul nou). Doar ca proiectul de teste să compileze;
  nimic rulat.
- **`KBot.DevHarness/Internal/BrowserDockHarnessForm.vb`** — linia de stare la detașare spune
  «Detașat și ascuns (în afara ecranului)», fiindcă asta se întâmplă acum.

## Rezultate

- `dotnet build` pe `src\KBot.Forexe`, `src\KBot.App`, `src\KBot.DevHarness`: **0 erori,
  0 avertismente** (în afara `MSB3825` preexistent pe `.resx` în App).
- **NIMIC nu s-a rulat**: niciun browser lansat, nicio andocare făcută, nimic pe ecran, nicio suită
  de teste construită sau rulată (cerere explicită: «no git. no tests!»). **Nimic comis.**

## Neverificat / amânat

- **Andocarea automată la `Shown` pe un formular nou** — `EnsureDockedAsync` pornește o dată din
  `ShowBrowserAsync` (imediat după `Show`) și o dată din `Shown`; gardul `_docking` trebuie să
  lase una singură să treacă. Verificat doar prin citire; de văzut la prima apăsare reală pe
  «Arată browserul».
- **Închiderea recorderului cu X-ul în timp ce andocarea e în curs** (fereastra abia deschisă,
  browserul încă neandocat): `FormClosing` găsește `IsDocked = False` și închide; andocarea care
  se termină după aceea ar reparenta într-un panou eliberat. Caz de o fracțiune de secundă,
  netratat.
- **Recorderul doar-pentru-privit nu are nicio comandă** — panoul din dreapta e vizibil dar stins,
  cum a cerut operatorul («disabled»). Dacă spațiul mort deranjează, `splitMain.Panel2Collapsed`
  în `ApplyMode` e o linie.
- `ExecuteMinimizeAsync` (pasul `<Minimize>`) mută browserul liber la (-2000,-2000): tot ascuns,
  dar în alt loc decât parcarea de stealth. Inofensiv; nealiniat.

## Corecție 0070-02 — tastatura nu ajungea în pagina andocată (19.09.2026)

**Văzut pe ecran de operator:** cu browserul andocat, mouse-ul merge, dar nimic tastat nu ajunge
în pagină.

**Cauza:** rama Chromium era andocată ca `WS_CHILD` adevărat. Windows nu activează niciodată o
fereastră-copil — activează strămoșul de nivel superior, adică formularul nostru — iar Chromium
trimite caracterele tastate prin metoda lui de intrare doar cât timp propria fereastră este cea
activă (`WM_ACTIVATE`). Nici nu se poate falsifica: Chromium aruncă un `WM_ACTIVATE` adresat unei
ferestre cu `WS_CHILD`. Apăsările de taste ajungeau, caracterele erau pierdute înainte de pagină.
Aceeași familie de problemă ca la Office (`OfficeDocumentHost.PulseActivation`, unde mesajul
lipsă era `WM_ACTIVATEAPP` și acolo se putea trimite de mână).

**Schimbarea (`Executor/WorkflowExecutor.Docking.vb`):**
- Rama e reparentată **fără `WS_CHILD`** — fereastră de nivel superior ca stil, agățată de panou
  prin `SetParent` (forma clasică «notepad în panou»). Rămâne poziționată în pixelii de client ai
  panoului, tăiată de el, mutată cu formularul; dar un clic pe pagină activează fereastra
  browserului, ca atunci când stătea singură, și tastarea ajunge în pagină. Efect vizibil: cât
  operatorul tastează în pagină, bara de titlu a formularului gazdă se desenează inactivă.
- `WS_EX_TOOLWINDOW` rămâne PUS și andocat (înainte era scos): o fereastră de nivel superior ca
  stil și-ar putea câștiga un buton în bara de activități.
- `GetParent` înlocuit cu `GetAncestor(GA_PARENT)` (`ParentOf`): pentru o fereastră fără
  `WS_CHILD`, `GetParent` răspunde cu proprietarul, nu cu părintele, și verificarea de după
  `SetParent` ar fi picat.
- Activarea unei ferestre nu ridică strămoșul ei: executorul ascultă `Deactivate` pe formularul
  gazdă (`ListenToHostForm` / `OnHostFormDeactivate`) și, când activarea a plecat CĂTRE propriul
  browser, ridică formularul cu `SetWindowPos(HWND_TOP, SWP_NOACTIVATE)` — altfel un formular pe
  jumătate acoperit rămânea acoperit cât operatorul tasta. Orice altă pierdere de activare e
  lăsată în pace.
- `AttachThreadInput` rămâne.

**Rezultat:** `dotnet build` pe `src\KBot.Forexe` 0/0, `src\KBot.App` 0 erori (doar `MSB3825`).
Diagnostic din citirea codului Chromium din memorie, nu măsurat; de confirmat pe ecran la prima
tastare. Nimic rulat, nimic comis.

**Neverificat:** dacă și fără `WS_CHILD` Windows dă activarea tot rădăcinii (formularului) — atunci
tastarea tot nu merge, iar următorul pas este `SetFocus` pe rama browserului din firul UI (firele
sunt legate) la `WM_PARENTNOTIFY` pe panou; gapul lateral al ramei poate diferi cu un pixel-doi.
