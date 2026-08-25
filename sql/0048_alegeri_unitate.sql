-- =====================================================================================
-- Felia 0048-02 — FX_Alegeri_Unitate: alegerile de unitate reținute de operator.
--
-- ATENTIE — SE APLICA PE FIECARE BAZA DE UNITATE, plus pe AVACONT_SURSA.
--   O baza MariaDB = o unitate (vezi routes/forexe/prelucrare.py, nota «Scope»), deci
--   fisierul se ruleaza o data pentru FIECARE baza de unitate. Se aplica SI pe
--   AVACONT_SURSA (schema de referinta), ca bazele create de acum inainte sa il aiba
--   din nastere si ca schema_sync sa nu raporteze o diferenta pe veci.
--
--   Aceasta este SINGURA diferenta fata de regula feliei 0048-01 §3.1: acolo cele sapte
--   ALTER-uri AUTO_INCREMENT nu au voie pe AVACONT_SURSA, fiindca elimina o poarta de
--   siguranta a migrarii. Aici nu se atinge nicio cheie existenta — se adauga un tabel
--   nou, gol, pe care migrarea nu il scrie niciodata.
--
-- =====================================================================================
-- DE CE EXISTA
-- =====================================================================================
-- Pasul 2 al ingestiei FOREXE (Prelucrare_Indicatori) rezolva unitatea unui indicator
-- prin clasificatie, pe perechea (SS, ClsfE) — ultimele 6 cifre, decizia D17 a feliei
-- 0048-01. Perechea NU e garantat unica: `ClsfE` are 6 cifre acolo unde `ClsfSal` are 12,
-- deci pliaza mai mult. In Access, cand perechea prindea mai multe unitati,
-- `Obtine_IdUnitate_Din` deschidea formularul modal `FX_Unitate` si INTREBA operatorul,
-- de fiecare data.
--
-- K-BOT intreaba tot de fiecare data — dar dialogul are o bifa «Nu mă mai întreba pentru
-- această combinație». Bifata, alegerea ajunge AICI, si urmatoarea ciocnire pe ACEEASI
-- pereche (SS, ClsfE) se rezolva in tacere. O pereche noua intreaba din nou.
--
-- Randul poarta CINE a ales si CAND, tocmai pentru ca o alegere gresita altfel ar ramane
-- ascunsa: un indicator atasat altei subunitati nu iese la iveala luni de zile. Cu tabelul
-- asta, «cine a hotarat asta si cand» se raspunde cu un SELECT:
--
--   SELECT A.SS, A.ClsfE, A.IdUnitate, U.Detalii, A.UN, A.DataAlegere
--     FROM FX_Alegeri_Unitate A INNER JOIN Unitati U ON U.IdUnitate = A.IdUnitate
--    ORDER BY A.DataAlegere DESC;
--
-- Stergerea unui rand readuce intrebarea la urmatoarea ingestie. Nimic altceva nu se
-- pierde — tabelul e o memorie, nu o sursa de date.
--
-- =====================================================================================
-- PROBA INAINTE DE A RULA (inlocuieste <BAZA> cu numele bazei de unitate)
-- =====================================================================================
-- Cheia straina de mai jos tinteste `Unitati.IdUnitate`. Numele si tipul sunt luate din
-- MariaDB_Schema/000_DEMO.sql (`IdUnitate int(11) NOT NULL`, PRIMARY KEY), dar acel dump
-- e din 22.08 si NU a fost confirmat pe serverul viu pentru felia asta. Un tip nepotrivit
-- (semnat vs. nesemnat) esueaza la CREATE TABLE cu errno 150 — zgomotos, nu tacut:
--
--   SELECT COLUMN_NAME, COLUMN_TYPE, COLUMN_KEY
--     FROM information_schema.COLUMNS
--    WHERE TABLE_SCHEMA = '<BAZA>' AND TABLE_NAME = 'Unitati' AND COLUMN_NAME = 'IdUnitate';
--
-- Asteptat: un rand, COLUMN_KEY = 'PRI', COLUMN_TYPE = 'int(11)'. Daca raspunsul difera —
-- OPRESTE-TE si raporteaza; nu ghici.
--
-- CHARSET: utf8 / utf8_general_ci, ca restul bazei de unitate (NU utf8mb4, care e ce a
-- folosit felia 0041). Aici conteaza: `SS` si `ClsfE` sunt aceleasi siruri ca in coloanele
-- generate ale lui `Clasificatii`, iar o comparatie intre doua colatii diferite e exact
-- genul de lucru pe care felia 0043 l-a scos la iveala.
-- =====================================================================================

CREATE TABLE FX_Alegeri_Unitate (
    IdAlegere   INT UNSIGNED NOT NULL AUTO_INCREMENT,
    -- Perechea intrebata. Aceleasi valori ca in coloanele GENERATE ale lui Clasificatii:
    -- SS = Sector + Sursa ('02E'), ClsfE = Articol + Alineat fara puncte ('200101').
    SS          VARCHAR(3)   CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
    ClsfE       VARCHAR(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
    -- Raspunsul operatorului.
    IdUnitate   INT(11)      NOT NULL,
    -- Urma: cine a raspuns (adresa de e-mail cu care s-a logat) si cand.
    UN          VARCHAR(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
    DataAlegere DATETIME     NOT NULL,
    PRIMARY KEY (IdAlegere),
    -- O singura alegere per pereche. Re-bifarea o INLOCUIESTE (ON DUPLICATE KEY UPDATE),
    -- nu adauga un al doilea rand — altfel «care e alegerea in vigoare» ar fi ambiguu.
    UNIQUE KEY UQ_FX_Alegeri_Unitate (SS, ClsfE),
    -- Stergerea unei unitati sterge alegerile care o numeau: o alegere care trimite catre
    -- o unitate inexistenta ar fi mai rea decat intrebarea pe care o inlocuieste.
    CONSTRAINT FK_FX_Alegeri_Unitate_Unitate FOREIGN KEY (IdUnitate)
        REFERENCES Unitati (IdUnitate) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = Dynamic;
