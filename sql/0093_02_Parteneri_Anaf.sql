-- Slice 0093-02: which partners were synchronised with ANAF ("Sincronizeaza ANAF" in the Parteneri window).
-- A partner WITHOUT a row here is "nesincronizat". Run on every K-BOT unit database (AVACONT_SURSA and 000_DEMO),
-- through the admin application. New table: CreatedAt + UpdatedAt as the project rule says.
CREATE TABLE IF NOT EXISTS `Parteneri_Anaf` (
  `IdPartener` int(11) NOT NULL,
  `DataSincronizare` datetime NOT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT current_timestamp(),
  `UpdatedAt` datetime NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`IdPartener`) USING BTREE,
  CONSTRAINT `Parteneri_Anaf_ibfk_1` FOREIGN KEY (`IdPartener`) REFERENCES `Parteneri` (`IdPartener`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;
