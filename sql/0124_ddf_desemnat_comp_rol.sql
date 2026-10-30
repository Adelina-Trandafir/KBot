-- Slice 0124-02: who has to sign a DDF revision next, and which role answers for which compartment.
--
-- Run on SURSA (admin app: «Interogare pe SURSA» > «Vezi» > «Ruleaza pe SURSA»), then «Sincronizeaza unitatile».
-- STRUCTURE ONLY (no data statements), so the sync can carry it to the units and mark it. No «USE»: the text runs on the
-- database the tool points it at (0078_05_kbot_banc_pdf.sql had a «USE 000_DEMO» and went to the wrong place).
-- Safe to run twice: IF NOT EXISTS everywhere.
--
-- DeSemnat (operator, 30.10.2026): the ROLE of the person who has to sign the revision next.
--   * filled when K-BOT generates the document, from FX_DDF_COMP_ROL by the document's compartment (FX_DDF.Comp);
--   * moves on with every signature: A -> the role that signs B -> Director -> empty;
--   * NULL = the revision is not in the K-BOT flow. Every revision that exists today stays NULL, and so does every
--     DDF that comes from FOREXE: none of them shows in any «to sign» list.
--   Values are roles as Unitati_Utilizatori.Rol stores them: Administrator, Secretar, Imputernicit, Contabil, Director.
--
-- FX_DDF_COMP_ROL: compartment -> the role that signs section A for it (one role per compartment). FX_DDF.Comp is free text,
-- so the match is by value (the collation is case-insensitive). Filled from a «Setari» window (0124-05), not by this script.
--
-- Collation as the rest of the database: utf8mb3 / utf8mb3_general_ci.

ALTER TABLE `FX_DDF_REV`
  ADD COLUMN IF NOT EXISTS `DeSemnat` VARCHAR(32) NULL DEFAULT NULL
  COMMENT 'Slice 0124: rolul care semneaza urmatorul (NULL = nu e in fluxul K-BOT)',
  ADD INDEX IF NOT EXISTS `IX_FX_DDF_REV_DeSemnat` (`DeSemnat`);

CREATE TABLE IF NOT EXISTS `FX_DDF_COMP_ROL` (
  `Comp` VARCHAR(255) NOT NULL COMMENT 'Compartimentul, ca in FX_DDF.Comp',
  `Rol`  VARCHAR(32)  NOT NULL COMMENT 'Rolul care semneaza sectiunea A pentru compartiment',
  PRIMARY KEY (`Comp`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;
