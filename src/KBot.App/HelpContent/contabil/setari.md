---
id: contabil.setari
title: Setări și aspect
part: contabil
order: 80
parent: contabil
screens: SetariForm, SetariInfoView, SetariAplicatieView, SetariForexeView, SetariMultithreadView, SetariExtraseView, SetariAutentificareView, SetariSemnareDdfView
keywords: setari, configurare, configurare k-bot, tema, marime text, parola, informatii, autentificare, sesiune expirata, reafiseaza fereastra, tine minte parola, uita datele memorate, certificat memorat, uita certificatul, schimbare unitate, fereastra marita, tot ecranul, full screen, maximizat, la pornire, tur initial, nu mai arata turul, tutorial initial, tutorialul de inceput, nu mai arata tutorialul, mini-meniu, meniul din pagina
open: setari:aplicatie
---
<!-- slice: 0072, 0072-01, 0100-02, 0113, 0114, 0118, 0124-08 -->
Setările se deschid din ribbon, **Configurări › Configurare › «Configurare K-BOT»**, ca pagină a ferestrei
principale; le închizi cu **«Închide»** din dreapta ribbonului sau cu Esc ([Ribbonul](topic:contabil.ribbon)).
Jurnalele și istoria versiunilor nu mai sunt pagini aici: au machetele lor, **«Jurnal activitate»** și **«Istoric
versiuni»**, în același grup al ribbonului — [Jurnal activitate](topic:contabil.jurnal),
[Actualizarea K-BOT](topic:contabil.actualizare).

<!-- capture: setari | caption: Setările, deschise ca pagină a ferestrei principale | goto: setari:aplicatie | redo: 2026-10-13 17:30 | why: 0113 / 0118: setările sunt acum o pagină a ferestrei principale (fără bara de titlu proprie și fără «Închide»), iar paginile «Istoric versiuni» și «Jurnal» au ieșit din lista din stânga; înainte, 0078-14: pagina «Aplicație» are un comutator nou, «Avertizează dacă Adobe e mai vechi de 2025» (sub tutorialul de început); înainte, 000T-07: pagina «Aplicație» are un comutator nou, «Arată tutorialul de început la pornirea K-BOT» (sub turul inițial); înainte, 0100-02: comutatoarele regrupate (Fereastra principală, FOREXE, Avansat) și rândul «Descărcări multiple» (apare doar când unitatea o permite). -->

| Pagina | Ce găsești |
|--------|------------|
| **Informații** | datele sesiunii (utilizator, unitate, an, versiunile K-BOT), **schimbarea parolei** și «Caută actualizări» |
| **Aplicație** | comutatoarele de zi cu zi și opțiunile listei de angajamente (sortare, coloane) |
| **FOREXE** | certificatul memorat, mini-meniul din pagina FOREXE, testul de viteză și timpii de așteptare ai robotului — mai jos |
| **Descărcări multiple** | **apare doar dacă unitatea permite descărcarea pe mai multe taburi** — câte angajamente se descarcă deodată și actualizarea la conectare — [Mai multe descărcări deodată](topic:contabil.forexe.descarcare-multipla) |
| **Extrase** | coloanele grilelor de extrase — vezi [Extrase de cont](topic:contabil.vederi.extrase) |
| **Autentificare** | ce ține minte fereastra de conectare, ce se întâmplă când expiră sesiunea și adresa serverului K-BOT — mai jos |

Setările se salvează pe măsură ce le schimbi.

## Pagina «Aplicație»: cum e grupată fila «Generale»
<!-- slice: 0100, 0100-02, 0114, 0114-24, 0000-100, 0115-02, 0000-106 -->

Comutatoarele filei **Generale** sunt grupate după rostul lor; ce face fiecare nu s-a schimbat:

| Grupul | Ce conține |
|--------|-----------|
| **Fereastra principală** | pornirea mărită, tutorialul de început, avertismentul despre Adobe, comutatorul **«Panglica din fereastra principală e compactă»** (iconițe și text mai mici în ribbon, pentru ecrane mici; se aplică imediat — [Ribbonul](topic:contabil.ribbon)), comutatorul **«Meniul lateral stă în spatele machetelor»** (bifat, implicit, panoul lateral se vede doar cât nu e nicio machetă pe ecran, iar o machetă deschisă ocupă toată lățimea; debifat, panoul se vede tot timpul; se aplică imediat — [Panoul lateral](topic:contabil.panou)) și comutatorul **«Rândul «Jurnal activitate» în meniul MENIU al ferestrei principale»** — acum el arată sau ascunde butonul **«Jurnal activitate»** din ribbon (Configurări › Configurare) |
| **FOREXE** | consola detaliată, butonul «Arată browserul», selectorul de recepții |
| **Avansat** | «Activează opțiuni avansate» și, sub ea, modul de capturi |

