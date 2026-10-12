-- SLICE-VC04-01: roles of section VR. Target: AVACONT_COMUN. Rerunnable (CREATE IF NOT EXISTS is in AD10_01; here only
-- INSERT IGNORE). Needs sql/AD10_01_sectiuni_roluri.sql and sql/AD10_03_roluri_kb_operatii.sql applied first.
-- One role = ONE operation (same shape as the AD_ roles). The operation names are the ones the server asks for
-- (PYTHON/routes/vercon/operations.py); the type each one is valid for is checked in code, not here.
-- NO role is granted by default and none grants everything. Not run from this chat.

USE AVACONT_COMUN;

INSERT IGNORE INTO Roluri (Cod, Sectiune, Operatie, Denumire) VALUES
  ('VR_CITIRE',        'VR', 'read',          'VERCON - citire'),
  ('VR_NOMENCLATOARE', 'VR', 'nomenclatoare', 'VERCON - nomenclatoare (produse, categorii, gestionari)'),
  ('VR_RECEPTIE',      'VR', 'receptie',      'VERCON - receptie (NIR)'),
  ('VR_STORNO',        'VR', 'storno',        'VERCON - storno intrari'),
  ('VR_CONSUM',        'VR', 'consum',        'VERCON - bon de consum'),
  ('VR_RETUR',         'VR', 'retur',         'VERCON - retur'),
  ('VR_ALIMENTE',      'VR', 'alimente',      'VERCON - lista de alimente'),
  ('VR_OI_PREDARE',    'VR', 'oi_predare',    'VERCON - OI/MF bon de predare'),
  ('VR_OI_TRANSFER',   'VR', 'oi_transfer',   'VERCON - OI/MF bon de transfer'),
  ('VR_OI_CASARE',     'VR', 'oi_casare',     'VERCON - OI/MF bon de casare'),
  ('VR_MF',            'VR', 'mf',            'VERCON - mijloace fixe'),
  ('VR_RAPORT',        'VR', 'raport',        'VERCON - rapoarte si export'),
  ('VR_IMPORT',        'VR', 'import',        'VERCON - import, reconciliere, activare'),
  ('VR_SETARI',        'VR', 'configure',     'VERCON - setari si tipul gestiunii'),
  ('VR_INCHIDERE',     'VR', 'close',         'VERCON - inchidere an'),
  ('VR_REDESCHIDERE',  'VR', 'reopen',        'VERCON - redeschidere an');

-- Grants are made by hand. Read-only user for one unit:
--   INSERT IGNORE INTO Utilizatori_Roluri (UN, DC, IdRol)
--     SELECT 'user@x.ro', '<DC>', IdRol FROM Roluri WHERE Cod IN ('VR_CITIRE', 'VR_RAPORT');
-- Storekeeper (reads, receives, consumes, returns):
--   ... WHERE Cod IN ('VR_CITIRE','VR_RECEPTIE','VR_STORNO','VR_CONSUM','VR_RETUR','VR_RAPORT');
-- Every screen needs VR_CITIRE: give it together with any other VR_ role.
-- Take one away:
--   DELETE ur FROM Utilizatori_Roluri ur JOIN Roluri r ON r.IdRol = ur.IdRol
--    WHERE ur.UN = '...' AND ur.DC = '...' AND r.Cod = 'VR_CONSUM';
-- The user must also exist in Unitati_Utilizatori for that unit (that lets the portal sign in).
-- Check what a user holds (the server reads exactly this):
--   SELECT r.Cod, ro.Operatie FROM Utilizatori_Roluri ur JOIN Roluri r ON r.IdRol = ur.IdRol
--     JOIN Roluri_Operatii ro ON ro.IdRol = r.IdRol WHERE ur.UN = '...' AND ur.DC = '...' AND r.Sectiune = 'VR';
--
-- The server derives operations from Roluri_Operatii (drepturi.operations_of), not from Roluri.Operatie alone: a role
-- without a row in Roluri_Operatii grants nothing. Fill it the way AD roles are filled:
INSERT IGNORE INTO Roluri_Operatii (IdRol, Operatie)
  SELECT IdRol, Operatie FROM Roluri WHERE Sectiune = 'VR';
