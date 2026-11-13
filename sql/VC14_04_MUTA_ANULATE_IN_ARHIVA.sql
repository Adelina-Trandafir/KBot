-- SLICE-VC14-04. Interogare unica pentru KBotAdmin > Interogari unice (TOATE bazele).
-- Muta in VC_ArhivaAnulate si sterge din tabelele VC_ toate documentele VERCON deja anulate (Anulat=1):
-- NIR, FCS, BC, BR, BP, BT, BS, BU si listele de alimente. Dupa rulare ele dispar din arbori, iar numerele lor se elibereaza.
-- Generat din schema reala MariaDB_Schema/AVACONT_SURSA.sql (11.11.2026); ruleaza pe baze sincronizate cu aceasta schema.
-- Fiecare baza ruleaza intr-o tranzactie (tabela se creeaza prima): daca o instructiune pica, in baza respectiva nu se schimba nimic
-- din date si baza apare ca ESUATA. O baza care a rulat deja textul este sarita de runner.
-- Ce se sterge este EXACT ce s-a copiat: fiecare DELETE cere sa existe randul din arhiva al documentului.
-- Un NIR cu storno inca activ si un BC cu retur inca activ NU se muta (raman anulate, neatinse).
-- Numaratorile (VC_Numbering) coboara la cel mai mare numar ramas, doar pentru tipurile la care s-a arhivat ceva.
-- Fara DELIMITER, fara proceduri. Numele de tabele nu sunt calificate cu baza.

SET SESSION group_concat_max_len = 1073741824;

CREATE TABLE IF NOT EXISTS VC_ArhivaAnulate (
    IdArhiva    INT UNSIGNED NOT NULL AUTO_INCREMENT,
    SubunitId   INT UNSIGNED NOT NULL,
    GestiuneId  INT UNSIGNED NOT NULL,
    ExercitiuId INT UNSIGNED NOT NULL,
    Familie     VARCHAR(10)  NOT NULL COMMENT 'nir, storno, bc, br, lista, bp, bt, bs, bu',
    IdDocument  BIGINT       NOT NULL COMMENT 'cheia documentului in momentul anularii (IdIntrare, IdBonConsum, IdLista, IDBP/IDBT/IDBC, ID-ul BU)',
    Numar       VARCHAR(60)  NULL     COMMENT 'numarul asa cum il citea operatorul (BC 13, NIR 707,1, ...)',
    DataDoc     DATETIME     NULL,
    Motiv       VARCHAR(255) NULL,
    AnulatDe    VARCHAR(255) NULL,
    AnulatLa    DATETIME     NULL,
    Continut    LONGTEXT     NOT NULL COMMENT 'JSON: {tabela: [randuri]} cu tot ce s-a sters',
    CreatedAt   DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt   DATETIME     NULL ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (IdArhiva),
    KEY ix_ArhivaAnulate_doc (SubunitId, GestiuneId, ExercitiuId, Familie, IdDocument)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

-- ===== 1. COPIERE IN ARHIVA =====

INSERT INTO VC_ArhivaAnulate (SubunitId, GestiuneId, ExercitiuId, Familie, IdDocument, Numar, DataDoc, Motiv, AnulatDe, AnulatLa, Continut)
SELECT t.SURSA_SUBUNITATE, t.SURSA_GESTIUNE, t.SURSA_EXERCITIU, 'bu', t.ID, CONCAT('BU ', t.ID), t.DATA_BU, t.MOTIV_ANULARE, NULL, NOW(),
  CONCAT('{', '"VC_TransferUnitati":[', JSON_OBJECT('ID', t.`ID`, 'SURSA_SUBUNITATE', t.`SURSA_SUBUNITATE`, 'SURSA_GESTIUNE', t.`SURSA_GESTIUNE`, 'SURSA_EXERCITIU', t.`SURSA_EXERCITIU`, 'IDBT', t.`IDBT`, 'PREDATOR', t.`PREDATOR`, 'DEST_SUBUNITATE', t.`DEST_SUBUNITATE`, 'DEST_GESTIUNE', t.`DEST_GESTIUNE`, 'DEST_EXERCITIU', t.`DEST_EXERCITIU`, 'PRIMITOR', t.`PRIMITOR`, 'DEST_NIR', t.`DEST_NIR`, 'DEST_BP', t.`DEST_BP`, 'DATA_BU', t.`DATA_BU`, 'EXPLICATIE', t.`EXPLICATIE`, 'ANULAT', t.`ANULAT`, 'MOTIV_ANULARE', t.`MOTIV_ANULARE`), ']',
         ',"VC_TransferUnitati_Linii":[', COALESCE((SELECT GROUP_CONCAT(JSON_OBJECT('ID', x.`ID`, 'BU_ID', x.`BU_ID`, 'SURSA_VARIANTA', x.`SURSA_VARIANTA`, 'SURSA_BC', x.`SURSA_BC`, 'DEST_VARIANTA', x.`DEST_VARIANTA`, 'ORIGINE_SUBUNITATE', x.`ORIGINE_SUBUNITATE`, 'ORIGINE_GESTIUNE', x.`ORIGINE_GESTIUNE`, 'ORIGINE_VARIANTA', x.`ORIGINE_VARIANTA`) SEPARATOR ',') FROM VC_TransferUnitati_Linii x WHERE x.BU_ID=t.ID), ''), ']', '}')
FROM VC_TransferUnitati t
WHERE t.ANULAT=1 AND NOT EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=t.SURSA_SUBUNITATE AND a.GestiuneId=t.SURSA_GESTIUNE AND a.ExercitiuId=t.SURSA_EXERCITIU AND a.Familie='bu' AND a.IdDocument=t.ID);

