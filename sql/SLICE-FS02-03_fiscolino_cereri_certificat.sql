-- SLICE-FS02-03. Se aplică de administratorul MariaDB pe serverul AVACONT, după SLICE-FS02-01 (și FS02-02), o singură dată.
-- Butonul „Trimite cerere Înregistrare” din FISCOLINO inserează în UTILIZATORI_CERTIFICATE un rând cu VALIDAT = 0 și UN = adresa de email a solicitantului.
-- Datele certificatului le citește serverul din handshake-ul TLS (nu le trimite aplicația). Un certificat devine utilizabil
-- abia după ce administratorul îi setează UN și VALIDAT = 1.
USE `FISCOLINO`;

-- Certificatele existente rămân validate. Rândurile noi pornesc nevalidate (VALIDAT = 0), deci un INSERT manual
-- trebuie să seteze explicit VALIDAT = 1.
ALTER TABLE `UTILIZATORI_CERTIFICATE`
    MODIFY COLUMN `UN` VARCHAR(255) NULL,
    ADD COLUMN IF NOT EXISTS `VALIDAT` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '0 = cerere în așteptare, 1 = validat de administrator' AFTER `ACTIV`,
    ADD COLUMN IF NOT EXISTS `SUBIECT` VARCHAR(400) NULL COMMENT 'Subiectul certificatului, din handshake-ul TLS',
    ADD COLUMN IF NOT EXISTS `EMITENT` VARCHAR(400) NULL,
    ADD COLUMN IF NOT EXISTS `VALID_PANA` DATETIME NULL COMMENT 'UTC',
    ADD COLUMN IF NOT EXISTS `TOKEN_INFO` VARCHAR(255) NULL COMMENT 'Etichetă / model / serie token, trimise de aplicație (informativ)',
    ADD COLUMN IF NOT EXISTS `CERERE_LA` DATETIME NULL COMMENT 'UTC';

-- Doar dacă VALIDAT tocmai a fost adăugată: rândurile care existau deja sunt validate.
UPDATE `UTILIZATORI_CERTIFICATE` SET `VALIDAT` = 1 WHERE `CERERE_LA` IS NULL AND `UN` IS NOT NULL AND `VALIDAT` = 0;

-- Contul serviciului poate crea doar cereri: coloanele VALIDAT, UN și ACTIV nu intră în dreptul de INSERT, deci o cerere
-- pornește cu VALIDAT = 0, UN NULL. Înlocuiți <FISCOLINO_DB_USER> (pentru FISCOLINO: 'Fiscolino'@'%', cu gazda reală a contului).
-- SELECT pe tabel există deja din SLICE-FS02-01.
-- UN este adresa de email a solicitantului (obligatorie la cerere); administratorul o verifică înainte să seteze VALIDAT = 1.
GRANT INSERT (`AMPRENTA`, `UN`, `SUBIECT`, `EMITENT`, `VALID_PANA`, `TOKEN_INFO`, `CERERE_LA`)
    ON `FISCOLINO`.`UTILIZATORI_CERTIFICATE` TO '<FISCOLINO_DB_USER>'@'%';

-- Validarea unei cereri (administratorul): setează contul și VALIDAT.
--   UPDATE `UTILIZATORI_CERTIFICATE` SET `UN` = '<utilizator>', `VALIDAT` = 1 WHERE `AMPRENTA` = '<amprenta>';
-- Respingerea: DELETE FROM `UTILIZATORI_CERTIFICATE` WHERE `AMPRENTA` = '<amprenta>' AND `VALIDAT` = 0;
-- Cererile în așteptare:
--   SELECT `AMPRENTA`, `SUBIECT`, `EMITENT`, `TOKEN_INFO`, `CERERE_LA` FROM `UTILIZATORI_CERTIFICATE` WHERE `VALIDAT` = 0;
