-- Diagnostic READ-ONLY pentru produsul din captura: CALCULATOR PORTABIL ASUS,
-- predat lui BADEA ION prin BP 27 si casat prin BS 5.
-- Nu modifica date sau schema. SET modifica numai variabilele acestei sesiuni.
-- Executati in baza unitatii din pagina, nu in AVACONT_COMUN.
-- Sectiunea 2 este completata cu ID-urile confirmate pentru cazul din captura.
-- Executati sectiunile 2-5 in aceeasi conexiune/sesiune SQL.
-- Numerele afisate BP 27 / BS 5 NU sunt ID-uri interne.

-- 1. Identificarea exacta a contextului si a variantei produsului.
-- Sunt afisate si documentele anulate, cu starea lor explicita.
WITH DOCUMENTE AS (
    SELECT 'BP' AS TIP, SubunitId, GestiuneId, ExercitiuId,
           IDBP AS DOCUMENT_ID, NRBP AS NUMAR, DATE(DATABP) AS DATA_BON,
           IDC_BP AS DETINATOR_ID, IdProdus, IdMasina, Cant, PUTVA, Anulat
    FROM VC_OI_Predare
    UNION ALL
    SELECT 'BS', SubunitId, GestiuneId, ExercitiuId,
           IDBC, NRBC, DATE(DATABC), IDC_BC, IdProdus, IdMasina, Cant, PUTVA, Anulat
    FROM VC_OI_Casare
)
SELECT DATABASE() AS BAZA, D.TIP, D.SubunitId, D.GestiuneId, D.ExercitiuId,
       D.DOCUMENT_ID, D.NUMAR, D.DATA_BON, D.DETINATOR_ID,
       C.Denumire AS DETINATOR, D.IdProdus AS VARIANTA_ID,
       P.DenumireP AS PRODUS, D.IdMasina AS CONT_ID, D.Cant, D.PUTVA, D.Anulat
FROM DOCUMENTE D
JOIN VC_Clienti C ON C.SubunitId=D.SubunitId AND C.GestiuneId=D.GestiuneId
                 AND C.IDC=D.DETINATOR_ID
JOIN VC_ProduseP V ON V.SubunitId=D.SubunitId AND V.GestiuneId=D.GestiuneId
                  AND V.IdProdus=D.IdProdus
JOIN VC_Produse P ON P.SubunitId=V.SubunitId AND P.GestiuneId=V.GestiuneId AND P.ID=V.ID
WHERE TRIM(C.Denumire)='BADEA ION'
  AND P.DenumireP LIKE '%CALCULATOR PORTABIL ASUS%'
ORDER BY D.SubunitId,D.GestiuneId,D.ExercitiuId,D.DATA_BON,D.DOCUMENT_ID;

-- 2. ID-uri confirmate de utilizator din rezultatul sectiunii 1 (000_DEMO).
-- BP 27 are IDBP=12; BS 5 are IDBC=38. Nu sunt numerele afisate ale bonurilor.
SET @DIAG_SUBUNIT = 10;
SET @DIAG_GESTIUNE = 8;
SET @DIAG_EXERCITIU = 9;
SET @DIAG_DETINATOR = 164;
SET @DIAG_VARIANTA = 3878;
SET @DIAG_DATA = DATE('2026-10-18');

SELECT DATABASE() AS BAZA, @DIAG_SUBUNIT AS SUBUNITATE, @DIAG_GESTIUNE AS GESTIUNE,
       @DIAG_EXERCITIU AS EXERCITIU, @DIAG_DETINATOR AS DETINATOR,
       @DIAG_VARIANTA AS VARIANTA, @DIAG_DATA AS DATA_BON,
       CASE WHEN @DIAG_SUBUNIT IS NULL OR @DIAG_GESTIUNE IS NULL
                  OR @DIAG_EXERCITIU IS NULL OR @DIAG_DETINATOR IS NULL
                  OR @DIAG_VARIANTA IS NULL
            THEN 'COMPLETATI ID-URILE INAINTE DE A CONTINUA'
            ELSE 'CONTEXT COMPLETAT' END AS STARE;

