---
id: ordonantare-din-plata
title: Adaugă o ordonanțare pe baza unei plăți
part: contabil
keywords: ordonantare ordonantari plata plati plus adaug creez nou beneficiari clasificatii documente justificative atasamente imagini fisier salvez
starts: KbotForm
host-key: plati-ordonantare
---

<!-- slice: 000T -->

## Primul pas

<!-- slice: 000T -->

Vei face o ordonanțare dintr-o plată existentă în sistemul FOREXE: o alegi din vederea «Plăți», completezi documentele ei justificative și o salvezi.<BR>
Pentru a putea completa acest tutorial este necesară descărcarea extraselor la zi.

## Alege un angajament

<!-- slice: 000T -->

target: KbotForm.tree
wait: select
Alege din lista de angajamente unul care are plăți încă neordonanțate.

## Deschide vederea «Plăți»

<!-- slice: 000T -->

target: KbotForm.navViews
part: item:plati
wait: tab:plati
Apasă butonul «Plăți» din stânga ferestrei.

## Apasă «+»

<!-- slice: 000T -->

target: PlatiView.tree
part: node.icon
wait: click
Semnul «+» apare în dreapta zilelor care au plăți neordonanțate. Apasă «+» pe ziua pentru care vrei ordonanțarea. Dacă nu îl vezi, deschide luna în care se află ziua. Apăsat pe o lună, «+» face câte o ordonanțare pentru fiecare zi a ei, fără să deschidă editorul: pentru acest tutorial alege o zi.

## Se deschide ordonanțarea

<!-- slice: 000T -->

wait: opens:OrdEditForm
Așteaptă puțin: K-BOT pregătește ordonanțarea din plățile zilei și o deschide într-o fereastră nouă, începând cu pagina «Beneficiari».

## Bifează «Grupează pe clasificații»

<!-- slice: 000T -->

target: OrdBeneficiariPage.chkClsf
wait: checked
optional: yes
why: Bifa schimbă doar felul în care vezi lista, nu ce se salvează; ordonanțarea se poate salva și fără ea.
Dacă vrei să vezi lista pe ordonată după clasificații bifează «Grupează pe clasificații». Nebifat, vezi beneficiarii și codul SSI al fiecăruia (așa cum se vede acum).

## Fila «Documente»

<!-- slice: 000T -->

target: OrdEditForm.navSub
part: item:documente
wait: tab:documente
optional: yes
why: Poți sări peste ea dacă ordonanțarea are deja un rând text de document justificativ; fără cel puțin un asemenea rând, ordonanțarea nu se poate salva.
Pentru Ordonanțarea creată din Plăți existente, fiecare Beneficiar primește automat un Document Justificativ în baza explicației din OP. Dacă vrei să adaugi alte documente justificative, deschide fila «Documente».

## Apasă «Adaugă rând»

<!-- slice: 000T -->

target: OrdDocumentePage.btnAdaugaText
wait: click
optional: yes
why: Rândul text se adaugă doar dacă ordonanțarea nu are încă unul; cel puțin un rând text trebuie să existe ca ordonanțarea să se poată salva.
Apasă «Adaugă rând» ca să adaugi textul unui document justificativ (de exemplu factura). Textul se pune pe beneficiarul selectat; pe rândul «< TOȚI BENEFICIARII >» se dă tuturor beneficiarilor, câte o copie fiecăruia.

## Se deschide fereastra documentului

<!-- slice: 000T -->

wait: opens:OrdTextForm
optional: yes
why: Fereastra se deschide doar dacă ai apăsat «Adaugă rând».
Se deschide o fereastră mică, în care scrii textul documentului justificativ.

## Scrie textul documentului

<!-- slice: 000T -->

target: OrdTextForm.txtDoc
wait: changed
optional: yes
why: Fără text nu se adaugă niciun rând; sari peste doar dacă ai renunțat la «Adaugă rând».
Scrie textul documentului justificativ, de exemplu «Factura nr. 123 din 05.10.2026», apoi apasă Enter sau treci în alt câmp.

## Apasă «Adaugă»

<!-- slice: 000T -->

target: OrdTextForm.btnOk
wait: click
optional: yes
why: Rândul text apare în listă doar după ce apeși «Adaugă»; sari peste dacă ai renunțat la «Adaugă rând».
Apasă «Adaugă». Rândul apare în lista documentelor justificative.

## Apasă «Adaugă fișier»

<!-- slice: 000T -->

target: OrdDocumentePage.btnAdaugaFisier
wait: click
optional: yes
why: Anexezi fișiere doar dacă ai documente care trebuie să însoțească ordonanțarea.
Apasă «Adaugă fișier» și alege de pe disc unul sau mai multe fișiere. Se anexează la beneficiarul selectat.

## Fila «Atașamente»

<!-- slice: 000T -->

target: OrdEditForm.navSub
part: item:atasamente
wait: tab:atasamente
optional: yes
why: Imaginile atașate nu sunt obligatorii.
Deschide fila «Atașamente», cu imaginile ordonanțării.

## Apasă «Adaugă»

<!-- slice: 000T -->

target: OrdAtasamentePage.btnAdauga
wait: click
optional: yes
why: Atașezi imagini doar dacă ai ce să anexezi ordonanțării.
Apasă «Adaugă» și alege de pe disc una sau mai multe imagini. Se încarcă pe server după ce salvezi ordonanțarea.

## Salvează ordonanțarea

<!-- slice: 000T -->

target: OrdEditForm.btnSalveaza
wait: click
guard: yes
Apasă «Salvează ordonanțarea». Pașii opționali de mai sus se pot sări cu «Sari peste». K-BOT verifică întâi tot și spune dintr-o dată ce lipsește; dacă totul e în regulă, te întreabă «Salvez datele?» și scrie ordonanțarea dintr-o singură mișcare.
