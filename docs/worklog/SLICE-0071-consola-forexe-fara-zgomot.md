# SLICE 0071 — Consola FOREXE fără zgomot: doar `<Log>` și erorile în Release

**Data:** 19.09.2026
**Cererea operatorului:** «why is there so much noise in the forexe logger? … also, i need that
when the app is in release mode only the LOG action will appear and the ERRORS. the rest are
only for debug mode or if a certain flag is activated. record somewhere the name of the flag,
because it will be used in a future slice. this is a new slice. no tests. no git»

## Ce s-a găsit

Jurnalul lipit de operator (o conectare cu certificat indisponibil) avea 33 de linii, din
care operatorul avea nevoie de 3. Două surse de zgomot, diferite:

1. **Aceeași eroare scrisă de CINCI ori.** Propoziția «FOREXE a respins autentificarea…»
   apărea: (a) în `ExecuteAuthClickAsync` (`LogError` chiar înainte de `Throw`); (b) în
   `WorkflowExecutor.ExecuteAsync` (`LogException(ex, "Eroare la executare workflow")`, apoi
   re-aruncă); (c) în `ForexeRunner.RunAsync` (`LogException(ex, "Eroare conectare")`);
   (d) și (e) tot acolo, cele două linii `[DIAG]` / `[DIAG][STACK]` marcate în cod
   «DIAGNOSTIC TEMPORAR». Fiecare strat prindea excepția, o scria și o dădea mai departe —
   nimeni nu era «locul» unde se scrie o dată. Același tipar la timeout-ul de autentificare
   (`LogError(msg)` + `Throw New Exception(msg)`).
2. **Liniile de diagnostic scrise la nivel Info/Action/Success.** Pașii (`[MAIN] Pasul 1/6`),
   așteptările (`Aștept element: body`), politica de certificat, stealth, throttle, andocarea,
   recorderul — toate utile la depanare, niciuna pentru operator. `RichTextBoxLogger` nu avea
   nicio noțiune de «pentru cine e linia»: `<Log>` din workflow ajungea pe consolă prin
   ACELAȘI `LogInfo` ca «[Throttle] Dezactivat.», deci nu se putea filtra pe nivel.

## Ce s-a schimbat și de ce

### 1. Filtrul de verbozitate — `RichTextBoxLogger.VerboseLogging` (numele rezervat)

- `Public Shared Property VerboseLogging As Boolean` pe `RichTextBoxLogger`. Pornește **True
  în Debug, False în Release** (`#If DEBUG` în `InitialVerbosity()`). **Acesta e steagul pe care
  îl va folosi felia viitoare** (un comutator «jurnal detaliat» pentru operator): setarea lui la
  rulare re-desenează toate consolele deschise din buffer, deci felia aceea nu are decât să-l
  seteze.
- Regula (`ShouldDisplay`): o linie ajunge pe consolă dacă `VerboseLogging` **sau** e scrisă
  pentru operator (`operatorFacing`) **sau** e de nivel `Error`. Restul — Info, Action, Success,
  Warning, Debug, Normal fără steag — stau ascunse.
- **Fișierul-jurnal (`Logs\Log_*.txt`) și istoricul lucrărilor NU sunt filtrate**: primesc
  fiecare linie ca înainte. Filtrul e doar pe ce vede operatorul; nimic nu se pierde pentru
  citit mai târziu.
- Bufferul de re-desenare ține acum și steagul (`(Level, Msg, OperatorFacing)`), ca
  `RedrawAll` (la schimbarea temei SAU a verbozității) să aplice aceeași regulă.
  `SetColorScheme` și noul setter folosesc același `RefreshAllInstances`.
- `Log(message, level, operatorFacing:=False)` primește parametrul opțional; `LogOperator(message,
  level)` e scurtătura. `Separator()` se arată doar în modul detaliat.

### 2. Ce e «pentru operator»

- `<Log>` (`ExecuteLog`) — nivelul din atribut se păstrează (success/warning/error/info/normal),
  dar linia merge prin `LogOperator`, deci apare mereu.
- `LogValue` de pe orice acțiune (`LogStep`) — tot `LogOperator`. **Presupunere declarată:**
  operatorul a cerut «only the LOG action», dar `LogValue` există exact ca să pună pe consolă
  «textul frumos din XML» în locul detaliului tehnic («Deschid fereastra de autentificare»), deci
  l-am tratat ca text pentru operator. Dacă nu trebuie, e o singură linie de întors în `LogStep`.

