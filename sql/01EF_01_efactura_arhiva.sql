-- 01EF-01: archive of the received e-invoices ("Arhiva" of the Access form EFACTURA_2025, view FA).
-- Run by the operator on AVACONT_SURSA only, then schema sync to every unit database (same procedure as 00EF_02).
-- NOT RUN anywhere yet. Safe to run again (CREATE TABLE IF NOT EXISTS). utf8mb3 / utf8mb3_general_ci (house rule).
--
-- One row = one invoice the operator put in the archive; no row = not archived. Un-archiving DELETES the row (no history).
-- IdPrimita  = EF_Primite.IdPrimita; UNIQUE, so an invoice is archived at most once. Deleting the invoice deletes its row.
-- DataArhivare / UN: filled by the SERVER on archiving (date and time; UN = the e-mail of the logged-in user, as in
--   EF_PrimiteAsocieri.UN). Both stay NULL for the invoices the Migrator brings from Access (Complet = True and no EFT_O row):
--   Access kept neither who nor when, so NULL is the honest value and also tells a migrated row from a new one.
--   No DEFAULT on DataArhivare on purpose: an omitted value must not invent a date for a migrated row.
-- Motiv      = optional short text typed by the operator.
-- The state of an invoice (new / booked / complete) is NOT stored: it is computed from EF_PrimiteOperatii (see 01EF-01).
CREATE TABLE IF NOT EXISTS `EF_PrimiteArhiva` (
  `IdArhiva` int(11) NOT NULL AUTO_INCREMENT,
  `IdPrimita` int(11) NOT NULL COMMENT 'EF_Primite.IdPrimita',
  `DataArhivare` datetime NULL DEFAULT NULL COMMENT 'When it was archived; NULL = migrated from Access, date unknown',
  `UN` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Who archived it (e-mail); NULL = migrated from Access',
  `Motiv` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Optional reason typed by the operator',
  `CreatedAt` datetime NULL DEFAULT current_timestamp() COMMENT 'When the row was created (migrated rows: the Access DTQ)',
  `UpdatedAt` datetime NULL DEFAULT NULL ON UPDATE current_timestamp() COMMENT 'When the row was last changed (NULL = not changed since it was created)',
  PRIMARY KEY (`IdArhiva`) USING BTREE,
  UNIQUE INDEX `UQ_EF_PrimiteArhiva_Primita`(`IdPrimita` ASC) USING BTREE,
  CONSTRAINT `FK_EF_PrimiteArhiva_Primita` FOREIGN KEY (`IdPrimita`) REFERENCES `EF_Primite` (`IdPrimita`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- The table was first made without CreatedAt / UpdatedAt: this adds them where they are missing (safe to run again). Rows that already exist keep NULL in CreatedAt.
ALTER TABLE `EF_PrimiteArhiva`
  ADD COLUMN IF NOT EXISTS `CreatedAt` datetime NULL DEFAULT NULL COMMENT 'When the row was created (migrated rows: the Access DTQ)',
  ADD COLUMN IF NOT EXISTS `UpdatedAt` datetime NULL DEFAULT NULL ON UPDATE current_timestamp() COMMENT 'When the row was last changed (NULL = not changed since it was created)';
ALTER TABLE `EF_PrimiteArhiva` ALTER COLUMN `CreatedAt` SET DEFAULT current_timestamp();
