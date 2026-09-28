# K-BOT — STATUS, slices 0080–0089

Moved verbatim out of `../KBOT_STATUS.md` (28.09.2026). The index there says what
each slice is; this file holds everything recorded about it: its registry row, its
«Current focus» notes and its «Open threads» notes.

---

## Slice 0080

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0080-01 | **`IdClsf` = `Clasificatii.IDClsf` pe șapte tabele FX_ + `FX_Extrase.DataDoc` → DATE** — cererea operatorului din 24.09.2026: pe `FX_Extrase_H`, `FX_Indicatori`, `FX_Istoric`, `FX_Plati`, `FX_Receptii`, `FX_Receptii_RHR`, `FX_Rezervari` `IdClsf` devine cheia MariaDB, FĂRĂ copie a id-ului Access în tabele (operatorul); datele mixte din `DataDoc` devin DATE | GATA pe cod (`py_compile` verde; `dotnet build src\KBot.Migrator` 0 / 0) / **scriptul unic nerulat, nimic pe server, nicio suită rulată** | `SLICE-0080-01-idclsf-pereche-datadoc.md` | Script unic `scripts/extrase_clsf_0080.py` (`--dry-run`, `--db`): faza 1 verifică TOATE bazele și se oprește fără să atingă nimic; faza 2: mysqldump, coloană temporară, o tranzacție, apoi `IdClsf` primește comentariul-marcaj `Clasificatii.IDClsf (0080-01)`. Tabelele convertite de mână (au deja `IdClsfAcc`, ex. `FX_Plati`) doar se verifică, `IdClsfAcc` se șterge, se marchează. Ambele migratoare traduc id-ul Access rând cu rând și refuză o țintă nemarcată. Migratorul VB: `DataDoc` citit ca dată, «Rânduri Access» completat, `FX_Angajamente` / `FX_Istoric` / `FX_Rezervari` urmează unitatea indicatorilor (un `FX_2026.accdb` ține DOUĂ DC-uri), `FX_Istoric` / `FX_Rezervari` fără `CodAngajament` = BLOCANT. Migrator 1.11 ▸ **1.12**. ⚠ `find_id_clsf` OPREȘTE prelucrarea la un ClsfSal dublat. ⚠ Codul de server și scriptul în aceeași fereastră. |
| 0080-02 | **Vederea «Extrase» + pagina «Setări → Extrase»** — cererea operatorului din 24.09.2026: arbore Toate ▸ lună ▸ zi (din `DataBanca`), antete FX_Extrase_H cu operațiile sub ele, ziua cu operațiile + detaliu ca la Plăți, doar angajamentul selectat (`CodContract`), TOTALURI pe debit/credit, descărcare din subsolul arborelui; coloane alese și ordonate din Setări, separat vedere / fereastră | GATA pe cod (`dotnet build src\KBot.App --no-incremental`: **0 erori, 0 avertismente**) / **văzut doar cu `DrawToBitmap` pe date inventate; ruta nerulată; nicio suită rulată** | `SLICE-0080-02-vederea-extrase.md` | Ruta `GET /api/forexe/extrase/lista[?cod=]`, fanion `AreExtrase` în arbore. `ExtrasePanel` comun (mod `Angajament` / `Toate`), coloanele declarate în designer, `ExtraseLayout.Apply` la rulare. `ExtraseColumns` + patru liste în `AppSettings`. `KBotDataColumn.AllowGrouping` (filtru fără grupare pentru Plătitor). Descărcarea mutată în `KbotForm.DescarcaExtraseAsync(owner)`. |
| 0080-03 | **Fereastra «Extrase de cont»** — cererea operatorului din 24.09.2026: toate extrasele bazei (cu / fără angajament), coloane în plus (Cod angajament, Indicator, Explicații), detaliu cu Referință destinatar / Cod program, modală cu maximizare, deschisă din iconița stângă a arborelui principal, buton real de descărcare în subsol | GATA pe cod (build App **0 / 0**) / **fereastra însăși nu a fost desenată; nerulată** | `SLICE-0080-03-fereastra-extrase-de-cont.md` | `ExtraseForm` (`KBotShellForm`, `ShowMaximize`), `ExtrasePanel` în mod `Toate`. Căutarea cu calendar în arbore amânată de operator. |
| 0080-04 | **`IdClsfAcc` doar în `Clasificatii`** — cererea operatorului din 25.09.2026: «it should only be left in Clasificatii - that should be the source of truth». Coloana se șterge de pe `FX_ORD_TBL`, `FX_DDF_REV_SA`, `FX_DDF_REV_SB`, `FX_DDF_REV_PRT`, `Parteneri_Coduri` (pe toate `IdClsf` e deja `Clasificatii.IDClsf`) | GATA pe cod (`py_compile` verde; build App + Migrator 0 / 0) / **scriptul nerulat, nimic pe server, nicio suită rulată** | `SLICE-0080-04-idclsfacc-doar-in-clasificatii.md` | Script unic `scripts/idclsfacc_0080_04.py` (`--dry-run`, `--db`): faza 1 oprește la un rând care ar pierde informație (IdClsfAcc fără IdClsf, IdClsfAcc diferit de nomenclator); faza 2: mysqldump + `DROP COLUMN`. Editoarele ORD / DDF nu mai trimit / scriu `id_clsf_acc`; rutele VBA vechi (`routes/ord`, `routes/ddf`, `parteneri.py`) ignoră valoarea primită și o citesc spre Access din `Clasificatii`. Ambele migratoare refuză o țintă care mai are coloana. Api 1.0.9, Domain 1.2.3, Migrator 1.13. ⚠ Codul de server și scriptul în aceeași fereastră. |

