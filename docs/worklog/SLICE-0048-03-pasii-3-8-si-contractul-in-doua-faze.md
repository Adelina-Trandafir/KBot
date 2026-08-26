# SLICE-0048-03 — pașii 3–8 ai ingestiei FOREXE și contractul în două faze

A treia transă din felia 0048 (portarea conductei de ingestie FOREXE din Access VBA în API-ul
Flask). Plan: `PLAN_ForexeIngestSteps3to8` peste `docs/PLAN_ForexeIngest.md`.
Temeiul deciziilor: `docs/FUNDAMENT_Asociere_Receptii.md` — regulile `F1`…`F27` și deciziile
`D-A`…`D-M` de acolo, nere-deduse aici.

Transa livrează **pașii 3, 4a–4d, 5 și 7** ai conductei, **contractul în două faze**
(propunere / salvare) cu tot ce ține de el — amprentă, decizii, validări, reconstituirea
recepțiilor dispărute — plus jumătatea de client (apeluri, modele, dosarul local, o intrare în
DevHarness). **Pasul 8 nu se execută, deliberat; vezi §0.2.**

**Transa NU livrează formular și NU leagă nimic în fluxul de descărcare.** Formularul e
0048-04. Vezi §4.1.

---

## 0. Poarta de citire — ce a arătat fiecare verificare cerută

### 0.1 `docs/FUNDAMENT_Asociere_Receptii.md` era în rădăcina depozitului, nu în `docs/`

Fișierul exista netracked la `./FUNDAMENT_Asociere_Receptii.md`, iar antetul lui însuși spunea
«**Destination:** `docs/FUNDAMENT_Asociere_Receptii.md`». S-a mutat acolo și antetul a devenit
`**Location:**`. Toate trimiterile din cod arată către calea din `docs/`.

### 0.2 `FX_Indicatori_Actualizare_Extrase` — CITITĂ, și răspunsul e «stop»

Deschiderea deschisă O5 se închide. Funcția are exact două instrucțiuni
(`mdl_FX_Tasks_Receive_DWN.md`), și amândouă scriu `FX_Extrase`:

```vb
UPDATE FX_Extrase INNER JOIN FX_Plati ON FX_Extrase.Referinta     = FX_Plati.Referinta_TREZOR
   SET FX_Extrase.CodAI = [FX_Plati]![CodAI] WHERE FX_Extrase.CodAI Is Null
UPDATE FX_Extrase INNER JOIN FX_Plati ON FX_Extrase.ReferintaDest = FX_Plati.Referinta_TREZOR
   SET FX_Extrase.CodAI = [FX_Plati]![CodAI] WHERE FX_Extrase.CodAI Is Null
```

`FX_Extrase` e scos din scop de planul părinte §12. §0 al planului acestei transe spune
textual: *«If it requires FX_Extrase, report that and stop rather than porting half of it.»*
Deci **pasul 8 nu se execută**. Nu e uitat și nu e înlocuit cu ceva aproximativ: fiecare
răspuns al rutei poartă un avertisment care spune că nu a rulat și de ce
(`prelucrare_pasi.MESAJ_PAS8`), iar un test îl pinuiește.

**Ce înseamnă absența lui, spus pe față:** rândurile din `FX_Extrase` care s-ar fi legat de
plățile scrise la pasul 5 rămân cu `CodAI` NULL. Nimic nu se strică; o legătură nu se face.
Ambele `UPDATE`-uri sunt filtrate pe `CodAI Is Null`, deci felia care aduce `FX_Extrase` în
scop recuperează tot restanțul dintr-o singură trecere. **D-G rămâne valabilă ca intenție.**

### 0.3 `DIFH = 0` ca marcaj de salvare-fără-schimbare (F20 / O3) — NECONFIRMABIL de aici

