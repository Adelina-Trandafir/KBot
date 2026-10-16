# VERCON — schema MariaDB `VC_` (SLICE-VC03)

Revizie 16.10.2026: **sursa unică a structurii este baza AVACONT_SURSA de pe server**, sincronizată separat în DC-uri. Dump-urile din `MariaDB_Schema` sunt fotografii pentru analiză. Scriptul complet generat și fragmentele sale de creare au fost eliminate; schimbările noi se livrează ca DDL incremental pentru aplicare manuală de operator.

VRC.Migrator reutilizează `KBot.Migrator.TargetSchema.Load` pentru a citi coloanele, tipurile, regulile NULL/default, coloanele generate, cheile și FK din DC-ul real. `WriteOrder.Derive` stabilește ordinea după FK; maparea importului Access adaugă numai regulile de conversie, remapare și fazele nomenclatoare/documente. Aceeași poartă de compatibilitate rulează în Testează și înaintea tranzacției. VRC nu creează și nu repară automat structura.

Stare a acestei revizii: **IMPLEMENTAT ÎN SURSE**, fără compilare, teste, verificări automate sau acces la baze de date. Secțiunile de model de mai jos sunt explicații de proiectare, nu un substitut pentru schema reală.

| Fisier | Continut | Cum se obtine |
|---|---|---|
| `MariaDB_Schema/AVACONT_SURSA.sql` | fotografia locală a structurii de referință | export furnizat de operator; nu este citit de migrator |
| `VRC.Migrator/import_map.json` | identitatea sursei, clase de conversie și remapări Access, fără declarații fizice ale țintei | `node VERCON/tools/import_map.js` |
| `sql/VC_FACTURI_NIR_20261016.sql` | modificările incrementale pentru facturi/NIR | SQL manual pentru operator |
| `sql/VC_Reguli_DC.sql` | CHECK-uri și triggere opționale, propunere istorică pentru fiecare DC | fișier păstrat separat; nu mai este regenerat de instrumentul de mapare |
| `sql/VC_00_preflight.sql` | verificari doar-citire inainte (pe AVACONT_SURSA) | scris de mana |
| `sql/VC_99_probe.sql` | 13 probe ale regulilor, cu rollback (pe un DC de test) | scris de mana |
| `VERCON/SCHEMA_VC_RELATII.mmd` | diagramă orientativă a relațiilor de import, fără autoritate asupra structurii MariaDB | instrumentul de mapare |

**Cum ajunge în DC-uri:** operatorul aplică DDL-ul incremental pe `AVACONT_SURSA`, apoi sincronizează schema prin mecanismul comun. Pentru baze noi, KBot.Migrator are deja copierea structurii direct din baza de referință prin `SHOW CREATE TABLE`; VRC nu dublează această funcție. Sincronizarea nu duce CHECK-uri și triggere; acestea și regulile aplicației se administrează separat.

## Ierarhie (VC03-01)

DC = baza de date. `VC_Subunitati` → `VC_Gestiuni` (tip, stare, `Zec`, `Scara`, `Populated`) → `VC_Exercitii` (un rand per an). Doua gestiuni de acelasi tip in aceeasi subunitate sunt randuri distincte; un an apartine unei singure gestiuni. Materiale/Alimente au cate un exercitiu per `tz_<an>`; OI/MF au o istorie unica, cu un rand de exercitiu per an cu documente (filtru, niciodata inchis). Nu exista tabele pe gestiune, pe an sau pe fisier.

- `Zec` si `Scara` sunt **pe gestiune** (decizia din 11.10.2026); `Scara >= Zec`, maxim 4.
- Tipul si scara nu se mai schimba dupa `Populated = 1`; o gestiune cu date nu redevine goala (trigger).
- OI/MF nu au exercitiu inchis (trigger).
- Acces exclusiv: cheia primara `(SubunitId, GestiuneId)` din `VC_ContextLock` (fara an); preluarea dupa expirare o face serviciul cu UPDATE conditionat.

## Tabele business (VC03-02)

- Fiecare are `SubunitId`, `GestiuneId` si, pentru documente anuale, `ExercitiuId`; **toate FK sunt compuse pe context**, deci o linie nu poate referi un antet sau un produs din alta gestiune (proba P4/P5).
- Gestiune: Masini, Produse, ProduseP, ProduseM, Clienti, ComisieNIR, ComisieInventar, TVA, FeluriMancare, Retetar, OI_Produse. Gestiune + exercitiu: NIR, Intrari, IntrariStorno, Iesiri_H, Iesiri, IesiriRetur, OI_Predare, OI_Transfer, OI_Casare, LABC, ListaAlimente, LegBCLA, MicDejun, Dejun, Cina.
- Valorile și cotele folosesc `DECIMAL(20,4)` direct, conform contractului actual de conversie, fără stocare INT scalată. VRC compară acest contract cu tipurile citite din schema reală.
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

În bazele Access această opțiune **nu există**: `LegBCLA.IdLista` este unic, adică o listă are exact un BC (1=1). Listă 1:N BC este o funcționalitate nouă a sistemului web. Modelul decis la 12.10.2026 pentru `VC_LegBCLA` folosește `RowId` tehnic, FK contextuale către `VC_LABC` și `VC_Iesiri_H`, index neunic pe `IdLista` și unicitate contextuală pe `IdBonConsum`. Unicitatea `IdLista` din Access nu se copiază prin import. Structura efectivă a acestor chei se citește din MariaDB, fără a regenera tabelele din relațiile Access.

Migrarea păstrează legăturile istorice și proveniența; asocierile incompatibile cu regula țintă se diagnostichează înainte de import, fără ștergere/deduplicare tacită. Alinierea DDL și probele de cardinalitate/rollback rămân de implementat, nu sunt validate prin PNG-uri.

## Limita acestei revizii

Nu s-a aplicat DDL, nu s-a compilat și nu s-au rulat teste sau probe pe baze de date în această revizie. Confirmările istorice de aplicare a schemei și de import din registrul VERCON nu certifică noua versiune a cititorului/mapării.
