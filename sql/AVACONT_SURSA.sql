/*
 Navicat MariaDB Data Transfer

 Source Server         : MariaDB AVACONT root
 Source Server Type    : MariaDB
 Source Server Version : 101114 (10.11.14-MariaDB-0ubuntu0.24.04.1)
 Source Host           : 89.33.25.34:3306
 Source Schema         : AVACONT_SURSA

 Target Server Type    : MariaDB
 Target Server Version : 101114 (10.11.14-MariaDB-0ubuntu0.24.04.1)
 File Encoding         : 65001

 Date: 09/09/2026 09:07:17
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for Clasificatii
-- ----------------------------
DROP TABLE IF EXISTS `Clasificatii`;
CREATE TABLE `Clasificatii`  (
  `IDClsf` int(11) NOT NULL AUTO_INCREMENT,
  `IdClsfAcc` int(11) NOT NULL DEFAULT 0,
  `IdUnitate` int(11) NULL DEFAULT NULL,
  `Capitol` varchar(5) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Subcapitol` varchar(5) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Articol` varchar(5) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Alineat` varchar(2) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Denumire` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Clsf` varchar(255) GENERATED ALWAYS AS (concat_ws('.',`Capitol`,`Subcapitol`,`Articol`,`Alineat`)) PERSISTENT,
  `Titlu` varchar(10) GENERATED ALWAYS AS (left(coalesce(`Articol`,''),2)) PERSISTENT,
  `ClsfSal` varchar(255) GENERATED ALWAYS AS (concat(left(coalesce(`Capitol`,''),2),replace(coalesce(`Subcapitol`,''),'.',''),replace(coalesce(`Articol`,''),'.',''),coalesce(`Alineat`,''))) PERSISTENT,
  `ClsfF` varchar(255) GENERATED ALWAYS AS (concat(left(coalesce(`Capitol`,''),2),replace(coalesce(`Subcapitol`,''),'.',''))) PERSISTENT,
  `ClsfE` varchar(255) GENERATED ALWAYS AS (concat(replace(coalesce(`Articol`,''),'.',''),coalesce(`Alineat`,''))) PERSISTENT,
  `ClsfX` varchar(255) GENERATED ALWAYS AS (concat_ws('.',`Capitol`,'XX.XX',`Articol`,`Alineat`)) PERSISTENT,
  `Sector` varchar(2) GENERATED ALWAYS AS (case right(coalesce(`Capitol`,''),2) when '02' then '02' when '01' then '01' when '10' then '02' when '00' then '01' else '' end) PERSISTENT,
  `Sursa` char(1) GENERATED ALWAYS AS (case right(coalesce(`Capitol`,''),2) when '02' then 'A' when '01' then 'A' when '10' then 'E' when '00' then 'A' else '' end) PERSISTENT,
  `SS` varchar(3) GENERATED ALWAYS AS (concat(case right(coalesce(`Capitol`,''),2) when '02' then '02' when '01' then '01' when '10' then '02' when '00' then '01' else '' end,case right(coalesce(`Capitol`,''),2) when '02' then 'A' when '01' then 'A' when '10' then 'E' when '00' then 'A' else '' end)) PERSISTENT,
  `DataAdugare` datetime NULL DEFAULT current_timestamp(),
  `DataModificare` datetime NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`IDClsf`) USING BTREE,
  INDEX `idx_Clsf`(`Clsf` ASC) USING BTREE,
  INDEX `idx_ClsfSal`(`ClsfSal` ASC) USING BTREE,
  INDEX `idx_SS`(`SS` ASC) USING BTREE,
  INDEX `idx_ClsfE`(`ClsfE` ASC) USING BTREE,
  INDEX `ClsfF`(`ClsfF` ASC) USING BTREE,
  INDEX `Titlu`(`Titlu` ASC) USING BTREE,
  INDEX `Articol`(`Articol` ASC) USING BTREE,
  INDEX `IdClsfAcc`(`IdClsfAcc` ASC) USING BTREE,
  INDEX `IdUnitate`(`IdUnitate` ASC) USING BTREE,
  INDEX `IDClsf`(`IDClsf` ASC) USING BTREE,
  INDEX `IDClsf_2`(`IDClsf` ASC) USING BTREE,
  INDEX `IDClsf_3`(`IDClsf` ASC) USING BTREE,
  CONSTRAINT `Clasificatii__DefaArticol` FOREIGN KEY (`Articol`) REFERENCES `AVACONT_COMUN`.`DefaArticol` (`Articol`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `Clasificatii__DefaClsfF` FOREIGN KEY (`ClsfF`) REFERENCES `AVACONT_COMUN`.`DefaClsfF` (`ClsfF`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `Clasificatii__DefaSS` FOREIGN KEY (`SS`) REFERENCES `AVACONT_COMUN`.`DefaSursaSector` (`SursaSector`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `Clasificatii__DefaTitlu` FOREIGN KEY (`Titlu`) REFERENCES `AVACONT_COMUN`.`DefaTitlu` (`Titlu`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `Clasificatii__Unitati` FOREIGN KEY (`IdUnitate`) REFERENCES `Unitati` (`IdUnitate`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for Clasificatii_Buget
-- ----------------------------
DROP TABLE IF EXISTS `Clasificatii_Buget`;
CREATE TABLE `Clasificatii_Buget`  (
  `IdBuget` int(11) NOT NULL AUTO_INCREMENT,
  `IdClsf` int(11) NOT NULL,
  `IdUnitate` int(11) NOT NULL,
  `TOTAL` double GENERATED ALWAYS AS (coalesce(`Trim1`,0) + coalesce(`Trim2`,0) + coalesce(`Trim3`,0) + coalesce(`Trim4`,0)) PERSISTENT,
  `Trim1` double NULL DEFAULT NULL,
  `Trim2` double NULL DEFAULT NULL,
  `Trim3` double NULL DEFAULT NULL,
  `Trim4` double NULL DEFAULT NULL,
  `An` int(4) NULL DEFAULT NULL,
  `DataAdugare` datetime NULL DEFAULT current_timestamp(),
  `DataModificare` datetime NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`IdBuget`) USING BTREE,
  UNIQUE INDEX `uq_clasificatii_buget_idclsf_an`(`IdClsf` ASC, `An` ASC) USING BTREE,
  INDEX `Clasificatii_Buget_ibfk_1`(`IdClsf` ASC) USING BTREE,
  INDEX `Clasificatii_Buget_ibfk_2`(`IdUnitate` ASC) USING BTREE,
  CONSTRAINT `Clasificatii_Buget_ibfk_1` FOREIGN KEY (`IdClsf`) REFERENCES `Clasificatii` (`IDClsf`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `Clasificatii_Buget_ibfk_2` FOREIGN KEY (`IdUnitate`) REFERENCES `Unitati` (`IdUnitate`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for Clasificatii_Rectificari
-- ----------------------------
DROP TABLE IF EXISTS `Clasificatii_Rectificari`;
CREATE TABLE `Clasificatii_Rectificari`  (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `IdClsf` int(11) NULL DEFAULT NULL,
  `Capitol` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Subcapitol` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Articol` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Alineat` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Data` datetime NULL DEFAULT NULL,
  `Document` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Trim1` double NULL DEFAULT NULL,
  `Trim2` double NULL DEFAULT NULL,
  `Trim3` double NULL DEFAULT NULL,
  `Trim4` double NULL DEFAULT NULL,
  `DataAdugare` datetime NULL DEFAULT current_timestamp(),
  `DataModificare` datetime NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`ID`) USING BTREE,
  UNIQUE INDEX `UK_Rectificari_IdClsf_Data_Document`(`IdClsf` ASC, `Data` ASC, `Document` ASC) USING BTREE,
  INDEX `Rectificari_ibfk_1`(`IdClsf` ASC) USING BTREE,
  CONSTRAINT `Clasificatii_Rectificari_ibfk_1` FOREIGN KEY (`IdClsf`) REFERENCES `Clasificatii` (`IDClsf`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for Clasificatii_Venituri
-- ----------------------------
DROP TABLE IF EXISTS `Clasificatii_Venituri`;
CREATE TABLE `Clasificatii_Venituri`  (
  `IdClsfV` int(11) UNSIGNED NOT NULL AUTO_INCREMENT,
  `Capitol` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `SubCapitol` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Paragraf` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Denumire` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Trim1` int(255) NULL DEFAULT 0,
  `Trim2` int(255) NULL DEFAULT 0,
  `Trim3` int(255) NULL DEFAULT 0,
  `Trim4` int(255) NULL DEFAULT 0,
  PRIMARY KEY (`IdClsfV`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for Clasificatii_Venituri_Rectificari
-- ----------------------------
DROP TABLE IF EXISTS `Clasificatii_Venituri_Rectificari`;
CREATE TABLE `Clasificatii_Venituri_Rectificari`  (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `IdClsfV` int(11) UNSIGNED NULL DEFAULT NULL,
  `Trim1` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Trim2` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Trim3` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Trim4` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Document` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Data` date NULL DEFAULT NULL,
  `DTQ` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`ID`) USING BTREE,
  INDEX `Clasificatii_Venituri__Clasificatii_Venituri_Rectificari`(`IdClsfV` ASC) USING BTREE,
  CONSTRAINT `Clasificatii_Venituri__Clasificatii_Venituri_Rectificari` FOREIGN KEY (`IdClsfV`) REFERENCES `Clasificatii_Venituri` (`IdClsfV`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_Alegeri_Unitate
-- ----------------------------
DROP TABLE IF EXISTS `FX_Alegeri_Unitate`;
CREATE TABLE `FX_Alegeri_Unitate`  (
  `IdAlegere` int(10) UNSIGNED NOT NULL AUTO_INCREMENT COMMENT 'Primary key',
  `SS` varchar(3) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'Sector + Sursa (ex: \"02E\")',
  `ClsfE` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'Articol + Alineat fara puncte (ex: \"200101\")',
  `IdUnitate` int(11) NOT NULL COMMENT 'Raspunsul operatorului - ID-ul unitatii alese',
  `UN` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'Adresa de email a utilizatorului care a raspuns',
  `DataAlegere` datetime NOT NULL COMMENT 'Data si ora alegerii',
  PRIMARY KEY (`IdAlegere`) USING BTREE,
  UNIQUE INDEX `UQ_FX_Alegeri_Unitate`(`SS` ASC, `ClsfE` ASC) USING BTREE COMMENT 'O singura alegere per pereche (SS, ClsfE)',
  INDEX `FK_FX_Alegeri_Unitate_Unitate`(`IdUnitate` ASC) USING BTREE,
  CONSTRAINT `FK_FX_Alegeri_Unitate_Unitate` FOREIGN KEY (`IdUnitate`) REFERENCES `Unitati` (`IdUnitate`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_Angajamente
-- ----------------------------
DROP TABLE IF EXISTS `FX_Angajamente`;
CREATE TABLE `FX_Angajamente`  (
  `CodAngajament` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `IDDF` int(11) NULL DEFAULT NULL,
  `DataCreare` datetime NULL DEFAULT NULL,
  `DataDefinitivare` datetime NULL DEFAULT NULL,
  `Descriere` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Stare` varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `DC` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `DTQ` datetime NULL DEFAULT NULL,
  `Incarcat` tinyint(1) NOT NULL DEFAULT 0,
  `Preluat` tinyint(1) NOT NULL DEFAULT 0,
  `Salarii` tinyint(1) NOT NULL DEFAULT 0,
  `Ascuns` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`CodAngajament`) USING BTREE,
  INDEX `ix_FX_Angajamente_IDDF`(`IDDF` ASC) USING BTREE,
  INDEX `ix_FX_Angajamente_DC`(`DC` ASC) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_DDF
-- ----------------------------
DROP TABLE IF EXISTS `FX_DDF`;
CREATE TABLE `FX_DDF`  (
  `IDDF` int(11) NOT NULL AUTO_INCREMENT,
  `CodAngajament` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `CUAL` int(11) NOT NULL,
  `ObiectDDF` varchar(500) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Comp` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Salarii` tinyint(4) NULL DEFAULT 0,
  `Buget` tinyint(4) NULL DEFAULT NULL,
  `DataCreare` datetime NOT NULL,
  `DC` varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Program` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `DataDef` datetime NULL DEFAULT NULL,
  `DataAdaugare` datetime NULL DEFAULT current_timestamp(),
  `DataModificare` datetime NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  `IdSalarii` int(11) NULL DEFAULT NULL,
  `Incarcat` tinyint(4) NULL DEFAULT 0,
  `Preluat` tinyint(4) NULL DEFAULT 0,
  `Manual` tinyint(4) NULL DEFAULT 0,
  `Stare` varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `PartAng` tinyint(1) NULL DEFAULT NULL,
  `CodFiscal` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `NumePartener` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`IDDF`) USING BTREE,
  INDEX `IDDF`(`IDDF` ASC) USING BTREE,
  INDEX `CodAngajament`(`CodAngajament` ASC) USING BTREE,
  CONSTRAINT `FX_DDF__FX_Angajamente` FOREIGN KEY (`CodAngajament`) REFERENCES `FX_Angajamente` (`CodAngajament`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_DDF_PDF
-- ----------------------------
DROP TABLE IF EXISTS `FX_DDF_PDF`;
CREATE TABLE `FX_DDF_PDF`  (
  `IDPDF` int(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `IDREV` int(11) NOT NULL,
  `NumeFisier` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Dimensiune` int(10) UNSIGNED NOT NULL,
  `Sha256` char(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Continut` longblob NOT NULL,
  `DataModif` datetime NOT NULL,
  PRIMARY KEY (`IDPDF`) USING BTREE,
  UNIQUE INDEX `UQ_FX_DDF_PDF_IDREV`(`IDREV` ASC) USING BTREE,
  CONSTRAINT `FK_FX_DDF_PDF_REV` FOREIGN KEY (`IDREV`) REFERENCES `FX_DDF_REV` (`IDREV`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_DDF_REV
-- ----------------------------
DROP TABLE IF EXISTS `FX_DDF_REV`;
CREATE TABLE `FX_DDF_REV`  (
  `IDREV` int(11) NOT NULL AUTO_INCREMENT,
  `IDDF` int(11) NULL DEFAULT NULL,
  `CodAngajament` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Tip` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `NumarRev` int(11) NULL DEFAULT NULL,
  `DataRev` date NULL DEFAULT NULL,
  `Desc_Scurta` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Desc_Lunga` mediumtext CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Desc_Lunga_ANSI` mediumtext CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `ESpeciala` tinyint(1) NULL DEFAULT 0,
  `DataAdugare` datetime NULL DEFAULT current_timestamp(),
  `DataModificare` datetime NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  `Incarcat` tinyint(4) NULL DEFAULT 0,
  `Preluat` tinyint(4) NULL DEFAULT 0,
  `DC` varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Semnatura` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`IDREV`) USING BTREE,
  INDEX `IdRevizie`(`IDREV` ASC) USING BTREE,
  INDEX `tblDocFund_Revizii_ibfk_1`(`IDDF` ASC) USING BTREE,
  CONSTRAINT `FX_DDF_REV_ibfk_1` FOREIGN KEY (`IDDF`) REFERENCES `FX_DDF` (`IDDF`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_DDF_REV_ATT
-- ----------------------------
DROP TABLE IF EXISTS `FX_DDF_REV_ATT`;
CREATE TABLE `FX_DDF_REV_ATT`  (
  `IdRevAtt` int(11) NOT NULL AUTO_INCREMENT,
  `IDDF` int(11) NULL DEFAULT NULL,
  `IDREV` int(11) NULL DEFAULT NULL,
  `IDVBNET` int(11) NULL DEFAULT NULL,
  `CaleFisier` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `PrtScr` tinyint(4) NULL DEFAULT NULL,
  `DateFisier` longtext CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `DataAdugare` datetime NULL DEFAULT current_timestamp(),
  `DataModificare` datetime NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`IdRevAtt`) USING BTREE,
  INDEX `tblDocFund_Revizii_Att_ibfk_1`(`IDREV` ASC) USING BTREE,
  CONSTRAINT `FX_DDF_REV_ATT_ibfk_1` FOREIGN KEY (`IDREV`) REFERENCES `FX_DDF_REV` (`IDREV`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_DDF_REV_ATT_IMG
-- ----------------------------
DROP TABLE IF EXISTS `FX_DDF_REV_ATT_IMG`;
CREATE TABLE `FX_DDF_REV_ATT_IMG`  (
  `IDIMG` int(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `IdRevAtt` int(11) NOT NULL,
  `NumeFisier` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `TipMime` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Dimensiune` int(10) UNSIGNED NOT NULL,
  `Sha256` char(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Continut` longblob NOT NULL,
  `DataModif` datetime NOT NULL,
  PRIMARY KEY (`IDIMG`) USING BTREE,
  UNIQUE INDEX `UQ_FX_DDF_REV_ATT_IMG_ATT`(`IdRevAtt` ASC) USING BTREE,
  CONSTRAINT `FK_FX_DDF_REV_ATT_IMG_ATT` FOREIGN KEY (`IdRevAtt`) REFERENCES `FX_DDF_REV_ATT` (`IdRevAtt`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_DDF_REV_PRT
-- ----------------------------
DROP TABLE IF EXISTS `FX_DDF_REV_PRT`;
CREATE TABLE `FX_DDF_REV_PRT`  (
  `IDREVP` int(11) NOT NULL AUTO_INCREMENT,
  `IDDF` int(11) NULL DEFAULT NULL,
  `IDREV` int(11) NULL DEFAULT NULL,
  `IdClsf` int(11) NULL DEFAULT NULL,
  `IdClsfAcc` int(11) NULL DEFAULT NULL,
  `DateFisier` longtext CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Expl` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Tip` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodAngajament` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`IDREVP`) USING BTREE,
  INDEX `FX_DDF_REV_PRT_ibfk_1`(`IDDF` ASC) USING BTREE,
  INDEX `FX_DDF_REV_PRT_ibfk_2`(`IDREV` ASC) USING BTREE,
  INDEX `FX_DDF_REV_PRT_ibfk_3`(`IdClsf` ASC) USING BTREE,
  CONSTRAINT `FX_DDF_REV_PRT_ibfk_1` FOREIGN KEY (`IDDF`) REFERENCES `FX_DDF` (`IDDF`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FX_DDF_REV_PRT_ibfk_2` FOREIGN KEY (`IDREV`) REFERENCES `FX_DDF_REV` (`IDREV`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FX_DDF_REV_PRT_ibfk_3` FOREIGN KEY (`IdClsf`) REFERENCES `Clasificatii` (`IDClsf`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_DDF_REV_SA
-- ----------------------------
DROP TABLE IF EXISTS `FX_DDF_REV_SA`;
CREATE TABLE `FX_DDF_REV_SA`  (
  `IdSecA` int(11) NOT NULL AUTO_INCREMENT,
  `IDDF` int(11) NULL DEFAULT NULL,
  `IDREV` int(11) NULL DEFAULT NULL,
  `IdUnitate` int(11) NULL DEFAULT NULL,
  `CodAngajament` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodIndicator` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodPartener` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `IdPartener` int(11) NULL DEFAULT NULL,
  `IdClsfAcc` int(11) NOT NULL,
  `IdClsf` int(11) NOT NULL,
  `Clsf` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `ElementFund` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `ParametriiFund` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `ValPrec` double NULL DEFAULT 0,
  `ValCur` double NULL DEFAULT 0,
  `ValTot` double NULL DEFAULT 0,
  `PartInd` tinyint(4) NULL DEFAULT NULL,
  `Ramane` double NULL DEFAULT NULL,
  `SS` varchar(3) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`IdSecA`) USING BTREE,
  INDEX `IDDF`(`IDDF` ASC) USING BTREE,
  INDEX `IdRev`(`IDREV` ASC) USING BTREE,
  INDEX `IdPartener`(`IdPartener` ASC) USING BTREE,
  INDEX `IdClsf`(`IdClsf` ASC) USING BTREE,
  INDEX `IdUnitate`(`IdUnitate` ASC) USING BTREE,
  CONSTRAINT `FX_DDF_REV_SA_ibfk_1` FOREIGN KEY (`IDDF`) REFERENCES `FX_DDF` (`IDDF`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FX_DDF_REV_SA_ibfk_2` FOREIGN KEY (`IDREV`) REFERENCES `FX_DDF_REV` (`IDREV`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FX_DDF_REV_SA_ibfk_3` FOREIGN KEY (`IdPartener`) REFERENCES `Parteneri` (`IdPartener`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FX_DDF_REV_SA_ibfk_4` FOREIGN KEY (`IdClsf`) REFERENCES `Clasificatii` (`IDClsf`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FX_DDF_REV_SA_ibfk_5` FOREIGN KEY (`IdUnitate`) REFERENCES `Unitati` (`IdUnitate`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_DDF_REV_SB
-- ----------------------------
DROP TABLE IF EXISTS `FX_DDF_REV_SB`;
CREATE TABLE `FX_DDF_REV_SB`  (
  `IdSecB` int(11) NOT NULL AUTO_INCREMENT,
  `IDDF` int(11) NULL DEFAULT NULL,
  `IDREV` int(11) NULL DEFAULT NULL,
  `CodAngajament` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodIndicator` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodPartener` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `IdUnitate` int(11) NULL DEFAULT NULL,
  `IdPartener` int(11) NULL DEFAULT NULL,
  `IdClsfAcc` int(11) NOT NULL,
  `IdClsf` int(11) NOT NULL,
  `CodSSI` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CA_Anterior` double NULL DEFAULT NULL,
  `Inf1` double NULL DEFAULT NULL,
  `CA_Curent` double NULL DEFAULT NULL,
  `CB_Anterior` double NULL DEFAULT NULL,
  `Inf2` double NULL DEFAULT NULL,
  `CB_Curent` double NULL DEFAULT NULL,
  `SS` varchar(3) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`IdSecB`) USING BTREE,
  INDEX `tblDocFund_SB_ibfk_1`(`IDDF` ASC) USING BTREE,
  INDEX `tblDocFund_SB_ibfk_2`(`IDREV` ASC) USING BTREE,
  INDEX `tblDocFund_SB_ibfk_3`(`IdPartener` ASC) USING BTREE,
  INDEX `tblDocFund_SB_ibfk_4`(`IdClsf` ASC) USING BTREE,
  INDEX `IdUnitate`(`IdUnitate` ASC) USING BTREE,
  CONSTRAINT `FX_DDF_REV_SB_ibfk_1` FOREIGN KEY (`IdUnitate`) REFERENCES `Unitati` (`IdUnitate`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `tblDocFund_SB_ibfk_1` FOREIGN KEY (`IDDF`) REFERENCES `FX_DDF` (`IDDF`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `tblDocFund_SB_ibfk_2` FOREIGN KEY (`IDREV`) REFERENCES `FX_DDF_REV` (`IDREV`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `tblDocFund_SB_ibfk_3` FOREIGN KEY (`IdPartener`) REFERENCES `Parteneri` (`IdPartener`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `tblDocFund_SB_ibfk_4` FOREIGN KEY (`IdClsf`) REFERENCES `Clasificatii` (`IDClsf`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_Extrase
-- ----------------------------
DROP TABLE IF EXISTS `FX_Extrase`;
CREATE TABLE `FX_Extrase`  (
  `IDFXE` int(11) NOT NULL,
  `IDFXH` int(11) NULL DEFAULT NULL,
  `IDOP` int(11) NULL DEFAULT NULL,
  `CodAI` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `DataBanca` datetime NULL DEFAULT NULL,
  `DataDoc` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `NrDoc` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Referinta` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `ReferintaDest` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `platitor_nume` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `platitor_cui` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `platitor_iban` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `suma_debit` double NULL DEFAULT NULL,
  `suma_credit` double NULL DEFAULT NULL,
  `Explicatii` longtext CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodContract` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `RandContract` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodProgram` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `HASH` varchar(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `IDORD` int(11) NULL DEFAULT NULL,
  `IDORDT` int(11) NULL DEFAULT NULL,
  `CodPartener` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `IdUnitate` int(11) NULL DEFAULT NULL,
  PRIMARY KEY (`IDFXE`) USING BTREE,
  INDEX `FX_Extrase__FX_Extrase_H`(`IDFXH` ASC) USING BTREE,
  CONSTRAINT `FX_Extrase__FX_Extrase_H` FOREIGN KEY (`IDFXH`) REFERENCES `FX_Extrase_H` (`IDEXH`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_Extrase_F
-- ----------------------------
DROP TABLE IF EXISTS `FX_Extrase_F`;
CREATE TABLE `FX_Extrase_F`  (
  `IDEXF` int(11) NOT NULL,
  `NumeFisier` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CaleFisier` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `DataExtras` datetime NULL DEFAULT NULL,
  `NumarExtras` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `XML` longtext CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `HASH` varchar(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`IDEXF`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_Extrase_H
-- ----------------------------
DROP TABLE IF EXISTS `FX_Extrase_H`;
CREATE TABLE `FX_Extrase_H`  (
  `IDEXH` int(11) NOT NULL,
  `IDEXF` int(11) NULL DEFAULT NULL,
  `IdClsf` int(11) NULL DEFAULT NULL,
  `IdUnitate` int(11) NULL DEFAULT NULL,
  `CodIBAN` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Cont` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `SID` double NULL DEFAULT NULL,
  `SIC` double NULL DEFAULT NULL,
  `RPD` double NULL DEFAULT NULL,
  `RPC` double NULL DEFAULT NULL,
  `TSD` double NULL DEFAULT NULL,
  `TSC` double NULL DEFAULT NULL,
  `SFD` double NULL DEFAULT NULL,
  `SFC` double NULL DEFAULT NULL,
  `HASH` varchar(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `IdClsfV` int(11) NULL DEFAULT NULL,
  PRIMARY KEY (`IDEXH`) USING BTREE,
  INDEX `FX_Extrase_H__FX_Extrase_F`(`IDEXF` ASC) USING BTREE,
  CONSTRAINT `FX_Extrase_H__FX_Extrase_F` FOREIGN KEY (`IDEXF`) REFERENCES `FX_Extrase_F` (`IDEXF`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_Indicatori
-- ----------------------------
DROP TABLE IF EXISTS `FX_Indicatori`;
CREATE TABLE `FX_Indicatori`  (
  `CodAI` varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `CodAngajament` varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodIndicator` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `IdClsf` int(11) NULL DEFAULT NULL,
  `IndicatorFX` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Prevedere_Bugetara_Initiala` double NULL DEFAULT NULL,
  `Credit_Bugetar_Initial` double NULL DEFAULT NULL,
  `Angajament_Legal` double NULL DEFAULT NULL,
  `Credit_Bugetar_Definitiv` double NULL DEFAULT NULL,
  `Receptii` double NULL DEFAULT NULL,
  `Plati` double NULL DEFAULT NULL,
  `DTQ` datetime NULL DEFAULT current_timestamp(),
  `NrCrt` int(11) NULL DEFAULT NULL,
  `IdUnitate` int(11) NULL DEFAULT NULL,
  `SS` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`CodAI`) USING BTREE,
  INDEX `ix_FX_Indicatori_CodAngajament`(`CodAngajament` ASC) USING BTREE,
  INDEX `ix_FX_Indicatori_IdClsf`(`IdClsf` ASC) USING BTREE,
  INDEX `ix_FX_Indicatori_IdUnitate`(`IdUnitate` ASC) USING BTREE,
  CONSTRAINT `FX_Indicatori__FX_Angajamente` FOREIGN KEY (`CodAngajament`) REFERENCES `FX_Angajamente` (`CodAngajament`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_Istoric
-- ----------------------------
DROP TABLE IF EXISTS `FX_Istoric`;
CREATE TABLE `FX_Istoric`  (
  `ID` int(11) NOT NULL,
  `IDREV` int(11) NULL DEFAULT NULL,
  `IdClsf` int(11) NULL DEFAULT NULL,
  `CodAI` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodAngajament` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodIndicator` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `DataFX` datetime NULL DEFAULT NULL,
  `Utilizator` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Descriere` text CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Observatii` text CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Val_Rezervare_I` double NULL DEFAULT NULL,
  `Val_Rezervare_D` double NULL DEFAULT NULL,
  `Val_AngLeg` double NULL DEFAULT NULL,
  `Val_Rezervare_Ant` double NULL DEFAULT NULL,
  `Val_Rezervare_Dif` double NULL DEFAULT NULL,
  `Val_Receptie` double NULL DEFAULT NULL,
  `Val_Plata` double NULL DEFAULT NULL,
  `TipRand` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `IdTrezor` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Doc` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `HASH` varchar(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Prelucrat` tinyint(1) NOT NULL DEFAULT 0,
  `DTQ` datetime NULL DEFAULT NULL,
  `Val_Receptie_T` double NULL DEFAULT NULL,
  `Rez_Ord` int(11) NULL DEFAULT NULL,
  `Clsf` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`ID`) USING BTREE,
  UNIQUE INDEX `ux_FX_Istoric_HASH`(`HASH` ASC) USING BTREE,
  INDEX `ix_FX_Istoric_CodAngajament`(`CodAngajament` ASC) USING BTREE,
  INDEX `ix_FX_Istoric_CodAI`(`CodAI` ASC) USING BTREE,
  CONSTRAINT `FX_Istoric__FX_Angajamente` FOREIGN KEY (`CodAngajament`) REFERENCES `FX_Angajamente` (`CodAngajament`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FX_Istoric__FX_Indicatori` FOREIGN KEY (`CodAI`) REFERENCES `FX_Indicatori` (`CodAI`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_NumberLock
-- ----------------------------
DROP TABLE IF EXISTS `FX_NumberLock`;
CREATE TABLE `FX_NumberLock`  (
  `IdLock` int(11) NOT NULL AUTO_INCREMENT,
  `Tip` varchar(16) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `DC` varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Valoare` int(11) NOT NULL,
  `CodAngajament` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Token` varchar(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Utilizator` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CreatLa` datetime NOT NULL DEFAULT current_timestamp(),
  `ExpiraLa` datetime NOT NULL,
  PRIMARY KEY (`IdLock`) USING BTREE,
  UNIQUE INDEX `UQ_FX_NumberLock`(`Tip` ASC, `DC` ASC, `Valoare` ASC) USING BTREE,
  INDEX `ix_FX_NumberLock_Token`(`Token` ASC) USING BTREE,
  INDEX `ix_FX_NumberLock_ExpiraLa`(`ExpiraLa` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_ORD
-- ----------------------------
DROP TABLE IF EXISTS `FX_ORD`;
CREATE TABLE `FX_ORD`  (
  `IDORDP` int(11) NOT NULL AUTO_INCREMENT,
  `IDORD` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'ACCESS',
  `IDDF` int(11) NULL DEFAULT NULL,
  `IDRR` int(11) NULL DEFAULT NULL COMMENT 'PK ACCESS FX_Receptii_R',
  `IDRH` int(11) NULL DEFAULT NULL COMMENT 'PK ACCESS FX_Receptii_H',
  `NrORD` int(11) NULL DEFAULT 0,
  `DataORD` datetime NULL DEFAULT NULL,
  `Comp` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CUAL` int(11) NULL DEFAULT NULL,
  `Incarcat` tinyint(4) NOT NULL DEFAULT 0,
  `Preluat` tinyint(4) NOT NULL DEFAULT 0,
  `CodAngajament` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Semnatura` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`IDORDP`) USING BTREE,
  INDEX `IDORD`(`IDORDP` ASC) USING BTREE,
  INDEX `FX_ORD__FX_DDF`(`IDDF` ASC) USING BTREE,
  CONSTRAINT `FX_ORD__FX_DDF` FOREIGN KEY (`IDDF`) REFERENCES `FX_DDF` (`IDDF`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_ORD_ATT
-- ----------------------------
DROP TABLE IF EXISTS `FX_ORD_ATT`;
CREATE TABLE `FX_ORD_ATT`  (
  `IDORDATTP` int(11) NOT NULL AUTO_INCREMENT,
  `IDORDATT` int(11) NULL DEFAULT NULL COMMENT 'ACCESS',
  `IDORDP` int(11) NULL DEFAULT 0,
  `IDORDPARTP` int(11) NULL DEFAULT NULL,
  `Imagine` longtext CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`IDORDATTP`) USING BTREE,
  INDEX `IDORD`(`IDORDP` ASC) USING BTREE,
  INDEX `IDORDD`(`IDORDPARTP` ASC) USING BTREE,
  INDEX `IDORDJ`(`IDORDATTP` ASC) USING BTREE,
  CONSTRAINT `FX_ORD_ATT__FX_ORD` FOREIGN KEY (`IDORDP`) REFERENCES `FX_ORD` (`IDORDP`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FX_ORD_ATT__FX_ORD_PART` FOREIGN KEY (`IDORDPARTP`) REFERENCES `FX_ORD_PART` (`IDORDPARTP`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_ORD_ATT_IMG
-- ----------------------------
DROP TABLE IF EXISTS `FX_ORD_ATT_IMG`;
CREATE TABLE `FX_ORD_ATT_IMG`  (
  `IDIMG` int(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `IDORDATTP` int(11) NOT NULL,
  `NumeFisier` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `TipMime` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Dimensiune` int(10) UNSIGNED NOT NULL,
  `Sha256` char(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Continut` longblob NOT NULL,
  `DataModif` datetime NOT NULL,
  PRIMARY KEY (`IDIMG`) USING BTREE,
  UNIQUE INDEX `UQ_FX_ORD_ATT_IMG_ATTP`(`IDORDATTP` ASC) USING BTREE,
  CONSTRAINT `FK_FX_ORD_ATT_IMG_ATT` FOREIGN KEY (`IDORDATTP`) REFERENCES `FX_ORD_ATT` (`IDORDATTP`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_ORD_DOC
-- ----------------------------
DROP TABLE IF EXISTS `FX_ORD_DOC`;
CREATE TABLE `FX_ORD_DOC`  (
  `IDORDDOCP` int(11) NOT NULL AUTO_INCREMENT,
  `IDORDDOC` int(11) NULL DEFAULT NULL COMMENT 'ACCESS. Vine 0. Se actualizeaza dupa salvarea ACCESS. Trebuie endpoint',
  `IDORDP` int(11) NULL DEFAULT 0,
  `IDORDPARTP` int(11) NULL DEFAULT NULL,
  `DocJust` longtext CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `NumeDoc` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `TipDoc` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`IDORDDOCP`) USING BTREE,
  INDEX `IDORD`(`IDORDP` ASC) USING BTREE,
  INDEX `IDORDD`(`IDORDPARTP` ASC) USING BTREE,
  INDEX `IDORDJ`(`IDORDDOCP` ASC) USING BTREE,
  CONSTRAINT `FX_ORD_DOC__FX_ORD` FOREIGN KEY (`IDORDP`) REFERENCES `FX_ORD` (`IDORDP`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FX_ORD_DOC__FX_ORD_PART` FOREIGN KEY (`IDORDPARTP`) REFERENCES `FX_ORD_PART` (`IDORDPARTP`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_ORD_PART
-- ----------------------------
DROP TABLE IF EXISTS `FX_ORD_PART`;
CREATE TABLE `FX_ORD_PART`  (
  `IDORDPARTP` int(11) NOT NULL AUTO_INCREMENT,
  `IDORDPART` int(11) NULL DEFAULT NULL COMMENT 'ACCESS',
  `IDORDP` int(11) NULL DEFAULT 0,
  `Counter` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `DenBene` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodFiscal` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `ContIBAN` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Banca` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `tmpID` int(11) NULL DEFAULT NULL COMMENT 'ID-ul unic din access pentru randurile din tabelul temporar. Pentru asociere dupa salvare mariadb',
  PRIMARY KEY (`IDORDPARTP`) USING BTREE,
  INDEX `IDORD`(`IDORDP` ASC) USING BTREE,
  INDEX `IDORDD`(`IDORDPARTP` ASC) USING BTREE,
  CONSTRAINT `FX_ORD_PART__FX_ORD` FOREIGN KEY (`IDORDP`) REFERENCES `FX_ORD` (`IDORDP`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_ORD_PDF
-- ----------------------------
DROP TABLE IF EXISTS `FX_ORD_PDF`;
CREATE TABLE `FX_ORD_PDF`  (
  `IDPDF` int(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `IDORDP` int(11) NOT NULL,
  `NumeFisier` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Dimensiune` int(10) UNSIGNED NOT NULL,
  `Sha256` char(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Continut` longblob NOT NULL,
  `DataModif` datetime NOT NULL,
  PRIMARY KEY (`IDPDF`) USING BTREE,
  UNIQUE INDEX `UQ_FX_ORD_PDF_IDORDP`(`IDORDP` ASC) USING BTREE,
  CONSTRAINT `FK_FX_ORD_PDF_ORD` FOREIGN KEY (`IDORDP`) REFERENCES `FX_ORD` (`IDORDP`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_ORD_TBL
-- ----------------------------
DROP TABLE IF EXISTS `FX_ORD_TBL`;
CREATE TABLE `FX_ORD_TBL`  (
  `IDORDTBLP` int(11) NOT NULL AUTO_INCREMENT,
  `IDORDTBL` int(11) NULL DEFAULT NULL COMMENT 'ACCESS',
  `IDORDP` int(11) NULL DEFAULT 0,
  `IDORDPARTP` int(11) NULL DEFAULT 0,
  `IDRP` int(11) NULL DEFAULT NULL COMMENT 'ACCESS PK: FX_Receptii_Plati',
  `IdClsf` int(11) NULL DEFAULT 0,
  `IdClsfAcc` int(11) NULL DEFAULT NULL,
  `CodAI` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodAngajament` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodIndicator` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodSSI` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `TotalReceptii` double NULL DEFAULT 0,
  `PlatiAnt` double NULL DEFAULT 0,
  `Valoare` double NULL DEFAULT 0,
  `Ramas` double NULL DEFAULT 0,
  `Explicatie` text CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodPartener` varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `IdPartener` int(11) NULL DEFAULT NULL,
  `IdUnitate` int(11) NOT NULL,
  PRIMARY KEY (`IDORDTBLP`) USING BTREE,
  INDEX `IdClsf`(`IdClsf` ASC) USING BTREE,
  INDEX `IDORD`(`IDORDP` ASC) USING BTREE,
  INDEX `IDORDD`(`IDORDPARTP` ASC) USING BTREE,
  INDEX `IDORDT`(`IDORDTBLP` ASC) USING BTREE,
  INDEX `IdPartener`(`IdPartener` ASC) USING BTREE,
  INDEX `IdUnitate`(`IdUnitate` ASC) USING BTREE,
  INDEX `FX_ORD_TBL__FX_Indicatori`(`CodAI` ASC) USING BTREE,
  CONSTRAINT `FX_ORD_TBL__Clasificatii` FOREIGN KEY (`IdClsf`) REFERENCES `Clasificatii` (`IDClsf`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `FX_ORD_TBL__FX_Indicatori` FOREIGN KEY (`CodAI`) REFERENCES `FX_Indicatori` (`CodAI`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FX_ORD_TBL__FX_ORD` FOREIGN KEY (`IDORDP`) REFERENCES `FX_ORD` (`IDORDP`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FX_ORD_TBL__FX_ORD_PART` FOREIGN KEY (`IDORDPARTP`) REFERENCES `FX_ORD_PART` (`IDORDPARTP`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FX_ORD_TBL_ibfk_1` FOREIGN KEY (`IdPartener`) REFERENCES `Parteneri` (`IdPartener`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FX_ORD_TBL_ibfk_2` FOREIGN KEY (`IdUnitate`) REFERENCES `Unitati` (`IdUnitate`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_ORD_TBL_REC
-- ----------------------------
DROP TABLE IF EXISTS `FX_ORD_TBL_REC`;
CREATE TABLE `FX_ORD_TBL_REC`  (
  `IDORDRECP` int(11) NOT NULL AUTO_INCREMENT COMMENT 'MARIADB PK',
  `IDORDTBLP` int(11) NULL DEFAULT 0 COMMENT 'FK -> FX_ORD_TBL',
  `IDORDREC` int(11) NULL DEFAULT NULL COMMENT 'ACCESS PK',
  `IDRP` int(20) NULL DEFAULT NULL COMMENT 'FK -> FX_Receptii',
  `IdPlataFX` int(11) NULL DEFAULT NULL COMMENT 'FX_PLATI ACCESS',
  `Valoare` double NULL DEFAULT 0 COMMENT 'FX_Plati -> Suma',
  PRIMARY KEY (`IDORDRECP`) USING BTREE,
  INDEX `idx_FX_ORD_TBL_REC_tblp`(`IDORDTBLP` ASC) USING BTREE,
  INDEX `FX_ORD_TBL_REC__FX_Plati`(`IdPlataFX` ASC) USING BTREE,
  CONSTRAINT `FX_ORD_TBL_REC__FX_ORD_TBL` FOREIGN KEY (`IDORDTBLP`) REFERENCES `FX_ORD_TBL` (`IDORDTBLP`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FX_ORD_TBL_REC__FX_Plati` FOREIGN KEY (`IdPlataFX`) REFERENCES `FX_Plati` (`IdPlataFX`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_Plati
-- ----------------------------
DROP TABLE IF EXISTS `FX_Plati`;
CREATE TABLE `FX_Plati`  (
  `IdPlataFX` int(11) NOT NULL,
  `IDH` int(11) NULL DEFAULT NULL,
  `IdClsf` int(11) NULL DEFAULT NULL,
  `IdOP` int(11) NULL DEFAULT NULL,
  `CodAI` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodAngajament` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodIndicator` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `NrOP` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Data_plata` datetime NULL DEFAULT NULL,
  `Clsf` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Indicator_IBAN` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Obiectiv` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Probleme` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Program` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Proiect` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Referinta_TREZOR` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Suma` double NULL DEFAULT NULL,
  `Tip` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Incarcat` tinyint(1) NOT NULL DEFAULT 0,
  `Preluat` tinyint(1) NOT NULL DEFAULT 0,
  `DTQ` datetime NULL DEFAULT NULL,
  `IDREV` int(11) NULL DEFAULT NULL,
  `IdUnitate` int(11) NULL DEFAULT NULL,
  PRIMARY KEY (`IdPlataFX`) USING BTREE,
  UNIQUE INDEX `ux_FX_Plati_IDH`(`IDH` ASC) USING BTREE,
  INDEX `ix_FX_Plati_CodAI`(`CodAI` ASC) USING BTREE,
  INDEX `ix_FX_Plati_IdOP`(`IdOP` ASC) USING BTREE,
  INDEX `ix_FX_Plati_IdUnitate`(`IdUnitate` ASC) USING BTREE,
  CONSTRAINT `FX_Plati__FX_Indicatori` FOREIGN KEY (`CodAI`) REFERENCES `FX_Indicatori` (`CodAI`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FX_Plati__FX_Istoric` FOREIGN KEY (`IDH`) REFERENCES `FX_Istoric` (`ID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_Receptii
-- ----------------------------
DROP TABLE IF EXISTS `FX_Receptii`;
CREATE TABLE `FX_Receptii`  (
  `IDR` int(11) NOT NULL,
  `IDRH` int(11) NULL DEFAULT NULL,
  `IDH` int(11) NULL DEFAULT NULL,
  `IdClsf` int(11) NULL DEFAULT NULL,
  `CodAI` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodAngajament` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodIndicator` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Data` datetime NULL DEFAULT NULL,
  `CodSSI` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Clsf` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Valoare` double NULL DEFAULT NULL,
  `HASH` varchar(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `TipIntern` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `IDRD` int(11) NULL DEFAULT NULL,
  `IdRevizieClsf` int(11) NULL DEFAULT NULL,
  `Descriere` longtext CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Suma` double NULL DEFAULT NULL,
  `Tip` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `DTQ` datetime NULL DEFAULT NULL,
  `IDREV` int(11) NULL DEFAULT NULL,
  `Incarcat` tinyint(1) NULL DEFAULT NULL,
  `Preluat` tinyint(1) NULL DEFAULT NULL,
  `SumaPlatita` double NULL DEFAULT NULL,
  `CuRezervare` tinyint(1) NULL DEFAULT NULL,
  `DIF` double NULL DEFAULT NULL,
  `DIFC` double NULL DEFAULT NULL,
  `IdUnitate` int(11) NULL DEFAULT NULL,
  `ValoareOrig` double NULL DEFAULT NULL,
  PRIMARY KEY (`IDR`) USING BTREE,
  INDEX `CodAI`(`CodAI` ASC) USING BTREE,
  INDEX `IDH`(`IDH` ASC) USING BTREE,
  INDEX `FX_Receptii__FX_Receptii_H`(`IDRH` ASC) USING BTREE,
  CONSTRAINT `FX_Receptii__FX_Indicatori` FOREIGN KEY (`CodAI`) REFERENCES `FX_Indicatori` (`CodAI`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FX_Receptii__FX_Istoric` FOREIGN KEY (`IDH`) REFERENCES `FX_Istoric` (`ID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FX_Receptii__FX_Receptii_H` FOREIGN KEY (`IDRH`) REFERENCES `FX_Receptii_H` (`IDRH`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_Receptii_H
-- ----------------------------
DROP TABLE IF EXISTS `FX_Receptii_H`;
CREATE TABLE `FX_Receptii_H`  (
  `IDRH` int(11) NOT NULL,
  `IDRR` int(11) NULL DEFAULT NULL,
  `IDH` int(11) NULL DEFAULT NULL,
  `CodAngajament` varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `DataH` datetime NULL DEFAULT NULL,
  `Total` double NULL DEFAULT NULL,
  `Descriere` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `TipReceptie` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `NrCrt` int(11) NULL DEFAULT NULL,
  `HASH` varchar(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `DIFH` double NULL DEFAULT NULL,
  `DIFHC` double NULL DEFAULT NULL,
  `TotalOrig` double NULL DEFAULT NULL,
  `Sters` tinyint(1) NULL DEFAULT NULL,
  `EsteStergere` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Instantaneul provine dintr-un rand «Stergere receptie» (F21): ultimul din lant, DataH = data stergerii.',
  PRIMARY KEY (`IDRH`) USING BTREE,
  INDEX `CodAngajament`(`CodAngajament` ASC) USING BTREE,
  INDEX `IDH`(`IDH` ASC) USING BTREE,
  INDEX `FX_Receptii_H__FX_Receptii_R`(`IDRR` ASC) USING BTREE,
  CONSTRAINT `FX_Receptii_H__FX_Angajamente` FOREIGN KEY (`CodAngajament`) REFERENCES `FX_Angajamente` (`CodAngajament`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FX_Receptii_H__FX_Istoric` FOREIGN KEY (`IDH`) REFERENCES `FX_Istoric` (`ID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FX_Receptii_H__FX_Receptii_R` FOREIGN KEY (`IDRR`) REFERENCES `FX_Receptii_R` (`IDRR`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_Receptii_IMG
-- ----------------------------
DROP TABLE IF EXISTS `FX_Receptii_IMG`;
CREATE TABLE `FX_Receptii_IMG`  (
  `IDRDC` int(11) NOT NULL,
  `IDRR` int(11) NULL DEFAULT NULL,
  `IDRH` int(11) NULL DEFAULT NULL,
  `IMG` longtext CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Nume` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `IDRD` int(11) NULL DEFAULT NULL,
  PRIMARY KEY (`IDRDC`) USING BTREE,
  INDEX `FX_Receptii_IMG`(`IDRR` ASC) USING BTREE,
  CONSTRAINT `FX_Receptii_IMG` FOREIGN KEY (`IDRR`) REFERENCES `FX_Receptii_R` (`IDRR`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_Receptii_R
-- ----------------------------
DROP TABLE IF EXISTS `FX_Receptii_R`;
CREATE TABLE `FX_Receptii_R`  (
  `IDRR` int(11) NOT NULL,
  `NRCRT` int(11) NULL DEFAULT NULL,
  `CodAngajament` varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Tip` varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `DataR` datetime NULL DEFAULT NULL,
  `SumaAntet` double NULL DEFAULT NULL,
  `Descriere` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `HASH` varchar(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `TipReceptie` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Incarcat` tinyint(1) NULL DEFAULT NULL,
  `Preluat` tinyint(1) NULL DEFAULT NULL,
  `Sters` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Receptie stearsa pe site (F22). Ramane in baza: platile anterioare stergerii se sprijina pe ea.',
  `Reconstituit` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Receptie construita din propriile instantanee, creata SI stearsa inainte de prima descarcare (F26). Mereu impreuna cu Sters=1, dar alt fapt.',
  `ReconstituitNesigur` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'F28: la reconstituire, alta reconstituire pe acelasi angajament facea gruparea neverificabila (F27). Se pune pe toate, nu se sterge niciodata.',
  PRIMARY KEY (`IDRR`) USING BTREE,
  INDEX `CodAngajament`(`CodAngajament` ASC) USING BTREE,
  CONSTRAINT `FX_Receptii_R__FX_Angajamente` FOREIGN KEY (`CodAngajament`) REFERENCES `FX_Angajamente` (`CodAngajament`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_Receptii_RHR
-- ----------------------------
DROP TABLE IF EXISTS `FX_Receptii_RHR`;
CREATE TABLE `FX_Receptii_RHR`  (
  `IDRHR` int(11) NOT NULL,
  `IDRR` int(11) NULL DEFAULT NULL,
  `IdClsf` int(11) NULL DEFAULT NULL,
  `CodAngajament` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodIndicator` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodAI` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodProgram` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodSSI` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CreditBugetar` double NULL DEFAULT NULL,
  `ValoareN` double NULL DEFAULT NULL,
  `Valoare` double NULL DEFAULT NULL,
  `TipIntern` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `IdUnitate` int(11) NULL DEFAULT NULL,
  PRIMARY KEY (`IDRHR`) USING BTREE,
  INDEX `CodAI`(`CodAI` ASC) USING BTREE,
  CONSTRAINT `FX_Receptii_RHR__FX_Indicatori` FOREIGN KEY (`CodAI`) REFERENCES `FX_Indicatori` (`CodAI`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_Rezervari
-- ----------------------------
DROP TABLE IF EXISTS `FX_Rezervari`;
CREATE TABLE `FX_Rezervari`  (
  `IDRZ` int(11) NOT NULL,
  `IDH` int(11) NULL DEFAULT NULL,
  `IDREV` int(11) NULL DEFAULT NULL,
  `IdClsf` int(11) NULL DEFAULT NULL,
  `CodAI` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodAngajament` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodIndicator` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `DataRezervare` datetime NULL DEFAULT NULL,
  `R_CreditBug` double NULL DEFAULT NULL,
  `R_Initiala` double NULL DEFAULT NULL,
  `R_Anterioara` double NULL DEFAULT NULL,
  `R_Valoare` double NULL DEFAULT NULL,
  `R_Definitiva` double NULL DEFAULT NULL,
  `PB_CreditAng` double NULL DEFAULT NULL,
  `RI_CreditAng` double NULL DEFAULT NULL,
  `RD_AngLegal` double NULL DEFAULT NULL,
  `Incarcat` tinyint(1) NOT NULL DEFAULT 0,
  `Preluat` tinyint(1) NOT NULL DEFAULT 0,
  `AreDDF` tinyint(1) NOT NULL DEFAULT 0,
  `EInitiala` tinyint(1) NOT NULL DEFAULT 0,
  `EMicsorare` tinyint(1) NOT NULL DEFAULT 0,
  `EMarire` tinyint(1) NOT NULL DEFAULT 0,
  `DTQ` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`IDRZ`) USING BTREE,
  UNIQUE INDEX `ux_FX_Rezervari_IDH`(`IDH` ASC) USING BTREE,
  INDEX `ix_FX_Rezervari_IDREV`(`IDREV` ASC) USING BTREE,
  INDEX `ix_FX_Rezervari_CodAI`(`CodAI` ASC) USING BTREE,
  INDEX `ix_FX_Rezervari_IdClsf`(`IdClsf` ASC) USING BTREE,
  CONSTRAINT `FX_Rezervari__FX_DDF_REV` FOREIGN KEY (`IDREV`) REFERENCES `FX_DDF_REV` (`IDREV`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FX_Rezervari__FX_Indicatori` FOREIGN KEY (`CodAI`) REFERENCES `FX_Indicatori` (`CodAI`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FX_Rezervari__FX_Istoric` FOREIGN KEY (`IDH`) REFERENCES `FX_Istoric` (`ID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for FX_Rezervarii_IMG
-- ----------------------------
DROP TABLE IF EXISTS `FX_Rezervarii_IMG`;
CREATE TABLE `FX_Rezervarii_IMG`  (
  `IDRZC` int(11) NOT NULL,
  `IDRZ` int(11) NULL DEFAULT NULL,
  `IMG` longtext CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Nume` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`IDRZC`) USING BTREE,
  INDEX `IDRZ`(`IDRZ` ASC) USING BTREE,
  CONSTRAINT `FX_Rezervari_IMG__FX_Rezervari` FOREIGN KEY (`IDRZ`) REFERENCES `FX_Rezervari` (`IDRZ`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for Parteneri
-- ----------------------------
DROP TABLE IF EXISTS `Parteneri`;
CREATE TABLE `Parteneri`  (
  `IdPartener` int(11) NOT NULL AUTO_INCREMENT,
  `IdUnitate` int(11) NOT NULL,
  `CodPartener` varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `DenumirePartener` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodFiscal` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `ContIBAN` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Banca` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Adresa` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Tip` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Ascuns` tinyint(4) NULL DEFAULT NULL,
  `DataAdugare` datetime NULL DEFAULT current_timestamp(),
  `DataModificare` datetime NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`IdPartener`) USING BTREE,
  UNIQUE INDEX `UK_Parteneri_IdUnitate_CodPartener`(`IdUnitate` ASC, `CodPartener` ASC) USING BTREE,
  INDEX `CodFiscal`(`CodFiscal` ASC) USING BTREE,
  INDEX `ContIBAN`(`ContIBAN` ASC) USING BTREE,
  INDEX `Parteneri_ibfk_1`(`IdUnitate` ASC) USING BTREE,
  CONSTRAINT `Parteneri_ibfk_1` FOREIGN KEY (`IdUnitate`) REFERENCES `Unitati` (`IdUnitate`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for Parteneri_Coduri
-- ----------------------------
DROP TABLE IF EXISTS `Parteneri_Coduri`;
CREATE TABLE `Parteneri_Coduri`  (
  `IdPartenerAng` int(11) NOT NULL AUTO_INCREMENT,
  `IdPartener` int(11) NOT NULL DEFAULT 0,
  `CodPartener` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `IdClsf` int(11) NOT NULL DEFAULT 0,
  `IdClsfAcc` int(11) NOT NULL DEFAULT 0,
  `CodAng` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodInd` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `ContBancar` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `DataAdugare` datetime NULL DEFAULT current_timestamp(),
  `DataModificare` datetime NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`IdPartenerAng`) USING BTREE,
  UNIQUE INDEX `idx_partener_clsf`(`IdPartener` ASC, `IdClsf` ASC) USING BTREE,
  INDEX `IdClsf`(`IdClsf` ASC) USING BTREE,
  CONSTRAINT `Parteneri_Coduri_ibfk_1` FOREIGN KEY (`IdPartener`) REFERENCES `Parteneri` (`IdPartener`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `Parteneri_Coduri_ibfk_2` FOREIGN KEY (`IdClsf`) REFERENCES `Clasificatii` (`IDClsf`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for Unitati
-- ----------------------------
DROP TABLE IF EXISTS `Unitati`;
CREATE TABLE `Unitati`  (
  `IdUnitate` int(11) NOT NULL,
  `Detalii` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `SursaSector` varchar(3) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `An` int(4) NOT NULL,
  `CodProgram` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Ascuns` tinyint(4) NOT NULL DEFAULT 0,
  `DataAdugare` datetime NULL DEFAULT current_timestamp(),
  `DataModificare` datetime NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`IdUnitate`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for Users
-- ----------------------------
DROP TABLE IF EXISTS `Users`;
CREATE TABLE `Users`  (
  `IdUser` int(11) NOT NULL AUTO_INCREMENT,
  `Utilizator` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`IdUser`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for Users_R
-- ----------------------------
DROP TABLE IF EXISTS `Users_R`;
CREATE TABLE `Users_R`  (
  `IdDrept` int(11) NOT NULL AUTO_INCREMENT,
  `IdUser` int(11) NOT NULL,
  `NumeTabel` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `ColoanaPartitie` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  PRIMARY KEY (`IdDrept`) USING BTREE,
  INDEX `Users_R_ibfk_1`(`IdUser` ASC) USING BTREE,
  CONSTRAINT `Users_R_ibfk_1` FOREIGN KEY (`IdUser`) REFERENCES `Users` (`IdUser`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for Users_V
-- ----------------------------
DROP TABLE IF EXISTS `Users_V`;
CREATE TABLE `Users_V`  (
  `IdDreptValoare` int(11) NOT NULL AUTO_INCREMENT,
  `IdDrept` int(11) NOT NULL,
  `Valoare` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  PRIMARY KEY (`IdDreptValoare`) USING BTREE,
  INDEX `Users_V_ibfk_1`(`IdDrept` ASC) USING BTREE,
  CONSTRAINT `Users_V_ibfk_1` FOREIGN KEY (`IdDrept`) REFERENCES `Users_R` (`IdDrept`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

SET FOREIGN_KEY_CHECKS = 1;
