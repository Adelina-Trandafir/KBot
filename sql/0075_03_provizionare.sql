-- =====================================================================
--  Slice 0075-03 — what the provisioning job needs on the K-BOT server.
--  Plan: docs/PLAN_AutoProvisioning.md §5.6. Run by the operator, in order.
--  K-BOT server only (89.33.25.34). Nothing on the legacy server (D8).
--
--  Three parts:
--    1. two columns on FX_Inregistrari for the one-time password link
--    2. the leftover work columns of 0075-00 off the template (plan §0.0)
--    3. the provisioning account (D18) — EDIT <PAROLA> first
-- =====================================================================


-- ---------------------------------------------------------------------
-- 1. The password link (§5.6 step 8, D13).
--    Only the SHA-256 of the link's token is stored, never the token.
--    Both NULL = no live link (never sent, used, or the run failed).
-- ---------------------------------------------------------------------
ALTER TABLE `AVACONT_COMUN`.`FX_Inregistrari`
  ADD COLUMN `ParolaHash`   CHAR(64) NULL DEFAULT NULL AFTER `IpAddress`,
  ADD COLUMN `ParolaExpira` DATETIME NULL DEFAULT NULL AFTER `ParolaHash`,
  ADD KEY `ix_FX_Inregistrari_ParolaHash` (`ParolaHash`);


-- ---------------------------------------------------------------------
-- 2. The template must not carry 0075-00's work columns: every unit
--    created from it would inherit three dead columns. The job REFUSES
--    to run while they are there.
-- ---------------------------------------------------------------------
ALTER TABLE `AVACONT_SURSA`.`Clasificatii`
  DROP COLUMN `Sector_w`, DROP COLUMN `Sursa_w`, DROP COLUMN `SS_w`;


-- ---------------------------------------------------------------------
-- 3. The provisioning account. Never the service account AVACONT (D18).
--
--    Host 'localhost': Flask and MariaDB run on the SAME machine (operator,
--    23.09.2026), so the account accepts local connections only — this
--    account can create users, and nothing outside the VPS should reach it.
--    config.py connects through the UNIX SOCKET ('unix_socket'), the one way
--    that always matches 'localhost'. A TCP connection to 127.0.0.1 matches it
--    only while skip_name_resolve is OFF, and one to the public IP never does.
--
--    EDIT before running:
--      <PAROLA> a long random password. The same value goes into config.py.
--
--    Unit databases are matched by the pattern `1__\_____` — `1nn_SSSS`,
--    the only shape the job builds (`\_` is a literal underscore, `_` any
--    one character). It also matches the old `101_CCDP`; the job only ever
--    drops a database it created in the same run.
-- ---------------------------------------------------------------------
CREATE USER 'kbot_provizionare'@'localhost' IDENTIFIED BY '<PAROLA>';

-- Create the MariaDB account of a new user, set its password (ALTER USER),
-- drop it again when a run unwinds. SHOW DATABASES: the free-name check reads
-- information_schema.SCHEMATA, which otherwise lists only databases this
-- account has rights on.
GRANT CREATE USER, SHOW DATABASES ON *.* TO 'kbot_provizionare'@'localhost';

-- Is this e-mail already an account?
GRANT SELECT ON `mysql`.`user` TO 'kbot_provizionare'@'localhost';

-- Build, fill and (on failure) drop a unit database. NOT hand its user rights:
-- WITH GRANT OPTION on a pattern cannot do that (1044 on the first real run,
-- 23.09.2026) -- the GRANT goes through a root-owned procedure instead, see
-- sql/0075_03_02_grant_procedura.sql, which must be run after this file.
GRANT CREATE, DROP, CREATE VIEW, SHOW VIEW, INDEX, REFERENCES,
      SELECT, INSERT, UPDATE, DELETE, EXECUTE
   ON `1__\_____`.* TO 'kbot_provizionare'@'localhost';

-- Read the template (SHOW CREATE TABLE / VIEW).
GRANT SELECT, SHOW VIEW ON `AVACONT_SURSA`.* TO 'kbot_provizionare'@'localhost';

-- The registry: write, and delete again when a run unwinds.
GRANT SELECT, INSERT, DELETE ON `AVACONT_COMUN`.`CAI`                 TO 'kbot_provizionare'@'localhost';
GRANT SELECT, INSERT, DELETE ON `AVACONT_COMUN`.`Unitati`             TO 'kbot_provizionare'@'localhost';
GRANT SELECT, INSERT, DELETE ON `AVACONT_COMUN`.`Unitati_Ani`         TO 'kbot_provizionare'@'localhost';
GRANT SELECT, INSERT, DELETE ON `AVACONT_COMUN`.`Unitati_Utilizatori` TO 'kbot_provizionare'@'localhost';
GRANT SELECT, UPDATE         ON `AVACONT_COMUN`.`FX_Inregistrari`     TO 'kbot_provizionare'@'localhost';

-- The dictionaries the rows are checked against, and that the new
-- Clasificatii table's foreign keys point at.
GRANT SELECT, REFERENCES ON `AVACONT_COMUN`.`DefaClsfF`       TO 'kbot_provizionare'@'localhost';
GRANT SELECT, REFERENCES ON `AVACONT_COMUN`.`DefaClsfE`       TO 'kbot_provizionare'@'localhost';
GRANT SELECT, REFERENCES ON `AVACONT_COMUN`.`DefaArticol`     TO 'kbot_provizionare'@'localhost';
GRANT SELECT, REFERENCES ON `AVACONT_COMUN`.`DefaTitlu`       TO 'kbot_provizionare'@'localhost';
GRANT SELECT, REFERENCES ON `AVACONT_COMUN`.`DefaSursaSector` TO 'kbot_provizionare'@'localhost';

-- Check what it ended up with:
-- SHOW GRANTS FOR 'kbot_provizionare'@'localhost';
