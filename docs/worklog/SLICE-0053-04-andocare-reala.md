# SLICE-0053-04 — Andocare REALĂ (`SetParent`) + banc de probă pentru andocare și monitor

Data: 2026-09-07

Operatorul: «andocarea tot nu merge». Și: «fă un banc de probă care deschide un browser,
navighează la <https://mfinante.gov.ro/web/forexepublic> și face tot ce trebuie să facă
formularul de monitorizare, inclusiv andocarea». Și, explicit: «ignoră regula după care
fereastra Chromium nu are voie să fie andocată cu `SetParent`; ai voie».

## 1. De ce nu mergea andocarea: fereastra nu era găsită

`GetOrRefreshBrowserHwndAsync` căuta fereastra plimbând TOATE procesele mașinii și comparând
`Process.MainWindowTitle` cu marcajul unic `WF_BROWSER_<guid>`. Marcajul se scrie o singură
dată, în `document.title`, pe `about:blank`, la pornirea browserului.

**Prima navigare reală șterge marcajul.** Titlul ferestrei devine titlul sitului, iar căutarea
nu mai găsește nimic: 20 de încercări × 250 ms = cinci secunde de scanat procese, apoi
`IntPtr.Zero`, apoi excepția «Nu am găsit fereastra browserului. Andocarea nu poate continua.»
Handle-ul nu era pus în cache la pornire decât pe calea *stealth* (`ApplyStealthWindowStyle`),
deci în mod normal nu exista niciun handle memorat care să salveze situația.

Acum:

- handle-ul se rezolvă **la pornire**, cât timp marcajul e încă pe pagină — o singură dată,
  în `LaunchAndPositionBrowserAsync`, imediat după ce se scrie titlul;
- dacă totuși cache-ul e gol mai târziu, căutarea **repune marcajul** în `document.title`,
  găsește fereastra, apoi **pune titlul real la loc** (operatorul nu rămâne cu o fereastră
  numită `WF_BROWSER_<guid>`);
- căutarea folosește `EnumWindows` + filtru pe clasa `Chrome_WidgetWin_1`, nu
  `Process.MainWindowTitle`: vede fereastra chiar dacă nu e fereastra principală a procesului,
  nu plimbă toate procesele, și nu prinde din greșeală ferestrele-ajutor invizibile pe care
  Chromium le atârnă de același titlu — o astfel de fereastră mutată prin ecran arată exact
  ca o andocare care nu face nimic.
- `GetBrowserHwndAsync`, geamănul mort al funcției (nechemat de nimeni), a fost șters.

## 2. Andocare adevărată: `SetParent`

Regula «fereastra Chromium nu se reparentează» a căzut la cererea operatorului, iar odată cu
ea a căzut și tot mecanismul construit ca s-o ocolească.

Înainte: fereastra rămânea de nivel superior, era ținută peste panou cu CDP
`Browser.setWindowBounds` (coordonate fizice → DIP cu `* 96 / DeviceDpi`) și împinsă imediat
deasupra formularului cu `SetWindowPos`, la fiecare mutare, redimensionare și activare.
Două lucruri se rupeau constant: găsirea ferestrei (§1) și cursa de z-order cu fiecare
activare a formularului gazdă.

Acum, în `DockBrowserToAsync`:

1. se rețin stilurile, părintele și dreptunghiul ferestrei, ca detașarea să le poată da înapoi;
2. `ShowWindow(SW_RESTORE)` — o fereastră maximizată se luptă cu dimensiunea primită;
3. stilurile devine de copil: fără `WS_CAPTION`, `WS_THICKFRAME`, `WS_SYSMENU`, butoanele de
   minimizare/maximizare și `WS_POPUP`; cu `WS_CHILD` și `WS_VISIBLE`; scoasă din bara de
   activități;
4. `SetParent(hwnd, panou)`. **Rezultatul lui `SetParent` nu spune nimic** — întoarce părintele
   ANTERIOR, care e null și pentru o fereastră de nivel superior, și null și la eșec — deci
   se întreabă `GetParent` cine e părintele acum. Dacă nu e panoul: stilurile se pun la loc și
   se aruncă. O fereastră rămasă fără stiluri e mai rea decât o andocare care n-a avut loc;
5. `AttachBrowserInput` leagă coada de intrare a firului UI de cea a firului ferestrei
   browserului. Fără asta, un copil Chromium reparentat primește mouse-ul dar **nu** și
   tastatura: focusul urmează fereastra de prim-plan, care e a firului formularului. Best
   effort — o tastare capricioasă nu e motiv să refuzi andocarea cerută;
6. viewportul emulat se elimină ca înainte (`Emulation.clearDeviceMetricsOverride`).

`SyncDockedBoundsAsync` s-a redus la un `MoveWindow(hwnd, 0, 0, latime, inaltime, True)`:
coordonatele unui copil sunt pixelii CLIENT ai părintelui, adică exact ce raportează WinForms.
**Nicio conversie DPI, niciun drum prin CDP, niciun z-order de întreținut** — un copil nu poate
cădea în spatele propriului părinte. Funcția nu mai e `Async` de fapt; întoarce `Task` ca să nu
se schimbe niciun apelant.