INSERT INTO VC_ArhivaAnulate (SubunitId, GestiuneId, ExercitiuId, Familie, IdDocument, Numar, DataDoc, Motiv, AnulatDe, AnulatLa, Continut)
SELECT h.SubunitId, h.GestiuneId, h.ExercitiuId, IF(COALESCE(h.Storno,0)=1,'storno','nir'), h.IdIntrare, CONCAT(ABS(h.Numar), IF(COALESCE(h.NumarSub,0)=0,'',IF(h.NumarSub%10=0,CONCAT(',',h.NumarSub DIV 10),CONCAT(',',LPAD(h.NumarSub,2,'0'))))), h.Data, h.MotivAnulare, h.AnulatDe, COALESCE(h.AnulatLa, NOW()),
  CONCAT('{', '"VC_NIR":[', JSON_OBJECT('IdIntrare', h.`IdIntrare`, 'SubunitId', h.`SubunitId`, 'GestiuneId', h.`GestiuneId`, 'ExercitiuId', h.`ExercitiuId`, 'IdComisie', h.`IdComisie`, 'IdMasina', h.`IdMasina`, 'IdStorno', h.`IdStorno`, 'NrFact', h.`NrFact`, 'Data', h.`Data`, 'Numar', h.`Numar`, 'Valoare', h.`Valoare`, 'DataFc', h.`DataFc`, 'Storno', h.`Storno`, 'FurnizorV', h.`FurnizorV`, 'IdPartener', h.`IdPartener`, 'CodPartener', h.`CodPartener`, 'CodFiscalSursa', h.`CodFiscalSursa`, 'TipDoc', h.`TipDoc`, 'Anulat', h.`Anulat`, 'MotivAnulare', h.`MotivAnulare`, 'AnulatDe', h.`AnulatDe`, 'AnulatLa', h.`AnulatLa`, 'CreatedAt', h.`CreatedAt`, 'UpdatedAt', h.`UpdatedAt`, 'Version', h.`Version`, 'NumarSub', h.`NumarSub`), ']',
    ',"VC_Intrari":[', COALESCE((SELECT GROUP_CONCAT(JSON_OBJECT('ID', x.`ID`, 'SubunitId', x.`SubunitId`, 'GestiuneId', x.`GestiuneId`, 'ExercitiuId', x.`ExercitiuId`, 'IdIntrare', x.`IdIntrare`, 'IdProdus', x.`IdProdus`, 'IDTVA', x.`IDTVA`, 'Cant', x.`Cant`, 'Pret', x.`Pret`, 'TVA', x.`TVA`, 'Valoare', x.`Valoare`, 'PUTVA', x.`PUTVA`, 'ValoareCUTVA', x.`ValoareCUTVA`, 'DataSalvare', x.`DataSalvare`, 'CreatedAt', x.`CreatedAt`, 'UpdatedAt', x.`UpdatedAt`, 'Version', x.`Version`) SEPARATOR ',') FROM VC_Intrari x WHERE x.SubunitId=h.SubunitId AND x.GestiuneId=h.GestiuneId AND x.ExercitiuId=h.ExercitiuId AND x.IdIntrare=h.IdIntrare), ''), ']',
    ',"VC_IntrariStorno":[', COALESCE((SELECT GROUP_CONCAT(JSON_OBJECT('ID', x.`ID`, 'SubunitId', x.`SubunitId`, 'GestiuneId', x.`GestiuneId`, 'ExercitiuId', x.`ExercitiuId`, 'IdIntrare', x.`IdIntrare`, 'IdProdus', x.`IdProdus`, 'Cant', x.`Cant`, 'PUTVA', x.`PUTVA`, 'DataSalvare', x.`DataSalvare`, 'Anulat', x.`Anulat`, 'MotivAnulare', x.`MotivAnulare`, 'AnulatDe', x.`AnulatDe`, 'AnulatLa', x.`AnulatLa`, 'CreatedAt', x.`CreatedAt`, 'UpdatedAt', x.`UpdatedAt`, 'Version', x.`Version`) SEPARATOR ',') FROM VC_IntrariStorno x WHERE x.SubunitId=h.SubunitId AND x.GestiuneId=h.GestiuneId AND x.ExercitiuId=h.ExercitiuId AND x.IdIntrare=h.IdIntrare), ''), ']',
    ',"VC_NIR_FACTURI":[', COALESCE((SELECT GROUP_CONCAT(JSON_OBJECT('ID', x.`ID`, 'FACTURA_ID', x.`FACTURA_ID`, 'SUBUNIT_ID', x.`SUBUNIT_ID`, 'GESTIUNE_ID', x.`GESTIUNE_ID`, 'EXERCITIU_ID', x.`EXERCITIU_ID`, 'NIR_ID', x.`NIR_ID`, 'TOTAL_SURSA', x.`TOTAL_SURSA`, 'SOURCE_ID', x.`SOURCE_ID`, 'SOURCE_KEY', x.`SOURCE_KEY`) SEPARATOR ',') FROM VC_NIR_FACTURI x WHERE x.SUBUNIT_ID=h.SubunitId AND x.GESTIUNE_ID=h.GestiuneId AND x.EXERCITIU_ID=h.ExercitiuId AND x.NIR_ID=h.IdIntrare), ''), ']',
    ',"VC_FACTURI_REPARTIZARI":[', COALESCE((SELECT GROUP_CONCAT(JSON_OBJECT('ID', x.`ID`, 'NIR_FACTURA_ID', x.`NIR_FACTURA_ID`, 'FACTURA_LINIE_ID', x.`FACTURA_LINIE_ID`, 'SUBUNIT_ID', x.`SUBUNIT_ID`, 'GESTIUNE_ID', x.`GESTIUNE_ID`, 'EXERCITIU_ID', x.`EXERCITIU_ID`, 'INTRARE_ID', x.`INTRARE_ID`, 'CANT', x.`CANT`, 'VALOARE', x.`VALOARE`) SEPARATOR ',') FROM VC_FACTURI_REPARTIZARI x WHERE x.SUBUNIT_ID=h.SubunitId AND x.GESTIUNE_ID=h.GestiuneId AND x.EXERCITIU_ID=h.ExercitiuId AND x.INTRARE_ID IN (SELECT i.ID FROM VC_Intrari i WHERE i.SubunitId=h.SubunitId AND i.GestiuneId=h.GestiuneId AND i.ExercitiuId=h.ExercitiuId AND i.IdIntrare=h.IdIntrare)), ''), ']',
    ',"VC_NIR_CALCULE":[', COALESCE((SELECT GROUP_CONCAT(JSON_OBJECT('SUBUNIT_ID', x.`SUBUNIT_ID`, 'GESTIUNE_ID', x.`GESTIUNE_ID`, 'EXERCITIU_ID', x.`EXERCITIU_ID`, 'INTRARE_ID', x.`INTRARE_ID`, 'TOTAL_BRUT', x.`TOTAL_BRUT`, 'PRET_INITIAL', x.`PRET_INITIAL`, 'ORIGINE_JSON', x.`ORIGINE_JSON`) SEPARATOR ',') FROM VC_NIR_CALCULE x WHERE x.SUBUNIT_ID=h.SubunitId AND x.GESTIUNE_ID=h.GestiuneId AND x.EXERCITIU_ID=h.ExercitiuId AND x.INTRARE_ID IN (SELECT i.ID FROM VC_Intrari i WHERE i.SubunitId=h.SubunitId AND i.GestiuneId=h.GestiuneId AND i.ExercitiuId=h.ExercitiuId AND i.IdIntrare=h.IdIntrare)), ''), ']',
    ',"EF_VERCON_ALOCARI":[', COALESCE((SELECT GROUP_CONCAT(JSON_OBJECT('ID', x.`ID`, 'ID_TRIMITERE', x.`ID_TRIMITERE`, 'ID_LINIE', x.`ID_LINIE`, 'SUBUNIT_ID', x.`SUBUNIT_ID`, 'GESTIUNE_ID', x.`GESTIUNE_ID`, 'EXERCITIU_ID', x.`EXERCITIU_ID`, 'NIR_ID', x.`NIR_ID`, 'NIR_VERSION', x.`NIR_VERSION`, 'INTRARE_ID', x.`INTRARE_ID`, 'PRODUS_PRET_ID', x.`PRODUS_PRET_ID`, 'CANT_FACTURA', x.`CANT_FACTURA`, 'CANT_NIR', x.`CANT_NIR`, 'CANT_SURSA', x.`CANT_SURSA`, 'PRET_SURSA', x.`PRET_SURSA`, 'CREAT_DE', x.`CREAT_DE`, 'CREAT_LA', x.`CREAT_LA`) SEPARATOR ',') FROM EF_VERCON_ALOCARI x WHERE x.SUBUNIT_ID=h.SubunitId AND x.GESTIUNE_ID=h.GestiuneId AND x.EXERCITIU_ID=h.ExercitiuId AND x.NIR_ID=h.IdIntrare), ''), ']',
    ',"EF_PrimiteVerconNir":[', COALESCE((SELECT GROUP_CONCAT(JSON_OBJECT('IdTrimitere', x.`IdTrimitere`, 'SubunitId', x.`SubunitId`, 'GestiuneId', x.`GestiuneId`, 'ExercitiuId', x.`ExercitiuId`, 'IdIntrare', x.`IdIntrare`) SEPARATOR ',') FROM EF_PrimiteVerconNir x WHERE x.SubunitId=h.SubunitId AND x.GestiuneId=h.GestiuneId AND x.ExercitiuId=h.ExercitiuId AND x.IdIntrare=h.IdIntrare), ''), ']', '}')
