# SLICE-0121-02 - Migrator: `ParteneriAng` -> supplier <-> angajament links by CodFiscal (19.10.2026)

Plan: [SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md](SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md). Needs 0121-01 ([schema](SLICE-0121-01-schema.md)) run and synced.

## What changed and why
Access `ParteneriAng` no longer goes into `Parteneri_Coduri`. It becomes two tables keyed by the supplier's normalized CodFiscal, so the partner no longer has to exist in the unit on the server.

- **Source of the CodFiscal:** the unit's own Access `Parteneri` (by `CodPartener`), normalized with `EfCui.Normalize` (the same rule as the DDF's `normalize_cui`). Not read from the target.
- **What is written:**
  - `Parteneri_Angajamente(CodFiscal, CodAng)` - one row per pair;
  - `Parteneri_Angajamente_Indicatori(IdLegAng, CodAI, CodInd, ContBancar)` - one row per `CodAI = CodAng-CodInd` (the key of `FX_Indicatori`); `ContBancar` from Access `ContBanca`.
  - The Access `IdClsf` / `Clsf` are not written (`CodAI` already gives the classification; `Clsf` is read only for the cross-unit report).
- **Upsert, safe to run twice:** the link finds its id back through `LAST_INSERT_ID(IdLegAng)`; an indicator fills `CodInd` / `ContBancar` only where still empty - what the operator edited after a first run is never overwritten.
- **Nothing is lost silently.** Every Access row ends in exactly one place: written, merged into a repeated pair, or listed in the report:
  - partners with no usable CodFiscal (count + list per unit) - no rows in the new tables, they stay in `Parteneri`;
  - partner codes of `ParteneriAng` missing in the unit's `Parteneri`;
  - rows with a partner but no `CodAng` (no link possible);
  - rows with `CodAng` but no `CodInd` (link written, no indicator);
  - repeats merged; two different non-empty bank accounts on the same link + `CodAI` (the first stays, listed);
  - same CodFiscal + same classification with different `CodAng` in two or more units (the operator believes it never happens - the report shows whether it does); all are written, the key is `(CodFiscal, CodAng)`;
  - CodFiscal values that are not all digits after normalization (CNP / foreign VAT ids) - listed, written as they come out;
  - `CodAI` missing in `FX_Indicatori` (the target's plus the ticked units' FOREXE files) - listed, still written (no foreign key).
  Only an unreadable `ParteneriAng` blocks.
- The ticked table is still «Parteneri_Angajamente» (Access source `ParteneriAng`); the indicators table is written with it. `Parteneri_Coduri` is not written any more and stays on the server untouched (0121-06 drops it, at the operator's word).

## Files touched
- `src/KBot.Migrator/Transfer/PartnerLinkSet.vb` (new): `PartnerLinkSet`, `PartnerLink`, `PartnerLinkIndicator`, `PartnerRowCount`, `PartnerLinkReader` (reads Access, shapes the links, reads the known `CodAI`).
- `src/KBot.Migrator/Transfer/TableMap.vb`: `WritesPartnerLinks`, `CompanionTable`, `AsPartnerLinks`.
- `src/KBot.Migrator/Transfer/TableMaps.vb`: the `ParteneriAng` map now targets `Parteneri_Angajamente` (+ companion).
- `src/KBot.Migrator/Transfer/Finding.vb`: new kinds `LEGATURI_PARTENERI`, `PARTENER_FARA_COD_FISCAL`, `PARTENER_LIPSA_ACCESS`, `LEGATURA_FARA_ANGAJAMENT`, `LEGATURI_CONFLICT_UNITATI`, `CONT_BANCAR_DIFERIT`, `INDICATOR_LIPSA_FX`, `COD_FISCAL_NENUMERIC`.
- `src/KBot.Migrator/Transfer/Verifier.vb`: companion table existence, the 1364 / missing-column guard for both tables, and the new gate `CheckPartnerLinks` (in the existing step «Se rezolvă partenerii…»).
- `src/KBot.Migrator/Transfer/TransferRunner.vb`: `WritePartnerLinks`, called from the write loop for a `WritesPartnerLinks` map.
- `src/KBot.Migrator/KBot.Migrator.vbproj`: FileVersion 1.20.0.0 -> 1.21.0.0.
- Status: `state/KBOT_STATUS_0120-0129.md`, `KBOT_STATUS.md`, the plan file.

## Test results
`dotnet build src/KBot.Migrator/KBot.Migrator.vbproj`: succeeded, 0 warnings, 0 errors. **Nothing else was run, on purpose** (operator, 19.10.2026: no way to test the Migrator now - built only; no tests written or run, no Access file opened, no database touched). The test project was not built.

## Left unverified or deferred
- Never run: the Access column names are taken from `qFX_Ang_Part` (`CodAng, IdClsf, CodPartener, CodInd, Clsf, ContBanca`) and from the old map; Access `Parteneri` is assumed to have `CodFiscal` (it travelled by name match to `Parteneri.CodFiscal`) and a name column (`DenumirePartener`, else `NumePartener`; only used to label the report).
- `LAST_INSERT_ID(IdLegAng)` through MySqlConnector's `LastInsertedId` is the expected behaviour but unproven here; a lookup by `(CodFiscal, CodAng)` is the fallback when it returns 0.
- The verifier and the runner each read the Access files (the runner does not take the verifier's `PartnerLinkSet`); both read the same files, so the figures agree unless a file changes in between.
- Not decided by me: a CodFiscal longer than 255 characters or a `CodAng` longer than 255 would fail the INSERT (loud, rolls back), not be cut.
- `tests/KBot.Migrator.Tests` was not checked for tests of the old `ParteneriAng` -> `Parteneri_Coduri` map (a grep found only a comment).
- No help change (the Migrator is an owner tool).
