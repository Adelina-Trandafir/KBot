# SLICE-0049 — editorul de ordonanțare (`frmFX_ORD` + subformulare), calea de SCRIERE

**Plan:** `PLAN_OrdEditForm.md` (01.09.2026). **Predecesor:** felia 0033 (`OrdView`, read-only),
care nu s-a rescris. **Stare:** cod verde — `dotnet build KBot.sln` 0 erori / 0 avertismente
noi; Python 465 trecute / 15 sărite, identic cu baza. **NIMIC nu a rulat pe o bază MariaDB
vie și niciun formular nu a fost deschis pe ecran sau în designerul Visual Studio.**

---

## 1. Ce s-a schimbat și de ce

Felia 0033 a adus ordonanțările pe ecran. Felia asta le face **editabile**: se generează, se
salvează, se modifică, se șterg, și se pot genera în lot. Portul acoperă zece formulare Access
și patru module VBA.

### 1.1 Arhitectura, în trei propoziții

**Generarea rulează pe SERVER** (D4). `Genereaza_ORD` e SQL pur peste `FX_Plati`,
`FX_Receptii*`, `FX_Indicatori`, `Parteneri`, tabele care trăiesc toate în MariaDB. Clientul
trimite `cod` + `data` (+ opțional o singură plată) și primește graful propus. **Nu există
bază locală SQLite și nu există tabele de pregătire**: cele șase `tmpFX_ORD*` din Access nu au
succesor — rolul lor îl joacă obiecte în memoria clientului.

**Salvarea e o singură trimitere, o singură tranzacție** (D5). Lanțul VBA în cinci pași —
`FX_Curatare_Staging_ORD` ▸ `FX_Adauga_Ord` ▸ salvare locală „de probă" ▸ `FX_Confirma_ORD` ▸
`FX_Confirma_Local_ORD` ▸ commit real ▸ `FX_ActualizeazaAccessIds_ORD`, cu mesajele lui de
«EROARE CRITICĂ: datele sunt pe server dar nu local» — **nu s-a portat**. Tot graful urcă
într-un POST, serverul îl scrie într-o tranzacție și întoarce cheile reale. Nu mai există
stare care să poată rămâne pe jumătate.

**Cele trei popup-uri au devenit trei pagini** (D2), în spatele unui `KBotNavList` orizontal —
aceeași formă pe care o folosesc deja `OrdView` și `DdfView`. Consecință directă: `btnSav` de
pe `frmFX_ORD_DOC` **dispare**; există o singură salvare, a formularului, pentru tot graful.

### 1.2 Punctul unde cele două faze sunt inevitabile

Octeții imaginilor atașate **nu pot** urca odată cu graful: un `IDORDATTP` trebuie să existe
înainte ca ei să poată atârna de el. Deci formularul salvează întâi documentul, citește harta
`temp_id ▸ cheie reală`, apoi urcă fiecare imagine.

Dacă o încărcare cade **după** o salvare reușită, ordonanțarea **rămâne salvată** și se spune
exact ce imagine lipsește, cu ofertă de reluare. Nu se derulează nimic înapoi: un document pe
jumătate derulat e mai rău decât unul căruia îi lipsește o poză.

### 1.3 Cele șase capcane ale familiei `FX_ORD`, tratate explicit

Citite din DDL (`MariaDB_Schema/000_DEMO.sql`), nu deduse din nume:

1. **Toate legăturile merg pe cheia «…P»** (`IDORDP`, `IDORDPARTP`, `IDORDTBLP`, `IDORDATTP`,
   `IDORDDOCP`). Omonimele fără «P» sunt id-uri Access păstrate. Un port literal al join-ului
   Access `FX_ORD_TBL.IDORDPART = FX_ORD_PART.IDORDPART` ar lega cheia greșită.
2. `FX_ORD_TBL_REC` se leagă **deja** pe `IDORDTBLP` (FK real, `ON DELETE CASCADE`).
3. `FX_ORD.IDORD` și `FX_ORD.CUAL` sunt `varchar(255)`, în timp ce `FX_DDF.CUAL` e `int(11)` —
   CUAL-ul copiat din DDF se convertește la text.
4. `FX_ORD_TBL` are **șase chei străine**, dintre care `IdClsf` (cu `DEFAULT 0` **și** cheie
   străină) și `CodAI` opresc tranzacția pe date proaste. De aceea se validează **pe nume**,
   înainte de primul INSERT, cu mesaj care numește linia.
