-- SLICE-ADE3-02: run only after confirming this common schema and DC.
-- No example series or number is seeded. Import CFGs/CH per unit explicitly.
CREATE TABLE IF NOT EXISTS AVACONT_COMUN.Unitati_Chitante (
  DC VARCHAR(32) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL PRIMARY KEY,
  Serie VARCHAR(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  Numar INT NOT NULL,
  Explicatie VARCHAR(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  Version BIGINT NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;
