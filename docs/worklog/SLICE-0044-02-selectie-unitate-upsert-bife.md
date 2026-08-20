# SLICE-0044-02 — `cale.accdb` scos cu totul, selecție pe unitate, upsert, bife pe tabele

## Ce s-a schimbat și de ce

Patru cereri ale operatorului, toate pe lanțul de migrare al feliei 0044.

### 1. `cale.accdb` nu mai există nicăieri

Tabelul `[Cai]` din `cale.accdb` spunea `IdUnitate → DC`. Aceeași pereche e
**în fișierul FOREXE însuși** — verificat în exportul Access
(`C:\AVACONT\FX_System_Export\TABLES`):

* `FX_Angajamente` are **și** `IdUnitate`, **și** `DC` (ex.: `75 → 045_CTER`);
* `FX_Indicatori` are `IdUnitate` lângă `CodAngajament`, deci completează
  angajamentele al căror rând n-are unitate.

Deci fișierul al doilea era ceremonie. `routing.py` a fost rescris: hărțile
A–E, `RoutingMaps`, `RowRouter`, `build_maps`, `resolve_plan` și
`distinct_units` au dispărut; în locul lor `build_plan()` construiește
mulțimile de chei ale unității alese. `storage.cai_file_name()` și felul de
fișier `"cai"` au fost scoase — `begin_upload` acceptă acum doar `"fx"`.

Pe partea .NET au fost șterse fișierele feliei 0042, singurul loc unde mai
trăia logica `[Cai]` în VB: `Routing/`, `Run/`, `ExportArtifacts/`,
`Api/SeedApiClient.vb`. **Nimic nu le mai referea** (verificat cu grep pe
`src/` și `tests/` înainte de ștergere): erau lanțul JSON înlocuit de 0044,
care doar compila. `AvacontRegistry.SuggestCaiPath` a plecat și el.

### 2. Un fișier poate purta mai multe unități; se scrie doar unitatea aleasă

Vechea regulă era «o unitate → totul în baza aleasă; mai multe → oprește dacă
n-ai `cale.accdb`». Noua regulă: fișierul poate purta oricâte unități, iar
rândurile unității bazei alese se scriu, restul rămân neatinse.

Fiecare familie de chei se ține **de două ori** (`KeySet.ours` / `.known`), ca
să nu se confunde două lucruri diferite:

| Rândul | Ce se întâmplă |
|---|---|
| al unității alese | se scrie |
| al altei unități din fișier | se sare **tăcut** — e normal, nu e problemă |
| cu o cheie care nu există nicăieri în fișier | constatare `SELECȚIE` (forțabilă), cu cheia primară și motivul |

Dacă baza aleasă nu apare pe niciun rând și fișierul are mai multe unități,
analiza **se oprește** cu unitățile și DC-urile numite — nu se cade înapoi pe
«tot în baza aleasă», fiindcă exact așa ar intra tăcut rândurile altei unități.
Fișierul cu o singură unitate merge în baza aleasă oricum ar fi scris DC-ul.
`FX_Extrase_F` nu se mai multiplică: un fișier de extras e al nostru dacă
măcar un antet din `FX_Extrase_H` al lui e al unității alese.

### 3. Upsert peste tot

`execute._write` scria `ON DUPLICATE KEY UPDATE <prima coloană> = <prima
coloană>` — auto-atribuire fără efect, deci un rând deja existent pe server
rămânea vechi pentru totdeauna. Acum se actualizează **toate** coloanele
comune, `= VALUES(...)`, cu coloanele de cheie primară scoase din listă (ele
identifică rândul). Numărătoarea folosește ce raportează MariaDB: 1 = inserat,
2 = chiar schimbat, 0 = deja identic → «scrise / actualizate / deja identice».

### 4. Listă de tabele cu bife, nebifate când Access n-are date

Rută nouă `POST /api/migrare/tabele` (lucrare, ca analiza): numără rândurile
fiecăruia dintre cele 16 tabele cu `accdb.count_rows` — numără linii, fără
`json.loads` și fără dicționare, deci mult mai ieftin decât analiza. Migratorul
arată lista cu bife; **un tabel fără rânduri (sau absent din fișier) se oferă
nebifat**, iar unul absent nici nu se poate bifa.

