# Trasabilitate schita raport OI_Fisa_Evidenta_Produse

Sursa: [Reports/OI_Fisa_Evidenta_Produse.txt](../../VERCON_ACCESS/Reports/OI_Fisa_Evidenta_Produse.txt) · imagine: [R_OI_Fisa_Evidenta_Produse.png](../R_OI_Fisa_Evidenta_Produse.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `OI_Evidenta`
- Latime: 9624 twips · sectiuni: 6 · controale: 47

## PageHeaderSection (inaltime 1008 twips, linia 182)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label0 | Label | FIȘĂ EVIDENȚĂ PRODUSE | 0,432 9621x456 | 189 |  |
| Text0 | TextBox | =DLookUp("Firma","caleTemp") | 0,0 4587x426 | 210 |  |

## GroupHeader1 (inaltime 2664 twips, linia 233)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| DenumireProdusX | TextBox | =UCase([DenumireProdus]) | 2916,48 5769x300 | 244 |  |
| Label1 | Label | Denumire Produs | 696,48 2220x300 | 269 |  |
| UMx | TextBox | =UCase(Nz([UM],"")) | 2916,348 5769x300 | 291 |  |
| Label2 | Label | Unitate măsură | 696,348 2220x300 | 317 |  |
| PU | TextBox | PUTVA | 2916,648 5769x300 | 339 |  |
| Label3 | Label | Preț unitar | 696,648 2220x300 | 367 |  |
| Label4 | Label | Data | 0,2376 1320x276 | 389 |  |
| Label5 | Label | Nr. doc. | 1320,2376 1392x276 | 412 |  |
| Label6 | Label | Cantitatea | 7104,2376 1080x276 | 437 |  |
| Label10 | Label | Primitor | 2712,2376 4392x276 | 462 |  |
| Label24 | Label | Valoare inițială | 1368,1392 1620x300 | 487 |  |
| Text35 | TextBox | =[Forms]![sfp]![di] | 1368,1692 1620x240 | 512 |  |
| I0 | TextBox | =Sum([V0]) | 1368,1932 1620x276 | 537 |  |
| Label30 | Label | Valoare intrată | 2988,1392 1620x300 | 564 |  |
| Label32 | Label | Valoare ieșită | 4608,1392 1620x300 | 589 |  |
| Label26 | Label | Valoare finală | 6228,1392 1620x300 | 614 |  |
| Text37 | TextBox | =[Forms]![sfp]![dsf] | 6228,1692 1620x240 | 639 |  |
| Text38 | TextBox | =[Forms]![sfp]![di] & " - " & [Forms]![sfp]![dsf] | 2988,1692 3240x240 | 664 |  |
| I1 | TextBox | =Sum([V1]) | 2988,1932 1620x276 | 689 |  |
| I2 | TextBox | =Sum([V2]) | 4608,1932 1620x276 | 716 |  |
| Text25 | TextBox | =[i0]+[i1]-[i2] | 6228,1932 1620x276 | 743 |  |
| Label39 | Label | Cont | 696,948 2220x300 | 770 |  |
| Text40 | TextBox | Masina | 2916,948 5775x300 | 790 |  |
| Box43 | Rectangle |  | 0,0 9624x2376 | 815 |  |
| Label44 | Label | Valoarea | 8184,2376 1440x276 | 829 |  |

## Detail (inaltime 300 twips, linia 856)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Numar | TextBox | Numar | 1320,0 1392x300 | 864 |  |
| Primitor | TextBox | Denumire | 2712,0 4392x300 | 888 |  |
| Cant | TextBox | =[c0]+[c1]-[c2] | 7104,0 1080x300 | 913 |  |
| Data | TextBox | Data | 0,0 1320x300 | 940 |  |
| Text45 | TextBox | =[v0]+[v1]-[v2] | 8184,0 1440x300 | 977 |  |
| Line53 | Line |  | 0,0 0x300 | 1003 |  |
| Line54 | Line |  | 9624,0 0x300 | 1014 |  |
| Ord | TextBox | Ord | 4080,0 1440x300 | 1028 |  |
| Text59 | TextBox | Tip | 5520,0 1440x300 | 1044 |  |

## GroupFooter0 (inaltime 300 twips, linia 1064)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text47 | TextBox | ="STOC FINAL " & [Denumire] | 0,0 7104x300 | 1075 |  |
| Text48 | TextBox | =Sum(Nz([c0]+[c1]-[c2],0)) | 7104,0 1080x300 | 1098 |  |
| Text49 | TextBox | =Sum([v0]+[v1]-[v2]) | 8184,0 1440x300 | 1126 |  |
| Line55 | Line |  | 0,0 0x300 | 1153 |  |
| Line56 | Line |  | 9624,0 0x300 | 1164 |  |

## GroupFooter2 (inaltime 300 twips, linia 1180)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text50 | TextBox | ="STOC FINAL" | 0,0 7104x300 | 1192 |  |
| Text51 | TextBox | =Sum(Nz([c0]+[c1]-[c2],0)) | 7104,0 1080x300 | 1214 |  |
| Text52 | TextBox | =Sum([v0]+[v1]-[v2]) | 8184,0 1440x300 | 1241 |  |

## PageFooterSection (inaltime 599 twips, linia 1269)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text46 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 4536,96 5040x285 | 1276 |  |
| Line42 | Line |  | 0,96 9621x0 | 1302 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,122 2736x477 | 1314 |  |

