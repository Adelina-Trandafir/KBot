# Trasabilitate schita raport NIR_SFP

Sursa: [Reports/NIR_SFP.txt](../../VERCON_ACCESS/Reports/NIR_SFP.txt) · imagine: [R_NIR_SFP.png](../R_NIR_SFP.png)

- Titlu (Caption): Nota de receptie si constatare diferente
- Sursa de date (RecordSource): `SELECT Furnizori.Denumire, Furnizori.Adresa, Furnizori.CF, Furnizori.J, Masini.Masina, NIR.NrFact, NIR.DataFc, NIR.Numar, NIR.Data, ProduseP.IdProdus, Produse.DenumireP, Produse.UM, ProduseP.PUTVA, Intrari.ID AS Ordine, Intrari.Cant, Intrari.Pret, Intrari.TVA, Intrari.Valoare, Intrari.ValoareCUTVA, ComisieNIR.Receptie1, ComisieNIR.Receptie2, ComisieNIR.Receptie3, ComisieNIR.Receptie4 FROM Masini I …`
- Latime: 11105 twips · sectiuni: 7 · controale: 78

## ReportHeader (inaltime 0 twips, linia 171)

_fara controale_

## PageHeaderSection (inaltime 855 twips, linia 179)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label7 | Label | NOTĂ DE RECEPȚIE ȘI CONSTATARE DE DIFERENȚE | 0,0 11085x435 | 186 |  |
| Text199 | TextBox | =DLookUp("Firma","caletemp") | 0,480 11085x375 | 202 |  |

## GroupHeader0 (inaltime 2892 twips, linia 225)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label15 | Label | Număr comandă | 7271,96 3621x507 | 234 |  |
| Label24 | Label | Număr | 4826,348 1140x255 | 258 |  |
| Label25 | Label | Data | 5966,348 1305x255 | 281 |  |
| Label28 | Label |  Subsemnații, membrii ai comisiei de recepție ,am procedat la recepționarea valorilor materiale furnizate de | 192,1008 4947x603 | 304 |  |
| DenumirePartener | TextBox | Denumire | 5139,1008 2586x603 | 325 |  |
| Adresa | TextBox | Adresa | 7725,1008 1701x603 | 347 |  |
| Label31 | Label | cu auto nr. | 9426,1008 1452x603 | 369 |  |
| Label33 | Label | Documente însoțitoare: | 192,1704 2148x255 | 390 |  |
| Label36 | Label | , constatăndu-se următoarele:  Marfa corespunde d.p.d.v. cantitativ | 5157,1704 5727x255 | 410 |  |
| Label146 | Label | Cod furnizor | 192,96 1511x507 | 430 |  |
| Label147 | Label | Nr. Factură / Aviz Expediere | 1703,96 1878x507 | 453 |  |
| Label148 | Label | Data Expedierii | 3581,96 1245x507 | 476 |  |
| Label149 | Label | Nota de recepție | 4826,96 2445x249 | 499 |  |
| IDF | TextBox | CF | 192,603 1511x264 | 522 |  |
| NrFact | TextBox | NrFact | 1703,603 1878x264 | 546 |  |
| Text171 | TextBox | DataFc | 3581,603 1245x264 | 570 |  |
| Text172 | TextBox | Numar | 4826,603 1140x264 | 594 |  |
| Text174 | TextBox | Data | 5966,603 1305x264 | 618 |  |
| Text175 | TextBox | NrFact | 2340,1704 2817x255 | 642 |  |
| Label96 | Label | Nr. crt. | 24,2328 505x552 | 668 |  |
| Label97 | Label | DENUMIRE ARTICOL | 1750,2328 3165x552 | 692 |  |
| Label99 | Label | UM | 4915,2328 520x552 | 716 |  |
| Label100 | Label | Cant. cnf. Docum. | 5435,2328 885x552 | 740 |  |
| Label136 | Label | COD ARTICOL | 529,2328 1221x552 | 763 |  |
| Label137 | Label | RECEPŢIONAT | 6320,2328 4785x285 | 787 |  |
| Label139 | Label | Cantitate | 6320,2613 1125x267 | 810 |  |
| Label140 | Label | Preţ unitar | 7445,2613 1365x267 | 833 |  |
| Label141 | Label | Valoare | 8810,2613 2295x267 | 856 |  |
| Text202 | TextBox | Masina | 7271,603 3627x264 | 879 |  |
| Box16 | Rectangle |  | 192,96 10698x779 | 903 |  |

