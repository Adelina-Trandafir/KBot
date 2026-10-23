---
id: contabil.ddf.nou
title: Angajament nou
part: contabil
order: 20
parent: contabil.ddf
keywords: angajament nou, creare angajament, revizia 0
open: menu:angajament_nou
---
<!-- slice: 0078-06, 0081-02, 0086, 0097-02, 0113, 0114, 0113-01, 0081-13 -->
Un angajament nou se face din ribbon: **Angajamente › Adăugare › «Angajament nou»**. Se deschide, ca fereastră peste
fereastra principală, editorul DDF pentru **revizia 0** a unui angajament care încă nu există în FOREXE.

> Cât timp există un angajament descărcat care **are rezervări, dar nu are încă DDF**, un angajament nou nu se poate crea: un mesaj
> îți arată care sunt angajamentele respective. Mai întâi adaugi DDF-ul lor din vederea **Rezervări**
> — [Adaugă rezervare](topic:contabil.ddf.rezervare).

> Butonul de lângă el, **«Creează în FOREXE»**, e pentru când vrei să faci
> angajamentul de mână, direct în pagina FOREXE —
> [Un angajament nou făcut direct în FOREXE](topic:contabil.forexe.browser).

<!-- capture: angajament-nou | caption: Editorul DDF pentru un angajament nou | goto: menu:angajament_nou | prepare: Completați obiectul și câteva rânduri în Secțiunea A, ca documentul să nu fie gol. | redo: 2026-10-23 12:00 | why: 0081-13: rândurile din Secțiunea A se adaugă în rândul gol al tabelului -->

1. Completezi antetul și Secțiunea A — [Editorul DDF](topic:contabil.ddf.editor).
2. **Salvezi** documentul. Angajamentul primește un cod provizoriu care începe cu «!».
3. Semnezi Secțiunea A pe PDF-ul intermediar — [Semnarea](topic:contabil.ddf.semnare).
4. **Trimiți în FOREXE**. Robotul creează angajamentul, iar codul provizoriu e înlocuit
   peste tot cu codul real din FOREXE — [Trimiterea](topic:contabil.ddf.trimitere).
5. Revizia rămâne în starea **«Trimis în FOREXE — în lucru»**. Din vederea **Rezervări**
   (subsolul arborelui, stânga) faci pe rând **Definitivează**, apoi **Derulează**, apoi
   **Generează PDF final** — [Definitivează, Derulează](topic:contabil.ddf.rezervare).
6. Semnezi PDF-ul final pe A și B. Urmează directorul.
