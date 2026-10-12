-- VERCON rules that schema sync does NOT carry (CHECK constraints and triggers). OPTIONAL, run on a DC database AFTER
-- the tables reached it through schema sync; also valid on a test DC for VC_99_probe.sql. Re-running fails on the first
-- ADD CONSTRAINT / CREATE TRIGGER that already exists -- that means the rules are already there. Not run from this chat.
-- The same rules are enforced by the service; this file is a second line of defence, not the only one.

-- ===== CHECK constraints =====
ALTER TABLE `VC_Gestiuni` ADD CONSTRAINT `ck_VC_Gestiune_Type` CHECK (`Type` IN ('MATERIALE','ALIMENTE','OI','MF'));
ALTER TABLE `VC_Gestiuni` ADD CONSTRAINT `ck_VC_Gestiune_State` CHECK (`State` IN ('IN_PREGATIRE','IMPORTATA_IN_REVIZIE','ACTIVA','ARHIVATA'));
ALTER TABLE `VC_Gestiuni` ADD CONSTRAINT `ck_VC_Gestiune_Zec` CHECK (`Zec` <= 4);
ALTER TABLE `VC_Gestiuni` ADD CONSTRAINT `ck_VC_Gestiune_Scara` CHECK (`Scara` >= `Zec` AND `Scara` <= 4);
ALTER TABLE `VC_Exercitii` ADD CONSTRAINT `ck_VC_Exercitiu_An` CHECK (`An` BETWEEN 1990 AND 2100);
ALTER TABLE `VC_ContextLock` ADD CONSTRAINT `ck_VC_Lock_Expiry` CHECK (`ExpiresAt` > `AcquiredAt`);
ALTER TABLE `VC_IdMap` ADD CONSTRAINT `ck_VC_IdMap_Kind` CHECK (`Kind` IN ('ROW','GROUP','ASSET'));
ALTER TABLE `VC_MigrationIssues` ADD CONSTRAINT `ck_VC_Issue_Severity` CHECK (`Severity` IN ('BLOCAJ','AVERTIZARE','CONVERSIE'));
ALTER TABLE `VC_AnnualClosings` ADD CONSTRAINT `ck_VC_Closing_order` CHECK (`FromExercitiuId` <> `ToExercitiuId`);
ALTER TABLE `VC_ProduseP` ADD CONSTRAINT `ck_ProduseP_PUTVA_nonneg` CHECK (`PUTVA` IS NULL OR `PUTVA` >= 0);
ALTER TABLE `VC_OI_Produse` ADD CONSTRAINT `ck_OI_Produse_PUTVA_nonneg` CHECK (`PUTVA` IS NULL OR `PUTVA` >= 0);
ALTER TABLE `VC_NIR` ADD CONSTRAINT `ck_NIR_TipDoc` CHECK (`TipDoc` IN ('NIR','STOC_INITIAL'));
ALTER TABLE `VC_NIR` ADD CONSTRAINT `ck_NIR_StocInitial_fara_partener` CHECK (`TipDoc` <> 'STOC_INITIAL' OR `IdPartener` IS NULL);
ALTER TABLE `VC_NIR` ADD CONSTRAINT `ck_NIR_motiv` CHECK (`Anulat` = 0 OR (`MotivAnulare` IS NOT NULL AND `MotivAnulare` <> ''));
ALTER TABLE `VC_Intrari` ADD CONSTRAINT `ck_Intrari_Pret_nonneg` CHECK (`Pret` IS NULL OR `Pret` >= 0);
ALTER TABLE `VC_Intrari` ADD CONSTRAINT `ck_Intrari_PUTVA_nonneg` CHECK (`PUTVA` IS NULL OR `PUTVA` >= 0);
ALTER TABLE `VC_IntrariStorno` ADD CONSTRAINT `ck_IntrariStorno_PUTVA_nonneg` CHECK (`PUTVA` IS NULL OR `PUTVA` >= 0);
ALTER TABLE `VC_IntrariStorno` ADD CONSTRAINT `ck_IntrariStorno_motiv` CHECK (`Anulat` = 0 OR (`MotivAnulare` IS NOT NULL AND `MotivAnulare` <> ''));
ALTER TABLE `VC_Iesiri_H` ADD CONSTRAINT `ck_Iesiri_H_motiv` CHECK (`Anulat` = 0 OR (`MotivAnulare` IS NOT NULL AND `MotivAnulare` <> ''));
ALTER TABLE `VC_Iesiri` ADD CONSTRAINT `ck_Iesiri_PUTVA_nonneg` CHECK (`PUTVA` IS NULL OR `PUTVA` >= 0);
ALTER TABLE `VC_IesiriRetur` ADD CONSTRAINT `ck_IesiriRetur_PUTVA_nonneg` CHECK (`PUTVA` IS NULL OR `PUTVA` >= 0);
ALTER TABLE `VC_OI_Predare` ADD CONSTRAINT `ck_OI_Predare_PUTVA_nonneg` CHECK (`PUTVA` IS NULL OR `PUTVA` >= 0);
ALTER TABLE `VC_OI_Predare` ADD CONSTRAINT `ck_OI_Predare_motiv` CHECK (`Anulat` = 0 OR (`MotivAnulare` IS NOT NULL AND `MotivAnulare` <> ''));
ALTER TABLE `VC_OI_Transfer` ADD CONSTRAINT `ck_OI_Transfer_PUTVA_nonneg` CHECK (`PUTVA` IS NULL OR `PUTVA` >= 0);
ALTER TABLE `VC_OI_Transfer` ADD CONSTRAINT `ck_OI_Transfer_motiv` CHECK (`Anulat` = 0 OR (`MotivAnulare` IS NOT NULL AND `MotivAnulare` <> ''));
ALTER TABLE `VC_OI_Casare` ADD CONSTRAINT `ck_OI_Casare_PUTVA_nonneg` CHECK (`PUTVA` IS NULL OR `PUTVA` >= 0);
ALTER TABLE `VC_OI_Casare` ADD CONSTRAINT `ck_OI_Casare_motiv` CHECK (`Anulat` = 0 OR (`MotivAnulare` IS NOT NULL AND `MotivAnulare` <> ''));
ALTER TABLE `VC_LABC` ADD CONSTRAINT `ck_LABC_motiv` CHECK (`Anulat` = 0 OR (`MotivAnulare` IS NOT NULL AND `MotivAnulare` <> ''));