Nu s-a putut verifica, și motivul e concret, nu o scăpare: **de pe această mașină nu există
acces la nicio bază de unitate.** `DB_CONFIG_NEW` (89.33.25.34) și `DB_CONFIG` (86.122.82.226)
resping amândouă acreditările gazdei — `1045 Access denied for user 'AVACONT'@'188.27.66.13'`.
Singurul cont care se conectează este `READER_DB_CONFIG`, iar `SHOW DATABASES` pe el întoarce
**doar `AVACONT_COMUN` și `information_schema`**. Nu există niciun `FX_Receptii_H` de interogat.

**Nu blochează transa**, și planul spune de ce (§10 O3): F20 afectează ce poate **sugera
formularul**, nu ce scrie transa asta. Pasul 4d e portat din `FX_CalculeazaDIF_Receptii_Tmp`
oricum, identic, indiferent de răspuns. **Rămâne deschis pentru 0048-04**, cu interogarea de
rulat scrisă în §5.3 de mai jos.

---

## 1. Ce s-a schimbat și de ce

### 1.1 Conducta — `routes/forexe/prelucrare_pasi.py` (fișier nou)

Pașii 3a–3e, 4a, 4b, 4d, 5 și 7, portați din sursa VBA reală, funcție cu funcție. Tot ce e
aici rulează **identic în ambele faze** — nicio ramură de «propunere» nu se strecoară prin
codul pașilor.

Ce merită știut înainte de a-l citi:

- **3b cheamă `CalculeazaValRezervareDif`**, pe care planul nu o numește deloc. Fără ea toate
  valorile `TipRand` de rezervare (`Rez_Initiala`, `Rez_Definitiva`, `Rez_Influenta`,
  `Rez_Zero` și cele trei cu `+`) ar rămâne goale, iar pașii 3c/3d — care filtrează pe ele —
  ar scrie zero rezervări. E portată integral.
- **3a întoarce harta `indice payload ▸ FX_Istoric.ID` pentru TOATE rândurile**, nu doar
  pentru cele inserate. `rand_istoric` din decizii e chiar acel indice (F24), iar D-F cere ca
  instantaneele rămase neașezate din rulări anterioare să poată fi decise acum — rândurile lor
  de istoric există deja în bază.
- **Două rânduri identice din același payload se colapsează într-unul.** `FX_Istoric.HASH` are
  index UNIQUE; fără colapsare, al doilea `INSERT` l-ar lovi. Access făcea la fel, fiindcă
  `rcHis.FindFirst` mergea peste un recordset care creștea cu fiecare `AddNew`.
- **4a: rândul de ștergere nu e deviat.** `Descriere = "Stergere receptie"` poartă
  `(activ:true)` ca orice antet, deci devine instantaneu pe calea normală, cu
  `EsteStergere = 1`. Nu are rânduri pe indicator, deci nu produce linii — și asta iese de la
  sine, fiindcă liniile lui nu există în istoric, nu fiindcă le-am opri noi (F21).
- **4b are regula F25 în SQL**, cu comentariul la fața locului. Potrivirea Access e pe
  `CLng(DataR)` — **granularitate de ZI** —, deci fără `Sters = 0` în interogarea de candidați
  o recepție creată în aceeași zi calendaristică în care fusese creată una ștearsă în martie
  s-ar potrivi peste aceea și i-ar suprascrie tăcut valorile.
- **4d ordonează după `DataH`, nu după `IDRH`.** Access ordona după `ID` cu comentariul
  «autonumber = ordine cronologică în tmp». Sub plasare MANUALĂ ordinea de inserare încetează
  să mai fie cea cronologică — operatorul poate atașa un instantaneu din ianuarie după unul din
  mai. `DataH` E axa timpului (F2). Cu plasare pur automată cele două ordini coincid, deci
  nimic din datele deja produse nu se schimbă.
- **5: `Data_plata` ia `FX_Istoric.DataFX`, nu data din Observații.** Contraintuitiv, dar e ce
  face `mdl_FX_Plati`: `data:` e parsată doar ca să fie VALIDATĂ (una neparsabilă sare rândul).
  Portat fidel, comentat la fața locului.