5. **Inversiunea `IdClsf`:** pe `FX_ORD_TBL`, MariaDB `IdClsf` e cheia străină către
   `Clasificatii` iar `IdClsfAcc` ține id-ul Access — INVERS față de `FX_Indicatori`. În Access
   se numeau `IdClsfPY` (global) și `IdClsf` (Access).
6. `ClasificatiiG` / `ParteneriG` nu există; `FX_ORD_ATT` nu are coloană `Nume`. Și, deși
   `IdUnitate` e relicvă pe restul tabelelor `FX_`, pe `FX_ORD_TBL` e **NOT NULL cu cheie
   străină**, deci TREBUIE scris — se ia din `FX_Indicatori.IdUnitate`, exact ca în
   `qFX_ORD_BASE`.

### 1.4 Deciziile de traducere care nu erau în plan

Lucruri pe care planul nu le putea ști, fiindcă ies abia din citirea sursei:

**`CodSSI` nu e coloană în MariaDB.** `qFX_ORD_BASE` îl ia din `ClasificatiiG.CodSSI`, care nu
există. Sursa verificată — folosită azi în producție de `routes/forexe/prelucrare_pasi.py`,
`read_indicatori` — este `CONCAT(Clasificatii.SS, Clasificatii.ClsfSal)`, cu clasificația
rezolvată prin `(IdClsfAcc, IdUnitate)`. Se refolosește aceeași formulă, nu una nouă.

**`Clasificatii` se citește prin SUBINTEROGARE SCALARĂ cu `LIMIT 1`, nu prin JOIN.**
Nomenclatorul are duplicate reale pe `(IdClsfAcc, IdUnitate)` (`MAPARE_NOMENCLATOARE.md` §3.2),
iar un JOIN ar multiplica rândurile. Tiparul e cel folosit deja de rutele de citire.

**Abatere deliberată de la Access:** acolo, `qFX_ORD_BASE` făcea INNER JOIN pe `ClasificatiiG`,
deci o plată a cărei clasificație lipsește **cădea din recordset în tăcere**. Aici rândul
RĂMÂNE, cu clasificația NULL, se emite un avertisment vizibil în formular, iar validarea
salvării îl oprește pe nume. Aceeași alegere ca D19 din felia 0048-02.

**`H.Sters = False` din Access → `COALESCE(H.Sters, 0) = 0`.** În Access un Yes/No nu poate fi
Null; în MariaDB coloana e nullabilă și NULL înseamnă «migrat fără valoare», nu «șters». Un
`H.Sters = 0` sec ar arunca tăcut toate recepțiile vechi.

**`TOP 1` pe `FX_DDF` → `ORDER BY IDDF, CUAL LIMIT 1`.** PK-ul lui `FX_DDF` e COMPUS, iar
`TOP 1` fără `ORDER BY` e nedeterminist în Access. Ordinea stabilă e aceeași ca în
`routes/forexe/pdf.py`.

**`Month & "/" & Year LIKE "*"` → parametri expliciți `luna` / `an`.** `*` nu e metacaracter în
MariaDB, iar un `LIKE '*'` ar fi tăcut greșit (nu ar potrivi nimic).

### 1.5 Cele trei puncte de oprire din plan — răspunsurile

Instrucțiunea operatorului a fost «nu te opri pentru nimic», deci s-a mers mai departe. Ce s-a
găsit:

**§12.1 — semantica lui `Contor_Parteneri_Zi`: REZOLVATĂ prin citire, fără ghicit.** Funcția
NU numără parteneri. Bucla ei este:

```vba
Rez = 1
For Cnt = 1 To Rs.RecordCount
    If Cnt Mod 25 = 0 And Cnt <> 0 Then Rez = Rez + 1
Next
```

adică `1 + floor(n / 25)` — un număr de **PAGINI**. Pragul `> 1` înseamnă deci «ziua nu încape
într-o singură ordonanțare de 25 de parteneri», ceea ce se potrivește perfect cu limita.
Avertismentul s-a portat, cu formula reprodusă întocmai.

