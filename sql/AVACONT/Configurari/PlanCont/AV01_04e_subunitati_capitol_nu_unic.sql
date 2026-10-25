-- =====================================================================================
-- Slice AV01-05 -- RUN THIS ON `AVACONT_SURSA` ONLY (Navicat / mysql, as root), then «Sincronizare schema» on the unit database.
--
-- WHAT THIS CHANGES in TZ_Subunitati: the budget chapter (Capitol) is NO LONGER unique IN THE DATABASE. A subunit and its children
-- (IdSubunitateParinte: they read the classifications of the parent) share the parent's chapter, and the migrator may repeat it
-- across years / demo databases. The rule "a chapter repeats only inside one family" is kept by the server's save route
-- (PYTHON/routes/avacont/configurari/plan_cont/capitol_familie.py, one switch there), not by an index. The denomination stays
-- unique, the Access unit link (IdUnitateAccess) stays unique. A plain index on Capitol replaces the unique one (lists are ordered by it).
--
-- NOTHING in the data changes. Nothing else points at the uniqueness of Capitol: accounts and balances reference IdSubunitate.
-- Safe to run twice (IF EXISTS / IF NOT EXISTS). One statement, so MariaDB swaps the two indexes together.
-- =====================================================================================

ALTER TABLE `AVACONT_SURSA`.`TZ_Subunitati`
  DROP INDEX IF EXISTS `UK_TZ_Subunitati_Capitol`,
  ADD INDEX IF NOT EXISTS `IX_TZ_Subunitati_Capitol`(`Capitol` ASC) USING BTREE;

-- Check (read only):
-- SHOW CREATE TABLE `AVACONT_SURSA`.`TZ_Subunitati`;   -- no UNIQUE on Capitol, IX_TZ_Subunitati_Capitol present
-- Then: AvacontPush, tab «Sincronizare schema», on every unit database (000_DEMO ...). Check there that the synchronisation also
-- DROPS the old unique index from a database that already has it; if it only adds, run the same ALTER by hand on each unit database.
