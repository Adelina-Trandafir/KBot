# Trasabilitate schita raport OI_Fisa_Evidenta_Primitori

Sursa: [Reports/OI_Fisa_Evidenta_Primitori.txt](../../VERCON_ACCESS/Reports/OI_Fisa_Evidenta_Primitori.txt) · imagine: [R_OI_Fisa_Evidenta_Primitori.png](../R_OI_Fisa_Evidenta_Primitori.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT OI_Evidenta.IDC, OI_Evidenta.PUTVA, OI_Evidenta.Denumire, Clienti_1.Adresa, Nz([clienti]![Denumire],Switch(Left([numar],2)="BP","-",Left([numar],2)="BS","Casare")) AS Primitor, OI_Evidenta.Numar, OI_Evidenta.Data, OI_Evidenta.CodProdus, OI_Evidenta.DenumireProdus, OI_Evidenta.UM, Sum(OI_Evidenta.C0) AS C0, Sum(OI_Evidenta.V0) AS V0, Sum(OI_Evidenta.C1) AS C1, Sum(OI_Evidenta.V1) AS V1, Sum( …`
- Latime: 9624 twips · sectiuni: 8 · controale: 40

## PageHeaderSection (inaltime 2352 twips, linia 198)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label0 | Label | FIȘĂ EVIDENȚĂ PRIMITORI | 0,432 9621x456 | 205 |  |
| Text0 | TextBox | =DLookUp("Firma","caleTemp") | 0,0 4587x426 | 226 |  |
| Label67 | TextBox | =[Forms]![SFP]![di] & " - " & [Forms]![sfp]![di] | 0,888 9621x456 | 247 |  |
| Denumire | TextBox | Denumire | 1968,1416 7560x288 | 272 |  |
| Label62 | Label | Denumire | 96,1416 1872x288 | 296 |  |
| Adresa | TextBox | Adresa | 1968,1704 7560x288 | 318 |  |
| Label63 | Label | Detalii | 96,1704 1872x288 | 342 |  |
| Label4 | Label | Data | 0,2064 1320x276 | 364 |  |
| Label5 | Label | Nr. doc. | 1320,2064 1392x276 | 387 |  |
| Label6 | Label | Cantitatea | 2712,2064 1440x276 | 412 |  |
| Label10 | Label | Primitor | 5592,2064 4032x276 | 437 |  |
| Label44 | Label | Valoarea | 4152,2064 1440x276 | 462 |  |

## GroupHeader0 (inaltime 0 twips, linia 489)

_fara controale_

## GroupHeader1 (inaltime 288 twips, linia 500)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text64 | TextBox | =UCase([DenumireProdus]) | 0,0 7680x288 | 511 |  |
| Text66 | TextBox | putva | 7680,0 1944x288 | 532 |  |
| Line84 | Line |  | 0,0 9621x0 | 559 |  |
| Line85 | Line |  | 0,0 0x288 | 568 |  |
| Line86 | Line |  | 9624,0 0x288 | 579 |  |

## Detail (inaltime 276 twips, linia 595)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label68 | TextBox | Data | 0,0 1320x276 | 603 |  |
| Label69 | TextBox | Numar | 1320,0 1392x276 | 625 |  |
| Label70 | TextBox | =[c0]+[c1]-[c2] | 2712,0 1440x276 | 650 |  |
| Label71 | TextBox | Primitor | 5592,0 4032x276 | 676 |  |
| Label72 | TextBox | =[v0]+[v1]-[v2] | 4152,0 1440x276 | 701 |  |
| Line81 | Line |  | 0,0 0x276 | 727 |  |
| Line82 | Line |  | 9624,0 0x276 | 738 |  |

## GroupFooter2 (inaltime 291 twips, linia 754)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text74 | TextBox | =Sum([c0]+[c1]-[c2]) | 2712,0 1440x276 | 765 |  |
| Text75 | TextBox | =Sum([v0]+[v1]-[v2]) | 4152,0 1440x276 | 793 |  |
| Label73 | Label |  | 0,0 2712x276 | 820 |  |
| Label76 | Label |  | 5592,0 4032x276 | 840 |  |
| Line77 | Line |  | 0,0 9621x0 | 862 |  |
| Line78 | Line |  | 0,276 9621x0 | 871 |  |
| Line79 | Line |  | 0,0 0x276 | 883 |  |
| Line80 | Line |  | 9621,0 0x276 | 894 |  |

## GroupFooter3 (inaltime 115 twips, linia 910)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Line91 | Line |  | 0,0 0x115 | 921 |  |
| Line92 | Line |  | 9624,0 0x115 | 932 |  |

## GroupFooter0 (inaltime 360 twips, linia 948)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text88 | TextBox | =Sum([v0]+[v1]-[v2]) | 4152,0 1440x276 | 959 |  |
| Label89 | TextBox | ="Stoc la " & [Forms]![SFP]![dsf] | 0,0 4152x276 | 985 |  |
| Label90 | TextBox | Denumire | 5592,0 4032x276 | 1007 |  |

## PageFooterSection (inaltime 477 twips, linia 1033)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text46 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 4536,0 5040x285 | 1040 |  |
| Line42 | Line |  | 0,0 9621x0 | 1064 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,0 2736x477 | 1073 |  |