-- 3. Bonurile exacte ale detinatorului, inclusiv transferurile in ambele sensuri.
-- Cantitatile sunt cele stocate. Semnul asteptat in registru: BP/BT primit +,
-- BS/BT predat -. Scara numerica nu este modificata de acest diagnostic.
WITH DOCUMENTE AS (
    SELECT 'BP' AS TIP, ID AS LINIE_ID, IDBP AS DOCUMENT_ID, NRBP AS NUMAR,
           DATE(DATABP) AS DATA_BON, IDC_BP AS DETINATOR_ID, IdProdus,
           IdMasina AS CONT_ID, Cant AS CANT_SEMNATA, PUTVA, Anulat
    FROM VC_OI_Predare
    WHERE SubunitId=@DIAG_SUBUNIT AND GestiuneId=@DIAG_GESTIUNE
      AND ExercitiuId=@DIAG_EXERCITIU AND IDC_BP=@DIAG_DETINATOR AND IdProdus=@DIAG_VARIANTA
    UNION ALL
    SELECT 'BT_PRIMIT', ID, IDBT, NRBT, DATE(DATABT), IDC_BT, IdProdus, IdMasina, Cant, PUTVA, Anulat
    FROM VC_OI_Transfer
    WHERE SubunitId=@DIAG_SUBUNIT AND GestiuneId=@DIAG_GESTIUNE
      AND ExercitiuId=@DIAG_EXERCITIU AND IDC_BT=@DIAG_DETINATOR AND IdProdus=@DIAG_VARIANTA
    UNION ALL
    SELECT 'BT_PREDAT', ID, IDBT, NRBT, DATE(DATABT), IDC_OLD, IdProdus, IdMasina, -Cant, PUTVA, Anulat
    FROM VC_OI_Transfer
    WHERE SubunitId=@DIAG_SUBUNIT AND GestiuneId=@DIAG_GESTIUNE
      AND ExercitiuId=@DIAG_EXERCITIU AND IDC_OLD=@DIAG_DETINATOR AND IdProdus=@DIAG_VARIANTA
    UNION ALL
    SELECT 'BS', ID, IDBC, NRBC, DATE(DATABC), IDC_BC, IdProdus, IdMasina, -Cant, PUTVA, Anulat
    FROM VC_OI_Casare
    WHERE SubunitId=@DIAG_SUBUNIT AND GestiuneId=@DIAG_GESTIUNE
      AND ExercitiuId=@DIAG_EXERCITIU AND IDC_BC=@DIAG_DETINATOR AND IdProdus=@DIAG_VARIANTA
)
SELECT *,
       CASE WHEN Anulat=0 AND DATA_BON<=@DIAG_DATA THEN CANT_SEMNATA ELSE 0 END AS CANT_IN_SOLD,
       SUM(CASE WHEN Anulat=0 AND DATA_BON<=@DIAG_DATA THEN CANT_SEMNATA ELSE 0 END) OVER () AS SOLD_DIN_BONURI
FROM DOCUMENTE
ORDER BY DATA_BON,DOCUMENT_ID,LINIE_ID,TIP;

-- 4. Registrul BRUT al variantei, pentru TOTI detinatorii din gestiune.
-- Astfel se vede si o eventuala scadere atribuita altui detinator.
-- Registrul este la nivel de gestiune; nu are ExercitiuId.
SELECT R.IDOI,R.IDBP,R.IDBT,R.IDBC,R.IDBO,R.IDC_PTC,
       C.Denumire AS DETINATOR,R.IdProdus,R.IDM AS CONT_ID,
       M.Masina AS CONT,R.Cant,R.PUTVA,R.ValoareCUTVA
FROM VC_OI_Produse R
LEFT JOIN VC_Clienti C ON C.SubunitId=R.SubunitId AND C.GestiuneId=R.GestiuneId AND C.IDC=R.IDC_PTC
LEFT JOIN VC_Masini M ON M.SubunitId=R.SubunitId AND M.GestiuneId=R.GestiuneId AND M.IdMasina=R.IDM
WHERE R.SubunitId=@DIAG_SUBUNIT AND R.GestiuneId=@DIAG_GESTIUNE AND R.IdProdus=@DIAG_VARIANTA
ORDER BY R.IDC_PTC,R.IDOI;

