-- SLICE-AD11-03. AvacontPush / Interogari unice: AD_08_portal_parinti_date.
-- Run AFTER AD_08_portal_parinti.sql on AVACONT_SURSA and schema synchronization.
-- Suspend ADE writes during this operation. No persistent schema changes.
-- Legacy conflicts keep their contacts; the affected portal Email is NULL.
-- Existing codes are preserved. No email is sent. Uses the current database.

DROP TEMPORARY TABLE IF EXISTS ad08_parent_contacts;

CREATE TEMPORARY TABLE ad08_parent_contacts AS SELECT TRIM(CNP_Platitor) AS CNP, MAX(NULLIF(LOWER(TRIM(EMail)),'')) AS Email, COUNT(DISTINCT NULLIF(LOWER(TRIM(EMail)),'')) AS EmailCount FROM AD_Platitori_sub WHERE TRIM(CNP_Platitor) REGEXP '^[0-9]{13}$' GROUP BY TRIM(CNP_Platitor);

UPDATE ad08_parent_contacts i SET Email=NULL WHERE i.EmailCount>1 OR EXISTS (SELECT 1 FROM AD_Platitori_sub p WHERE LOWER(TRIM(p.EMail))=i.Email AND COALESCE(TRIM(p.CNP_Platitor),'')<>i.CNP);

UPDATE AD_PortalParents SET Email=NULL;

INSERT INTO AD_PortalParents (CNP,Email) SELECT CNP,Email FROM ad08_parent_contacts WHERE 1=1 ON DUPLICATE KEY UPDATE Email=VALUES(Email);

UPDATE AD_PortalParents i SET CodAccesPortal=LOWER(HEX(RANDOM_BYTES(16))) WHERE i.CodAccesPortal IS NULL AND EXISTS (SELECT 1 FROM AD_Platitori_sub p JOIN AD_Platitori c ON c.IDP=p.IDP AND c.SubunitId=p.SubunitId WHERE TRIM(p.CNP_Platitor)=i.CNP AND COALESCE(c.Plecat,0)=0);

UPDATE AD_Platitori_sub p JOIN AD_PortalParents i ON TRIM(p.CNP_Platitor)=i.CNP SET p.Version=p.Version+IF(NOT(p.CodAccesPortal<=>i.CodAccesPortal),1,0),p.CodAccesPortal=i.CodAccesPortal;

INSERT IGNORE INTO AD_Lock (ID) VALUES (0);

DROP TEMPORARY TABLE ad08_parent_contacts;

