# Setări K-BOT — ghidul operatorului

Acest fișier descrie setările pe care le poate schimba operatorul din interfața K-BOT: unde apar,
unde se păstrează, ce fac de fapt și cum sunt legate în cod. E scris atât pentru cine le folosește,
cât și pentru cine le va întreține data viitoare.

Toate setările descrise aici se păstrează în **`<AppDir>\kbot_paths.json`** — fișierul JSON de lângă
executabil (la o instalare standard, `C:\KBOT\kbot_paths.json`). Fișierul poate lipsi: atunci se
folosesc valorile implicite. Un fișier stricat (JSON nevalid) **nu oprește pornirea** — se încarcă
tot valorile implicite, iar eroarea se scrie în `<AppDir>\Logs\harness_errors.log`.

Exemplu de fișier complet:

```json
{
  "DdfPdfRoot": "C:\\AVACONT\\FOREXE\\PDF\\DDF\\",
  "AdobeViewerMode": "Auto",
  "AdobeNewInstance": "Auto"
}
```

---

## 1. Mod vizualizator Adobe

### 1.1 Unde apare

Vederea **DDF** → fila **«Document»** → banda de sus, eticheta **«Mod vizualizator Adobe:»**.
Este o listă derulantă cu trei opțiuni: **Automat**, **Modern**, **Clasic**.

### 1.2 Cheia stocată

| | |
|---|---|
| Cheie | `AdobeViewerMode` |
| Fișier | `<AppDir>\kbot_paths.json` |
| Tip | text (șir JSON) |
| Valori acceptate | `Auto` (sau `Automat`), `Modern`, `Classic` (sau `Clasic`) — fără diferență între majuscule și minuscule |

### 1.3 Valoarea implicită și ce se întâmplă la valori greșite

Implicit: **`Auto`**.

* cheie **lipsă** sau șir **gol** → `Auto`, fără avertisment (lipsa e starea normală, nu o greșeală);
* valoare **nerecunoscută** (de exemplu `turbo`) → `Auto`, **plus** o linie de avertisment în
  `<AppDir>\Logs\adobe_preview.log` care numește cheia și valoarea găsită.

Nu există caz în care o setare greșită să împiedice deschiderea unui document.

### 1.4 Ce face, pe înțelesul tuturor

K-BOT nu redesenează PDF-ul singur: **pornește Adobe și îi mută fereastra în interiorul panoului
K-BOT**. Adobe are însă două interfețe foarte diferite, iar ce trebuie ascuns din ele diferă complet.
Setarea alege *rețeta* de găzduire.

**Automat** (recomandat) — după ce fereastra e încorporată, K-BOT se uită în arborele de ferestre al
Adobe și decide singur:

| ce găsește în arbore | concluzie |
|---|---|
| `AVTaskPaneHostView` | interfață **clasică** |
| `AV2DocumentTabView` sau `AV2DockableTabStripView` | interfață **modernă** |
| niciunul | nerecunoscut → se folosește profilul **clasic**, iar sub previzualizare apare o notă discretă |

Decizia și dovada ei se scriu de fiecare dată în jurnal, în forma
`Mod detectat: Modern (AV2DockableTabStripView prezent)`.

**Modern** / **Clasic** — forțează rețeta, indiferent ce arată arborele. Detecția rulează oricum, iar
dacă arborele contrazice alegerea forțată, jurnalul o spune explicit. Acea linie este ce va explica
o previzualizare stricată după un update Adobe.

#### Ce conține fiecare profil

Valorile de mai jos sunt **măsurate**, nu alese: provin din două stări salvate din bancul de probă la
**04.08.2026, orele 20:06 și 20:10**, pe Acrobat **26.1.21771.0**.

**Profilul MODERN** (Adobe cu interfața nouă, cu file):

| element | valoare | ce scoate de pe ecran |
|---|---|---|
| instanță nouă (`/n`) | nu | — (vezi avertismentul din §2.6) |
| fără splash (`/s`) | nu | — |
| parametri de deschidere (`/A`) | **niciunul** | pe această interfață comutatoarele `/A` nu au niciun efect, deci nu se trimit |
| decupare dreapta | 230 px | banda din dreapta (panoul de instrumente) iese în afara zonei vizibile |
| decupare sus | 152 px | bara de sus (file + unelte) iese în afara zonei vizibile |
| deplasare `dx` | −130 px | trage fereastra spre stânga, ca marginea din stânga să iasă din zona vizibilă |
| `dy` / `dw` / `dh` | 0 | — |

