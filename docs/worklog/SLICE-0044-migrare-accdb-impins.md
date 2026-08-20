# SLICE-0044 — Migrarea prin fișier `.accdb` împins pe server

**Data:** 2026-08-20
**Felia:** 0044 (următorul număr liber din registru).
**Înlocuiește:** lanțul feliei 0042 (VBA scrie JSON → `KBot.Migrator` citește fișierele →
rutele de seed).

---

## Ce s-a schimbat și de ce

Forma cerută de operator: **un singur lanț, cu fișierul Access pe server**. Migratorul
pornește cu un formular de conectare, arată bazele de pe MariaDB și unitățile din registrul
AVACONT, împinge fișierul, cere analiza, arată ce nu e în regulă, și abia apoi are două
butoane — «Rulează» (doar pe curat) și «Forțează rularea» (doar dacă nu există probleme de
tip sau de dimensiune).

### Trei lucruri verificate înainte de a scrie o linie

Planul de pornire spunea «`pip install pyaccdb`». Verificat pe index:

1. **`pyaccdb` nu există pe PyPI.** `pip index versions pyaccdb` →
   *No matching distribution found*. (Nu s-a instalat nimic, nici local, nici pe server.)
2. **Fișierele chiar sunt criptate.** `C:\AVACONT\forexe\FX_2026.accdb` (28,7 MB) și
   `C:\AVACONT\cale.accdb` poartă parola `andreI`
   (`Surse/VBA_MIGRARE/mdl_FX_ExportSeed.bas:36`). Măsurat pe octeți: în `DB_STRUCT.accdb`,
   fără parolă, numele de tabele se citesc direct din fișier (42 potriviri în primii 6 MB);
   în cele două de mai sus, **zero**. Deci niciun cititor pur-Python (`access-parser`,
   `mdbtools`) nu le poate deschide așa cum sunt.
3. **28,7 MB > plafonul de 17 MB al serverului** (`main.py`, `MAX_CONTENT_LENGTH`). Deci
   împingerea nu poate fi un singur POST.

### Deciziile luate în consecință (alese de operator)

- **Decriptarea se face înainte de împingere**, în Access (Fișier ▸ Informații ▸ Decriptare
  bază de date), pe o copie. Serverul citește un fișier fără parolă. Alternativa cu Java
  (UCanAccess + jackcess-encrypt) a fost respinsă: n-ar fi cerut pasul manual, dar ar fi
  adus o mașină virtuală Java pe server.
- **Cititorul e `mdbtools`**, și anume `mdb-json`, nu `mdb-export`: CSV-ul lui `mdb-export`
  face NULL și șirul gol să arate identic, iar «NULL într-o coloană `NOT NULL`» e chiar una
  dintre constatările pe care trebuie să le vadă operatorul. `mdb-json` omite din obiect
  coloanele NULL, deci cele două rămân distincte.
- **Lista bazelor vine printr-o rută nouă**, `GET /api/migrare/baze`, care rulează
  `SHOW DATABASES` pe server (tiparul din `routes/admin.py:check_db`). Migratorul nu capătă
  nici driver MySQL, nici credențiale de bază de date.
- **Felia 0042 e înlocuită, nu dublată.** Fișierele ei
  (`ExportArtifacts/`, `Routing/`, `Run/`, `Api/SeedApiClient.vb`, `mdl_FX_ExportSeed.bas`)
  rămân în depozit și compilează, dar **nu mai sunt legate de nimic**: `MigratorForm` nu le
  mai atinge.

---

## Partea de server — `PYTHON/routes/migrare/`

