# SLICE-0073/01/76 — Istoric al urmăririi paginii FOREXE, stilurilor și marcajelor

21–23.09.2026: etapele sunt păstrate integral. 0073 introduce meniul și urmărirea cu descărcare imediată; 0073-01 repară instalarea timpurie a stilurilor și adaugă filtrul Pagina; 0076 adaugă modificarea recepției (TODO-ul inițial), fluxurile parțiale, memoria rezervărilor și marcajele. În 0073-01, afirmația «singurul fișier atins» aparține primei treceri JS; pasul 2 descrie și modificări .NET. Rezervarea ID-urilor și operațiile server sunt descrieri istorice, neexecutate aici. Setările operatorului sunt în [ghid](../SETARI_UTILIZATOR.md).

Gazdele și andocarea sunt în [0053/0070/0074](SLICE-0053-recorder.md). Legarea inițială numai după IDREV din 0076 este restrânsă ulterior în [0114-26](SLICE-0114-25-repara-rezervari-fereastra-previzualizare.md#slice-0114-26) la ziua și tipul rezervării; se păstrează aici mecanismul inițial și limitele lui.

[0073-recorder](#slice-0073) · [0073-01](#slice-0073-01) · [0076-editari](#slice-0076)

## SLICE 0073 — Recorder: mod de vizualizare, meniu plutitor în pagină, urmărirea operațiunilor FOREXE <a id="slice-0073"></a>

**Data:** 21.09.2026 **Cererea operatorului:** «1. two options when showing it (1. default - no recording - splitmain.panel2 should be hidden, 2 - recording - panel2 should be visible; 2. all the buttons in the panel 2 must have defaultbutton from theme applied; 3. if possible i need a popup float menu inserted into the browser which will be always visible for the user in the browser and which will have zoom in / zoom out options. also, when certain buttons in the browser are clicked it must start a kind of recording which will send the data back to kbot.app.forexe … when the user, for instance, creates a new Angajament in the ForexeCab app, my app will instantly know it and when he finishes pressing a certain button, all the info will be sent back to kbot and saved to the server - just like when downloading from forexecab. it, then, should start a new window getting the history from that angajament for the selected timeframe (when the user made the first click until it pressed that button)» — completat: «it must be able to do it for: modificare rezervare, adaugare/modificare receptie si creeare anagament (simplu sau multiplu)» și «citeste wfl-urile de aici: Workflows\Creare. Lipseste DOAR modificare receptie. o sa ramana TODO dupa ce ii aflu fluxul.»

### Ce s-a schimbat și de ce

#### 1. Cele două moduri ale recorderului (`RecorderForm`)

- `ViewOnly = True` («Arată browserul») **strânge** panoul din dreapta (`splitMain.Panel2Collapsed`), nu-l mai dezactivează: pagina are toată fereastra. `ViewOnly = False` («Recorder») îl desface. Comutarea cere o resincronizare a browserului andocat (`ScheduleResync`), fiindcă strângerea nu ridică `SplitterMoved`.
- Toate butoanele din panoul din dreapta, la orice adâncime (`ButtonsUnder`), primesc **butonul implicit al temei**: `ButtonStyles.ApplyDefault` (nou în `KBot.Theming`) — exact regula generică a lui `ThemeManager` pentru un `Button` (sloturile ButtonBack / ButtonText / ButtonBorder, colțuri moderne când schema le cere), dar aplicată explicit, deci și sub o schemă care păstrează culorile de sistem. Se reaplică la fiecare `ThemeChanged`.

#### 2. Meniul plutitor din pagină (`Services/JavaScripts/ForexeWatch.js`, resursă embedată)

- Un panou fix, jos-dreapta, mutabil cu mouse-ul (poziția rămâne în `localStorage`): **−, +, 100 %** (zoom CSS pe `documentElement`, meniul contra-scalat ca să-și țină mărimea; factorul rămâne în `localStorage`, deci supraviețuiește navigărilor), linia de stare a urmăririi și trei butoane manuale: **▶ Începe, ■ Gata, ✕**.
- Se instalează ca `Recorder.js`: `AddInitScriptAsync` (navigările viitoare) + `EvaluateAsync` (pagina de pe ecran), gardat împotriva dublei instalări. Un temporizator îl repune dacă dispare.
- **Urmărirea operațiunilor** — reguli `OPS`, toate cu selectori copiați din `.wfl`-urile din `Workflows\Creare` (aceiași pe care îi apasă robotul):
  - `angajament`: start `a:has-text('Angajament nou')`; final = **prima** `button.btn-success` din `.form-group` (salvarea FINALĂ; a doua e cea intermediară «încă un rând» — deci «multiplu» dă o singură operațiune, la salvarea finală).
  - `rezervare` (doar cu `li.tab0.active`): start «Adaug…» (`button.btn-default.btn-small`) sau ochiul unui rând (`table.table-striped a:has(.glyphicon-eye-open)`); final `button.btn-success.btn-small:nth-child(1)` sau «Continuă» din modalul motivului (`div.modal-footer button.btn-success`).
  - `receptie` (doar cu `li.tab1.active`): start `button` «Adaugă»; final «Salvează» din `form.form-horizontal`.
  - **`receptie-modificare`: TODO** — fluxul nu e cunoscut; nu există reguli, doar butoanele manuale.
- După apăsarea salvării, scriptul **așteaptă să se așeze pagina** (sondaj la 400 ms, cel mult 30 s): `#animlogo` ocupat sau modal deschis → așteaptă; `.feedbackPanelERROR` / `.alert-danger` → salvare respinsă, operațiunea rămâne în curs; testul `done` al operațiunii (codul din `.well.well-small h4 span:nth-child(2)` prezent + starea paginii) → **finished**. Starea trece prin `sessionStorage`, deci o salvare care navighează e confirmată de pagina următoare.
- Evenimente către .NET prin `_kbotWatchCallback`: `started / finished / cancelled / info`, cu `op, label, cod, codAtStart, startedAt, finishedAt, url, message`.
- **Suspendare cât rulează robotul**: `ForexeRunner.RunJobAsync` cheamă `SetWatchSuspendedAsync(True/False)` în jurul lui `ExecuteAsync` — clicurile robotului ar arma operațiuni (Creare Angajament.wfl apasă chiar «Angajament nou»), iar meniul ar sta peste țintele lui Playwright. Suspendarea se ține și în `sessionStorage`, pentru navigările robotului.

#### 3. Conducta .NET

- `Models/ForexeWatchEvent.vb` (+ `ForexeOperationKind`, `ForexeWatchEventKind`).
- `Executor/WorkflowExecutor.Watch.vb`: `StartWatchingAsync` (instalare o dată, `ExposeFunction` `_kbotWatchCallback`), `StopWatching`, `SetWatchSuspendedAsync`, evenimentul `OnWatchEvent`; scrie pe consolă liniile pe care le citește operatorul («A început…», «s-a salvat…»).
- `RecorderForm.EnsureDockedAsync` / «Andochează» → `StartWatchAsync` după andocare, în ambele moduri (un eșec e spus, nu desface andocarea).
- `IForexeRunner.OperationCaptured` → `ForexeRunner` (abonat la executor, dezabonat la `DisposeExecutorAsync`) → `ForexeController.OperatiuneCapturata` → shell.
- `KbotForm.ForexeWatch.vb` (partial nou): la **Finished** — pe firul de UI —
  - `rezervare / receptie / manual`: codul din pagină (la sfârșit, altfel la început), în lipsă nodul selectat în arbore (spus pe consolă); apoi **`DownloadNodeAsync`** (fără întrebarea recepțiilor de sărit — totul e proaspăt) + **`DuLaIngestieAsync`** (aceleași două faze ca iconița nodului) + fereastra de istoric.
  - `angajament`: întâi **sincronizarea listei** (drumul iconiței din subsol, fără caseta de mesaj), codurile noi = cele care nu erau în arbore + codul citit din pagină; fiecare e descărcat și ingerat pe rând, cu fereastra lui de istoric.
  - o singură operațiune în lucru la un moment dat; a doua e anunțată și lăsată pe iconița nodului.
- **`IstoricIntervalForm`** (nou, `KBotShellForm`): găzduiește `IstoricView`-ul real, încărcat cu `SetContextInterval(cod, deLa, panaLa)` — un segment nou de dată **cu oră** (`IstoricFilter.SetDataFxRange`), aplicat imediat după `ClearAll()`-ul obligatoriu din `LoadAsync`. Intervalul e lărgit cu **±2 minute** (`MarjaMinute`): momentele vin din ceasul mașinii, `DataFX` din ceasul serverului FOREXE. «Tot istoricul» scoate limita; «TOATE» din meniul de dată și «Reset» fac același lucru.

### Fișiere atinse

`src/KBot.Theming/ButtonStyles.vb` (+`ApplyDefault`), `KBot.Theming.vbproj` (1.13.0 ▸ 1.13.1); `src/KBot.Forexe/Services/JavaScripts/ForexeWatch.js` (nou), `Models/ForexeWatchEvent.vb` (nou), `Executor/WorkflowExecutor.Watch.vb` (nou), `Helpers/RecorderForm.vb`, `IForexeRunner.vb`, `ForexeRunner.vb`, `KBot.Forexe.vbproj` (resursă + 1.0.10 ▸ 1.0.11); `src/KBot.App/Forexe/ForexeController.vb`, `KbotForm.vb` (`LeagaUrmarirea()` în Load), `KbotForm.ForexeWatch.vb` (nou), `Forexe/IstoricIntervalForm.vb` + `.Designer.vb` (noi), `Views/IstoricView.vb`, `Views/IstoricFilter.vb`, `KBot.App.vbproj` (1.0.32.4 ▸ 1.0.33.0); `tests/KBot.App.Tests/ForexeControllerFailureTests.vb` — `FakeRunner` declară noul eveniment (doar editat; nicio suită construită sau rulată); `docs/worklog/KBOT_STATUS.md`, acest fișier

### Rezultatele testelor

- `dotnet build` pe `src\KBot.Forexe`, `src\KBot.App` (`--no-incremental`), `src\KBot.DevHarness`: **0 erori, 0 avertismente**. `node --check ForexeWatch.js`: valid.
- **Nicio suită xUnit / pytest construită sau rulată** (regula casei).
- **Văzut pe ecran** prin `DrawToBitmap` (proiect aruncabil în scratchpad, schema Classic din fișierul temei, fără `SetScheme`): recorderul în modul înregistrare (panoul desfăcut, butoanele plate în culorile temei) și în modul vizualizare (titlu «K-BOT Browser FOREXE», panoul strâns, pagina pe toată fereastra).

### Neverificat / amânat

- **Nimic nu a atins un browser adevărat sau CABWeb.** Meniul plutitor, zoom-ul CSS pe paginile Wicket, potrivirea regulilor de start/final pe butoanele reale, sondajul de confirmare a salvării și conducta până la `DuLaIngestieAsync` + fereastra de istoric se confirmă la prima operațiune reală. Selectorii sunt cei ai robotului, dar robotul îi apasă într-o ordine controlată — operatorul poate ajunge pe alte drumuri (ex. pagina de editare a unui indicator deschisă altfel decât prin ochi).
- **`receptie-modificare` — TODO** până operatorul aduce fluxul; între timp, ▶ Începe / ■ Gata.
- **Codul unui angajament nou** se ia din antetul paginii după salvarea finală (`.well.well-small h4 span:nth-child(2)`, ca în `Creare Angajament.wfl` §4) ȘI din diferența listei; dacă antetul nu apare pe pagina de după salvare, rămâne diferența listei — care vede doar perioada afișată în arbore.
- **Descărcarea pornește imediat**, fără confirmare, cum a cerut operatorul («instantly»): robotul preia browserul andocat chiar după salvare. Dacă operatorul înlănțuie operațiuni pe aceeași pagină, a doua e anunțată și lăsată pe iconița nodului.
- Ceasurile: ±2 minute e o presupunere; dacă istoricul iese gol în fereastră, «Tot istoricul» arată totul, iar marja se poate mări.
- Nu există încă un fișier de reguli editabil de operator — regulile stau în `ForexeWatch.js`.

## SLICE 0073-01 — Foile de stil ale paginii FOREXE intră înainte de prima afișare și rezistă re-randărilor Wicket <a id="slice-0073-01"></a>

**Data:** 21.09.2026 **Fișier:** `src/KBot.Forexe/Services/JavaScripts/ForexeWatch.js` (singurul atins)

### Cererea operatorului

1. Blur-ul (vălul din timpul unei lucrări a robotului) se pierde «la fiecare refresh AJAX al paginii».
2. La fel culorile întunecate (modul dark trimis în pagină).
3. Bara de meniu FOREXE (ascunsă de regulile paginii) dispare «abia după un timp scurt».
4. Dacă meniul nu poate fi ascuns mai repede, blur-ul să fie forțat până dispare meniul.

### Ce s-a găsit

Jurnalul monitorului Wicket dat de operator pentru clicul pe «Istoric» se termină cu:

```
19.169  #animlogo → none
19.176  #statlogo → inline      ← sfârșitul apelului Ajax
19.223  #statlogo → inline      ← DUBLURĂ, în ordinea stat/anim
19.228  #animlogo → none
```

Ultimele două linii sunt exact raportul inițial pe care `WicketMonitor.js` îl face la `DOMContentLoaded` (`#statlogo` întâi, apoi `#animlogo`) — adică un **document nou**: apelul Ajax s-a încheiat cu o redirecționare Wicket, o navigare completă, nu doar o re-randare.

Pe document nou, `AddInitScriptAsync` rulează înainte de `<html>`: `head` și `documentElement` sunt null, iar inserarea stilului cade în `catch (ignored)`. `boot()` nu instala foile; abia bătaia `MENU_KEEPALIVE_MS` de 2 s instala `kbot-watch-style` (reguli + dark) și `kbot-watch-busy-style`. Aceeași lipsă explica toate cele trei simptome; punctul 4 ar fi mascat cauza, nu era necesar după corecție.

Presupunere marcată ca atare: că `documentElement` e `null` în init script-ul Playwright pe Chromium nu a fost măsurat pe FOREXE (nu s-a atins un browser real); e comportamentul cunoscut al `Page.addScriptToEvaluateOnNewDocument` și se potrivește cu toate simptomele. Dacă totuși foile ar intra târziu, scriptul spune acum în consolă, o singură dată pe document: «stilurile paginii au intrat abia la încărcarea completă».

### Ce s-a schimbat

Secțiunea nouă **11** în antetul scriptului. Concret:

- `sheetParent()` — `<head>`, altfel `<html>`, altfel `null`; `installStyles()` / `installBusyStyle()` nu mai creează un `<style>` fără părinte și întorc `true/false`. `installStyles` rescrie `textContent` doar când s-a schimbat.
- `whenRootExists(fn)` — rulează imediat dacă `<html>` există, altfel un `MutationObserver` pe `document` (`childList`) prinde momentul în care parserul inserează `<html>`, cu mult înainte de prima afișare; `DOMContentLoaded` e rezerva. La start: `whenRootExists(earlyInstall)`.
- `resync()` — într-o singură trecere: cele două foi, clasa `kbot-busy` de pe `<html>`, vălul, starea ascunsă a meniului; `scheduleResync()` o amână cu `setTimeout(0)` ca o rafală de mutații să dea o singură rulare.
- `hookWicket()` — `Wicket.Event.subscribe('/ajax/call/complete' | '/dom/node/added')` (Wicket 6+) sau `Wicket.Ajax.registerPostCallHandler` (versiuni vechi); reîncercat din bătaie până când scriptul Wicket e în pagină. Versiunea Wicket a FOREXE nu e cunoscută din depozit, de aceea ambele.
- `watchRootAndHead()` — observatoare pe `<html>` (`class`, copii) și pe `<head>` (copii): o foaie scoasă sau clasa pierdută revine în același tick.
- `syncBusy()` atinge clasa doar când e greșită (altfel observatorul de pe `<html>` s-ar trezi singur la nesfârșit) și nu mai aruncă pe `documentElement` null.
- `boot()`: `resync()` + `watchRootAndHead()` întâi, apoi mesajul «au intrat târziu» dacă e cazul. Bătaia de 2 s rămâne ultima plasă, prin `resync()`.

Nimic pe partea .NET: `SetWatchSuspendedAsync` / `ApplyWatchConfigAsync` / ridicarea regulilor pentru un clic al robotului (`liftStyles`) sunt neschimbate.

### Rezultatele testelor

- `node --check ForexeWatch.js`: valid.
- Pagina de probă în browserul încorporat (fișier de scratch, cu un `Wicket.Event` fals, `sessionStorage` marcat «suspendat», config cu `darkMode` + o regulă `.navbar{display:none}`):
  - după încărcare: `kbot-busy` pus, ambele foi în `<head>`, `.navbar` `display:none`, `<html>` cu `invert(1)`, conținutul cu `blur(7px)`, meniul K-BOT ascuns;
  - foile și clasa scoase de mână + `Wicket.Event.publish('/ajax/call/complete')`: totul la loc în < 30 ms;
  - foaia și clasa scoase din nou, FĂRĂ eveniment Wicket: totul la loc în < 30 ms (doar observatoarele);
  - `setSuspended(false)`: blur-ul și clasa pleacă, meniul apare, dark + regula rămân.
- Nicio suită `dotnet test` rulată (regula casei). Niciun build necesar: fișierul e resursă încorporată, nicio linie VB nu s-a schimbat.

### Neverificat / amânat

- Nu s-a atins un browser Playwright real, deci calea `AddInitScriptAsync` cu `<html>` încă neparsat nu a fost văzută pe ecran — de urmărit în consola K-BOT la prima lucrare: mesajul «stilurile paginii au intrat abia la încărcarea completă» NU trebuie să apară.
- API-ul Wicket al FOREXE (Event vs. registerPostCallHandler) nu a fost citit din pagină (rețeaua era închisă din sandbox); ambele sunt abonate defensiv.
- Codul din arborele de lucru (vălul, dark, «Pagina FOREXE» din Setări, gardurile de salvare) este încă necomis și fără felie proprie; această corecție stă deasupra lui în același fișier și nu poate fi comisă separat de el.

### Pasul 2 (aceeași zi) — o regulă ține doar pe O pagină: «Pagina»

**Cererea:** butonul «Înapoi» (`button.btn.btn-default` din `[class*='col-lg-10']` din `span.nav.nav-tabs`) ascuns DOAR pe `https://forexe.mfinante.gov.ro/CABWeb/contract` (fără `?6`), ca regulă vizibilă și editabilă în «Setări» → «Pagina FOREXE».

- `PageStyleRule.Page` (gol = orice pagină) + constructor cu patru argumente, `Clone`, și a cincea regulă implicită: «Butonul «Înapoi» din bara de file ascuns pe pagina angajamentului», selector `span.nav.nav-tabs [class*='col-lg-10'] button.btn.btn-default`, `display: none`, pagina `https://forexe.mfinante.gov.ro/CABWeb/contract`.
- `AppSettings`: `PageStyleRuleDto.Page` dus-întors (cheia JSON `page`; lipsă = gol).
- `ForexeWatchConfig`: câmpul `page` în JSON-ul trimis paginii.
- `ForexeWatch.js`: `pageMatches(page)` — adresa documentului fără `?…`/`#…`; o valoare cu `://` se compară cu origin + cale, una care începe cu `/` cu calea, altfel cu ultimul segment al căii; fără majuscule/minuscule. `cssFromRules` sare regulile care nu țin pe pagina de acum; foaia se recalculează la fiecare `resync()` (navigare, eveniment Ajax Wicket, bătaie).
- `RegulaPaginaEditor` (+ Designer): al patrulea câmp «Pagina (gol = pe orice pagină)» între «Selector» și «Stil», cu tooltip; `Fill` (elementul ales din arbore) nu atinge «Pagina».
- `SetariPaginaView` (+ Designer): coloana «Pagina» în grilă («(toate)» când e gol).
- `docs/SETARI_UTILIZATOR.md` §7.1: a cincea regulă și paragraful «Pagina».
- `docs/SETARI_UTILIZATOR.md` §7.4 (nou): când intră foile și cum rămân — versiunea pentru operator a pasului 1, cu trimiteri din introducerea §7 și din §7.2.

**Build:** `dotnet build src\KBot.App\KBot.App.vbproj --no-incremental`: 0 erori, 0 avertismente. `node --check` valid. Nicio suită rulată (regula casei).

**De știut:** regulile implicite intră în listă doar la prima pornire sau la «Implicite» (care înlocuiește lista), deci pe un `app_settings.json` deja salvat regula nouă NU apare singură: ori «Implicite» + «Salvează și aplică», ori «Regulă nouă…» cu cele patru câmpuri de mai sus. Selectorul e CSS, deci nu poate cere textul «Înapoi»; dacă în același `span.nav.nav-tabs` mai stă un `btn-default`, selectorul trebuie strâmtat de mână (neverificat pe pagina reală).

## SLICE 0076 — Editările din «Browser FOREXE»: recepția salvată, rezervările ținute în memorie, marcajele de id <a id="slice-0076"></a>

**Data:** 23.09.2026 **Cererea operatorului (rezumat):**

1. Fluxul de recepții rulat după o editare în vederea «Browser FOREXE» **nu pornește de la început** (operatorul e deja pe tabul Recepții), citește **doar recepția editată** (identificată prin data ei, reținută la clicul pe ochiul rândului) sau **doar ultima** pentru o recepție nouă, apasă «Istoric» și citește istoricul **ca fluxul REVERSE**. Modificarea unei recepții trebuie și detectată (ochiul din fiecare rând).
2. Fluxul de rezervări: **fiecare editare ținută în memorie** (tot ce ia partea de rezervări din «Prelucrare Completa»), fără preambul, **întrebare după fiecare salvare** dacă s-a terminat; la «DA», memoria devine exact ce ar fi întors fluxul normal (ca să nu se mai citească o dată), plus istoricul citit ca REVERSE.
3. Marcajele: `(IDREV: x)` la sfârșitul motivului unei rezervări, `(IDRH: x; IDR: y)` în descrierea unei recepții, ca istoricul să spună direct cui îi aparține fiecare rând (rezervarea în `Descriere`, recepția în `Observatii`-le rândului de total).

Răspunsurile operatorului la cele trei întrebări puse înainte de scris: **(a)** marcajele: *rezervare + scriere + citire* (serverul rezervă id-urile, pagina le scrie, ingestia le folosește); **(b)** «șirul utilizabil» = ce întoarce fluxul normal de rezervări, ca să nu se mai citească încă o dată; **(c)** fișiere `.wfl` **noi** pentru browser, iar cele vechi își păstrează preambulul (iconițele din subsolul arborilor le folosesc cu browserul oriunde).

### Ce s-a schimbat și de ce

#### 1. Două fluxuri noi, fără preambul (`Workflows\`)

- **`adlop - Receptie Editata.wfl`**: antetul (.well), tabul Recepții, `ScrapeTable` pe listă, apoi `ForEachVar` care deschide **numai** rândul-țintă: `{{RECEPTIE_TINTA}}` = `ULTIMA` (recepție nouă → `[[R.IsLast]]`) sau data `zz/ll/aaaa` (recepție editată → `[[R.Data]]`). Fiecare rând poartă în `collectFields` și **`Citita`** (1 / 0). Pe ramura necitită se golesc cele patru variabile, ca în fluxul complet (altfel rândul ar lua `Detaliu`-ul vecinului). Apoi tab0 → «Istoric» → citire **de la ultima pagină înapoi** până la `{{DATA_IESIRE}}` (exact `Istoric Angajament REVERSE.wfl`) → «Înapoi» → **înapoi pe tabul Recepții**.
- **`adlop - Rezervari Editate.wfl`**: antetul + istoricul REVERSE + «Înapoi». Indicatorii NU se citesc: vin din memorie (§3).
- **`DATA_IESIRE` fără istoric local** = `(?!)` (`WorkflowCatalog.DataIesireNiciuna`), un tipar care nu se potrivește cu nimic, deci se citește tot istoricul. Gol ar fi fost invers: `^` se potrivește cu orice rând, deci citirea s-ar fi oprit la primul.
- **`adlop - Receptii Angajament.wfl`**: preambulul comentat de operator (V.3) **pus la loc**, cu nota V.4 — fișierul e rulat și de iconița din subsolul arborelui de recepții, care se apasă cu browserul oriunde (decizia (c)).
- `WorkflowCatalog`: `ReceptieEditataFile`, `RezervariEditateFile`, `VarReceptieTinta`, `ReceptieTintaUltima`, `DataIesireNiciuna`. `JobBuilder.BuildReceptieEditata(cod, data?, ultimaData?)`, `BuildRezervariEditate(cod, ultimaData?)`, `PuneDataIesire`.

#### 2. Pagina (`ForexeWatch.js`, secțiunile 12 și 13)

- **`receptie-modificare` are reguli** (nu mai e TODO): pornește la ochiul unui rând din tabul Recepții (`li.tab1.active`, același ochi pe care îl apasă «Prelucrare Completa»), se salvează cu același «Salvează». La clic se reține **data rândului** (`rowDate`), la salvare **data din formular** (`formDate`, `input[name='data']`, normalizată la `zz/ll/aaaa`); evenimentul `finished` duce `data.dataReceptie` = data din formular, altfel cea a rândului.
- **Rezervarea**: la pornire se citește lista indicatorilor din tab0 (`rezBefore`) și, la ochi, codul rândului (`rezIndicator`); la clicul pe salvare se citește **tabelul de buget** de pe pagina de editare (exact tabelul pe care îl răzuiește fluxul complet în spatele ochiului); la confirmare, pe tab0, se citește **rândul indicatorului** (după cod; la «Adaugă» — rândul al cărui cod nu era în listă). `data = {indicator, indicatorCod, buget}`.
- Tabelele se citesc cu o **copie** a lui `ScrapeTableExtract.js` (`scrapeTable`), ca rândul să aibă exact cheile dintr-un pachet descărcat (`Indicator_ang`, …, `Col_12`).
- **Formular închis fără salvare** («Înapoi» al paginii, alt tab): la următoarea bătaie de 2 s operațiunea se încheie ca la «Renunță» (`checkAbandoned`, după ce formularul a fost văzut o dată). Altfel o recepție doar deschisă pentru privit ar fi blocat orice pornire ulterioară.
- **Marcajele**: clicul pe «Continuă» din modalul motivului / pe «Salvează» din formularul recepției e **ținut** (după gardianul salvărilor fără modificări, înaintea urmăririi), pagina cere marcajul prin `_kbotWatchCallback` (`event: 'marcaj'`), .NET răspunde cu `window._kbotWatch.setMarcaj(id, text, motiv)`, marcajul se pune la sfârșitul textului (**înlocuind** unul mai vechi — o recepție editată își păstrează descrierea) și clicul e **reluat**. Fără răspuns în 8 s, sau cu eroare, salvarea pleacă **fără** marcaj și consola spune de ce: munca operatorului nu e niciodată ținută ostatică.

#### 3. .NET

- `ForexeWatchEvent.DetaliiJson` (obiectul `data` al paginii, text JSON); `WorkflowExecutor` tratează `marcaj` separat (nu e operațiune, nu ajunge în shell): `SetMarcajProvider`, `RaspundeLaMarcaj` (răspunde **întotdeauna**, cu marcajul sau cu motivul).
- `ForexeRunner` primește în constructor puntea `Func(tip, cod, ct) → marcaj` (ca la Excel); `Program.vb` o înregistrează peste `IMarcajApi` (nou, `KBot.Api`, implementat de `ApiClient` într-un fișier parțial — **ținut în afara lui `IApiClient`**, altfel toate cele zece falsuri din teste ar fi trebuit atinse).
- `ForexeController`: `DescarcaPartialAsync` primește acum o **fabrică de job** (rulată în interiorul porții, fiindcă citirea istoricului local folosește tokenul operației) și o **transformare** a pachetului. Noi: `DownloadReceptieEditataAsync(cod, data?, citesteIstoric)` și `DownloadRezervariEditateAsync(cod, randuri, citesteIstoric)`.
- `WorkflowResultStore`: `DoarReceptiileCitite` (rândurile cu `Citita = 1`, fără coloana `Citita`; lista brută tăiată după datele păstrate), `CuIndicatoriMemorati` (pune `TabelIndicatori_results` — rând + `BugetIndicator` imbricat — și `TabelIndicatori`, adică exact cele două tabele ale secțiunii 1 din «Rezervari Angajament»).
- **`KbotForm.ForexeWatch.vb`**:
  - recepție nouă → `DownloadReceptieEditataAsync(cod, Nothing)` (ultimul rând); recepție editată → cu data din pagină; fără dată → reîmprospătarea tuturor recepțiilor (mai lent, niciodată greșit), spus pe consolă;
  - rezervare → **nu se mai descarcă la fiecare salvare**: rândul se pune în `RezervariInLucru` (pe cod de angajament, un rând pe indicator, ultima salvare câștigă) și se întreabă «Ați terminat modificarea rezervărilor?». **NU** = se continuă. **DA** = `DownloadRezervariEditateAsync` + ingestia în două faze; memoria se golește doar după o descărcare reușită.

#### 4. Serverul

- **`routes/forexe/marcaj.py`** (nou) — `POST /api/forexe/marcaj/rezerva` `{tip, cod}`:
  - **rezervarea unui număr = avansarea contorului AUTO_INCREMENT** al tabelei: un rând-substitut inserat și șters în aceeași tranzacție. InnoDB nu mai dă niciodată acel număr, deci un `INSERT` ulterior care îl numește explicit nu se poate ciocni cu unul automat. Numărul se notează și în `FX_NumberLock` (Tip `IDREV` / `IDRH` / `IDR`, codul angajamentului, cine, până când) — doar evidență;
  - `rezervare` → **același IDREV** cât timp lacătul angajamentului e viu (o sesiune de editare a rezervărilor = o revizie DDF), TTL 30 de zile, prelungit la fiecare cerere; `receptie` → o pereche nouă la fiecare salvare, TTL 2 zile;
  - `id_marcaj_utilizabil` (id-ul nu e deja în tabelă ȘI e sub contor — deci chiar a fost dat de contor), `consuma_lacatul`, `idrev_tinut`.
- `prelucrare_helpers`: `compune_marcaj`, `extract_marcaj` (ultimul marcaj din text), `fara_marcaj` (un text **fără** marcaj se întoarce neatins).
- Ingestia (`prelucrare_pasi.py`):
  - **3b**: `(IDREV: n)` din `Descriere` (sau `Observatii`) câștigă în fața vechiului `(REV:nn)`; se scrie în `FX_Istoric.IDREV` chiar dacă revizia nu există încă (coloana n-are cheie străină); 3c/3d scriu deja NULL + avertisment în `FX_Rezervari` cât timp lipsește;
  - **4a**: antetul cu `(IDRH: n; IDR: m)` naște `FX_Receptii_H` **cu IDRH = n**, iar prima lui linie `FX_Receptii` cu **IDR = m** (restul liniilor pe contor); un marcaj nefolosibil e ignorat și spus în jurnal; `Descriere` se scrie fără marcaj;
  - **4b**: marcajul iese din `DescriereReceptie` înainte de hash și de `FX_Receptii_R.Descriere`.
- **Salvarea DDF** (`ddf_edit.py`): o revizie **nouă** a unui angajament care are un IDREV ținut se naște **cu acel IDREV** (lacătul se consumă), iar pasul nou **8.1b** leagă rezervările nelegate ale căror rânduri de istoric poartă acel IDREV.

#### 5. O reparație găsită pe drum (felia 0060)

`WorkflowResultStore.FaraReceptiileSarite` tăia doar `ListaReceptii`, dar serverul citește **`ListaReceptii_results`** — deci recepțiile nebifate ajungeau totuși la pasul 4b cu `Detaliu` gol, adică exact recepția pe jumătate actualizată pe care tăietura trebuia s-o împiedice. Acum se taie ambele tabele.

### Fișiere atinse

`src/KBot.Forexe/Workflows/adlop - Receptie Editata.wfl` (nou), `adlop - Rezervari Editate.wfl` (nou), `adlop - Receptii Angajament.wfl` (preambul pus la loc), `KBot.Forexe.vbproj` (copiere + 1.0.12 ▸ **1.0.13**), `WorkflowCatalog.vb`, `JobBuilder.vb`, `ForexeRunner.vb`, `Executor/WorkflowExecutor.Watch.vb`, `Models/ForexeWatchEvent.vb`, `Services/JavaScripts/ForexeWatch.js`; `src/KBot.Api/IMarcajApi.vb` (nou), `ApiClient.Marcaj.vb` (nou), `KBot.Api.vbproj` (1.0.6 ▸ **1.0.7**); `src/KBot.App/Program.vb`, `Forexe/ForexeController.vb`, `Forexe/WorkflowResultStore.vb`, `KbotForm.ForexeWatch.vb`, `KBot.App.vbproj` (1.0.36.5 ▸ **1.0.37.0**); `PYTHON/routes/forexe/marcaj.py` (nou), `__init__.py`, `prelucrare_helpers.py`, `prelucrare_pasi.py`, `ddf_edit.py`; `docs/worklog/KBOT_STATUS.md`, acest fișier

### Rezultatele testelor

- `dotnet build` pe `src\KBot.Api`, `src\KBot.Forexe`, `src\KBot.App` (`--no-incremental`), `src\KBot.DevHarness` (`--no-incremental`): **0 erori, 0 avertismente**. Cele două `.wfl` noi apar în `bin\...\Workflows\`.
- Toate cele 11 `.wfl` se parsează ca XML; `node --check ForexeWatch.js`: valid.
- Python: pachetul `routes.forexe` se importă cu `.venv`; `compune_marcaj` / `extract_marcaj` / `fara_marcaj` exersate de mână (`(IDREV: 112)`, `(IDRH: 631; IDR: 2570)`, un text fără marcaj rămâne identic, `(REV:3)` nu e luat drept marcaj, `extract_numar_rev` nu vede `(IDREV:`).
- **Nicio suită xUnit / pytest construită sau rulată** (cerere explicită: «no TESTS»). Interfețele pe care le implementează falsurile din teste (`IApiClient`, `IForexeRunner`) **nu s-au schimbat**.
- Regula 0: diacritice numai în textele pe care le citește operatorul (verificat pe diff).

### Neverificat / amânat

- **Nimic nu a atins CABWeb, un browser adevărat sau MariaDB.** De confirmat la prima sesiune:
  - că ochiul din tabul Recepții deschide «Modifică recepție» și că `input[name='data']` există în formular (altfel rămâne data rândului);
  - ordinea listei de recepții — «ultima» = **ultimul rând** al tabelului, cum a cerut operatorul; dacă FOREXE sortează altfel, o recepție nouă cu dată mai veche ar fi citită greșit;
  - că rândul de total al recepției are descrierea (cu marcaj) între `Receptie: ` și prima virgulă a `Observatii`-lor (cum o citește deja pasul 4a) și că FOREXE nu taie textul;
  - că motivul rezervării ajunge în `FX_Istoric.Descriere` (afirmația operatorului, nevăzută);
  - că reluarea clicului (`button.click()`) e acceptată de Wicket ca un clic obișnuit;
  - că tabelul de buget de pe pagina de editare e cel din fluxul complet (`table.table-bordered.table-hover.table-condensed`) și că `input[name^='tableContainer:']` NU apare și pe lista din tab0 (altfel închiderea fără salvare a unei rezervări nu se detectează — inofensiv, doar nu se curăță singură).
- **Drepturile contului serverului**: rezervarea șterge rânduri din `FX_DDF_REV`, `FX_Receptii_H`, `FX_Receptii` și citește `information_schema.TABLES`. Nevăzut dacă contul le are pe fiecare bază.
- **Persistența contorului**: se sprijină pe MariaDB ≥ 10.2.4 (contorul InnoDB supraviețuiește repornirii). Versiunea serverului e 10.11 după felia 0075-00 — deci în regulă, dar nescris nicăieri ca verificat.
- **Memoria rezervărilor trăiește cât shell-ul.** La închiderea K-BOT cu rezervări nepreluate ele rămân în FOREXE (și în istoric), dar nu în K-BOT până la o reîmprospătare a nodului. Nu se întreabă la închidere.
- O rezervare pe un **indicator nou** al cărui rând n-a putut fi citit din pagină: istoricul lui vine, dar serverul nu cunoaște indicatorul — se spune pe consolă să se descarce nodul.
- **Enter** într-o casetă de text a formularului de recepție sau a ferestrei «Motiv» este oprit (`onMarcajKey`): se salvează doar cu butonul, care pune marcajul. În casetele cu mai multe rânduri Enter face rând nou, ca înainte.
- Salvarea DDF leagă automat numai o revizie **nouă**; o revizie existentă editată nu consumă IDREV-ul ținut.

## Reper de livrare păstrat din registru

Registrul 0076 cerea publicarea pe VPS a `routes/forexe/marcaj.py` nou, `prelucrare_helpers.py`, `prelucrare_pasi.py`, `ddf_edit.py`, `__init__.py`, apoi repornirea gunicorn. Este lista istorică de livrare, fără confirmare că publicarea a avut loc și fără executare în această consolidare.