### Current focus

- **Slice 0080 — Extrase (24.09.2026).** 01: `IdClsf` = `Clasificatii.IDClsf` pe șapte tabele (fără
  `IdClsfAcc`) + `DataDoc` DATE (script unic `scripts/extrase_clsf_0080.py`, **întâi `--dry-run`**),
  migratorul VB reparat pe baza jurnalului 014_SCSV; 02: vederea
  «Extrase» + «Setări → Extrase»; 03: fereastra «Extrase de cont»; 04: `IdClsfAcc` șters de pe
  `FX_ORD_TBL` / `FX_DDF_REV_*` / `Parteneri_Coduri` (script `scripts/idclsfacc_0080_04.py`,
  **întâi `--dry-run`**). Nimic rulat pe server.

### Open threads

- **Extrase (felia 0080) — punerea în funcțiune.** Pe VPS, în aceeași fereastră: (1)
  `python -m scripts.extrase_clsf_0080 --dry-run` și citit tot ce listează; (2) rularea reală;
  (3) fișierele de server 0080 (rutele `forexe/*`, `migrare/execute.py`, `utils/clsf_pair.py`,
  `routes/forexe/extrase_lista.py`) + repornire gunicorn. Cod vechi pe bază convertită ▸ `IdClsf`
  citit ca id Access; o bază nemarcată e refuzată de migratoare (rulați scriptul cu `--db`).
  Migratorul VB 1.12 de dat operatorului. `FX_Extrase_H` cu `IdUnitate` 77 / 0 (D10) pot bloca
  la «Verifică» dacă au `IdClsf` nenul — de văzut la rularea următoare. De confirmat de
  operator: un ClsfSal dublat în nomenclatorul unei unități OPREȘTE acum prelucrarea; nodurile
  arborelui Extrase n-au sume în dreapta; operațiile fără `DataBanca` n-au nod de zi (apar doar
  sub antetul lor); fereastra «Extrase de cont» nedesenată; căutarea cu calendar amânată.

---

