# Trasabilitate schita raport Bon_Consum

Sursa: [Reports/Bon_Consum.txt](../../VERCON_ACCESS/Reports/Bon_Consum.txt) · imagine: [R_Bon_Consum.png](../R_Bon_Consum.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT Iesiri_H.IdBonConsum, Masini.Masina, Clienti.Denumire AS Client, Iesiri_H.NumarBC AS NrFact, Iesiri_H.DataBC, Day([databc]) AS Ziua, Month([databc]) AS Luna, Year([databc]) AS Anul, Iesiri_H.ValoareBC, ProduseP.IdProdus, Produse.DenumireP, Produse.UM, Iesiri.Cant, Iesiri.PUTVA AS Pret, Iesiri.ValoareCUTVA AS Valoare, Iesiri_H.Expl FROM ((Masini INNER JOIN Produse ON Masini.IdMasina = Produs …`
- Latime: 10731 twips · sectiuni: 5 · controale: 44

## PageHeaderSection (inaltime 0 twips, linia 166)

_fara controale_

## GroupHeader0 (inaltime 2238 twips, linia 173)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text0 | TextBox | =DLookUp("Firma","caleTemp") | 0,0 3459x510 | 181 |  |
| Label2 | Label | Produs, lucrare (comandă) | 3461,0 3465x793 | 202 |  |
| Label3 | Label | Număr document ....................... | 0,510 1247x853 | 222 |  |
| Label5 | Label | Data | 1247,510 2214x283 | 242 |  |
| Label6 | Label | Ziua | 1247,793 737x284 | 264 |  |
| Label7 | Label | Luna | 1984,793 737x284 | 286 |  |
| Label8 | Label | Anul | 2721,793 737x284 | 308 |  |
| Ziua | TextBox | Ziua | 1247,1077 737x285 | 330 |  |
| Text10 | TextBox | Luna | 1984,1077 737x285 | 356 |  |
| Text11 | TextBox | Anul | 2721,1077 737x285 | 382 |  |
| Label12 | Label | Predator | 3458,793 1155x285 | 408 |  |
| Label13 | Label | Primitor | 4613,793 1185x285 | 430 |  |
| Label14 | Label | Nr. comandă | 5798,793 1125x285 | 452 |  |
| Text16 | TextBox |  | 3458,1077 1155x285 | 474 |  |
| Text17 | TextBox |  | 4613,1077 1185x285 | 499 |  |
| Text18 | TextBox | NrFact | 5798,1077 1125x285 | 524 |  |
| Label19 | Label | BON DE CONSUM ( COLECTIV ) | 6926,0 3783x1372 | 550 |  |
| Label20 | Label | Nr. crt. | 0,1716 630x510 | 571 |  |
| Label21 | Label | Denumirea materialelor (inclusiv sort, marca, profil, dimensiune) | 630,1716 3135x510 | 594 |  |
| Label22 | Label | Cant. necesară | 3773,1716 990x510 | 619 |  |
| Label23 | Label | Cod | 4763,1716 1470x510 | 644 |  |
| Label24 | Label | U/M | 6233,1716 690x510 | 669 |  |
| Label25 | Label | Cantitatea eliberată | 6923,1716 1185x510 | 694 |  |
| Label26 | Label | Prețul unitar | 8108,1716 1185x510 | 719 |  |
| Label27 | Label | Valoarea | 9293,1716 1425x510 | 744 |  |
| Expl | TextBox | Expl | 3456,336 3471x285 | 769 |  |

## Detail (inaltime 285 twips, linia 797)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text29 | TextBox | =1 | 0,0 630x285 | 805 |  |
| Produse.Denumire | TextBox | DenumireP | 630,0 3135x285 | 823 |  |
| Cant | TextBox | Cant | 3773,0 990x285 | 849 |  |
| xCodProdus | TextBox | IdProdus | 4763,0 1461x285 | 875 |  |
| UM | TextBox | UM | 6224,0 696x285 | 895 |  |
| Text35 | TextBox | Cant | 6923,0 1185x285 | 915 |  |
| Pret | TextBox | Pret | 8108,0 1185x285 | 938 |  |
| Valoare | TextBox | Valoare | 9293,0 1425x285 | 961 |  |

## GroupFooter1 (inaltime 1017 twips, linia 986)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text38 | TextBox | ="Data și semnătura" | 0,504 3471x510 | 995 |  |
| Text39 | TextBox | ="Șef compartiment" | 3458,504 2436x510 | 1018 |  |
| Text40 | TextBox | ="Gestionar" | 5896,504 2436x510 | 1044 |  |
| Text41 | TextBox | ="Primitor" | 8325,504 2391x510 | 1070 |  |
| Text42 | TextBox | Client | 5892,756 2436x261 | 1096 |  |
| Text43 | TextBox | ="TOTAL BON CONSUM" | 630,0 2835x285 | 1121 |  |
| Text46 | TextBox | =Sum([Valoare]) | 9285,0 1425x285 | 1146 |  |
| Box47 | Rectangle |  | 3465,0 5820x285 | 1171 |  |
| Box49 | Rectangle |  | 0,0 630x285 | 1188 |  |

## PageFooterSection (inaltime 510 twips, linia 1205)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,0 2736x477 | 1212 |  |