### 1.2 Asocierea — `routes/forexe/prelucrare_asociere.py` (fișier nou)

Singurul loc în care cele două faze diferă, plus tot ce ține de contract: amprenta, forma
deciziilor, validările F13–F16, regulile etichetelor și 4c-bis.

- **Faza unu** rulează trecerea automată ca VBA-ul (două treceri, LIFO, fiecare recepție
  consumată o dată) și o raportează ca `sugestie_idrr` / `sugestie_automata` — **nu scrie
  niciun `IDRR`**. Un test verifică asta direct: toate interogările emise sunt `SELECT`.
- **Faza doi** aplică `decizii` și ignoră complet trecerea automată. Operatorul i-a văzut
  sugestiile; a o rula din nou ar însemna să ne batem cu propriul om.
- **`Final` = cel mai TÂRZIU instantaneu după `DataH`**, recalculat per recepție o singură dată
  după aplicarea tuturor deciziilor. `AsociazaFinal` făcea `Final` orice tocmai atașase — cu
  plasare manuală, regula aceea ar lăsa un instantaneu din februarie să devină `Final` pe o
  recepție care are deja unul din mai. `HASH` se rescrie doar când `TipReceptie` se schimbă.
- **Recepțiile ȘTERSE nu sunt candidate ale trecerii AUTOMATE.** Nimic nu se mai poate adăuga pe
  site unei recepții șterse, deci o potrivire automată pe ea ar fi mereu o coliziune — același
  raționament ca F25. Plasarea MANUALĂ pe una ștearsă rămâne permisă: vetoul e pe dată (F13),
  nu pe steag, iar un instantaneu dinaintea ștergerii îi aparține pe drept.

### 1.3 Ruta — `routes/forexe/prelucrare.py`

O singură rută, două moduri. Ce le desparte nu e adresa, ci ce are voie serverul să facă la
coada tranzacției: `rollback()` sau `commit()`.

**Modul implicit e `propunere`** — un client care nu știe de faze primește faza care NU scrie.
Tăcerea nu are voie să însemne «salvează».

Faza unu se termină cu `conn.rollback()` **necondiționat**, pe calea de succes. Nu e o cale de
eroare: e chiar contractul. Câmpul `scrise` raportează **ce s-ar fi scris**, iar docstring-ul o
spune — un contor care arată a rezultat dar descrie o rulare anulată e exact genul de lucru care
se citește greșit.

Cele două 409-uri stau alături și nu se contopesc:

| cod-motiv | ce înseamnă | ce face clientul |
|---|---|---|
| `ALEGERE_UNITATE` (0048-02) | o clasificație se potrivește cu mai multe unități | întreabă operatorul, retrimite ACEEAȘI sarcină cu `alegeri` |
| `STARE_MODIFICATA` (0048-03) | baza s-a mișcat între propunere și salvare | descarcă din nou; nu există nimic de răspuns |

Un angajament poate avea nevoie de **două** drumuri dus-întors înainte ca operatorul să vadă
formularul de asociere. Așa trebuie.

### 1.4 Amprenta — ce conține, exact (cerut explicit de plan §2.3)

```
FX_Istoric      COUNT(*), MAX(ID), MAX(DataFX)
FX_Receptii_R   COUNT(*), MAX(IDRR)
FX_Receptii_H   COUNT(*), MAX(IDRH), COUNT-ul celor cu IDRR NULL și Sters = 0
```

toate filtrate pe `CodAngajament`, concatenate `cheie=valoare|…` și trecute prin SHA-256; se
păstrează primele 32 de caractere hex.

