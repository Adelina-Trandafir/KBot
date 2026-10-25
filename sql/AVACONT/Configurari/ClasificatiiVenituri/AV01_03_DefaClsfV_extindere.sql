-- AV01-03: DefaClsfV gets the split code (Capitol / Subcapitol / Paragraf) and the VN/CH type. Extend, NOT replace.
-- Database: AVACONT_COMUN. Run by hand in Navicat, statement by statement (select one, run it).
-- Nothing reads DefaClsfV in the code today (checked 24.10.2026); ClsfV and DENUMIREA are left untouched.
-- Source: AVACONT/EXPORT_VBA/tabledata/DefaClsfV.csv (columns ID, C, S, P, D, Field5, CV, Field7), imported by hand
--   into a table named DefaClsfV_Temp. Field5 and Field7 are NOT used (internal ministry keys, operator's decision).
-- Existing denominations (with diacritics) are kept. Drop DefaClsfV_Temp when done.

-- 1) DDL: the missing columns. The three code parts are GENERATED from ClsfV, so they can never disagree with it.
ALTER TABLE `DefaClsfV`
  ADD COLUMN IF NOT EXISTS `Capitol`    char(2) GENERATED ALWAYS AS (LEFT(`ClsfV`, 2)) PERSISTENT,
  ADD COLUMN IF NOT EXISTS `Subcapitol` char(2) GENERATED ALWAYS AS (SUBSTRING(`ClsfV`, 3, 2)) PERSISTENT,
  ADD COLUMN IF NOT EXISTS `Paragraf`   char(2) GENERATED ALWAYS AS (SUBSTRING(`ClsfV`, 5, 2)) PERSISTENT,
  ADD COLUMN IF NOT EXISTS `Tip`        char(2) NULL DEFAULT NULL COMMENT 'VN = revenue, CH = expense';

-- 2) CHECK before the update (read only). Navicat may import C / S / P as numbers and drop the leading zero
--    ("01" -> 1), so the code is rebuilt with LPAD. Expected: Matched ~1403, only in Temp = 1 (420700),
--    only in DefaClsfV = 26 (these stay without Tip).
--    2a) How many codes match.
SELECT COUNT(*) AS `Matched`
FROM (SELECT CONVERT(CONCAT(LPAD(`C`, 2, '0'), LPAD(`S`, 2, '0'), LPAD(`P`, 2, '0')) USING utf8mb3) AS `Cod` FROM `DefaClsfV_Temp`) T
JOIN `DefaClsfV` D ON D.`ClsfV` = T.`Cod`;
--    2b) Codes only in the CSV (will be inserted by step 4).
SELECT T.`Cod` AS `OnlyInTemp`, T.`D`
FROM (SELECT CONVERT(CONCAT(LPAD(`C`, 2, '0'), LPAD(`S`, 2, '0'), LPAD(`P`, 2, '0')) USING utf8mb3) AS `Cod`, `D` FROM `DefaClsfV_Temp`) T
LEFT JOIN `DefaClsfV` D ON D.`ClsfV` = T.`Cod`
WHERE D.`ClsfV` IS NULL;
--    2c) Codes only in DefaClsfV (stay without Tip).
SELECT D.`ClsfV` AS `OnlyInDefaClsfV`, D.`DENUMIREA`
FROM `DefaClsfV` D
LEFT JOIN (SELECT CONVERT(CONCAT(LPAD(`C`, 2, '0'), LPAD(`S`, 2, '0'), LPAD(`P`, 2, '0')) USING utf8mb3) AS `Cod` FROM `DefaClsfV_Temp`) T
       ON D.`ClsfV` = T.`Cod`
WHERE T.`Cod` IS NULL;

-- 3) UPDATE: Tip (VN / CH) from the CSV, matched on the 6-character code.
UPDATE `DefaClsfV` D
JOIN (SELECT CONVERT(CONCAT(LPAD(`C`, 2, '0'), LPAD(`S`, 2, '0'), LPAD(`P`, 2, '0')) USING utf8mb3) AS `Cod`, TRIM(`CV`) AS `CV`
      FROM `DefaClsfV_Temp`) T ON D.`ClsfV` = T.`Cod`
SET D.`Tip` = T.`CV`;

-- 4) INSERT the code(s) that exist only in the CSV (expected: 420700). Name comes from the CSV (no diacritics there).
INSERT INTO `DefaClsfV` (`ClsfV`, `DENUMIREA`, `Tip`)
SELECT T.`Cod`, T.`D`, T.`CV`
FROM (SELECT CONVERT(CONCAT(LPAD(`C`, 2, '0'), LPAD(`S`, 2, '0'), LPAD(`P`, 2, '0')) USING utf8mb3) AS `Cod`, `D`, TRIM(`CV`) AS `CV`
      FROM `DefaClsfV_Temp`) T
LEFT JOIN `DefaClsfV` D ON D.`ClsfV` = T.`Cod`
WHERE D.`ClsfV` IS NULL;

-- 5) CHECK after (read only): rows per Tip, and the codes left without a Tip (expected: the ~26 that are not in the CSV).
SELECT `Tip`, COUNT(*) AS `Randuri` FROM `DefaClsfV` GROUP BY `Tip`;
SELECT `ClsfV`, `DENUMIREA` FROM `DefaClsfV` WHERE `Tip` IS NULL ORDER BY `ClsfV`;
