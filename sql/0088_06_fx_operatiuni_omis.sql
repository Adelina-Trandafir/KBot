-- =====================================================================================
-- Slice 0088-06 -- «Omite randul»: an ERR operation the operator took out of the correction
-- notes (made by hand in FOREXE, no documents in K-BOT).
--
--   FX_Operatiuni + Omis: 0 = asked for in «Operatiuni necorelate» (default, every existing row);
--                         1 = omitted: not in that window, not in the ribbon / menu count, until the
--                         operator brings it back from «Operatiuni omise» («Reia randul»).
--
-- The login still inserts new rows with Omis = 0; a row already there (same ReferintaTrezor +
-- NrDoc) is never re-inserted, so an omitted row stays omitted.
--
-- APPLY ON EVERY UNIT DATABASE (and on AVACONT_SURSA). Until it ran, routes/forexe/operatiuni.py
-- keeps working (nothing is hidden) and answers 409 to «Omite randul».
-- =====================================================================================

ALTER TABLE `FX_Operatiuni`
  ADD COLUMN IF NOT EXISTS `Omis` tinyint(1) NOT NULL DEFAULT 0
  COMMENT '0088-06: 1 = the operator omitted this operation from the correction notes';
