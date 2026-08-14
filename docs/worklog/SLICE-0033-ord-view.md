# SLICE-0033 — vederea ORD (Ordonanțări), read-only + `GET /api/forexe/ord`

Ultima dintre cele trei vederi reale rămase (Istoric și DDF sunt gata). `MainForm.CreateView("ord")`
nu mai întoarce `PlaceholderView`.

Forma e cea a sub-vederilor DDF (`PLAN_DdfSubViews`, felia 0032): arbore în `split.Panel1`,
`KBotNavList` orizontal la dreapta, sub-pagini leneșe, editabile în designer, hrănite de părinte.
Părintele deține datele; paginile sunt proaste prin construcție (fără DI, deci cu constructor fără
parametri → se deschid în designerul Visual Studio).

## Ce s-a construit

### 1. Ruta nouă `GET /api/forexe/ord?cod=` — `PYTHON/routes/forexe/ord.py`

Aceeași formă de casă ca `ddf.py` / `plati.py`: `@require_session`, un singur parametru `cod`,
**fără** `db_name` / `id_unitate` (baza vine din sesiune — o bază = o unitate), `ensure_ascii=False`,
eroare de bază → 500 cu motiv în română, niciodată liste goale care să mintă operatorul.

Un singur drum dus-întors, două tablouri:

```json
{ "cod": "AAB2EF2MCP4",
  "ordonantari": [ { "idordp": 130, "nr_ord": 14, "data_ord": "2026-04-07",
                     "total_ord": 12345.67, "pdf": null,
                     "part_ang": false, "nume_partener": null,
                     "incarcat": false, "preluat": true } ],
  "linii":       [ { "idordtblp": 987, "idordp": 130, "clsf": "…", "descriere": "…",
                     "total_receptii": 0.0, "plati_ant": 0.0,
                     "valoare": 12345.67, "ramas": 0.0 } ] }
```

**Cele trei capcane `FX_ORD`, tratate explicit:**

1. **Cheile «…P».** Toate legăturile merg pe `IDORDP` / `IDORDTBLP` — cheile MariaDB reale;
   omonimele fără «P» sunt id-urile Access păstrate. Un port literal al join-ului din
   `qFX_MAIN_ORD_TBL` (`FX_ORD_TBL.IDORDPART = FX_ORD_PART.IDORDPART`) ar lega cheia greșită.
2. **`ClasificatiiG` nu există în MariaDB** — nu se atinge deloc.
3. **Inversiunea `IdClsf`.** În `FX_ORD_TBL`, MariaDB `IdClsf` e FK-ul către `Clasificatii`
   (id global/PY) și `IdClsfAcc` e id-ul Access — documentat în `routes/ord/sync_mdb_acc.py`
   liniile 6-8 și confirmat de `sync_acc_mdb.py`. Invers față de `FX_Indicatori`.

**Filtrul pe `CodAngajament` — direct pe `FX_ORD`, nu prin `FX_DDF`.** Access ajungea la cod prin
`FX_ORD.IDDF = FX_DDF.IDDF` (`qFX_ORD_TREE`), dar în MariaDB `FX_ORD.CodAngajament` e scris de
AMBELE căi de migrare cu `_strict_str_nonempty` (`commit.py:85`, `sync_acc_mdb.py:80`), deci e
populat prin contract, nu prin noroc. Ocolul prin `FX_DDF` ar fi adus și un fan-out (PK compus
`(IDDF, CUAL)`).

**Clasificația — AMÂNDOUĂ drumurile, într-un `COALESCE`.** Planul cerea „alege unul după o probă pe
date reale"; proba pe date reale **nu s-a putut face** (ruta n-a atins niciodată o bază vie). Deci:
se încearcă întâi drumul DIRECT (`Clasificatii.IDClsf = t.IdClsf`, cel documentat de sync-ul ORD și
identic cu ce face `ddf.py` pentru `FX_DDF_REV_SA`), și se cade pe drumul VERIFICAT în 0011-03
(`FX_Indicatori` pe `CodAI` → `Clasificatii.IdClsfAcc + IdUnitate`, cel folosit de `plati.py` /
`receptii.py`) când primul e NULL sau gol. Alternativa — să ghicim unul — reintra exact în capcana
«Clsf gol în producție» din 0011-03 / 0015. `FX_ORD_TBL` **nu** are coloană `Clsf` denormalizată
(vezi lista din `routes/ord/tbl.py`), deci nu există a treia variantă.
`descriere` = `Clasificatii.Denumire`, pe același `COALESCE`.

**`total_ord` = subinterogare scalară**, nu `JOIN … GROUP BY`: Access adună peste un join cu
`FX_ORD_PART`, unde mai mulți beneficiari umflă totalul (familia `aggOrd` / `aggRev`). La fel,
`linii` se filtrează prin `IN (SELECT …)`.

