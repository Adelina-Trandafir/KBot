---
id: contabil.asocieri
title: Asocierile recepțiilor — de ce există
part: contabil
order: 35
parent: contabil
screens: AsociereForm
keywords: asocieri, asociere, instantanee, instantaneu, lant, receptii, istoric, dubii, legaturi receptii, neasezate
---
<!-- slice: 0048-04, 0000-12 -->
Fereastra **Asocieri** este pasul cel mai important — și cel mai greu de înțeles — din tot ce faci
în K-BOT. **Nu e o listă de erori.** E locul în care spui, pentru fiecare recepție, **povestea ei în
timp**. De ea depind cifrele fiecărei ordonanțări.

## Două jumătăți care nu se întâlnesc
<!-- slice: 0048-03, 0000-12 -->

FOREXE îi dă lui K-BOT recepțiile pe două căi, și niciuna nu e întreagă:

| Ce vine din FOREXE | Ce știe | Ce NU știe |
|--------------------|---------|------------|
| **Lista recepțiilor** | fiecare recepție **așa cum e azi**: data, valoarea, indicatorii | cum a ajuns la valoarea asta |
| **Istoricul** | fiecare **salvare** a unei recepții, cu ora și valoarea din acel moment — un **instantaneu** | **a cărei recepții** e salvarea |

Istoricul FOREXE **nu scrie niciodată numele recepției**. Scrie doar: la ora cutare, o recepție a
ajuns la valoarea cutare, cu indicatorii cutare.

## Un exemplu
<!-- slice: 0064, 0000-12 -->

O recepție pornește la **1.000 lei** pe indicatorul AAB. Apoi cineva adaugă 100 pe AAB și un
indicator nou, AA2, cu 200. Mai târziu scoate 500 de pe AAB și tot AA2.

| Ora salvării | Ce scrie istoricul (instantaneul) |
|--------------|-----------------------------------|
| 10.01, 09:12 | AAB 1.000 · **total 1.000** |
| 15.01, 11:40 | AAB 1.100 · AA2 200 · **total 1.300** |
| 03.02, 14:05 | AAB 600 · AA2 0 · **total 600** |

- Valorile sunt **întregi, nu diferențe**: al doilea rând spune «1.100», nu «+100».
- Fiecare instantaneu numește **toți** indicatorii recepției, chiar și pe cei care n-au mișcat
  (sau au ajuns la 0).
- Azi recepția valorează 600. Cele trei instantanee, puse în ordinea orei, sunt **lanțul** ei.

## De ce trebuie să fie corect
<!-- slice: 0048-09, 0000-12 -->

Fiecare ordonanțare are nevoie de **totalul recepțiilor așa cum era în ziua plății**. Pentru o plată
din 20.01, recepția de mai sus contează cu 1.300, nu cu 600. Ca să afle asta, K-BOT merge înapoi pe
lanțul recepției până la data plății.

Un instantaneu pus pe **altă** recepție strică, fără niciun mesaj, cifrele (recepții, plăți
anterioare, rămas) ale **fiecărei** plăți de după el. Greșeala nu o prinde nimeni mai târziu: iese
la iveală, poate, peste luni, ca o reconciliere care nu se închide.

## De ce n-o poate face K-BOT singur
<!-- slice: 0048-07, 0058, 0065, 0000-12 -->

- **După valoare** nu se poate: mai multe recepții au adesea aceeași sumă.
- **După dată** nu se poate: ora instantaneului e ora salvării, iar data recepției e scrisă de mână
  pe site și se poate schimba oricând.
- **O salvare fără nicio schimbare** produce și ea un instantaneu întreg, cu aceleași cifre. Numărul
  instantaneelor nu spune câte schimbări au fost.

Singurul instantaneu pe care K-BOT îl poate recunoaște sigur e **ultimul** din lanț — cel care are
valoarea de azi a recepției. Pe celelalte le știi doar tu (sau documentele tale). De aceea, la un
angajament la care recepțiile s-au modificat des, **fereastra Asocieri va avea mereu de lucru**:
e mersul normal, nu o problemă.

## Când se deschide
<!-- slice: 0048-04, 0055 -->

- **După o descărcare** din FOREXE, dacă a rămas ceva nehotărât — fereastra «Așezarea recepțiilor
  descărcate». Vezi [Descărcarea unui angajament](topic:contabil.forexe.descarcare).
- **Oricând**, din vederea **Recepții**: iconița din dreapta, sus, a arborelui — fereastra
  «Legăturile recepțiilor».

Mai departe: [Fereastra, pe părți](topic:contabil.asocieri.fereastra) ·
[Cum așezi instantaneele](topic:contabil.asocieri.pasi) ·
[Cazuri speciale](topic:contabil.asocieri.cazuri).
