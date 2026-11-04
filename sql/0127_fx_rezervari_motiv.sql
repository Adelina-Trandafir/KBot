-- Slice 0127-01: the reason the operator typed in FOREXE when saving a reservation (the "motiv" window), kept by K-BOT so
-- the revision that follows can offer it as its short description.
--
-- Run on SURSA (admin app: «Interogare pe SURSA» > «Vezi» > «Ruleaza pe SURSA»), then «Sincronizeaza unitatile».
-- STRUCTURE ONLY (no data statements), so the sync can carry it to the units and mark it. No «USE»: the text runs on the
-- database the tool points it at. Safe to run twice: IF NOT EXISTS.
--
-- One row per (revision number, indicator): IDREV is the number the marker «(IDREV: n)» reserved for the edit session
-- (FX_NumberLock, slice 0076) and CodIndicator is the indicator whose reservation was saved. When the same indicator is
-- saved again in the same session its reasons are joined with «; » (operator, 04.11.2026: join ONLY for the same
-- angajament AND indicator). No foreign key to FX_DDF_REV: the revision does not exist yet when the reason is saved,
-- its number is only reserved. New table, so CreatedAt + UpdatedAt (UpdatedAt ON UPDATE).
--
-- Collation as the rest of the database: utf8mb3 / utf8mb3_general_ci.

CREATE TABLE IF NOT EXISTS `FX_RezervariMotiv` (
  `IdMotiv`       INT(11)      NOT NULL AUTO_INCREMENT,
  `IDREV`         INT(11)      NOT NULL COMMENT 'Numarul reviziei rezervat de marcaj, (IDREV: n)',
  `CodAngajament` VARCHAR(255) NOT NULL,
  `CodIndicator`  VARCHAR(255) NOT NULL,
  `Motiv`         TEXT         NOT NULL COMMENT 'Motivul din FOREXE, fara marcaj; salvarile repetate ale aceluiasi indicator unite cu «; »',
  `CreatedAt`     DATETIME     NOT NULL DEFAULT current_timestamp(),
  `UpdatedAt`     DATETIME     NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`IdMotiv`),
  UNIQUE KEY `UQ_FX_RezervariMotiv_Rev_Ind` (`IDREV`, `CodIndicator`),
  KEY `IX_FX_RezervariMotiv_Cod` (`CodAngajament`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;
