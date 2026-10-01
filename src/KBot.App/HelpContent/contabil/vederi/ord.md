---
id: contabil.vederi.ord
title: Ordonanțare
part: contabil
order: 70
parent: contabil.vederi
screens: OrdView, OrdVizualizarePage, OrdDocumentPage
keywords: ordonantare, ord, ordonantari de plata, imprimare, tiparire, listat, document, pdf, semnare, genereaza, validare formular, anulare validare, alte avize, verificat avizat, cfp, control financiar preventiv, ordonator, compartiment de specialitate
open: view:ord
---
<!-- slice: 0033, 0041, 0099 -->
Ordonanțările de plată ale angajamentului, cu documentul fiecăreia.

<!-- capture: ord | caption: Vederea «Ordonanțare» | goto: view:ord | prepare: Selectați un angajament care are ordonanțări, apoi o ordonanțare din arbore. -->

- **Arborele** listează ordonanțările pe luni, cu data și totalul fiecăreia, sub rădăcina
  **«Toate ordonanțările»** (clic pe ea = toate rândurile angajamentului).
- În dreapta, două pagini:
  - **Vizualizare** — rândurile ordonanțării alese (clasificație, descriere, recepții, plăți,
    valoare, rămas) și, pe fiecare beneficiar, codul fiscal, contul IBAN și documentele
    justificative. **«Caută Beneficiar»** te duce la un beneficiar anume.
  - **Document** — PDF-ul ordonanțării.
- Pe o **lună** sau pe **«Toate ordonanțările»** dreapta arată în schimb lista de tipărire — mai jos.

## Comenzile
<!-- slice: 0049, 0049-01, 0097, 0000-14 -->

**Clic dreapta** pe arbore deschide meniul:

| Comandă | Ce face |
|---------|---------|
| **Adaugă ordonanțare…** | o ordonanțare nouă, din plățile unei zile — [Ordonanțări noi](topic:contabil.ord.generare) |
| **Modifică ordonanțarea** | deschide ordonanțarea selectată în editor — [Editorul de ordonanțare](topic:contabil.ord.editor) |
| **Șterge ordonanțarea** | o șterge, după confirmare — [Ordonanțări noi](topic:contabil.ord.generare) |
| **Generare în lot…** | câte o ordonanțare pentru fiecare zi cu plăți neordonanțate — [Ordonanțări noi](topic:contabil.ord.generare) |
| **Șterge TOATE ordonanțările lunii** / **Șterge TOATE ordonanțările** | pe o lună, respectiv pe rădăcină: le șterge pe toate, după o singură confirmare — [Ordonanțări noi](topic:contabil.ord.generare) |

Subsolul arborelui («Adaugă») mai are **butonul de strângere**, care îngustează arborele; lupa din cap
caută o ordonanțare — [Arborii și tabelele](topic:contabil.liste).

«Modifică» și «Șterge ordonanțarea» apar doar când clicul e pe o ordonanțare; ștergerea tuturor, doar
pe o lună sau pe rădăcină, și **doar dacă niciuna de sub ea nu e semnată**.

> **O ordonanțare semnată nu se mai modifică și nu se mai șterge.** Ajunge o singură semnătură: pe
> ea clicul dreapta nu mai deschide niciun meniu. Iconița **«Adaugă»** din
subsolul arborelui face același lucru ca «Adaugă ordonanțare…».

## Lista de tipărire
<!-- slice: 0099 -->

Când dai clic pe o **lună** sau pe **«Toate ordonanțările»**, dreapta nu mai arată paginile, ci **lista ordonanțărilor** de sub rândul ales. Pe fiecare rând vezi:

| Coloană | Ce arată |
|---------|----------|
| **bifa** din stânga | alegi rândul |
| **Document** | ordonanțarea (numărul și data) |
| **Semnături** | ce semnături are |
| **Semnat la** | când a fost semnat |
| **Listat** | bifat dacă documentul a fost tipărit măcar o dată |
| **Nr. tipăriri** | de câte ori a fost tipărit |

