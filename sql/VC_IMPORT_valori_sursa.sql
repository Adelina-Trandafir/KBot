-- De executat MANUAL dupa schema DECIMAL si inainte de reimportul VRC.
-- Permite copierea semnelor originale din Access, inclusiv preturi negative.
-- Elimina numai vechile CHECK-uri de pret nenegativ, daca au fost instalate.
-- Nu schimba date, chei straine, numerotare sau reguli de calcul ale documentelor noi.
-- Generatorul si VC_Reguli_DC.sql nu mai adauga aceste CHECK-uri.
-- Se numesc explicit AVACONT_SURSA si 000_DEMO: sincronizarea schemei nu copiaza CHECK-uri.
-- Referinta sintaxa: https://mariadb.com/docs/server/reference/sql-statements/data-definition/alter/alter-table

-- AVACONT_SURSA
ALTER TABLE IF EXISTS `AVACONT_SURSA`.`VC_ProduseP` DROP CONSTRAINT IF EXISTS `ck_ProduseP_PUTVA_nonneg`;
ALTER TABLE IF EXISTS `AVACONT_SURSA`.`VC_OI_Produse` DROP CONSTRAINT IF EXISTS `ck_OI_Produse_PUTVA_nonneg`;
ALTER TABLE IF EXISTS `AVACONT_SURSA`.`VC_Intrari` DROP CONSTRAINT IF EXISTS `ck_Intrari_Pret_nonneg`;
ALTER TABLE IF EXISTS `AVACONT_SURSA`.`VC_Intrari` DROP CONSTRAINT IF EXISTS `ck_Intrari_PUTVA_nonneg`;
ALTER TABLE IF EXISTS `AVACONT_SURSA`.`VC_IntrariStorno` DROP CONSTRAINT IF EXISTS `ck_IntrariStorno_PUTVA_nonneg`;
ALTER TABLE IF EXISTS `AVACONT_SURSA`.`VC_Iesiri` DROP CONSTRAINT IF EXISTS `ck_Iesiri_PUTVA_nonneg`;
ALTER TABLE IF EXISTS `AVACONT_SURSA`.`VC_IesiriRetur` DROP CONSTRAINT IF EXISTS `ck_IesiriRetur_PUTVA_nonneg`;
ALTER TABLE IF EXISTS `AVACONT_SURSA`.`VC_OI_Predare` DROP CONSTRAINT IF EXISTS `ck_OI_Predare_PUTVA_nonneg`;
ALTER TABLE IF EXISTS `AVACONT_SURSA`.`VC_OI_Transfer` DROP CONSTRAINT IF EXISTS `ck_OI_Transfer_PUTVA_nonneg`;
ALTER TABLE IF EXISTS `AVACONT_SURSA`.`VC_OI_Casare` DROP CONSTRAINT IF EXISTS `ck_OI_Casare_PUTVA_nonneg`;

-- 000_DEMO
ALTER TABLE IF EXISTS `000_DEMO`.`VC_ProduseP` DROP CONSTRAINT IF EXISTS `ck_ProduseP_PUTVA_nonneg`;
ALTER TABLE IF EXISTS `000_DEMO`.`VC_OI_Produse` DROP CONSTRAINT IF EXISTS `ck_OI_Produse_PUTVA_nonneg`;
ALTER TABLE IF EXISTS `000_DEMO`.`VC_Intrari` DROP CONSTRAINT IF EXISTS `ck_Intrari_Pret_nonneg`;
ALTER TABLE IF EXISTS `000_DEMO`.`VC_Intrari` DROP CONSTRAINT IF EXISTS `ck_Intrari_PUTVA_nonneg`;
ALTER TABLE IF EXISTS `000_DEMO`.`VC_IntrariStorno` DROP CONSTRAINT IF EXISTS `ck_IntrariStorno_PUTVA_nonneg`;
ALTER TABLE IF EXISTS `000_DEMO`.`VC_Iesiri` DROP CONSTRAINT IF EXISTS `ck_Iesiri_PUTVA_nonneg`;
ALTER TABLE IF EXISTS `000_DEMO`.`VC_IesiriRetur` DROP CONSTRAINT IF EXISTS `ck_IesiriRetur_PUTVA_nonneg`;
ALTER TABLE IF EXISTS `000_DEMO`.`VC_OI_Predare` DROP CONSTRAINT IF EXISTS `ck_OI_Predare_PUTVA_nonneg`;
ALTER TABLE IF EXISTS `000_DEMO`.`VC_OI_Transfer` DROP CONSTRAINT IF EXISTS `ck_OI_Transfer_PUTVA_nonneg`;
ALTER TABLE IF EXISTS `000_DEMO`.`VC_OI_Casare` DROP CONSTRAINT IF EXISTS `ck_OI_Casare_PUTVA_nonneg`;
