# Plan - K-BOT afla singur cand o receptie isi schimba data sau este stearsa in browserul FOREXE

Felie: **0076** (editarile din «Browser FOREXE»), trei subfelii noi: **0076-01** (data schimbata), **0076-02** (stergerea), **0076-03** (plasa la descarcare). Numerele sunt libere (0076 nu are subfelii numerotate). Text fara diacritice, ca restul documentelor de lucru.

## Context

Problema (06.11.2026): un user a schimbat data unei receptii **in afara K-BOT**. Receptiile din `FX_Receptii_R` se potrivesc cu `ListaReceptii` dupa **zi** (`DATE(DataR)`, pasul 4b, `prelucrare_pasi.py` - `_R_CANDIDATI_SQL` + `_potriveste_ziua`). Cu data schimbata nu mai exista candidat in ziua noua, deci 4b a **inserat un R nou** (cu `RHR`), iar R-ul vechi a ramas cu lantul lui de instantanee: lantul de receptii s-a rupt. Fundamentul stie riscul: **F30** (`FUNDAMENT_Asociere_Receptii.md`) il numeste «pericol cunoscut, nerezolvat», cu mentiunea ca nu s-a vazut niciodata in practica.

Stergerea e simetrica: o receptie stearsa pe site dispare din `ListaReceptii`, iar K-BOT nu afla nimic pana nu vine randul din istoric «Stergere receptie» (F21) si operatorul il aseaza in fereastra de asociere. Pana atunci `R.Sters` ramane 0.

Ce exista deja si se refoloseste:
- `src/KBot.Forexe/Services/JavaScripts/ForexeWatch.js` urmareste deja editarea unei receptii in browserul din K-BOT: operatia `receptie-modificare` retine **data randului la apasarea ochiului** (`state.rowDate`, vechea) si **data din formular la Salveaza** (`state.formDate`, noua) si le trimite in evenimentul `finished` (`finishedData()`: `{dataReceptie, rowDate, formDate}`). Deci schimbarea datei se vede deja in pagina, doar ca nimeni nu o foloseste: `KbotForm.ForexeWatch.vb` (`DataReceptieDin`) ia doar data noua si descarca un singur rand dupa ea - exact ce naste R-ul nou.
- `ForexeWatchEvent.vb` (`ForexeOperationKind`, `ParseOperation`), `KbotForm.ForexeWatch.vb` (`TrateazaOperatiuneaCapturata` -> `PreiaOperatiuneaAsync` -> `DescarcaPentruOperatiuneAsync`), coada robotului (`_robotQueue`).
- Server: `routes/forexe/marcaj.py` (marcajul «(IDRH: n; IDR: m)» pus in descrierea receptiei la fiecare salvare facuta din K-BOT), `prelucrare_pasi.py` (`_potriveste_ziua`, `step4b_receptii_prelucrare`), `prelucrare_asociere.py` (`pas4c_automat`, `_R_MARCHEAZA_STEARSA_SQL`), `asociere.py` (aceleasi SQL-uri pentru editorul oricand).

## Decizii luate (operator, 06.11.2026)

1. **Stergerea**: `R.Sters = 1` **imediat** + semn separat (coloane noi: cand si de cine a fost observata). Randul «Stergere receptie» care vine apoi se leaga singur de acea receptie.
2. **Plasa la descarcare**: da (subfelia 0076-03) - pentru schimbarile facute in afara K-BOT.
3. **Felia**: subfelii noi in 0076.

## Principiu comun (de ce rute directe, nu prin pachetul de ingest)

Schimbarea de data si stergerea sunt **fapte deja petrecute pe site** (salvarea e confirmata de pagina), nu decizii ale operatorului. De aceea nu trec prin contractul in doua faze (care deruleaza inapoi si se poate abandona): se inregistreaza **imediat**, prin doua rute mici, **inainte** de descarcarea care urmeaza. Astfel ingestul de dupa gaseste deja R-ul cu data buna / marcat sters si nu mai are nimic special de facut. Daca descarcarea se pierde (robotul ocupat, eroare), faptul ramane inregistrat.

