# VERCON — schema MariaDB `VC_` (SLICE-VC03)

Stare: **IMPLEMENTAT_LOCAL** (scripturi scrise). **Nimic aplicat pe MariaDB, nimic rulat** — nici preflight, nici proba. Deci nimic VERIFICAT_LOCAL / VALIDAT_SERVER.

| Fisier | Continut | Cum se obtine |
|---|---|---|
| `sql/VC_AVACONT_SURSA.sql` | **toate tabelele** (ierarhie, 26 business, import/audit/setari/numerotare), instructiuni simple, `CREATE TABLE IF NOT EXISTS`, fara proceduri sau DELIMITER | **generat**: `node VERCON/tools/ddl.js` (piesele scrise de mana sunt in `VERCON/tools/ddl_parts/`) |
| `sql/VC_Reguli_DC.sql` | CHECK-uri si triggere (optional, pe fiecare DC) | generat de acelasi script |
| `sql/VC_00_preflight.sql` | verificari doar-citire inainte (pe AVACONT_SURSA) | scris de mana |
| `sql/VC_99_probe.sql` | 13 probe ale regulilor, cu rollback (pe un DC de test) | scris de mana |
| `VERCON/SCHEMA_VC_RELATII.mmd` | diagrama relatiilor (Mermaid) | generat |

**Cum ajunge in DC-uri:** rulezi `VC_AVACONT_SURSA.sql` O SINGURA DATA pe `AVACONT_SURSA`; sincronizarea schemei din KBotAdmin duce tabelele, coloanele, cheile si cheile externe in fiecare DC. **Sincronizarea nu duce CHECK-uri si triggere** (verificat in `routes/schema_sync`: citeste doar tabele, coloane, chei, indexuri, chei externe, charset). De aceea pretul >= 0, tipul/scara inghetate dupa populare, OI/MF neinchise si motivul de anulare sunt impuse **si de serviciu**; `VC_Reguli_DC.sql` este o a doua linie de aparare, optionala. Unicitatile si cheile externe (import unic, acces exclusiv, context compus) ajung prin sincronizare.

## Ierarhie (VC03-01)

DC = baza de date. `VC_Subunitati` → `VC_Gestiuni` (tip, stare, `Zec`, `Scara`, `Populated`) → `VC_Exercitii` (un rand per an). Doua gestiuni de acelasi tip in aceeasi subunitate sunt randuri distincte; un an apartine unei singure gestiuni. Materiale/Alimente au cate un exercitiu per `tz_<an>`; OI/MF au o istorie unica, cu un rand de exercitiu per an cu documente (filtru, niciodata inchis). Nu exista tabele pe gestiune, pe an sau pe fisier.

- `Zec` si `Scara` sunt **pe gestiune** (decizia din 11.10.2026); `Scara >= Zec`, maxim 4.
- Tipul si scara nu se mai schimba dupa `Populated = 1`; o gestiune cu date nu redevine goala (trigger).
- OI/MF nu au exercitiu inchis (trigger).
- Acces exclusiv: cheia primara `(SubunitId, GestiuneId)` din `VC_ContextLock` (fara an); preluarea dupa expirare o face serviciul cu UPDATE conditionat.

## Tabele business (VC03-02)

- Fiecare are `SubunitId`, `GestiuneId` si, pentru documente anuale, `ExercitiuId`; **toate FK sunt compuse pe context**, deci o linie nu poate referi un antet sau un produs din alta gestiune (proba P4/P5).
- Gestiune: Masini, Produse, ProduseP, ProduseM, Clienti, ComisieNIR, ComisieInventar, TVA, FeluriMancare, Retetar, OI_Produse. Gestiune + exercitiu: NIR, Intrari, IntrariStorno, Iesiri_H, Iesiri, IesiriRetur, OI_Predare, OI_Transfer, OI_Casare, LABC, ListaAlimente, LegBCLA, MicDejun, Dejun, Cina.
- Valori = `INT` scalat `10^Scara`; cota TVA = `INT` x 10000; preturi `CHECK >= 0` (`Pret`, `PUTVA`, `PU`).
- `RESTRICT` peste tot; niciun `CASCADE` peste istoric. Anularea ca la ADECHIT: `Anulat` + motiv obligatoriu (CHECK) pe NIR, IntrariStorno, Iesiri_H, LABC, OI_Predare/Transfer/Casare. Nomenclatoarele au `Activ`.
- Cheile sursa nu se stocheaza: MariaDB genereaza cheile, `VC_IdMap` tine perechea. Tabelele a caror cheie primara este si FK (LegBCLA) si cele fara cheie simpla (ProduseM) primesc `RowId`.

### Deviatii fata de Access (deliberate)

