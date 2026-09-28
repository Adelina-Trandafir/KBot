-- =====================================================================================
-- Slice 0088-04 -- the FOREXE receipt («recipisa») of an uploaded correction note.
--
-- After «Trimite», FOREXE registers the upload under a number such as
-- «INTERNT-1230450081-2026/28-09-2026» and, a little later, puts a message in the SNM inbox
-- (category 1): «recipisa pentru CIF ..., tip F1135, numar_inregistrare INTERNT-1230450081-...».
-- K-BOT finds that message by the index (1230450081), downloads its file and stores it here,
-- attached to the note's PDF (FX_NoteCAB_PDF).
--
--   FX_NoteCAB            + IndexInregistrare: the index of the last upload (from FOREXE's answer
--                         or typed by the operator), so the receipt can be looked up later.
--   FX_NoteCAB_Recipisa   the receipt file, one row per registration index.
--
-- APPLY ON EVERY UNIT DATABASE (and on AVACONT_SURSA), after sql/0088 (+ 0088_02 / 0088_03 where
-- needed) and BEFORE the new note_cab.py reaches the VPS.
-- =====================================================================================

ALTER TABLE `FX_NoteCAB`
  ADD COLUMN IF NOT EXISTS `IndexInregistrare` varchar(20) NULL DEFAULT NULL AFTER `RaspunsTrimitere`;

CREATE TABLE IF NOT EXISTS `FX_NoteCAB_Recipisa` (
  `IDRCP`              int(11)          NOT NULL AUTO_INCREMENT,
  -- The note's PDF the receipt answers (FX_NoteCAB_PDF.IDPDF; the PDF row keeps its id when a
  -- signed copy replaces the unsigned one).
  `IDPDF`              int(10) UNSIGNED NOT NULL,
  -- «1230450081»: the index FOREXE gave the upload; unique -- one receipt per registration.
  `IndexInregistrare`  varchar(20)      NOT NULL,
  -- «INTERNT-1230450081-2026/28-09-2026», as the SNM message writes it.
  `NumarInregistrare`  varchar(64)      NULL DEFAULT NULL,
  -- The SNM message (FOREXE's id, text, creation time).
  `IdMesaj`            bigint(20)       NULL DEFAULT NULL,
  `DescriereMesaj`     varchar(500)     NULL DEFAULT NULL,
  `DataMesaj`          datetime         NULL DEFAULT NULL,
  -- The file as FOREXE served it.
  `NumeFisier`         varchar(255)     NOT NULL,
  `Dimensiune`         int(10) UNSIGNED NOT NULL,
  `Sha256`             char(64)         NOT NULL,
  `Continut`           longblob         NOT NULL,
  `UN`                 varchar(128)     NULL DEFAULT NULL,
  `DataAdaugare`       datetime         NOT NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`IDRCP`) USING BTREE,
  UNIQUE INDEX `UQ_FX_NoteCAB_Recipisa_Index` (`IndexInregistrare` ASC) USING BTREE,
  INDEX `IX_FX_NoteCAB_Recipisa_IDPDF` (`IDPDF` ASC) USING BTREE,
  CONSTRAINT `FK_FX_NoteCAB_Recipisa_PDF` FOREIGN KEY (`IDPDF`)
    REFERENCES `FX_NoteCAB_PDF` (`IDPDF`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;
