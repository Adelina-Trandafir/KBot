# SLICE-ME01-36 - AVC.Migrator, step «Operatii»: missing partner found by tax code; every omission is BLOCKING (10.11.2026)

Slice given by Adelina (10.11.2026). Built clean (`AVC.Migrator.vbproj`: 0 errors, 0 warnings). Nothing run against a database or Access file.

## What changed and why

Verification of the step «Operatii» listed «5 randuri NU se migreaza: Partener negasit in Parteneri» (operations 1191, 1193, 1210 of unit LOCAL, 166, 168 of unit VENITURI; partners 320, 321, 47) as a WARNING, without saying who the partner was. Adelina's rules: any error that keeps rows of Oper out must BLOCK the migration, the lines must carry the Access `IdOperatie`, and a partner that the target lacks must be looked for by its tax code.

- **Every omission of the step is now BLOCKING** (`FindingClass.Blocant` instead of `Atentie`): operations without a document, without a date, with an unavailable journal, classification, account or partner, with no debit/credit lines, with debit different from credit; documents without a note, a date or operations. The detail lists up to 200 rows (was 30) and says how many more. The number after «operatia» in each line is the Access `IdOperatie` (it was already there).
- **Partner not found by code** (`Parteneri` of the target, unit + `CodPartener`):
  1. The partner is looked for in the Access `Parteneri` table of the SAME unit file (`AvcOperatiiSet.Parteneri`: code, name from `DenumirePartener` or else `NumePartener`, `CodFiscal`).
  2. Its tax code is normalised (`EfCui.Normalize`: no spaces, no «RO») and searched in `Parteneri` of the target, same unit (`AvcOperatiiLookups.ParteneriDupaCui`).
  3. Exactly ONE match: the line is allocated to it, and the report carries a WARNING per partner: «Partener realocat (unitatea U): in Access 320 «NAME», cod fiscal «X» ▸ in MariaDB C «NAME2», cod fiscal «Y» (IdPartener n); k linii, operatiile Access: ...». The same text goes to the progress line and to the SQL dump comment when the step is written. Only for operations that are kept.
  4. Otherwise the operation stays out and BLOCKS, with a reason of its own and the partner's name and tax code from Access: not in Access `Parteneri`; no tax code in Access; no partner with that tax code in the unit; several partners with it (they are listed, none is chosen).
- Same-unit search only: `Parteneri` is per unit, and a partner of another unit would be a guess.
- Also: the unassigned tuple `k_sub` in `BuildPlan` got an initial value (BC42108 warning that was already there).

## Correction, same day - a partner that exists nowhere in the target is ADDED, not blocked (Adelina)

