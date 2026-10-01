-- =====================================================================================
-- Slice 0100 -- Multithreading: the date an angajament was last updated from FOREXE.
--
-- WHY (operator, 01.10.2026). Slice 0100 downloads several angajamente at once, one FOREXE tab
-- per download. Two of its features need to know HOW OLD the local copy of an angajament is:
--   * «Actualizeaza la conectare angajamentele neactualizate de N zile» (Setari -> Aplicatie,
--     N at most 10): after a FOREXE connection K-BOT picks the angajamente whose
--     `DataActualizare` is older than N days and downloads them with the multi-thread mechanism;
--   * the selection window «Actualizeaza angajamente...» (popup of the tree) shows the date, so
--     the operator can tick the old ones.
--
-- THE COLUMN
--   FX_Angajamente.DataActualizare  datetime NULL
--   The moment (server time) the LAST download of this angajament was SAVED on the server, i.e.
--   the commit of phase two of the ingest (PrelucrareCompleta / Receptii / Rezervari refresh).
--   NULL = no download has been saved since the column exists. K-BOT treats an angajament that
--   already has history / indicators and a NULL date as «old» (it was downloaded before this
--   slice), and one with no history and no indicators (header only, from the list refresh) as
--   «never downloaded» -- the on-connect update leaves those alone.
--   It is NOT `DTQ` (an unrelated datetime) and NOT `DataCreare` / `DataDefinitivare` (dates
--   FOREXE gives the angajament): this one is K-BOT's own bookkeeping.
--
-- Written by: routes/forexe/prelucrare.py (the commit of the ingest), once per saved package.
-- Read by:    routes/forexe/tree.py (GET /api/forexe/tree -> AngajamentTreeInfo.DataActualizare).
--
-- APPLY ON EVERY UNIT DATABASE (000_DEMO first) and on AVACONT_SURSA, the template for new
-- units, BEFORE the server files of this slice reach the VPS. Safe to run twice (IF NOT EXISTS).
-- Until it is applied the tree route and the ingest keep working without the date: the new
-- features see every angajament as «never updated» and the on-connect update stays off.
--
-- Shape read from MariaDB_Schema/000_DEMO.sql (dump of 22.09.2026). Every INSERT into
-- FX_Angajamente names its columns (checked in PYTHON/routes), so a new nullable column breaks
-- none of them.
-- =====================================================================================

ALTER TABLE `FX_Angajamente`
  ADD COLUMN IF NOT EXISTS `DataActualizare` datetime NULL DEFAULT NULL
  COMMENT 'Slice 0100: when the last FOREXE download of this angajament was saved (NULL = none since the column exists)';

-- The on-connect update and the selection window sort / filter on it.
ALTER TABLE `FX_Angajamente`
  ADD INDEX IF NOT EXISTS `ix_FX_Angajamente_DataActualizare` (`DataActualizare`);
