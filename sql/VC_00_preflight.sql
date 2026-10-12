-- SLICE-VC03-05: read-only checks BEFORE running VC_AVACONT_SURSA.sql. Changes nothing.
-- Run it on AVACONT_SURSA (USE AVACONT_SURSA;). ONE result set: every row must show ok = 1,
-- except 'tabele VC_ existente' which is informational (the script uses IF NOT EXISTS, so they are left as they are).
-- The last row matters only for VC_Reguli_DC.sql (triggers) on a DC database. Not run from this chat.

SELECT 'MariaDB cu CHECK (>= 10.2.1)' AS verificare,
       (SUBSTRING_INDEX(VERSION(), '.', 1) + 0 > 10
        OR (SUBSTRING_INDEX(VERSION(), '.', 1) + 0 = 10 AND SUBSTRING_INDEX(SUBSTRING_INDEX(VERSION(), '.', 2), '.', -1) + 0 >= 3)) AS ok
UNION ALL
SELECT 'Parteneri exista (furnizorii sunt Parteneri KBOT)',
       EXISTS (SELECT 1 FROM information_schema.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Parteneri')
UNION ALL
SELECT 'Parteneri.IdPartener este INT(11) cu semn',
       EXISTS (SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Parteneri'
               AND COLUMN_NAME = 'IdPartener' AND COLUMN_TYPE = 'int(11)')
UNION ALL
SELECT 'Baza foloseste utf8mb3_general_ci',
       (SELECT DEFAULT_COLLATION_NAME FROM information_schema.SCHEMATA WHERE SCHEMA_NAME = DATABASE()) = 'utf8mb3_general_ci'
UNION ALL
SELECT 'Motorul implicit este InnoDB', @@default_storage_engine = 'InnoDB'
UNION ALL
SELECT 'Triggerele nu cer SUPER (log_bin_trust_function_creators sau log_bin oprit)',
       (@@log_bin = 0 OR @@log_bin_trust_function_creators = 1)
UNION ALL
SELECT 'INFO: tabele VC_ existente (nu e verificare)',
       (SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME LIKE 'VC\_%');
