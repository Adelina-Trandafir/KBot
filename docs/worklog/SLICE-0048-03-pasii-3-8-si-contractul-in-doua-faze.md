# SLICE-0048-03 — pașii 3–8 ai ingestiei FOREXE și contractul în două faze

**Istoric al tranșei inițiale.** [Completarea 0048-03](#slice-0048-03-completare) activează pasul 8, retrage F20/O3 și tolerarea Detaliu aplatizat; SQL/0049 ajunge la patru coloane. Interogarea din §5.3 este retrasă și nu se rulează. [0048-07](SLICE-0048-05-graficul-evolutiei.md#slice-0048-07) retrage ulterior F13 pe ambele căi; vetoul din descrierea de mai jos nu este contractul actual.

A treia transă din felia 0048 (portarea conductei de ingestie FOREXE din Access VBA în API-ul Flask). Plan: `PLAN_ForexeIngestSteps3to8` peste `docs/PLAN_ForexeIngest.md`. Temeiul deciziilor: `docs/FUNDAMENT_Asociere_Receptii.md` — regulile `F1`…`F27` și deciziile `D-A`…`D-M` de acolo, nere-deduse aici.

Transa livrează **pașii 3, 4a–4d, 5 și 7** ai conductei, **contractul în două faze** (propunere / salvare) cu tot ce ține de el — amprentă, decizii, validări, reconstituirea recepțiilor dispărute — plus jumătatea de client (apeluri, modele, dosarul local, o intrare în DevHarness). **Pasul 8 nu se execută, deliberat; vezi §0.2.**

**Transa NU livrează formular și NU leagă nimic în fluxul de descărcare.** Formularul e 0048-04. Vezi §4.1.

---

## 0. Poarta de citire — ce a arătat fiecare verificare cerută

### 0.1 `docs/FUNDAMENT_Asociere_Receptii.md` era în rădăcina depozitului, nu în `docs/`

Fișierul exista netracked la `./FUNDAMENT_Asociere_Receptii.md`, iar antetul lui însuși spunea «**Destination:** `docs/FUNDAMENT_Asociere_Receptii.md`». S-a mutat acolo și antetul a devenit `**Location:**`. Toate trimiterile din cod arată către calea din `docs/`.

### 0.2 `FX_Indicatori_Actualizare_Extrase` — CITITĂ, și răspunsul e «stop»

Deschiderea deschisă O5 se închide. Funcția are exact două instrucțiuni (`mdl_FX_Tasks_Receive_DWN.md`), și amândouă scriu `FX_Extrase`:

```vb
UPDATE FX_Extrase INNER JOIN FX_Plati ON FX_Extrase.Referinta     = FX_Plati.Referinta_TREZOR
   SET FX_Extrase.CodAI = [FX_Plati]![CodAI] WHERE FX_Extrase.CodAI Is Null
UPDATE FX_Extrase INNER JOIN FX_Plati ON FX_Extrase.ReferintaDest = FX_Plati.Referinta_TREZOR
   SET FX_Extrase.CodAI = [FX_Plati]![CodAI] WHERE FX_Extrase.CodAI Is Null
```

`FX_Extrase` e scos din scop de planul părinte §12. §0 al planului acestei transe spune textual: *«If it requires FX_Extrase, report that and stop rather than porting half of it.»* Deci **pasul 8 nu se execută**. Nu e uitat și nu e înlocuit cu ceva aproximativ: fiecare răspuns al rutei poartă un avertisment care spune că nu a rulat și de ce (`prelucrare_pasi.MESAJ_PAS8`), iar un test îl pinuiește.

**Ce înseamnă absența lui, spus pe față:** rândurile din `FX_Extrase` care s-ar fi legat de plățile scrise la pasul 5 rămân cu `CodAI` NULL. Nimic nu se strică; o legătură nu se face. Ambele `UPDATE`-uri sunt filtrate pe `CodAI Is Null`, deci felia care aduce `FX_Extrase` în scop recuperează tot restanțul dintr-o singură trecere. **D-G rămâne valabilă ca intenție.**

### 0.3 `DIFH = 0` ca marcaj de salvare-fără-schimbare (F20 / O3) — NECONFIRMABIL de aici

Nu s-a putut verifica, și motivul e concret, nu o scăpare: **de pe această mașină nu există acces la nicio bază de unitate.** `DB_CONFIG_NEW` (89.33.25.34) și `DB_CONFIG` (86.122.82.226) resping amândouă acreditările gazdei — `1045 Access denied for user 'AVACONT'@'188.27.66.13'`. Singurul cont care se conectează este `READER_DB_CONFIG`, iar `SHOW DATABASES` pe el întoarce **doar `AVACONT_COMUN` și `information_schema`**. Nu există niciun `FX_Receptii_H` de interogat.

**Nu blochează transa**, și planul spune de ce (§10 O3): F20 afectează ce poate **sugera formularul**, nu ce scrie transa asta. Pasul 4d e portat din `FX_CalculeazaDIF_Receptii_Tmp` oricum, identic, indiferent de răspuns. **Rămâne deschis pentru 0048-04**, cu interogarea de rulat scrisă în §5.3 de mai jos.

---

## 1. Ce s-a schimbat și de ce

### 1.1 Conducta — `routes/forexe/prelucrare_pasi.py` (fișier nou)

Pașii 3a–3e, 4a, 4b, 4d, 5 și 7, portați din sursa VBA reală, funcție cu funcție. Tot ce e aici rulează **identic în ambele faze** — nicio ramură de «propunere» nu se strecoară prin codul pașilor.

Ce merită știut înainte de a-l citi:

- **3b cheamă `CalculeazaValRezervareDif`**, pe care planul nu o numește deloc. Fără ea toate valorile `TipRand` de rezervare (`Rez_Initiala`, `Rez_Definitiva`, `Rez_Influenta`, `Rez_Zero` și cele trei cu `+`) ar rămâne goale, iar pașii 3c/3d — care filtrează pe ele — ar scrie zero rezervări. E portată integral.
- **3a întoarce harta `indice payload ▸ FX_Istoric.ID` pentru TOATE rândurile**, nu doar pentru cele inserate. `rand_istoric` din decizii e chiar acel indice (F24), iar D-F cere ca instantaneele rămase neașezate din rulări anterioare să poată fi decise acum — rândurile lor de istoric există deja în bază.
- **Două rânduri identice din același payload se colapsează într-unul.** `FX_Istoric.HASH` are index UNIQUE; fără colapsare, al doilea `INSERT` l-ar lovi. Access făcea la fel, fiindcă `rcHis.FindFirst` mergea peste un recordset care creștea cu fiecare `AddNew`.
- **4a: rândul de ștergere nu e deviat.** `Descriere = "Stergere receptie"` poartă `(activ:true)` ca orice antet, deci devine instantaneu pe calea normală, cu `EsteStergere = 1`. Nu are rânduri pe indicator, deci nu produce linii — și asta iese de la sine, fiindcă liniile lui nu există în istoric, nu fiindcă le-am opri noi (F21).
- **4b are regula F25 în SQL**, cu comentariul la fața locului. Potrivirea Access e pe `CLng(DataR)` — **granularitate de ZI** —, deci fără `Sters = 0` în interogarea de candidați o recepție creată în aceeași zi calendaristică în care fusese creată una ștearsă în martie s-ar potrivi peste aceea și i-ar suprascrie tăcut valorile.
- **4d ordonează după `DataH`, nu după `IDRH`.** Access ordona după `ID` cu comentariul «autonumber = ordine cronologică în tmp». Sub plasare MANUALĂ ordinea de inserare încetează să mai fie cea cronologică — operatorul poate atașa un instantaneu din ianuarie după unul din mai. `DataH` E axa timpului (F2). Cu plasare pur automată cele două ordini coincid, deci nimic din datele deja produse nu se schimbă.
- **5: `Data_plata` ia `FX_Istoric.DataFX`, nu data din Observații.** Contraintuitiv, dar e ce face `mdl_FX_Plati`: `data:` e parsată doar ca să fie VALIDATĂ (una neparsabilă sare rândul). Portat fidel, comentat la fața locului.

### 1.2 Asocierea — `routes/forexe/prelucrare_asociere.py` (fișier nou)

Singurul loc în care cele două faze diferă, plus tot ce ține de contract: amprenta, forma deciziilor, validările F13–F16, regulile etichetelor și 4c-bis.

- **Faza unu** rulează trecerea automată ca VBA-ul (două treceri, LIFO, fiecare recepție consumată o dată) și o raportează ca `sugestie_idrr` / `sugestie_automata` — **nu scrie niciun `IDRR`**. Un test verifică asta direct: toate interogările emise sunt `SELECT`.
- **Faza doi** aplică `decizii` și ignoră complet trecerea automată. Operatorul i-a văzut sugestiile; a o rula din nou ar însemna să ne batem cu propriul om.
- **`Final` = cel mai TÂRZIU instantaneu după `DataH`**, recalculat per recepție o singură dată după aplicarea tuturor deciziilor. `AsociazaFinal` făcea `Final` orice tocmai atașase — cu plasare manuală, regula aceea ar lăsa un instantaneu din februarie să devină `Final` pe o recepție care are deja unul din mai. `HASH` se rescrie doar când `TipReceptie` se schimbă.
- **Recepțiile ȘTERSE nu sunt candidate ale trecerii AUTOMATE.** Nimic nu se mai poate adăuga pe site unei recepții șterse, deci o potrivire automată pe ea ar fi mereu o coliziune — același raționament ca F25. Plasarea MANUALĂ pe una ștearsă rămâne permisă: vetoul e pe dată (F13), nu pe steag, iar un instantaneu dinaintea ștergerii îi aparține pe drept.

### 1.3 Ruta — `routes/forexe/prelucrare.py`

O singură rută, două moduri. Ce le desparte nu e adresa, ci ce are voie serverul să facă la coada tranzacției: `rollback()` sau `commit()`.

**Modul implicit e `propunere`** — un client care nu știe de faze primește faza care NU scrie. Tăcerea nu are voie să însemne «salvează».

Faza unu se termină cu `conn.rollback()` **necondiționat**, pe calea de succes. Nu e o cale de eroare: e chiar contractul. Câmpul `scrise` raportează **ce s-ar fi scris**, iar docstring-ul o spune — un contor care arată a rezultat dar descrie o rulare anulată e exact genul de lucru care se citește greșit.

Cele două 409-uri stau alături și nu se contopesc:

| cod-motiv | ce înseamnă | ce face clientul |
|---|---|---|
| `ALEGERE_UNITATE` (0048-02) | o clasificație se potrivește cu mai multe unități | întreabă operatorul, retrimite ACEEAȘI sarcină cu `alegeri` |
| `STARE_MODIFICATA` (0048-03) | baza s-a mișcat între propunere și salvare | descarcă din nou; nu există nimic de răspuns |

Un angajament poate avea nevoie de **două** drumuri dus-întors înainte ca operatorul să vadă formularul de asociere. Așa trebuie.

### 1.4 Amprenta — ce conține, exact (cerut explicit de plan §2.3)

```
FX_Istoric      COUNT(*), MAX(ID), MAX(DataFX)
FX_Receptii_R   COUNT(*), MAX(IDRR)
FX_Receptii_H   COUNT(*), MAX(IDRH), COUNT-ul celor cu IDRR NULL și Sters = 0
```

toate filtrate pe `CodAngajament`, concatenate `cheie=valoare|…` și trecute prin SHA-256; se păstrează primele 32 de caractere hex.

**De ce și `COUNT` și `MAX`:** `MAX` singur nu se mișcă la o ȘTERGERE de rând; `COUNT` singur nu se mișcă la o ștergere urmată de o inserare. Împreună prind ambele cazuri. **De ce ultimul câmp:** el E chiar mulțimea despre care operatorul ia decizii. Dacă altă sesiune a asociat ceva între timp, fișierul local nu mai descrie aceeași listă, iar `MAX(IDRH)` nu s-ar clinti. Nimic din ce se schimbă la CITIRE nu intră (fără `DTQ`, fără ceasuri).

**Se calculează ÎNAINTE de orice scriere, în ambele faze.** Luată la coada fazei întâi ar descrie starea scrisă — care e apoi derulată înapoi — și faza a doua nu s-ar potrivi niciodată.

### 1.5 4c-bis — recepțiile reconstituite (F26)

Nimic nu se inventează; fiecare câmp are o sursă în istoric. `Preluat`, `Incarcat` și `TipReceptie` se pun **exact ca la o recepție nou inserată de pasul 4b**, iar planul cere să se scrie aici ce a ieșit: **`TipReceptie = 'NOU'`, `Preluat = 1`, `Incarcat` NEATINS (rămâne NULL)**. Rândurile astea n-au trecut niciodată prin `ListaReceptii`, deci oriunde altundeva ar însemna valorile astea ceva, aici înseamnă ceva ușor diferit.

`CreditBugetar` se ia dintr-un rând `RHR` existent cu același `CodAI`. **Dacă nu există niciunul, se RIDICĂ** — un zero acolo nu se deosebește de un zero real și ar fi citit ca fapt pe veci.

### 1.6 Schema — `sql/0049_receptii_stergere.sql`

Trei coloane noi, toate `NOT NULL DEFAULT 0`, cu probă `information_schema` de rulat **înainte** (și așteptările ei scrise pe litere) și cu `COMMENT` pe fiecare coloană.

`FX_Receptii_R.Sters` · `FX_Receptii_R.Reconstituit` · `FX_Receptii_H.EsteStergere`

Se aplică **și pe `AVACONT_SURSA`**, ca `0048_alegeri_unitate.sql`. Scutirea care ține cele șapte `ALTER`-uri `AUTO_INCREMENT` ale feliei 0048-01 §3.1 departe de schema de referință **nu se aplică aici**: acolo scutirea păzea o poartă de siguranță a migrării (un rând fără cheie ar primi tăcut următorul număr); aici nu se dezarmează nimic.

`Sters` și `Reconstituit` **nu se colapsează**: `Sters` spune ce s-a întâmplat cu recepția, `Reconstituit` spune de unde știm că a existat. Fără al doilea, «câte recepții am reconstituit noi» nu are răspuns.

### 1.7 Clientul

`KBot.Domain/AsociereInfo.vb` (nou): `ActiuneAsociere`, `PrelucrarePropunere`, `ReceptiePropusa`, `LinieReceptie`, `InstantaneuPropus`, `LinieInstantaneu`, `DecizieAsociere`, `AsociereDosar`. `PrelucrareStare` capătă `Propunere`, iar `PrelucrareRaspuns` capătă `Propunere As PrelucrarePropunere` — un singur tip pentru toate stările, ca la 409-ul din 0048-02.

`KBot.Api`: `CerePropunereAsync` și `SalveazaAsociereaAsync`, amândouă pe aceeași rută, cu `mod` diferit. Un set **separat** de opțiuni JSON (`_jsonFaraNull`) ține `amprenta` și `decizii` afară din corpul unei propuneri; `_json` cel comun **nu** s-a atins, fiindcă «omite null-urile» ar fi o schimbare de contract pentru fiecare altă cerere a clientului.

`KBot.Common/AsociereStore.vb` + `AsociereDosar`: dosarul local `<AppDir>\Asociere\<cod>.json`, scris prin fișier temporar + `File.Move` (o cădere în mijlocul scrierii ar lăsa un JSON trunchiat, adică exact un dosar corupt pe angajamentul la care operatorul tocmai lucra).

`PrelucrareCoordinator.CerePropunereAsync` — aceeași buclă de întrebări ca `TrimiteAsync`, fiindcă 409 `ALEGERE_UNITATE` se poate declanșa și în faza întâi.

### 1.8 Corecțiile C1 și C2 — migratorul

- **`FX_ORD_TBL_REC` iese din lista de excluderi** (`KBot.Migrator/Transfer/TableMaps.vb`) și capătă o hartă reală, plus o intrare în `routes/migrare/tables.py` cu un fel de selecție nou, `BY_ORD_TBL`, și familia de chei `ord_tbl` în `routing.py` (construită din `FX_ORD_TBL`, cheie `IDORDTBL`, moștenind apartenența de la `IDORD`). Coloanele Access s-au citit din `artifacts/accdb-schema/FX_2026.md`: `IDORDRECP` poartă 475, 476… — oglinda serverului VECHI — deci **nu călătorește**; pe MariaDB coloana omonimă e `AUTO_INCREMENT`.
- **`FX_Receptii_Plati` iese din `ALL` și intră în `OUT_OF_SCOPE`** în migratorul Python (era ÎN listă, ceea ce era greșeala propriu-zisă). În `TableMaps.vb` era deja exclus.
- `FX_ORD_TBL.IDRP` e **mort**, nu doar orfan: arăta către `FX_Receptii_Plati`. Nota hărții s-a rescris.

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

**Mutat**: `FUNDAMENT_Asociere_Receptii.md` ▸ `docs/FUNDAMENT_Asociere_Receptii.md`. **Șters**: `src/KBot.App/Forexe/AsociereStore.vb` (mutat — vezi §4.3).

---

## 3. Rezultatele testelor

**Python** — `PYTHON/.venv`, din `PYTHON/`:

```
410 trecute, 15 sărite, 0 căzute        (înainte de transă: 346 / 14)
```

`+64` teste. Sărirea în plus e modulul `test_forexe_prelucrare_live.py`, care se sare **în întregime** în afara gazdei (`config.py` / `main` indisponibile) — vezi §0.3.

Cele mai grele dintre ele:

- `test_the_default_mode_is_the_proposal_and_it_never_commits` — conducta chiar rulează (`INSERT`-urile se emit) și abia apoi se anulează.
- `test_step4b_never_matches_a_deleted_reception_even_on_the_same_calendar_day` (F25).
- `test_stergere_receptie_becomes_a_snapshot_with_no_lines` (F21).
- `test_the_date_veto_rejects_on_a_one_second_difference` (F13) — TIMESTAMP complet.
- `test_the_chain_end_check_is_skipped_for_a_chain_ending_in_a_deletion` (F15).
- `test_final_lands_on_the_latest_snapshot_by_data_h_not_the_last_attached`.
- `test_a_reconstructed_chain_without_a_deletion_is_rejected` / `…with_two_deletions…`.
- `test_a_stale_data_h_fails_loudly_instead_of_associating_the_wrong_row`.
- `test_every_component_moves_the_fingerprint` (parametrizat pe toate cele 8 componente).
- `test_the_real_payload_maps_all_44_history_rows` — pe sarcina utilă REALĂ.

**Testul cel mai important al transei nu a rulat aici.** `test_the_proposal_leaves_every_table_byte_identical` populează baza, ia un instantaneu `SELECT *` peste toate cele nouă tabele atinse, rulează propunerea și compară rând cu rând. Se sare în afara gazdei (§0.3) și **trebuie rulat pe server înainte ca transa să fie considerată verificată**.

**.NET** — `dotnet build KBot.sln`: **0 erori**. (Nu s-a rulat `dotnet test KBot.sln`: suita DevHarness deschide ferestre Adobe reale pe ecranul operatorului.)

| proiect | înainte | după |
|---|---|---|
| `KBot.Api.Tests` | 78 trecute / **1 căzut** | 87 trecute / **1 căzut** (`+9`) |
| `KBot.Common.Tests` | 58 trecute / 0 căzute | 66 trecute / 0 căzute (`+8`) |
| `KBot.Domain.Tests` | 14 trecute / **3 căzute** | 14 trecute / **3 căzute** |
| `KBot.App.Tests` | 169 trecute / **13 căzute** | 169 trecute / **13 căzute** |

**Zero teste noi căzute.** Cele 17 roșii sunt de pe `master` și s-au măsurat explicit, prin `git stash`, ca să nu rămână bănuiala. Vezi §4.2.

---

## 4. Rămas neverificat sau amânat

### 4.1 Ce NU face transa asta, deliberat

- **Niciun formular, niciun dialog, niciun drag în `AdvancedTreeControl`.** 0048-04.
- **Niciun apel din `DownloadNodeAsync`.** A lega coordonatorul înainte să existe formularul ar construi exact ce interzice D-A: o ingestie care poate produce instantanee nerezolvate, fără unde să fie rezolvate. Coordonatorul are acum și calea de propunere, dar **nimic nu-l cheamă** — se exercită din DevHarness.
- **Niciun calcul ORD, nicio schimbare în `ReceptiiView`.** Ascunderea recepțiilor șterse din arbore e o schimbare de rută de citire + vedere și merge cu formularul.
- **Pasul 8.** Vezi §0.2.

### 4.2 ⚠ Teste CĂZUTE care NU sunt din transa asta (17)

Toate de pe `master`, măsurate cu `git stash` înainte și după. Niciuna nu atinge ingestia — sunt DDF / Istoric / XFA:

```
KBot.Domain.Tests  (3)  DdfInfoTests.EtichetaRevizie_* (Pads/Handles/MissingDate)
KBot.Api.Tests     (1)  ApiClientTests.GetDdf_FormatsRevisionLabel_WithSpacePadding_Not_Zeroes
KBot.App.Tests    (13)  DdfViewTests (3), DdfXfaParserTests (2), IstoricViewTests (6),
                        MainFormNavItemsTests.Designer_WroteLiteralDiacritics_NotEscapes,
                        XfaXmlPreviewTests.ShowDocument_WithSiblingXml_RendersHeaderAndLines
```

Cinci dintre ele au aceeași cauză consemnată deja în worklog-ul 0048-02: `DdfInfo.vb:130` are prefixul cu numărul reviziei comentat. Restul nu s-au investigat — sunt în afara transei.

### 4.3 Abateri de la literă, cu motivul

**§6 al planului cere dosarul local «în `KBot.App`» ȘI «exercitat din `KBot.DevHarness`». Cele două nu pot fi amândouă adevărate.** `KBot.App` **referă** `KBot.DevHarness` (numai pe Debug, `KBot.App.vbproj:82`), deci săgeata merge App ▸ DevHarness și un tip din App nu poate fi văzut de harness. S-a rezolvat pe partea pe care planul o numește el însuși ca formă de urmat: `KBotPaths` — celălalt magazin-fișier de lângă executabil — stă în **`KBot.Common`**. Acolo a mers `AsociereStore`, iar POCO-ul `AsociereDosar` în `KBot.Domain`. Amândouă sunt văzute și de App, și de harness.

**`TipReceptie` din payload nu se citește nicăieri, deci nu se cere.** Planul părinte §5.3 cerea un 400 la lipsa lui. Citind `ObtineDateHeader` din `mdl_FX_Receptii`: hash-ul de identitate al antetului se construiește din **`Tip`**, nu din `TipReceptie`, iar `FX_Receptii_R.TipReceptie` primește `"NOU"` / `"EDIT"`, calculate. Cheia `TipReceptie` nu e atinsă de codul portat. Cerința cade; nu s-a adăugat un 400 pentru un câmp pe care nimeni nu-l citește.

**Un indicator fără clasificație rămâne, cu avertisment.** Access folosea `INNER JOIN ClasificatiiG`, deci un indicator a cărui clasificație lipsește CĂDEA din recordset și rândurile lui de istoric rămâneau tăcut fără `CodAI`. Aici indicatorul rămâne, cu `Clsf`/`CodSSI` NULL și un avertisment — aceeași alegere ca D19 din 0048-02, și regula casei (fără no-op-uri tăcute).

### 4.4 ⚠ `Detaliu` sosește în DOUĂ forme, și amândouă sunt legitime

Găsit citind `ForexeRunner.TryParseTable`, nu presupus. Sarcina utilă produsă de FOREXE/Access (`FB_JOBS/resend/*.json`) păstrează `Detaliu` ca **listă imbricată**. Clientul K-BOT îl trimite ca **șir care conține JSON**, fiindcă `JobResult.Tables` e `Dictionary(Of String, String)` și fiecare celulă se aplatizează cu `prop.Value.ToString()`.

Serverul acceptă amândouă (`prelucrare_pasi._ca_lista`), fiindcă chiar le va primi pe amândouă. Un șir care nu e o listă JSON **ridică** — fără degradare tăcută în «fără detalii», care ar scrie o recepție fără linii. Două teste pinuiesc că ambele forme produc aceleași `INSERT`-uri.

**De decis, nu aici:** dacă e mai bine ca `PrelucrareRezultat.Tabele` să păstreze structura imbricată (schimbare în `JobResult` / `ForexeRunner` / `WorkflowResultStore`) în loc ca serverul să tolereze două forme. Nu s-a atins: e o schimbare în lanțul de descărcare, iar transa asta nu are voie să-l atingă.

### 4.5 F27 — limita care nu e o poartă

Când **două** recepții au fost și create, și șterse înainte de prima descărcare, instantaneele lor sunt de nedeosebit altfel decât după sumă și indicator. **Gruparea operatorului nu poate fi verificată de mașină.** F14, F16 și regula «exact o ștergere pe lanț» sunt **paze, nu o demonstrație**. Se scrie aici ca nimeni să nu le confunde mai târziu cu o dovadă.

### 4.6 Neverificat, pe scurt

- Nimic din transă **nu a rulat vreodată pe o bază MariaDB** (§0.3).
- `sql/0049_receptii_stergere.sql` **nu s-a aplicat nicăieri**; proba lui n-a fost rulată.
- Cele șapte `ALTER` `AUTO_INCREMENT` ale feliei 0048-01 §3 sunt **precondiție**: fără ele pașii 3–5 nu pot aloca chei deloc. Nu se știe dacă s-au aplicat pe vreo bază.
- Rata de potrivire a hash-ului față de datele migrate (§8 al planului părinte) — **nerulată**, același motiv.
- `FX_Istoric.Val_Receptie_T`: confirmată deschiderea 6 a planului părinte — **nimeni nu o scrie**. `FX_Istoric_Prelucreaza_Observatii` scrie `Val_Receptie` pe ambele ramuri, inclusiv pe cea a antetului. Coloana rămâne neatinsă de conductă.

---

## 5. Pentru cine vine după

### 5.1 Pentru cine construiește formularul (0048-04)

Propunerea îi dă tot: `receptii` (TOATE, inclusiv cele șterse și cele neatinse de rulare — sunt ținte de plasare), `instantanee` cu `sugestie_idrr` / `sugestie_automata`, și `amprenta` de trimis înapoi. Dosarul local există deja și supraviețuiește repornirii.

Serverul **nu se încrede în client**: fiecare veto pe care îl face formularul la momentul plasării e reverificat la salvare, și acolo RIDICĂ.

### 5.2 Pentru cine construiește ORD — propoziția cerută de §5 al planului

> **O recepție ștearsă contribuie la totalul recepțiilor pentru plățile făcute ÎNAINTE de data
> ștergerii ei și nu contribuie deloc după (F22). Calculul trebuie să meargă pe lanțul de
> instantanee al recepției până la ultimul instantaneu aflat la sau înaintea datei plății, și
> să trateze un instantaneu cu `EsteStergere = 1` drept «de aici încolo recepția asta nu mai
> contribuie cu nimic». Data ștergerii ESTE `DataH` al acelui instantaneu — nu există și nu
> trebuie adăugată o coloană separată pentru ea (F21).**

Ce livrează transa asta pentru regula de mai sus: `FX_Receptii_R.Sters` (steagul) și `FX_Receptii_H.EsteStergere` + `DataH` (data). Calculul nu e scris aici.

### 5.3 Interogarea de rulat pe gazdă pentru F20 / O3

**RETRASĂ prin completarea 0048-03.** Se păstrează numai ca dovadă a ipotezei abandonate; nu se execută.

```sql
SELECT H.IDRR, H.IDRH, H.DataH, H.Total, H.DIFH,
       (SELECT COUNT(*) FROM FX_Receptii R WHERE R.IDRH = H.IDRH) AS Linii
  FROM FX_Receptii_H H
 WHERE H.CodAngajament = '<cod>'
 ORDER BY H.IDRR, H.DataH, H.IDRH;
```

Se caută perechi de instantanee consecutive pe aceeași recepție, cu același `Total`, la distanță de un minut, unde al doilea are `DIFH = 0`. Dacă tiparul se confirmă, formularul poate eticheta singur salvările fără schimbare; dacă nu, comparația se face direct pe linii.
---

<a id="slice-0048-03-completare"></a>

Completarea de mai jos înlocuiește explicit deciziile inițiale despre pasul 8, F20/O3 și structura aplatizată. Numerotarea secțiunilor ei rămâne proprie; probele și limitele sunt cele ale completării. Reunire documentară D001-90.

<a id="completare-slice-0048-03-completare--pasul-8-structura-care-călătorește-căile-operatorului"></a>

## SLICE-0048-03-completare — pasul 8, structura care călătorește, căile operatorului

Închiderea feliei 0048-03. Plan: `docs/PLAN_ForexeIngest_0048-03_Completion.md`, peste `docs/PLAN_ForexeIngest.md` și `docs/FUNDAMENT_Asociere_Receptii.md`.

Transa **corectează trei decizii aplicate greșit**, **retrage o constatare care s-a dovedit imposibilă**, și **face patru lucruri care fuseseră amânate și nu aveau voie să fie**. Niciun ciot, niciun no-op documentat, nimic „lăsat pentru o felie viitoare".

---

<a id="completare-0-poarta-de-citire--ce-a-arătat-fiecare-verificare-cerută"></a>

### 0. Poarta de citire — ce a arătat fiecare verificare cerută

<a id="completare-01-nicio-bază-și-niciun-server-nu-au-fost-atinse"></a>

#### 0.1 Nicio bază și niciun server nu au fost atinse

Cerința §0.1 a planului. **Nu s-a încercat nicio conexiune** — nici ca să se confirme o coloană, nici ca să se ruleze un test. Fiecare fapt despre schemă vine din `MariaDB_Schema/000_DEMO.sql` și din exportul Access de la `C:\AVACONT\FX_System_Export`, citite direct.

<a id="completare-02-numele-de-coloane-ale-pasului-8--verificate-toate-cinci"></a>

#### 0.2 Numele de coloane ale pasului 8 — VERIFICATE, toate cinci

Cerința §2.1: orice nepotrivire ▸ stop. Nu a fost niciuna. Din `000_DEMO.sql`:

| Tabel | Coloană | Tip |
|---|---|---|
| `FX_Extrase` | `Referinta` | `varchar(255)` |
| `FX_Extrase` | `ReferintaDest` | `varchar(255)` |
| `FX_Extrase` | `CodAI` | `varchar(255)` |
| `FX_Plati` | `Referinta_TREZOR` | `varchar(255)` |
| `FX_Plati` | `CodAI` | `varchar(255)` |

<a id="completare-03-marcajul-nu-s-a-schimbat-nimic--găsit-în-schemă-și-cu-mecanismul-access-lângă-el"></a>

#### 0.3 Marcajul „nu s-a schimbat nimic" — GĂSIT în schemă, și cu mecanismul Access lângă el

Cerința §4.4: se stabilește din schemă **care coloană de pe `FX_Receptii_H`** îl poartă, sau se raportează că lipsește. **Există**, și e chiar `FX_Receptii_H.Sters`, deja în `000_DEMO.sql` (`tinyint(1) NULL`), deci nu se adaugă nimic.

Cum îl punea Access, citit în `FORMS/frmFX_DUBII_LISTA_HN.md`:

- e o **casetă de bifat** legată direct de coloană (`ControlSource = Sters`, eveniment `AfterUpdate`);
- la bifare, `Form_AfterUpdate` **golește** `DIFH`/`DIFHC` pe antet și `DIF`/`DIFC` pe linii (`UPDATE … SET DIFH=Null, DIFHC=Null WHERE ID=…`), fiindcă un instantaneu care nu consemnează nicio schimbare nu are o diferență de arătat;
- e chiar **poarta de salvare**: `frmFX_DUBII.btnSav_Click` refuză cât timp `DCount("ID", "tmpFX_Receptii_H", "TmpIDRecR IS NULL AND Sters=False") > 0` — adică un instantaneu poate rămâne neasociat **doar dacă** e marcat.

Asta confirmă §1.6 al fundamentului la literă: `Sters` pe latura de ISTORIC nu e un mecanism de ascundere, e afirmația *acest instantaneu nu consemnează nicio schimbare*. Nu se confundă cu `FX_Receptii_R.Sters` (D-L, «recepția a fost ștearsă pe site»): alt tabel, alt fapt. Codul portat îl scria deja, prin acțiunea `ignorat`; ce lipsea era proba că e coloana potrivită, iar acum e făcută.

<a id="completare-04-două-documente-cerute-de-plan-nu-există-și-n-au-existat-niciodată-în-depozit"></a>

#### 0.4 Două documente cerute de plan NU EXISTĂ, și n-au existat niciodată în depozit

Căutate în tot depozitul, în tot istoricul git (`git log --all --name-only`) și pe disc, în `Desktop`/`Documents`/`Downloads`/`C:\AVACONT`:

- **`docs/PLAN_ForexeIngestSteps3to8.md`** — §6.5 și §8.10 cer corectarea §6 al lui. Fișierul nu e nicăieri și **nu apare în niciun commit**, deși e citat de F26, de O1, de worklog-ul 0048-03 și de antetul lui `routes/forexe/prelucrare.py`. A fost scris și s-a lucrat după el, dar nu a fost comis niciodată.
- **copia veche a lui `FUNDAMENT_Asociere_Receptii.md` din afara lui `docs/`** — §8.1 cere ștergerea ei. **Era deja ștearsă**: felia 0048-03 a mutat-o în `docs/` (worklog-ul ei §0.1). Nu există două revizii în circulație.

Nicio corecție nu s-a pierdut din cauza asta: amândouă s-au scris în `docs/FUNDAMENT_Asociere_Receptii.md`, ca **corecția C5**, fiindcă el e documentul care supraviețuiește. C5 consemnează și de ce §6 al planului absent era imposibil de urmat, și retragerea cerinței lui §5.3.

---

<a id="completare-1-ce-s-a-schimbat-și-de-ce"></a>

### 1. Ce s-a schimbat și de ce

<a id="completare-11-pasul-8--portat-2"></a>

#### 1.1 Pasul 8 — portat (§2)

`FX_Indicatori_Actualizare_Extrase` din `mdl_FX_Tasks_Receive_DWN`. Două instrucțiuni, în ordine, a doua prinzând doar ce a lăsat prima:

```sql
UPDATE FX_Extrase E INNER JOIN FX_Plati P ON E.Referinta     = P.Referinta_TREZOR
   SET E.CodAI = P.CodAI WHERE E.CodAI IS NULL
UPDATE FX_Extrase E INNER JOIN FX_Plati P ON E.ReferintaDest = P.Referinta_TREZOR
   SET E.CodAI = P.CodAI WHERE E.CodAI IS NULL
```

- **Ordinea nu e o întâmplare și cele două nu se contopesc într-una cu `OR`.** Amândouă filtrează pe `CodAI IS NULL`, deci a doua trebuie să vadă rândurile pe care prima le-a completat deja. Contopite, `Referinta` și `ReferintaDest` ar concura pe același rând și ar câștiga oricare, tăcut.
- **Necondiționat**, la coada pașilor 1–7, în aceeași tranzacție. Nu se pazește cu niciun steag `are` și nu se sare când pasul 5 n-a scris nimic — exact ca originalul Access. `FX_Extrase` poate purta restanțe din rulări mai vechi, iar filtrul `CodAI IS NULL` face ca fiecare trecere să le recupereze.
- **Rulează în amândouă fazele**, fiindcă amândouă parcurg același drum; în propunere se derulează înapoi cu restul. **Rezultatul lui NU se raportează în propunere**: nu e ceva despre care operatorul are de decis, e o legătură mecanică între extrase și plăți. La salvare iese sub `scrise.FX_Extrase`.
- **Avertismentul care spunea că pasul 8 nu rulează a fost șters**, împreună cu `MESAJ_PAS8` și cu testul care îl pinuia.
- Comentariul la fața locului consemnează ce s-a cântărit: dacă două rânduri `FX_Plati` ar împărți o `Referinta_TREZOR`, MariaDB alege unul fără să spună care — aceeași purtare ca Access. Pasul 5 deduplică plățile chiar pe `Referinta_TREZOR`, deci nu ar trebui să apară.

`PLAN_ForexeIngest.md` §12 e amendat: `FX_Extrase` rămâne în afara scopului **cu excepția** acestor două instrucțiuni. §13 punctul 3 e închis.

<a id="completare-12-f20-retrasă-pasul-4d-mutat-unde-îl-cheamă-access-1-4"></a>

#### 1.2 F20 retrasă, pasul 4d mutat unde îl cheamă Access (§1, §4)

**F20 e RETRASĂ**, și nu prin măsurătoare, ci prin raționament: `DIFH` **nu e ceva ce trimite FOREXE**. Se calculează local, de noi, în `FX_CalculeazaDIF_Receptii_Tmp`, **după** ce un instantaneu a fost asociat unei recepții. În clipa în care ar fi nevoie de judecata «s-a schimbat ceva?», `DIFH` nu există încă. **O3 se închide prin retragere, nu prin confirmare** — nicio interogare pe date vii nu mai e datorată, iar interogarea scrisă în worklog-ul 0048-03 §5.3 nu mai trebuie rulată.

Ce s-a schimbat în cod: **nimic de șters, fiindcă nimic nu citea `DIFH` ca să decidă ceva.** Căutat peste tot Python-ul și tot `src/`: singurii cititori sunt calea de AFIȘARE din Recepții (felia 0015-02, eticheta plutitoare), care e un consumator legitim al unei cifre calculate, nu o clasificare. Se consemnează ca fapt verificat, nu ca presupunere.

`step4d_calculeaza_dif` **rulează acum în amândouă fazele**, prin `_pas4d_pe_receptiile_atinse`, exact unde îl cheamă Access — după 4c. Înainte rula doar la salvare, ceea ce însemna o ramură care nu se testa decât la salvare.

**F17 e amendată:** nu există clasificare automată de „salvare fără schimbare", în niciun punct. Două blocuri cu aceleași cifre pot fi o salvare care n-a schimbat nimic, sau o modificare reală pe **altă** recepție care se întâmplă să aibă aceeași sumă — mașina nu le poate deosebi, omul da. Formularul (0048-04) arată «identic cu instantaneul dinainte în acest lanț» ca **informație**, recalculată la fiecare drag, care nu scrie niciodată nimic. Marcajul e o **acțiune a operatorului**, pe `FX_Receptii_H.Sters` — vezi §0.3.

<a id="completare-13-structura-călătorește-de-la-site-până-în-baza-de-date-3-decizia-d-n"></a>

#### 1.3 Structura călătorește, de la site până în baza de date (§3, decizia D-N)

**Ce e imbricat NU s-a ghicit.** Enumerat din definițiile de workflow, din sarcina utilă reală și din codul executorului. Tiparul e: un `ForEachVar` al cărui `collectFields` numește un câmp pe care un `ScrapeTable` **interior** îl scrie cu `saveTo`. Peste toate cele șase `.wfl` din `src/KBot.Forexe/Workflows/` sunt **exact două**:

| Tabel | Coloană | Ce e | Cine o citește |
|---|---|---|---|
| `ListaReceptii_results` | `Detaliu` | listă de obiecte — liniile recepției | pasul 4b |
| `TabelIndicatori_results` | `BugetIndicator` | listă de obiecte — bugetul indicatorului | **nimeni** (defectul D18, portat) |

Restul câmpurilor din `collectFields` (`Tip`, `Data`, `Suma`, `TipReceptie`, `DescriereReceptie`, `CodIndicator`) sunt scrise de `<Read saveTo>`, deci scalare. `TabelIstoric` e un `ScrapeTable` simplu, fără buclă, deci fără imbricare. `ListaAngajamente` la fel.

**Confirmat pe date reale, nu doar din definiții:** sarcina utilă de probă `PYTHON/tests/fixtures/prelucrare_AAB37CNBK95.json` poartă exact aceste două coloane ca liste imbricate, și nicio alta.

**Unde se pierdea și unde s-a oprit.** Executorul păstra deja structura — `WorkflowExecutor.Actions.vb:BuildCollectedRow` face `JToken.Parse(fromVar)`, cu comentariul «Preservă structura JArray/JObject». Pierderea era un pas mai încolo, în `ForexeRunner.TryParseTable`: `row(prop.Name) = prop.Value.ToString()`. Pentru un scalar e fără pierdere; pentru un `JArray` dă chiar textul JSON — o listă deghizată în text, pe care serverul trebuia apoi să o parseze a doua oară.

**Clientul.** Trei tipuri noi în `KBot.Domain/CelulaTabel.vb`:

- `CelulaTabel` — o celulă e **exact una** din trei lucruri, recursiv: text, listă ordonată de celule, sau set de celule cu nume. Citirea ca text a unei celule imbricate **RIDICĂ**; turtirea tăcută e chiar defectul reparat.
- `RandTabel` — un rând, set ordonat de celule cu nume (ordinea coloanelor de pe site).
- `TabelRezultat` — un tabel, listă ordonată de rânduri, cu `ColoaneImbricate()` care răspunde «ce e imbricat aici» **din date**, nu dintr-o listă ținută de mână.

`JobResult.Tables` și `PrelucrareRezultat.Tabele` sunt acum `Dictionary(Of String, TabelRezultat)`.

**Serializarea: o punte, nu un `JsonConverter`.** `JsonConverter(Of T).Read` primește `ByRef Utf8JsonReader`, iar `Utf8JsonReader` e `ref struct` — o formă pe care compilatorul VB.NET o refuză din principiu (`BC30668`). **Niciun proiect VB nu poate scrie un convertor System.Text.Json.** Deci conversia e explicită, în `TabeleJson`, la granițele de serializare care există: cererea HTTP, depozitul local de rezultate, dosarul de asociere. `PrelucrareRezultat.Tabele` poartă `<JsonIgnore>`, iar `TabeleSerializate As JsonObject` (`<JsonPropertyName("Tabele")>`) e puntea — deci numele pe disc nu se schimbă și fișierele scrise înainte se citesc mai departe.

**Citit în depanator** (cerință, nu înfrumusețare):

- `DebuggerDisplay` pe toate trei tipurile: celula arată `listă[3]` sau `"Plata ces"`, rândul arată `7 coloane · Tip=«Plata ces» · Detaliu=listă[3]`, tabelul arată `12 rânduri · imbricate: Detaliu`;
- dumpul JSON local era deja `WriteIndented = True` (`WorkflowResultStore._jsonOptions`), deci structura se vede într-un editor de text;
- `PrelucrareRezultat.ToDebugString()` scrie tot arborele, indentat, cu coloanele imbricate numite pe fiecare tabel;
- **și istoricul lucrărilor** (`ForexeRunner.InregistreazaRezultat`) numește acum coloanele imbricate — e primul loc în care cineva se uită după o descărcare.

**Serverul.** Calea tolerantă a plecat odată cu aplatizarea. `_ca_lista` a devenit `cere_lista`:

- listă ▸ se acceptă;
- șir **negol** ▸ **RESPINS**, cu un mesaj care numește coloana și spune că *clientul trimite valoarea aplatizată* — nu «JSON nevalid», care ar trimite pe cine îl citește să caute o virgulă lipsă într-un șir perfect corect;
- șir gol ▸ depinde de apelant (`gol_permis`): pentru `BugetIndicator` înseamnă «tabelul interior n-a avut rânduri», care e chiar ce scrie `BuildCollectedRow`; pentru `Detaliu` rămâne o eroare.

De ce se poate închide ușa fără plasă: **D2 spune că Access e scos din uz și nu există un al doilea scriitor, iar conducta nu a rulat încă niciodată pe o bază reală.** Nu există sarcini utile stocate în forma veche de recuperat.

**`BugetIndicator` i se verifică forma deși nimeni nu-l citește.** Dacă EL sosește ca text, clientul aplatizează — iar clientul acela aplatizează și `Detaliu`, pe care chiar îl citim. E cel mai ieftin loc în care se prinde regresia, și prinde toată sarcina utilă deodată.

**Cealaltă jumătate a regulii (§3.6).** Fiecare loc din `routes/forexe/` care citește o celulă de payload ca text o cere acum prin `text_celula(valoare, unde, coloana)`, care **ridică** pe o listă sau un obiect. Un `.strip()` peste o listă ar crăpa cu `AttributeError` la prima rulare reală; un `str()` peste ea ar fi mai rău — ar scrie liniștit `[{'Cod': 'AAB'}]` într-o coloană de bază de date și nimeni n-ar afla vreodată. Locurile: cele patru celule ale rândului de istoric (3a), cele două de identificare și cele patru de bani ale indicatorului (2), cele patru ale antetului de recepție (4b) și cele cinci frunze din interiorul lui `Detaliu`.

<a id="completare-14-f28--reconstituirea-neverificabilă-5"></a>

#### 1.4 F28 — reconstituirea neverificabilă (§5)

`FX_Receptii_R` capătă `ReconstituitNesigur tinyint(1) NOT NULL DEFAULT 0`, în **același** `sql/0049_receptii_stergere.sql` (fișierul nu s-a aplicat nicăieri, deci se amendează, nu se dublează), și pe `AVACONT_SURSA` la fel. Proba `information_schema` de dinainte de rulare și așteptările de după sunt actualizate: patru coloane noi acum, nu trei.

Trei coloane, trei fapte, niciodată colapsate:

| Coloană | Înseamnă |
|---|---|
| `FX_Receptii_R.Sters` | recepția a fost ștearsă pe site |
| `FX_Receptii_R.Reconstituit` | a fost reconstruită din propriile ei instantanee; n-a avut niciodată rând în `ListaReceptii` |
| `FX_Receptii_R.ReconstituitNesigur` | în clipa reconstituirii, altă reconstituire pe același angajament făcea gruparea imposibil de verificat |

Regula e o **funcție pură**, `f28_de_marcat(reconstituite)`: una singură ▸ nimic; două sau mai multe ▸ **toate**. Ambiguitatea e ÎNTRE ele, deci nu aparține niciuneia singure — și nici măcar celei adăugate ultima: fiecare instantaneu al oricăreia ar fi putut sta pe cealaltă. Exact condiția lui F27 și nimic mai larg.

`marcheaza_reconstituirile_nesigure` o aplică la salvare, la coada pasului 4c, **recitind reconstituirile din tabel** — nu numărând deciziile — ca să intre în socoteală și cele rămase din rulări mai vechi: două reconstituiri făcute în două sesiuni diferite sunt exact la fel de imposibil de deosebit ca două făcute în aceeași sesiune.

**Nu se șterge niciodată.** Funcția spune doar pe cine să marchezi, nu și pe cine să demarchezi, iar un test pinuiază chiar absența acelui drum.

**Se vede:** răspunsul de salvare poartă un avertisment românesc care numește recepțiile; `GET /api/forexe/receptii` întoarce `reconstituit` și `reconstituit_nesigur`; arborele Recepțiilor pune un semn pe rând — `⟲ reconstituită`, sau `⚠ reconstituită (grupare neverificată)`. Text, nu pictogramă: **nu s-a adăugat și nu s-a scos niciun control din vreun designer** (§11). Avertizarea operatorului în clipa în care pornește a doua reconstituire e a formularului, deci 0048-04 — și propunerea îi dă deja steagul pe fiecare recepție ca să o poată face.

F28 e scrisă în fundament, în Part 2.

<a id="completare-15-căile-aparțin-operatorului-6-decizia-d-o"></a>

#### 1.5 Căile aparțin operatorului (§6, decizia D-O)

**Întâi s-au enumerat.** Fiecare loc care compunea o cale din directorul aplicației sau dintr-un literal — cerința §6.1, făcută **înainte** de orice schimbare:

| Ce | Unde era compus | Setare | Implicit |
|---|---|---|---|
| Jurnale | `LogPaths.LogsDirectory` + `MainForm.vb:161` + `Program.vb:227` + `ForexeConnectTest.vb:69` | `Logs` | `<AppDir>\Logs` |
| Dosarele de asociere | `AsociereStore.Folder` | `Asociere` | `<AppDir>\Asociere` |
| Rezultatele brute FOREXE | `WorkflowResultStore.OutputFolder` | `WorkflowResults` | `<AppDir>\WorkflowResults` |
| PDF temporar | `TempPdfStore.Folder` | `TempPdf` | `<AppDir>\TempPdf` |
| Definițiile `.wfl` | `WorkflowCatalog.ResolvePath` + `ForexeController.vb:129` + `ForexeConnectTest.vb:55` | `Workflows` | `<AppDir>\Workflows` |
| Exporturile bancului | `TreeSettingsExporter.vb:123` | `Exports` | `<AppDir>\Exports` |
| PDF-uri DDF | `KBotPaths.DefaultDdfPdfRoot` | `DdfPdf` | `C:\AVACONT\FOREXE\PDF\DDF\` |
| PDF-uri ORD | `KBotPaths.DefaultOrdPdfRoot` | `OrdPdf` | `C:\AVACONT\FOREXE\PDF\ORD\` |

Rămase **deliberat** în afară, cu motivul: `AdobeWindowHosting` (`ProgramFiles` — caută unde e INSTALAT Adobe, nu un folder în care scriem noi), `ThemeStore` (`%AppData%`, magazinul de temă, deja per utilizator), `MigratorSettings` (setările uneltei de migrare, nu ale aplicației), `KBotPaths` însuși (locul fișierului de setări), și `src/KBot.Forexe/_reference/` (instantaneu importat, nu se compilează).

**Un singur rezolvator.** `KBot.Common.KBotPaths` e singurul loc în care se rezolvă o cale. Fiecare loc de mai sus îl întreabă; nicăieri altundeva nu se mai compune una. Două căi compuse de mână în `ForexeController` și `ForexeConnectTest` au căpătat pe drum o constantă, `WorkflowCatalog.ConectareFile`, ca toate celelalte workflow-uri.

**Unde stau:** `%APPDATA%\AVACONT\KBot\settings.json`, o intrare per folder, JSON plat. *Alegerea mea, nu o decizie deja luată* — ușor de privit, ușor de salvat, per utilizator prin construcție, și ține căile în afara ramurii de registru care duce deja `CodFiscal`-ul per DC. Un cuvânt și devine registru; nimic altceva nu se schimbă.

**Regulile, toate implementate:**

- fișier lipsă, cheie lipsă sau valoare goală ▸ implicitul. **Un operator care nu configurează nimic primește exact ce primea înainte de felia asta**, fișierul poate lipsi cu totul;
- o valoare relativă se rezolvă față de directorul aplicației;
- un folder configurat care nu există **se creează la pornire** — dar numai cele în care SCRIEM. Un folder doar-citit (`Workflows`) nu se inventează: un folder gol creat de noi ar ascunde o instalare incompletă în loc să o arate;
- un folder în care nu se poate scrie ▸ **se oprește lansarea**, cu un mesaj românesc care numește **setarea și calea** și spune ce să corecteze. Niciodată o cădere tăcută pe implicit: un operator care a pus o cale și a primit-o pe cea veche a fost mințit, iar minciuna se descoperă abia când caută un fișier care nu e unde l-a trimis;
- **totul se validează la PORNIRE**, în `Program.Main`, înaintea temei și a oricărei ferestre — o cale greșită trebuie să oprească lansarea, nu o ingestie ajunsă la jumătate.

**O buclă care trebuia evitată, și cum.** `GlobalErrorLog` scrie în folderul de jurnale, iar folderul de jurnale se rezolvă tocmai de aici. Dacă `SetariFoldere.Incarca` ar loga, prima citire s-ar închide în ea însăși. De-aia **nu loghează niciodată**: strânge problemele neblocante într-o listă (`Probleme`), iar `Program.ValideazaSetarileDeFolder` le scrie în jurnal la pornire, unde oricum le e locul.

**Cele două rădăcini de PDF trăiesc în amândouă fișierele.** `settings.json` are ultimul cuvânt, `kbot_paths.json` rămâne citit ca al doilea. Un operator care le-a pus deja acolo nu se trezește tăcut cu implicitul — ceea ce ar încălca chiar regula de mai sus. `kbot_paths.json` păstrează opțiunile de COMPORTAMENT ale gazdei Adobe, care nu sunt căi.

**Nu s-a scris niciun formular** (§6.6). S-au livrat magazinul, validarea și API-ul de citit/scris; tabelul de mai sus e contractul de care se leagă formularul, iar `SetariFoldere.Toate` îl poartă în cod, cu descriere românească pe fiecare setare. Adăugarea controalelor în designer și proba dus-întors în Visual Studio sunt ale Adelinei, ca întotdeauna.

**`AsociereStore` rămâne în `KBot.Common`** (§6.5), lângă `KBotPaths`. Motivul e scris acum în corecția C5 a fundamentului, fiindcă planul care cerea altceva nu există ca fișier — vezi §0.4.

<a id="completare-16-fx_ord_tbl_rec-7--era-deja-făcut-mai-puțin-proba"></a>

#### 1.6 `FX_ORD_TBL_REC` (§7) — era deja făcut, mai puțin proba

Cele șase puncte, verificate unul câte unul, nu presupuse:

1. **Afară din lista de excluderi** — făcut în 0048-03 (`TableMaps.vb:49` nu-l mai conține).
2. **Harta** — există și e corectă: `IDORDREC` călătorește ca el însuși (cheia Access, păstrată), `IDORDRECP` exclus (oglinda serverului VECHI; pe MariaDB e `AUTO_INCREMENT`), `IDORDTBL` redenumit `IDORDTBLP`, `IDRP` exclus (mort, C2), `Valoare` și `IdPlataFX` directe.
3. **Ordinea** — cele două chei străine sunt reale și citite din `000_DEMO.sql` (`FX_ORD_TBL_REC__FX_ORD_TBL` pe `IDORDTBLP`, `FX_ORD_TBL_REC__FX_Plati` pe `IdPlataFX`, amândouă `ON DELETE CASCADE`). **Asta lipsea: proba.** Adăugat `test_fx_ord_tbl_rec_is_written_after_both_its_parents`, care verifică ordinea implicită din `routes/migrare/tables.ALL`. Latura VB (`WriteOrder.Derive`) deduce ordinea din cheile străine VII ale țintei, deci ajunge în același loc pe alt drum — **dar asta nu s-a putut rula**, fiindcă `TargetSchema` se construiește doar dintr-o conexiune vie și nu există proiect `KBot.Migrator.Tests`. Vezi §4.
4. **`FX_Plati` e în setul migrat** — da, `tables.py:78`; pinuit acum separat, fiindcă «e mai sus în listă» și «există» sunt două lucruri.
5. **`FX_Receptii_Plati` rămâne exclus** — pinuit: nu e în `ALL`, e în `OUT_OF_SCOPE`.
6. **Documentele** — `PLAN_ForexeIngest.md` D5 era deja împărțit în 0048-03; `KBOT_STATUS.md` nu mai listează tabelul ca exclus. C1 punctul 2 e marcat făcut, cu commit-ul.

---

<a id="completare-2-fișiere-atinse"></a>

### 2. Fișiere atinse

**Nou**
```
src/KBot.Domain/CelulaTabel.vb                     CelulaTabel / RandTabel / TabelRezultat / TabeleJson
src/KBot.Common/SetariFoldere.vb                   setările de folder + validarea + excepția
tests/KBot.Api.Tests/StructuraCelulelorTests.vb    D-N pe firul HTTP
tests/KBot.Common.Tests/SetariFoldereTests.vb      D-O
docs/worklog/SLICE-0048-03-completare.md
```

**Modificat — server**
```
PYTHON/routes/forexe/prelucrare.py            pasul 8, 4d în ambele faze, F28, text_celula
PYTHON/routes/forexe/prelucrare_pasi.py       step8_*, pas8_instructiuni, cere_lista, text_celula
PYTHON/routes/forexe/prelucrare_asociere.py   f28_de_marcat + marcheaza_reconstituirile_nesigure
PYTHON/routes/forexe/receptii.py              reconstituit / reconstituit_nesigur pe firul de citire
sql/0049_receptii_stergere.sql                + ReconstituitNesigur, proba actualizată
```

**Modificat — client**
```
src/KBot.Domain/PrelucrareRezultat.vb         TabelRezultat + puntea JSON + ToDebugString
src/KBot.Domain/AngajamentMapper.vb           supraîncărcare peste TabelRezultat
src/KBot.Domain/ReceptiiInfo.vb               Reconstituit / ReconstituitNesigur
src/KBot.Forexe/JobModels.vb                  Tables As Dictionary(Of String, TabelRezultat)
src/KBot.Forexe/ForexeRunner.vb               TryParseTable nu mai aplatizează; DinJToken
src/KBot.Forexe/WorkflowCatalog.vb            ConectareFile + ResolvePath prin KBotPaths
src/KBot.Forexe/Workflows/adlop - Prelucrare Completa*.wfl   nota despre TipReceptie / Detaliu
src/KBot.Api/ApiClient.vb                     TabeleJson.Catre + cele două steaguri
src/KBot.Api/UpsertAngajamenteRequest.vb      tabele As JsonObject; GetReceptieRow
src/KBot.App/Program.vb                       validarea setărilor la pornire
src/KBot.App/Forexe/ForexeController.vb       TabelRezultat + calea de conectare
src/KBot.App/Forexe/WorkflowResultStore.vb    TabelRezultat + KBotPaths
src/KBot.App/Views/ReceptiiView.vb            semnul de reconstituire în arbore
src/KBot.App/MainForm.vb                      LogPaths în loc de cale compusă
src/KBot.Common/KBotPaths.vb                  Foldere / ValideazaFoldere / cele șase proprietăți
src/KBot.Common/Logging/LogPaths.vb           prin KBotPaths
src/KBot.Common/AsociereStore.vb              prin KBotPaths
src/KBot.Common/TempPdfStore.vb               prin KBotPaths
src/KBot.DevHarness/…                         4 fișiere: tipurile noi + căile
src/KBot.{Api,App,Common,DevHarness,Domain,Forexe}.vbproj   FileVersion
```

**Modificat — documente**
```
docs/FUNDAMENT_Asociere_Receptii.md   F17 amendată, F20 retrasă, F28 nouă, C5 nouă, D-N/D-O, O3/O5 închise
docs/PLAN_ForexeIngest.md             §12 (FX_Extrase, cu excepția pasului 8), §13 punctul 3 închis
docs/worklog/KBOT_STATUS.md           rândul feliei + firele
```

**Modificat — teste** (6 fișiere Python, 2 .NET)

---

<a id="completare-3-rezultatele-testelor"></a>

### 3. Rezultatele testelor

Numărate acum, nu copiate.

**Python** — `PYTHON/.venv`, din `PYTHON/`:

```
429 trecute, 15 sărite, 0 căzute        (înainte de transă: 410 / 15)
```

`+19`. Cele 15 sărite sunt neschimbate ca număr: `test_forexe_prelucrare_live.py` se sare **în întregime** în afara gazdei, deci cele patru teste noi de acolo nu se văd în contor.

Cele care contează:

- `test_step8_sql_joins_on_referinta_then_referintadest_in_that_order` — forma SQL-ului, pinuită prin **același** ajutor pe care îl cheamă ruta (`pas8_instructiuni`); o constantă copiată în test ar rămâne verde și după ce ruta ar înceta să o mai folosească;
- `test_step8_runs_unconditionally_even_with_an_empty_payload` — niciun steag `are` ridicat, și tot rulează;
- `test_step8_result_is_not_reported_in_the_proposal` — a rulat, s-a derulat înapoi, nu se raportează;
- `test_a_flattened_detaliu_string_is_rejected_and_the_message_names_the_column` — mesajul spune *aplatizat*, nu «JSON nevalid»;
- `test_a_flattened_buget_indicator_is_rejected_by_name` + `…_nested_…_is_accepted` + `…_empty_…` + `…_missing_…` — a doua coloană imbricată, toate patru intrările;
- `test_a_nested_scalar_column_is_rejected_by_name` — cealaltă jumătate a regulii;
- `f28_de_marcat` pe cinci cazuri, inclusiv `…_never_cleared_by_a_later_run_seeing_only_one`;
- `test_fx_ord_tbl_rec_is_written_after_both_its_parents` (§7.3).

**Scrise și lăsate sărite** (au nevoie de MariaDB, tiparul de sărire din `test_forexe_ddf.py`): `test_step8_links_an_extras_to_a_payment_by_referinta`, `test_step8_does_not_overwrite_an_extras_that_already_has_a_codai`, `test_the_proposal_rolls_step8_back_like_everything_else`, `test_the_reconstituit_nesigur_column_exists`, `test_two_reconstructions_on_one_angajament_flag_both` — plus cele șase din 0048-03, între care **`test_the_proposal_leaves_every_table_byte_identical`**, care rămâne cel mai important test nerulat al feliei.

**.NET** — `dotnet build KBot.sln`: **0 erori, 0 avertismente noi.** Cele 10 avertismente `MSB3825` (BinaryFormatter în patru `.resx` de vederi) apar doar la o reconstrucție completă și sunt de pe `master` — măsurate explicit, prin `git stash`, înainte și după.

| proiect | înainte | după |
|---|---|---|
| `KBot.Api.Tests` | 87 trecute / **1 căzut** | 95 trecute / **1 căzut** (`+8`) |
| `KBot.Common.Tests` | 66 trecute / 0 | 85 trecute / 0 (`+19`) |
| `KBot.Domain.Tests` | 14 trecute / **3 căzute** | 14 trecute / **3 căzute** |
| `KBot.App.Tests` | 169 trecute / **13 căzute** | 169 trecute / **13 căzute** |
| `KBot.Controls.Tests` | 917 / 0 | 917 / 0 |
| `KBot.Theming.Tests` | 110 / 0 | 110 / 0 |
| `KBot.LocalStore.Tests` | 1 / 0 | 1 / 0 |
| `KBot.Xfa.Tests` | 39 / 0 | 39 / 0 |

**Zero teste noi căzute.** Cele **17** roșii sunt exact cele de pe `master`, neatinse (§0.6): DDF / Istoric / XFA, enumerate în worklog-ul 0048-03 §4.2. Nu s-a rulat `dotnet test KBot.sln` ca întreg — suita DevHarness deschide ferestre Adobe reale pe ecranul operatorului; fiecare proiect s-a rulat separat.

---

<a id="4-rămas-neverificat"></a>
<a id="completare-4-rămas-neverificat"></a>

### 4. Rămas neverificat

- **Nimic din transă nu a rulat pe o bază MariaDB, și nici nu s-a încercat** (§0.1, §11).
- `sql/0049_receptii_stergere.sql` **nu s-a aplicat nicăieri**. Acum are patru `ALTER`-uri, nu trei; proba `information_schema` de rulat înainte e scrisă în fișier.
- **Nimic nu s-a văzut pe ecran.** Semnul de reconstituire din arborele Recepțiilor, mesajul de oprire la o cale nevalidă, și afișarea în depanator a celulelor imbricate sunt **cod verde, nerulat sub ochii cuiva**.
- **`WriteOrder.Derive` nu s-a probat** (§7.3, latura VB). `TargetSchema` are constructor privat și se încarcă doar dintr-o conexiune vie, iar `KBot.Migrator` nu are proiect de teste. Ordinea e confirmată **static**: cele două chei străine sunt în `000_DEMO.sql`, algoritmul construiește muchiile chiar din ele, și ambii părinți sunt în setul migrat. Latura Python a aceleiași ordini e probată. Ce ar închide golul: o fabrică de `TargetSchema` vizibilă testelor + un proiect `KBot.Migrator.Tests`.
- **Formularul de setări nu există** (§6.6, deliberat). Contractul de care se leagă e §1.5 și `SetariFoldere.Toate`.
- **Formularul de asociere nu există** — 0048-04, D-A. Nimic nu cheamă coordonatorul.
- Rata de potrivire a hash-ului față de datele migrate (§8 al planului părinte) — **nerulată**, același motiv.
- `%APPDATA%\AVACONT\KBot\settings.json` **nu s-a scris niciodată pe mașina asta**: toate testele lucrează pe directoare temporare, deliberat, ca să nu contamineze mașina operatorului.

---

<a id="completare-5-pentru-cine-vine-după"></a>

### 5. Pentru cine vine după

<a id="completare-51-contractul-setărilor-de-folder-pentru-formularul-din-0048-04"></a>

#### 5.1 Contractul setărilor de folder (pentru formularul din 0048-04)

Tabelul din §1.5 e complet: cheia, ce e folderul, implicitul. În cod, `SetariFoldere.Toate` poartă aceleași lucruri, cu `Descriere` în română pentru etichetă și cu `SeScrie` care spune dacă folderul se creează și i se verifică dreptul de scriere.

Formularul citește `Bruta(cheie)` — **nu** `Cale(cheie)`: câmpul trebuie să apară **gol** când operatorul n-a configurat nimic, nu preumplut cu implicitul rezolvat, altfel prima salvare îngheață implicitul de azi ca alegere deliberată pe veci. Aceeași capcană ca `ShouldSerialize` la controale.

Scrierea e `SetariFoldere.Salveaza(valori, [dir])`; cheile cu valoare goală se omit. După salvare, setările se aplică **la repornire** — `KBotPaths.Foldere` e încărcat o singură dată, iar folderul de jurnale nu se poate muta sub un fișier deja deschis.

<a id="completare-52-dacă-vreo-coloană-devine-imbricată-în-forexe"></a>

#### 5.2 Dacă vreo coloană devine imbricată în FOREXE

Nu se ghicește și nu se caută prin cod: se citesc `.wfl`-urile după tiparul din §1.3 (`ForEachVar` + `collectFields` + `ScrapeTable saveTo` interior). La rulare, `TabelRezultat.ColoaneImbricate()` răspunde din date, iar istoricul lucrărilor o scrie după fiecare descărcare.

O coloană nouă imbricată **se oprește singură**: dacă e citită ca text, `text_celula` ridică și o numește. Asta e chiar de dorit — e o schimbare în FOREXE, și trebuie aflată, nu ocolită.

<a id="completare-53-ce-nu-mai-trebuie-făcut"></a>

#### 5.3 Ce NU mai trebuie făcut

- **Nu mai rulați interogarea F20** din worklog-ul 0048-03 §5.3. F20 e retrasă; O3 e închisă prin retragere. `DIFH` nu poate răspunde la întrebarea aceea, fiindcă se calculează după asociere.
- **Nu mai căutați `PLAN_ForexeIngestSteps3to8.md`.** Nu există, nu a fost comis niciodată, și tot ce a decis și mai contează e în fundament sau în comentariile codului — vezi corecția C5.

<a id="completare-notă-de-reluare-absorbită-din-registru-d001-04"></a>

#### Notă de reluare absorbită din registru (D001-04)

La data acestei tranșe, lipsa celor patru coloane din SQL/0049 afecta și GET /api/forexe/receptii, care selectează R.Reconstituit și R.ReconstituitNesigur, nu numai ingestia. Necesitatea aplicării manuale a scriptului nu înseamnă că a fost aplicat sau că există autorizare de execuție în reorganizarea documentară.
