---
id: contabil.asocieri.pasi
title: Cum așezi instantaneele, pas cu pas
part: contabil
order: 20
parent: contabil.asocieri
screens: AsociereForm.treeLibere, AsociereForm.gridLibere, AsociereForm.lblIntro, AsociereForm.btnSalveaza
keywords: trage, tragere, aseaza, desprinde, ctrl, shift, selectie multipla, ordinea, ora, salveaza legaturile, asezat automat, cum aleg receptia
---
<!-- slice: 0048-04 -->
## Gestul de bază: tragi
<!-- slice: 0048-04, 0061, 0107 -->

- **Pe o recepție** — tragi instantaneul din coș peste rândul recepției lui din stânga. Recepția se
  aprinde toată, iar în timpul tragerii se vede **unde ar cădea** în lanț.
- **Locul în lanț îl dă ora instantaneului**, nu tu: nu-l poți pune «înainte» sau «după» de mână.
- **Înapoi în coș** — tragi un instantaneu din lanț în dreapta, ca să-l desprinzi. Sau clic dreapta pe el
  (după ce l-ai ales cu clic stânga) › **«Desprinde de recepție»**.
- **Mai multe deodată** — cu **Ctrl** (câte unul) sau **Shift** (un șir) alegi mai multe instantanee
  din același loc și le tragi împreună; clicul dreapta lucrează atunci pe toate.

Nimic nu se scrie până nu apeși **«Salvează legăturile»**: până atunci poți muta cât vrei.

## Cum îți dai seama a cărei recepții e un instantaneu
<!-- slice: 0048-07, 0064, 0065, 0000-12 -->

Nu există o regulă sigură — de asta te întreabă K-BOT. Te ajută, în ordine:

1. **Valoarea.** Instantaneele unei recepții urcă și coboară împreună; ultimul trebuie să aibă
   valoarea de azi a recepției. Dacă o singură recepție are o anumită valoare, e de obicei a ei.
   Valoarea o dă recepția: un instantaneu **mai nou** decât ultimul din lanț trebuie să aibă
   **valoarea recepției**, altfel K-BOT refuză aruncarea.
2. **Indicatorii** (grila de sub arbore). Un indicator poate ajunge la 0, dar **nu poate dispărea**:
   dacă lanțul recepției avea AA2 mai devreme, un instantaneu mai târziu fără AA2 nu e al ei — K-BOT
   refuză așezarea cu «Instantaneul pierde indicatorii ..., prezenți mai devreme în lanțul recepției».
3. **Ora și descrierea** (ținând mouse-ul pe instantaneu). Salvările făcute în aceeași zi, la câteva
   minute distanță, sunt adesea aceeași recepție modificată de mai multe ori.
4. **Documentele tale** (facturile, procesele-verbale de recepție): data și suma lor.
5. **Graficul și reperele plăților.** După ce așezi, uită-te la etichetele plăților: diferența
   recepții – plăți trebuie să aibă sens.

## Ordinea de lucru recomandată
<!-- slice: 0000-12 -->

1. Alege în stânga **o recepție** și citește-i lanțul și valoarea de azi.
2. Caută în coș instantaneele cu valori pe drumul ei (vezi graficul «Recepția»).
3. Trage-le pe ea. Verifică semnul **«⚠ Lanțul nu se închide»**: după ce lanțul e complet, nu
   trebuie să mai apară.
4. Treci la recepția următoare.
5. Ce rămâne în coș: salvări fără nicio schimbare, recepții șterse înainte de prima descărcare —
   vezi [Cazuri speciale](topic:contabil.asocieri.cazuri).
6. Verifică reperele plăților, apoi **«Salvează legăturile»**.

## Așezările automate
<!-- slice: 0056, 0065 -->

- **După o descărcare**, serverul pune singur pe recepție ce poate recunoaște sigur (de obicei
  ultimul instantaneu al fiecărei recepții).
- **În fereastra deschisă oricând**, K-BOT așază singur instantaneele a căror valoare o are **o
  singură** recepție și spune: «N instantanee au fost așezate automat, pe singura recepție cu aceeași
  valoare. Verifică în arbore și apasă «Salvează» dacă e bine; altfel trage-le înapoi în coș.»

O așezare automată **poate fi greșită** (două recepții pot avea aceeași valoare în momente diferite).
Verifică-le ca pe ale tale — nimic nu se scrie până nu salvezi.

<!-- capture: asocieri-tragere | caption: Un instantaneu tras din coș peste recepția lui | goto: view:receptii | prepare: Deschideți Asocieri pe un angajament cu instantanee neașezate și începeți să trageți unul peste o recepție; faceți poza cât țineți mouse-ul apăsat. -->
