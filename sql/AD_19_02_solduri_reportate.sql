-- SLICE-AD19-02. Run manually on AVACONT_SURSA, then propagate the schema
-- to the ADECHIT unit databases through the existing schema synchronization.
-- No seed rows and no changes to AD_Platitori.SI or saved situations.
-- New application and migrator versions must be deployed after the table exists
-- in every unit where carried balances will be transferred.
CREATE TABLE `AD_SOLDURI_REPORTATE` (
    `SUBUNIT_ID` INT NOT NULL,
    `IDP` INT NOT NULL,
    `AN_START` SMALLINT UNSIGNED NOT NULL,
    `SOLD` BIGINT NOT NULL,
    `SOURCE_IDP` INT NOT NULL,
    `SOURCE_HASH` CHAR(64) NOT NULL,
    `SOURCE_FILE` VARCHAR(255) NOT NULL,
    `VERSION` BIGINT NOT NULL DEFAULT 1,
    `CREATED_AT` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UPDATED_AT` DATETIME NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`SUBUNIT_ID`, `IDP`, `AN_START`),
    CONSTRAINT `FK_AD_SOLDURI_REPORTATE_COPIL`
        FOREIGN KEY (`SUBUNIT_ID`, `IDP`) REFERENCES `AD_Platitori` (`SubunitId`, `IDP`)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT `CK_AD_SOLDURI_REPORTATE_AN` CHECK (`AN_START` BETWEEN 1900 AND 9999)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;
