-- SLICE-VC03-05: read-only checks BEFORE running VC_AVACONT_SURSA.sql. Changes nothing.
-- Every line must show ok = 1. Run it on AVACONT_SURSA (USE AVACONT_SURSA;). Not run from this chat.

SELECT 'MariaDB cu CHECK (>= 10.2.1)' AS verificare,
       (SUBSTRING_INDEX(VERSION(), '.', 1) + 0 > 10
        OR (SUBSTRING_INDEX(VERSION(), '.', 1) + 0 = 10 AND SUBSTRING_INDEX(SUBSTRING_INDEX(VERSION(), '.', 2), '.', -1) + 0 >= 3)) AS ok;

SELECT 'Parteneri exista (furnizorii sunt Parteneri KBOT)' AS verificare,
       EXISTS (SELECT 1 FROM information_schema.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Parteneri') AS ok;

SELECT 'Parteneri.IdPartener este INT(11) cu semn' AS verificare,
       EXISTS (SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Parteneri'
               AND COLUMN_NAME = 'IdPartener' AND COLUMN_TYPE = 'int(11)') AS ok;

SELECT 'Baza foloseste utf8mb3_general_ci' AS verificare,
       (SELECT DEFAULT_COLLATION_NAME FROM information_schema.SCHEMATA WHERE SCHEMA_NAME = DATABASE()) = 'utf8mb3_general_ci' AS ok;

SELECT 'Motorul implicit este InnoDB' AS verificare, @@default_storage_engine = 'InnoDB' AS ok;

-- Informational: VC_ tables that already exist (the script uses IF NOT EXISTS, so they are left as they are).
SELECT COUNT(*) AS tabele_VC_existente FROM information_schema.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME LIKE 'VC\_%';

-- Only for VC_Reguli_DC.sql (triggers), on a DC database:
SELECT 'Triggerele nu cer SUPER (log_bin_trust_function_creators sau log_bin oprit)' AS verificare,
       (@@log_bin = 0 OR @@log_bin_trust_function_creators = 1) AS ok;
