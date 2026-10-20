-- Empties the E-Factura transfer of ONE unit (027_SCGM) so the migrator can copy it from Access again.
-- Run by hand on the unit database only. Nothing outside the EF_ tables is touched:
--   * the receptions themselves (FX_Receptii, FX_Receptii_H, FX_Receptii_R) stay as they are;
--   * what goes is the link invoice <-> reception (EF_PrimiteReceptii) and, with the invoices, everything hanging on them.
--
-- Part 1 = RECEIVED invoices (EF_Mesaje / EF_Primite* / EF_PrimiteReceptii). Always runs.
-- Part 2 = ISSUED invoices (EF_Facturi*, EF_Clienti). Runs only if @golire_emise = 1: issued invoices may have been created in K-BOT
--          after the transfer (and sent to ANAF), and Access does not have those. Leave 0 unless that is really wanted.
--
-- Lost for good, because the transfer reads them from the Access files only: received messages/invoices downloaded through K-BOT
-- AFTER the first transfer and absent from ef_<year>.accdb, and every note / classification / link made in K-BOT on received invoices.

USE `027_SCGM`;

SET @golire_emise = 0;

-- before
SELECT 'EF_PrimiteReceptii' AS tabel, COUNT(*) AS randuri FROM `EF_PrimiteReceptii`
UNION ALL SELECT 'EF_PrimiteAsocieri', COUNT(*) FROM `EF_PrimiteAsocieri`
UNION ALL SELECT 'EF_PrimiteOperatii', COUNT(*) FROM `EF_PrimiteOperatii`
UNION ALL SELECT 'EF_PrimiteMesaje', COUNT(*) FROM `EF_PrimiteMesaje`
UNION ALL SELECT 'EF_PrimiteNote', COUNT(*) FROM `EF_PrimiteNote`
UNION ALL SELECT 'EF_PrimiteTVA', COUNT(*) FROM `EF_PrimiteTVA`
UNION ALL SELECT 'EF_PrimiteLinii', COUNT(*) FROM `EF_PrimiteLinii`
UNION ALL SELECT 'EF_Primite', COUNT(*) FROM `EF_Primite`
UNION ALL SELECT 'EF_Mesaje', COUNT(*) FROM `EF_Mesaje`
UNION ALL SELECT 'EF_FacturiLinii', COUNT(*) FROM `EF_FacturiLinii`
UNION ALL SELECT 'EF_Facturi', COUNT(*) FROM `EF_Facturi`
UNION ALL SELECT 'EF_Clienti', COUNT(*) FROM `EF_Clienti`;

START TRANSACTION;

-- Part 1: received (children first; the foreign keys would cascade anyway, this keeps it explicit)
DELETE FROM `EF_PrimiteReceptii`;   -- the reception links (the receptions stay)
DELETE FROM `EF_PrimiteAsocieri`;   -- invoice <-> DDF links
DELETE FROM `EF_PrimiteOperatii`;
DELETE FROM `EF_PrimiteMesaje`;
DELETE FROM `EF_PrimiteNote`;
DELETE FROM `EF_PrimiteTVA`;
DELETE FROM `EF_PrimiteLinii`;      -- takes the per-line classification (IdClsf) with it
DELETE FROM `EF_Primite`;
DELETE FROM `EF_Mesaje`;

-- Part 2: issued (only with @golire_emise = 1)
DELETE FROM `EF_FacturiLinii` WHERE @golire_emise = 1;
DELETE FROM `EF_Facturi`      WHERE @golire_emise = 1;
DELETE FROM `EF_Clienti`      WHERE @golire_emise = 1;

-- after (all 0 for part 1; part 2 only if it was switched on) - check, then COMMIT or ROLLBACK
SELECT 'EF_PrimiteReceptii' AS tabel, COUNT(*) AS randuri FROM `EF_PrimiteReceptii`
UNION ALL SELECT 'EF_Primite', COUNT(*) FROM `EF_Primite`
UNION ALL SELECT 'EF_Mesaje', COUNT(*) FROM `EF_Mesaje`
UNION ALL SELECT 'EF_Facturi', COUNT(*) FROM `EF_Facturi`
UNION ALL SELECT 'FX_Receptii_R (must be unchanged)', COUNT(*) FROM `FX_Receptii_R`;

COMMIT;