## Slice 0081

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0081 | **Trimiterea unei revizii DDF din K-BOT în FOREXE (G0-G6)** — cererea operatorului din 25.09.2026: doar fluxul de TRIMITERE (preluarea din FOREXE e completă și nu se atinge); starea reviziei din `Incarcat` + `Semnatura`; Secțiunea B ascunsă până la trimitere; trimiterea prin workflow-urile existente; capturile în `Subform51/Table4`; «Definitivează» / «Derulează» din butonul stâng din subsolul arborelui; KBOT-ul directorului (rol `Director`, mai multe unități) | GATA pe cod — sub-feliile 0081-01 … 0081-10 (nimic rulat live, nicio suită rulată) | vezi rândurile de mai jos | Plan: `docs/PLAN_DDF_Trimitere.md`; fundament: `docs/FUNDAMENT_DocumentFundamentare_CAB.md`; predare: `docs/HANDOFF_0081_DDF_Trimitere.md`. ⚠ Starea NU se deduce din `Incarcat` (importul îl pune singur pe 1): coloană nouă `FX_DDF_REV.StareTrimitere` (decizia operatorului, 25.09.2026). |
| 0081-01 | **Starea reviziei (G0a)** — S0-S4 + S1x dintr-o funcție pură; iconiță în dreapta fiecărei revizii în vederea DDF; `X-Semnatura: -` = fără semnătură | GATA pe cod (build App **0 / 0**; `py_compile` verde) / **SQL nerulat, nevăzut pe ecran, nicio suită rulată** | `SLICE-0081-01-starea-reviziei.md` | `sql/0081_ddf_rev_stare_trimitere.sql` (de rulat pe FIECARE bază). `DdfRevisionStates` în Domain; ruta DDF poartă `stare_trimitere` + `are_rezervari`. |
| 0081-02 | **Editorul: moduri noi, Secțiunea B ascunsă, PDF intermediar (G6a)** — butonul «Angajament nou» în antet (înainte de «An»); iconița STÂNGĂ din subsolul arborelui Rezervări (o singură opțiune, meniu `CustomPopup`); «Adaugă rezervare»; B ascunsă până la trimitere; PDF intermediar fără B; S1 editat → semnătura A și PDF-ul semnat se șterg | GATA pe cod (build App **0 / 0**; `py_compile` verde) / **nevăzut pe ecran, nerulat, nicio suită rulată** | `SLICE-0081-02-editor-moduri-sectiunea-b.md` | `DdfDraftFactory` + `RezervariMenu` (Domain), `IDdfSendApi` separat de `IApiClient` (tiparul `IMarcajApi`), ruta nouă `routes/forexe/ddf_trimitere.py` (`anuleaza-semnatura`). Șase presupuneri scrise în worklog. |
| 0081-03 | **Workflow-urile (G1, G2; G3 renunțat)** — `Definitivare_Derulare` spart în `adlop - Definitivare Angajament.wfl` + `adlop - Derulare Angajament.wfl`, fiecare cu capturile lui; `Incarca Rezervare` V4.1 cu captura «Informații complete contract» (`CAPTURA_INFO_COMPLETE`) | GATA pe fișiere (XML valid; build Forexe **0 / 0**) / **NICIUN flux rulat — fiecare ramură o dată pe 000_DEMO** | `SLICE-0081-03-workflow-uri.md` | `Creare\*.wfl` nu ajungeau deloc în output — acum copiate prin `KBot.Forexe.vbproj`. ⚠ `{{X|…}}` nu are valoare implicită (textul după «|» e descriere). Originalul `Definitivare_Derulare` rămâne până rulează ambele jumătăți. |
| 0081-04 | **Trimiterea (G6b)** — «Trimite în FOREXE» / «Reia trimiterea» din vederea DDF: start (etapa 1) → Creare sau Incarca Rezervare → codurile și capturile salvate imediat (și la eșec) → citire înapoi + verificare față de secțiunea A → importul existent → etapa 2 → PDF final nesemnat (etapa 3); «Definitivează» / «Derulează» / «Generează PDF final» din meniul Rezervări | GATA pe cod (build App **0 / 0**) / **nerulat live, nevăzut pe ecran, nicio suită rulată** | `SLICE-0081-04-trimiterea.md` | Orice oprire înainte de etapa 2 lasă S1x. O Creare întreruptă fără cod citit: operatorul hotărăște (codul se ia din pagina FOREXE). Opt presupuneri în worklog. |
| 0081-05 | **Capturile în PDF-ul final (G5)** — `Subform51/Table4`: rândul-șablon gol + câte un `Row1/Cell1` `image/png` pe captură, în ordinea în care le-au făcut workflow-urile; capturile nu mai intră niciodată ca anexe | GATA pe cod (build App + Xfa **0 / 0**) / **niciun PDF final deschis în Adobe, nicio suită rulată** | `SLICE-0081-05-capturi-table4.md` | Defect real reparat în `KBot.Xfa/AdobeUtils`: celulele de tabel își pierdeau atributele (`xfa:contentType`), deci imaginea ar fi ieșit text. |
| 0081-06 | **K-BOT-ul directorului (G0b)** — la login cu rolul `Director` se deschide doar lista «Aveți N documente de fundamentare de semnat» din TOATE unitățile directorului, cu documentul selectat în vederea DDF (semnarea existentă → `A,B,Ordonator` → S4) | GATA pe cod (build App **0 / 0**; `py_compile` verde) / **nerulat, nevăzut pe ecran, nicio suită rulată** | `SLICE-0081-06-kbot-director.md` | O sesiune = o unitate: un document din altă unitate se deschide după o nouă autentificare pe unitatea lui (unitatea pre-selectată). Rândurile `Director` din `Unitati_Utilizatori` se pun de mână. |
| 0081-07 | **Proba și reîncărcarea (cererea operatorului, 26.09.2026)** — fiecare răspuns FOREXE păstrat în `Rezultate_Forexe` (JSON reîncărcabil); «Mod reîncărcare»: răspunsurile se aleg din fișiere, FOREXE nu se atinge; «Mod probă»: robotul se oprește înainte de orice pas `commits="true"`; capturile identice nu se mai pun de două ori pe revizie | GATA pe cod (build App **0 / 0**; `py_compile` verde) / **nerulat, nevăzut pe ecran, nicio suită rulată** | `SLICE-0081-07-proba-si-reincarcare.md` | Cele două moduri doar pe sesiune (Setări → FOREXE). «Adaugă angajament» nu salvează (operatorul, 26.09): proba lui Creare merge până la salvarea primului rând. Ordinea Creare → Definitivează → Derulează verificată în ghidul ministerului. Serverul: `ddf_trimitere.py` de copiat pe VPS. |
| 0081-08 | **Rândul din Secțiunea A într-o fereastră (cererea operatorului, 26.09.2026)** — «Adaugă rând» deschide `DdfEditLinieAForm` (toate valorile unui rând, în designer); dublu clic pe «Clsf» o deschide pe rândul existent; butonul nu mai depinde de lista de clasificații (doar de blocarea documentelor din rezervări); `Titlu` pentru lista MANUAL = `Mid(Clsf,13,2)` (era partea a treia, greșit) | GATA pe cod (build Controls + App **0 / 0**; fereastra randată cu DrawToBitmap, tastarea `650204` → `65.02.04` + listă + Enter verificate) / **nerulat live, nicio suită rulată** | `SLICE-0081-08-rand-sectiunea-a.md` | ⚠ Cauza reală a butonului inactiv la «Adaugă rezervare» NU e verificată pe date: fie serverul a întors lista goală, fie cererea a eșuat — acum fereastra o spune pe nume. Coloana ascunsă «Clasificatie» nu mai e alimentată. |
| 0081-09 | **Antet blocat, sursa în Secțiunea A, partenerul doar în antet (cererea operatorului, 26.09.2026)** — o revizie nouă pe un angajament existent ia antetul de la angajament, blocat (compartiment, program, obiect, CUAL, data creării, partener); fereastra rândului cere întâi sursa / sectorul — **nicicând blocată, legată de programul din antet prin `AVACONT_COMUN.DefaProgram`** (0000000000 → 02A / 02E, 0000002510 → 01A; corecția operatorului din aceeași zi), la angajament nou ȘI la revizie nouă; SS-ul ales în K-BOT doar propus; ruta nouă `GET /api/forexe/ddf/surse-program`; «Adaugă rând» oprit la angajament nou până antetul are program, compartiment, obiect (și partener, dacă e bifat); câmpul Partener scos din fereastra rândului | GATA pe cod (build App **0 / 0**; fereastra rândului randată pentru ambele programe) / **teste VB scrise, nerulate; ruta Flask nerulată, fără test; nimic pe server** | `SLICE-0081-09-antet-sursa-partener.md` | ⚠ Excepție de la blocare: compartimentul / obiectul rămân deschise cât sunt GOALE (`FX_Angajamente` nu are compartiment). ⚠ Ruta nouă trebuie urcată pe VPS înainte de a folosi fereastra. |
| 0081-10 | **Clasificația fără «.02», lista de parteneri, antetul reviziei 0 (cererea operatorului, 26.09.2026)** — clasificația din fereastra rândului și din intrările workflow-urilor de trimitere fără partea «.02» de după capitol (`DdfSendInputs.ForexeClsf`; `Clasificatia` = 65.04.02.20.01.01, `ClsfSal` = 650402200101; datele salvate neatinse); blocarea antetului readusă la «revizie nouă pe document existent» (revizia 0 a unui angajament descărcat din FOREXE e din nou editabilă, inclusiv «Partener asociat»); `/api/forexe/ddf/parteneri` oferă toate unitățile bazei când angajamentul nu are încă indicatori (angajament nou) | GATA pe cod (build App **0 / 0**; fereastra rândului randată) / **teste VB scrise, nerulate; SQL-ul partenerilor nerulat; nimic pe server** | `SLICE-0081-10-clsf-parteneri-antet.md` | Grila Secțiunii A arată încă «65.02…» (cererea a limitat schimbarea). `ddf_edit.py` de urcat pe VPS. |
| 0081-12 | **Revizie nouă din indicatorii existenți, clasificațiile folosite întâi, rândurile cu 0 șterse la salvare (cererea operatorului, 26.09.2026)** — «Adaugă rezervare» din + (subsolul arborelui Rezervări) deschide «1. Adaugă revizie goală» / «2. Folosește indicatorii existenți» (Secțiunea A pornește cu câte un rând, valoare 0, pentru fiecare indicator al angajamentului); în `DdfEditLinieAForm` clasificațiile angajamentului apar primele, și în listă, și la căutarea prin tastare (`KBotComboBox.FindFirstGroupCount`); la salvare rândurile cu valoarea 0 se șterg după confirmare, iar dacă nu rămâne niciunul documentul NU se salvează | GATA pe cod (build App **0 / 0**) / **nevăzut pe ecran, nerulat live; niciun test scris** | `SLICE-0081-12-indicatori-existenti-randuri-zero.md` | Codul indicatorului vine din `FX_DDF_REV_SA` (ca în Access); o clasificație doar pe `FX_Indicatori` primește un cod nou «!xxx». Programul reviziei precompletate și al celei adăugate din «+» pe rândul arborelui Rezervări = `FX_Indicatori.SS` JOIN `DefaProgram` și al reviziei goale (runda a treia). Bugetul rândurilor fără rezervare = `FX_Indicatori.Credit_Bugetar` (câmp nou `buget` în `/clasificatii` — **`ddf_edit.py` de urcat pe VPS**). Clasificațiile deja adăugate nu mai apar în listă (aceeași cheie SAU același cod pe aceeași SS). Valorile în grilă (designer): Buget, Recepții, Disponibil, ValPrec, ValCur, Val. rămasă = Disponibil − ValCur; lățime 90, fără subsol, și în fereastra rândului și în Secțiunea A. |

