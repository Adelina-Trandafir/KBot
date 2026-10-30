-- SLICE-AD15-01. Execute MANUALLY in each ADECHIT unit database after AD_11.
-- Based on MariaDB_Schema/AVACONT_SURSA.sql exported on 2026-10-29.
-- Existing associations, foreign keys and unique payment/document keys are preserved.
-- A bank movement can have one allocation per child, saved together in one transaction.
-- The application still reserves the entire IDFXE across all subunits, including on cancellation.
-- No changes in AVACONT_COMUN. No application-side DDL.
ALTER TABLE `AD_EXTRASE_ASOCIERI`
  DROP PRIMARY KEY,
  ADD PRIMARY KEY (`IDFXE`, `IDP`);
