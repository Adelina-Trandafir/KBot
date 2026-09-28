-- =====================================================================================
-- Slice 0088-03 -- FX_NoteCAB in the FIRST draft shape (one operation per note) -> the final
-- shape: header FX_NoteCAB + one FX_NoteCAB_Corectii row per operation.
--
-- Why: the first draft of sql/0088 made FX_NoteCAB carry the operation itself (ReferintaTrezor,
-- NrDoc, SimbolCont ... Explicatii). The final sql/0088 uses CREATE TABLE IF NOT EXISTS, so on a
-- database where the draft already ran it SKIPPED FX_NoteCAB and the note server
-- (routes/forexe/note_cab.py) finds neither FX_NoteCAB_Corectii nor the header it expects.
-- Seen in the AVACONT_SURSA dump of 28.09.2026 14:18.
--
-- Which databases: those where step 1 answers 1. Where it answers 0 the table is already in the
-- final shape -- do NOT run the rest there (step 3 would fail on the missing columns).
-- Any note already stored is kept: it becomes a note with one correction (NrOrdine 1).
-- Run the statements ONE BY ONE.
-- =====================================================================================

-- 1. Draft shape? 1 = yes (run the rest), 0 = no (stop here).
SELECT COUNT(*) AS forma_veche
  FROM information_schema.COLUMNS
 WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'FX_NoteCAB' AND COLUMN_NAME = 'ReferintaTrezor';

-- 2. The corrections table (same text as sql/0088).
CREATE TABLE IF NOT EXISTS `FX_NoteCAB_Corectii` (
  `IDNCC`              int(11)       NOT NULL AUTO_INCREMENT,
  `IDNC`               int(11)       NOT NULL,
  `NrOrdine`           int(11)       NOT NULL,
  `IDFXP`              int(11)       NULL DEFAULT NULL,
  `ReferintaTrezor`    varchar(15)   NOT NULL,
  `NrDoc`              varchar(255)  NOT NULL,
  `SimbolCont`         varchar(15)   NOT NULL,
  `CodProgram`         varchar(10)   NOT NULL,
  `DataOperInitiala`   date          NOT NULL,
  `Coloana`            char(1)       NOT NULL,
  `Suma`               decimal(15,2) NOT NULL,
  `CodAngajament`      varchar(255)  NOT NULL,
  `CodIndicator`       varchar(3)    NOT NULL,
  `CodAI`              varchar(50)   NULL DEFAULT NULL,
  `SimbolContCorectie` varchar(15)   NOT NULL,
  `CodProgramCorectie` varchar(10)   NOT NULL,
  `Explicatii`         varchar(70)   NOT NULL,
  PRIMARY KEY (`IDNCC`) USING BTREE,
  UNIQUE INDEX `UQ_FX_NoteCAB_Corectii_Operatiune` (`ReferintaTrezor` ASC, `NrDoc` ASC) USING BTREE,
  INDEX `IX_FX_NoteCAB_Corectii_IDNC` (`IDNC` ASC, `NrOrdine` ASC) USING BTREE,
  INDEX `IX_FX_NoteCAB_Corectii_CodAngajament` (`CodAngajament` ASC) USING BTREE,
  INDEX `IX_FX_NoteCAB_Corectii_IDFXP` (`IDFXP` ASC) USING BTREE,
  CONSTRAINT `FK_FX_NoteCAB_Corectii_Nota` FOREIGN KEY (`IDNC`)
    REFERENCES `FX_NoteCAB` (`IDNC`) ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT `FK_FX_NoteCAB_Corectii_Angajament` FOREIGN KEY (`CodAngajament`)
    REFERENCES `FX_Angajamente` (`CodAngajament`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `FK_FX_NoteCAB_Corectii_Operatiune` FOREIGN KEY (`IDFXP`)
    REFERENCES `FX_Operatiuni` (`IDFXP`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `CK_FX_NoteCAB_Corectii_Coloana` CHECK (`Coloana` IN ('D', 'C')),
  CONSTRAINT `CK_FX_NoteCAB_Corectii_Suma` CHECK (`Suma` < 0)
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- 3. Notes already stored in the draft shape -> one correction each (skips what was copied).
INSERT INTO `FX_NoteCAB_Corectii`
       (`IDNC`, `NrOrdine`, `IDFXP`, `ReferintaTrezor`, `NrDoc`, `SimbolCont`, `CodProgram`,
        `DataOperInitiala`, `Coloana`, `Suma`, `CodAngajament`, `CodIndicator`, `CodAI`,
        `SimbolContCorectie`, `CodProgramCorectie`, `Explicatii`)
SELECT n.`IDNC`, 1, n.`IDFXP`, n.`ReferintaTrezor`, n.`NrDoc`, n.`SimbolCont`, n.`CodProgram`,
       n.`DataOperInitiala`, n.`Coloana`, n.`Suma`, n.`CodAngajament`, n.`CodIndicator`, n.`CodAI`,
       n.`SimbolContCorectie`, n.`CodProgramCorectie`, n.`Explicatii`
  FROM `FX_NoteCAB` n
 WHERE NOT EXISTS (SELECT 1 FROM `FX_NoteCAB_Corectii` c WHERE c.`IDNC` = n.`IDNC`);

-- 4. The header loses the operation: keys first, then checks and indexes, then the columns.
ALTER TABLE `FX_NoteCAB` DROP FOREIGN KEY IF EXISTS `FK_FX_NoteCAB_Angajament`;
ALTER TABLE `FX_NoteCAB` DROP FOREIGN KEY IF EXISTS `FK_FX_NoteCAB_Operatiune`;
ALTER TABLE `FX_NoteCAB` DROP CONSTRAINT IF EXISTS `CK_FX_NoteCAB_Coloana`;
ALTER TABLE `FX_NoteCAB` DROP CONSTRAINT IF EXISTS `CK_FX_NoteCAB_Suma`;
ALTER TABLE `FX_NoteCAB`
  DROP INDEX IF EXISTS `UQ_FX_NoteCAB_Operatiune`,
  DROP INDEX IF EXISTS `IX_FX_NoteCAB_CodAngajament`,
  DROP INDEX IF EXISTS `IX_FX_NoteCAB_IDFXP`;
ALTER TABLE `FX_NoteCAB`
  DROP COLUMN IF EXISTS `IDFXP`,
  DROP COLUMN IF EXISTS `ReferintaTrezor`,
  DROP COLUMN IF EXISTS `NrDoc`,
  DROP COLUMN IF EXISTS `SimbolCont`,
  DROP COLUMN IF EXISTS `CodProgram`,
  DROP COLUMN IF EXISTS `DataOperInitiala`,
  DROP COLUMN IF EXISTS `Coloana`,
  DROP COLUMN IF EXISTS `Suma`,
  DROP COLUMN IF EXISTS `CodAngajament`,
  DROP COLUMN IF EXISTS `CodIndicator`,
  DROP COLUMN IF EXISTS `CodAI`,
  DROP COLUMN IF EXISTS `SimbolContCorectie`,
  DROP COLUMN IF EXISTS `CodProgramCorectie`,
  DROP COLUMN IF EXISTS `Explicatii`;

-- 5. Check: step 1 must now answer 0, and FX_NoteCAB_Corectii holds one row per old note.
SELECT (SELECT COUNT(*) FROM `FX_NoteCAB`) AS note,
       (SELECT COUNT(*) FROM `FX_NoteCAB_Corectii`) AS corectii;
