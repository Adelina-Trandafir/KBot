-- Reset manual al tuturor datelor VERCON, EXCLUSIV din 000_DEMO.
-- Include documente, facturi, corectii, setari, audit si markerii de import.
-- TRUNCATE goleste datele si reseteaza AUTO_INCREMENT; nu exista rollback.
-- Parteneri si AVACONT_COMUN nu sunt atinse. Nu creeaza proceduri/tabele.
-- Opriti operatiile VERCON pe 000_DEMO in timpul resetului.
-- Executati intregul fisier ca script, pe o singura conexiune MariaDB.
-- Lista se citeste din schema reala: include si tabelele noi daca exista.
-- Documentatie sintaxa:
-- https://mariadb.com/docs/server/reference/sql-statements/programmatic-compound-statements/using-compound-statements-outside-of-stored-programs
-- https://mariadb.com/docs/server/reference/sql-statements/prepared-statements/execute-immediate

DELIMITER //
BEGIN NOT ATOMIC
    DECLARE VC_RESET_DONE BOOLEAN DEFAULT FALSE;
    DECLARE VC_RESET_TABLE VARCHAR(64);
    DECLARE VC_RESET_COUNT INT DEFAULT 0;
    DECLARE VC_RESET_PREVIOUS_FK INT DEFAULT 1;
    DECLARE VC_RESET_TABLES CURSOR FOR
        SELECT TABLE_NAME
        FROM information_schema.TABLES
        WHERE TABLE_SCHEMA = '000_DEMO'
          AND TABLE_TYPE = 'BASE TABLE'
          AND LEFT(TABLE_NAME, 3) = 'VC_'
        ORDER BY TABLE_NAME;
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET VC_RESET_DONE = TRUE;
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        SET SESSION FOREIGN_KEY_CHECKS = VC_RESET_PREVIOUS_FK;
        RESIGNAL;
    END;

    SET VC_RESET_PREVIOUS_FK = @@SESSION.FOREIGN_KEY_CHECKS;
    SET SESSION FOREIGN_KEY_CHECKS = 0;
    OPEN VC_RESET_TABLES;
    VC_RESET_LOOP: LOOP
        FETCH VC_RESET_TABLES INTO VC_RESET_TABLE;
        IF VC_RESET_DONE THEN
            LEAVE VC_RESET_LOOP;
        END IF;
        EXECUTE IMMEDIATE CONCAT(
            'TRUNCATE TABLE `000_DEMO`.`', REPLACE(VC_RESET_TABLE, '`', '``'), '`'
        );
        SET VC_RESET_COUNT = VC_RESET_COUNT + 1;
    END LOOP;
    CLOSE VC_RESET_TABLES;
    SET SESSION FOREIGN_KEY_CHECKS = VC_RESET_PREVIOUS_FK;

    SELECT '000_DEMO' AS BAZA, VC_RESET_COUNT AS TABELE_VERCON_GOLITE;
END//
DELIMITER ;
