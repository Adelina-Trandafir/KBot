# SLICE-0000-107 — ajutor pentru tutorialul de pornire (000T-13)

## What changed and why

Ajutorul urmează schimbarea din [000T-13](SLICE-000T-07-tutorial-initial.md#slice-000t-13): la pornire K-BOT nu mai arată turul ferestrei principale și tutorialul «Ce sunt tutorialele», ci tutorialul «Primii pași în K-BOT», cu o singură bifă în Setări › Aplicație › Generale.

- `contabil/ajutor.md`: «Tururile ghidate» (turul nu mai pornește singur) și «Tutorialul de început» (rescrisă: cei 11 pași, «Mă opresc», caseta de pe bulă, cum se oprește / pornește).
- `contabil/setari.md`: rândul grupului «Fereastra principală» și bifa «Arată tutorialul de început la pornirea K-BOT» (bifa turului a plecat).
- Etichete `000T-13, 0000-107` pe secțiunile atinse. Fără capturi noi.

## Files touched

`src/KBot.App/HelpContent/contabil/ajutor.md`, `setari.md`.

## Test results

`Check-Help.ps1 -Coverage`: nicio eroare despre aceste fișiere. Nimic văzut pe ecran.

## Left unverified or deferred

Tutorialul `setare-tutorial-initial` (cum pornești / oprești tutorialul de început) a fost șters la cererea operatorului, tot 18.10.2026.
