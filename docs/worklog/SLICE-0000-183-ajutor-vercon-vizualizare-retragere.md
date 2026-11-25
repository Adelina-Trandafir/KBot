# SLICE-0000-183 — Ajutor: factura trimisă la Vercon se vede și se retrage (25.11.2026)

Felia 00EF-68. Procedura: `docs/HELP_SYSTEM.md` §4.

## What changed and why
- `contabil/efactura/index.md`, secțiunea «Trimiterea la Vercon»: alineat nou despre clicul pe o factură trimisă (liniile doar de citit, butonul de trimitere stins, «Retrage de la Vercon» cât timp așteaptă Administratorul, bifarea automată după retragere, factura preluată doar se vede). Eticheta secțiunii: `00EF-68, 0000-183`.
- `tutorials/efactura-trimite-la-vercon.md`, secțiunea «Retragere sau schimbare»: aceeași explicație, pe scurt. Eticheta: `00EF-68, 0000-183`.
- `help-version.txt`: 2026-11-25.

## Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`, `src/KBot.App/HelpContent/tutorials/efactura-trimite-la-vercon.md`, `src/KBot.App/HelpContent/help-version.txt`.

## Test results
Nimic rulat (regula operatorului). `Check-Help.ps1` (fără `-Coverage`) arată doar erori vechi, niciuna despre aceste secțiuni.

## Unverified / deferred
- Nimic văzut pe ecran. Nicio captură nouă; tutorialul nu are pas nou (secțiunea «Retragere sau schimbare» e text, fără `target`).
