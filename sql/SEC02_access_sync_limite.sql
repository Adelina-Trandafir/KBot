-- Security plan S2: one successful Access sync per route, unit and day.
-- Run ONCE on the K-BOT server BEFORE deploying utils/access_limits.py. The server's service account
-- (the one that writes Jurnal and Conturi_Securitate) needs SELECT, INSERT, DELETE on it.
-- utf8mb3 / utf8mb3_general_ci like every other table in the schema.
USE `AVACONT_COMUN`;

CREATE TABLE IF NOT EXISTS `Access_Sync_Limite` (
  `Ruta` varchar(40) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `DC` varchar(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Zi` date NOT NULL,
  `Randuri` int NOT NULL DEFAULT 0,
  `Moment` datetime NOT NULL,
  PRIMARY KEY (`Ruta`, `DC`, `Zi`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- To let a unit sync again today (after a mistake): DELETE FROM Access_Sync_Limite WHERE DC = '000_DEMO' AND Zi = CURDATE();
-- Old rows are harmless; clean them now and then: DELETE FROM Access_Sync_Limite WHERE Zi < CURDATE() - INTERVAL 30 DAY;
