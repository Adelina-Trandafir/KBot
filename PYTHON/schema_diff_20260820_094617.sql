-- ------------------------------------------------------------
-- schema_diff — 5 instrucțiuni, în ordinea execuției
-- generat: 2026-08-20T09:46:17
-- ------------------------------------------------------------


-- === 000_demo — FK DROP   *** DISTRUCTIV *** ===
-- [id 49] fx_platilinii.fk_pl_cod
ALTER TABLE `000_demo`.`fx_platilinii` DROP FOREIGN KEY `fk_pl_cod`;

-- === 000_demo — COLLATION MODIFY ===
-- [id 50] fx_plati.Cod
ALTER TABLE `000_demo`.`fx_plati` MODIFY COLUMN `Cod` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL;
-- [id 51] fx_platilinii.Cod
ALTER TABLE `000_demo`.`fx_platilinii` MODIFY COLUMN `Cod` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL;

-- === 000_demo — FK CREATE ===
-- [id 52] fx_istoric.fk_ist_tip
ALTER TABLE `000_demo`.`fx_istoric` ADD CONSTRAINT `fk_ist_tip` FOREIGN KEY (`TipRand`) REFERENCES `avacont_comun`.`defatiprand` (`TipRand`) ON DELETE NO ACTION ON UPDATE NO ACTION;
-- [id 53] fx_platilinii.fk_pl_cod
ALTER TABLE `000_demo`.`fx_platilinii` ADD CONSTRAINT `fk_pl_cod` FOREIGN KEY (`Cod`) REFERENCES `000_demo`.`fx_plati` (`Cod`) ON DELETE NO ACTION ON UPDATE NO ACTION;
