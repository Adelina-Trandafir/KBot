-- SLICE-AD20-15. Run MANUALLY in the selected ADECHIT unit database.
-- Requested period: November 2026 (the current month on 2026-11-05).
-- Applies to every subunit of DATABASE(), only for that accounting month (IDL).
-- Run the WHOLE file in a new HeidiSQL/MariaDB-client query session supporting DELIMITER.
-- Close ADECHIT association/payment windows first; reopen after this script succeeds.
-- The successful block COMMITS the deletion. The agent has NOT executed this file.
--
-- Deletes only the exact association/payment/document triples recorded in
-- AD_EXTRASE_ASOCIERI. Preserves child-payer links, payer history, attendance,
-- manually entered documents, cash receipts, original statements and upload history.
-- Removes the statement-payment reservations so those receipts can be associated again.
-- Refuses closed months, affected saved situations, or inconsistent links.
-- No persistent DDL, no foreign-key disabling, no AUTO_INCREMENT reset.
-- Temporary tables do not implicitly commit in MariaDB:
-- https://mariadb.com/docs/server/reference/sql-statements/transactions/sql-statements-that-cause-an-implicit-commit

DELIMITER $$

BEGIN NOT ATOMIC
    DECLARE V_YEAR INT DEFAULT 2026;
    DECLARE V_MONTH INT DEFAULT 11;
    DECLARE V_STARTED BOOLEAN DEFAULT FALSE;
    DECLARE V_LOCK_ID INT DEFAULT NULL;
    DECLARE V_TARGETS BIGINT DEFAULT 0;
    DECLARE V_EXTRACTS BIGINT DEFAULT 0;
    DECLARE V_ASSOCIATIONS_DELETED BIGINT DEFAULT 0;
    DECLARE V_DOCUMENTS_DELETED BIGINT DEFAULT 0;
    DECLARE V_PAYMENTS_DELETED BIGINT DEFAULT 0;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        IF V_STARTED THEN
            ROLLBACK;
            DROP TEMPORARY TABLE IF EXISTS AD20_15_RESET_TARGETS;
        END IF;
        RESIGNAL;
    END;

    IF DATABASE() IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Selectați întâi baza de date a unității ADECHIT.';
    END IF;
    IF @@in_transaction <> 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Rulați scriptul într-o sesiune nouă, fără tranzacție deja deschisă.';
    END IF;
    IF (SELECT COUNT(*) FROM information_schema.TABLES
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME IN ('AD_Lock', 'AD_EXTRASE_ASOCIERI', 'AD_AlteDoc', 'AD_Plati')
          AND ENGINE = 'InnoDB') <> 4 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Lipsesc tabelele ADE necesare sau acestea nu sunt InnoDB. Nu s-a șters nimic.';
    END IF;

    START TRANSACTION;
    SET V_STARTED = TRUE;

    -- Same first lock as every ADE application mutation; protects all subunits.
    SELECT ID INTO V_LOCK_ID FROM AD_Lock WHERE ID = 0 FOR UPDATE;
    IF V_LOCK_ID IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Lipsește lacătul ADE al unității (AD_Lock.ID=0). Nu s-a șters nimic.';
    END IF;

    DROP TEMPORARY TABLE IF EXISTS AD20_15_RESET_TARGETS;
    CREATE TEMPORARY TABLE AD20_15_RESET_TARGETS (
        SUBUNIT_ID INT NOT NULL,
        IDFXE INT NOT NULL,
        IDP INT NOT NULL,
        IDZ INT NOT NULL,
        IDL INT NOT NULL,
        IDPL INT NOT NULL,
        IDA INT NOT NULL,
        PRIMARY KEY (IDFXE, IDP),
        UNIQUE KEY UQ_PAYMENT (IDPL),
        UNIQUE KEY UQ_DOCUMENT (IDA)
    ) ENGINE = InnoDB;

    INSERT INTO AD20_15_RESET_TARGETS (SUBUNIT_ID, IDFXE, IDP, IDZ, IDL, IDPL, IDA)
    SELECT A.SUBUNIT_ID, A.IDFXE, A.IDP, A.IDZ, A.IDL, A.IDPL, A.IDA
    FROM AD_EXTRASE_ASOCIERI A
    JOIN AD_LunaD M ON M.SubunitId = A.SUBUNIT_ID AND M.IDL = A.IDL
    WHERE M.Anul = V_YEAR AND M.Luna = V_MONTH;

    SELECT COUNT(*), COUNT(DISTINCT IDFXE) INTO V_TARGETS, V_EXTRACTS
    FROM AD20_15_RESET_TARGETS;

    IF EXISTS (
        SELECT 1 FROM AD20_15_RESET_TARGETS T
        LEFT JOIN AD_Plati P ON P.SubunitId = T.SUBUNIT_ID AND P.IDPL = T.IDPL
        LEFT JOIN AD_AlteDoc D ON D.SubunitId = T.SUBUNIT_ID AND D.IDA = T.IDA
        WHERE P.IDPL IS NULL OR D.IDA IS NULL
           OR NOT (P.IDP <=> T.IDP) OR NOT (P.IDZ <=> T.IDZ)
           OR NOT (P.IDL <=> T.IDL) OR NOT (P.TIP <=> 1)
           OR NOT (D.IDPL <=> T.IDPL)
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Legături neconcordante între extras, plată și AlteDoc. Nu s-a șters nimic.';
    END IF;

    IF EXISTS (
        SELECT 1 FROM AD20_15_RESET_TARGETS T
        JOIN AD_LunaD M ON M.SubunitId = T.SUBUNIT_ID AND M.IDL = T.IDL
        WHERE COALESCE(M.Inchisa, 1) <> 0
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Luna vizată este închisă. Nu s-a șters nimic; situațiile salvate necesită tratare separată.';
    END IF;

    -- A later saved month can also include the selected payment in its opening balance.
    IF EXISTS (
        SELECT 1 FROM AD20_15_RESET_TARGETS T
        JOIN AD_SS_Buget S ON S.SubunitId = T.SUBUNIT_ID AND S.IDP = T.IDP
        JOIN AD_LunaD M ON M.SubunitId = S.SubunitId AND M.IDL = S.IDL
        WHERE M.Anul > V_YEAR OR (M.Anul = V_YEAR AND M.Luna >= V_MONTH)
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Există situații salvate afectate, în luna vizată sau ulterior. Nu s-a șters nimic.';
    END IF;

    IF EXISTS (
        SELECT 1 FROM AD20_15_RESET_TARGETS T
        JOIN AD_Chitante C ON C.SubunitId = T.SUBUNIT_ID AND C.IDPL = T.IDPL
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'O plată vizată are și o chitanță legată. Nu s-a șters nimic.';
    END IF;

    IF EXISTS (
        SELECT 1 FROM AD20_15_RESET_TARGETS T
        JOIN AD_AlteDoc D ON D.SubunitId = T.SUBUNIT_ID AND D.IDPL = T.IDPL
        WHERE D.IDA <> T.IDA
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'O plată vizată are și alte documente legate. Nu s-a șters nimic.';
    END IF;

    -- Release reservations first, then remove only their own documents and payments.
    DELETE A FROM AD_EXTRASE_ASOCIERI A
    JOIN AD20_15_RESET_TARGETS T
      ON T.SUBUNIT_ID = A.SUBUNIT_ID AND T.IDFXE = A.IDFXE AND T.IDP = A.IDP;
    SET V_ASSOCIATIONS_DELETED = ROW_COUNT();

    DELETE D FROM AD_AlteDoc D
    JOIN AD20_15_RESET_TARGETS T
      ON T.SUBUNIT_ID = D.SubunitId AND T.IDA = D.IDA AND T.IDPL = D.IDPL;
    SET V_DOCUMENTS_DELETED = ROW_COUNT();

    DELETE P FROM AD_Plati P
    JOIN AD20_15_RESET_TARGETS T
      ON T.SUBUNIT_ID = P.SubunitId AND T.IDPL = P.IDPL;
    SET V_PAYMENTS_DELETED = ROW_COUNT();

    IF V_ASSOCIATIONS_DELETED <> V_TARGETS
       OR V_DOCUMENTS_DELETED <> V_TARGETS OR V_PAYMENTS_DELETED <> V_TARGETS THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Numărul ștergerilor nu corespunde selecției; întreaga operație a fost anulată.';
    END IF;

    DROP TEMPORARY TABLE AD20_15_RESET_TARGETS;
    COMMIT;
    SET V_STARTED = FALSE;

    SELECT DATABASE() AS BAZA_CURENTA, V_YEAR AS ANUL, V_MONTH AS LUNA,
           V_PAYMENTS_DELETED AS PLATI_STERSE,
           V_DOCUMENTS_DELETED AS DOCUMENTE_ALTEDOC_STERSE,
           V_ASSOCIATIONS_DELETED AS REPARTIZARI_ELIBERATE,
           V_EXTRACTS AS INCASARI_DISPONIBILE_PENTRU_REPRELUARE,
           'Legăturile copil–plătitor și istoricul plătitorilor au fost păstrate.' AS REZULTAT;
END$$

DELIMITER ;