Siguranta: observatia e scrisa **local pe disc** (ca `AsociereStore`, in `KBotPaths`) inainte de apelul de server si stearsa doar dupa raspuns OK; la urmatoarea descarcare/pornire se reia. Aici intra si cazul `_urmarireInLucru` (o operatiune anterioara in lucru): operatiunea se arunca azi, observatia NU trebuie sa se piarda.

## 0076-01 - data schimbata in browserul K-BOT

**Browser (JS)** - `ForexeWatch.js`, `startOperation` pentru `receptie-modificare`: pe langa `rowDate` se retine **identitatea randului** din `rowReadOf(el)` (tot randul razuit: Data, Suma, Descriere...) plus pozitia lui in lista (`idx + 1` = `NrCrtForexe`) si rangul in ziua lui (cate randuri cu aceeasi data il preced). Se trimit in `finishedData()` ca `rand`, `pozitie`, `rang`. Fara schimbare pentru receptia noua.

**Shell (VB)** - `KbotForm.ForexeWatch.vb`, `PreiaOperatiuneaAsync`: pentru `ReceptieModificare` cu `rowDate <> formDate` (amandoua valide), **inainte** de `DescarcaPentruOperatiuneAsync`:
1. scrie observatia local;
2. apeleaza ruta noua (mai jos) prin `WithReauth`;
3. OK -> continua descarcarea ca azi (acum 4b va gasi R-ul pe data noua);
4. 409 ambiguu / eroare -> **nu** descarca randul singur (ar crea duplicatul); mesaj in romana prin `KBotMessage.Show` (stanga Nu/Renunta, dreapta OK - regula butoanelor) si observatia ramane pentru reluare.

**Server** - fisier nou `PYTHON/routes/forexe/receptii_observate.py` (pastrat sub ~1000 de linii), `POST /api/forexe/receptii/data-schimbata`, `@require_session`, `db_name` din sesiune:
- intrare: `cod`, `data_veche`, `data_noua`, `suma_veche`, `rang`, `pozitie`;
- alege R-ul din ziua veche cu **`_potriveste_ziua`** (refolosit, nu rescris): `RangZi`, apoi valoare, apoi IDRR; `Sters = 0`;
- un singur candidat sigur -> `UPDATE FX_Receptii_R SET DataR = noua, RangZi = NULL` (NULL = «rand vechi» pentru `_potriveste_ziua`, care il potriveste dupa valoare exacta si il re-stampileaza la urmatorul 4b);
- **idempotent**: daca in ziua veche nu mai e nimic si in ziua noua exista exact un R cu aceeasi suma -> 200 «deja facut»;
- ambiguu (mai multi candidati nedeosebiti) sau niciunul -> 409, **nu scrie nimic**, mesaj clar; fara fallback tacut;
- tranzactie proprie, jurnal in `journal.line`, `ensure_ascii=False` ca restul rutelor.

**Client HTTP** - `IApiClient`/`ApiClient` (`KBot.Api`) + DTO in `KBot.Domain`; local: `ObservatiiReceptiiStore` in `KBot.Common` (model `AsociereStore`).

## 0076-02 - stergere in browserul K-BOT

**Necunoscut de rezolvat INAINTE de cod (nu pot verifica de aici, site-ul cere login):** cum se sterge o receptie pe FOREXE - iconita/butonul din rand si fereastra de confirmare. Am nevoie de **HTML-ul randului din tab «Receptii» (cu butonul de stergere) si al ferestrei de confirmare**, ori o inregistrare cu Recorder-ul. Pe baza lor se scriu selectorii; fara ei nu implementez (altfel ghicesc selectori).

**Browser (JS)** - operatie noua `receptie-stergere` in `OPS`: `when: 'li.tab1.active'`, `start` = click pe butonul de stergere al randului (retine `rowReadOf` + pozitie + rang, ca mai sus), `finish` = butonul de confirmare, `done` = pagina inapoi pe lista **si** randul a disparut (numar de randuri scazut / nu mai exista Data+Suma). Renuntarea din fereastra de confirmare = `cancelled`, nu `finished`. Se adauga in `OP_FORMS`/garzi doar daca fereastra de confirmare e o forma urmarita.