## Detail (inaltime 240 twips, linia 921)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text49 | TextBox | =1 | 24,0 505x240 | 929 |  |
| CodProdu | TextBox | IdProdus | 529,0 1221x240 | 949 |  |
| Text177 | TextBox | DenumireP | 1750,0 3165x240 | 970 |  |
| UM | TextBox | UM | 4915,0 520x240 | 991 |  |
| Text179 | TextBox | Cant | 5435,0 885x240 | 1012 |  |
| Text180 | TextBox | Cant | 6320,0 1125x240 | 1033 |  |
| Text181 | TextBox | PUTVA | 7445,0 1365x240 | 1057 |  |
| Text182 | TextBox | ValoareCUTVA | 8810,0 2295x240 | 1081 |  |
| Line204 | Line |  | 24,0 0x240 | 1105 |  |
| Line205 | Line |  | 529,0 0x240 | 1118 |  |
| Line206 | Line |  | 1750,0 0x240 | 1131 |  |
| Line207 | Line |  | 5435,0 0x240 | 1144 |  |
| Line208 | Line |  | 6320,0 0x240 | 1157 |  |
| Line209 | Line |  | 11105,0 0x240 | 1170 |  |

## GroupFooter1 (inaltime 0 twips, linia 1185)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Line188 | Line |  | 79,360 10920x0 | 1194 |  |
| Line189 | Line |  | 79,1365 10920x0 | 1207 |  |
| Line190 | Line |  | 72,363 10965x1007 | 1220 |  |
| Label66 | Label | TOTALURI | 7445,0 1365x285 | 1235 |  |
| tVAL | TextBox | =Sum([ValoareCUTVA]) | 8810,0 2295x285 | 1256 |  |
| Line210 | Line |  | 24,0 7392x0 | 1280 |  |

## PageFooterSection (inaltime 1725 twips, linia 1292)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label65 | Label | CERTIFICARE COMISIA DE RECEPȚIE | 48,25 7200x285 | 1299 |  |
| Label71 | Label | NUME ȘI PRENUME | 48,312 1800x255 | 1320 |  |
| Label72 | Label | SEMNATURA | 1848,312 1800x255 | 1343 |  |
| Label73 | Label | NUME ȘI PRENUME | 3648,312 1800x255 | 1366 |  |
| Label74 | Label | SEMNATURA | 5448,312 1800x255 | 1389 |  |
| Box155 | Rectangle |  | 48,567 1800x255 | 1412 |  |
| Box156 | Rectangle |  | 48,822 1800x255 | 1426 |  |
| Box157 | Rectangle |  | 1848,567 1800x255 | 1440 |  |
| Box158 | Rectangle |  | 1848,822 1800x255 | 1454 |  |
| Box159 | Rectangle |  | 3648,567 1800x255 | 1468 |  |
| Box160 | Rectangle |  | 3648,822 1800x255 | 1482 |  |
| Box161 | Rectangle |  | 5448,567 1800x255 | 1496 |  |
| Box162 | Rectangle |  | 5448,822 1800x255 | 1510 |  |
| Box163 | Rectangle |  | 7248,567 1920x510 | 1524 |  |
| Box165 | Rectangle |  | 9168,567 1875x510 | 1538 |  |
| Label166 | Label | DATA PRIMIRII | 7248,312 1920x255 | 1552 |  |
| Label167 | Label | SEMNATURA | 9168,312 1875x255 | 1575 |  |
| Receptie1 | TextBox | Receptie1 | 48,567 1800x255 | 1598 |  |
| Receptie2 | TextBox | Receptie2 | 48,822 1800x255 | 1620 |  |
| Text196 | TextBox | Receptie3 | 3648,567 1800x255 | 1643 |  |
| Text197 | TextBox | Receptie4 | 3648,822 1800x255 | 1666 |  |
| Label201 | TextBox | =DLookUp("Reprezentant","Cale","ID=" & DLookUp("ID","CaleTemp")) | 8298,24 2745x285 | 1689 |  |
| Label75 | Label | Gestionar | 7248,27 3795x285 | 1712 |  |
| Line212 | Line |  | 0,1248 11085x0 | 1736 |  |
| Text46 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 5976,1248 5040x285 | 1747 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 48,1197 2736x477 | 1771 |  |

## ReportFooter (inaltime 0 twips, linia 1799)

_fara controale_

