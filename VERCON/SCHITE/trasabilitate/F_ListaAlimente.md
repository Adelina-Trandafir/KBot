# Trasabilitate schite liste de alimente

Convenții și limite: [citirea fișelor consolidate](../INDEX.md#fise-consolidate). SLICE-D003-16, 28.10.2026. Secțiunea este coloana întâi.

## Variante și surse

### Trasabilitate schita formular ListaAlimente

Sursa: [Forms/ListaAlimente.txt](../../VERCON_ACCESS/Forms/ListaAlimente.txt) · imagine: [F_ListaAlimente.png](../F_ListaAlimente.png)

- Titlu (Caption): Lista Alimente · Sursa de date (RecordSource): — · Latime: 16314 twips · sectiuni: 5 · controale: 68

### Trasabilitate schita formular ListaAlimente_copy

Sursa: [Forms/ListaAlimente_copy.txt](../../VERCON_ACCESS/Forms/ListaAlimente_copy.txt) · imagine: [F_ListaAlimente_copy.png](../F_ListaAlimente_copy.png)

- Titlu (Caption): Lista Alimente · Sursa de date (RecordSource): — · Latime: 15579 twips · sectiuni: 5 · controale: 68

## Secțiuni

| Secțiune | ListaAlimente | ListaAlimente_copy |
|---|---|---|
| FormHeader <a id="formheader-inaltime-450-twips-linia-210"></a><a id="formheader-inaltime-450-twips-linia-202"></a> | 1 / 450 / 210 | 1 / 450 / 202 |
| PageHeaderSection <a id="pageheadersection-inaltime-299-twips-linia-262"></a><a id="pageheadersection-inaltime-285-twips-linia-255"></a> | 2 / 299 / 262 | 2 / 285 / 255 |
| Detail <a id="detail-inaltime-5130-twips-linia-628"></a><a id="detail-inaltime-4944-twips-linia-491"></a> | 3 / 5130 / 628 | 3 / 4944 / 491 |
| PageFooterSection <a id="pagefootersection-inaltime-0-twips-linia-1736"></a><a id="pagefootersection-inaltime-0-twips-linia-1730"></a> | 4 / 0 / 1736 | 4 / 0 / 1730 |
| FormFooter <a id="formfooter-inaltime-766-twips-linia-1747"></a><a id="formfooter-inaltime-766-twips-linia-1741"></a> | 5 / 766 / 1747 | 5 / 766 / 1741 |

## Controale

| Secțiune | Control | Tip | Text / camp / sursa | Pozitie comună (twips) | Evenimente | ListaAlimente | ListaAlimente_copy |
|---|---|---|---|---|---|---|---|
| FormHeader | Label0 | Label | Listă de alimente | 0,0 10800x450 |  | 1:220 | 1:213 |
| FormHeader | Label44 | Label | Liste salvate | 10800,0 5514x450 |  | 2:239 | 2:232 / 10800,0 4779x450 |
| PageHeaderSection | J | CheckBox |  | 0,0 260x240 |  | 1:272 | 1:265 |
| PageHeaderSection | E | CheckBox |  | 168,0 260x240 |  | 2:283 | 2:276 |
| PageHeaderSection | IDLL | TextBox |  | 432,0 516x240 |  | 3:297 | 3:290 |
| PageHeaderSection | IDLISTA | TextBox |  | 948,0 516x240 |  | 4:314 | 4:307 |
| PageHeaderSection | DV | TextBox |  | 1464,0 516x240 |  | 5:331 | 5:324 |
| PageHeaderSection | CV | TextBox |  | 1980,0 516x240 |  | 6:348 | 6:341 |
| PageHeaderSection | MDV | TextBox |  | 2496,0 516x240 |  | 7:365 | 7:358 |
| PageHeaderSection | IDBC | TextBox |  | 3012,0 516x240 |  | 8:382 | 8:375 |
| PageHeaderSection | DataBC | TextBox |  | 3528,0 516x240 |  | 9:399 | 9:392 |
| PageHeaderSection | IDL | TextBox |  | 4044,0 516x240 |  | 10:416 | 10:409 |
| PageHeaderSection | CAED | CheckBox |  | 4824,24 260x240 |  | 11:433 | 11:426 |
| PageHeaderSection | Dif | TextBox | =nz([valbc],0)-nz([vall],0) | 7944,0 1011x285 |  | 12:450 | 13:459 |
| PageHeaderSection | Command77 | CommandButton |  Alege B.C. | 9203,0 487x299 | OnClick | 13:480 | — |
| PageHeaderSection | NrBC | TextBox |  | 9826,0 1437x285 |  | 14:550 | — |
| PageHeaderSection | ValoareBon | TextBox |  | 11348,0 1437x285 |  | 15:578 | — |
| PageHeaderSection | IdBonConsum | TextBox |  | 5168,0 516x240 |  | 16:608 | — |
| PageHeaderSection | Label96 | Label | Calculeaza automat Economi / Depasiri | 5058,0 2835x240 |  | — | 12:443 |
| Detail | MD | Subform | Form.md | 120,2421 3405x2652 | OnExit | 1:636 | 1:499 / 120,1896 3405x2928 |
| Detail | DataLA | TextBox |  | 1806,333 1749x285 | OnEnter,OnExit,OnChange | 2:657 | — |
| Detail | Label3 | Label | Data L.A.: | 123,333 1683x285 |  | 3:690 | 3:550 / 120,360 1683x285 |
| Detail | NumarLA | TextBox |  | 1806,48 1749x285 |  | 4:714 | 4:574 / 1803,48 1749x285 |
| Detail | Label4 | Label | Număr listă | 123,48 1683x285 |  | 5:742 | 5:601 / 120,48 1683x285 |
| Detail | MicDejun_P | TextBox |  | 120,2094 825x255 |  | 6:766 | 6:625 / 120,1566 825x255 |
| Detail | Label54 | Label | Cant. | 120,1839 825x255 |  | 7:790 | 7:649 / 120,1311 825x255 |
| Detail | MicDejun_V | TextBox | =nz([MicDejun_P],0)*nz([vmd],0) | 1770,2094 825x255 |  | 8:817 | 8:676 / 1770,1566 825x255 |
| Detail | Label8 | Label | Valoare | 1770,1839 825x255 |  | 9:840 | 9:699 / 1770,1311 825x255 |
| Detail | MicDejun_D | TextBox |  | 2595,2094 930x255 |  | 10:866 | 10:725 / 2595,1566 930x255 |
| Detail | Label9 | Label | Distribuit | 2595,1839 930x255 |  | 11:889 | 11:748 / 2595,1311 930x255 |
| Detail | Label11 | Label |  Mic Dejun | 120,1586 3405x255 |  | 12:915 | 12:774 / 120,1058 3405x255 |
| Detail | D | Subform | Form.d | 3692,2421 3441x2652 | OnExit | 13:941 | — |
| Detail | Dejun_P | TextBox |  | 3692,2094 825x255 |  | 14:962 | 14:820 / 3692,1566 825x255 |
| Detail | Label58 | Label | Cant. | 3692,1839 825x255 |  | 15:985 | 15:843 / 3692,1311 825x255 |
| Detail | Dejun_V | TextBox | =nz([Dejun_P],0)*nz([vd],0) | 5342,2094 825x255 |  | 16:1012 | 16:870 / 5342,1566 825x255 |
| Detail | Label29 | Label | Valoare | 5342,1839 825x255 |  | 17:1035 | 17:893 / 5342,1311 825x255 |
| Detail | Dejun_D | TextBox |  | 6167,2094 966x255 |  | 18:1062 | 18:920 / 6167,1566 966x255 |
| Detail | Label31 | Label | Distribuit | 6167,1839 966x255 |  | 19:1085 | 19:943 / 6167,1311 966x255 |
| Detail | Label32 | Label | Dejun | 3692,1586 3441x255 |  | 20:1111 | 20:969 / 3692,1058 3441x255 |
| Detail | CN | Subform | Form.cn | 7263,2421 3405x2652 | OnExit | 21:1137 | — |
| Detail | Cina_P | TextBox |  | 7263,2094 825x255 |  | 22:1158 | 22:1015 / 7263,1566 825x255 |
| Detail | Label62 | Label | Cant. | 7263,1839 825x255 |  | 23:1181 | 23:1038 / 7263,1311 825x255 |
| Detail | Cina_V | TextBox | =nz([cina_P],0)*nz([vc],0) | 8913,2094 825x255 |  | 24:1208 | 24:1065 / 8913,1566 825x255 |
| Detail | Label38 | Label | Valoare | 8913,1839 825x255 |  | 25:1231 | 25:1088 / 8913,1311 825x255 |
| Detail | Cina_D | TextBox |  | 9738,2094 930x255 |  | 26:1258 | 26:1115 / 9738,1566 930x255 |
| Detail | Label40 | Label | Distribuit | 9738,1839 930x255 |  | 27:1281 | 27:1138 / 9738,1311 930x255 |
| Detail | VMD | TextBox |  | 945,2094 825x255 |  | 28:1307 | 28:1164 / 945,1566 825x255 |
| Detail | Label55 | Label | Preț | 945,1839 825x255 |  | 29:1329 | 29:1186 / 945,1311 825x255 |
| Detail | VD | TextBox |  | 4517,2094 825x255 |  | 30:1356 | 30:1213 / 4517,1566 825x255 |
| Detail | Label59 | Label | Preț | 4517,1839 825x255 |  | 31:1378 | 31:1235 / 4517,1311 825x255 |
| Detail | VC | TextBox |  | 8088,2094 825x255 |  | 32:1405 | 32:1262 / 8088,1566 825x255 |
| Detail | Label63 | Label | Preț | 8088,1839 825x255 |  | 33:1427 | 33:1284 / 8088,1311 825x255 |
| Detail | Label86 | Label | Cină / Gustare | 7263,1584 3405x255 |  | 34:1454 | 38:1485 / 7263,1056 3405x255 |
| Detail | subTreeView | Subform | Form.subTreeView | 10752,48 5442x5033 |  | 35:1480 | 41:1565 / 10752,48 4707x4773 |
| Detail | ValoareLista | TextBox |  | 1806,618 1749x285 |  | 36:1501 | 42:1586 / 1803,672 1749x285 |
| Detail | DEP | TextBox |  | 1806,903 1749x285 |  | 37:1532 | 43:1617 / 5379,672 1437x285 |
| Detail | ValL | TextBox | =Nz([MicDejun_V],0)+Nz([Dejun_V],0)+Nz([Cina_V],0) | 1804,618 1749x285 |  | 38:1563 | 45:1673 / 1800,672 1749x285 |
| Detail | LEGBCLA | Subform | Form.LEGBCLA | 3692,48 6966x1150 |  | 39:1595 | — |
| Detail | Text71 | TextBox | =IIf([DEP]<0,"Depășiri","Economii") | 123,903 1683x285 |  | 40:1615 | 36:1367 / 7968,48 2685x909 |
| Detail | Label68 | Label | Valoare Lista: | 123,618 1683x285 |  | 41:1661 | 46:1706 / 117,672 1683x285 |
| Detail | Label103 | Label | Comentarii Lista | 120,1260 1683x285 |  | 42:1683 | — |
| Detail | Comentarii | TextBox |  | 1803,1262 8841x285 |  | 43:1705 | — |
| Detail | DataLA | TextBox |  | 1803,360 1749x285 | OnChange | — | 2:520 |
| Detail | D | Subform | Form.d | 3692,1896 3441x2928 |  | — | 13:800 |
| Detail | CN | Subform | Form.cn | 7263,1896 3405x2928 |  | — | 21:995 |
| Detail | ValoareBon | TextBox |  | 5379,360 1437x285 |  | — | 34:1311 |
| Detail | Label66 | Label | Valoare BC: | 3696,360 1683x285 |  | — | 35:1343 |
| Detail | Command77 | CommandButton |  Alege B.C. | 6864,48 957x909 | OnClick | — | 37:1413 |
| Detail | NrBC | TextBox |  | 5379,48 1437x285 |  | — | 39:1511 |
| Detail | Label88 | Label | B.C. atașat: | 3696,48 1683x285 |  | — | 40:1541 |
| Detail | Label99 | Label | Depășiri / Economii | 3696,672 1683x285 |  | — | 44:1649 |
| FormFooter | btnSavNIR | CommandButton | SALVEAZĂ | 10752,72 1350x633 | OnClick | 1:1757 | 1:1751 |
| FormFooter | btnAddNIR | CommandButton | ADAUGĂ | 9318,72 1350x633 | OnClick | 2:1823 | 2:1817 |
| FormFooter | btnIesire | CommandButton | IEȘIRE | 72,72 1365x633 | OnClick | 3:1892 | 3:1886 |
| FormFooter | btnListNIR | CommandButton | LISTEAZĂ | 3264,72 1350x633 | OnClick | 4:1962 | 4:1956 |
| FormFooter | btnModNIR | CommandButton | MODIFICĂ | 7927,72 1350x633 | OnClick | 5:2032 | 5:2026 |
| FormFooter | btnUndoBC | CommandButton | RENUNȚĂ | 6528,72 1351x633 | OnClick | 6:2101 | 6:2095 |
| FormFooter | btnStergNIR | CommandButton | ȘTERGE | 14844,72 1350x633 | OnClick | 7:2168 | 7:2162 / 14109,72 1350x633 |
