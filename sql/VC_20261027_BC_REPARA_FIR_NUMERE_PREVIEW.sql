-- Read-only look at what VC_20261027_BC_REPARA_FIR_NUMERE.sql would change. Changes NOTHING: run it in any SQL client on the database you mean.
-- Write the two ids, then run the SET lines and the SELECT together. Same rules as the repair: consumption bons (not returns),
-- in the order of IdBonConsum, per account when the gestiune numbers per account (NUM_BC_PE_CONT = 1), the first bon of a series keeps
-- its number, every next one = previous + 1.
SET @bcr_subunit = NULL;    -- IdSubunitate (VC_Subunitati.SubunitId)
SET @bcr_gestiune = NULL;   -- IdGestiune (VC_Gestiuni.GestiuneId)

WITH base AS (
  SELECT h.IdBonConsum, h.SubunitId, h.GestiuneId, h.ExercitiuId, h.IdMasina, h.DataBC, h.Anulat, h.NumarBC AS OldText,
         CASE WHEN COALESCE(s.IntValue, 0) = 1 THEN COALESCE(h.IdMasina, 0) ELSE 0 END AS Scope,
         CAST(REGEXP_REPLACE(h.NumarBC, '[^0-9]', '') AS UNSIGNED) AS OldNumber
  FROM VC_Iesiri_H h
  LEFT JOIN VC_Settings s ON s.SubunitId = h.SubunitId AND s.GestiuneId = h.GestiuneId AND s.SettingKey = 'NUM_BC_PE_CONT'
  WHERE h.SubunitId = @bcr_subunit AND h.GestiuneId = @bcr_gestiune
    AND COALESCE(h.Retur, 0) = 0 AND h.NumarBC REGEXP '^ *(BC)? *[0-9]+ *$'
) , numbered AS (
  SELECT b.*, FIRST_VALUE(b.OldNumber) OVER w + ROW_NUMBER() OVER w - 1 AS NewNumber
  FROM base b
  WINDOW w AS (PARTITION BY b.SubunitId, b.GestiuneId, b.ExercitiuId, b.Scope ORDER BY b.IdBonConsum)
)
SELECT e.An, m.Masina AS Cont, p.IdBonConsum, DATE(p.DataBC) AS Data, p.Anulat,
       p.OldText AS Numar_acum, CONCAT('BC ', p.NewNumber) AS Numar_nou,
       IF(p.OldText <=> CONCAT('BC ', p.NewNumber), '', 'SCHIMBAT') AS Stare
FROM numbered p
JOIN VC_Exercitii e ON e.SubunitId = p.SubunitId AND e.GestiuneId = p.GestiuneId AND e.ExercitiuId = p.ExercitiuId
LEFT JOIN VC_Masini m ON m.SubunitId = p.SubunitId AND m.GestiuneId = p.GestiuneId AND m.IdMasina = p.IdMasina
ORDER BY e.An, p.Scope, m.Masina, p.IdBonConsum;