`analiza` și `rulare` primesc amândouă `tabele`. Lipsa câmpului = toate 16; o
listă **goală** e eroare, nu «toate»; un nume străin oprește cu mesaj;
`tables.selected()` le reașază în ordinea de scriere (părinții înaintea
copiilor). Un tabel neanalizat nu se poate scrie (`execute.run` verifică).
După analiză, coloana «Ale unității» se completează din raport și tabelele cu
zero rânduri ale unității se debifează singure.

## Fișiere atinse

**Server (`PYTHON/routes/migrare/`)**
* `routing.py` — rescris: `UnitPlan`, `KeySet`, `TableSelector`, `build_plan`
* `tables.py` — `routing`/`route_column` → `selection`/`key_column`,
  `FAN_OUT_EXTRAS` → `BY_EXTRAS`, `selected()`
* `validate.py` — `analyze(..., only=…)`, `F_RUTARE` → `F_SELECTIE`, contorul
  `rutate` → `ale_unității`, raportul întoarce și `tabele`
* `execute.py` — upsert real, `only=…`, contoare noi
* `storage.py` — fără `cai_file_name`, doar felul `"fx"`
* `accdb.py` — `count_rows()`
* `migrare.py` — rută nouă `/api/migrare/tabele`, `_tabele_cerute()`, fără
  `cai_path`
* `README.md` — secțiunea `cale.accdb` înlocuită cu «un fișier, mai multe
  unități» + «lista cu bife»; nota despre upsert

**Migrator (`src/KBot.Migrator/`)**
* `Api/MigrareApiClient.vb` — `PushAsync` fără `fel`, `StartInventarAsync` +
  `CitesteInventar`, `tabele` pe analiză și rulare, POCO-urile `TabelFisier` /
  `InventarFisier`
* `MigratorForm.vb` / `.Designer.vb` — panoul «Tabele de actualizat»
  (`dgvTabele` cu bifă / tabel / rânduri / ale unității), butonul «Citește
  tabelele», inventar automat după împingere, `ResetInventar`, mesajele
  rescrise
* `Registry/AvacontRegistry.vb` — `SuggestCaiPath` scos
* `KBot.Migrator.vbproj` — `FileVersion` 1.1.0.0 → **1.2.0.0**, comentariu
  actualizat
* **Șterse**: `Routing/`, `Run/`, `ExportArtifacts/`, `Api/SeedApiClient.vb`

**Teste**
* `PYTHON/tests/test_migrare_routing.py` — rescris pe noua regulă (33 teste,
  inclusiv `build_plan` cu `accdb.iter_rows` înlocuit)
* `PYTHON/tests/test_migrare_validate.py` — `F_RUTARE` → `F_SELECTIE`

## Rezultatele testelor

* `PYTHON/.venv -m pytest tests/` → **132 passed, 15 skipped** (skip-urile sunt
  cele care cer baza vie, de pe alt host).
* `dotnet build KBot.sln` → **0 erori**; cele 5 avertismente sunt MSB3825
  (BinaryFormatter în `.resx`-urile din `KBot.App`), preexistente și
  nelegate de felia asta. `KBot.Migrator` singur: **0 avertismente**.

## Rămas neverificat / amânat

* **Nimic n-a fost rulat pe date reale.** `mdbtools` n-a fost pornit niciodată,
  nicio rută n-a fost apelată de un client viu, formularul n-a fost văzut pe
  ecran — la fel ca la 0044. Panoul nou cu bife n-a fost deschis nici în
  designerul VS.
* `count_rows` presupune că `mdb-json` scoate **o linie per rând**; e forma pe
  care se bazează și `iter_rows`, dar numărul n-a fost comparat cu unul real.
* Cazul «rând fără `IdUnitate` într-un fișier cu mai multe unități» se
  raportează ca `SELECȚIE` forțabilă. Dacă pe date reale se dovedește frecvent
  (ex.: `FX_Extrase` fără unitate), regula merită discutată cu operatorul — nu
  schimbată tăcut.
* Ordinea în care operatorul bifează nu contează, dar **dependențele dintre
  tabele nu se verifică**: se poate bifa `FX_Rezervari` fără `FX_Angajamente`.
  Analiza va raporta cheile străine lipsă pe țintă, deci nu trece tăcut, dar un
  avertisment în interfață ar fi mai bun.
