---
id: contabil.ddf.editor
title: Editorul DDF
part: contabil
order: 30
parent: contabil.ddf
screens: DdfEditForm, DdfEditSectiuneaAPage, DdfEditSectiuneaBPage, DdfEditDescrierePage, DdfEditFisierePage, DdfEditPartnersPage, DdfPartnersView, DdfEditLinieAForm
keywords: editor, antet, sectiunea a, sectiunea b, cual, revizie, clasificatie, partener, parteneri, asociere, mai multi parteneri, fisiere, salveaza
open: view:ddf
---
<!-- slice: 0051, 0081-02 -->
Editorul DDF se deschide la un angajament nou, la «Adaugă rezervare» și la «Modifică revizia».

<!-- capture: ddf-editor | caption: Editorul DDF, pagina «Secțiunea A» | prepare: Deschideți editorul DDF pe o revizie cu mai multe rânduri în Secțiunea A. -->

## Antetul
<!-- slice: 0081-09, 0081-10, 0094-02 -->

| Câmp | Ce e de știut |
|------|---------------|
| **CUAL** | numărul documentului; rezervat pe server cât timp editorul e deschis. Îl poți schimba, dacă numărul cerut e liber |
| **Data creării** | data documentului de fundamentare |
| **Obiectul documentului** | se scrie și în descrierea angajamentului (peste 255 de caractere se scurtează) |
| **Program** | programul documentului |
| **Compartiment** | alegi unul folosit pe documentele anterioare sau scrii altul |
| **Partener asociat** | bifat, leagă documentul de un partener principal, ales în câmpul de lângă; el se scrie pe toate rândurile din A și B. Pentru mai mulți parteneri vezi pagina «Parteneri» |
| **Număr revizie** | rezervat pe server; revizia inițială e 0 |
| **Data reviziei** | nu poate fi mai veche decât ultima revizie a angajamentului |
| **Descriere scurtă** | motivul revizuirii; descrierea lungă primește același text, rescris apoi pe pagina «Descriere» |
| **Total** | suma valorilor curente din Secțiunea A, recalculată la fiecare schimbare |

## Secțiunea A
<!-- slice: 0081-08, 0081-12, 0102 -->

- **Adaugă rând** deschide fereastra rândului nou:
  - **Sursă / sector** al rândului;
  - **Clasificația**: tastezi doar cifrele, punctele se pun singure. Clasificațiile deja folosite
    în angajament apar primele; cele deja puse în Secțiunea A nu mai apar;
  - **Element fundamentare** (obligatoriu): se completează cu denumirea clasificației, dar îl
    poți rescrie;
  - **Codul indicatorului**: al clasificației, dacă angajamentul îl are deja; altfel unul nou,
    «!» + trei caractere;
  - **Valorile**: tastezi doar **valoarea curentă**, obligatorie și diferită de 0. Se văd și
    disponibilul (buget − recepții) și valoarea rămasă. Pe rândurile aduse automat din rezervări,
    **bugetul** este cel pe care îl avea clasificația la data reviziei, din «Clasificații
    bugetare» (versiunea în vigoare plus rectificările, până la trimestrul acelei date), nu cel
    de azi din FOREXE. Dacă la data aceea clasificația nu are buget, se folosește creditul bugetar
    de azi și un mesaj te anunță. Pe un rând adăugat de tine cu «Adaugă rând», bugetul rămâne
    creditul bugetar de azi.
- **Șterge rândul** scoate linia; rândul pereche din Secțiunea B dispare odată cu ea.

<!-- capture: ddf-rand-nou | caption: Fereastra «Rând nou în secțiunea A» | prepare: Apăsați «Adaugă rând» în Secțiunea A și alegeți o clasificație. -->

## Secțiunea B, Descriere, Fișiere
<!-- slice: 0081-02, 0000-30 -->

- **Secțiunea B** se calculează din Secțiunea A și **nu se editează**. Pagina se vede abia după ce revizia a fost trimisă în FOREXE (din meniul reviziei, «Trimite în FOREXE»); până atunci butonul ei lipsește.
- **Descriere** are descrierea scurtă și pe cea lungă (starea de fapt și de drept).
- **Fișiere**: atașezi imagini, documente sau tabele. Se încarcă pe server după salvarea
  documentului. Capturile venite din FOREXE nu se pot șterge de aici, dar se pot salva pe disc.

## Parteneri
<!-- slice: 0094-02, 0084-02, 0000-28 -->
Pagina **Parteneri**, în dreapta barei cu pagini, arată toți partenerii asociați documentului.
Majoritatea documentelor au un singur partener și pentru ele ajunge câmpul din antet; pagina e
pentru documentele cu mai mulți.

- **Principal** e partenerul ales în antet. Apare mereu primul în listă și se schimbă din antet, nu
  de aici.
- Pentru încă un partener, îl alegi din listă (scrii începutul numelui sau al codului fiscal) și
  apeși **Asociază**. Un partener deja asociat nu mai apare în listă.
- **Scoate din asociere** scoate partenerul selectat; pe cel principal nu îl poți scoate.
- Lista se salvează odată cu documentul, cu **Salvează documentul**.
- Dacă debifezi «Partener asociat», partenerul principal dispare din listă; ceilalți rămân.

> Doar partenerul principal se scrie pe rândurile din secțiunile A și B. Ceilalți rămân asociați
> documentului.

Poți asocia parteneri și din vederea «Sumar» — [Sumar](topic:contabil.vederi.sumar).

## Salvarea
<!-- slice: 0051, 0081-12 -->

**Salvează documentul** trimite tot documentul într-o singură tranzacție. Rândurile din Secțiunea A
cu valoarea 0 se scot la salvare, după ce confirmi. Dacă nu rămâne niciun rând cu valoare,
documentul **nu** se salvează.

**Renunță** închide fără să salveze nimic; numerele rezervate se eliberează.
