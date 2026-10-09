-- =====================================================================================
-- Slice 0112-04 -- error messages the operator chose to send from the K-BOT message window
-- (the button in its title bar). One row per report: the message as shown, where in the code it
-- came from, who/where/when, the state of the application and the tail of the two local logs.
--
-- Written only by POST /api/errors/report (PYTHON/routes/error_report.py): an idempotent insert by
-- `Rid` (a GUID made by the client), so a retry changes nothing. Who sent it (user, unit, database,
-- PC name) is taken from the SESSION on the server, not from the body, so it cannot be forged.
-- Rows are kept for good; `Stare` / `Observatii` are for whoever reads them.
--
-- APPLY ONCE, on the K-BOT server, in AVACONT_COMUN, BEFORE the client that has the button is
-- published. The service account AVACONT already has INSERT / UPDATE on AVACONT_COMUN.*
-- (sql/0076_avacont_drepturi.sql): no GRANT needed. utf8mb3 / utf8mb3_general_ci like the rest of
-- the K-BOT schemas (the route replaces 4-byte characters before it writes).
-- =====================================================================================

USE `AVACONT_COMUN`;

CREATE TABLE IF NOT EXISTS `FX_RaportErori` (
  `IdRaport`       bigint(20)    NOT NULL AUTO_INCREMENT,
  -- GUID made on the client, lower case, 36 characters: the retry guard.
  `Rid`            char(36)      NOT NULL,
  -- When the message was shown (the client's clock, UTC) and when the server got the report.
  `MomentAfisat`   datetime      NULL DEFAULT NULL,
  `MomentPrimit`   datetime      NOT NULL DEFAULT UTC_TIMESTAMP(),
  -- From the session on the server.
  `Utilizator`     varchar(120)  NOT NULL DEFAULT '',
  `IdUnitate`      int(11)       NULL DEFAULT NULL,
  `DbName`         varchar(64)   NOT NULL DEFAULT '',
  `NumePc`         varchar(100)  NOT NULL DEFAULT '',
  `Ip`             varchar(45)   NOT NULL DEFAULT '',
  -- From the client: the working context.
  `NumeUnitate`    varchar(200)  NOT NULL DEFAULT '',
  `Cf`             varchar(32)   NOT NULL DEFAULT '',
  `An`             smallint(6)   NULL DEFAULT NULL,
  `Ss`             varchar(16)   NOT NULL DEFAULT '',
  `CodProgram`     varchar(32)   NOT NULL DEFAULT '',
  `Rol`            varchar(32)   NOT NULL DEFAULT '',
  -- The application and the machine.
  `VersiuneApp`    varchar(32)   NOT NULL DEFAULT '',
  `Sistem`         varchar(200)  NOT NULL DEFAULT '',
  `Runtime`        varchar(80)   NOT NULL DEFAULT '',
  `NumeCalculator` varchar(100)  NOT NULL DEFAULT '',
  `UtilizatorWin`  varchar(100)  NOT NULL DEFAULT '',
  `Cultura`        varchar(20)   NOT NULL DEFAULT '',
  `Ecran`          varchar(120)  NOT NULL DEFAULT '',
  `Tema`           varchar(40)   NOT NULL DEFAULT '',
  `MemorieMb`      int(11)       NULL DEFAULT NULL,
  `ActivMinute`    int(11)       NULL DEFAULT NULL,
  -- The message: where in the code (File.Member, line), the window it was shown over, the windows open.
  `Sursa`          varchar(200)  NOT NULL DEFAULT '',
  `LinieSursa`     int(11)       NULL DEFAULT NULL,
  `FereastraOwner` varchar(300)  NOT NULL DEFAULT '',
  `FereastraActiva` varchar(300) NOT NULL DEFAULT '',
  `FerestreDeschise` text        NULL DEFAULT NULL,
  `Titlu`          varchar(300)  NOT NULL DEFAULT '',
  `Antet`          varchar(600)  NOT NULL DEFAULT '',
  `Mesaj`          mediumtext    NULL DEFAULT NULL,
  `Butoane`        varchar(40)   NOT NULL DEFAULT '',
  -- The tail of harness_errors.log (exceptions, with stacks) and of mesaje_operator.log (what the operator was told).
  `JurnalErori`    mediumtext    NULL DEFAULT NULL,
  `JurnalMesaje`   mediumtext    NULL DEFAULT NULL,
  -- For whoever reads the reports.
  `Stare`          varchar(12)   NOT NULL DEFAULT 'NOU',
  `Observatii`     text          NULL DEFAULT NULL,
  PRIMARY KEY (`IdRaport`) USING BTREE,
  UNIQUE INDEX `ux_FX_RaportErori_Rid` (`Rid` ASC) USING BTREE,
  INDEX `ix_FX_RaportErori_Primit` (`MomentPrimit` ASC) USING BTREE,
  INDEX `ix_FX_RaportErori_Stare` (`Stare` ASC, `MomentPrimit` ASC) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- Check after applying:
-- SHOW CREATE TABLE `AVACONT_COMUN`.`FX_RaportErori`;
