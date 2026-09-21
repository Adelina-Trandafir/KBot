# SLICE 0073-01 — Foile de stil ale paginii FOREXE intră înainte de prima afișare și rezistă re-randărilor Wicket

**Data:** 21.09.2026
**Fișier:** `src/KBot.Forexe/Services/JavaScripts/ForexeWatch.js` (singurul atins)

## Cererea operatorului

1. Blur-ul (vălul din timpul unei lucrări a robotului) se pierde «la fiecare refresh AJAX al paginii».
2. La fel culorile întunecate (modul dark trimis în pagină).
3. Bara de meniu FOREXE (ascunsă de regulile paginii) dispare «abia după un timp scurt».
4. Dacă meniul nu poate fi ascuns mai repede, blur-ul să fie forțat până dispare meniul.

## Ce s-a găsit

Jurnalul monitorului Wicket dat de operator pentru clicul pe «Istoric» se termină cu:

```
19.169  #animlogo → none
19.176  #statlogo → inline      ← sfârșitul apelului Ajax
19.223  #statlogo → inline      ← DUBLURĂ, în ordinea stat/anim
19.228  #animlogo → none
```

Ultimele două linii sunt exact raportul inițial pe care `WicketMonitor.js` îl face la
`DOMContentLoaded` (`#statlogo` întâi, apoi `#animlogo`) — adică un **document nou**: apelul
Ajax s-a încheiat cu o redirecționare Wicket, o navigare completă, nu doar o re-randare.

Pe un document nou scriptul intră prin `AddInitScriptAsync`, care rulează ÎNAINTE ca parserul
să fi produs `<html>`: în acel moment `document.head` ȘI `document.documentElement` sunt `null`,
deci `(document.head || document.documentElement).appendChild(style)` arunca în `catch (ignored)`
și **nimic nu se instala la start**. `boot()` nu instala foile, iar bătaia de 2 s
(`MENU_KEEPALIVE_MS`) era primul lucru care le punea la loc. Toate cele trei simptome sunt
aceeași foaie (`kbot-watch-style`: regulile + dark) și foaia vălului (`kbot-watch-busy-style`),
care apăreau după până la 2 secunde. Punctul 4 nu mai are obiect: intră toate deodată,
înainte de prima afișare.

Presupunere marcată ca atare: că `documentElement` e `null` în init script-ul Playwright pe
Chromium nu a fost măsurat pe FOREXE (nu s-a atins un browser real); e comportamentul cunoscut
al `Page.addScriptToEvaluateOnNewDocument` și se potrivește cu toate simptomele. Dacă totuși
foile ar intra târziu, scriptul spune acum în consolă, o singură dată pe document:
«stilurile paginii au intrat abia la încărcarea completă».

## Ce s-a schimbat

Secțiunea nouă **11** în antetul scriptului. Concret:

- `sheetParent()` — `<head>`, altfel `<html>`, altfel `null`; `installStyles()` /
  `installBusyStyle()` nu mai creează un `<style>` fără părinte și întorc `true/false`.
  `installStyles` rescrie `textContent` doar când s-a schimbat.
- `whenRootExists(fn)` — rulează imediat dacă `<html>` există, altfel un `MutationObserver`
  pe `document` (`childList`) prinde momentul în care parserul inserează `<html>`, cu mult
  înainte de prima afișare; `DOMContentLoaded` e rezerva. La start: `whenRootExists(earlyInstall)`.
- `resync()` — într-o singură trecere: cele două foi, clasa `kbot-busy` de pe `<html>`, vălul,
  starea ascunsă a meniului; `scheduleResync()` o amână cu `setTimeout(0)` ca o rafală de
  mutații să dea o singură rulare.
- `hookWicket()` — `Wicket.Event.subscribe('/ajax/call/complete' | '/dom/node/added')` (Wicket 6+)
  sau `Wicket.Ajax.registerPostCallHandler` (versiuni vechi); reîncercat din bătaie până
  când scriptul Wicket e în pagină. Versiunea Wicket a FOREXE nu e cunoscută din depozit, de
  aceea ambele.
- `watchRootAndHead()` — observatoare pe `<html>` (`class`, copii) și pe `<head>` (copii): o foaie
  scoasă sau clasa pierdută revine în același tick.
- `syncBusy()` atinge clasa doar când e greșită (altfel observatorul de pe `<html>` s-ar trezi
  singur la nesfârșit) și nu mai aruncă pe `documentElement` null.
- `boot()`: `resync()` + `watchRootAndHead()` întâi, apoi mesajul «au intrat târziu» dacă e cazul.
  Bătaia de 2 s rămâne ultima plasă, prin `resync()`.

Nimic pe partea .NET: `SetWatchSuspendedAsync` / `ApplyWatchConfigAsync` / ridicarea regulilor
pentru un clic al robotului (`liftStyles`) sunt neschimbate.

