---
id: contabil.ddf.semnare
title: Semnarea documentului
part: contabil
order: 50
parent: contabil.ddf
keywords: semnare, semnatura, adobe, pdf, sectiunea a, sectiunea b, validez
---
Documentul se semnează în K-BOT, pe pagina **Document PDF** a vederii «Fundamentare». Pagina
arată documentul în Adobe, chiar în fereastra K-BOT.

<!-- capture: ddf-semnare | caption: Pagina «Document PDF», gata de semnare | goto: view:ddf | prepare: Alegeți o revizie în starea «Ciornă» și deschideți pagina «Document PDF». -->

## PDF-ul intermediar — semnătura A

1. În document apeși **«Validez antet și secțiunea A»**.
2. Semnezi în câmpul **Semnătura A** cu certificatul tău.
3. K-BOT salvează documentul semnat pe server. Revizia trece în **«Semnat A — gata de trimis»**.

Cât timp e semnat doar pe A, poți încă modifica revizia: se face un PDF intermediar nou, pe care
îl semnezi din nou.

## PDF-ul final — semnăturile A și B

După trimiterea în FOREXE, K-BOT face PDF-ul final, cu Secțiunea B și capturile. Îl semnezi din
nou, **de la capăt**: întâi **A**, apoi **«Validez secțiunea B»** și **B**. Semnătura A de pe
PDF-ul intermediar nu trece pe cel final.

## Dacă ceva nu merge

- Documentul semnat se păstrează pe calculator până ajunge pe server. Dacă încărcarea nu reușește,
  K-BOT o reîncearcă la următoarea pornire și îți spune ce a rămas neîncărcat.
- Dacă pe server există între timp **altă** versiune semnată, copia de pe calculator **nu** o
  înlocuiește; se arată versiunea de pe server.
- «Semnătura NU a fost aplicată» înseamnă că salvarea a fost oprită. Semnează din nou.
