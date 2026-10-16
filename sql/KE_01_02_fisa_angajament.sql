-- KE01-02: one result table with EVERYTHING the database knows about ONE angajament, read-only.
-- Purpose: hand the result to someone (or to Claude) to ask questions about the logic of an angajament, e.g. why the
-- classification does not show correctly when a received invoice is linked to receptions (KE, docs/KE_PLAN.md).
--
-- Lives on AVACONT_COMUN (ONE copy for every unit database) and takes the name of the unit database as its first argument:
--
--   CALL AVACONT_COMUN.FX_FisaAngajament('000_DEMO', '<CodAngajament>');
--
-- RUN THIS FILE BY HAND, once, on AVACONT_COMUN (the procedures are created in the current database, so USE AVACONT_COMUN first).
-- Use the mysql command-line client or HeidiSQL (both understand DELIMITER). Needs CREATE ROUTINE on AVACONT_COMUN to create it;
-- to call it: EXECUTE on it, CREATE TEMPORARY TABLES on AVACONT_COMUN, and SELECT on the unit database. It writes nothing but two
-- temporary tables that vanish with the session. NOT part of the schema sync and not for the clients: a diagnostic tool of the owner.
-- The unit database name is checked against information_schema (it must exist, hold FX_Angajamente and be visible to the caller)
-- before it is put, in backticks, into any statement.
--
-- Result: ONE table, columns  Ord | Sectiune | Tabel | Detalii.   Detalii = the row as one JSON object (every column of the
-- table; binary columns show only their length, long texts are cut at 300 characters).
-- Sectiune order:
--   DIAG          checks computed for the KE association (read these first):
--                   indicator_clasificatie       each indicator of the angajament with its classification and what looks wrong
--                   linie_factura_vs_indicator   each line of each candidate invoice against the indicators of the angajament
--                   receptie_pe_clasificatie     the value of each reception per classification, the way KE computes it
--                   legatura_ke                  the links already written (EF_PrimiteReceptii)
--   ANGAJAMENT, INDICATORI, CLASIFICATII, DDF, REZERVARI, RECEPTII, PLATI, ORDONANTARE, ISTORIC, NOTE_CAB, NUMERE, EFACTURI
--                 the raw rows of every table that holds the angajament (directly by CodAngajament, or through its DDFs,
--                 receptions, indicators or invoices).
-- Candidate invoices = linked by hand to a DDF of the angajament (EF_PrimiteAsocieri), already linked to it by KE
-- (EF_PrimiteReceptii) or whose supplier CUI is a partner of a DDF of the angajament (the same rule as the KE screen).
-- A table or column the unit database does not have is skipped (error 1146 / 1054 caught in FX_Fisa_Exec), and shown as such.
--
-- Inside the dynamic SQL the string literals use double quotes (the session's ANSI_QUOTES is switched off for the call and put back
-- at the end), and `#.` stands for the `database`. prefix of the unit database.

DELIMITER $$

-- Internal helper: runs one dynamic statement. p_n = rows affected, or -1 when a table / column is missing.
DROP PROCEDURE IF EXISTS FX_Fisa_Exec$$
CREATE PROCEDURE FX_Fisa_Exec(IN p_sql LONGTEXT, OUT p_n INT)
BEGIN
  DECLARE EXIT HANDLER FOR 1146, 1054 SET p_n = -1;
  SET p_n = 0;
  SET @fisa_sql = p_sql;
  PREPARE fisa_stmt FROM @fisa_sql;
  EXECUTE fisa_stmt;
  SET p_n = ROW_COUNT();
  DEALLOCATE PREPARE fisa_stmt;
END$$

