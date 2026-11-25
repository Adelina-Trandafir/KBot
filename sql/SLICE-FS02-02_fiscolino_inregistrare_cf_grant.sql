-- SLICE-FS02-02. Se aplică de administratorul MariaDB pe serverul AVACONT, după SLICE-FS02-01.
-- Butonul „Înregistrează Cod Fiscal” din FISCOLINO leagă un CF de contul certificatului prin
-- POST /api/fiscolino/efactura/unitate/inregistreaza: primul cont care înregistrează un CF îl primește,
-- iar un alt cont este refuzat (CIF_ALT_CONT) până când administratorul îl leagă în UNITATI_UTILIZATORI.
-- Serviciul inserează în UNITATI și UNITATI_UTILIZATORI; blocarea rândului unității (SELECT ... FOR UPDATE)
-- o face deja, deci nu cere alte drepturi.
--
-- Înlocuiți <FISCOLINO_DB_USER> cu valoarea din /etc/avacont/fiscolino-efactura.env (și gazda contului, dacă nu e '%').
-- Contul rămâne fără acces în afara bazei FISCOLINO.

GRANT INSERT ON `FISCOLINO`.`UNITATI` TO '<FISCOLINO_DB_USER>'@'%';
GRANT INSERT ON `FISCOLINO`.`UNITATI_UTILIZATORI` TO '<FISCOLINO_DB_USER>'@'%';