**Profilul CLASIC** (Adobe cu interfața veche):

| element | valoare | ce scoate de pe ecran |
|---|---|---|
| instanță nouă (`/n`) | da | Adobe pornește un proces separat, al nostru |
| fără splash (`/s`) | da | nu apare ecranul de întâmpinare |
| parametri de deschidere (`/A`) | `toolbar=0&navpanes=0` | ascunde bara de unelte și panourile de navigare |
| decupare | dezactivată | — |
| deplasare | niciuna | — |

Pe **ambele** profiluri se ascunde, în plus, fereastra plutitoare a Adobe (clasa `AVL_AVPopup`) —
insigna care apare peste document. Ea nu poate fi exprimată într-o stare salvată din banc, fiindcă
este o fereastră de nivel superior, nu un copil al ferestrei găzduite; de aceea o ascunde un
supraveghetor care mătură ecranul la fiecare 500 ms cât timp documentul e afișat.

**Ce NU face K-BOT:** nu scrie nimic în registry. Bancul de probă scrie valoarea `bEnableAv2` ca să
*forțeze* o interfață sau alta; aplicația nu face asta, fiindcă acea valoare ar schimba Adobe-ul
dumneavoastră pentru **orice** PDF ați deschide, inclusiv în afara K-BOT. Aplicația se adaptează la
ce găsește.

### 1.5 Cum e legată în cod

| pas | fișier |
|---|---|
| citirea/scrierea textului din JSON | `src\KBot.Common\KBotPaths.vb` |
| textul → valoare, cu căderea pe «Auto» + avertisment | `src\KBot.Controls\Adobe\AdobeViewerSettings.vb` |
| cele două profiluri, cu numerele măsurate | `src\KBot.Controls\Adobe\AdobeViewerProfile.vb` |
| detecția generației din arborele de ferestre | `src\KBot.Controls\Adobe\AdobeUiDetector.vb` |
| parcurgerea arborelui de ferestre | `src\KBot.Controls\Adobe\AdobeWindowProbe.vb` |
| aritmetica decupare + deplasare | `src\KBot.Controls\Adobe\AdobeHostGeometry.vb` |
| pornirea Adobe, găsirea și reparentarea ferestrei | `src\KBot.Controls\Adobe\AdobeWindowHosting.vb` |
| orchestrarea (profil → fereastră găzduită) | `src\KBot.Controls\Adobe\AdobeReaderHost.vb` |
| ascunderea ferestrei plutitoare | `src\KBot.Controls\Adobe\AdobePopupWatcher.vb` + `AdobePopupFilter.vb` |
| suprafața din vedere (stări, temă, buton de generare) | `src\KBot.App\Views\Ddf\ReaderHostPreview.vb` |
| combo-ul din interfață | `src\KBot.App\Views\DdfView.vb` + `DdfView.Designer.vb` |
| jurnalul de lucru | `src\KBot.Controls\Adobe\AdobeHostLog.vb` |

Apelurile Windows care rezultă, în ordinea în care se petrec (toate în
`src\KBot.Controls\Adobe\AdobeNativeMethods.vb`):

1. `Process.Start` pe executabilul Adobe, cu argumentele profilului;
2. `EnumWindows` + `GetClassName` + `GetWindowText` — se caută fereastra Adobe al cărei titlu conține
   numele fișierului, cu un timp de așteptare de 8 secunde;
3. `GetWindowLongPtr` / `SetWindowLongPtr` — se curăță stilurile `WS_CAPTION`, `WS_THICKFRAME`,
   `WS_POPUP`, `WS_SYSMENU`, `WS_MINIMIZEBOX`, `WS_MAXIMIZEBOX` și se adaugă `WS_CHILD`;