### Current focus

- **Slice 0081-12 — revizie nouă din indicatorii existenți + rândurile cu 0 (26.09.2026).** Meniul «+»
  din Rezervări are două intrări; varianta 2 precompletează Secțiunea A cu indicatorii angajamentului
  (valoare 0). Salvarea șterge rândurile cu 0 după confirmare și refuză dacă nu rămâne niciunul.
  Clasificațiile angajamentului sunt primele în fereastra rândului. Programul reviziei precompletate
  vine din `FX_Indicatori.SS` → `DefaProgram`, nu din ultima revizie (și la «+» din arborele
  Rezervări). Buget din `FX_Indicatori.Credit_Bugetar`; grilele de valori refăcute (Disponibil,
  Val. rămasă). `ddf_edit.py` de urcat pe VPS. Nimic rulat live.

- **Slice 0081-10 — clasificația fără «.02», parteneri, antetul reviziei 0 (26.09.2026).** Workflow-urile
  primesc clasificația în forma FOREXE; revizia 0 a unui angajament descărcat are antetul editabil din nou;
  angajamentul nou are parteneri. `ddf_edit.py` de urcat pe VPS. Nimic rulat live.

- **Slice 0081-09 — antet blocat + sursa în Secțiunea A (26.09.2026).** Revizia nouă pe un angajament
  existent are antetul blocat; în fereastra rândului se alege întâi sursa, dintre sursele programului
  din antet (`DefaProgram`), la angajament nou și la revizie; «Adaugă rând» așteaptă antetul complet
  la angajament nou. Partenerul e doar în antet. Ruta `surse-program` de urcat pe VPS. Nimic rulat live.

