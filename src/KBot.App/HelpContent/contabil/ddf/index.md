---
id: contabil.ddf
title: Documentul de fundamentare (DDF)
part: contabil
order: 50
parent: contabil
screens: DdfView, DdfVizualizarePage, DdfDocumentPage, DdfFisierePage
keywords: ddf, fundamentare, document de fundamentare, angajament bugetar, revizie, stari, flux, trimitere, semnare, imprimare, tiparire, listat
open: view:ddf
---
<!-- slice: 0020-02, 0081 -->
Documentul de fundamentare (DDF) justifică fiecare rezervare de credite a unui angajament. K-BOT
îl face, îl semnezi în K-BOT, iar K-BOT îl trimite în FOREXE și completează singur ce vine de acolo.

<!-- capture: ddf-vedere | caption: Vederea «Fundamentare» cu reviziile unui angajament | goto: view:ddf | prepare: Selectați un angajament cu cel puțin două revizii DDF și alegeți una în arbore. -->

## Drumul unei revizii
<!-- slice: 0078-06, 0081, 0000-14 -->

1. **Scrii revizia**: antetul și Secțiunea A (valorile, pe clasificații) — [Editorul DDF](topic:contabil.ddf.editor).
2. K-BOT face **PDF-ul intermediar** (fără Secțiunea B). Îl **semnezi pe Secțiunea A** — [Semnarea](topic:contabil.ddf.semnare).
3. **Trimiți în FOREXE** — [Trimiterea](topic:contabil.ddf.trimitere). Din acest moment revizia nu
   se mai poate modifica; orice schimbare de valori înseamnă o revizie nouă.
4. K-BOT pune **Secțiunea B** și **capturile de ecran** din FOREXE **în documentul semnat pe A** (nu
   face altul; semnătura A rămâne).
5. **Semnezi Secțiunea B**.
6. **Directorul** semnează ultimul, în K-BOT-ul lui.

## Stările unei revizii
<!-- slice: 0081-01, 0097, 0078-06, 0000-15 -->

Starea se vede la fiecare revizie din arborele vederii «Fundamentare».

| Stare | Ce înseamnă | Ce poți face |
|-------|-------------|--------------|
| **Ciornă** | revizia e scrisă, nesemnată | o modifici; o semnezi pe A |
| **Semnat A — gata de trimis** | PDF-ul intermediar e semnat pe A | **Trimite în FOREXE** (nu se mai modifică și nu se mai șterge) |
| **Trimitere întreruptă** | trimiterea s-a oprit la jumătate | **Reia trimiterea în FOREXE** |
| **Trimis în FOREXE — în lucru** | doar la un angajament nou: urmează Definitivează / Derulează | pașii din meniul Rezervărilor — [Definitivează, Derulează](topic:contabil.ddf.rezervare) |
| **PDF final — de semnat B** | FOREXE s-a terminat; Secțiunea B e pusă în documentul semnat pe A | semnezi B |
| **Semnat A și B — la director** | așteaptă semnătura directorului | nimic |
| **Aprobat** | directorul a semnat | capătul drumului |

> **O singură revizie deschisă pe angajament.** Cât timp o revizie nu are PDF final, nu se poate
> începe alta pe același angajament.

## Vederea «Fundamentare»
<!-- slice: 0020-02, 0033-02, 0081-04, 0097, 0000-14, 0078-08, 0099, 0101 -->

- **Arborele**: rădăcina **«Toate reviziile»** (clic pe ea = tot documentul), lunile, apoi
  reviziile. **Clic dreapta** pe o revizie oferă, după stare: «Trimite în FOREXE» / «Reia trimiterea
  în FOREXE», «Modifică revizia», «Șterge revizia», «Șterge documentul». Pe o lună: «Șterge TOATE
  reviziile lunii»; pe rădăcină: «Șterge documentul (TOATE reviziile)».
- **O revizie semnată** (fie și cu o singură semnătură) **nu se mai modifică și nu se mai șterge**:
  meniul ei păstrează doar «Trimite în FOREXE» / «Reia trimiterea», când starea o cere. Ștergerea
  unei luni sau a documentului întreg se oferă doar cât **nicio** revizie de sub ea nu e semnată.
