# SLICE-0000-151 - Ajutor: motivul rezervarii oferit la revizie (04.11.2026)

Ajutorul feliei **0127-01**. Etichete `0127-01, 0000-151`.

## What changed and why
- `contabil/forexe/browser.md`: sectiune noua «Motivul unei rezervări se păstrează pentru revizie» (ce se pastreaza, unirea cu «; » doar la acelasi indicator, motive diferite cu codul indicatorului, taierea la 255, ce rezervari NU au motiv).
- `contabil/ddf/editor.md`: randul «Descriere scurtă» din tabelul «Antetul» trimite la sectiunea de mai sus.
- `tutorials/revizie-din-rezervare.md`: textul pasului «Descriere scurtă» (continutul implicit al reviziilor > 0).

Scris pentru utilizator: fara mecanismul intern (clicul oprit, marcajul, tabelul). 

## Files touched
`src/KBot.App/HelpContent/contabil/forexe/browser.md`, `contabil/ddf/editor.md`, `tutorials/revizie-din-rezervare.md`.

## Test results
`Check-Help.ps1 -Coverage` rulat o data: erorile ramase sunt vechi si nu din aceasta felie (sectiuni fara eticheta in `tutorials/ordonantare-din-plata.md`; control lipsa `chkFaraActivitate` in `tutorials/parteneri.md`). Eticheta `0127-01` / `0000-151` a primit rand in registru dupa prima rulare.

## Unverified / deferred
- Nevazut pe ecran. Nicio captura noua; `help-version.txt` neatins.
- Textul descrie comportamentul de la 0127-01, care **nu a rulat inca** (DDL nerulat, server nedeployat).
