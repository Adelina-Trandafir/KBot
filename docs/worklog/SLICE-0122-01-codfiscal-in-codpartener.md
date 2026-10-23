# SLICE-0122-01 - CodFiscal in CodPartener: the column of VC_NIR and the one-time query (20.10.2026)

**Status: WRITTEN, NOT RUN (no test, by the operator's rule).**

## Operator decisions (20.10.2026)
- The CodFiscal (normalized) goes INTO `CodPartener` on `FX_DDF_REV_SA`, `FX_DDF_REV_SB`, `FX_ORD_TBL`: no new column there.
- `VC_NIR` gets the same: a new column `CodPartener` ("we put CodPartener in VC_NIR too, we add the column").
- The next update is mandatory, so old clients are not a constraint.

## What was written
- `sql/P122_01_vc_nir_codpartener.sql`: `ALTER TABLE VC_NIR ADD COLUMN IF NOT EXISTS CodPartener varchar(50) NULL AFTER IdPartener` + index `ix_NIR_CodPartener`. utf8mb3/utf8mb3_general_ci. Idempotent. The operator runs it on AVACONT_SURSA and then syncs the structure to the units.
- `sql/0122_01_interogare_unica.sql` (name in KBotAdmin: `0122_01_codfiscal_in_codpartener`): for `FX_DDF_REV_SA`, `FX_DDF_REV_SB`, `FX_ORD_TBL` sets `CodPartener` := normalized CodFiscal (from `Parteneri` through `IdPartener` when set, else the existing `CodPartener` normalized) and `IdPartener` := NULL. For `VC_NIR` fills only the new `CodPartener` and KEEPS `IdPartener` (Vercon still uses it until 0122-05). Rows whose partner has no CodFiscal are not touched. Idempotent. Check queries as trailing comments.
- Normalization as everywhere (0121): letters and digits, upper case, a leading RO before a digit dropped.

## Order for the operator
P122_01 on AVACONT_SURSA -> structure sync -> the one-time query from KBotAdmin, right before the new server files and the new K-BOT go out (0122-02, 0122-03) -> only then the foreign keys are dropped (0122-04).

## Known gap between the query and the new server
After the query the old rows have `IdPartener` NULL. Until the new `parteneri_edit._ACTIVE` (0122-03) is on the server, the window «Parteneri» no longer sees that activity through `IdPartener` (a DDF header with the same CodFiscal still counts). So the query and the server files go out together.

## Left unverified
- Nothing was run: the temporary-table statements follow the pattern of 0121_07 (which the operator ran), the UPDATE ... JOIN on a temporary table is new.
- Real data not seen: how many rows still carry an IdPartener afterwards, and whether any row has an old unit code in `CodPartener` with no `IdPartener` (the third check query lists them).
- `VC_FACTURI.PARTENER_ID` (no foreign key) is not covered: decide with Vercon (0122-05).

## Follow-up 23.10.2026: 0122_01b
The first check on one database found (a) the 246 `VC_NIR` rows not filled (the column was probably missing when the first query reached that block) and (b) 41 DDF/ORD rows holding an old unit code (025, 026) with no IdPartener. Written: `sql/0122_01b_interogare_unica.sql` (name in KBotAdmin `0122_01b_completari_codfiscal`): matches such a code to the partner of the same unit and fills `VC_NIR.CodPartener` (with `CodFiscalSursa` as fallback). The operator ran it on that database and `sql/0122_01_verificari.sql` came back clean. The other databases are still to be done.
