# SLICE-VC07-07 — Bara de acțiuni a listei de alimente

Data: 2026-10-23. Istoricul aceleiași bare a listei: ordinea acțiunilor, apoi eliminarea mesajului permanent. VC07-08 păstrează mesajele operațiilor și restricțiile funcționale.

<a id="slice-vc07-07"></a>

În consultarea unei liste, bara afișează numai Vizualizare și Modifică, în această ordine. Istoric este scos din bară și rămâne în meniul contextual al listei din arborele Liste salvate, împreună cu înregistrările de audit. Eticheta Previzualizare a listei devine Vizualizare, inclusiv în meniu.

La adăugare, modificare completă sau modificare numai a porțiilor, Renunță este aliniat la marginea stângă și Salvează lista la marginea dreaptă. Vizualizare nu apare în editare. Mesajele sunt între cele două butoane; pe ecrane înguste trec pe rândul următor, păstrând pozițiile butoanelor. Salvarea este marcată ca acțiune principală. Regulile de eligibilitate, salvare, calcule și audit rămân cele existente.

Helpul este aliniat cu noile etichete, ordinea și accesul la Istoric din meniul arborelui. Nu au fost refăcute capturi.

Fișiere comune VC07-07/08: PYTHON/static/js/vercon/lista.js; PYTHON/static/css/vercon-lista.css; PYTHON/static/help/vercon.json; implementare_vercon_v2.md și acest jurnal.

Stare comună VC07-07/08: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

<a id="slice-vc07-08"></a>

## SLICE-VC07-08 — Lista de alimente compactă, fără mesaj permanent — 23.10.2026

### Ce s-a schimbat

La deschiderea unei liste nu se mai afișează automat mesajul de ordine cronologică și nici mesajul alternativ despre modificarea porțiilor. Spanul vc-save-status și containerul de mesaje nu sunt inserate în footer când nu există un mesaj activ sau o perioadă închisă. Nu este ascundere CSS a textului: elementul cu mesajul permanent nu se mai generează în documentul afișat. Pe ecrane înguste, footerul nu rezervă nici rândul secundar de mesaje dacă nu există containerul de mesaje.

Mesajele unei operații în curs, erorile și confirmările rămân disponibile atunci când sunt necesare. Restricțiile existente asupra modificării și anulării, inclusiv excepția pentru porții în ultima lună, rămân aplicate.

Publicare necesară: modulul JS și CSS/help împreună, apoi reîncărcarea paginii.
