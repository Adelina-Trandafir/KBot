# ME02-01 — Data creării / modificării pe tabelele care nu o au

20.10.2026. **SCRIS ȘI GENERAT. NERULAT: nici un SQL executat nicăieri, nici un test.**

## Ce s-a cerut

Înainte de copiile locale (ME02), să se vadă ce tabele au dată de creare și de modificare, iar la cele care nu le au să se adauge ce lipsește. Planul întreg: [ME02_PLAN.md](../ME02_PLAN.md).

## Ce s-a făcut

- `tools/Me02Dates/gen_dates_ddl.js` citește `MariaDB_Schema/AVACONT_SURSA.sql` (șablonul pe care îl urmează cele 22 de baze) și scrie `sql/ME02_01_date_creare_modificare.sql`. Nu execută nimic.
- Din cele **133 de tabele**:
  - **47** au deja și dată de creare, și dată de modificare → nu se ating;
  - **7** sunt sărite cu rost: `AD_Lock`, `FX_NumberLock`, `VC_ContextLock` (lacăte, rândurile trăiesc secunde), `AD_PortalChallenges` (provocări de o dată), `VC_Audit`, `Interogari_Unice` (doar adăugare, rândurile nu se schimbă), `_snap_clsf` (copie fără cheie primară);
  - **79** primesc ce le lipsește: 55 primesc ambele coloane, 18 doar data modificării, 6 doar data creării (`EF_Furnizor` și cele cinci tabele de PDF/imagini care au `DataModif` pusă de cod, dar nu și data creării).
- Denumiri (nu s-a inventat o a patra familie): `VC_` și `AD_` → `CreatedAt` / `UpdatedAt`; restul → `DataAdaugare` / `DataModificare`.
- **Data modificării** = `datetime NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP`: o completează MariaDB singură la orice `UPDATE` care schimbă o valoare. Nu se atinge cod de aplicație.
- **Data creării** se adaugă întâi `DEFAULT NULL`, apoi `ALTER COLUMN … SET DEFAULT current_timestamp()` (a doua comandă schimbă doar valoarea implicită, nu rândurile). Rezultat: rândurile de dinainte au **NULL** = «existau înainte de ME02-01», nu o dată falsă; rândurile noi primesc ora.
- Fișierul se poate rula de două ori (`ADD COLUMN IF NOT EXISTS`). Se rulează pe `AVACONT_SURSA`; `schema_sync` îl duce în unități.
- Evenimentele nu contează ca «creare»: `ClosedAt`, `ConfirmedAt`, `ImportedAt`, `ReadAt` spun ce s-a întâmplat mai târziu cu rândul, deci tabelele lor primesc și o coloană de creare adevărată.

## Verificat în cod (înainte de predare)

- Nu există `INSERT INTO t VALUES (...)` fără listă de coloane în `.py`, `.vb`, `.js`, `.sql`.
- Toate cititoarele `SELECT *` din tabelele atinse sunt sigure la o coloană nouă de tip dată: ADECHIT trece datele prin `.isoformat()` în `Repository.query`; E-Factura prin `_plain`; VERCON/FOREXE folosesc coloane pe nume. Scrierile ADECHIT merg pe liste albe de câmpuri (`SCHEMA[...]['fields']`), deci coloanele noi nu ajung în `INSERT`/`UPDATE`.
- Clonarea în DEMO (`admin_demo.py`) scrie lista de coloane.

## Nevalidat / limite

- **Nimic nu a fost rulat pe o bază.** Nu știu dacă bazele reale au exact forma șablonului.
- **Timpul ALTER-ului:** tabelele cu conținut mare care primesc coloane (`EF_Mesaje`, `FX_PDF_BUCATI`, `FX_NoteCAB_Recipisa`, `FX_Receptii_IMG`, `FX_Rezervarii_IMG`) pot fi reconstruite de MariaDB 10.3 la adăugarea coloanei (nu am verificat dacă merge «instant» cu `ON UPDATE`). Rulați într-un moment liniștit. Și tabelele de PDF semnate / imagini (`FX_DDF_PDF`, `FX_ORD_PDF`, `FX_NoteCAB_PDF`, `FX_DDF_REV_ATT_IMG`, `FX_ORD_ATT_IMG`) primesc data creării și sunt chiar cele mai mari: ele sunt cele care pot dura. Dacă vreți, le puteți scoate din fișier la prima rulare (backup-ul oricum nu are nevoie de coloana asta pe ele: au `DataModif`).
- Clienții VB cu DTO strict (care ar respinge un câmp nou) nu au fost parcurși; cei cu `System.Text.Json` obișnuit ignoră câmpurile în plus.
- După rulare, `MariaDB_Schema/` trebuie reîmprospătat de mână (e gitignored), apoi motorul de backup folosește coloanele noi.
- Nu există script de întoarcere (se pot șterge coloanele, dar nu e nevoie).

## Fișiere

`tools/Me02Dates/gen_dates_ddl.js` (nou), `tools/Me02Dates/report.json` (rezultatul generatorului), `sql/ME02_01_date_creare_modificare.sql` (nou).
