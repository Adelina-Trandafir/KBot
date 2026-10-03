-- =====================================================================================
-- ONE-TIME QUERY  0108_dti_si_credit_pe_clasificatie        (slice 0108)
--
-- NOT run by hand. Paste this text into AvacontPush, tab «Interogări unice», name it
-- `0108_dti_si_credit_pe_clasificatie`, press «Vezi (nu execută)» and then «Execută». The server
-- runs it once on AVACONT_SURSA and once on every unit database, each in its own database (so the
-- table names below are NOT qualified), and writes it in each database's `Interogari_Unice`.
--
-- BEFORE it: `0108_01_sursa.sql` on AVACONT_SURSA and a schema sync on the units (they must have
-- `DTI`, `FX_Indicatori_Buget` and `FX_Angajamente.CreditInitialLa`; without them the first
-- statement fails on that unit, nothing is marked, and the query can be run again afterwards).
--
-- WHAT IT DOES, on each database:
--   1. DTI = the old DTQ (the creation moment where it was ever filled; NULL stays NULL).
--      `DTQ = DTQ` is written ON PURPOSE: DTQ is now ON UPDATE CURRENT_TIMESTAMP, and a column the
--      UPDATE names itself is not touched by the automatic update, so every row keeps its date;
--   2. makes sure DTQ has the new definition (a SAFE schema sync adds columns but may not change an
--      existing one; the MODIFY is the same text as in 0108_01_sursa.sql);
--   3. seeds FX_Indicatori_Buget, one row per classification, from the credit the indicator rows
--      hold today. When several angajamente have an indicator on the same classification the one
--      with the latest «Dată început derulare» (FX_Angajamente.DataDefinitivare) wins, then the
--      latest DataCreare, then the higher credit. It is a starting point: the next download of any
--      angajament on the classification writes the figure FOREXE reports.
-- Every statement can be run again without harm (INSERT IGNORE; the UPDATE repeats the same copy).
-- =====================================================================================

UPDATE `Clasificatii_Venituri_Rectificari` SET `DTI` = `DTQ`, `DTQ` = `DTQ`;
UPDATE `FX_Angajamente`                    SET `DTI` = `DTQ`, `DTQ` = `DTQ`;
UPDATE `FX_Indicatori`                     SET `DTI` = `DTQ`, `DTQ` = `DTQ`;
UPDATE `FX_Istoric`                        SET `DTI` = `DTQ`, `DTQ` = `DTQ`;
UPDATE `FX_Plati`                          SET `DTI` = `DTQ`, `DTQ` = `DTQ`;
UPDATE `FX_Receptii`                       SET `DTI` = `DTQ`, `DTQ` = `DTQ`;
UPDATE `FX_Rezervari`                      SET `DTI` = `DTQ`, `DTQ` = `DTQ`;

ALTER TABLE `Clasificatii_Venituri_Rectificari`
  MODIFY COLUMN `DTQ` datetime NULL DEFAULT current_timestamp() ON UPDATE current_timestamp() COMMENT 'Slice 0108: when the row was last changed';
ALTER TABLE `FX_Angajamente`
  MODIFY COLUMN `DTQ` datetime NULL DEFAULT current_timestamp() ON UPDATE current_timestamp() COMMENT 'Slice 0108: when the row was last changed';
ALTER TABLE `FX_Indicatori`
  MODIFY COLUMN `DTQ` datetime NULL DEFAULT current_timestamp() ON UPDATE current_timestamp() COMMENT 'Slice 0108: when the row was last changed';
ALTER TABLE `FX_Istoric`
  MODIFY COLUMN `DTQ` datetime NULL DEFAULT current_timestamp() ON UPDATE current_timestamp() COMMENT 'Slice 0108: when the row was last changed';
ALTER TABLE `FX_Plati`
  MODIFY COLUMN `DTQ` datetime NULL DEFAULT current_timestamp() ON UPDATE current_timestamp() COMMENT 'Slice 0108: when the row was last changed';
ALTER TABLE `FX_Receptii`
  MODIFY COLUMN `DTQ` datetime NULL DEFAULT current_timestamp() ON UPDATE current_timestamp() COMMENT 'Slice 0108: when the row was last changed';
ALTER TABLE `FX_Rezervari`
  MODIFY COLUMN `DTQ` datetime NULL DEFAULT current_timestamp() ON UPDATE current_timestamp() COMMENT 'Slice 0108: when the row was last changed';

INSERT IGNORE INTO `FX_Indicatori_Buget` (`IdClsf`, `IdUnitate`, `CreditBugetar`, `CodAngajament`)
SELECT I.`IdClsf`, I.`IdUnitate`, I.`Credit_Bugetar`, I.`CodAngajament`
  FROM `FX_Indicatori` I
 WHERE I.`IdClsf` IS NOT NULL AND I.`IdClsf` <> 0 AND I.`Credit_Bugetar` IS NOT NULL
   AND EXISTS (SELECT 1 FROM `Clasificatii` C WHERE C.`IDClsf` = I.`IdClsf`)
   AND I.`CodAI` = (SELECT I2.`CodAI`
                      FROM `FX_Indicatori` I2
                      LEFT JOIN `FX_Angajamente` A2 ON A2.`CodAngajament` = I2.`CodAngajament`
                     WHERE I2.`IdClsf` = I.`IdClsf` AND I2.`Credit_Bugetar` IS NOT NULL
                     ORDER BY A2.`DataDefinitivare` DESC, A2.`DataCreare` DESC,
                              I2.`Credit_Bugetar` DESC, I2.`CodAI` DESC
                     LIMIT 1);
