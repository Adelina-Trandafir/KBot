---
id: contabil.ddf.nou
title: Angajament nou
part: contabil
order: 20
parent: contabil.ddf
keywords: angajament nou, creare angajament, revizia 0
---
<!-- slice: 0078-06, 0081-02, 0086 -->
Un angajament nou se face din **MENIU › Angajament nou**. Se deschide editorul DDF pentru
**revizia 0** a unui angajament care încă nu există în FOREXE.

<!-- capture: angajament-nou | caption: Editorul DDF pentru un angajament nou | goto: menu:angajament_nou | prepare: Completați obiectul și câteva rânduri în Secțiunea A, ca documentul să nu fie gol. -->

1. Completezi antetul și Secțiunea A — [Editorul DDF](topic:contabil.ddf.editor).
2. **Salvezi** documentul. Angajamentul primește un cod provizoriu care începe cu «!».
3. Semnezi Secțiunea A pe PDF-ul intermediar — [Semnarea](topic:contabil.ddf.semnare).
4. **Trimiți în FOREXE**. Robotul creează angajamentul, iar codul provizoriu e înlocuit
   peste tot cu codul real din FOREXE — [Trimiterea](topic:contabil.ddf.trimitere).
5. Revizia rămâne în starea **«Trimis în FOREXE — în lucru»**. Din vederea **Rezervări**
   (subsolul arborelui, stânga) faci pe rând **Definitivează**, apoi **Derulează**, apoi
   **Generează PDF final** — [Definitivează, Derulează](topic:contabil.ddf.rezervare).
6. Semnezi PDF-ul final pe A și B. Urmează directorul.
