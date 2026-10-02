# K-BOT — STATUS, work outside the slice system

Requests the operator made without a slice number, plus notes from `../KBOT_STATUS.md`
that belong to no single slice. New sliceless work is recorded HERE.

## Current focus (sliceless)

- **No slice (operator, 02.10.2026) — list without source filter when the tree is sorted by date; angajament name in
  the association window; Wicket monitor after a multi-thread run.** (1) `JobBuilder.BuildListaAngajamente(.., toateSursele)`
  sends `SURSA` empty when `TreeSortIsDate`. (2) `AsociereForm` title = «cod — denumire». (3) «Function
  `_wicketMonitorCallback` has been already registered»: `AdoptTabAsync` now takes the worker's Wicket/click/key monitors over
  (`TakeOverWicketMonitoring`) instead of installing them a second time. Build clean, nothing run; item 3 is a diagnosis from
  code + log, to confirm on a run. Help in 0000-39. See `SLICELESS-lista-sursa-asociere-denumire-wicket.md`.

- **No slice (operator, 01.10.2026) — the «Setări» button leaves the main caption bar; its two rows move
  into the header menu (MENIU).** `capBar.ShowOptionsButton` off; `menuNou` rows `setari`
  («Configurare K-BOT») and `jurnal` («Jurnal activitate») wired in `MenuNou_ItemClicked`; the log row
  follows `FeatureSwitches.VizualizatorJurnaleActiv`. Build clean, nothing run. Help in 0000-29
  (`SLICE-0000-29-meniu-jurnal-setari.md`).

- **No slice (operator, 30.09.2026) — «Reanalizează rezervările».** Route `POST /api/forexe/rezervari/reanaliza` + footer-menu entry: replays FX_Istoric to fix Rez_Ord/TipRand/Val_Rezervare_Ant/Dif and R_Anterioara/R_Valoare. Code done, nothing run. See `SLICELESS-rezervari-reanaliza.md`.
- **MOVED TO SLICE 0091 (same day)** — the cause turned out to be a cut `Detaliu` scrape, see
  `SLICE-0091-detaliu-taiat-viteza-asteptari.md`; the note below is kept as it was written.
- **No slice (operator's request, 29.09.2026) — F14 PAUSED.** Run 34 (017_SCNB,
  AAB2DH3X6SK) was refused by F14: the snapshot of 12.01.2026 names AA2 (at 0), reception 84's
  `RHR` has no AA2. Receptions 86 (RHR = only AA6=0 against 286417.00) and 90 have the same
  gap, so the `RHR` lines are what is incomplete, not the snapshots. F14 now only writes a
  journal line + a warning: `F14_PAUSED = True` in `PYTHON/routes/forexe/prelucrare_asociere.py`
  (both the ingest and the any-time association editor go through `valideaza_plasarile`) and
  `F14Paused = True` in `src/KBot.App/Forexe/AsociereForm.vb` (`MotivulRefuzului`, drag veto).
  F16 and F15 are unchanged, so reception 86 would still be refused by F15 (its lines do not
  add up to its header) until its `RHR` is fixed. **Who writes `RHR`:** (a) the Access
  migration (`routes/migrare`), (b) step 4b `step4b_receptii_prelucrare` from
  `ListaReceptii[i].Detaliu` — it only INSERTs missing indicators and UPDATEs changed values,
  never deletes, so a line absent from `RHR` was never in any `Detaliu` that arrived (or the
  row predates K-BOT), (c) reconstituted receptions (not the case here). **Unverified:** what
  `Detaliu` actually carried for these rows — the payload is not in the run log. Build clean;
  no tests run (operator rule). To restore: set both flags back to False.