-- 5. Clasificarea si cheile BRUTE care au demonstrat eroarea din Access.
-- Dupa regula confirmata la 18.10.2026, aplicatia mosteneste contul TC din BP
-- prin IDBO. Acest SELECT continua sa arate IDM stocat, fara normalizare,
-- pentru a putea identifica diferenta sursa/contul efectiv.
-- La intrare, lotul este bonul curent BP/BT; la iesire, lotul este IDBO.
-- O predare cu IDBP+IDBC completate NU este o eroare daca valorile coincid.
-- La fel pentru IDBT+IDBC. Nu impunem exclusivitate intre cele trei coloane.
WITH DOCUMENTE AS (
    SELECT 'bp' AS TIP, IDBP AS DOCUMENT_ID, IdProdus,
           MIN(DATE(DATABP)) AS DATA_MIN,MAX(DATE(DATABP)) AS DATA_MAX
    FROM VC_OI_Predare
    WHERE SubunitId=@DIAG_SUBUNIT AND GestiuneId=@DIAG_GESTIUNE
      AND ExercitiuId=@DIAG_EXERCITIU AND Anulat=0 AND IdProdus=@DIAG_VARIANTA
    GROUP BY IDBP,IdProdus
    UNION ALL
    SELECT 'bt',IDBT,IdProdus,MIN(DATE(DATABT)),MAX(DATE(DATABT))
    FROM VC_OI_Transfer
    WHERE SubunitId=@DIAG_SUBUNIT AND GestiuneId=@DIAG_GESTIUNE
      AND ExercitiuId=@DIAG_EXERCITIU AND Anulat=0 AND IdProdus=@DIAG_VARIANTA
    GROUP BY IDBT,IdProdus
    UNION ALL
    SELECT 'bs',IDBC,IdProdus,MIN(DATE(DATABC)),MAX(DATE(DATABC))
    FROM VC_OI_Casare
    WHERE SubunitId=@DIAG_SUBUNIT AND GestiuneId=@DIAG_GESTIUNE
      AND ExercitiuId=@DIAG_EXERCITIU AND Anulat=0 AND IdProdus=@DIAG_VARIANTA
    GROUP BY IDBC,IdProdus
), REGISTRU AS (
    SELECT R.*,
           CASE WHEN COALESCE(IDBP,0)<>0 THEN 'bp'
                WHEN COALESCE(IDBT,0)<>0 THEN 'bt' ELSE 'bs' END AS TIP,
           CASE WHEN COALESCE(IDBP,0)<>0 THEN IDBP
                WHEN COALESCE(IDBT,0)<>0 THEN IDBT ELSE IDBC END AS DOCUMENT_ID
    FROM VC_OI_Produse R
    WHERE SubunitId=@DIAG_SUBUNIT AND GestiuneId=@DIAG_GESTIUNE
      AND IDC_PTC=@DIAG_DETINATOR AND IdProdus=@DIAG_VARIANTA
), EVENIMENTE AS (
    SELECT R.*,D.DATA_MIN,D.DATA_MAX,
           CASE WHEN R.Cant>0 THEN R.DOCUMENT_ID ELSE R.IDBO END AS LOT_CALCULAT
    FROM REGISTRU R
    LEFT JOIN DOCUMENTE D ON D.TIP=R.TIP AND D.DOCUMENT_ID=R.DOCUMENT_ID AND D.IdProdus=R.IdProdus
)
SELECT IDOI,TIP,DOCUMENT_ID,IDBP,IDBT,IDBC,IDBO,LOT_CALCULAT,
       IDC_PTC,IdProdus,IDM AS CONT_ID,PUTVA,Cant,DATA_MIN,DATA_MAX,
       CASE
           WHEN COALESCE(IDBP,0)<>0 AND COALESCE(IDBT,0)<>0 THEN 'EROARE: BP SI BT SIMULTAN'
           WHEN COALESCE(IDBC,0)<>0 AND IDBC<>DOCUMENT_ID THEN 'EROARE: IDBC CONTRAZICE MARCAJUL'
           WHEN COALESCE(DOCUMENT_ID,0)=0 THEN 'EROARE: LIPSA BON CURENT'
           WHEN DATA_MIN IS NULL THEN 'EROARE: BON ACTIV LIPSA IN EXERCITIUL SELECTAT SAU DATA LIPSA'
           WHEN DATA_MIN<>DATA_MAX THEN 'EROARE: DATE NEUNIFORME'
           WHEN TIP='bp' AND COALESCE(IDBO,0)<>0 THEN 'EROARE: BP CU IDBO'
           WHEN TIP<>'bp' AND Cant<>0 AND COALESCE(IDBO,0)=0 THEN 'EROARE: LIPSA IDBO'
           ELSE 'DE COMPARAT CHEIA LOTULUI SI SEMNUL'
       END AS OBSERVATIE,
       SUM(CASE WHEN DATA_MIN<=@DIAG_DATA THEN Cant ELSE 0 END)
           OVER (PARTITION BY LOT_CALCULAT,IdProdus,IDM,PUTVA) AS SOLD_PE_CHEIA_BRUTA,
       SUM(CASE WHEN DATA_MIN<=@DIAG_DATA THEN Cant ELSE 0 END)
           OVER () AS SOLD_TOTAL_REGISTRU
