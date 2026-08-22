# SLICE-0044-04 — coloane obligatorii apărate · dosarul de instrucțiuni SQL

**Data:** 2026-08-21
**Domeniu:** doar server (`PYTHON/routes/migrare/`) + o cheie nouă în `config.py` pe VPS.
`src/KBot.Migrator/` **neatins** — nimic din felia asta nu ajunge pe ecran.

---

## 1. Ce s-a schimbat și de ce

### 1.1 Defectul: o coloană obligatorie putea cădea din `INSERT`

`execute.run()` construia lista de coloane a instrucțiunii prin două filtre:

```python
target_name = rename.get(c.lower())                             # (a) corelația
if target_name is None: continue
if chosen_cols is not None and target_name not in chosen_cols:  # (b) bifa
    continue
```

`chosen_columns_of()` apăra cheia primară de (b) (`chosen.update(pk_columns)`).
**Nimic nu o apăra de (a).** `MigratorForm.CorelatiiAlese` trimite harta de
corelații ÎNTREAGĂ pentru fiecare tabel bifat, inclusiv `""` pentru coloanele
fără țintă; `column_rename_map` citea `""` ca «nu călătorește» și făcea
`rename.pop(key)`. O cheie primară — sau orice coloană `NOT NULL` fără valoare
implicită — putea deci să dispară din instrucțiune.

MariaDB răspunde atunci `1364 (HY000): Field '<col>' doesn't have a default
value` — eroare despre **lista de coloane**, nu despre valori (un NULL într-o
coloană `NOT NULL` ar fi 1048, altă eroare). Ăsta e eșecul observat pe
`FX_Angajamente` / `CodAngajament` pe 2026-08-21.

### 1.2 `TargetSchema` — ce s-a găsit acolo (cerut explicit de plan)

**Interogarea selecta deja `IS_NULLABLE`, `COLUMN_DEFAULT` și `EXTRA`.** Pasul
§1.1 al planului a fost aproape un no-op. Forma exactă a `meta`-ului pe coloană,
înainte de felia asta:

| cheie | din ce | ce ține |
|---|---|---|
| `nume` | `COLUMN_NAME` | numele exact al țintei |
| `tip` | `DATA_TYPE` | litere mici (`varchar`, `int`…) |
| `tip_complet` | `COLUMN_TYPE` | cu `unsigned`, lungime, `enum(...)` |
| `lungime` | `CHARACTER_MAXIMUM_LENGTH` | `int` sau `None` |
| `precizie` / `scara` | `NUMERIC_PRECISION` / `NUMERIC_SCALE` | `int` sau `None` |
| `accepta_nul` | `IS_NULLABLE == 'YES'` | bool |
| `are_implicit` | `COLUMN_DEFAULT is not None` | bool |
| `auto` | `'auto_increment' in EXTRA` | bool |
| `cheie` | `COLUMN_KEY` | `PRI` / `MUL` / … |

**Singura adăugare:** cheia `extra` — `EXTRA` brut, litere mici. `auto` răspundea
la o singură întrebare din `EXTRA`; `is_required` are nevoie și de celelalte
(coloane generate, expresii `on update`). Nicio cheie existentă n-a fost
redenumită sau schimbată — `check_value()` și `_row_is_blocked()` le citesc.

### 1.3 `is_required` + apărarea în trei locuri

`validate.is_required(meta)` — coloana pe care MariaDB refuză s-o lase afară din
`INSERT`: `NOT NULL`, fără valoare implicită, necompletată de server.
`auto_increment` → **False** (serverul dă valoarea), coloană generată → False,
`on update <expr>` → False. Orice altceva `NOT NULL` fără `COLUMN_DEFAULT` →
True. `DEFAULT_GENERATED` iese pe ramura `are_implicit`, care se testează
înainte, deci e acoperit indiferent dacă MariaDB pune ceva în `EXTRA` sau nu.

Trei paze, ca la regula celorlalte constatări:

1. **`column_rename_map(..., protected)`** — parametru nou, al patrulea,
   `None` implicit (apelanții vechi merg neschimbați). O țintă vidă pusă pe o
   coloană din `protected` **se refuză**, nu se ascultă: `ValidationError` →
   **400** la `POST /api/migrare/analiza`, cu tabelul, coloana Access și coloana
   țintă numite. `protected` = cheile primare ale țintei + toate coloanele
   `is_required` (`validate.required_columns_of`), luate din
   `schema.columns` / `schema.primary_key` — **niciodată** din `tables.py`, care
   știe doar cheia din Access și poate să nici nu fie o coloană de pe MariaDB.
2. **constatare nouă `COLOANA_OBLIGATORIE`, clasă `BLOCANT`** — o coloană
   obligatorie absentă din listă din orice alt motiv (debifată, necorelată). Una
   pe coloană, cu `column=` numele țintei. Blocant înseamnă că niciun buton nu
   pornește și coloana e numită în grila de constatări, adică exact ce lipsea.
3. **`ExecuteError` în `execute.run()`**, înainte de primul rând.

> **Abatere de la plan, deliberată:** planul scria
> `F_COLOANA_OBLIGATORIE = "COLOANĂ_OBLIGATORIE"`, cu diacritică. Jetonul ăsta
> pleacă pe sârmă și e citit de migrator — REGULA 0 din `CLAUDE.md` îl vrea
> ASCII, iar felia 0044-03 a trecut toate celelalte jetoane la ASCII exact din
> motivul ăsta. S-a scris **`"COLOANA_OBLIGATORIE"`**.

### 1.4 O singură funcție construiește lista de coloane

`validate.insert_columns(table_name, access_columns, rename, chosen_cols)`
întoarce `(columns, skipped)`, unde `skipped` e `[(nume_access, motiv)]` cu
motivul `necorelata` sau `debifata` (jetoane ASCII; textul românesc îl face
`describe_skipped()` la cele două locuri unde îl citește operatorul). Verificarea
dublurii de țintă a intrat tot acolo.

