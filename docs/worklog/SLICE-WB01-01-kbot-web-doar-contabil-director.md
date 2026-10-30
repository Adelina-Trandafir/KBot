# SLICE-WB01-01 — KBOT web numai pentru Contabil și Director

Data: 30.10.2026.

## Scop

Aplicația KBOT este `/portal`. Numai rolurile Contabil și Director, din `Unitati_Utilizatori.Rol` pentru unitatea curentă, pot selecta și deschide aplicația. Administratorul, Secretarul, rolurile necunoscute și rolul lipsă nu primesc acces. Restricția privește aplicația web.

## Schimbări

- `PYTHON/routes/portal/policy.py`: politica explicită de intrare, cu normalizarea literelor și a spațiilor.
- `PYTHON/routes/portal/drepturi.py`: secțiunea KB este disponibilă numai celor două roluri; acordarea unui rol de secțiune KB prin `Utilizatori_Roluri` nu ocolește această condiție.
- `PYTHON/routes/portal/access.py`: fiecare cerere de date și PDF recitește apartenența și rolul din baza comună. Administratorul și Secretarul nu mai au excepția de acces la DDF/ORD. Refuzul pentru rol nepermis este HTTP 403. Drepturile de consultare existente ale Directorului în interiorul KBOT rămân cele existente.
- `PYTHON/routes/portal/portal.py`: `GET /api/portal/kbot` controlează intrarea pe server, recitește rolul și livrează contextul KBOT numai pentru Contabil/Director; lista de unități conține numai unitățile eligibile.
- `PYTHON/static/js/portal/{application-choice.js,portal.js}` și `PYTHON/static/portal.html`: alegerea aplicației are loc după autentificare și alegerea unității. Opțiunea KBOT lipsește pentru celelalte roluri. Introducerea directă a adresei `/portal` nu deschide ecranele KBOT pentru acestea; rămâne disponibil fluxul comun de autentificare/alegere a altei aplicații. Înainte de afișarea ecranelor KBOT se cere contextul autorizat de server.
- `PYTHON/static/js/portal/{access.js,app.js}`: aceeași limită în interfață; schimbarea unității cu un rol devenit nepermis oprește încărcarea și revine la alegerea aplicației.
- `PYTHON/static/js/portal/application-access.js`, `PYTHON/static/js/portal/apps-menu.js`, `PYTHON/static/js/adechit/app.js`, `PYTHON/static/js/vercon/application-menu.js`: meniurile cer rolul curent și omit intrarea KBOT pentru celelalte roluri, inclusiv dacă citirea drepturilor eșuează.
- `PYTHON/static/css/portal-signin.css`: adaptarea selectorului de unitate și a derulării pe ecrane mici la cardul de alegere a aplicației.

## Stare și limite

Implementare locală. Nu s-au rulat teste, compilări, verificări de sintaxă, server local sau probe în browser, conform instrucțiunilor proiectului. Nu s-au executat SQL și nu s-au modificat scheme MariaDB. Nu s-a publicat pe server.

`/portal` păstrează pagina comună de autentificare folosită și de ADECHIT/VERCON; aceasta poate fi încărcată fără sesiune. Restricția împiedică afișarea aplicației KBOT și este aplicată pe server atât la intrarea în aplicație, cât și la fiecare cerere de date/PDF. Autentificarea comună, accesul celorlalte aplicații și aplicația desktop nu primesc restricții suplimentare.
