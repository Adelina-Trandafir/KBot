# SLICE-0043 — Pasul de deblocare a cheilor străine din `schema_sync`

**Data:** 2026-08-20
**Fișiere:** `PYTHON/routes/schema_sync/schema_diff.py`,
`PYTHON/routes/schema_sync/README.md`, `PYTHON/routes/schema_sync/__init__.py` (nou, gol)
**Stare:** DONE — **verificat pe un server MariaDB 10.3.32 real**, cu o bază de
test construită instrucțiune cu instrucțiune. Nu pe baza de producție.

---

## 1. Ce s-a cerut

Se bănuia că `SchemaDiff._unblock_collation` generează doar jumătate din
instrucțiuni: pune `ADD CONSTRAINT` la loc, dar nu emite `DROP FOREIGN KEY`
înainte. Simptomul raportat, dintr-o rulare `--view --mode SAFE`:

```
      1  COLUMN          DROP       [DISTRUCTIV]
      2  COLLATION       MODIFY     [DISTRUCTIV]
      2  FK              CREATE
```

Două `FK CREATE`, zero `FK DROP`.

## 2. Verdictul: ipoteza A era cea corectă

**Codul emitea corect și ștergerea, și recrearea.** Ce se stricase era baza de
test. Fusese construită ca un singur lot `mariadb`, clientul se oprește la prima
eroare, iar lotul murise înainte de a ajunge la `ADD CONSTRAINT` — deci ținta nu
avea **nicio** cheie străină. Neexistând nimic de desfăcut, `_unblock_collation`
tăcea, pe bună dreptate; cele două `FK CREATE` veneau din comparația obișnuită
din `_foreign_keys` (`t is None` → creează), nu din pasul de deblocare.

Ipoteza B (`_fk_blocks` ratează partea referită) — **infirmată**. Sondă pe
10.3.32: `information_schema.KEY_COLUMN_USAGE.REFERENCED_TABLE_SCHEMA` întoarce
exact numele bazei (`probe_db`), deci `fk.ref_schema == self.db` chiar se
declanșează.

Ipoteza C (ordinea din `run()`) — **infirmată** prin citire: `_recollated` se
inițializează o singură dată în `__init__`, se completează pe parcursul buclei
pe tabele, nu se golește nicăieri, iar `_unblock_collation` rulează după buclă.

## 3. Ce s-a găsit totuși — patru defecte reale

Baza de test a fost extinsă până a acoperit scenariul cerut: o cheie în aceeași
schemă și una încrucișată spre `AVACONT_COMUN`, ambele **identice** pe cele două
părți (deci comparația obișnuită nu le atinge). Rulate, au ieșit patru eșecuri:

```
[5/12]  EȘEC COLUMN MODIFY  fx_modif   — [errno=1832] Cannot change column 'Cod':
                                          used in a foreign key constraint 'fk_modif_parent'
[6/12]  EȘEC COLUMN MODIFY  fx_parent  — [errno=1833] ... of table '000_demo.fx_modif'
[10/12] EȘEC FK CREATE      fx_copil   — [errno=1452] ... REFERENCES `avacont_sursa`.`fx_angajamente`
[12/12] EȘEC FK CREATE      fx_orfan   — [errno=1146] Table '000_demo.fx_orfan' doesn't exist
```

plus un al cincilea, **fără eroare**: `fk_copil_extra`, o cheie pe care sursa nu
o are, ștearsă corect de comparația obișnuită și apoi **înviată** de pasul de
deblocare.

