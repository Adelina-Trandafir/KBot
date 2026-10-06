-- =====================================================================================
-- Slice 00EF-07 -- the reason ANAF refused an invoice. RUN ONLY IF 00EF_02_efactura_unitate.sql WAS ALREADY RUN
-- (a database made from the file as it is now already has the column). NOT RUN anywhere yet. Safe to run again.
--
-- Why: when ANAF refuses an invoice, Access wrote the word «Err» in id_descarcare and showed ANAF's text once, without
-- keeping it. The server keeps the text (ANAF's error message, or the reasons read from its error report) so the screen
-- can show it again. Written only by the server (trimitere.verifica); no token and no secret ever goes in it.
--
-- ORDER (same two steps as 00EF-02 / 00EF-06):
--   1. this file, on AVACONT_SURSA (the template);
--   2. AvacontPush, tab «Sincronizare schema»: SAFE. If that tab does not add a missing COLUMN to existing tables,
--      run the statement below on each unit database that already has EF_Facturi.
-- =====================================================================================

USE `AVACONT_SURSA`;

ALTER TABLE `EF_Facturi`
  ADD COLUMN IF NOT EXISTS `EroareAnaf` varchar(2000) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL
  COMMENT 'Slice 00EF-07: why ANAF refused the invoice (id_descarcare = Err); no secret in it'
  AFTER `AtasamentOriginal`;

-- Check afterwards:  SHOW COLUMNS FROM EF_Facturi LIKE 'EroareAnaf';   -- one row, varchar(2000)
