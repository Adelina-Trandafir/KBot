---
id: contabil.ddf.rezervare
title: Adaugă rezervare, Definitivează, Derulează
part: contabil
order: 40
parent: contabil.ddf
keywords: adauga rezervare, definitiveaza, deruleaza, genereaza pdf final, revizie noua, indicatori existenti
---
Iconița din **stânga, jos, a arborelui din vederea Rezervări** deschide acțiunile documentului
de fundamentare pentru angajamentul selectat. Meniul arată **o singură** acțiune, cea potrivită
stării angajamentului; iconița e gri când nu e nimic de făcut.

<!-- capture: rezervari-meniu-ddf | caption: Meniul DDF din subsolul arborelui de rezervări | goto: view:rezervari | prepare: Selectați un angajament «În derulare» și apăsați iconița din stânga, jos, a arborelui. -->

| Angajamentul | Ultima revizie | Meniul oferă |
|--------------|----------------|--------------|
| nou, trimis, **Inițial** | revizia 0, «Trimis în FOREXE — în lucru» | **Definitivează** |
| nou, trimis, **În definitivare** | revizia 0, «Trimis în FOREXE — în lucru» | **Derulează** |
| nou, trimis, **În derulare** | revizia 0, «Trimis în FOREXE — în lucru» | **Generează PDF final** |
| **În derulare** | ultima revizie are PDF final | **Adaugă rezervare** |
| oricare | o revizie încă nefinalizată | nimic — termin-o întâi din vederea «Fundamentare» |
| anulat / reziliat / suspendat | — | nimic |

## Adaugă rezervare

Pornește o **revizie nouă** a documentului. Alegi:

1. **Adaugă revizie goală** — Secțiunea A pornește goală;
2. **Folosește indicatorii existenți** — Secțiunea A pornește cu câte un rând pentru fiecare
   indicator al angajamentului, cu valoarea 0. Completezi doar rândurile care se schimbă;
   cele rămase cu 0 se scot la salvare.

Apoi urmezi drumul obișnuit: salvare, semnare A, trimitere, semnare A și B.

## Definitivează și Derulează

Există **doar pentru un angajament nou**, creat în K-BOT, cât timp revizia lui 0 e «în lucru».
Fiecare pas se face în FOREXE de robot, iar capturile lui intră în documentul reviziei 0.
Abia după **Derulează** apare **Generează PDF final**, care închide revizia 0.

> K-BOT nu definitivează și nu derulează angajamente pe care nu le-a creat el.
