---
id: contabil.adechit
title: ADECHIT: Rapoarte
part: contabil
order: 77
parent: contabil
screens: AdechitRapoarteForm
keywords: adechit, rapoarte, raport, situatie lunara, situatie de plata, registru de casa, raport banca, fisa cont, documente anulate, copil, grupa, luna, subunitate, pdf, excel, previzualizare, gradinita, taxa, debitori, diferente, comparatie, solduri, lipsa, sid, sic, sfd, sfc
---
<!-- slice: 0123-01, 0123-03, 0123-05, 0000-119 -->
Rapoartele evidenței **ADECHIT** se fac acum direct în K-BOT, cu aceleași cifre ca pe site. Din ribbon, fila **ADECHIT** (apare doar dacă unitatea are date în ADECHIT), apasă **«Rapoarte»**. Se deschide o pagină care rămâne deschisă până o închizi. Ai nevoie de dreptul de citire în ADECHIT pentru unitate; dacă nu îl ai, pagina spune asta și nu arată nimic.

## Ce alegi în stânga
<!-- slice: 0123-01, 0123-03, 0123-05, 0000-119 -->

- **Subunitate**: evidența pentru care faci raportul. Lista se poate schimba doar când unitatea are mai multe subunități la care ai acces.
- **An** și **Luna**: lunile subunității, cea mai nouă prima. O lună închisă are în listă cuvântul **(închisă)**. La alegerea lunii se pune singură și perioada pentru registrul de casă și raportul de bancă, pe toată luna.
- **Grupa**: grupele care au avut prezență în luna aleasă. Pentru situația lunară și situația de plată alegi o grupă sau bifezi **«Toate grupele»**. **«Separă grupele»** se poate bifa doar după ce ai bifat «Toate grupele»: la situația lunară, fiecare grupă vine pe pagină nouă.
- **De la** / **Până la**: perioada pentru **Registru casă** și **Raport bancă**.
- **Ascunde rândurile cu zero**: numai la **Situație lunară** (debitori), elimină copiii cu zero în toate coloanele numerice, inclusiv zilele de prezență. Este bifată implicit. Funcționează pe o grupă, pe toate grupele și cu grupe separate; PDF-ul și Excelul folosesc aceeași filtrare. Un sold final zero nu ascunde un rând care are prezență, sold inițial sau mișcări diferite de zero. Grupele fără rânduri rămase nu se listează; dacă nu rămâne niciun rând, apare un mesaj. Totalurile valorice se păstrează. Generează din nou raportul după schimbarea bifei. <!-- slice: AD17-01 -->
- **Caută copil**: scrii cel puțin 3 caractere din nume sau din CNP; căutarea pornește singură după o scurtă pauză de tastare (sau imediat, dacă apeși **Enter**). Copiii găsiți în luna aleasă apar în lista **Copil** (cu grupa și ultimele cifre din CNP, ca să deosebești omonimii). Se arată cel mult 100; dacă sunt mai mulți, restrânge căutarea.

## Rapoartele
<!-- slice: 0123-01, 0123-03, 0123-05, 0000-119, AD18-01 -->

| Buton | Ce faci | Ce îți trebuie |
|-------|---------|----------------|
| **Situație lunară** | Situația debitorilor pe luna aleasă | luna și o grupă sau «Toate grupele» |
| **Situație de plată** | Soldurile inițiale și taxa lunară integrală; pe toate grupele, fiecare grupă pe pagină nouă | luna și o grupă sau «Toate grupele» |
| **Diferențe între luni** | Numai diferențele SID/SIC față de SFD/SFC anterior și copiii lipsă într-una dintre luni | luna curentă a comparației și o grupă sau «Toate grupele»; luna calendaristică anterioară trebuie să existe |
| **Registru casă** | Registrul de casă pe perioadă | cele două date |
| **Raport bancă** | Încasările din extrasele de bancă, pe perioadă | cele două date |
| **Fișă cont** | Fișa de cont a unui copil, sau a fiecărui copil din grupă dacă bifezi **«Toți copiii din grupa aleasă»** | luna și un copil din listă (sau o grupă, cu bifa) |
| **Documente anulate** | Documentele anulate în luna aleasă | luna |

Un buton este gri până când ai ales tot ce îi trebuie. Cât timp se așteaptă răspunsul serverului (un raport, o listă, o căutare), toate butoanele sunt gri și sub bara de titlu aleargă o bară de încărcare.

## Diferențe între luni
<!-- slice: AD18-01 -->

Alege luna curentă a comparației și grupa sau bifează **«Toate grupele»**, apoi apasă **«Diferențe între luni»**. Raportul folosește automat luna calendaristică anterioară, inclusiv la trecerea din ianuarie în decembrie anul anterior. Dacă acea lună nu există în evidență, apare un mesaj.

Apar numai copiii cu **SID curent diferit de SFD anterior** sau **SIC curent diferit de SFC anterior**, plus copiii cu rând într-o singură lună. Diferențele din alte coloane nu includ singure copilul în raport. Pentru fiecare copil vezi rândul complet din luna curentă și, imediat sub el, rândul complet din luna anterioară. **Luna**, **Grupa** și **Diferență / lipsă** identifică rândurile și motivul afișării. Luna fără rând este marcată **LIPSĂ**, cu numele și CNP-ul pentru identificare și cu celulele numerice goale. Bifa **«Ascunde rândurile cu zero»** nu se aplică: un rând lipsă apare chiar dacă cel existent are numai zero.

Copiii sunt ordonați alfabetic. Dacă un copil s-a mutat între grupe, rândurile lui rămân împreună. Filtrul de grupă include perechea dacă rândul din oricare dintre luni aparține grupei alese. Cu **«Separă grupele»**, fiecare grupă începe pe pagină nouă: se folosește grupa curentă, iar dacă rândul curent lipsește, grupa anterioară. Pentru o grupă absentă din lista lunii curente, bifează **«Toate grupele»**.

O lună închisă folosește situația salvată, iar o lună deschisă folosește calculul situației lunare. Soldurile necompletate sunt comparate ca zero. Dacă nu sunt diferențe sau rânduri lipsă, apare un mesaj. Antetul arată câte perechi au solduri diferite și câte rânduri lipsesc în fiecare lună. PDF-ul și Excelul conțin aceeași selecție.

## Raportul pe ecran
<!-- slice: 0123-01, 0123-03, 0123-05, 0000-119, AD19-02 -->

Raportul apare în dreapta, ca PDF, potrivit pe lățimea panoului. Îl poți tipări, salva sau mări cu butoanele vizualizatorului. Alt raport îl înlocuiește pe cel de pe ecran.

**«Salvează Excel»** (jos în stânga) salvează raportul de pe ecran ca fișier Excel, cu toate coloanele; alegi unde. Funcționează cât timp raportul este deschis aici: după ce ai ales alt raport sau ai închis pagina, trebuie să-l faci din nou.

Datele raportului sunt cele de la momentul în care l-ai cerut; PDF-ul și Excelul vin din același raport, deci au aceleași cifre. Dacă între timp s-au schimbat date în evidență, cere raportul din nou.

Dacă a fost completat separat un istoric omis la migrare, **Fișă cont** păstrează rândul **Sold Inițial** și arată distinct **Sold reportat din anii anteriori**, la începutul anului de aplicare. Componenta suplimentară intră o singură dată în soldurile calculate; nu dublează SI existent. Situațiile salvate ale lunilor închise rămân cele existente. După transfer, generează din nou raportul.

Orice problemă (nu ai drepturi, luna nu mai există, serverul nu răspunde) apare pe linia de sub raport, cu textul serverului.
