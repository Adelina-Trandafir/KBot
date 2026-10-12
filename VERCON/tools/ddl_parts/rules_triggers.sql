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
