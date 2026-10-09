---
id: contabil.setari
title: Setări și aspect
part: contabil
order: 80
parent: contabil
screens: SetariForm, SetariInfoView, SetariAplicatieView, SetariForexeView, SetariMultithreadView, SetariExtraseView, SetariAutentificareView, SetariJurnalView, LogViewerForm, LogClearDialog
keywords: setari, tema, marime text, jurnal, parola, informatii, autentificare, sesiune expirata, reafiseaza fereastra, tine minte parola, uita datele memorate, certificat memorat, uita certificatul, schimbare unitate, fereastra marita, tot ecranul, full screen, maximizat, la pornire, tur initial, nu mai arata turul, tutorial initial, tutorialul de inceput, nu mai arata tutorialul, mini-meniu, meniul din pagina
open: setari:aplicatie
---
<!-- slice: 0072, 0072-01, 0000-29, 0100-02, fara-felie -->
Butonul **MENIU** al ferestrei principale are două rânduri pentru această fereastră:
**Configurare K-BOT** o deschide, iar **Jurnal activitate** o deschide direct pe pagina **Jurnal**
(rândul se poate ascunde din pagina **Aplicație**).

<!-- capture: setari | caption: Fereastra «Setări» | goto: setari:aplicatie | redo: 2026-10-06 12:00 | why: 0078-14: pagina «Aplicație» are un comutator nou, «Avertizează dacă Adobe e mai vechi de 2025» (sub tutorialul de început); înainte, 000T-07: pagina «Aplicație» are un comutator nou, «Arată tutorialul de început la pornirea K-BOT» (sub turul inițial); înainte, 0100-02: comutatoarele regrupate (Fereastra principală, FOREXE, Avansat) și rândul «Descărcări multiple» (apare doar când unitatea o permite). -->

| Pagina | Ce găsești |
|--------|------------|
| **Informații** | datele sesiunii (utilizator, unitate, an, versiunile K-BOT), **schimbarea parolei** și «Caută actualizări» |
| **Aplicație** | comutatoarele de zi cu zi și opțiunile listei de angajamente (sortare, coloane) |
| **FOREXE** | certificatul memorat, mini-meniul din pagina FOREXE, testul de viteză și timpii de așteptare ai robotului — mai jos |
| **Descărcări multiple** | **apare doar dacă unitatea permite descărcarea pe mai multe taburi** — câte angajamente se descarcă deodată și actualizarea la conectare — [Mai multe descărcări deodată](topic:contabil.forexe.descarcare-multipla) |
| **Extrase** | coloanele grilelor de extrase — vezi [Extrase de cont](topic:contabil.vederi.extrase) |
| **Autentificare** | ce ține minte fereastra de conectare, ce se întâmplă când expiră sesiunea și adresa serverului K-BOT — mai jos |
| **Jurnal** | jurnalele K-BOT, pentru când ceva nu merge |

Setările se salvează pe măsură ce le schimbi.

## Pagina «Aplicație»: cum e grupată fila «Generale»
<!-- slice: 0100, 0100-02 -->

Comutatoarele filei **Generale** sunt grupate după rostul lor; ce face fiecare nu s-a schimbat:

| Grupul | Ce conține |
|--------|-----------|
| **Fereastra principală** | pornirea mărită, turul inițial, avertismentul despre Adobe, rândul «Jurnal activitate» din meniul MENIU |
| **FOREXE** | consola detaliată, butonul «Arată browserul», selectorul de recepții |
| **Avansat** | «Activează opțiuni avansate» și, sub ea, modul de capturi |

Descărcările pe mai multe taburi au pagina lor, **Descărcări multiple** —
[Mai multe descărcări deodată](topic:contabil.forexe.descarcare-multipla).

## Pagina «Aplicație»: pornirea K-BOT
<!-- slice: 0097-02, 000T-07, 0000-51, 0078-14 -->

Din grupul **Fereastra principală**:

- **«Fereastra principală pornește mărită (pe tot ecranul)»** — **debifată la început.** Bifată,
  fereastra principală se deschide mărită pe tot ecranul; debifată, la mărimea ei obișnuită, în
  mijlocul ecranului. Se aplică de la următoarea pornire.
- **«Arată turul ferestrei principale la pornirea K-BOT»** — **debifată la început.** Cât e debifată,
  turul ferestrei principale nu pornește singur; îl pornești din meniul «?». O bifezi dacă vrei să
  pornească singur la fiecare pornire. Se debifează singură după ce ai văzut turul până la capăt
  sau ai bifat «Nu mai arăta turul inițial» pe bula lui — [Tururile ghidate](topic:contabil.ajutor).
- **«Arată tutorialul de început la pornirea K-BOT»** — **bifată la început.** Cât e bifată,
  tutorialul scurt despre tutoriale pornește singur la pornire, după turul ferestrei principale
  (dacă acela mai e de văzut). Nu se poate închide la jumătate: se parcurge până la capăt. Se debifează
  singură după ce l-ai văzut până la capăt; o bifezi din nou dacă vrei să-l revezi la pornire. Cum faci asta pas cu pas îți arată tutorialul «Pornește sau oprește tutorialul de
  început» — [Tutorialele](topic:contabil.ajutor).