FROM VC_NIR h
WHERE h.Anulat=1
  AND NOT EXISTS (SELECT 1 FROM VC_NIR s WHERE s.SubunitId=h.SubunitId AND s.GestiuneId=h.GestiuneId AND s.ExercitiuId=h.ExercitiuId AND s.IdStorno=h.IdIntrare AND COALESCE(s.Storno,0)=1 AND s.Anulat=0)
  AND NOT EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=h.SubunitId AND a.GestiuneId=h.GestiuneId AND a.ExercitiuId=h.ExercitiuId AND a.Familie=IF(COALESCE(h.Storno,0)=1,'storno','nir') AND a.IdDocument=h.IdIntrare);

INSERT INTO VC_ArhivaAnulate (SubunitId, GestiuneId, ExercitiuId, Familie, IdDocument, Numar, DataDoc, Motiv, AnulatDe, AnulatLa, Continut)
SELECT h.SubunitId, h.GestiuneId, h.ExercitiuId, IF(COALESCE(h.Retur,0)=1,'br','bc'), h.IdBonConsum, LEFT(h.NumarBC, 60), h.DataBC, h.MotivAnulare, h.AnulatDe, COALESCE(h.AnulatLa, NOW()),
  CONCAT('{', '"VC_Iesiri_H":[', JSON_OBJECT('IdBonConsum', h.`IdBonConsum`, 'SubunitId', h.`SubunitId`, 'GestiuneId', h.`GestiuneId`, 'ExercitiuId', h.`ExercitiuId`, 'IdMasina', h.`IdMasina`, 'IdRetur', h.`IdRetur`, 'IDC', h.`IDC`, 'NumarBC', h.`NumarBC`, 'DataBC', h.`DataBC`, 'ValoareBC', h.`ValoareBC`, 'Expl', h.`Expl`, 'Retur', h.`Retur`, 'DataSalvare', h.`DataSalvare`, 'Anulat', h.`Anulat`, 'MotivAnulare', h.`MotivAnulare`, 'AnulatDe', h.`AnulatDe`, 'AnulatLa', h.`AnulatLa`, 'CreatedAt', h.`CreatedAt`, 'UpdatedAt', h.`UpdatedAt`, 'Version', h.`Version`), ']',
    ',"VC_Iesiri":[', COALESCE((SELECT GROUP_CONCAT(JSON_OBJECT('ID', x.`ID`, 'SubunitId', x.`SubunitId`, 'GestiuneId', x.`GestiuneId`, 'ExercitiuId', x.`ExercitiuId`, 'IdBonConsum', x.`IdBonConsum`, 'IdProdus', x.`IdProdus`, 'Cant', x.`Cant`, 'PUTVA', x.`PUTVA`, 'ValoareCUTVA', x.`ValoareCUTVA`, 'DataSalvare', x.`DataSalvare`, 'CreatedAt', x.`CreatedAt`, 'UpdatedAt', x.`UpdatedAt`, 'Version', x.`Version`) SEPARATOR ',') FROM VC_Iesiri x WHERE x.SubunitId=h.SubunitId AND x.GestiuneId=h.GestiuneId AND x.ExercitiuId=h.ExercitiuId AND x.IdBonConsum=h.IdBonConsum), ''), ']',
    ',"VC_IesiriRetur":[', COALESCE((SELECT GROUP_CONCAT(JSON_OBJECT('ID', x.`ID`, 'SubunitId', x.`SubunitId`, 'GestiuneId', x.`GestiuneId`, 'ExercitiuId', x.`ExercitiuId`, 'IdBonConsum', x.`IdBonConsum`, 'IdProdus', x.`IdProdus`, 'Cant', x.`Cant`, 'PUTVA', x.`PUTVA`, 'DataSalvare', x.`DataSalvare`, 'CreatedAt', x.`CreatedAt`, 'UpdatedAt', x.`UpdatedAt`, 'Version', x.`Version`) SEPARATOR ',') FROM VC_IesiriRetur x WHERE x.SubunitId=h.SubunitId AND x.GestiuneId=h.GestiuneId AND x.ExercitiuId=h.ExercitiuId AND x.IdBonConsum=h.IdBonConsum), ''), ']',
    ',"VC_LegBCLA":[', COALESCE((SELECT GROUP_CONCAT(JSON_OBJECT('RowId', x.`RowId`, 'SubunitId', x.`SubunitId`, 'GestiuneId', x.`GestiuneId`, 'ExercitiuId', x.`ExercitiuId`, 'IdBonConsum', x.`IdBonConsum`, 'IdLista', x.`IdLista`, 'CreatedAt', x.`CreatedAt`, 'UpdatedAt', x.`UpdatedAt`, 'Version', x.`Version`) SEPARATOR ',') FROM VC_LegBCLA x WHERE x.SubunitId=h.SubunitId AND x.GestiuneId=h.GestiuneId AND x.ExercitiuId=h.ExercitiuId AND x.IdBonConsum=h.IdBonConsum), ''), ']', '}')
FROM VC_Iesiri_H h
WHERE h.Anulat=1
  AND NOT EXISTS (SELECT 1 FROM VC_Iesiri_H r WHERE r.SubunitId=h.SubunitId AND r.GestiuneId=h.GestiuneId AND r.ExercitiuId=h.ExercitiuId AND r.IdRetur=h.IdBonConsum AND COALESCE(r.Retur,0)=1 AND r.Anulat=0)
  AND NOT EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=h.SubunitId AND a.GestiuneId=h.GestiuneId AND a.ExercitiuId=h.ExercitiuId AND a.Familie=IF(COALESCE(h.Retur,0)=1,'br','bc') AND a.IdDocument=h.IdBonConsum);