**De ce și `COUNT` și `MAX`:** `MAX` singur nu se mișcă la o ȘTERGERE de rând; `COUNT` singur nu
se mișcă la o ștergere urmată de o inserare. Împreună prind ambele cazuri.
**De ce ultimul câmp:** el E chiar mulțimea despre care operatorul ia decizii. Dacă altă sesiune
a asociat ceva între timp, fișierul local nu mai descrie aceeași listă, iar `MAX(IDRH)` nu s-ar
clinti. Nimic din ce se schimbă la CITIRE nu intră (fără `DTQ`, fără ceasuri).

**Se calculează ÎNAINTE de orice scriere, în ambele faze.** Luată la coada fazei întâi ar
descrie starea scrisă — care e apoi derulată înapoi — și faza a doua nu s-ar potrivi niciodată.

### 1.5 4c-bis — recepțiile reconstituite (F26)

Nimic nu se inventează; fiecare câmp are o sursă în istoric. `Preluat`, `Incarcat` și
`TipReceptie` se pun **exact ca la o recepție nou inserată de pasul 4b**, iar planul cere să se
scrie aici ce a ieșit: **`TipReceptie = 'NOU'`, `Preluat = 1`, `Incarcat` NEATINS (rămâne
NULL)**. Rândurile astea n-au trecut niciodată prin `ListaReceptii`, deci oriunde altundeva ar
însemna valorile astea ceva, aici înseamnă ceva ușor diferit.

`CreditBugetar` se ia dintr-un rând `RHR` existent cu același `CodAI`. **Dacă nu există niciunul,
se RIDICĂ** — un zero acolo nu se deosebește de un zero real și ar fi citit ca fapt pe veci.

### 1.6 Schema — `sql/0049_receptii_stergere.sql`

Trei coloane noi, toate `NOT NULL DEFAULT 0`, cu probă `information_schema` de rulat **înainte**
(și așteptările ei scrise pe litere) și cu `COMMENT` pe fiecare coloană.

`FX_Receptii_R.Sters` · `FX_Receptii_R.Reconstituit` · `FX_Receptii_H.EsteStergere`

Se aplică **și pe `AVACONT_SURSA`**, ca `0048_alegeri_unitate.sql`. Scutirea care ține cele șapte
`ALTER`-uri `AUTO_INCREMENT` ale feliei 0048-01 §3.1 departe de schema de referință **nu se
aplică aici**: acolo scutirea păzea o poartă de siguranță a migrării (un rând fără cheie ar primi
tăcut următorul număr); aici nu se dezarmează nimic.

`Sters` și `Reconstituit` **nu se colapsează**: `Sters` spune ce s-a întâmplat cu recepția,
`Reconstituit` spune de unde știm că a existat. Fără al doilea, «câte recepții am reconstituit
noi» nu are răspuns.

### 1.7 Clientul

`KBot.Domain/AsociereInfo.vb` (nou): `ActiuneAsociere`, `PrelucrarePropunere`,
`ReceptiePropusa`, `LinieReceptie`, `InstantaneuPropus`, `LinieInstantaneu`,
`DecizieAsociere`, `AsociereDosar`. `PrelucrareStare` capătă `Propunere`, iar
`PrelucrareRaspuns` capătă `Propunere As PrelucrarePropunere` — un singur tip pentru toate
stările, ca la 409-ul din 0048-02.

`KBot.Api`: `CerePropunereAsync` și `SalveazaAsociereaAsync`, amândouă pe aceeași rută, cu
`mod` diferit. Un set **separat** de opțiuni JSON (`_jsonFaraNull`) ține `amprenta` și `decizii`
afară din corpul unei propuneri; `_json` cel comun **nu** s-a atins, fiindcă «omite null-urile»
ar fi o schimbare de contract pentru fiecare altă cerere a clientului.

`KBot.Common/AsociereStore.vb` + `AsociereDosar`: dosarul local
`<AppDir>\Asociere\<cod>.json`, scris prin fișier temporar + `File.Move` (o cădere în mijlocul
scrierii ar lăsa un JSON trunchiat, adică exact un dosar corupt pe angajamentul la care
operatorul tocmai lucra).

