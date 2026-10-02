-- =====================================================================================
-- Slice 0102 (+ 0103) -- RUN THIS ON `AVACONT_SURSA` ONLY (HeidiSQL / mysql, as root).
--
-- AVACONT_SURSA is the template: schema_sync copies its STRUCTURE to every unit database and the
-- provisioning job clones it for every new unit. So the structure changes of this slice are made
-- here, once, and travel from here:
--   1. `Interogari_Unice`  -- the ledger of one-time queries (slice 0103, see below);
--   2. `Clasificatii_Buget.DataInceput` + the new unique key (slice 0102).
-- The DATA part of slice 0102 (give the existing budget rows their start date, drop the old
-- unique key on the units) is NOT here: it is the one-time query in `0102_02_interogare_unica.sql`,
-- pasted into AvacontPush, which runs it on every unit AND on this database.
--
-- ORDER (the whole slice):
--   1. this file, on AVACONT_SURSA;
--   2. AvacontPush: push the new Python files, restart the service;
--   3. AvacontPush, tab «Sincronizare schemă»: SAFE, on every unit database (it creates
--      `Interogari_Unice` there and adds `DataInceput` and the new unique key);
--   4. AvacontPush, tab «Interogări unice»: the text of `0102_02_interogare_unica.sql`, name
--      `0102_buget_data_inceput`, «Vezi», then «Execută».
--
-- Safe to run twice (IF NOT EXISTS / IF EXISTS everywhere).
-- =====================================================================================

SET NAMES utf8mb4;

-- -------------------------------------------------------------------------------------
-- 1. The ledger of one-time queries (slice 0103).
--    One row = one query that has been run on THIS database. The identity is the SHA-256 of the
--    query text (normalized: line ends, trailing blanks), so the same text is never run twice and
--    a CHANGED text under an old name is refused. A unit created later copies the rows of this
--    table at provisioning, so a new unit is born with every query already «run».
--    Written by routes/one_time/runner.py (AvacontPush) and routes/inregistrare/provizionare.py.
-- -------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `AVACONT_SURSA`.`Interogari_Unice` (
  `Hash`       char(64)     NOT NULL COMMENT 'Slice 0103: SHA-256 of the normalized query text',
  `Nume`       varchar(100) NOT NULL COMMENT 'The name the operator gave the query',
  `RulatLa`    datetime     NOT NULL DEFAULT current_timestamp() COMMENT 'When it was run on this database',
  `RulatDe`    varchar(64)  NULL DEFAULT NULL COMMENT 'AvacontPush, or provisioning when a new unit is born with it marked done',
  `Randuri`    int(11)      NULL DEFAULT NULL COMMENT 'Rows the statements reported as affected',
  `Interogare` mediumtext   NULL DEFAULT NULL COMMENT 'The normalized text that ran',
  PRIMARY KEY (`Hash`) USING BTREE,
  UNIQUE INDEX `uq_interogari_unice_nume`(`Nume` ASC) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- -------------------------------------------------------------------------------------
-- 2. Clasificatii_Buget.DataInceput (slice 0102): one row = one version of the budget of a
--    classification for a year, starting on that day. The template normally has no rows; if it
--    has, they get the start date the operator chose (01.04.2026; another year: 01.01).
-- -------------------------------------------------------------------------------------
ALTER TABLE `AVACONT_SURSA`.`Clasificatii_Buget`
  ADD COLUMN IF NOT EXISTS `DataInceput` date NOT NULL
  COMMENT 'Slice 0102: the date this budget row starts to apply (one row = one version of the budget)'
  AFTER `An`;

UPDATE `AVACONT_SURSA`.`Clasificatii_Buget`
   SET `DataInceput` = IF(`An` IS NULL OR `An` = 2026, '2026-04-01', MAKEDATE(`An`, 1))
 WHERE `DataInceput` < '2000-01-01';

-- (IdClsf, An) -> (IdClsf, An, DataInceput). The foreign key on IdClsf has its own index
-- (`Clasificatii_Buget_ibfk_1`), so the old unique key can go.
ALTER TABLE `AVACONT_SURSA`.`Clasificatii_Buget`
  DROP INDEX IF EXISTS `uq_clasificatii_buget_idclsf_an`;

ALTER TABLE `AVACONT_SURSA`.`Clasificatii_Buget`
  ADD UNIQUE INDEX IF NOT EXISTS `uq_clasificatii_buget_idclsf_an_data` (`IdClsf`, `An`, `DataInceput`);

-- Check:
-- SHOW CREATE TABLE `AVACONT_SURSA`.`Clasificatii_Buget`;
-- SHOW CREATE TABLE `AVACONT_SURSA`.`Interogari_Unice`;