| Nr. | Cauza | Corecția |
|---|---|---|
| 1 | O coloană modificată în FORCE care schimbă **și** tipul, **și** charset-ul mergea pe ramura `_modify_column` și nu se înregistra în `_recollated` — deci cheia care o bloca nu era ștearsă | `_modify_column` primește coloana din țintă și înregistrează `(tabel, coloană)` când `charset_differs`; la fel `_rename_column`, sub numele **vechi** (numele la care cheia încă se referă) |
| 2 | `_fk_clause` compara `fk.ref_schema == SOURCE_DB` cu majuscule exacte. Unde serverul întoarce numele cu litere mici, rescrierea «schema proprie → baza țintă» nu se făcea, iar unitatea rămânea cu o cheie **spre șablonul** din care e clonată | `same_schema()`, comparație fără majuscule/minuscule, folosită și în `_fk_clause`, și în `_fk_blocks` |
| 3 | O cheie de pe un tabel absent din sursă (deci programat pentru `DROP TABLE`, prioritatea 5) era pusă la loc la prioritatea 11, pe un tabel care nu mai există | `_unblock_collation` sare peste tabelele care nu există în sursă |
| 4 | O cheie absentă din sursă era ștearsă de comparația obișnuită, apoi recreată din definiția **țintei** (`src_fk or fk`) | Ștergerea se cere în continuare (ea deblochează coloana, iar `_drop_fk` e idempotent), dar recrearea se face **numai** dintr-o definiție a sursei; fără ea, cheia rămâne ștearsă |

Defectul 2 este cel mai urât dintre ele: nu dă eroare decât dacă datele se
opun. Cu rânduri potrivite în șablon ar fi trecut în tăcere, lăsând baza unei
unități legată prin cheie străină de `AVACONT_SURSA`.

## 4. Rezultatele rulărilor (măsurate, nu reportate)

Server: **MariaDB 10.3.32** (aceeași versiune ca serverul de producție), ridicat
local pe portul 3399, cu `sql_mode=STRICT_TRANS_TABLES,...`. Baza de test
construită instrucțiune cu instrucțiune, cu starea verificată după fiecare pas:
5 chei străine prezente, charset-urile confirmate, diacritice reale în date.

| Rulare | Rezultat |
|---|---|
| `--view --mode SAFE` (scenariul de bază, înainte de corecții) | `FK DROP` prezent la prioritatea 1 — **simptomul raportat nu s-a reprodus** |
| `--allow-destructive --continue-on-error` (scenariul extins, înainte) | 12 instrucțiuni: **8 reușite, 4 eșuate** |
| `--run --mode FORCE --allow-destructive` (după corecții) | 11 instrucțiuni: **11 reușite, 0 eșuate, 0 neatinse** |
| `--view --mode SAFE` (a doua oară) | `000_demo: 0 instrucțiuni` |
| `--view --mode FORCE` (a doua oară) | `000_demo: 0 instrucțiuni` |

Copia de siguranță cu `mysqldump` s-a făcut și s-a verificat pe disc în rularea
distructivă; confirmarea tastată `DA` a fost cerută.

**Chei străine la final:** 3 în `AVACONT_SURSA`, 3 în `000_demo`, identice după
rescrierea referinței proprii; **niciuna** nu arată spre `AVACONT_SURSA`.

**Diacritice**, după conversia `utf8mb4` → `utf8` a coloanei `Denumire`:

```
('A-001', 'Întreținere șosele: ăâîșț ĂÂÎȘȚ')
('A-002', 'Achiziție hârtie și tonere — ședința nr. 3')
```

Text identic cu cel introdus, toate cele zece semne — ă â î ș ț Ă Â Î Ș Ț —
intacte. (Și liniuța lungă «—», care nu e diacritică, dar tot din planul de bază
Unicode face parte.)

**Secvența cerută**, exact așa cum o scrie fișierul `.sql` generat:

```sql
ALTER TABLE `000_demo`.`fx_modif` DROP FOREIGN KEY `fk_modif_parent`;      -- prioritate 1
ALTER TABLE `000_demo`.`fx_modif` MODIFY COLUMN `Cod` varchar(40)
      CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL;       -- prioritate 7
ALTER TABLE `000_demo`.`fx_modif` ADD CONSTRAINT `fk_modif_parent`
      FOREIGN KEY (`Cod`) REFERENCES `000_demo`.`fx_parent` (`Cod`) ...;    -- prioritate 11
```

