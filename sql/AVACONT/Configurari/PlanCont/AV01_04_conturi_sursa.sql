-- =====================================================================================
-- Slice AV01-04 -- RUN THIS ON `AVACONT_SURSA` ONLY (Navicat / mysql, as root).
--
-- AVACONT_SURSA is the template: the schema sync copies its STRUCTURE to every unit database
-- (same order as 0111_01_sursa.sql). The sync creates a missing table by itself (checked in
-- PYTHON/routes/schema_sync/schema_diff.py, _tables): nothing has to be registered anywhere.
--
-- WHAT THIS CREATES (stage 1 of the AVACONT port, window «Conturi»; design:
-- docs/access-map/avacont/proiect-de-date-etapa-1.md, sections 3)
--   TZ_Subunitati   a subordinate unit with its own accounting. It decides the chapter and the
--                   sector+source (65.01 = 01A, 65.02 = 02A, 65.10 = 02E, 65.11 = 01A). SEPARATE
--                   from VC_Subunitati (VERCON) and WITHOUT any link to it (operator, 23.10.2026).
--   TZ_PlanCont     the chart of accounts, PER SUBUNIT. One row = one full account. The base account
--                   is a foreign key into AVACONT_COMUN.DefaCont (818 rows, 7 digits).
--   TZ_SoldInitial  opening balances (year + account [+ partner]). Replaces Access PlanCont.SID/SIC
--                   and ParteneriSI. Filled by the Migrator for the current year only.
--
-- RULES (operator, 23.10.2026)
--   * Deleting an account that has operations / is in a template / has an opening balance is BLOCKED
--     (RESTRICT everywhere, no cascade on accounting data). The server turns the refusal into a message.
--   * An account out of use is marked Activ = 0, not deleted. The plan does not depend on the year (no An).
--   * A unit without subunits has ONE default subunit (Implicita = 1), so no key is ever NULL.
--
-- NOT HERE: SID/SIC columns on the account (they are in TZ_SoldInitial), the Access RPC/RPD live there too as RulajCreditor/RulajDebitor, ContLung, Expr1, TipCont (decided out).
-- NOT VERIFIED: that every ClsfF / ClsfE code of the Access plan exists in AVACONT_COMUN.DefaClsfF /
--   DefaClsfE (the foreign keys below need it). The Migrator's source-vs-target counts will show a miss.
--
-- Safe to run twice (IF NOT EXISTS). utf8mb3_general_ci like every other table.
-- =====================================================================================