4. `SetParent` — fereastra devine copil al panoului K-BOT;
5. `GetWindow` (`GW_CHILD` / `GW_HWNDNEXT`) + `GetWindowRect` + `MapWindowPoints` — se parcurge
   arborele de ferestre pentru detecție;
6. `MoveWindow` + `SetWindowPos` + `RedrawWindow` — se așază fereastra la dreptunghiul calculat și e
   forțată să se redeseneze (fără acest ultim pas rămâne **nevăzută**);
7. `ShowWindow(SW_HIDE)` — doar pe ferestrele plutitoare acceptate de filtru;
8. la închidere: se pun la loc stilul și părintele original (`SetWindowLongPtr` + `SetParent`).

---

## 2. Instanță nouă Adobe

### 2.1 Unde apare

Aceeași bandă, lângă setarea anterioară: **«Instanță nouă Adobe:»**, cu opțiunile **Automat**,
**Da**, **Nu**.

### 2.2 Cheia stocată

| | |
|---|---|
| Cheie | `AdobeNewInstance` |
| Fișier | `<AppDir>\kbot_paths.json` |
| Tip | text (șir JSON) |
| Valori acceptate | `Auto` / `Automat`, `Da` / `yes` / `true` / `1`, `Nu` / `no` / `false` / `0` |

### 2.3 Valoarea implicită și ce se întâmplă la valori greșite

Implicit: **`Auto`** — adică se folosește ce spune profilul ales la §1 (modern: fără instanță nouă;
clasic: cu instanță nouă). Valoare lipsă sau goală → `Auto` tăcut; valoare nerecunoscută → `Auto` cu
avertisment în jurnal.

### 2.4 Ce face

Controlează comutatorul `/n` cu care se pornește Adobe.

* **Da** — Adobe pornește un **proces nou**, al K-BOT. Fereastra găzduită ne aparține sigur.
* **Nu** — Adobe poate **preda documentul unei instanțe deja deschise** de dumneavoastră.
* **Automat** — decide profilul.

### 2.5 Cum e legată în cod

`AdobeViewerSettings.ParseNewInstance` citește valoarea; `AdobeViewerProfile.WithNewInstance` o
suprapune peste profil; `AdobeWindowHosting.BuildArguments` construiește linia de comandă
(`[/n] [/s] [/A "…"] "fișier.pdf"` — parametrii `/A` trebuie **înaintea** fișierului, altfel Adobe îi
ignoră).

### 2.6 De ce contează (avertisment)

Profilul **modern** are, așa cum a fost măsurat, `/n` **oprit**. În funcționare reală asta înseamnă
că Adobe poate da documentul unei instanțe pe care ați deschis-o dumneavoastră, iar K-BOT ajunge să
mute în panoul lui o fereastră pe care nu a creat-o.

Ce face K-BOT în acest caz:

* scrie **explicit** în jurnal că fereastra încorporată aparține altui proces decât cel pornit de el;
* **nu închide** acel proces la schimbarea documentului (altfel ar închide și lucrul dumneavoastră).

Dacă vă deranjează comportamentul, puneți setarea pe **Da**.

---

## 3. Rădăcina PDF-urilor DDF

