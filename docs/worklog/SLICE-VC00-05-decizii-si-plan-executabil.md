# SLICE-VC00-05 — registrul deciziilor si planul executabil

Data: 11.10.2026. Registru: [VERCON_STATUS](../../VERCON/VERCON_STATUS.md).

## Ce s-a schimbat si de ce

Registru care separa deciziile utilizatorului (neredeschise) de constatarile noi F1-F9 si de propunerile modelului (D03, D14, HALF_EVEN, domeniul Z, tratamentul coloanei TVA); starea D01-D12 dupa VC00; backlog independent de deciziile deschise. Registru de status separat `VERCON_STATUS.md` (pe modelul ADECHIT_STATUS) si trimitere in `KBOT_STATUS.md`.

## Fisiere atinse

- `VERCON/DECIZII_DESCHISE.md`, `VERCON/VERCON_STATUS.md`, `docs/worklog/KBOT_STATUS.md` (trimitere VERCON), cele 5 worklogs VC00

## Rezultate / probe

- Verificare documentara: toate linkurile relative din documentele noi verificate cu script (vezi sectiunea de verificare din status).

## Nevalidat sau amanat

- (Stare initiala; dupa runda 3 nu mai exista intrebari deschise cerute utilizatorului, vezi sectiunea de mai jos.)
- Documentul PLAN_IMPLEMENTARE.md nu a fost rescris; constatarile care il corecteaza (balante BF/AFB exclus tranzitiv; antet ca ajustare) sunt in DECIZII_DESCHISE.

## Revizie 11.10.2026

Registrul rescris: F2 retras (totalurile incap cu `Z=NR.ZEC`), D14 eliminat, D03 decis pe `Z = NR.ZEC`, D01/D10 reduse, D15 (ZEC in subunitate, rotunjirea celor 13 valori) si D16 (verificare ANAF in Furnizori) adaugate. Planul principal are corectiile in checkpoint.

## Revizie runda 3 (11.10.2026)

Registrul `DECIZII_DESCHISE.md` rescris dupa raspunsurile Q1-Q12; `CONTRACT_CALCULE.md`, `MAPARE_ACCESS_WEB.md`, `RAPOARTE.md`, `MANIFEST_SURSE.md`, `DICTIONAR_DATE.md`, `INVENTAR_OBIECTE.md` si `VERCON_STATUS.md` aliniate la sectiunea autoritara din `PLAN_IMPLEMENTARE.md`. Stare: documentat / verificat local (linkuri); nimic validat pe server.
