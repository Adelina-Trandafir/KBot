-- SLICE-VE01-01
-- Executare manuala in baza care contine Consultanti_LOG.
-- Respinge toate versiunile numerice major.minor.patch mai mici decat 2.6.5.
-- Componentele sunt comparate numeric, nu alfabetic.
-- NULL si valorile din afara formatului major.minor.patch nu sunt blocate aici.
-- Verificarea se aplica numai daca Versiune este trimisa in INSERT.
-- SIGNAL respinge INSERT-ul; nu inchide automat conexiunea MariaDB.

DELIMITER $$

CREATE OR REPLACE DEFINER=`Admin`@`%`
TRIGGER `Consultanti_LOG__ADD_BEFORE`
BEFORE INSERT ON `Consultanti_LOG`
FOR EACH ROW
BEGIN
    DECLARE V_VERSIUNE VARCHAR(255);
    DECLARE V_MAJOR BIGINT UNSIGNED;
    DECLARE V_MINOR BIGINT UNSIGNED;
    DECLARE V_PATCH BIGINT UNSIGNED;

    SET V_VERSIUNE = TRIM(NEW.`Versiune`);

    IF V_VERSIUNE REGEXP '^[0-9]+[.][0-9]+[.][0-9]+$' THEN
        SET V_MAJOR = CAST(SUBSTRING_INDEX(V_VERSIUNE, '.', 1) AS UNSIGNED);
        SET V_MINOR = CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(V_VERSIUNE, '.', 2), '.', -1) AS UNSIGNED);
        SET V_PATCH = CAST(SUBSTRING_INDEX(V_VERSIUNE, '.', -1) AS UNSIGNED);

        IF V_MAJOR < 2
           OR (V_MAJOR = 2 AND V_MINOR < 6)
           OR (V_MAJOR = 2 AND V_MINOR = 6 AND V_PATCH < 5) THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Versiune expirata. Versiunea minima acceptata este 2.6.5.';
        END IF;
    END IF;

    SET NEW.`ConnectionID` = FLOOR(1000000 + RAND() * 9000000);
END$$

DELIMITER ;
