# SLICE-0123-04 - Ctrl+H / Ctrl+2 hotarate de ferestrele Adobe (23.10.2026)

Operator: Ctrl+H este critic (comuta modul de citire; nu trebuie trimis orbeste), Ctrl+2 mai putin; sa nu se apese «indiscriminator», doar cand ferestrele dovedesc ca trebuie. Atasamente (Window Detective, Acrobat DC gazduit): panourile vizibile = fereastra `AVTaskPaneView` lata de 345 px, pagina 1557 px din 1902 px; mod citire = `AVPageView` 1876 px din 1902 px.

## What changed and why
- **`KBot.Controls/Adobe/AdobeReaderHost.vb`** (motorul «fereastra gazduita», cel implicit): `PlanKeys` decide la fiecare tic al timerului ce tasta pleaca.
  - Ctrl+H numai cu dovada panourilor vizibile: AVTaskPaneView/AVTaskPaneHostView vizibilă și lată SAU pagină cu peste 100 px mai îngustă decât gazda. Panouri ascunse → modul de citire păstrat de Adobe: fără Ctrl+H/Ctrl+2. Nicio fereastră de panou → așteaptă până la 3 s pentru construirea UI, apoi consideră modul deja pornit.
  - După Ctrl+H, panouri încă vizibile la 700 ms → încă o trimitere (maximum 2 total), apoi abandon cu indicație manuală; fără Ctrl+2.
  - Ctrl+2 o singură dată, după dispariția panourilor și lățime de pagină egală două tacte consecutiv (sau 8 s de la Ctrl+H). Numai dacă s-a trimis Ctrl+H.
  - Focus în document numai când pleacă o tastă; așteptarea păstrează focusul K-BOT. Ctrl+S după semnătură folosește aceeași coadă; tastele ies înainte de SendInput, Ctrl apăsare/eliberare în același apel.
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
