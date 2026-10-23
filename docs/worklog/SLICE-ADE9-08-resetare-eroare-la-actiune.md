# SLICE-ADE9-08 — ascunderea erorii generale la următoarea acțiune

19.10.2026. TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

Notificarea `ade-message` din pagina principală rămânea afișată după eroare,
inclusiv după selectarea unui copil sau alegerea altei luni/grupe.

Modulul comun `static/js/portal/messages.js` oferă acum legarea opțională
`clearMessageOnAction`. Evenimentele pointerdown, click, keydown, input,
change și submit sunt ascultate în captura ferestrei, înainte de handlerii
acțiunilor. Mesajul existent cu rol alert este golit și ascuns prin
`showMessage`, care resetează și rolul la status. Mesajul rămâne în istoricul
comun; golirea nu adaugă o înregistrare nouă. Un handler care raportează o
nouă eroare o poate afișa după această resetare.

ADECHIT leagă mecanismul numai pentru notificarea generală a paginii și
eliberează listenerii la pagehide. Nu se modifică validarea câmpurilor sau
mesajele proprii formularelor. Hoverul și răspunsurile de fundal nu declanșează
resetarea notificării.

De publicat prin AvacontPush:

- `PYTHON/static/js/portal/messages.js`;
- `PYTHON/static/js/adechit/app.js`;
- `PYTHON/static/help/adechit.json`.

Documentație: `ADECHIT/UTILIZARE_WEB.md`, statusul principal și starea ADE0–ADE9.
Probat de utilizator: eroarea „Selectați un copil din tabel.” urmată de
alegerea unui copil, schimbarea lunii/grupei, editare, tastatură și repetarea
unei acțiuni invalide: TESTAT, RULAT, PUBLICAT.