INSERT INTO VC_ArhivaAnulate (SubunitId, GestiuneId, ExercitiuId, Familie, IdDocument, Numar, DataDoc, Motiv, AnulatDe, AnulatLa, Continut)
SELECT h.SubunitId, h.GestiuneId, h.ExercitiuId, 'lista', h.IdLista, h.NumarLA, h.DATALA, h.MotivAnulare, h.AnulatDe, COALESCE(h.AnulatLa, NOW()),
  CONCAT('{', '"VC_LABC":[', JSON_OBJECT('IdLista', h.`IdLista`, 'SubunitId', h.`SubunitId`, 'GestiuneId', h.`GestiuneId`, 'ExercitiuId', h.`ExercitiuId`, 'ID', h.`ID`, 'NumarLA', h.`NumarLA`, 'DATALA', h.`DATALA`, 'ValoareBon', h.`ValoareBon`, 'DEP', h.`DEP`, 'ValoareLista', h.`ValoareLista`, 'Anulat', h.`Anulat`, 'MotivAnulare', h.`MotivAnulare`, 'AnulatDe', h.`AnulatDe`, 'AnulatLa', h.`AnulatLa`, 'CreatedAt', h.`CreatedAt`, 'UpdatedAt', h.`UpdatedAt`, 'Version', h.`Version`), ']',
    ',"VC_LegBCLA":[', COALESCE((SELECT GROUP_CONCAT(JSON_OBJECT('RowId', x.`RowId`, 'SubunitId', x.`SubunitId`, 'GestiuneId', x.`GestiuneId`, 'ExercitiuId', x.`ExercitiuId`, 'IdBonConsum', x.`IdBonConsum`, 'IdLista', x.`IdLista`, 'CreatedAt', x.`CreatedAt`, 'UpdatedAt', x.`UpdatedAt`, 'Version', x.`Version`) SEPARATOR ',') FROM VC_LegBCLA x WHERE x.SubunitId=h.SubunitId AND x.GestiuneId=h.GestiuneId AND x.ExercitiuId=h.ExercitiuId AND x.IdLista=h.IdLista), ''), ']',
    ',"VC_Cina":[', COALESCE((SELECT GROUP_CONCAT(JSON_OBJECT('Id', x.`Id`, 'SubunitId', x.`SubunitId`, 'GestiuneId', x.`GestiuneId`, 'ExercitiuId', x.`ExercitiuId`, 'IdLista', x.`IdLista`, 'IdFel', x.`IdFel`, 'Tip', x.`Tip`, 'Explicatie', x.`Explicatie`, 'CreatedAt', x.`CreatedAt`, 'UpdatedAt', x.`UpdatedAt`, 'Version', x.`Version`) SEPARATOR ',') FROM VC_Cina x WHERE x.SubunitId=h.SubunitId AND x.GestiuneId=h.GestiuneId AND x.ExercitiuId=h.ExercitiuId AND x.IdLista=h.IdLista), ''), ']',
    ',"VC_Dejun":[', COALESCE((SELECT GROUP_CONCAT(JSON_OBJECT('Id', x.`Id`, 'SubunitId', x.`SubunitId`, 'GestiuneId', x.`GestiuneId`, 'ExercitiuId', x.`ExercitiuId`, 'IdLista', x.`IdLista`, 'IdFel', x.`IdFel`, 'Tip', x.`Tip`, 'Explicatie', x.`Explicatie`, 'CreatedAt', x.`CreatedAt`, 'UpdatedAt', x.`UpdatedAt`, 'Version', x.`Version`) SEPARATOR ',') FROM VC_Dejun x WHERE x.SubunitId=h.SubunitId AND x.GestiuneId=h.GestiuneId AND x.ExercitiuId=h.ExercitiuId AND x.IdLista=h.IdLista), ''), ']',
    ',"VC_MicDejun":[', COALESCE((SELECT GROUP_CONCAT(JSON_OBJECT('Id', x.`Id`, 'SubunitId', x.`SubunitId`, 'GestiuneId', x.`GestiuneId`, 'ExercitiuId', x.`ExercitiuId`, 'IdLista', x.`IdLista`, 'IdFel', x.`IdFel`, 'Tip', x.`Tip`, 'Explicatie', x.`Explicatie`, 'CreatedAt', x.`CreatedAt`, 'UpdatedAt', x.`UpdatedAt`, 'Version', x.`Version`) SEPARATOR ',') FROM VC_MicDejun x WHERE x.SubunitId=h.SubunitId AND x.GestiuneId=h.GestiuneId AND x.ExercitiuId=h.ExercitiuId AND x.IdLista=h.IdLista), ''), ']',
    ',"VC_ListaAlimente":[', COALESCE((SELECT GROUP_CONCAT(JSON_OBJECT('ID', x.`ID`, 'SubunitId', x.`SubunitId`, 'GestiuneId', x.`GestiuneId`, 'ExercitiuId', x.`ExercitiuId`, 'IDLISTA', x.`IDLISTA`, 'MicDejun_P', x.`MicDejun_P`, 'VMD', x.`VMD`, 'MicDejun_V', x.`MicDejun_V`, 'MicDejun_D', x.`MicDejun_D`, 'Dejun_P', x.`Dejun_P`, 'VD', x.`VD`, 'Dejun_V', x.`Dejun_V`, 'Dejun_D', x.`Dejun_D`, 'Cina_P', x.`Cina_P`, 'VC', x.`VC`, 'Cina_V', x.`Cina_V`, 'Cina_D', x.`Cina_D`, 'Comentarii', x.`Comentarii`, 'CreatedAt', x.`CreatedAt`, 'UpdatedAt', x.`UpdatedAt`, 'Version', x.`Version`) SEPARATOR ',') FROM VC_ListaAlimente x WHERE x.SubunitId=h.SubunitId AND x.GestiuneId=h.GestiuneId AND x.ExercitiuId=h.ExercitiuId AND x.IDLISTA=h.IdLista), ''), ']', '}')
FROM VC_LABC h
WHERE h.Anulat=1 AND NOT EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=h.SubunitId AND a.GestiuneId=h.GestiuneId AND a.ExercitiuId=h.ExercitiuId AND a.Familie='lista' AND a.IdDocument=h.IdLista);

