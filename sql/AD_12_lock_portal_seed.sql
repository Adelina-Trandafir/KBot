-- Date de initializare, fara modificari de schema.
-- Se executa MANUAL in baza DESTINATIE a migrarii, dupa sincronizarea structurii.
-- Pentru jurnalul raportat: 012_GR30. Pentru alta destinatie, schimbati USE.
-- Nu este suficienta executarea in AVACONT_SURSA: sincronizarea schemei nu copiaza randuri.
-- Repetarea nu modifica randul daca exista deja. Nu modifica identitatile sau codurile portalului.
USE `012_GR30`;

INSERT INTO `AD_Lock` (`ID`) VALUES (0)
ON DUPLICATE KEY UPDATE `ID` = `ID`;
