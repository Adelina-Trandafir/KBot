---
id: contabil.panou
title: Panoul lateral
part: contabil
order: 22
parent: contabil
screens: AvacontMain.sidebar
keywords: panou lateral, panoul din stanga, machete deschise, machete recente, ultimele machete, redeschide, actiuni recente, ce am salvat, salvari, informatii, token e-factura, versiunea, rol, baza, perioada, erori, erori din sesiune, strange panoul
---
<!-- slice: 0115, 0115-01, 0115-02, 0000-106 -->
Panoul lateral are patru secțiuni: **Machete deschise**, **Acțiuni recente**, **Informații** și **Erori din sesiune**.

Implicit panoul stă **în spatele machetelor**: se vede în stânga ferestrei cât **nu e nicio machetă pe ecran** (pagina
«Acasă»), iar când deschizi o machetă — o pagină din ribbon sau **lista angajamentelor** — el trece în spatele ei și
macheta ocupă toată lățimea ferestrei. Ca panoul să se vadă **tot timpul**, **debifează** în **Setări › Aplicație ›
Fereastra principală** căsuța **«Meniul lateral stă în spatele machetelor»** (se aplică imediat). Cât panoul stă în
spatele machetelor, butonul **«Ascunde meniul lateral»** nu se mai afișează și panoul e mereu desfăcut (nu are rost să-l
strângi, când se vede doar cât nu e nicio machetă pe ecran); cu căsuța debifată, butonul și felul în care ai lăsat panoul
revin.

<!-- capture: panou-lateral | caption: Panoul lateral, cu cele patru secțiuni desfăcute | prepare: Deschideți două-trei machete și o vedere a unui angajament, ca «Machete deschise» să aibă rânduri; apoi reveniți la «Acasă». Prindeți panoul pe toată înălțimea lui. | redo: 2026-10-17 12:00 | why: 0115-01: butonul de strângere a coborât sub «Erori din sesiune»; 0113-01: «Machete deschise» e o stivă -->

- Un clic pe **titlul unei secțiuni** o strânge sau o desface.
- Butonul «Ascunde meniul lateral» din **partea de jos** a panoului (sub «Erori din sesiune», mereu lipit de marginea de jos, chiar și când secțiunile se derulează; cu aceeași pictogramă ca butonul care strânge lista
  din stânga) îl strânge la o **bandă îngustă**, doar cu pictogramele secțiunilor; un clic pe o pictogramă (sau din nou
  pe buton) îl desface. K-BOT ține minte cum l-ai lăsat, **și la următoarea pornire**.
- Cât panoul e strâns, **ținând mouse-ul pe o pictogramă** vezi, într-o etichetă, ce conține secțiunea ei (rândurile
  machetelor, acțiunilor, informațiilor sau erorilor).
- Când secțiunile nu încap, **rotița mouse-ului** le derulează.

<!-- capture: panou-lateral-strans | caption: Panoul lateral strâns la banda cu pictograme | prepare: Strângeți panoul lateral cu butonul din partea lui de jos. După captură, desfaceți-l la loc. | redo: 2026-10-17 12:00 | why: 0115-01: butonul a coborât jos -->

## Machete deschise
<!-- slice: 0116, 0113-01, 0115-01 -->

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

Ținând mouse-ul pe o machetă **deschisă**, apare o **poză** a ei, așa cum a rămas când ai deschis altceva (e o poză,
nu o fereastră vie). Pe o machetă **închisă** nu apare nimic.

Un **clic pe un rând** o aduce pe ecran sau o deschide din nou:

- o pagină deschisă revine în față, așa cum o lăsaseși;
- o machetă din ribbon, închisă, se deschide ca de obicei;
- lista angajamentelor revine pe ecran, cu angajamentul și vederea pe care le-ai lăsat.

Lista se **golește la închiderea K-BOT**.

## Acțiuni recente
<!-- slice: 0117, 0115-01 -->

Ultimele **10** lucruri **salvate de tine azi pe server**, pe unitatea pe care lucrezi, cel mai recent sus: textul
salvării și ora. Lista **începe din nou în fiecare zi**: la miezul nopții se golește și acțiunile de ieri sau mai vechi
nu se mai arată. Ținând mouse-ul pe un rând vezi data și ora întregi. Lista se actualizează singură, la câteva secunde după ce salvezi ceva. Cât nu e nimic de arătat, scrie
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
