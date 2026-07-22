# SLICE-0015-02 — Recepții: tooltip de recepție (reconciliere recepții/plăți)

**Data:** 2026-07-22
**Felia:** 0015 (Recepții) — pass 02, peste scheletul tree + LISTA din 0015-01.
**Depinde de:** 0015-01 (endpoint-ul trimite deja `plati` + `difh` + `sters_h`, deci NU
e nevoie de niciun apel de rețea nou).

---

## Ce s-a schimbat și de ce

Am adăugat **tooltip-ul de recepție** pe rădăcinile arborelui (`ReceptiiView`), oglindind
`NewRootPlatiTooltip` din `frmFX_MAIN_REC`. E singura agregare vie din formul Access
(frunzele pe clsf și tabelele-tooltip de nod/frunză sunt cod comentat — vezi 0015-01).

Per recepție (`IDRR`), în ordinea `DataR`, un tabel cu patru rânduri:

| Rând | Valoare |
|---|---|
| Data recepție | `DataR` |
| Recepții cumulate | `difhCum` — sumă rulantă a `Sum(DIFH)` pe recepție |
| Plăți cumulate | `platiCum` — `Sum(Suma)` pe plăți cu `DataPlata ≤ MaxDataH` |
| Diferență | `difhCum − platiCum`, **roșu dacă <0, albastru dacă >0** |

### Fidelitate față de sursă (finding 3, verificat în export)

- **`difhCum`**: `DIFH` e per antet, deci se însumează **anteturile DISTINCTE** ale
  recepției (o dată per `IDRH`), apoi se cumulează pe recepții în ordinea `DataR`.
  **Anteturile șterse sunt EXCLUSE** — `qFX_MAIN_REC_TT_DIFH` filtrează `Sters=False`,
  deși arborele (qFX_MAIN_REC_TREE) NU filtrează Sters. De aceea `sters_h` a fost purtat
  în rând din 0015-01.
- **`platiCum`**: cumulativ, **fără limită inferioară** — toate plățile cu `DataPlata ≤
  MaxDataH` al recepției curente (`MaxDataH` = max `DataH` pe anteturile recepției).
  Sursa plăților (`qFX_MAIN_REC_TT_PLATI`) nu filtrează nimic în plus.
- **`valAsoc` NU e folosit** — linia care l-ar acumula (`sAsocR`) e comentată în Access;
  `NewRootPlatiTooltip` are doar patru rânduri și ignoră `asocCum`.
- Anteturile și grila NU primesc tooltip în această pasă (tabelele de nod/frunză sunt
  dormante în Access).

### Cum e randat

Tooltip-ul se construiește ca **XML `<table>`** (același contract pe care Access îl emitea
cu `clsTT_Table.ToXml`) și se pune pe `TreeItem.Tooltip`. La hover, `AdvancedTreeControl`
detectează `IsTableXml` și îl randează cu `TooltipTableParser` + `TooltipPopup` (header
„Data recepție / Valoare" + 4 rânduri). Culoarea diferenței = `#CC0000` / `#0033CC` (roșu/
albastru), format acceptat de `AdvancedTreeControl.ParseColor`. Textul e XML-escapat.

Nu există endpoint nou: totul se calculează din `_rows` + `_plati` deja încărcate.
Reconstrucția pe schimbarea temei (`ApplyTheme` → `BuildTree` → `ComputeRootTooltips`)
reface și tooltip-urile.

## Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.App/Views/ReceptiiView.vb` | + `ComputeRootTooltips` + `BuildRootTooltipXml` (+ helperi `SumDistinctAntetDifh`, `MaxDate`, `AppendTtRow`, `XmlEscape`, clasa `ReceptieTtRow`); hook în `BuildTree` |
| `tests/KBot.App.Tests/ReceptiiViewTests.vb` | + 2 teste (tooltip = `<table>` cumulat; diferență negativă = roșu) |
| `docs/worklog/SLICE-0015-02-receptii-root-tooltip.md` | **NOU** — acest worklog |
| `docs/worklog/KBOT_STATUS.md` | rândul 0015 marcat cu 0015-02 DONE |

## Rezultate teste

- **.NET:** `dotnet build KBot.sln` — **0 warnings, 0 errors**. `dotnet test KBot.sln` —
  **tot verde**: App 37→**39** (+2 tooltip), Api 48, Controls 111, Theming 27, Common 7,
  Domain 1, LocalStore 1.
- **Python:** neschimbat (pass 02 nu atinge serverul); `test_forexe_receptii.py` skip
  off-host.

## Neverificat / amânat (Open threads)

- **Tooltip-ul n-a fost văzut pe ecran.** Shaping-ul (cumul, semn, culoare) e testat
  headless, dar randarea reală a tabelului (`TooltipPopup`) + declanșarea pe hover pe
  rădăcină nu au fost verificate vizual — la fel ca tot restul lui 0015 și `KBotDataView`.
- **Endpoint-ul tot n-a atins o bază reală** (vezi 0015-01): cifrele din tooltip depind de
  `DIFH`/`Sters`/`Data_plata` reale, nereproduse numeric offline.
- `bModRel` („modificare legături") + crearea de ordonanțări rămân în afara scope-ului.
