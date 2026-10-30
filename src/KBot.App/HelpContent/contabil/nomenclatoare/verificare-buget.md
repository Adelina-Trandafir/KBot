---
id: contabil.nomenclatoare.verificare-buget
title: Verificare buget FOREXE
part: contabil
order: 10
parent: contabil.nomenclatoare.clasificatii
screens: BudgetCheckForm
keywords: verificare buget, verifica bugetul, credit bugetar, diferenta, forexe, buget k-bot, clasificatii cu diferente
---
<!-- slice: 0103-04, 0105, 0108, 0107, 0000-47, AV01-13 -->
Fereastra **«Verificare buget FOREXE»** pune față în față, pentru fiecare clasificație, bugetul din K-BOT și creditul bugetar raportat de FOREXE, ca să vezi unde nu se potrivesc. Ea doar arată; nu modifică nimic.

<!-- capture: verificare-buget | caption: Fereastra «Verificare buget FOREXE» | prepare: Din «Clasificații cheltuieli» apăsați «Verifică bugetul». Fereastra trebuie să arate cel puțin un rând cu diferență. -->

## Cum se deschide
<!-- slice: 0103-04, 0000-47, AV01-13 -->

- Din fereastra [Clasificații cheltuieli](topic:contabil.nomenclatoare.clasificatii), cu butonul **«Verifică bugetul»** din josul ei. Se deschide întotdeauna, chiar dacă totul se potrivește (atunci scrie asta în josul ferestrei).
- Numai așa: o descărcare din FOREXE nu o deschide singură.

## Ce arată
<!-- slice: 0103-04, 0105, 0108, 0107, 0000-47 -->

Un rând pe clasificație, cu coloanele **Clasificație**, **Denumire**, **SS** și trei valori:

- **Buget K-BOT** – totalul versiunii de buget în vigoare azi plus totalul rectificărilor ei (nu se mai oprește la trimestrul curent). Gol înseamnă că nu există buget K-BOT în vigoare azi;
- **Buget FOREXE** – creditul bugetar al clasificației, așa cum l-a raportat FOREXE la ultima descărcare. Este o singură valoare pe clasificație, la fel pentru toate angajamentele care o folosesc;
- **Diferență** – FOREXE minus K-BOT.

Se verifică doar clasificațiile **folosite în FOREXE**, adică cele pentru care FOREXE a raportat un credit. O clasificație care are valori în K-BOT, dar nu apare în FOREXE, nu se verifică și nu se arată. Se văd doar rândurile la care diferența nu este zero; în josul ferestrei scrie câte clasificații diferă din câte au fost verificate.

## Dublu clic pe un rând
<!-- slice: 0107, 0000-47, AV01-13 -->

**Dublu clic pe un rând** închide fereastra și alege în arbore clasificația respectivă, ca să vezi direct bugetul și rectificările ei. Dacă filtrele o ascundeau, arborele arată din nou toate clasificațiile. Dacă ai modificări nesalvate, K-BOT te întreabă întâi ce faci cu ele. Pentru a închide fereastra fără să alegi nimic, apasă **«Închide»**.
