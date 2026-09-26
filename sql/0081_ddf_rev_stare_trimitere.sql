-- Slice 0081-01: the send stage of a DDF revision (KBOT -> forexecab).
--
-- Run ONCE on EVERY unit database (000_DEMO first). Safe to run twice: IF NOT EXISTS.
--
-- Why a column and not Incarcat / Semnatura (operator decision, 25.09.2026):
--   * the import (prelucrare_pasi.py, step 3e) sets FX_DDF_REV.Incarcat = 1 by itself as soon
--     as it reads a reservation tagged «(REV:n)», so Incarcat cannot mark the send;
--   * «only A signed on the FINAL PDF» and «A signed on the interim PDF» both read Semnatura = 'A'.
--
-- Values (KBot.Domain DdfSendStage):
--   0 = not sent by K-BOT (also every revision written before this slice)
--   1 = send started and interrupted (forexecab may be half changed)
--   2 = sent, final PDF not generated yet (a new angajament's Rev 0, before «Deruleaza»)
--   3 = final PDF generated (signatures decide the rest: A,B -> director -> approved)

ALTER TABLE `FX_DDF_REV`
  ADD COLUMN IF NOT EXISTS `StareTrimitere` TINYINT NOT NULL DEFAULT 0
  COMMENT 'Slice 0081: 0 netrimis, 1 intrerupt, 2 trimis in lucru, 3 PDF final';
