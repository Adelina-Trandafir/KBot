-- =====================================================================================
-- Felia 0048-03 — stergerea si reconstituirea receptiilor.
--
-- ATENTIE — SE APLICA PE FIECARE BAZA DE UNITATE, plus pe AVACONT_SURSA.
--   Trei coloane NOI, toate cu DEFAULT, pe care migrarea nu le scrie niciodata. Se aplica
--   SI pe AVACONT_SURSA ca bazele create de acum inainte sa le aiba din nastere si ca
--   schema_sync sa nu raporteze o diferenta pe veci — exact regula feliei 0048-02
--   (sql/0048_alegeri_unitate.sql), din exact acelasi motiv.
--
--   Scutirea care tine cele SAPTE alter-uri AUTO_INCREMENT ale feliei 0048-01 §3.1 departe
--   de AVACONT_SURSA NU se aplica aici. Scutirea aceea exista fiindca AUTO_INCREMENT
--   dezarmeaza o poarta de siguranta a migrarii: un rand care soseste fara cheie ar primi
--   tacut urmatorul numar in loc sa ridice. Nimic de mai jos nu dezarmeaza nimic.
--
-- =====================================================================================
-- DE CE EXISTA
-- =====================================================================================
-- FX_Receptii_R.Sters
--   O receptie stearsa pe site NU dispare de aici. Ea a existat, a purtat valoare, si
--   platile facute INAINTE de stergere se sprijina in continuare pe ea (F22). Steagul
--   spune «nu mai e in ListaReceptii», nu «nu a existat».
--
--   Steagul are si un al doilea rol, imediat: pasul 4b al ingestiei potriveste o receptie
--   care soseste cu una stocata prin `CLng(DataR)` — GRANULARITATE DE ZI (F25, verificat
--   citind `Receptii_Prelucrare` in mdl_FX_Receptii). Fara `Sters = 0` in interogarea de
--   candidati, o receptie creata in aceeasi zi calendaristica in care fusese creata una
--   stearsa in martie s-ar potrivi peste aceea si ar suprascrie-o in tacere.
--
-- FX_Receptii_R.Reconstituit
--   Marcheaza o receptie construita din propriile ei instantanee, fiindca a fost creata
--   SI stearsa inainte ca K-BOT sa fi descarcat vreodata angajamentul (F26). Pentru ea nu
--   exista rand in `ListaReceptii` — site-ul nu o mai listeaza — deci nu are de unde sa
--   vina altfel. Toata informatia e in istoric; nimic nu se inventeaza (vezi
--   PLAN_ForexeIngestSteps3to8 §4c-bis pentru fiecare camp si sursa lui).
--
--   Orice receptie reconstituita este SI stearsa, deci `Sters = 1` o insoteste mereu. Cele
--   doua NU sunt acelasi fapt si nu se colapseaza: `Sters` spune ce s-a intamplat cu ea,
--   `Reconstituit` spune de unde stim ca a existat. A citi unul din celalalt ar face
--   imposibil de raspuns «cate receptii am reconstituit noi».
--
--   NU folositi `HASH IS NULL` ca semn. Se intampla sa fie adevarat azi — o receptie
--   reconstituita nu are bloc de sarcina utila pe care sa-l hasuim — dar ar putrezi din
--   prima zi in care altceva lasa un hash gol.
--
-- FX_Receptii_H.EsteStergere
--   Randul de istoric cu `Descriere = "Stergere receptie"` (ortografia exact asa, fara
--   diacritice — confirmat de operator 26.08.2026) poarta `(activ:true)` ca orice antet,
--   deci devine instantaneu pe calea obisnuita. Nu are randuri pe indicator, deci nu
--   produce linii `FX_Receptii`. Steagul asta il numeste ca atare: e ULTIMUL instantaneu
--   din lantul receptiei lui, `DataH` e data stergerii, `Total` e cat valora cand a plecat.
--
-- FARA COLOANA DE DATA A STERGERII. F21 face din `DataH` al instantaneului de stergere
-- chiar data stergerii. O a doua copie a unei date e un al doilea lucru care poate sa nu
-- fie de acord cu primul.
--
-- FX_Receptii_H.Sters exista deja (000_DEMO.sql) si isi pastreaza intelesul, acum spus pe
-- fata: ACEST INSTANTANEU NU CONSEMNEAZA NICIO SCHIMBARE SI E LASAT DELIBERAT NEATASAT
-- (F17 / §1.6 din fundament). Nu e un mecanism de ascundere.
--
-- =====================================================================================
-- PROBA INAINTE DE A RULA (inlocuieste <BAZA> cu numele bazei)
-- =====================================================================================
-- O coloana lipsa esueaza zgomotos la ALTER. Una de tip gresit NU esueaza — se scrie si
-- se citeste altceva decat crede codul. De-asta proba se uita la tipuri, nu doar la nume:
--
--   SELECT TABLE_NAME, COLUMN_NAME, COLUMN_TYPE, IS_NULLABLE, COLUMN_DEFAULT
--     FROM information_schema.COLUMNS
--    WHERE TABLE_SCHEMA = '<BAZA>'
--      AND ( (TABLE_NAME = 'FX_Receptii_R' AND COLUMN_NAME IN ('IDRR','Sters','Reconstituit'))
--         OR (TABLE_NAME = 'FX_Receptii_H' AND COLUMN_NAME IN ('IDRH','Sters','EsteStergere')) )
--    ORDER BY TABLE_NAME, COLUMN_NAME;
--
-- ASTEPTAT INAINTE de a rula fisierul — EXACT trei randuri:
--   FX_Receptii_H | IDRH  | int(11)    | NO  | NULL
--   FX_Receptii_H | Sters | tinyint(1) | YES | NULL
--   FX_Receptii_R | IDRR  | int(11)    | NO  | NULL
--
-- Daca `FX_Receptii_H.Sters` LIPSESTE — opreste-te si raporteaza. Nu e o baza pe care
-- fisierul asta stie sa lucreze, si nu ghici ce mai lipseste.
-- Daca `Sters` / `Reconstituit` / `EsteStergere` APAR deja — fisierul a fost rulat; nu-l
-- rula a doua oara (ALTER ... ADD COLUMN nu e idempotent, esueaza cu 1060).
--
-- ASTEPTAT DUPA rulare — cinci randuri, cele trei noi fiind:
--   FX_Receptii_H | EsteStergere | tinyint(1) | NO | 0
--   FX_Receptii_R | Reconstituit | tinyint(1) | NO | 0
--   FX_Receptii_R | Sters        | tinyint(1) | NO | 0
--
-- NOT NULL DEFAULT 0 pe toate trei, deliberat: raspunsul «nu stiu daca e stearsa» nu are
-- niciun inteles. Randurile existente devin toate «nestearsa, neconstituita, nu e
-- stergere», ceea ce e adevarat pentru fiecare rand care exista azi — nimic nu putea fi
-- marcat inainte ca marcajul sa existe.
-- =====================================================================================

ALTER TABLE FX_Receptii_R
    ADD COLUMN Sters tinyint(1) NOT NULL DEFAULT 0
    COMMENT 'Receptie stearsa pe site (F22). Ramane in baza: platile anterioare stergerii se sprijina pe ea.';

ALTER TABLE FX_Receptii_R
    ADD COLUMN Reconstituit tinyint(1) NOT NULL DEFAULT 0
    COMMENT 'Receptie construita din propriile instantanee, creata SI stearsa inainte de prima descarcare (F26). Mereu impreuna cu Sters=1, dar alt fapt.';

ALTER TABLE FX_Receptii_H
    ADD COLUMN EsteStergere tinyint(1) NOT NULL DEFAULT 0
    COMMENT 'Instantaneul provine dintr-un rand «Stergere receptie» (F21): ultimul din lant, DataH = data stergerii.';
