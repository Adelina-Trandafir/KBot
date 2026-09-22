# SLICE-0075-00 — `Clasificatii`: `Sector`, `Sursa` și `SS` devin coloane scrise

> **REVIZIA 2 (22.09.2026, după ce serverul a refuzat prima formă).** Ce scrie mai jos sub
> «Ce s-a schimbat și de ce» descrie **prima încercare**, în care `SS` rămânea generată ca
> `concat(<CASE sector>, Sursa)`. MariaDB 10.11 o refuză cu **eroarea 1901**, reprodusă
> într-un tabel NOU, deci nu e vina lui `ALTER`, e forma expresiei:
>
> ```
> Function or expression 'concat(case right(coalesce(`Capitol`,''),2) ... ,`Sursa`)'
> cannot be used in the GENERATED ALWAYS AS clause of `SS`
> ```
>
> O coloană generată STORED nu poate fi construită dintr-o altă coloană. Deci `SS` trebuie
> scrisă, iar `Sector` și `Sursa` — cele două jumătăți ale ei — o urmează. **Celelalte șase
> coloane generate rămân neatinse** (`Clsf`, `Titlu`, `ClsfSal`, `ClsfF`, `ClsfE`, `ClsfX`):
> sunt funcții pure de Capitol/Subcapitol/Articol/Alineat, pe care orice scriitor le dă deja,
> două dintre ele poartă chei străine proprii, iar generate **nu pot** să nu fie de acord cu
> coloanele de bază. Scrise, un `UPDATE` care schimbă `Capitol` și uită `ClsfF` ar da un rând
> care trece de toate cheile străine și totuși minte. Decizia operatorului, 22.09.2026.
>
> Ce aduce revizia, peste textul de mai jos:
> - `SS` e `varchar(3) NOT NULL` **fără implicit**: cine o omite primește `1364`, cine inventează
>   o valoare primește `1452`. Amândouă zgomotoase — exact opusul unui sector greșit în tăcere.
> - `PYTHON/routes/clasificatii_ss.py` (nou) ține singura regulă: `ss_values(capitol, sursa=None)`
>   → `(Sector, Sursa, SS)`, care reproduce **exact** vechea expresie generată când primește doar
>   capitolul. Cele șase locuri care scriau în `Clasificatii` se comportă deci ca înainte; doar
>   codul nou (provizionarea acestei felii, Migratorul care citește `Sursa` din Access) trimite
>   o literă adevărată și ajunge la celelalte unsprezece valori din `DefaSursaSector`.
> - Atinse: `routes/clasificatii.py` (două `INSERT` + upsert-ul, care primește cele trei coloane
>   ȘI în lista `ON DUPLICATE KEY UPDATE` — poate schimba `Capitol`) și `routes/nomenclatoare.py`.
> - `ColumnSourceKind.ClasificatieSursa` ▸ **`ClasificatieSursaSector`**, una pentru toate trei;
>   coloana-țintă alege valoarea. `SS` e `NOT NULL`, cu cheie străină, și **nu are coloană Access
>   cu acest nume**, deci fără mapare explicită fiecare `INSERT` al Migratorului ar muri cu `1364`.
> - `--clean-leftovers` (nou) curăță ce a lăsat o rulare eșuată, dar **numai** pe un tabel unde
>   toate trei sunt încă generate — adică unde nu s-a întâmplat nimic structural.
> - **Fereastra de instalare:** după migrare un `INSERT` fără `SS` pică; înainte de ea, unul *cu*
>   `SS` pică (scrierea într-o coloană generată e eroare). Deci rutele actualizate și migrarea
>   merg în aceeași fereastră de mentenanță; între ele, scrierile în `Clasificatii` pică, iar
>   citirile merg neatinse. Rutele acelea servesc sincronizarea Access/VBA, nu trafic continuu.
>
> Prima formă a ajuns pe server o dată, pe `AVACONT_SURSA`: `ALTER`-ul a picat la pregătire,
> deci **tabelul a rămas neatins**, copia de siguranță fusese luată, iar `_snap_clsf` și
> `Sursa_w` au rămas în urmă (de unde `--clean-leftovers`).