| Tema | Tratament |
|---|---|
| Furnizori | fara `VC_Furnizori`. `VC_NIR.IdPartener` → `Parteneri.IdPartener` (KBOT); `IDF`/`IDFV` nu se copiaza; `FurnizorV` (text) si `CodFiscalSursa` (CF-ul din Access la migrare) raman ca istoric; legatura reala este `IdPartener`. Mai multe NIR pot fi «Stoc initial» (fara unicitate pe `TipDoc`) |
| «Stoc initial» | `VC_NIR.TipDoc = 'STOC_INITIAL'`, fara partener (CHECK) |
| Legaturi logice fara relatie declarata (IdMasina, IdStorno, IDBP/IDBT/IDBC, IDC_OLD, ProduseM.IdProdus…) | coloane simple, **fara FK**: se pun FK doar dupa profilarea din migrator (VC05-01) |
| Număr NIR cu zecimale | `VC_NIR.Numar` INT + `NumarSub` SMALLINT (sutimi): 707,1 = 707 + 10; ordine (Numar, NumarSub) |
| Numere de document | nicio unicitate pe coloanele de numar: istoricul importat poate repeta numere; numerotarea noua o face `VC_Numbering` + serviciul (VC06) |
| TVA, comisii | pe gestiune, nededuplicate dupa nume (D08 ramane pentru partajare) |
| `cpv`, `Alimente` (lista), `Caletemp`, `tmp*`, `FM_2`, UNIT/Setari/NR (contoarele NR.* nu se importa: numerotarea porneste de la ultimul numar real salvat, decizie 12.10.2026) | nu sunt tabele business VC_: UNIT → date comune; Setari/NR → `VC_Settings`/`VC_Numbering`/`VC_Gestiuni` |
| `ProduseM.ValoareLunara` | exista doar in baza MF; in tinta e coloana normala (NULL la celelalte) |

## Import, audit, setari (VC03-03/04)

`VC_Imports` are `UNIQUE (SubunitId)`: un singur transfer confirmat per subunitate, scris in aceeasi tranzactie cu tot lotul. `VC_ImportSources` tine fisierul (nume, nu cale), hash-ul, anul si gestiunea; `VC_IdMap` tine `(SourceId, tabel, tip, cheie sursa semnata) → cheie tinta`, cu `Kind` ROW/GROUP/ASSET. `VC_MigrationIssues` tine fiecare regula aplicata (N01/N02 etc.) cu valoarea originala. `VC_Operations` face comenzile web idempotente (cheia nu se confunda cu markerul de import). Nu se importa `cale.PASS` sau alte credentiale. Rolurile si dreptul nu au tabele VC_ (doar roluri pe DC in AVACONT_COMUN).

## Cardinalitatea listă–BC — cerință nouă, 12.10.2026 revizia 2

**Decis de utilizator:** o listă de alimente poate avea mai multe bonuri de consum. [FLUXURI_UI](FLUXURI_UI.md) și [CONTRACT_CALCULE](CONTRACT_CALCULE.md) definesc selecția, totalurile și salvarea atomică a setului. Un BC nu este ales pentru altă listă și nu apare de două ori în aceeași listă.

În bazele Access această opțiune **nu există**: `LegBCLA.IdLista` este unic, adică o listă are exact un BC (1=1). Listă 1:N BC este o funcționalitate nouă a sistemului web. **DDL-ul și generatorul sunt aliniate (12.10.2026):** `VC_LegBCLA` are `RowId` tehnic, FK contextuale către `VC_LABC` și `VC_Iesiri_H`, index neunic `ix_LegBCLA_IdLista`, iar `uq_LegBCLA_IdBonConsum` (contextual) garantează că un BC aparține unei singure liste, deci apare o singură dată în ea (MariaDB admite mai multe NULL). Unicitatea `IdLista` din Access nu se copiază. Generatorul `tools/ddl.js` tratează explicit relația LABC→LegBCLA (one-to-one în Access) ca 1:N. Protecția concurentă a atribuirii este dată de cheia unică; serviciul VC07-01 trebuie să prindă eroarea de duplicat și să o raporteze ca „BC deja asociat altei liste".

Migrarea păstrează legăturile istorice și proveniența; asocierile incompatibile cu regula țintă se diagnostichează înainte de import, fără ștergere/deduplicare tacită. Alinierea DDL și probele de cardinalitate/rollback rămân de implementat, nu sunt validate prin PNG-uri.

## Ce nu s-a facut

Nu s-a aplicat nimic pe MariaDB si nu s-a rulat nicio probă, deci **nu s-a dovedit** ca scripturile se compileaza pe MariaDB. Preflight + VC_99 sunt scrise pentru asta (VC03-05) si asteapta o rulare pe o baza de test.
