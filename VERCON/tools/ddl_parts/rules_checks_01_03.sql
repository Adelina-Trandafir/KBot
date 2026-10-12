ALTER TABLE `VC_Gestiuni` ADD CONSTRAINT `ck_VC_Gestiune_Type` CHECK (`Type` IN ('MATERIALE','ALIMENTE','OI','MF'));
ALTER TABLE `VC_Gestiuni` ADD CONSTRAINT `ck_VC_Gestiune_State` CHECK (`State` IN ('IN_PREGATIRE','IMPORTATA_IN_REVIZIE','ACTIVA','ARHIVATA'));
ALTER TABLE `VC_Gestiuni` ADD CONSTRAINT `ck_VC_Gestiune_Zec` CHECK (`Zec` <= 4);
ALTER TABLE `VC_Gestiuni` ADD CONSTRAINT `ck_VC_Gestiune_Scara` CHECK (`Scara` >= `Zec` AND `Scara` <= 4);
ALTER TABLE `VC_Exercitii` ADD CONSTRAINT `ck_VC_Exercitiu_An` CHECK (`An` BETWEEN 1990 AND 2100);
ALTER TABLE `VC_ContextLock` ADD CONSTRAINT `ck_VC_Lock_Expiry` CHECK (`ExpiresAt` > `AcquiredAt`);
ALTER TABLE `VC_IdMap` ADD CONSTRAINT `ck_VC_IdMap_Kind` CHECK (`Kind` IN ('ROW','GROUP','ASSET'));
ALTER TABLE `VC_MigrationIssues` ADD CONSTRAINT `ck_VC_Issue_Severity` CHECK (`Severity` IN ('BLOCAJ','AVERTIZARE','CONVERSIE'));
ALTER TABLE `VC_AnnualClosings` ADD CONSTRAINT `ck_VC_Closing_order` CHECK (`FromExercitiuId` <> `ToExercitiuId`);