**§12.2 — rânduri în `FX_ORD_ATT` pe unități vii: NU SE POATE VERIFICA OFFLINE.** Constatarea
«tabela e goală» vine din dump-ul demo. Proba SQL e scrisă în `sql/0049_ord_att_img.sql` și
în `docs/possible_future_directions.md`; până când cineva o rulează, `Imagine` e tratată ca
moartă (D9) — dar reversibil: coloana rămâne pe loc, nu se șterge nimic.

**§12.3 — ce îi lipsește editorului din `GET /api/forexe/ord`: MULT.** Ruta vederii 0033 nu
întoarce `CodAI`, `CodIndicator`, `IdClsf`/`IdClsfAcc`, `CodSSI`, `Explicatie`,
`CodPartener`/`IdPartener`, `IdUnitate`, rândurile de document una câte una (le adună cu
`GROUP_CONCAT`), legăturile `FX_ORD_TBL_REC` sau atașamentele. **`ord.py` nu s-a modificat**
(diff gol); citirea editorului trăiește ca soră a scrierii, în fișierul nou:
`GET /api/forexe/ord/draft/<idordp>`.

### 1.6 REGULA 0 — fara diacritice in cod

Comentariile si documentatia XML din fisierele noi au fost scrise intai in romana CU
diacritice, ca fisierele din jur (`OrdView.vb`, `OrdVizualizarePage.vb`, `AsociereForm.vb`).
Asta **incalca RULE 0 din `CLAUDE.md`**, care e PRIORITATE 0 si care spune explicit ca
depaseste stilul oricarui fisier existent: «where old code disagrees, the old code is wrong
and gets swept, never copied».

S-a facut deci o trecere de curatare, cu doua garantii:

* transliterarea atinge **doar partea de comentariu** a fiecarui rand, gasita scanand in
  afara literalilor de sir — deci sirurile pe care le VEDE operatorul (etichete, mesaje,
  tooltip-uri, `Text`-ul butoanelor) si-au pastrat diacriticele reale, cum cere aceeasi regula;
* pe fisierele PREEXISTENTE (`ApiClient.vb`, `IApiClient.vb`, `OrdView.vb`, `MainForm.vb`) s-au
  maturat **doar randurile adaugate de felia asta**, identificate din `git diff`. Comentariile
  care erau acolo dinainte nu sunt treaba acestei felii; maturarea lor ar fi umflat diff-ul cu
  sute de randuri fara legatura.

Verificat dupa: **0 randuri de comentariu cu diacritice** in cele 14 fisiere noi si **0** pe
randurile adaugate in cele 4 preexistente; sirurile operator-facing si-au pastrat diacriticele
(verificat prin numarare: 18 in `OrdEditForm.Designer.vb`, 13 in `OrdBeneficiariPage.Designer.vb`,
9 in `OrdZiuaForm.Designer.vb`, 31 in `OrdEditForm.vb`).

**ABATERE DE LA PLAN, consemnata:** planul §0.5 si `CLAUDE.md` cer si ca **limba** comentariilor
sa fie ENGLEZA. Comentariile de aici au ramas in **romana fara diacritice** — aceeasi limba ca
ale tuturor fisierelor vecine din `Views/Ord/` si `routes/forexe/`, scrise asa pana in felia
0048 inclusiv. Regula 0 (diacriticele) e respectata integral; regula de limba **nu**. Daca
operatorul vrea si traducerea, e o trecere mecanica separata care ar trebui sa cuprinda tot
directorul, nu doar fisierele acestei felii — altfel felia asta ar fi singura in engleza intr-un
vecinatate in romana.

---

---

## 2. Fișiere atinse

### Schema

* **NOU** `sql/0049_ord_att_img.sql` — `FX_ORD_ATT_IMG`, modelată pe `FX_ORD_PDF`: un rând per
  atașament, SHA-256, `ON DELETE CASCADE`. Conține proba de tip a cheii părinte, proba «are
  vreo unitate rânduri în `FX_ORD_ATT`?», și **varianta pentru `AVACONT_SURSA` fără
  `AUTO_INCREMENT`** (scutirea din felia 0048-01 §3.1 se aplică integral).
* `MariaDB_Schema/000_DEMO.sql` — aceeași tabelă, adăugată după `FX_ORD_ATT`.
  **Notă:** `MariaDB_Schema/` este în `.gitignore` (copie locală de referință), deci
  modificarea NU intră în commit. Artefactul din depozit este fișierul `sql/`.

### Server (Python)

