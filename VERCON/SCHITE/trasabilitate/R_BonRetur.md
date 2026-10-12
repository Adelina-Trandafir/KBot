# Trasabilitate schita raport BonRetur

Sursa: [Reports/BonRetur.txt](../../VERCON_ACCESS/Reports/BonRetur.txt) · imagine: [R_BonRetur.png](../R_BonRetur.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT Day([iesiri_H].[databc]) AS Ziua, Month([iesiri_H].[databc]) AS Luna, Year([iesiri_H].[databc]) AS Anul, Iesiri_H.NumarBC AS NrFact, ProduseP.IdProdus, Produse.DenumireP, Sum(IesiriRetur.Cant) AS Cant, Produse.UM, Round([produsep]![PUTVA],[tempvars]![zec]) AS Pret, Sum(Round([iesiriretur].[cant]*[produsep]![putva],[tempvars]![zec])) AS Valoare, Clienti.Denumire AS cDenumire, Iesiri_H.Expl,  …`
- Latime: 10731 twips · sectiuni: 5 · controale: 41

## PageHeaderSection (inaltime 0 twips, linia 156)

_fara controale_

## GroupHeader0 (inaltime 1875 twips, linia 163)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text0 | TextBox | =DLookUp("Firma","caleTemp") | 0,0 3471x510 | 171 |  |
| Label2 | Label | Produs, lucrare (comanda) | 3465,0 3465x793 | 188 |  |
| Label3 | Label | Numar document ....................... | 0,510 1247x853 | 202 |  |
| Label5 | Label | Data | 1251,510 2214x283 | 217 |  |
| Label6 | Label | Ziua | 1254,793 737x284 | 233 |  |
| Label7 | Label | Luna | 1991,793 737x284 | 249 |  |
| Label8 | Label | Anul | 2728,793 737x284 | 265 |  |
| Ziua | TextBox | Ziua | 1254,1077 737x285 | 281 |  |
| Text10 | TextBox | Luna | 1991,1077 737x285 | 301 |  |
| Text11 | TextBox | Anul | 2728,1077 737x285 | 321 |  |
| Label12 | Label | Predator | 3465,793 1155x285 | 341 |  |
| Label13 | Label | Primitor | 4620,793 1185x285 | 357 |  |
| Label14 | Label | Nr. comanda | 5805,795 1125x285 | 373 |  |
| Text16 | TextBox | BCOLD | 3465,1077 1155x285 | 389 |  |
| Text17 | TextBox |  | 4620,1077 1185x285 | 409 |  |
| Text18 | TextBox | NrFact | 5805,1077 1125x285 | 428 |  |
| Label19 | Label | BON DE CONSUM ( RETUR ) | 6930,0 3783x1360 | 448 |  |
| Label20 | Label | Nr. crt. | 0,1365 630x510 | 464 |  |
| Label21 | Label | Denumirea materialelor (inclusiv sort, marca, profil, dimensiune) | 630,1365 2835x510 | 479 |  |
| Label22 | Label | Cantitatea necesara | 3473,1365 1290x510 | 495 |  |
| Label23 | Label | Cod | 4763,1365 1470x510 | 511 |  |
| Label24 | Label | U/M | 6233,1365 690x510 | 527 |  |
| Label25 | Label | Cantitatea eliberata | 6923,1365 1185x510 | 543 |  |
| Label26 | Label | Pretul unitar | 8108,1365 1185x510 | 559 |  |
| Label27 | Label | Valoare | 9293,1365 1425x510 | 575 |  |

## Detail (inaltime 285 twips, linia 593)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text29 | TextBox | =1 | 0,0 630x285 | 601 |  |
| Produse.Denumire | TextBox | DenumireP | 630,0 2835x285 | 617 |  |
| Cant | TextBox | Cant | 3473,0 1290x285 | 638 |  |
| CodProdus | TextBox | IdProdus | 4763,0 1461x285 | 658 |  |
| UM | TextBox | UM | 6224,0 696x285 | 677 |  |
| Text35 | TextBox | Cant | 6923,0 1185x285 | 695 |  |
| Pret | TextBox | Pret | 8108,0 1185x285 | 715 |  |
| Valoare | TextBox | Valoare | 9293,0 1425x285 | 735 |  |

## GroupFooter1 (inaltime 795 twips, linia 757)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text38 | TextBox | ="Data si semnatura" | 0,285 3471x510 | 766 |  |
| Text39 | TextBox | ="Sef compartiment" | 3458,285 2436x510 | 784 |  |
| Text40 | TextBox | ="Gestionar" | 5896,285 2436x510 | 804 |  |
| Text41 | TextBox | ="Primitor" | 8325,285 2391x510 | 823 |  |
| Text42 | TextBox | cDenumire | 5895,510 2436x285 | 843 |  |
| Text43 | TextBox | ="TOTAL BON CONSUM" | 630,0 2835x285 | 864 |  |
| Text46 | TextBox | =Sum([Valoare]) | 9285,0 1425x285 | 885 |  |

## PageFooterSection (inaltime 510 twips, linia 909)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,0 2736x477 | 916 |  |

