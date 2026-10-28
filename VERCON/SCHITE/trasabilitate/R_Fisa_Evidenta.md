# Trasabilitate schite fișe de evidență Materiale și OI

Convenții și limite: [citirea fișelor consolidate](../INDEX.md#fise-consolidate). SLICE-D003-17, 28.10.2026. Secțiunea este coloana întâi.

## Variante și surse

### Trasabilitate schita raport Fisa_Evidenta

Sursa: [Reports/Fisa_Evidenta.txt](../../VERCON_ACCESS/Reports/Fisa_Evidenta.txt) · imagine: [R_Fisa_Evidenta.png](../R_Fisa_Evidenta.png)

- Titlu (Caption): — · Sursa de date (RecordSource): `SELECT Iesiri_H.IDC, Clienti.Denumire AS Primitor, Iesiri_H.NumarBC, Iesiri_H.DataBC, Iesiri.Denumire, Iesiri.UM, Iesiri.Cant, Iesiri.PUTVA, Iesiri.ValoareCUTVA FROM (Clienti INNER JOIN Iesiri_H ON Clienti.IDC = Iesiri_H.IDC) INNER JOIN Iesiri ON Iesiri_H.IdBonConsum = Iesiri.IdBonConsum WHERE (((Left([numarbc],2))<>'BP')) GROUP BY Iesiri_H.IDC, Clienti.Denumire, Iesiri_H.NumarBC, Iesiri_H.DataBC, …` · Latime: 15111 twips · sectiuni: 5 · controale: 50

### Trasabilitate schita raport OI_Fisa_Evidenta

Sursa: [Reports/OI_Fisa_Evidenta.txt](../../VERCON_ACCESS/Reports/OI_Fisa_Evidenta.txt) · imagine: [R_OI_Fisa_Evidenta.png](../R_OI_Fisa_Evidenta.png)

- Titlu (Caption): — · Sursa de date (RecordSource): `SELECT X.Denumire, X.Adresa, X.Masina, OI_Evidenta.Numar, OI_Evidenta.Data, X.CodProdus, X.DenumireProdus, X.UM, X.PUTVA, X.CF, X.VF FROM (SELECT OI_Evidenta.IDC, Min(IIf(Left([numar],2)="BP" Or "BT",[ID],0)) AS IDX, OI_Evidenta.Masina, OI_Evidenta.Denumire, Clienti.Adresa, OI_Evidenta.CodProdus, OI_Evidenta.DenumireProdus, OI_Evidenta.UM, OI_Evidenta.PUTVA, Sum([c0]+[c1]-[c2]) AS CF, Sum([v0]+[v1 …` · Latime: 15111 twips · sectiuni: 5 · controale: 50

## Secțiuni

| Secțiune | Fisa_Evidenta | OI_Fisa_Evidenta |
|---|---|---|
| PageHeaderSection <a id="pageheadersection-inaltime-2839-twips-linia-194"></a><a id="pageheadersection-inaltime-2856-twips-linia-197"></a> | 1 / 2839 / 194 | 1 / 2856 / 197 |
| GroupHeader0 <a id="groupheader0-inaltime-0-twips-linia-956"></a><a id="groupheader0-inaltime-0-twips-linia-960"></a> | 2 / 0 / 956 | 2 / 0 / 960 |
| Detail <a id="detail-inaltime-282-twips-linia-971"></a><a id="detail-inaltime-282-twips-linia-975"></a> | 3 / 282 / 971 | 3 / 282 / 975 |
| GroupFooter0 <a id="groupfooter0-inaltime-72-twips-linia-1406"></a><a id="groupfooter0-inaltime-72-twips-linia-1411"></a> | 4 / 72 / 1406 | 4 / 72 / 1411 |
| PageFooterSection <a id="pagefootersection-inaltime-678-twips-linia-1435"></a><a id="pagefootersection-inaltime-792-twips-linia-1440"></a> | 5 / 678 / 1435 | 5 / 792 / 1440 |

## Controale

| Secțiune | Control | Tip | Text / camp / sursa | Pozitie comună (twips) | Evenimente | Fisa_Evidenta | OI_Fisa_Evidenta |
|---|---|---|---|---|---|---|---|
| PageHeaderSection | Text1 | TextBox | =DLookUp("Firma","caleTemp") | 1151,59 4521x315 |  | 1:204 | 1:207 |
| PageHeaderSection | Label2 | Label | Unitatea | 56,56 1092x315 |  | 2:232 | 2:235 |
| PageHeaderSection | Primitor | TextBox | Primitor | 56,374 5616x405 |  | 3:259 | — |
| PageHeaderSection | Label6 | Label | FIŞĂ DE EVIDENŢĂ A MATERIALELOR DE NATURA OBIECTELOR DE INVENTAR ÎN FOLOSINŢĂ | 6060,60 9030x735 |  | 4:289 | 4:292 |
| PageHeaderSection | Box7 | Rectangle |  | 24,0 5758x907 |  | 5:316 | 5:319 |
| PageHeaderSection | Text8 | TextBox |  | 3411,1128 4533x345 |  | 6:332 | — |
| PageHeaderSection | Label9 | Label | Numele şi prenumele | 216,1128 3195x345 |  | 7:361 | 7:365 / 216,1128 2667x345 |
| PageHeaderSection | Text10 | TextBox |  | 11997,1104 2901x345 |  | 8:388 | 8:392 |
| PageHeaderSection | Label11 | Label | Funcţia | 10656,1104 1341x345 |  | 9:417 | 9:421 |
| PageHeaderSection | Label12 | Label | Denumirea obiectelor (inclusiv seria, dimensiunea, etc.) | 24,1699 4590x1140 |  | 10:444 | 10:448 / 24,1704 4590x1140 |
| PageHeaderSection | Label13 | Label | Nr. inv/ term folosire | 4614,1699 1065x1140 |  | 11:475 | 11:479 / 4614,1704 1065x1140 |
| PageHeaderSection | Label14 | Label | P/U | 5679,1699 870x1140 |  | 12:506 | 12:510 / 5679,1704 870x1140 |
| PageHeaderSection | Label15 | Label | U/M | 6549,1699 870x1140 |  | 13:537 | 13:541 / 6549,1704 870x1140 |
| PageHeaderSection | Label16 | Label | PRIMIRI | 7416,1704 4440x368 |  | 14:568 | 14:572 / 7416,1709 4440x368 |
| PageHeaderSection | Label17 | Label | RESTITUIRI | 11856,1704 3240x368 |  | 15:599 | 15:603 / 11856,1709 3240x368 |
| PageHeaderSection | Label18 | Label | Documentul | 7419,2079 2175x380 |  | 16:630 | 16:634 / 7419,2084 2175x380 |
| PageHeaderSection | Label19 | Label | Data | 7419,2459 975x380 |  | 17:661 | 17:665 / 7419,2464 975x380 |
| PageHeaderSection | Label20 | Label | Felul şi numărul | 8394,2459 1200x380 |  | 18:692 | 18:696 / 8394,2464 1200x380 |
| PageHeaderSection | Label21 | Label | Cant. | 9594,2072 855x767 |  | 19:722 | 19:726 / 9594,2077 855x767 |
| PageHeaderSection | Label22 | Label | Semnătura | 10449,2072 1410x767 |  | 20:753 | 20:757 / 10449,2077 1410x767 |
| PageHeaderSection | Label23 | Label | Documentul | 11859,2079 1485x380 |  | 21:784 | 21:788 / 11859,2084 1485x380 |
| PageHeaderSection | Label24 | Label | Data | 11859,2459 570x380 |  | 22:815 | 22:819 / 11859,2464 570x380 |
| PageHeaderSection | Label25 | Label | Felul şi numărul | 12429,2459 915x380 |  | 23:846 | 23:850 / 12429,2464 915x380 |
| PageHeaderSection | Label26 | Label | Cant. | 13344,2072 720x767 |  | 24:876 | 24:880 / 13344,2077 720x767 |
| PageHeaderSection | Label27 | Label | Semnătura | 14064,2072 1035x767 |  | 25:907 | 25:911 / 14064,2077 1035x767 |
| PageHeaderSection | Box41 | Rectangle |  | 24,0 15070x1583 |  | 26:938 | 26:942 |
| PageHeaderSection | Primitor | TextBox | Denumire | 56,374 5616x405 |  | — | 3:262 |
| PageHeaderSection | Text8 | TextBox | Adresa | 2883,1128 5061x345 |  | — | 6:335 |
| Detail | Denumirex | TextBox | =UCase([Denumire]) | 24,0 4590x282 |  | 1:981 | — |
| Detail | Text30 | TextBox |  | 4614,0 1065x282 |  | 2:1009 | — |
| Detail | PUTVA | TextBox | PUTVA | 5679,0 870x282 |  | 3:1035 | 3:1040 |
| Detail | UM | TextBox | UM | 6549,0 870x282 |  | 4:1065 | 4:1070 |
| Detail | DataBC | TextBox | DataBC | 7419,0 975x282 |  | 5:1092 | — |
| Detail | NumarBC | TextBox | NumarBC | 8394,0 1200x282 |  | 6:1118 | — |
| Detail | Cant | TextBox | Cant | 9594,0 855x282 |  | 7:1146 | — |
| Detail | Text36 | TextBox |  | 10449,0 1410x282 |  | 8:1175 | 8:1180 |
| Detail | Text37 | TextBox |  | 11859,0 570x282 |  | 9:1201 | 9:1206 |
| Detail | Text38 | TextBox |  | 12429,0 915x282 |  | 10:1227 | 10:1232 |
| Detail | Text39 | TextBox |  | 13344,0 720x282 |  | 11:1253 | 11:1258 |
| Detail | Text40 | TextBox |  | 14064,0 1035x282 |  | 12:1280 | 12:1285 |
| Detail | Line43 | Line |  | 24,0 0x282 |  | 13:1306 | 13:1311 |
| Detail | Line44 | Line |  | 4614,0 0x282 |  | 14:1320 | 14:1325 |
| Detail | Line45 | Line |  | 7419,0 0x282 |  | 15:1334 | 15:1339 |
| Detail | Line46 | Line |  | 11859,0 0x282 |  | 16:1348 | 16:1353 |
| Detail | Line47 | Line |  | 15099,0 0x282 |  | 17:1362 | 17:1367 |
| Detail | Line48 | Line |  | 5679,0 0x282 |  | 18:1376 | 18:1381 |
| Detail | Line49 | Line |  | 6549,0 0x282 |  | 19:1390 | 19:1395 |
| Detail | Denumirex | TextBox | =UCase([DenumireProdus]) | 24,0 4590x282 |  | — | 1:985 |
| Detail | Text30 | TextBox | =Left([CodProdus],10) | 4614,0 1065x282 |  | — | 2:1013 |
| Detail | DataBC | TextBox | Data | 7419,0 975x282 |  | — | 5:1097 |
| Detail | NumarBC | TextBox | Numar | 8394,0 1200x282 |  | — | 6:1123 |
| Detail | Cant | TextBox | CF | 9594,0 855x282 |  | — | 7:1151 |
| GroupFooter0 | Box51 | Rectangle |  | 0,0 15094x0 |  | 1:1420 | 1:1425 |
| PageFooterSection | Label42 | Label | 14.3.9 | 10771,453 4320x225 |  | 1:1445 | 1:1450 |
| PageFooterSection | Box50 | Rectangle |  | 0,0 15094x0 |  | 2:1473 | 2:1478 |
| PageFooterSection | Text50 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 12694,0 2400x285 |  | 3:1486 | 3:1491 |
| PageFooterSection | Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,0 2736x477 |  | 4:1517 | 4:1522 |
