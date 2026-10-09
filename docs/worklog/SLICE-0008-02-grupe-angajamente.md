# SLICE-0008-02 — Alias și grupe de angajamente

Cererea operatorului, 09.10.2026. Număr ales de operator: **0008-02** (sub-felie a arborelui de angajamente, 0008).

## What changed and why

Operatorul vrea să poată (1) da un **alias** fiecărui angajament și (2) să **grupeze** angajamentele (un
angajament poate fi în mai multe grupe), cu o fereastră de editare și un filtru al arborelui principal.

### Server

- `sql/0008_02_alias_grupe.sql` (**nerulat**): `FX_Angajamente.Alias varchar(255) NULL` și tabelul
  `FX_Angajamente_Grupe(IDGR, DenumireGrupa, CuloareGrupa, CodAngajament)`, cheie primară
  `(IDGR, CodAngajament)`, FK spre `FX_Angajamente` cu `ON DELETE CASCADE`. Fără `IdUnitate` (angajamentele
  nu sunt pe unitate — decizia operatorului). Denumirea și culoarea grupei se repetă pe rândurile ei (forma cerută).
- `PYTHON/routes/forexe/grupe.py` (nou, înregistrat în `routes/forexe/__init__.py`):
  - `GET /api/forexe/grupe` — grupele cu membri + angajamentele vizibile (fără ascunse/anulate/suspendate, toți anii)
    cu alias și indicatori (clasificație, denumire, SS);
  - `POST /api/forexe/grupe` — salvează o grupă întreagă (nume, culoare `#RRGGBB`, membri); IDGR nou = MAX+1 în tranzacție;
    refuză: nume gol, culoare invalidă, niciun angajament, cod inexistent (400), grupă dispărută (404), nume deja luat (409);
  - `POST /api/forexe/grupe/<idgr>/angajamente` — adaugă un angajament (drag and drop, imediat);
  - `POST /api/forexe/grupe/alias` — setează/șterge aliasul.
- `PYTHON/routes/forexe/tree.py`: coloana `Alias` pe răspuns; dacă coloana nu există încă în baza unității, tree.py pune `NULL`
  (ca la `DataActualizare`), deci arborele nu cade înainte de aplicarea SQL-ului.

### Client

- `KBot.Domain/GrupeAngajamente.vb` (modele), `AngajamentTreeInfo.AliasAng`.
- `KBot.Api/IGrupeApi.vb` + `ApiClient.Grupe.vb`; `ApiClient.vb` citește `Alias` din arbore (`AliasAng`, `JsonPropertyName("Alias")`).
- `KBot.Controls`: `AdvancedTreeControl` primește **aruncări din alte controale** (`ExternalDragOver` / `ExternalDropped`,
  `TreeExternalDragEventArgs`; același chenar și aceeași etichetă de refuz ca la tragerea dintre noduri); `KBotDataView.ColumnKeyAt(pt)` public
  (pereche cu `RowIndexAt`).
- `KBot.App/Views/GrupeForm.vb` + `.Designer.vb` + `GrupeUi.vb`: fereastra **modală** «Grupe de angajamente» (stilul
  Parteneri/Clasificații: `KBotShellForm`, caption bar, card, subsol cu «Ieșire» stânga / «Salvează» dreapta):
  - arbore stânga: «Angajamente negrupate» + grupele (pătrat de culoare, text în culoarea grupei, număr de angajamente); în subsol «+ Adăugare grupă»;
  - dreapta: denumire, culoare (`ColorDialog`, implicit negru), tabel (bifă, cod, denumire, nr. indicatori cu tooltip ce listează clasificațiile, alias editabil);
  - negrupate: rândurile se trag peste o grupă → salvat imediat; grupă existentă: doar membrii, toți bifați, debifat = dispare din listă (iese la «Salvează»);
    grupă nouă: toate nebifate; «Salvează» cere nume, culoare, ≥1 bifă;
  - aliasul se salvează la ieșirea din celulă (la eroare revine valoarea veche); modificări nesalvate → întrebare la schimbarea rândului / închidere.
- `KBot.App/KbotForm.Grupe.vb` + `KbotForm.Designer.vb` (`menuGrupe`): rând nou **«Grupe»** în meniul rotiței arborelui; deschide `menuGrupe` (KBotDropDownMenu)
  cu grupele alfabetic, fiecare cu iconiță-pătrat și `ForeColor` = culoarea grupei, separator, «Editează grupe...» (deschide `GrupeForm.ShowDialog`).
  Filtru: `KbotForm.Tree.vb` `PopulateTree` arată doar angajamentele grupei, caption = alias (altfel denumirea), `NodeForeColor` = culoarea grupei
  (rândurile cu «lanț neînchis» rămân roșii, `ColoreazaLanturiNeinchise` rulează după). Antetul arborelui spune «GRUPA: …».
  După editor, dacă s-a scris ceva: grupele se recitesc și arborele se reîncarcă (aliasurile vin de pe server).