INSERT INTO VC_ArhivaAnulate (SubunitId, GestiuneId, ExercitiuId, Familie, IdDocument, Numar, DataDoc, Motiv, AnulatDe, AnulatLa, Continut)
SELECT h.SubunitId, h.GestiuneId, h.ExercitiuId, 'bp', h.k, LEFT(h.n, 60), h.d, h.m, h.u, COALESCE(h.l, NOW()),
  CONCAT('{', '"VC_OI_Predare":[', COALESCE((SELECT GROUP_CONCAT(JSON_OBJECT('ID', x.`ID`, 'SubunitId', x.`SubunitId`, 'GestiuneId', x.`GestiuneId`, 'ExercitiuId', x.`ExercitiuId`, 'IDBP', x.`IDBP`, 'IdBonConsum', x.`IdBonConsum`, 'IdMasina', x.`IdMasina`, 'IdMasinaP', x.`IdMasinaP`, 'IDC_BP', x.`IDC_BP`, 'IdProdus', x.`IdProdus`, 'NRBP', x.`NRBP`, 'DATABP', x.`DATABP`, 'Cant', x.`Cant`, 'PUTVA', x.`PUTVA`, 'ValoareCUTVA', x.`ValoareCUTVA`, 'Expl', x.`Expl`, 'DataSalvare', x.`DataSalvare`, 'Anulat', x.`Anulat`, 'MotivAnulare', x.`MotivAnulare`, 'AnulatDe', x.`AnulatDe`, 'AnulatLa', x.`AnulatLa`, 'CreatedAt', x.`CreatedAt`, 'UpdatedAt', x.`UpdatedAt`, 'Version', x.`Version`) SEPARATOR ',') FROM VC_OI_Predare x WHERE x.SubunitId=h.SubunitId AND x.GestiuneId=h.GestiuneId AND x.ExercitiuId=h.ExercitiuId AND x.IDBP=h.k), ''), ']', '}')
FROM (SELECT SubunitId, GestiuneId, ExercitiuId, IDBP AS k, MIN(NRBP) AS n, MIN(DATABP) AS d, MIN(MotivAnulare) AS m, MIN(AnulatDe) AS u, MAX(AnulatLa) AS l
      FROM VC_OI_Predare WHERE IDBP IS NOT NULL GROUP BY SubunitId, GestiuneId, ExercitiuId, IDBP HAVING MIN(Anulat)=1) h
WHERE NOT EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=h.SubunitId AND a.GestiuneId=h.GestiuneId AND a.ExercitiuId=h.ExercitiuId AND a.Familie='bp' AND a.IdDocument=h.k);

INSERT INTO VC_ArhivaAnulate (SubunitId, GestiuneId, ExercitiuId, Familie, IdDocument, Numar, DataDoc, Motiv, AnulatDe, AnulatLa, Continut)
SELECT h.SubunitId, h.GestiuneId, h.ExercitiuId, 'bt', h.k, LEFT(h.n, 60), h.d, h.m, h.u, COALESCE(h.l, NOW()),
  CONCAT('{', '"VC_OI_Transfer":[', COALESCE((SELECT GROUP_CONCAT(JSON_OBJECT('ID', x.`ID`, 'SubunitId', x.`SubunitId`, 'GestiuneId', x.`GestiuneId`, 'ExercitiuId', x.`ExercitiuId`, 'IDBT', x.`IDBT`, 'IdMasina', x.`IdMasina`, 'IDC_BT', x.`IDC_BT`, 'IDC_OLD', x.`IDC_OLD`, 'IdProdus', x.`IdProdus`, 'NRBT', x.`NRBT`, 'DATABT', x.`DATABT`, 'Cant', x.`Cant`, 'PUTVA', x.`PUTVA`, 'ValoareCUTVA', x.`ValoareCUTVA`, 'Expl', x.`Expl`, 'DataSalvare', x.`DataSalvare`, 'Anulat', x.`Anulat`, 'MotivAnulare', x.`MotivAnulare`, 'AnulatDe', x.`AnulatDe`, 'AnulatLa', x.`AnulatLa`, 'CreatedAt', x.`CreatedAt`, 'UpdatedAt', x.`UpdatedAt`, 'Version', x.`Version`, 'TRANSFER_SENS', x.`TRANSFER_SENS`, 'PERECHE_SUBUNITATE', x.`PERECHE_SUBUNITATE`, 'PERECHE_GESTIUNE', x.`PERECHE_GESTIUNE`, 'PERECHE_EXERCITIU', x.`PERECHE_EXERCITIU`, 'PERECHE_BON', x.`PERECHE_BON`) SEPARATOR ',') FROM VC_OI_Transfer x WHERE x.SubunitId=h.SubunitId AND x.GestiuneId=h.GestiuneId AND x.ExercitiuId=h.ExercitiuId AND x.IDBT=h.k), ''), ']', '}')
FROM (SELECT SubunitId, GestiuneId, ExercitiuId, IDBT AS k, MIN(NRBT) AS n, MIN(DATABT) AS d, MIN(MotivAnulare) AS m, MIN(AnulatDe) AS u, MAX(AnulatLa) AS l
      FROM VC_OI_Transfer WHERE IDBT IS NOT NULL GROUP BY SubunitId, GestiuneId, ExercitiuId, IDBT HAVING MIN(Anulat)=1) h
WHERE NOT EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=h.SubunitId AND a.GestiuneId=h.GestiuneId AND a.ExercitiuId=h.ExercitiuId AND a.Familie='bt' AND a.IdDocument=h.k);

INSERT INTO VC_ArhivaAnulate (SubunitId, GestiuneId, ExercitiuId, Familie, IdDocument, Numar, DataDoc, Motiv, AnulatDe, AnulatLa, Continut)
SELECT h.SubunitId, h.GestiuneId, h.ExercitiuId, 'bs', h.k, LEFT(h.n, 60), h.d, h.m, h.u, COALESCE(h.l, NOW()),
  CONCAT('{', '"VC_OI_Casare":[', COALESCE((SELECT GROUP_CONCAT(JSON_OBJECT('ID', x.`ID`, 'SubunitId', x.`SubunitId`, 'GestiuneId', x.`GestiuneId`, 'ExercitiuId', x.`ExercitiuId`, 'IDBC', x.`IDBC`, 'IdMasina', x.`IdMasina`, 'IDC_BC', x.`IDC_BC`, 'IdProdus', x.`IdProdus`, 'NRBC', x.`NRBC`, 'DATABC', x.`DATABC`, 'Cant', x.`Cant`, 'PUTVA', x.`PUTVA`, 'ValoareCUTVA', x.`ValoareCUTVA`, 'Expl', x.`Expl`, 'DataSalvare', x.`DataSalvare`, 'Anulat', x.`Anulat`, 'MotivAnulare', x.`MotivAnulare`, 'AnulatDe', x.`AnulatDe`, 'AnulatLa', x.`AnulatLa`, 'CreatedAt', x.`CreatedAt`, 'UpdatedAt', x.`UpdatedAt`, 'Version', x.`Version`) SEPARATOR ',') FROM VC_OI_Casare x WHERE x.SubunitId=h.SubunitId AND x.GestiuneId=h.GestiuneId AND x.ExercitiuId=h.ExercitiuId AND x.IDBC=h.k), ''), ']', '}')
FROM (SELECT SubunitId, GestiuneId, ExercitiuId, IDBC AS k, MIN(NRBC) AS n, MIN(DATABC) AS d, MIN(MotivAnulare) AS m, MIN(AnulatDe) AS u, MAX(AnulatLa) AS l
      FROM VC_OI_Casare WHERE IDBC IS NOT NULL GROUP BY SubunitId, GestiuneId, ExercitiuId, IDBC HAVING MIN(Anulat)=1) h
