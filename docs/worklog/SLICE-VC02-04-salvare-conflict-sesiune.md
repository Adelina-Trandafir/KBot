# SLICE-VC02-04 — salvare, conflict, sesiune expirata

Stare: **IMPLEMENTAT_LOCAL**. Verificat doar sintaxa (Python `ast`, `node --check`); nimic rulat, previzualizat sau testat; nimic validat pe server.

## Facut
- `static/js/vercon/save.js`: o salvare = un RequestKey; rezultate saved / error / conflict / unknown (+ expired, stale). La unknown cheia NU se schimba; urmeaza `reconcile` (GET dupa cheie). Al doilea apel in timpul unei salvari returneaza aceeasi promisiune (dublu Enter).
- Raspunsul este folosit doar daca versiunea de context e cea din momentul pornirii; eroarea si sesiunea expirata pastreaza datele in formular.
- Stand-in de server doar in preview: `POST /api/vercon/preview/save` (revizie + cheie idempotenta, `X-Preview-Simulate: lose-response`) si `GET …/save/<cheie>`.

## Limite
Endpoint-urile reale de salvare vin la VC06; in productie butonul spune ca salvarea vine cu VC06. Diagnosticul pe linie/camp exista doar pentru validarea din pagina.
