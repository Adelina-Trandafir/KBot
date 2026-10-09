-- SLICE-ADE6-15: apply once to a unit database with AD_03 already installed.
-- Month periods are stored as YYYY-MM; old rows remain unknown (NULL).
ALTER TABLE `AD_ValoriTaxe`
  ADD COLUMN `DeLa` VARCHAR(7) NULL,
  ADD COLUMN `PanaLa` VARCHAR(7) NULL;
