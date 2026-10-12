# Trasabilitate schita raport Centralizator_OI_Bon_BT

Sursa: [Reports/Centralizator_OI_Bon_BT.txt](../../VERCON_ACCESS/Reports/Centralizator_OI_Bon_BT.txt) · imagine: [R_Centralizator_OI_Bon_BT.png](../R_Centralizator_OI_Bon_BT.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT OI_Transfer.IDBT, OI_Transfer.IDC_BT, OI_Transfer.IDC_OLD, OI_Transfer.IdMasina, Masini.Masina, C1.Denumire AS Primitor, C2.Denumire AS Predator, OI_Transfer.NRBT, OI_Transfer.DATABT, OI_Transfer.CodProdus, OI_Transfer.DenumireProdus, OI_Transfer.UM, OI_Transfer.Cant, OI_Transfer.PUTVA, OI_Transfer.ValoareCUTVA, OI_Transfer.Expl FROM Masini INNER JOIN (Clienti AS C2 INNER JOIN (Clienti AS C …`
- Latime: 10320 twips · sectiuni: 7 · controale: 25

## ReportHeader (inaltime 1728 twips, linia 456)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label53 | Label | CENTRALIZATOR BONURI TRANSFER | 0,624 10320x360 | 466 |  |
| Label54 | TextBox | ="Perioada " & [Forms]![sfp]![di] & " - " & [Forms]![sfp]![dsf] | 0,984 10320x360 | 485 |  |
| Text199 | TextBox | =DLookUp("Firma","CaleTemp") | 1275,0 6621x279 | 508 |  |
| Label200 | Label | Unitatea: | 0,0 1275x279 | 531 |  |
| Label74 | Label | Număr BT | 0,1440 1197x288 | 548 |  |
| Label75 | Label | Data | 1197,1440 1293x288 | 570 |  |
| Label76 | Label | Primitor | 5682,1440 3417x288 | 594 |  |
| Label77 | Label | Valoare | 9099,1440 1209x288 | 618 |  |
| Label78 | Label | Predător | 2490,1440 3201x288 | 642 |  |

## PageHeaderSection (inaltime 1728 twips, linia 668)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label66 | Label | CENTRALIZATOR BONURI TRANSFER | 0,624 10320x360 | 675 |  |
| Text67 | TextBox | ="Perioada " & [Forms]![sfp]![di] & " - " & [Forms]![sfp]![dsf] | 0,984 10320x360 | 694 |  |
| Text68 | TextBox | =DLookUp("Firma","CaleTemp") | 1275,0 6621x279 | 716 |  |
| Label69 | Label | Unitatea: | 0,0 1275x279 | 738 |  |
| Text60 | Label | Număr BT | 0,1440 1197x288 | 755 |  |
| Text63 | Label | Data | 1197,1440 1293x288 | 777 |  |
| Text61 | Label | Primitor | 5682,1440 3417x288 | 801 |  |
| Text62 | Label | Valoare | 9099,1440 1209x288 | 825 |  |
| Label79 | Label | Predător | 2490,1440 3201x288 | 849 |  |

## GroupHeader0 (inaltime 0 twips, linia 875)

_fara controale_

## Detail (inaltime 0 twips, linia 888)

_fara controale_

## GroupFooter1 (inaltime 288 twips, linia 896)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text55 | TextBox | NRBT | 0,0 1197x288 | 909 |  |
| Text57 | TextBox | Primitor | 5682,0 3417x288 | 928 |  |
| Text58 | TextBox | =Sum([ValoareCUTVA]) | 9099,0 1209x288 | 950 |  |
| Text59 | TextBox | DATABT | 1197,0 1293x288 | 972 |  |
| Text80 | TextBox | Predator | 2490,0 3201x288 | 994 |  |

## PageFooterSection (inaltime 0 twips, linia 1018)

_fara controale_

## ReportFooter (inaltime 360 twips, linia 1025)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text65 | TextBox | =Sum([ValoareCUTVA]) | 9099,0 1209x288 | 1036 |  |
| Label64 | Label | TOTAL BONURI TRANSFER | 0,0 9093x288 | 1059 |  |

