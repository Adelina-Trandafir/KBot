---
id: contabil.forexe.lista
title: Lista de angajamente din FOREXE
part: contabil
order: 20
parent: contabil.forexe
keywords: actualizare lista, angajamente noi, sincronizare
---
<!-- slice: 0034, 0057, 0100, 0000-39, 0114-12, 0000-90, 0097-06, 0097-10 -->
Iconița **din dreapta, jos**, a listei de angajamente (cu textul «Actualizează» de lângă ea) deschide un meniu mic cu două rânduri.
Rândul **«Descarcă angajamente NOI»** aduce din FOREXE lista tuturor
angajamentelor unității (celălalt rând, «Actualizează angajamente VRAC», e în [Mai multe descărcări deodată](topic:contabil.forexe.descarcare-multipla)). Cât **Sursă/Sector** din bara de titlu e pe **«Toate»**, se aduc **toate sursele** unității;
cu o sursă anume aleasă, doar cele ale ei.

<!-- capture: lista-actualizare | caption: Iconița de actualizare a listei de angajamente și meniul ei | prepare: Apăsați iconița de actualizare din colțul de jos al listei de angajamente și țineți meniul cu cele două rânduri deschis. | redo: 2026-10-17 20:00 | why: 0114-06/14: ribbonul are in dreapta grupul machetei (langa «Inchide») si grupul vederii; meniul din subsolul listei are doua randuri; E-Factura sta sub Extrase in bara vederilor; 0114-05: arborii nu mai au iconita de descarcare a extraselor (cea cu cardul din lista angajamentelor a plecat; descarcarea e butonul din ribbon) -->

- Angajamentele **noi** se adaugă în listă (doar cu codul, descrierea și starea).
- Angajamentele **existente** rămân neatinse.
- La sfârșit K-BOT spune câte angajamente noi a găsit.

<!-- slice: 0097-10 -->
FOREXE așază cele mai noi angajamente **primele**. K-BOT citește lista de la început și se oprește la primul angajament pe care îl avea deja: nu mai trece prin cele descărcate înainte, așa că descărcarea e mai scurtă.
Dacă FOREXE oprește citirea cu o pagină de eroare, K-BOT păstrează ce a citit până atunci și scrie în consolă că citirea a fost incompletă; ce a rămas necitit se aduce la o altă descărcare.

Cu [descărcarea pe mai multe taburi](topic:contabil.forexe.descarcare-multipla) pornită, dacă lista a
adus angajamente noi, K-BOT întreabă și «Dorești actualizarea angajamentelor noi?».

Datele complete ale unui angajament (indicatori, rezervări, recepții, istoric) nu vin cu lista:
ele se aduc separat, pe fiecare angajament — [Descărcarea unui angajament](topic:contabil.forexe.descarcare).
