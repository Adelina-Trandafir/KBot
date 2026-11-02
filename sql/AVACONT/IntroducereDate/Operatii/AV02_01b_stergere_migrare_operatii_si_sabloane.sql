-- =====================================================================================
-- AV02-01b / AV01-18 -- DELETES WHAT AVC.Migrator WROTE for the operations and the templates, so the migration can be run again from a clean state.
-- RUN THIS ON THE DATABASE THE MIGRATOR WROTE TO (e.g. `000_DEMO`; select it first). NOT on `AVACONT_SURSA`: that one holds no data.
-- It covers the same templates as AV01_18b (that script is not needed as well).
--
-- WHAT GOES
--   operations   TZ_Debit, TZ_Credit, TZ_Oper (rows with IdOperatieAccess), TZ_Documente (rows with IdDocumentAccess, once no operation is left under them),
--                TZ_NoteC (rows with UN = NULL = not made in K-BOT, once no document is left under them)
--   templates    TZ_SablonOperatiiLinie + TZ_SablonOperatii and TZ_SablonSalariiLinie + TZ_SablonSalarii: ALL rows, except an operation template that an
--                operation still points at (TZ_Oper.IdSablon, RESTRICT); those stay and the last check shows how many.
-- WHAT STAYS    everything made in K-BOT (notes, documents and operations with their own numbers), journals, chart of accounts, partners, classifications, subunits.
--
-- The deletion runs in ONE transaction; no statement here can stop on a key (each one leaves out what is still pointed at).
--
-- THE LINKS OF THE INVOICES (EF_PrimiteOperatii.IdOperatie): when a migration reaches its end, its last statement points the migrated invoices at the NEW operation numbers.
-- A migration that did NOT reach COMMIT changed nothing there. Only if one DID finish once, run the optional step 1b BEFORE the deletion (it turns the links back into the
-- Access numbers, so the next migration can send them to the new ones again). Do NOT run it otherwise: it would change links that were never changed.
-- =====================================================================================

-- 0. What is there now (read only).
SELECT (SELECT COUNT(*) FROM `TZ_NoteC`)                                         AS Note,
       (SELECT COUNT(*) FROM `TZ_NoteC` WHERE `UN` IS NULL)                      AS NoteMigrate,
       (SELECT COUNT(*) FROM `TZ_Documente`)                                     AS Documente,
       (SELECT COUNT(*) FROM `TZ_Documente` WHERE `IdDocumentAccess` IS NOT NULL) AS DocumenteMigrate,
       (SELECT COUNT(*) FROM `TZ_Oper`)                                          AS Operatii,
       (SELECT COUNT(*) FROM `TZ_Oper` WHERE `IdOperatieAccess` IS NOT NULL)     AS OperatiiMigrate,
       (SELECT COUNT(*) FROM `TZ_Oper` WHERE `IdSablon` IS NOT NULL)             AS OperatiiCuSablon,
       (SELECT COUNT(*) FROM `TZ_SablonOperatii`)                                AS SabloaneOperatii,
       (SELECT COUNT(*) FROM `TZ_SablonSalarii`)                                 AS SabloaneSalarii;

-- 1b. OPTIONAL, only after a migration that reached COMMIT once (see above). Uncomment the statement to use it.
-- UPDATE `EF_PrimiteOperatii` e
--   JOIN `TZ_Oper` o       ON o.`IdOper` = e.`IdOperatie` AND o.`IdOperatieAccess` IS NOT NULL
--   JOIN `TZ_Documente` d  ON d.`IdDocument` = o.`IdDocument`
--   JOIN `TZ_NoteC` n      ON n.`IdNota` = d.`IdNota`
--   JOIN `TZ_Subunitati` s ON s.`IdSubunitate` = n.`IdSubunitate` AND s.`IdUnitateAccess` = e.`IdUnitate`
--    SET e.`IdOperatie` = o.`IdOperatieAccess`;

-- 2. The deletion, all or nothing.
START TRANSACTION;

DELETE FROM `TZ_Debit`  WHERE `IdOper` IN (SELECT `IdOper` FROM `TZ_Oper` WHERE `IdOperatieAccess` IS NOT NULL);
DELETE FROM `TZ_Credit` WHERE `IdOper` IN (SELECT `IdOper` FROM `TZ_Oper` WHERE `IdOperatieAccess` IS NOT NULL);
DELETE FROM `TZ_Oper`   WHERE `IdOperatieAccess` IS NOT NULL;
DELETE FROM `TZ_Documente` WHERE `IdDocumentAccess` IS NOT NULL AND `IdDocument` NOT IN (SELECT `IdDocument` FROM `TZ_Oper`);
DELETE FROM `TZ_NoteC`  WHERE `UN` IS NULL AND `IdNota` NOT IN (SELECT `IdNota` FROM `TZ_Documente`);

-- The operation templates an operation still points at stay; everything else goes (lines first).
DELETE FROM `TZ_SablonOperatiiLinie` WHERE `IdSablon` NOT IN (SELECT `IdSablon` FROM `TZ_Oper` WHERE `IdSablon` IS NOT NULL);
DELETE FROM `TZ_SablonOperatii`      WHERE `IdSablon` NOT IN (SELECT `IdSablon` FROM `TZ_Oper` WHERE `IdSablon` IS NOT NULL);
DELETE FROM `TZ_SablonSalariiLinie`;
DELETE FROM `TZ_SablonSalarii`;

COMMIT;

-- 3. The numbers start again (MariaDB keeps a number above the highest row that is left, so this is safe when K-BOT rows remain).
ALTER TABLE `TZ_Credit`  AUTO_INCREMENT = 1;
ALTER TABLE `TZ_Debit`   AUTO_INCREMENT = 1;
ALTER TABLE `TZ_Oper`    AUTO_INCREMENT = 1;
ALTER TABLE `TZ_Documente` AUTO_INCREMENT = 1;
ALTER TABLE `TZ_NoteC`   AUTO_INCREMENT = 1;
ALTER TABLE `TZ_SablonOperatiiLinie` AUTO_INCREMENT = 1;
ALTER TABLE `TZ_SablonOperatii`      AUTO_INCREMENT = 1;
ALTER TABLE `TZ_SablonSalariiLinie`  AUTO_INCREMENT = 1;
ALTER TABLE `TZ_SablonSalarii`       AUTO_INCREMENT = 1;

-- 4. What is left (read only). On a database with no K-BOT rows every figure is 0.
SELECT (SELECT COUNT(*) FROM `TZ_NoteC`)           AS Note,
       (SELECT COUNT(*) FROM `TZ_Documente`)       AS Documente,
       (SELECT COUNT(*) FROM `TZ_Oper`)            AS Operatii,
       (SELECT COUNT(*) FROM `TZ_Debit`)           AS Debit,
       (SELECT COUNT(*) FROM `TZ_Credit`)          AS Credit,
       (SELECT COUNT(*) FROM `TZ_SablonOperatii`)  AS SabloaneOperatii,
       (SELECT COUNT(*) FROM `TZ_SablonSalarii`)   AS SabloaneSalarii;
