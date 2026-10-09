-- SLICE-AD10-02: a role is a SET of operations (Roluri_Operatii) + the KB (K-BOT) roles.  Target: AVACONT_COMUN.
-- Step 1 only DESCRIBES the rights; no server route refuses anything yet because of them.
-- Run ONCE after AD10_01 (rerunnable: every step checks first).
USE AVACONT_COMUN;

CREATE TABLE IF NOT EXISTS Roluri_Operatii (
  IdRol      INT         NOT NULL,
  Operatie   VARCHAR(32) NOT NULL,
  PRIMARY KEY (IdRol, Operatie),
  CONSTRAINT fk_RoluriOp_Rol FOREIGN KEY (IdRol) REFERENCES Roluri (IdRol) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

-- Move the ADECHIT operations (one per role) from Roluri.Operatie into Roluri_Operatii.
SET @has := (SELECT COUNT(*) FROM information_schema.columns
             WHERE table_schema = 'AVACONT_COMUN' AND table_name = 'Roluri' AND column_name = 'Operatie');
SET @sql := IF(@has > 0,
  'INSERT IGNORE INTO Roluri_Operatii (IdRol, Operatie) SELECT IdRol, Operatie FROM Roluri', 'DO 0');
PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;

-- The foreign key on Sectiune leans on the (Sectiune, Operatie) index: give it its own index first.
ALTER TABLE Roluri ADD INDEX IF NOT EXISTS ix_Roluri_Sectiune (Sectiune);
ALTER TABLE Roluri DROP INDEX IF EXISTS ux_Roluri_SectOp;
ALTER TABLE Roluri DROP COLUMN IF EXISTS Operatie;

-- KB roles. Their name is 'KB_' + the text in Unitati_Utilizatori.Rol (upper case): that column stays the
-- ONE place where a user's K-BOT role is set; nobody is granted a KB role in Utilizatori_Roluri.
INSERT IGNORE INTO Roluri (Cod, Sectiune, Denumire) VALUES
  ('KB_CONTABIL',      'KB', 'K-BOT - contabil (tot)'),
  ('KB_DIRECTOR',      'KB', 'K-BOT - director (vede tot, doar semneaza)'),
  ('KB_ADMINISTRATOR', 'KB', 'K-BOT - administrator (ca contabilul, fara FOREXE)'),
  ('KB_SECRETAR',      'KB', 'K-BOT - secretar (vede doar DDF si ORD, semneaza)');

-- Operations of KB:
--   CITIRE          sees everything              EDITARE   add / change / delete
--   FOREXE          connects to and works in FOREXE
--   SEMNARE_A, SEMNARE_B, SEMNARE_ORDONATOR   the signature fields (DDF: A, B, Ordonator;
--                   ORD: AB, CD, Ordonator -- how AB/CD map onto A/B is decided when signing gets enforced)
--   CITIRE_DDF_ORD  sees only DDF and ORD
-- For now EVERYBODY may sign everything (operator decision 09.10.2026). To bind a signature to a role later:
--   DELETE ro FROM Roluri_Operatii ro JOIN Roluri r ON r.IdRol = ro.IdRol
--    WHERE r.Cod = 'KB_CONTABIL' AND ro.Operatie = 'SEMNARE_A';
INSERT IGNORE INTO Roluri_Operatii (IdRol, Operatie)
  SELECT r.IdRol, x.Operatie FROM Roluri r JOIN (
    SELECT 'KB_CONTABIL' AS Cod, 'CITIRE' AS Operatie UNION ALL SELECT 'KB_CONTABIL','EDITARE'
    UNION ALL SELECT 'KB_CONTABIL','FOREXE'
    UNION ALL SELECT 'KB_DIRECTOR','CITIRE'
    UNION ALL SELECT 'KB_ADMINISTRATOR','CITIRE' UNION ALL SELECT 'KB_ADMINISTRATOR','EDITARE'
    UNION ALL SELECT 'KB_SECRETAR','CITIRE_DDF_ORD'
    UNION ALL SELECT c.Cod, s.Operatie
      FROM (SELECT 'KB_CONTABIL' AS Cod UNION ALL SELECT 'KB_DIRECTOR' UNION ALL SELECT 'KB_ADMINISTRATOR'
            UNION ALL SELECT 'KB_SECRETAR') c
      CROSS JOIN (SELECT 'SEMNARE_A' AS Operatie UNION ALL SELECT 'SEMNARE_B' UNION ALL SELECT 'SEMNARE_ORDONATOR') s
  ) x ON x.Cod = r.Cod;

-- Check:  SELECT r.Cod, GROUP_CONCAT(ro.Operatie ORDER BY ro.Operatie) FROM Roluri r
--         LEFT JOIN Roluri_Operatii ro ON ro.IdRol = r.IdRol GROUP BY r.Cod;
