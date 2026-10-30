-- VC_OI_Produse: the register row of a write-off (BS) has a negative quantity, and its value must be negative too (oi.py writes
-- both with the same sign). A row migrated from Access kept a positive value (IDOI 5992 in 047_GR47: quantity -1, value +67.83).
-- Only rows of a BS (no BP, no BT marker) with negative quantity and positive value are touched; the transfer rows were done by
-- VC_OI_semn_valoare_transfer.sql. Running it again changes nothing. Version is bumped like every register edit.
UPDATE VC_OI_Produse
SET ValoareCUTVA = -ABS(ValoareCUTVA), Version = Version + 1
WHERE COALESCE(IDBP, 0) = 0 AND COALESCE(IDBT, 0) = 0 AND Cant < 0 AND ValoareCUTVA > 0;