<!-- capture: ord-lista-tiparire | caption: Lista de tipărire a ordonanțărilor, pe «Toate ordonanțările» | goto: view:ord | prepare: Selectați un angajament cu mai multe ordonanțări, unele semnate și tipărite, altele nu; clic pe rădăcina «Toate ordonanțările». -->

Iconița din capul coloanei cu bife deschide un meniu:

- **«Selectează / deselectează toate»** — bifează toate rândurile, sau le debifează dacă erau toate bifate;
- **«Selectează / deselectează doar cele nelistate»** — la fel, dar doar pe rândurile cu 0 tipăriri; cele deja listate rămân cum erau.

<!-- capture: ord-lista-meniu | caption: Meniul din capul coloanei cu bife | goto: view:ord | prepare: Pe lista de tipărire, apăsați iconița din capul coloanei cu bife și țineți meniul deschis. -->

Jos sunt două butoane, care lucrează pe rândurile **bifate**:

- **«Generează și imprimă»** — alegi imprimanta, iar K-BOT pregătește documentele și le trimite la imprimantă, **fără să le deschidă**. Cele semnate sunt cele de pe server; cele nesemnate se fac pe loc din datele salvate, ca la «Generează». Fiecare document trimis se numără ca tipărit.
- **«Salvează local»** — alegi un dosar, iar K-BOT salvează acolo documentele bifate. Nu le numără ca tipărite.

Dacă un document nu poate fi tipărit sau salvat, la sfârșit primești un mesaj cu documentul și motivul; altfel nu apare niciun mesaj.

**Listat, de mână.** Dacă bifezi **«Listat»** la un document cu 0 tipăriri (l-ai tipărit în altă parte), K-BOT întreabă dacă îl marchezi ca listat. **«Da»** îl numără ca o tipărire; **«Nu»** scoate bifa. După «Da» nu mai apare niciun mesaj, decât dacă ceva nu merge. La un document deja listat caseta e blocată: numărul doar crește.

Numărul crește și când tipărești documentul din pagina **Document**.

## Documentul și semnarea
<!-- slice: 0078, 0041, 0000-14, 0078-08, 0099 -->

Pe pagina **Document**:

- Dacă ordonanțarea nu are încă PDF, apasă **«Generează»**: K-BOT face documentul din datele
  salvate.
- Documentul se semnează în Adobe, chiar în fereastra K-BOT. După fiecare semnătură, K-BOT îl
  urcă pe server.
- Cât Adobe încă deschide documentul, arborele din stânga nu primește alt rând — așteaptă câteva
  secunde ([Cât se deschide un document](topic:contabil.liste)).
- **Un document semnat nu se mai generează niciodată din nou** — ajunge o singură semnătură.
  «Generează» pe o ordonanțare semnată răspunde «Documentul are cel puțin o semnătură, deci nu se
  mai generează din nou.»

<!-- capture: ord-document | caption: Pagina «Document» a unei ordonanțări semnate | goto: view:ord | prepare: Selectați o ordonanțare semnată din arbore și deschideți pagina «Document». -->

<!-- capture: ord-semnaturi | caption: Semnăturile ordonanțării, în josul documentului | goto: view:ord | prepare: Selectați o ordonanțare semnată și deschideți pagina «Document»; derulați la semnături. -->


### Pașii în Adobe
<!-- slice: 0000-10, 0000-14 -->

Documentul se validează și se semnează **pe rând**. Butoanele **«Validare formular»** și
**«Anulare Validare»** sunt jos, în dreapta. Documentul făcut de K-BOT are **tot tabelul
completat**, deci nu scrii nimic în el: doar validezi și semnezi.

