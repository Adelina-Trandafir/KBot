-- =====================================================================================
-- Slice 0108 -- RUN THIS ON `AVACONT_SURSA` ONLY (HeidiSQL / mysql, as root).
--
-- AVACONT_SURSA is the template: the schema sync copies its STRUCTURE to every unit database and
-- provisioning clones it for every new unit (see 0102_01_sursa.sql). The structure changes of
-- this slice are made here once. The DATA part is the one-time query `0108_02_interogare_unica.sql`.
--
-- WHAT THIS SLICE CHANGES (operator, 03.10.2026)
--   * FOREXE's credit bugetar belongs to the CLASSIFICATION, not to the angajament: every
--     angajament that uses a classification has the same figure. It gets its own table,
--     `FX_Indicatori_Buget`, ONE row per classification, fed only by FOREXE.
--   * `FX_Indicatori.Credit_Bugetar_Initial` becomes what FOREXE showed on the page «Informatii
--     complete contract» the first time the angajament was «In derulare», written ONCE and never
--     changed. `FX_Angajamente.CreditInitialLa` says it was done (NULL = not yet), so the robot
--     opens that page once per angajament.
--   * `DTQ` = the last change of the row (ON UPDATE), `DTI` = the creation of the row.
--
-- ORDER (the whole slice):
--   1. this file, on AVACONT_SURSA;
--   2. AvacontPush, tab «Sincronizare schemă»: SAFE, on every unit database (adds `DTI`,
--      `FX_Indicatori_Buget` and `CreditInitialLa` there);
--   3. AvacontPush, tab «Interogări unice»: the text of `0108_02_interogare_unica.sql`, name
--      `0108_dti_si_credit_pe_clasificatie`, «Vezi», then «Execută»;
--   4. AvacontPush: push the new Python files, restart the service;
--   5. the client (K-BOT) and the Migrator;
--   6. LAST, once nothing reads it any more: `0108_03_sursa_scoate_credit_bugetar.sql`.
--
-- Safe to run twice (IF NOT EXISTS everywhere; the MODIFY of DTQ gives the same definition).
-- =====================================================================================

SET NAMES utf8mb4;

-- -------------------------------------------------------------------------------------
-- 1. DTI (created) and DTQ (last change) on every table that has a DTQ.
--
--    DTI is filled by the one-time query from the OLD DTQ, which was the creation moment where it
--    was filled at all (two tables defaulted it to the insert time; the other five were set by the
--    application or the import). A NEW row gets both from the server clock.
--
--    TRAP: a new column with DEFAULT current_timestamp() gives every EXISTING row the moment of
--    this ALTER. That is wrong until the one-time query has copied DTQ into DTI, so do not read
--    DTI between steps 2 and 3.
-- -------------------------------------------------------------------------------------
ALTER TABLE `AVACONT_SURSA`.`Clasificatii_Venituri_Rectificari`
  ADD COLUMN IF NOT EXISTS `DTI` datetime NULL DEFAULT current_timestamp()
  COMMENT 'Slice 0108: when the row was created' AFTER `DTQ`,
  MODIFY COLUMN `DTQ` datetime NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
  COMMENT 'Slice 0108: when the row was last changed';

ALTER TABLE `AVACONT_SURSA`.`FX_Angajamente`
  ADD COLUMN IF NOT EXISTS `DTI` datetime NULL DEFAULT current_timestamp()
  COMMENT 'Slice 0108: when the row was created' AFTER `DTQ`,
  MODIFY COLUMN `DTQ` datetime NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
  COMMENT 'Slice 0108: when the row was last changed';

ALTER TABLE `AVACONT_SURSA`.`FX_Indicatori`
  ADD COLUMN IF NOT EXISTS `DTI` datetime NULL DEFAULT current_timestamp()
  COMMENT 'Slice 0108: when the row was created' AFTER `DTQ`,
  MODIFY COLUMN `DTQ` datetime NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
  COMMENT 'Slice 0108: when the row was last changed';

