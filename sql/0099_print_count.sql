-- =====================================================================================
-- Slice 0099 -- PrintCount: how many times a document was sent to a printer from K-BOT.
--
-- WHY (operator, 01.10.2026). Every table that holds a PDF gets `PrintCount`, raised by one
-- each time the operator prints the document shown in K-BOT's Adobe pane. K-BOT has no print
-- command of its own: the operator prints from inside Adobe, and K-BOT notices the job in the
-- Windows print queue (KBot.Controls/Adobe/AdobePrintWatcher.vb). «Microsoft Print to PDF» is a
-- printer like any other and counts.
--
-- THE FOUR PDF TABLES:
--   FX_DDF_PDF            the signed PDF of a DDF revision
--   FX_ORD_PDF            the signed PDF of an ordonantare
--   FX_NoteCAB_PDF        the PDF of a CAB correction note (stored from its creation)
--   FX_NoteCAB_Recipisa   the FOREXE receipt of a note
--
-- AND TWO DOCUMENT TABLES (operator's choice, same day). FX_DDF_PDF / FX_ORD_PDF have a row
-- only once the document is SIGNED; an unsigned DDF / ORD is generated on the operator's
-- computer and never stored. A print of an unsigned document is counted on the document row:
--   FX_DDF_REV            prints made while the revision had no PDF row
--   FX_ORD                prints made while the ordonantare had no PDF row
-- Total prints of a document = its PDF row's count + its document row's count. A re-signed PDF
-- replaces the bytes of the same row (pdf.py upsert) and KEEPS the count.
--
-- Written by: POST .../print (routes/forexe/print_count.py). Nothing else reads or writes it.
--
-- APPLY ON EVERY UNIT DATABASE (000_DEMO first) and on AVACONT_SURSA, the template for new
-- units, BEFORE print_count.py reaches the VPS. Safe to run twice (IF NOT EXISTS). Until it is
-- applied the route answers with the name of this file and counts nothing; nothing else changes.
--
-- FX_NoteCAB_Recipisa exists only where sql/0088_04_fx_notecab_recipisa.sql was applied (the
-- dump of 22.09.2026 has it in 000_DEMO, not in AVACONT_SURSA): run that file first there, or
-- the last statement fails with «table doesn't exist» -- the five before it are already done.
--
-- Shapes read from MariaDB_Schema/000_DEMO.sql (dump of 22.09.2026). Every INSERT into these
-- six tables names its columns (checked in PYTHON/routes), so a new column with a default
-- breaks none of them.
-- =====================================================================================

ALTER TABLE `FX_DDF_PDF`
  ADD COLUMN IF NOT EXISTS `PrintCount` int(11) NOT NULL DEFAULT 0;

ALTER TABLE `FX_ORD_PDF`
  ADD COLUMN IF NOT EXISTS `PrintCount` int(11) NOT NULL DEFAULT 0;

ALTER TABLE `FX_NoteCAB_PDF`
  ADD COLUMN IF NOT EXISTS `PrintCount` int(11) NOT NULL DEFAULT 0;

ALTER TABLE `FX_DDF_REV`
  ADD COLUMN IF NOT EXISTS `PrintCount` int(11) NOT NULL DEFAULT 0;

ALTER TABLE `FX_ORD`
  ADD COLUMN IF NOT EXISTS `PrintCount` int(11) NOT NULL DEFAULT 0;

ALTER TABLE `FX_NoteCAB_Recipisa`
  ADD COLUMN IF NOT EXISTS `PrintCount` int(11) NOT NULL DEFAULT 0;
