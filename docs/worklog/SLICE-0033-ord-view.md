# SLICE-0033 — vederea ORD (Ordonanțări), read-only + `GET /api/forexe/ord`

Ultima dintre cele trei vederi reale rămase (Istoric și DDF sunt gata). `MainForm.CreateView("ord")` nu mai întoarce `PlaceholderView`.

Vederea urmează sub-vederile DDF (`PLAN_DdfSubViews`, 0032): arbore în `split.Panel1`, `KBotNavList` orizontal la dreapta și pagini leneșe, editabile în designer. Părintele deține/furnizează datele; paginile nu folosesc DI și au constructor fără parametri pentru designerul VS.

## Ce s-a construit

### 1. Ruta nouă `GET /api/forexe/ord?cod=` — `PYTHON/routes/forexe/ord.py`

Aceeași formă de casă ca `ddf.py` / `plati.py`: `@require_session`, un singur parametru `cod`, **fără** `db_name` / `id_unitate` (baza vine din sesiune — o bază = o unitate), `ensure_ascii=False`, eroare de bază → 500 cu motiv în română, niciodată liste goale care să mintă operatorul.

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

1. **Cheile «…P».** Toate legăturile merg pe `IDORDP` / `IDORDTBLP` — cheile MariaDB reale; omonimele fără «P» sunt id-urile Access păstrate. Un port literal al join-ului din `qFX_MAIN_ORD_TBL` (`FX_ORD_TBL.IDORDPART = FX_ORD_PART.IDORDPART`) ar lega cheia greșită.
2. **`ClasificatiiG` nu există în MariaDB** — nu se atinge deloc.
3. **Inversiunea `IdClsf`.** În `FX_ORD_TBL`, MariaDB `IdClsf` e FK-ul către `Clasificatii` (id global/PY) și `IdClsfAcc` e id-ul Access — documentat în `routes/ord/sync_mdb_acc.py` liniile 6-8 și confirmat de `sync_acc_mdb.py`. Invers față de `FX_Indicatori`.

**CodAngajament direct în FX_ORD:** ambele migrări îl scriu prin _strict_str_nonempty (commit.py:85, sync_acc_mdb.py:80). Înlocuiește joinul Access FX_ORD.IDDF=FX_DDF.IDDF din qFX_ORD_TREE și evită fan-out-ul PK compus FX_DDF(IDDF,CUAL).

**Clasificație COALESCE, alegere live cerută de plan încă neconfirmată:** întâi Clasificatii.IDClsf=t.IdClsf, documentat de sync ORD/folosit DDF SA. La NULL/gol, fallback verificat 0011-03: FX_Indicatori.CodAI → Clasificatii.IdClsfAcc+IdUnitate, ca Plăți/Recepții, evitând Clsf gol din 0011-03/0015. Fără al treilea drum: FX_ORD_TBL nu are Clsf denormalizat (routes/ord/tbl.py). descriere folosește Clasificatii.Denumire prin același COALESCE.

**`total_ord` folosește subinterogare scalară**, evitând `JOIN … GROUP BY` peste `FX_ORD_PART`, care în Access umfla totalul la mai mulți beneficiari (`aggOrd` / `aggRev`). `linii` se filtrează tot fără multiplicare, prin `IN (SELECT …)`.

**pdf, abatere:** coloana CalePDF, fără FX_ORD.PDF (Access ArePDF/CalePDF); nicio migrare nu o scrie, ca cele patru câmpuri PDF DDF. Existența MariaDB rămâne neconfirmabilă offline; probă information_schema o dată per bază, memorată, evită 500 când lipsește. Răspuns: valoarea coloanei sau None; numai semnal, clientul calculează/verifică propria cale locală.

`part_ang` / `nume_partener` vin din `FX_DDF` prin `o.IDDF`, ca subinterogări scalare cu `LIMIT 1` (PK compus → un join ar dubla ordonanțarea). Nu se afișează: compun folderul PDF-ului.

Înregistrată în `routes/forexe/__init__.py` cu `from . import ord as ord_route` — importată simplu, legarea `ord` ar umbri built-in-ul `ord()` în spațiul de nume al pachetului.

### 2. Clientul