ALTER TABLE `AVACONT_SURSA`.`FX_Istoric`
  ADD COLUMN IF NOT EXISTS `DTI` datetime NULL DEFAULT current_timestamp()
  COMMENT 'Slice 0108: when the row was created' AFTER `DTQ`,
  MODIFY COLUMN `DTQ` datetime NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
  COMMENT 'Slice 0108: when the row was last changed';

ALTER TABLE `AVACONT_SURSA`.`FX_Plati`
  ADD COLUMN IF NOT EXISTS `DTI` datetime NULL DEFAULT current_timestamp()
  COMMENT 'Slice 0108: when the row was created' AFTER `DTQ`,
  MODIFY COLUMN `DTQ` datetime NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
  COMMENT 'Slice 0108: when the row was last changed';

ALTER TABLE `AVACONT_SURSA`.`FX_Receptii`
  ADD COLUMN IF NOT EXISTS `DTI` datetime NULL DEFAULT current_timestamp()
  COMMENT 'Slice 0108: when the row was created' AFTER `DTQ`,
  MODIFY COLUMN `DTQ` datetime NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
  COMMENT 'Slice 0108: when the row was last changed';

ALTER TABLE `AVACONT_SURSA`.`FX_Rezervari`
  ADD COLUMN IF NOT EXISTS `DTI` datetime NULL DEFAULT current_timestamp()
  COMMENT 'Slice 0108: when the row was created' AFTER `DTQ`,
  MODIFY COLUMN `DTQ` datetime NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
  COMMENT 'Slice 0108: when the row was last changed';

-- -------------------------------------------------------------------------------------
-- 2. FX_Indicatori_Buget: the credit bugetar FOREXE reports, ONE row per classification.
--    Written by routes/forexe/prelucrare.py on every download (the credit of the indicator row it
--    scraped, whatever angajament it came from -- it is the same figure by FOREXE's own rule);
--    read by Sumar, by «Verifica bugetul» and by the readers that used FX_Indicatori.Credit_Bugetar.
-- -------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `AVACONT_SURSA`.`FX_Indicatori_Buget` (
  `IdClsf`         int(11)      NOT NULL COMMENT 'Clasificatii.IDClsf: one row per classification',
  `IdUnitate`      int(11)      NULL DEFAULT NULL COMMENT 'Clasificatii.IdUnitate of the classification',
  `CreditBugetar`  double       NOT NULL DEFAULT 0 COMMENT 'The credit bugetar FOREXE reported at the last download',
  `DTI`            datetime     NULL DEFAULT current_timestamp() COMMENT 'When the row was created',
  `DTQ`            datetime     NULL DEFAULT current_timestamp() ON UPDATE current_timestamp() COMMENT 'When the row was last changed',
  PRIMARY KEY (`IdClsf`) USING BTREE,
  INDEX `ix_FX_Indicatori_Buget_IdUnitate`(`IdUnitate` ASC) USING BTREE,
  CONSTRAINT `FX_Indicatori_Buget__Clasificatii` FOREIGN KEY (`IdClsf`) REFERENCES `AVACONT_SURSA`.`Clasificatii` (`IDClsf`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic
  COMMENT = 'Slice 0108: FOREXE credit bugetar, one row per classification (data only from FOREXE)';

-- -------------------------------------------------------------------------------------
-- 3. FX_Angajamente.CreditInitialLa: the moment the page «Informatii complete contract» was read
--    for this angajament. NULL = not read yet. Set ONCE, together with FX_Indicatori.Credit_Bugetar_Initial
--    of all its indicators, by routes/forexe/credit_initial.py (WHERE CreditInitialLa IS NULL).
-- -------------------------------------------------------------------------------------
ALTER TABLE `AVACONT_SURSA`.`FX_Angajamente`
  ADD COLUMN IF NOT EXISTS `CreditInitialLa` datetime NULL DEFAULT NULL
  COMMENT 'Slice 0108: when the initial credit was read from «Informatii complete contract» (NULL = not yet; set once)';

-- Check:
-- SHOW CREATE TABLE `AVACONT_SURSA`.`FX_Indicatori_Buget`;
-- SHOW CREATE TABLE `AVACONT_SURSA`.`FX_Indicatori`;
-- SHOW CREATE TABLE `AVACONT_SURSA`.`FX_Angajamente`;
