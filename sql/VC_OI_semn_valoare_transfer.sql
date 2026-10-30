-- VC_OI_Produse: the register row of a transfer (BT) on the side that GIVES has a negative quantity, and its value must be
-- negative too (oi.py writes both with the same sign). Rows migrated from Access kept a positive value there, which leaves a
-- holder with quantity 0 and a value left over. Only BT rows with negative quantity and positive value are touched;
-- running it again changes nothing (no row matches after the first run). Version is bumped like every register edit.
UPDATE VC_OI_Produse
SET ValoareCUTVA = -ABS(ValoareCUTVA), Version = Version + 1
WHERE COALESCE(IDBT, 0) <> 0 AND Cant < 0 AND ValoareCUTVA > 0;