| Etapă | Cine | Ce face |
|-------|------|---------|
| 1 | compartimentul de specialitate | **«Validare formular»** → **semnătura 1**. Formularul spune apoi «Completați coloanele 1-3 ale tabelului...»: sunt deja completate, nu ai nimic de făcut. |
| 2 | persoana cu acces la sistemul de control al angajamentelor | **«Validare formular»** (acum se verifică tot tabelul) → **semnătura 2**. Poate fi altă persoană, pe alt calculator, altă zi. |
| 3 | CFP (dacă e cazul) | semnăturile 3 / 4 |
| 4 | ordonatorul de credite | **semnătura 5** — documentul e complet |

> **Semnătura 1 se pune doar pe un tabel complet.** Formularul Ministerului pare să permită
> completarea coloanelor 1-3 după semnătura 1, dar asta merge doar cât documentul rămâne deschis:
> la redeschidere coloanele sunt din nou blocate și goale, iar validarea dă erori ca «Nu ați
> completat coloana Cod SSI» sau «Nu se acceptă valori negative în coloana 5». Un astfel de document
> nu se mai poate repara, iar fiind semnat nu se mai generează din nou. Documentul făcut de K-BOT are
> tabelul complet, deci nu intră în această capcană — nu-l modifica în Adobe înainte de semnătura 1.

Rostul celor două etape nu e completarea, ci **răspunderea**: prin semnătura 1 compartimentul de
specialitate răspunde de suma ordonanțată (coloana 4); prin semnătura 2 persoana nominalizată
răspunde de coloanele 1, 2, 3 și 5. Înainte de semnătura 2 apasă totuși **«Validare formular»**:
verificarea întregului tabel se face abia acum.

Dacă validarea găsește greșeli **înainte de semnătura 1**, le arată pe toate într-un singur mesaj.
Corectează ordonanțarea în K-BOT («Modifică ordonanțarea») și generează din nou documentul — cât
timp nu e semnat, se poate.

**«Anulare Validare»** readuce formularul la început (coloana 4 se poate edita din nou), dar
**doar cât nu e pusă nicio semnătură**. După prima semnătură răspunde «Am semnături aplicate, nu pot
anula validarea». De aici documentul rămâne așa cum a fost semnat: **verifică totul înainte de
semnătura 1**.

### Semnăturile
<!-- slice: 0078, 0000-10 -->

| Semnătura | Cine semnează | Obligatorie |
|-----------|---------------|-------------|
| 1. **Compartiment de specialitate** (stânga) | compartimentul care propune plata (coloana 4) | **da** |
| 2. **Compartiment de specialitate** (dreapta) — «În calitate de persoană nominalizată pentru a avea acces la sistemul de control al angajamentelor, răspund pentru corectitudinea informațiilor din col. 1, 2, 3 și 5» | persoana cu acces la sistemul de control al angajamentelor | **da** |
| **Alte Avize** (6 câmpuri) | alte avize cerute la voi; se pot pune între semnăturile 1 și 2 | nu |
| **Verificat/Avizat** (6 câmpuri) | se pot pune după semnătura 2, înainte de CFP propriu | nu |
| 3. **Control financiar preventiv propriu** | CFP propriu | nu |
| 4. **Control financiar preventiv delegat** | CFP delegat | nu |
| 5. **Ordonator de credite** — «Aprob cele prevăzute în prezentul document» | ordonatorul | **da** |

- **Semnătura 2 blochează tot documentul**: după ea nu se mai poate schimba nimic în tabel.
- **Semnătura ordonatorului închide semnăturile CFP.** Dacă documentul trebuie să aibă viza CFP,
  ea se pune **înainte** de semnătura ordonatorului.
- Documentul e **complet** cu semnăturile **1, 2 și 5**. Pe baza lui se poate face recepția în
  FOREXE.

> Ce cere Ministerul Finanțelor la trimiterea ordonanțării în FOREXE (transformarea ei în recepție
> și încărcarea pe serverul CAB) nu e descris încă în acest ajutor: K-BOT nu face deocamdată acest
> pas.
