-- =====================================================================================
-- Felia 0049 — octetii atasamentelor de ordonantare (FX_ORD_ATT_IMG).
--
-- ATENTIE — SE APLICA PE FIECARE BAZA DE UNITATE, plus pe AVACONT_SURSA.
--   O baza MariaDB = o unitate (vezi routes/forexe/ord.py, nota «Scope»), deci fisierul
--   se ruleaza o data pentru FIECARE baza de unitate, nu o singura data pe VPS. Se aplica
--   SI pe AVACONT_SURSA, ca bazele create de acum inainte sa aiba tabela din nastere si ca
--   schema_sync sa nu raporteze o diferenta pe veci — aceeasi regula ca la
--   sql/0048_alegeri_unitate.sql si sql/0049_receptii_stergere.sql.
--
--   PE AVACONT_SURSA: vezi sectiunea de la finalul fisierului. Cheia primara ramane
--   `INT NOT NULL` — FARA `AUTO_INCREMENT`. Scutirea din felia 0048-01 §3.1 se aplica aici
--   integral: AUTO_INCREMENT pe schema-sursa dezarmeaza o poarta de siguranta a migrarii
--   (un rand care soseste fara cheie ar primi tacut urmatorul numar in loc sa ridice).
--
-- =====================================================================================
-- DE CE EXISTA
-- =====================================================================================
-- `FX_ORD_ATT.Imagine` este `longtext` si tine base64 — asa scria Access
-- (`frmFX_ORD_PRTSCR_S.SelectFile` -> `FileToBase64`). In dump-ul 000_DEMO tabela
-- `FX_ORD_ATT` este GOALA (`AUTO_INCREMENT = 1`), in timp ce `FX_ORD_DOC` e la 719 si
-- `FX_ORD_TBL` la 891 — adica functia n-a fost folosita niciodata cu adevarat, deci nu
-- exista date vechi cu care sa ramanem compatibili.
--
-- K-BOT stocheaza deci octetii BRUTI intr-o tabela separata, dupa exact tiparul lui
-- `FX_ORD_PDF` (felia 0041): un rand per parinte, suma SHA-256 pentru integritate,
-- ON DELETE CASCADE ca randul de atasament si octetii lui sa nu poata diverge.
-- `FX_ORD_ATT.Imagine` ramane pe loc, dar NU se mai scrie si NU se mai citeste.
--
-- `NumeFisier` traieste AICI, nu in `FX_ORD_ATT`: `frmFX_ORD_PRTSCR_S` lega un control
-- `Nume` de `tmpFX_ORD_ATT`, dar `FX_ORD_ATT` din MariaDB NU are coloana `Nume`.
--
-- =====================================================================================
-- !!! NEVERIFICAT !!! — CITESTE INAINTE DE A RULA
-- =====================================================================================
-- Ca la sql/0041_fx_pdf_tables.sql: numele SI TIPUL cheii primare parinte trebuie
-- confirmate pe o baza VIE inainte ca linia FOREIGN KEY sa fie considerata finala.
-- Ce se stie si de unde: `FX_ORD_ATT.IDORDATTP` — numele si tipul `int(11)` sunt citite
-- din MariaDB_Schema/000_DEMO.sql (dump real), NU ghicite. Nu s-a putut verifica pe o
-- baza de productie.
--
-- PROBA (inlocuieste <BAZA> cu numele bazei de unitate):
--
--   SELECT TABLE_NAME, COLUMN_NAME, COLUMN_TYPE, COLUMN_KEY
--     FROM information_schema.COLUMNS
--    WHERE TABLE_SCHEMA = '<BAZA>'
--      AND TABLE_NAME = 'FX_ORD_ATT' AND COLUMN_NAME = 'IDORDATTP';
--
-- Asteptat: un rand, COLUMN_KEY = 'PRI', COLUMN_TYPE = 'int(11)'. Daca raspunsul difera —
-- alt nume, alt tip, sau coloana nu e cheie primara — OPRESTE-TE si raporteaza; nu ghici.
--
-- PROBA A DOUA (deschisa in planul feliei, §12 punctul 2): are vreo unitate VIE randuri in
-- `FX_ORD_ATT`? Constatarea «tabela e goala» vine DOAR din dump-ul demo.
--
--   SELECT COUNT(*) AS randuri,
--          SUM(CASE WHEN Imagine IS NOT NULL AND Imagine <> '' THEN 1 ELSE 0 END) AS cu_imagine
--     FROM <BAZA>.FX_ORD_ATT;
--
-- Daca `cu_imagine` > 0 pe vreo unitate, `Imagine` NU e moarta si intrebarea migrarii se
-- redeschide (octetii vechi ar trebui mutati incoace inainte de a ignora coloana).
--
-- Verifica si marimea maxima a pachetului si plafonul nginx (un blob plus antetul
-- instructiunii trebuie sa incapa intr-un singur pachet; nginx are azi 20m):
--
--   SHOW VARIABLES LIKE 'max_allowed_packet';   -- se vrea >= 32M
--
-- =====================================================================================