### Ajutor (regula «orice schimbare vizuală intră în ajutor»)

`HelpContent/contabil/fereastra.md`: rând în «Lista angajamentelor», secțiune nouă «Grupele de angajamente» (+ `GrupeForm` în `screens:`, cuvinte-cheie),
etichetă `0008-02`. `Check-Help.ps1 -Coverage`: fără erori noi pe fișierele atinse (rămân cele vechi din `tutorials\*` și `0000-60`). `help-version.txt` neschimbat (`2026-10-09`).

## Files touched

`sql/0008_02_alias_grupe.sql` (nou) · `PYTHON/routes/forexe/grupe.py` (nou) · `PYTHON/routes/forexe/__init__.py` · `PYTHON/routes/forexe/tree.py` ·
`src/KBot.Domain/GrupeAngajamente.vb` (nou) · `src/KBot.Domain/AngajamentTreeInfo.vb` · `src/KBot.Api/IGrupeApi.vb` (nou) · `src/KBot.Api/ApiClient.Grupe.vb` (nou) ·
`src/KBot.Api/ApiClient.vb` · `src/KBot.Api/UpsertAngajamenteRequest.vb` · `src/KBot.Controls/Tree/AdvancedTreeControl.Drag.vb` ·
`src/KBot.Controls/DataView/KBotDataView.Input.vb` · `src/KBot.App/Views/GrupeForm.vb` / `.Designer.vb` / `GrupeUi.vb` (noi) ·
`src/KBot.App/KbotForm.Grupe.vb` (nou) · `src/KBot.App/KbotForm.Designer.vb` · `src/KBot.App/KbotForm.Tree.vb` · `src/KBot.App/KbotForm.TreeOptions.vb` ·
`src/KBot.App/HelpContent/contabil/fereastra.md`.

## Test results

- `dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: **0 erori, 0 avertismente**.
- `py_compile` pe `grupe.py` și `tree.py`: OK.
- Nu s-au scris și nu s-au rulat teste (regula operatorului). **Testele existente ale rutei arborelui** (`PYTHON/tests/test_forexe_tree.py`, dacă mock-uiesc rânduri cu
  număr fix de coloane) trebuie reverificate: rândul întors de `tree.py` are acum o coloană în plus (`Alias`, înainte de `LantNeinchis`).

## Left unverified or deferred

- **SQL-ul nu e rulat nicăieri.** De aplicat pe fiecare bază de unitate (000_DEMO prima) și pe AVACONT_SURSA, înainte de serverul nou. Fișierele Python nu sunt deployate.
- **Nimic văzut pe ecran și nimic rulat:** fereastra, drag and drop-ul grilă → arbore, tooltip-ul de pe «Indicatori», filtrul arborelui, meniul de grupe.
  Puncte de văzut: dacă `KBotDataView` lasă `MouseDown/MouseMove` să ajungă la gazdă înainte de logica proprie (drag-ul pornește din celulele ne-«Alias»);
  dacă `RemoveRowAt` din `BeginInvoke` la debifare se comportă bine; poziția `menuGrupe.ShowAt(tree, Cursor.Position)`.
- Decizie luată de mine (de confirmat): meniul «Grupe» se deschide dintr-un rând al meniului rotiței (acel meniu nu are subniveluri), ca meniu separat la mouse.
  Dacă ai vrut folderul în meniul MENIU (`btnMeniu`), se mută simplu.
- Adăugat fără să fi fost cerut: rândul **«Toate angajamentele»** în meniul de grupe, vizibil doar cât e aleasă o grupă (altfel nu s-ar putea ieși din filtru).
- **Nu există ștergere de grupă** (nu a fost cerută). O grupă nu poate rămâne fără angajamente la salvare, dar nici nu poate fi ștearsă.
- Angajamentele ascunse/anulate/suspendate nu apar în fereastră; dacă sunt într-o grupă, rămân în ea la «Salvează».
- Filtrul grupei nu se ține minte între rulări. Grupa aleasă se reîmprospătează din server când se deschide meniul.
- Mărimile ferestrei sunt la 96 dpi (`AutoScaleDimensions 96`); neverificate la 125/150%.
- Captură de ajutor nouă (de făcut de operator): fereastra «Grupe de angajamente». Nicio captură adăugată în text.