`UndockBrowserAsync` face drumul invers: dezleagă cozile de intrare, `SetParent` la părintele
dinainte, repune stilurile și dreptunghiul (sau `100,100 × 1400×900` dacă cel memorat nu era
folosibil), `SWP_FRAMECHANGED`.

`RaiseBrowserAboveAsync` rămâne, dar **numai pentru browserul NEdocat** — are acum o gardă
`If _isDocked Then Return`.

### Prețul, spus cu voce tare

Fereastra browserului aparține acum panoului. **Eliberarea panoului sau închiderea formularului
gazdă fără detașare distruge fereastra Chromium și, cu ea, sesiunea.** De-aia `RecorderForm` și
bancul de probă detașează în `FormClosing`, înaintea oricărei eliberări. Comentariul-antet din
`RecorderForm.vb`, care spunea exact pe dos («browserul nu e reparentat»), a fost măturat.

## 3. Bancul de probă: `BrowserDockHarnessForm`

Nou în `KBot.DevHarness`, se vede în banc la categoria **FOREXE**, sub numele
«FOREXE — Andocare browser + monitor Wicket». Nu cere certificat și nu cere token: sesiunea nu
se autentifică niciodată, singurul lucru care contează e fereastra.

Un buton («Pornește + navighează») face toată secvența:

```
lansează Chromium → instalează monitorul Wicket → navighează la pagina publică → andochează
```

**Ordinea nu e întâmplătoare.** `StartWicketMonitoringAsync` își pune scriptul cu
`AddInitScriptAsync`, iar un script de inițializare rulează abia la navigarea URMĂTOARE — deci
monitorul intră ÎNAINTE de încărcarea paginii, niciodată după. Butonul «Reîncarcă pagina»
există pentru același motiv: e felul în care un monitor pornit târziu devine viu.

Monitorul **nu e copiat**: `pnlMonitor` găzduiește `WicketMonitorForm`-ul real
(`TopLevel = False`), legat de același executor. Ce se vede în banc e literalmente ce vede
operatorul în aplicație — inclusiv culorile pe fluxuri `[WFL]` / `[WICKET]` / `IDLE` /
`[CLICK]` / `[KEY]`.

Restul barei: «Andochează», «Detașează», «Resincronizează», «Pornește/Oprește monitorul».
Jos: jurnalul executorului (`RichTextBoxLogger` real, care scrie și în
`<AppDir>\Logs\harness_dock_<stamp>.log`) și verdictul uman («Andocarea merge» / «nu merge»),
ca la celelalte probe vizuale din banc.

Pe pagina publică FOREXE **fluxul `[WICKET]` rămâne tăcut**: `#statlogo` / `#animlogo` sunt
elemente ale aplicației Wicket și nu există acolo, iar `WicketMonitor.js` iese din prima linie
când nu le găsește. `[CLICK]` și `[KEY]` funcționează pe orice pagină. Formularul spune asta
în jurnal la pornire, ca tăcerea să nu fie citită drept defect.

## Fișiere atinse

- `src/KBot.Forexe/Executor/WorkflowExecutor.Docking.vb` — rescris pe reparentare
- `src/KBot.Forexe/Executor/WorkflowExecutor.Browser.vb` — căutarea ferestrei prin
  `EnumWindows` + remarcare a titlului, handle rezolvat la pornire, `GetBrowserHwndAsync` șters
- `src/KBot.Forexe/Helpers/RecorderForm.vb` — comentariul-antet, care descria vechea andocare
- `src/KBot.DevHarness/Internal/BrowserDockHarnessForm.vb` / `.Designer.vb` — bancul
- `src/KBot.DevHarness/Tests/BrowserDockHarnessTest.vb` — intrarea în banc
- FileVersion: `KBot.Forexe` 1.0.7 ▸ 1.0.8, `KBot.DevHarness` 1.0.25 ▸ 1.0.26

## Rezultate

- `dotnet build KBot.sln -v q --nologo` — **0 erori**, 7 avertismente `MSB3825` preexistente
  pe `.resx`-urile din `KBot.App`.
- **Formularul a fost randat cu `DrawToBitmap`** înainte de raportare, și randarea a prins o
  eroare adevărată: `SplitContainer` proaspăt construit are 150 px lățime, iar `EndInit`
  refuză o `SplitterDistance` care nu încape între `Panel1MinSize` și `Width - Panel2MinSize` —
  formularul **arunca în constructor**, deci bancul nu s-ar fi deschis niciodată. Reparat prin
  `Size` înaintea minimelor și a distanței (`Dock = Fill` preia la prima trecere de aranjare).

## Rămas neverificat

- **Nimic n-a atins încă un browser adevărat.** Exact ăsta e rostul bancului: prima rulare
  spune dacă reparentarea prinde.
