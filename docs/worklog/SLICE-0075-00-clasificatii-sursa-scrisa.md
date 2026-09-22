# SLICE-0075-00 — `Clasificatii.Sursa` devine coloană scrisă

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
| `PYTHON/scripts/clasificatii_sursa.py` | nou — scriptul de migrare |
| `PYTHON/tests/test_clasificatii_sursa.py` | nou — 20 de teste offline |
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

- `dotnet build src\KBot.Migrator --no-incremental`: **0 erori, 0 avertismente**.
- `ast.parse` pe cele trei fișiere Python noi: curat.
- Suitele **nu au fost rulate** (regula casei: testele se scriu, nu se rulează).

## Neverificat / amânat

- **Nimic nu a atins un MariaDB.** Scriptul nu a rulat niciodată, în niciun mod — nici `--dry-run`.
  Dezvoltatorul nu are acces la server; operatorul rulează.
- **`tests/KBot.Migrator.Tests` nu a fost compilat** (regula casei interzice `dotnet build tests*`).
  E un proiect nou, adăugat în soluție: la prima `dotnet build KBot.sln` a operatorului poate
  cere corecturi de compilare. De raportat.
- **ALTER-ul dintr-o bucată e NEVERIFICAT pe MariaDB 10.11**: ștergerea lui `Sursa` în aceeași
  instrucțiune cu ștergerea și re-adăugarea lui `SS` (a cărui expresie inline conține același
  CASE, nu coloana). Dacă serverul refuză, `--split-alter` face aceiași pași în trei instrucțiuni;
  între ele tabelul stă câteva secunde fără `SS`, iar copia de siguranță acoperă intervalul.
- Cele 3 rânduri `Unitati_Utilizatori` au `Rol = 'Contabil'`, iar D4 cere `'CO'` — de hotărât dacă
  se aliniază (un singur `UPDATE`), altfel utilizatorii noi vor avea alt rol decât cei vechi.
- `Unitati_Ani.CodProgram` pentru o unitate nouă: `000_DEMO` are `0000002510` și `0000000000`;
  până la alt ordin, trecerea 0075-03 va scrie `0000000000`.
- Neobținute încă (necesare la 0075-03/04/05): `SHOW GRANTS` pentru un cont de e-mail și unul
  `_Contabil`, blocul nginx al lui `kbot.avatarsoft.ro`, și cele patru piese lipsă ale
  componentelor JS (`listener-tracker-mixin.js`, `ZIndexManager`, `getClassNumericProperty`, CSS-ul).
