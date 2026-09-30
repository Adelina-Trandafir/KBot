---
id: contabil.ddf
title: Documentul de fundamentare (DDF)
part: contabil
order: 50
parent: contabil
screens: DdfView, DdfVizualizarePage, DdfDocumentPage, DdfFisierePage
keywords: ddf, fundamentare, revizie, stari, flux, trimitere, semnare
---
Documentul de fundamentare (DDF) justifică fiecare rezervare de credite a unui angajament. K-BOT
îl face, îl semnezi în K-BOT, iar K-BOT îl trimite în FOREXE și completează singur ce vine de acolo.

<!-- capture: ddf-vedere | caption: Vederea «Fundamentare» cu reviziile unui angajament | goto: view:ddf | prepare: Selectați un angajament cu cel puțin două revizii DDF și alegeți una în arbore. -->

## Drumul unei revizii

1. **Scrii revizia**: antetul și Secțiunea A (valorile, pe clasificații) — [Editorul DDF](topic:contabil.ddf.editor).
2. K-BOT face **PDF-ul intermediar** (fără Secțiunea B). Îl **semnezi pe Secțiunea A** — [Semnarea](topic:contabil.ddf.semnare).
3. **Trimiți în FOREXE** — [Trimiterea](topic:contabil.ddf.trimitere). Din acest moment revizia nu
   se mai poate modifica; orice schimbare de valori înseamnă o revizie nouă.
4. K-BOT completează **Secțiunea B** și **capturile de ecran** din FOREXE și face **PDF-ul final**.
5. **Semnezi PDF-ul final** pe Secțiunile A și B.
6. **Directorul** semnează ultimul, în K-BOT-ul lui.

## Stările unei revizii

Starea se vede la fiecare revizie din arborele vederii «Fundamentare».

| Stare | Ce înseamnă | Ce poți face |
|-------|-------------|--------------|
| **Ciornă** | revizia e scrisă, nesemnată | o modifici; o semnezi pe A |
| **Semnat A — gata de trimis** | PDF-ul intermediar e semnat pe A | **Trimite în FOREXE**; sau o modifici (semnătura se pierde, înapoi la Ciornă) |
| **Trimitere întreruptă** | trimiterea s-a oprit la jumătate | **Reia trimiterea în FOREXE** |
| **Trimis în FOREXE — în lucru** | doar la un angajament nou: urmează Definitivează / Derulează | pașii din meniul Rezervărilor — [Definitivează, Derulează](topic:contabil.ddf.rezervare) |
| **PDF final — de semnat A și B** | FOREXE s-a terminat, PDF-ul final e gata | semnezi A și B |
| **Semnat A și B — la director** | așteaptă semnătura directorului | nimic |
| **Aprobat** | directorul a semnat | capătul drumului |

> **O singură revizie deschisă pe angajament.** Cât timp o revizie nu are PDF final, nu se poate
> începe alta pe același angajament.

## Vederea «Fundamentare»

- **Arborele**: lunile, apoi reviziile. **Clic dreapta** pe o revizie oferă, după stare:
  «Trimite în FOREXE» / «Reia trimiterea în FOREXE», «Modifică revizia», «Șterge revizia»,
  «Șterge documentul». Pe o lună: «Șterge TOATE reviziile lunii».
- Paginile din dreapta: **Vizualizare** (valorile reviziei), **Document PDF** (documentul, unde
  se și semnează) și **Fișiere** (atașamentele, inclusiv capturile venite din FOREXE).
