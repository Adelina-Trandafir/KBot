-- =====================================================================================
-- Slice 0000-21 -- the questions typed in K-BOT's help (the «?» popup and the help window),
-- with the results shown, what was opened and the 1-5 star rating. Read later to decide whether
-- plain search is good enough (docs/PLAN_help_assistant.md, «step 4»).
--
-- NOTHING ABOUT WHO OR WHERE: no user, e-mail, unit, DC, IP, machine or session. `Qid` is a
-- fresh random GUID made on the client for each question -- not derived from any of those.
-- Rows are kept for good (operator, 30.09.2026).
--
-- Written only by POST /api/help/feedback (PYTHON/routes/help_feedback.py): an idempotent upsert
-- by Qid, so a batch sent twice (a retry) changes nothing, and a later rating updates the row.
--
-- APPLY ONCE, on the K-BOT server, in AVACONT_COMUN, BEFORE the client that sends questions is
-- published. The service account AVACONT already has INSERT / UPDATE on AVACONT_COMUN.*
-- (sql/0076_avacont_drepturi.sql): no GRANT needed. Naming follows AVACONT_COMUN (FX_ prefix,
-- PascalCase columns), checked against MariaDB_Schema/AVACONT_COMUN.sql (dump of 26.09.2026).
-- =====================================================================================

USE `AVACONT_COMUN`;

CREATE TABLE IF NOT EXISTS `FX_AjutorIntrebari` (
  `IdIntrebare`    bigint(20)    NOT NULL AUTO_INCREMENT,
  -- Random GUID from the client, lower case, 36 characters. One row per question.
  `Qid`            char(36)      NOT NULL,
  -- What was typed, trimmed, at most 300 characters.
  `Intrebare`      varchar(300)  NOT NULL,
  -- When it was asked, UTC (the client's clock).
  `Moment`         datetime      NOT NULL,
  -- The help parts the login could read: 'contabil', 'contabil+avansat' or 'director'.
  `Parte`          varchar(32)   NOT NULL,
  -- K-BOT's version and the help's version (the date the help content was last brought up to date).
  `VersiuneApp`    varchar(32)   NOT NULL,
  `VersiuneAjutor` varchar(16)   NOT NULL DEFAULT '',
  -- The top hits shown, best first, at most 5, as 'topicId#section' joined by '|'. Ids only.
  `Rezultate`      varchar(600)  NOT NULL DEFAULT '',
  -- The hit opened ('topicId#section'), NULL = none.
  `Deschis`        varchar(120)  NULL DEFAULT NULL,
  -- The button used on a hit: 'open' («Deschide ...») or 'tour' («Tur ghidat»), NULL = none.
  `Actiune`        varchar(8)    NULL DEFAULT NULL,
  -- The star rating under the results: 1..5, NULL = not rated.
  `Nota`           tinyint(4)    NULL DEFAULT NULL CHECK (`Nota` IS NULL OR `Nota` BETWEEN 1 AND 5),
  -- Where it was asked: 'popup' (the «?» button) or 'fereastra' (the help window).
  `Loc`            varchar(10)   NOT NULL,
  PRIMARY KEY (`IdIntrebare`) USING BTREE,
  UNIQUE INDEX `ux_FX_AjutorIntrebari_Qid` (`Qid` ASC) USING BTREE,
  INDEX `ix_FX_AjutorIntrebari_Moment` (`Moment` ASC) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- Check after applying:
-- SHOW CREATE TABLE `AVACONT_COMUN`.`FX_AjutorIntrebari`;