Analiza și scrierea o folosesc **amândouă** — două copii ale regulii care se
depărtează una de alta e chiar felul în care s-a născut defectul. Analiza n-avea
lista de coloane deloc; a căpătat-o (`validate.accdb_columns` → `mdb-schema`, un
apel scurt pe tabel, ca la inventar).

`validate.missing_required(target_columns, columns)` și
`validate.required_columns_message(table_name, missing)` — condiția și
propoziția, scrise o singură dată și spuse la fel de analiză și de scriere.

### 1.5 Jurnalul spune lista de coloane cu voce tare

`say("Se scrie «%s».")` a fost înlocuit cu:

```
«FX_Angajamente»: 23 coloane — CodAngajament, Denumire, DataCreare, …
«FX_Angajamente»: coloane Access sărite — IdUnitate (necorelată), DC (debifată).
```

A doua linie apare doar când s-a sărit ceva, și spune **care filtru** a scos
fiecare coloană — distincția care era imposibil de văzut. Numele se plafonează
la 30, apoi `… și încă N` (`execute._name_list`). Aceeași linie de coloane sărite
se spune și la analiză.

### 1.6 Dosarul de instrucțiuni SQL

Modul nou `PYTHON/routes/migrare/dump.py`, clasa `SqlDump`. Fiecare **scriere**
lasă pe disc, în text simplu, instrucțiunile trimise:

```
<MIGRARE_SQL_DIR>/000_DEMO/20260821_142942_scriere/
    _00_info.txt  _01_stergeri.sql  FX_Angajamente.sql  …  _99_final.txt
```

Reguli, toate respectate în cod:

* **Rândul care pică e în fișier**: `dump.row()` se cheamă ÎNAINTE de
  `cur.execute`, niciodată după.
* **`flush` pe lot**, nu la final (`dump.flush()` din `_write`): un proces omorât
  lasă pe disc tot până la ultimul lot încheiat.
* **Un singur fișier deschis** odată; se închide în `close_table()`.
* **Consemnarea nu rupe niciodată migrarea.** Fiecare metodă publică trece prin
  `_guard`: o scriere pe disc care eșuează se scrie în jurnalul serverului cu
  `logger.exception` (urmă completă — consemnat, nu înghițit), se spune o dată în
  jurnalul lucrării, pune `disabled = True` și migrarea merge mai departe. Nimic
  nu iese din `dump.py` prin excepție **în afară de constructor**, care e
  deliberat nepăzit: `MIGRARE_SQL_DIR` lipsă trebuie să oprească scrierea.
* Fără curățenie automată. Migrarea se face o dată pe bază de unitate;
  `FX_Plati` ~2744 rânduri + `FX_Istoric` ~3246 dau câțiva MB pe fișier, 20–40 MB
  pe rulare pentru un an întreg.
* **Analiza nu scrie nimic** în dosar (`analyze()` n-a căpătat parametru `dump`).

`storage.sql_dir()` — cheia `MIGRARE_SQL_DIR`, exact tiparul lui `pushed_dir()`:
fără valoare implicită ascunsă, lipsa cheii oprește cu numele ei în mesaj.
(Accesul la `config` stă tot în `storage.py`, unde stau și celelalte două chei;
`dump.py` îl cheamă de acolo.)

### 1.7 Reconstrucția valorilor

`execute._literal(value)` → `(text, ok)`. Escape-ul e al **driverului însuși**:

```python
from mysql.connector.conversion import MySQLConverter
_CONVERTER = MySQLConverter()
out = _CONVERTER.quote(_CONVERTER.escape(_CONVERTER.to_mysql(value)))
```

**Lanțul verificat pe versiunea instalată** (`mysql-connector-python 9.7.0`, în
`PYTHON/.venv`), nu presupus. Ce dă, măsurat:

| valoare | rezultat |
|---|---|
| `None` | `NULL` |
| `"a'b"` | `'a\'b'` |
| `"ăî"` | `'ăî'` (UTF-8 literal) |
| `5` / `True` / `False` | `5` / `1` / `0` |
| `Decimal("1.25")` | `'1.25'` |
| `datetime(2026,8,21,14,29,42)` | `'2026-08-21 14:29:42'` |
| `b"\x00\xff"` | `0x00ff` |
| `b""` | `''` |
| un tip pe care nu-l știe | `TypeError` |

`MySQLConverter` e Python curat și nu cere conexiune, deci merge oricare ar fi
tipul de conexiune (serverul rulează extensia C, a cărei conexiune nu expune un
converter la fel). Importul e păzit ca `storage.config`, ca modulele pure să
rămână importabile pe o stație fără driver.

**`bytes` sunt singurul lucru scris de mână**, ca literal `0x…`: quoting-ul
driverului pentru ele e octet brut, care n-are ce căuta într-un fișier text
UTF-8. Un tip pe care converterul îl refuză nu capătă un escape inventat: se
scrie `/* VALOARE NEREPREZENTABILĂ: <tip> */`, iar coloana și cheia rândului
ajung în `_99_final.txt`, la «Observații». Instrucțiunea chiar trimisă nu e
atinsă — doar consemnarea ei.

### 1.8 Legarea

* `execute.run(..., dump=None)` — argument nou cu nume, deci fiecare apelant și
  fiecare test existent merge neschimbat.
* `_empty_tables(..., dump=None)` și `_write(..., dump=None)` la fel.
* `migrare.py` → `rulare` → `work(job)` construiește `SqlDump`: e singurul loc
  care știe deodată `an`, `dc`, `job.id`, `force` și `replace`. Se construiește
  **înaintea conexiunii** — `MIGRARE_SQL_DIR` lipsă oprește scrierea înainte să
  înceapă ceva.

---

## 1.9 Pasul 05 — CAUZA ADEVĂRATĂ: parserul de coloane pierdea coloana

Adăugat după ce operatorul a semnalat că **`CodAngajament` lipsește din grila
«Coloane»**, deși există în `.accdb`. Tot ce e mai sus apără regula; **defectul
care a produs eroarea 1364 era mai devreme pe lanț**, iar pasul 04 nu l-a găsit,
fiindcă planul arăta spre harta de corelații și n-am verificat dacă lista de
coloane dinspre Access e măcar completă.

