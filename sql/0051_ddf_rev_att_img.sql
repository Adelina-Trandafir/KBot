-- =====================================================================================
-- Slice 0051 -- attachment bytes for the DDF family (FX_DDF_REV_ATT_IMG).
--
-- WARNING -- APPLY TO EVERY UNIT DATABASE, plus AVACONT_SURSA.
--   One MariaDB database = one unit (see routes/forexe/ord.py, the "Scope" note), so this
--   file runs once for EACH unit database, not once per VPS. It also runs on AVACONT_SURSA
--   so databases created from now on carry the table from birth and schema_sync does not
--   report a difference forever -- the same rule as sql/0048_alegeri_unitate.sql,
--   sql/0049_receptii_stergere.sql and sql/0049_ord_att_img.sql.
--
--   ON AVACONT_SURSA: see the section at the foot of this file. The primary key stays
--   `INT NOT NULL` -- WITHOUT `AUTO_INCREMENT`. The exemption from slice 0048-01 section 3.1
--   applies here in full: AUTO_INCREMENT on the source schema disarms a migration safety
--   gate (a row arriving without a key would silently receive the next number instead of
--   raising).
--
-- =====================================================================================
-- WHY THIS TABLE EXISTS
-- =====================================================================================
-- `FX_DDF_REV_ATT.DateFisier` is `longtext` and holds base64 -- that is how Access wrote it.
-- K-BOT stores the RAW bytes in a separate table instead, following exactly the pattern of
-- `FX_DDF_PDF` (slice 0041) and `FX_ORD_ATT_IMG` (slice 0049): one row per parent, a SHA-256
-- sum for integrity, ON DELETE CASCADE so the attachment row and its bytes cannot diverge.
-- `FX_DDF_REV_ATT.DateFisier` stays in place but is NEITHER written NOR read from now on
-- (slice 0051 decision D12).
--
-- `NumeFisier` lives HERE, not in `FX_DDF_REV_ATT`: Access carried the file name only on
-- `tmpFX_DDF_REV_ATT`, and `FX_DDF_REV_ATT` in MariaDB has NO such column. Verified in
-- MariaDB_Schema/000_DEMO.sql lines 216-233 -- the columns there are IdRevAtt, IDDF, IDREV,
-- IDVBNET, CaleFisier, PrtScr, DateFisier, DataAdugare, DataModificare, and nothing else.
--
-- SHAPE IS DELIBERATELY IDENTICAL to FX_ORD_ATT_IMG, column for column, including
-- `TipMime` and including the UNIQUE index. Do not let the two drift apart.
--
-- THE UNIQUE INDEX IS LOAD-BEARING, NOT DECORATION. One attachment row is one file, and the
-- upload path writes with `INSERT ... ON DUPLICATE KEY UPDATE`, which is what makes a
-- re-upload REPLACE the bytes instead of appending a second row. Remove the uniqueness and
-- every re-upload silently accumulates duplicates, after which the reader's
-- `... WHERE IdRevAtt = %s LIMIT 1` picks an arbitrary one. Confirmed on the ORD side at
-- PYTHON/routes/forexe/ord_edit.py lines 1816-1823 (write) and 1720 / 1736 (read).
--
-- =====================================================================================
-- !!! UNVERIFIED !!! -- READ BEFORE RUNNING
-- =====================================================================================
-- As with sql/0041_fx_pdf_tables.sql and sql/0049_ord_att_img.sql: the NAME AND TYPE of the
-- parent primary key must be confirmed on a LIVE database before the FOREIGN KEY line is
-- considered final. What is known and from where: `FX_DDF_REV_ATT.IdRevAtt` -- the name and
-- the type `int(11)` are read from MariaDB_Schema/000_DEMO.sql (a real dump), NOT guessed.
-- It could not be checked against production; neither MariaDB nor the Python server is
-- reachable from the development machine.
--
-- PROBE ONE -- the parent key's real type (replace <DB> with the unit database name):
--
--   SELECT TABLE_NAME, COLUMN_NAME, COLUMN_TYPE, COLUMN_KEY
--     FROM information_schema.COLUMNS
--    WHERE TABLE_SCHEMA = '<DB>'
--      AND TABLE_NAME = 'FX_DDF_REV_ATT' AND COLUMN_NAME = 'IdRevAtt';
--
-- Expected: one row, COLUMN_KEY = 'PRI', COLUMN_TYPE = 'int(11)'. If the answer differs --
-- another name, another type, or the column is not the primary key -- STOP and report; do
-- not guess.
--
-- PROBE TWO -- does any live unit already hold rows in the parent table? The claim "the
-- table is empty" comes ONLY from the demo dump (AUTO_INCREMENT = 1 there).
--
--   SELECT COUNT(*) AS randuri,
--          SUM(CASE WHEN DateFisier IS NOT NULL AND DateFisier <> '' THEN 1 ELSE 0 END)
--            AS cu_continut,
--          SUM(CASE WHEN PrtScr = 1 THEN 1 ELSE 0 END) AS print_screen
--     FROM <DB>.FX_DDF_REV_ATT;
--
-- If `cu_continut` > 0 on any unit then `DateFisier` is NOT dead and the migration question
-- reopens (the old bytes would have to be moved here before the column is ignored).
--
-- Check the packet ceiling too (one blob plus the statement header must fit in a single
-- packet; nginx is at 20m today):
--
--   SHOW VARIABLES LIKE 'max_allowed_packet';   -- want >= 32M
--
-- =====================================================================================