`PrelucrareCoordinator.CerePropunereAsync` — aceeași buclă de întrebări ca `TrimiteAsync`,
fiindcă 409 `ALEGERE_UNITATE` se poate declanșa și în faza întâi.

### 1.8 Corecțiile C1 și C2 — migratorul

- **`FX_ORD_TBL_REC` iese din lista de excluderi** (`KBot.Migrator/Transfer/TableMaps.vb`) și
  capătă o hartă reală, plus o intrare în `routes/migrare/tables.py` cu un fel de selecție nou,
  `BY_ORD_TBL`, și familia de chei `ord_tbl` în `routing.py` (construită din `FX_ORD_TBL`,
  cheie `IDORDTBL`, moștenind apartenența de la `IDORD`). Coloanele Access s-au citit din
  `artifacts/accdb-schema/FX_2026.md`: `IDORDRECP` poartă 475, 476… — oglinda serverului VECHI —
  deci **nu călătorește**; pe MariaDB coloana omonimă e `AUTO_INCREMENT`.
- **`FX_Receptii_Plati` iese din `ALL` și intră în `OUT_OF_SCOPE`** în migratorul Python (era
  ÎN listă, ceea ce era greșeala propriu-zisă). În `TableMaps.vb` era deja exclus.
- `FX_ORD_TBL.IDRP` e **mort**, nu doar orfan: arăta către `FX_Receptii_Plati`. Nota hărții s-a
  rescris.

---

## 2. Fișiere atinse

**Nou**
```
sql/0049_receptii_stergere.sql
PYTHON/routes/forexe/prelucrare_pasi.py
PYTHON/routes/forexe/prelucrare_asociere.py
PYTHON/tests/fixtures/prelucrare_AAB37CNBK95.json
PYTHON/tests/test_forexe_prelucrare_pasi.py
PYTHON/tests/test_forexe_prelucrare_asociere.py
PYTHON/tests/test_forexe_prelucrare_live.py
src/KBot.Domain/AsociereInfo.vb
src/KBot.Common/AsociereStore.vb
src/KBot.DevHarness/Tests/AsociereDosarRoundTripTest.vb
tests/KBot.Api.Tests/AsociereApiClientTests.vb
tests/KBot.Common.Tests/AsociereStoreTests.vb
docs/worklog/SLICE-0048-03-pasii-3-8-si-contractul-in-doua-faze.md
```

**Modificat**
```
PYTHON/routes/forexe/prelucrare.py            pașii 3-8 + cele două faze
PYTHON/routes/forexe/prelucrare_helpers.py    + fx_receptii_parse_ro_date, is_stergere_receptie
PYTHON/routes/migrare/tables.py               FX_ORD_TBL_REC in, FX_Receptii_Plati out
PYTHON/routes/migrare/routing.py              familia ord_tbl + felul BY_ORD_TBL
PYTHON/tests/test_forexe_prelucrare_route.py  fals extins pentru pașii 3-8; teste de fază
PYTHON/tests/test_migrare_routing.py          C1/C2
src/KBot.Api/IApiClient.vb                    cele două apeluri
src/KBot.Api/ApiClient.vb                     implementările + _jsonFaraNull
src/KBot.Api/UpsertAngajamenteRequest.vb      DTO-urile de fir
src/KBot.Domain/PrelucrareInfo.vb             starea Propunere + proprietatea
src/KBot.App/Forexe/PrelucrareCoordinator.vb  bucla de propunere
src/KBot.Migrator/Transfer/TableMaps.vb       C1 + C2
tests/KBot.App.Tests/*.vb  (8)                cele două metode noi pe falsurile IApiClient
docs/PLAN_ForexeIngest.md                     D1, D3, D4 amendate; D5 împărțit; §12
docs/MAPARE_NOMENCLATOARE.md                  rândul D4
docs/worklog/KBOT_STATUS.md                   rândul feliei + firele
```

