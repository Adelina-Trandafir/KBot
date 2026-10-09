# ADE6-23 — Eliminarea mesajului introductiv din Plătitori

Mesajul «Selectați grupa, copilul și plătitorul. Folosiți Adaugă sau Modifică.»
nu mai apare la deschidere, reîncărcare sau salvare. Elementul ade-payers-message
rămâne ascuns pentru fluxul normal și este folosit numai pentru erori.

Fișiere: PYTHON/static/js/adechit/payers.js, PYTHON/static/adechit.html,
UTILIZARE_WEB.md și documentele de stare. Încărcarea inițială păstrează numai
grupele; copiii și plătitorii se încarcă după selecție.

Nu s-au executat teste automate sau verificări funcționale/vizuale.
Publicarea și proba în browser revin utilizatorului.

Următoarea subfelie liberă: ADE6-24.