- **Slices 0081-08 + 0082 — rândul din Secțiunea A + combo-ul cu căutare și mască (26.09.2026).**
  «Adaugă rând» deschide fereastra nouă `DdfEditLinieAForm`; clasificația se alege tastând doar
  cifrele (`00.00.00.00.00.00.00`), lista se îngustează după 2 cifre. Nimic rulat live.

### Open threads

- **0081-12 — de confirmat cu operatorul.** (1) «Indicatorii existenți» = clasificațiile cu cod de
  indicator dintr-o revizie anterioară sau, la un angajament din FOREXE, cele de pe `FX_Indicatori`;
  codul vine din `FX_DDF_REV_SA` (ca în Access), deci una aflată doar pe `FX_Indicatori` primește un cod
  nou «!xxx» — de confirmat dacă trebuie luat codul din `FX_Indicatori.CodIndicator`. (2) Elementul de
  fundamentare al rândului precompletat = denumirea clasificației (nu textul din revizia anterioară).

- **0081-09 — de confirmat cu operatorul.** (1) Compartimentul și obiectul rămân deschise la o
  revizie nouă pe un document existent DOAR cât documentul anterior le-a lăsat goale (0081-10: revizia 0
  a unui angajament venit din FOREXE are din nou tot antetul editabil). (2) Sursa aleasă în fereastra principală e preselectată dacă aparține programului
  din antet (se poate schimba) — de confirmat. (3) Ruta `GET /api/forexe/ddf/surse-program`
  (`routes/forexe/ddf_edit.py`, citește `AVACONT_COMUN.DefaProgram` prin `get_kbot_comun_connection`)
  trebuie urcată pe VPS; până atunci «Adaugă rând» spune că sursele nu au putut fi aduse.

