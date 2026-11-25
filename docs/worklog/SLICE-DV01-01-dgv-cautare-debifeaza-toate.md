# SLICE-DV01-01 — DGV VB.NET: căutarea debifează toate valorile

Data: 16.11.2026.

În meniul de filtrare pe coloană al controlului comun `KBotDataView`, fiecare modificare a textului de căutare golește selecția completă înainte de reconstruirea listei afișate. Sunt debifate inclusiv valorile ascunse de căutare. Golirea textului păstrează toate valorile debifate. Operatorul bifează apoi valorile dorite, individual sau prin «Selectează tot», și aplică filtrul prin butonul existent OK.

Testul existent pentru căutare a fost actualizat pentru debifarea tuturor valorilor, selecția explicită a unui rezultat și resetarea selecției la modificarea sau golirea căutării.

Completare la cererea Adelinei: alegerea operatorului «Conține» nu mai deschide dialogul de operand. Textul se editează în caseta de căutare existentă sub forma `%text%`, cu partea dintre semne selectată pentru înlocuire. Semnele `%` de la capete indică potrivirea «conține» și nu sunt căutate literal; nu este introdus un limbaj general de wildcarduri. Același text alimentează atât lista de opțiuni, cât și operandul condiției aplicate la OK. Bifele se golesc la alegerea operatorului și la editarea căutării; rezultatele se bifează explicit. Redeschiderea unui filtru «Conține» reface textul în casetă și păstrează selecția salvată. Celelalte condiții păstrează dialogurile existente.

Nu au fost rulate teste, builduri sau alte verificări. Înregistrarea pentru deployment așteaptă confirmarea explicită a Adelinei după probă, conform AGENTS.md.

Fișiere proprii:

- `src/KBot.Controls/DataView/Filter/KBotFilterPopupFilterView.vb`
- `src/KBot.Controls/DataView/Filter/KBotFilterPopup.vb`
- `tests/KBot.Controls.Tests/KBotFilterPopupTests.vb`
- `docs/worklog/SLICE-DV01-01-dgv-cautare-debifeaza-toate.md`
