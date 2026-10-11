-- Security plan S1: per-account password state, shared by every unit of the account.
-- Run ONCE on the K-BOT server BEFORE deploying the new auth routes.
-- The server's service account (the one that writes Jurnal) needs SELECT, INSERT, UPDATE on it.
-- utf8mb3 / utf8mb3_general_ci like every other table in the schema.
USE `AVACONT_COMUN`;

CREATE TABLE IF NOT EXISTS `Conturi_Securitate` (
  `UN` varchar(80) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `MustChangePassword` tinyint(1) NOT NULL DEFAULT 0,
  `PasswordChangedAt` datetime NULL DEFAULT NULL,
  `AuthVersion` int NOT NULL DEFAULT 1,
  PRIMARY KEY (`UN`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- Mark the accounts whose password is still the initial one (decided by the administrator,
-- not guessed): the SQL below, or  admin_cli.py must-change <email>.
-- INSERT INTO Conturi_Securitate (UN, MustChangePassword, AuthVersion) VALUES ('x@y.ro', 1, 2)
--   ON DUPLICATE KEY UPDATE MustChangePassword = 1, AuthVersion = AuthVersion + 1;

-- The password plugin must be active, otherwise strict_password_validation=ON enforces nothing:
--   INSTALL SONAME 'simple_password_check';
--   SET GLOBAL simple_password_check_minimal_length = 12;  -- (and persist in the server config)
--   SET GLOBAL strict_password_validation = ON;
