---
id: avansat.foldere
title: Căi fișiere
part: avansat
order: 40
parent: avansat
screens: SetariFolder
keywords: foldere, cai, dosare, logs, extrase, temp, workflow, settings.json
open: setari:foldere
---
<!-- slice: 0072, 0072-02 -->
Pagina **Căi fișiere** din Setări arată folderele în care scrie K-BOT și îți permite să le muți.

<!-- capture: avansat-foldere | caption: Setări › Căi fișiere | goto: setari:foldere -->

| Folder | Ce ține | Implicit |
|--------|---------|----------|
| Logs | jurnalele aplicației (erori, Adobe, arbore, FOREXE) | `Logs` |
| Asociere | dosarele locale de asociere a recepțiilor, unul pe angajament | `Asociere` |
| WorkflowResults | ce aduce robotul la fiecare descărcare, neprelucrat | `WorkflowResults` |
| PDF de lucru | PDF-urile generate (golite la fiecare pornire) și copiile DDF / ORD descărcate de pe server | `C:\KBOT\Temp\PDF` |
| Workflows | fișierele cu pașii robotului — doar se citesc | `Workflows` |
| Extrase | extrasele de cont (SNM) descărcate din FOREXE, ca PDF | `Extrase` |

- Doar coloana **«Calea configurată»** se editează. **Calea goală înseamnă «implicit».**
- O cale relativă se socotește față de folderul aplicației (de obicei `C:\KBOT`).
- **Salvează** scrie căile. Folderele se verifică la pornire, deci schimbarea are efect **la
  următoarea pornire**.

> Dacă un folder configurat nu poate fi folosit (nu există și nu se poate crea, acces refuzat),
> K-BOT nu pornește și spune ce setare și ce cale trebuie corectate.
