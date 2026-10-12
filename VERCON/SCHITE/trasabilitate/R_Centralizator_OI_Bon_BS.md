# Trasabilitate schita raport Centralizator_OI_Bon_BS

Sursa: [Reports/Centralizator_OI_Bon_BS.txt](../../VERCON_ACCESS/Reports/Centralizator_OI_Bon_BS.txt) · imagine: [R_Centralizator_OI_Bon_BS.png](../R_Centralizator_OI_Bon_BS.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT OI_Casare.IDBC, OI_Casare.IDC_BC, OI_Casare.IdMasina, Masini.Masina, Clienti.Denumire, OI_Casare.NRBC, OI_Casare.DATABC, OI_Casare.CodProdus, OI_Casare.DenumireProdus, OI_Casare.UM, OI_Casare.Cant, OI_Casare.PUTVA, OI_Casare.ValoareCUTVA, OI_Casare.Expl FROM Masini INNER JOIN (Clienti INNER JOIN OI_Casare ON Clienti.IDC = OI_Casare.IDC_BC) ON Masini.IdMasina = OI_Casare.IdMasina WHERE (((OI …`
- Latime: 10320 twips · sectiuni: 7 · controale: 22

## ReportHeader (inaltime 1728 twips, linia 451)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label53 | Label | CENTRALIZATOR BONURI CASARE | 0,624 10320x360 | 461 |  |
| Label54 | TextBox | ="Perioada " & [Forms]![sfp]![di] & " - " & [Forms]![sfp]![dsf] | 0,984 10320x360 | 480 |  |
| Text199 | TextBox | =DLookUp("Firma","CaleTemp") | 1275,0 6621x279 | 503 |  |
| Label200 | Label | Unitatea: | 0,0 1275x279 | 526 |  |
| Label74 | Label | Număr BP | 0,1440 1341x288 | 543 |  |
| Label75 | Label | Data | 1341,1440 1437x288 | 565 |  |
| Label76 | Label | Primitor | 2778,1440 5781x288 | 589 |  |
| Label77 | Label | Valoare | 8559,1440 1749x288 | 613 |  |

## PageHeaderSection (inaltime 1800 twips, linia 639)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label66 | Label | CENTRALIZATOR BONURI CASARE | 0,624 10320x360 | 646 |  |
| Text67 | TextBox | ="Perioada " & [Forms]![sfp]![di] & " - " & [Forms]![sfp]![dsf] | 0,984 10320x360 | 665 |  |
| Text68 | TextBox | =DLookUp("Firma","CaleTemp") | 1275,0 6621x279 | 687 |  |
| Label69 | Label | Unitatea: | 0,0 1275x279 | 709 |  |
| Text60 | Label | Număr BS | 0,1440 1341x288 | 726 |  |
| Text63 | Label | Data | 1341,1440 1437x288 | 748 |  |
| Text61 | Label | Primitor | 2778,1440 5781x288 | 772 |  |
| Text62 | Label | Valoare | 8559,1440 1749x288 | 796 |  |

## GroupHeader0 (inaltime 0 twips, linia 822)

_fara controale_

## Detail (inaltime 0 twips, linia 836)

_fara controale_

## GroupFooter1 (inaltime 288 twips, linia 844)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text55 | TextBox | NRBC | 0,0 1341x288 | 855 |  |
| Text57 | TextBox | Denumire | 2778,0 5781x288 | 874 |  |
| Text58 | TextBox | =Sum([ValoareCUTVA]) | 8559,0 1749x288 | 896 |  |
| Text59 | TextBox | DATABC | 1341,0 1437x288 | 920 |  |

## PageFooterSection (inaltime 0 twips, linia 944)

_fara controale_

## ReportFooter (inaltime 360 twips, linia 951)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text65 | TextBox | =Sum([ValoareCUTVA]) | 8559,0 1749x288 | 962 |  |
| Label64 | Label | TOTAL BONURI CASARE | 0,0 8553x288 | 987 |  |