### 3. Eroarea scrisă o singură dată

Locul unic e **`ForexeRunner`** (`RunAsync` / `RunJobAsync`): el prinde și ce vine din executor,
și ce vine dinaintea lui (lansarea browserului, parsarea), și știe faza («Eroare conectare»,
«Eroare rulare 'X'»). Restul straturilor nu mai scriu la nivel Error:
- `ExecuteAuthClickAsync`: cele două `LogError` de dinaintea `Throw` scoase (mesajul călătorește
  în excepție). Mesajul de timeout a rămas fără «EROARE CRITICĂ» și fără `[Auth]` — prefixul
  «Eroare conectare:» îl pune runner-ul.
- `WorkflowExecutor.ExecuteAsync`: `LogException` → `LogDebug("Eroare la executare workflow: …")`.
- `ForexeRunner`: `[DIAG]` (tip + mesaj, dublura a treia) scos; `[DIAG][STACK]` (stiva completă,
  singura informație nouă) → `LogDebug`, deci în fișier și în modul detaliat.

Aceeași conectare eșuată, în Release, va arăta acum:
```
[17:51:18] Încep workflow FOREXEBUG
[17:51:19] Deschid fereastra de autentificare
[17:51:20] ✗ Eroare conectare: FOREXE a respins autentificarea: certificatul nu este disponibil sau nu a fost acceptat. Verificați tokenul/cardul și încercați din nou.
```

### Tot în acest arbore de lucru, fără felie (cerere «no slice»)

Santinelele de eșec de pe `<AuthClick>` (`failUrl` / `failSelector` / `failMessage`;
`RaceAuthWaitsAsync` în `WorkflowExecutor.Actions.AuthClick.vb`; «Conectare» le poartă pentru
`**/vdesk/hangup.php3**` și `table#main_table.logout_page`). Jurnalul de mai sus le arată
funcționând: verdictul a venit la o secundă după click, nu după cele 120 s de `authTimeout`.
Documentate în `Surse/SURSA_FOREXE/Helpers/wfl_help.md` (singurul loc cu atributele tag-urilor).

## Fișiere atinse

- `src/KBot.Forexe/Helpers/RichTextBoxLogger.vb` — `VerboseLogging`, `ShouldDisplay`, `LogOperator`, parametrul `operatorFacing`, bufferul cu steag, `RefreshAllInstances`, `WriteInternal(display)`
- `src/KBot.Forexe/Executor/Actions/WorkflowExecutor.Actions.Log.vb` — `<Log>` prin `LogOperator`
- `src/KBot.Forexe/Executor/WorkflowExecutor.Core.vb` — `LogStep`: `LogValue` prin `LogOperator`
- `src/KBot.Forexe/Executor/WorkflowExecutor.Flow.vb` — eroarea de la granița executorului la Debug
- `src/KBot.Forexe/Executor/Actions/WorkflowExecutor.Actions.AuthClick.vb` — fără `LogError` înainte de `Throw`
- `src/KBot.Forexe/ForexeRunner.vb` — `[DIAG]` scos, `[DIAG][STACK]` la Debug (ambele blocuri)
- `docs/worklog/SLICE-0071-consola-forexe-fara-zgomot.md` (acesta), `docs/worklog/KBOT_STATUS.md`

## Rezultate

- `dotnet build` pe `src\KBot.Forexe` (Debug **și** Release, ca să se compileze ambele ramuri
  ale `#If DEBUG`), `src\KBot.Forexe.Editor`, `src\KBot.DevHarness`: **0 erori, 0 avertismente**.
- `src\KBot.App`: compilarea trece; pasul de copiere a DLL-urilor a eșuat (`MSB3021`/`MSB3027`)
  pentru că `KBot.App` rula sub Visual Studio în acel moment — nu e o eroare de cod.
- Nicio suită rulată (cerere explicită). **Nimic pe ecran, nimic comis** (cerere explicită).

## Neverificat / amânat

- Consola Release n-a fost văzută: cele 3 linii de mai sus sunt deduse din reguli, nu citite de
  pe ecran. De confirmat la prima conectare pe un build Release.