`accdb.columns()` citea ieșirea lui `mdb-schema ... mysql` cu O SINGURĂ expresie
ancorată, care acoperea nume + tip + dimensiune opțională **și nimic altceva**:

```python
_COL_RE = re.compile(r"^\s*`(?P<name>[^`]+)`\s+(?P<type>[A-Za-z0-9_ ]+?)\s*(?:\((?P<size>[0-9, ]+)\))?\s*,?\s*$")
```

Orice linie cu ceva **după** dimensiune nu se potrivea, iar coloana era scoasă
din listă **fără un cuvânt**. Măsurat pe expresia veche:

| linie | vechi |
|---|---|
| `` `CodAngajament` varchar (50), `` | păstrată |
| `` `CodAngajament` varchar (50) NOT NULL, `` | **PIERDUTĂ** |
| `` `Valoare` numeric (19,4) NOT NULL, `` | **PIERDUTĂ** |
| `` `IdUnitate` long int NOT NULL, `` | păstrată, dar cu tipul `LONG INT NOT NULL` |

Deci exact **coloanele cu dimensiune care sunt `NOT NULL`** dispăreau —
`varchar(n)`, `numeric(p,s)`, `text(n)`. `CodAngajament` e `varchar(50)` și e
cheia primară a lui `FX_Angajamente`, deci `NOT NULL`: lipsea din grilă, lipsea
din `access_columns`, deci nu intra niciodată în `INSERT`, deci 1364. Un tip fără
dimensiune scăpa doar fiindcă clasa de caractere înghițea constrângerea în tip.

**Reparat** prin despărțirea a ceea ce trebuie să fie strict de ceea ce e
best-effort: `_COL_RE` potrivește acum **doar numele** (între accente grave) plus
restul liniei; `_CONSTRAINT_RE` curăță de la coadă constrângerile
(`NOT NULL`, `NULL`, `AUTO_INCREMENT`, `PRIMARY KEY`, `UNIQUE`, `DEFAULT …`,
`COMMENT …`), iar `_TYPE_RE` citește tipul și dimensiunea din ce rămâne.

**Regula care contează:** o coloană nu se pierde NICIODATĂ pentru că nu i s-a
putut citi tipul. Tipul de acolo nu decide nimic — validarea îl ia din MariaDB,
care e cea care acceptă sau refuză rândul — pe când un nume lipsă schimbă tăcut
`INSERT`-ul. Un tip necitit dă `tip = ""` și coloana rămâne.

Parsarea a ieșit din `columns()` în `parse_columns(text)` / `_parse_column(line)`,
funcții pure de text, ca să poată fi testate fără mdbtools, fără `.accdb` și fără
server.

**Neverificat, spus limpede:** nu am putut rula `mdb-schema` (nu e pe stația
Windows și nu există ieșire capturată în depozit), deci **nu am confirmat pe
octeți că tokenul de după dimensiune este chiar `NOT NULL`**. Ce E verificat, cu
măsurătoare pe expresia veche: orice text după `(dimensiune)` făcea linia să nu
se potrivească, iar coloana se pierdea. Reparația nu depinde de care e tokenul —
parserul păstrează acum coloana oricare ar fi el.

### Teste noi

`PYTHON/tests/test_migrare_accdb_columns.py` — 11 teste, offline, fără mdbtools:
că o coloană `NOT NULL` cu dimensiune nu se pierde, că toate coloanele și
ordinea lor se păstrează, că tipul nu mai înghite constrângerea, dimensiunea
(inclusiv perechea zecimală), și cele două care apără regula de mai sus — un tip
de necitit și o constrângere necunoscută **nu costă coloana**.

> Pasul 04 spunea «fără teste automate noi». Asta e alt pas și alt defect: o
> regresie care a costat o rulare merită un test, și e ieftin fiindcă parsarea a
> devenit text pur.

---

## 1.10 Pasul 06 — parserul Access ▸ MariaDB (`parser.py`)

Semnalat de operator imediat după pasul 05:

```
EROARE: Scrierea în «FX_Angajamente» a eșuat: 1292 (22007): Incorrect datetime
value: '04/28/26 15:28:03' for column `000_DEMO`.`FX_Angajamente`.`DTQ` at row 1
```

Aceeași boală ca la §1.9, cu un etaj mai jos. `validate._DATE_FORMATS` conținea
DEJA `"%m/%d/%y %H:%M:%S"`, deci analiza **accepta** valoarea — o convertea ca
s-o judece — dar scrierea trimitea **șirul original**. Un verificator care
convertește ca să judece, lângă un scriitor care nu convertește, e un
verificator care minte.

**`PYTHON/routes/migrare/parser.py`**, modul nou: singurul loc care traduce, și
îl cheamă **amândouă** — `validate.analyze()` și `execute.run()`, în același
punct al buclei, imediat după `with_target_names`. Ce s-a măsurat e ce pleacă.

**Ținta decide.** Forma o dă tipul coloanei MariaDB; Access e doar proveniența.

| Ce vine | Ce pleacă |
|---|---|
| `04/28/26 15:28:03`, `28/04/2026`, `28.04.2026`, `04/28/2026`, ISO, cu/fără oră, `AM`/`PM` | `datetime` / `date` adevărat |
| `1234,56` | `Decimal("1234.56")` |
| `-1`/`0`, `True`/`False`, `Da`/`Nu` pe `tinyint(1)` | `1` / `0` |
| text gol într-o coloană ne-text | `NULL` |
| dată întreagă spre o coloană `time` | `15:28:03` |

**Nu inventează.** Ce nu poate citi trece **neschimbat**, ca `check_value` să-l
raporteze drept `TIP` — constatare blocantă. Un zero pus în locul unei valori
necitite e mai rău decât o rulare oprită. (`31/02/2026` și `nu e o dată` ies
exact așa, verificat.)

Trei decizii luate anume, fiindcă ghicitul aici strică date:

1. **`tinyint(1)` e boolean, `tinyint` simplu NU.** `-1` e un tinyint valid; pe o
   coloană care numără ceva, `-1 → 1` ar fi corupție, nu conversie. Recunoașterea
   se face pe `tip_complet`, nu pe `tip`.
2. **Fără separator de mii** — informație de la operator (2026-08-21): Access nu
   scrie separator de mii nici când coloana are format de afișare, fiindcă
   formatul e cum se ARATĂ valoarea, nu cum se păstrează. Deci `,` = separator
   zecimal, punct. Prima variantă pe care o scrisesem avea o euristică
   „separatorul cel mai din dreapta e cel zecimal"; a fost **scoasă** — era
   invenție. Un șir cu amândoi separatorii sau cu spațiu între cifre rămâne
   neatins: nu poate veni din Access, deci nu există citire sigură a lui
   `1.234,56`.
3. **Ziua/luna când sunt amândouă ≤ 12** — an din 2 cifre cu `/` → **luna prima**
   (formatul propriu al lui mdbtools, `%m/%d/%y`, și mdbtools ne produce
   rândurile); an din 4 cifre cu `/` → **ziua prima**; `.` sau `-` → **ziua
   prima**. Cele două reguli sunt constante sus în `parser.py`. **Fiecare** dată
   ambiguă intră în `_02_parsare.log` cu citirea aleasă — auditabilă, nu de
   crezut pe cuvânt. Separatorul e **capturat** de expresie (`(?P=sep)`), nu
   căutat după aceea: `.` și `/` se citesc diferit, deci a ghici care a fost
   înseamnă a ghici ziua și luna.

**Un defect al meu, prins la verificare:** prima versiune identifica anul cu
`c > 31`, ceea ce arunca **toți anii din două cifre** — adică exact formatul
raportat. Al treilea număr e anul, punct; pivotul e 70 (sub → 2000, de la 70 →
1900), ca la POSIX și MariaDB.

### `_02_parsare.log`

Cerut de operator: parsarea să lase o urmă în `MIGRARE_SQL_DIR`, pe DC-ul ei.
Fișier nou în dosarul rulării (care e deja pe DC), cu mânerul lui, deschis pe
toată rularea fiindcă schimbările vin amestecate de la toate tabelele:

```
=== FX_Angajamente ===
CodAngajament=AN-2026-0001 | DTQ | «04/28/26 15:28:03» → 2026-04-28 15:28:03
CodAngajament=AN-2026-0001 | Valoare | «1234,56» → 1234.56
CodAngajament=AN-2026-0001 | Activ | -1 → 1 (Access folosește -1 pentru «da»)

--- Totaluri ---
3 conversii, dintre care 0 cu zi/lună ambiguă.
  FX_Angajamente.Activ: 1  …
```

**Numai ce s-a schimbat.** O valoare trecută neatinsă n-are ce spune, iar scrisul
tuturor celulelor ar face fișierul de neconsultat exact pentru cel care caută o
conversie greșită. `_99_final.txt` capătă o linie cu totalul conversiilor și cu
câte au fost ambigue. `flush` pe lot, ca restul dosarului; păzit de același
`_guard`, deci nici jurnalul de parsare nu poate rupe migrarea. Analiza NU scrie
în dosar (decizia rămâne), dar spune pe tabel, în jurnalul lucrării, câte valori
a adus la formă și câte date au fost ambigue.

### Jurnalizarea erorilor (cerută explicit)

* `migrare._err()` **loghează acum el însuși**, `logger.warning`, cu codul și
  mesajul — și e singurul loc prin care trec TOATE răspunsurile de eroare ale
  migrării, deci o singură schimbare le acoperă pe toate ~20. `_fail()` îi
  trimite `log=False`: el a scris deja urma completă cu `logger.exception`.
* `routing._key_rows()` înghițea un `AccdbError` în jurnalul LUCRĂRII (care
  trăiește două ore) fără să-l pună în jurnalul SERVERULUI. Acum face amândouă.
* `parser.parse_value()` prinde orice și loghează cu `logger.exception`, apoi
  întoarce valoarea neatinsă: parsarea nu poate omorî migrarea.

**Ce am lăsat anume nelogat, și de ce:** predicatele `_as_int` / `_as_decimal` /
`_as_float` / `_as_datetime` din `validate.py` și `routing.py`,
`parser._number_from_text` și `parser._valid`. Alea nu sunt erori, sunt
întrebări — «e numărul ăsta un întreg?». Ele răspund `None` pentru fiecare
celulă care nu e un număr, iar asta e chiar mecanismul prin care `check_value`
produce constatarea. Logate, ar scrie o linie per celulă neconformă — milioane
de linii care ar îneca erorile adevărate. La fel cele trei `except ImportError`
(`config`, `mysql.connector`): sunt sonde de capabilitate, documentate, iar
`logger` nici nu e configurat la momentul importului.

### Teste noi

`PYTHON/tests/test_migrare_parser.py` — 37 de teste, offline: eșecul raportat,
cele șase forme de dată, ora AM/PM, pivotul anului din două cifre, coloanele
`date`/`time`, data imposibilă și textul care nu e dată (amândouă rămân
neatinse), cele trei reguli de ambiguitate, virgula zecimală, refuzul de a ghici
`1.234,56` și `1 234`, `tinyint(1)` față de `tinyint` simplu, textul gol, și
promisiunea că `parse_row` **nu aruncă niciodată** (un obiect al cărui `__str__`
explodează trece neatins).

---

## 1.11 Pasul 07 — cheile străine: ordinea de scriere, familia ORD, orfanii

Adăugat după eșecul `FX_Rezervari` de pe 21.08 (`1452`,
`FX_Rezervari__FX_DDF_REV`) și după recitirea schemei `000_DEMO` reîmprospătate:
**55 de tabele, 95 de chei străine**.

### Schema se mișcă — nu se mai scrie de mână ce spune ea deja

Copia veche de `000_DEMO.sql` arăta `FX_Rezervari` **fără nicio cheie străină**.
Constrângerea care a picat rularea a fost adăugată după ce s-a luat acea copie.
De aceea tot ce se putea citi din `information_schema` la rulare **se citește**,
nu se scrie în `tables.py`. Listele de mai jos sunt starea de pe 22.08 și sunt
acolo ca să verific rezultatul, nu ca să fie tastate.

### (a) Ordinea de scriere

O cheie străină cere rândul părinte **prezent la INSERT**. Golirea părintelui
întâi, în «Înlocuiește tot», nu relaxează nimic: face eșecul **sigur**, nu îl
evită. `FX_Istoric` scrisese 3246 de rânduri cu o secundă mai devreme, cu
propria lui coloană `IDREV`, numai fiindcă `FX_Istoric.IDREV` **nu poartă**
constrângere.

Printre cele 27 de tabele migrate, ordinea veche din `tables.ALL` încălca
**exact una** dintre cele 95 de constrângeri:

```
poziția 4  FX_Rezervari.IDREV -> FX_DDF_REV  (scris pe poziția 14)
```

**Ordinea implicită nouă** mută `FX_DDF` și `FX_DDF_REV` pe pozițiile 3 și 4.
Nimic altceva nu se mută. `FX_DDF` are nevoie doar de `FX_Angajamente`, care
rămâne primul; `FX_DDF_REV` doar de `FX_DDF`.

```
 1 FX_Angajamente     10 FX_Receptii         19 FX_ORD
 2 FX_Indicatori      11 FX_Plati            20 FX_ORD_PART
 3 FX_DDF        <--  12 FX_Extrase_F        21 FX_ORD_TBL
 4 FX_DDF_REV    <--  13 FX_Extrase_H        22 FX_ORD_DOC
 5 FX_Istoric         14 FX_Extrase          23 FX_ORD_ATT
 6 FX_Rezervari       15 FX_DDF_REV_SA       24 FX_Salarii
 7 FX_Receptii_R      16 FX_DDF_REV_SB       25 FX_Rezervarii_IMG
 8 FX_Receptii_RHR    17 FX_DDF_REV_ATT      26 FX_Receptii_IMG
 9 FX_Receptii_H      18 FX_DDF_REV_PRT      27 FX_Receptii_Plati
```

Și **verificarea care face ordinea verificabilă**, ca ordinea implicită să fie
un punct de plecare bun, nu o promisiune pe care se sprijină codul:
`validate.order_findings(schema, chosen_names, db_name)` citește constrângerile
dintre tabelele bifate din `information_schema.KEY_COLUMN_USAGE` (le are deja
`TargetSchema`) și compară pozițiile în aranjarea **operatorului**. Un copil
înaintea părintelui lui e o constatare `ORDINE_TABELE`, clasă **BLOCANT**:

```
«FX_Rezervari» se scrie înaintea lui «FX_DDF_REV», dar depinde de el prin
«FX_Rezervari__FX_DDF_REV» (IDREV). Mută-l după el în lista de tabele.
```

Se sare peste o cheie pe care tabelul o are pe **el însuși** (nu se poate
rearanja) și peste una al cărei părinte **nu e bifat** (nu se scrie în rularea
asta; rândurile lui se verifică față de țintă așa cum e). `execute.run()` face
aceeași verificare încă o dată, pe ordinea pe care a trimis-o chiar cererea de
rulare — analiza a măsurat o aranjare, dar nimic nu oprea alta să ajungă la
`rulare`.

### (b) Familia ORD: id-urile din Access SUNT id-urile de pe MariaDB

Confirmat de operator (22.08). Pe MariaDB cheile familiei sunt coloanele cu `P`
la coadă și sunt `AUTO_INCREMENT`, dar auto-increment-ul pornește **doar când
coloana lipsește din `INSERT`**. Scrisă explicit, valoarea din Access intră cum
e — exact ce fac deja `FX_DDF.IDDF` și `FX_DDF_REV.IDREV`, tot `AUTO_INCREMENT`
amândouă, și de-aia ține lanțul DDF și nu ținea cel ORD. Fără hartă de id-uri,
fără `lastrowid`, fără a doua trecere.

Cinci corelații noi în `tables.COLUMN_RENAMES`, lângă perechea `IdClsf`:
`IDORD→IDORDP`, `IDORDPART→IDORDPARTP`, `IDORDTBL→IDORDTBLP`,
`IDORDDOC→IDORDDOCP`, `IDORDATT→IDORDATTP`. Regula veche de siguranță e
neschimbată: o corelație se aplică **doar unde ținta chiar are coloana pe care o
numește**, deci fiecare atinge exact tabelul ei.

**O reparație pe care planul o cerea implicit și pe care mecanismul n-o avea:**
`default_rename_map` **eliberează acum potrivirea după nume pe care o
înlocuiește**. Fără asta, Access `IDORD` și Access `IDORDP` ajungeau amândouă pe
`IDORDP`, iar `insert_columns` oprea totul cu «două coloane din Access corelate
cu aceeași coloană». Așa se împlinește promisiunea planului că **cele 144 de
zerouri (`IDORDP` în Access, măsurat 144/144) nu mai călătoresc deloc**: coloana
rămâne pur și simplu necorelată. Coloana care e ea însăși SURSA unei corelații e
lăsată în pace — `IdClsf` e și revendicat (de `IdClsfPY`), și redenumit (în
`IdClsfAcc`), iar o ștergere naivă i-ar fi anulat propria regulă. Efect
secundar, corect și verificat: pe un tabel al cărui MariaDB are `IdClsf` dar nu
și `IdClsfAcc`, Access `IdClsf` rămâne acum **necorelat** în loc să se ciocnească
de `IdClsfPY` pe aceeași țintă.

`SeedTable.primary_key` al celor cinci tabele devine numele **țintei**
(`IDORDP`, `IDORDPARTP`, `IDORDTBLP`, `IDORDDOCP`, `IDORDATTP`). Cu asta începe
să funcționeze upsert-ul lui `FX_ORD`: până acum `tables.py` declara cheia
`IDORD`, cea de pe MariaDB e `IDORDP`, iar `IDORD` n-are index unic — deci a
doua rulare «Adaugă/actualizează» ar fi inserat din nou toate cele 38 de ordine
în loc să le actualizeze.

**`key_column` NU s-a schimbat** (verificat pe diff: fiecare valoare e
neatinsă). Rutarea citește rândul cu numele lui din **Access**, înaintea
oricărei corelații. Planul avertiza că ăsta e locul cel mai probabil de defect
tăcut, și chiar era unul: `TableSelector.primary_key_of` citea
`table.primary_key` **dintr-un rând Access**. Cu cheia mutată pe numele țintei,
ar fi raportat fiecare rând ORD sub coloana Access `IDORDP` — adică `0`, pe
toate. De aceea `SeedTable` a căpătat **`access_key`** (implicit egal cu
`primary_key`), iar `primary_key_of` citește de acolo.

Al doilea loc cu aceeași boală, găsit tot așa: în `migrare.tabele_fisier`,
steagul «cheie» al grilei de coloane compara numele **Access** cu cheia primară
a **țintei**. Pe familia ORD ar fi bifat exact coloana greșită — cea care în
Access e numai zerouri. Se judecă acum pe **ținta corelației**.

### (c) Un orfan pe o coloană care acceptă NULL călătorește ca NULL

Decizia operatorului (22.08), care înlocuiește convenția «se sare rândul» pentru
cazul ăsta. Orfanul se judecă acum după **coloana copilului**, nu după tabel:

* **acceptă NULL** → rândul se **scrie**, cu coloana aceea golită. Rândul se
  păstrează; se pierde doar legătura. Constatare **FORȚABIL**, cu valoarea
  aruncată; o linie pe tabel în jurnalul lucrării — `«FX_Rezervari»: 3 valori
  IDREV fără corespondent, scrise ca NULL.` — și aceeași linie în
  `_99_final.txt` din dosarul de la §1.6;
* **`NOT NULL`** → NULL nu e disponibil, deci rândul nu poate fi nici scris,
  nici golit: constatare nouă **`CHEIE_STRAINA_OBLIGATORIE`**, clasă
  **BLOCANT**, cu coloana numită, și nu se scrie nimic.

Un **`0`** e orfan oriunde cheia părintelui e `AUTO_INCREMENT`: un asemenea rând
nu poate exista. Ca să știe asta, `TargetSchema` mai face o trecere prin
`information_schema.COLUMNS` pentru tabelele către care **arată** cheile —
inclusiv cele din afara setului migrat, ale căror coloane nu se încarcă altfel
(`referenced` + `referenced_is_auto`). Necunoscut răspunde `False`: nu inventăm
un orfan. (Măsurat pe `FX_Rezervari.IDREV`: 125 NULL, 0 zerouri, 34 reale — deci
după (a) asta ar trebui să atingă puține rânduri sau niciunul, dar regula
trebuie să existe.)

Pe partea de scriere, `execute._null_orphans(row, nullable)` golește valorile
**înainte** ca rândul să fie judecat (coloana acceptă NULL, deci trece de
`check_value` pe propriile ei condiții) și întoarce coloanele golite; numărarea
se face **abia după** `_row_is_blocked`, altfel un rând sărit din alt motiv ar fi
raportat drept «scris cu coloana golită», ceea ce ar fi o minciună.

`Report.add` a căpătat un `count` (implicit 1): cheile străine adună valori
**distincte**, dar operatorul are nevoie să citească pe câte **rânduri** apare
fiecare. Exemplul rămâne cheia primului rând, cea pe care merită s-o caute.
`report.null_fk` e geamănul lui `report.missing_fk` și cele două nu se suprapun
niciodată.

### (d) Cheile străine din afara setului migrat

Zece dintre ele, și «Înlocuiește tot» nu poate ajuta cu niciuna — părinții aceia
nici nu se golesc, nici nu se scriu:

```
FX_DDF_REV_SA   IdPartener -> Parteneri   IdClsf -> Clasificatii   IdUnitate -> Unitati
FX_DDF_REV_SB   IdPartener -> Parteneri   IdClsf -> Clasificatii   IdUnitate -> Unitati
FX_DDF_REV_PRT  IdClsf -> Clasificatii
FX_ORD_TBL      IdPartener -> Parteneri   IdClsf -> Clasificatii   IdUnitate -> Unitati
```

**Nu a fost nevoie de cod nou.** `_check_foreign_keys` întreabă ținta pentru
FIECARE cheie străină a tabelului, oricare ar fi părintele; `incoming` (rândurile
scrise în aceeași rulare) e pur și simplu `None` pentru ele, fiindcă nu există
«se va scrie în rularea asta» de adăugat. Ce s-a schimbat pentru ele e regula de
la (c): `FX_ORD_TBL.IdClsf` acceptă NULL, deci o clasificație nepotrivită devine
NULL — rândul supraviețuiește, legătura nu, și **apare în raport**, nu doar
într-o linie de jurnal. Aici trebuie să fie corectă perechea `IdClsf`/`IdClsfPY`;
e următorul `1452` care așteaptă.

### (e) `FX_ORD_TBL.IdUnitate`

`int(11) NOT NULL`, cheie străină spre `Unitati(IdUnitate)`, și **călătorește din
Access** — nu se completează pe server (confirmat de operator). Nu are nevoie de
cod propriu; cele trei feluri în care mușcă sunt acoperite de pazele de mai sus,
verificat:

1. arată a coloană de rutare, iar coloanele de rutare se debifează din
   obișnuință — dar fiind `NOT NULL` fără implicit, §1.3 oprește rularea și o
   numește (`COLOANA_OBLIGATORIE`);
2. fiind `NOT NULL`, regula de la (c) nu i se aplică: un `IdUnitate` absent din
   `Unitati` e **BLOCANT** (verificat: 3 rânduri cu `IdUnitate=99` →
   `CHEIE_STRAINA_OBLIGATORIE`, `are_blocante=True`, `poate_forta=False`);
3. `Unitati` e în afara setului migrat, deci **înainte de prima rulare adevărată
   trebuie confirmat că `Unitati` de pe țintă chiar conține id-urile de unitate
   pe care le poartă fișierul** — altfel toate rândurile `FX_ORD_TBL` pică
   deodată. Rămâne de făcut pe server (vezi §4).

### Verificări (scripturi de unică folosință, nu în depozit)

* `order_findings`: aranjarea veche dă exact perechea raportată, cu mesajul
  cuvânt cu cuvânt; **ordinea implicită nouă dă zero**; o cheie pe sine, un
  părinte nebifat și o cheie spre altă bază nu produc nimic;
* `_check_foreign_keys` cu o conexiune falsă: coloană nullable → `null_fk`,
  FORȚABIL, `numar` = numărul de RÂNDURI (2 rânduri pe o valoare, nu 1);
  aceeași valoare pe o coloană `NOT NULL` → `missing_fk`, BLOCANT, ambele butoane
  stinse; un `0` pe un părinte `AUTO_INCREMENT` iese orfan chiar dacă `0` chiar
  există pe țintă, iar fără `AUTO_INCREMENT` nu iese;
* corelațiile ORD cap-coadă prin `insert_columns`: `FX_ORD_PART` scrie
  `IDORDPARTP, IDORDP, Counter, DenBene`, iar Access `IDORDP` iese
  `necorelată`; `FX_ORD` scrie `IDORDP, CodAngajament, Numar`;
* `analyze()` + `execute.run()` cap-coadă pe o conexiune și un fișier false:
  rândul cu `IDREV=999` (orfan) **se scrie** cu `IDREV=None`, rândul cu `IDREV=7`
  (scris în aceeași rulare) își păstrează valoarea, jurnalul spune linia cu
  NULL-urile; cu ordinea inversată, `execute.run()` se oprește cu mesajul de
  ordine chiar și pe un raport altfel curat.

---

## 2. Fișiere atinse

| Fișier | Ce |
|---|---|
| `PYTHON/routes/migrare/validate.py` | `F_COLOANA_OBLIGATORIE` + `CLASS_OF`; `meta["extra"]`; `is_required`, `required_columns_of`, `insert_columns`, `describe_skipped`, `missing_required`, `required_columns_message`, `accdb_columns`; `column_rename_map(..., protected)`; `analyze()` construiește lista de coloane și raportează lipsurile |
| `PYTHON/routes/migrare/execute.py` | `run(..., dump=)`; `protected` + `insert_columns` + `missing_required`; liniile de jurnal cu coloanele; `_name_list`, `_statement_text`, `_key_text`, `_literal`; `_empty_tables`/`_write` consemnează |
| `PYTHON/routes/migrare/dump.py` | **nou** — `SqlDump` |
| `PYTHON/routes/migrare/storage.py` | `sql_dir()` (`MIGRARE_SQL_DIR`) |
| `PYTHON/routes/migrare/migrare.py` | `rulare` construiește și pasează `SqlDump` |
| `PYTHON/routes/migrare/parser.py` | **nou** — traducerea Access ▸ MariaDB (date, zecimale, Da/Nu, text gol), chemată identic de analiză și de scriere |
| `PYTHON/tests/test_migrare_parser.py` | **nou** — 37 de teste de parser |
| `PYTHON/routes/migrare/routing.py` | `AccdbError` înghițit ajunge acum și în jurnalul serverului |
| `PYTHON/routes/migrare/accdb.py` | **cauza adevărată** — `_COL_RE` potrivește acum doar numele; `_CONSTRAINT_RE` + `_TYPE_RE` citesc restul best-effort; parsarea scoasă în `parse_columns` / `_parse_column` |
| `PYTHON/tests/test_migrare_accdb_columns.py` | **nou** — 11 teste de regresie pe parser |
| `PYTHON/routes/migrare/README.md` | `MIGRARE_SQL_DIR` în `config.py` + `mkdir`; secțiune «Coloane obligatorii — a treia pază»; secțiune «Ce SQL s-a rulat (dosarul de instrucțiuni)»; refuzul nou în «Corelațiile de coloane»; `COLOANA_OBLIGATORIE` în lista BLOCANT |

**Pasul 07 (cheile străine):**

| Fișier | Ce |
|---|---|
| `PYTHON/routes/migrare/tables.py` | `FX_DDF`/`FX_DDF_REV` pe pozițiile 3 și 4; `SeedTable.access_key`; cheile primare ale familiei ORD devin numele țintei; cinci corelații noi în `COLUMN_RENAMES`; `default_rename_map` eliberează potrivirea după nume pe care o înlocuiește |
| `PYTHON/routes/migrare/validate.py` | `F_ORDINE_TABELE` + `F_CHEIE_STRAINA_OBLIGATORIE` în `CLASS_OF`; `order_findings` / `order_message`; `TargetSchema.referenced` + `referenced_is_auto` (a doua trecere prin `information_schema` pentru tabelele referite); `Report.null_fk`; `Report.add(..., count)`; `_check_foreign_keys` judecă orfanul după coloană și numără rândurile |
| `PYTHON/routes/migrare/execute.py` | verificarea ordinii pe cererea de rulare; `_null_orphans`; linia de jurnal + observația din `_99_final.txt` pentru valorile golite |
| `PYTHON/routes/migrare/routing.py` | `primary_key_of` citește `access_key`, nu `primary_key` |
| `PYTHON/routes/migrare/migrare.py` | steagul «cheie» al grilei de coloane se judecă pe ținta corelației |
| `PYTHON/tests/test_migrare_routing.py` | ordinea așteptată actualizată, cu motivul scris lângă ea |
| `PYTHON/routes/migrare/README.md` | «Ordinea de scriere și cheile străine»; familia ORD în «Corelațiile de coloane»; secțiune nouă «Cheile străine: orfanii» + «Chei străine care ies din setul migrat»; cele două feluri noi în lista BLOCANT |

---

## 3. Rezultate de test

```
PYTHON/.venv/Scripts/python -m pytest tests/ -q
198 passed, 15 skipped in 21.95s
```

Verde. 150 la sfârșitul pasului 04, 161 după pasul 05 (11 teste de parsare a
coloanelor), 198 după pasul 06 (37 de teste de parser de valori). **Pasul 07 nu
adaugă teste** (planul o cere anume) și lasă suita tot la 198: singurul test
atins e cel al ordinii tabelelor, care acum așteaptă perechea DDF pe pozițiile 3
și 4, cu motivul scris lângă el.

Verificat pe lângă suită, cu scripturi de unică folosință (nu în depozit):

* `is_required` pe toate cele șase forme (`NOT NULL` simplu → True; nullable,
  cu implicit, `auto_increment`, generated, `on update` → False);
* refuzul corelației «(nu se scrie)» pe cheia primară, cu mesajul complet;
  `protected=None` păstrează comportamentul vechi;
* `insert_columns` + `describe_skipped` → `CodAngajament (debifată),
  IdUnitate (necorelată)`; `missing_required` → `['CodAngajament']`; dublura de
  țintă oprește;
* `_literal` pe cele zece valori din tabelul de la §1.7;
* `SqlDump` cap-coadă cu o conexiune falsă: cele patru fișiere au ieșit exact în
  forma din plan; pe calea de eșec, instrucțiunea care pică e ȘI în fișierul
  tabelului, ȘI în `_99_final.txt` cu `ROLLBACK` și mesajul driverului; al doilea
  `failure()` (din `run`) nu a înlocuit-o cu unul mai sărac;
* `MIGRARE_SQL_DIR` lipsă → `StorageError` cu numele cheii;
* mâner de fișier rupt în mijlocul rulării → `disabled = True`, o linie în
  jurnalul lucrării, urmă completă în jurnalul serverului, migrarea continuă.

---

## 4. Neverificat / rămas

* **Nimic nu s-a rulat pe date reale.** Nici MariaDB, nici `mdbtools`, nici un
  fișier `.accdb`. Toată felia 0044 e în starea asta.
* **`MIGRARE_SQL_DIR` trebuie pus DE MÂNĂ în `config.py` pe server** —
  `config.py` nu e în depozit. Până atunci **`POST /api/migrare/rulare` nu
  pornește**, cu numele cheii în mesaj. Asta e ordinea de operații pentru
  următoarea rulare: cheia + `mkdir`, apoi repornire `avacont`.
* Dimensiunea dosarului nu e măsurată pe date reale — 20–40 MB pe rulare e
  estimare din numărul de rânduri, nu observație.
* Textul `_00_info.txt` scrie anul din cererea de rulare (`body["an"]`); dacă
  clientul nu-l trimite, rândul rămâne gol. Nu s-a adăugat o gardă: numele
  fișierului împins îl cere oricum mai devreme.
* Abatere mică de la semnătura din plan: `SqlDump.__init__` primește `replace`
  (bool), nu `mode` — textul modului se face înăuntru. Fără efect funcțional.
* Migratorul (.NET) nu știe încă de `COLOANA_OBLIGATORIE`; îl arată ca pe orice
  alt fel de constatare, iar clasa `BLOCANT` vine gata calculată de server, deci
  butoanele se sting corect fără nicio schimbare pe .NET. La fel cele două
  feluri noi ale pasului 07, `ORDINE_TABELE` și `CHEIE_STRAINA_OBLIGATORIE`.

**Pasul 07 — ce rămâne neverificat, spus limpede:**

* **Cele 95 de chei străine și cele 55 de tabele n-au fost citite de mine.**
  Numerele, lista celor zece chei din afara setului migrat și «exact o
  încălcare în ordinea veche» vin din plan, adică din citirea operatorului.
  Ce AM verificat e mecanismul: `order_findings` găsește perechea
  `FX_Rezervari`/`FX_DDF_REV` pe ordinea veche și **niciuna** pe cea nouă, cu
  setul de constrângeri dat lui. Dacă schema de pe server spune altceva,
  verificarea din `analyze()` o va spune la prima rulare — de-aia citește din
  `information_schema` și nu dintr-o listă.
* **Coloanele vechi de pe MariaDB ale familiei ORD.** `PYTHON/routes/ord/*.py`
  (fluxul ORD care există deja) **scrie** `IDORDPART`, `IDORDTBL`, `IDORDDOC`,
  `IDORDATT` cu id-urile din Access, prin `_strict_pos_int` — deci nu sunt
  coloane moarte. După corelațiile pasului 07, **migrarea nu le mai umple**:
  valoarea din Access se duce în coloana cu `P`. Asta e decizia operatorului
  («id-urile din Access SUNT id-urile de pe MariaDB»), scrisă de două ori în
  plan, deci am implementat-o — dar consecința trebuie știută. Dacă vreuna e
  `NOT NULL` fără valoare implicită, analiza se oprește și o numește
  (`COLOANA_OBLIGATORIE`): eșec zgomotos, cu numele coloanei, înainte să se
  scrie ceva — nu corupție tăcută. **De verificat pe server înainte de prima
  rulare a familiei ORD.**
* `IDORDATTP` — planul însuși spunea «verifică numele adevărat înainte de a-l
  scrie». Nu am putut interoga baza; l-am scris așa cum îl scrie
  `PYTHON/routes/ord/att.py` (`IDORDATTP`, cu `cursor.lastrowid`), care e cea
  mai bună dovadă din depozit. Dacă numele e altul, corelația **nu se aplică**
  deloc (regula veche: se aplică doar unde ținta chiar are coloana), deci
  greșeala e inertă, nu distructivă.
* **Un orfan pe o coloană `NOT NULL` e acum BLOCANT**, unde înainte era forțabil
  și rândul se sărea. Nu mai există cale de «Forțează rularea» pentru cazul
  ăsta. E chiar ce cere planul (§3.3, §3.5), dar e o schimbare de comportament,
  nu o adăugare.
* **`Unitati` de pe țintă nu a fost verificată** că poartă id-urile de unitate
  din fișier. Dacă nu le poartă, toate rândurile `FX_ORD_TBL` pică deodată, ca
  BLOCANT. E prima verificare de făcut pe server, lângă `MIGRARE_SQL_DIR`.
* Ca tot restul feliei 0044: **nimic nu s-a rulat pe date reale** — nici
  MariaDB, nici `mdbtools`, nici un `.accdb`.
