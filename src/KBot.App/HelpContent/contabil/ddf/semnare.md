---
id: contabil.ddf.semnare
title: Semnarea documentului
part: contabil
order: 50
parent: contabil.ddf
keywords: semnare, semnatura, adobe, pdf, sectiunea a, sectiunea b, validez
---
<!-- slice: 0078 -->
Documentul se semnează în K-BOT, pe pagina **Document PDF** a vederii «Fundamentare». Pagina
arată documentul în Adobe, chiar în fereastra K-BOT.

<!-- capture: ddf-semnare | caption: Pagina «Document PDF», gata de semnare | goto: view:ddf | prepare: Alegeți o revizie în starea «Ciornă» și deschideți pagina «Document PDF». -->

## PDF-ul intermediar — semnătura A
<!-- slice: 0078, 0081-02, 0097 -->

1. În document apeși **«Validez antet și secțiunea A»**.
2. Semnezi în câmpul **Semnătura A** cu certificatul tău.
3. K-BOT salvează documentul semnat pe server. Revizia trece în **«Semnat A — gata de trimis»**.

După semnătură revizia **nu se mai modifică** și nu se mai șterge: urmează trimiterea în FOREXE.
Verifică deci valorile **înainte** să semnezi.

## După trimitere — semnătura B
<!-- slice: 0078-04, 0078-06, 0081-05, 0000-14 -->

După trimiterea în FOREXE, K-BOT **nu face alt document**: pune **Secțiunea B** (codul angajamentului
și indicatorii din FOREXE) și **capturile** în documentul **deja semnat pe A**. Semnătura A rămâne
neatinsă, iar K-BOT îți spune câte rânduri și capturi a pus.

1. Deschide revizia, pe pagina **Document PDF**.
2. Apasă **«Validez secțiunea B»** și semnează în câmpul **Semnătura B**.
3. La semnare documentul se salvează pe server; revizia trece în **«Semnat A și B — la director»**.
   Fără semnătură nu se salvează nimic.

Dacă Secțiunea B nu are încă, pe server, codurile din FOREXE, K-BOT arată documentul semnat pe A și
îți spune că B se completează după ce trimiterea le-a salvat.

> **Un document semnat nu se mai generează niciodată din nou** — nici ordonanțarea, nici documentul
> de fundamentare. Rămâne așa cum a fost semnat; ce mai trebuie adăugat se pune în el.

## Dacă ceva nu merge
<!-- slice: 0078, 0079 -->

- Documentul semnat se păstrează pe calculator până ajunge pe server. Dacă încărcarea nu reușește,
  K-BOT o reîncearcă la următoarea pornire și îți spune ce a rămas neîncărcat.
- Dacă pe server există între timp **altă** versiune semnată, copia de pe calculator **nu** o
  înlocuiește; se arată versiunea de pe server.
- «Semnătura NU a fost aplicată» înseamnă că salvarea a fost oprită. Semnează din nou.
