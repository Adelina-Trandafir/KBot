-- =====================================================================================
-- ONE-TIME QUERY  0102_buget_data_inceput        (slice 0102, run with slice 0103's tool)
--
-- NOT run by hand. Paste this text into AvacontPush, tab «Interogări unice», name it
-- `0102_buget_data_inceput`, press «Vezi (nu execută)» and then «Execută». The server runs it
-- once on AVACONT_SURSA and once on every unit database, each in its own database (so the
-- table names below are NOT qualified), and writes it in each database's `Interogari_Unice`.
-- A database that has already run it (same text) is skipped; a new unit is born with it marked.
--
-- BEFORE it: `0102_01_sursa.sql` on AVACONT_SURSA, and a schema sync on the units (they must have
-- `Clasificatii_Buget.DataInceput` already; without it the first statement fails on that unit,
-- nothing is marked, and the query can be run again afterwards).
--
-- WHAT IT DOES, on each database:
--   1. the budget rows that got no real start date from the schema sync (the new column comes
--      empty = 0000-00-00) start on 01.04.2026 (operator, 02.10.2026); a row of another year, if
--      any, starts on 01.01 of its year;
--   2. drops the old unique key (IdClsf, An) where it is still there -- a SAFE schema sync adds
--      the new key but never drops the old one, and the old one would forbid a second version
--      of the same year.
-- Both statements can be run again without harm.
-- =====================================================================================

UPDATE `Clasificatii_Buget`
   SET `DataInceput` = IF(`An` IS NULL OR `An` = 2026, '2026-04-01', MAKEDATE(`An`, 1))
 WHERE `DataInceput` < '2000-01-01';

ALTER TABLE `Clasificatii_Buget`
  DROP INDEX IF EXISTS `uq_clasificatii_buget_idclsf_an`;