**Mutat**: `FUNDAMENT_Asociere_Receptii.md` ▸ `docs/FUNDAMENT_Asociere_Receptii.md`.
**Șters**: `src/KBot.App/Forexe/AsociereStore.vb` (mutat — vezi §4.3).

---

## 3. Rezultatele testelor

**Python** — `PYTHON/.venv`, din `PYTHON/`:

```
410 trecute, 15 sărite, 0 căzute        (înainte de transă: 346 / 14)
```

`+64` teste. Sărirea în plus e modulul `test_forexe_prelucrare_live.py`, care se sare **în
întregime** în afara gazdei (`config.py` / `main` indisponibile) — vezi §0.3.

Cele mai grele dintre ele:

- `test_the_default_mode_is_the_proposal_and_it_never_commits` — conducta chiar rulează
  (`INSERT`-urile se emit) și abia apoi se anulează.
- `test_step4b_never_matches_a_deleted_reception_even_on_the_same_calendar_day` (F25).
- `test_stergere_receptie_becomes_a_snapshot_with_no_lines` (F21).
- `test_the_date_veto_rejects_on_a_one_second_difference` (F13) — TIMESTAMP complet.
- `test_the_chain_end_check_is_skipped_for_a_chain_ending_in_a_deletion` (F15).
- `test_final_lands_on_the_latest_snapshot_by_data_h_not_the_last_attached`.
- `test_a_reconstructed_chain_without_a_deletion_is_rejected` / `…with_two_deletions…`.
- `test_a_stale_data_h_fails_loudly_instead_of_associating_the_wrong_row`.
- `test_every_component_moves_the_fingerprint` (parametrizat pe toate cele 8 componente).
- `test_the_real_payload_maps_all_44_history_rows` — pe sarcina utilă REALĂ.

**Testul cel mai important al transei nu a rulat aici.**
`test_the_proposal_leaves_every_table_byte_identical` populează baza, ia un instantaneu
`SELECT *` peste toate cele nouă tabele atinse, rulează propunerea și compară rând cu rând.
Se sare în afara gazdei (§0.3) și **trebuie rulat pe server înainte ca transa să fie considerată
verificată**.

**.NET** — `dotnet build KBot.sln`: **0 erori**. (Nu s-a rulat `dotnet test KBot.sln`: suita
DevHarness deschide ferestre Adobe reale pe ecranul operatorului.)

| proiect | înainte | după |
|---|---|---|
| `KBot.Api.Tests` | 78 trecute / **1 căzut** | 87 trecute / **1 căzut** (`+9`) |
| `KBot.Common.Tests` | 58 trecute / 0 căzute | 66 trecute / 0 căzute (`+8`) |
| `KBot.Domain.Tests` | 14 trecute / **3 căzute** | 14 trecute / **3 căzute** |
| `KBot.App.Tests` | 169 trecute / **13 căzute** | 169 trecute / **13 căzute** |

**Zero teste noi căzute.** Cele 17 roșii sunt de pe `master` și s-au măsurat explicit, prin
`git stash`, ca să nu rămână bănuiala. Vezi §4.2.

---

## 4. Rămas neverificat sau amânat

### 4.1 Ce NU face transa asta, deliberat

- **Niciun formular, niciun dialog, niciun drag în `AdvancedTreeControl`.** 0048-04.
- **Niciun apel din `DownloadNodeAsync`.** A lega coordonatorul înainte să existe formularul
  ar construi exact ce interzice D-A: o ingestie care poate produce instantanee nerezolvate,
  fără unde să fie rezolvate. Coordonatorul are acum și calea de propunere, dar **nimic nu-l
  cheamă** — se exercită din DevHarness.
- **Niciun calcul ORD, nicio schimbare în `ReceptiiView`.** Ascunderea recepțiilor șterse din
  arbore e o schimbare de rută de citire + vedere și merge cu formularul.
- **Pasul 8.** Vezi §0.2.

### 4.2 ⚠ Teste CĂZUTE care NU sunt din transa asta (17)