- **0081-08 / 0082 — de confirmat cu operatorul.** (1) Punctul 3 al cererii pentru felia 0082 a
  venit gol. (2) De ce «Adaugă rând» era inactiv la «Adaugă rezervare» nu e verificat pe date:
  butonul era oprit când lista de clasificații venea goală SAU cererea eșua; acum rămâne activ și
  fereastra spune care din două. Dacă spune «Serverul nu a trimis nicio clasificație», de verificat
  `FX_Indicatori` pentru acel `CodAngajament` (lista ne-manuală vine doar de acolo). (3) Masca
  permite doar cifre: căutarea după denumire nu se poate face în combo-ul din fereastră (lista
  întreagă rămâne la săgeată).

---

## Slice 0082

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0082 | **`KBotComboBox`: `FindAsYouType`, `FindAfterNChars`, `InputMask` (cererea operatorului, 26.09.2026)** — listă a rândurilor potrivite sub casetă pe măsură ce se tastează (fereastră separată, fără focus; începe-cu întâi, apoi conține; fără majuscule/diacritice); căutarea pornește după N caractere tastate (literalii măștii nu se numără); masca `0/L/A/&/\x`, literalii puși automat | GATA pe cod (build Controls **0 / 0**; verificat prin randare în fereastra 0081-08) / **teste scrise, nerulate** | `SLICE-0082-combo-cautare-si-masca.md` | Punctul 3 al cererii a venit gol — de întrebat operatorul. `Items` nu se ating (merge și cu `DataSource`). |

---

## Slice 0083

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0083 | **`KBotComboBox.OfferNewItem` (cererea operatorului, 26.09.2026)** — cu `LimitToList` pornit: când lista nu are ce arăta (textul tastat nu se potrivește cu niciun rând sau combo-ul nu are elemente), apare un singur rând `OfferNewItemText` (implicit «Adaugă un element nou…»); click / Enter pe el ridică `NewItemRequested` (`e.Text` = textul tastat) | GATA pe cod (build Controls **0 / 0**) / **fără verificări și fără teste, la cererea operatorului** | `SLICE-0083-combo-element-nou.md` | Nerandat, nerulat. |

---

## Slice 0084

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0084 | **«Operațiuni necorectate» după conectarea FOREXE (cererea operatorului, 26.09.2026)** — după fiecare conectare reușită se citește tabelul din pagina de start; dacă are rânduri: salvare în `FX_Operatiuni` (doar cele noi, cheie ReferintaTrezor + NrDoc) și un mesaj cu Program, SSI, Ref. TREZOR, Nr. doc., Dată, Tip, Suma (+ Probleme doar dacă există) | GATA pe cod (build App **0 / 0**; `py_compile` verde) / **nerulat, scriptul paginii netestat, fără teste** | `SLICE-0084-operatiuni-necorectate.md` | ⚠ `FX_Operatiuni.Suma` o adaugă operatorul. `operatiuni.py` de urcat pe VPS. URMEAZĂ: corelarea operațiunilor cu angajamentele din bază. |
| 0084/01 | **Indicatorii la reîmprospătare + redenumirea `FX_Indicatori.Prevedere_Bugetara_Initiala` → `Credit_Bugetar` (operator, 26.09.2026)** — (1) verificat: pasul 2 al prelucrării rescrie deja cele patru coloane de bani pentru fiecare rând (fără schimbare de cod; secțiunea din fluxurile de recepții anulată la cerere). (2) Redenumirea: Python (`prelucrare.py`, `prelucrare_pasi.py`, `ddf_edit.py`), Migrator (`.Rename(...)` pe FX_Indicatori, 1.16.1.0), `sql/AVACONT_SURSA.sql` | GATA pe cod (`py_compile` verde, build Migrator **0 / 0**) / **nerulat, fără teste** | `SLICE-0084-01-indicatori-la-fiecare-reimprospatare.md` | ⚠ Redenumirea în bază o face operatorul, în AVACONT_SURSA cu `COMMENT 'rename:Prevedere_Bugetara_Initiala'` (altfel sincronizarea face DROP + ADD); cele 3 fișiere Python urcă pe VPS ODATĂ cu ea. |