**`pdf` — ABATERE de la plan, motivată.** Planul cerea `FX_ORD.PDF` ca șir înregistrat. Coloana se
numește de fapt `CalePDF` (Access: `ArePDF` + `CalePDF`), și **nicio** rută de migrare nu o scrie —
exact ca cele patru coloane PDF ale DDF-ului, scoase deliberat la migrare. Offline nu se poate ști
dacă mai există în MariaDB, iar un `SELECT` pe o coloană inexistentă ar da 500 pe toată ruta. Soluția:
coloana se **probează o dată** (`information_schema`, memorat per bază) și câmpul se trimite doar când
există cu adevărat; altfel `None`. Oricum e DOAR UN SEMNAL — clientul își calculează singur calea și
o verifică pe discul lui.

`part_ang` / `nume_partener` vin din `FX_DDF` prin `o.IDDF`, ca subinterogări scalare cu `LIMIT 1`
(PK compus → un join ar dubla ordonanțarea). Nu se afișează: compun folderul PDF-ului.

Înregistrată în `routes/forexe/__init__.py` cu `from . import ord as ord_route` — importată simplu,
legarea `ord` ar umbri built-in-ul `ord()` în spațiul de nume al pachetului.

### 2. Clientul

* **DTO-uri de fir** (`KBot.Api/UpsertAngajamenteRequest.vb`): `GetOrdResponse` +
  `GetOrdHeaderRow` + `GetOrdLinieRow`. Numele proprietăților SUNT cheile JSON (snake_case
  verbatim, `PropertyNamingPolicy = Nothing`); snake_case-ul se oprește la graniță.
* **POCO-uri de domeniu** (`KBot.Domain/OrdInfo.vb`): `OrdHeaderRow` / `OrdLinieRow` / `OrdInfo`.
  `OrdHeaderRow.FolderPdf` refolosește `DdfAntet.NormalizeazaNume` — o singură regulă de
  normalizare peste ambele documente, nu două copii care pot diverge.
* **`ApiClient.GetOrdAsync` + `IApiClient`**: aceeași formă ca getter-ul DDF; 401 curge spre
  `WithReauth`, non-2xx → `ApiException` cu mesajul românesc din câmpul `error`.
