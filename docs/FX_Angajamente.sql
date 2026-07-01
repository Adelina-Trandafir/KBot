-- FX_Angajamente — tabel țintă pentru fluxul ListaAngajamente (felia FOREXE).
-- Sursă coloane: export Access FX_Angajamente, MINUS IdUnitate (decizie: IdUnitate
-- a migrat în FX_Indicatori). Text->VARCHAR, DateTime->DATETIME, Boolean->TINYINT(1).
-- Charset identic cu clona admin.setup_db (utf8mb4 / utf8mb4_general_ci).
--
-- A se rula pe:
--   1. baza unității de test (ex. 018_GRRS) — pentru round-trip;
--   2. etalonul AVACONT_SURSA — ca setup_db să includă tabelul la clonările viitoare.
-- NICIODATĂ în AVACONT_COMUN.
--
-- Upsert-ul scrie doar: CodAngajament, Descriere, Stare, DC, Preluat.
-- IDDF/DataCreare/DataDefinitivare/DTQ/Incarcat/Salarii există pentru fluxuri viitoare.

CREATE TABLE IF NOT EXISTS FX_Angajamente (
    CodAngajament    VARCHAR(50)   NOT NULL,
    IDDF             INT           NULL,            -- legătură cross-family (FX_DDF); FĂRĂ FK, doar index
    DataCreare       DATETIME      NULL,
    DataDefinitivare DATETIME      NULL,
    Descriere        VARCHAR(255)  NULL,
    Stare            VARCHAR(50)   NULL,
    DC               VARCHAR(255)  NULL,
    DTQ              DATETIME      NULL,
    Incarcat         TINYINT(1)    NOT NULL DEFAULT 0,
    Preluat          TINYINT(1)    NOT NULL DEFAULT 0,
    Salarii          TINYINT(1)    NOT NULL DEFAULT 0,
    PRIMARY KEY (CodAngajament),
    KEY ix_FX_Angajamente_IDDF (IDDF),
    KEY ix_FX_Angajamente_DC (DC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
