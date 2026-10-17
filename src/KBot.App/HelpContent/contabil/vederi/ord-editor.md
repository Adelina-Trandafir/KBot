---
id: contabil.ord.editor
title: Editorul de ordonanțare
part: contabil
order: 72
parent: contabil.vederi.ord
screens: OrdEditForm, OrdBeneficiariPage, OrdDocumentePage, OrdAtasamentePage, OrdTextForm
keywords: editor ordonantare, beneficiari, documente justificative, atasamente, imagini, salveaza ordonantarea, cod fiscal, iban
open: view:ord
---
<!-- slice: 0049, 0049-02, 0113, 0113-01 -->
Editorul **«Ordonanțare de plată»** se deschide, ca fereastră a lui, cu bara lui de titlu, mai mică decât spațiul de
lucru, la o ordonanțare nouă și la «Modifică ordonanțarea». Cât e deschis nu poți lucra în fereastra principală.

<!-- capture: ord-editor | caption: Editorul de ordonanțare, pagina «Beneficiari» | goto: view:ord | prepare: Clic dreapta pe o ordonanțare din arbore › «Modifică ordonanțarea». | redo: 2026-10-17 20:00 | why: 0114-06/14: ribbonul are in dreapta grupul machetei (langa «Inchide») si grupul vederii; meniul din subsolul listei are doua randuri; E-Factura sta sub Extrase in bara vederilor; 0114-05: arborii nu mai au iconita de descarcare a extraselor (cea cu cardul din lista angajamentelor a plecat; descarcarea e butonul din ribbon); 0113-01: editorul e din nou o fereastră cu bara lui de titlu și cu «Renunță», mai mică decât spațiul de lucru, nu o pagină a ferestrei principale -->

**Sus** e antetul: codul angajamentului, obiectul DDF, **numărul** (la o ordonanțare nouă: «se
alocă la salvare»), **data** și totalul. Data e cea care se scrie în document; plățile acoperite
rămân cele ale zilei pentru care s-a generat ordonanțarea.

Dacă la generare ceva lipsește (o clasificație, de exemplu) sau ziua are peste 25 de parteneri,
avertismentul apare sus, de la început.

## Beneficiari
<!-- slice: 0049, 0049-02 -->

- Lista din stânga: beneficiarii ordonanțării. Pentru cel ales vezi și completezi **denumirea,
  codul fiscal, contul IBAN și banca**.
- **Partener** — partenerul din nomenclator care corespunde beneficiarului; completează codul
  fiscal și contul, dacă sunt goale.
- Grila: rândurile de plată ale beneficiarului (clasificație, explicație, recepții, plăți
  anterioare, valoare, rămas), cu **TOTAL** jos.
- **«Grupează pe clasificații»** — bifat, lista din stânga arată clasificațiile, iar grila contul
  IBAN; nebifat, lista arată beneficiarii, iar grila codul SSI.

## Documente justificative
<!-- slice: 0049, 0049-02 -->

- Primul rând al listei, **«< TOȚI BENEFICIARII >»**, ține documentele comune întregii
  ordonanțări. Un beneficiar anume arată și documentele lui, și pe cele comune. Cu un singur
  beneficiar, rândul acesta lipsește.
- **«Adaugă rând»** cere textul documentului (de exemplu factura) și îl pune pe beneficiarul
  selectat; pe «< TOȚI BENEFICIARII >» textul se dă tuturor, câte o copie fiecăruia.
- **«Adaugă fișier»** anexează unul sau mai multe fișiere la beneficiarul selectat.
- **«Șterge rândul»** / **«Șterge fișierul»**. Un document comun nu se poate șterge de pe un
  beneficiar anume: se șterge de pe «< TOȚI BENEFICIARII >».

**Cel puțin un rând text trebuie să existe**, altfel ordonanțarea nu se poate salva.

## Atașamente
<!-- slice: 0049 -->

Imaginile atașate ordonanțării:

- **«Adaugă»** — alegi una sau mai multe imagini de pe disc;
- **«Lipește»** — pune imaginea din memoria temporară (de exemplu o captură de ecran copiată);
- **«Șterge»** — scoate imaginea; dispare de pe server la următoarea salvare.

Imaginile se încarcă pe server **după** salvarea ordonanțării.

## Salvarea
<!-- slice: 0049, 0049-02, 0113, 0113-01 -->

**«Salvează ordonanțarea»** verifică întâi tot și, dacă lipsește ceva, spune **toate** problemele
deodată: data, compartimentul, CUAL, denumirea / codul fiscal / contul unui beneficiar,
clasificația sau unitatea unui rând de plată, legătura cu documentul de fundamentare, rândul text.
Cu totul în regulă, K-BOT întreabă «Salvez datele?» și scrie ordonanțarea **dintr-o dată**: ori se
salvează toată, ori nimic.

Dacă o imagine nu se poate încărca după salvare, ordonanțarea **rămâne salvată** și K-BOT spune ce
imagine lipsește; o adaugi din nou cu «Modifică ordonanțarea».

**«Renunță»** închide editorul fără să salveze nimic.
