-- SLICE-ADE9-01: AD_Permissions (invented, empty everywhere) is gone; rights live in AVACONT_COMUN.
-- Run this query, then run the DROP statements it prints (one per unit database that has the table).
SELECT CONCAT('DROP TABLE IF EXISTS `', table_schema, '`.`AD_Permissions`;') AS stmt
FROM information_schema.tables WHERE table_name = 'AD_Permissions';
-- Check first that they really are empty:
--   SELECT table_schema, table_rows FROM information_schema.tables WHERE table_name = 'AD_Permissions';
