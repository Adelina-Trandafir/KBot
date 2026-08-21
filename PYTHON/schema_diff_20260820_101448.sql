-- ------------------------------------------------------------
-- schema_diff — 11 instrucțiuni, în ordinea execuției
-- generat: 2026-08-20T10:14:48
-- ------------------------------------------------------------


-- === 000_demo — FK DROP   *** DISTRUCTIV *** ===
-- [id 1] fx_copil.fk_copil_extra
ALTER TABLE `000_demo`.`fx_copil` DROP FOREIGN KEY `fk_copil_extra`;
-- [id 2] fx_copil.fk_copil_ang
ALTER TABLE `000_demo`.`fx_copil` DROP FOREIGN KEY `fk_copil_ang`;
-- [id 3] fx_modif.fk_modif_parent
ALTER TABLE `000_demo`.`fx_modif` DROP FOREIGN KEY `fk_modif_parent`;

-- === 000_demo — TABLE DROP   *** DISTRUCTIV *** ===
-- [id 4] fx_orfan
DROP TABLE `000_demo`.`fx_orfan`;

-- === 000_demo — COLUMN MODIFY ===
-- [id 5] fx_modif.Cod
ALTER TABLE `000_demo`.`fx_modif` MODIFY COLUMN `Cod` varchar(40) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL;
-- [id 6] fx_parent.Cod
ALTER TABLE `000_demo`.`fx_parent` MODIFY COLUMN `Cod` varchar(40) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL;

-- === 000_demo — COLLATION MODIFY ===
-- [id 7] fx_angajamente.CodAngajament
ALTER TABLE `000_demo`.`fx_angajamente` MODIFY COLUMN `CodAngajament` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL;
-- [id 8] fx_angajamente.Denumire
ALTER TABLE `000_demo`.`fx_angajamente` MODIFY COLUMN `Denumire` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL;
-- [id 9] fx_copil.CodAngajament
ALTER TABLE `000_demo`.`fx_copil` MODIFY COLUMN `CodAngajament` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL;

-- === 000_demo — FK CREATE ===
-- [id 10] fx_copil.fk_copil_ang
ALTER TABLE `000_demo`.`fx_copil` ADD CONSTRAINT `fk_copil_ang` FOREIGN KEY (`CodAngajament`) REFERENCES `000_demo`.`fx_angajamente` (`CodAngajament`) ON DELETE NO ACTION ON UPDATE NO ACTION;
-- [id 11] fx_modif.fk_modif_parent
ALTER TABLE `000_demo`.`fx_modif` ADD CONSTRAINT `fk_modif_parent` FOREIGN KEY (`Cod`) REFERENCES `000_demo`.`fx_parent` (`Cod`) ON DELETE NO ACTION ON UPDATE NO ACTION;