- **«Avertizează dacă Adobe e mai vechi de 2025»** — **bifată la început.** Cât e bifată, la
  pornire K-BOT îți spune, într-o fereastră, dacă Adobe de pe calculator e mai vechi de versiunea
  2025 și îți recomandă **Adobe Acrobat Reader gratuit**, versiunea 2025 sau mai nouă: pe un Acrobat
  mai vechi sau neoriginal pot apărea mesaje de eroare la completarea sau la semnarea documentelor.
  În fereastra avertismentului, **«Da»** la întrebarea «Vrei să nu mai primești acest avertisment?»
  îl oprește (debifează comutatorul), iar **«Nu»** îl lasă. Îl bifezi din nou de aici.

## Pagina «FOREXE»: mini-meniul din pagină
<!-- slice: 0097-02 -->

**«Arată mini-meniul K-BOT în pagina FOREXE (mărire / micșorare, starea urmăririi)»** — **bifată
la început.** Debifată, micul meniu K-BOT din colțul paginii FOREXE nu se mai vede; K-BOT urmărește
în continuare ce salvezi în pagină. Se aplică imediat —
[Vederea «Browser FOREXE»](topic:contabil.forexe.browser).

## Pagina «FOREXE»: certificatul memorat
<!-- slice: 0072, 0097 -->

**«Certificatul memorat»** arată certificatul pe care «Conectare» îl folosește fără să te mai
întrebe. **«Uită certificatul»** îl șterge; la următoarea conectare îl alegi din nou. Șterge și regula
prin care browserul alegea singur certificatul. Dacă Windows nu o lasă ștearsă direct, îți cere
permisiunea de administrator; răspunde «Da».

**«Uită certificatul memorat când schimb unitatea din bara de titlu»** — **debifată la început.**
Când treci pe altă unitate din bara de titlu (vezi [Fereastra principală](topic:contabil.fereastra)),
conexiunea FOREXE se închide oricum:

- **debifată** — certificatul memorat rămâne; pe unitatea nouă, «Conectare» îl folosește tot pe el;
- **bifată** — certificatul memorat se uită odată cu conexiunea, iar «Conectare» ți-l cere din nou.
  Bifeaz-o dacă unitățile tale au certificate diferite.

## Pagina «Autentificare»
<!-- slice: 0063, 0072, 0097 -->

**«Fereastra de autentificare»** — ce ține minte fereastra de conectare pe acest calculator:

- **«Ține minte ultimul utilizator…»** — e-mailul se completează singur la pornire;
- **«Ține minte și unitatea aleasă ultima dată…»** — are efect doar împreună cu cea de sus;
- **«Uită datele memorate»** șterge e-mailul și unitatea ținute minte.

**«În timpul lucrului (sesiunea expirată)»** — ce face K-BOT când sesiunea expiră (vezi
[Conectarea](topic:contabil.autentificare)):

- **«Reafișează fereastra de autentificare când expiră sesiunea»** — bifată, K-BOT îți arată
  fereastra de conectare; debifată, se reconectează singur, cu parola scrisă la intrare. **Se poate
  debifa doar cu opțiunile avansate**; fără ele e mereu bifată.
- **«Reafișează cel mult o dată la»** — dacă sesiunea expiră mai devreme de atât după ultima
  conectare în fereastră, K-BOT se reconectează singur, fără fereastră. Poți alege între 10 minute
  și o oră; cu opțiunile avansate, și 5 minute sau până la 8 ore.

Cu [opțiunile avansate](topic:avansat) mai apar:

- **«Arată în fereastra de autentificare bifa «Ține minte parola până la repornirea
  calculatorului»»** — ascunde sau arată bifa aceea sub parolă;
- **«Uită parola memorată»** — șterge parola ținută minte pentru sesiunea Windows curentă.

**«Serverul»** — adresa serverului K-BOT și timpul maxim de așteptare. Adresa e fixată în program
și nu se schimbă de aici.

<!-- capture: setari-autentificare | caption: Pagina «Autentificare» din Setări | goto: setari:autentificare -->

## Tema și mărimea textului
<!-- slice: 0028-09, 0036, 0036-01 -->

Butonul de **temă** din bara de titlu (lângă «?») schimbă culorile K-BOT (Clasic, Întunecat,
Modern...). Tot acolo, **Mărime text** mărește sau micșorează textul și controalele.

## Mesajele K-BOT
<!-- slice: fara-felie -->

Orice mesaj pe care ți-l arată K-BOT se scrie și în jurnalul de mesaje, așa că îl poți regăsi
după ce ai apăsat OK: **Setări › Jurnal**.

## Golirea jurnalelor
<!-- slice: 0031-04, 0000-07 -->

Pe pagina **Jurnal**, **«Golește jurnale…»** deschide lista fișierelor de jurnal de pe calculatorul
tău, cu mărimea și numărul de intrări ale fiecăruia. La deschidere nu e nimic bifat.

1. Bifează fișierele de șters.
2. Apasă **«Șterge»**. K-BOT îți spune exact ce dispare și cât ocupă, și cere confirmarea.

**Ștergerea nu se poate anula.** Un fișier folosit chiar acum de K-BOT nu se poate șterge; unul
ținut deschis de alt program se golește în loc să se șteargă. Jurnalele serverului nu se ating de
aici.

> Înainte să golești jurnalele după o problemă, trimite-le întâi cui te ajută.
