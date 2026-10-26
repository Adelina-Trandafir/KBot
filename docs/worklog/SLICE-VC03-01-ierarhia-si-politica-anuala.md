# SLICE-VC03-01 — ierarhia si politica anuala <a id="slice-vc03-01"></a>

Stare comună celor patru etape ale aceluiași DDL inițial: **IMPLEMENTAT_LOCAL**. Scripturile sunt scrise si generatorul a rulat (a produs fisierul); **nimic nu a fost aplicat pe MariaDB si nicio proba nu a fost rulata**. Acestea sunt checkpointuri istorice; [revizia din 16.10](../../VERCON/VERCON_STATUS.md#revizie-infrastructură-schemă--16102026) consemnează ulterior retragerea scriptului complet și a generatorului, fără a certifica aceste etape prin rulări noi.

## Facut
- `sql/VC_AVACONT_SURSA.sql`: `VC_Subunitati`, `VC_Gestiuni` (tip, stare, `Zec`, `Scara`, `Populated`), `VC_Exercitii` (un an per rand), `VC_ContextLock` (acces exclusiv, PK pe subunitate+gestiune, fara an).
- Triggere: tipul si scara inghetate dupa `Populated`; fara revenire la gol; OI/MF nu se inchid anual.
- Reprezentabil: mai multe gestiuni/tipuri si ani multipli in acelasi DC fara tabele separate (VERCON/SCHEMA_VC.md).

## Limite
Nimic aplicat/rulat. Alegerea `AnEvidenta` vs `DataDocument` ramane la serviciu (VC06): tabelele pastreaza ambele, distincte.

## Corectie (aceeasi zi)
Scriptul initial era gandit „o data pe fiecare DC", cu garda si triggere in proceduri. Gresit: schema se ruleaza pe AVACONT_SURSA si KBotAdmin (schema sync) o duce in DC-uri; sync-ul nu duce CHECK-uri si triggere. Refacut: un singur `sql/VC_AVACONT_SURSA.sql` (instructiuni simple) + `sql/VC_Reguli_DC.sql` optional pentru CHECK/triggere; regulile sunt impuse si de serviciu.

## SLICE-VC03-02 — tabelele business, tipurile, FK <a id="slice-vc03-02"></a>

### Facut
- `VERCON/tools/ddl.js` (citeste doar exporturile VERCON_ACCESS; foloseste acelasi cititor/clasificare ca dictionarul, mutat in `lib_tz.js`) genereaza `sql/VC_AVACONT_SURSA.sql`: 26 tabele, FK compuse pe context, INT scalat, `CHECK pret >= 0`, RESTRICT, anulare cu motiv, `TipDoc`/`IdPartener` pe NIR.
- Diagrama relatiilor: `VERCON/SCHEMA_VC_RELATII.mmd`.
- Deviatii deliberate listate in `VERCON/SCHEMA_VC.md` (Furnizori → Parteneri, legaturi logice fara FK, fara unicitate pe numere).

### Limite
Tratamentul exact pe fiecare relatie (fosta D05) este cel din generator: RESTRICT peste tot; ajustari dupa profilarea din migrator. `ProduseM` nu are FK spre ProduseP pana la profilare. Nerulat.

## SLICE-VC03-03 — importuri, audit, idempotency <a id="slice-vc03-03"></a>

### Facut
- `sql/VC_AVACONT_SURSA.sql`: `VC_Imports` (UNIQUE pe subunitate), `VC_ImportSources`, `VC_IdMap` (sursa+tabel+tip+cheie semnata), `VC_MigrationIssues`, `VC_Operations` (cheia comenzilor web, separata de markerul de import), `VC_Audit`, `VC_AnnualClosings`.
- `VC_ContextLock` (titular, jeton de sesiune, expirare) este in VC_01.

### Limite
Nerulat; atomicitatea markerului cu lotul o asigura `VRC.Migrator` (VC05-03).

## SLICE-VC03-04 — configurare, numerotare, date comune <a id="slice-vc03-04"></a>

### Facut
- `VC_Settings` (Setari/NR tipizate pe gestiune), `VC_Numbering` (contor per gestiune+exercitiu+fel de document), zecimalele in `VC_Gestiuni.Zec` (nu pe subunitate si nu global).
- Fara copie a identitatii DC/unitatii: raman in AVACONT_COMUN; fara scriere automata acolo.

### Limite
Maparea camp cu camp UNIT/cale → date comune se face in migrator (VC05-02). Nerulat.
