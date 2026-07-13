-- =============================================================================
-- DDL_FX_ListaAngajamente.sql
-- MariaDB DDL for the ListaAngajamente slice (per-unit database, e.g. 045_CTER).
--
-- Mirrors the Access FX tables exported at FX_System_Export/TABLES/*.md:
--   FX_Angajamente  (VERIFY ONLY — already exists in production; kept here IF NOT
--                    EXISTS for a fresh/dev database, never altered destructively)
--   FX_Indicatori   (feeds GET /api/forexe/angajamente: Surse = GROUP_CONCAT(SS),
--                    IdUnitate filter)
--   FX_Istoric, FX_Rezervari, FX_Plati  (DDL only in this slice; data flows deferred)
--
-- Type mapping (Access -> MariaDB):
--   Text N -> VARCHAR(N)   Memo -> LONGTEXT   Long -> INT   Double -> DOUBLE
--   Boolean -> TINYINT(1)  DateTime -> DATETIME
--
-- Charset: utf8mb4 — Stare/Descriere carry Romanian diacritics ("În derulare").
--
-- OPEN ITEM (must be answered before running on a real database):
--   FX_Istoric.ID, FX_Rezervari.IDRZ, FX_Plati.IdPlataFX are Access "Long" columns.
--   It is not yet confirmed whether Access generated them as AutoNumber or whether
--   FOREXE supplies the values on insert. They are declared WITHOUT AUTO_INCREMENT
--   here (FOREXE-supplied assumption). If they turn out to be AutoNumber, add
--   AUTO_INCREMENT to those PRIMARY KEY columns.
-- =============================================================================

-- -----------------------------------------------------------------------------
-- FX_Angajamente  (VERIFY ONLY)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS FX_Angajamente (
    CodAngajament     VARCHAR(50)  NOT NULL,
    IDDF              INT          NULL,
    IdUnitate         INT          NULL,
    DataCreare        DATETIME     NULL,
    DataDefinitivare  DATETIME     NULL,
    Descriere         VARCHAR(255) NULL,
    Stare             VARCHAR(50)  NULL,
    DC                VARCHAR(255) NULL,
    DTQ               DATETIME     NULL,
    Incarcat          TINYINT(1)   NOT NULL DEFAULT 0,
    Preluat           TINYINT(1)   NOT NULL DEFAULT 0,
    Salarii           TINYINT(1)   NOT NULL DEFAULT 0,
    ASCUNS            TINYINT(1)   NOT NULL DEFAULT 0,
    PRIMARY KEY (CodAngajament),
    KEY IdUnitate (IdUnitate)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- -----------------------------------------------------------------------------
-- FX_Indicatori
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS FX_Indicatori (
    CodAI                        VARCHAR(50)  NOT NULL,
    CodAngajament                VARCHAR(50)  NULL,
    CodIndicator                 VARCHAR(255) NULL,
    IdClsf                       INT          NULL,
    IndicatorFX                  VARCHAR(255) NULL,
    Prevedere_Bugetara_Initiala  DOUBLE       NULL,
    Credit_Bugetar_Initial       DOUBLE       NULL,
    Angajament_Legal             DOUBLE       NULL,
    Credit_Bugetar_Definitiv     DOUBLE       NULL,
    Receptii                     DOUBLE       NULL,
    Plati                        DOUBLE       NULL,
    DTQ                          DATETIME     NULL,
    NrCrt                        INT          NULL,
    IdUnitate                    INT          NULL,
    SS                           VARCHAR(255) NULL,
    PRIMARY KEY (CodAI),
    KEY CodAngajament (CodAngajament),
    KEY IdClsf (IdClsf)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- -----------------------------------------------------------------------------
-- FX_Istoric
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS FX_Istoric (
    ID                  INT          NOT NULL,   -- see OPEN ITEM (AutoNumber?)
    IDREV               INT          NULL,
    IdClsf              INT          NULL,
    CodAI               VARCHAR(255) NULL,
    CodAngajament       VARCHAR(255) NULL,
    CodIndicator        VARCHAR(255) NULL,
    DataFX              DATETIME     NULL,
    Utilizator          VARCHAR(255) NULL,
    Descriere           LONGTEXT     NULL,
    Observatii          LONGTEXT     NULL,
    Val_Rezervare_I     DOUBLE       NULL,
    Val_Rezervare_D     DOUBLE       NULL,
    Val_AngLeg          DOUBLE       NULL,
    Val_Rezervare_Ant   DOUBLE       NULL,
    Val_Rezervare_Dif   DOUBLE       NULL,
    Val_Receptie        DOUBLE       NULL,
    Val_Plata           DOUBLE       NULL,
    TipRand             VARCHAR(255) NULL,
    IdTrezor            VARCHAR(255) NULL,
    Doc                 VARCHAR(255) NULL,
    HASH                VARCHAR(64)  NULL,
    Prelucrat           TINYINT(1)   NOT NULL DEFAULT 0,
    DTQ                 DATETIME     NULL,
    Val_Receptie_T      DOUBLE       NULL,
    Rez_Ord             INT          NULL,
    Clsf                VARCHAR(255) NULL,
    PRIMARY KEY (ID),
    UNIQUE KEY HASH (HASH),
    KEY FX_AngajamenteFX_Istoric (CodAngajament),
    KEY FX_Indicatori__FX_Istoric (CodAI)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- -----------------------------------------------------------------------------
-- FX_Rezervari
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS FX_Rezervari (
    IDRZ           INT          NOT NULL,   -- see OPEN ITEM (AutoNumber?)
    IDH            INT          NULL,
    IDREV          INT          NULL,
    IdClsf         INT          NULL,
    CodAI          VARCHAR(255) NULL,
    CodAngajament  VARCHAR(255) NULL,
    CodIndicator   VARCHAR(255) NULL,
    DataRezervare  DATETIME     NULL,
    R_CreditBug    DOUBLE       NULL,
    R_Initiala     DOUBLE       NULL,
    R_Anterioara   DOUBLE       NULL,
    R_Valoare      DOUBLE       NULL,
    R_Definitiva   DOUBLE       NULL,
    PB_CreditAng   DOUBLE       NULL,
    RI_CreditAng   DOUBLE       NULL,
    RD_AngLegal    DOUBLE       NULL,
    Incarcat       TINYINT(1)   NOT NULL DEFAULT 0,
    Preluat        TINYINT(1)   NOT NULL DEFAULT 0,
    AreDDF         TINYINT(1)   NOT NULL DEFAULT 0,
    EInitiala      TINYINT(1)   NOT NULL DEFAULT 0,
    EMicsorare     TINYINT(1)   NOT NULL DEFAULT 0,
    EMarire        TINYINT(1)   NOT NULL DEFAULT 0,
    DTQ            DATETIME     NULL,
    PRIMARY KEY (IDRZ),
    UNIQUE KEY IDH (IDH),
    KEY FX_DDF_REV__FX_Rezervari (IDREV),
    KEY FX_Indicatori__FX_Rezervari (CodAI),
    KEY IdClsf (IdClsf)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- -----------------------------------------------------------------------------
-- FX_Plati
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS FX_Plati (
    IdPlataFX         INT          NOT NULL,   -- see OPEN ITEM (AutoNumber?)
    IDH               INT          NULL,
    IdClsf            INT          NULL,
    IdOP              INT          NULL,
    CodAI             VARCHAR(255) NULL,
    CodAngajament     VARCHAR(255) NULL,
    CodIndicator      VARCHAR(255) NULL,
    NrOP              VARCHAR(255) NULL,
    Data_plata        DATETIME     NULL,
    Clsf              VARCHAR(255) NULL,
    Indicator_IBAN    VARCHAR(255) NULL,
    Obiectiv          VARCHAR(255) NULL,
    Probleme          VARCHAR(255) NULL,
    Program           VARCHAR(255) NULL,
    Proiect           VARCHAR(255) NULL,
    Referinta_TREZOR  VARCHAR(255) NULL,
    Suma              DOUBLE       NULL,
    Tip               VARCHAR(255) NULL,
    Incarcat          TINYINT(1)   NOT NULL DEFAULT 0,
    Preluat           TINYINT(1)   NOT NULL DEFAULT 0,
    DTQ               DATETIME     NULL,
    IDREV             INT          NULL,
    IdUnitate         INT          NULL,
    PRIMARY KEY (IdPlataFX),
    UNIQUE KEY FX_Istoric__FX_Plati (IDH),
    KEY FX_Indicatori__FX_Plati (CodAI),
    KEY IdOP (IdOP)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