## 5. Ce am aflat despre MariaDB pe drum

- **1832 și 1833 sunt două erori diferite.** 1832 = coloana care *ține* cheia;
  1833 = coloana *spre care* arată cheia, iar mesajul numește tabelul care o
  referă. README-ul pomenea doar 1832.
- **O cheie străină cere aceeași colaționare, nu doar același charset** —
  altfel 1005 / errno 150. De aceea cele două capete ale unei chei se mută
  întotdeauna împreună, iar o coloană aflată sub o cheie încrucișată spre
  `AVACONT_COMUN` **nu poate** fi reparată singură. Baza de test din raportul
  inițial cerea exact asta, ceea ce explică de ce lotul ei murise.
- **O schimbare doar de colaționare, cu același charset, este la fel de
  blocată** (1832). `charset_differs` compară și colaționarea, deci cazul e
  acoperit.

## 6. Ce rămâne deschis

- **SAFE nu repară charset-ul unei coloane care diferă și la tip.** Cele două
  verificări sunt pe ramuri `elif` ale aceleiași structuri: dacă `columns_differ`
  e adevărat, în SAFE nu se emite nimic. Neatins în mod deliberat — ar fi o
  schimbare de semantică a modului SAFE, dincolo de pasul de deblocare. Scris
  ca atare în README.
- **O cheie străină dintr-o altă bază de unitate** care arată spre o coloană
  reparată aici nu poate fi văzută (se citește doar ținta) și ar bloca
  modificarea cu 1833. Nu apare în structura actuală, unde referințele
  încrucișate merg doar spre `AVACONT_COMUN`.
- Verificarea s-a făcut pe Windows, unde `lower_case_table_names=1`. De aceea
  numele bazelor apar cu litere mici în rezultate. Chiar acest amănunt a scos la
  iveală defectul 2 — pe Linux, unde numele se păstrează cum au fost create,
  comparația exactă ar fi mers, iar fragilitatea ar fi rămas ascunsă.
- Nu s-a rulat pe baza de producție și nu s-au adăugat teste automate (nu au
  fost cerute).


---

# Pasul 0043-01 — o singură bază + integritatea legăturilor încrucișate

**Data:** 2026-08-20 (aceeași zi, cerut de operator după verificarea de mai sus)

## 1. Sincronizarea unei singure baze

`--targets` făcea deja exact asta — `--targets 000_DEMO` lucrează numai pe acea
bază și sare peste tot ce e în `CAI`. Verificat, merge.

Ce **nu** mergea era greșeala de tastare. `--targets 000_DEMOO` producea:

```
WARNING - Baza `000_demoo` nu există — ignorată.
INFO    - Schemele sunt deja sincronizate. Nimic de făcut.
(cod de ieșire 0)
```

Adică fix ce se vede după o rulare curată pe baza corectă. Nou:
`verify_targets()` — o bază **indicată explicit** care nu există oprește
rularea cu un mesaj și cod 1. O bază trecută în `CAI` dar absentă rămâne
doar avertisment: registrul are voie să fie înaintea realității, un nume
tastat de mână nu.

## 2. Legăturile spre `AVACONT_COMUN`

Întrebarea operatorului: cheile spre baza externă sunt corecte, sau rămân
date inconsistente?

Structural erau corecte deja (referința spre `AVACONT_SURSA` se rescrie, cea
spre `AVACONT_COMUN` se lasă). Ce lipsea era verificarea **datelor**. O cheie
creată peste rânduri fără părinte eșuează cu 1452 în mijlocul lotului — și
dacă acea cheie fusese ștearsă de pasul de deblocare, rularea se termina cu
cheia **pierdută**, deci cu baza mai puțin consistentă decât la început.

