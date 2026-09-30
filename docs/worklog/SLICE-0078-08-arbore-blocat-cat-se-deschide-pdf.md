# SLICE 0078-08 — Arborele blocat cât Adobe încă deschide documentul

Data: 30.09.2026. Cererea operatorului: în orice arbore care, la clic, deschide un PDF, un «debounce»
— de preferat NU un cronometru, ci un semnal că Adobe a terminat de procesat. Cât e activ, clicul pe
alt rând nu e permis: un al doilea document trimis la Adobe cât primul încă se încarcă dă erori
Adobe aleatorii și lasă procese Adobe fantomă. Număr: **0078-08** (propus).

## 1. Ce s-a schimbat

### 1.1 Semnalul «Adobe a terminat» (`AdobeReaderHost.DocumentReady`)

Ridicat O DATĂ pe document găzduit, când toate sunt adevărate:
- pagina e așezată (`AVPageView` cu marcaj propriu de vizibil și mărime — merge și cu panoul ascuns,
  când operatorul e pe altă pagină);
- nu rulează o rafală de mesaje de script (`AdobeSaveTrap.InScriptBurst`);
- Ctrl+H / Ctrl+2 au plecat sau s-a renunțat la ele (doar cât panoul e pe ecran — ascuns, tastele
  așteaptă panoul, documentul e deja gata);
- **nicio fereastră a procesului Adobe nu s-a mai arătat / ascuns / mișcat de 250 ms**.

Ultimul punct e semnalul venit de la Adobe: cârligul WinEvent din `AdobeWindowWatcher` (fost
`AdobeSizeWatcher`, 0078-07, redenumit; acum ascultă `EVENT_OBJECT_SHOW` … `EVENT_OBJECT_LOCATIONCHANGE`
pe tot procesul, doar ferestre întregi) ridică `Quiet` când o rafală de evenimente s-a încheiat.
Singurul cronometru e cel care strânge rafala (250 ms fără evenimente), nu o așteptare ghicită.
«Poke» după găzduire (Adobe poate fi deja gata — atunci n-ar mai veni niciun eveniment), după
trimiterea tastelor și după o rafală de script.

**Plasă de siguranță:** dacă Adobe nu termină în 60 s (un mesaj de eroare, fereastra închisă),
`DocumentReady` pleacă oricum, cu linia «Document gata în Adobe (ATENȚIE: Adobe nu a terminat în
60 s…)». Fără ea, un document care nu se termină ar ține arborii blocați. În jurnal, calea normală:
«Document gata în Adobe (pagina așezată, ferestrele Adobe nu s-au mai schimbat de 250 ms).»

### 1.2 Poarta comună (`AdobeOpenGate`, KBot.Controls/Adobe)

Una pe aplicație: clicul care trimite un document nou poate veni din mai mulți arbori. Proprietarii:
- `ReaderHostPreview` (paginile «Document» DDF / ORD / Note CAB, recepția, bancurile): intră la
  trimiterea documentului spre Adobe; iese la `DocumentReady`, la eșecul găzduirii, la fișier lipsă,
  la `Clear`, la eliberare; pe motorul ActiveX iese când încărcarea (care așteaptă așezarea) se întoarce;
- `DdfFisierPreview` (pagina «Fișiere»): steagul lui «ocupat» (care deja bloca lista) ține acum până
  la `DocumentReady` pentru PDF-uri și intră și în poartă.

### 1.3 Arborii

`AdvancedTreeControl.SelectionLocked` (NOU, doar la rulare, nu se scrie în designer): apăsarea pe un
rând, dublu-clicul și tastele de navigare sunt ignorate; eliberarea unei apăsări refuzate e ignorată
și ea; cursorul = `AppStarting`. Derularea, lupa, capul și subsolul merg. Legați de poartă
(`AdobeOpenGate.LockWhileOpening`) după `InitializeComponent`: arborele principal (`KbotForm`),
`DdfView`, `OrdView`, `NoteCabView`.

### 1.4 Ajutorul

`contabil.liste` — secțiune nouă «Cât se deschide un document» (eticheta `0078-08`), trecută ca
**0000-16**.

## 2. Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.Controls/Adobe/AdobeWindowWatcher.vb` | **NOU** (înlocuiește `AdobeSizeWatcher.vb`, șters) |
| `src/KBot.Controls/Adobe/AdobeOpenGate.vb` | **NOU** |
| `src/KBot.Controls/Adobe/AdobeReaderHost.vb` | `DocumentReady`, `IsDocumentReady`, `OnAdobeQuiet`, termenul de 60 s, `FindPageView` |
| `src/KBot.Controls/Tree/AdvancedTreeControl.Lock.vb` | **NOU** — `SelectionLocked` |
| `src/KBot.Controls/Tree/AdvancedTreeControl.Overrides.vb`, `.Keyboard.vb` | refuzul clicurilor / tastelor |
| `src/KBot.App/Views/Ddf/ReaderHostPreview.vb` | intrare / ieșire din poartă |
| `src/KBot.App/Views/Ddf/DdfFisierPreview.vb` | «ocupat» până la `DocumentReady` + poarta |
| `src/KBot.App/KbotForm.vb`, `Views/DdfView.vb`, `Views/OrdView.vb`, `Views/NoteCabView.vb` | `LockWhileOpening(tree)` |
| `src/KBot.App/HelpContent/contabil/liste.md` | secțiunea nouă (0000-16) |

FileVersion: Controls 1.56 ▸ **1.57**. App: de crescut la publicare (`push-update.ps1`).

## 3. Rezultatele testelor

`dotnet build src\KBot.App\KBot.App.vbproj`: **0 erori, 0 avertismente**. `Check-Help.ps1 -Coverage`:
«No errors.» (acoperirea arată `RobotQueueForm` — din 0098, deja în lista «Ajutor de actualizat»).
Nimic rulat.

## 4. Rămas NEVERIFICAT

1. Nerulat. De văzut în `adobe_preview.log` linia «Document gata în Adobe (…)» și cât durează de la
   «Pornesc Adobe»; dacă apare des varianta de 60 s, semnalul nu se prinde pe acel PC.
2. **250 ms de liniște** e o presupunere: dacă Adobe face o pauză mai lungă la mijlocul încărcării
   (formulare XFA mari, scripturi), arborele se poate debloca puțin prea devreme.
3. Arborele principal se blochează și el; o schimbare de angajament în timpul încărcării nu mai e
   posibilă — voit (tot ea detașa documentul pe jumătate încărcat).
4. Motorul ActiveX: se deblochează când se întoarce încărcarea, nu după aceeași liniște.
5. Procesele fantomă: poarta oprește cauza cerută (al doilea document în timpul încărcării); dacă
   mai apar, de trimis `adobe_preview.log` de pe PC-ul respectiv.
