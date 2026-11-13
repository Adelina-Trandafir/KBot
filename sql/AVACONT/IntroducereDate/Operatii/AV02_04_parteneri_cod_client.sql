-- =====================================================================================
-- Slice AV02-04 -- RUN THIS ON `AVACONT_SURSA` ONLY (Navicat / mysql, as root), then «Sincronizare schema» on the unit databases.
--
-- WHAT THIS ADDS to Parteneri (one nullable column, nothing existing changes):
--   CodClient   the «Cod client» of the partner, right after CodFiscal. In the old Access system it is a column of
--               Parteneri that is EMPTY for most users; where it holds data, the step «Cod client pe parteneri» of
--               AVC.Migrator copies it here. When several partner rows of a unit share the same CodFiscal and carry
--               different client codes, the codes are joined with ";" (e.g. «A12;B7») and every row with that
--               CodFiscal gets the joined text. The page «Facturi si plati» shows it (read only).
--
-- KBot.Migrator keeps `CodClient` in its Exclude list for Parteneri on purpose: only AVC.Migrator fills this column.
-- Safe to run twice (IF NOT EXISTS). utf8mb3_general_ci like every other table.
-- =====================================================================================

ALTER TABLE `AVACONT_SURSA`.`Parteneri`
  ADD COLUMN IF NOT EXISTS `CodClient` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL
    COMMENT 'Cod client (Access Parteneri.CodClient); several codes of the same CodFiscal are joined with ;' AFTER `CodFiscal`;

-- Check (read only):
-- SHOW CREATE TABLE `AVACONT_SURSA`.`Parteneri`;
-- Then: AvacontPush, tab «Sincronizare schema», SAFE, on every unit database (000_DEMO ...).
