-- =====================================================================================
-- Slice 0100-02 -- `Setari`: settings that the SERVER decides, one small table per database.
--
-- WHY (operator, 01.10.2026). Multi-thread downloading must be switched on / limited from the
-- server, not by each operator. The first two settings:
--   Multithread       0 / 1  -- 0: the «Descarcari multiple» page of K-BOT Setari is not shown and
--                              every download runs one at a time; 1: the page is shown.
--   Multithread_Max   n      -- the most FOREXE tabs a download may use at once (K-BOT never goes
--                              above 10, whatever is written here).
-- K-BOT reads the table of the database the operator is connected to (GET /api/setari), once
-- after login and again after a change of unit.
--
-- THE TABLE
--   Cheie        the setting's key (primary key). English / ASCII, read by code.
--   TextVizibil  the text the operator may see for it (Romanian, literal diacritics).
--   Valoare      the value, kept as text so ONE column holds all three kinds:
--                  int   -> digits only            '3'
--                  date  -> 'YYYY-MM-DD' or 'YYYY-MM-DD HH:MM:SS'
--                  text  -> anything
--   Tip          which of the three Valoare is: 'int' | 'date' | 'text'. The server turns Valoare
--                into that type before it answers; a value that does not fit its Tip answers null.
--
-- THIS FILE: the table + the default rows on AVACONT_SURSA, the template new units are cloned
-- from. The same table + rows for EVERY EXISTING unit database: 0100_02_setari_toate_bazele.sql.
-- Safe to run twice (IF NOT EXISTS / INSERT IGNORE): an existing row is never overwritten, so a
-- value you changed by hand survives a second run.
--
-- To change a setting afterwards, e.g. allow 3 tabs on one unit:
--   UPDATE `<unit db>`.`Setari` SET Valoare = '1' WHERE Cheie = 'Multithread';
--   UPDATE `<unit db>`.`Setari` SET Valoare = '3' WHERE Cheie = 'Multithread_Max';
-- Nothing to restart: K-BOT reads it at the operator's next login / unit change.
-- =====================================================================================

SET NAMES utf8mb4;

CREATE TABLE IF NOT EXISTS `AVACONT_SURSA`.`Setari` (
  `Cheie`       varchar(64)  NOT NULL COMMENT 'Slice 0100-02: the setting key (ASCII, read by code)',
  `TextVizibil` varchar(255) NOT NULL COMMENT 'The text the operator may see for it',
  `Valoare`     varchar(255) NULL DEFAULT NULL COMMENT 'The value, as text; its kind is Tip',
  `Tip`         enum('int','date','text') NOT NULL DEFAULT 'text' COMMENT 'Which kind Valoare is',
  PRIMARY KEY (`Cheie`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT IGNORE INTO `AVACONT_SURSA`.`Setari` (`Cheie`, `TextVizibil`, `Valoare`, `Tip`) VALUES
  ('Multithread',     'Descărcare pe mai multe taburi (1 = permisă, 0 = oprită)', '0', 'int'),
  ('Multithread_Max', 'Numărul maxim de taburi folosite deodată',                '1', 'int');
