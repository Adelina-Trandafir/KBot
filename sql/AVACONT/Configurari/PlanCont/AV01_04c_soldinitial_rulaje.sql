-- =====================================================================================
-- AV01-04c  TZ_SoldInitial: the cumulated turnovers (RulajDebitor / RulajCreditor)
-- Run ONCE, on AVACONT_SURSA only, then AvacontPush > «Sincronizare schema» (SAFE) on every unit database.
-- =====================================================================================
-- The «Plan de conturi» window shows and edits the cumulated turnovers (Access RPD / RPC), so the columns must exist in MariaDB.
-- AVC.Migrator does NOT migrate their values (operator, 25.10.2026): it only checks that the columns are there.
-- The two columns were added to AV01_04_conturi_sursa.sql on 24.10.2026, after some databases had already run it;
-- this script is that same ALTER on its own. Safe to run twice (IF NOT EXISTS). Changes no data.
-- =====================================================================================

ALTER TABLE `AVACONT_SURSA`.`TZ_SoldInitial`
  ADD COLUMN IF NOT EXISTS `RulajDebitor`  decimal(18, 2) NOT NULL DEFAULT 0.00 COMMENT 'Cumulated turnover before the opening (Access RPD)' AFTER `SoldCreditor`,
  ADD COLUMN IF NOT EXISTS `RulajCreditor` decimal(18, 2) NOT NULL DEFAULT 0.00 COMMENT 'Cumulated turnover before the opening (Access RPC)' AFTER `RulajDebitor`;

-- Check (read only):
-- SHOW CREATE TABLE `AVACONT_SURSA`.`TZ_SoldInitial`;
