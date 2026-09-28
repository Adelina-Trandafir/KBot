-- =====================================================================================
-- Slice 0088-02 -- repair of FX_PDF_SEMNATURI on a database where sql/0088 ran only in part:
-- the IDNC column is there but the slice 0079 check («IDREV or IDORDP») was kept, so every
-- signed correction note is refused with
--   4025 (23000): CONSTRAINT `CK_FX_PDF_SEMNATURI_DOC` failed
-- Seen on 014_SCSV, 28.09.2026. Safe to run again; run it on EVERY unit database + AVACONT_SURSA.
--
-- Run the statements ONE BY ONE and read the answer of each: if one fails, the ones after it
-- still need to run.
-- =====================================================================================

-- 1. What is there now. Before the repair: CHECK_CLAUSE without IDNC.
SELECT CONSTRAINT_NAME, CHECK_CLAUSE
  FROM information_schema.CHECK_CONSTRAINTS
 WHERE CONSTRAINT_SCHEMA = DATABASE() AND TABLE_NAME = 'FX_PDF_SEMNATURI';

-- 2. The column (no-op when already there).
ALTER TABLE `FX_PDF_SEMNATURI`
  ADD COLUMN IF NOT EXISTS `IDNC` int(11) NULL DEFAULT NULL AFTER `IDORDP`;
ALTER TABLE `FX_PDF_SEMNATURI`
  ADD INDEX IF NOT EXISTS `IX_FX_PDF_SEMNATURI_IDNC` (`IDNC` ASC, `Camp` ASC);

-- 3. The check: old one out, «exactly one of three» in.
ALTER TABLE `FX_PDF_SEMNATURI` DROP CONSTRAINT IF EXISTS `CK_FX_PDF_SEMNATURI_DOC`;
ALTER TABLE `FX_PDF_SEMNATURI`
  ADD CONSTRAINT `CK_FX_PDF_SEMNATURI_DOC`
    CHECK ((`IDREV` IS NOT NULL) + (`IDORDP` IS NOT NULL) + (`IDNC` IS NOT NULL) = 1);

-- 4. The foreign key to the note (needs FX_NoteCAB from sql/0088). Skip it if it already exists
--    (error 1826 «Duplicate foreign key constraint name»).
ALTER TABLE `FX_PDF_SEMNATURI`
  ADD CONSTRAINT `FK_FX_PDF_SEMNATURI_NC` FOREIGN KEY (`IDNC`)
    REFERENCES `FX_NoteCAB` (`IDNC`) ON DELETE CASCADE ON UPDATE RESTRICT;

-- 5. Check again: CHECK_CLAUSE must now name IDNC.
SELECT CONSTRAINT_NAME, CHECK_CLAUSE
  FROM information_schema.CHECK_CONSTRAINTS
 WHERE CONSTRAINT_SCHEMA = DATABASE() AND TABLE_NAME = 'FX_PDF_SEMNATURI';