* **DTO-uri de fir** (`KBot.Api/UpsertAngajamenteRequest.vb`): `GetOrdResponse` + `GetOrdHeaderRow` + `GetOrdLinieRow`. Numele proprietăților SUNT cheile JSON (snake_case verbatim, `PropertyNamingPolicy = Nothing`); snake_case-ul se oprește la graniță.
* **POCO-uri de domeniu** (`KBot.Domain/OrdInfo.vb`): `OrdHeaderRow` / `OrdLinieRow` / `OrdInfo`. `OrdHeaderRow.FolderPdf` refolosește `DdfAntet.NormalizeazaNume` — o singură regulă de normalizare peste ambele documente, nu două copii care pot diverge.
* **`ApiClient.GetOrdAsync` + `IApiClient`**: aceeași formă ca getter-ul DDF; 401 curge spre `WithReauth`, non-2xx → `ApiException` cu mesajul românesc din câmpul `error`.
* **`KBotPaths.OrdPdfRoot`** (implicit `C:\AVACONT\FOREXE\PDF\ORD\`) — proprietate + citire + scriere + DTO, sora lui `DdfPdfRoot`.

### 3. Vederea + paginile

* Views/OrdView.vb + Designer/resx: split.Panel1 tree, Panel2 pnlPages/navSub, image_list în NodeImages, lblEmpty. Un GetOrdAsync/WithReauth, stale-guard _requestedCod; arbore lună→ordonanțare, clic→filtru local/PushToActivePage. Tree_CollapsedChanged/ClampSplitter copiate RezervariView.
* Views/Ord/IOrdPage.vb: contract readonly mai mic decât IDdfPage, fără GenerateRequested/FileActivated spre părinte. Vor fi adăugate la generare.
* `Views\Ord\OrdPageContext.vb`, `OrdVizualizarePage` (grilă, șase coloane autorite în designer), `OrdDocumentPage` (PDF real, montare leneșă cu gardă pe perechea `(cale, existență)`), `OrdPdfLocator`.
* `MainForm.CreateView("ord")` → `New OrdView(...)`. Intrarea de navigare era deja poartată de `AreORD` (felia 0008) — nimic de schimbat acolo.

**Convenția PDF a fost CITITĂ, nu ghicită** (`mdl_FX_ORD_PDF.md:261-286`): `<root>\<partener | GENERAL>\ORD_NR_{NrORD}_{CodAngajament}.PDF`, cu partenerul luat din `FX_DDF` prin `FX_ORD.IDDF` — de unde și `part_ang` / `nume_partener` pe firul rutei.

**Diferență față de Access:** căile exportate `ORD_NR_0_…` provin din dicționarul numerelor, populat numai pentru «toate documentele lunii», gol pentru un singur document. `OrdPdfLocator` folosește mereu `NrORD` real; fișierele vechi cu `_0_` apar ca PDF absent.

**Pagina «Document» folosește propria etichetă pentru lipsa PDF**, deoarece suprafața `ReaderHostPreview` are «Generează», iar generarea ORD este amânată; nu se afișează un buton fără comportament.

### 4. Amânat DELIBERAT (nu scăpări)

Generarea PDF-ului; pagina Atașamente (`FX_ORD_ATT`); pagina Documente (`FX_ORD_DOC`); pagina Fișiere (browser de disc); gruparea pe beneficiar (`FX_ORD_PART`) în grilă; bifele de selecție multiplă din formularul Access (ORD-plăți în lot).

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

Cele șapte fișiere de test au primit DOAR ciotul cerut de contract (`Throw New NotSupportedException()`), ca și celelalte metode neexercitate — **niciun test nou**, conform planului §11.

## Verificat

* `dotnet build KBot.sln` — **0 erori**. **10 avertismente**, toate `MSB3825` (BinaryFormatter / `ImageStream` în `.resx`): 8 preexistente pe `DdfView` / `PlatiView` / `ReceptiiView` / `RezervariView`, +2 aduse de `OrdView.resx`, adică ACEEAȘI familie structurală pe care o are orice vedere cu `ImageList` autorit în designer. Nu «0 avertismente», cum cerea planul §15 — și nu se poate ajunge acolo fără a atinge toate vederile.
* `python -m pytest tests` (venv-ul `PYTHON/.venv`) — **75 trecute, 15 sărite**, la fel ca înainte.
* Ruta se ÎNREGISTREAZĂ: cu un `config.py` de umplutură pe `PYTHONPATH` (config-ul real lipsește de pe mașina asta), `forexe_bp` produce `['/api/forexe/ord']`.
* `py_compile` curat pe `ord.py` + `__init__.py`.

## NEVERIFICAT — de citit înainte de a te baza pe felia asta

1. **Ruta n-a atins niciodată o bază vie.** SQL-ul e scris după schema din codul de migrare și după exportul Access, nu rulat. În particular:
   * care ramură a `COALESCE`-ului de clasificație câștigă pe date reale (vezi mai sus);
   * dacă `FX_ORD.CalePDF` mai există în MariaDB (proba spune, dar proba n-a rulat live);
   * dacă `information_schema` e citibil cu utilizatorul aplicației.
2. **Nimic nu a fost randat pe ecran.** Nici vederea, nici cele două pagini. Dus-întorsul prin designerul Visual Studio (planul §13) e pasul operatorului — `OrdView`, `OrdVizualizarePage`, `OrdDocumentPage`.
3. **Calea PDF n-a fost confirmată pe un disc real de client.** Convenția e citită din VBA, nu probată contra unui fișier existent.
4. **Iconițele pornesc de la setul lui `RezervariView`** (`OrdView.resx` e o copie a lui `RezervariView.resx`), cu cheile `month` / `up` / `down` / `neutral` (+ `plus`, nefolosit de ORD). Cheia `neutral` arată azi glifa «=» a Rezervărilor. Operatorul le poate schimba din designer; cheile rămân.
5. **Butoanele de sortare / bara de subsol a arborelui** n-au handler aici (ca în celelalte vederi).
