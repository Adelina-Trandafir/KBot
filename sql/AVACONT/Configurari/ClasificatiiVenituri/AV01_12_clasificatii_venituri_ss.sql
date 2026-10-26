-- =====================================================================================
-- Slice AV01-12 -- RUN THIS ON `AVACONT_SURSA` ONLY (Navicat / mysql, as root), then «Sincronizare schema» on the unit databases.
-- NOT RUN YET. Run it BEFORE the server code of AV01-12 reaches the VPS (the code reads and writes Clasificatii_Venituri.SS).
--
-- WHAT THIS DOES (the «Venituri» page of the «Clasificatii» window follows the source selector of the title bar)
--   1. Clasificatii_Venituri.SS   NEW column: the sector+source the classification belongs to. NOT NULL DEFAULT '02E': every row that exists
--                                 today was transferred for 02E (operator, 26.10.2026), and the schema sync gives that value to the rows of the
--                                 unit databases by itself, so there is no separate UPDATE to run on them.
--   2. UK_Clasificatii_Venituri_CSP   was unique on (Capitol, SubCapitol, Paragraf): the same code may now exist once PER SOURCE, so the key becomes
--                                 (SS, Capitol, SubCapitol, Paragraf).
--
-- The default stays on the column on purpose (extrase.py and the Access migration insert without SS: they work on 02E for now). The window and the
-- server always write SS explicitly.
--
-- Safe to run twice. utf8mb3_general_ci like every other table.
-- =====================================================================================

-- 1. The source of the classification.
ALTER TABLE `AVACONT_SURSA`.`Clasificatii_Venituri`
  ADD COLUMN IF NOT EXISTS `SS` varchar(16) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT '02E'
    COMMENT 'AV01-12: the sector+source the revenue classification belongs to' AFTER `IdClsfV`;

-- 2. One row per code and source.
ALTER TABLE `AVACONT_SURSA`.`Clasificatii_Venituri`
  DROP INDEX IF EXISTS `UK_Clasificatii_Venituri_CSP`,
  ADD UNIQUE INDEX IF NOT EXISTS `UK_Clasificatii_Venituri_SS_CSP`(`SS` ASC, `Capitol` ASC, `SubCapitol` ASC, `Paragraf` ASC) USING BTREE;

-- Check (read only):
-- SHOW CREATE TABLE `AVACONT_SURSA`.`Clasificatii_Venituri`;
-- Then: AvacontPush, tab «Sincronizare schema», SAFE, on every unit database; afterwards on a unit database:
-- SELECT SS, COUNT(*) FROM `<unit database>`.`Clasificatii_Venituri` GROUP BY SS;   -- expected: 02E only, until a unit gets another source
