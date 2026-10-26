-- Se executa MANUAL numai in AVACONT_SURSA, dupa AD_05.
-- Propagarea structurii catre bazele unitatilor se face de utilizator prin KBotAdmin.
-- ADE.Migrator nu executa acest DDL.
--
-- Numerele chitantelor se pot relua la fiecare an scolar (septembrie -> august).
-- Cheia unica veche (SubunitId, Serie, Numar) interzicea acelasi numar in ani diferiti; cheia noua
-- (SubunitId, Serie, AnScolar, Numar) il permite, dar tot nu accepta doua chitante cu acelasi numar in acelasi an scolar.
--
-- AnScolar este o coloana calculata din Data chitantei: anul in care a inceput anul scolar.
--   Data 15.10.2026 -> 2026      Data 20.03.2027 -> 2026      Data 31.08.2027 -> 2026      Data 01.09.2027 -> 2027
-- Fiind calculata, nu se scrie nicaieri (nici de migrator, nici de server) si nu poate iesi din pas cu Data.
-- O chitanta fara Data are AnScolar NULL si nu intra in cheie (ca la Serie sau Numar NULL).
--
-- Bazele unitatilor: se propaga prin sincronizarea de structura. schema_diff.py adauga acum si coloana calculata lipsa
-- (ADD COLUMN, inaintea cheilor), apoi creeaza cheia noua; cheia veche se sterge in FORCE.
-- schema_diff.py trebuie sa fie deja pe server (corectura din 26.10.2026), altfel coloana nu se adauga.
USE `AVACONT_SURSA`;

ALTER TABLE `AD_Chitante`
  ADD COLUMN `AnScolar` SMALLINT GENERATED ALWAYS AS (YEAR(`Data`) - IF(MONTH(`Data`) < 9, 1, 0)) STORED
  COMMENT 'AD13: anul in care a inceput anul scolar al chitantei, calculat din Data (septembrie = anul nou)';

-- Intai cheia noua (acopera tot ce acoperea cea veche), apoi se renunta la cea veche.
ALTER TABLE `AD_Chitante`
  ADD UNIQUE KEY `uq_Sub_Serie_An_Numar` (`SubunitId`, `Serie`, `AnScolar`, `Numar`),
  DROP INDEX `uq_Sub_Serie_Numar`;

-- Verificare (nu modifica nimic):
-- SELECT SubunitId, Serie, AnScolar, COUNT(*) AS Chitante, MIN(Numar) AS DeLa, MAX(Numar) AS PanaLa
--   FROM AD_Chitante GROUP BY SubunitId, Serie, AnScolar ORDER BY SubunitId, Serie, AnScolar;
