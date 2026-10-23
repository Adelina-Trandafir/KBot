# SLICE-VC07-08 — Lista de alimente compactă, fără mesaj permanent — 23.10.2026

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

## Ce s-a schimbat

La deschiderea unei liste nu se mai afișează automat mesajul de ordine
cronologică și nici mesajul alternativ despre modificarea porțiilor. Spanul
vc-save-status și containerul de mesaje nu sunt inserate în footer când nu
există un mesaj activ sau o perioadă închisă. Nu este ascundere CSS a textului:
elementul cu mesajul permanent nu se mai generează în documentul afișat.
Pe ecrane înguste, footerul nu rezervă nici rândul secundar de mesaje dacă
nu există containerul de mesaje.

Mesajele unei operații în curs, erorile și confirmările rămân disponibile
atunci când sunt necesare. Restricțiile existente asupra modificării și
anulării, inclusiv excepția pentru porții în ultima lună, rămân aplicate.

## Fișiere

PYTHON/static/js/vercon/lista.js; PYTHON/static/css/vercon-lista.css;
PYTHON/static/help/vercon.json; implementare_vercon_v2.md; acest jurnal.

Publicare necesară: modulul JS și CSS/help împreună, apoi reîncărcarea paginii.