Toate de pe `master`, măsurate cu `git stash` înainte și după. Niciuna nu atinge ingestia —
sunt DDF / Istoric / XFA:

```
KBot.Domain.Tests  (3)  DdfInfoTests.EtichetaRevizie_* (Pads/Handles/MissingDate)
KBot.Api.Tests     (1)  ApiClientTests.GetDdf_FormatsRevisionLabel_WithSpacePadding_Not_Zeroes
KBot.App.Tests    (13)  DdfViewTests (3), DdfXfaParserTests (2), IstoricViewTests (6),
                        MainFormNavItemsTests.Designer_WroteLiteralDiacritics_NotEscapes,
                        XfaXmlPreviewTests.ShowDocument_WithSiblingXml_RendersHeaderAndLines
```

Cinci dintre ele au aceeași cauză consemnată deja în worklog-ul 0048-02: `DdfInfo.vb:130` are
prefixul cu numărul reviziei comentat. Restul nu s-au investigat — sunt în afara transei.

### 4.3 Abateri de la literă, cu motivul

**§6 al planului cere dosarul local «în `KBot.App`» ȘI «exercitat din `KBot.DevHarness`». Cele
două nu pot fi amândouă adevărate.** `KBot.App` **referă** `KBot.DevHarness` (numai pe Debug,
`KBot.App.vbproj:82`), deci săgeata merge App ▸ DevHarness și un tip din App nu poate fi văzut
de harness. S-a rezolvat pe partea pe care planul o numește el însuși ca formă de urmat:
`KBotPaths` — celălalt magazin-fișier de lângă executabil — stă în **`KBot.Common`**. Acolo a
mers `AsociereStore`, iar POCO-ul `AsociereDosar` în `KBot.Domain`. Amândouă sunt văzute și de
App, și de harness.

**`TipReceptie` din payload nu se citește nicăieri, deci nu se cere.** Planul părinte §5.3 cerea
un 400 la lipsa lui. Citind `ObtineDateHeader` din `mdl_FX_Receptii`: hash-ul de identitate al
antetului se construiește din **`Tip`**, nu din `TipReceptie`, iar `FX_Receptii_R.TipReceptie`
primește `"NOU"` / `"EDIT"`, calculate. Cheia `TipReceptie` nu e atinsă de codul portat. Cerința
cade; nu s-a adăugat un 400 pentru un câmp pe care nimeni nu-l citește.

**Un indicator fără clasificație rămâne, cu avertisment.** Access folosea `INNER JOIN
ClasificatiiG`, deci un indicator a cărui clasificație lipsește CĂDEA din recordset și rândurile
lui de istoric rămâneau tăcut fără `CodAI`. Aici indicatorul rămâne, cu `Clsf`/`CodSSI` NULL și
un avertisment — aceeași alegere ca D19 din 0048-02, și regula casei (fără no-op-uri tăcute).

### 4.4 ⚠ `Detaliu` sosește în DOUĂ forme, și amândouă sunt legitime

Găsit citind `ForexeRunner.TryParseTable`, nu presupus. Sarcina utilă produsă de FOREXE/Access
(`FB_JOBS/resend/*.json`) păstrează `Detaliu` ca **listă imbricată**. Clientul K-BOT îl trimite
ca **șir care conține JSON**, fiindcă `JobResult.Tables` e
`Dictionary(Of String, String)` și fiecare celulă se aplatizează cu `prop.Value.ToString()`.

Serverul acceptă amândouă (`prelucrare_pasi._ca_lista`), fiindcă chiar le va primi pe amândouă.
Un șir care nu e o listă JSON **ridică** — fără degradare tăcută în «fără detalii», care ar
scrie o recepție fără linii. Două teste pinuiesc că ambele forme produc aceleași `INSERT`-uri.

