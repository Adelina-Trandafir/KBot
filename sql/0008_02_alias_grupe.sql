-- =====================================================================================
-- Slice 0008-02 -- Alias per angajament + groups of angajamente (FX_Angajamente_Grupe).
--
-- WHY (operator, 09.10.2026).
--   * `FX_Angajamente.Alias`: a short name the operator may give to an angajament. When the main
--     tree is filtered on a group, the row caption is the Alias (the Descriere when it has none).
--   * `FX_Angajamente_Grupe`: groups of angajamente. An angajament can be in several groups.
--     One row = one (group, angajament) pair; the group's name and colour repeat on its rows
--     (the shape the operator asked for). Angajamente are NOT per unit (no IdUnitate here);
--     only FX_Indicatori carries IdUnitate.
--
-- THE TABLE
--   IDGR            int          the group number (MAX + 1, taken by the save, inside its
--                                transaction). No AUTO_INCREMENT: it repeats on every row of
--                                a group, so it is part of the primary key, not the key itself.
--   DenumireGrupa   varchar(100) the group's name (unique among groups, checked by the route).
--   CuloareGrupa    varchar(7)   '#RRGGBB'; black by default.
--   CodAngajament   varchar(255) -> FX_Angajamente.CodAngajament (ON DELETE CASCADE: an
--                                angajament that goes away leaves its groups).
--   PRIMARY KEY (IDGR, CodAngajament): an angajament once per group.
--
-- Written / read by: PYTHON/routes/forexe/grupe.py (GET/POST /api/forexe/grupe...);
--                    Alias also read by routes/forexe/tree.py.
--
-- APPLY ON EVERY UNIT DATABASE (000_DEMO first) and on AVACONT_SURSA, the template for new
-- units, BEFORE the server files of this slice reach the VPS. Safe to run twice.
-- Until it is applied the tree keeps working (tree.py checks that the Alias column exists);
-- the groups window answers with the database error.
--
-- utf8mb3 / utf8mb3_general_ci like the rest of the K-BOT schemas. Shape of FX_Angajamente
-- read from MariaDB_Schema/000_DEMO.sql (dump of 22.09.2026).
-- =====================================================================================

ALTER TABLE `FX_Angajamente`
  ADD COLUMN IF NOT EXISTS `Alias` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL
  COMMENT 'Slice 0008-02: the name the operator gave to the angajament (NULL = none)';

CREATE TABLE IF NOT EXISTS `FX_Angajamente_Grupe` (
  `IDGR`          int(11)      NOT NULL,
  `DenumireGrupa` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `CuloareGrupa`  varchar(7)   CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT '#000000',
  `CodAngajament` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  PRIMARY KEY (`IDGR`, `CodAngajament`) USING BTREE,
  INDEX `ix_FX_Angajamente_Grupe_Cod` (`CodAngajament`) USING BTREE,
  CONSTRAINT `FX_Angajamente_Grupe__FX_Angajamente` FOREIGN KEY (`CodAngajament`)
    REFERENCES `FX_Angajamente` (`CodAngajament`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;