## Rezultatele testelor

- `node --check ForexeWatch.js`: valid.
- Pagina de probă în browserul încorporat (fișier de scratch, cu un `Wicket.Event` fals,
  `sessionStorage` marcat «suspendat», config cu `darkMode` + o regulă `.navbar{display:none}`):
  - după încărcare: `kbot-busy` pus, ambele foi în `<head>`, `.navbar` `display:none`,
    `<html>` cu `invert(1)`, conținutul cu `blur(7px)`, meniul K-BOT ascuns;
  - foile și clasa scoase de mână + `Wicket.Event.publish('/ajax/call/complete')`: totul la loc
    în < 30 ms;
  - foaia și clasa scoase din nou, FĂRĂ eveniment Wicket: totul la loc în < 30 ms (doar
    observatoarele);
  - `setSuspended(false)`: blur-ul și clasa pleacă, meniul apare, dark + regula rămân.
- Nicio suită `dotnet test` rulată (regula casei). Niciun build necesar: fișierul e resursă
  încorporată, nicio linie VB nu s-a schimbat.

## Neverificat / amânat

- Nu s-a atins un browser Playwright real, deci calea `AddInitScriptAsync` cu `<html>` încă
  neparsat nu a fost văzută pe ecran — de urmărit în consola K-BOT la prima lucrare: mesajul
  «stilurile paginii au intrat abia la încărcarea completă» NU trebuie să apară.
- API-ul Wicket al FOREXE (Event vs. registerPostCallHandler) nu a fost citit din pagină
  (rețeaua era închisă din sandbox); ambele sunt abonate defensiv.
- Codul din arborele de lucru (vălul, dark, «Pagina FOREXE» din Setări, gardurile de salvare)
  este încă necomis și fără felie proprie; această corecție stă deasupra lui în același fișier
  și nu poate fi comisă separat de el.

## Pasul 2 (aceeași zi) — o regulă ține doar pe O pagină: «Pagina»

**Cererea:** butonul «Înapoi» (`button.btn.btn-default` din `[class*='col-lg-10']` din
`span.nav.nav-tabs`) ascuns DOAR pe `https://forexe.mfinante.gov.ro/CABWeb/contract` (fără `?6`),
ca regulă vizibilă și editabilă în «Setări» → «Pagina FOREXE».

- `PageStyleRule.Page` (gol = orice pagină) + constructor cu patru argumente, `Clone`, și a
  cincea regulă implicită: «Butonul «Înapoi» din bara de file ascuns pe pagina angajamentului»,
  selector `span.nav.nav-tabs [class*='col-lg-10'] button.btn.btn-default`, `display: none`,
  pagina `https://forexe.mfinante.gov.ro/CABWeb/contract`.
- `AppSettings`: `PageStyleRuleDto.Page` dus-întors (cheia JSON `page`; lipsă = gol).
- `ForexeWatchConfig`: câmpul `page` în JSON-ul trimis paginii.
- `ForexeWatch.js`: `pageMatches(page)` — adresa documentului fără `?…`/`#…`; o valoare cu
  `://` se compară cu origin + cale, una care începe cu `/` cu calea, altfel cu ultimul segment
  al căii; fără majuscule/minuscule. `cssFromRules` sare regulile care nu țin pe pagina de acum;
  foaia se recalculează la fiecare `resync()` (navigare, eveniment Ajax Wicket, bătaie).
- `RegulaPaginaEditor` (+ Designer): al patrulea câmp «Pagina (gol = pe orice pagină)» între
  «Selector» și «Stil», cu tooltip; `Fill` (elementul ales din arbore) nu atinge «Pagina».
- `SetariPaginaView` (+ Designer): coloana «Pagina» în grilă («(toate)» când e gol).
- `docs/SETARI_UTILIZATOR.md` §7.1: a cincea regulă și paragraful «Pagina».
- `docs/SETARI_UTILIZATOR.md` §7.4 (nou): când intră foile și cum rămân — versiunea pentru operator a
  pasului 1, cu trimiteri din introducerea §7 și din §7.2.

**Build:** `dotnet build src\KBot.App\KBot.App.vbproj --no-incremental`: 0 erori, 0 avertismente.
`node --check` valid. Nicio suită rulată (regula casei).

**De știut:** regulile implicite intră în listă doar la prima pornire sau la «Implicite» (care
înlocuiește lista), deci pe un `app_settings.json` deja salvat regula nouă NU apare singură:
ori «Implicite» + «Salvează și aplică», ori «Regulă nouă…» cu cele patru câmpuri de mai sus.
Selectorul e CSS, deci nu poate cere textul «Înapoi»; dacă în același `span.nav.nav-tabs` mai stă
un `btn-default`, selectorul trebuie strâmtat de mână (neverificat pe pagina reală).
