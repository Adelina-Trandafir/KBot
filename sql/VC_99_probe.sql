-- SLICE-VC03-05: proof of the rules on the TARGET engine. Run ONLY on a throw-away test DC database (a clone of
-- AVACONT_SURSA after VC_AVACONT_SURSA.sql and schema sync) where VC_Reguli_DC.sql was ALSO run; rules P3, P8-P10 and
-- P12 are CHECKs / triggers and fail to fail without it. Everything is rolled back at the end. Not run from this chat -- the results column stays empty
-- until somebody runs it. Statements marked EXPECT ERROR must fail with the stated error; if one succeeds the rule is
-- NOT enforced. (Run statement by statement in a client that continues after an error.)

START TRANSACTION;
INSERT INTO VC_Subunitati (SubunitId, Name) VALUES (9001, 'probe A'), (9002, 'probe B');
INSERT INTO VC_Gestiuni (GestiuneId, SubunitId, Name, Type, Zec, Scara) VALUES
  (9101, 9001, 'Mat 1', 'MATERIALE', 4, 4), (9102, 9001, 'Mat 2', 'MATERIALE', 4, 4),
  (9103, 9001, 'OI',    'OI',        2, 3), (9104, 9002, 'Mat B', 'MATERIALE', 4, 4);
INSERT INTO VC_Exercitii (ExercitiuId, SubunitId, GestiuneId, An) VALUES
  (9201, 9001, 9101, 2026), (9202, 9001, 9102, 2026), (9203, 9001, 9103, 2026);
INSERT INTO VC_Produse (ID, SubunitId, GestiuneId) VALUES (9301, 9001, 9101);
INSERT INTO VC_ProduseP (IdProdus, SubunitId, GestiuneId, ID, PUTVA) VALUES (9401, 9001, 9101, 9301, 12345);

-- P1 (ok): INT stores a scaled value exactly.
SELECT PUTVA = 12345 AS p1_exact FROM VC_ProduseP WHERE IdProdus = 9401;
-- P2 EXPECT ERROR 1264 (out of range): overflow of INT is refused, not truncated.
UPDATE VC_ProduseP SET PUTVA = 2147483648 WHERE IdProdus = 9401;
-- P3 EXPECT ERROR 4025 / 3819 (CHECK ck_ProduseP_PUTVA_nonneg): a negative price is refused.
UPDATE VC_ProduseP SET PUTVA = -1 WHERE IdProdus = 9401;
-- P4 EXPECT ERROR 1452 (FK): a product of gestiune 9101 cannot be used by a document of gestiune 9102.
INSERT INTO VC_NIR (IdIntrare, SubunitId, GestiuneId, ExercitiuId, IdComisie) VALUES (9501, 9001, 9102, 9202, NULL);
INSERT INTO VC_Intrari (SubunitId, GestiuneId, ExercitiuId, IdIntrare, IdProdus) VALUES (9001, 9102, 9202, 9501, 9401);
-- P5 EXPECT ERROR 1452 (FK): a gestiune that does not belong to the subunit is refused.
INSERT INTO VC_Exercitii (SubunitId, GestiuneId, An) VALUES (9002, 9101, 2025);
-- P6 EXPECT ERROR 1062 (duplicate): a second confirmed import for the same subunit, whatever the hash.
INSERT INTO VC_Imports (SubunitId, ConfirmedBy, ToolVersion, PlanHash) VALUES (9001, 'probe', '0', REPEAT('a', 64));
INSERT INTO VC_Imports (SubunitId, ConfirmedBy, ToolVersion, PlanHash) VALUES (9001, 'probe', '0', REPEAT('b', 64));
-- P7 EXPECT ERROR 1062: a second holder cannot take the same gestiune.
INSERT INTO VC_ContextLock (SubunitId, GestiuneId, HolderEmail, SessionToken, ExpiresAt) VALUES (9001, 9101, 'a@x.test', REPEAT('1', 64), NOW() + INTERVAL 5 MINUTE);
INSERT INTO VC_ContextLock (SubunitId, GestiuneId, HolderEmail, SessionToken, ExpiresAt) VALUES (9001, 9101, 'b@x.test', REPEAT('2', 64), NOW() + INTERVAL 5 MINUTE);
-- P8 EXPECT ERROR 1644 (trigger): the type is frozen once the gestiune holds data.
UPDATE VC_Gestiuni SET Populated = 1 WHERE GestiuneId = 9101;
UPDATE VC_Gestiuni SET Type = 'ALIMENTE' WHERE GestiuneId = 9101;
-- P9 EXPECT ERROR 1644 (trigger): the storage scale is frozen too.
UPDATE VC_Gestiuni SET Scara = 3 WHERE GestiuneId = 9101;
-- P10 EXPECT ERROR 1644 (trigger): OI and MF are never closed.
UPDATE VC_Exercitii SET Closed = 1 WHERE ExercitiuId = 9203;
-- P11 (ok): Materiale may be closed.
UPDATE VC_Exercitii SET Closed = 1 WHERE ExercitiuId = 9201;
-- P12 EXPECT ERROR 4025 / 3819: a cancelled document needs a reason.
UPDATE VC_NIR SET Anulat = 1 WHERE IdIntrare = 9501;
-- P13 EXPECT ERROR 1451 (FK): a nomenclator that is used cannot be deleted (no CASCADE over history).
DELETE FROM VC_ProduseP WHERE IdProdus = 9401;
ROLLBACK;