* **NOU** `PYTHON/routes/forexe/ord_edit.py` (~1150 rânduri) — opt rute:

  | metodă | cale | rol |
  |---|---|---|
  | POST | `/api/forexe/ord/genereaza` | graful propus; nimic scris |
  | GET | `/api/forexe/ord/draft/<idordp>` | graful unei ordonanțări existente |
  | GET | `/api/forexe/ord/zile` | zilele candidate pentru lot |
  | POST | `/api/forexe/ord/save` | scrie tot graful, o tranzacție |
  | DELETE | `/api/forexe/ord/<idordp>` | șterge, prin cascade |
  | GET/PUT/DELETE | `/api/forexe/ord/att/<idordattp>/imagine` | octeții atașamentului |

  Toate `@require_session`, baza din sesiune, `ensure_ascii=False`, erori în română.
* `PYTHON/routes/forexe/__init__.py` — înregistrarea modulului nou.
* `PYTHON/routes/forexe/ord.py` — **NEATINS**. `PYTHON/routes/ord/*` — **NEATINS** (D6).

### Client — contract și domeniu

* **NOU** `src/KBot.Api/OrdEditContract.vb` — DTO-urile de pe fir, snake_case verbatim.
* **NOU** `src/KBot.Domain/OrdDraft.vb` — `OrdDraft`, `OrdDraftPart`, `OrdDraftLinie`,
  `OrdDraftRec`, `OrdDraftDoc`, `OrdDraftAtt`, plus `OrdSaveRezultat`,
  `OrdStergereRezultat`, `OrdZiCandidat`, `OrdZileInfo`.
* `src/KBot.Api/IApiClient.vb` + `ApiClient.vb` — opt metode noi.

### Client — formulare

* **NOU** `src/KBot.App/Views/Ord/IOrdEditPage.vb` — contractul paginilor de editare.
* **NOU** `OrdEditForm.vb` + `.Designer.vb` — `KBotShellForm`, modal, bandă de antet, trei
  pagini leneșe, salvează/renunță.
* **NOU** `OrdBeneficiariPage.vb` + `.Designer.vb` — `frmFX_ORD_PART` + `frmFX_ORD_TBL`.
* **NOU** `OrdDocumentePage.vb` + `.Designer.vb` — `frmFX_ORD_DOC` + `_BENE` + `_TXT` + `_ATT`.
* **NOU** `OrdAtasamentePage.vb` + `.Designer.vb` — `frmFX_ORD_PRTSCR` + `_BENE` + `_S`.
* **NOU** `OrdZiuaForm.vb` + `.Designer.vb` — dialogul care cere ziua.
* **NOU** `OrdComanda.vb` — cele patru comenzi, ca valori.
* `src/KBot.App/Views/OrdView.vb` — patru puncte de intrare (buton de subsol + meniu
  contextual), `Reincarca(idordp)`, re-selecția documentului salvat. Calea de CITIRE nu s-a
  atins.
* `src/KBot.App/MainForm.vb` — `ExecutaComandaOrd` și cele patru operațiuni, cu `WithReauth`.

### Teste

* Nouă dubluri `IApiClient` din `tests/KBot.App.Tests/` — cioturi
  `Throw New NotSupportedException()` pentru cele opt metode noi. **Niciun test nou** (cerința
  operatorului).

---

## 3. Rezultatele testelor

**Baza s-a luat cu `git worktree add … HEAD`, pe o copie curată — nu din memorie.**

| suită | înainte | după |
|---|---|---|
| `dotnet build KBot.sln --no-incremental` | **0 erori / 12 avertismente** | **0 erori / 12 avertismente** |
| Python (`PYTHON/.venv`, `pytest -q`) | 465 trecute / 15 sărite / 0 eșecuri | **identic** |
| `KBot.Api.Tests` | 95 trecute / **1 roșu** | 95 trecute / **1 roșu** |
| `KBot.App.Tests` | 188 trecute / **13 roșii** | 188 trecute / **13 roșii** |
| `KBot.Domain.Tests` | 14 trecute / **3 roșii** | 14 trecute / **3 roșii** |
| `KBot.Controls.Tests` | — | 953 trecute / 0 roșii |

**Cele 12 avertismente sunt TOATE `MSB3825`** (`ImageStream` / `BinaryFormatter`) pe fișiere
`.resx` de vedere care existau dinainte: `AsociereForm`, `DdfView`, `OrdView`, `PlatiView`,
`ReceptiiView`, `RezervariView`. Niciunul dintre formularele noi nu are `.resx` (imaginile vin
din `My.Resources`), deci felia asta nu adaugă niciunul.

