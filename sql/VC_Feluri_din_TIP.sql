-- Corectare MANUALA pentru o gestiune deja migrata cu vechea regula IdFel.
-- Selectati baza DC in clientul SQL si completati cele doua ID-uri de mai jos.
-- Nu modifica schema si nu sterge nomenclatoare sau explicatii existente.
-- Nu este executat de aplicatie sau din aceasta conversatie.
SET @VC_SUBUNIT_ID = NULL;
SET @VC_GESTIUNE_ID = NULL;

START TRANSACTION;

INSERT INTO VC_FeluriMancare (SubunitId, GestiuneId, DenFel, Explicatie)
SELECT DISTINCT m.SubunitId, m.GestiuneId,
       CONCAT('Felul ', CAST(SUBSTRING_INDEX(TRIM(m.Tip), ' ', -1) AS UNSIGNED)), NULL
FROM (
    SELECT SubunitId, GestiuneId, Tip FROM VC_MicDejun
    WHERE SubunitId=@VC_SUBUNIT_ID AND GestiuneId=@VC_GESTIUNE_ID
    UNION ALL
    SELECT SubunitId, GestiuneId, Tip FROM VC_Dejun
    WHERE SubunitId=@VC_SUBUNIT_ID AND GestiuneId=@VC_GESTIUNE_ID
    UNION ALL
    SELECT SubunitId, GestiuneId, Tip FROM VC_Cina
    WHERE SubunitId=@VC_SUBUNIT_ID AND GestiuneId=@VC_GESTIUNE_ID
) m
WHERE LOWER(TRIM(m.Tip)) REGEXP '^felul +[0-9]+$'
  AND CAST(SUBSTRING_INDEX(TRIM(m.Tip), ' ', -1) AS UNSIGNED)>0
  AND NOT EXISTS (
      SELECT 1 FROM VC_FeluriMancare f
      WHERE f.SubunitId=m.SubunitId AND f.GestiuneId=m.GestiuneId
        AND LOWER(TRIM(f.DenFel))=LOWER(CONCAT('Felul ', CAST(SUBSTRING_INDEX(TRIM(m.Tip), ' ', -1) AS UNSIGNED)))
  );

UPDATE VC_MicDejun m
JOIN (
    SELECT SubunitId, GestiuneId, LOWER(TRIM(DenFel)) AS Tip, MIN(IDFel) AS IDFel
    FROM VC_FeluriMancare WHERE SubunitId=@VC_SUBUNIT_ID AND GestiuneId=@VC_GESTIUNE_ID
    GROUP BY SubunitId, GestiuneId, LOWER(TRIM(DenFel))
) f ON f.SubunitId=m.SubunitId AND f.GestiuneId=m.GestiuneId
   AND f.Tip=LOWER(CONCAT('Felul ', CAST(SUBSTRING_INDEX(TRIM(m.Tip), ' ', -1) AS UNSIGNED)))
SET m.IdFel=f.IDFel, m.Version=m.Version+1
WHERE m.SubunitId=@VC_SUBUNIT_ID AND m.GestiuneId=@VC_GESTIUNE_ID
  AND LOWER(TRIM(m.Tip)) REGEXP '^felul +[0-9]+$'
  AND NOT (m.IdFel <=> f.IDFel);

UPDATE VC_Dejun m
JOIN (
    SELECT SubunitId, GestiuneId, LOWER(TRIM(DenFel)) AS Tip, MIN(IDFel) AS IDFel
    FROM VC_FeluriMancare WHERE SubunitId=@VC_SUBUNIT_ID AND GestiuneId=@VC_GESTIUNE_ID
    GROUP BY SubunitId, GestiuneId, LOWER(TRIM(DenFel))
) f ON f.SubunitId=m.SubunitId AND f.GestiuneId=m.GestiuneId
   AND f.Tip=LOWER(CONCAT('Felul ', CAST(SUBSTRING_INDEX(TRIM(m.Tip), ' ', -1) AS UNSIGNED)))
SET m.IdFel=f.IDFel, m.Version=m.Version+1
WHERE m.SubunitId=@VC_SUBUNIT_ID AND m.GestiuneId=@VC_GESTIUNE_ID
  AND LOWER(TRIM(m.Tip)) REGEXP '^felul +[0-9]+$'
  AND NOT (m.IdFel <=> f.IDFel);

UPDATE VC_Cina m
JOIN (
    SELECT SubunitId, GestiuneId, LOWER(TRIM(DenFel)) AS Tip, MIN(IDFel) AS IDFel
    FROM VC_FeluriMancare WHERE SubunitId=@VC_SUBUNIT_ID AND GestiuneId=@VC_GESTIUNE_ID
    GROUP BY SubunitId, GestiuneId, LOWER(TRIM(DenFel))
) f ON f.SubunitId=m.SubunitId AND f.GestiuneId=m.GestiuneId
   AND f.Tip=LOWER(CONCAT('Felul ', CAST(SUBSTRING_INDEX(TRIM(m.Tip), ' ', -1) AS UNSIGNED)))
SET m.IdFel=f.IDFel, m.Version=m.Version+1
WHERE m.SubunitId=@VC_SUBUNIT_ID AND m.GestiuneId=@VC_GESTIUNE_ID
  AND LOWER(TRIM(m.Tip)) REGEXP '^felul +[0-9]+$'
  AND NOT (m.IdFel <=> f.IDFel);

COMMIT;
