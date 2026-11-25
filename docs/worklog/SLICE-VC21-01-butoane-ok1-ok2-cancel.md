# SLICE-VC21-01 — Clase comune de butoane: OK1, Cancel, OK2

18.11.2026 — Cerință (Adelina): dacă nu există, trei clase de butoane (js/css): OK1 (etichetă configurabilă, declanșează salvarea sau orice înseamnă OK), Cancel (etichetă configurabilă, renunțare / închiderea paginii), OK2 (etichetă configurabilă, acțiune suplimentară, arată altfel decât OK1).

## Ce s-a schimbat

- **Verificare.** Nu exista un set comun: doar `.btn`, `.btn-primary`, `.btn-secondary` (inregistrare.css), `.btn-danger` (operator.css), `.btn--*` pe site, iar VERCON/ADECHIT aveau doar efectele de hover din `application-button-effects.css`.
- **Componentul nou** `PYTHON/static/js/components/dialog-button/`, ca `action-add-button`:
  - `dialog-button.css`: `.k-dialog-button--ok1` (fundal accent plin), `.k-dialog-button--cancel` (neutru, contur din `--line`), `.k-dialog-button--ok2` (contur accent, fundal transparent). Rândul `.k-dialog-buttons` pune Cancel în stânga, OK2 apoi OK1 în dreapta. Culorile vin din variabilele de temă (`--blue-mid`, `--blue-deep`, `--card`, `--line`, `--ink`, `--sky-2`) cu rezerve, plus variante pentru tema dark; toate reglabile prin `--dialog-button-*`.
  - `dialog-button.js`: `new DialogButton(container, { kind: 'ok1'|'cancel'|'ok2', label, title, disabled, className, onClick })`, cu `setLabel`, `setEnabled`, `destroy`; `dialogButtonRow(container)`. Etichete implicite: «Salvează» (OK1), «Renunță» (Cancel); OK2 cere etichetă. Un `kind` necunoscut sau o etichetă goală aruncă eroare.
- Clasele se pot folosi și direct în HTML: `<button class="k-dialog-button k-dialog-button--ok2">`.

- **Montat într-o pagină (la cererea Adelinei).** Fereastra «Produs din eFactura» (`efactura-line-dialog.js`): «Renunță» = Cancel, «Salvează» = OK1, prin `DialogButton`. `vercon.html` încarcă acum `dialog-button.css`. În `vercon-efactura.css` (rândurile 21–25) regulile vechi ale butoanelor din subsolul `.vc-efi` exclud `.k-dialog-button` prin `:not(:where(.k-dialog-button))` (fără să crească specificitatea), altfel ar fi acoperit stilul nou. Celelalte butoane din `.vc-efi` rămân neschimbate. OK2 nu e folosit nicăieri încă.

- **Mesajele declanșate de grile (și de toate paginile).** Mesajele din `js/utils/confirm-box.js` (`confirmBox`, `reasonBox`, `numberBox`, `messageBox`, `pickBox`), cele pe care le pornesc grilele din nomenclatoare (de ex. «Renunțați la modificările nesalvate…», «Activați / Dezactivați…», «Produsul nu are cod CPV…»), folosesc acum `DialogButton`: «Nu» / «Renunță» = Cancel (stânga), «Da» / «Salvează» = OK1 (dreapta), acțiunea suplimentară din `messageBox` = OK2 (rămâne în stânga, ca înainte). Alegerile din lista `pickBox` rămân butoane simple. `ensureStyle` încarcă și `dialog-button.css`, deci merge în toate paginile care folosesc mesajele (vercon, adechit, inventar). În `confirm-box.css` regulile vechi ale butoanelor exclud `.k-dialog-button` (`:not(:where(...))`); un mesaj de avertizare își păstrează butonul principal roșu (variabile `--dialog-button-ok1-*`, selector lung ca regula dark din `dialog-button.css` să nu câștige). Ferestrele care își construiesc singure butoanele în `.confirm-box` (`supplier-change.js`, `receipt-number-change.js`) nu s-au atins.

## Fișiere

- `PYTHON/static/js/components/dialog-button/dialog-button.css` (nou), `dialog-button.js` (nou)
- `PYTHON/static/js/utils/confirm-box.js`, `PYTHON/static/css/confirm-box.css`
- `PYTHON/static/js/vercon/efactura-line-dialog.js`, `PYTHON/static/css/vercon-efactura.css`, `PYTHON/static/vercon.html`
- `VERCON/VERCON_STATUS.md`, acest worklog

## Presupuneri

- Numele claselor (`k-dialog-button--ok1/ok2/cancel`) și aspectul (OK1 plin, OK2 contur accent, Cancel neutru) sunt alese de mine; nu cer nimic la vederea operatorului până nu e folosit într-o pagină.
- Doar fereastra «Produs din eFactura» folosește componentul; celelalte butoane existente nu au fost înlocuite (nu s-a cerut).

## Neverificat

Înregistrarea pentru publicare (5 fișiere) s-a făcut înainte de extinderea la mesaje; `confirm-box.js` și `confirm-box.css` nu sunt încă înregistrate (nouă confirmare de la Adelina, `/ssh`). Mesajele nu au fost deschise pe ecran (aspect light/dark, avertizare roșie, hover pe butonul roșu, cele 4 tipuri de mesaj).

Doar sintaxa JS (`node --check`). Nu s-a rulat pagina: aspectul ferestrei «Produs din eFactura» (light/dark, hover, focus, lățimea butoanelor față de înainte) nu a fost văzut pe ecran. Ajutorul: fereastra își schimbă doar aspectul butoanelor, textele și pașii rămân; nu am modificat `vercon.json` (de verificat dacă o captură a ferestrei trebuie refăcută). Nepublicat; nu s-a făcut înregistrarea pentru publicare (abia după proba Adelinei).
