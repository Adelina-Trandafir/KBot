-- =====================================================================================
-- Slice 0051 -- the number lock (FX_NumberLock).
--
-- WARNING -- APPLY TO EVERY UNIT DATABASE, plus AVACONT_SURSA.
--   Same rule as sql/0051_ddf_rev_att_img.sql: one MariaDB database = one unit, so this file
--   runs once for EACH unit database. It also runs on AVACONT_SURSA so databases created
--   from now on carry the table from birth and schema_sync does not report a difference
--   forever. The AVACONT_SURSA variant is at the foot of this file.
--
-- =====================================================================================
-- WHY THIS TABLE EXISTS
-- =====================================================================================
-- `FX_DDF.CUAL` and `FX_DDF_REV.NumarRev` are allocated by the operator, not by the
-- database: Access computed them with `Nz(DMax(...), -1) + 1` at the moment the editor
-- opened, and nothing stopped a second operator computing the same number a second later.
-- The two saves then raced, and the loser found out only at INSERT time -- or, worse, did
-- not find out at all, because neither column carries a unique constraint.
--
-- The DDF editor is modal and an operator can sit in section A for a long time, so a number
-- shown in the header has to be genuinely HELD, not merely guessed. This table is that hold:
-- the editor takes a row when it opens, renews it while it is open, and the save transaction
-- verifies, consumes and deletes it. A crash leaves the row behind; the next allocation
-- sweeps everything with `ExpiraLa < NOW()`.
--
-- THIS IS DELIBERATELY DIFFERENT FROM SLICE 0049. `NrORD` is allocated inside the save
-- transaction and the editor only shows a guess ("probabil N"). `CUAL` / `NumarRev` are
-- held for real. Do not "harmonise" the two: the ORD editor can guess because a wrong guess
-- costs nothing, and the DDF editor cannot because the number is on screen and the operator
-- may retype it.
--
-- =====================================================================================
-- ABOUT THE `DC` COLUMN
-- =====================================================================================
-- `DC` is part of the unique key even though one database is one `DC`. That is the
-- operator's explicit instruction, and it keeps the lock key IDENTICAL to the `DMax`
-- predicates the allocation ports from Access:
--
--   CUAL      : MAX(CUAL)     over FX_DDF     WHERE DC = ?
--   NumarRev  : MAX(NumarRev) over FX_DDF_REV WHERE CodAngajament = ? AND DC = ?
--
-- Note that Access uses `Nz(DMax(...), -1) + 1` for NumarRev, so THE INITIAL REVISION IS
-- NUMBER 0, not 1. That is kept.
--
-- The per-year reset stays deferred (docs/possible_future_directions.md). This table does
-- not foreclose it: a year column can be added to the unique key later without touching the
-- rows already written, because a lock row never outlives its TTL.
--
-- =====================================================================================
-- !!! UNVERIFIED !!!
-- =====================================================================================
-- Nothing here has been run against a live MariaDB; neither the database nor the Python
-- server is reachable from the development machine. The column types follow the tables the
-- lock mirrors, read from MariaDB_Schema/000_DEMO.sql:
--   `Valoare`       <- FX_DDF.CUAL / FX_DDF_REV.NumarRev, both int(11)   (lines 143, 193)
--   `DC`            <- FX_DDF.DC / FX_DDF_REV.DC, both varchar(50)       (lines 146, 202)
--   `CodAngajament` <- FX_DDF_REV.CodAngajament, varchar(255)            (line 191)
--
-- No lock has ever been contended for by two real sessions. The TTL, the renewal interval
-- and the sweep are untested in anger.
-- =====================================================================================

CREATE TABLE `FX_NumberLock` (
  `IdLock`        int(11)      NOT NULL AUTO_INCREMENT,
  `Tip`           varchar(16)  NOT NULL,   -- 'CUAL' | 'NUMARREV'
  `DC`            varchar(50)  NOT NULL,
  `Valoare`       int(11)      NOT NULL,
  `CodAngajament` varchar(255)     NULL,   -- NULL for 'CUAL'; set for 'NUMARREV'
  `Token`         varchar(64)  NOT NULL,   -- the session token that holds the lock
  `Utilizator`    varchar(255)     NULL,   -- for the "held by another operator" message
  `CreatLa`       datetime     NOT NULL DEFAULT current_timestamp(),
  `ExpiraLa`      datetime     NOT NULL,
  PRIMARY KEY (`IdLock`) USING BTREE,
  UNIQUE INDEX `UQ_FX_NumberLock`(`Tip`, `DC`, `Valoare`) USING BTREE,
  INDEX `ix_FX_NumberLock_Token`(`Token`) USING BTREE,
  INDEX `ix_FX_NumberLock_ExpiraLa`(`ExpiraLa`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = Dynamic;

-- -------------------------------------------------------------------------------------
-- NOTE ON THE UNIQUE KEY.
--
-- `(Tip, DC, Valoare)` deliberately does NOT include `CodAngajament`, even though NumarRev
-- is per-angajament. That is not an oversight -- it is what makes the key work for BOTH
-- kinds of lock with one index, and the allocation query already carries the
-- `CodAngajament` predicate itself. The cost is that two different angajamente cannot hold
-- the same NumarRev at the same moment; the benefit is that a duplicate can never be
-- inserted for either kind. If that cost ever bites, `CodAngajament` joins the key with
-- `COALESCE(CodAngajament, '')` -- but measure first, because revision numbers are small
-- integers and simultaneous editing of two angajamente on one unit is rare.
-- -------------------------------------------------------------------------------------

-- =====================================================================================
-- VARIANT FOR AVACONT_SURSA -- run THIS ONE on it, INSTEAD of the one above.
--
-- The only difference: `IdLock` is `int(11) NOT NULL`, WITHOUT `AUTO_INCREMENT`, per the
-- exemption from slice 0048-01 section 3.1 -- on the source schema, AUTO_INCREMENT would
-- make the migration silently fabricate keys for rows that arrive without them.
--
-- The table will always be EMPTY on AVACONT_SURSA: it holds transient locks, and nothing
-- migrates into it. It exists there only so schema_sync stops reporting a difference.
-- =====================================================================================
--
-- CREATE TABLE `FX_NumberLock` (
--   `IdLock`        int(11)      NOT NULL,
--   `Tip`           varchar(16)  NOT NULL,
--   `DC`            varchar(50)  NOT NULL,
--   `Valoare`       int(11)      NOT NULL,
--   `CodAngajament` varchar(255)     NULL,
--   `Token`         varchar(64)  NOT NULL,
--   `Utilizator`    varchar(255)     NULL,
--   `CreatLa`       datetime     NOT NULL DEFAULT current_timestamp(),
--   `ExpiraLa`      datetime     NOT NULL,
--   PRIMARY KEY (`IdLock`) USING BTREE,
--   UNIQUE INDEX `UQ_FX_NumberLock`(`Tip`, `DC`, `Valoare`) USING BTREE,
--   INDEX `ix_FX_NumberLock_Token`(`Token`) USING BTREE,
--   INDEX `ix_FX_NumberLock_ExpiraLa`(`ExpiraLa`) USING BTREE
-- ) ENGINE = InnoDB CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = Dynamic;
