---
id: director.semnare
title: Semnarea ca director
part: director
order: 30
parent: director
screens: DirectorForm.pnlDocument, DdfView, DdfVizualizarePage, DdfDocumentPage, DdfFisierePage
keywords: semnare, semnatura, ordonator, adobe, pdf, certificat, aprobat
---
<!-- slice: 0078, 0081-06 -->
Documentul ales din listă se deschide în dreapta, în aceeași vedere pe care o folosește și
contabilul pentru documentele de fundamentare. Poți răsfoi paginile ei (conținutul documentului,
fișierele atașate), dar **nu poți modifica nimic**: la orice comandă de modificare K-BOT răspunde
«Aici documentul se semnează doar. Modificările se fac din K-BOT-ul unității.»

<!-- capture: director-semnare | caption: Documentul deschis pe pagina «Document PDF», gata de semnătura directorului | prepare: În fereastra directorului, selectați un document și deschideți pagina «Document PDF». Faceți poza pe calculatorul directorului și încărcați-o cu «Încarcă». -->

## Pașii
<!-- slice: 0078-06, 0081-06 -->

1. Selectează documentul din listă.
2. Deschide pagina **Document PDF**. Documentul apare în Adobe, chiar în fereastra K-BOT, cu
   semnăturile A și B deja puse.
3. Citește documentul. Dacă ceva nu e în regulă, **nu semna** — spune-i contabilului unității; el
   face o revizie nouă.
4. Semnează în câmpul semnăturii **ordonatorului de credite**, cu certificatul tău.
5. K-BOT salvează documentul semnat pe server. Revizia trece în **«Aprobat»**.
6. Apasă **«Reîncarcă lista»**: documentul semnat dispare din listă.

Semnăturile A și B nu se ating: semnătura ta se adaugă lângă ele.

## Dacă ceva nu merge
<!-- slice: 0078, 0081-06 -->

- Documentul semnat se păstrează pe calculator până ajunge pe server. Dacă încărcarea nu reușește,
  K-BOT o reîncearcă la următoarea pornire și îți spune ce a rămas neîncărcat.
- Dacă între timp pe server a ajuns **altă** versiune semnată, copia de pe calculator **nu** o
  înlocuiește; se arată versiunea de pe server.
- «Semnătura NU a fost aplicată» înseamnă că salvarea a fost oprită. Semnează din nou.
- Dacă documentul nu se deschide deloc, K-BOT spune «Documentul nu a putut fi deschis». Alege-l din
  nou; dacă mesajul rămâne, anunță administratorul K-BOT.