- După o ștergere reușită nu mai apare niciun mesaj: ce s-a șters se scrie în jurnalul de mesaje
  (**Setări › Jurnal**).
- **Lupa** din capul arborelui caută o revizie; **butonul de strângere** din subsol îngustează
  arborele — [Arborii și tabelele](topic:contabil.liste).
- Paginile din dreapta, alese din bara de sus: **Vizualizare** (valorile reviziei), **Document PDF**
  (documentul, unde se și semnează) și **Fișiere** (atașamentele, inclusiv capturile venite din FOREXE).
  Pe o **lună** sau pe **«Toate reviziile»** bara rămâne: **Vizualizare** arată valorile tuturor
  reviziilor de sub rând, iar pagina **Document PDF** se numește **Documente** și arată lista de
  tipărire — vezi **Lista de tipărire**, la sfârșitul acestui ajutor.
- Cât Adobe încă deschide documentul unei revizii (sau un fișier din «Fișiere»), arborele și lista
  de fișiere nu primesc alt rând — [Cât se deschide un document](topic:contabil.liste).

<!-- capture: ddf-document | caption: Pagina «Document PDF» a unei revizii semnate | goto: view:ddf | prepare: Alegeți o revizie semnată din arbore și deschideți pagina «Document PDF». -->

## Lista de tipărire
<!-- slice: 0099, 0101 -->

Când dai clic pe o **lună** sau pe **«Toate reviziile»**, bara cu pagini rămâne, iar pagina **Document PDF** se numește **Documente**. Deschide-o ca să vezi **lista reviziilor** de sub rândul ales; pagina **Vizualizare** arată în continuare valorile lor. Pe fiecare rând al listei vezi:

| Coloană | Ce arată |
|---------|----------|
| **bifa** din stânga | alegi rândul |
| **Document** | revizia (numărul și data) |
| **Semnături** | ce semnături are |
| **Semnat la** | când a fost semnat |
| **Listat** | bifat dacă documentul a fost tipărit măcar o dată |
| **Nr. tipăriri** | de câte ori a fost tipărit |

<!-- capture: ddf-lista-tiparire | caption: Lista de tipărire a reviziilor, pe «Toate reviziile» | goto: view:ddf | prepare: Selectați un angajament cu mai multe revizii, unele semnate și tipărite, altele nu; clic pe rădăcina «Toate reviziile», apoi pe pagina «Documente». -->

Iconița din capul coloanei cu bife deschide un meniu:

- **«Selectează / deselectează toate»** — bifează toate rândurile, sau le debifează dacă erau toate bifate;
- **«Selectează / deselectează doar cele nelistate»** — la fel, dar doar pe rândurile cu 0 tipăriri; cele deja listate rămân cum erau.

<!-- capture: ddf-lista-meniu | caption: Meniul din capul coloanei cu bife | goto: view:ddf | prepare: Pe pagina «Documente» a rădăcinii «Toate reviziile», apăsați iconița din capul coloanei cu bife și țineți meniul deschis. -->

Jos sunt două butoane, care lucrează pe rândurile **bifate**:

- **«Generează și imprimă»** — alegi imprimanta, iar K-BOT pregătește documentele și le trimite la imprimantă, **fără să le deschidă**. Cele semnate sunt cele de pe server; cele nesemnate se fac pe loc din datele salvate, ca la «Generează». Fiecare document trimis se numără ca tipărit.
- **«Salvează local»** — alegi un dosar, iar K-BOT salvează acolo documentele bifate. Nu le numără ca tipărite.

Dacă un document nu poate fi tipărit sau salvat, la sfârșit primești un mesaj cu documentul și motivul; altfel nu apare niciun mesaj.

**Listat, de mână.** Dacă bifezi **«Listat»** la un document cu 0 tipăriri (l-ai tipărit în altă parte), K-BOT întreabă dacă îl marchezi ca listat. **«Da»** îl numără ca o tipărire; **«Nu»** scoate bifa. După «Da» nu mai apare niciun mesaj, decât dacă ceva nu merge. La un document deja listat caseta e blocată: numărul doar crește.

Numărul crește și când tipărești documentul din pagina **Document**.