| Fișier | Rol |
|---|---|
| `storage.py` | folderul fișierelor împinse + încărcarea în bucăți (4 MB, amprentă pe fiecare bucată ȘI pe fișierul întreg) |
| `accdb.py` | `mdb-tables` / `mdb-schema` / `mdb-json`; zero tabele = «cel mai probabil încă protejat prin parolă», cu numele fișierului |
| `tables.py` | cele 16 tabele și regula de rutare a fiecăruia — port al lui `SeedTables.vb` |
| `routing.py` | hărțile Cai/A/B/C/D/E și rutarea rândurilor — port al lui `RoutingMaps.vb` + `RowRouter.vb` |
| `validate.py` | schema țintei din `information_schema` + verificarea valoare-cu-valoare; produce raportul |
| `execute.py` | scrierea, `insert-if-absent`, cu aceleași reguli aplicate din nou rând cu rând |
| `jobs.py` | registru de lucrări în fundal (analiza și scrierea durează minute) |
| `migrare.py` | blueprintul: opt rute, gardă `X-Api-Key` |
| `README.md` | pasul din Access + instalarea pe Linux |

**Regulile vin din țintă, nu din Access.** MariaDB e cea care acceptă sau refuză rândul,
deci `information_schema` e singura sursă onestă pentru tip, lungime, nulabilitate și chei
străine. Tipul dedus de `mdb-schema` nu decide nimic; servește doar la raportarea coloanelor
care există în Access și lipsesc din țintă.

**Cele două clase de constatări** sunt tot mecanismul celor două butoane:

- `BLOCANT` — `TABEL_LIPSĂ`, `COLOANĂ_LIPSĂ`, `TIP`, `DIMENSIUNE`, `NUL_INTERZIS`. Cât timp
  există una, **niciun** buton nu pornește.
- `FORȚABIL` — `CHEIE_STRĂINĂ`, `ID_DDF_LIPSĂ`, `CHEIE_DUBLĂ`, `RUTARE`. «Rulează» stă
  oprit, «Forțează rularea» pornește și **sare** peste rândurile vinovate.

Regula e verificată de **trei** ori: în interfață, la `POST /api/migrare/rulare`, și încă o
dată în `execute.run()`. Interfața nu e singura pază.

Numărătoarea constatărilor e întreagă; doar lista de exemple e plafonată (25 per
tabel/coloană/fel), ca raportul să încapă pe ecran fără să mintă despre volum.

---

## Partea de client — `src/KBot.Migrator/`

| Fișier | Rol |
|---|---|
| `ConnectForm.vb` + `.Designer.vb` | **NOU** — formularul de pornire: adresa serverului, cheia API, și proba că amândouă sunt bune (lista bazelor) |
| `Registry/AvacontRegistry.vb` | **NOU** — citește `HKCU\Software\VB and VBA Program Settings\AVACONT`: DC, nume unitate, cod fiscal, `CaleUnitate`, anii din subcheile ISJ/LOCAL/REPUBLICAN. Doar citește |
| `Api/MigrareApiClient.vb` | **NOU** — cele opt rute + împingerea în bucăți cu SHA-256 |
| `MigratorForm.vb` + `.Designer.vb` | **REFĂCUT** — unitate/an/bază, cele două fișiere, împingere cu bară de progres, analiză, grila constatărilor, «Rulează» / «Forțează rularea» |
| `Program.vb` | `ConnectForm` înaintea lui `MigratorForm`; clientul trece mai departe |
| `KBot.Migrator.vbproj` | referință nouă la `KBot.Controls` (doar pentru `KBotToolTip`), `FileVersion` 1.0.0.0 → 1.1.0.0 |

De ce există formularul de conectare: fără el, prima greșeală de cheie sau de adresă ar
apărea abia la prima operație lungă, după ce operatorul a ales deja DC-ul, anul și fișierul.
Aici e o cerere mică, iar răspunsul ei **este** chiar lista din care se alege pe ecranul
următor.

Etichetele plutitoare sunt `KBotToolTip`, nu `System.Windows.Forms.ToolTip` (regula casei),
autorate în `.Designer.vb`, în română, cu diacritice literale.

Din .NET nu se referă în continuare **niciun** driver Access — nici OleDb, nici ACE, nici
COM. Motivul s-a schimbat (nu mai citim fișiere JSON, ci nu citim deloc), regula nu.

---

## Fișiere atinse

