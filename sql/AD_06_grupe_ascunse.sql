-- De rulat manual de utilizator pe avacont_sursa. Nu este executat de aplicație.
ALTER TABLE `avacont_sursa`.`AD_Grupe`
  ADD COLUMN `Ascunsa` BOOLEAN NOT NULL DEFAULT FALSE AFTER `InchisaDinAn`;
