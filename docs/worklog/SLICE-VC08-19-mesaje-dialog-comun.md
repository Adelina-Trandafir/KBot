# SLICE-VC08-19 — mesaje în dialogul comun, eliminarea vc-message

Corecție cerută explicit: eliminarea elementului vizual `p#vc-message`,
renunțarea la notificările mici de tip tooltip și folosirea sistemului
existent de dialoguri, ca la confirmări/erori. Nu se adaugă elemente
vizuale noi în pagină fără acordul utilizatorului.

## Implementare

- `PYTHON/static/vercon.html`: șters efectiv `p#vc-message`.
- `PYTHON/static/js/vercon/messages.js`: adaptor VERCON peste `messageBox`
  din `utils/confirm-box.js`. Folosește dialogul comun, butonul Am înțeles
  și backdrop-ul modal existent pe întreg ecranul; fără alt design/CSS.
  Mesajele păstrează jurnalul de sesiune; apelul către jurnal dezactivează
  tooltipul automat. Mesajele generale nu au un host inline în pagină.
  Statusurile deja autorizate din barele de acțiuni își păstrează textul.
  O singură notificare modală curentă; următorul mesaj o înlocuiește.
  Închiderea ecranului-sursă închide și mesajul acestuia.
- `ui.js`: exportul showMessage folosește adaptorul VERCON, astfel încât
  notificările operațiilor și erorilor nu mai deschid toast-uri mici.
- `app.js`: erorile generale, pierderea ocupării și informarea schimbării
  selecției folosesc notify; eliminate dependențele de vc-message.
- `efactura-notice.js`: informarea facturilor folosește același dialog
  comun, cu titlu și text lizibil. Păstrează excluderea facturilor inactive,
  numărarea resturilor și protecția împotriva răspunsurilor din alt context.
  La părăsirea gestiunii se închide numai propriul dialog, fără banner.
- `PYTHON/static/help/vercon.json`: descrierea informării modale.

Nu s-a modificat sistemul de mesaje al altor aplicații. Tooltipurile
explicative ale controalelor și cele pentru elipsis rămân cele cerute
anterior; schimbarea privește prezentarea notificărilor aplicației.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).