---

## Slice 0085

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0085 | **`KBotDataView`: editorul arată ca celula, editare la un clic, Sus/Jos prin coloană (cererea operatorului, 26.09.2026)** — editorul de text fără chenar, în dreptunghiul de conținut al celulei (padding scalat DPI), cu fontul, culorile și alinierea rezolvate prin RowFormatting/CellFormatting, marginile = umplutura TextRenderer, centrat vertical; repoziționat la fiecare layout (redimensionare, temă, DPI). Clic pe o celulă editabilă = editare cu cursorul unde s-a apăsat. Sus/Jos = cea mai apropiată celulă editabilă din aceeași coloană (benzile de grup și subsolul sărite; fără țintă nu se întâmplă nimic). Un commit fără schimbare nu mai scrie nimic (fără IsDirty / CellValueChanged). Trecerea 2: clicul selectează tot textul, celula editată păstrează culoarea nesel. a coloanei (nu pe cea a rândului selectat), cursor I-beam peste celulele editabile | GATA pe cod (build Controls + App **0 / 0**; verificare pe pixeli în afara ecranului: **0 pixeli diferiți** la 144 dpi pe 5 celule, 4/5 la 96 dpi — Tahoma diferă doar la franjurile ClearType) / **nevăzut în aplicație pe ecranul operatorului, nicio suită rulată** | `SLICE-0085-dataview-editor-matches-cell.md` | Combo-ul păstrează celula întreagă (doar font + culori). Restul parțialelor DataView au încă comentarii în română cu diacritice (regula 0). |

### Current focus

- **Slice 0085 — `KBotDataView` editor = the cell (26.09.2026).** Borderless editor placed on the
  cell text to the pixel (checked off-screen at 144 and 96 dpi), single-click editing (selects all, I-beam cursor, edited cell keeps its unselected colour), Up/Down
  through the column's editable cells, unchanged commits write nothing. **Next:** the operator
  looks at it in the real app (DDF Secțiunea A grid is the main user).

---

## Slice 0086

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0086 | **`KbotForm` împărțit în clase parțiale + «Angajament nou» buton primar (cererea operatorului, 26.09.2026)** — `KbotForm.vb` (2766 rânduri) rămâne cu câmpurile, constructorul, `WithReauth` și Load (266); restul mutat fără schimbare de comportament în 12 parțiale noi, fiecare sub 340 de rânduri: Periods, Views, Tree, Receptii, Ord, Ddf, DdfDelete, Extrase, Download, Ingest, Console, Chrome. Comentariile mutate rescrise în engleză, fără diacritice (regula 0); textele operatorului neatinse. Codul comentat al vechiului buton «Conectare» din antet șters. `btnAngajamentNou` primește `ButtonStyles.ApplyPrimary` în `OnThemeChanged`. Trecerea 2: `KbotForm.DdfSend.vb` împărțit în DdfSend / DdfSendMenu / DdfSendHelpers; toate parțialele formularelor și controalelor (21 în App, 56 în Controls) au `DependentUpon` în .vbproj, ca frunze sub fișierul principal în Solution Explorer | GATA pe cod (build App + Controls **0 / 0**) / **nevăzut pe ecran și în VS, fără teste (la cererea operatorului)** | `SLICE-0086-kbotform-partials-primary-button.md` | Etichetele de jurnal rămân `MainForm.*`. ApiClient / WorkflowModels nu sunt formulare, neatinse. |

### Current focus

- **Slice 0086 — `KbotForm` split + primary «Angajament nou» (26.09.2026).** `KbotForm.vb` cut from
  2766 to 266 lines; 12 new partials by concern, all under ~340 lines, moved comments in English.
  `btnAngajamentNou` themed as primary. Build App 0 / 0; not seen on screen, no tests.

---

