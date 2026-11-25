-- SLICE-FS02-01. Se aplică de administratorul MariaDB pe serverul AVACONT, o singură dată, pe o bază FISCOLINO creată cu schema veche (cu coloana DC).
-- Codul fiscal devine cheia unică a unității; coloanele DC dispar. Datele se păstrează.
-- Se aplică ÎMPREUNĂ cu codul nou (fiscolino_store.py / fiscolino_tokens.py / fiscolino_routes.py): codul vechi citește DC.
-- Pe o bază nouă se rulează sql/SLICE-FS02-01_fiscolino_efactura.sql, nu acest fișier.
USE `FISCOLINO`;

-- 1. Verificare: trebuie să returneze 0 rânduri (CIF-ul din EF_TOKEN este CF-ul unității). Dacă returnează rânduri, nu se continuă.
SELECT t.`DC`, t.`CIF`, u.`CF` FROM `EF_TOKEN` t JOIN `UNITATI` u ON u.`DC` = t.`DC` WHERE t.`CIF` <> u.`CF`;

-- 2. Legăturile spre UNITATI(DC) se scot (nume generate automat; dacă una lipsește, se verifică cu SHOW CREATE TABLE).
ALTER TABLE `UNITATI_UTILIZATORI` DROP FOREIGN KEY `UNITATI_UTILIZATORI_ibfk_1`;
ALTER TABLE `EF_TOKEN_START`      DROP FOREIGN KEY `EF_TOKEN_START_ibfk_1`;
ALTER TABLE `EF_TOKEN`            DROP FOREIGN KEY `EF_TOKEN_ibfk_1`;

-- 3. UNITATI_UTILIZATORI: (UN, DC) devine (UN, CF).
ALTER TABLE `UNITATI_UTILIZATORI` ADD COLUMN `CF` VARCHAR(13) CHARACTER SET ascii COLLATE ascii_bin NULL AFTER `UN`;
UPDATE `UNITATI_UTILIZATORI` A JOIN `UNITATI` U ON U.`DC` = A.`DC` SET A.`CF` = U.`CF`;
ALTER TABLE `UNITATI_UTILIZATORI`
    DROP PRIMARY KEY,
    DROP COLUMN `DC`,
    MODIFY `CF` VARCHAR(13) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    ADD PRIMARY KEY (`UN`, `CF`);

-- 4. EF_TOKEN_START: rândurile trăiesc câteva minute, se golesc; CIF este deja codul fiscal.
DELETE FROM `EF_TOKEN_START`;
ALTER TABLE `EF_TOKEN_START` DROP COLUMN `DC`;

-- 5. EF_TOKEN: cheia devine CIF.
ALTER TABLE `EF_TOKEN` DROP PRIMARY KEY, DROP COLUMN `DC`, ADD PRIMARY KEY (`CIF`);

-- 6. UNITATI: CF devine cheia, DC dispare.
ALTER TABLE `UNITATI` DROP PRIMARY KEY, DROP INDEX `UX_UNITATI_CF`, DROP COLUMN `DC`, ADD PRIMARY KEY (`CF`);

-- 7. Legăturile noi, pe codul fiscal.
ALTER TABLE `UNITATI_UTILIZATORI` ADD CONSTRAINT `FK_UU_UNITATI`   FOREIGN KEY (`CF`)  REFERENCES `UNITATI` (`CF`) ON UPDATE CASCADE ON DELETE CASCADE;
ALTER TABLE `EF_TOKEN_START`      ADD CONSTRAINT `FK_EFTS_UNITATI` FOREIGN KEY (`CIF`) REFERENCES `UNITATI` (`CF`) ON UPDATE CASCADE ON DELETE CASCADE;
ALTER TABLE `EF_TOKEN`            ADD CONSTRAINT `FK_EFT_UNITATI`  FOREIGN KEY (`CIF`) REFERENCES `UNITATI` (`CF`) ON UPDATE CASCADE ON DELETE CASCADE;
