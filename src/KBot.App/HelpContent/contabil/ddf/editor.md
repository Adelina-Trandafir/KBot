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
<!-- slice: 0051, 0081-02, 0113, 0113-01 -->
Editorul DDF se deschide la un angajament nou, la «Adaugă rezervare» și la «Modifică revizia», ca **fereastră a lui**,
cu bara lui de titlu, mai mică decât spațiul de lucru, în mijlocul ferestrei principale. Cât e deschis nu poți lucra în
fereastra principală.

<!-- capture: ddf-editor | caption: Editorul DDF, pagina «Secțiunea A», cu rândul gol de la sfârșitul tabelului și butonul ✕ pe fiecare rând | prepare: Deschideți editorul DDF pe o revizie cu mai multe rânduri în Secțiunea A. | redo: 2026-10-23 12:00 | why: 0081-13: Secțiunea A nu mai are «Adaugă rând» / «Șterge rândul»; rândurile se adaugă în rândul gol al tabelului și se șterg cu ✕ -->

## Antetul
<!-- slice: 0081-09, 0081-10, 0094-02, 0094-03 -->

| Câmp | Ce e de știut |
|------|---------------|
| **CUAL** | numărul documentului; rezervat pe server cât timp editorul e deschis. Îl poți schimba, dacă numărul cerut e liber |
| **Data creării** | data documentului de fundamentare |
| **Obiectul documentului** | se scrie și în descrierea angajamentului (peste 255 de caractere se scurtează) |
| **Program** | programul documentului |
| **Compartiment** | alegi unul folosit pe documentele anterioare sau scrii altul |
| **Partener asociat** | bifat, leagă documentul de un partener principal, ales în câmpul de lângă; el se scrie pe toate rândurile din A și B. Pentru mai mulți parteneri vezi pagina «Parteneri», care apare doar cât timp bifa e pusă |
| **Număr revizie** | rezervat pe server; revizia inițială e 0 |
| **Data reviziei** | nu poate fi mai veche decât ultima revizie a angajamentului |
| **Descriere scurtă** | motivul revizuirii; descrierea lungă primește același text, rescris apoi pe pagina «Descriere» |
| **Total** | suma valorilor curente din Secțiunea A, recalculată la fiecare schimbare |

## Secțiunea A
<!-- slice: 0081-08, 0081-12, 0102, 0108, 0120, 0081-13, AV01-13 -->

Tabelul din **Secțiunea A** se completează direct în el. La sfârșitul tabelului este mereu **un rând gol**: aici începi un rând nou.

- **Rând nou**: în rândul gol, deschizi lista din celula **Clasificație** și alegi clasificația. Rândul se adaugă singur, iar dedesubt apare un nou rând gol. Cât timp antetul unui angajament nou nu e complet (programul, compartimentul, obiectul și, dacă e cazul, partenerul), celula rămâne blocată, iar sub tabel scrie ce lipsește.
- **Lista de clasificații** arată doar clasificațiile **programului** ales în antet, grupate pe sursă; fiecare grup se poate strânge sau desfășura. Clasificațiile deja folosite în angajament apar primele, iar cele deja puse în Secțiunea A nu mai apar. Poți și să tastezi cifrele clasificației, ca să o găsești mai repede.
- **Ce se completează singur** după alegere: bugetul, recepțiile, disponibilul (buget − recepții), valoarea precedentă și valoarea rămasă se văd imediat în rând. **Element fundamentare** (obligatoriu) primește denumirea clasificației, dar îl poți rescrie. Codul indicatorului este al clasificației, dacă angajamentul îl are deja; altfel se face unul nou, «!» + trei caractere.
- **Valoarea curentă**: după alegerea clasificației, cursorul trece direct pe ea. Este obligatorie și diferită de 0. Pe rândurile aduse automat din rezervări, **bugetul** este cel pe care îl avea clasificația la data reviziei, din «Clasificații cheltuieli» (totalul versiunii în vigoare plus rectificările), nu cel de azi din FOREXE. Dacă la data aceea clasificația nu are buget, se folosește creditul bugetar de azi și un mesaj te anunță. Pe un rând adăugat de tine, bugetul rămâne creditul bugetar de azi.
- **Altă clasificație** pe un rând existent: deschizi lista din celula **Clasificație** a rândului și alegi alta.
- **✕** de la capătul rândului îl scoate din tabel, după o confirmare; rândul pereche din Secțiunea B dispare odată cu el.

Cât e deschisă pagina **Secțiunea A**, rândul de jos al editorului arată, lângă **Salvează documentul**, câte clasificații mai sunt de ales sau ce lipsește din antet.

## Secțiunea B, Descriere, Fișiere
<!-- slice: 0081-02, 0000-30 -->

- **Secțiunea B** se calculează din Secțiunea A și **nu se editează**. Pagina se vede abia după ce revizia a fost trimisă în FOREXE (din meniul reviziei, «Trimite în FOREXE»); până atunci butonul ei lipsește.
- **Descriere** are descrierea scurtă și pe cea lungă (starea de fapt și de drept).
- **Fișiere**: atașezi imagini, documente sau tabele. Se încarcă pe server după salvarea
  documentului. Capturile venite din FOREXE nu se pot șterge de aici, dar se pot salva pe disc.

## Parteneri
<!-- slice: 0094-02, 0094-03, 0084-02, 0000-28 -->
Pagina **Parteneri**, în dreapta barei cu pagini, arată toți partenerii asociați documentului.
Apare doar când «Partener asociat» este bifat; debifat, pagina dispare din bara cu pagini (dacă
era deschisă, editorul trece la Secțiunea A).
Majoritatea documentelor au un singur partener și pentru ele ajunge câmpul din antet; pagina e
pentru documentele cu mai mulți.

- **Principal** e partenerul ales în antet. Apare mereu primul în listă și se schimbă din antet, nu
  de aici.
- Pentru încă un partener, îl alegi din listă (scrii începutul numelui sau al codului fiscal) și
  apeși **Asociază**. Un partener deja asociat nu mai apare în listă.
- **Scoate din asociere** scoate partenerul selectat; pe cel principal nu îl poți scoate.
- Lista se salvează odată cu documentul, cu **Salvează documentul**.
- Dacă debifezi «Partener asociat», partenerul principal dispare din listă; ceilalți rămân, iar
  pagina se ascunde până bifezi din nou.

> Doar partenerul principal se scrie pe rândurile din secțiunile A și B. Ceilalți rămân asociați
> documentului.

Poți asocia parteneri și din vederea «Sumar» — [Sumar](topic:contabil.vederi.sumar).

## Salvarea
<!-- slice: 0051, 0081-12, 0113, 0113-01 -->

**Salvează documentul** trimite tot documentul într-o singură tranzacție. Rândurile din Secțiunea A
cu valoarea 0 se scot la salvare, după ce confirmi. Dacă nu rămâne niciun rând cu valoare,
documentul **nu** se salvează.

**«Renunță»** închide editorul fără să salveze nimic; numerele rezervate se eliberează.
