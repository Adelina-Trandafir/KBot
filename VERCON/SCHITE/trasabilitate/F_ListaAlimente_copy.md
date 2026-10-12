# Trasabilitate schita formular ListaAlimente_copy

Sursa: [Forms/ListaAlimente_copy.txt](../../VERCON_ACCESS/Forms/ListaAlimente_copy.txt) · imagine: [F_ListaAlimente_copy.png](../F_ListaAlimente_copy.png)

- Titlu (Caption): Lista Alimente
- Sursa de date (RecordSource): —
- Latime: 15579 twips · sectiuni: 5 · controale: 68

## FormHeader (inaltime 450 twips, linia 202)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label0 | Label | Listă de alimente | 0,0 10800x450 | 213 |  |
| Label44 | Label | Liste salvate | 10800,0 4779x450 | 232 |  |

## PageHeaderSection (inaltime 285 twips, linia 255)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| J | CheckBox |  | 0,0 260x240 | 265 |  |
| E | CheckBox |  | 168,0 260x240 | 276 |  |
| IDLL | TextBox |  | 432,0 516x240 | 290 |  |
| IDLISTA | TextBox |  | 948,0 516x240 | 307 |  |
| DV | TextBox |  | 1464,0 516x240 | 324 |  |
| CV | TextBox |  | 1980,0 516x240 | 341 |  |
| MDV | TextBox |  | 2496,0 516x240 | 358 |  |
| IDBC | TextBox |  | 3012,0 516x240 | 375 |  |
| DataBC | TextBox |  | 3528,0 516x240 | 392 |  |
| IDL | TextBox |  | 4044,0 516x240 | 409 |  |
| CAED | CheckBox |  | 4824,24 260x240 | 426 |  |
| Label96 | Label | Calculeaza automat Economi / Depasiri | 5058,0 2835x240 | 443 |  |
| Dif | TextBox | =nz([valbc],0)-nz([vall],0) | 7944,0 1011x285 | 459 |  |

## Detail (inaltime 4944 twips, linia 491)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| MD | Subform | Form.md | 120,1896 3405x2928 | 499 | OnExit |
| DataLA | TextBox |  | 1803,360 1749x285 | 520 | OnChange |
| Label3 | Label | Data L.A.: | 120,360 1683x285 | 550 |  |
| NumarLA | TextBox |  | 1803,48 1749x285 | 574 |  |
| Label4 | Label | Număr listă | 120,48 1683x285 | 601 |  |
| MicDejun_P | TextBox |  | 120,1566 825x255 | 625 |  |
| Label54 | Label | Cant. | 120,1311 825x255 | 649 |  |
| MicDejun_V | TextBox | =nz([MicDejun_P],0)*nz([vmd],0) | 1770,1566 825x255 | 676 |  |
| Label8 | Label | Valoare | 1770,1311 825x255 | 699 |  |
| MicDejun_D | TextBox |  | 2595,1566 930x255 | 725 |  |
| Label9 | Label | Distribuit | 2595,1311 930x255 | 748 |  |
| Label11 | Label |  Mic Dejun | 120,1058 3405x255 | 774 |  |
| D | Subform | Form.d | 3692,1896 3441x2928 | 800 |  |
| Dejun_P | TextBox |  | 3692,1566 825x255 | 820 |  |
| Label58 | Label | Cant. | 3692,1311 825x255 | 843 |  |
| Dejun_V | TextBox | =nz([Dejun_P],0)*nz([vd],0) | 5342,1566 825x255 | 870 |  |
| Label29 | Label | Valoare | 5342,1311 825x255 | 893 |  |
| Dejun_D | TextBox |  | 6167,1566 966x255 | 920 |  |
| Label31 | Label | Distribuit | 6167,1311 966x255 | 943 |  |
| Label32 | Label | Dejun | 3692,1058 3441x255 | 969 |  |
| CN | Subform | Form.cn | 7263,1896 3405x2928 | 995 |  |
| Cina_P | TextBox |  | 7263,1566 825x255 | 1015 |  |
| Label62 | Label | Cant. | 7263,1311 825x255 | 1038 |  |
| Cina_V | TextBox | =nz([cina_P],0)*nz([vc],0) | 8913,1566 825x255 | 1065 |  |
| Label38 | Label | Valoare | 8913,1311 825x255 | 1088 |  |
| Cina_D | TextBox |  | 9738,1566 930x255 | 1115 |  |
| Label40 | Label | Distribuit | 9738,1311 930x255 | 1138 |  |
| VMD | TextBox |  | 945,1566 825x255 | 1164 |  |
| Label55 | Label | Preț | 945,1311 825x255 | 1186 |  |
| VD | TextBox |  | 4517,1566 825x255 | 1213 |  |
| Label59 | Label | Preț | 4517,1311 825x255 | 1235 |  |
| VC | TextBox |  | 8088,1566 825x255 | 1262 |  |
| Label63 | Label | Preț | 8088,1311 825x255 | 1284 |  |
| ValoareBon | TextBox |  | 5379,360 1437x285 | 1311 |  |
| Label66 | Label | Valoare BC: | 3696,360 1683x285 | 1343 |  |
| Text71 | TextBox | =IIf([DEP]<0,"Depășiri","Economii") | 7968,48 2685x909 | 1367 |  |
| Command77 | CommandButton |  Alege B.C. | 6864,48 957x909 | 1413 | OnClick |
| Label86 | Label | Cină / Gustare | 7263,1056 3405x255 | 1485 |  |
| NrBC | TextBox |  | 5379,48 1437x285 | 1511 |  |
| Label88 | Label | B.C. atașat: | 3696,48 1683x285 | 1541 |  |
| subTreeView | Subform | Form.subTreeView | 10752,48 4707x4773 | 1565 |  |
| ValoareLista | TextBox |  | 1803,672 1749x285 | 1586 |  |
| DEP | TextBox |  | 5379,672 1437x285 | 1617 |  |
| Label99 | Label | Depășiri / Economii | 3696,672 1683x285 | 1649 |  |
| ValL | TextBox | =Nz([MicDejun_V],0)+Nz([Dejun_V],0)+Nz([Cina_V],0) | 1800,672 1749x285 | 1673 |  |
| Label68 | Label | Valoare Lista: | 117,672 1683x285 | 1706 |  |

## PageFooterSection (inaltime 0 twips, linia 1730)

_fara controale_

## FormFooter (inaltime 766 twips, linia 1741)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| btnSavNIR | CommandButton | SALVEAZĂ | 10752,72 1350x633 | 1751 | OnClick |
| btnAddNIR | CommandButton | ADAUGĂ | 9318,72 1350x633 | 1817 | OnClick |
| btnIesire | CommandButton | IEȘIRE | 72,72 1365x633 | 1886 | OnClick |
| btnListNIR | CommandButton | LISTEAZĂ | 3264,72 1350x633 | 1956 | OnClick |
| btnModNIR | CommandButton | MODIFICĂ | 7927,72 1350x633 | 2026 | OnClick |
| btnUndoBC | CommandButton | RENUNȚĂ | 6528,72 1351x633 | 2095 | OnClick |
| btnStergNIR | CommandButton | ȘTERGE | 14109,72 1350x633 | 2162 | OnClick |

