---
id: director.semnare
title: Semnarea ca director
part: director
order: 30
parent: director
screens: DdfView, DdfVizualizarePage, DdfDocumentPage, DdfFisierePage, OrdView
keywords: semnare, semnatura, ordonator, adobe, pdf, certificat, aprobat
---
<!-- slice: 0078, 0081-06, DIR1-01, 0000-141 -->
Pagina **Fundamentare** a unui angajament e aceeași vedere pe care o folosește și contabilul. Poți
răsfoi paginile ei (conținutul documentului, fișierele atașate), dar **nu poți modifica nimic**: la
orice comandă de modificare K-BOT răspunde «Cu rolul «Director» documentele se citesc și se
semnează. Modificările și lucrul cu FOREXE se fac din K-BOT-ul contabilului.»

<!-- capture: director-semnare | caption: Documentul deschis pe pagina «Document PDF», gata de semnătura directorului | prepare: Conectați-vă ca director, alegeți un angajament cu document semnat A și B și deschideți pagina «Document PDF». Faceți poza pe calculatorul directorului. -->

## Pașii
<!-- slice: 0078-06, 0081-06, DIR1-02, 0000-141 -->

1. Alege angajamentul în lista din stânga și deschide pagina **Fundamentare**.
2. Alege revizia care așteaptă semnătura (starea **«Semnat A și B — la director»**).
3. Deschide pagina **Document PDF**. Documentul apare în Adobe, chiar în fereastra K-BOT, cu
   semnăturile A și B deja puse.
4. Citește documentul. Dacă ceva nu e în regulă, **nu semna** — spune-i contabilului unității; el
   face o revizie nouă.
5. Semnează în câmpul semnăturii **ordonatorului de credite**, cu certificatul tău.
6. K-BOT salvează documentul semnat pe server. Revizia trece în **«Aprobat»**.

Semnăturile A și B nu se ating: semnătura ta se adaugă lângă ele.

## Unde ai voie să semnezi
<!-- slice: DIR1-03, 0000-141 -->

Directorul semnează **numai în câmpul ordonatorului**, atât pe fundamentare, cât și pe ordonanțare.
Dacă semnezi în alt câmp, K-BOT **nu trimite nimic pe server** și arată o eroare: «Semnătura NU a
fost trimisă pe server: ați semnat în alt câmp decât cel al ordonatorului». După ce apeși **OK**,
K-BOT închide singur documentul, șterge copia de pe calculator și îl aduce din nou, curat, de pe
server; poți semna atunci în câmpul ordonatorului. Notele de corecție CAB nu au câmp de ordonator, deci un
director le citește, dar nu le semnează.

## Dacă ceva nu merge
<!-- slice: 0078, 0081-06 -->

- Documentul semnat se păstrează pe calculator până ajunge pe server. Dacă încărcarea nu reușește,
  K-BOT o reîncearcă la următoarea pornire și îți spune ce a rămas neîncărcat.
- Dacă între timp pe server a ajuns **altă** versiune semnată, copia de pe calculator **nu** o
  înlocuiește; se arată versiunea de pe server.
- «Semnătura NU a fost aplicată» înseamnă că salvarea a fost oprită. Semnează din nou.
- Dacă documentul nu se deschide deloc, K-BOT spune «Documentul nu a putut fi deschis». Alege-l din
  nou; dacă mesajul rămâne, anunță administratorul K-BOT.
