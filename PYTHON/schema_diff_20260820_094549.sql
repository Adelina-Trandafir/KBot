-- ------------------------------------------------------------
-- schema_diff — 11 instrucțiuni, în ordinea execuției
-- generat: 2026-08-20T09:45:49
-- ------------------------------------------------------------


-- === 000_demo — FK DROP   *** DISTRUCTIV *** ===
-- [id 33] fx_copil.fk_copil_extra
ALTER TABLE `000_demo`.`fx_copil` DROP FOREIGN KEY `fk_copil_extra`;
-- [id 34] fx_copil.fk_copil_ang
ALTER TABLE `000_demo`.`fx_copil` DROP FOREIGN KEY `fk_copil_ang`;
-- [id 35] fx_modif.fk_modif_parent
ALTER TABLE `000_demo`.`fx_modif` DROP FOREIGN KEY `fk_modif_parent`;

-- === 000_demo — TABLE DROP   *** DISTRUCTIV *** ===
-- [id 37] fx_orfan
DROP TABLE `000_demo`.`fx_orfan`;

-- === 000_demo — COLUMN MODIFY ===
-- [id 38] fx_modif.Cod
ALTER TABLE `000_demo`.`fx_modif` MODIFY COLUMN `Cod` varchar(40) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL;
-- [id 39] fx_parent.Cod
ALTER TABLE `000_demo`.`fx_parent` MODIFY COLUMN `Cod` varchar(40) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL;

-- === 000_demo — COLLATION MODIFY ===
-- [id 40] fx_angajamente.CodAngajament
ALTER TABLE `000_demo`.`fx_angajamente` MODIFY COLUMN `CodAngajament` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL;
-- [id 41] fx_angajamente.Denumire
ALTER TABLE `000_demo`.`fx_angajamente` MODIFY COLUMN `Denumire` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL;
-- [id 42] fx_copil.CodAngajament
ALTER TABLE `000_demo`.`fx_copil` MODIFY COLUMN `CodAngajament` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL;

-- === 000_demo — FK CREATE ===
-- [id 46] fx_copil.fk_copil_ang
ALTER TABLE `000_demo`.`fx_copil` ADD CONSTRAINT `fk_copil_ang` FOREIGN KEY (`CodAngajament`) REFERENCES `000_demo`.`fx_angajamente` (`CodAngajament`) ON DELETE NO ACTION ON UPDATE NO ACTION;
-- [id 47] fx_modif.fk_modif_parent
ALTER TABLE `000_demo`.`fx_modif` ADD CONSTRAINT `fk_modif_parent` FOREIGN KEY (`Cod`) REFERENCES `000_demo`.`fx_parent` (`Cod`) ON DELETE NO ACTION ON UPDATE NO ACTION;
