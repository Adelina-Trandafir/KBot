-- SLICE-00EF-23: the ANAF archive (fact<id>.zip: invoice XML + ANAF signature file) kept next to the XML.
-- Run in EVERY unit database (000_DEMO and each DC). Safe to run again.
-- A message migrated with its zip keeps ONLY the zip (XmlContinut is NULL): the server reads the XML out of it.
-- NOT RUN anywhere yet. The Migrator tab E-Factura fills it from the folder of zip files.
ALTER TABLE `EF_Mesaje`
  ADD COLUMN IF NOT EXISTS `ZipContinut` longblob NULL DEFAULT NULL
  COMMENT 'The zip ANAF returned (invoice XML + semnatura_<id>.xml, the ANAF seal). NULL when only the XML was kept'
  AFTER `XmlContinut`;

-- Check afterwards:  SHOW CREATE TABLE EF_Mesaje;
