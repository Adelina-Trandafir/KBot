-- =====================================================================
--  AVACONT_COMUN.FX_Inregistrari — self-service registration requests
--  Slice 0075-02. Plan: docs/PLAN_AutoProvisioning.md §5.5.
--
--  One row per request filed from the public page. The row is written by
--  POST /api/inregistrare/cerere and then waits: the operator approves or
--  rejects it (§7), and the provisioning job (§5.6) fills DbName and moves
--  Stare on. Nothing outside AVACONT_COMUN is touched until approval.
--
--  K-BOT server only. Nothing is ever written on the legacy server (D8).
--  InnoDB / utf8mb4, to match the login tables next to it. Idempotent.
-- =====================================================================

USE `AVACONT_COMUN`;

CREATE TABLE IF NOT EXISTS `FX_Inregistrari` (
  `IdCerere`     INT           NOT NULL AUTO_INCREMENT,

  -- Proven by the six-digit code before the request could be filed, and ≤ 80
  -- characters because it becomes `Unitati_Utilizatori.UN`, which is varchar(80).
  `Email`        VARCHAR(80)   NOT NULL,

  -- Digits only, as normalized by the ANAF step (the `RO` prefix is stripped).
  `CF`           VARCHAR(32)   NOT NULL,

  -- TWO names on purpose, and they are allowed to differ. `DenumireAnaf` is what
  -- ANAF answered, fetched by the server itself; `Denumire` is what the applicant
  -- typed, which they may edit freely. The approval page shows them side by side,
  -- which only means anything while the first one is the server's own copy.
  `DenumireAnaf` VARCHAR(255)  NOT NULL,
  `Denumire`     VARCHAR(255)  NOT NULL,

  -- Current year or the one before it (D6). Never the future.
  `An`           INT           NOT NULL,

  -- {"ss": [...], "f": [...], "e": [...]} — the chosen sector-sources and the
  -- checked leaves of both trees. LONGTEXT + json_valid is what MariaDB's JSON
  -- type is underneath; written explicitly so the check is visible in the DDL.
  `Payload`      LONGTEXT      NOT NULL CHECK (json_valid(`Payload`)),

  -- InAsteptare / Aprobata / Respinsa / Esuata. ASCII identifiers, not sentences:
  -- the operator page translates them for the screen.
  `Stare`        VARCHAR(16)   NOT NULL DEFAULT 'InAsteptare',

  -- Filled at approval, not here: the name is recomputed then, because a number
  -- free when the wizard ran can be taken by the time the operator decides (§5.4).
  `DbName`       VARCHAR(64)       NULL,

  -- Why it was rejected, or how the provisioning job failed. Read by a human.
  `Motiv`        TEXT              NULL,

  `DataCerere`   DATETIME      NOT NULL DEFAULT current_timestamp(),
  `DataDecizie`  DATETIME          NULL,
  `Decis`        VARCHAR(80)       NULL,            -- the operator who decided

  `IpAddress`    VARCHAR(45)       NULL,            -- IPv4/IPv6, from ProxyFix

  PRIMARY KEY (`IdCerere`),
  KEY `ix_FX_Inregistrari_Stare` (`Stare`),         -- the list is filtered by it
  KEY `ix_FX_Inregistrari_CF`    (`CF`),
  KEY `ix_FX_Inregistrari_Email` (`Email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- No foreign keys, deliberately. A request is a piece of correspondence, not a
-- unit: it exists before the database, the CAI row and the account do, it may end
-- as `Respinsa` and never point at anything, and a failed run must leave the row
-- behind with its `Motiv` intact rather than be cascaded away with the wreckage.
