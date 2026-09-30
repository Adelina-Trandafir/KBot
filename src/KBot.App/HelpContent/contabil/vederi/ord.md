---
id: contabil.vederi.ord
title: Ordonanțare
part: contabil
order: 70
parent: contabil.vederi
screens: OrdView, OrdVizualizarePage, OrdDocumentPage
keywords: ordonantare, ord, ordonantari de plata, document, pdf, semnare, genereaza, validare formular, anulare validare, alte avize, verificat avizat, cfp, control financiar preventiv, ordonator, compartiment de specialitate
---
Ordonanțările de plată ale angajamentului, cu documentul fiecăreia.

<!-- capture: ord | caption: Vederea «Ordonanțare» | goto: view:ord | prepare: Selectați un angajament care are ordonanțări, apoi o ordonanțare din arbore. -->

- **Arborele** listează ordonanțările pe luni, cu data și totalul fiecăreia.
- În dreapta, două pagini:
  - **Vizualizare** — rândurile ordonanțării alese (clasificație, descriere, recepții, plăți,
    valoare, rămas) și, pe fiecare beneficiar, codul fiscal, contul IBAN și documentele
    justificative. **«Caută Beneficiar»** te duce la un beneficiar anume.
  - **Document** — PDF-ul ordonanțării.

## Comenzile

**Clic dreapta** pe arbore deschide meniul:

| Comandă | Ce face |
|---------|---------|
| **Adaugă ordonanțare…** | o ordonanțare nouă, din plățile unei zile — [Ordonanțări noi](topic:contabil.ord.generare) |
| **Modifică ordonanțarea** | deschide ordonanțarea selectată în editor — [Editorul de ordonanțare](topic:contabil.ord.editor) |
| **Șterge ordonanțarea** | o șterge, după confirmare — [Ordonanțări noi](topic:contabil.ord.generare) |
| **Generare în lot…** | câte o ordonanțare pentru fiecare zi cu plăți neordonanțate — [Ordonanțări noi](topic:contabil.ord.generare) |

«Modifică» și «Șterge» apar doar când clicul e pe o ordonanțare. Iconița **«Adaugă»** din
subsolul arborelui face același lucru ca «Adaugă ordonanțare…».

## Documentul și semnarea

Pe pagina **Document**:

- Dacă ordonanțarea nu are încă PDF, apasă **«Generează»**: K-BOT face documentul din datele
  salvate.
- Documentul se semnează în Adobe, chiar în fereastra K-BOT. După fiecare semnătură, K-BOT îl
  urcă pe server.
- Dacă ordonanțarea are deja un PDF **semnat** pe server și generezi din nou, K-BOT te întreabă
  întâi: documentul nou e **nesemnat** și, odată semnat, **înlocuiește** versiunea de pe server.

<!-- capture: ord-semnaturi | caption: Semnăturile ordonanțării, în josul documentului | goto: view:ord | prepare: Selectați o ordonanțare semnată și deschideți pagina «Document»; derulați la semnături. -->


### Pașii în Adobe

Documentul se validează și se semnează **pe rând**. Butoanele **«Validare formular»** și
**«Anulare Validare»** sunt jos, în dreapta. Documentul făcut de K-BOT are **tot tabelul
completat**, deci nu scrii nimic în el: doar validezi și semnezi.

| Etapă | Cine | Ce face |
|-------|------|---------|
| 1 | compartimentul de specialitate | **«Validare formular»** → **semnătura 1**. Formularul spune apoi «Completați coloanele 1-3 ale tabelului...»: sunt deja completate, nu ai nimic de făcut. |
| 2 | persoana cu acces la sistemul de control al angajamentelor | **«Validare formular»** (acum se verifică tot tabelul) → **semnătura 2**. Poate fi altă persoană, pe alt calculator, altă zi. |
| 3 | CFP (dacă e cazul) | semnăturile 3 / 4 |
| 4 | ordonatorul de credite | **semnătura 5** — documentul e complet |

> **Semnătura 1 se pune doar pe un tabel complet.** Formularul Ministerului pare să permită
> completarea coloanelor 1-3 după semnătura 1, dar asta merge doar cât documentul rămâne deschis:
> la redeschidere coloanele sunt din nou blocate și goale, iar validarea dă erori ca «Nu ați
> completat coloana Cod SSI» sau «Nu se acceptă valori negative în coloana 5». Un astfel de document
> nu se mai poate repara — generează-l din nou din K-BOT.

Rostul celor două etape nu e completarea, ci **răspunderea**: prin semnătura 1 compartimentul de
specialitate răspunde de suma ordonanțată (coloana 4); prin semnătura 2 persoana nominalizată
răspunde de coloanele 1, 2, 3 și 5. Înainte de semnătura 2 apasă totuși **«Validare formular»**:
verificarea întregului tabel se face abia acum.

Dacă validarea găsește greșeli, le arată pe toate într-un singur mesaj. Corectează ordonanțarea în
K-BOT («Modifică ordonanțarea») și generează din nou documentul.

**«Anulare Validare»** readuce formularul la început (coloana 4 se poate edita din nou), dar
**doar cât nu e pusă nicio semnătură**. După prima semnătură răspunde «Am semnături aplicate, nu pot
anula validarea»: atunci generezi documentul din nou din K-BOT.

### Semnăturile

| Semnătura | Cine semnează | Obligatorie |
|-----------|---------------|-------------|
| 1. **Compartiment de specialitate** (stânga) | compartimentul care propune plata (coloana 4) | **da** |
| 2. **Compartiment de specialitate** (dreapta) — «În calitate de persoană nominalizată pentru a avea acces la sistemul de control al angajamentelor, răspund pentru corectitudinea informațiilor din col. 1, 2, 3 și 5» | persoana cu acces la sistemul de control al angajamentelor | **da** |
| **Alte Avize** (6 câmpuri) | alte avize cerute la voi; se pot pune între semnăturile 1 și 2 | nu |
| **Verificat/Avizat** (6 câmpuri) | se pot pune după semnătura 2, înainte de CFP propriu | nu |
| 3. **Control financiar preventiv propriu** | CFP propriu | nu |
| 4. **Control financiar preventiv delegat** | CFP delegat | nu |
| 5. **Ordonator de credite** — «Aprob cele prevăzute în prezentul document» | ordonatorul | **da** |

- **Semnătura 2 blochează tot documentul**: după ea nu se mai poate schimba nimic în tabel.
- **Semnătura ordonatorului închide semnăturile CFP.** Dacă documentul trebuie să aibă viza CFP,
  ea se pune **înainte** de semnătura ordonatorului.
- Documentul e **complet** cu semnăturile **1, 2 și 5**. Pe baza lui se poate face recepția în
  FOREXE.

> Ce cere Ministerul Finanțelor la trimiterea ordonanțării în FOREXE (transformarea ei în recepție
> și încărcarea pe serverul CAB) nu e descris încă în acest ajutor: K-BOT nu face deocamdată acest
> pas.