-- -------------------------------------------------------------------------------------
-- FX_ORD_ATT_IMG — octetii unui atasament de ordonantare.
--
-- Un rand per `FX_ORD_ATT` (cheie UNICA pe IDORDATTP): o re-incarcare INLOCUIESTE randul,
-- nu adauga unul. Fara istoric, exact ca la FX_DDF_PDF / FX_ORD_PDF.
--
-- Continut = LONGBLOB, nu MEDIUMBLOB: MEDIUMBLOB se opreste la 16.777.215 octeti, adica
-- fix pe plafonul practic. Plafonul se impune in Flask si in nginx, nu prin tipul coloanei.
--
-- ON DELETE CASCADE: stergerea randului de atasament sterge si octetii. Si, prin lantul
-- FX_ORD -> FX_ORD_ATT -> aici, stergerea ordonantarii ii sterge pe toti.
-- -------------------------------------------------------------------------------------
CREATE TABLE `FX_ORD_ATT_IMG` (
  `IDIMG`      int(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `IDORDATTP`  int(11)          NOT NULL,
  `NumeFisier` varchar(255)     NOT NULL,   -- numele fisierului ales de operator
  `TipMime`    varchar(100)     NOT NULL,   -- image/png, image/jpeg, ... dedus pe server
  `Dimensiune` int(10) UNSIGNED NOT NULL,   -- numarul exact de octeti din Continut
  `Sha256`     char(64)         NOT NULL,   -- hex minuscule, peste Continut
  `Continut`   longblob         NOT NULL,
  `DataModif`  datetime         NOT NULL,
  PRIMARY KEY (`IDIMG`) USING BTREE,
  UNIQUE INDEX `UQ_FX_ORD_ATT_IMG_ATTP`(`IDORDATTP`) USING BTREE,
  CONSTRAINT `FK_FX_ORD_ATT_IMG_ATT` FOREIGN KEY (`IDORDATTP`)
    REFERENCES `FX_ORD_ATT` (`IDORDATTP`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = Dynamic;

-- =====================================================================================
-- VARIANTA PENTRU AVACONT_SURSA — ruleaz-o pe ea IN LOCUL celei de mai sus.
--
-- Singura diferenta: `IDIMG` este `INT UNSIGNED NOT NULL`, FARA `AUTO_INCREMENT`.
-- Motivul e in felia 0048-01 §3.1: pe schema-sursa, AUTO_INCREMENT ar face ca migrarea sa
-- fabrice tacut chei pentru randuri care sosesc fara ele, in loc sa ridice eroare.
-- =====================================================================================
--
-- CREATE TABLE `FX_ORD_ATT_IMG` (
--   `IDIMG`      int(10) UNSIGNED NOT NULL,
--   `IDORDATTP`  int(11)          NOT NULL,
--   `NumeFisier` varchar(255)     NOT NULL,
--   `TipMime`    varchar(100)     NOT NULL,
--   `Dimensiune` int(10) UNSIGNED NOT NULL,
--   `Sha256`     char(64)         NOT NULL,
--   `Continut`   longblob         NOT NULL,
--   `DataModif`  datetime         NOT NULL,
--   PRIMARY KEY (`IDIMG`) USING BTREE,
--   UNIQUE INDEX `UQ_FX_ORD_ATT_IMG_ATTP`(`IDORDATTP`) USING BTREE,
--   CONSTRAINT `FK_FX_ORD_ATT_IMG_ATT` FOREIGN KEY (`IDORDATTP`)
--     REFERENCES `FX_ORD_ATT` (`IDORDATTP`) ON DELETE CASCADE ON UPDATE RESTRICT
-- ) ENGINE = InnoDB CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = Dynamic;
