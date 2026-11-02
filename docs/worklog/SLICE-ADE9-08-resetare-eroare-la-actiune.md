# SLICE-ADE9-08 — ascunderea erorii generale la următoarea acțiune

19.10.2026. TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

Notificarea `ade-message` din pagina principală rămânea afișată după eroare, inclusiv după selectarea unui copil sau alegerea altei luni/grupe.

Legarea opțională `clearMessageOnAction` din `static/js/portal/messages.js` ascultă pointerdown, click, keydown, input, change și submit în captura ferestrei, înaintea handlerilor. `showMessage` golește/ascunde alerta și schimbă rolul în status; istoricul comun o păstrează, fără înregistrare pentru golire. Handlerul poate afișa apoi o eroare nouă.

ADECHIT îl leagă numai la notificarea generală și eliberează listenerii la pagehide. Validarea câmpurilor și mesajele formularelor rămân neschimbate; hoverul și răspunsurile de fundal nu resetează notificarea.

De publicat prin AvacontPush:

- `PYTHON/static/js/portal/messages.js`;
- `PYTHON/static/js/adechit/app.js`;
- `PYTHON/static/help/adechit.json`.

Documentație: `ADECHIT/UTILIZARE_WEB.md`, statusul principal și starea ADE0–ADE9. Probat de utilizator: eroarea „Selectați un copil din tabel.” urmată de alegerea unui copil, schimbarea lunii/grupei, editare, tastatură și repetarea unei acțiuni invalide: TESTAT, RULAT, PUBLICAT.
