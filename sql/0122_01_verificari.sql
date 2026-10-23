-- 0122-01: VERIFICARE, de lipit in HeidiSQL pe o baza de unitate (ex. 000_DEMO) si rulat dintr-o data (F9). Doar citeste, nu schimba nimic.
-- Da UN SINGUR rezultat, cu coloanele: verificare | detaliu | randuri.
--
-- Cum il citesti (dupa interogarea unica din KBotAdmin):
--   * "1 ramase cu IdPartener": ideal 0 pe SA, SB, ORD_TBL si "VC_NIR fara CodPartener". Daca nu e 0, vezi liniile "2".
--   * "2 partenerul randurilor ramase": o linie pentru fiecare partener al randurilor ramase (detaliu = unitate / cod / denumire,
--     randuri = cate randuri il folosesc). Poate avea sau nu cod fiscal in Parteneri: daca are, ruleaza interogarea
--     0122_01b_completari_codfiscal, iar daca nu, i-l dai de mana (sau il iei din CodFiscalSursa a NIR-urilor).
--   * "3 CodPartener necunoscut": ideal nicio linie. O linie = un cod (tabel: cod) care nu e codul fiscal al niciunui partener.
--   * "4 furnizori pe SA (primii 20)": informativ, ar trebui sa vezi coduri fiscale fara "RO" in fata.

SELECT '1 ramase cu IdPartener' AS verificare, 'SA' AS detaliu, COUNT(*) AS randuri
  FROM FX_DDF_REV_SA WHERE IdPartener IS NOT NULL
UNION ALL
SELECT '1 ramase cu IdPartener', 'SB', COUNT(*) FROM FX_DDF_REV_SB WHERE IdPartener IS NOT NULL
UNION ALL
SELECT '1 ramase cu IdPartener', 'ORD_TBL', COUNT(*) FROM FX_ORD_TBL WHERE IdPartener IS NOT NULL
UNION ALL
SELECT '1 ramase cu IdPartener', 'VC_NIR fara CodPartener', COUNT(*)
  FROM VC_NIR WHERE IdPartener IS NOT NULL AND CodPartener IS NULL
UNION ALL
SELECT '2 partenerul randurilor ramase',
       CONCAT('unitate ', P.IdUnitate, ' / ', P.CodPartener, ' / ', COALESCE(P.DenumirePartener, '')),
       x.n
  FROM Parteneri P
  JOIN (SELECT u.IdPartener, COUNT(*) AS n
          FROM (SELECT IdPartener FROM FX_DDF_REV_SA WHERE IdPartener IS NOT NULL
                UNION ALL SELECT IdPartener FROM FX_DDF_REV_SB WHERE IdPartener IS NOT NULL
                UNION ALL SELECT IdPartener FROM FX_ORD_TBL WHERE IdPartener IS NOT NULL
                UNION ALL SELECT IdPartener FROM VC_NIR WHERE IdPartener IS NOT NULL AND CodPartener IS NULL) u
         GROUP BY u.IdPartener) x ON x.IdPartener = P.IdPartener
UNION ALL
SELECT '3 CodPartener necunoscut', CONCAT(t.tabel, ': ', t.CodPartener), COUNT(*)
  FROM (SELECT 'SA' AS tabel, R.CodPartener
          FROM FX_DDF_REV_SA R
         WHERE R.IdPartener IS NULL AND COALESCE(R.CodPartener, '') <> ''
           AND NOT EXISTS (SELECT 1 FROM Parteneri P
                            WHERE UPPER(REGEXP_REPLACE(COALESCE(P.CodFiscal, ''), '[^A-Za-z0-9]', '')) IN (R.CodPartener, CONCAT('RO', R.CodPartener)))
        UNION ALL
        SELECT 'SB', R.CodPartener
          FROM FX_DDF_REV_SB R
         WHERE R.IdPartener IS NULL AND COALESCE(R.CodPartener, '') <> ''
           AND NOT EXISTS (SELECT 1 FROM Parteneri P
                            WHERE UPPER(REGEXP_REPLACE(COALESCE(P.CodFiscal, ''), '[^A-Za-z0-9]', '')) IN (R.CodPartener, CONCAT('RO', R.CodPartener)))
        UNION ALL
        SELECT 'ORD_TBL', R.CodPartener
          FROM FX_ORD_TBL R
         WHERE R.IdPartener IS NULL AND COALESCE(R.CodPartener, '') <> ''
           AND NOT EXISTS (SELECT 1 FROM Parteneri P
                            WHERE UPPER(REGEXP_REPLACE(COALESCE(P.CodFiscal, ''), '[^A-Za-z0-9]', '')) IN (R.CodPartener, CONCAT('RO', R.CodPartener)))) t
 GROUP BY t.tabel, t.CodPartener
UNION ALL
(SELECT '4 furnizori pe SA (primii 20)', CodPartener, COUNT(*)
   FROM FX_DDF_REV_SA WHERE COALESCE(CodPartener, '') <> ''
  GROUP BY CodPartener ORDER BY 3 DESC LIMIT 20);
