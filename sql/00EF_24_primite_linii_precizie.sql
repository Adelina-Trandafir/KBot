-- Facturi emise si primite: cantitati, preturi si sume cu 4 zecimale.
-- De executat manual numai pe AVACONT_SURSA, apoi de propagat structura
-- prin sincronizarea schemei catre bazele unitatilor (inclusiv DC-ul importat).
-- DECIMAL(20,4) pastreaza cele 16 cifre intregi permise anterior de DECIMAL(18,2).
-- Regulile de calcul si afisare raman in cod; acest script modifica doar stocarea.
-- Folositi KBot.Migrator adaptat pentru DECIMAL(20,4).
-- Modificarea preciziei nu recupereaza zecimalele pierdute la importuri anterioare.

ALTER TABLE `AVACONT_SURSA`.`EF_FacturiLinii`
    MODIFY COLUMN `Cant` DECIMAL(20,4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `PU` DECIMAL(20,4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `Valoare` DECIMAL(20,4) NOT NULL DEFAULT 0.0000;

ALTER TABLE `AVACONT_SURSA`.`EF_Primite`
    MODIFY COLUMN `CotaTVA` DECIMAL(20,4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `TVA` DECIMAL(20,4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `Valoare` DECIMAL(20,4) NOT NULL DEFAULT 0.0000 COMMENT 'Taxable amount',
    MODIFY COLUMN `Total` DECIMAL(20,4) NOT NULL DEFAULT 0.0000 COMMENT 'Payable amount';

ALTER TABLE `AVACONT_SURSA`.`EF_PrimiteLinii`
    MODIFY COLUMN `Cant` DECIMAL(20,4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `Pret` DECIMAL(20,4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `Valoare` DECIMAL(20,4) NOT NULL DEFAULT 0.0000;

ALTER TABLE `AVACONT_SURSA`.`EF_PrimiteTVA`
    MODIFY COLUMN `CotaTVA` DECIMAL(20,4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `Baza` DECIMAL(20,4) NOT NULL DEFAULT 0.0000 COMMENT 'Taxable amount',
    MODIFY COLUMN `TVA` DECIMAL(20,4) NOT NULL DEFAULT 0.0000;
