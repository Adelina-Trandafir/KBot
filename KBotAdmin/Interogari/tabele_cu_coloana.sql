-- @title Tabele cu o coloană
-- @description Caută în baza aleasă toate tabelele și vizualizările care au o coloană cu numele dat sau al cărei nume conține textul dat.
-- @param coloana | Coloana | text | Numele coloanei sau o parte din el, de exemplu IdUnitate sau Cod.
SELECT TABLE_NAME AS Tabel, COLUMN_NAME AS Coloana, COLUMN_TYPE AS Tip, IS_NULLABLE AS PoateFiNull, COLUMN_KEY AS Cheie
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND COLUMN_NAME LIKE CONCAT('%', :coloana, '%')
ORDER BY TABLE_NAME, ORDINAL_POSITION
