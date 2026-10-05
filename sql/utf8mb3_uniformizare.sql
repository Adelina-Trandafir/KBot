-- =====================================================================================
-- Brings EVERY base table of EVERY database to utf8mb3 / utf8mb3_general_ci (house rule).
-- It changes NOTHING by itself: each step is a SELECT that prints what to look at or the
-- statements to run. Run it as a user that can read information_schema and ALTER the schemas.
--
--   PASO 0  BACKUP FIRST (mysqldump of every schema). The conversion drops characters that need
--           4 bytes (emoji and a few rare symbols): they become '?'. It cannot be undone.
--   STEP 1  what is NOT utf8mb3_general_ci today (a list, to read).
--   STEP 2  for each such table, the query that counts values that would LOSE characters.
--           Run the printed queries; every count must be 0 (else look at those rows first).
--   STEP 3  the ALTER statements. Copy the printed column into a new query tab and run it.
--
-- Skipped: system schemas, views (they follow their tables), stored routines/triggers/events
-- (they keep the charset they were created with: check them apart if one compares text).
-- Foreign keys: the statements of step 3 are wrapped in FOREIGN_KEY_CHECKS = 0 because the two
-- ends of a key must change together; check afterwards that no key was lost (step 1 again must
-- print nothing, and SHOW CREATE TABLE of a parent/child pair still has its CONSTRAINT).
-- A big table (Jurnal) is copied by ALTER and locked while it runs: run off hours.
-- Names: 'utf8_general_ci' is the old spelling of utf8mb3_general_ci and counts as correct.
-- =====================================================================================

-- ---------- STEP 1: what is off ---------------------------------------------------
SELECT t.TABLE_SCHEMA, t.TABLE_NAME, t.TABLE_COLLATION AS table_collation,
       (SELECT GROUP_CONCAT(CONCAT(c.COLUMN_NAME, ' ', c.COLLATION_NAME) SEPARATOR ', ')
          FROM information_schema.COLUMNS c
         WHERE c.TABLE_SCHEMA = t.TABLE_SCHEMA AND c.TABLE_NAME = t.TABLE_NAME
           AND c.COLLATION_NAME IS NOT NULL
           AND c.COLLATION_NAME NOT IN ('utf8mb3_general_ci', 'utf8_general_ci')) AS columns_off
  FROM information_schema.TABLES t
 WHERE t.TABLE_TYPE = 'BASE TABLE'
   AND t.TABLE_SCHEMA NOT IN ('information_schema', 'mysql', 'performance_schema', 'sys')
   AND ( t.TABLE_COLLATION NOT IN ('utf8mb3_general_ci', 'utf8_general_ci')
         OR EXISTS (SELECT 1 FROM information_schema.COLUMNS c
                     WHERE c.TABLE_SCHEMA = t.TABLE_SCHEMA AND c.TABLE_NAME = t.TABLE_NAME
                       AND c.COLLATION_NAME IS NOT NULL
                       AND c.COLLATION_NAME NOT IN ('utf8mb3_general_ci', 'utf8_general_ci')) )
 ORDER BY t.TABLE_SCHEMA, t.TABLE_NAME;

-- ---------- STEP 2: would any value lose characters? (one query per text column) ----
-- Counts values whose bytes change when forced to utf8mb3. BINARY compares bytes, so the
-- accent-insensitive collation cannot hide a difference.
SELECT CONCAT('SELECT ''', c.TABLE_SCHEMA, '.', c.TABLE_NAME, '.', c.COLUMN_NAME, ''' AS col, COUNT(*) AS lose ',
              'FROM `', c.TABLE_SCHEMA, '`.`', c.TABLE_NAME, '` ',
              'WHERE BINARY `', c.COLUMN_NAME, '` <> BINARY CONVERT(`', c.COLUMN_NAME, '` USING utf8mb3);') AS check_query
  FROM information_schema.COLUMNS c
  JOIN information_schema.TABLES t
    ON t.TABLE_SCHEMA = c.TABLE_SCHEMA AND t.TABLE_NAME = c.TABLE_NAME AND t.TABLE_TYPE = 'BASE TABLE'
 WHERE c.TABLE_SCHEMA NOT IN ('information_schema', 'mysql', 'performance_schema', 'sys')
   AND c.CHARACTER_SET_NAME IN ('utf8mb4', 'utf16', 'utf32', 'ucs2')
 ORDER BY c.TABLE_SCHEMA, c.TABLE_NAME, c.ORDINAL_POSITION;

-- ---------- STEP 3: the statements (run them only after steps 1 and 2 were read) -------
SELECT 'SET FOREIGN_KEY_CHECKS = 0;' AS statement_to_run
UNION ALL
SELECT CONCAT('ALTER TABLE `', t.TABLE_SCHEMA, '`.`', t.TABLE_NAME,
              '` CONVERT TO CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci;')
  FROM information_schema.TABLES t
 WHERE t.TABLE_TYPE = 'BASE TABLE'
   AND t.TABLE_SCHEMA NOT IN ('information_schema', 'mysql', 'performance_schema', 'sys')
   AND ( t.TABLE_COLLATION NOT IN ('utf8mb3_general_ci', 'utf8_general_ci')
         OR EXISTS (SELECT 1 FROM information_schema.COLUMNS c
                     WHERE c.TABLE_SCHEMA = t.TABLE_SCHEMA AND c.TABLE_NAME = t.TABLE_NAME
                       AND c.COLLATION_NAME IS NOT NULL
                       AND c.COLLATION_NAME NOT IN ('utf8mb3_general_ci', 'utf8_general_ci')) )
UNION ALL
-- The default of each database, so a table created later starts right.
SELECT CONCAT('ALTER DATABASE `', s.SCHEMA_NAME, '` CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci;')
  FROM information_schema.SCHEMATA s
 WHERE s.SCHEMA_NAME NOT IN ('information_schema', 'mysql', 'performance_schema', 'sys')
   AND s.DEFAULT_COLLATION_NAME NOT IN ('utf8mb3_general_ci', 'utf8_general_ci')
UNION ALL
SELECT 'SET FOREIGN_KEY_CHECKS = 1;';