**Nou:**
- `PYTHON/routes/migrare/{__init__,storage,accdb,tables,routing,validate,execute,jobs,migrare}.py`
- `PYTHON/routes/migrare/README.md`
- `PYTHON/tests/test_migrare_validate.py`, `PYTHON/tests/test_migrare_routing.py`
- `src/KBot.Migrator/ConnectForm.vb`, `ConnectForm.Designer.vb`
- `src/KBot.Migrator/Registry/AvacontRegistry.vb`
- `src/KBot.Migrator/Api/MigrareApiClient.vb`
- `docs/worklog/SLICE-0044-migrare-accdb-impins.md`

**Modificat:**
- `PYTHON/main.py` — blueprintul `migrare_bp`
- `src/KBot.Migrator/MigratorForm.vb`, `MigratorForm.Designer.vb` — refăcute
- `src/KBot.Migrator/Program.vb`, `KBot.Migrator.vbproj`
- `docs/worklog/KBOT_STATUS.md`

**Neatins, dar de acum nelegat:** `src/KBot.Migrator/{ExportArtifacts,Routing,Run}/`,
`Api/SeedApiClient.vb`, `Surse/VBA_MIGRARE/mdl_FX_ExportSeed.bas`,
`POST /api/forexe/seed/*`.

---

## Rezultatele testelor

- `dotnet build KBot.sln` — **0 erori**. Cele 5 avertismente sunt vechi și nelegate
  (`MSB3825`, `BinaryFormatter` în cinci `.resx` din `KBot.App`).
- `dotnet build src/KBot.Migrator/KBot.Migrator.vbproj` — **0 avertismente, 0 erori**.
- `python -m pytest tests` (în `PYTHON/.venv`) — **117 trecute, 15 sărite** (cele sărite
  sunt cele care cer gazda: `config.py` + MariaDB viu). Dintre ele, 42 sunt noi:
  verificatorul de valori și routerul.

---

## Ce rămâne NEVERIFICAT

Nimic din felia asta nu a rulat pe date reale. Explicit:

1. **`mdbtools` nu a fost pornit niciodată pe fișierele astea.** Nu e instalat nici local
   (cerință: fără instalări pe stație), nici verificat pe server. Rămân de dovedit, pe un
   fișier decriptat real: că `mdb-json` există în pachetul distribuției, că `-D` e acceptat
   în forma folosită, că numele tabelelor cu `_` trec neatinse, și că Memo-urile mari
   (`FX_Receptii_IMG`, `FX_Rezervarii_IMG`) ies întregi.
2. **Analizatorul de `mdb-schema`** (`accdb.columns`) e scris după forma așteptată a DDL-ului
   `mysql` produs de mdbtools, nu după o ieșire reală. Dacă forma diferă, funcția aruncă
   explicit («nu s-au putut citi coloanele»), nu întoarce o listă goală.
3. **Nicio rută nouă nu a fost apelată** de un client viu, și niciun formular nu a fost
   văzut pe ecran. Migratorul e a treia felie la rând care e verde la compilare și nevăzută.
4. **Ipoteza care trebuie confirmată prima pe date reale:** că `FX_<an>.accdb` chiar poate
   purta rânduri pentru mai multe unități, deci că rutarea prin `[Cai]` e necesară. Pe disc
   există și fișiere per unitate (`FX_2026_gr35.accdb`, `FX_2026_sc29.accdb`). Modelul de
   acum acoperă ambele cazuri — un fișier cu o singură unitate se rutează întreg către un
   singur DC — dar dacă în practică toate fișierele sunt per unitate, `cale.accdb` devine
   un pas inutil de care merită scăpat.
5. **`FX_Extrase_F` se multiplică intenționat** (un fișier de extras poate purta linii
   pentru mai multe unități). Portat ca atare din felia 0042, unde la rândul lui nu fusese
   niciodată rulat.
6. **Rularea forțată nu scrie un CSV de respinse.** Rândurile sărite sunt numărate și apar
   în raportul analizei, dar nu există un fișier de descărcat cu ele. De adăugat dacă
   operatorul îl cere după prima rulare reală.
