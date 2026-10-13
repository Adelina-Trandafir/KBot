-- SLICE-ADE9-04: run MANUALLY in each ADECHIT database, after AD_05_subunitati.sql.
-- The application never executes this script or creates this table automatically.
-- Operators configure the note per subunit in the ADECHIT menu.
-- Without a saved row the application uses its default editable template.
CREATE TABLE IF NOT EXISTS AD_REPORT_CONFIG (
  SUBUNIT_ID INT NOT NULL,
  REPORT_KEY VARCHAR(64) NOT NULL,
  FOOTER_HTML MEDIUMTEXT NOT NULL,
  VERSION BIGINT NOT NULL DEFAULT 1,
  PRIMARY KEY (SUBUNIT_ID, REPORT_KEY),
  CONSTRAINT FK_AD_REPORT_CONFIG_SUBUNIT FOREIGN KEY (SUBUNIT_ID)
    REFERENCES AD_Subunits (SubunitId) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;
