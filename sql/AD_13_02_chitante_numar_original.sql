-- Se executa MANUAL numai in AVACONT_SURSA, dupa AD_13_chitante_an_scolar.sql.
-- Propagarea structurii catre bazele unitatilor se face de utilizator prin KBotAdmin (coloana obisnuita: o adauga sincronizarea).
-- ADE.Migrator nu executa acest DDL, dar Testeaza refuza baza fara aceasta coloana.
--
-- NumarSub: sub-numarul chitantei, in sutimi, ca VC_NIR.NumarSub (751,1 = Numar 751 + NumarSub 10). Il da doar migratorul, automat, pentru
--   chitantele migrate care au acelasi numar in acelasi an scolar (cele mai vechi primesc ,1 ,2 ...; cea mai noua ramane fara);
--   numarul din Access nu se schimba. Aplicatia web emite mereu numere intregi (NumarSub 0). Se vede numai in Registrul de casa
--   si in Fisa debitor.
-- Cheia unica devine (SubunitId, Serie, AnScolar, Numar, NumarSub), cu acelasi nume uq_Sub_Serie_An_Numar.
-- (Numele fisierului ramane din faza anterioara, cand se planuia si o coloana NumarOriginal; ea nu se mai foloseste. Daca a fost deja
--  adaugata, ramane NULL peste tot si poate fi stearsa: ALTER TABLE AD_Chitante DROP COLUMN NumarOriginal.)
USE `AVACONT_SURSA`;

ALTER TABLE `AD_Chitante`
  ADD COLUMN `NumarSub` SMALLINT NOT NULL DEFAULT 0
    COMMENT 'AD13-02: sub-numarul in sutimi ca la VC_NIR.NumarSub: 751,1 = Numar 751 + NumarSub 10; 0 = fara sub-numar';

ALTER TABLE `AD_Chitante`
  DROP INDEX `uq_Sub_Serie_An_Numar`,
  ADD UNIQUE KEY `uq_Sub_Serie_An_Numar` (`SubunitId`, `Serie`, `AnScolar`, `Numar`, `NumarSub`);

-- Verificare (nu modifica nimic):
-- SHOW INDEX FROM `AD_Chitante` WHERE Key_name LIKE 'uq_%';
-- SELECT SubunitId, Serie, Numar, NumarSub, Data FROM AD_Chitante WHERE NumarSub<>0 ORDER BY SubunitId, Serie, Data;
