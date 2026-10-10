-- SLICE-AD11-03. DDL ONLY: run on AVACONT_SURSA after AD_05, AD_06, AD_07.
-- AvacontPush schema synchronization propagates these definitions to the units.
-- Then run AD_08_02_interogare_unica.sql through Interogari unice.
USE AVACONT_SURSA;

ALTER TABLE AD_Platitori_sub ADD COLUMN IF NOT EXISTS CodAccesPortal VARCHAR(64) NULL;
CREATE TABLE IF NOT EXISTS AD_PortalParents (
  CNP VARCHAR(13) CHARACTER SET ascii COLLATE ascii_bin NOT NULL PRIMARY KEY,
  Email VARCHAR(255) NULL,
  CodAccesPortal VARCHAR(64) CHARACTER SET ascii COLLATE ascii_bin NULL,
  UNIQUE KEY uq_Portal_Email (Email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;
CREATE TABLE IF NOT EXISTS AD_PortalChallenges (
  ChallengeId VARCHAR(64) CHARACTER SET ascii COLLATE ascii_bin NOT NULL PRIMARY KEY,
  CNP VARCHAR(13) NOT NULL,
  Email VARCHAR(255) NOT NULL,
  AccessHash CHAR(64) NOT NULL,
  CodeHash CHAR(64) NOT NULL,
  ExpiresAt BIGINT NOT NULL,
  Attempts INT NOT NULL DEFAULT 0,
  CreatedAt BIGINT NOT NULL,
  KEY ix_Challenge_CNP (CNP, CreatedAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;
