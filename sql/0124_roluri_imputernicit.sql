-- Slice 0124-05: the K-BOT role «Imputernicit» (the FOREXE proxy) joins the roles KBotAdmin offers for an account.
-- Target: AVACONT_COMUN (the roles live there, like AD10_03_roluri_kb_operatii.sql, which this follows). Rerunnable.
--
-- Unitati_Utilizatori.Rol takes the text after «KB_» in the role's code, title-cased: KB_IMPUTERNICIT -> «Imputernicit». KB_SECRETAR and
-- KB_ADMINISTRATOR already exist; their descriptions are brought in line with the rule of 30.10.2026 (they see ONLY the documents addressed
-- to them; the SERVER enforces it, routes/forexe/ddf_flux_core.py).
USE AVACONT_COMUN;

INSERT IGNORE INTO Roluri (Cod, Sectiune, Denumire) VALUES
  ('KB_IMPUTERNICIT', 'KB', 'K-BOT - imputernicit FOREXE (ca contabilul; vede si ce e adresat contabilului)');

UPDATE Roluri SET Denumire = 'K-BOT - secretar (vede DOAR documentele adresate lui, semneaza Sectiunea A)' WHERE Cod = 'KB_SECRETAR';
UPDATE Roluri SET Denumire = 'K-BOT - administrator (vede DOAR documentele adresate lui, semneaza Sectiunea A)' WHERE Cod = 'KB_ADMINISTRATOR';

-- The operations of the proxy: the accountant's.
INSERT IGNORE INTO Roluri_Operatii (IdRol, Operatie)
  SELECT r.IdRol, x.Operatie FROM Roluri r JOIN (
    SELECT 'CITIRE' AS Operatie UNION ALL SELECT 'EDITARE' UNION ALL SELECT 'FOREXE'
    UNION ALL SELECT 'SEMNARE_A' UNION ALL SELECT 'SEMNARE_B' UNION ALL SELECT 'SEMNARE_ORDONATOR'
  ) x WHERE r.Cod = 'KB_IMPUTERNICIT';

-- Check:  SELECT r.Cod, GROUP_CONCAT(ro.Operatie ORDER BY ro.Operatie) FROM Roluri r
--         LEFT JOIN Roluri_Operatii ro ON ro.IdRol = r.IdRol WHERE r.Sectiune = 'KB' GROUP BY r.Cod;
