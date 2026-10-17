# DGV + ComboTree pe localhost

## Probă sesiune 3 / 2 / 1 / 0

Deschide http://localhost:5052/session-demo și apasă **Pornește testul de
3 minute**. Cronometrul este vizibil între selectorii de context și cont.
La 02:00 apare primul mesaj custom; la 01:00 apare mesajul serios și
headerul devine portocaliu; la 00:00 se închid formularele/ferestrele de
probă și pagina trece direct la `/session-ended`, fără confirmare nativă.

Poți lăsa text nesalvat, un formular sau o fereastră de probă deschise.
Mesajul custom arată timpul rămas și poate fi închis cu Am înțeles; aceasta
nu prelungește sesiunea. Testul folosește direct timerul și dialogurile
comune din aplicație. Numai pragurile sunt injectate pentru localhost;
aplicația păstrează avertizările la 15 și 5 minute. Nicio autentificare,
cerere de logout reală sau conexiune la baze de date nu este folosită aici.

## Probă DGV / ComboTree

Din rădăcina repository-ului:

```powershell
& .\PYTHON\.venv\Scripts\python.exe .\tools\ComboTreePreview\preview_server.py
```

Deschide http://localhost:5052/demo. Oprire: Ctrl+C. Pentru alt port,
adaugă `--port 5053`. Serverul ascultă numai pe 127.0.0.1 și nu importă
aplicația, configurarea sau modulele bazelor de date.

Pagina folosește direct sursele curente ale DGV și ComboTree din
`PYTHON/static`. Fișierele demonstrației rămân în `tools`, în afara aplicației
publicate. Reîncărcarea paginii preia modificările JS/CSS și resetează datele.

Probe manuale:

- Click în Produs: căutarea trebuie să acopere câmpul celulei, cu aceeași lățime.
- Alege o coloană de 160 px și un dropdown de minimum 420 px.
- Caută `ALMETTE`, selectează un produs și urmărește Produs, UM și jurnalul.
- Redeschide celula și restrânge/extinde o categorie: arborele rămâne deschis.
- Cu selecția părinților oprită, clickul pe categorie doar o extinde/restrânge.
- Pornește selecția părinților: eticheta categoriei o selectează, expanderul doar o extinde/restrânge.
- Încearcă frunzele din subramura Mobilier, Enter, Tab, Shift+Tab și Escape.
- După alegerea unui produs (click sau Enter), editorul Cantitate primește focus.
- Din căutare, sus/jos mută focusul în arbore. Sus/jos navighează rândurile vizibile.
- În arbore, stânga restrânge ramura sau merge la părinte; dreapta extinde
  ramura sau intră în primul copil. Enter execută același lucru ca un click pe etichetă.
- Tastarea în arbore mută focusul în căutare și introduce caracterul tastat.
  În căutare, stânga/dreapta păstrează comportamentul nativ al cursorului.
- Editează ultimele rânduri vizibile pentru deschiderea listei în sus.

Toate datele sunt fictive și rămân în memoria paginii. Consola browserului
primește logurile componentelor prin endpointul local `/api/test-mode`.
- Primele două rânduri au Preț `0,5550`, Cantitate `1,25` și Valoare `1.234,50`.
  Intră în editare pe primul rând și compară-l cu al doilea: aliniere,
  font și text identice. UM este editabil și centrat; Observații la stânga.
- Schimbă Font grilă între 15 și 20 px și repetă comparația.
- Confirmarea fără modificări păstrează valoarea originală; modificarea
  unui număr cu virgulă sau cu separatori de mii în Valoare se salvează numeric.