«Daca nu exista, sa nu fie blocant, ci sa transfere ce exista - sa adauge partenerii lipsa.» Now, for a partner code missing from `Parteneri` of the target:
1. found by tax code, exactly one row of the unit: allocated to it (WARNING with both sides), as before;
2. found by tax code, several rows: still BLOCKING (no guess);
3. NOT found by tax code, or the Access partner has no tax code: the partner is ADDED to `Parteneri` of the unit by the write, from the Access `Parteneri` row (`CodPartener` as in Access, `DenumirePartener`, `CodFiscal`, `ContIBAN`, `Banca`, `Adresa`, `Tip`, `Ascuns`, `Provenienta` = `avacont`, the value the Access sync always wrote). Verification only announces it: WARNING «Partener nou, adaugat in Parteneri (unitatea U): cod C «NAME», cod fiscal «X» (nu exista in MariaDB nici dupa cod, nici dupa cod fiscal); k linii, operatiile Access: ...». The write inserts the partners first (inside the step's transaction), then the lines use their new `IdPartener` (`AvcPlanLinie.NewPartnerKey`); `Parteneri` is added to the rows reported as written;
4. the code is not in the Access `Parteneri` of the unit either: nothing to copy, the operation stays out and BLOCKS (the line says the partner is not in Access).
Only partners of operations that are kept are added. A repeat finds them by code and adds nothing.

Unverified: the Access columns `ContIBAN`, `Banca`, `Adresa`, `Tip`, `Ascuns` (read by name; a missing one is simply empty); a partner without tax code is added even if the target has a same-named one without a tax code (nothing to match on).

## Extension, same day - the templates (operations and salaries): a missing partner is CREATED, as a NON-supplier (Adelina)

Verification of the steps «Sabloane operatii» and «Sabloane salarii» left templates out with «partenerul «035» ... nu este in «Parteneri» (rulati intai KBot.Migrator, pasul Parteneri)» (codes 035, 134, 135, 136 used by the salary templates of unit VENITURI). Rule from Adelina: the same treatment as in Operatii (create the missing partner), with the mention that they are NOT suppliers, so they must not show in the normal supplier lists.
- New `Configurari/Parteneri/AvcPartnerEnsure.vb`. A partner code that a template names and `Parteneri` of the unit lacks is no longer a problem of the template: it is collected, and the write creates it. Name, tax code, IBAN, bank and address come from the Access `Parteneri` of the unit when it has the code (`AvcOperatiiReader.ReadAllPartners`); otherwise only the code and the name «Partener <cod>».
- **Not a supplier**: `Tip` = «0» (the supplier lists show only `Tip = '1'`, `PYTHON/routes/forexe/parteneri_edit.py`, slice 0093) and `Provenienta` = «AVC_NEFURNIZOR», so the mention stays on the row. `Ascuns` = 0.
- Two passes in the write, so no partner is made for a template that is left out for another reason (e.g. the chapter problem): resolve once collecting, `CreateNeeded` inserts the partners that at least one WRITTEN template needs, resolve again (finds them). Verification resolves once and warns, per partner, «Partener nou, de creat in Parteneri (unitatea U), NU este furnizor ...: cod C «NAME» (date din Access | nu exista in Access: «Partener C»); folosit in n locuri din sabloane». The write says «Partener creat ... IdPartener n» in the progress and in the SQL dump comment, and `Parteneri` is added to the rows written by the step.
- Applied in `AvcSablonOperatiiStep` and `AvcSablonSalariiStep` (`ResolvePartner` is no longer Shared; `_ensure`, `WrittenOwner`).
- Confirmed by Adelina (10.11.2026): partners are created ONLY when a written template needs them, and `Tip` «0» has no other meaning anywhere (Access or K-BOT).
- NOT changed: the Operatii step still copies the Access `Tip` for the partners it adds (those are real partners from Access); the problem «capitolul 65.02 nu este al subunitatii unitatii (65.10)» of the salary templates is a separate check and still leaves those templates out.

## Files touched
- `src/AVC.Migrator/IntroducereDate/Operatii/AvcOperatiiSet.vb` - `AvcPartnerRow`, `Parteneri`, `ReadPartners`.
- `src/AVC.Migrator/Configurari/Parteneri/AvcPartnerEnsure.vb` (new), `Configurari/SablonOperatii/AvcSablonOperatiiStep.vb`, `Configurari/SablonSalarii/AvcSablonSalariiStep.vb`, `IntroducereDate/Operatii/AvcOperatiiSet.vb` (`ReadAllPartners`).
- `src/AVC.Migrator/IntroducereDate/Operatii/AvcOperatiiStep.vb` - lookups, `AvcTargetPartner`, `AvcPartnerMove`, `ResolveOperation`, `Verify`, `Write`, `DescribePartnerMove`.

## Unverified / deferred
- Never run. The column names of the Access `Parteneri` table (`CodPartener`, `DenumirePartener` / `NumePartener`, `CodFiscal`) were taken from the transfer map (`TableMaps`), not from a file; if `CodFiscal` is named otherwise every partner looks «fara cod fiscal» and blocks, with the Access name shown.
- Making every omission blocking can stop runs that went through before (e.g. «Operatii fara document in Access»): intended, per Adelina.
- The step «Plati» (`AvcPlatiStep`) still reports its omissions as warnings; not asked here.
- Help: none (owner-side tool).
