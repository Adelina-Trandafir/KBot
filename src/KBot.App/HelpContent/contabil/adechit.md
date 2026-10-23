---
id: contabil.adechit
title: ADECHIT: Rapoarte
part: contabil
order: 77
parent: contabil
screens: AdechitRapoarteForm
keywords: adechit, rapoarte, raport, situatie lunara, situatie de plata, registru de casa, raport banca, fisa cont, documente anulate, copil, grupa, luna, subunitate, pdf, excel, previzualizare, gradinita, taxa, debitori
---
<!-- slice: 0123-01, 0123-03, 0000-119 -->
Rapoartele evidenței **ADECHIT** se fac acum direct în K-BOT, cu aceleași cifre ca pe site. Din ribbon, fila **ADECHIT**, apasă **«Rapoarte»**. Se deschide o pagină care rămâne deschisă până o închizi. Ai nevoie de dreptul de citire în ADECHIT pentru unitate; dacă nu îl ai, pagina spune asta și nu arată nimic.

## Ce alegi în stânga
<!-- slice: 0123-01, 0123-03, 0000-119 -->

- **Subunitate**: evidența pentru care faci raportul. Lista se poate schimba doar când unitatea are mai multe subunități la care ai acces.
- **An** și **Luna**: lunile subunității, cea mai nouă prima. O lună închisă are în listă cuvântul **(închisă)**. La alegerea lunii se pune singură și perioada pentru registrul de casă și raportul de bancă, pe toată luna.
- **Grupa**: grupele care au avut prezență în luna aleasă. Pentru situația lunară și situația de plată alegi o grupă sau bifezi **«Toate grupele»**. **«Separă grupele»** se poate bifa doar după ce ai bifat «Toate grupele»: la situația lunară, fiecare grupă vine pe pagină nouă.
- **De la** / **Până la**: perioada pentru **Registru casă** și **Raport bancă**.
- **Caută copil**: scrii cel puțin 3 caractere din nume sau din CNP; căutarea pornește singură după o scurtă pauză de tastare (sau imediat, dacă apeși **Enter**). Copiii găsiți în luna aleasă apar în lista **Copil** (cu grupa și ultimele cifre din CNP, ca să deosebești omonimii). Se arată cel mult 100; dacă sunt mai mulți, restrânge căutarea.

## Rapoartele
<!-- slice: 0123-01, 0123-03, 0000-119 -->

| Buton | Ce faci | Ce îți trebuie |
|-------|---------|----------------|
| **Situație lunară** | Situația debitorilor pe luna aleasă | luna și o grupă sau «Toate grupele» |
| **Situație de plată** | Soldurile inițiale și taxa lunară integrală; pe toate grupele, fiecare grupă pe pagină nouă | luna și o grupă sau «Toate grupele» |
| **Registru casă** | Registrul de casă pe perioadă | cele două date |
| **Raport bancă** | Încasările din extrasele de bancă, pe perioadă | cele două date |
| **Fișă cont** | Fișa de cont a unui copil, sau a fiecărui copil din grupă dacă bifezi **«Toți copiii din grupa aleasă»** | luna și un copil din listă (sau o grupă, cu bifa) |
| **Documente anulate** | Documentele anulate în luna aleasă | luna |

Un buton este gri până când ai ales tot ce îi trebuie. Cât timp se așteaptă răspunsul serverului (un raport, o listă, o căutare), toate butoanele sunt gri și sub bara de titlu aleargă o bară de încărcare.

## Raportul pe ecran
<!-- slice: 0123-01, 0123-03, 0000-119 -->

Raportul apare în dreapta, ca PDF, potrivit pe lățimea panoului. Îl poți tipări, salva sau mări cu butoanele vizualizatorului. Alt raport îl înlocuiește pe cel de pe ecran.

**«Salvează Excel»** (jos în stânga) salvează raportul de pe ecran ca fișier Excel, cu toate coloanele; alegi unde. Funcționează cât timp raportul este deschis aici: după ce ai ales alt raport sau ai închis pagina, trebuie să-l faci din nou.

Datele raportului sunt cele de la momentul în care l-ai cerut; PDF-ul și Excelul vin din același raport, deci au aceleași cifre. Dacă între timp s-au schimbat date în evidență, cere raportul din nou.

Orice problemă (nu ai drepturi, luna nu mai există, serverul nu răspunde) apare pe linia de sub raport, cu textul serverului.
