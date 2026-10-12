# Trasabilitate schita raport Amortizare

Sursa: [Reports/Amortizare.txt](../../VERCON_ACCESS/Reports/Amortizare.txt) · imagine: [R_Amortizare.png](../R_Amortizare.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `Amortizare`
- Latime: 15531 twips · sectiuni: 7 · controale: 39

## ReportHeader (inaltime 0 twips, linia 162)

_fara controale_

## PageHeaderSection (inaltime 1400 twips, linia 170)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label0 | Label | Listare amortizare luna | 0,0 5385x460 | 177 |  |
| Text1 | TextBox | =UCase(Format([Forms]![SFP]![dsf],"mmmm\\/yyyy")) | 5385,0 3006x460 | 194 |  |
| Label60 | Label | Număr inventar | 0,680 1140x720 | 216 |  |
| Label61 | Label | Denumire mijloc fix | 1140,680 4111x720 | 240 |  |
| Label62 | Label | Dată intrare | 5251,680 1140x720 | 266 |  |
| Label63 | Label | Dată achiziţ. | 6391,680 1140x720 | 292 |  |
| Label64 | Label | Durată normală folosire | 8671,680 1141x720 | 318 |  |
| Label65 | Label | Valoare intrare | 7531,680 1140x720 | 343 |  |
| Label66 | Label | Durată rămasă | 13235,680 1141x720 | 369 |  |
| Label67 | Label | Durată folosită | 10953,680 1141x720 | 395 |  |
| Label68 | Label | Valoare amortiz. | 12094,680 1141x720 | 421 |  |
| Label69 | Label | Valoare ramasă | 14376,680 1151x720 | 447 |  |
| Label70 | Label | Valoare lunară | 9812,680 1141x720 | 473 |  |
| Text0 | TextBox | =DLookUp("FIrma","CaleTemp") | 8391,0 5781x460 | 499 |  |

## GroupHeader0 (inaltime 275 twips, linia 525)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| lblGrp0Head | Label | Clasificare | 0,0 3230x275 | 535 |  |
| txtGrp0Head | TextBox | Categorie | 3220,0 12311x275 | 555 |  |

## Detail (inaltime 285 twips, linia 581)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| CodProdus | TextBox | CodProdus | 0,0 1140x285 | 589 |  |
| Text12 | TextBox | DenumireProdus | 1140,0 4111x285 | 611 |  |
| Text14 | TextBox | DataIntrare | 5251,0 1141x285 | 635 |  |
| Text15 | TextBox | DataAchizitie | 6392,0 1141x285 | 661 |  |
| ValoareIntrare | TextBox | ValoareIntrare | 7533,0 1141x285 | 687 |  |
| Text30 | TextBox | DurataNormala | 8674,0 1141x285 | 712 |  |
| Text31 | TextBox | ValoareLuna | 9815,0 1141x285 | 737 |  |
| Text32 | TextBox | DurataFolosinta | 10956,0 1141x285 | 762 |  |
| ValoareAmortizata | TextBox | ValoareAmortizata | 12097,0 1141x285 | 787 |  |
| Text34 | TextBox | DurataRamasa | 13238,0 1141x285 | 812 |  |
| VLR | TextBox | ValoareRamasa | 14379,0 1141x285 | 837 |  |

## GroupFooter1 (inaltime 396 twips, linia 864)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text41 | TextBox | =Sum([ValoareIntrare]) | 6918,0 1756x275 | 874 |  |
| Text43 | TextBox | =Sum([ValoareLuna]) | 8670,0 2286x275 | 900 |  |
| Text44 | TextBox | =Sum([ValoareAmortizata]) | 10957,0 2281x275 | 927 |  |
| Text45 | TextBox | =Sum([ValoareRamasa]) | 13239,0 2281x275 | 954 |  |
| Label36 | Label | Total... | 0,0 6920x275 | 981 |  |

## PageFooterSection (inaltime 477 twips, linia 1004)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text46 | TextBox | ="Pagina " & [Page] | 10487,0 5040x285 | 1011 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,0 2736x477 | 1034 |  |

## ReportFooter (inaltime 340 twips, linia 1059)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text2 | TextBox | =Sum([ValoareIntrare]) | 6918,0 1756x285 | 1067 |  |
| Text3 | TextBox | =Sum([ValoareLuna]) | 8670,0 2286x285 | 1093 |  |
| Text4 | TextBox | =Sum([ValoareAmortizata]) | 10957,0 2281x285 | 1120 |  |
| Text5 | TextBox | =Sum([ValoareRamasa]) | 13239,0 2281x285 | 1147 |  |
| Label6 | Label | TOTAL AMORTIZARE LUNA | 0,0 6920x285 | 1174 |  |

