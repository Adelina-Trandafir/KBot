-- 00EF-56: a line of a received invoice can be marked «NU SE TRIMITE» when the invoice is sent to VERCON. Run by hand ONLY on AVACONT_SURSA,
-- then propagate with the schema sync to the unit databases (same procedure as EV_01_primite_vercon.sql). Run it BEFORE the new Python is deployed:
-- the VERCON queue and the association read this column.
--
-- NuSeTrimite = 1: the row of the sending is kept (K-BOT still shows the line and the choice can be changed while the sending waits), but VERCON never
-- reads the line: not in «Adaugare NIR», not in the association with a NIR, not in the quantities that decide PRELUATA. The line itself stays in
-- EF_PrimiteLinii (it stays in e-Factura). Existing rows get 0 = they went, as before.
ALTER TABLE `EF_PrimiteVerconLinii`
  ADD COLUMN IF NOT EXISTS `NuSeTrimite` tinyint(1) NOT NULL DEFAULT 0 COMMENT '1 = marked NU SE TRIMITE: VERCON does not see the line' AFTER `IdMasina`;
