# SLICE-0126-01 - Clasificatii: randuri goale de adaugare, totaluri in arbore, tabel de angajamente (30.10.2026)

Operator, 30.10.2026, fereastra «Clasificatii cheltuieli» (pagina Cheltuieli). Numar de felie: primul liber (0126); fereastra e din 0087 (cu 0102, 0105, 0107), dar cererea nu a numit o felie - de confirmat.

## What changed and why
1. **Rand gol de adaugare** la tabelul de buget si la cel de rectificari: ultimul rand e un rand de rezerva (Tag `SpareTag`, ca in `FacturiForm.Linii`): fara buton «✕», cu un indiciu in prima celula text. Cand scrii in el devine rand real (data de inceput 01.01 sau azi, documentul gol si data de azi, ca la «+» de pana acum) si apare altul dedesubt. «+» din subsol foloseste acelasi rand. Salvarea si totalul il sar. Esc nu-l sterge.
2. **Inaltimi**: tabelul de buget incape 3 versiuni, cel de rectificari 6, **plus** randul gol, fara bara de derulare (`BudgetRowsFit`, `CorrectionsRowsFit`). Inaltimile se pun in cod (`ApplyGridHeights`) din noua `KBotDataView.HeightForRows`, urmarind DPI-ul si tema. Fereastra a crescut (ClientSize 1822x1140, minim 1570x1000 la 144 dpi) ca sa incapa si tabelul nou.
3. **Arbore**: la dreapta fiecarui rand apare totalul final (ultimul buget + toate rectificarile), format Standard fara zecimale (`N0`); la un nod, suma clasificatiilor vizibile de sub el. In `AdvancedTreeControl` o a TREIA parte `~~~` se deseneaza aliniata la dreapta; cautarea nu o cauta. Dupa «Salveaza» cifrele se refac pe loc, fara a reface arborele.
4. **Randul de total** are culoarea subsolului celorlalte tabele (`KBotDataView.FooterBackColorInUse` / `FooterForeColorInUse`, citite la desenare, deci urmeaza tema).
5. **Tabel nou «Angajamente»** (`gridAngajamente`): cod angajament + descrierea din FX_Angajamente in aceeasi celula («COD — descriere», coloana care se intinde), cod indicator, total rezervari, total receptii, total plati, pentru toate angajamentele cu un indicator pe clasificatia aleasa (la un nod: pe toate cele de sub el); subsol cu totalul fiecarei coloane. Ruta noua `POST /api/forexe/nomenclatoare/clasificatii/angajamente` ({ids}); totalurile ca in `sumar.py` (SUM(R_Valoare), SUM(DIF), SUM(Suma)), legate prin `CodAI`; angajamentele ascunse sunt lasate deoparte; fara filtru de an.

## 0126-02 (30.10.2026, aceeasi cerere continuata)
- Scoase butoanele «+» din subsolul tabelelor de buget si rectificari (randul gol le tine locul; handlerele si pictograma scoase). «+» din subsolul ARBORELUI ramane; pagina Venituri neatinsa.
- Butonul «✕» umple toata celula (margin si padding 0, fara chenar), plat: arata culoarea randului pe care sta. Nou in `KBotDataView`: `KBotDataColumn.ButtonHoverBackColor` (Empty = fara efect) si urmarirea hover-ului pe celula-buton; view-ul pune la hover o spalare (alpha 70) din `ErrorColor` al temei.
- Ajutor: `clasificatii.md`, `tur-clasificatii.md`, `clasificatii-buget.md`, `clasificatii-rectificare.md` (pasii cu «+» din subsol rescrisi pe randul gol; eticheta `0126-02`).
- Nevazut pe ecran; culoarea hover (rosu pal) aleasa de mine. Fisiere in plus: `KBotDataColumn.vb`, `KBotDataView.CellButton.vb`, `.Input.vb`, `.Painting.vb`.

## Files touched
`PYTHON/routes/forexe/clasificatii_edit.py`; `src/KBot.Domain/Nomenclatoare.vb` (+`ClasificatieAngajament`); `src/KBot.Api/INomenclatoareApi.vb`, `ApiClient.Nomenclatoare.vb`; `src/KBot.Controls/DataView/KBotDataView.Layout.vb`, `KBotDataView.Theming.vb`; `src/KBot.Controls/Tree/AdvancedTreeControl.Painting.vb`, `AdvancedTreeControl.Search.vb`; `src/KBot.App/Views/Nomenclatoare/ClasificatiiBugetView.vb`, `.Designer.vb`, `ClasificatiiForm.Designer.vb`; FileVersion: Controls 1.75.0.0, Api 1.0.29.0, Domain 1.2.20.0, App 1.1.4.0; ajutor `contabil/nomenclatoare/clasificatii.md` (eticheta `0126-01`); `docs/worklog/state/KBOT_STATUS_0120-0129.md`, `docs/worklog/KBOT_STATUS.md`.

## Test results
`py_compile` pe `clasificatii_edit.py`: verde. `dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: 0 avertismente, 0 erori. Nimic rulat, nici un test (la cererea regulii proiectului).

## Unverified / deferred
- **Nevazut pe ecran**, nerulat pe server, ruta noua nedeployata. Nu s-a verificat ca inaltimile cad exact (fara bara de derulare la 3+1 si 6+1 randuri) la 100/125/150%.
- **Interpretare de confirmat:** «3 randuri» si «6 randuri» sunt randuri cu date; randul gol vine in plus (deci 4 si 7 linii vizibile). Daca operatorul le voia INCLUSIV randul gol, se schimba doua constante (`BudgetRowsFit`, `CorrectionsRowsFit`).
- La o fereastra mai joasa de ~1000 px logici (la 144 dpi) tabelul «Angajamente» ramane cu putine randuri; cele doua tabele de sus au inaltime fixa.
- Totalul din arbore se calculeaza din ultimul buget al anului + toate rectificarile anului; clasificatiile fara nimic arata «0». Mai multe angajamente ale aceluiasi indicator se aduna pe `CodAI`; ce are `CodAI` nul nu se numara.
- Ajutorul: `help-version.txt` neatins; captura `clasificatii` nerefacuta (fereastra s-a schimbat). Nimic in `NOUTATI.md`.
