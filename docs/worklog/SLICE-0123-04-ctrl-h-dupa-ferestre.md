# SLICE-0123-04 - Ctrl+H / Ctrl+2 hotarate de ferestrele Adobe (23.10.2026)

Operator: Ctrl+H este critic (comuta modul de citire; nu trebuie trimis orbeste), Ctrl+2 mai putin; sa nu se apese «indiscriminator», doar cand ferestrele dovedesc ca trebuie. Atasamente (Window Detective, Acrobat DC gazduit): panourile vizibile = fereastra `AVTaskPaneView` lata de 345 px, pagina 1557 px din 1902 px; mod citire = `AVPageView` 1876 px din 1902 px.

## What changed and why
- **`KBot.Controls/Adobe/AdobeReaderHost.vb`** (motorul «fereastra gazduita», cel implicit): `PlanKeys` decide la fiecare tic al timerului ce tasta pleaca.
  - Ctrl+H: pleaca doar daca panourile sunt dovedite vizibile (o fereastra `AVTaskPaneView` / `AVTaskPaneHostView` vizibila si lata, SAU pagina cu peste 100 px mai ingusta decat gazda). Daca sunt ascunse = modul de citire e deja pornit (Adobe l-a pastrat de la documentul anterior): nu se trimite nici Ctrl+H, nici Ctrl+2. Daca nu exista nicio fereastra de panou, se asteapta pana la 3 s (Adobe poate inca isi construieste interfata), apoi se socoteste «deja pornit».
  - Dupa Ctrl+H: daca panourile se vad inca dupa 700 ms, Ctrl+H se mai trimite o data (cel mult 2 in total), apoi se renunta, cu indicatia de apasare manuala; Ctrl+2 nu se mai trimite.
  - Ctrl+2: o singura data, doar dupa ce panourile au disparut si latimea paginii e aceeasi la doua tacte la rand (sau dupa 8 s de la Ctrl+H). Niciodata daca Ctrl+H nu a fost trimis.
  - Focusul tastaturii intra in document abia cand o tasta chiar pleaca (tacturile de asteptare nu mai muta focusul din K-BOT). Ctrl+S dupa semnatura merge ca pana acum (aceeasi coada); tastele trimise ies din coada inainte de `SendInput`; Ctrl apasare+eliberare in acelasi apel.
  - Fiecare decizie intra in `adobe_preview.log` (numele si latimile ferestrelor vazute), cate o data pe schimbare.
- **Scos** `FitWidthOnly` din 0123-03 (`AdobeReaderHost`, `AcroPdfViewer.ReadMode`, `ReaderHostPreview`, pagina ADECHIT). Pagina de rapoarte foloseste acum aceeasi logica ca toate paginile cu PDF.
- Motorul ActiveX (`AcroPdfViewer.ReadMode.vb`) NU a fost schimbat: el verifica deja starea, dar cauta numele clasice `AVTaskPaneHostView` + `AVDockableTabStripView`.

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: 0 avertismente, 0 erori. Nimic rulat.

## Unverified / deferred
- NU am o captura a ferestrei panoului in modul de citire (daca dispare, ramane ascunsa sau isi pierde doar stilul vizibil). Ambele se citesc «ascuns»; fereastra lipsa se asteapta 3 s. Linia din `adobe_preview.log` (ex. «Ctrl+H: AVTaskPaneView ascuns 0 px; pagina 1876 px din 1902 px») arata ce a vazut aplicatia.
- Pragul de 100 px (intre 26 px si 345 px din atasamente) si pauza de 3 s sunt alese de mine, nemasurate pe alte ecrane/DPI.
- Logica se aplica TUTUROR paginilor cu PDF care folosesc fereastra gazduita (DDF, ORD, facturi, rapoarte), nu doar rapoartelor; nu s-a rulat pe niciuna. Ctrl+S dupa semnatura nu a fost atins, dar trece prin aceeasi coada si acelasi timer.
- ActiveX: riscul ca, pe Acrobat DC, numele clasice sa nu se potriveasca (si a doua trimitere Ctrl+H sa readuca panourile) ramane nerezolvat si nedovedit.