---

## Textul reviziei 1 (păstrat pentru istoric)

Prima trecere din felia 0075 (creare automată a unei baze de unitate, `docs/PLAN_AutoProvisioning.md`).
Rulează înaintea oricărei alte treceri: fără ea, pagina publică poate oferi cele 14 surse-sector,
dar serverul nu poate scrie decât trei.

## Ce s-a schimbat și de ce

`Clasificatii.Sector`, `Sursa` și `SS` erau GENERATE din `right(Capitol, 2)`, iar `SS` poartă
cheia străină spre `AVACONT_COMUN.DefaSursaSector`. CASE-ul cunoștea doar terminațiile de capitol
`00/01/02/10`, deci `SS` generat putea da **numai** `01A`, `02A` și `02E` — din cele 14 valori
reale ale dicționarului (`01A 01D 01E 01F 01G 02A 02C 02D 02E 02G 03A 04A 05A 08A`).
Litera sursei (A/C/D/E/F/G) **nu se poate citi din capitol**: `01A`, `01D`, `01F` și `01G` se
termină toate în `01`. Deci trebuie stocată. Decizia operatorului (D15/D19, 22.09.2026), cu două
condiții: **nicio schimbare de date în vreo bază existentă** și **fluxul `Clasificatii` al
Migratorului scrie noua coloană**.

Forma nouă: `Sursa char(1) NOT NULL DEFAULT 'A'` (scrisă), `Sector` rămâne generată cu CASE-ul
extins (`03/04/05/08` în plus), `SS = concat(<sector>, Sursa)` rămâne generată — deci cheia
străină funcționează neschimbată.

### Scriptul de migrare

`PYTHON/scripts/clasificatii_sursa.py`, rulat de mână (`python -m scripts.clasificatii_sursa`),
**nu** prin `proc_SchemaDiff_DDL`: un instrument de diferențe e unealta greșită pentru ștergerea
și recrearea unei coloane generate care poartă o cheie străină. Pe fiecare bază — întâi șablonul
`AVACONT_SURSA`, apoi fiecare `NNN_*` din `SCHEMATA`:

1. `mysqldump` doar pe `Clasificatii`, într-un fișier datat (copia de siguranță).
2. Instantaneu: `_snap_clsf (IDClsf, Sector, Sursa, SS)`.
3. `ADD COLUMN Sursa_w char(1) NULL` + `UPDATE ... SET Sursa_w = Sursa` — valorile se **copiază**,
   deci nimic nu depinde de felul în care MariaDB tratează conversia unei coloane generate
   (comportamentul e documentat pentru MySQL, NEVERIFICAT pentru MariaDB 10.11).
4. Un singur `ALTER TABLE`: `DROP FOREIGN KEY` pe SS, `DROP COLUMN SS`, `DROP COLUMN Sursa`,
   `CHANGE Sursa_w Sursa char(1) NOT NULL DEFAULT 'A'`, `MODIFY Sector` cu CASE-ul extins,
   `ADD COLUMN SS ... STORED`, `ADD KEY idx_SS`, `ADD CONSTRAINT Clasificatii__DefaSS`.
5. Verificare față de instantaneu: același număr de rânduri și **zero** rânduri cu `Sector`,
   `Sursa` sau `SS` diferit (`<=>`). Orice diferență oprește rularea, păstrează instantaneul și
   tipărește comanda de restaurare. Instantaneul se șterge doar după ce verificarea trece.

De ce niciun rând existent nu se poate schimba: azi trec de cheia străină doar terminațiile
`00/01/02/10` (orice alta dă `SS = ''`, pe care `DefaSursaSector` îl refuză), iar pentru acele
patru CASE-ul extins și `Sursa` copiată dau exact valorile vechi. Pasul 5 o **dovedește**, nu se
bazează pe argument.