**De decis, nu aici:** dacă e mai bine ca `PrelucrareRezultat.Tabele` să păstreze structura
imbricată (schimbare în `JobResult` / `ForexeRunner` / `WorkflowResultStore`) în loc ca serverul
să tolereze două forme. Nu s-a atins: e o schimbare în lanțul de descărcare, iar transa asta nu
are voie să-l atingă.

### 4.5 F27 — limita care nu e o poartă

Când **două** recepții au fost și create, și șterse înainte de prima descărcare, instantaneele
lor sunt de nedeosebit altfel decât după sumă și indicator. **Gruparea operatorului nu poate fi
verificată de mașină.** F14, F16 și regula «exact o ștergere pe lanț» sunt **paze, nu o
demonstrație**. Se scrie aici ca nimeni să nu le confunde mai târziu cu o dovadă.

### 4.6 Neverificat, pe scurt

- Nimic din transă **nu a rulat vreodată pe o bază MariaDB** (§0.3).
- `sql/0049_receptii_stergere.sql` **nu s-a aplicat nicăieri**; proba lui n-a fost rulată.
- Cele șapte `ALTER` `AUTO_INCREMENT` ale feliei 0048-01 §3 sunt **precondiție**: fără ele
  pașii 3–5 nu pot aloca chei deloc. Nu se știe dacă s-au aplicat pe vreo bază.
- Rata de potrivire a hash-ului față de datele migrate (§8 al planului părinte) — **nerulată**,
  același motiv.
- `FX_Istoric.Val_Receptie_T`: confirmată deschiderea 6 a planului părinte — **nimeni nu o
  scrie**. `FX_Istoric_Prelucreaza_Observatii` scrie `Val_Receptie` pe ambele ramuri, inclusiv
  pe cea a antetului. Coloana rămâne neatinsă de conductă.

---

## 5. Pentru cine vine după

### 5.1 Pentru cine construiește formularul (0048-04)

Propunerea îi dă tot: `receptii` (TOATE, inclusiv cele șterse și cele neatinse de rulare — sunt
ținte de plasare), `instantanee` cu `sugestie_idrr` / `sugestie_automata`, și `amprenta` de
trimis înapoi. Dosarul local există deja și supraviețuiește repornirii.

Serverul **nu se încrede în client**: fiecare veto pe care îl face formularul la momentul
plasării e reverificat la salvare, și acolo RIDICĂ.

### 5.2 Pentru cine construiește ORD — propoziția cerută de §5 al planului

> **O recepție ștearsă contribuie la totalul recepțiilor pentru plățile făcute ÎNAINTE de data
> ștergerii ei și nu contribuie deloc după (F22). Calculul trebuie să meargă pe lanțul de
> instantanee al recepției până la ultimul instantaneu aflat la sau înaintea datei plății, și
> să trateze un instantaneu cu `EsteStergere = 1` drept «de aici încolo recepția asta nu mai
> contribuie cu nimic». Data ștergerii ESTE `DataH` al acelui instantaneu — nu există și nu
> trebuie adăugată o coloană separată pentru ea (F21).**

Ce livrează transa asta pentru regula de mai sus: `FX_Receptii_R.Sters` (steagul) și
`FX_Receptii_H.EsteStergere` + `DataH` (data). Calculul nu e scris aici.

### 5.3 Interogarea de rulat pe gazdă pentru F20 / O3

```sql
SELECT H.IDRR, H.IDRH, H.DataH, H.Total, H.DIFH,
       (SELECT COUNT(*) FROM FX_Receptii R WHERE R.IDRH = H.IDRH) AS Linii
  FROM FX_Receptii_H H
 WHERE H.CodAngajament = '<cod>'
 ORDER BY H.IDRR, H.DataH, H.IDRH;
```

Se caută perechi de instantanee consecutive pe aceeași recepție, cu același `Total`, la
distanță de un minut, unde al doilea are `DIFH = 0`. Dacă tiparul se confirmă, formularul poate
eticheta singur salvările fără schimbare; dacă nu, comparația se face direct pe linii.
