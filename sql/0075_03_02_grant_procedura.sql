-- =====================================================================
--  Slice 0075-03, fix after the first real run (23.09.2026).
--  Run on the K-BOT server, once, after sql/0075_03_provizionare.sql, AS AN
--  ACCOUNT WITH GLOBAL PRIVILEGES WITH GRANT OPTION -- it becomes the definer and
--  the procedure runs with its rights. Created as `Admin`@`%` on 23.09.2026;
--  if Admin is ever dropped or trimmed (pass 0075-06), recreate it first.
--
--  WHY. Step 7 of the provisioning job failed with 1044 on
--      GRANT ... ON `111_TRND`.* TO '<email>'@'%'
--  In `GRANT ... ON db.*` MariaDB treats the database name as a PATTERN,
--  and a grantor's database-level rights then count only when they were
--  granted on that very same string. The provisioning account's rights are
--  on the pattern `1__\_____`, which is a different string -- good for
--  CREATE DATABASE, CREATE TABLE and INSERT (steps 2-6 worked), never for
--  passing rights on. The only grant that would satisfy the check is a
--  GLOBAL one, which would make the account a superuser.
--
--  SO: one procedure, owned by root (SQL SECURITY DEFINER), that does that
--  single GRANT and nothing else, after checking its two arguments. The
--  provisioning account may EXECUTE it and loses the GRANT OPTION it no
--  longer needs.
-- =====================================================================

USE `AVACONT_COMUN`;

DROP PROCEDURE IF EXISTS `proc_Provizionare_Grant`;

DELIMITER $$
CREATE DEFINER = CURRENT_USER PROCEDURE `proc_Provizionare_Grant`(
    IN p_db   VARCHAR(64),
    IN p_user VARCHAR(80)
)
    MODIFIES SQL DATA
    SQL SECURITY DEFINER
    COMMENT 'Slice 0075-03: GRANT SELECT, INSERT, UPDATE, DELETE, EXECUTE ON <1nn_SSSS>.* TO <email>@%'
BEGIN
    -- Only the shape the provisioning job builds. BINARY: case-sensitive,
    -- like the database names themselves (lower_case_table_names = 0).
    IF p_db IS NULL OR CAST(p_db AS BINARY) NOT REGEXP '^1[1-9]{2}_[A-Z]{4}$' THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'proc_Provizionare_Grant: baza nu are forma 1nn_SSSS';
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.SCHEMATA WHERE SCHEMA_NAME = p_db) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'proc_Provizionare_Grant: baza nu exista';
    END IF;

    -- Only an e-mail-shaped account on '%', i.e. a unit user -- never a service
    -- account such as AVACONT or Admin.
    IF p_user IS NULL OR CAST(p_user AS BINARY)
           NOT REGEXP '^[A-Za-z0-9._%+-]+@[A-Za-z0-9-]+(\\.[A-Za-z0-9-]+)+$' THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'proc_Provizionare_Grant: utilizatorul nu are forma unei adrese de e-mail';
    END IF;
    IF NOT EXISTS (SELECT 1 FROM mysql.user WHERE User = p_user AND Host = '%') THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'proc_Provizionare_Grant: contul nu exista';
    END IF;

    -- The underscore escaped, so the grant names THIS database and not a
    -- pattern that would also match e.g. 111XTRND. QUOTE() escapes the user.
    SET @kbot_grant_sql = CONCAT(
        'GRANT SELECT, INSERT, UPDATE, DELETE, EXECUTE ON `',
        REPLACE(p_db, '_', '\\_'),
        '`.* TO ', QUOTE(p_user), '@''%''');
    PREPARE kbot_grant_stmt FROM @kbot_grant_sql;
    EXECUTE kbot_grant_stmt;
    DEALLOCATE PREPARE kbot_grant_stmt;
END$$
DELIMITER ;

GRANT EXECUTE ON PROCEDURE `AVACONT_COMUN`.`proc_Provizionare_Grant`
   TO 'kbot_provizionare'@'localhost';

-- No longer needed: the procedure does the granting.
REVOKE GRANT OPTION ON `1__\_____`.* FROM 'kbot_provizionare'@'localhost';

-- Check:
-- SHOW GRANTS FOR 'kbot_provizionare'@'localhost';
--   the `1__\_____` line must no longer end in WITH GRANT OPTION, and a line
--   GRANT EXECUTE ON PROCEDURE `AVACONT_COMUN`.`proc_Provizionare_Grant` ... must be there.
