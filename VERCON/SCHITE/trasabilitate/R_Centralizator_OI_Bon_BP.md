# Trasabilitate schite centralizatoare bonuri OI

Convenții și limite: [citirea fișelor consolidate](../INDEX.md#fise-consolidate). SLICE-D003-09, 27.10.2026.

Comune tuturor: Titlu (Caption): —; lățime 10320 twips. Coloana Evenimente era goală la fiecare control.

## Variante și surse

### Trasabilitate schita raport Centralizator_OI_Bon_BP

Sursa: [Reports/Centralizator_OI_Bon_BP.txt](../../VERCON_ACCESS/Reports/Centralizator_OI_Bon_BP.txt) · imagine: [R_Centralizator_OI_Bon_BP.png](../R_Centralizator_OI_Bon_BP.png)

- Sursa de date (RecordSource): valoare exportată identică cu [OI_Bon_BP](R_OI_Bon_BP.md#trasabilitate-schita-raport-oi_bon_bp); fragmentul trunchiat rămâne acolo.
- sectiuni: 7 · controale: 22

### Trasabilitate schita raport Centralizator_OI_Bon_BS

Sursa: [Reports/Centralizator_OI_Bon_BS.txt](../../VERCON_ACCESS/Reports/Centralizator_OI_Bon_BS.txt) · imagine: [R_Centralizator_OI_Bon_BS.png](../R_Centralizator_OI_Bon_BS.png)

- Sursa de date (RecordSource): valoare exportată identică cu [OI_Bon_BS](R_OI_Bon_BP.md#trasabilitate-schita-raport-oi_bon_bs); fragmentul trunchiat rămâne acolo.
- sectiuni: 7 · controale: 22

### Trasabilitate schita raport Centralizator_OI_Bon_BT

Sursa: [Reports/Centralizator_OI_Bon_BT.txt](../../VERCON_ACCESS/Reports/Centralizator_OI_Bon_BT.txt) · imagine: [R_Centralizator_OI_Bon_BT.png](../R_Centralizator_OI_Bon_BT.png)

- Sursa de date (RecordSource): valoare exportată identică cu [OI_Bon_BT](R_OI_Bon_BP.md#trasabilitate-schita-raport-oi_bon_bt); fragmentul trunchiat rămâne acolo.
- sectiuni: 7 · controale: 25

## Secțiuni

| Secțiune | Centralizator_OI_Bon_BP | Centralizator_OI_Bon_BS | Centralizator_OI_Bon_BT |
|---|---|---|---|
| ReportHeader | 1 / 1728 / 452 | 1 / 1728 / 451 | 1 / 1728 / 456 |
| PageHeaderSection | 2 / 1800 / 640 | 2 / 1800 / 639 | 2 / 1728 / 668 |
| GroupHeader0 | 3 / 0 / 823 | 3 / 0 / 822 | 3 / 0 / 875 |
| Detail | 4 / 0 / 837 | 4 / 0 / 836 | 4 / 0 / 888 |
| GroupFooter1 | 5 / 288 / 845 | 5 / 288 / 844 | 5 / 288 / 896 |
| PageFooterSection | 6 / 0 / 945 | 6 / 0 / 944 | 6 / 0 / 1018 |
| ReportFooter | 7 / 360 / 952 | 7 / 360 / 951 | 7 / 360 / 1025 |

## Controale

### ReportHeader <a id="reportheader-inaltime-1728-twips-linia-452"></a><a id="reportheader-inaltime-1728-twips-linia-451"></a><a id="reportheader-inaltime-1728-twips-linia-456"></a>

| Control | Tip | Text / camp / sursa | Pozitie comună (twips) | Centralizator_OI_Bon_BP | Centralizator_OI_Bon_BS | Centralizator_OI_Bon_BT |
|---|---|---|---|---|---|---|
| Label53 | Label | CENTRALIZATOR BONURI PREDARE | 0,624 10320x360 | 1:462 | — | — |
| Label54 | TextBox | ="Perioada " & [Forms]![sfp]![di] & " - " & [Forms]![sfp]![dsf] | 0,984 10320x360 | 2:481 | 2:480 | 2:485 |
| Text199 | TextBox | =DLookUp("Firma","CaleTemp") | 1275,0 6621x279 | 3:504 | 3:503 | 3:508 |
| Label200 | Label | Unitatea: | 0,0 1275x279 | 4:527 | 4:526 | 4:531 |
| Label74 | Label | Număr BP | 0,1440 1341x288 | 5:544 | 5:543 | — |
| Label75 | Label | Data | 1341,1440 1437x288 | 6:566 | 6:565 | 6:570 / 1197,1440 1293x288 |
| Label76 | Label | Primitor | 2778,1440 5781x288 | 7:590 | 7:589 | 7:594 / 5682,1440 3417x288 |
| Label77 | Label | Valoare | 8559,1440 1749x288 | 8:614 | 8:613 | 8:618 / 9099,1440 1209x288 |
| Label53 | Label | CENTRALIZATOR BONURI CASARE | 0,624 10320x360 | — | 1:461 | — |
| Label53 | Label | CENTRALIZATOR BONURI TRANSFER | 0,624 10320x360 | — | — | 1:466 |
| Label74 | Label | Număr BT | 0,1440 1197x288 | — | — | 5:548 |
| Label78 | Label | Predător | 2490,1440 3201x288 | — | — | 9:642 |

### PageHeaderSection <a id="pageheadersection-inaltime-1800-twips-linia-640"></a><a id="pageheadersection-inaltime-1800-twips-linia-639"></a><a id="pageheadersection-inaltime-1728-twips-linia-668"></a>

| Control | Tip | Text / camp / sursa | Pozitie comună (twips) | Centralizator_OI_Bon_BP | Centralizator_OI_Bon_BS | Centralizator_OI_Bon_BT |
|---|---|---|---|---|---|---|
| Label66 | Label | CENTRALIZATOR BONURI PREDARE | 0,624 10320x360 | 1:647 | — | — |
| Text67 | TextBox | ="Perioada " & [Forms]![sfp]![di] & " - " & [Forms]![sfp]![dsf] | 0,984 10320x360 | 2:666 | 2:665 | 2:694 |
| Text68 | TextBox | =DLookUp("Firma","CaleTemp") | 1275,0 6621x279 | 3:688 | 3:687 | 3:716 |
| Label69 | Label | Unitatea: | 0,0 1275x279 | 4:710 | 4:709 | 4:738 |
| Text60 | Label | Număr BP | 0,1440 1341x288 | 5:727 | — | — |
| Text63 | Label | Data | 1341,1440 1437x288 | 6:749 | 6:748 | 6:777 / 1197,1440 1293x288 |
| Text61 | Label | Primitor | 2778,1440 5781x288 | 7:773 | 7:772 | 7:801 / 5682,1440 3417x288 |
| Text62 | Label | Valoare | 8559,1440 1749x288 | 8:797 | 8:796 | 8:825 / 9099,1440 1209x288 |
| Label66 | Label | CENTRALIZATOR BONURI CASARE | 0,624 10320x360 | — | 1:646 | — |
| Text60 | Label | Număr BS | 0,1440 1341x288 | — | 5:726 | — |
| Label66 | Label | CENTRALIZATOR BONURI TRANSFER | 0,624 10320x360 | — | — | 1:675 |
| Text60 | Label | Număr BT | 0,1440 1197x288 | — | — | 5:755 |
| Label79 | Label | Predător | 2490,1440 3201x288 | — | — | 9:849 |

### GroupHeader0 <a id="groupheader0-inaltime-0-twips-linia-823"></a><a id="groupheader0-inaltime-0-twips-linia-822"></a><a id="groupheader0-inaltime-0-twips-linia-875"></a>

_fara controale_

### Detail <a id="detail-inaltime-0-twips-linia-837"></a><a id="detail-inaltime-0-twips-linia-836"></a><a id="detail-inaltime-0-twips-linia-888"></a>

_fara controale_

### GroupFooter1 <a id="groupfooter1-inaltime-288-twips-linia-845"></a><a id="groupfooter1-inaltime-288-twips-linia-844"></a><a id="groupfooter1-inaltime-288-twips-linia-896"></a>

| Control | Tip | Text / camp / sursa | Pozitie comună (twips) | Centralizator_OI_Bon_BP | Centralizator_OI_Bon_BS | Centralizator_OI_Bon_BT |
|---|---|---|---|---|---|---|
| Text55 | TextBox | NRBP | 0,0 1341x288 | 1:856 | — | — |
| Text57 | TextBox | Denumire | 2778,0 5781x288 | 2:875 | 2:874 | — |
| Text58 | TextBox | =Sum([ValoareCUTVA]) | 8559,0 1749x288 | 3:897 | 3:896 | 3:950 / 9099,0 1209x288 |
| Text59 | TextBox | DATABP | 1341,0 1437x288 | 4:921 | — | — |
| Text55 | TextBox | NRBC | 0,0 1341x288 | — | 1:855 | — |
| Text59 | TextBox | DATABC | 1341,0 1437x288 | — | 4:920 | — |
| Text55 | TextBox | NRBT | 0,0 1197x288 | — | — | 1:909 |
| Text57 | TextBox | Primitor | 5682,0 3417x288 | — | — | 2:928 |
| Text59 | TextBox | DATABT | 1197,0 1293x288 | — | — | 4:972 |
| Text80 | TextBox | Predator | 2490,0 3201x288 | — | — | 5:994 |

### PageFooterSection <a id="pagefootersection-inaltime-0-twips-linia-945"></a><a id="pagefootersection-inaltime-0-twips-linia-944"></a><a id="pagefootersection-inaltime-0-twips-linia-1018"></a>

_fara controale_

### ReportFooter <a id="reportfooter-inaltime-360-twips-linia-952"></a><a id="reportfooter-inaltime-360-twips-linia-951"></a><a id="reportfooter-inaltime-360-twips-linia-1025"></a>

| Control | Tip | Text / camp / sursa | Pozitie comună (twips) | Centralizator_OI_Bon_BP | Centralizator_OI_Bon_BS | Centralizator_OI_Bon_BT |
|---|---|---|---|---|---|---|
| Text65 | TextBox | =Sum([ValoareCUTVA]) | 8559,0 1749x288 | 1:963 | 1:962 | 1:1036 / 9099,0 1209x288 |
| Label64 | Label | TOTAL BONURI PREDARE | 0,0 8553x288 | 2:988 | — | — |
| Label64 | Label | TOTAL BONURI CASARE | 0,0 8553x288 | — | 2:987 | — |
| Label64 | Label | TOTAL BONURI TRANSFER | 0,0 9093x288 | — | — | 2:1059 |
