# Trasabilitate schita raport Centralizator_OI_Bon_BP

Sursa: [Reports/Centralizator_OI_Bon_BP.txt](../../VERCON_ACCESS/Reports/Centralizator_OI_Bon_BP.txt) · imagine: [R_Centralizator_OI_Bon_BP.png](../R_Centralizator_OI_Bon_BP.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT OI_Predare.IDBP, OI_Predare.IDC_BP, OI_Predare.IdMasina, Masini.Masina, Clienti.Denumire, OI_Predare.NRBP, OI_Predare.DATABP, OI_Predare.CodProdus, OI_Predare.DenumireProdus, OI_Predare.UM, OI_Predare.Cant, OI_Predare.PUTVA, OI_Predare.ValoareCUTVA, OI_Predare.Expl FROM Masini INNER JOIN (Clienti INNER JOIN OI_Predare ON Clienti.IDC = OI_Predare.IDC_BP) ON Masini.IdMasina = OI_Predare.IdMas …`
- Latime: 10320 twips · sectiuni: 7 · controale: 22

## ReportHeader (inaltime 1728 twips, linia 452)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label53 | Label | CENTRALIZATOR BONURI PREDARE | 0,624 10320x360 | 462 |  |
| Label54 | TextBox | ="Perioada " & [Forms]![sfp]![di] & " - " & [Forms]![sfp]![dsf] | 0,984 10320x360 | 481 |  |
| Text199 | TextBox | =DLookUp("Firma","CaleTemp") | 1275,0 6621x279 | 504 |  |
| Label200 | Label | Unitatea: | 0,0 1275x279 | 527 |  |
| Label74 | Label | Număr BP | 0,1440 1341x288 | 544 |  |
| Label75 | Label | Data | 1341,1440 1437x288 | 566 |  |
| Label76 | Label | Primitor | 2778,1440 5781x288 | 590 |  |
| Label77 | Label | Valoare | 8559,1440 1749x288 | 614 |  |

## PageHeaderSection (inaltime 1800 twips, linia 640)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label66 | Label | CENTRALIZATOR BONURI PREDARE | 0,624 10320x360 | 647 |  |
| Text67 | TextBox | ="Perioada " & [Forms]![sfp]![di] & " - " & [Forms]![sfp]![dsf] | 0,984 10320x360 | 666 |  |
| Text68 | TextBox | =DLookUp("Firma","CaleTemp") | 1275,0 6621x279 | 688 |  |
| Label69 | Label | Unitatea: | 0,0 1275x279 | 710 |  |
| Text60 | Label | Număr BP | 0,1440 1341x288 | 727 |  |
| Text63 | Label | Data | 1341,1440 1437x288 | 749 |  |
| Text61 | Label | Primitor | 2778,1440 5781x288 | 773 |  |
| Text62 | Label | Valoare | 8559,1440 1749x288 | 797 |  |

## GroupHeader0 (inaltime 0 twips, linia 823)

_fara controale_

## Detail (inaltime 0 twips, linia 837)

_fara controale_

## GroupFooter1 (inaltime 288 twips, linia 845)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text55 | TextBox | NRBP | 0,0 1341x288 | 856 |  |
| Text57 | TextBox | Denumire | 2778,0 5781x288 | 875 |  |
| Text58 | TextBox | =Sum([ValoareCUTVA]) | 8559,0 1749x288 | 897 |  |
| Text59 | TextBox | DATABP | 1341,0 1437x288 | 921 |  |

## PageFooterSection (inaltime 0 twips, linia 945)

_fara controale_

## ReportFooter (inaltime 360 twips, linia 952)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text65 | TextBox | =Sum([ValoareCUTVA]) | 8559,0 1749x288 | 963 |  |
| Label64 | Label | TOTAL BONURI PREDARE | 0,0 8553x288 | 988 |  |