Refuzuri (nimic nu se scrie): bază fără tabelul `Clasificatii` → sărită; bază deja migrată →
sărită; rămășițe ale unei rulări eșuate (`_snap_clsf` sau `Sursa_w`) → oprire; stare intermediară
(`Sursa` scrisă dar `Sector` pe CASE-ul vechi) → oprire; lipsă `mysqldump` → oprire.
`--dry-run` listează bazele, numărul de rânduri și starea fiecăreia, fără să scrie nimic.

### Migratorul (planul §12)

- `ClasificatieDerived`: `Sector` primește CASE-ul extins; `Sursa` e acum **valoarea scrisă**, nu
  una calculată; `NormalizeSursa(sursa, capitol)` ține regula — capitol `xx10` ⇒ `E` indiferent de
  fișier, altfel valoarea Access curățată, cu majuscule, primul caracter, iar gol ⇒ `A`.
- `ColumnSourceKind.ClasificatieSursa` (nou) + `ColumnMapping.ClasificatieSursa`: odată ce ținta
  devine scriptibilă, coloana Access `Sursa` ar începe să călătorească **singură**, prin potrivirea
  de nume — rezultatul bun din motivul greșit, și cu textul brut. Maparea explicită o revendică
  și o normalizează. `AccessColumn = "Sursa"` face ca `ColumnPlan` să o considere consumată.
- `TransferRunner.BuildValues`: ramura nouă citește `Sursa` ȘI `Capitol` din **același** rând.
- `Verifier` / `ClasificatieRow`: citesc coloana Access `Sursa` și o trimit mai departe, ca poarta
  pe `DefaSursaSector` să vadă `SS`-ul real. Un fișier fără coloană dă „", care se normalizează la `A`.
- `sql/AVACONT_SURSA.sql` adus la forma nouă.

## Fișiere atinse

| Fișier | Ce |
|---|---|
| `docs/PLAN_AutoProvisioning.md` | **nou** — planul, cu §0 (constatările Pasului 0) și D20–D23 |
| `PYTHON/scripts/__init__.py` | nou |
| `PYTHON/scripts/clasificatii_sursa.py` | nou — scriptul de migrare (rescris la revizia 2) |
| `PYTHON/routes/clasificatii_ss.py` | **nou (revizia 2)** — regula unică `ss_values` |
| `PYTHON/routes/clasificatii.py` | **revizia 2** — două `INSERT` + upsert-ul scriu Sector/Sursa/SS |
| `PYTHON/routes/nomenclatoare.py` | **revizia 2** — `INSERT`-ul scrie Sector/Sursa/SS |
| `PYTHON/tests/test_clasificatii_sursa.py` | nou — teste offline pentru script |
| `PYTHON/tests/test_clasificatii_ss.py` | **nou (revizia 2)** — teste pentru regula comună |
| `sql/AVACONT_SURSA.sql` | `Clasificatii`: `Sursa` scrisă, `Sector`/`SS` cu CASE extins |
| `src/KBot.Migrator/Transfer/ClasificatieDerived.vb` | `NormalizeSursa`, `Sursa` scrisă, CASE extins |
| `src/KBot.Migrator/Transfer/ColumnMapping.vb` | `ColumnSourceKind.ClasificatieSursa` + fabrica |
| `src/KBot.Migrator/Transfer/TableMaps.vb` | maparea explicită pe `Clasificatii` |
| `src/KBot.Migrator/Transfer/TransferRunner.vb` | ramura de valoare |
| `src/KBot.Migrator/Transfer/Verifier.vb` | citirea coloanei Access `Sursa` |
| `src/KBot.Migrator/KBot.Migrator.vbproj` | FileVersion 1.10 ▸ **1.11** |
| `tests/KBot.Migrator.Tests/*` | **proiect de teste nou** (nu exista niciunul pentru Migrator) |
| `docs/MAPARE_NOMENCLATOARE.md` | nouă ▸ opt coloane generate; cele două corecturi de pe server |
| `KBot.sln` | proiectul de teste adăugat în folderul «Tests» |

