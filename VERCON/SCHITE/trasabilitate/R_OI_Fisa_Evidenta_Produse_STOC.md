# Trasabilitate schita raport OI_Fisa_Evidenta_Produse_STOC

Sursa: [Reports/OI_Fisa_Evidenta_Produse_STOC.txt](../../VERCON_ACCESS/Reports/OI_Fisa_Evidenta_Produse_STOC.txt) · imagine: [R_OI_Fisa_Evidenta_Produse_STOC.png](../R_OI_Fisa_Evidenta_Produse_STOC.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT OI_Evidenta.Masina, OI_Evidenta.CodProdus, OI_Evidenta.DenumireProdus, OI_Evidenta.Denumire, OI_Evidenta.UM, OI_Evidenta.PUTVA, Sum([c0]+[c1]-[c2]) AS CF, Sum([v0]+[v1]-[v2]) AS VF FROM OI_Evidenta GROUP BY OI_Evidenta.Masina, OI_Evidenta.CodProdus, OI_Evidenta.DenumireProdus, OI_Evidenta.Denumire, OI_Evidenta.UM, OI_Evidenta.PUTVA HAVING (((Sum([c0]+[c1]-[c2]))<>0)) ORDER BY OI_Evidenta.Ma …`
- Latime: 9627 twips · sectiuni: 6 · controale: 32

## PageHeaderSection (inaltime 1776 twips, linia 186)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label0 | Label | STOC FINAL OBIECTE INVENTAR | 0,432 9621x456 | 194 |  |
| Text0 | TextBox | =DLookUp("Firma","caleTemp") | 0,0 4587x426 | 215 |  |
| L4 | Label | Valoarea | 7893,1464 1440x300 | 236 |  |
| L3 | Label | Cant. | 6813,1464 1080x300 | 261 |  |
| L2 | Label | Preț unitar | 4893,1464 1920x300 | 286 |  |
| L1 | Label | Denumire Produs | 264,1464 4629x300 | 310 |  |
| Text5 | TextBox | =fluna(Month([Forms]![sfp]![dsf])) & "/" & Year([Forms]![SFP]![di]) | 0,888 9621x456 | 334 |  |

## GroupHeader0 (inaltime 768 twips, linia 362)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label1 | Label | Denumire Produs | 264,456 4629x300 | 375 |  |
| Label3 | Label | Preț unitar | 4893,456 1920x300 | 399 |  |
| Text40 | TextBox | Masina | 0,0 9627x300 | 423 |  |
| Label6 | Label | Cant. | 6813,456 1080x300 | 447 |  |
| Label44 | Label | Valoarea | 7893,456 1440x300 | 472 |  |
| Text82 | TextBox | =Sum([vf]) | 7893,0 1440x300 | 497 |  |

## GroupHeader1 (inaltime 300 twips, linia 528)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| DenumireProdusX | TextBox | =UCase([DenumireProdus]) | 264,0 4629x300 | 539 |  |
| PU | TextBox | PUTVA | 4893,0 1920x300 | 566 |  |
| Line61 | Line |  | 264,0 0x300 | 594 |  |
| Text51 | TextBox | =Sum([cf]) | 6813,0 1080x300 | 608 |  |
| Text52 | TextBox | =Sum([vf]) | 7893,0 1440x300 | 638 |  |
| Line62 | Line |  | 9333,0 0x300 | 667 |  |
| txtCnt | TextBox | =Count(*) | 3696,0 840x300 | 681 |  |
| Line68 | Line |  | 264,0 9069x0 | 698 |  |

## Detail (inaltime 315 twips, linia 711)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Primitor | TextBox | Denumire | 504,0 6312x311 | 720 |  |
| Cant | TextBox | CF | 6816,0 1080x311 | 743 |  |
| Text45 | TextBox | VF | 7896,0 1440x311 | 769 |  |
| Line53 | Line |  | 264,0 0x311 | 794 |  |
| Line54 | Line |  | 9336,0 0x311 | 808 |  |
| txtRow | TextBox | =1 | 3696,0 840x311 | 822 |  |
| Ln1 | Line |  | 264,300 9069x0 | 842 |  |

## GroupFooter1 (inaltime 120 twips, linia 860)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Line63 | Line |  | 264,0 9069x0 | 871 |  |

## PageFooterSection (inaltime 693 twips, linia 884)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text46 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 4536,96 5040x285 | 891 |  |
| Line42 | Line |  | 0,96 9621x0 | 917 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,96 2736x477 | 929 |  |

