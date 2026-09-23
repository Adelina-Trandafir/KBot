-- =====================================================================
--  Slice 0076 -- narrow the service account AVACONT on the K-BOT server.
--  K-BOT server only (89.33.25.34). Nothing on the legacy server.
--  Run as root on the VPS (`mysql`, unix socket), part by part.
--
--  BEFORE: GRANT <almost everything> ON *.* TO AVACONT@'%' WITH GRANT OPTION
--          -- every database on the box (ADCREDIT3 too), SUPER, SHUTDOWN,
--          FILE, CREATE USER, and the right to hand all of it on.
--  AFTER:  data + table structure in the K-BOT databases only, plus the
--          three reads the code needs outside them. Host stays '%'
--          (decision 23.09.2026: not sure nothing else logs in as AVACONT).
--
--  What the rights are FOR (so nobody trims one blindly later):
--    SELECT/INSERT/UPDATE/DELETE  every route (routes/forexe, auth, inregistrare)
--    CREATE/ALTER/DROP/INDEX/REFERENCES/CREATE VIEW
--                                 schema_sync (schema_execute.py) and
--                                 scripts/clasificatii_sursa.py, which change
--                                 table structure as AVACONT
--    SHOW VIEW/TRIGGER/EVENT/LOCK TABLES
--                                 the mysqldump those two take first
--                                 (--routines --triggers --events)
--    SHOW DATABASES (global)      nume.used_prefixes: the free unit number must
--                                 see EVERY database, not only AVACONT's
--    SELECT mysql.user            inregistrare.py: "does this e-mail already
--                                 have an account"
--    SELECT mysql.proc            mysqldump --routines on MariaDB 10.11
--  AVACONT_COMUN gets no DROP: nothing drops a table there, and DROP on
--  db.* would also allow DROP DATABASE of the login store.
-- =====================================================================


-- ---------------------------------------------------------------------
-- 0. LOOK FIRST (read-only). Stop and ask if anything is unexpected.
-- ---------------------------------------------------------------------
-- 0a. The databases. Every one K-BOT uses must match a line of part 1:
--     AVACONT_COMUN, AVACONT_SURSA, 000_DEMO, 1nn_SSSS.
SHOW DATABASES;

-- 0b. Anything else granted to AVACONT (expected: one line, ON *.*).
SHOW GRANTS FOR 'AVACONT'@'%';

-- 0c. Objects that RUN AS AVACONT (views, procedures, triggers, events).
--     Fine inside the four database groups; a row in any OTHER database
--     stops working after part 2.
SELECT 'VIEW' AS tip, TABLE_SCHEMA AS db, TABLE_NAME AS nume FROM information_schema.VIEWS    WHERE DEFINER LIKE 'AVACONT@%'
UNION ALL
SELECT ROUTINE_TYPE, ROUTINE_SCHEMA, ROUTINE_NAME                FROM information_schema.ROUTINES WHERE DEFINER LIKE 'AVACONT@%'
UNION ALL
SELECT 'TRIGGER', TRIGGER_SCHEMA, TRIGGER_NAME                   FROM information_schema.TRIGGERS WHERE DEFINER LIKE 'AVACONT@%'
UNION ALL
SELECT 'EVENT', EVENT_SCHEMA, EVENT_NAME                         FROM information_schema.EVENTS   WHERE DEFINER LIKE 'AVACONT@%';


-- ---------------------------------------------------------------------
-- 1. ADD the narrow rights first. Nothing breaks: the global ones are
--    still there. `\_` = a literal underscore; `_` = any one character.
-- ---------------------------------------------------------------------
GRANT SELECT, INSERT, UPDATE, DELETE, CREATE, ALTER, DROP, INDEX, REFERENCES,
      CREATE VIEW, SHOW VIEW, TRIGGER, EVENT, LOCK TABLES
   ON `AVACONT\_SURSA`.* TO 'AVACONT'@'%';

GRANT SELECT, INSERT, UPDATE, DELETE, CREATE, ALTER, DROP, INDEX, REFERENCES,
      CREATE VIEW, SHOW VIEW, TRIGGER, EVENT, LOCK TABLES
   ON `000\_DEMO`.* TO 'AVACONT'@'%';

-- Every unit database: 1nn_SSSS (routes/inregistrare/nume.py).
GRANT SELECT, INSERT, UPDATE, DELETE, CREATE, ALTER, DROP, INDEX, REFERENCES,
      CREATE VIEW, SHOW VIEW, TRIGGER, EVENT, LOCK TABLES
   ON `1__\_____`.* TO 'AVACONT'@'%';

GRANT SELECT, INSERT, UPDATE, DELETE, CREATE, ALTER, INDEX, REFERENCES,
      SHOW VIEW, LOCK TABLES
   ON `AVACONT\_COMUN`.* TO 'AVACONT'@'%';

GRANT SELECT ON `mysql`.`user` TO 'AVACONT'@'%';
GRANT SELECT ON `mysql`.`proc` TO 'AVACONT'@'%';


-- ---------------------------------------------------------------------
-- 2. TAKE AWAY the global rights. ON *.* only: the lines of part 1 stay.
--    (A bare `REVOKE ALL PRIVILEGES, GRANT OPTION FROM ...` would wipe
--    part 1 too -- do not "simplify" to it.)
-- ---------------------------------------------------------------------
REVOKE ALL PRIVILEGES ON *.* FROM 'AVACONT'@'%';
REVOKE GRANT OPTION   ON *.* FROM 'AVACONT'@'%';
GRANT SHOW DATABASES  ON *.* TO   'AVACONT'@'%';

SHOW GRANTS FOR 'AVACONT'@'%';


-- ---------------------------------------------------------------------
-- 3. UNDO (only if something broke): the exact list it had before.
-- ---------------------------------------------------------------------
-- GRANT SELECT, INSERT, UPDATE, DELETE, CREATE, DROP, RELOAD, SHUTDOWN, PROCESS,
--       FILE, REFERENCES, INDEX, ALTER, SHOW DATABASES, SUPER,
--       CREATE TEMPORARY TABLES, LOCK TABLES, EXECUTE, REPLICATION SLAVE,
--       BINLOG MONITOR, CREATE VIEW, SHOW VIEW, CREATE ROUTINE, ALTER ROUTINE,
--       CREATE USER, EVENT, TRIGGER
--    ON *.* TO 'AVACONT'@'%' WITH GRANT OPTION;