WHERE NOT EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=h.SubunitId AND a.GestiuneId=h.GestiuneId AND a.ExercitiuId=h.ExercitiuId AND a.Familie='bs' AND a.IdDocument=h.k);

-- ===== 2. STERGERE (doar ce s-a copiat; copiii inaintea parintilor) =====

DELETE x FROM VC_TransferUnitati_Linii x JOIN VC_TransferUnitati t ON t.ID=x.BU_ID
WHERE t.ANULAT=1 AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=t.SURSA_SUBUNITATE AND a.GestiuneId=t.SURSA_GESTIUNE AND a.ExercitiuId=t.SURSA_EXERCITIU AND a.Familie='bu' AND a.IdDocument=t.ID);

DELETE t FROM VC_TransferUnitati t
WHERE t.ANULAT=1 AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=t.SURSA_SUBUNITATE AND a.GestiuneId=t.SURSA_GESTIUNE AND a.ExercitiuId=t.SURSA_EXERCITIU AND a.Familie='bu' AND a.IdDocument=t.ID);

DELETE x FROM VC_FACTURI_REPARTIZARI x JOIN VC_Intrari i ON i.SubunitId=x.SUBUNIT_ID AND i.GestiuneId=x.GESTIUNE_ID AND i.ExercitiuId=x.EXERCITIU_ID AND i.ID=x.INTRARE_ID
JOIN VC_NIR h ON i.SubunitId=h.SubunitId AND i.GestiuneId=h.GestiuneId AND i.ExercitiuId=h.ExercitiuId AND h.IdIntrare=i.IdIntrare
WHERE h.Anulat=1 AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=h.SubunitId AND a.GestiuneId=h.GestiuneId AND a.ExercitiuId=h.ExercitiuId AND a.Familie=IF(COALESCE(h.Storno,0)=1,'storno','nir') AND a.IdDocument=h.IdIntrare);

DELETE x FROM VC_NIR_FACTURI x JOIN VC_NIR h ON x.SUBUNIT_ID=h.SubunitId AND x.GESTIUNE_ID=h.GestiuneId AND x.EXERCITIU_ID=h.ExercitiuId AND x.NIR_ID=h.IdIntrare WHERE h.Anulat=1 AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=h.SubunitId AND a.GestiuneId=h.GestiuneId AND a.ExercitiuId=h.ExercitiuId AND a.Familie=IF(COALESCE(h.Storno,0)=1,'storno','nir') AND a.IdDocument=h.IdIntrare);

DELETE x FROM VC_NIR_CALCULE x JOIN VC_Intrari i ON i.SubunitId=x.SUBUNIT_ID AND i.GestiuneId=x.GESTIUNE_ID AND i.ExercitiuId=x.EXERCITIU_ID AND i.ID=x.INTRARE_ID
JOIN VC_NIR h ON i.SubunitId=h.SubunitId AND i.GestiuneId=h.GestiuneId AND i.ExercitiuId=h.ExercitiuId AND h.IdIntrare=i.IdIntrare
WHERE h.Anulat=1 AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=h.SubunitId AND a.GestiuneId=h.GestiuneId AND a.ExercitiuId=h.ExercitiuId AND a.Familie=IF(COALESCE(h.Storno,0)=1,'storno','nir') AND a.IdDocument=h.IdIntrare);

DELETE x FROM EF_VERCON_ALOCARI x JOIN VC_NIR h ON x.SUBUNIT_ID=h.SubunitId AND x.GESTIUNE_ID=h.GestiuneId AND x.EXERCITIU_ID=h.ExercitiuId AND x.NIR_ID=h.IdIntrare WHERE h.Anulat=1 AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=h.SubunitId AND a.GestiuneId=h.GestiuneId AND a.ExercitiuId=h.ExercitiuId AND a.Familie=IF(COALESCE(h.Storno,0)=1,'storno','nir') AND a.IdDocument=h.IdIntrare);

DELETE x FROM EF_PrimiteVerconNir x JOIN VC_NIR h ON x.SubunitId=h.SubunitId AND x.GestiuneId=h.GestiuneId AND x.ExercitiuId=h.ExercitiuId AND x.IdIntrare=h.IdIntrare WHERE h.Anulat=1 AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=h.SubunitId AND a.GestiuneId=h.GestiuneId AND a.ExercitiuId=h.ExercitiuId AND a.Familie=IF(COALESCE(h.Storno,0)=1,'storno','nir') AND a.IdDocument=h.IdIntrare);

DELETE x FROM VC_Intrari x JOIN VC_NIR h ON x.SubunitId=h.SubunitId AND x.GestiuneId=h.GestiuneId AND x.ExercitiuId=h.ExercitiuId AND x.IdIntrare=h.IdIntrare WHERE h.Anulat=1 AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=h.SubunitId AND a.GestiuneId=h.GestiuneId AND a.ExercitiuId=h.ExercitiuId AND a.Familie=IF(COALESCE(h.Storno,0)=1,'storno','nir') AND a.IdDocument=h.IdIntrare);

DELETE x FROM VC_IntrariStorno x JOIN VC_NIR h ON x.SubunitId=h.SubunitId AND x.GestiuneId=h.GestiuneId AND x.ExercitiuId=h.ExercitiuId AND x.IdIntrare=h.IdIntrare WHERE h.Anulat=1 AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=h.SubunitId AND a.GestiuneId=h.GestiuneId AND a.ExercitiuId=h.ExercitiuId AND a.Familie=IF(COALESCE(h.Storno,0)=1,'storno','nir') AND a.IdDocument=h.IdIntrare);

DELETE h FROM VC_NIR h WHERE h.Anulat=1 AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=h.SubunitId AND a.GestiuneId=h.GestiuneId AND a.ExercitiuId=h.ExercitiuId AND a.Familie=IF(COALESCE(h.Storno,0)=1,'storno','nir') AND a.IdDocument=h.IdIntrare);

DELETE x FROM VC_LegBCLA x JOIN VC_Iesiri_H h ON x.SubunitId=h.SubunitId AND x.GestiuneId=h.GestiuneId AND x.ExercitiuId=h.ExercitiuId AND x.IdBonConsum=h.IdBonConsum WHERE h.Anulat=1 AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=h.SubunitId AND a.GestiuneId=h.GestiuneId AND a.ExercitiuId=h.ExercitiuId AND a.Familie=IF(COALESCE(h.Retur,0)=1,'br','bc') AND a.IdDocument=h.IdBonConsum);

DELETE x FROM VC_Iesiri x JOIN VC_Iesiri_H h ON x.SubunitId=h.SubunitId AND x.GestiuneId=h.GestiuneId AND x.ExercitiuId=h.ExercitiuId AND x.IdBonConsum=h.IdBonConsum WHERE h.Anulat=1 AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=h.SubunitId AND a.GestiuneId=h.GestiuneId AND a.ExercitiuId=h.ExercitiuId AND a.Familie=IF(COALESCE(h.Retur,0)=1,'br','bc') AND a.IdDocument=h.IdBonConsum);

