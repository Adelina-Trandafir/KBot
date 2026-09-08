# SLICE-0053-03 — Recorder: z-order nedocat, viewport la andocare, conectare fără token în Debug

Data: 2026-09-07

Trei observații ale operatorului după prima încercare de a folosi recorderul cablat în
pasul 02 (`SLICE-0053-02-recorder-cablare.md`). Toate trei sunt piedici în calea
verificării pe viu a andocării — punctul (2) din firele deschise ale feliei.

## 1. Nedocat, browserul cădea în spatele formularului

**Simptom:** cu browserul NEandocat, orice clic pe recorder trimitea fereastra Chromium
în spatele lui.

Era exact ce trebuia să se întâmple: nimic nu ținea browserul deasupra. Calea andocată
avea deja linia care rezolvă asta — `SyncDockedBoundsAsync` cheamă `SetWindowPos(hwnd,
ownerHandle, …, NOACTIVATE)`, adică «pune browserul imediat deasupra formularului gazdă,
fără să-i dai focusul». Nedocat, `ScheduleResync` ieșea din prima linie
(`If Not _executor.IsDocked Then Return`), deci nu se întâmpla nimic.

Acum:

- `WorkflowExecutor.RaiseBrowserAboveAsync(owner)` — perechea nedocată a acelei linii de
  z-order, fără CDP și fără mutarea ferestrei. `owner.Handle` se citește ÎNAINTE de
  `Await`: după el continuarea poate să nu mai fie pe firul de UI.
- `RecorderForm.ScheduleResync` nu mai iese când e nedocat, atâta timp cât există browser;
  `TmrResync_Tick` se ramifică — andocat resincronizează marginile, nedocat doar ridică
  browserul deasupra.

Temporizatorul era deja acolo pentru amortizarea tragerilor, deci nu s-a adăugat niciun
mecanism nou: aceleași evenimente (`Move`, `Resize`, `Activated`, mutarea splitterului)
duc acum la două acțiuni diferite, după starea de andocare.

**Consecință de spus cu voce tare:** nedocat, browserul stă acum DEASUPRA recorderului și
îl poate acoperi complet (fereastra detașată e pusă la `100,100` × `1400×900`).
Recorderul rămâne cel activ pentru tastatură, dar poate fi nevoie să fie mutat browserul
ca să se vadă lista de pași. Andocat, nu se schimbă nimic — panoul e tocmai locul făcut
pentru browser.

## 2. Viewportul emulat, la andocare

Playwright desenează pagina într-un viewport EMULAT — contextul se creează în
`WorkflowExecutor.Browser.vb` fără `ViewportSize`, deci implicit 1280×720 — iar viewportul
acela NU urmează fereastra. Andocarea redimensionează fereastra la panoul gazdă, deci
pagina rămânea desenată la dimensiunea veche, cu chenar gol în jur.

`DockBrowserToAsync` cheamă acum `ClearEmulatedViewportAsync()` imediat după prima
sincronizare de margini: dacă `_page.ViewportSize` e ceva (adică viewportul e activ), o
sesiune CDP trimite `Emulation.clearDeviceMetricsOverride` și pagina începe să urmeze
fereastra reală. Este exact modul «no viewport» al lui Playwright, aplicat după crearea
contextului — API-ul .NET nu-l poate scoate altfel decât la construcție.

Nu se pune la loc la detașare, intenționat: o pagină care își urmează fereastra e corectă
în ambele stări, pe când repunerea celor 1280×720 ar lăsa chenar gol în fereastra
detașată de 1400×900.

Eșecul se STRIGĂ în jurnal (`LogWarning`), nu pică andocarea: o pagină desenată la
dimensiunea veche e urâtă, nu un motiv să refuzi andocarea pe care operatorul tocmai a
cerut-o.

## 3. Conectare fără token, doar în Debug

Fără token în cititor, `CertificateSelectionForm` se deschidea goală, anunța lipsa și se
închidea cu `Cancel`; `ForexeController.ConnectAsync` ieșea cu `False` și nu se lansa
niciun browser. Adică pe un calculator fără token nu se putea încerca NIMIC din andocare,
înregistrare sau monitorizare.

`ConnectAsync` întreabă acum întâi `NoTokenInDebug()`:

- **Release:** `False` întotdeauna — comportament neschimbat, lipsa tokenului oprește
  conectarea.
- **Debug cu token:** `False` — dialogul apare normal, iar un «Renunță» al operatorului
  rămâne un «Renunță».
- **Debug fără niciun certificat pe token/smartcard:** se sare peste dialog (ar fi gol) și
  se merge mai departe cu `certificate = Nothing`.

Merge fiindcă nimic din calea de lansare nu are nevoie de certificat:
`ConfigureAutoSelectCertificatePolicy` iese din prima linie când e `Nothing`, iar
`_windowsSecurityAutomation` nu se instanțiază în build-ul curent. Autentificarea la
FOREXE va pica — dar **browserul rămâne deschis** (decizia A3 din `ForexeRunner.RunAsync`:
«Browserul rămâne deschis pentru investigație»), deci `HasLiveSession` devine `True`,
`btnRecorder` se aprinde și andocarea/monitorul pot fi încercate.

Starea afișată spune ce s-a întâmplat («Fără token — pornesc sesiunea NEAUTENTIFICATĂ
(doar Debug).»), iar `lblCert` din consolă rămâne pe «—», onest.

## Fișiere atinse

- `src/KBot.Forexe/Executor/WorkflowExecutor.Docking.vb` — `ClearEmulatedViewportAsync`,
  `RaiseBrowserAboveAsync`, apelul de curățare a viewportului în `DockBrowserToAsync`
- `src/KBot.Forexe/Helpers/RecorderForm.vb` — `ScheduleResync` / `TmrResync_Tick`
  ramificate pe starea de andocare
- `src/KBot.App/Forexe/ForexeController.vb` — `NoTokenInDebug`, ramura din `ConnectAsync`
- FileVersion: `KBot.Forexe` 1.0.6 ▸ 1.0.7, `KBot.App` 1.0.26 ▸ 1.0.27

## Rezultate

- `dotnet build KBot.sln -v q --nologo` — **0 erori**, 7 avertismente (`MSB3825` pe
  `.resx`-urile din `KBot.App`, preexistente).

## Rămas neverificat

- **Nimic din cele trei n-a fost rulat pe un browser adevărat.** Toate trei ating exact
  punctul (2) al feliei — andocarea nerulată vreodată pe viu — deci se verifică odată cu el.
- `Emulation.clearDeviceMetricsOverride` se trimite pe o sesiune CDP NOUĂ, nu pe cea a
  lui Playwright. Emularea se aplică pe `WebView`, deci ștergerea dintr-o a doua sesiune ar
  trebui să funcționeze (ultimul scrie câștigă), dar asta rămâne de confirmat pe ecran:
  dacă pagina tot rămâne la 1280×720 după andocare, alternativa e crearea contextului cu
  `ViewportSize.NoViewport` în `WorkflowExecutor.Browser.vb` — schimbare care atinge însă
  TOATE workflow-urile, nu doar recorderul.
- Ridicarea browserului deasupra formularului nedocat n-a fost văzută: nu se știe cât de
  vizibil e pâlpâitul dintre activarea formularului și tick-ul temporizatorului.
