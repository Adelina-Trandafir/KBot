---
id: contabil.setari
title: Setări și aspect
part: contabil
order: 80
parent: contabil
screens: SetariForm, SetariInfoView, SetariAplicatieView, SetariForexeView, SetariExtraseView, SetariAutentificareView, SetariJurnalView, LogViewerForm, LogClearDialog
keywords: setari, tema, marime text, jurnal, parola, informatii, autentificare, sesiune expirata, reafiseaza fereastra, tine minte parola, uita datele memorate, certificat memorat, uita certificatul, schimbare unitate, fereastra marita, tot ecranul, full screen, maximizat, la pornire, tur initial, nu mai arata turul, mini-meniu, meniul din pagina
open: setari:aplicatie
---
<!-- slice: 0072, 0072-01 -->
Butonul cu **rotița** din bara de titlu a ferestrei principale deschide **Setări...** și
**Arată jurnal**.

<!-- capture: setari | caption: Fereastra «Setări» | goto: setari:aplicatie -->

| Pagina | Ce găsești |
|--------|------------|
| **Informații** | datele sesiunii (utilizator, unitate, an, versiunile K-BOT), **schimbarea parolei** și «Caută actualizări» |
| **Aplicație** | comutatoarele de zi cu zi și opțiunile listei de angajamente (sortare, coloane) |
| **FOREXE** | certificatul memorat, mini-meniul din pagina FOREXE, testul de viteză și timpii de așteptare ai robotului — mai jos |
| **Extrase** | coloanele grilelor de extrase — vezi [Extrase de cont](topic:contabil.vederi.extrase) |
| **Autentificare** | ce ține minte fereastra de conectare, ce se întâmplă când expiră sesiunea și adresa serverului K-BOT — mai jos |
| **Jurnal** | jurnalele K-BOT, pentru când ceva nu merge |

Setările se salvează pe măsură ce le schimbi.

## Pagina «Aplicație»: pornirea K-BOT
<!-- slice: 0097-02 -->

Pe fila **Generale**:

- **«Fereastra principală pornește mărită (pe tot ecranul)»** — **debifată la început.** Bifată,
  fereastra principală se deschide mărită pe tot ecranul; debifată, la mărimea ei obișnuită, în
  mijlocul ecranului. Se aplică de la următoarea pornire.
- **«Arată turul ferestrei principale la pornirea K-BOT»** — **bifată la început.** Cât e bifată,
  turul ferestrei principale pornește singur la fiecare pornire. Se debifează singură după ce ai
  văzut turul până la capăt sau ai bifat «Nu mai arăta turul inițial» pe bula lui; o bifezi din
  nou dacă vrei să-l revezi la pornire — [Tururile ghidate](topic:contabil.ajutor).

## Pagina «FOREXE»: mini-meniul din pagină
<!-- slice: 0097-02 -->

**«Arată mini-meniul K-BOT în pagina FOREXE (mărire / micșorare, starea urmăririi)»** — **bifată
la început.** Debifată, micul meniu K-BOT din colțul paginii FOREXE nu se mai vede; K-BOT urmărește
în continuare ce salvezi în pagină. Se aplică imediat —
[Vederea «Browser FOREXE»](topic:contabil.forexe.browser).

## Pagina «FOREXE»: certificatul memorat
<!-- slice: 0072, 0097 -->

**«Certificatul memorat»** arată certificatul pe care «Conectare» îl folosește fără să te mai
întrebe. **«Uită certificatul»** îl șterge; la următoarea conectare îl alegi din nou.

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
