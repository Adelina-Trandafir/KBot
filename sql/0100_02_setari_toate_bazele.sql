-- =====================================================================================
-- Slice 0100-02 -- `Setari` on EVERY existing unit database, with the default values.
--
-- Walks AVACONT_COMUN.Unitati (its DC column IS the database name: '000_DEMO', '005_CEVM', ...)
-- and, in each database that exists:
--   1. creates `Setari` if it is missing;
--   2. adds the default rows that are missing (INSERT IGNORE: a row that is already there keeps
--      the value somebody set -- running this twice, or after changing a value, undoes nothing).
-- A DC with no database of that name is skipped and named in the report.
--
-- Defaults:  Multithread = 0 (off)   Multithread_Max = 1
-- To switch a unit on afterwards, see the end of 0100_02_setari.sql.
--
-- RUN WITH the mysql command-line client or HeidiSQL (both understand DELIMITER), as an account
-- that may CREATE / INSERT in every unit database (root). The last line prints the report.
-- AVACONT_SURSA is NOT in Unitati: it gets its table from 0100_02_setari.sql.
-- =====================================================================================

SET NAMES utf8mb4;

DELIMITER $$

BEGIN NOT ATOMIC
  DECLARE done    INT DEFAULT 0;
  DECLARE v_db    VARCHAR(64);
  DECLARE v_exists INT;
  DECLARE cur CURSOR FOR SELECT DC FROM `AVACONT_COMUN`.`Unitati` ORDER BY DC;
  DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = 1;

  SET @raport = '';

  OPEN cur;
  unitati: LOOP
    FETCH cur INTO v_db;
    IF done = 1 THEN LEAVE unitati; END IF;

    SELECT COUNT(*) INTO v_exists FROM information_schema.SCHEMATA WHERE SCHEMA_NAME = v_db;
    IF v_exists = 0 THEN
      SET @raport = CONCAT(@raport, v_db, ': NU EXISTA (sarita); ');
    ELSE
      SET @s = CONCAT(
        'CREATE TABLE IF NOT EXISTS `', v_db, '`.`Setari` (',
        '`Cheie` varchar(64) NOT NULL COMMENT ''Slice 0100-02: the setting key (ASCII, read by code)'', ',
        '`TextVizibil` varchar(255) NOT NULL COMMENT ''The text the operator may see for it'', ',
        '`Valoare` varchar(255) NULL DEFAULT NULL COMMENT ''The value, as text; its kind is Tip'', ',
        '`Tip` enum(''int'',''date'',''text'') NOT NULL DEFAULT ''text'' COMMENT ''Which kind Valoare is'', ',
        'PRIMARY KEY (`Cheie`)) ',
        'ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci');
      PREPARE st FROM @s; EXECUTE st; DEALLOCATE PREPARE st;

      SET @s = CONCAT(
        'INSERT IGNORE INTO `', v_db, '`.`Setari` (`Cheie`, `TextVizibil`, `Valoare`, `Tip`) VALUES ',
        '(''Multithread'', ''Descărcare pe mai multe taburi (1 = permisă, 0 = oprită)'', ''0'', ''int''), ',
        '(''Multithread_Max'', ''Numărul maxim de taburi folosite deodată'', ''1'', ''int'')');
      PREPARE st FROM @s; EXECUTE st; DEALLOCATE PREPARE st;

      SET @raport = CONCAT(@raport, v_db, ': ok; ');
    END IF;
  END LOOP unitati;
  CLOSE cur;

  SELECT @raport AS raport;
END$$

DELIMITER ;
