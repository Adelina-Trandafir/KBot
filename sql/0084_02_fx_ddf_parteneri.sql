-- =====================================================================================
-- Slice 0084-02 -- FX_DDF_Parteneri: every partner associated with a DDF.
--
-- WHY. FX_DDF has ONE pair of columns for the partner (CodFiscal, NumePartener), so a document
-- can carry exactly one. There are documents with several partners, so the whole list lives
-- here, one row per (document, fiscal code). FX_DDF.CodFiscal / NumePartener / PartAng stay as
-- they are: they are the HEADER partner (the one the editor's combo picks, the one written on
-- every section-A / section-B line, the one in the signed PDF). The header partner is also a
-- row of this table, so the table alone answers «which partners does this DDF have».
--
-- Written by:
--   * the DDF editor's save (routes/forexe/ddf_edit.py -> ddf_parteneri.py), page «Parteneri»;
--   * the Sumar button «Asociaza parteneri» (POST /api/forexe/ddf/parteneri-asociati).
--
-- KEY. CodFiscal, not IdPartener: FX_DDF itself keeps only the fiscal code (one fiscal code can
-- be several Parteneri rows, one per unit). The name is kept next to it as it was when the
-- partner was associated, exactly like FX_DDF.NumePartener.
--
-- APPLY ON EVERY UNIT DATABASE (000_DEMO first) and on AVACONT_SURSA, the template for new
-- units, BEFORE the new ddf_edit.py / ddf_parteneri.py reach the VPS. Safe to run twice
-- (IF NOT EXISTS). Until it is applied the server keeps working: a save that carries only the
-- header partner skips the table, and the Sumar button answers with the name of this file.
--
-- The shape of FX_DDF was read from MariaDB_Schema/000_DEMO.sql (dump of 22.09.2026):
-- PRIMARY KEY (IDDF) int(11), engine InnoDB, utf8mb3.
-- =====================================================================================

CREATE TABLE IF NOT EXISTS `FX_DDF_Parteneri` (
  `IdDdfPartener` int(11)      NOT NULL AUTO_INCREMENT,
  `IDDF`          int(11)      NOT NULL,
  `CodFiscal`     varchar(255) NOT NULL,
  `NumePartener`  varchar(255) NULL DEFAULT NULL,
  `DataAdaugare`  datetime     NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`IdDdfPartener`) USING BTREE,
  -- One row per fiscal code and document: the association is never written twice.
  UNIQUE INDEX `UQ_FX_DDF_Parteneri_IDDF_CodFiscal` (`IDDF` ASC, `CodFiscal` ASC) USING BTREE,
  CONSTRAINT `FX_DDF_Parteneri_ibfk_1` FOREIGN KEY (`IDDF`) REFERENCES `FX_DDF` (`IDDF`)
    ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- Backfill: the partner every existing document already has in its header becomes its first
-- row, so the new page and the Sumar window show it from the first day. Re-runnable: the
-- unique key skips what is already there.
INSERT IGNORE INTO `FX_DDF_Parteneri` (`IDDF`, `CodFiscal`, `NumePartener`)
SELECT D.`IDDF`, D.`CodFiscal`, D.`NumePartener`
  FROM `FX_DDF` D
 WHERE COALESCE(D.`PartAng`, 0) = 1
   AND TRIM(COALESCE(D.`CodFiscal`, '')) <> '';
