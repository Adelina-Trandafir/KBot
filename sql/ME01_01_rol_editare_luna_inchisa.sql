-- SLICE-ME01: the ADECHIT right "edit the attendance of the LAST closed month", per account.  Target: AVACONT_COMUN.
-- Live shape (see MariaDB_Schema/AVACONT_COMUN.sql): Roluri has no Operatie column, the operations live in
-- Roluri_Operatii. Rerunnable (INSERT IGNORE). KBotAdmin can do the same from its "Drepturi pe conturi" page
-- (button "Creeaza rolul"); this file is for doing it by hand or on a copy.
--
-- The role only means something together with AD_PREZENTA ('attendance'): the route that saves the days is
-- guarded by 'attendance', and 'edit_closed' lets it also touch the last closed month
-- (PYTHON/routes/adechit/service.py save_attendance).

USE AVACONT_COMUN;

INSERT IGNORE INTO Roluri (Cod, Sectiune, Denumire)
VALUES ('AD_EDIT_INCHISA', 'AD', 'ADECHIT - editare prezenta in ultima luna inchisa');

INSERT IGNORE INTO Roluri_Operatii (IdRol, Operatie)
SELECT IdRol, 'edit_closed' FROM Roluri WHERE Cod = 'AD_EDIT_INCHISA';

-- Give it to one account in one unit (the account must already be linked in Unitati_Utilizatori):
--   INSERT IGNORE INTO Utilizatori_Roluri (UN, DC, IdRol)
--     SELECT 'cont@exemplu.ro', '000_DEMO', IdRol FROM Roluri WHERE Cod = 'AD_EDIT_INCHISA';
-- Take it away:
--   DELETE ur FROM Utilizatori_Roluri ur JOIN Roluri r ON r.IdRol = ur.IdRol
--    WHERE ur.UN = 'cont@exemplu.ro' AND ur.DC = '000_DEMO' AND r.Cod = 'AD_EDIT_INCHISA';
