-- SLICE-ADE6-24. Run only on AVACONT_SURSA; KBotAdmin synchronizes the unit schemas.
-- Web-only optional fields. Access has no corresponding columns; no migration is required.
USE `AVACONT_SURSA`;

ALTER TABLE `AD_Grupe_Educator`
  ADD COLUMN IF NOT EXISTS `EMail` VARCHAR(255) NULL DEFAULT NULL AFTER `Educator`,
  ADD COLUMN IF NOT EXISTS `CNP` VARCHAR(13) NULL DEFAULT NULL AFTER `EMail`;
