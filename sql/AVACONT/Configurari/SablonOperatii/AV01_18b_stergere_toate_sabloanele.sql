-- =====================================================================================
-- Slice AV01-18 -- DELETES EVERY TEMPLATE of ONE unit database: the operation templates AND the salary templates.
-- RUN THIS ON THE UNIT DATABASE (select it first; once per unit database). NOT on `AVACONT_SURSA`: that one holds no data.
--
-- WHY (operator, 02.11.2026): the migrated templates carry no subunit; after AV01_18 the migrator writes each template into the subunit it comes from, so the
-- old ones are deleted and migrated again (AVC.Migrator, steps «Sabloane de operatii» and «Sabloane de salarii»).
--
-- WHAT GOES:   TZ_SablonOperatiiLinie, TZ_SablonOperatii, TZ_SablonSalariiLinie, TZ_SablonSalarii  (all rows).
-- WHAT STAYS:  journals, chart of accounts, partners, classifications, notes, documents, operations.
--
-- THE ONE THING THAT CAN STOP IT: an operation that was made FROM a template points at it (TZ_Oper.IdSablon, RESTRICT). The migrated operations point at none.
-- Run the check first; when it shows 0, run the rest. When it shows more, either keep the templates or run the optional UPDATE below (it only forgets
-- which template an operation came from; nothing else of the operation changes).
-- =====================================================================================

-- 1. Check (read only). Must be 0.
SELECT COUNT(*) AS OperatiiFacuteDinSablon FROM `TZ_Oper` WHERE `IdSablon` IS NOT NULL;

-- 1b. OPTIONAL, only when the check above is not 0 and the operator agrees:
-- UPDATE `TZ_Oper` SET `IdSablon` = NULL WHERE `IdSablon` IS NOT NULL;

-- 2. The deletion, all or nothing.
START TRANSACTION;
DELETE FROM `TZ_SablonOperatiiLinie`;
DELETE FROM `TZ_SablonOperatii`;
DELETE FROM `TZ_SablonSalariiLinie`;
DELETE FROM `TZ_SablonSalarii`;
COMMIT;

-- 3. The numbers start again from 1.
ALTER TABLE `TZ_SablonOperatiiLinie` AUTO_INCREMENT = 1;
ALTER TABLE `TZ_SablonOperatii` AUTO_INCREMENT = 1;
ALTER TABLE `TZ_SablonSalariiLinie` AUTO_INCREMENT = 1;
ALTER TABLE `TZ_SablonSalarii` AUTO_INCREMENT = 1;

-- 4. Check (read only). All four must be 0.
SELECT (SELECT COUNT(*) FROM `TZ_SablonOperatii`)       AS SabloaneOperatii,
       (SELECT COUNT(*) FROM `TZ_SablonOperatiiLinie`)  AS LiniiOperatii,
       (SELECT COUNT(*) FROM `TZ_SablonSalarii`)        AS SabloaneSalarii,
       (SELECT COUNT(*) FROM `TZ_SablonSalariiLinie`)   AS LiniiSalarii;
