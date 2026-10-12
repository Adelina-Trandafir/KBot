# Trasabilitate schita raport BUConsum

Sursa: [Reports/BUConsum.txt](../../VERCON_ACCESS/Reports/BUConsum.txt) · imagine: [R_BUConsum.png](../R_BUConsum.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT Day([databc]) AS Ziua, Month([databc]) AS Luna, Year([databc]) AS Anul, Iesiri_H.NumarBC AS NrFact, ProduseP.IdProdus, Produse.DenumireP, Sum(Iesiri.Cant) AS Cant, Produse.UM, Sum(Iesiri.Cant) AS SumOfCant, Round([produsep]![PUTVA],[tempvars]![zec]) AS Pret, Sum(Round([Iesiri]![cant]*[produsep]![putva],[tempvars]![zec])) AS Valoare, Clienti.Denumire AS cDenumire, Iesiri_H.Expl, Masini.Masin …`
- Latime: 10755 twips · sectiuni: 5 · controale: 49

## PageHeaderSection (inaltime 686 twips, linia 163)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text0 | TextBox | =DLookUp("FIrma","CaleTemp") | 0,0 5229x343 | 170 |  |
| Text53 | TextBox | =DLookUp("Detalii","CaleTemp") | 0,343 5229x343 | 194 |  |
| Expl | TextBox | Expl | 5490,343 5229x343 | 221 |  |
| Text52 | TextBox | Masina | 5496,0 5229x343 | 249 |  |

## GroupHeader0 (inaltime 1470 twips, linia 278)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Ziua | TextBox | Ziua | 1271,546 737x255 | 286 |  |
| Label6 | Label | Ziua | 1271,292 737x254 | 310 |  |
| Text10 | TextBox | Luna | 2008,546 737x254 | 333 |  |
| Label7 | Label | Luna | 2008,292 737x254 | 358 |  |
| Text11 | TextBox | Anul | 2745,546 842x254 | 381 |  |
| Label8 | Label | Anul | 2745,292 842x254 | 406 |  |
| Text16 | TextBox |  | 3587,292 1155x509 | 429 |  |
| Label12 | Label | Predător | 3590,24 1155x268 | 453 |  |
| Text17 | TextBox | cDenumire | 4742,292 1755x509 | 476 |  |
| Label13 | Label | Primitor | 4745,24 1755x268 | 501 |  |
| Text18 | TextBox | NrFact | 6497,292 1380x509 | 524 |  |
| Label14 | Label | Nr. comandă | 6500,24 1380x268 | 549 |  |
| Label20 | Label | Nr. crt. | 30,960 450x510 | 572 |  |
| Label21 | Label | Denumirea materialelor (inclusiv sort, marca, profil, dimensiune) | 480,960 3111x510 | 594 |  |
| Label22 | Label | Cantitatea necesară | 5286,960 1209x510 | 616 |  |
| LCod | Label | Cod | 4221,960 1065x510 | 638 |  |
| Label24 | Label | U/M | 3591,960 630x510 | 660 |  |
| Label25 | Label | Cantitatea eliberată | 6495,960 1380x510 | 682 |  |
| Label26 | Label | Prețul unitar | 7875,960 1380x510 | 704 |  |
| Label27 | Label | Valoare | 9255,960 1500x510 | 726 |  |
| Label19 | Label | BON DE CONSUM ( COLECTIV ) | 7880,24 2868x778 | 748 |  |
| Label5 | Label | Data | 1271,24 2319x268 | 771 |  |
| Label3 | TextBox | ="Număr document " | 24,24 1247x778 | 792 |  |

## Detail (inaltime 285 twips, linia 818)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text29 | TextBox | =1 | 24,0 450x285 | 826 |  |
| Produse.Denumire | TextBox | =UCase([DenumireP]) | 474,0 3111x285 | 847 |  |
| Cant | TextBox | Cant | 5280,0 1209x285 | 873 |  |
| TCod | TextBox | IdProdus | 4215,0 1065x285 | 899 |  |
| UMx | TextBox | =UCase(Nz([UM],"")) | 3585,0 636x285 | 921 |  |
| Text35 | TextBox | Cant | 6489,0 1380x285 | 943 |  |
| Pret | TextBox | Pret | 7869,0 1380x285 | 969 |  |
| Valoare | TextBox | Valoare | 9249,0 1500x285 | 995 |  |

## GroupFooter1 (inaltime 1038 twips, linia 1023)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text38 | TextBox | ="Data si semnatura" | 24,408 3471x630 | 1032 |  |
| Text39 | TextBox | ="Sef compartiment" | 3495,408 2436x630 | 1055 |  |
| Text40 | TextBox | ="Gestionar" | 5931,408 2436x630 | 1079 |  |
| Text41 | TextBox | ="Primitor" | 8367,408 2379x630 | 1102 |  |
| T1 | TextBox | =DLookUp("Gestionar","CaleTemp") | 5931,633 2436x405 | 1126 |  |
| Text43 | TextBox | ="TOTAL BON CONSUM" | 474,0 3111x285 | 1151 |  |
| Text46 | TextBox | =Sum([Valoare]) | 9249,0 1500x285 | 1175 |  |
| T2 | TextBox | cDenumire | 8367,633 2379x405 | 1201 |  |
| cDenumire | TextBox | cDenumire | 3912,624 1701x345 | 1226 |  |
| Text54 | TextBox |  | 24,0 450x285 | 1244 |  |
| Text55 | TextBox |  | 3585,0 5670x285 | 1268 |  |

## PageFooterSection (inaltime 597 twips, linia 1294)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text50 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 8346,24 2400x285 | 1301 |  |
| Line51 | Line |  | 0,0 10752x0 | 1324 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,0 2736x477 | 1334 |  |

