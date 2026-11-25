# SLICE-0097-10 — «Descarcă angajamente NOI»: citirea listei de la sfârșit + ce s-a citit rămâne dacă FOREXE dă eroare

Cererea operatorului (20.11.2026): citirea listei de angajamente cădea mereu pe la pagina 10; angajamentele din FOREXE sunt în ordinea introducerii, deci robotul să se oprească la ultimul angajament deja descărcat din program, iar la eroare să aducă ce a citit până atunci (doar pentru acest flux). Felia: ultima care a atins citirea robotului (0097-08), primul sub-număr liber.

## What changed and why

**Diagnosticul (din jurnalele calculatorului «energetic», `Logs_energetic_20112026.rar`).** Fluxul «Listă angajamente curente» citea înainte, de la pagina 1, cu `a[rel='next']`, ~5 s pe pagină. Pe 20.11 a căzut la pagina 11 (08:28 și 08:54) și la 12 (09:52); pe 19.11 la pagina 6 (de două ori), 11 și 12. De fiecare dată, la rularea următoare apare linia «Url gresit … `CABWeb/ErrorPage?NN`»: FOREXE a răspuns la pagina următoare cu pagina lui de eroare, iar tabelul n-a mai apărut în 10 s. Fluxul n-avea `partialOnError`, așa că tot ce se citise se pierdea. **Textul paginii de eroare, primit de la operator (20.11.2026, 10:11:55):** «S-a produs o eroare: org.apache.wicket.core.request.handler.ListenerInvocationNotAllowedException: Behavior rejected interface invocation. Component: [AjaxPagingNavigationIncrementLink [Component id = next]] … IBehaviorListener.onRequest()». Deci e aceeași eroare ca la istoric (02.10.2026), iar sondarea după textul «S-a produs o eroare» (și, în plus, după adresa `ErrorPage`) o prinde. **De ce o dă FOREXE nu e dovedit**: Wicket refuză clicul pe legătura «next» (comportament respins), cel mai probabil o legătură rămasă veche după ce tabelul s-a redesenat (jurnalul din 19.11 are și «element was detached from the DOM»); asta e ipoteză. Citirea de la sfârșit folosește legătura «precedenta», nu «next»: dacă și ea e respinsă, se păstrează ce s-a citit.

**Schimbat**
- `adlop - Lista Angajamente Curente.wfl` (V.5): `startFromLast`, `lastPageSelector="a.last"`, `prevPageSelector="a.prev"`, `exitIfCellEquals="Cod:~:^\s*{{COD_IESIRE}}"`, `partialOnError="true"`. Noile angajamente sunt la sfârșit: se citesc 1–2 pagini, nu 30+.
- `COD_IESIRE` (`WorkflowCatalog.VarCodIesire`, mereu trimis, `JobBuilder.BuildListaAngajamente(…, codIesire)`): (1) ultimul angajament ținut minte pentru unitate | an | program | sursă, din `WorkflowResults\UltimulAngajament.json` (`WorkflowResultStore.UltimulCunoscut`); (2) altfel, oricare din codurile din arbore (primul întâlnit de la sfârșit e cel mai nou cunoscut); (3) altfel `(?!)` — se citește tot, tot de la sfârșit.
- Ținerea minte se face **abia după ce serverul a primit rândurile** (`ForexeController.ConfirmaListaPrimita`, chemat din `KbotForm.Download` și `KbotForm.ForexeWatch`) și **nu** după o citire tăiată de eroare (variabila `ListaAngajamente_INCOMPLET`, pusă de `SaveScrapeResults`): rândurile mai vechi nu s-au citit, nu trebuie socotite cunoscute.
- `WicketErrorTextAsync` recunoaște și adresa `…/ErrorPage` (nu doar textul «S-a produs o eroare», nevăzut confirmat pe această pagină). Se aplică și fluxurilor de istoric (care aveau deja `partialOnError`).
- Lista goală (nimic nou: rândul cunoscut e primul) e acum rezultat normal, nu «tabelul lipsește»; `ApiClient.AdaugaAngajamenteNoiAsync` nu mai face apel pentru 0 rânduri.
- Ajutor: `contabil/forexe/lista.md` și turul `descarcare-angajamente-noi.md` (etichetă 0097-10).
- Versiuni: KBot.Forexe 1.0.21.0 → 1.0.22.0, KBot.Api 1.0.31.0 → 1.0.32.0 (KBot.App: la `push-update.ps1`).

## Files touched

`src/KBot.Forexe/Workflows/adlop - Lista Angajamente Curente.wfl`, `…/WorkflowCatalog.vb`, `…/JobBuilder.vb`, `…/Executor/Actions/WorkflowExecutor.Actions.ScrapeTable.vb`, `KBot.Forexe.vbproj`; `src/KBot.Api/ApiClient.vb`, `KBot.Api.vbproj`; `src/KBot.App/Forexe/Robot/ForexeController.vb`, `WorkflowResultStore.vb`; `src/KBot.App/KBotForm/Robot/KbotForm.Download.vb`, `KbotForm.ForexeWatch.vb`; ajutor în `HelpContent/`.

## Test results

`dotnet build src/KBot.App/KBot.App.vbproj -c Debug` (cu Forexe și Api): 0 avertismente, 0 erori. Nimic rulat pe FOREXE, niciun test (regula proiectului).

## Unverified / deferred

