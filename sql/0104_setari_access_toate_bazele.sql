-- =====================================================================================
-- Slice 0104 -- `Setari.Access` on EVERY existing unit database.
--
-- Walks AVACONT_COMUN.Unitati (its DC column IS the database name) and, in each database that
-- exists, creates `Setari` if it is missing and adds the row `Access` when it is not there
-- (INSERT IGNORE: a row somebody already set keeps its value -- running this twice, or after
-- changing a value, undoes nothing). A DC with no database of that name is skipped and named
-- in the report. See 0104_setari_access.sql for what the row means.
--
-- THE DEFAULT FOR EXISTING UNITS IS 1 (v_access below). Every unit that exists today was served
-- the K-BOT package WITH the Migrator and the Access code, and the units that exist today are the
-- Access clients; giving them 0 would switch their Access features off. Set v_access to '0'
-- before running if that is not so, or change single units afterwards:
--   UPDATE `<unit db>`.`Setari` SET Valoare = '0' WHERE Cheie = 'Access';
--
-- RUN WITH the mysql command-line client or HeidiSQL (both understand DELIMITER), as an account
-- that may CREATE / INSERT in every unit database (root). The last line prints the report.
-- AVACONT_SURSA is NOT in Unitati: it gets its row from 0104_setari_access.sql.
-- =====================================================================================

SET NAMES utf8mb4;

DELIMITER $$

BEGIN NOT ATOMIC
  DECLARE done     INT DEFAULT 0;
  DECLARE v_db     VARCHAR(64);
  DECLARE v_exists INT;
  DECLARE v_access VARCHAR(1) DEFAULT '1';
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
        '(''Access'', ''Clientul folosește și aplicația Access (1 = da, 0 = nu)'', ''', v_access, ''', ''int'')');
      PREPARE st FROM @s; EXECUTE st; DEALLOCATE PREPARE st;

      SET @raport = CONCAT(@raport, v_db, ': ok; ');
    END IF;
  END LOOP unitati;
  CLOSE cur;

  SELECT @raport AS raport;
END$$

DELIMITER ;
