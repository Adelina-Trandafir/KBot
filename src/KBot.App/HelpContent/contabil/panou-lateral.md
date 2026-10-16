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

<!-- capture: panou-lateral | caption: Panoul lateral, cu cele patru secțiuni desfăcute | prepare: Deschideți două-trei machete și o vedere a unui angajament, ca «Machete deschise» să aibă rânduri; apoi reveniți la «Acasă». Prindeți panoul pe toată înălțimea lui. | redo: 2026-10-16 12:00 | why: 0113-01: «Machete deschise» e o stivă — deschisele sus, închisele în italice jos -->

- Un clic pe **titlul unei secțiuni** o strânge sau o desface.
- Butonul din **bara de sus** a panoului îl strânge la o **bandă îngustă**, doar cu pictogramele secțiunilor; un clic
  pe o pictogramă (sau din nou pe buton) îl desface. K-BOT ține minte cum l-ai lăsat.
- Când secțiunile nu încap, **rotița mouse-ului** le derulează.

<!-- capture: panou-lateral-strans | caption: Panoul lateral strâns la banda cu pictograme | prepare: Strângeți panoul lateral cu butonul din bara lui de sus. După captură, desfaceți-l la loc. -->

## Machete deschise
<!-- slice: 0116, 0113-01 -->

Până la **10** rânduri, de la pornirea K-BOT, ca o **stivă**:

- sus sunt machetele **deschise acum**, în ordinea în care le-ai deschis. **Ordinea nu se schimbă** cât timp rămân
  deschise: dacă treci de la una la alta, rândurile stau pe loc. O machetă nouă se adaugă după ultima deschisă;
- machetele **închise** sunt scrise **cu litere italice** și stau jos, în coadă; cea închisă ultima e la capăt. Dacă
  deschizi din nou una închisă, ea iese din coadă și se pune după ultima deschisă;
- când lista e plină, ies din ea cele **închise**, începând cu cea închisă de cel mai mult timp, ca machetele deschise
  să rămână vizibile cât mai mult.

În listă apar paginile din ribbon (Extrase de cont, Clasificații bugetare, Setări…) și **lista angajamentelor**.
**Vederile unui angajament** (Sumar, Rezervări…) **nu apar**, și nici **ferestrele care cer un răspuns** (editoarele DDF și
de ordonanțare, Asocieri, Operațiuni necorelate, graficul rezervărilor, Grupele).

Un **clic pe un rând** o aduce pe ecran sau o deschide din nou:

- o pagină deschisă revine în față, așa cum o lăsaseși;
- o machetă din ribbon, închisă, se deschide ca de obicei;
- lista angajamentelor revine pe ecran, cu angajamentul și vederea pe care le-ai lăsat.

Lista se **golește la închiderea K-BOT**.

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
