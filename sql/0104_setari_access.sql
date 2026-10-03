-- =====================================================================================
-- Slice 0104 -- `Setari.Access`: is this unit a client of the Access application too?
--
-- WHY (operator, 02.10.2026). The Migrator (Migrare\) and the code that talks to Access
-- (KBot.Access.dll) matter ONLY to clients who also run the Access VBA application. A new
-- client gets a K-BOT without them, so the server must know which kind each unit is:
--   Access = 1   the unit has the Access application: K-BOT shows the Access features and the
--                client is served the update package WITH the Access components.
--   Access = 0   it does not: no Access feature is shown, and the update package is the one
--                WITHOUT the Access components.
-- K-BOT learns it two ways, both from this row:
--   * before login, POST /api/access/client-type (routes/access.py) answers from the rows of
--     every unit the e-mail may open -- it picks the update package;
--   * after login, GET /api/setari (routes/setari.py) carries the row of the connected unit --
--     it switches the Access features on or off.
--
-- A unit with no `Setari` table, or no `Access` row, counts as Access = 0 ("no row = off").
-- NEW units are cloned from AVACONT_SURSA by routes/inregistrare/provizionare.py, which copies
-- the table definition only, not the rows: a new unit therefore starts with NO row = 0. That is
-- the wanted default (a newly registered client has no Access application).
--
-- THIS FILE: the default row on AVACONT_SURSA, the template (documentation of the key; it is not
-- copied to new units). The row for EVERY EXISTING unit database: 0104_setari_access_toate_bazele.sql.
-- Safe to run twice: an existing row is never overwritten.
--
-- To change one unit afterwards:
--   UPDATE `<unit db>`.`Setari` SET Valoare = '0' WHERE Cheie = 'Access';
-- Nothing to restart: K-BOT reads it at the operator's next start / login.
-- Needs sql/0100_02_setari.sql first (the table).
-- =====================================================================================

SET NAMES utf8mb4;

INSERT IGNORE INTO `AVACONT_SURSA`.`Setari` (`Cheie`, `TextVizibil`, `Valoare`, `Tip`) VALUES
  ('Access', 'Clientul folosește și aplicația Access (1 = da, 0 = nu)', '0', 'int');
