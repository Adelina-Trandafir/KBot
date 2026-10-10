-- AD_07: AD_AlteDoc has no FelDoc column any more. The Access FelDoc text (e.g. 'C/Val. luna ...') is an explanation:
-- it is copied into Explicatie (only where Explicatie is empty), then the column is dropped. NrDoc stays untouched.
-- Run on EACH ADE database (000_DEMO, ...). Safe to run twice: when FelDoc is already gone nothing happens.
-- Nothing is lost: the copy runs first, in the same script. Back up AD_AlteDoc before the first run.
SET @has_feldoc = (SELECT COUNT(*) FROM information_schema.COLUMNS
                   WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'AD_AlteDoc' AND COLUMN_NAME = 'FelDoc');

SET @copy = IF(@has_feldoc > 0,
  'UPDATE `AD_AlteDoc` SET `Explicatie` = `FelDoc` WHERE (`Explicatie` IS NULL OR `Explicatie` = '''') AND `FelDoc` IS NOT NULL AND `FelDoc` <> ''''',
  'DO 0');
PREPARE stmt FROM @copy; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @drop = IF(@has_feldoc > 0, 'ALTER TABLE `AD_AlteDoc` DROP COLUMN `FelDoc`', 'DO 0');
PREPARE stmt FROM @drop; EXECUTE stmt; DEALLOCATE PREPARE stmt;
