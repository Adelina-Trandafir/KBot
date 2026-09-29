-- Slice 0078-05: bench-only store of the signed PDFs the signing benches send.
--
-- ONLY in 000_DEMO. Unrelated to every other table (no foreign keys, no parent): it keeps EVERY
-- upload as a new row, so what the client sent -- and when -- can be looked at on the server,
-- byte for byte. The route (PUT /api/forexe/banc/pdf/<tip>/<id>) refuses any other database.
-- Not created on the unit databases; nothing in production reads or writes it.

USE `000_DEMO`;

CREATE TABLE IF NOT EXISTS `KBOT_BANC_PDF` (
  `Id`            BIGINT        NOT NULL AUTO_INCREMENT,
  `Primit`        DATETIME(3)   NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
  `Tip`           VARCHAR(8)    NOT NULL,              -- DDF / ORD / NC
  `IdDoc`         INT           NOT NULL,              -- IDREV / IDORDP / IDNC typed on the bench
  `NumeFisier`    VARCHAR(255)  NOT NULL DEFAULT '',   -- the local file the bench had on screen
  `Pas`           VARCHAR(255)  NOT NULL DEFAULT '',   -- free text from the bench (what was done)
  `Semnatura`     VARCHAR(64)   NOT NULL DEFAULT '',   -- X-Semnatura: roles found in the PDF
  `Semnaturi`     TEXT          NULL,                  -- X-Semnaturi decoded: JSON of the added signatures
  `Statie`        TEXT          NULL,                  -- X-Statie decoded: JSON of the computer
  `ShaPrecedent`  VARCHAR(64)   NOT NULL DEFAULT '',   -- X-Sha-Precedent, stored only (no concurrency check)
  `Sha256`        CHAR(64)      NOT NULL,              -- recomputed on the server over the body
  `Dimensiune`    INT           NOT NULL,
  `Operator`      VARCHAR(128)  NOT NULL DEFAULT '',   -- the logged-in user
  `Continut`      LONGBLOB      NOT NULL,              -- the bytes exactly as received
  PRIMARY KEY (`Id`),
  KEY `IX_KBOT_BANC_PDF_Doc` (`Tip`, `IdDoc`, `Primit`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
