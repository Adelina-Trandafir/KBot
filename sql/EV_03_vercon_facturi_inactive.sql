-- Executare MANUALĂ pe AVACONT_SURSA, apoi sincronizarea structurii în DC-uri.
-- Nu se execută pe AVACONT_COMUN. Necesită EV_01_primite_vercon.sql.
-- Marcaj GLOBAL pentru unitate, comun tuturor gestionarilor/subunităților,
-- independent de gestiune și exercițiu. Debifarea INACTIV restaurează factura.
CREATE TABLE IF NOT EXISTS EF_VERCON_VIZIBILITATE (
  ID_TRIMITERE INT(11) NOT NULL,
  INACTIV TINYINT(1) NOT NULL DEFAULT 0,
  MODIFICAT_DE VARCHAR(255) NOT NULL,
  MODIFICAT_LA DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (ID_TRIMITERE),
  CONSTRAINT FK_EV_VIZ_TRIMITERE FOREIGN KEY (ID_TRIMITERE)
    REFERENCES EF_PrimiteVercon (IdTrimitere) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;
