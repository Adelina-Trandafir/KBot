-- 00EF-55: the partner chosen for a supplier of the received e-invoices. Run by hand ONLY on AVACONT_SURSA, then propagate the structure with the schema
-- sync to the unit databases (same procedure as 00EF_17_primite_asocieri.sql / EV_01_primite_vercon.sql).
--
-- The supplier of a received invoice is found among the partners by TAX CODE ONLY. When several partners have the same tax code the operator chooses ONE
-- (sending to Vercon, booking in «Contari»); this table keeps the choice, so it is not asked again. A supplier with a single partner needs no row: the server
-- uses that partner, and writes the row the first time it is used. A row whose partner no longer exists (or no longer has that tax code) is ignored and the
-- operator is asked again. No foreign key on purpose: a refill of Parteneri must never be blocked or silently erase the choices; the row is checked when read.
--
-- CuiNormalizat = the tax code the way utils.vercon_suppliers.code() writes it (letters and digits, upper case, no leading «RO» before a digit).
CREATE TABLE IF NOT EXISTS `EF_FurnizorPartener` (
  `CuiNormalizat` varchar(32) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `IdPartener` int(11) NOT NULL COMMENT 'Parteneri.IdPartener chosen for the supplier with this tax code',
  `UnAles` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'who chose it (or whose action linked the only partner)',
  `CreatedAt` datetime NOT NULL DEFAULT current_timestamp(),
  `UpdatedAt` datetime NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`CuiNormalizat`) USING BTREE,
  INDEX `IX_EF_FurnizorPartener_IdPartener`(`IdPartener` ASC) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;