* **`KBotPaths.OrdPdfRoot`** (implicit `C:\AVACONT\FOREXE\PDF\ORD\`) — proprietate + citire +
  scriere + DTO, sora lui `DdfPdfRoot`.

### 3. Vederea + paginile

* `Views\OrdView.vb` / `.Designer.vb` / `.resx` — `split` (Panel1 = `tree`, Panel2 = `pnlPages` +
  `navSub`), un `image_list` legat prin `tree.NodeImages`, `lblEmpty`. Încărcare cu un singur
  `GetOrdAsync` prin `WithReauth`, cu stale-guard pe `_requestedCod`; arbore lună → ordonanțare;
  click pe nod = filtrare locală + `PushToActivePage`; `Tree_CollapsedChanged` + `ClampSplitter`
  copiate din `RezervariView`.
* `Views\Ord\IOrdPage.vb` — contractul, **mai subțire** decât `IDdfPage`: ORD e read-only în felia
  asta, deci nicio pagină n-are ce ridica spre părinte (fără `GenerateRequested`, fără
  `FileActivated`). Când vine generarea, acolo se adaugă.
* `Views\Ord\OrdPageContext.vb`, `OrdVizualizarePage` (grilă, șase coloane autorite în designer),
  `OrdDocumentPage` (PDF real, montare leneșă cu gardă pe perechea `(cale, existență)`),
  `OrdPdfLocator`.
* `MainForm.CreateView("ord")` → `New OrdView(...)`. Intrarea de navigare era deja poartată de
  `AreORD` (felia 0008) — nimic de schimbat acolo.

**Convenția PDF a fost CITITĂ, nu ghicită** (`mdl_FX_ORD_PDF.md:261-286`):
`<root>\<partener | GENERAL>\ORD_NR_{NrORD}_{CodAngajament}.PDF`, cu partenerul luat din `FX_DDF`
prin `FX_ORD.IDDF` — de unde și `part_ang` / `nume_partener` pe firul rutei.

**O ciudățenie Access reținută, nu reprodusă:** în datele exportate există căi `ORD_NR_0_…`, fiindcă
VBA-ul ia numărul dintr-un dicționar populat DOAR pe ramura «toate documentele lunii»; pe ramura «un
singur document» dicționarul e gol și numărul iese 0. `OrdPdfLocator` compune întotdeauna cu `NrORD`-ul
real. Consecință onestă: pentru documentele vechi salvate cu «_0_», vederea va spune «nu există PDF».

**Pagina «Document» NU folosește suprafața „document lipsă" a lui `ReaderHostPreview`**, ci eticheta
ei proprie: aceea poartă un buton «Generează», iar generarea ORD e o felie ulterioară — un buton care
nu face nimic e mai rău decât niciun buton.

### 4. Amânat DELIBERAT (nu scăpări)

Generarea PDF-ului; pagina Atașamente (`FX_ORD_ATT`); pagina Documente (`FX_ORD_DOC`); pagina
Fișiere (browser de disc); gruparea pe beneficiar (`FX_ORD_PART`) în grilă; bifele de selecție
multiplă din formularul Access (ORD-plăți în lot).

## Fișiere atinse

| fișier | ce |
|---|---|
| `PYTHON/routes/forexe/ord.py` | **nou** — ruta |
| `PYTHON/routes/forexe/__init__.py` | înregistrarea rutei |
| `src/KBot.Api/UpsertAngajamenteRequest.vb` | +3 DTO-uri de fir |
| `src/KBot.Api/IApiClient.vb`, `ApiClient.vb` | `GetOrdAsync` |
| `src/KBot.Domain/OrdInfo.vb` | **nou** — POCO-urile |
| `src/KBot.Common/KBotPaths.vb` | `OrdPdfRoot` (+ implicit, citire, scriere, DTO) |
| `src/KBot.App/Views/OrdView.vb` / `.Designer.vb` / `.resx` | **noi** — vederea |
| `src/KBot.App/Views/Ord/IOrdPage.vb` | **nou** |
| `src/KBot.App/Views/Ord/OrdPageContext.vb` | **nou** |
| `src/KBot.App/Views/Ord/OrdPdfLocator.vb` | **nou** |
| `src/KBot.App/Views/Ord/OrdVizualizarePage.vb` / `.Designer.vb` | **noi** |
| `src/KBot.App/Views/Ord/OrdDocumentPage.vb` / `.Designer.vb` | **noi** |
| `src/KBot.App/MainForm.vb` | `CreateView("ord")` |
| `tests/KBot.App.Tests/*.vb` (7 fișiere) | ciotul `GetOrdAsync` în dublurile de `IApiClient` |

Cele șapte fișiere de test au primit DOAR ciotul cerut de contract (`Throw New
NotSupportedException()`), ca și celelalte metode neexercitate — **niciun test nou**, conform
planului §11.

## Verificat

* `dotnet build KBot.sln` — **0 erori**. **10 avertismente**, toate `MSB3825`
  (BinaryFormatter / `ImageStream` în `.resx`): 8 preexistente pe `DdfView` / `PlatiView` /
  `ReceptiiView` / `RezervariView`, +2 aduse de `OrdView.resx`, adică ACEEAȘI familie structurală
  pe care o are orice vedere cu `ImageList` autorit în designer. Nu «0 avertismente», cum cerea
  planul §15 — și nu se poate ajunge acolo fără a atinge toate vederile.
* `python -m pytest tests` (venv-ul `PYTHON/.venv`) — **75 trecute, 15 sărite**, la fel ca înainte.
* Ruta se ÎNREGISTREAZĂ: cu un `config.py` de umplutură pe `PYTHONPATH` (config-ul real lipsește
  de pe mașina asta), `forexe_bp` produce `['/api/forexe/ord']`.
* `py_compile` curat pe `ord.py` + `__init__.py`.

## NEVERIFICAT — de citit înainte de a te baza pe felia asta

1. **Ruta n-a atins niciodată o bază vie.** SQL-ul e scris după schema din codul de migrare și
   după exportul Access, nu rulat. În particular:
   * care ramură a `COALESCE`-ului de clasificație câștigă pe date reale (vezi mai sus);
   * dacă `FX_ORD.CalePDF` mai există în MariaDB (proba spune, dar proba n-a rulat live);
   * dacă `information_schema` e citibil cu utilizatorul aplicației.
2. **Nimic nu a fost randat pe ecran.** Nici vederea, nici cele două pagini. Dus-întorsul prin
   designerul Visual Studio (planul §13) e pasul operatorului — `OrdView`, `OrdVizualizarePage`,
   `OrdDocumentPage`.
3. **Calea PDF n-a fost confirmată pe un disc real de client.** Convenția e citită din VBA, nu
   probată contra unui fișier existent.
4. **Iconițele pornesc de la setul lui `RezervariView`** (`OrdView.resx` e o copie a lui
   `RezervariView.resx`), cu cheile `month` / `up` / `down` / `neutral` (+ `plus`, nefolosit de ORD).
   Cheia `neutral` arată azi glifa «=» a Rezervărilor. Operatorul le poate schimba din designer;
   cheile rămân.
5. **Butoanele de sortare / bara de subsol a arborelui** n-au handler aici (ca în celelalte vederi).