- **No slice (operator's request, 28.09.2026) — silent FOREXE downloads + the FOREXE page
  pictures reach the server.** Not given a slice number on purpose; recorded only here.
  (1) **Silent on success:** a download from forexecab shows NO box when everything is fine
  (counts, «Descărcarea a fost salvată», «Sincronizare reușită», «Nu există extrase noi»,
  the extrase import summary); the text goes to `Logs\mesaje_operator.log` through
  `OperatorLog.Write`. A box stays only for errors, server warnings, the empty package and the
  questions (`KbotForm.Download/Ingest/Console/Extrase.vb`). (2) **No history window** after a
  new or edited reception saved in the in-app browser (the Recepții view shows it) —
  `KbotForm.ForexeWatch.vb`. (3) **The pictures never reached the server — cause NOT found
  yet.** The server log shows no request at all on the capture routes, so the upload never
  left the client (the IMG keys DO have AUTO_INCREMENT — a first guess in that direction was
  wrong and reverted). The client log is on the client's PC, not here. To see it next time:
  `capturi.py` logs every request on ARRIVAL, refusals, INSERTs and duplicates under
  `[forexe.rezervari.img]` / `[forexe.receptii.img]`; `ddf_edit.py` / `ord_edit.py` log their
  SELECTs of those tables under the same tags; `pdf.py` logs every PDF download request (first
  download vs. re-check with the client's cached sha, and the 404 that was silent). On the
  client, a new `Logs\capturi_forexe.log` (`KBot.Common/Logging/CapturiLog.vb`, shows in the
  Setări log viewer) records every step: the marker given to the page (or why none), every
  picture taken / not taken / kept (path, code, kind, moment, number), each upload with its
  address and answer, each picture left on disk and why, the uploads skipped because the
  download failed or was not saved, «NU» at the reservations question, the delete answer.
  An upload that finds nothing for its code (case 1) now says so on the console too, and the
  log line lists everything waiting under `Capturi\` (a picture taken when the page showed no
  code lands in `fara_cod\`). FileVersion: Common 1.5.5.0, Forexe 1.0.18.0. (4) **Leftover pictures
  are also sent** after the node download (both kinds), the Recepții refresh (receptions) and the
  Rezervări refresh (reservations), only when the ingest saved. (5) **Delete on request**, like
  the signed PDFs that did not reach the server: pictures that still did not go are listed
  (kind, date, reason) with «Le ștergeți de pe acest calculator?» — Da deletes, Nu (default)
  keeps them for the next download. A reservation session still in progress is not asked about
  (kept rows waiting for «DA», or today's «before» picture with no number yet). Build App
  **0 / 0**, `py_compile` green; nothing run, not seen on screen, no tests. **Next:**
  `capturi.py`, `pdf.py`, `ddf_edit.py`, `ord_edit.py` to the VPS, then the client's
  `harness_errors.log` + console from the next browser session (see Open threads).

## Open threads (sliceless)

- **FOREXE page pictures (no slice, 28.09.2026) — why they never left the client.** Seen
  28.09 15:10 (005_CEVM, AAB5T2585AE, DDF revision 305 found 0 pictures): EXPECTED — the
  reservation was saved outside the in-app browser, so no marker and no picture exist; K-BOT
  takes pictures only there. The operator's first report (new reservation + new reception
  made IN the in-app browser, no pictures) is still unexplained. The
  server log has NO request on `/api/forexe/capturi/*`, so the client stopped before sending.
  The client log is on the client's PC. Places in the client where nothing is sent:
  (a) no file in `Capturi\<cod>\` for that angajament and kind — the picture was not taken
  (`IsConnected` false, or the capture failed: said only on the console) or was kept under
  ANOTHER code (the reservation «before» picture takes the code from the page, the upload
  uses the event's code); (b) the picture has no marker number (`Marcaj = 0`: the marker was
  not reserved, or was reserved under another code); (c) the ingest did not save (placement
  form closed, or «NU» at «Ați terminat modificarea rezervărilor?»). With the new client build
  all three are written to `Logs\capturi_forexe.log`; (a) and (c) also on the console, (b) in
  the deletion question. **To do:** (1) `capturi.py`, `pdf.py`, `ddf_edit.py`, `ord_edit.py` to the VPS +
  restart gunicorn; (2) on the client's PC after the next browser session:
  `Logs\capturi_forexe.log` (+ `harness_errors.log`), and the server log grepped for
  `[forexe.rezervari.img]` / `[forexe.receptii.img]`.

- **Datele comerciale ale instalării (demo / înregistrată) — fără sursă pe server.** Pagina «Informații» din «Setări» are rândul «Tip instalare» și spune că restul e în lucru; când va exista un model de licență pe server, acolo se leagă.

- **Rutele Flask de migrare (`PYTHON/routes/migrare/`) poartă acum un model de rutare care
  CONTRAZICE `KBot.Migrator`.** Felia 0046 a mutat proprietatea rândului pe autorități citite de
  jos în sus (`FX_DDF_REV_SA`, `FX_ORD_TBL`) și a declarat `FX_DDF.IdUnitate`, `FX_Extrase.IdUnitate`
  și `FX_Extrase_H.IdUnitate` relicve; rutele Python n-au fost atinse și nu știu nimic din asta.
  **Nu sunt pe calea vie** — migrarea se face din `KBot.Migrator`, direct pe MariaDB — dar
  divergența e consemnată aici ca să fie găsită, nu descoperită.

- **⚠️ 11 teste ROȘII comise deliberat la 13.08.2026, ca să nu se piardă munca din arborele de
  lucru.** Niciunul nu ține de Plăți (0017-04 e 16/16 verde); soluția compilează cu 0 avertismente.
  Două grupuri, cu cauze diferite:
  - **Roșii DEJA pe `master`, în cod NEATINS de arborele de lucru** (deci regresie mai veche, nu
    din munca asta): `KBot.Domain.Tests.DdfInfoTests.EtichetaRevizie_*` (3) și
    `KBot.Api.Tests.ApiClientTests.GetDdf_FormatsRevisionLabel_WithSpacePadding_Not_Zeroes` (1).
    Toate patru se învârt în jurul aceleiași reguli: eticheta de revizie DDF se umple cu SPAȚII,
    nu cu zerouri. Cineva a schimbat regula fără să atingă testele, sau invers.
  - **Roșii din munca necomisă până acum, pe vederile trecute prin designer**:
    `IstoricViewTests` (4), `DdfViewTests` (2), `MainFormNavItemsTests.Designer_WroteLiteralDiacritics_NotEscapes` (1).
    `IstoricView.Designer.vb`, `DdfView.Designer.vb` și `MainForm.Designer.vb` au fost toate
    regenerate de designerul Visual Studio. **Verifică întâi dacă n-au pățit ce a pățit
    `PlatiView`** (vezi 0017-04): o regenerare a lui `InitializeComponent` îi ștersese cele zece
    apeluri `InitDetailPair`, lăsând panoul de detaliu gol fără niciun zgomot. Testul de
    diacritice sugerează în plus că designerul a rescris șiruri românești ca `\uXXXX`.
  - **Remăsurate la felia 0031-01 (14.08.2026): tot 11, aceleași nume.** Mulțimea numelor picate e
    IDENTICĂ înainte și după acea felie (comparată cu `diff`, ieșire goală), deci firul ăsta e încă
    deschis exact cum a fost lăsat, iar 0031-01 nu l-a atins nici într-un sens, nici în celălalt.
    Rămâne valabil și avertismentul de acolo: cine repară grupul DDF ar trebui să înceapă de la
    regula unică (umplere cu SPAȚII, nu zerouri), fiindcă un singur defect se vede în Domain, Api
    și App deodată.

- **Cod livrat FĂRĂ felie și fără worklog (constatat la comiterea din 10.08.2026).** Arborele de
  lucru conținea, pe lângă feliile 0028…0028-04, un corp de muncă pe care NICIUN worklog și
  niciun rând din registru nu-l descrie: editorul de teme (`KBot.Controls/ThemeEditor/` —
  `ThemeEditorForm`, `ThemeScope`, `ControlStyleProxy`), magazia de suprascrieri de temă
  (`KBot.Theming/Overrides/` + `DesignerBaseline.vb`), schema **Colorful**, `CustomPopup`
  (`KBot.Controls/Popup/`), `KBotComboBox` (`KBot.Controls/Combo/`) și butonul de opțiuni al
  barei de titlu — cu teste proprii, toate verzi. A fost comis ca atare, ca să nu se piardă, dar
  **nu e documentat de nimeni**: ce a cerut operatorul, ce decizii s-au luat și ce a rămas
  neverificat nu se pot reconstitui din cod. Cine reia zona scrie întâi worklog-urile lipsă
  (numere libere de la 0029) și abia apoi le modifică. `SLICE-0028-03` se sprijină deja pe ele
  (`DesignerBaseline.Restore`, «cele patru scheme compilate»), deci golul e vizibil din felie.

- **Cele trei chei primare ale extraselor au intrat pe lista AUTO_INCREMENT** (08.09.2026): `FX_Extrase_F.IDEXF`, `FX_Extrase_H.IDEXH`, `FX_Extrase.IDFXE` — șapte perechi au devenit **zece**, în `AutoIncrementStep.Targets` (KBot.Migrator) ȘI în `schema_common.EXEMPT_COLUMNS` (cele două descriu o singură decizie și se schimbă împreună; pinul din `test_schema_sync_exempt_columns.py` le verifică una față de alta). Era obligatoriu: `routes/forexe/extrase.py` leagă rândurile copil prin `cursor.lastrowid`, care pe o cheie INT simplă răspunde `0`, iar ruta refuză zgomotos un `0` — deci pe o bază migrată importul nu putea rula deloc. Transferul din Access NU e afectat: id-urile călătoresc verbatim și sunt AutoNumber în Access (`mdl_FX_Extrase` le citește înapoi după `.Update`, nu le atribuie), deci nu sunt niciodată `0` sau `NULL` — singurul caz în care MariaDB ar inventa o cheie. **Ordinea rămâne garda:** creare din `AVACONT_SURSA` ▸ migrare ▸ verificare ▸ `ALTER`; cheile NU se convertesc cu mâna înaintea unei migrări. **`ALTER`-ul n-a rulat încă pe nicio bază.**

- `_upload_sessions` (routes/ftp.py) is still an in-process dict → blocks multi-worker.

- The rate limiter's counters are also in-process → restart clears lockouts; also blocks
  multi-worker. Both must move to Redis before `workers > 1` is possible.

- **No MariaDB DDL for `FX_DDF`, `FX_ORD`, `FX_DDF_REV_SA`, `FX_Receptii_H`** — five of
  the nine tree flags read them and `DDL_FX_ListaAngajamente.sql` creates none of them.
  The operator confirms all four exist live (2026-07-15); the endpoint hard-fails if one
  does not. Add their DDL so the repo stops disagreeing with production.

- **The two FX_Angajamente DDLs contradict each other**: `DDL_FX_ListaAngajamente.sql:42`
  has `ASCUNS` and `IdUnitate`, `docs/FX_Angajamente.sql` has neither. Operator confirms
  `ASCUNS` exists live. Pick one file as canonical and delete the other.

- The Access export used for the tree contract is the **repo copy**
  (`FX_System_Export/QUERIES/`); nobody has confirmed it matches the live Access file, or
  `C:\AVACONT\FX_System_Export`.

- `GET /api/forexe/angajamente` + `IApiClient.GetAngajamenteAsync` have **no caller left**
  after 0008 (the tree replaced them on MainForm's load path). Retire or find them a use.

- Server housekeeping: kernel reboot, systemd hardening, de-root the service user (blocked
  by AvacontPush SSH dependency), close stray ufw port 5010.

- Naming ambiguity: `Unitati` exists in both `AVACONT_COMUN` and each per-unit DB with
  different columns.

- **`Clasificatii` has real duplicates on `(IdClsfAcc, IdUnitate)`** — on `000_DEMO`:
  `(75,79)`, `(75,84)`, `(75,90)`, `(75,92)`, `(75,93)`, each twice. Sumar is safe
  (scalar subquery + `LIMIT 1` = one row per indicator), so this does not block, but
  it needs triage: **if the duplicate rows differ by `Sursa`/`Sector`, a join
  dimension is MISSING** and `LIMIT 1` is silently choosing between two genuinely
  different classifications; if they are identical, they are data to clean. Any
  future view that needs the classification's `Sursa`/`Sector` must settle this first.

- **Ingestia FOREXE merge, dar durează prea mult — cronometru pus pe 09.09.2026, cauza
  încă NEIDENTIFICATĂ.** Operatorul: partea de preluare/transformare a datelor din
  FOREXE ține mult mai mult decât ar trebui. Nu s-a schimbat nimic din conducta de
  ingestie și nu s-a ghicit niciun vinovat — s-a pus un **jurnal separat de timpi**,
  `PYTHON/utils/timing.py`, care scrie în `forexe_timing.log` (NU în `api_server.log`,
  care rămâne cât era de vorbăreț: logger propriu, `propagate = False`). Măsoară pe
  etapă **total / sql / propriu**, numără fiecare enunț SQL și adună formele repetate,
  deci arată dacă timpul stă în MariaDB (și în care enunț, rulat de câte ori) sau în
  Python. Cronometrate: `POST /api/forexe/prelucrare` (pașii 1–8, fiecare separat),
  `POST /api/forexe/extrase/import` (parsarea XML separat de scrieri) și
  `POST /api/forexe/angajamente/upsert`. Pornit implicit, oprit cu `KBOT_TIMING=0`.
  **Nu s-a rulat pe date reale** — nicio bază vie atinsă, deci concluzia despre
  gâtuire se citește din primul jurnal făcut pe calculatorul cu FOREXE.

---

## Old root `project_state.md` snapshot (last updated 21.07.2026)

Moved here verbatim when the root file was retired (28.09.2026). Historical only.

# K-BOT — project state (snapshot)

_Last updated: 2026-07-21._

High-level "where are we right now" snapshot for quick orientation. **The detailed,
authoritative record is [`docs/worklog/KBOT_STATUS.md`](docs/worklog/KBOT_STATUS.md)**
(slice registry + open threads) and the per-slice worklogs in `docs/worklog/`. When this
file and `KBOT_STATUS.md` disagree, `KBOT_STATUS.md` wins — fix it there first.

## What just landed (most recent first)

- **History read keeps its rows when FOREXE answers a page turn with the Wicket «Eroare» page** (uncommitted, 2026-10-02)
  - `ListenerInvocationNotAllowedException` on the history's «next» link used to make the next page's table never
    appear -> `TimeoutException` -> whole job failed, rows lost. New `<ScrapeTable partialOnError="true">`
    (`ScrapeTableAction.PartialOnError`, `WorkflowParser`): when the read fails (or at the top of a page) AND the page
    shows «S-a produs o eroare» AND at least one page was read, the rows so far are saved to `saveTo` and the flow
    goes on (console warns it is incomplete). Any other failure, or no error page, still throws as before.
    Set on the `TabelIstoric` scrape of `Istoric Angajament`, `… REVERSE`, `Prelucrare Completa`, `… Reverse`.
  - Not run against FOREXE (no local log of the error exists; the probe text comes from the operator's screenshot).
    Open: what page FOREXE is left on after the error (the following «Înapoi» step is skipped when absent).

- **Rezervări — endpoint + `RezervariView`, slice 0014 complete** (uncommitted, 2026-07-21)
  - The **second real view** (after Sumar, 0011). `MainForm.CreateView("rezervari")` now returns a
    real `RezervariView` instead of a `PlaceholderView` — a **master/detail** mirroring Access
    `frmFX_MAIN_REZ`: a reservations tree on the left (month folders → (date, type) leaves) and a
    read-only `KBotDataView` on the right (Clsf / Credit bugetar / Rezervări inițiale / Rezervare
    curentă / Rezervări definitive). Worklog `docs/worklog/SLICE-0014-rezervari-view.md`.
  - Server: `GET /api/forexe/rezervari?cod=` — a **raw reader** (one row per `FX_Rezervari`); the
    client shapes both tree and grid so the row list isn't duplicated on the wire. Classification
    resolved through `FX_Indicatori` (the verified 0011-03 path — `IdClsf` = Access id), scalar
    subquery `LIMIT 1` + kept `IdUnitate` predicate; `LEFT JOIN FX_Indicatori` so a reservation
    never disappears for a missing label.
  - **The plan's biggest "open" question was answered from the Access source, not guessed:** the
    month-folder total is `SUM(R_Valoare)` — literally the `TOTALL` column of `qFX_REZERVARI_TREE`.
    Leaf value = `SUM(IIf(EInitiala, R_Initiala, R_Valoare))` (= `Suma` in `QFX_DDF_REZERVARI`);
    type derived client-side (Inițială > Mărire > Micșorare).
  - Tree icons are **GDI-drawn and palette-tinted** (`RezervariIcons`: «=»/«▲»/«▼» + «+»), not
    binary resources. «+» is **display-only** this slice (raises `AdaugaDdfCerut`, no subscriber);
    the `IncarcaRezervare`/DDF workflow (migration-plan item 7) is a later slice. Same
    `WithReauth(Of RezervariInfo)` + stale-guard as Sumar.
  - Api 36 → 42 (+6 `GetRezervariAsync`), App 22 → 30 (+8 view/shaping), +16 Python host-only tests
    (skip off-host). Build 0 warnings, full suite green.
- **`KBotDataView` — owner-drawn unbound grid, slice 0010 complete** (`00b0cc9` → `3d69e2a`, 2026-07-18…21)
  - A reusable, **unbound**, **virtualized**, owner-drawn grid mimicking an Access continuous
    form — the shared list widget for the real views. Built in seven passes (worklogs
    `docs/worklog/SLICE-0010-0{1..7}-*.md`); see `KBOT_STATUS.md` slice 0010 for the full detail.
  - **01 skeleton** (models + `IThemedControl` + themed header/body) · **02 render+virtualize**
    (frozen + scrolling bands, integer virtualization, scrollbars, Text/CheckBox) · **03** the
    other four column types (Combo/OptionButton/Button/ProgressBar) + `OptionGroup` exclusivity ·
    **04** three-level effective-enabled (`IsCellEnabled`/`IsRowEnabled`) + disabled rendering +
    conditional formatting · **05** selection + Access-style keyboard nav + click/toggle + header
    resize · **06** in-place editing (floating Text/Combo editor, `CellValidating` veto+coercion,
    corrected `IsDirty` = operator-edits-only) · **07** `ScrollByColumn`.
  - **Placement decision:** lives in `KBot.Controls` (next to `AdvancedTreeControl`) but gained a
    `KBot.Theming` ProjectReference (no cycle) so it self-themes via `IThemedControl`/`ThemePalette`
    — the original plan's "`KBotTheme` constants" don't exist (that's a ~9-slot Forexe façade).
  - Virtualization proven **headlessly**: painted-row count is identical at 5,000 and 50,000 rows.
  - Editing pass caught **2 real VB case-insensitivity bugs** (a param shadowing a same-named
    property → silent no-op): `HeaderText` (every column header had been `Nothing` since pass 01,
    invisible to headless tests) and `ProposedValue` (commit always wrote `Nothing`). Both fixed
    with `Me.` + regression tests. See the [[vbnet-case-insensitive-shadowing]] pattern.
- **`KBotDataView.ScrollByColumn`** (`1ad2ff5`, fix `3d69e2a`, 2026-07-21)
  - New property: horizontal scroll snaps to column edges (a whole column at a time) instead of
    per-pixel. Vertical is untouched (already row-quantised by virtualization).
  - Arrows / trough / wheel snap **directionally** (a small step still advances one full column).
  - **Thumb drag** scrolls **freely** while dragging and snaps to the **nearest** edge only on
    release — the first version snapped on every `ValueChanged` mid-drag, which made the thumb
    jitter ("refreshes horribly, trying to decide which column"); fixed via the `Scroll` event's
    ThumbTrack/EndScroll distinction. DevHarness got a «Derulare pe coloană» checkbox.
- **AdvancedTreeControl → design-time control** (uncommitted, 2026-07-21)
  - Same treatment `KBotDataView` got: class marked `<ToolboxItem(True)>` +
    `<DefaultProperty("HeaderCaption")>`, so it drops from the VS Toolbox onto a form.
  - Constructor is now design-time safe — the `TooltipPopup` (a real `Form`) is no longer
    created / handle-forced under `LicenseManager.UsageMode = Designtime`; it's built lazily
    at runtime instead (all callers already null-guard it).
  - Property grid organized via `<Category>`/`<Description>`/`<DefaultValue>` into
    _K-BOT Arbore_ + _· Culori / Antet / Căutare / Tooltip / Coloane_. Runtime-only members
    hidden (`SelectedNode`, `OldSelectedNode`, `Items`, resolved header `Image`s,
    `TooltipPopupHandle`, redundant `FontName`/`FontSize`).
  - Pure metadata + a designer-only guard — no runtime behavior change. Full suite green.
  - **Not verified in the actual VS designer** (can't drive it headless); mirrors the
    known-good DGV setup. Note: the control still has **no `Dispose` override** (timers,
    `_vScroll`, tooltip `Form` never torn down) — pre-existing, left as-is.
- **Tree UI polish + Internal Info popup** (`b8c558c`, 2026-07-17)
  - Left status icons now render in the flat list (flat mode forced `Expanded=True`, so the
    control drew the never-set `LeftIconOpen`; it now falls back to `LeftIconClosed`).
  - The hover refresh icon no longer overlaps the `CodAngajament` column — opt-in
    `AdvancedTreeControl.ReserveRightIconSpace` reserves the icon's width; MainForm opts in.
  - New **non-modal `InternalInfoForm`** (themed, borderless): shows every
    `AngajamentTreeInfo` field incl. all nine `Are*` flags for the selected node. Opened
    from the `ⓘ` button in the tree header; auto-refreshes on tree selection + has its own
    Reîmprospătează button.
- **Slice 0009 — tree orphan escape + client tests** (`14770ea`, 2026-07-17)
  - Server: `GET /api/forexe/tree` SS filter now keeps zero-indicator (orphan) angajamente
    visible (`EXISTS SS OR NOT EXISTS any indicators`).
  - Client: the `.NET` `GetTreeAsync` tests that slice 0008 never wrote (Api 26 → 30).
  - Slice 0009's Parts B/C/D were already shipped by slice 0008; see the worklog.

## What works today

- Auth / login (bearer token; DC + An/SS periods; 401 → re-login retry once).
- ListaAngajamente scrape → upsert (offline round-trip; live still needs the table + env).
- Theming engine (Classic / Dark / Modern, live switching).
- MainForm shell: header (unit, An/SS, Forexe dot), nav sidebar, tree card, view host,
  status bar (Istoric / **Sincronizare**).
- **Angajamente tree**: `GET /api/forexe/tree` bound in MainForm, An/SS-driven reload,
  `btnOpt` hidden toggle, per-node `Are*` gating of nav entries, status icons, and the
  Internal Info popup.
- **`KBotDataView`**: reusable owner-drawn grid (six column types, virtualized, themed,
  editable, `ScrollByColumn`). Consumed by the real views that have landed (Sumar, Rezervări).

## What's next / deferred

- **Seven of the real views are still `PlaceholderView`** (Indicatori, Istoric, Revizii,
  Partener, Recepții, Plăți, DDF, ORD) — **Sumar (0011) and Rezervări (0014) are now real.**
  For the rest, the Internal Info popup is still the way to see live per-node data.
- `btnSort` / `btnIstoric` are placeholder `MsgBox` stubs.
- `KBotNavList.SetItemVisible` (real hide vs. grey-out gating).
- The tree is a flat list, not a nested tree.
- `GET /api/forexe/angajamente` + `GetAngajamenteAsync` are now tree-unused (kept, no caller
  removed).

## Known-unverified (needs a real environment)

- **No part of the tree endpoint has run against a live database** — all route tests
  (incl. the two new orphan tests) are host-only and skip off-station.
- **The tree UI fixes above were not eyeballed in a running MainForm** (needs login +
  server + DB); they build clean and the full `.NET` suite is green, but confirm visually.
- **`KBotDataView`'s visual harness has never been run** — for any of the seven passes. Scroll
  smoothness (incl. `ScrollByColumn` thumb release), the WinForms key→handler path, resize drag,
  floating-editor placement and the actual colours are all unconfirmed. The tests cover the
  logic; the pixels don't. The blank-header bug (headers `Nothing` for five passes, caught only
  in pass 06) is the argument for running it: DevHarness (Debug start → «Nu») → Controls/UI →
  «KBotDataView — virtualizare + temă (5.000 × 20)».
- **The real views (Sumar 0011, Rezervări 0014) have never run against a live DB nor been
  rendered on screen.** Their endpoints (`/api/forexe/sumar`, `/api/forexe/rezervari`) have
  host-only tests that skip off-station; if `Clsf` comes back blank on every row, the cause is
  the join key, not the view (the 0011-03 trap). For Rezervări specifically: the month-total
  formula is correct-from-source, but the exact screenshot figures (Ian 1.091.940 / …) were not
  reproduced numerically (no data for that angajament), and click-to-filter on a tree node is
  tested only at the data level, not as real interaction.
- Missing MariaDB DDL for four `FX_*` flag tables; the two `FX_Angajamente` DDLs disagree
  on `ASCUNS` (see `KBOT_STATUS.md` open threads).

## Build / test

```powershell
dotnet build KBot.sln
dotnet test KBot.sln          # 219 green: Api 42, App 30, Controls 111, Theming 27, Common 7, Domain 1, LocalStore 1
# Python server tests via PYTHON\.venv; live-DB tests skip off-host
```
