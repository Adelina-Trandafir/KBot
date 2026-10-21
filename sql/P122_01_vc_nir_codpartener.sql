-- 0122-01: VC_NIR gets CodPartener, the supplier's NORMALIZED CodFiscal (letters and digits only, upper case, a leading RO before a digit dropped).
-- Plan: docs/worklog/SLICE-0122-parteneri-fara-idpartener.md.
-- Run by hand ONLY on AVACONT_SURSA, then propagate the structure with the schema sync (KBotAdmin) to the unit databases.
-- Safe to run twice (IF NOT EXISTS). Nothing is dropped and nothing is changed in the existing columns:
-- IdPartener and its foreign key stay as they are until Vercon stops using them (slice 0122-05).
--
-- Why this column: VC_NIR had only IdPartener, the key of ONE unit's row in Parteneri. The same supplier exists once per unit,
-- so the receipts of one supplier were spread over several IdPartener values. CodPartener here holds the CodFiscal instead
-- (the same convention as FX_DDF_REV_SA, FX_DDF_REV_SB and FX_ORD_TBL, where the column already exists and is filled by 0122-01).
-- The existing receipts are filled by the one-time query sql/0122_01_interogare_unica.sql.

ALTER TABLE `VC_NIR`
  ADD COLUMN IF NOT EXISTS `CodPartener` varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL
    COMMENT '0122-01: normalized CodFiscal of the supplier (not the unit code of Parteneri). NULL for a Stoc initial document'
    AFTER `IdPartener`,
  ADD INDEX IF NOT EXISTS `ix_NIR_CodPartener` (`CodPartener`);
