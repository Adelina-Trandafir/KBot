# SLICE-0000-163 - Ajutor: factura se leagă pe total, cu alocări în loc de linii (06.11.2026, felia KE20-04)

## What changed and why
`contabil/efactura/index.md`, «Pagina «Recepții»» (zona din mijloc sus, acum «Factura …») și «Recepțiile unei facturi primite»: tabelul cu facturi nu mai are produsele facturii, ci alocările ei (clasificație și sumă): primul rând din
antet, un rând gol cât mai e de alocat, suma rândurilor nu depășește restul facturii; factura legată arată clasificațiile pe care e alocată; pentru «Leagă» coloana «Valoare» a clasificațiilor nu se mai scrie. Etichete `KE20-04, 0000-163`.

## Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`.

## Test results
`Check-Help.ps1` nerulat.

## Unverified / deferred
- Nevăzut pe ecran; capturile paginii (produse, rânduri de grup) trebuie refăcute.
- Secțiunea «Recepțiile unei facturi primite» (vederea din bara facturii) a primit doar linia cu «alocările facturii»; restul textului ei despre produse nu a fost recitit.
