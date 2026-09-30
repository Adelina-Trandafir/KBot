---
id: contabil.ddf.mf
title: Ce cere Ministerul Finanțelor la un DDF nou
part: contabil
order: 10
parent: contabil.ddf
keywords: ministerul finantelor, mf, omf 1140/2025, ghid alop, sectiunea a, sectiunea b, cod ssi, program, revizie, reguli
---
Documentul de fundamentare urmează formularul și ghidul Ministerului Finanțelor pentru
**OMF 1140/2025** (ghidul de utilizare ALOP). K-BOT completează formularul după regulile de mai
jos; le poți folosi ca să verifici un document înainte de semnare.

## Antetul

| Câmp | Regula |
|------|--------|
| Instituția publică, Cod de identificare fiscală | ale unității |
| Titlul (obiectul documentului) | obiectul cheltuielii |
| **Număr unic de înregistrare** | **nu se schimbă niciodată** pe toată viața cheltuielii |
| **Revizuirea** | 0 la primul document, apoi 1, 2, ... |
| **Data** | data reviziei |

## Secțiunea A

| Punct | Revizia 0 | Reviziile următoare |
|-------|-----------|---------------------|
| 1. Compartiment de specialitate | compartimentul care inițiază | neschimbat |
| 2. Descrierea pe scurt / motivul revizuirii | obiectul, pe scurt | **motivul revizuirii** |
| 3. Descrierea pe larg a stării de fapt și de drept | contextul și argumentele | ce s-a schimbat față de revizia precedentă |
| 4. Valoarea angajamentelor legale | tabelul **«Se stabilește ținând cont de:»** | același tabel |

**Tabelul de la punctul 4** are câte un rând pe fiecare cod SSI, plus TOTAL:

| Coloana | Regula |
|---------|--------|
| 1. Element de fundamentare | descriere scurtă |
| 2. Program | codul programului; **0000000000** pentru sectorul 02 (buget local) și pentru sursele fără programe |
| 3. Cod SSI | sector + sursă + clasificația funcțională + cea economică, **fără separatori** (de ex. `01A510103564801`) |
| 4. Parametrii de fundamentare | opțional |
| 5. Valoare totală revizie precedentă | revizia 0: **0**; revizia n: coloana 7 din revizia n-1 |
| 6. Influențe +/- | revizia 0: valoarea; revizia n: **nou − precedent** |
| 7. Valoarea totală actualizată | **5 + 6** |

**Punctul 5** nu poate rămâne gol (butonul de validare îl verifică). K-BOT bifează, ca în toate
exemplele din ghid, «în anul curent se anticipează emiterea a cel puțin unui angajament legal» și
«se sting în anul curent toate obligațiile de plată».

## Secțiunea B

Se bifează **«Propunerile de la secțiunea A au fost înregistrate în sistemul de control al
angajamentelor după cum urmează:»** și se completează un rând pe fiecare rând din FOREXE:
codul angajamentului, indicatorul, programul, codul SSI, apoi creditul de angajament (CA) și
creditul bugetar (CB) rezervate: valoarea dinainte, influența și valoarea nouă. Sub tabel vin
**capturile de ecran** din sistemul de control al angajamentelor.

K-BOT completează Secțiunea B **singur**, după trimiterea în FOREXE; tu n-o scrii niciodată.

## A și B trebuie să se potrivească

Pentru fiecare cod SSI:

- valoarea precedentă din A (col. 5) = CA precedent din B = CB precedent din B;
- influența din A (col. 6) = influența CA = influența CB;
- valoarea actualizată din A (col. 7) = CA actualizat = CB actualizat.

> **Înainte de o revizie nouă**, valoarea precedentă din A trebuie să fie egală cu valoarea
> rezervată acum în FOREXE. Altfel revizia e greșită, iar ghidul spune că documentul se
> restituie. K-BOT ia valorile precedente din ultima revizie și din FOREXE, care sunt mereu la fel
> cât timp angajamentul se lucrează doar prin K-BOT.

## Trei situații tipice

| Situația | Secțiunea A | În FOREXE |
|----------|-------------|-----------|
| **Angajament nou** (revizia 0) | col. 5 = 0; col. 6 = col. 7 = valoarea | se creează angajamentul; dacă angajamentul legal e deja semnat (de ex. salarii), se face și definitivarea / derularea |
| **Modificarea sumei** (revizia 1, 2...) | col. 5 = valoarea precedentă; col. 6 = diferența; col. 7 = valoarea nouă | se modifică rezervarea; un cod SSI nou devine un rând nou |
| **Rest de plată din anii anteriori** | revizia 0 a unui DDF nou; col. 5 = 0; col. 6 = col. 7 = restul neplătit | se modifică doar partea de CB; CA rămâne 0 |