-- ===== triggers =====
-- ---- invariants the CHECKs cannot express ---------------------------------------------------------------------------
DELIMITER $$
CREATE TRIGGER `trg_VC_Gestiune_immutable` BEFORE UPDATE ON `VC_Gestiuni` FOR EACH ROW
BEGIN
  IF OLD.`Populated` = 1 AND NEW.`Type` <> OLD.`Type` THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Tipul gestiunii nu se mai poate schimba: gestiunea contine date.';
  END IF;
  IF OLD.`Populated` = 1 AND NEW.`Scara` <> OLD.`Scara` THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Scara de stocare nu se mai poate schimba: valorile existente ar fi reinterpretate.';
  END IF;
  IF OLD.`Populated` = 1 AND NEW.`Populated` = 0 THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'O gestiune cu date nu mai poate redeveni goala.';
  END IF;
  IF OLD.`SubunitId` <> NEW.`SubunitId` THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'O gestiune nu se muta in alta subunitate.';
  END IF;
END$$

-- OI and MF are never closed (no annual close button): a closed exercise is only valid for MATERIALE / ALIMENTE.
CREATE TRIGGER `trg_VC_Exercitiu_close_ins` BEFORE INSERT ON `VC_Exercitii` FOR EACH ROW
BEGIN
  IF NEW.`Closed` = 1 AND (SELECT `Type` FROM `VC_Gestiuni` WHERE `GestiuneId` = NEW.`GestiuneId`) IN ('OI','MF') THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'OI si MF nu se inchid anual.';
  END IF;
END$$
CREATE TRIGGER `trg_VC_Exercitiu_close_upd` BEFORE UPDATE ON `VC_Exercitii` FOR EACH ROW
BEGIN
  IF NEW.`Closed` = 1 AND (SELECT `Type` FROM `VC_Gestiuni` WHERE `GestiuneId` = NEW.`GestiuneId`) IN ('OI','MF') THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'OI si MF nu se inchid anual.';
  END IF;
  IF OLD.`GestiuneId` <> NEW.`GestiuneId` OR OLD.`An` <> NEW.`An` THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Un exercitiu nu isi schimba gestiunea sau anul.';
  END IF;
END$$
DELIMITER ;
