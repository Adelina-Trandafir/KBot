-- =====================================================================================
-- Slice 0110-09 -- public site: who looked at the presentation page (operator, 05.10.2026).
-- One row per page visit, written by routes/landing/vizite.py (POST /api/vizita, public, no
-- cookie): the visitor's address and country, mobile or not, how long the page was really in
-- front of them (tab visible and the person active) and how long each section of the page
-- was in the middle of the screen. Read back only by the admin page of the portal
-- (routes/portal/admin.py), and only for the accounts in config.PORTAL_ADMIN_EMAILS.
-- AVACONT_COMUN, utf8mb3 / utf8mb3_general_ci like every other table.
-- Run BEFORE deploying the route. The service account needs SELECT/INSERT/UPDATE/DELETE on
-- the table (it has them on the AVACONT_COMUN tables it already writes; check after creating).
-- Rows older than config.VIZITE_RETENTIE_ZILE (180 by default) are deleted by the route.
-- Safe to run twice.
-- =====================================================================================
USE `AVACONT_COMUN`;

CREATE TABLE IF NOT EXISTS `Vizite_Site` (
  `IdVizita`  bigint(20)   NOT NULL AUTO_INCREMENT,
  `VisitId`   char(32)     CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Prima`     datetime     NOT NULL DEFAULT current_timestamp(),
  `Ultima`    datetime     NOT NULL DEFAULT current_timestamp(),
  `IP`        varchar(45)  CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Tara`      char(2)      CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Mobil`     tinyint(1)   NOT NULL DEFAULT 0,
  `Bot`       tinyint(1)   NOT NULL DEFAULT 0,
  `Pagina`    varchar(40)  CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT '/',
  `Referrer`  varchar(80)  CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Ecran`     varchar(16)  CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Limba`     varchar(16)  CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `UserAgent` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `DurataSec` int(11)      NOT NULL DEFAULT 0,
  `Sectiuni`  text         CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`IdVizita`) USING BTREE,
  UNIQUE INDEX `ux_Vizite_Site_VisitId`(`VisitId` ASC) USING BTREE,
  INDEX `ix_Vizite_Site_Prima`(`Prima` ASC) USING BTREE,
  INDEX `ix_Vizite_Site_IP`(`IP` ASC) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;
