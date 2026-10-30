<a id="slice-0059"></a>

# SLICE 0059 — «Începe o recepție nouă», din coșul instantaneelor neașezate

**Ordinea etapelor:** baza introduce gestul UI care lipsea în 0055; 0059-02 stabilește că recepția pornită aici este ștearsă/reconstituită și înlocuiește marcarea manuală a sfârșitului cu citirea capetelor lanțului. Probele modului de oricând nu confirmă modul propunere sau salvarea MariaDB. Mențiunea istoricului întreg privește cazul înainte de prima descărcare; [0068](SLICE-0062-receptii-neasezate-si-refacere-din-istoric.md#slice-0068) documentează ulterior istoricul diferențial REVERSE.

Cererea operatorului: în `AsociereForm`, clic dreapta pe un instantaneu din arborele **Neașezate** trebuie să ofere și opțiunea de a porni de la el o **recepție nouă (R)**.

---

## 1. Ce lipsea, de fapt

Coșul din dreapta avea până acum exact două ieșiri: **pune instantaneul pe o recepție de aici** (tragere) sau **nu consemnează nicio schimbare** (F17, din meniu). Amândouă presupun că recepția lui există undeva.

F26: recepția creată/ștearsă înaintea primei descărcări lipsește din ListaReceptii și FX_Receptii_R, dar instantaneele rămân în istoricul întreg. Fără țintă, coșul nu avea ieșire corectă; fără schimbare ar fi fost fals.

**Contractul exista, gestul UI lipsea.** `reconstituire` / `receptie_noua` erau implementate și testate în `routes/forexe/asociere.py`, `routes/forexe/prelucrare_asociere.py` (§4c-bis: `verifica_etichetele`, `materializeaza_receptiile_reconstituite`), `ApiClient` și `ComandaAsociere.ReceptieNoua` / `DecizieAsociere.ReceptieNoua`. Formularul nu putea produce acțiunea; felia adaugă acel capăt de client.

Nicio linie de Python nu s-a atins.

---

## 2. Cum e ținută o recepție nouă în formular

**IDRR negativ în `_pozitie`.** Recepția nouă este mulțimea instantaneelor cu același număr negativ. `_pozitie` rămâne evidența unică; `_receptiiNoi` este proiecția refăcută de `ActualizeazaReceptiileNoi` la fiecare `Reconstruieste`.

Trei lucruri ies gratis din alegerea asta:

- **Recepția dispare singură** când pleacă de pe ea și ultimul instantaneu — nu «rămâne goală» și nu trebuie ștearsă de nimeni.
- **Toate suprafețele o văd fără să știe de ea**: arborele din stânga, graficul, benzile și țintele de tragere citesc acum `Receptiile()` (server + pornite aici) în loc de `_stare.Receptii`. Cheia de nod e `R:-1`, cheia de serie `R-1`, iar `IdrrDinCheie` le citește ca pe oricare altele.
- **`_stare` rămâne neatins**, adică rămâne adevărul de pe server, cu care se compară ce s-a schimbat.

La fiecare mutare, cifrele sunt recalculate din lanț după regula de salvare a serverului: `DataR` = momentul celui mai vechi instantaneu; `SumaAntet` = totalul rândului de ștergere.

`EtichetaTrimisa(-1)` = "R1". Numere nereciclate pe durata ferestrei: reutilizarea ar declara aceeași etichetă de două ori pe server, unde este permisă o singură declarare.

---

## 3. Ce pleacă pe fir

Serverul cere, pentru fiecare lanț reconstituit: exact o `reconstituire` (care declară eticheta), exact o `stergere`, iar ștergerea să fie **ultima** din lanț.

`Comenzi()` (modul de oricând) și `DeciziiDin` (modul propunere) traduc la fel, prin aceeași pereche de funcții — `Declarantii…` alege **o dată** cine declară (primul instantaneu după `DataH`), iar `ActiuneaPeReceptieNoua` dă acțiunea rândului:

| rândul | acțiune | ce poartă |
|---|---|---|
| primul din lanț | `reconstituire` | `receptie_noua` |
| marcat «rândul de ștergere» | `stergere` | `receptie_noua` |
| restul | `asociat` | `receptie_noua` |

`Idrr` rămâne **nescris** pe toate trei: serverul cere exact una dintre `idrr` și `receptie_noua`, iar recepția nu are încă o cheie de numit.

Declarantul se alege o dată, înaintea buclei; alegerea per rând ar putea emite două reconstituiri pentru etichetă ori niciuna.

---

## 4. Refuzurile care nu ajung la server

**F14 nu se aplică recepției noi**, fără linii pe indicator până la salvarea lor din instantanee: ar respinge chiar primul instantaneu creator. F16 rămâne, fiind regula creșterii mulțimilor între instantanee.

**ProblemaReceptiilorNoi verifică §4c-bis înaintea salvării**, în ambele moduri: minimum două instantanee, exact o ștergere, ultima în lanț. Altfel serverul respinge tot lotul; local butonul rămâne inactiv și banda explică lipsa.

**Renunțarea** e în meniul rândului-recepție: pune înapoi în coș toate instantaneele, iar recepția piere cu ele. Fără ea, o recepție pornită din greșeală s-ar desface doar desprinzând instantaneele unul câte unul.

---

## 5. Două lucruri prinse abia pe ecran

Formularul a fost desenat cu `DrawToBitmap` peste un tablou nascocit (trei stări: recepție nouă cu un instantaneu, cu două, și închisă cu rândul de ștergere). Amândouă defectele de mai jos treceau de toate testele.

1. **Textul lung ascundea partea dreaptă:** „— lipsește rândul de ștergere” nu încăpea în rândul-recepție, iar arborele omite întreaga zonă aliniată dreapta, inclusiv suma/numărul instantaneelor. Rămâne `[recepție nouă]`; explicația trece în tooltip/bandă.
2. **Avertisment persistent:** reconstruirea îl scria fără ștergere la rezolvare. Acum se retrage numai dacă aparține acestei logici (`_avertismentReceptiiNoi`); Clear necondiționat ar pierde și „legăturile au fost salvate” după reîncărcare.

---

## 6. Fișiere atinse

| fișier | ce |
|---|---|
| `src/KBot.App/Forexe/AsociereForm.vb` | tot ce e mai sus (+331 / −13) |
| `tests/KBot.App.Tests/AsociereFormTests.vb` | 8 teste noi + `StareCuDouaNeasezate` și trei ajutoare de reflexie |
| `tests/KBot.App.Tests/AsociereDeciziiTests.vb` | 3 teste noi, pe drumul propunerii |

Nimic în `KBot.Domain`, `KBot.Api` sau `PYTHON/` — contractul era deja acolo.

---

## 7. Rezultatele testelor

| suită | rezultat |
|---|---|
| `dotnet build src\KBot.App\KBot.App.vbproj` | **0 erori**, 7 avertismente MSB3825 (BinaryFormatter în `.resx`, preexistente pe tot proiectul) |
| `dotnet test tests\KBot.App.Tests --filter ~Asociere` | **41 trecute / 0 picate** |
| `tests\KBot.App.Tests` (întreg) | 238 trecute / **13 picate** |
| `tests\KBot.Api.Tests` | 103 trecute / **1 picat** |
| `tests\KBot.Domain.Tests` | 28 trecute / **3 picate** |

**Cele 17 picate sunt preexistente și nu au legătură cu felia asta** — verificat prin `git stash` al celor trei fișiere: pe arborele fără modificări suita `KBot.App.Tests` dă **exact aceleași 13 picate** (227 trecute în loc de 238, adică toate cele 11 teste noi trec). Toate sunt din familia DDF/revizii (`DdfXfaParserTests`, `DdfViewTests`, `IstoricViewTests`, `XfaXmlPreviewTests`, `MainFormNavItemsTests`, `ApiClientTests.GetDdf_FormatsRevisionLabel…`, `DdfInfoTests.EtichetaRevizie…`).

`dotnet test KBot.sln` **nu s-a rulat** — deschide ferestre Adobe reale pe ecranul operatorului (regula casei).

---

## 8. Ce a rămas neverificat

- **Nu s-a salvat nimic pe o bază vie.** Serverul nu s-a pornit. Că lanțul `reconstituire` + `stergere` cu aceeași etichetă e ce așteaptă `verifica_etichetele` / `materializeaza_receptiile_reconstituite` e citit din codul Python, nu observat pe o rulare.
- **Nu s-a rulat suita PYTHON** — n-a fost atins niciun fișier Python.
- **Fereastra mare de benzi** (`AsociereBenziForm`) arată recepțiile noi și primește trageri pe ele (împrumută aceiași trei tratatori), dar **nu s-a văzut pe ecran** în sesiunea asta, și nu are niciun gest de creare — recepția se pornește doar din arbore.
- **Modul propunere nu s-a văzut pe ecran** deloc; drumul lui e acoperit numai de testele pe `DeciziiDin`. Capturile sunt toate din modul de oricând.
- `KBotLaneView` tot **nu are niciun test** (semnalat și de felia 0058).

---

<a id="slice-0059-02"></a>

# 0059-02 — corectură: recepția pornită aici e ȘTEARSĂ prin definiție

Observația operatorului, după prima citire pe ecran: *«recepția pornită e întotdeauna o recepție ștearsă. NU există fel în care un R pornit de mână să nu fie șters. Deci asta trebuie să se vadă și în mesaj, și în culoare. În rest e ca un R normal — intră în calculul ordonanțărilor și e parte din grafic și din benzi.»*

Are dreptate, și taie o întrebare pe care prima variantă o punea degeaba.

## 1. Rândul de ștergere nu mai e o hotărâre, e o citire

F26 impune recepție creată/ștearsă înaintea primei descărcări; una încă vie ar fi venit în ListaReceptii. Rândul ștergerii se deduce astfel: ultimul instantaneu al lanțului.

- `_stergereNoua` ține IDRH-urile care închid lanțurile pornite aici — tot o proiecție a lui `_pozitie`, refăcută în aceeași trecere cu `_receptiiNoi`.
- EsteStergere citește steagul operatorului pentru recepție server (posibil încă vie); pentru nouă, citește capătul lanțului și ignoră `_stergere`, evitând afișarea unui steag vechi absent de pe fir.
- În meniul unui instantaneu de pe o recepție pornită aici nu se mai oferă «Este rândul de ștergere» — o intrare care nu schimbă nimic e mai rea decât niciuna.
- `Comenzi` și `DeciziiDin` iau amândouă capetele din aceeași ordonare (`CapeteLant`: `Declara` = primul, `Inchide` = ultimul), nu din dicționarul de ștergeri.
- Din cele patru verificări dinainte de salvare **a rămas una**: lanțul are nevoie de cel puțin două instantanee. Celelalte trei (zero ștergeri, două ștergeri, ștergerea la mijloc) nu se mai pot întâmpla de când capetele se citesc, nu se marchează.

## 2. «În mesaj»

- `[nouă, ștearsă]` înlocuiește `[recepție nouă]`; evită repetarea aceleiași informații prin trei semne Sters/Reconstituit, care ar ascunde întreaga zonă dreaptă dacă nu încape (§5 inițial).
- Proiecția poartă acum `Sters = True` și `Reconstituit = True`, fiindcă exact așa o scrie `_R_INSERT_RECONST_SQL`. Rândul de pe ecran arată ce se va scrie.
- Tooltipul spune deschis «se va scrie ca recepție ȘTEARSĂ și reconstituită — o recepție de aici există tocmai fiindcă a fost ștearsă», și că ultimul instantaneu e ștergerea ei.
- Suma se citește «Valoarea la ștergere», nu «Valoare acum»: «acum» ar fi o minciună pentru o recepție care nu mai există.

## 3. «Și în culoare»

**Fond pentru ștergere:** pe Tot angajamentul, textul identifică linia graficului prin culoare. Suprapunerea celor două semnificații pe text ar pierde una, mai ales în timpul tragerii; fondul liber le păstrează simultan pe ambele file.

Paletă `SurfaceAlt` + 14% `TextDim`: SurfaceAlt singur era alb/invizibil pe temele deschise, verificat pe ecran. Amestecul se inversează în dark, fără culoare literală.

Nuanța discretă se aplică tuturor recepțiilor șterse, inclusiv existente, pentru același fapt. Recepția rămâne întreagă, cu totalul istoric citit de ordonanțări (§1.3).

## 4. «În rest e ca un R normal»

Două locuri unde nu era, amândouă din același `> 0` care nu lăsa niciodată un IDRR negativ să treacă:

- `SincronizeazaCulorile` colora rândul-recepție cu culoarea liniei din grafic doar pentru `idrr > 0` — recepția pornită aici era singura al cărei rând nu se lega de linia lui.
- `AplicaCulorileBenzii` la fel, pentru bandă.

Amândouă sunt acum `<> 0`. `ValoareaLa` (deci și totalul din grafic, și reperele plăților) cădea deja corect la zero după ștergere, fiindcă citește `EsteStergere`.

## 5. Un defect prins iar numai pe ecran

`Amesteca` producea OverflowException: Color.R și scăderea a doi Byte rămâneau Byte, deci diferența negativă eșua. Granița UI a arborelui înghițea excepția conform regulii, oprind desenul după rândul-recepție, fără instantanee/coș, deși testele treceau. Canalele sunt convertite la Double înaintea scăderii.

## 6. Alt defect, din felia 0059

`AratMeniul` folosea PozitiaLui(inst) > 0; IDRR negativ al recepției noi era confundat cu coșul, oferind fără schimbare/recepție nouă și ascunzând desprinderea. Acum verifică <> 0.

## 7. Rezultate

| suită | rezultat |
|---|---|
| `dotnet build src\KBot.App\KBot.App.vbproj` | **0 erori, 0 avertismente** |
| `--filter ~Asociere` | **47 trecute / 0 picate** |
| `tests\KBot.App.Tests` (întreg) | 241 trecute / **13 picate** — exact cele preexistente (DDF/revizii/Istoric) |

Văzut pe ecran (`DrawToBitmap`, 1500×950, modul de oricând): recepție nouă cu un instantaneu (bandă de avertisment, salvare stinsă), cu două, cu trei și închisă, plus fila «Tot angajamentul» — acolo se vede și rândul verde legat de linia lui verde, cu fondul de recepție ștearsă peste. **Modul propunere tot nu s-a văzut pe ecran**, și tot **nu s-a salvat nimic pe o bază vie**.

## Reper din focusul registrului

Focusul numea căderea `Amesteca` a treia situație consecutivă în care defectul relevant fusese prins numai pe ecran, deși testele treceau; nu este o probă suplimentară față de cele descrise în etape.
