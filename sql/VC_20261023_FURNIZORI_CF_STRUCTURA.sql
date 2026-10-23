-- VERCON ONLINE ONLY. Prepared SQL; execute ONLY by the operator.
-- KBotAdmin -> Interogări unice -> Din fișier.
-- NAME: VC_20261023_FURNIZORI_CF_STRUCTURA
-- Check the DC databases to update. AVACONT_SURSA is always included first.
-- The same structure is then installed directly in each checked DC.
-- Alternatively run only on AVACONT_SURSA manually, then use schema synchronization.
-- Prerequisite: VC_FACTURI exists (VC_FACTURI_NIR_20261016.sql).
-- DDL commits implicitly in MariaDB. IF NOT EXISTS permits resuming a partial run.
-- After this succeeds, use VC_20261023_FURNIZORI_CF_KBOTADMIN.sql on the SAME DCs.
-- No existing identity, foreign key, or historical column is removed/changed.
-- CodPartener means NORMALIZED FISCAL CODE in BOTH tables, never a unit code.

ALTER TABLE VC_NIR
  ADD COLUMN IF NOT EXISTS CodPartener VARCHAR(50)
    CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL
    COMMENT 'Online VERCON: normalized supplier fiscal code',
  ADD INDEX IF NOT EXISTS IX_VC_NIR_CF (CodPartener);

ALTER TABLE VC_FACTURI
  ADD COLUMN IF NOT EXISTS CodPartener VARCHAR(50)
    CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL
    COMMENT 'Online VERCON: normalized supplier fiscal code',
  ADD INDEX IF NOT EXISTS IX_VC_FACTURI_CF_DATA (CodPartener, DATA);
