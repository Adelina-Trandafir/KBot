-- @title Istoric / CodAngajament
-- @description Arată toate rândurile din istoric ale unui cod de angajament: rândul din FX_Istoric și, sub el, ce se leagă de el prin IDH (rezervări cu revizia DDF, plăți, recepții, antetele recepțiilor, rândurile și indicatorii lor). Ultimele două coloane, descriere și observatii, arată pe fiecare rând textul întreg din FX_Istoric.Descriere și FX_Istoric.Observatii ale rândului de istoric de care se leagă. Rezultatul e ordonat pe rândul de istoric, apoi pe tabel.
-- @param cod | Cod angajament | text | Codul angajamentului, exact ca în FX_Istoric.CodAngajament, de exemplu AAB2SGAKRH4.
SELECT i.ID AS istoric_id, 0 AS ordine, 'FX_Istoric' AS tabel, '(randul in sine)' AS prin_ce_se_leaga,
       CONCAT('ID=', i.ID) AS cheie, i.DataFX AS data, i.TipRand AS tip, NULL AS valoare,
       CONCAT_WS(' | ', CONCAT('marcaj IDREV=', IFNULL(i.IDREV, '-')),
                 CONCAT('rezervare I/D=', IFNULL(i.Val_Rezervare_I, '-'), '/', IFNULL(i.Val_Rezervare_D, '-')),
                 CONCAT('ang.legal=', IFNULL(i.Val_AngLeg, '-')),
                 CONCAT('receptie=', IFNULL(i.Val_Receptie, '-')),
                 CONCAT('plata=', IFNULL(i.Val_Plata, '-')),
                 LEFT(IFNULL(i.Descriere, ''), 120)) AS detalii,
       i.Descriere AS descriere, i.Observatii AS observatii
FROM FX_Istoric i WHERE i.CodAngajament = :cod

UNION ALL
SELECT r.IDH, 1, 'FX_Rezervari', 'FX_Rezervari.IDH = FX_Istoric.ID', CONCAT('IDRZ=', r.IDRZ),
       r.DataRezervare, IF(r.EInitiala, 'Initiala', IF(r.EMarire, 'Marire', 'Micsorare')),
       IF(r.EInitiala, r.R_Initiala, r.R_Valoare),
       CONCAT('AreDDF=', r.AreDDF, ' | IDREV=', IFNULL(r.IDREV, '-'),
              IFNULL(CONCAT(' -> FX_DDF_REV: IDDF=', v.IDDF, ', NumarRev=', v.NumarRev), '')), ih.Descriere, ih.Observatii
FROM FX_Rezervari r LEFT JOIN FX_DDF_REV v ON v.IDREV = r.IDREV JOIN FX_Istoric ih ON ih.ID = r.IDH
WHERE r.IDH IN (SELECT ID FROM FX_Istoric WHERE CodAngajament = :cod)

UNION ALL
SELECT p.IDH, 2, 'FX_Plati', 'FX_Plati.IDH = FX_Istoric.ID', CONCAT('IdPlataFX=', p.IdPlataFX),
       p.Data_plata, p.Tip, p.Suma,
       CONCAT('NrOP=', IFNULL(p.NrOP, '-'), ' | IdOP=', IFNULL(p.IdOP, '-'), ' | IDREV=', IFNULL(p.IDREV, '-')), ih.Descriere, ih.Observatii
FROM FX_Plati p JOIN FX_Istoric ih ON ih.ID = p.IDH
WHERE p.IDH IN (SELECT ID FROM FX_Istoric WHERE CodAngajament = :cod)

UNION ALL
SELECT x.IDH, 3, 'FX_Receptii', 'FX_Receptii.IDH = FX_Istoric.ID', CONCAT('IDR=', x.IDR, ' (IDRH=', IFNULL(x.IDRH, '-'), ')'),
       x.Data, x.Tip, x.Valoare,
       CONCAT('IDREV=', IFNULL(x.IDREV, '-'), ' | SumaPlatita=', IFNULL(x.SumaPlatita, '-')), ih.Descriere, ih.Observatii
FROM FX_Receptii x JOIN FX_Istoric ih ON ih.ID = x.IDH
WHERE x.IDH IN (SELECT ID FROM FX_Istoric WHERE CodAngajament = :cod)

UNION ALL
SELECT h.IDH, 4, 'FX_Receptii_H', 'FX_Receptii_H.IDH = FX_Istoric.ID', CONCAT('IDRH=', h.IDRH, ' (IDRR=', IFNULL(h.IDRR, '-'), ')'),
       h.DataH, h.TipReceptie, h.Total,
       CONCAT('Sters=', IFNULL(h.Sters, '-'), ' | EsteStergere=', IFNULL(h.EsteStergere, '-'), ' | ', IFNULL(h.Descriere, '')), ih.Descriere, ih.Observatii
FROM FX_Receptii_H h JOIN FX_Istoric ih ON ih.ID = h.IDH
WHERE h.IDH IN (SELECT ID FROM FX_Istoric WHERE CodAngajament = :cod)

UNION ALL
SELECT h.IDH, 5, 'FX_Receptii_R', 'FX_Receptii_H.IDRR = FX_Receptii_R.IDRR', CONCAT('IDRR=', rr.IDRR),
       rr.DataR, rr.Tip, rr.SumaAntet,
       CONCAT('Sters=', IFNULL(rr.Sters, '-'), ' | ', IFNULL(rr.Descriere, '')), ih.Descriere, ih.Observatii
FROM FX_Receptii_H h JOIN FX_Receptii_R rr ON rr.IDRR = h.IDRR JOIN FX_Istoric ih ON ih.ID = h.IDH
WHERE h.IDH IN (SELECT ID FROM FX_Istoric WHERE CodAngajament = :cod)

UNION ALL
SELECT h.IDH, 6, 'FX_Receptii_RHR', 'FX_Receptii_H.IDRR = FX_Receptii_RHR.IDRR', CONCAT('IDRHR=', rhr.IDRHR),
       NULL, rhr.TipIntern, rhr.Valoare,
       CONCAT('CodIndicator=', IFNULL(rhr.CodIndicator, '-'), ' | IdClsf=', IFNULL(rhr.IdClsf, '-')), ih.Descriere, ih.Observatii
FROM FX_Receptii_H h JOIN FX_Receptii_RHR rhr ON rhr.IDRR = h.IDRR JOIN FX_Istoric ih ON ih.ID = h.IDH
WHERE h.IDH IN (SELECT ID FROM FX_Istoric WHERE CodAngajament = :cod)

ORDER BY 1, 2, 5
