---
id: contabil.panou
title: Panoul lateral
part: contabil
order: 22
parent: contabil
screens: AvacontMain.sidebar
keywords: panou lateral, panoul din stanga, machete deschise, machete recente, ultimele machete, redeschide, actiuni recente, ce am salvat, salvari, informatii, token e-factura, versiunea, rol, baza, perioada, erori, erori din sesiune, strange panoul
---
<!-- slice: 0115 -->
În stânga ferestrei principale, lângă **orice pagină** («Acasă», lista angajamentelor sau o machetă), stă panoul
lateral. Are patru secțiuni: **Machete deschise**, **Acțiuni recente**, **Informații** și **Erori din sesiune**.

<!-- capture: panou-lateral | caption: Panoul lateral, cu cele patru secțiuni desfăcute | prepare: Deschideți două-trei machete și o vedere a unui angajament, ca «Machete deschise» să aibă rânduri; apoi reveniți la «Acasă». Prindeți panoul pe toată înălțimea lui. -->

- Un clic pe **titlul unei secțiuni** o strânge sau o desface.
- Butonul din **bara de sus** a panoului îl strânge la o **bandă îngustă**, doar cu pictogramele secțiunilor; un clic
  pe o pictogramă (sau din nou pe buton) îl desface. K-BOT ține minte cum l-ai lăsat.
- Când secțiunile nu încap, **rotița mouse-ului** le derulează.

<!-- capture: panou-lateral-strans | caption: Panoul lateral strâns la banda cu pictograme | prepare: Strângeți panoul lateral cu butonul din bara lui de sus. După captură, desfaceți-l la loc. -->

## Machete deschise
<!-- slice: 0116 -->

Ultimele **10** machete pe care le-ai deschis **de la pornirea K-BOT**, cea mai recentă sus: ferestrele din ribbon
(Extrase de cont, Clasificații bugetare, Setări…), lista angajamentelor, dar și **vederile unui angajament** și
editoarele lui, cu codul angajamentului în titlu (de exemplu «Rezervări · cod»). Dacă o deschizi din nou, urcă sus,
nu apare de două ori.

Un **clic pe un rând** o deschide din nou:

- o machetă din ribbon se deschide ca de obicei;
- o vedere a unui angajament: K-BOT aduce lista angajamentelor pe ecran, trece pe **anul** ei dacă e din alt an,
  alege **angajamentul** în listă și deschide **vederea**;
- un editor de document (DDF sau ordonanțare) se redeschide în vederea **Fundamentare** sau **Ordonanțare** a
  angajamentului; de acolo deschizi documentul din nou. Ce nu ai salvat în editor nu se păstrează.

Dacă nu se poate, K-BOT spune de ce: anul nu mai e disponibil pentru unitate, angajamentul nu e în lista curentă
(ascuns sau din altă sursă/sector) sau vederea nu există pentru acel angajament.

Lista se **golește la închiderea K-BOT**. Un angajament nou, încă nesalvat, nu apare în ea.

## Acțiuni recente
<!-- slice: 0117 -->

Ultimele **10** lucruri **salvate de tine pe server**, pe unitatea pe care lucrezi, cel mai recent sus: textul
salvării și ora (doar ora pentru azi; ziua și ora pentru zilele dinainte). Ținând mouse-ul pe un rând vezi data și
ora întregi. Lista se actualizează singură, la câteva secunde după ce salvezi ceva. Cât nu e nimic de arătat, scrie
**«Nicio acțiune încă»**.

## Informații
<!-- slice: 0115 -->

| Rândul | Ce arată |
|--------|----------|
| **Operator** | utilizatorul conectat |
| **Rol** | rolul tău pe unitate |
| **Baza și perioada** | baza de date a unității, anul și sursa/sectorul alese |
| **FOREXE** | **Conectat** (cu numele certificatului), cu verde, sau **Neconectat**, cu portocaliu |
| **Activitate robot** | ce face robotul acum sau «În așteptare» |
| **Token E-Factura** | starea tokenului ANAF al unității: valabil până la o dată (verde), de reînnoit curând sau inexistent (portocaliu), expirat (roșu) — [Tokenul ANAF](topic:contabil.efactura.token) |
| **Versiunea K-BOT** | versiunea programului |

## Erori din sesiune
<!-- slice: 0115 -->

Ultimele **10** erori apărute **de la pornirea K-BOT**, cu ora. Un clic pe una arată detaliul ei într-o casetă;
textul întreg rămâne în jurnalul de erori — [Jurnal activitate](topic:contabil.jurnal). Cât nu a fost nicio eroare,
scrie **«Nicio eroare în această sesiune»**. Când ceri ajutor, spune și ora erorii de aici.
