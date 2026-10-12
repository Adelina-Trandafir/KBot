# Trasabilitate schita raport OI_Fisa_Evidenta_Primitori_STOC

Sursa: [Reports/OI_Fisa_Evidenta_Primitori_STOC.txt](../../VERCON_ACCESS/Reports/OI_Fisa_Evidenta_Primitori_STOC.txt) · imagine: [R_OI_Fisa_Evidenta_Primitori_STOC.png](../R_OI_Fisa_Evidenta_Primitori_STOC.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT X.IDC, X.Denumire, X.Adresa, X.CodProdus, X.DenumireProdus, X.UM, X.PUTVA, [c0]+[c1]-[c2] AS CF, [v0]+[v1]-[v2] AS VF FROM (SELECT OI_Evidenta.IDC, OI_Evidenta.Denumire, Clienti_1.Adresa, OI_Evidenta.CodProdus, OI_Evidenta.DenumireProdus, OI_Evidenta.UM, OI_Evidenta.PUTVA, Sum(OI_Evidenta.C0) AS C0, Sum(OI_Evidenta.V0) AS V0, Sum(OI_Evidenta.C1) AS C1, Sum(OI_Evidenta.V1) AS V1, Sum(OI_Evid …`
- Latime: 9624 twips · sectiuni: 4 · controale: 25

## PageHeaderSection (inaltime 2352 twips, linia 184)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label0 | Label | FIȘĂ EVIDENȚĂ PRIMITORI | 0,432 9621x456 | 191 |  |
| Text0 | TextBox | =DLookUp("Firma","caleTemp") | 0,0 4587x426 | 212 |  |
| Label67 | TextBox | =[Forms]![SFP]![di] & " - " & [Forms]![sfp]![di] | 0,888 9621x456 | 233 |  |
| Denumire | TextBox | Denumire | 1968,1416 7560x288 | 258 |  |
| Label62 | Label | Denumire | 96,1416 1872x288 | 282 |  |
| Adresa | TextBox | Adresa | 1968,1704 7560x288 | 304 |  |
| Label63 | Label | Detalii | 96,1704 1872x288 | 328 |  |
| Label4 | Label | Denumire produs | 576,2064 3360x276 | 350 |  |
| Label6 | Label | U/M | 3936,2064 792x276 | 375 |  |
| Label44 | Label | Preț | 4728,2064 1440x276 | 400 |  |
| Label94 | Label | Cantitate | 6168,2064 1440x276 | 425 |  |
| Label95 | Label | Valoare | 7608,2064 1440x276 | 450 |  |

## Detail (inaltime 276 twips, linia 477)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text64 | TextBox | =UCase([DenumireProdus]) | 576,0 3360x276 | 485 |  |
| Text66 | TextBox | putva | 4728,0 1440x276 | 510 |  |
| Text93 | TextBox | =UCase([UM]) | 3936,0 792x276 | 538 |  |
| Text74 | TextBox | CF | 6168,0 1440x276 | 564 |  |
| Text75 | TextBox | VF | 7608,0 1440x276 | 592 |  |
| Line86 | Line |  | 9048,0 0x276 | 620 |  |
| Line85 | Line |  | 576,0 0x276 | 635 |  |

## GroupFooter0 (inaltime 276 twips, linia 652)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text88 | TextBox | =Sum([vf]) | 7608,0 1440x276 | 663 |  |
| Label89 | TextBox | ="Stoc la " & [Forms]![SFP]![dsf] | 576,0 3360x276 | 689 |  |
| Label90 | TextBox | Denumire | 3936,0 3672x276 | 713 |  |

## PageFooterSection (inaltime 477 twips, linia 739)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text46 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 4536,0 5040x285 | 746 |  |
| Line42 | Line |  | 0,0 9621x0 | 770 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,0 2736x477 | 779 |  |

