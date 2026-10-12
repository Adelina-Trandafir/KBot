-- SLICE-VC04-03: MariaDB privileges for VERCON. TEMPLATE -- replace <DC> (the DC database name) and run it by hand as
-- the administrator, one DC at a time. Not run from this chat. No password is written here: set it when you create
-- the account (CREATE USER ... IDENTIFIED BY '<set by the administrator>') and keep it out of chat, logs and git.
--
-- Decisions (PLAN runda 3, rule 12): rights are ROLES ON THE DC (section VR in AVACONT_COMUN). There is NO local ACL per
-- subunit or gestiune: VC_SubunitAccess / VC_GestiuneAccess are not created. An absent ACL therefore means exactly this:
-- every user with the right VR role in the DC may enter every subunit and gestiune of that DC; what keeps two people
-- apart is the exclusive access (VC_ContextLock), not a list.
--
-- Four separate privilege levels:
--   1. Business API   = the application's existing service account (Flask). It already works on the DC databases; it
--                       needs NO new account. It reads/writes VC_* and reads Parteneri. It never does DDL.
--   2. Report reading = the same service account, read paths only. A separate SELECT-only account is possible
--                       (template B) but not required by this application.
--   3. Migrator       = a dedicated account for VRC.Migrator (template A): writes ONLY the VC_ tables of the target DC,
--                       reads Parteneri, reads (never writes) the unit identity in AVACONT_COMUN. No DELETE, no DDL, no
--                       access to other modules' tables (AD_*, Forexe, ...), no GRANT OPTION.
--   4. DDL            = schema sync from KBotAdmin (existing mechanism). VERCON adds no DDL account.
--
-- A rights pattern on database names cannot be passed on (MariaDB 1044): grant on the exact database, with the
-- underscore escaped, as the administrator (or through the existing root-owned grant procedure).

-- ===== A. migrator (one DC at a time) ===================================================================================
-- CREATE USER 'vrc_migrator'@'localhost' IDENTIFIED BY '<set by the administrator>';   -- only once
GRANT SELECT, INSERT, UPDATE ON `<DC>`.`VC\_Subunitati`        TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT, UPDATE ON `<DC>`.`VC\_Gestiuni`          TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT, UPDATE ON `<DC>`.`VC\_Exercitii`         TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT, DELETE ON `<DC>`.`VC\_ContextLock`       TO 'vrc_migrator'@'localhost';  -- reserves/releases every gestiune of the subunit
GRANT SELECT, INSERT         ON `<DC>`.`VC\_Imports`           TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT         ON `<DC>`.`VC\_ImportSources`     TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT         ON `<DC>`.`VC\_IdMap`             TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT, UPDATE ON `<DC>`.`VC\_MigrationIssues`   TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT         ON `<DC>`.`VC\_Audit`             TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT, UPDATE ON `<DC>`.`VC\_Settings`          TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT, UPDATE ON `<DC>`.`VC\_Numbering`         TO 'vrc_migrator'@'localhost';
-- the business tables (generated list: PYTHON/routes/vercon/tables.py); insert-only for the one-time transfer, plus
-- UPDATE only where the transfer fixes a row it just wrote (Populated / State are on VC_Gestiuni above)
GRANT SELECT, INSERT ON `<DC>`.`VC\_Masini`          TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT ON `<DC>`.`VC\_Produse`         TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT ON `<DC>`.`VC\_ProduseP`        TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT ON `<DC>`.`VC\_ProduseM`        TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT ON `<DC>`.`VC\_Clienti`         TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT ON `<DC>`.`VC\_ComisieNIR`      TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT ON `<DC>`.`VC\_ComisieInventar` TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT ON `<DC>`.`VC\_TVA`             TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT ON `<DC>`.`VC\_FeluriMancare`   TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT ON `<DC>`.`VC\_Retetar`         TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT ON `<DC>`.`VC\_OI\_Produse`     TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT ON `<DC>`.`VC\_NIR`             TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT ON `<DC>`.`VC\_Intrari`         TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT ON `<DC>`.`VC\_IntrariStorno`   TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT ON `<DC>`.`VC\_Iesiri\_H`       TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT ON `<DC>`.`VC\_Iesiri`          TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT ON `<DC>`.`VC\_IesiriRetur`     TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT ON `<DC>`.`VC\_OI\_Predare`     TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT ON `<DC>`.`VC\_OI\_Transfer`    TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT ON `<DC>`.`VC\_OI\_Casare`      TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT ON `<DC>`.`VC\_LABC`            TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT ON `<DC>`.`VC\_ListaAlimente`   TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT ON `<DC>`.`VC\_LegBCLA`         TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT ON `<DC>`.`VC\_MicDejun`        TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT ON `<DC>`.`VC\_Dejun`           TO 'vrc_migrator'@'localhost';
GRANT SELECT, INSERT ON `<DC>`.`VC\_Cina`            TO 'vrc_migrator'@'localhost';
-- partners are only READ (matching by CodFiscal); creating a partner is done in KBOT, not by the migrator
GRANT SELECT ON `<DC>`.`Parteneri` TO 'vrc_migrator'@'localhost';
-- unit identity for the cross-check cale/UNIT <-> common data: read only
GRANT SELECT ON `AVACONT_COMUN`.`Unitati`      TO 'vrc_migrator'@'localhost';
GRANT SELECT ON `AVACONT_COMUN`.`Unitati_Date` TO 'vrc_migrator'@'localhost';

-- ===== B. optional SELECT-only reporting account ========================================================================
-- CREATE USER 'vrc_report'@'localhost' IDENTIFIED BY '<set by the administrator>';
-- GRANT SELECT ON `<DC>`.* TO 'vrc_report'@'localhost';   -- per DC; never on AVACONT_COMUN

-- ===== C. what must NOT exist (check after the grants) ===================================================================
--   SHOW GRANTS FOR 'vrc_migrator'@'localhost';
-- must show no DELETE except VC_ContextLock, no ALTER/CREATE/DROP, no GRANT OPTION, nothing on other modules' tables,
-- nothing but SELECT on AVACONT_COMUN.

-- ===== D. revoke when the transfer of the DC is confirmed ================================================================
--   REVOKE ALL PRIVILEGES, GRANT OPTION FROM 'vrc_migrator'@'localhost';   -- then re-grant only for the next DC