CREATE TABLE IF NOT EXISTS `AVACONT_SURSA`.`TZ_Subunitati` (
  `IdSubunitate`   int(11) NOT NULL AUTO_INCREMENT,
  `Denumire`       varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Capitol`        varchar(5) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'Budget chapter, e.g. 65.01',
  `SursaSector`    varchar(3) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'Sector + source, e.g. 01A, 02E',
  `ZecimaleSalarii` tinyint(4) NOT NULL DEFAULT 0 COMMENT '0 or 2: salaries shown / typed with 2 decimals (EU projects). Was the Access flag Zec2',
  `Implicita`      tinyint(1) NOT NULL DEFAULT 0 COMMENT '1 = the default subunit of the unit (exactly one)',
  `Activ`          tinyint(1) NOT NULL DEFAULT 1,
  `DataAdaugare`   datetime NOT NULL DEFAULT current_timestamp(),
  `DataModificare` datetime NULL DEFAULT NULL ON UPDATE current_timestamp(),
  PRIMARY KEY (`IdSubunitate`) USING BTREE,
  INDEX `IX_TZ_Subunitati_Capitol`(`Capitol` ASC) USING BTREE,           -- NOT unique since AV01_04e: several subunits may share a chapter
  UNIQUE INDEX `UK_TZ_Subunitati_Denumire`(`Denumire` ASC) USING BTREE,
  INDEX `IX_TZ_Subunitati_SursaSector`(`SursaSector` ASC) USING BTREE,
  CONSTRAINT `TZ_Subunitati__DefaSS` FOREIGN KEY (`SursaSector`) REFERENCES `AVACONT_COMUN`.`DefaSursaSector` (`SursaSector`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `CK_TZ_Subunitati_Zec` CHECK (`ZecimaleSalarii` IN (0, 2))
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

CREATE TABLE IF NOT EXISTS `AVACONT_SURSA`.`TZ_PlanCont` (
  `IdCont`         int(11) NOT NULL AUTO_INCREMENT,
  `IdSubunitate`   int(11) NOT NULL,
  `SursaSector`    varchar(3) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'Sector + source of THIS account (e.g. 01A): follows the title bar choice, editable only on «Toate». Subunit value = the default',
  `Cont`           int(11) NOT NULL COMMENT 'Base account (7 digits) = AVACONT_COMUN.DefaCont.Simbol',
  `Analitic`       varchar(10) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Digits, capital letters and _ only',
  `ClsfF`          varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Functional classification, only when DefaCont.CLSFF = 1',
  `ClsfE`          varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Economic classification, only when DefaCont.CLSFE = 1; never without ClsfF',
  `SimbolCont`     varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'Cont + sector + source + [ClsfF] + [ClsfE] + [.Analitic]; written by the server, unique per subunit',
  `Denumire`       varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `FelCont`        char(1) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'A = asset, B = bifunctional, P = liability',
  `Extra`          tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Access flag Extra (Da/Nu)',
  `Activ`          tinyint(1) NOT NULL DEFAULT 1 COMMENT '0 = out of use (never deleted once used)',
  `DataAdaugare`   datetime NOT NULL DEFAULT current_timestamp(),
  `DataModificare` datetime NULL DEFAULT NULL ON UPDATE current_timestamp(),
  PRIMARY KEY (`IdCont`) USING BTREE,
  UNIQUE INDEX `UK_TZ_PlanCont_Simbol`(`IdSubunitate` ASC, `SimbolCont` ASC) USING BTREE,
  INDEX `IX_TZ_PlanCont_Cont`(`Cont` ASC) USING BTREE,
  INDEX `IX_TZ_PlanCont_ClsfF`(`ClsfF` ASC) USING BTREE,
  INDEX `IX_TZ_PlanCont_ClsfE`(`ClsfE` ASC) USING BTREE,
  CONSTRAINT `TZ_PlanCont__Subunitate` FOREIGN KEY (`IdSubunitate`) REFERENCES `TZ_Subunitati` (`IdSubunitate`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  INDEX `IX_TZ_PlanCont_SS`(`SursaSector` ASC) USING BTREE,
  CONSTRAINT `TZ_PlanCont__DefaSS` FOREIGN KEY (`SursaSector`) REFERENCES `AVACONT_COMUN`.`DefaSursaSector` (`SursaSector`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `TZ_PlanCont__DefaCont` FOREIGN KEY (`Cont`) REFERENCES `AVACONT_COMUN`.`DefaCont` (`Simbol`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `TZ_PlanCont__DefaClsfF` FOREIGN KEY (`ClsfF`) REFERENCES `AVACONT_COMUN`.`DefaClsfF` (`ClsfF`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `TZ_PlanCont__DefaClsfE` FOREIGN KEY (`ClsfE`) REFERENCES `AVACONT_COMUN`.`DefaClsfE` (`ClsfE`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `CK_TZ_PlanCont_Fel` CHECK (`FelCont` IN ('A', 'B', 'P')),
  CONSTRAINT `CK_TZ_PlanCont_ClsfE` CHECK (`ClsfE` IS NULL OR `ClsfF` IS NOT NULL)
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

CREATE TABLE IF NOT EXISTS `AVACONT_SURSA`.`TZ_SoldInitial` (
  `IdSold`         int(11) NOT NULL AUTO_INCREMENT,
  `An`             int(11) NOT NULL,
  `IdCont`         int(11) NOT NULL,
  `IdPartener`     int(11) NULL DEFAULT NULL COMMENT 'NULL = the balance of the account itself; set = the balance of that partner / analytic',
  `PartenerKey`    int(11) GENERATED ALWAYS AS (COALESCE(`IdPartener`, 0)) PERSISTENT COMMENT 'Makes the unique key treat NULL as one value',
  `SoldDebitor`    decimal(18, 2) NOT NULL DEFAULT 0.00,
  `SoldCreditor`   decimal(18, 2) NOT NULL DEFAULT 0.00,
  `RulajDebitor`   decimal(18, 2) NOT NULL DEFAULT 0.00 COMMENT 'Cumulated turnover before the opening (Access RPD)',
  `RulajCreditor`  decimal(18, 2) NOT NULL DEFAULT 0.00 COMMENT 'Cumulated turnover before the opening (Access RPC)',
  `DataAdaugare`   datetime NOT NULL DEFAULT current_timestamp(),
  `DataModificare` datetime NULL DEFAULT NULL ON UPDATE current_timestamp(),
  PRIMARY KEY (`IdSold`) USING BTREE,
  UNIQUE INDEX `UK_TZ_SoldInitial`(`An` ASC, `IdCont` ASC, `PartenerKey` ASC) USING BTREE,
  INDEX `IX_TZ_SoldInitial_Cont`(`IdCont` ASC) USING BTREE,
  INDEX `IX_TZ_SoldInitial_Partener`(`IdPartener` ASC) USING BTREE,
  CONSTRAINT `TZ_SoldInitial__Cont` FOREIGN KEY (`IdCont`) REFERENCES `TZ_PlanCont` (`IdCont`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `TZ_SoldInitial__Partener` FOREIGN KEY (`IdPartener`) REFERENCES `Parteneri` (`IdPartener`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `CK_TZ_SoldInitial_Pozitiv` CHECK (`SoldDebitor` >= 0 AND `SoldCreditor` >= 0 AND `RulajDebitor` >= 0 AND `RulajCreditor` >= 0)
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- 24.10.2026 (operator): the window shows and edits the cumulated turnovers too (Access RPD / RPC), so TZ_SoldInitial has them.
-- If the CREATE above was already run WITHOUT them, this adds them (safe to run twice):
ALTER TABLE `AVACONT_SURSA`.`TZ_SoldInitial`
  ADD COLUMN IF NOT EXISTS `RulajDebitor`  decimal(18, 2) NOT NULL DEFAULT 0.00 COMMENT 'Cumulated turnover before the opening (Access RPD)' AFTER `SoldCreditor`,
  ADD COLUMN IF NOT EXISTS `RulajCreditor` decimal(18, 2) NOT NULL DEFAULT 0.00 COMMENT 'Cumulated turnover before the opening (Access RPC)' AFTER `RulajDebitor`;

-- Checks (read only):
-- SHOW CREATE TABLE `AVACONT_SURSA`.`TZ_Subunitati`;
-- SHOW CREATE TABLE `AVACONT_SURSA`.`TZ_PlanCont`;
-- SHOW CREATE TABLE `AVACONT_SURSA`.`TZ_SoldInitial`;
-- Then: AvacontPush, tab «Sincronizare schema», SAFE, on every unit database.