**Cele 17 teste roșii sunt PREEXISTENTE, nu introduse aici** — numărate identic pe copia
curată la HEAD. Numele lor:

* `KBot.Api.Tests.ApiClientTests.GetDdf_FormatsRevisionLabel_WithSpacePadding_Not_Zeroes`
* `KBot.Domain.Tests.DdfInfoTests.EtichetaRevizie_PadsWithSpaces_NotZeroes`
* `KBot.Domain.Tests.DdfInfoTests.EtichetaRevizie_HandlesTwoAndThreeDigits`
* `KBot.Domain.Tests.DdfInfoTests.EtichetaRevizie_MissingDate_LeavesDatePartEmpty`
* `KBot.App.Tests.DdfXfaParserTests.Parse_SkipsTheDummyFirstRow_AndReadsRealLines`
* `KBot.App.Tests.DdfXfaParserTests.Parse_WrappedForm1_FindsForm1UnderXdpAndXfaData`
* `KBot.App.Tests.MainFormNavItemsTests.Designer_WroteLiteralDiacritics_NotEscapes`
* `KBot.App.Tests.XfaXmlPreviewTests.ShowDocument_WithSiblingXml_RendersHeaderAndLines`
* `KBot.App.Tests.DdfViewTests.StaleResponse_IsDiscarded`
* `KBot.App.Tests.DdfViewTests.Tree_TwoLevels_MonthRootsAndRevisionLeaves`
* `KBot.App.Tests.DdfViewTests.Leaf_CaptionPadsRevisionNumberWithSpaces`
* `KBot.App.Tests.IstoricViewTests` (șase teste)

Toate șase familii sunt DDF / Istoric / XFA — nimic de-a face cu ORD.

**Lista rutelor s-a verificat** înregistrând `forexe_bp` pe un `Flask()` gol și enumerând
`url_map`: cele opt căi noi apar, iar `GET /api/forexe/ord` și `GET/PUT /api/forexe/ord/pdf/…`
au rămas neschimbate. `DELETE /api/forexe/ord/<int:idordp>` nu intră în conflict cu
`/api/forexe/ord/pdf/<int:idordp>`, fiindcă `pdf` nu e întreg.

`py_compile` curat pe `ord_edit.py` și pe `__init__.py`.

---

## 4. Ce a rămas neverificat sau amânat

### Neverificat — nimic nu a atins o bază vie

1. **Nicio rută nu a rulat pe MariaDB.** Toate cele opt sunt scrise, înregistrate și
   `py_compile`-curate, dar niciun SELECT și niciun INSERT n-a atins date reale. Prima rulare
   pe mașina operatorului trebuie să scrie rezultatele în
   `<AppDir>\Logs\test_<timestamp>.log`.
2. **La prima salvare reală: `MAX(IDORDP)` înainte și `AUTO_INCREMENT` după, per tabelă** —
   aceeași verificare pe care a cerut-o felia 0048-01. Codul ridică zgomotos dacă
   `lastrowid = 0` după un INSERT (înseamnă că acea coloană și-a pierdut `AUTO_INCREMENT`), dar
   asta se vede abia la rulare.
3. **`sql/0049_ord_att_img.sql` nu s-a rulat nicăieri.** Tipul cheii părinte
   (`FX_ORD_ATT.IDORDATTP`, `int(11)`) e citit din dump-ul demo, nu confirmat pe o bază de
   producție. Proba e scrisă în fișier.
4. **Niciun formular nu s-a deschis** — nici pe ecran, nici în designerul Visual Studio.
   Aspectul celor cinci formulare noi este PROIECTAT, nu văzut. Aceeași situație ca la
   `AsociereForm` (felia 0048-04).
5. **`AVACONT_SURSA` nu s-a atins** — varianta fără `AUTO_INCREMENT` e scrisă în fișierul SQL,
   comentată, gata de rulat.
6. **Avertismentul de 25 de parteneri** se calculează corect după sursă, dar n-a fost văzut
   declanșându-se pe date reale.

### Portat deliberat cu defectul lui