Nou: `_validate_foreign_keys()`, singurul loc din diff care se uită la
rânduri, nu la structură. Pentru fiecare cheie care urmează să fie creată:

1. există tabelul/coloana referită? (pentru `AVACONT_COMUN` se spune explicit
   că nu se poate repara de aici);
2. se potrivesc seturile de caractere — comparate cu forma **finală**, nu cu
   cea curentă. În interiorul țintei coloana referită se repară la pasul 8,
   înainte ca cheia să fie refăcută la 11; pentru `AVACONT_COMUN`, starea de
   acum este starea finală. **Prima variantă a comparat greșit** (planificat
   vs. curent) și a marcat drept imposibile trei chei perfect sănătoase —
   prins la rulare, nu la citire;
3. se potrivesc datele? `LEFT JOIN` + `COUNT`, plus trei valori vinovate în
   mesaj.

Ce nu trece primește `error_msg` și nu se execută (`fetch_pending` sare
rândurile cu eroare). Iar dacă cheia **există deja** în țintă, refuzul se
extinde peste tot grupul — ștergerea, schimbarea de charset de pe ambele
capete și recrearea stau pe loc împreună, altfel cheia s-ar pierde.

## 3. Rulări (măsurate)

Bază de test extinsă cu: o cheie încrucișată nouă spre `AVACONT_COMUN` peste
rânduri orfane; o cheie existentă peste date strecurate cu
`FOREIGN_KEY_CHECKS=0`, sub o coloană care avea și nevoie de reparație de
charset.

| Rulare | Rezultat |
|---|---|
| `--targets 000_demoo` (tastare greșită) | oprit, cod 1, «Baze inexistente pe server» |
| `--run --mode FORCE --targets 000_demo`, date murdare | 11 instrucțiuni: **11 reușite, 0 eșuate**; 5 rânduri amânate cu explicație |
| starea după | `fk_pl_cod` **încă există**, coloanele ei încă `utf8` — cheia nu s-a pierdut; `fk_ist_tip` necreată |
| după curățarea datelor, `--run --mode FORCE` | 5 instrucțiuni: **5 reușite, 0 eșuate** |
| `--view` SAFE și FORCE, după | `000_demo: 0 instrucțiuni` — convergent |
| chei la final | toate 5, cele încrucișate spre `avacont_comun`, cele proprii spre `000_demo` |

Diacriticele au rămas intacte pe tot parcursul.

Mesajul care contează, așa cum îl vede operatorul:

```
Cheia `fk_ist_tip` nu poate fi creată: 3 rânduri din `000_demo`.`fx_istoric`
au în `TipRand` valori care nu există în `avacont_comun`.`defatiprand`
(de exemplu «X», «Z»). Datele trebuie corectate întâi.
```

## 4. Ce am aflat, și ce rămâne

- Cazul «charset ireconciliabil spre `AVACONT_COMUN`» **nu se poate construi**:
  cheia n-ar fi putut fi creată nici în sursă, tot din regula colaționării.
  Verificarea rămâne, dar e defensivă — poate ieși la iveală doar dacă
  `AVACONT_COMUN` e schimbată după ce cheile există.
- Verificarea costă un `COUNT(*)` per cheie de creat. Pe baze mari, pe coloane
  fără index, poate fi simțită. Nu s-a măsurat pe volum real.
- Rândurile amânate rămân în `schema_diff_log` cu `error_msg` și sunt
  regenerate la rularea următoare (`clear_pending` le șterge pe cele
  neexecutate). Nu se pierd, dar nici nu se reîncearcă singure.


---

# Pasul 0043-02 — eroarea 1054 de la prima rulare pe toate bazele

**Data:** 2026-08-20 (raportat de operator din rularea pe cele 22 de baze reale)

## Ce s-a întâmplat

Prima rulare adevărată, pe toate bazele din `CAI`, a mers 21 de baze și a
căzut pe a 22-a:

