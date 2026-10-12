# Trasabilitate schita raport ListaAlimente2015

Sursa: [Reports/ListaAlimente2015.txt](../../VERCON_ACCESS/Reports/ListaAlimente2015.txt) · imagine: [R_ListaAlimente2015.png](../R_ListaAlimente2015.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT ListaAlimente.IDLISTA, LegBCLA.IdBonConsum AS IdIesire, LABC.DATALA AS Data, ListaAlimente.Comentarii, LABC.NumarLA AS Numar, ListaAlimente.MicDejun_P, ListaAlimente.VMD, ListaAlimente.MicDejun_V, ListaAlimente.MicDejun_D, ListaAlimente.Dejun_P, ListaAlimente.VD, ListaAlimente.Dejun_V, ListaAlimente.Dejun_D, ListaAlimente.Cina_P, ListaAlimente.VC, ListaAlimente.Cina_V, ListaAlimente.Cina_D, …`
- Latime: 10565 twips · sectiuni: 5 · controale: 92

## ReportHeader (inaltime 863 twips, linia 166)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| SI | TextBox | =DLookUp("SI_LA","NR") | 0,0 1371x315 | 175 |  |
| IDLISTA | TextBox | IDLISTA | 1584,0 1701x315 | 190 |  |
| VBC | TextBox | Valoare | 1584,432 456x240 | 206 |  |
| SF | TextBox | =[dep]+[depa] | 0,336 1416x240 | 228 |  |
| DEPa | TextBox | =[QDP]+[si] | 3360,24 1416x240 | 249 |  |
| DEP | TextBox | DEP | 4920,0 1701x315 | 272 |  |

## PageHeaderSection (inaltime 3120 twips, linia 290)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Denumire | TextBox | =DLookUp("firma","CaleTemp") | 225,285 2715x285 | 297 |  |
| Label0 | Label | Denumire unitate: | 225,0 2715x570 | 320 |  |
| Label2 | Label | Şef de compartiment | 7980,0 2325x570 | 342 |  |
| Label3 | Label | Drepturi la masa | 1095,888 750x462 | 362 |  |
| Label4 | Label | Alocaţie LEI | 1845,888 750x462 | 384 |  |
| Label5 | Label | Efectiv LEI | 2595,888 750x462 | 406 |  |
| MicDejun_P | TextBox | MicDejun_P | 1095,1350 750x240 | 428 |  |
| Label6 | Label |  Mic dejun | 240,1350 855x240 | 464 |  |
| Dejun_P | TextBox | Dejun_P | 1095,1590 750x240 | 487 |  |
| Label7 | Label |  Dejun | 240,1590 855x240 | 523 |  |
| Cina_P | TextBox | Cina_P | 1095,1830 750x240 | 546 |  |
| Label8 | Label |  Cină | 240,1830 855x240 | 582 |  |
| Text12 | TextBox | VMD | 1845,1350 750x240 | 605 |  |
| Text13 | TextBox | VD | 1845,1590 750x240 | 641 |  |
| Text14 | TextBox | VC | 1845,1830 750x240 | 677 |  |
| MicDejun_V | TextBox | MicDejun_V | 2595,1350 750x240 | 713 |  |
| Dejun_V | TextBox | Dejun_V | 2595,1590 750x240 | 749 |  |
| Cina_V | TextBox | Cina_V | 2595,1830 750x240 | 785 |  |
| VLA | TextBox | =[Cina_V]+[dejun_v]+[micdejun_v] | 1095,2070 2250x240 | 821 |  |
| Label18 | Label |  TOTAL | 240,2070 855x240 | 857 |  |
| Label20 | Label | Mic dejun la masa | 8280,888 675x462 | 880 |  |
| Label21 | Label | Dejun | 8955,888 675x462 | 902 |  |
| Label22 | Label | Cină | 9630,888 675x462 | 924 |  |
| Label23 | Label |  Efectiv la hrană | 6975,1350 1305x240 | 946 |  |
| Label24 | Label |  Porţii preparate | 6975,1590 1305x240 | 967 |  |
| Label25 | Label |  Rămase | 6975,1830 1305x240 | 988 |  |
| MicDejun_P1 | TextBox | MicDejun_P | 8280,1350 675x240 | 1009 |  |
| Text28 | TextBox | =[micdejun_p]-[micdejun_d] | 8280,1830 675x240 | 1035 |  |
| Dejun_P1 | TextBox | Dejun_P | 8955,1350 675x240 | 1061 |  |
| Text31 | TextBox | =[dejun_p]-[dejun_d] | 8955,1830 675x240 | 1087 |  |
| Cina_P1 | TextBox | Cina_P | 9630,1350 675x240 | 1113 |  |
| Text34 | TextBox | =[cina_p]-[cina_d] | 9630,1830 675x240 | 1139 |  |
| Label35 | Label |  Distribute | 6975,2070 1305x240 | 1165 |  |
| MicDejun_D | TextBox | MicDejun_D | 8280,2070 675x240 | 1186 |  |
| Dejun_D | TextBox | Dejun_D | 8955,2070 675x240 | 1212 |  |
| Cina_D | TextBox | Cina_D | 9630,2070 675x240 | 1238 |  |
| Text43 | TextBox | MicDejun_P | 8280,1590 675x240 | 1264 |  |
| Text44 | TextBox | Dejun_P | 8955,1590 675x240 | 1290 |  |
| Text45 | TextBox | Cina_P | 9630,1590 675x240 | 1316 |  |
| Label46 | Label | Mese servite | 6975,888 1305x462 | 1342 |  |
| Label47 | Label | Recapitulaţia cheltuielilor pentru hrană | 3345,888 3630x246 | 1364 |  |
| Label48 | Label |  Economii sau depăşiri | 3345,1134 1710x216 | 1386 |  |
| Label52 | Label | Economii | 5055,1134 960x216 | 1410 |  |
| Label53 | Label | Depăşiri | 6015,1134 960x216 | 1432 |  |
| EcA | TextBox | =IIf(CDbl([depa])>0,0,Abs([depa])) | 5055,1350 960x240 | 1454 |  |
| Label49 | Label |  De la începutul perioadă | 3345,1350 1710x240 | 1488 |  |
| DeA | TextBox | =IIf(CDbl([depa])<0,0,Abs([depa])) | 6015,1350 960x240 | 1513 |  |
| Economii | TextBox | =IIf([dep]>0,0,Abs([dep])) | 5055,1590 960x240 | 1547 |  |
| Label50 | Label |  Pe ziua curentă | 3345,1590 1710x240 | 1581 |  |
| Depasiri | TextBox | =IIf([dep]<0,0,Abs([dep])) | 6015,1590 960x240 | 1606 |  |
| Text59 | TextBox | =IIf([sf]>0,0,Abs([sf])) | 5055,1830 960x240 | 1640 |  |
| Label51 | Label |  La finele perioadei | 3345,1830 1710x240 | 1674 |  |
| Text60 | TextBox | =IIf([sf]<0,0,Abs([sf])) | 6015,1830 960x240 | 1699 |  |
| Label61 | Label | Aprobat, _____________________ | 240,2376 4650x630 | 1733 |  |
| Label63 | Label | Vizat compartiment financiar contabil, _____________________ | 5682,2383 4650x630 | 1754 |  |
| Text75 | TextBox | =[Numar] & "/" & [Data] | 2940,285 5040x285 | 1775 |  |
| Label1 | Label | LISTĂ ZILNICĂ DE ALIMENTE | 2940,0 5040x285 | 1802 |  |
| Text40 | TextBox | Comentarii | 225,570 10080x285 | 1824 |  |

## Detail (inaltime 7754 twips, linia 1851)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| MicDejun subreport | Subform | Report.MicDejun subreport | 225,341 3285x2372 | 1860 |  |
| MicDejun subreport Label | Label | Mic Dejun | 225,56 3285x285 | 1881 |  |
| Dejun subreport | Subform | Report.Dejun subreport | 3630,345 3285x2372 | 1910 |  |
| Label67 | Label | Dejun | 3630,60 3285x285 | 1932 |  |
| Child68 | Subform | Report.Cina subreport | 7035,341 3285x2372 | 1960 |  |
| Label69 | Label | Cina / Gustare | 7035,56 3285x285 | 1981 |  |
| ListaABC | Subform | Report.BC subreport 2013 | 0,3630 10565x3899 | 2009 |  |
| Label17 | Label | TOTAL | 0,7529 9420x225 | 2027 |  |
| Label70 | Label | ALIMENTE | 0,2835 10485x330 | 2052 |  |
| Label9 | Label | Denumire | 0,3165 2206x465 | 2075 |  |
| Label10 | Label | U/M | 2206,3165 465x465 | 2098 |  |
| Label11 | Label | Cant. solic.  | 2671,3165 555x465 | 2123 |  |
| Label26 | Label | Pret unitar | 3780,3165 675x465 | 2148 |  |
| Label27 | Label | Valoare | 4455,3165 675x465 | 2173 |  |
| Text46 | TextBox | =[ListaABC]![sumvaloare] | 9420,7529 1065x225 | 2198 |  |
| Label29 | Label | Cant. elib.  | 3225,3165 555x465 | 2226 |  |
| Label30 | Label | Denumire | 5354,3165 2206x465 | 2251 |  |
| Label31 | Label | U/M | 7560,3165 465x465 | 2276 |  |
| Label32 | Label | Cant. solic.  | 8025,3165 555x465 | 2301 |  |
| Label33 | Label | Pret unitar | 9135,3165 675x465 | 2326 |  |
| Label34 | Label | Valoare | 9810,3165 675x465 | 2351 |  |
| Label36 | Label | Cant. elib.  | 8580,3165 555x465 | 2376 |  |

## PageFooterSection (inaltime 1365 twips, linia 2403)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label71 | Label | Întocmit, __________________ | 216,312 1680x510 | 2410 |  |
| Label72 | Label | Confirmat, __________________ | 2993,312 1680x510 | 2429 |  |
| Label73 | Label | Gestionar, __________________ | 5828,312 1680x510 | 2448 |  |
| Label74 | Label | Bucătar, __________________ | 8606,312 1680x510 | 2467 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,888 2736x477 | 2486 |  |
| Label42 | Label | PRODUSELE CORESPUND D.P.D.V. ORGANOLEPTIC. | 216,12 10068x300 | 2509 |  |

## ReportFooter (inaltime 0 twips, linia 2530)

_fara controale_