DELETE x FROM VC_IesiriRetur x JOIN VC_Iesiri_H h ON x.SubunitId=h.SubunitId AND x.GestiuneId=h.GestiuneId AND x.ExercitiuId=h.ExercitiuId AND x.IdBonConsum=h.IdBonConsum WHERE h.Anulat=1 AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=h.SubunitId AND a.GestiuneId=h.GestiuneId AND a.ExercitiuId=h.ExercitiuId AND a.Familie=IF(COALESCE(h.Retur,0)=1,'br','bc') AND a.IdDocument=h.IdBonConsum);

DELETE h FROM VC_Iesiri_H h WHERE h.Anulat=1 AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=h.SubunitId AND a.GestiuneId=h.GestiuneId AND a.ExercitiuId=h.ExercitiuId AND a.Familie=IF(COALESCE(h.Retur,0)=1,'br','bc') AND a.IdDocument=h.IdBonConsum);

DELETE x FROM VC_LegBCLA x JOIN VC_LABC h ON x.SubunitId=h.SubunitId AND x.GestiuneId=h.GestiuneId AND x.ExercitiuId=h.ExercitiuId AND x.IdLista=h.IdLista WHERE h.Anulat=1 AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=h.SubunitId AND a.GestiuneId=h.GestiuneId AND a.ExercitiuId=h.ExercitiuId AND a.Familie='lista' AND a.IdDocument=h.IdLista);

DELETE x FROM VC_Cina x JOIN VC_LABC h ON x.SubunitId=h.SubunitId AND x.GestiuneId=h.GestiuneId AND x.ExercitiuId=h.ExercitiuId AND x.IdLista=h.IdLista WHERE h.Anulat=1 AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=h.SubunitId AND a.GestiuneId=h.GestiuneId AND a.ExercitiuId=h.ExercitiuId AND a.Familie='lista' AND a.IdDocument=h.IdLista);

DELETE x FROM VC_Dejun x JOIN VC_LABC h ON x.SubunitId=h.SubunitId AND x.GestiuneId=h.GestiuneId AND x.ExercitiuId=h.ExercitiuId AND x.IdLista=h.IdLista WHERE h.Anulat=1 AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=h.SubunitId AND a.GestiuneId=h.GestiuneId AND a.ExercitiuId=h.ExercitiuId AND a.Familie='lista' AND a.IdDocument=h.IdLista);

DELETE x FROM VC_MicDejun x JOIN VC_LABC h ON x.SubunitId=h.SubunitId AND x.GestiuneId=h.GestiuneId AND x.ExercitiuId=h.ExercitiuId AND x.IdLista=h.IdLista WHERE h.Anulat=1 AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=h.SubunitId AND a.GestiuneId=h.GestiuneId AND a.ExercitiuId=h.ExercitiuId AND a.Familie='lista' AND a.IdDocument=h.IdLista);

DELETE x FROM VC_ListaAlimente x JOIN VC_LABC h ON x.SubunitId=h.SubunitId AND x.GestiuneId=h.GestiuneId AND x.ExercitiuId=h.ExercitiuId AND x.IDLISTA=h.IdLista WHERE h.Anulat=1 AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=h.SubunitId AND a.GestiuneId=h.GestiuneId AND a.ExercitiuId=h.ExercitiuId AND a.Familie='lista' AND a.IdDocument=h.IdLista);

DELETE h FROM VC_LABC h WHERE h.Anulat=1 AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=h.SubunitId AND a.GestiuneId=h.GestiuneId AND a.ExercitiuId=h.ExercitiuId AND a.Familie='lista' AND a.IdDocument=h.IdLista);

DELETE h FROM VC_OI_Predare h WHERE h.Anulat=1 AND h.IDBP IS NOT NULL AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=h.SubunitId AND a.GestiuneId=h.GestiuneId AND a.ExercitiuId=h.ExercitiuId AND a.Familie='bp' AND a.IdDocument=h.IDBP);

DELETE h FROM VC_OI_Transfer h WHERE h.Anulat=1 AND h.IDBT IS NOT NULL AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=h.SubunitId AND a.GestiuneId=h.GestiuneId AND a.ExercitiuId=h.ExercitiuId AND a.Familie='bt' AND a.IdDocument=h.IDBT);

DELETE h FROM VC_OI_Casare h WHERE h.Anulat=1 AND h.IDBC IS NOT NULL AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=h.SubunitId AND a.GestiuneId=h.GestiuneId AND a.ExercitiuId=h.ExercitiuId AND a.Familie='bs' AND a.IdDocument=h.IDBC);

-- ===== 3. NUMARATORI: coboara la cel mai mare numar ramas =====

UPDATE VC_Numbering v SET v.LastNumber=COALESCE((SELECT MAX(CAST(REGEXP_REPLACE(h.NumarBC, '[^0-9]', '') AS UNSIGNED)) FROM VC_Iesiri_H h WHERE h.SubunitId=v.SubunitId AND h.GestiuneId=v.GestiuneId AND h.ExercitiuId=v.ExercitiuId AND COALESCE(h.Retur,0)=0 AND h.NumarBC REGEXP '^[[:space:]]*(BC[[:space:]]*)?[0-9]+[[:space:]]*$'), 0), v.Version=v.Version+1
WHERE v.Kind='BC' AND v.LastNumber > COALESCE((SELECT MAX(CAST(REGEXP_REPLACE(h.NumarBC, '[^0-9]', '') AS UNSIGNED)) FROM VC_Iesiri_H h WHERE h.SubunitId=v.SubunitId AND h.GestiuneId=v.GestiuneId AND h.ExercitiuId=v.ExercitiuId AND COALESCE(h.Retur,0)=0 AND h.NumarBC REGEXP '^[[:space:]]*(BC[[:space:]]*)?[0-9]+[[:space:]]*$'), 0)
  AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=v.SubunitId AND a.GestiuneId=v.GestiuneId AND a.ExercitiuId=v.ExercitiuId AND a.Familie='bc');

