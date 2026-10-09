-- SLICE-ADE9-01: rights per SECTION of the site, by role, per unit.  Target: AVACONT_COMUN.
-- A user can hold roles in several sections (KB = K-BOT itself, AD = ADECHIT, later VR, AV).
-- A role belongs to ONE section and stands for ONE operation of that section (names in Romanian,
-- no diacritics: AD_CITIRE, AD_PREZENTA, ...). The grant is (user, unit, role); a user holds
-- as many roles as needed. utf8mb3 / utf8mb3_general_ci like the rest of the server.
-- Rerunnable: CREATE IF NOT EXISTS + INSERT IGNORE.

USE AVACONT_COMUN;

CREATE TABLE IF NOT EXISTS Sectiuni (
  Cod        VARCHAR(8)  NOT NULL,
  Denumire   VARCHAR(80) NOT NULL,
  PRIMARY KEY (Cod)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

-- Operatie = the name the server routes ask for (routes/adechit/__init__.py guard('...')).
CREATE TABLE IF NOT EXISTS Roluri (
  IdRol      INT         NOT NULL AUTO_INCREMENT,
  Cod        VARCHAR(32) NOT NULL,
  Sectiune   VARCHAR(8)  NOT NULL,
  Operatie   VARCHAR(32) NOT NULL,
  Denumire   VARCHAR(120) NOT NULL,
  PRIMARY KEY (IdRol),
  UNIQUE KEY ux_Roluri_Cod (Cod),
  UNIQUE KEY ux_Roluri_SectOp (Sectiune, Operatie),
  CONSTRAINT fk_Roluri_Sectiune FOREIGN KEY (Sectiune) REFERENCES Sectiuni (Cod) ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

CREATE TABLE IF NOT EXISTS Utilizatori_Roluri (
  UN         VARCHAR(80) NOT NULL,
  DC         VARCHAR(64) NOT NULL,
  IdRol      INT         NOT NULL,
  PRIMARY KEY (UN, DC, IdRol),
  KEY ix_UtilRoluri_DC (DC),
  KEY ix_UtilRoluri_Rol (IdRol),
  CONSTRAINT fk_UtilRoluri_DC  FOREIGN KEY (DC)    REFERENCES Unitati (DC) ON UPDATE CASCADE,
  CONSTRAINT fk_UtilRoluri_Rol FOREIGN KEY (IdRol) REFERENCES Roluri (IdRol) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

-- KB is listed so every section has a row; its rights stay in Unitati_Utilizatori.Rol for now.
INSERT IGNORE INTO Sectiuni (Cod, Denumire) VALUES
  ('KB', 'K-BOT'), ('AD', 'ADECHIT'), ('VR', 'VR'), ('AV', 'AV');

INSERT IGNORE INTO Roluri (Cod, Sectiune, Operatie, Denumire) VALUES
  ('AD_CITIRE',      'AD', 'read',       'ADECHIT - citire'),
  ('AD_PREZENTA',    'AD', 'attendance', 'ADECHIT - prezenta'),
  ('AD_PLATI',       'AD', 'collect',    'ADECHIT - plati (incasari, alte plati, restituiri)'),
  ('AD_ANULARE',     'AD', 'cancel',     'ADECHIT - anulare documente'),
  ('AD_INCHIDERE',   'AD', 'close',      'ADECHIT - inchidere luna'),
  ('AD_REDESCHIDERE','AD', 'reopen',     'ADECHIT - redeschidere luna'),
  ('AD_CATALOAGE',   'AD', 'catalog',    'ADECHIT - cataloage (grupe, copii, taxe)'),
  ('AD_TRANSFER',    'AD', 'transfer',   'ADECHIT - mutare copil / plecat'),
  ('AD_IMPORT',      'AD', 'import',     'ADECHIT - import');

-- Grants are made by hand (the administrator). Everything in ADECHIT for one user and unit:
--   INSERT IGNORE INTO Utilizatori_Roluri (UN, DC, IdRol)
--     SELECT 'scavatarsoft@gmail.com', '000_DEMO', IdRol FROM Roluri WHERE Sectiune = 'AD';
-- Only some roles: ... WHERE Cod IN ('AD_CITIRE', 'AD_PREZENTA');
-- Take one away:
--   DELETE ur FROM Utilizatori_Roluri ur JOIN Roluri r ON r.IdRol = ur.IdRol
--    WHERE ur.UN = '...' AND ur.DC = '...' AND r.Cod = 'AD_PLATI';
-- The user must also exist in Unitati_Utilizatori for that unit (that is what lets the portal sign in).
-- Note: the data routes need AD_CITIRE for every screen, so give it together with any other AD_ role.