```
053_LTTR: 169 instrucțiuni.
ERROR - Eroare MariaDB: [1054] 1054 (42S22): Unknown column 'c.IdPartener' in 'where clause'
```

Eroarea era a verificării de integritate adăugate la pasul 0043-01. Aliasul
`c` este tabelul-copil din `_orphan_rows`: numărătoarea de rânduri orfane se
face pe **ținta așa cum e ACUM**, dar coloana `IdPartener` urma abia să fie
adăugată de acel lot, la prioritatea 7. Deci se cerea o coloană care încă nu
există.

Reprodus local pe 10.3.32 în ambele variante — coloana lipsă pe partea
copilului (`c.IdPartener`, cazul operatorului) și pe partea părintelui
(`p.Cod`).

**Corecția:** verificarea datelor rulează numai dacă ambele capete au deja
toate coloanele cerute. Când nu le au, nu se poate răspunde la întrebare, deci
nu se pune. Cheia se creează atunci fără verificare prealabilă; dacă datele nu
se potrivesc, eșecul apare la execuție, cu 1452, și se scrie în jurnal.

Verificarea prezenței se face din schemele deja citite în memorie (fără
interogare în plus), iar coloanele schemelor externe (`AVACONT_COMUN`) se
citesc o dată și se țin în `_ext_columns` — aceleași câteva tabele-părinte
erau întrebate iar și iar.

## De ce nu s-a salvat nimic în `schema_diff_log`

Ordinea din `generate()` era: golește rândurile neexecutate, apoi compară,
apoi scrie. Comparația a crăpat între golire și scriere, deci tabelul a rămas
gol — cele 693 de rânduri vechi șterse, cele noi niciodată scrise. Nu s-a
pierdut nimic de neînlocuit (se regenerează), dar operatorul rămâne fără nimic
de citit exact când are mai multă nevoie.

**Corecția:** comparația se face întâi; golirea, abia după ce există ceva de
pus în loc.

## Mesaje de eroare care spun unde să te uiți

`[1054] Unknown column 'c.IdPartener'`, în mijlocul a 22 de baze și ~3600 de
instrucțiuni, nu spune nimic. Verificarea fiecărei chei este acum învelită
astfel încât o eroare de-a ei să fie **relansată** (nu înghițită) cu numele
cheii și al bazei:

```
Verificarea cheii `fk_ctr_partener` de pe `101_CCDP`.`FX_Contracte` a eșuat: ...
```

## Rulări (măsurate)

| Rulare | Rezultat |
|---|---|
| scenariul 1054 reprodus, înainte de corecție | `Eroare MariaDB: [1054] Unknown column 'p.Cod'` |
| același, după corecție, `--run --mode FORCE` | 17 instrucțiuni: **17 reușite, 0 eșuate** |
| convergență, SAFE și FORCE | `000_demo: 0 instrucțiuni` |
| regresie — scenariul cu date murdare (0043-01) | cheia `fk_pl_cod` tot păstrată, `fk_ist_tip` tot refuzată cu valorile «X», «Z» |
| regresie — scenariul curat (0043) | 11 din 11 reușite, verificare finală OK |

## Găsit pe drum, NEREPARAT

Într-o variantă extremă a bazei de test (tabel-țintă fără nicio coloană comună
cu sursa) au ieșit două erori care **nu țin de această corecție**:

```
[errno=1072] Key column 'Cod' doesn't exist in table     <- PK MODIFY, prioritatea 3
[errno=1090] You can't delete all columns with ALTER TABLE
```

`PK MODIFY` stă la prioritatea 3, dar coloana pe care se construiește cheia
primară se adaugă abia la 7. `_pk_handled_by_add_column` acoperă doar cazul
`auto_increment`. Deci o cheie primară mutată pe o coloană **nouă** eșuează.
Nu a fost atins: e un defect al pasului de chei primare, nu al celui de chei
străine, și cere verificarea lui separată. De semnalat înainte de următoarea
rulare pe toate bazele.
