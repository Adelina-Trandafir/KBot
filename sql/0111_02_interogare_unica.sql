-- =====================================================================================
-- ONE-TIME QUERY  0111_total_orig_si_valoare_orig        (slice 0111)
--
-- NOT run by hand. Paste this text into AvacontPush, tab «Interogari unice», name it
-- `0111_total_orig_si_valoare_orig`, press «Vezi (nu executa)» and then «Executa». The server runs it
-- once on AVACONT_SURSA and once on every unit database, each in its own database (so the table
-- names below are NOT qualified), and writes it in each database's `Interogari_Unice`.
--
-- BEFORE it: `0111_01_sursa.sql` on AVACONT_SURSA, a schema sync on the units, and the new Python
-- files on the server (a header born between this query and that deploy would keep a NULL
-- TotalOrig). The query itself touches only TotalOrig / ValoareOrig, which already exist.
--
-- WHAT IT DOES, on each database: writes ONCE the value FOREXE gave (the working value as it is
-- today) into the original column, only where the original is still NULL.
--   * FX_Receptii_H.TotalOrig  <- Total      (K-BOT never wrote it);
--   * FX_Receptii.ValoareOrig  <- Valoare    (written at birth by K-BOT, filled by Access before).
-- `DTQ = DTQ` on FX_Receptii is written ON PURPOSE: DTQ is ON UPDATE CURRENT_TIMESTAMP, and a column
-- the UPDATE names itself is not touched by the automatic update, so every row keeps its date.
-- FX_Receptii_H has no DTQ.
--
-- CHECKED 06.10.2026 on every database (count query of the operator): no row has an original that
-- differs from its working value, so nothing shows as «corrected» after this query.
-- Safe to run again: it writes only where the original is NULL.
-- =====================================================================================

UPDATE `FX_Receptii_H` SET `TotalOrig` = `Total` WHERE `TotalOrig` IS NULL;

UPDATE `FX_Receptii` SET `ValoareOrig` = `Valoare`, `DTQ` = `DTQ` WHERE `ValoareOrig` IS NULL;