-- Internal helper: appends the rows of ONE table of the unit database to FX_FisaTmp as JSON. p_where is a condition on the alias `t`.
DROP PROCEDURE IF EXISTS FX_Fisa_Adauga$$
CREATE PROCEDURE FX_Fisa_Adauga(IN p_db VARCHAR(64), IN p_sectiune VARCHAR(30), IN p_tabel VARCHAR(64), IN p_where TEXT)
BEGIN
  DECLARE v_cols LONGTEXT;
  DECLARE v_first VARCHAR(64);
  DECLARE v_n INT DEFAULT 0;

  IF NOT EXISTS (SELECT 1 FROM information_schema.TABLES WHERE TABLE_SCHEMA = p_db AND TABLE_NAME = p_tabel) THEN
    INSERT INTO FX_FisaTmp (Sectiune, Tabel, Detalii) VALUES (p_sectiune, p_tabel, JSON_OBJECT('lipsa', 'tabela nu exista in aceasta baza'));
  ELSE
    SET SESSION group_concat_max_len = 1048576;
    SELECT GROUP_CONCAT(
             CONCAT('''', COLUMN_NAME, ''', ',
               CASE
                 WHEN DATA_TYPE IN ('tinyblob', 'blob', 'mediumblob', 'longblob', 'binary', 'varbinary')
                   THEN CONCAT('IF(t.`', COLUMN_NAME, '` IS NULL, NULL, CONCAT(''<binar '', LENGTH(t.`', COLUMN_NAME, '`), '' octeti>''))')
                 WHEN DATA_TYPE IN ('mediumtext', 'longtext')
                   THEN CONCAT('LEFT(t.`', COLUMN_NAME, '`, 300)')
                 ELSE CONCAT('t.`', COLUMN_NAME, '`')
               END)
             ORDER BY ORDINAL_POSITION SEPARATOR ', ')
      INTO v_cols
      FROM information_schema.COLUMNS
     WHERE TABLE_SCHEMA = p_db AND TABLE_NAME = p_tabel;
    SELECT COLUMN_NAME INTO v_first
      FROM information_schema.COLUMNS
     WHERE TABLE_SCHEMA = p_db AND TABLE_NAME = p_tabel AND ORDINAL_POSITION = 1;

    CALL FX_Fisa_Exec(
      CONCAT('INSERT INTO FX_FisaTmp (Sectiune, Tabel, Detalii) SELECT ', QUOTE(p_sectiune), ', ', QUOTE(p_tabel),
             ', JSON_OBJECT(', v_cols, ') FROM `', REPLACE(p_db, '`', '``'), '`.`', p_tabel, '` t WHERE ', p_where,
             ' ORDER BY t.`', v_first, '`'),
      v_n);

    IF v_n = 0 THEN
      INSERT INTO FX_FisaTmp (Sectiune, Tabel, Detalii) VALUES (p_sectiune, p_tabel, JSON_OBJECT('randuri', 0));
    ELSEIF v_n < 0 THEN
      INSERT INTO FX_FisaTmp (Sectiune, Tabel, Detalii) VALUES (p_sectiune, p_tabel, JSON_OBJECT('omis', 'o tabela sau o coloana din conditie lipseste in aceasta baza'));
    END IF;
  END IF;
END$$

