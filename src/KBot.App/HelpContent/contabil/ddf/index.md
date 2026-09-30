---
id: contabil.ddf
title: Documentul de fundamentare (DDF)
part: contabil
order: 50
parent: contabil
screens: DdfView, DdfVizualizarePage, DdfDocumentPage, DdfFisierePage
keywords: ddf, fundamentare, revizie, stari, flux, trimitere, semnare
---
<!-- slice: 0020-02, 0081 -->
Documentul de fundamentare (DDF) justifică fiecare rezervare de credite a unui angajament. K-BOT
îl face, îl semnezi în K-BOT, iar K-BOT îl trimite în FOREXE și completează singur ce vine de acolo.

<!-- capture: ddf-vedere | caption: Vederea «Fundamentare» cu reviziile unui angajament | goto: view:ddf | prepare: Selectați un angajament cu cel puțin două revizii DDF și alegeți una în arbore. -->

## Drumul unei revizii
<!-- slice: 0078-06, 0081, 0000-14 -->

1. **Scrii revizia**: antetul și Secțiunea A (valorile, pe clasificații) — [Editorul DDF](topic:contabil.ddf.editor).
2. K-BOT face **PDF-ul intermediar** (fără Secțiunea B). Îl **semnezi pe Secțiunea A** — [Semnarea](topic:contabil.ddf.semnare).
3. **Trimiți în FOREXE** — [Trimiterea](topic:contabil.ddf.trimitere). Din acest moment revizia nu
   se mai poate modifica; orice schimbare de valori înseamnă o revizie nouă.
4. K-BOT pune **Secțiunea B** și **capturile de ecran** din FOREXE **în documentul semnat pe A** (nu
   face altul; semnătura A rămâne).
5. **Semnezi Secțiunea B**.
6. **Directorul** semnează ultimul, în K-BOT-ul lui.

## Stările unei revizii
<!-- slice: 0081-01, 0097, 0078-06, 0000-15 -->

Starea se vede la fiecare revizie din arborele vederii «Fundamentare».

| Stare | Ce înseamnă | Ce poți face |
|-------|-------------|--------------|
| **Ciornă** | revizia e scrisă, nesemnată | o modifici; o semnezi pe A |
| **Semnat A — gata de trimis** | PDF-ul intermediar e semnat pe A | **Trimite în FOREXE** (nu se mai modifică și nu se mai șterge) |
| **Trimitere întreruptă** | trimiterea s-a oprit la jumătate | **Reia trimiterea în FOREXE** |
| **Trimis în FOREXE — în lucru** | doar la un angajament nou: urmează Definitivează / Derulează | pașii din meniul Rezervărilor — [Definitivează, Derulează](topic:contabil.ddf.rezervare) |
| **PDF final — de semnat B** | FOREXE s-a terminat; Secțiunea B e pusă în documentul semnat pe A | semnezi B |
| **Semnat A și B — la director** | așteaptă semnătura directorului | nimic |
| **Aprobat** | directorul a semnat | capătul drumului |

> **O singură revizie deschisă pe angajament.** Cât timp o revizie nu are PDF final, nu se poate
> începe alta pe același angajament.

## Vederea «Fundamentare»
<!-- slice: 0020-02, 0033-02, 0081-04, 0097, 0000-14, 0078-08 -->

- **Arborele**: rădăcina **«Toate reviziile»** (clic pe ea = tot documentul), lunile, apoi
  reviziile. **Clic dreapta** pe o revizie oferă, după stare: «Trimite în FOREXE» / «Reia trimiterea
  în FOREXE», «Modifică revizia», «Șterge revizia», «Șterge documentul». Pe o lună: «Șterge TOATE
  reviziile lunii»; pe rădăcină: «Șterge documentul (TOATE reviziile)».
- **O revizie semnată** (fie și cu o singură semnătură) **nu se mai modifică și nu se mai șterge**:
  meniul ei păstrează doar «Trimite în FOREXE» / «Reia trimiterea», când starea o cere. Ștergerea
  unei luni sau a documentului întreg se oferă doar cât **nicio** revizie de sub ea nu e semnată.
- După o ștergere reușită nu mai apare niciun mesaj: ce s-a șters se scrie în jurnalul de mesaje
  (**Setări › Jurnal**).
- **Lupa** din capul arborelui caută o revizie; **butonul de strângere** din subsol îngustează
  arborele — [Arborii și tabelele](topic:contabil.liste).
- Paginile din dreapta: **Vizualizare** (valorile reviziei), **Document PDF** (documentul, unde
  se și semnează) și **Fișiere** (atașamentele, inclusiv capturile venite din FOREXE).
- Cât Adobe încă deschide documentul unei revizii (sau un fișier din «Fișiere»), arborele și lista
  de fișiere nu primesc alt rând — [Cât se deschide un document](topic:contabil.liste).