FROM EVENIMENTE
ORDER BY LOT_CALCULAT,IdProdus,IDM,PUTVA,DATA_MIN,IDOI;

-- Interpretare:
-- * Sectiunea 3: pentru cazul din captura, BP +1 si BS -1 => sold 0.
-- * Sectiunea 4: cautam randul de BS, Cant negativ si detinatorul corect.
-- * Sectiunea 5: BP si BS trebuie sa aiba aceeasi cheie de lot:
--   (LOT_CALCULAT,IdProdus,CONT_ID,PUTVA). IDBO al BS indica ID-ul intern
--   al BP de provenienta, NU numarul afisat 27.
-- * Daca totalul registrului este 0, dar o cheie are +1 si alta -1,
--   rezultatele arata exact coloana care separa intrarea de iesire.
-- * Daca registrul contine numai +1, rezultatele arata ca lipseste scaderea
--   pentru aceasta varianta/detinator, fara sa presupunem cauza lipsei.
-- * Daca cheia corecta are sold 0 si totusi API-ul ofera produsul, urmatorul
--   pas este raspunsul GET /api/vercon/doc/bt/eligible din pagina (holder,
--   date, exclude, context), urmat de asocierea rezultatului in DGV.
-- * Acest fisier NU valideaza toate regulile registrului si NU corecteaza
--   automat date. Soldul pe cheie este la data aleasa; aplicatia verifica
--   separat si minima cronologica a documentelor ulterioare.

-- 6. Urmarirea diferentei de cont CONFIRMATE prin rezultatele utilizatorului:
-- IDOI 1821: BP ID 12, IDM 34, Cant +1;
-- IDOI 1848: BS ID 38, IDBO 12, IDM 33, Cant -1.
-- Nu modificam conturile si nu eliminam contul din cheia lotului.
-- Citim ambele conturi ale BP si cheile Access pastrate de migrator.
SELECT P.ID,P.IDBP,P.NRBP,P.IdProdus,P.IDC_BP,
       P.IdMasina,M.Masina AS DENUMIRE_IdMasina,
       P.IdMasinaP,MP.Masina AS DENUMIRE_IdMasinaP
FROM VC_OI_Predare P
LEFT JOIN VC_Masini M ON M.SubunitId=P.SubunitId AND M.GestiuneId=P.GestiuneId AND M.IdMasina=P.IdMasina
LEFT JOIN VC_Masini MP ON MP.SubunitId=P.SubunitId AND MP.GestiuneId=P.GestiuneId AND MP.IdMasina=P.IdMasinaP
WHERE P.SubunitId=@DIAG_SUBUNIT AND P.GestiuneId=@DIAG_GESTIUNE
  AND P.ExercitiuId=@DIAG_EXERCITIU AND P.ID=1877 AND P.IDBP=12 AND P.IdProdus=@DIAG_VARIANTA;

-- ROW, nu GROUP: cheile primare ale randurilor sursa sunt necesare pentru
-- compararea OI_Produse.IDM si OI_Casare.IdMasina direct in fisierul Access.
SELECT SourceId,TableName,Kind,SourceKey,TargetId
FROM VC_IdMap
WHERE Kind='ROW' AND (
       (TableName='OI_Produse' AND TargetId IN (1821,1848))
    OR (TableName='OI_Predare' AND TargetId=1877)
    OR (TableName='OI_Casare' AND TargetId=6)
    OR (TableName='Masini' AND TargetId IN (33,34))
)
ORDER BY SourceId,TableName,TargetId;
