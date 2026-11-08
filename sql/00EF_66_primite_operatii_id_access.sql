-- EF_PrimiteOperatii: the Access operation number gets its OWN column, so IdOperatie has ONE meaning only.
-- To run by hand on AVACONT_SURSA, then propagate with the schema sync to the unit databases.
--
-- Before: IdOperatie held the Access Oper.IdOperatie for a migrated invoice and TZ_Oper.IdOper for one booked in K-BOT; a number could name two operations.
-- Now:
--   IdOperatie       = TZ_Oper.IdOper of the operation the link points to (the key). 0 = the operation is not in TZ_Oper (yet).
--   IdOperatieAccess = the number Access kept (Oper.IdOperatie), as imported; NULL for a link made in K-BOT. Never used to read, only to find IdOperatie
--                      when the operations are migrated: UPDATE ... SET IdOperatie = TZ_Oper.IdOper WHERE IdOperatieAccess = TZ_Oper.IdOperatieAccess AND IdUnitate = unit.
-- The table is emptied before the new migration, so there is nothing to convert.
ALTER TABLE `EF_PrimiteOperatii`
  ADD COLUMN `IdOperatieAccess` int(11) NULL DEFAULT NULL COMMENT 'Access Oper.IdOperatie as imported; NULL = a link made in K-BOT. IdOperatie is TZ_Oper.IdOper' AFTER `IdOperatie`,
  MODIFY COLUMN `IdOperatie` int(11) NOT NULL DEFAULT 0 COMMENT 'TZ_Oper.IdOper of the operation (0 = not in TZ_Oper yet)',
  ADD INDEX `IX_EF_PrimiteOperatii_OperatieAccess`(`IdOperatieAccess` ASC) USING BTREE;
