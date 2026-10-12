-- Parteneri.Provenienta: de unde a venit randul.
--   MIGRARE_VERCON = adaugat de VRC.Migrator (furnizori din ANAF)
--   manual         = adaugat in K-BOT (fereastra Parteneri)
--   avacont        = trecut din Access prin rutele /api/parteneri/syncdb si /api/parteneri/upsert
--   gol (NULL)     = randul exista deja (migrare_kbot); asa raman toate randurile de pana acum
-- Se ruleaza INTAI pe AVACONT_SURSA, apoi se sincronizeaza pe fiecare DC (KBotAdmin), INAINTE de a pune pe server
-- codul nou al rutelor (INSERT-urile numesc coloana).
ALTER TABLE `AVACONT_SURSA`.`Parteneri`
  ADD COLUMN `Provenienta` varchar(30) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL
  COMMENT 'De unde a venit randul: MIGRARE_VERCON, manual, avacont; gol = migrare_kbot'
  AFTER `Ascuns`;
