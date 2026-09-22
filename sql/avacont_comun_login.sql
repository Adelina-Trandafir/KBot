-- =====================================================================
--  AVACONT_COMUN — Login system schema
--    CAI          — units registry (source of per-unit SessionContext)
--    FX_LoginLog  — session audit (role, ip, pcname, login/logout timestamps)
--  Common SOURCE database; NEVER a proc_SchemaDiff_DDL target.
--  InnoDB / utf8mb4. Idempotent (IF NOT EXISTS).
-- =====================================================================

CREATE DATABASE IF NOT EXISTS `AVACONT_COMUN`
  CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

USE `AVACONT_COMUN`;

-- ---------------------------------------------------------------------
--  CAI — units registry (mirrors the legacy Access CAI table).
--  `DbName` replaces the legacy `Cale` connection string: only the target
--  MariaDB schema name is stored (e.g. '000_DEMO').
--  `CF` and `CodProgram` are NEW columns (per design decision); CF is the
--  authoritative source of globCF for a unit.
--  IdUnitate values are preserved from Access -> plain INT PK.
-- ---------------------------------------------------------------------
-- ---------------------------------------------------------------------
--  ⚠ THIS FILE IS A STALE REFERENCE, NOT THE SERVER (read 22.09.2026,
--  slice 0075). It is kept for the column meanings; the shapes below have
--  drifted. What the live K-BOT server actually has, and what any code
--  must be written against:
--    * CAI's primary key is `IdCai`, AUTO_INCREMENT — NOT `IdUnitate`.
--    * `IdUnitate` is a plain, NON-UNIQUE column. 70 rows, MAX = 200.
--      Nothing protects it, which is why the provisioning job of §5.6
--      takes GET_LOCK('cai_idunitate') before reading MAX(IdUnitate)+1.
--    * `DbName` equals `DC` on every row.
--  Corrected below. Treat the rest of the file the same way: useful for
--  what a column MEANS, not authoritative for its type or its keys.
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `CAI` (
  `IdCai`       INT           NOT NULL AUTO_INCREMENT,
  `IdUnitate`   INT           NOT NULL,           -- NOT unique; see the note above
  `DbName`      VARCHAR(64)   NOT NULL,           -- per-unit schema, e.g. '000_DEMO'
  `NumeUnitate` VARCHAR(255)      NULL,
  `AlteDetalii` VARCHAR(255)      NULL,
  `Sursa`       VARCHAR(32)       NULL,           -- SectorSursa (e.g. '02A')
  `CF`          VARCHAR(32)       NULL,           -- fiscal code (NEW; source of globCF)
  `CodProgram`  VARCHAR(32)       NULL,           -- program code (NEW)
  `AnDate`      INT               NULL,           -- year (-> ANL); CLng(AnDate)=globANL
  `DC`          VARCHAR(32)       NULL,           -- data-context tag (legacy DC())
  PRIMARY KEY (`IdCai`),
  KEY `ix_CAI_IdUnitate`  (`IdUnitate`),
  KEY `ix_CAI_DbName`    (`DbName`),
  KEY `ix_CAI_DC_AnDate` (`DC`, `AnDate`),
  KEY `ix_CAI_Sursa_An`  (`Sursa`, `AnDate`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
-- ANSWERED (22.09.2026): DbName is not unique and neither is IdUnitate — a unit
-- gets one CAI row per sector-source, so several rows share both. That is why
-- the free-name check of §5.4 scans SCHEMATA *and* CAI, and why §5.6 step 3
-- needs an explicit lock: no key stops two runs from picking the same
-- MAX(IdUnitate)+1.

-- ---------------------------------------------------------------------
--  FX_LoginLog — session audit. One row per successful login.
--  Role is stored (store-only; no enforcement yet).
--  LogoutTime is stamped later by /api/auth/logout.
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `FX_LoginLog` (
  `IdLog`      INT           NOT NULL AUTO_INCREMENT,
  `Username`   VARCHAR(128)  NOT NULL,
  `Role`       VARCHAR(32)       NULL,            -- Contabil / Administrator (from username suffix)
  `IdUnitate`  INT               NULL,
  `DbName`     VARCHAR(64)       NULL,
  `IpAddress`  VARCHAR(45)       NULL,            -- IPv4/IPv6 (server-side remote_addr)
  `PcName`     VARCHAR(128)      NULL,
  `LoginTime`  DATETIME      NOT NULL,
  `LogoutTime` DATETIME          NULL,
  PRIMARY KEY (`IdLog`),
  KEY `ix_LoginLog_User` (`Username`),
  KEY `ix_LoginLog_Time` (`LoginTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
