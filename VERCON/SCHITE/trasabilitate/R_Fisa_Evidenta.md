# Trasabilitate schita raport Fisa_Evidenta

Sursa: [Reports/Fisa_Evidenta.txt](../../VERCON_ACCESS/Reports/Fisa_Evidenta.txt) · imagine: [R_Fisa_Evidenta.png](../R_Fisa_Evidenta.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT Iesiri_H.IDC, Clienti.Denumire AS Primitor, Iesiri_H.NumarBC, Iesiri_H.DataBC, Iesiri.Denumire, Iesiri.UM, Iesiri.Cant, Iesiri.PUTVA, Iesiri.ValoareCUTVA FROM (Clienti INNER JOIN Iesiri_H ON Clienti.IDC = Iesiri_H.IDC) INNER JOIN Iesiri ON Iesiri_H.IdBonConsum = Iesiri.IdBonConsum WHERE (((Left([numarbc],2))<>'BP')) GROUP BY Iesiri_H.IDC, Clienti.Denumire, Iesiri_H.NumarBC, Iesiri_H.DataBC, …`
- Latime: 15111 twips · sectiuni: 5 · controale: 50

## PageHeaderSection (inaltime 2839 twips, linia 194)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text1 | TextBox | =DLookUp("Firma","caleTemp") | 1151,59 4521x315 | 204 |  |
| Label2 | Label | Unitatea | 56,56 1092x315 | 232 |  |
| Primitor | TextBox | Primitor | 56,374 5616x405 | 259 |  |
| Label6 | Label | FIŞĂ DE EVIDENŢĂ A MATERIALELOR DE NATURA OBIECTELOR DE INVENTAR ÎN FOLOSINŢĂ | 6060,60 9030x735 | 289 |  |
| Box7 | Rectangle |  | 24,0 5758x907 | 316 |  |
| Text8 | TextBox |  | 3411,1128 4533x345 | 332 |  |
| Label9 | Label | Numele şi prenumele | 216,1128 3195x345 | 361 |  |
| Text10 | TextBox |  | 11997,1104 2901x345 | 388 |  |
| Label11 | Label | Funcţia | 10656,1104 1341x345 | 417 |  |
| Label12 | Label | Denumirea obiectelor (inclusiv seria, dimensiunea, etc.) | 24,1699 4590x1140 | 444 |  |
| Label13 | Label | Nr. inv/ term folosire | 4614,1699 1065x1140 | 475 |  |
| Label14 | Label | P/U | 5679,1699 870x1140 | 506 |  |
| Label15 | Label | U/M | 6549,1699 870x1140 | 537 |  |
| Label16 | Label | PRIMIRI | 7416,1704 4440x368 | 568 |  |
| Label17 | Label | RESTITUIRI | 11856,1704 3240x368 | 599 |  |
| Label18 | Label | Documentul | 7419,2079 2175x380 | 630 |  |
| Label19 | Label | Data | 7419,2459 975x380 | 661 |  |
| Label20 | Label | Felul şi numărul | 8394,2459 1200x380 | 692 |  |
| Label21 | Label | Cant. | 9594,2072 855x767 | 722 |  |
| Label22 | Label | Semnătura | 10449,2072 1410x767 | 753 |  |
| Label23 | Label | Documentul | 11859,2079 1485x380 | 784 |  |
| Label24 | Label | Data | 11859,2459 570x380 | 815 |  |
| Label25 | Label | Felul şi numărul | 12429,2459 915x380 | 846 |  |
| Label26 | Label | Cant. | 13344,2072 720x767 | 876 |  |
| Label27 | Label | Semnătura | 14064,2072 1035x767 | 907 |  |
| Box41 | Rectangle |  | 24,0 15070x1583 | 938 |  |

## GroupHeader0 (inaltime 0 twips, linia 956)

_fara controale_

## Detail (inaltime 282 twips, linia 971)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Denumirex | TextBox | =UCase([Denumire]) | 24,0 4590x282 | 981 |  |
| Text30 | TextBox |  | 4614,0 1065x282 | 1009 |  |
| PUTVA | TextBox | PUTVA | 5679,0 870x282 | 1035 |  |
| UM | TextBox | UM | 6549,0 870x282 | 1065 |  |
| DataBC | TextBox | DataBC | 7419,0 975x282 | 1092 |  |
| NumarBC | TextBox | NumarBC | 8394,0 1200x282 | 1118 |  |
| Cant | TextBox | Cant | 9594,0 855x282 | 1146 |  |
| Text36 | TextBox |  | 10449,0 1410x282 | 1175 |  |
| Text37 | TextBox |  | 11859,0 570x282 | 1201 |  |
| Text38 | TextBox |  | 12429,0 915x282 | 1227 |  |
| Text39 | TextBox |  | 13344,0 720x282 | 1253 |  |
| Text40 | TextBox |  | 14064,0 1035x282 | 1280 |  |
| Line43 | Line |  | 24,0 0x282 | 1306 |  |
| Line44 | Line |  | 4614,0 0x282 | 1320 |  |
| Line45 | Line |  | 7419,0 0x282 | 1334 |  |
| Line46 | Line |  | 11859,0 0x282 | 1348 |  |
| Line47 | Line |  | 15099,0 0x282 | 1362 |  |
| Line48 | Line |  | 5679,0 0x282 | 1376 |  |
| Line49 | Line |  | 6549,0 0x282 | 1390 |  |

## GroupFooter0 (inaltime 72 twips, linia 1406)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Box51 | Rectangle |  | 0,0 15094x0 | 1420 |  |

## PageFooterSection (inaltime 678 twips, linia 1435)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label42 | Label | 14.3.9 | 10771,453 4320x225 | 1445 |  |
| Box50 | Rectangle |  | 0,0 15094x0 | 1473 |  |
| Text50 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 12694,0 2400x285 | 1486 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,0 2736x477 | 1517 |  |

