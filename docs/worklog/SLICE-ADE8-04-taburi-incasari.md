# SLICE-ADE8-04 — taburile de încasări și restituiri

## Ce s-a schimbat și de ce

Footerul are taburile Chitanțe, Alte plăți și Restituiri legate de copilul selectat. Rândurile
salvate sunt numai pentru citire; numai rândul nou permite Data și Valoarea. După alegerea
datei se propune SID la Chitanțe/Alte plăți și SIC la Restituiri, iar suma poate fi modificată
înainte de salvare. Salvarea reîncarcă situația lunară.

## Fișiere atinse

- `PYTHON/static/adechit.html`
- `PYTHON/static/css/adechit.css`
- `PYTHON/static/js/adechit/app.js`

## Verificări

- taburile, selecția copilului și rândul nou au fost probate vizual în preview.
- suită ADE: 9 teste trecute; sintaxa JavaScript și `git diff --check` au trecut.

## Neverificat sau amânat

Configurația și numerotarea reală a chitanțelor necesită proba pe server. Pentru Alte plăți,
interfața trimite automat tipul „Altă plată” și numărul „Fără număr”, deoarece cerința curentă
lasă editabile numai data și valoarea. Anularea restituirii rămâne blocată de M03.
