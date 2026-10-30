-- =====================================================================================
-- Slice AV01-17 -- RUN THIS ON `AVACONT_SURSA` ONLY (Navicat / mysql, as root), then «Sincronizare schema» on the unit database.
-- Needs AV01_14 (TZ_SablonOperatiiLinie) already run.
--
-- WHAT THIS ADDS to TZ_SablonOperatiiLinie  (operator, 29.10.2026: «Sabloane» pages «Operatii cu clasificatie» / «Operatii diverse»)
--   IdClsf          the classification of THE LINE: link to Clasificatii.IDClsf (one concrete row: chapter, form, article, paragraph, sector+source).
--                   Type M: the header (TZ_SablonOperatii.Cheie) holds the classification with XX.XX; the server fills IdClsf of each line from the key + the form
--                   of the line + the sector+source of the journal (NULL when no such row exists).
--                   Type A: the header has NO classification; each line may have its own (NULL = none), chosen by the operator.
--   AnaliticDebit   the analytic of the debit account (TZ_PlanCont.Analitic, e.g. 01 of 421000002A.01); NULL = the account without analytic.
--   AnaliticCredit  idem credit.
--   The account of a line stays the BASE account (ContDebit / ContCredit); base + analytic picks the account in the chart of accounts, the part tied to the
--   classification (ClsfF / ClsfE) is composed from the classification of the line when the template is used.
--
-- A classification that a line points at cannot be deleted (ON DELETE RESTRICT; the dependency check of the classifications finds the key by itself).
-- Safe to run twice (ADD COLUMN IF NOT EXISTS, MariaDB 10.0.2+; the key is added only when it is missing).
-- =====================================================================================

ALTER TABLE `AVACONT_SURSA`.`TZ_SablonOperatiiLinie`
  ADD COLUMN IF NOT EXISTS `IdClsf` int(11) NULL DEFAULT NULL
      COMMENT 'Classification of the line = Clasificatii.IDClsf; type M filled by the server from the key + form, type A chosen per line; NULL = none' AFTER `Forma`,
  ADD COLUMN IF NOT EXISTS `AnaliticDebit` varchar(10) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL
      COMMENT 'Analytic of the debit account (TZ_PlanCont.Analitic); NULL = none' AFTER `ContDebit`,
  ADD COLUMN IF NOT EXISTS `AnaliticCredit` varchar(10) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL
      COMMENT 'Analytic of the credit account (TZ_PlanCont.Analitic); NULL = none' AFTER `ContCredit`,
  ADD INDEX IF NOT EXISTS `IX_TZ_SablonOperatiiLinie_Clsf` (`IdClsf` ASC) USING BTREE;

ALTER TABLE `AVACONT_SURSA`.`TZ_SablonOperatiiLinie`
  DROP FOREIGN KEY IF EXISTS `TZ_SablonOperatiiLinie__Clsf`;
ALTER TABLE `AVACONT_SURSA`.`TZ_SablonOperatiiLinie`
  ADD CONSTRAINT `TZ_SablonOperatiiLinie__Clsf` FOREIGN KEY (`IdClsf`) REFERENCES `AVACONT_SURSA`.`Clasificatii` (`IDClsf`) ON DELETE RESTRICT ON UPDATE RESTRICT;

-- Check (read only):
-- SHOW CREATE TABLE `AVACONT_SURSA`.`TZ_SablonOperatiiLinie`;
-- Then: AvacontPush, tab «Sincronizare schema», SAFE, on every unit database.