- Re-desenarea la comutarea `VerboseLogging` la rulare n-a fost exersată (nu există încă
  comutator) — e drumul `SetColorScheme`, care există și merge de la felia temelor.
- Presupunerea despre `LogValue` (§2).
- `Warning`-urile rămân ascunse în Release (regula cerută: `<Log>` + erori). Oprirea printr-un
  `<Exit>` («Execuție oprită: …», `LogWarning`) nu se vede pe consolă, dar runner-ul întoarce
  `Failed`, deci operatorul o primește pe drumul lucrării, nu prin jurnal.
- **Numele steagului pentru felia viitoare: `RichTextBoxLogger.VerboseLogging`** (Shared,
  `KBot.Forexe/Helpers/RichTextBoxLogger.vb`). Notat și în Open threads din STATUS.

## Corecție 0071-02 — în Release robotul îngheța după «Deschid fereastra de autentificare» (19.09.2026)

**Semnalat de operator:** «something is wrong. in release the browser doesn't navigate to the
authpage so the failsafe is never triggered.»

**Ce spun jurnalele** (`src\KBot.App\bin\Release\net8.0-windows\Logs\`, nefiltrate):
- `Log_20260919_180148.txt`: conectarea a eșuat corect, santinela a vorbit la o secundă după click.
- `Log_20260919_180320.txt` și `Log_20260919_180944.txt`: ultima linie a robotului e «Deschid
  fereastra de autentificare»; `[Auth] Aștept navigarea către…` — linie care merge în fișier
  oricum, fără filtru — **nu mai apare**. Deci firul executorului s-a oprit ÎNTRE cele două, adică
  în `LogStep`, înainte de click. Browserul nu «nu navighează»: nimeni n-a mai apucat să dea click.

**Cauza:** o capcană WinForms scoasă la iveală de filtru. Consola e construită ascunsă
(`MainForm.EnsureConsole`), deci `RichTextBox`-ul ei **n-are handle** până la prima deschidere.
`Control.InvokeRequired` răspunde **False cât timp nu există handle**, așa că prima linie afișată
creează handle-ul PE FIRUL CARE O SCRIE. Până acum prima linie era mereu «Lansare browser…», scrisă
de `ForexeRunner.RunAsync` pe firul UI (înainte de primul `Await Task.Run`), deci handle-ul se năștea
unde trebuie. În Release, cu filtrul, prima linie afișată e `<Log>Încep workflow FOREXEBUG`, scrisă
din executor pe un fir din pool → handle-ul se naște pe firul acela → următoarea linie afișată
(«Deschid fereastra…», alt fir din pool după `Await`) face `Invoke` către un fir care nu pompează
mesaje → blocat pentru totdeauna. Rularea de la 18:01 a mers pentru că atunci consola era deja
deschisă (handle pe firul UI). În Debug nu se vede pentru că acolo TOATE liniile se afișează, deci
prima e tot cea de pe firul UI.

**Ce s-a schimbat (`RichTextBoxLogger`):**
- `UiReady()` = cutia există, nu e dispusă **și are handle**. Toate atingerile UI (`WriteInternal`,
  `RefreshDisplay`, `Clear`) trec pe aici; fără handle, linia rămâne în buffer și fișierul/istoricul
  o primesc oricum. Niciodată nu mai creăm handle-ul dintr-un fir de lucru.
- La construcție: `AddHandler richTextBox.HandleCreated, Sub() RefreshDisplay()` — când consola se
  deschide prima oară (pe firul UI), bufferul se rejoacă în ea, deci nimic nu lipsește. La fel la
  o recreare de handle.
- `Clear()` fără handle golește doar bufferul, apoi continuă cu ștergerea fișierului ca înainte.

**Rezultate:** `dotnet build` pe `src\KBot.Forexe` (Debug + Release), `src\KBot.DevHarness`,
`src\KBot.App`: **0 erori, 0 avertismente**. Nimic rulat, nimic pe ecran, nimic comis.

**Neverificat:** drumul reparat n-a fost văzut pe un build Release cu consola închisă — exact
scenariul de la 18:04/18:09; de refăcut de operator. Rămâne în picioare și `Application.DoEvents()`
apelat din `WriteInternal` pe firul de lucru (moștenit; nu-l apără nimic — de cântărit separat).
