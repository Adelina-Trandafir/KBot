-- =====================================================================================
-- Slice 0110-02 -- public site: the «Cere mai multe detalii» form (operator, 05.10.2026).
-- One row per request sent from /detalii. Written by routes/detalii/detalii.py, which also
-- mails the request to DETALII_EMAIL (info@avatarsoft.ro). Nothing reads the table back yet:
-- it is the record, so a request is never lost when the mail is.
-- AVACONT_COMUN, utf8mb3 / utf8mb3_general_ci like every other table (the route strips
-- characters outside the BMP before the insert). Run BEFORE deploying the route.
-- The service account needs INSERT/UPDATE/SELECT on the table (it has them on the
-- AVACONT_COMUN tables it already writes; check after creating).
-- Safe to run twice.
-- =====================================================================================
USE `AVACONT_COMUN`;

CREATE TABLE IF NOT EXISTS `FX_CereriDetalii` (
  `IdCerere`    int(11)      NOT NULL AUTO_INCREMENT,
  `Nume`        varchar(120) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Institutie`  varchar(160) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `CF`          varchar(20)  CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Email`       varchar(80)  CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Telefon`     varchar(30)  CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Mesaj`       text         CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Stare`       varchar(16)  CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT 'Noua',
  `Notificat`   tinyint(1)   NOT NULL DEFAULT 0,
  `DataCerere`  datetime     NOT NULL DEFAULT current_timestamp(),
  `DataRaspuns` datetime     NULL DEFAULT NULL,
  `IpAddress`   varchar(45)  CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`IdCerere`) USING BTREE,
  INDEX `ix_FX_CereriDetalii_Stare`(`Stare` ASC) USING BTREE,
  INDEX `ix_FX_CereriDetalii_Email`(`Email` ASC) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;
