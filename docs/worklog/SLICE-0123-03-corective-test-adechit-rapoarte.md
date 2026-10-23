# SLICE-0123-03 - Corective dupa primul test al paginii «ADECHIT Rapoarte» (22.10.2026)

Operator, dupa ce a incercat pagina (cu imaginea «Situatie Lunara Debitori», toate grupele separate): patru lucruri.

## What changed and why
1. **Bara de incarcare la orice cerere catre server.** Pagina are `busyBar` (KBotBusyBar, sub bara de titlu); merge cat timp `_busy` (incarcarea contextului, anilor, lunilor, grupelor, cautarea copilului, pregatirea si descarcarea raportului, Excelul). Pana acum pagina parea blocata.
2. **Subunitatea aparea de 12 ori** in antet la «toate grupele + separa grupe». Cauza (din cod, nu din ghicit): `situation_report` pune ACELASI dictionar `letterhead` pe fiecare pagina (`blocks += [*head, ...]`), iar `report_identity.add_subunit` modifica dictionarul pe loc la fiecare pagina, deci textul «Subunitate: ...» se aduna cate o data pe pagina. Acum `add_subunit` modifica o COPIE a blocului (`PYTHON/routes/adechit/report_identity.py`). Acopera si Situatia de plata (acelasi tipar).
3. **Ctrl+2 dupa afisarea raportului**, o singura data, dupa ce focusul tastaturii intra in document: `ReaderHostPreview.FitWidthOnly` (nou, implicit False, deci celelalte pagini nu se schimba) trimite Ctrl+2 FARA Ctrl+H, pe ambele motoare: fereastra gazduita (`AdobeReaderHost.FitWidthOnly` -> `QueueKey(VK_2)`, aceleasi conditii si acelasi focus ca la celelalte taste) si ActiveX (`AcroPdfViewer.FitWidthOnly` -> `_fitWidthPending`). Tasta se trimite cu apasare + eliberare a lui Ctrl in acelasi apel `SendInput` (nu ramane apasat). Pagina o pune pe `previewPdf`.
4. **Lista «Copil» nu se activa.** Nu am gasit un defect clar in cod (lista se activeaza doar dupa o cautare cu rezultate, iar cautarea pornea doar la Enter). Cautarea porneste acum si singura, dupa 700 ms fara tastare (cel putin 3 caractere), plus imediat la Enter (`tmrCauta` in Designer). Daca tot nu se activeaza, cauza e alta: vezi mai jos.

## Files touched
`PYTHON/routes/adechit/report_identity.py`; `KBot.Controls/Adobe/AdobeReaderHost.vb`, `AcroPdfViewer.ReadMode.vb`; `KBot.App/Views/Ddf/ReaderHostPreview.vb`; `KBot.App/Views/Adechit/AdechitRapoarteForm(.Designer).vb`; help `contabil/adechit.md`.

## Server de urcat (in plus fata de 0123-01)
`routes/adechit/report_identity.py`.

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: 0 avertismente, 0 erori. `py_compile` ok. Nimic rulat: nici aplicatia, nici serverul.

## Unverified / deferred
- **Lista «Copil»**: cauza nu e dovedita. Daca dupa cautare (cu cel putin 3 litere dintr-un nume care exista in luna aleasa) ramane gri, e nevoie de `harness_errors.log` de pe PC-ul de test (linia `AdechitRapoarteForm.SearchChildrenAsync`) si de textul din linia de sub raport; nu am citit jurnalele.
- Ctrl+2: nu stiu ce motor Adobe e pe PC-ul de test (fereastra gazduita sau ActiveX); ambele sunt acoperite, niciunul rulat. Ctrl+H nu se mai trimite pe pagina asta (la cererea «doar Ctrl+2»).
- Bara de incarcare: nevazuta pe ecran (culoare, inaltime 5 px).
- Cat timp Adobe isi deschide documentul, bara de incarcare s-a oprit deja (se opreste cand PDF-ul e predat vizualizatorului).