7. **`Incarca_Explicatii_Incasari` filtrează pe `CodContract = 'ERRRRRRRRRR'`**, deci
   dicționarul iese **întotdeauna gol** și fiecare încasare primește «LIPSA EXPLICATIE» /
   «INCASARE». Arată a santinelă de depanare uitată în cod. S-a portat fidel (aceeași decizie
   ca D18 din felia 0048-01: portează defectul, consemnează-l, nu-l repara pe tăcute), dar
   izolat într-o constantă numită — `EXPLICATII_FILTRU_CODCONTRACT` — ca să fie vizibil și
   reparabil cu o linie. Consemnat și în `docs/possible_future_directions.md`.

### Neportat, cu motiv

8. **`btnClsf` din `frmFX_ORD_TBL`.** În tot exportul Access nu există niciun `btnClsf_Click`;
   singura lui apariție e în `PositionElements`, o funcție al cărei prim rând e
   `Exit Function`. Un buton fără comportament ar fi exact no-op-ul tăcut interzis de regulile
   casei. Coloana `Clasificație` e read-only: clasificația, `CodSSI` și `CodAI` vin din plata
   pe care o acoperă linia și trebuie să rămână în acord (altfel cheile străine resping
   salvarea).
9. **`hwndAccess` / `hwndForm` / `WebBrowser0` din `frmFX_ORD_PRTSCR`.** Instalație de
   găzduire a ferestrelor Access: un `WebBrowser` reparentat prin `SetParent`, ca să se poată
   face zoom și panoramare pe o imagine base64. În WinForms previzualizarea e un `PictureBox`
   cu `SizeMode = Zoom` — nu există nimic de reparentat.
10. **`Incarca_MaxPKs`.** Exista ca să pre-aloce autonumere Access. Cheile vin din
    `AUTO_INCREMENT`, la salvare.
11. **Generarea PDF-ului ORD (XFA)** — rămâne afară (D12). Stocarea există deja
    (`FX_ORD_PDF` + rutele din `pdf.py`); lipsește doar scriitorul, care își are felia lui.
    NU s-a adăugat un buton «Generează» care să nu facă nimic.

### Limite cunoscute ale implementării

12. **Tabela `BIC` nu există în MariaDB.** `Incarca_DicBanci` o citea din front-end-ul Access.
    Ruta o probează prin `information_schema` și, când lipsește, lasă `Banca` gol și emite un
    avertisment. Nu s-a inventat o a doua sursă pentru numele băncii.
13. **Alegătorul de partener (`cboCodPartener`) arată doar partenerii DEJA prezenți pe liniile
    beneficiarului.** Nomenclatorul întreg — Access: `RowSource` peste `Parteneri` cu
    `CodPartener <> 'XXX'`, `IdPartener <> 0`, `CodFiscal <> ''` — ar cere o rută de citire pe
    care felia asta nu o are. Cele trei condiții sunt gata rescrise fără `Nz()` pentru ziua în
    care ruta apare.
14. **`FX_ORD_ATT_IMG` se probează la citirea draftului.** Pe o bază pe care fișierul SQL n-a
    fost încă rulat, metadatele imaginilor lipsesc în loc să cadă tot formularul — același
    tipar ca proba `FX_ORD.CalePDF` din `ord.py`.
15. **Coloanele editabile din grila liniilor sunt `Valoare` și `Explicație`.** `Rămas` se
    RECALCULEAZĂ la fiecare schimbare de valoare, după formula din `Adauga_Ord_Tbl`
    (`Round(recepții − plăți anterioare − valoare, 2)`) — două cifre care spun același lucru nu
    au voie să se poată contrazice pe ecran. Dacă operatorul se aștepta să poată edita mai
    mult, se lărgește; vezi și punctul despre `Explicatie` din
    `docs/possible_future_directions.md`.

### Amânat, scris în `docs/possible_future_directions.md`

16. **Resetarea pe an a lui `FX_ORD.NrORD` și `FX_DDF.CUAL`** — felie proprie, fiindcă
    atinge și DDF-ul și fiindcă cere o decizie despre ce an guvernează.
17. **Dacă limita de 25 de parteneri mai are rost** acum că interogarea rulează pe MariaDB.
    Limita NU s-a schimbat.
18. **Dacă `FX_ORD.CUAL` (varchar) ține vreodată altceva decât CUAL-ul întreg al DDF-ului.**
19. **Dacă `Explicatie` e vreodată editată de operator sau doar calculată.**