DROP PROCEDURE IF EXISTS FX_FisaAngajament$$
CREATE PROCEDURE FX_FisaAngajament(IN p_db VARCHAR(64), IN p_cod VARCHAR(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci)
fisa: BEGIN
  DECLARE v_mode VARCHAR(2000);
  DECLARE v_n   INT DEFAULT 0;
  DECLARE v_db  VARCHAR(140);   -- `database`.   (prefix of every table of the unit database)
  DECLARE v_sql LONGTEXT;
  DECLARE v_q   VARCHAR(600);   -- the quoted code
  DECLARE v_c   VARCHAR(700);   -- t.CodAngajament = <code>
  DECLARE v_ind VARCHAR(4000);  -- the CodAI of the indicators of the angajament
  DECLARE v_ddf VARCHAR(4000);  -- the IDDF of its DDFs
  DECLARE v_rr  VARCHAR(4000);  -- the IDRR of its receptions
  DECLARE v_rh  VARCHAR(4000);  -- the IDRH of the snapshots of its receptions
  DECLARE v_ist VARCHAR(4000);  -- the ID of its FX_Istoric rows
  DECLARE v_ord VARCHAR(4000);  -- the IDORDP of its payment orders
  DECLARE v_tbl VARCHAR(4000);  -- the IDORDTBLP of the order lines
  DECLARE v_prm VARCHAR(100) DEFAULT '(SELECT IdPrimita FROM FX_FisaPrim)';

  IF NOT EXISTS (SELECT 1 FROM information_schema.TABLES WHERE TABLE_SCHEMA = p_db AND TABLE_NAME = 'FX_Angajamente') THEN
    SELECT CONCAT('Baza de date ', COALESCE(p_db, '(null)'), ' nu exista, nu are tabela FX_Angajamente sau nu este vizibila contului') AS Eroare;
    LEAVE fisa;
  END IF;

  SET v_mode = @@SESSION.sql_mode;
  SET SESSION sql_mode = REPLACE(v_mode, 'ANSI_QUOTES', '');

  SET v_db  = CONCAT('`', REPLACE(p_db, '`', '``'), '`.');
  SET v_q   = QUOTE(p_cod);
  SET v_c   = CONCAT('t.CodAngajament = ', v_q);
  SET v_ind = CONCAT('(SELECT CodAI FROM ', v_db, 'FX_Indicatori WHERE CodAngajament = ', v_q, ')');
  SET v_ddf = CONCAT('(SELECT IDDF FROM ', v_db, 'FX_DDF WHERE CodAngajament = ', v_q, ')');
  SET v_rr  = CONCAT('(SELECT IDRR FROM ', v_db, 'FX_Receptii_R WHERE CodAngajament = ', v_q, ')');
  SET v_rh  = CONCAT('(SELECT IDRH FROM ', v_db, 'FX_Receptii_H WHERE CodAngajament = ', v_q, ' OR IDRR IN ', v_rr, ')');
  SET v_ist = CONCAT('(SELECT ID FROM ', v_db, 'FX_Istoric WHERE CodAngajament = ', v_q, ')');
  SET v_ord = CONCAT('(SELECT IDORDP FROM ', v_db, 'FX_ORD WHERE CodAngajament = ', v_q, ' OR IDDF IN ', v_ddf, ')');
  SET v_tbl = CONCAT('(SELECT IDORDTBLP FROM ', v_db, 'FX_ORD_TBL WHERE CodAngajament = ', v_q, ' OR IDORDP IN ', v_ord, ')');

  DROP TEMPORARY TABLE IF EXISTS FX_FisaTmp;
  DROP TEMPORARY TABLE IF EXISTS FX_FisaPrim;
  -- utf8mb4: this is only a transient buffer, and a character outside the BMP in some Alias must not make the insert fail
  CREATE TEMPORARY TABLE FX_FisaTmp (
    Ord INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Sectiune VARCHAR(30) NOT NULL,
    Tabel VARCHAR(64) NOT NULL,
    Detalii LONGTEXT CHARACTER SET utf8mb4 NULL
  );
  CREATE TEMPORARY TABLE FX_FisaPrim (IdPrimita INT NOT NULL PRIMARY KEY, Sursa VARCHAR(60) NOT NULL);

  -- the candidate invoices (see the header)
  SET v_sql = CONCAT(
    'INSERT INTO FX_FisaPrim (IdPrimita, Sursa) SELECT DISTINCT a.IdPrimita, "manual" ',
    'FROM #.EF_PrimiteAsocieri a JOIN #.FX_DDF d ON d.IDDF = a.IDDF WHERE d.CodAngajament = ', v_q,
    ' ON DUPLICATE KEY UPDATE Sursa = CONCAT(Sursa, "+manual")');
  CALL FX_Fisa_Exec(REPLACE(v_sql, '#.', v_db), v_n);
  SET v_sql = CONCAT(
    'INSERT INTO FX_FisaPrim (IdPrimita, Sursa) SELECT DISTINCT l.IdPrimita, "legatura" ',
    'FROM #.EF_PrimiteReceptii l WHERE l.CodAngajament = ', v_q,
    ' ON DUPLICATE KEY UPDATE Sursa = CONCAT(Sursa, "+legatura")');
  CALL FX_Fisa_Exec(REPLACE(v_sql, '#.', v_db), v_n);
  SET v_sql = CONCAT(
    'INSERT INTO FX_FisaPrim (IdPrimita, Sursa) SELECT pr.IdPrimita, "partener" FROM #.EF_Primite pr ',
    'WHERE EXISTS (SELECT 1 FROM #.FX_DDF_Parteneri dp JOIN #.FX_DDF d ON d.IDDF = dp.IDDF WHERE d.CodAngajament = ', v_q,
    ' AND UPPER(REGEXP_REPLACE(REGEXP_REPLACE(dp.CodFiscal, "[^[:alnum:]]", ""), "^RO([0-9])", "\\\\1")) = pr.CuiNormalizat) ',
    'ON DUPLICATE KEY UPDATE Sursa = CONCAT(Sursa, "+partener")');
  CALL FX_Fisa_Exec(REPLACE(v_sql, '#.', v_db), v_n);

  -- ===================================================================================================
  -- DIAG
  -- ===================================================================================================
  SET v_sql = CONCAT(
    'INSERT INTO FX_FisaTmp (Sectiune, Tabel, Detalii) ',
    'SELECT "DIAG", "angajament", JSON_OBJECT("probleme", "CodAngajament nu exista in FX_Angajamente (verifica codul, spatii, baza de date)") ',
    'FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM #.FX_Angajamente WHERE CodAngajament = ', v_q, ')');
  CALL FX_Fisa_Exec(REPLACE(v_sql, '#.', v_db), v_n);

  -- each indicator with its classification
  SET v_sql = CONCAT(
    'INSERT INTO FX_FisaTmp (Sectiune, Tabel, Detalii) ',
    'SELECT "DIAG", "indicator_clasificatie", JSON_OBJECT(',
      '"CodAI", i.CodAI, "CodIndicator", i.CodIndicator, "IndicatorFX", i.IndicatorFX, "NrCrt", i.NrCrt, ',
      '"IdClsf_indicator", i.IdClsf, "Clsf", c.Clsf, "Denumire", c.Denumire, "IdClsfAcc", c.IdClsfAcc, ',
      '"SS_indicator", i.SS, "SS_clasificatie", c.SS, ',
      '"IdUnitate_indicator", i.IdUnitate, "IdUnitate_clasificatie", c.IdUnitate, ',
      '"Angajament_Legal", i.Angajament_Legal, "Receptii", i.Receptii, "Plati", i.Plati, ',
      '"alte_clasificatii_acelasi_cod", (SELECT GROUP_CONCAT(CONCAT(c2.IDClsf, "/unit", c2.IdUnitate) ORDER BY c2.IDClsf SEPARATOR ", ") ',
        'FROM #.Clasificatii c2 WHERE c2.Clsf = c.Clsf AND c2.SS = c.SS AND c2.IDClsf <> c.IDClsf), ',
      '"probleme", CONCAT_WS("; ", ',
        'IF(i.IdClsf IS NULL, "indicator fara IdClsf (nu poate aparea la asociere)", NULL), ',
        'IF(i.IdClsf IS NOT NULL AND c.IDClsf IS NULL, "IdClsf nu exista in Clasificatii", NULL), ',
        'IF(c.IDClsf IS NOT NULL AND NOT (i.IdUnitate <=> c.IdUnitate), "IdUnitate diferit intre indicator si clasificatie", NULL), ',
        'IF(c.IDClsf IS NOT NULL AND NOT (i.SS <=> c.SS), "SS diferit intre indicator si clasificatie", NULL))) ',
    'FROM #.FX_Indicatori i LEFT JOIN #.Clasificatii c ON c.IDClsf = i.IdClsf ',
    'WHERE i.CodAngajament = ', v_q, ' ORDER BY i.NrCrt, i.CodAI');
  CALL FX_Fisa_Exec(REPLACE(v_sql, '#.', v_db), v_n);

  -- each line of each candidate invoice against the indicators
  SET v_sql = CONCAT(
    'INSERT INTO FX_FisaTmp (Sectiune, Tabel, Detalii) ',
    'SELECT "DIAG", "linie_factura_vs_indicator", JSON_OBJECT(',
      '"IdPrimita", l.IdPrimita, "NrFact", p.NrFact, "DataFact", p.DataFact, "Furnizor", p.DenumireP, "CUI", p.CuiNormalizat, ',
      '"candidata_prin", fp.Sursa, ',
      '"IdLinie", l.IdLinie, "NrLinie", l.NrLinie, "Denumire", LEFT(l.Denumire, 80), "Cpv", l.Cpv, "Valoare", l.Valoare, ',
      '"IdClsf_linie", l.IdClsf, "Clsf_linie", c.Clsf, "SS_linie", c.SS, "IdUnitate_clasificatie", c.IdUnitate, ',
      '"indicator_cu_acelasi_IdClsf", (SELECT GROUP_CONCAT(i.CodAI ORDER BY i.CodAI SEPARATOR ", ") ',
        'FROM #.FX_Indicatori i WHERE i.CodAngajament = ', v_q, ' AND i.IdClsf = l.IdClsf), ',
      '"indicatoare_acelasi_cod_alt_IdClsf", (SELECT GROUP_CONCAT(CONCAT(i.CodAI, " IdClsf=", i.IdClsf, " SS=", COALESCE(ci.SS, "?")) ORDER BY i.CodAI SEPARATOR ", ") ',
        'FROM #.FX_Indicatori i JOIN #.Clasificatii ci ON ci.IDClsf = i.IdClsf ',
        'WHERE i.CodAngajament = ', v_q, ' AND ci.Clsf = c.Clsf AND i.IdClsf <> l.IdClsf), ',
      '"verdict", CASE ',
        'WHEN l.IdClsf IS NULL THEN "linie fara clasificatie aleasa" ',
        'WHEN c.IDClsf IS NULL THEN "IdClsf al liniei nu exista in Clasificatii" ',
        'WHEN EXISTS (SELECT 1 FROM #.FX_Indicatori i WHERE i.CodAngajament = ', v_q, ' AND i.IdClsf = l.IdClsf) THEN "ok: angajamentul are indicator pe aceasta clasificatie" ',
        'WHEN EXISTS (SELECT 1 FROM #.FX_Indicatori i JOIN #.Clasificatii ci ON ci.IDClsf = i.IdClsf ',
                     'WHERE i.CodAngajament = ', v_q, ' AND ci.Clsf = c.Clsf) THEN "acelasi cod de clasificatie, alt IdClsf (duplicat / alta unitate / alt SS)" ',
        'ELSE "angajamentul nu are indicator pe aceasta clasificatie" END) ',
    'FROM FX_FisaPrim fp JOIN #.EF_Primite p ON p.IdPrimita = fp.IdPrimita ',
    'JOIN #.EF_PrimiteLinii l ON l.IdPrimita = fp.IdPrimita LEFT JOIN #.Clasificatii c ON c.IDClsf = l.IdClsf ',
    'ORDER BY l.IdPrimita, l.IdLinie');
  CALL FX_Fisa_Exec(REPLACE(v_sql, '#.', v_db), v_n);

  -- the value of each reception per classification, the rule of the KE screen (last counting snapshot, deleted ones left out)
  SET v_sql = CONCAT(
    'INSERT INTO FX_FisaTmp (Sectiune, Tabel, Detalii) ',
    'SELECT "DIAG", "receptie_pe_clasificatie", JSON_OBJECT(',
      '"IDRR", x.IDRR, "DataR", x.DataR, "TipReceptie", x.TipReceptie, "Descriere", LEFT(x.Descriere, 80), ',
      '"IDRH_ultim", x.IDRH, "DataH", x.DataH, "IdClsf", x.IdClsf, ',
      '"Clsf", (SELECT C.Clsf FROM #.Clasificatii C WHERE C.IDClsf = x.IdClsf LIMIT 1), ',
      '"Valoare", x.Valoare, ',
      '"Alocat_facturi", (SELECT SUM(l.Suma) FROM #.EF_PrimiteReceptii l WHERE l.CodAngajament = ', v_q, ' AND l.IdClsf = x.IdClsf ',
        'AND COALESCE(l.IDRR, (SELECT H.IDRR FROM #.FX_Receptii_H H WHERE H.IDRH = l.IDRH LIMIT 1)) = x.IDRR), ',
      '"observatie", IF(x.IdClsf IS NULL, "linii a caror CodAI nu are IdClsf in FX_Indicatori: nu intra in calculul KE", NULL)) ',
    'FROM (SELECT R.IDRR, R.DataR, R.TipReceptie, R.Descriere, H.IDRH, H.DataH, I.IdClsf, SUM(Rc.Valoare) AS Valoare ',
           'FROM #.FX_Receptii_R R ',
           'JOIN #.FX_Receptii_H H ON H.IDRH = (SELECT H2.IDRH FROM #.FX_Receptii_H H2 ',
             'WHERE H2.IDRR = R.IDRR AND COALESCE(H2.Sters, 0) = 0 ',
             'AND (COALESCE(H2.EsteStergere, 0) <> 0 OR EXISTS (SELECT 1 FROM #.FX_Receptii L WHERE L.IDRH = H2.IDRH)) ',
             'ORDER BY H2.DataH DESC, H2.NrCrt DESC, H2.IDRH DESC LIMIT 1) ',
           'LEFT JOIN #.FX_Receptii Rc ON Rc.IDRH = H.IDRH ',
           'LEFT JOIN #.FX_Indicatori I ON I.CodAI = Rc.CodAI ',
           'WHERE R.CodAngajament = ', v_q, ' AND COALESCE(R.Sters, 0) = 0 AND COALESCE(H.EsteStergere, 0) = 0 ',
           'GROUP BY R.IDRR, R.DataR, R.TipReceptie, R.Descriere, H.IDRH, H.DataH, I.IdClsf) x ',
    'ORDER BY x.DataR DESC, x.IDRR DESC');
  CALL FX_Fisa_Exec(REPLACE(v_sql, '#.', v_db), v_n);

  -- the links already written
  SET v_sql = CONCAT(
    'INSERT INTO FX_FisaTmp (Sectiune, Tabel, Detalii) ',
    'SELECT "DIAG", "legatura_ke", JSON_OBJECT(',
      '"IdLegatura", l.IdLegatura, "IdPrimita", l.IdPrimita, "NrFact", p.NrFact, "IdClsf", l.IdClsf, "Clsf", c.Clsf, ',
      '"Suma", l.Suma, "Tip", l.Tip, "UN", l.UN, "DataAdaugare", l.DataAdaugare, ',
      '"IDRR_scris", l.IDRR, "IDRH_rezervat", l.IDRH, "IDR_rezervat", l.IDR, ',
      '"IDRR_rezolvat", COALESCE(l.IDRR, (SELECT H.IDRR FROM #.FX_Receptii_H H WHERE H.IDRH = l.IDRH LIMIT 1)), ',
      '"indicator_pe_clasificatie", (SELECT GROUP_CONCAT(i.CodAI SEPARATOR ", ") FROM #.FX_Indicatori i ',
        'WHERE i.CodAngajament = ', v_q, ' AND i.IdClsf = l.IdClsf), ',
      '"receptie_stearsa", (SELECT R.Sters FROM #.FX_Receptii_R R ',
        'WHERE R.IDRR = COALESCE(l.IDRR, (SELECT H.IDRR FROM #.FX_Receptii_H H WHERE H.IDRH = l.IDRH LIMIT 1)) LIMIT 1)) ',
    'FROM #.EF_PrimiteReceptii l LEFT JOIN #.EF_Primite p ON p.IdPrimita = l.IdPrimita ',
    'LEFT JOIN #.Clasificatii c ON c.IDClsf = l.IdClsf WHERE l.CodAngajament = ', v_q, ' ORDER BY l.IdLegatura');
  CALL FX_Fisa_Exec(REPLACE(v_sql, '#.', v_db), v_n);

  -- ===================================================================================================
  -- RAW ROWS
  -- ===================================================================================================
  CALL FX_Fisa_Adauga(p_db, 'ANGAJAMENT', 'FX_Angajamente', v_c);
  CALL FX_Fisa_Adauga(p_db, 'ANGAJAMENT', 'FX_Angajamente_Grupe', v_c);

  CALL FX_Fisa_Adauga(p_db, 'INDICATORI', 'FX_Indicatori', v_c);

  CALL FX_Fisa_Adauga(p_db, 'CLASIFICATII', 'Clasificatii',
    CONCAT('t.IDClsf IN (SELECT IdClsf FROM ', v_db, 'FX_Indicatori WHERE CodAngajament = ', v_q, ')',
           ' OR t.IDClsf IN (SELECT IdClsf FROM ', v_db, 'EF_PrimiteLinii WHERE IdPrimita IN ', v_prm, ')',
           ' OR t.IDClsf IN (SELECT IdClsf FROM ', v_db, 'FX_DDF_REV_SA WHERE CodAngajament = ', v_q, ' OR IDDF IN ', v_ddf, ')'));

  CALL FX_Fisa_Adauga(p_db, 'DDF', 'FX_DDF', v_c);
  CALL FX_Fisa_Adauga(p_db, 'DDF', 'FX_DDF_Parteneri', CONCAT('t.IDDF IN ', v_ddf));
  CALL FX_Fisa_Adauga(p_db, 'DDF', 'FX_DDF_REV', CONCAT(v_c, ' OR t.IDDF IN ', v_ddf));
  CALL FX_Fisa_Adauga(p_db, 'DDF', 'FX_DDF_REV_SA', CONCAT(v_c, ' OR t.IDDF IN ', v_ddf));
  CALL FX_Fisa_Adauga(p_db, 'DDF', 'FX_DDF_REV_SB', CONCAT(v_c, ' OR t.IDDF IN ', v_ddf));
  CALL FX_Fisa_Adauga(p_db, 'DDF', 'FX_DDF_REV_PRT', CONCAT(v_c, ' OR t.IDDF IN ', v_ddf));
  CALL FX_Fisa_Adauga(p_db, 'DDF', 'FX_DDF_REV_ATT', CONCAT('t.IDDF IN ', v_ddf));

  CALL FX_Fisa_Adauga(p_db, 'REZERVARI', 'FX_Rezervari', CONCAT(v_c, ' OR t.CodAI IN ', v_ind, ' OR t.IDH IN ', v_ist));

  CALL FX_Fisa_Adauga(p_db, 'RECEPTII', 'FX_Receptii_R', v_c);
  CALL FX_Fisa_Adauga(p_db, 'RECEPTII', 'FX_Receptii_H', CONCAT(v_c, ' OR t.IDRR IN ', v_rr));
  CALL FX_Fisa_Adauga(p_db, 'RECEPTII', 'FX_Receptii', CONCAT(v_c, ' OR t.IDRH IN ', v_rh, ' OR t.CodAI IN ', v_ind));
  CALL FX_Fisa_Adauga(p_db, 'RECEPTII', 'FX_Receptii_RHR', CONCAT(v_c, ' OR t.IDRR IN ', v_rr));

  CALL FX_Fisa_Adauga(p_db, 'PLATI', 'FX_Plati', CONCAT(v_c, ' OR t.CodAI IN ', v_ind));
  CALL FX_Fisa_Adauga(p_db, 'PLATI', 'FX_Operatiuni', v_c);

  CALL FX_Fisa_Adauga(p_db, 'ORDONANTARE', 'FX_ORD', CONCAT(v_c, ' OR t.IDDF IN ', v_ddf));
  CALL FX_Fisa_Adauga(p_db, 'ORDONANTARE', 'FX_ORD_PART', CONCAT('t.IDORDP IN ', v_ord));
  CALL FX_Fisa_Adauga(p_db, 'ORDONANTARE', 'FX_ORD_TBL', CONCAT(v_c, ' OR t.IDORDP IN ', v_ord));
  CALL FX_Fisa_Adauga(p_db, 'ORDONANTARE', 'FX_ORD_TBL_REC', CONCAT('t.IDORDTBLP IN ', v_tbl));
  CALL FX_Fisa_Adauga(p_db, 'ORDONANTARE', 'FX_ORD_DOC', CONCAT('t.IDORDP IN ', v_ord));

  CALL FX_Fisa_Adauga(p_db, 'ISTORIC', 'FX_Istoric', v_c);

  CALL FX_Fisa_Adauga(p_db, 'NOTE_CAB', 'FX_NoteCAB_Corectii', v_c);
  CALL FX_Fisa_Adauga(p_db, 'NOTE_CAB', 'FX_NoteCAB',
    CONCAT('t.IDNC IN (SELECT IDNC FROM ', v_db, 'FX_NoteCAB_Corectii WHERE CodAngajament = ', v_q, ')'));

  CALL FX_Fisa_Adauga(p_db, 'NUMERE', 'FX_NumberLock', v_c);

  CALL FX_Fisa_Adauga(p_db, 'EFACTURI', 'EF_PrimiteAsocieri', CONCAT('t.IDDF IN ', v_ddf));
  CALL FX_Fisa_Adauga(p_db, 'EFACTURI', 'EF_PrimiteReceptii', v_c);
  CALL FX_Fisa_Adauga(p_db, 'EFACTURI', 'EF_Primite', CONCAT('t.IdPrimita IN ', v_prm));
  CALL FX_Fisa_Adauga(p_db, 'EFACTURI', 'EF_PrimiteLinii', CONCAT('t.IdPrimita IN ', v_prm));
  CALL FX_Fisa_Adauga(p_db, 'EFACTURI', 'EF_PrimiteTVA', CONCAT('t.IdPrimita IN ', v_prm));
  CALL FX_Fisa_Adauga(p_db, 'EFACTURI', 'EF_PrimiteOperatii', CONCAT('t.IdPrimita IN ', v_prm));
  CALL FX_Fisa_Adauga(p_db, 'EFACTURI', 'EF_PrimiteNote', CONCAT('t.IdPrimita IN ', v_prm));

  SET SESSION sql_mode = v_mode;
  SELECT Ord, Sectiune, Tabel, Detalii FROM FX_FisaTmp ORDER BY Ord;

  DROP TEMPORARY TABLE IF EXISTS FX_FisaTmp;
  DROP TEMPORARY TABLE IF EXISTS FX_FisaPrim;
END$$

DELIMITER ;