UPDATE VC_Numbering v SET v.LastNumber=COALESCE((SELECT MAX(CAST(REGEXP_REPLACE(h.NumarBC, '[^0-9]', '') AS UNSIGNED)) FROM VC_Iesiri_H h WHERE h.SubunitId=v.SubunitId AND h.GestiuneId=v.GestiuneId AND h.ExercitiuId=v.ExercitiuId AND COALESCE(h.Retur,0)=0 AND h.NumarBC REGEXP '^[[:space:]]*(BC[[:space:]]*)?[0-9]+[[:space:]]*$' AND h.IdMasina=CAST(SUBSTRING(v.Kind, 4) AS UNSIGNED)), 0), v.Version=v.Version+1
WHERE v.Kind LIKE 'BC:%' AND v.LastNumber > COALESCE((SELECT MAX(CAST(REGEXP_REPLACE(h.NumarBC, '[^0-9]', '') AS UNSIGNED)) FROM VC_Iesiri_H h WHERE h.SubunitId=v.SubunitId AND h.GestiuneId=v.GestiuneId AND h.ExercitiuId=v.ExercitiuId AND COALESCE(h.Retur,0)=0 AND h.NumarBC REGEXP '^[[:space:]]*(BC[[:space:]]*)?[0-9]+[[:space:]]*$' AND h.IdMasina=CAST(SUBSTRING(v.Kind, 4) AS UNSIGNED)), 0)
  AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=v.SubunitId AND a.GestiuneId=v.GestiuneId AND a.ExercitiuId=v.ExercitiuId AND a.Familie='bc');

UPDATE VC_Numbering v SET v.LastNumber=COALESCE((SELECT MAX(CAST(REGEXP_REPLACE(h.NumarBC, '[^0-9]', '') AS UNSIGNED)) FROM VC_Iesiri_H h WHERE h.SubunitId=v.SubunitId AND h.GestiuneId=v.GestiuneId AND h.ExercitiuId=v.ExercitiuId AND COALESCE(h.Retur,0)=1 AND h.NumarBC REGEXP '^[[:space:]]*(BX[[:space:]]*)?[0-9]+[[:space:]]*$'), 0), v.Version=v.Version+1
WHERE v.Kind='BR' AND v.LastNumber > COALESCE((SELECT MAX(CAST(REGEXP_REPLACE(h.NumarBC, '[^0-9]', '') AS UNSIGNED)) FROM VC_Iesiri_H h WHERE h.SubunitId=v.SubunitId AND h.GestiuneId=v.GestiuneId AND h.ExercitiuId=v.ExercitiuId AND COALESCE(h.Retur,0)=1 AND h.NumarBC REGEXP '^[[:space:]]*(BX[[:space:]]*)?[0-9]+[[:space:]]*$'), 0)
  AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=v.SubunitId AND a.GestiuneId=v.GestiuneId AND a.ExercitiuId=v.ExercitiuId AND a.Familie='br');

UPDATE VC_Numbering v SET v.LastNumber=COALESCE((SELECT MAX(CAST(h.NumarLA AS UNSIGNED)) FROM VC_LABC h WHERE h.SubunitId=v.SubunitId AND h.GestiuneId=v.GestiuneId AND h.ExercitiuId=v.ExercitiuId AND h.NumarLA REGEXP '^[0-9]+$'), 0), v.Version=v.Version+1
WHERE v.Kind='LISTA' AND v.LastNumber > COALESCE((SELECT MAX(CAST(h.NumarLA AS UNSIGNED)) FROM VC_LABC h WHERE h.SubunitId=v.SubunitId AND h.GestiuneId=v.GestiuneId AND h.ExercitiuId=v.ExercitiuId AND h.NumarLA REGEXP '^[0-9]+$'), 0)
  AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=v.SubunitId AND a.GestiuneId=v.GestiuneId AND a.ExercitiuId=v.ExercitiuId AND a.Familie='lista');

UPDATE VC_Numbering v SET v.LastNumber=COALESCE((SELECT MAX(CAST(REGEXP_REPLACE(h.NRBP, '[^0-9]', '') AS UNSIGNED)) FROM VC_OI_Predare h WHERE h.SubunitId=v.SubunitId AND h.GestiuneId=v.GestiuneId AND h.ExercitiuId=v.ExercitiuId AND h.NRBP REGEXP '^[[:space:]]*(BP[[:space:]]*)?[0-9]+[[:space:]]*$'), 0), v.Version=v.Version+1
WHERE v.Kind='BP' AND v.LastNumber > COALESCE((SELECT MAX(CAST(REGEXP_REPLACE(h.NRBP, '[^0-9]', '') AS UNSIGNED)) FROM VC_OI_Predare h WHERE h.SubunitId=v.SubunitId AND h.GestiuneId=v.GestiuneId AND h.ExercitiuId=v.ExercitiuId AND h.NRBP REGEXP '^[[:space:]]*(BP[[:space:]]*)?[0-9]+[[:space:]]*$'), 0)
  AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=v.SubunitId AND a.GestiuneId=v.GestiuneId AND a.ExercitiuId=v.ExercitiuId AND a.Familie='bp');

UPDATE VC_Numbering v SET v.LastNumber=COALESCE((SELECT MAX(CAST(REGEXP_REPLACE(h.NRBT, '[^0-9]', '') AS UNSIGNED)) FROM VC_OI_Transfer h WHERE h.SubunitId=v.SubunitId AND h.GestiuneId=v.GestiuneId AND h.ExercitiuId=v.ExercitiuId AND h.NRBT REGEXP '^[[:space:]]*(BT[[:space:]]*)?[0-9]+[[:space:]]*$'), 0), v.Version=v.Version+1
WHERE v.Kind='BT' AND v.LastNumber > COALESCE((SELECT MAX(CAST(REGEXP_REPLACE(h.NRBT, '[^0-9]', '') AS UNSIGNED)) FROM VC_OI_Transfer h WHERE h.SubunitId=v.SubunitId AND h.GestiuneId=v.GestiuneId AND h.ExercitiuId=v.ExercitiuId AND h.NRBT REGEXP '^[[:space:]]*(BT[[:space:]]*)?[0-9]+[[:space:]]*$'), 0)
  AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=v.SubunitId AND a.GestiuneId=v.GestiuneId AND a.ExercitiuId=v.ExercitiuId AND a.Familie='bt');

UPDATE VC_Numbering v SET v.LastNumber=COALESCE((SELECT MAX(CAST(REGEXP_REPLACE(h.NRBC, '[^0-9]', '') AS UNSIGNED)) FROM VC_OI_Casare h WHERE h.SubunitId=v.SubunitId AND h.GestiuneId=v.GestiuneId AND h.ExercitiuId=v.ExercitiuId AND h.NRBC REGEXP '^[[:space:]]*(BS[[:space:]]*)?[0-9]+[[:space:]]*$'), 0), v.Version=v.Version+1
WHERE v.Kind='BS' AND v.LastNumber > COALESCE((SELECT MAX(CAST(REGEXP_REPLACE(h.NRBC, '[^0-9]', '') AS UNSIGNED)) FROM VC_OI_Casare h WHERE h.SubunitId=v.SubunitId AND h.GestiuneId=v.GestiuneId AND h.ExercitiuId=v.ExercitiuId AND h.NRBC REGEXP '^[[:space:]]*(BS[[:space:]]*)?[0-9]+[[:space:]]*$'), 0)
  AND EXISTS (SELECT 1 FROM VC_ArhivaAnulate a WHERE a.SubunitId=v.SubunitId AND a.GestiuneId=v.GestiuneId AND a.ExercitiuId=v.ExercitiuId AND a.Familie='bs');
