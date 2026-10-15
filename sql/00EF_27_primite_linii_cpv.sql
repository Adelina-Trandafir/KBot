-- Facturi primite: codul CPV al liniei (cbc:ItemClassificationCode listID="STI").
-- De executat manual numai pe AVACONT_SURSA, apoi de propagat structura
-- prin sincronizarea schemei catre bazele unitatilor (inclusiv DC-ul importat).
-- Cpv NULL  = linia a fost scrisa inainte de aceasta coloana (necitita, nu se coloreaza).
-- Cpv ''    = factura noua, citita, iar linia nu are cod STI (se arata cu rosu inchis).
-- Cpv <cod> = codul citit, de ex. 72415000-2.
-- Facturile primite prin migrarea din Access (KBot.Migrator, fila E-Factura) isi iau CPV-ul la import:
-- fiecare zip din EF.XML este deschis, iar codul se scrie pe linie; o linie care are deja valoare ramane cum este.

ALTER TABLE `AVACONT_SURSA`.`EF_PrimiteLinii`
    ADD COLUMN `Cpv` varchar(16) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL
        COMMENT 'CPV (ItemClassificationCode STI); NULL = not read, empty = read, none in the XML'
        AFTER `Explicatie`;