- Comportamente de urmărit la prima rulare, în ordinea probabilității de a supăra:
  **(a)** Chromium își desenează propriul chenar (banda de file ESTE bara de titlu); scoaterea
  lui `WS_CAPTION` s-ar putea să nu schimbe nimic vizual — ceea ce e în regulă, bara de adresă
  rămâne folosibilă. **(b)** Tastarea în pagina andocată: dacă `AttachThreadInput` nu ajunge,
  se vede imediat în jurnal («Firele de input nu au putut fi legate»). **(c)** DPI: un copil
  moștenește contextul de DPI al părintelui, deci pe un monitor cu altă scalare pagina poate
  ieși la scară greșită. **(d)** Dialogul de certificat / PIN peste browserul andocat.
- Butonul de copiere din `WicketMonitorForm` (`btnCopyText`) are ca text emoji-ul `🗐`, pe care
  fontul Calibri al aplicației nu-l redă: în randare apare un pătrat gol. Preexistent, nu ține
  de felia asta.

---

## Adăugire — bara browserului ascunsă în panou

Cerință: cât timp e andocat, browserul nu trebuie să-și arate **banda de file** și **bara de
adrese**. Nu se face cu un argument de Chromium (`--app` / `--kiosk` se bat cu felul în care
Playwright deschide paginile și cu dimensionarea panoului), ci prin **poziționare**:

- fereastra primește un `Top` negativ, exact cât banda ei de sus, și o înălțime mai mare cu
  aceeași valoare — zona paginii cade fix peste panou, iar banda rămâne deasupra marginii de
  sus a panoului;
- o fereastră-copil e decupată de părinte, deci banda nu se desenează și nu se poate apăsa;
- fereastra rămâne neatinsă: la detașare, sau cu `HideChromeWhenDocked = False`, bara revine.

**Se compensează DOAR partea de sus.** Prima variantă compensa și stânga, dreapta și jos, cu
aceeași măsurătoare — și operatorul a rămas fără **barele de derulare** ale paginii: fereastra
copilului de randare nu le acoperă, așa că lărgirea le împingea dincolo de marginile panoului.
O bordură reală rămasă pe laturi înseamnă câțiva pixeli morți; o bară de derulare lipsă
înseamnă o pagină pe care nu o mai poți citi.

Banda e **măsurată, nu presupusă**: pagina se desenează într-o fereastră-copil de clasă
`Chrome_RenderWidgetHostHWND`, iar distanța de la marginea de sus a cadrului la ea dă
înălțimea reală, oricare ar fi DPI-ul, tema sau bara de marcaje. O cifră neverosimilă (peste
400 px) e respinsă; dacă măsurarea eșuează se folosește ultima bună, iar dacă nu există niciuna
bara rămâne vizibilă și jurnalul o spune **o singură dată** pe andocare. Valoarea măsurată e
scrisă în jurnal la fiecare schimbare, ca o bandă greșită să se vadă în cifre, nu doar în
imagine.

`WorkflowExecutor.HideChromeWhenDocked` (implicit `True`) comută starea, cu efect imediat pe
un browser deja andocat. Bancul are bifa «Ascunde bara browserului», ca ambele stări să poată
fi văzute fără repornire.

### Viewport: contextul se creează cu `NoViewport`

Cu bara ascunsă a ieșit la iveală un defect mai vechi: **pagina nu urma dimensiunea panoului**,
așa că barele ei de derulare cădeau în afara lui. Vinovat e viewport-ul implicit al Playwright
— 1280x720 **emulat**, împins cu `Emulation.setDeviceMetricsOverride` — care nu urmează
fereastra. Andocat, fereastra e panoul, iar pagina continua să se așeze la lățimea veche.

Ștergerea override-ului la andocare (`ClearEmulatedViewportAsync`) **nu e de ajuns**: Playwright
îl pune la loc. Reparația stă la crearea contextului:

```vb
_context = Await _browser.NewContextAsync(New BrowserNewContextOptions() With {
    .IgnoreHTTPSErrors = True,
    .AcceptDownloads = True,
    .ViewportSize = ViewportSize.NoViewport
})
```

Fără emulare, pagina **este** fereastra ei, corect și andocată și liberă. `ScreenshotAsync` se
face peste tot cu `FullPage = True`, deci nimic din fluxuri nu depindea de 1280x720.
`ClearEmulatedViewportAsync` rămâne ca plasă de siguranță pentru o sesiune construită altfel.

### Fișiere atinse

- `src/KBot.Forexe/Executor/WorkflowExecutor.Docking.vb` — `HideChromeWhenDocked`,
  `ApplyDockedBounds`, măsurarea benzii (`MeasureChromeBandPx` / `FindLargestRenderWidget`)
- `src/KBot.Forexe/Executor/WorkflowExecutor.Browser.vb` — contextul cu `NoViewport`
- `src/KBot.DevHarness/Internal/BrowserDockHarnessForm.vb` / `.Designer.vb` — bifa

### Rămas neverificat

- **Nici asta n-a atins încă un browser adevărat**, din același motiv ca restul feliei.
- Cu bara ascunsă, operatorul **nu mai poate tasta o adresă**: navigarea rămâne treaba
  fluxului. Dacă vreodată e nevoie de o adresă scrisă de mână, bifa o aduce înapoi.
- Scurtături ca `Ctrl+T` deschid o filă pe care nu o mai vede nimeni.

