-- =====================================================================================
-- Slice 0078-05 -- shared-chunk storage for SIGNED PDFs (FX_PDF_BUCATI).
--
-- APPLY ON EVERY UNIT DATABASE (and on AVACONT_SURSA, the template for new units).
-- One MariaDB database = one unit, so this runs once per database, not once per VPS.
--
-- Shapes checked against MariaDB_Schema/000_DEMO.sql and AVACONT_SURSA.sql (dump of
-- 22.09.2026): FX_DDF_PDF / FX_ORD_PDF exist there with `Continut longblob NOT NULL`,
-- utf8mb3. Nothing here touches their keys or foreign keys.
--
-- ORDER IS FREE: routes/forexe/pdf.py probes for BOTH `FX_PDF_BUCATI.Continut` and the
-- `Bucati` column before using chunks; until this file has run on a database, that database
-- keeps storing whole files exactly as in slice 0041. Rows already written with `Continut`
-- stay readable forever (the route serves `Continut` when it is not NULL).
--
-- max_allowed_packet: one chunk is at most 64 KB (compressed, a few bytes more), one chunk
-- list at most ~550 KB for a 17 MB PDF -- far below the 32M already asked for by 0041.
-- =====================================================================================

-- One row per DISTINCT chunk in this database. Key = raw SHA-256 of the UNCOMPRESSED chunk.
-- `Continut` = zlib-compressed chunk (utils/pdf_chunks.py). MEDIUMBLOB, not BLOB: a chunk is
-- up to 64 KB and an incompressible one grows by a few bytes under zlib, past BLOB's 65535.
-- `DataCreare` lets the manual clean-up (scripts/pdf_chunks_cleanup.py) skip chunks written
-- in the last hour, i.e. by an upload that may not have committed its chunk list yet.
CREATE TABLE IF NOT EXISTS `FX_PDF_BUCATI` (
  `Sha256`     binary(32)       NOT NULL,
  `Dimensiune` int(10) UNSIGNED NOT NULL,
  `Continut`   mediumblob       NOT NULL,
  `DataCreare` datetime         NOT NULL,
  PRIMARY KEY (`Sha256`) USING BTREE,
  INDEX `IX_FX_PDF_BUCATI_DataCreare` (`DataCreare`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- The ordered chunk list of a document: its 32-byte digests, concatenated. NULL on rows
-- written before this slice (they keep `Continut`). `Continut` becomes NULLable because
-- chunked rows no longer carry the whole file.
ALTER TABLE `FX_DDF_PDF`
  ADD COLUMN IF NOT EXISTS `Bucati` mediumblob NULL DEFAULT NULL AFTER `Continut`,
  MODIFY COLUMN `Continut` longblob NULL DEFAULT NULL;

ALTER TABLE `FX_ORD_PDF`
  ADD COLUMN IF NOT EXISTS `Bucati` mediumblob NULL DEFAULT NULL AFTER `Continut`,
  MODIFY COLUMN `Continut` longblob NULL DEFAULT NULL;

-- Every row must carry exactly one of the two forms.
-- (MariaDB 10.11 enforces CHECK constraints.)
ALTER TABLE `FX_DDF_PDF`
  ADD CONSTRAINT `CK_FX_DDF_PDF_FORMA` CHECK ((`Continut` IS NULL) <> (`Bucati` IS NULL));
ALTER TABLE `FX_ORD_PDF`
  ADD CONSTRAINT `CK_FX_ORD_PDF_FORMA` CHECK ((`Continut` IS NULL) <> (`Bucati` IS NULL));