## Rezultatele testelor

- `dotnet build src\KBot.Migrator --no-incremental`: **0 erori, 0 avertismente** (și după revizia 2).
- `ast.parse` pe toate fișierele Python atinse: curat.
- `ss_values` / `derive_ss` verificate prin apel direct: `65.01`▸`01A`, `65.10`▸`02E`,
  `65.02`▸`02A`, `65.99`▸`A`, `('65.01','F')`▸`('01','F','01F')`.
- Suitele **nu au fost rulate** (regula casei: testele se scriu, nu se rulează).

## Neverificat / amânat

- **Migrarea nu a rulat cu succes pe niciun MariaDB.** Prima formă a picat pe `AVACONT_SURSA`
  cu 1901 fără să schimbe tabelul; forma a doua **nu a fost încercată deloc**. Dezvoltatorul nu
  are acces la server; operatorul rulează.
- **Rutele Python atinse n-au fost pornite.** `clasificatii.py` și `nomenclatoare.py` scriu acum
  zece coloane în loc de șapte; corectitudinea lor e dovedită doar de `ast.parse` și de testele
  scrise, nerulate.
- **`tests/KBot.Migrator.Tests` nu a fost compilat** (regula casei interzice `dotnet build tests*`).
  E un proiect nou, adăugat în soluție: la prima `dotnet build KBot.sln` a operatorului poate
  cere corecturi de compilare. De raportat.
- **Cauza exactă a lui 1901 rămâne nedemonstrată.** Se știe că `concat(<CASE>, <coloană>)` e
  refuzat într-o coloană generată STORED pe 10.11; nu s-a testat dacă un `concat('01', Sursa)`
  simplu trece. Nu blochează nimic — forma nouă nu creează nicio expresie generată.
- Decizii luate între timp (D24–D26 în plan): rolurile rămân cuvintele românești (`Contabil`
  acum, `Administrator`/`Director` mai târziu); `Unitati_Ani.CodProgram` urmează sectorul
  (`01`▸`0000002510`, `02`▸`0000000000`), precompletat și editabil de operator; conturile
  `_Contabil` dispar, fiecare utilizator e un cont pe e-mail.
- **Drepturile conturilor existente** (§13 din plan, trecerea 0075-06): conturile pe e-mail NU
  sunt SU — au `USAGE` global plus drepturi pe baza lor. Ce e greșit: `WITH GRANT OPTION` pe baza
  proprie și drepturile DDL (`CREATE`, `DROP`, `ALTER`, `CREATE ROUTINE`, `EVENT`, `TRIGGER`).
  SU adevărat au doar `Admin` (`ALL PRIVILEGES ON *.*`) și `AVACONT` (`SUPER`, `FILE`,
  `CREATE USER`, `SHUTDOWN`). În plus: `scavatarsoft@gmail.com` și `AVACONT` au **același hash
  de parolă**, iar `030_SCTC` și `050_GRSA` **n-au niciun cont**.
- nginx: **rezolvat** — `/etc/nginx/sites-available/avacont-ssl` are un singur `location /` spre
  `127.0.0.1:5009`, deci pagina publică și fișierele ei statice ajung la Flask fără nicio
  modificare de nginx.
- Componentele JS: **rezolvat** — `listener-tracker/`, `utils/z-max.js` (`ZIndexManager`),
  `utils/css.js` (`getClassNumericProperty`) și `css/treeview/*` + `css/combobox.css` există acum
  în `JS_COMPONENTS`. Atenție la felia 0075-04: importurile sunt `../../listener-tracker/…`, deci
  componentele trebuie servite din `static/js/components/<nume>/`, cu folderele comune în
  `static/js/`.