**Shell (VB)** - `ForexeOperationKind.ReceptieStergere = 6` + `ParseOperation("receptie-stergere")` (`ForexeWatchEvent.vb`); in `PreiaOperatiuneaAsync`: observatie locala -> ruta -> apoi **descarcarea nodului cu istoric** (`DownloadNodeAsync`), nu «Receptie Editata» (receptia nu mai exista in lista, iar randul «Stergere receptie» e in istoric) -> ingest normal. Pozele de receptie (`TakeReceptionPicturesAsync`) NU se fac pentru stergere.

**Server** - aceeasi fisier: `POST /api/forexe/receptii/stearsa`: identifica R cu `_potriveste_ziua` pe `(data, suma, rang)`; `UPDATE FX_Receptii_R SET Sters = 1, StersObservatLa = NOW(), StersObservatDe = <utilizator>`. Idempotent (deja sters-observat -> 200).

**DDL** (`sql/0076_02_sursa.sql`, pe modelul `sql/0111_01_sursa.sql`; se aplica si pe `AVACONT_SURSA`, coloane noi cu valoare implicita pe care migrarea nu le scrie; `utf8mb3`):
`ALTER TABLE FX_Receptii_R ADD COLUMN StersObservatLa DATETIME NULL, ADD COLUMN StersObservatDe VARCHAR(100) NULL;` - rulat de operator, de mana; **nu fac deploy**.
- `StersObservatLa` = data si ora la care K-BOT a aflat din browser ca receptia a fost stearsa. Nu e doar jurnal: **daca e completata, receptia e «stearsa dovedit de FOREXE»** - asta o deosebeste de un `Sters = 1` pus de operator din asociere, si pe ea se sprijina blocajul de mai jos.
- `StersObservatDe` = numele utilizatorului K-BOT care lucra in browser atunci (sa se stie cine a facut stergerea). Daca nu vrei istoric pe persoana, se scoate si ramane o singura coloana.

**Legarea randului «Stergere receptie»** (`prelucrare_asociere.py`, `pas4c_automat`): un instantaneu cu `EsteStergere = 1` fara asezare, al carui `Total` = `SumaAntet` (2 zecimale) al **unui singur** R cu `StersObservatLa IS NOT NULL` si fara alt instantaneu de stergere asezat -> se asaza automat pe acel R. **Scutit de pragul F37 (31 zile)** - data stergerii poate fi la luni dupa `DataR`; legatura vine din observatie, nu din apropierea datelor. Mai multe potriviri -> ramane operatorului, ca azi.

**`Sters` observat = blocat fata de asociere (cerinta operatorului).** O stergere confirmata de FOREXE (in browser sau prin plasa 0076-03 confirmata de operator) nu mai poate fi atinsa din ferestrele de asociere. Singurele doua locuri din cod care pun `R.Sters = 0` sunt la dezlegarea instantaneului de stergere: `asociere.py` (~l.512, `_R_..._SQL`) si `prelucrare_asociere.py` (~l.1544). Amandoua primesc conditia `AND StersObservatLa IS NULL`, deci dezlegarea/mutarea randului «Stergere receptie» nu poate readuce receptia la viata. Verific la implementare, cu grep pe `Sters` in `routes/forexe`, ca nu mai exista alta cale care o reseteaza; iar daca fereastra de asociere ar permite oricum o actiune pe aceasta receptie (de ex. sa o marcheze nesterasa), actiunea se refuza cu mesaj in romana.

**Risc acceptat (spus pe fata):** cu `Sters = 1` imediat, cititorii care filtreaza `Sters = 0` (arborele Receptii, e-factura `primite_ke.py`, ordonantare `ord_edit.py`) ascund receptia din clipa stergerii, **inainte** sa existe randul de stergere; F22 vrea ca ea sa mai conteze pentru platile dinainte de data stergerii. Fereastra e scurta (istoricul vine in aceeasi preluare si se asaza automat), dar daca descarcarea cade, receptia ramane ascunsa pana la reluare. De aceea receptia primeste un semn in tooltip-ul din vederea Receptii («stearsa pe site, astept istoricul») cat timp nu are instantaneu de stergere asezat.

## 0076-03 - plasa la descarcare (schimbari facute in afara K-BOT)

