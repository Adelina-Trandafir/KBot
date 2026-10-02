---
id: contabil.nomenclatoare.clasificatii
title: Clasificații bugetare
part: contabil
order: 10
parent: contabil.nomenclatoare
screens: ClasificatiiForm, ClasificatiiAddForm
keywords: clasificatii, clasificatie bugetara, indicatori, buget, trimestre, rectificari, capitol, subcapitol, articol, alineat
open: menu:clasificatii
---
<!-- slice: 0087, 0075-00, 0102 -->
<!-- capture: clasificatii | caption: Fereastra «Clasificații bugetare» | goto: menu:clasificatii | prepare: Alegeți în arbore un alineat care are buget și rectificări. | redo: 2026-10-02 16:46 | why: 0102: bugetul pe versiuni cu «Început», fără total -->

- **Arborele** din stânga: Capitol › Subcapitol › Articol › Alineat, cu denumirile alături.
- În dreapta, pentru clasificația aleasă:
  - **bugetul** anului, pe **versiuni**: fiecare rând este bugetul de la data din coloana
    **«Început»** încolo, pe trimestre (Trim. 1–4). Nu există total, pentru că un buget nu se
    adună pe an. Semnul **«+»** din josul grilei adaugă o versiune, **«✕»** o șterge;
  - **rectificările**: numărul și data documentului, plus trimestrul (sau trimestrele) pe care le
    schimbă. Ele nu sunt buget, dar îl influențează. Semnul **«+»** din josul grilei adaugă o
    rectificare, completată direct în tabel.
- **Salvează** scrie versiunile de buget și rectificările. Dacă treci pe alt nod sau închizi
  fereastra cu modificări nesalvate, K-BOT te întreabă ce faci cu ele.

> Data de început a unei versiuni și data unei rectificări trebuie să fie în anul de lucru. Două
> versiuni ale aceleiași clasificații nu pot începe în aceeași zi.

## Cum se folosește bugetul în documentul de fundamentare
<!-- slice: 0102 -->

Documentul de fundamentare arată la «Buget» ce buget avea clasificația **la data reviziei**, nu cel
de azi: versiunea cu cea mai mare dată de început până la acea zi, plus rectificările de la data ei
de început până la data reviziei, adunate de la trimestrul 1 până la trimestrul zilei reviziei.

Ca să iasă corect o revizie din ianuarie – martie, adaugă pe clasificație o versiune care începe
la **01.01** a anului. Dacă la data reviziei nu există nicio versiune, documentul folosește creditul
bugetar de azi din FOREXE și te anunță pe ecran care clasificații sunt în cazul acesta.

## Clasificații noi
<!-- slice: 0087 -->

Semnul **«+»** din josul arborelui deschide fereastra de adăugare: bifezi sursele / sectoarele,
apoi clasificațiile funcționale și economice. Se pot alege doar surse / sectoare pe care unitatea
le are deja. Clasificațiile care există deja sunt sărite și numărate.