- **`a.last` / `a.prev` pe pagina «Listă angajamente» NU au fost văzute** (luate de la istoric, același paginator). Dacă lipsesc, citirea rămâne pe prima pagină și se oprește — semn: «Am ajuns la prima pagină».
- **Ordinea «cel mai nou la sfârșit» e din spusa operatorului**, nevăzută în pagină; dacă ar fi inversă, rândul cunoscut nu se întâlnește la început, iar `MissedKnownRow` reia o dată și citește tot.
- Dacă ultimul cunoscut a dispărut din FOREXE, citirea merge pe tot, cu o reluare (cost dublu, o dată).
- Prima citire după o instalare (fișierul `UltimulAngajament.json` nu există — `WorkflowResults` e șters de actualizare) folosește codurile din arbore; un angajament salvat din «Browser FOREXE» (0097-07) e «cunoscut» și poate ascunde unul mai vechi, necitit încă.
- Dacă FOREXE taie citirea chiar din prima descărcare, rămân neadunate cele mai vechi; se aduc la o descărcare care nu e tăiată.
- `KbotForm.Console.SincronizeazaAsync` (nu se mai poate porni din shell din 20.09.2026) face upsert complet; cu lista scurtă nu mai trimite tot — netrecut prin `ConfirmaListaPrimita`.
- Rândul din jurnalul operatorului spune acum «Angajamente citite din FOREXE (de la sfârșit, până la ultimul cunoscut)»; cifra e a celor noi, nu a întregii liste. NOUTATI: linia de adăugat la următoarea versiune.
- Nu s-a făcut nicio operațiune git.

## Corrective (20.11.2026, 10:26) — «waiting for Locator(a.last)» deși ultima pagină era afișată

Jurnalul operatorului: «Navighez la ultima pagina» → după 5 s «Navigare la ultima pagina nereușită (tentativa 1/3)» → 30 s mai târziu «Timeout 30000ms exceeded … waiting for Locator("a.last")». Operatorul a văzut că robotul ajunsese pe ultima pagină: `a.last` a fost găsit și apăsat, deci selectorul din `.wfl` e bun.

**Cauza (din cod + jurnal, nu din pagină):** `NavigateToLastPageAsync` aștepta doar `WaitForWicketIdleAsync` (cel mult 5 s), apoi verifica dacă butonul a dispărut. Răspunsul Ajax al unei liste lungi a întârziat peste cele 5 s, deci butonul încă exista la verificare; bucla a apăsat din nou același locator, dar între timp butonul dispăruse (pe ultima pagină nu mai există), iar clicul a așteptat 30 s și a căzut. Erori de același fel puteau apărea la orice flux cu citire de la sfârșit (`Istoric Angajament REVERSE`, `Prelucrare Completa Reverse` etc.), dar numai pe un răspuns lent.

**Schimbat** (`WorkflowExecutor.Actions.ScrapeTable.vb`): înainte de fiecare clic se verifică din nou dacă butonul a dispărut (= suntem pe ultima pagină, se iese cu succes); clicul are timp propriu (5 s × multiplicatorul din Setări), iar un timp depășit nu mai oprește fluxul; după clic se așteaptă până la 10 s (× multiplicatorul) ca butonul să dispară. KBot.Forexe 1.0.23.0 → 1.0.24.0. Build curat, 0 avertismente; nerulat pe FOREXE.

Nerezolvat aici: de ce durează răspunsul Ajax al listei peste 5 s (listă lungă, rețea sau FOREXE lent) — nevăzut.

## Corrective 2 (20.11.2026, 10:43) — citirea de la sfârșit s-a oprit pe loc: ordinea din FOREXE e cea mai nouă PRIMA

Jurnalul operatorului: «Navighez la ultima pagina» → «exitIfCellEquals declanșat la rândul 0» → «Total 0 rânduri»: citirea s-a oprit pe primul rând citit (cel de jos al ultimei pagini) și a spus «nimic nou».

**Cauza (deducție din jurnal + cererea inițială, nevăzută în pagină):** am înțeles greșit «listate în ordinea adăugării». Lista din FOREXE are angajamentul cel mai nou PRIMUL; ultima pagină îl are pe cel mai vechi, care e cunoscut, deci rândul de ieșire s-a potrivit imediat. Tocmai de aceea citirea înainte trecea prin atâtea pagini de angajamente vechi — ce voia operatorul era oprirea la primul cunoscut, de sus în jos.

**Schimbat:** `.wfl` V.6 — fără `startFromLast` / `a.last` / `a.prev`; citire înainte de la pagina 1 cu `a[rel='next']`, oprire la `exitIfCellEquals` (primul cod cunoscut), `partialOnError` păstrat. `ForexeController`: ultimul cunoscut ținut minte = PRIMUL rând al listei citite (cel mai nou). Jurnalul ScrapeTable arată acum și valoarea care a oprit citirea și câte rânduri au fost păstrate până atunci (`… (Cod = «…»); rânduri păstrate înainte de el: N`). Ajutorul (`lista.md`, turul) și jurnalul operatorului corectate. Corectivul pentru `a.last` rămâne în cod (alte fluxuri cu citire de la sfârșit), dar nu mai e folosit de acest flux.

**Nevăzut / de verificat:** că primul rând de pe pagina 1 e într-adevăr cel mai nou (de confirmat în pagină și în rândul din jurnal «Cod = …»); fără ultim cunoscut ținut minte, prima rulare se oprește la primul cod din arbore, iar o eroare FOREXE la o pagină târzie nu mai e probabilă (se citesc 1–2 pagini).
