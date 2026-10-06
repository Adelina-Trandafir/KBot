-- =====================================================================================
-- Slice 00EF-06 -- the first invoice number of the issuer. RUN ONLY IF 00EF_02_efactura_unitate.sql WAS ALREADY RUN
-- (a database made from the file as it is now already has the column). NOT RUN anywhere yet. Safe to run again.
--
-- Why: Access took the first number from Scheme.C5 of the row NumeForm='DPIFV' (DMAX_EFACTURA: «the highest
-- NumarFactura, or C5 when there is none»). 00EF-02 kept the series (T3) and the received-invoice switch (C2) but
-- not C5, so a unit that starts a series at 245 had nowhere to say so. The server now numbers a new invoice as
-- the highest number of the series + 1, or NumarInitial when the series has no invoice yet.
--
-- ORDER (the same two steps as 00EF-02):
--   1. this file, on AVACONT_SURSA (the template);
--   2. AvacontPush, tab «Sincronizare schema»: SAFE. If that tab does not add a missing COLUMN to existing tables,
--      run the statement below on each unit database that already has EF_Furnizor.
-- Then the operator sets the value with PUT /api/efactura/furnizor (field NumarInitial); the Migrator tab
-- «E-Factura» does NOT import Scheme.C5 yet (open thread of 00EF-06).
-- =====================================================================================

USE `AVACONT_SURSA`;

ALTER TABLE `EF_Furnizor`
  ADD COLUMN IF NOT EXISTS `NumarInitial` int(11) NOT NULL DEFAULT 1
  COMMENT 'Slice 00EF-06: first invoice number of a series that has no invoice yet (was Scheme.C5 of DPIFV)'
  AFTER `SerieFactura`;

-- Check afterwards:  SHOW COLUMNS FROM EF_Furnizor LIKE 'NumarInitial';   -- one row, int(11), default 1
