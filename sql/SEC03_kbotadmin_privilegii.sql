-- SEC03 / 2026-10-19: configurare MANUALA, pe serverul K-BOT (DB_CONFIG_NEW).
-- Nu se executa din aplicatie. Nu modifica schemele sau tabelele existente.
-- Necesita MariaDB >= 10.5.2 (CONNECTION ADMIN).
-- Inlocuiti parola de mai jos INAINTE de executare, intr-o sesiune DBA.
-- Contul este local si dedicat exclusiv comenzilor private KBotAdmin prin SSH.
-- Nu configurati acest cont ca DB_CONFIG_NEW sau ca utilizator de client.

CREATE USER 'kbot_drepturi'@'localhost'
  IDENTIFIED BY '<INLOCUITI_CU_O_PAROLA_LUNGA_UNICA>';

-- CREATE USER permite REVOKE ALL PRIVILEGES, GRANT OPTION inclusiv pentru
-- conturi care au primit initial privilegii globale. Aplicatia nu creeaza useri.
-- PROCESS + CONNECTION ADMIN permit inchiderea conexiunilor utilizatorului,
-- altfel privilegiile globale din sesiunile vechi pot ramane in cache.
GRANT CREATE USER, PROCESS, CONNECTION ADMIN ON *.*
  TO 'kbot_drepturi'@'localhost';

-- Poate acorda DOAR operatii pe date. Nu primeste SUPER, FILE, DDL, EXECUTE.
-- Sfera globala este necesara pentru toate bazele de unitate existente/viitoare
-- si pentru citirea metadatelor mysql.user / roles_mapping / proxies_priv.
-- Este un cont administrativ sensibil: poate administra conturi si date pe
-- server. Parola ramane numai in configuratia privata a serverului.
GRANT SELECT, INSERT, UPDATE, DELETE ON *.*
  TO 'kbot_drepturi'@'localhost' WITH GRANT OPTION;

-- Configuratie MANUALA in PYTHON/config.py de pe server (fisier privat):
-- DB_CONFIG_DREPTURI = {
--     "host": DB_CONFIG_NEW["host"],
--     "port": DB_CONFIG_NEW.get("port", 3306),
--     "user": "kbot_drepturi",
--     "password": "PAROLA_ALEASA_MAI_SUS",
-- }
-- Daca API si MariaDB sunt pe masini diferite, alegeti explicit un Host limitat
-- la masina API in CREATE USER / GRANT; nu inlocuiti automat localhost cu %.
-- Protectii suplimentare optionale:
-- DREPTURI_CONTURI_PROTEJATE = ("alt_cont_tehnic", "alt_administrator@firma.ro")

-- Dupa configurare si publicarea surselor + recompilarea KBotAdmin:
-- Drepturi -> selectati contul -> Aplica drepturile MariaDB.
-- Nu exista migratie globala automata a conturilor existente.
