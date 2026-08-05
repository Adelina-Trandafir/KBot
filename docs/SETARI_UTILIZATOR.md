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