## Slice 0087

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0087 | **«Clasificații bugetare», «Parteneri», `KBotDropDownMenu`, meniul din antet (cererea operatorului, 26.09.2026)** — fereastra clasificațiilor (arbore Capitol ▸ Subcapitol ▸ Articol ▸ Alineat cu denumirea ca a doua coloană; bugetul anual pe un rând, Trim. 1-4 editabile, Total calculat; rectificările editate direct în grilă, totaluri în subsol, «+» în subsol; «Salvează» într-o singură tranzacție; «+» din subsolul arborelui = adăugarea clasificațiilor cu regulile înregistrării, doar pe sursele unității); fereastra partenerilor (arbore cod ▸ denumire, filtrele ascunși / fără activitate, fără Burse, coduri angajament în grilă, ștergere refuzată pentru partenerii folosiți pe documente); control nou `KBotDropDownMenu` (bară de pictograme, text formatat, submeniuri, elemente în designer); `btnAngajamentNou` înlocuit de `btnMeniu` + `menuNou` (Angajament nou │ Nomenclatoare ▸ Clasificații / Parteneri). În plus: `KBotDataView.FooterRightIcon` + `RemoveRowAt`, `AdvancedTreeControl.RightTextColumn` | GATA pe cod (build App **0 / 0**; `py_compile` verde; randat cu `DrawToBitmap`, Classic + Dark, date false) / **nerulat pe server, SQL netestat, neapăsat pe ecran, fără teste (la cererea operatorului)** | `SLICE-0087-clasificatii-parteneri-meniu.md` | Rute noi `/api/forexe/nomenclatoare/...` (Flask de repornit). Din formularul Access lipsesc «Cod Client (OP)», «Solduri inițiale», «Exportă în Excel». «Alte detalii» = `Parteneri.Adresa`. |

---

## Slice 0088

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0088 | **«Nota contabila corectie CAB» (F1135) pentru operațiunile «ERRRRRRRRRR» (cererea operatorului, 28.09.2026)** — după login, operațiunile ERR NOI din «Operațiuni necorectate» deschid direct fereastra (fără mesaj); fiecare rând primește angajament + indicator, rândul complet se bifează, «Salvează tot» doar când toate sunt bifate → O notă (FX_NoteCAB + FX_NoteCAB_Corectii), PDF F1135 (macheta = prima revizie a «NOTA CAB 23.pdf», resursă în KBot.Xfa), PDF stocat (FX_NoteCAB_PDF, familia «nc» din pdf.py), întrebarea «Vrei să încarci NOTA DE CORECȚIE în CAB?» → «Transmitere documente electronice» (linkdoc + Trimite, în iframe). «Ieșire» nu salvează nimic în plus (rândurile sunt deja în FX_Operatiuni). Meniu → «Operațiuni necorelate» (ascuns când nu există; «(!)» pe intrare și pe «Meniu»). Vederea «Note corecție»: arbore lună ▸ notă, «Vizualizare» + «Document» (semnată → descărcată, nesemnată → generată; semnătura urcă imediat) | GATA pe cod (build Domain/Xfa/Api/Forexe/App **0 / 0**; `py_compile` verde) / **nerulat, nevăzut pe ecran, fără teste** | `SLICE-0088-note-corectie-cab.md` | ⚠ ÎNTÂI `sql/0088_fx_note_cab.sql` pe toate bazele + AVACONT_SURSA, APOI `note_cab.py`, `operatiuni.py`, `pdf.py`, `__init__.py` pe VPS. De confirmat: «CodAngajament și NumarDocument» = angajament + indicator; prefixul contului doar pentru sectorul 02 («24»). |

---

## Slice 0089

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0089 | **Jurnalele serverului în K-BOT, pe utilizator și pe sesiune (cererea operatorului, 28.09.2026)** — fiecare linie din `api_server.log` scrisă într-o cerere autentificată poartă `{s=<token8> u=<utilizator> dc=<unitate>}`, fiecare bloc din `forexe_timing.log` poartă `session= user= dc=`; rute noi `GET /api/logs/server` și `/api/logs/timing` (`routes/logs.py`) întorc DOAR liniile apelantului, din ultimele N sesiuni (implicit 3), iar din `api_server.log` doar `[forexe]`/`[forexe.xxx]` (restul e calea veche Access); `CmbTipJurnal` = Jurnale locale / Server FOREXE / Timpi FOREXE + `cmbSesiuni` (3/5/10/20/50); cipurile de nivel împart Eroare/Avertisment/Informație (timpi: nivelul din statusul HTTP, `ForexeTimingParser`); `ApiClient.GetAsync` implementat. Calea veche VBA (X-Api-Key fără bearer) scrie de acum în `api_server_vba.log`, separat. Liniile de dinainte de actualizarea serverului nu au marcaj și nu apar. | done | [SLICE-0089-jurnale-server-per-utilizator.md](SLICE-0089-jurnale-server-per-utilizator.md) | ⚠ ÎNTÂI pe VPS: `utils/logger.py`, `utils/timing.py`, `routes/logs.py`, `main.py` + repornire. Neverificat pe VPS și pe ecran. |
