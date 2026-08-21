-- ------------------------------------------------------------
-- schema_diff — 12 instrucțiuni, în ordinea execuției
-- generat: 2026-08-20T10:13:39
-- ------------------------------------------------------------


-- === 000_demo — FK DROP   *** DISTRUCTIV *** ===
-- [id 12] fx_copil.fk_copil_ang
ALTER TABLE `000_demo`.`fx_copil` DROP FOREIGN KEY `fk_copil_ang`;

-- === 000_demo — PK MODIFY   *** DISTRUCTIV *** ===
-- [id 13] fx_cap.PRIMARY
ALTER TABLE `000_demo`.`fx_cap` DROP PRIMARY KEY, ADD PRIMARY KEY (`Cod`);

-- === 000_demo — COLUMN DROP   *** DISTRUCTIV *** ===
-- [id 14] fx_cap.Alt
ALTER TABLE `000_demo`.`fx_cap` DROP COLUMN `Alt`;

-- === 000_demo — COLUMN ADD ===
-- [id 15] fx_cap.Cod
ALTER TABLE `000_demo`.`fx_cap` ADD COLUMN `Cod` int(11) NOT NULL;
-- [id 16] fx_contracte.IdPartener
ALTER TABLE `000_demo`.`fx_contracte` ADD COLUMN `IdPartener` int(11) DEFAULT NULL;

-- === 000_demo — COLLATION MODIFY ===
-- [id 17] fx_angajamente.CodAngajament
ALTER TABLE `000_demo`.`fx_angajamente` MODIFY COLUMN `CodAngajament` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL;
-- [id 18] fx_angajamente.Denumire
ALTER TABLE `000_demo`.`fx_angajamente` MODIFY COLUMN `Denumire` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL;
-- [id 19] fx_copil.CodAngajament
ALTER TABLE `000_demo`.`fx_copil` MODIFY COLUMN `CodAngajament` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL;

-- === 000_demo — INDEX CREATE ===
-- [id 20] fx_contracte.ix_ctr_part
CREATE INDEX `ix_ctr_part` ON `000_demo`.`fx_contracte` (`IdPartener`);

-- === 000_demo — FK CREATE ===
-- [id 21] fx_caplinii.fk_cap_linii
ALTER TABLE `000_demo`.`fx_caplinii` ADD CONSTRAINT `fk_cap_linii` FOREIGN KEY (`Cod`) REFERENCES `000_demo`.`fx_cap` (`Cod`) ON DELETE NO ACTION ON UPDATE NO ACTION;
-- [id 22] fx_contracte.fk_ctr_partener
ALTER TABLE `000_demo`.`fx_contracte` ADD CONSTRAINT `fk_ctr_partener` FOREIGN KEY (`IdPartener`) REFERENCES `avacont_comun`.`parteneri` (`IdPartener`) ON DELETE NO ACTION ON UPDATE NO ACTION;
-- [id 23] fx_copil.fk_copil_ang
ALTER TABLE `000_demo`.`fx_copil` ADD CONSTRAINT `fk_copil_ang` FOREIGN KEY (`CodAngajament`) REFERENCES `000_demo`.`fx_angajamente` (`CodAngajament`) ON DELETE NO ACTION ON UPDATE NO ACTION;
