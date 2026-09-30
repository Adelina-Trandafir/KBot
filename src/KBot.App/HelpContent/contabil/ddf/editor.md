---
id: contabil.ddf.editor
title: Editorul DDF
part: contabil
order: 30
parent: contabil.ddf
screens: DdfEditForm, DdfEditSectiuneaAPage, DdfEditSectiuneaBPage, DdfEditDescrierePage, DdfEditFisierePage, DdfEditLinieAForm
keywords: editor, antet, sectiunea a, sectiunea b, cual, revizie, clasificatie, partener, fisiere, salveaza
---
Editorul DDF se deschide la un angajament nou, la «Adaugă rezervare» și la «Modifică revizia».

<!-- capture: ddf-editor | caption: Editorul DDF, pagina «Secțiunea A» | prepare: Deschideți editorul DDF pe o revizie cu mai multe rânduri în Secțiunea A. -->

## Antetul

| Câmp | Ce e de știut |
|------|---------------|
| **CUAL** | numărul documentului; rezervat pe server cât timp editorul e deschis. Îl poți schimba, dacă numărul cerut e liber |
| **Data creării** | data documentului de fundamentare |
| **Obiectul documentului** | se scrie și în descrierea angajamentului (peste 255 de caractere se scurtează) |
| **Program** | programul documentului |
| **Compartiment** | alegi unul folosit pe documentele anterioare sau scrii altul |
| **Partener asociat** | bifat, leagă tot documentul de un singur partener; partenerul se scrie pe toate rândurile din A și B |
| **Număr revizie** | rezervat pe server; revizia inițială e 0 |
| **Data reviziei** | nu poate fi mai veche decât ultima revizie a angajamentului |
| **Descriere scurtă** | motivul revizuirii; descrierea lungă primește același text, rescris apoi pe pagina «Descriere» |
| **Total** | suma valorilor curente din Secțiunea A, recalculată la fiecare schimbare |

## Secțiunea A

- **Adaugă rând** deschide fereastra rândului nou:
  - **Sursă / sector** al rândului;
  - **Clasificația**: tastezi doar cifrele, punctele se pun singure. Clasificațiile deja folosite
    în angajament apar primele; cele deja puse în Secțiunea A nu mai apar;
  - **Element fundamentare** (obligatoriu): se completează cu denumirea clasificației, dar îl
    poți rescrie;
  - **Codul indicatorului**: al clasificației, dacă angajamentul îl are deja; altfel unul nou,
    «!» + trei caractere;
  - **Valorile**: tastezi doar **valoarea curentă**, obligatorie și diferită de 0. Se văd și
    disponibilul (buget − recepții) și valoarea rămasă.
- **Șterge rândul** scoate linia; rândul pereche din Secțiunea B dispare odată cu ea.

<!-- capture: ddf-rand-nou | caption: Fereastra «Rând nou în secțiunea A» | prepare: Apăsați «Adaugă rând» în Secțiunea A și alegeți o clasificație. -->

## Secțiunea B, Descriere, Fișiere

- **Secțiunea B** se calculează din Secțiunea A și **nu se editează**.
- **Descriere** are descrierea scurtă și pe cea lungă (starea de fapt și de drept).
- **Fișiere**: atașezi imagini, documente sau tabele. Se încarcă pe server după salvarea
  documentului. Capturile venite din FOREXE nu se pot șterge de aici, dar se pot salva pe disc.

## Salvarea

**Salvează documentul** trimite tot documentul într-o singură tranzacție. Rândurile din Secțiunea A
cu valoarea 0 se scot la salvare, după ce confirmi. Dacă nu rămâne niciun rând cu valoare,
documentul **nu** se salvează.

**Renunță** închide fără să salveze nimic; numerele rezervate se eliberează.