| | |
|---|---|
| Unde apare în interfață | nicăieri deocamdată — se editează direct în fișier |
| Cheie | `DdfPdfRoot` |
| Tip | text (cale de folder) |
| Implicit | `C:\AVACONT\FOREXE\PDF\DDF\` |
| La valoare lipsă/goală/stricată | se folosește implicitul |

Folderul în care K-BOT caută (recursiv) PDF-urile DDF ale unui angajament și în care le scrie pe cele
generate, sub subfolderul partenerului sau sub `GENERAL`. Citit de `KBotPaths.Current.DdfPdfRoot` și
folosit de `src\KBot.App\Views\Ddf\DdfPdfLocator.vb` și `DdfFileBrowser.vb`. Dacă folderul nu există,
lista de fișiere rămâne goală și **numește calea configurată** în mesaj — nu se creează singur.

---

## 4. Unde se uită cineva când ceva nu merge

| fișier | ce conține |
|---|---|
| `<AppDir>\Logs\adobe_preview.log` | tot ce a decis gazda Adobe: profilul folosit, marcajul care a decis generația, dreptunghiul cerut față de cel obținut, fiecare fereastră plutitoare acceptată sau respinsă și motivul |
| `<AppDir>\Logs\harness_errors.log` | excepțiile propriu-zise, cu stivă |
| `<AppDir>\Logs\test_adobe_rhp.log` | jurnalul bancului de probă (doar când se rulează bancul) |

---

## 5. Limitări cunoscute și dependența de versiune

* **Geometria a fost măsurată pe Acrobat 26.1.21771.0.** Numerele (230 / 152 / −130) sunt valabile
  pentru acea versiune și pentru interfața ei. **Un update Adobe le poate invalida** — simptomul
  tipic este o bandă de unelte reapărută sau o fâșie goală pe margine.
* **Locul în care se măsoară valori noi este bancul de probă**, nu codul aplicației:
  `AdobeReaderHarnessForm` (DevHarness → categoria Adobe). Acolo se deschide un PDF, se ajustează
  decuparea și poziția până arată corect, apoi se salvează starea ca fișier JSON. Numerele din acel
  fișier se trec în `AdobeViewerProfiles` (`src\KBot.Controls\Adobe\AdobeViewerProfile.vb`).
  Valorile sunt fixate de teste, deci o modificare greșită pică la `dotnet test`.
* **Marcajele de detecție sunt și ele dependente de versiune.** Dacă Adobe redenumește
  `AVTaskPaneHostView` sau `AV2DockableTabStripView`, detecția va spune «nerecunoscut», va cădea pe
  profilul clasic și va scrie arborele complet de ferestre în jurnal — exact ce trebuie ca marcajele
  să fie actualizate.
* **Mărirea/micșorarea paginii.** Fereastra găzduită este, prin construcție, mai mare decât panoul,
  deci o potrivire „pe lățime" din Adobe se raportează la lățimea mărită, nu la cea vizibilă.
* **Tastatura și focusul** peste granița de proces nu se comportă ca la un control nativ. E o
  limitare cunoscută a mecanismului, nu o defecțiune de configurare.
* **Nu semnați un document cât timp fila «Document» ține o fereastră Adobe găzduită** — semnarea
  pornește Adobe într-un alt mod, peste același proces.

---

## 6. Fereastra «Setări» (felia 0072)

Din 19.09.2026 setările de mai sus, plus cele noi, se schimbă din **fereastra «Setări»**, deschisă din
meniul butonului de opțiuni al ferestrei principale (rândul «Setări…»). Fereastra are aceeași formă
ca aplicația — navigație în stânga, pagina în dreapta — și **salvează fiecare setare în clipa în care
o schimbi**; «Închide» doar închide. Paginile:

| pagina | ce conține | unde se scrie |
|---|---|---|
| **Informații** | operatorul, unitatea, rolul, baza; tipul instalării (datele comerciale — în lucru); versiunea + «Caută actualizări»; **schimbarea parolei** | server (parola) |
| **Aplicație** | comutatoarele globale, cum se deschid PDF / Word / Excel (setările ferestrei găzduite Adobe stau într-un dialog propriu, deschis la alegerea motorului), folderele | `app_settings.json`, `kbot_paths.json`, `settings.json` |
| **FOREXE** | starea robotului, certificatul memorat (+ «Uită certificatul»), bara browserului andocat, instrumentele pentru dezvoltatori în pagină, folderele robotului (doar citire) | `app_settings.json`, `last_certificate.cer` |
| **Pagina FOREXE** | regulile CSS pe care K-BOT le scrie în pagina FOREXE (vezi §7): lista în stânga, rândul ales se editează în dreapta («Ce face», «Selector», «Pagina», «Stil»); «Regulă nouă…» deschide arborele paginii deschise; «Implicite» pune la loc regulile K-BOT; **«Salvează și aplică»** — singurul buton care scrie | `app_settings.json` |
| **Temă** | schema (cele 23 de culori + stil), scalarea (cursorul de mărime se lipește la 100 / 110 / 125 %) și comutatorul «Font din temă» — meniul butonului de temă a rămas doar cu cursorul și schemele | `…\AVACONT\Themes\*.json`, `theme.json` |
| **Autentificare** | ce ține minte fereastra de login (+ «Uită datele memorate»); adresa serverului (doar citire) | `app_settings.json`, `last_login.json` |
| **Jurnal** | vizualizatorul de jurnale (fostul `LogViewerForm`), ca pagină: cele mai noi intrări sus, mesajul întreg în panoul de jos; «Arată jurnal» din meniul shell-ului deschide fereastra pe pagina asta | `Logs*.log`, `/api/logs/*` |

### 6.1 `app_settings.json` — comutatoarele operatorului

| | |
|---|---|
| Fișier | `%APPDATA%\AVACONT\KBot\app_settings.json` (per utilizator Windows) |
| Lipsă / gol | valorile implicite (= comportamentul de dinainte de felia 0072) |
| Stricat | valorile implicite + o linie în `harness_errors.log` |

| cheie | implicit | ce face |
|---|---|---|
| `VerboseLogging` | *(lipsă)* = după build: pornit pe Debug, oprit pe Release | consola FOREXE arată tot (pași, așteptări, andocare, stive), nu doar `<Log>` și erorile |
| `LogViewerEnabled` | `true` | rândul «Arată jurnal» în meniul de opțiuni; debifat, butonul de opțiuni deschide direct fereastra «Setări» (meniul ar fi avut un singur rând) |
| `ShowBrowserButton` | `true` | butonul «Arată browserul» în banda FOREXE |
| `ForexeHideBrowserChrome` | `true` | în vizualizator, bara browserului rămâne în afara panoului (se aplică lucrării următoare) |
| `ForexeDevToolsAllowed` | `false` | pagina FOREXE lasă F12, Ctrl+Shift+I/J/C, Ctrl+U și clicul dreapta să ajungă la Chromium; debifat, meniul K-BOT din pagină le înghite |
| `ForexePageStyles` | cele 5 reguli K-BOT (§7.1) | lista de reguli CSS `{enabled, selector, css, note, page}` scrisă în fiecare pagină FOREXE; lipsă = regulile implicite, listă goală = nicio regulă |
| `ReceptiiCheckedOnOpen` | `true` | selectorul de recepții pornește cu tot bifat |
| `AdobeDetachMode` | `KillProcess` | cum se eliberează fereastra Adobe la schimbarea documentului: `KillProcess` (A) sau `CloseWindow` (B) |
| `AdobePopupWatch` | `true` | ascunde fereastra plutitoare a Adobe cât timp documentul e afișat |
| `ExcelRibbon` | `HideDockWindow` | cum se ascunde panglica Excel în previzualizare: `HideDockWindow` (fereastra, ca la Word) sau `Excel4Macro` |
| `RememberLastLogin` | `true` | fereastra de login completează singură ultimul e-mail |
| `RememberLastUnit` | `true` | …și preselectează unitatea aleasă ultima dată |

O valoare nerecunoscută la `AdobeDetachMode` / `ExcelRibbon` cade pe implicit cu un avertisment în
jurnalul gazdei (`adobe_preview.log`, respectiv jurnalul Office). Cod: `src\KBot.Common\AppSettings.vb`,
`src\KBot.Controls\Adobe\AdobeHostSettings.vb`, `src\KBot.Controls\Office\OfficeHostSettings.vb`.

### 6.2 Schimbarea parolei — doi factori

1. Operatorul scrie **parola actuală** și apasă «Trimite codul pe e-mail». Serverul o verifică (un login
   MariaDB ca operatorul, exact ca la autentificare) și trimite un **cod de 6 cifre** pe adresa
   operatorului — numele de utilizator ESTE e-mailul. Codul e valabil 10 minute, o singură dată, cel mult
   5 încercări greșite.
2. Operatorul scrie codul, parola nouă (minimum 8 caractere, diferită de cea actuală) și confirmarea, apoi
   «Schimbă parola». Serverul schimbă parola pe serverul K-BOT (`SET PASSWORD` pe conexiunea
   operatorului — fără niciun privilegiu al contului de serviciu) și, **cu bună-credință**, pe serverul
   vechi (Access). Dacă serverul vechi nu a putut fi actualizat, fereastra o spune: acolo rămâne parola
   veche până o aliniază un administrator.

Nimic despre parole nu rămâne pe calculatorul operatorului. Pe server, trimiterea e-mailului cere
`SMTP_HOST` / `SMTP_PORT` / `SMTP_USER` / `SMTP_PASSWORD` / `SMTP_FROM` în `config.py` (sau ca variabile
de mediu cu aceleași nume); fără `SMTP_HOST`, butonul răspunde «Trimiterea e-mailului nu este
configurată pe server». Rute: `POST /api/auth/password/code`, `POST /api/auth/password/change`
(`PYTHON\routes\auth\auth.py`, `mailer.py`).

## 7. Pagina FOREXE din browserul andocat (21.09.2026)

Tot ce urmează face scriptul K-BOT din pagină (`src\KBot.Forexe\Services\JavaScripts\ForexeWatch.js`,
instalat la andocare, în ORICE document pe care Wicket îl încarcă — `contract?6`, `contract_edit_rand?7`,
`receptie_edit?7`, `wicket/page?9`…). Setările îi ajung prin `ForexeWatchConfig` (JSON
`{devTools, darkMode, rules}`), la fiecare andocare și în clipa în care se apasă «Salvează și aplică» sau
se schimbă tema; pagina le ține în `localStorage`, ca următoarea încărcare să pornească gata stilizată,
înainte de prima afișare (cum anume — §7.4).

### 7.1 Regulile CSS (pagina «Pagina FOREXE»)

Fiecare regulă = un selector + declarații `proprietate: valoare;`. Se scriu într-un `<style>` la începutul
documentului, fiecare declarație cu `!important`, deci bat stilurile proprii ale paginii și nu sunt atinse
de reîmprospătările Ajax ale lui Wicket. Cele cinci reguli implicite:

| ce face | selector | stil |
|---|---|---|
| meniul lateral ascuns cât e deschis un angajament | `body:has(.well.well-small h4 span:nth-child(2)) [class*='col-lg-2']:has(.bs-sidebar)` | `visibility: hidden` |
| conținutul pe toată lățimea, în același caz | `body:has(.well.well-small h4 span:nth-child(2)) [class*='col-lg-2']:has(.bs-sidebar) + [class*='col-lg-10']` | `width: 100%` |
| pagina folosește 90 % din lățime | `#main.container` | `max-width: 90%` |
| fără mărirea textului a FOREXE-ului | `body` | `font-size: 100%` |
| butonul «Înapoi» din bara de file ascuns, DOAR pe `…/CABWeb/contract` | `span.nav.nav-tabs [class*='col-lg-10'] button.btn.btn-default` | `display: none` |

**«Pagina»** (a cincea regulă o folosește): gol = regula ține pe orice pagină; altfel doar cât timp adresa
documentului, FĂRĂ `?…`, este cea scrisă — întreagă (`https://forexe.mfinante.gov.ro/CABWeb/contract`),
doar calea (`/CABWeb/contract`) sau doar ultimul ei cuvânt (`contract`). Scriptul recalculează foaia la fiecare
navigare, la fiecare apel Ajax Wicket și la bătaia de 2 s, deci regula vine și pleacă odată cu adresa.
Regulile implicite intră în listă numai la prima pornire sau la «Implicite» (care ÎNLOCUIEȘTE lista) — o
regulă implicită nouă nu se adaugă singură peste o listă deja salvată.

Condiția `body:has(…)` ține meniul lateral la vedere pe listă / acasă — paginile de pe care pornesc
fluxurile robotului. **Regulile rămân pornite și cât rulează robotul**: un pas `Click` a cărui țintă e
ascunsă de ele (linkul «Listă angajamente» din meniul lateral) le ridică pentru acel singur clic.

Selectorii nu folosesc niciodată id-urile Wicket (`id98`…): se schimbă la fiecare afișare. Fereastra
«Regulă nouă…» citește elementele paginii deschise (tag, id static, `name`, clase, textul, stilul
inline) și le arată ca arbore; un clic pe un element îi pune selectorul propus și stilul de acum în
câmpuri și **încadrează elementul în pagina FOREXE** (chenar portocaliu, derulat la vedere); chenarul
piere la închiderea ferestrei.

### 7.2 Cât lucrează robotul

- fereastra browserului e **închisă pentru operator** (`EnableWindow`): clicul și tastele nu ajung în
  pagină; robotul, care vorbește prin protocolul de depanare, nu e afectat;
- pagina e **încețoșată** și un cartonaș în mijloc spune «K-BOT lucrează în FOREXE: <numele lucrării>»
  + «Vă rugăm așteptați»; o navigare din mijlocul lucrării vine deja încețoșată, iar o re-randare Ajax
  Wicket nu o ridică (§7.4);
- meniul K-BOT din pagină e ascuns, urmărirea operațiunilor e suspendată.

Totul se ridică singur la sfârșitul lucrării.

### 7.3 Ce mai face scriptul

- **Salvare fără modificări**: la formularele de rezervare (`input[name^='tableContainer:']`) și de
  recepție (`form.form-horizontal input[name$=':valoare']`) valorile se fotografiază când apare
  formularul; un clic pe butonul de salvare — sau Enter într-un câmp — cu aceleași valori e oprit și un
  mesaj blocant cere «NU salvați! Apăsați «Renunță»». Butoanele din `.modal-dialog` («Da» / «Nu» din
  întrebarea de renunțare) trec întotdeauna. Mesajul se închide cu «Am înțeles», Esc sau Enter.
- **«Renunță»** apăsat oriunde încheie operațiunea urmărită fără descărcare.
- **Rezervare modificată** → se descarcă doar rezervările; **recepție** → doar recepțiile.
- **Istoric**: după citirea istoricului, fluxurile apasă «Înapoi».
- **Instrumentele pentru dezvoltatori** stau închise dacă `ForexeDevToolsAllowed` e `false`.
- **Mod întunecat**: cu o schemă K-BOT întunecată pagina e inversată (`html { filter: invert(1)
  hue-rotate(180deg) }`, imaginile și elementele K-BOT inversate înapoi); urmează tema pe loc.
- Meniul K-BOT plutitor se **strânge / desface** din butonul ▾/▸ din rândul titlului (ținut minte).
- Fereastra andocată e **verificată** după fiecare andocare / redimensionare (imediat și încă de 5 ori
  în 1,6 s) și pusă la loc dacă s-a mutat: bara de adrese nu trebuie să se vadă.

### 7.4 Când intră stilurile și cum rămân (21.09.2026, felia 0073-01)

Tot ce scrie scriptul în pagină stă în două elemente `<style>` (regulile + modul întunecat într-unul,
încețoșarea în celălalt) și într-o clasă pe `<html>` (`kbot-busy`, cât lucrează robotul). Semnalate de
operator: încețoșarea, culorile întunecate și bara de meniu ascunsă «reveneau abia după un moment».
Cauza: pe un document nou scriptul rulează ÎNAINTE ca browserul să fi construit `<html>` — nu avea de ce
să agațe foile, instalarea de la început eșua în tăcere și bătaia de 2 s era prima care le punea.
(Clicul «Istoric» din jurnal se termina cu o redirecționare Wicket, deci un document nou, nu doar Ajax.)

Acum:

- foile intră **în clipa în care apare `<html>`** (un observator pe document), cu mult înainte de prima
  afișare; la `DOMContentLoaded` se verifică din nou, și dacă ar fi intrat abia atunci consola K-BOT
  spune o singură dată «stilurile paginii au intrat abia la încărcarea completă» — mesaj care NU
  trebuie să apară;
- după **fiecare apel Ajax Wicket** (`/ajax/call/complete`, `/dom/node/added`; pe Wicket vechi
  `registerPostCallHandler`) și la **orice schimbare pe `<html>` (clasa) sau în `<head>` (copiii)** foile,
  clasa, vălul și starea ascunsă a meniului K-BOT se repun pe loc, nu la următoarea bătaie; tot atunci se
  recalculează foaia regulilor, ca regulile legate de o pagină (§7.1, «Pagina») să vină și să plece cu
  adresa;
- bătaia de 2 s rămâne doar ca ultimă plasă.

Nimic pe partea .NET nu s-a schimbat pentru asta.