-- -------------------------------------------------------------------------------------
-- FX_DDF_REV_ATT_IMG -- the bytes of one DDF revision attachment.
--
-- One row per `FX_DDF_REV_ATT` (UNIQUE key on IdRevAtt): a re-upload REPLACES the row, it
-- does not add one. No history, exactly as in FX_DDF_PDF / FX_ORD_PDF / FX_ORD_ATT_IMG.
--
-- Continut = LONGBLOB, not MEDIUMBLOB: MEDIUMBLOB stops at 16,777,215 bytes, which is right
-- on the practical ceiling. The ceiling is imposed in Flask and in nginx, not by the column
-- type.
--
-- ON DELETE CASCADE: deleting the attachment row deletes the bytes. And, through the chain
-- FX_DDF -> FX_DDF_REV -> FX_DDF_REV_ATT -> here, deleting the document deletes them all.
-- -------------------------------------------------------------------------------------
CREATE TABLE `FX_DDF_REV_ATT_IMG` (
  `IDIMG`      int(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `IdRevAtt`   int(11)          NOT NULL,
  `NumeFisier` varchar(255)     NOT NULL,   -- the file name the operator chose
  `TipMime`    varchar(100)     NOT NULL,   -- image/png, application/pdf, ... deduced server-side
  `Dimensiune` int(10) UNSIGNED NOT NULL,   -- the exact number of bytes in Continut
  `Sha256`     char(64)         NOT NULL,   -- lowercase hex, over Continut
  `Continut`   longblob         NOT NULL,
  `DataModif`  datetime         NOT NULL,
  PRIMARY KEY (`IDIMG`) USING BTREE,
  UNIQUE INDEX `UQ_FX_DDF_REV_ATT_IMG_ATT`(`IdRevAtt`) USING BTREE,
  CONSTRAINT `FK_FX_DDF_REV_ATT_IMG_ATT` FOREIGN KEY (`IdRevAtt`)
    REFERENCES `FX_DDF_REV_ATT` (`IdRevAtt`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = Dynamic;

-- =====================================================================================
-- VARIANT FOR AVACONT_SURSA -- run THIS ONE on it, INSTEAD of the one above.
--
-- The only difference: `IDIMG` is `INT UNSIGNED NOT NULL`, WITHOUT `AUTO_INCREMENT`.
-- The reason is in slice 0048-01 section 3.1: on the source schema, AUTO_INCREMENT would
-- make the migration silently fabricate keys for rows that arrive without them, instead of
-- raising an error.
-- =====================================================================================
--
-- CREATE TABLE `FX_DDF_REV_ATT_IMG` (
--   `IDIMG`      int(10) UNSIGNED NOT NULL,
--   `IdRevAtt`   int(11)          NOT NULL,
--   `NumeFisier` varchar(255)     NOT NULL,
--   `TipMime`    varchar(100)     NOT NULL,
--   `Dimensiune` int(10) UNSIGNED NOT NULL,
--   `Sha256`     char(64)         NOT NULL,
--   `Continut`   longblob         NOT NULL,
--   `DataModif`  datetime         NOT NULL,
--   PRIMARY KEY (`IDIMG`) USING BTREE,
--   UNIQUE INDEX `UQ_FX_DDF_REV_ATT_IMG_ATT`(`IdRevAtt`) USING BTREE,
--   CONSTRAINT `FK_FX_DDF_REV_ATT_IMG_ATT` FOREIGN KEY (`IdRevAtt`)
--     REFERENCES `FX_DDF_REV_ATT` (`IdRevAtt`) ON DELETE CASCADE ON UPDATE RESTRICT
-- ) ENGINE = InnoDB CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = Dynamic;
