# Trasabilitate schita raport OI_Fisa_Evidenta

Sursa: [Reports/OI_Fisa_Evidenta.txt](../../VERCON_ACCESS/Reports/OI_Fisa_Evidenta.txt) · imagine: [R_OI_Fisa_Evidenta.png](../R_OI_Fisa_Evidenta.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT X.Denumire, X.Adresa, X.Masina, OI_Evidenta.Numar, OI_Evidenta.Data, X.CodProdus, X.DenumireProdus, X.UM, X.PUTVA, X.CF, X.VF FROM (SELECT OI_Evidenta.IDC, Min(IIf(Left([numar],2)="BP" Or "BT",[ID],0)) AS IDX, OI_Evidenta.Masina, OI_Evidenta.Denumire, Clienti.Adresa, OI_Evidenta.CodProdus, OI_Evidenta.DenumireProdus, OI_Evidenta.UM, OI_Evidenta.PUTVA, Sum([c0]+[c1]-[c2]) AS CF, Sum([v0]+[v1 …`
- Latime: 15111 twips · sectiuni: 5 · controale: 50

## PageHeaderSection (inaltime 2856 twips, linia 197)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text1 | TextBox | =DLookUp("Firma","caleTemp") | 1151,59 4521x315 | 207 |  |
| Label2 | Label | Unitatea | 56,56 1092x315 | 235 |  |
| Primitor | TextBox | Denumire | 56,374 5616x405 | 262 |  |
| Label6 | Label | FIŞĂ DE EVIDENŢĂ A MATERIALELOR DE NATURA OBIECTELOR DE INVENTAR ÎN FOLOSINŢĂ | 6060,60 9030x735 | 292 |  |
| Box7 | Rectangle |  | 24,0 5758x907 | 319 |  |
| Text8 | TextBox | Adresa | 2883,1128 5061x345 | 335 |  |
| Label9 | Label | Numele şi prenumele | 216,1128 2667x345 | 365 |  |
| Text10 | TextBox |  | 11997,1104 2901x345 | 392 |  |
| Label11 | Label | Funcţia | 10656,1104 1341x345 | 421 |  |
| Label12 | Label | Denumirea obiectelor (inclusiv seria, dimensiunea, etc.) | 24,1704 4590x1140 | 448 |  |
| Label13 | Label | Nr. inv/ term folosire | 4614,1704 1065x1140 | 479 |  |
| Label14 | Label | P/U | 5679,1704 870x1140 | 510 |  |
| Label15 | Label | U/M | 6549,1704 870x1140 | 541 |  |
| Label16 | Label | PRIMIRI | 7416,1709 4440x368 | 572 |  |
| Label17 | Label | RESTITUIRI | 11856,1709 3240x368 | 603 |  |
| Label18 | Label | Documentul | 7419,2084 2175x380 | 634 |  |
| Label19 | Label | Data | 7419,2464 975x380 | 665 |  |
| Label20 | Label | Felul şi numărul | 8394,2464 1200x380 | 696 |  |
| Label21 | Label | Cant. | 9594,2077 855x767 | 726 |  |
| Label22 | Label | Semnătura | 10449,2077 1410x767 | 757 |  |
| Label23 | Label | Documentul | 11859,2084 1485x380 | 788 |  |
| Label24 | Label | Data | 11859,2464 570x380 | 819 |  |
| Label25 | Label | Felul şi numărul | 12429,2464 915x380 | 850 |  |
| Label26 | Label | Cant. | 13344,2077 720x767 | 880 |  |
| Label27 | Label | Semnătura | 14064,2077 1035x767 | 911 |  |
| Box41 | Rectangle |  | 24,0 15070x1583 | 942 |  |

## GroupHeader0 (inaltime 0 twips, linia 960)

_fara controale_

## Detail (inaltime 282 twips, linia 975)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Denumirex | TextBox | =UCase([DenumireProdus]) | 24,0 4590x282 | 985 |  |
| Text30 | TextBox | =Left([CodProdus],10) | 4614,0 1065x282 | 1013 |  |
| PUTVA | TextBox | PUTVA | 5679,0 870x282 | 1040 |  |
| UM | TextBox | UM | 6549,0 870x282 | 1070 |  |
| DataBC | TextBox | Data | 7419,0 975x282 | 1097 |  |
| NumarBC | TextBox | Numar | 8394,0 1200x282 | 1123 |  |
| Cant | TextBox | CF | 9594,0 855x282 | 1151 |  |
| Text36 | TextBox |  | 10449,0 1410x282 | 1180 |  |
| Text37 | TextBox |  | 11859,0 570x282 | 1206 |  |
| Text38 | TextBox |  | 12429,0 915x282 | 1232 |  |
| Text39 | TextBox |  | 13344,0 720x282 | 1258 |  |
| Text40 | TextBox |  | 14064,0 1035x282 | 1285 |  |
| Line43 | Line |  | 24,0 0x282 | 1311 |  |
| Line44 | Line |  | 4614,0 0x282 | 1325 |  |
| Line45 | Line |  | 7419,0 0x282 | 1339 |  |
| Line46 | Line |  | 11859,0 0x282 | 1353 |  |
| Line47 | Line |  | 15099,0 0x282 | 1367 |  |
| Line48 | Line |  | 5679,0 0x282 | 1381 |  |
| Line49 | Line |  | 6549,0 0x282 | 1395 |  |

## GroupFooter0 (inaltime 72 twips, linia 1411)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Box51 | Rectangle |  | 0,0 15094x0 | 1425 |  |

## PageFooterSection (inaltime 792 twips, linia 1440)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label42 | Label | 14.3.9 | 10771,453 4320x225 | 1450 |  |
| Box50 | Rectangle |  | 0,0 15094x0 | 1478 |  |
| Text50 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 12694,0 2400x285 | 1491 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,0 2736x477 | 1522 |  |

