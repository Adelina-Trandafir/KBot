# SLICE-AD14-01 · Închiderea lunii cu schimbarea taxei

26.10.2026. Cerința operatorului: confirmarea de la închiderea lunii nu este suficientă; o fereastră pe același sistem ca mesajele (fundal estompat) care permite schimbarea taxei, cu DGV-ul din Taxe (taxa activă prima) și buton de adăugare taxă nouă, care deschide și poziționează DGV-ul pe rândul nou. Taxa adăugată devine activă, celelalte nu mai sunt.

## Ce s-a schimbat și de ce

- Luna următoare se deschide cu taxa activă (`create_next_month`); confirmarea simplă nu lăsa operatorul să o vadă sau să o schimbe. Lacătul unei luni deschise (alta decât august) deschide acum fereastra «Închidere lună».
- Fereastra refolosește tabelul și validările din Taxe (`catalogs.js`) și aceeași salvare `/api/adechit/catalog-save`: nicio logică nouă pe server. Diferențe: rândurile sunt puse taxa activă prima, apoi începuturile descrescător, fără sortare implicită; «Adaugă taxă» propune începutul în luna care se deschide (sau luna de după ultimul început) și poziționează tabelul pe rândul nou; sub întrebare se vede taxa activă, actualizată la fiecare modificare.
- «Închide luna»: salvează modificările de taxe (dacă sunt), apoi se apelează închiderea obișnuită. «Renunță» / Escape / Escape: luna rămâne deschisă, modificările din fereastră se abandonează. Dacă lista de taxe nu se încarcă, butonul de închidere rămâne dezactivat.
- Cele două cereri (salvarea taxelor, apoi închiderea) nu sunt o singură tranzacție: dacă închiderea eșuează după ce taxele s-au salvat, taxele rămân salvate (se pot vedea în Taxe).

## Fișiere atinse

`PYTHON/static/adechit.html` (dialogul `ade-close-month`); `PYTHON/static/css/adechit-close-month.css` (nou), `PYTHON/static/css/adechit.css` (import); `PYTHON/static/js/adechit/catalogs.js` (două ferestre pe același tabel; `confirmMonthClose`); `PYTHON/static/js/adechit/app.js` (lacătul apelează `confirmMonthClose` în loc de `confirmBox`); `PYTHON/static/help/adechit.json`, `ADECHIT/UTILIZARE_WEB.md`, `ADECHIT_STATUS.md`, `docs/worklog/state/ADECHIT_STATUS_ADE10-ADE19.md`

## Verificări

- `node --check` pe `catalogs.js` și `app.js`; `adechit.json` parsează. Atât.
- Nu s-a rulat aplicația, nu s-au rulat teste, nu s-a văzut fereastra (preferința operatorului pentru acest flux).

## Nerulat / amânat

- Aspectul ferestrei, PC/mobil, tema întunecată; fluxul complet pe server.
- Închiderea lui august (fereastra anuală) nu primește selecția taxei.
- Activarea unei taxe mai vechi decât cea activă este respinsă de server («Perioada taxei se suprapune»), ca și în Taxe.
