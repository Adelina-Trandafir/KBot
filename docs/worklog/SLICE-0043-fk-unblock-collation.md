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
