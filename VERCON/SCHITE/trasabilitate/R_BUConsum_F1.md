# Trasabilitate schita raport BUConsum_F1

Sursa: [Reports/BUConsum_F1.txt](../../VERCON_ACCESS/Reports/BUConsum_F1.txt) · imagine: [R_BUConsum_F1.png](../R_BUConsum_F1.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT Iesiri_H.DataBC, Day([databc]) AS Ziua, Month([databc]) AS Luna, Year([databc]) AS Anul, Iesiri_H.NumarBC AS NrFact, Iesiri.Denumire, Sum(Iesiri.Cant) AS Cant, Iesiri.CodProdus, Iesiri.UM, Sum(Iesiri.Cant) AS SumOfCant, Round([PUTVA],[tempvars]![zec]) AS Pret, Sum(Round([Iesiri]![cant]*[putva],[tempvars]![zec])) AS Valoare, Clienti.Denumire AS cDenumire, Iesiri_H.Expl, Masini.Masina, Furniz …`
- Latime: 11213 twips · sectiuni: 5 · controale: 50

## PageHeaderSection (inaltime 0 twips, linia 163)

_fara controale_

## GroupHeader0 (inaltime 1998 twips, linia 170)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Expl | TextBox | Expl | 3485,427 3465x375 | 178 |  |
| Label2 | Label | Produs, lucrare (comandă) | 3485,0 3465x1372 | 198 |  |
| Text0 | TextBox | =DLookUp("FIrma","CaleTemp") | 24,0 3459x1372 | 221 |  |
| Label3 | Label | Categoria | 24,519 1751x853 | 244 |  |
| Label5 | Label | Data | 1775,519 1710x283 | 267 |  |
| Label6 | Label | Ziua | 1778,803 569x284 | 290 |  |
| Label7 | Label | Luna | 2347,803 569x284 | 313 |  |
| Label8 | Label | Anul | 2916,803 569x284 | 336 |  |
| Ziua | TextBox | Ziua | 1778,1087 569x285 | 359 |  |
| Text10 | TextBox | Luna | 2347,1087 569x285 | 383 |  |
| Text11 | TextBox | Anul | 2916,1087 569x285 | 407 |  |
| Label12 | Label | Predător | 3482,802 1155x285 | 431 |  |
| Label13 | Label | Primitor | 4637,802 1101x285 | 454 |  |
| Label14 | Label | Nr. comandă | 5738,802 1209x285 | 477 |  |
| Text16 | TextBox |  | 3482,1087 1155x285 | 500 |  |
| Text17 | TextBox |  | 4637,1087 1101x285 | 523 |  |
| Text18 | TextBox | NrFact | 5738,1087 1209x285 | 546 |  |
| Label20 | Label | Nr. crt. | 24,1488 354x510 | 570 |  |
| Label21 | Label | Denumirea materialelor (inclusiv sort, marca, profil, dimensiune) | 378,1488 3111x510 | 592 |  |
| Label22 | Label | Cantitatea necesară | 7611,1488 822x510 | 614 |  |
| LCod | Label | Cod | 4059,1488 1122x510 | 636 |  |
| Label24 | Label | U/M | 3489,1488 570x510 | 659 |  |
| Label25 | Label | Cantitatea eliberată | 8433,1488 822x510 | 682 |  |
| Label26 | Label | Prețul unitar | 9255,1488 822x510 | 704 |  |
| Label27 | Label | Valoare | 10077,1488 1113x510 | 726 |  |
| Label19 | Label | BON DE CONSUM ( COLECTIV ) | 6950,0 4263x1372 | 749 |  |
| Label52 | Label | Furnizorul | 5181,1488 2430x510 | 769 |  |
| Masina | TextBox | Masina | 24,802 1751x561 | 793 |  |

## Detail (inaltime 285 twips, linia 819)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text29 | TextBox | =1 | 24,0 354x285 | 827 |  |
| Produse.Denumire | TextBox | =UCase([Denumire]) | 378,0 3111x285 | 848 |  |
| Cant | TextBox | Cant | 7617,0 822x285 | 873 |  |
| TCod | TextBox | =UCase(Left([CodProdus],10)) | 4065,0 1122x285 | 899 |  |
| UMx | TextBox | =UCase(Nz([UM],"")) | 3489,0 576x285 | 921 |  |
| Text35 | TextBox | Cant | 8439,0 822x285 | 943 |  |
| Pret | TextBox | Pret | 9261,0 822x285 | 968 |  |
| Valoare | TextBox | Valoare | 10083,0 1113x285 | 993 |  |
| tFurnizor | TextBox | =UCase([Furnizor]) | 5187,0 2430x285 | 1018 |  |

## GroupFooter1 (inaltime 1269 twips, linia 1042)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text43 | TextBox | ="TOTAL BON CONSUM" | 378,0 3111x285 | 1051 |  |
| Text46 | TextBox | =Sum([Valoare]) | 9264,0 1929x285 | 1075 |  |
| Text53 | TextBox |  | 3489,0 5775x285 | 1101 |  |
| Text54 | TextBox |  | 24,0 354x285 | 1125 |  |
| Line51 | Line |  | 0,1032 11208x0 | 1149 |  |
| DataBC | TextBox | DataBC | 24,633 3471x285 | 1162 |  |
| Text38 | Label | DATA ȘI SEMNĂTURA | 24,408 3471x510 | 1187 |  |
| Text39 | Label | ȘEF COMPARTIMENT | 3495,408 2436x510 | 1211 |  |
| T2 | TextBox | cDenumire | 8367,633 2835x285 | 1233 |  |
| Text41 | Label | PRIMITOR | 8367,408 2835x510 | 1258 |  |
| T1 | TextBox |  | 5931,633 2436x285 | 1282 |  |
| Text40 | Label | GESTIONAR | 5931,408 2436x510 | 1305 |  |

## PageFooterSection (inaltime 477 twips, linia 1331)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,0 2736x477 | 1338 |  |