In `step4b_receptii_prelucrare`, **inainte** de potrivirea pe zile, doar cand pachetul e **lista completa** de receptii (nu «Receptie Editata», care trimite un singur rand - altfel ar parea ca restul au disparut): se calculeaza R-urile salvate fara randuri pe ziua lor («orfane») si randurile din payload fara R pe ziua lor («noi»), apoi se leaga in ordinea increderii:
1. **Marcajul**: descrierea unei receptii editate vreodata din K-BOT poarta «(IDRH: n; IDR: m)» (`extract_marcaj`). `FX_Receptii_H(IDRH).IDRR` da R-ul, oricare ar fi data. Incredere mare -> se aplica singur, cu rand in jurnal si avertisment.
2. **Descriere + suma** identice, pereche 1-la-1 unica -> **propunere**: in raspunsul de propunere, cu intrebare Da/Nu pentru operator (in fluxul din `KbotForm.Ingest.vb`; decizia calatoreste in faza 2 ca restul deciziilor).
3. Orfane ramase fara pereche = **posibil sterse in afara K-BOT**: doar **avertisment** + propunere de marcare `Sters`, niciodata automat (o lista taiata ar marca pe nedrept receptii; exista deja detectia de `Detaliu` taiat, slice 0091, dar nu pentru lista intreaga).
Aplicarea foloseste exact aceleasi doua functii ca rutele din 0076-01/02 (o singura sursa a regulii).

## Dincolo de cod (regulile proiectului)

- Fiecare subfelie: worklog `docs/worklog/SLICE-0076-0x-*.md` (4 sectiuni obligatorii), randul in `state/KBOT_STATUS_0070-0079.md` + indexul din `KBOT_STATUS.md`; la inchidere se taie pasii intermediari (regula md-final-state-only).
- `FUNDAMENT_Asociere_Receptii.md`: amendament la **F30** (schimbarea de data din K-BOT nu mai e pericol; ramane doar cea din afara, acoperita de 0076-03) si la **F21** (stergerea observata).
- Ajutor (slice 0000-NN): mesajele noi pe care operatorul le vede + semnul din tooltip, cu eticheta `<!-- slice: 0076-0x -->`; fara detalii interne.
- Cod fara diacritice (RULE 0), prefix `k_` la parametri/locale la risc, `Try/Catch` cu `GlobalErrorLog.Write` conform politicii (rutele HTTP si I/O: log + `Throw`; handlerele UI: log + inghit).
- Fara teste, fara rulare, fara git, fara deploy, fara `config.py` (reguli permanente). Fisierele Python/JS ajung pe server doar prin mana operatorului; `Register-DeploymentTask.ps1` numai dupa confirmarea lui dupa test.

## Verificare

Build curat al proiectelor atinse (`KBot.Forexe`, `KBot.Api`, `KBot.Common`, `KBot.Domain`, `KBot.App`) cu `dotnet build` pe proiecte, nu pe `KBot.sln` cu teste. Proba facuta de operator pe `000_DEMO`, in browserul FOREXE din K-BOT:
1. schimba data unei receptii care are lant -> `SELECT IDRR, DataR, RangZi FROM FX_Receptii_R WHERE CodAngajament = '...'`: acelasi `IDRR`, data noua, **niciun R nou**; lantul de instantanee ramane pe acelasi `IDRR`;
2. schimba data a doua receptii din aceeasi zi -> se muta cea corecta (rang/valoare); un caz fortat ambiguu -> mesaj, nimic scris;
3. sterge o receptie -> imediat `Sters = 1`, `StersObservatLa` completat; dupa preluare randul «Stergere receptie» apare asezat singur pe ea (`FX_Receptii_H.IDRR` = ea, `EsteStergere = 1`);
4. descarcarea cade/e ocupata dupa o schimbare -> fisierul local ramane, se reia la urmatoarea descarcare;
5. 0076-03: schimba data **in afara** K-BOT pe o receptie editata cu marcaj -> se leaga singura; pe una fara marcaj -> intrebare Da/Nu.

## De clarificat cu operatorul inainte de 0076-02

HTML-ul butonului de stergere din randul de receptie si al ferestrei de confirmare de pe FOREXE (sau o inregistrare cu Recorder-ul).