Descărcările pe mai multe taburi au pagina lor, **Descărcări multiple** —
[Mai multe descărcări deodată](topic:contabil.forexe.descarcare-multipla).

## Pagina «Aplicație»: pornirea K-BOT
<!-- slice: 0097-02, 000T-07, 0000-51, 0078-14, 0113, 000T-13, 0000-107 -->

Din grupul **Fereastra principală**:

- **«Fereastra principală pornește mărită (pe tot ecranul)»** — la prima pornire după actualizarea care a adus
  ribbonul, K-BOT o **bifează o dată**, ca să vezi noua fereastră pe tot ecranul. De atunci alegerea e a ta: bifată,
  fereastra principală se deschide mărită pe tot ecranul; debifată, la mărimea ei obișnuită, în mijlocul ecranului.
  Se aplică de la următoarea pornire.
- **«Arată tutorialul de început la pornirea K-BOT»** — **bifată la început.** Cât e bifată, la pornire
  K-BOT te duce printr-un tutorial scurt al ferestrei principale (filele panglicii, tokenul ANAF, lista angajamentelor,
  cardul «Ce este nou»). Se debifează singură după ce l-ai văzut până la capăt sau ai bifat «Nu mai arăta tutorialul de
  început» pe bula lui; o bifezi din nou dacă vrei să-l revezi la pornire. Turul ferestrei principale nu mai pornește
  singur; îl găsești la «?», sub «Tururi ghidate» — [Tutorialele](topic:contabil.ajutor).
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
<!-- slice: 0063, 0072, 0097, 0097-04, 0097-05 -->

**«Fereastra de autentificare»** — ce ține minte fereastra de conectare pe acest calculator:

- **«Ține minte ultimul utilizator…»** — e-mailul se completează singur la pornire;
- **«Ține minte și unitatea aleasă ultima dată…»** — are efect doar împreună cu cea de sus;
- **«Uită datele memorate»** șterge e-mailul și unitatea ținute minte.

**«În timpul lucrului (sesiunea expirată)»** — ce face K-BOT când sesiunea expiră (vezi
[Conectarea](topic:contabil.autentificare)):

- **«Reafișează fereastra de autentificare când expiră sesiunea»** — bifată, K-BOT îți arată
  fereastra de conectare; debifată, se reconectează singur, cu parola scrisă la intrare. **Se poate
  schimba doar cu opțiunile avansate**; dacă le oprești după aceea, bifa rămâne așa cum ai lăsat-o.
- **«Reafișează cel mult o dată la»** — dacă sesiunea expiră mai devreme de atât după ultima
  conectare în fereastră, K-BOT se reconectează singur, fără fereastră. Poți alege între 10 minute
  și o oră; cu opțiunile avansate, și 5 minute sau până la 8 ore.

**«Arată în fereastra de autentificare bifa «Ține minte parola până la repornirea
calculatorului»»** — ascunde sau arată bifa aceea sub parolă. Se vede mereu și își păstrează starea
fără opțiunile avansate, dar **se poate schimba doar cu [opțiunile avansate](topic:avansat)**
pornite.

Cu opțiunile avansate mai apare:

- **«Uită parola memorată»** — șterge parola ținută minte pentru sesiunea Windows curentă.

**«Serverul»** — adresa serverului K-BOT și timpul maxim de așteptare. Adresa e fixată în program
și nu se schimbă de aici.

<!-- capture: setari-autentificare | caption: Pagina «Autentificare» din Setări | goto: setari:autentificare -->

## Pagina «Semnare DDF»
<!-- slice: 0124-05, 0124-08 -->

Aici stabilești **ce rol semnează Secțiunea A** a unui document de fundamentare nou, în funcție de **compartimentul**
documentului. Lista are câte un rând pentru fiecare compartiment folosit de documentele unității; în a doua coloană
alegi rolul (Contabil, Împuternicit, Secretar, Administrator sau Director) sau «(niciun rol)». Nimic nu se aplică până la
**«Salvează»**; **«Reîncarcă»** aruncă alegerile nesalvate.

Rolul ales se aplică documentelor **noi**, generate de acum înainte. Un compartiment fără rol oprește generarea unui document nou,
cu un mesaj care îl numește. Vezi [Cine semnează A](topic:contabil.ddf.semnare).

## Tema și mărimea textului
<!-- slice: 0028-09, 0036, 0036-01 -->

Butonul de **temă** din bara de titlu (lângă «?») schimbă culorile K-BOT (Clasic, Întunecat,
Modern...). Tot acolo, **Mărime text** mărește sau micșorează textul și controalele.

## Jurnalele și mesajele K-BOT
<!-- slice: 0118 -->

Jurnalele (și mesajele pe care ți le-a arătat K-BOT) nu mai sunt o pagină a setărilor: le citești și le golești din
**Configurări › Configurare › «Jurnal activitate»** — [Jurnal activitate](topic:contabil.jurnal).
