-- =====================================================================================
-- Felia 0041 — stocarea PDF-urilor SEMNATE pe server (FX_DDF_PDF / FX_ORD_PDF).
--
-- ATENTIE — SE APLICA PE FIECARE BAZA DE UNITATE.
--   O baza MariaDB = o unitate (vezi routes/forexe/ddf.py, nota «Scope»), deci acest
--   fisier se ruleaza o data pentru FIECARE baza de unitate, nu o singura data pe VPS.
--
-- =====================================================================================
-- !!! NEVERIFICAT !!! — CITESTE INAINTE DE A RULA
-- =====================================================================================
-- Planul feliei cere ca numele SI TIPUL cheilor primare parinte sa fie confirmate pe o
-- baza VIE inainte ca liniile FOREIGN KEY sa fie considerate finale. Confirmarea NU s-a
-- putut face la scrierea acestui fisier (fara acces la o baza reala; in depozit nu exista
-- niciun dump DDL MariaDB). Ce se stie si de unde:
--
--   * `FX_DDF_REV.IDREV`  — NUMELE e coroborat de SQL-ul rutei care ruleaza azi
--                           (routes/forexe/ddf.py, `r.IDREV`) si de FK-ul citat acolo
--                           (`FX_DDF_REV_SA_ibfk_4`). TIPUL nu e verificat.
--   * `FX_ORD.IDORDP`     — NUMELE e coroborat de routes/forexe/ord.py (capcana 1: cheile
--                           «...P» sunt cheile REALE MariaDB). TIPUL nu e verificat.
--
-- Un FK cu tip nepotrivit (INT vs INT UNSIGNED, sau semnat vs nesemnat) esueaza la
-- CREATE TABLE cu errno 150 — zgomotos, nu tacut. Deci: RULEAZA INTAI PROBA de mai jos
-- si potriveste tipul coloanelor IDREV / IDORDP din acest fisier cu ce raspunde ea.
--
-- PROBA (inlocuieste <BAZA> cu numele bazei de unitate):
--
--   SELECT TABLE_NAME, COLUMN_NAME, COLUMN_TYPE, COLUMN_KEY
--     FROM information_schema.COLUMNS
--    WHERE TABLE_SCHEMA = '<BAZA>'
--      AND ((TABLE_NAME = 'FX_DDF_REV' AND COLUMN_NAME = 'IDREV')
--        OR (TABLE_NAME = 'FX_ORD'     AND COLUMN_NAME = 'IDORDP'));
--
-- Asteptat: doua randuri, amandoua cu COLUMN_KEY = 'PRI' si COLUMN_TYPE = 'int(11)'
-- (echivalentul lui `INT` de mai jos). Daca raspunsul difera — alt nume, alt tip, sau
-- coloana nu e cheie primara — OPRESTE-TE si raporteaza; nu ghici.
--
-- Verifica si marimea maxima a pachetului (un blob de 16 MB plus antetul instructiunii
-- trebuie sa incapa intr-un singur pachet):
--
--   SHOW VARIABLES LIKE 'max_allowed_packet';   -- se vrea >= 32M
--
-- =====================================================================================

-- -------------------------------------------------------------------------------------
-- FX_DDF_PDF — un PDF SEMNAT per revizie de document de fundamentare.
--
-- Fara istoric (decizia operatorului 2026-08-17): cheia unica pe IDREV face ca o
-- re-semnare sa INLOCUIASCA randul, nu sa adauge unul. EXISTENTA randului inseamna
-- «exista PDF semnat» — de aceea nu exista o coloana `Semnat`.
--
-- Continut = LONGBLOB, NU MEDIUMBLOB: MEDIUMBLOB se opreste la 16.777.215 octeti, exact
-- pe plafonul estimat, deci ar cadea fix la limita. Plafonul practic se impune in Flask
-- (MAX_CONTENT_LENGTH), nu prin tipul coloanei.
--
-- ON DELETE CASCADE: stergerea reviziei sterge si PDF-ul ei. Comportament documentat,
-- nu un accident.
-- -------------------------------------------------------------------------------------
CREATE TABLE FX_DDF_PDF (
    IDPDF      INT UNSIGNED NOT NULL AUTO_INCREMENT,
    IDREV      INT          NOT NULL,
    NumeFisier VARCHAR(255) NOT NULL,   -- derivat pe SERVER, niciodata primit de la client
    Dimensiune INT UNSIGNED NOT NULL,   -- numarul exact de octeti din Continut
    Sha256     CHAR(64)     NOT NULL,   -- hex minuscule, peste Continut
    Continut   LONGBLOB     NOT NULL,
    DataModif  DATETIME     NOT NULL,
    PRIMARY KEY (IDPDF),
    UNIQUE KEY UQ_FX_DDF_PDF_IDREV (IDREV),
    CONSTRAINT FK_FX_DDF_PDF_REV FOREIGN KEY (IDREV)
        REFERENCES FX_DDF_REV (IDREV) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- -------------------------------------------------------------------------------------
-- FX_ORD_PDF — un PDF SEMNAT per ordonantare. Sora tabelei de mai sus.
--
-- Cheia e `IDORDP` — cheia REALA MariaDB a lui FX_ORD (capcana 1 din routes/forexe/ord.py),
-- NICIODATA omonimul `IDORD`, care e id-ul Access pastrat.
-- -------------------------------------------------------------------------------------
CREATE TABLE FX_ORD_PDF (
    IDPDF      INT UNSIGNED NOT NULL AUTO_INCREMENT,
    IDORDP     INT          NOT NULL,
    NumeFisier VARCHAR(255) NOT NULL,
    Dimensiune INT UNSIGNED NOT NULL,
    Sha256     CHAR(64)     NOT NULL,
    Continut   LONGBLOB     NOT NULL,
    DataModif  DATETIME     NOT NULL,
    PRIMARY KEY (IDPDF),
    UNIQUE KEY UQ_FX_ORD_PDF_IDORDP (IDORDP),
    CONSTRAINT FK_FX_ORD_PDF_ORD FOREIGN KEY (IDORDP)
        REFERENCES FX_ORD (IDORDP) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
