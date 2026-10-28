# Trasabilitate schite bonuri de consum și retur

Convenții și limite: [citirea fișelor consolidate](../INDEX.md#fise-consolidate). SLICE-D003-16, 28.10.2026. Secțiunea este coloana întâi.

## Variante și surse

### Trasabilitate schita raport Bon_Consum

Sursa: [Reports/Bon_Consum.txt](../../VERCON_ACCESS/Reports/Bon_Consum.txt) · imagine: [R_Bon_Consum.png](../R_Bon_Consum.png)

- Titlu (Caption): — · Sursa de date (RecordSource): `SELECT Iesiri_H.IdBonConsum, Masini.Masina, Clienti.Denumire AS Client, Iesiri_H.NumarBC AS NrFact, Iesiri_H.DataBC, Day([databc]) AS Ziua, Month([databc]) AS Luna, Year([databc]) AS Anul, Iesiri_H.ValoareBC, ProduseP.IdProdus, Produse.DenumireP, Produse.UM, Iesiri.Cant, Iesiri.PUTVA AS Pret, Iesiri.ValoareCUTVA AS Valoare, Iesiri_H.Expl FROM ((Masini INNER JOIN Produse ON Masini.IdMasina = Produs …` · Latime: 10731 twips · sectiuni: 5 · controale: 44

### Trasabilitate schita raport BonRetur

Sursa: [Reports/BonRetur.txt](../../VERCON_ACCESS/Reports/BonRetur.txt) · imagine: [R_BonRetur.png](../R_BonRetur.png)

- Titlu (Caption): — · Sursa de date (RecordSource): `SELECT Day([iesiri_H].[databc]) AS Ziua, Month([iesiri_H].[databc]) AS Luna, Year([iesiri_H].[databc]) AS Anul, Iesiri_H.NumarBC AS NrFact, ProduseP.IdProdus, Produse.DenumireP, Sum(IesiriRetur.Cant) AS Cant, Produse.UM, Round([produsep]![PUTVA],[tempvars]![zec]) AS Pret, Sum(Round([iesiriretur].[cant]*[produsep]![putva],[tempvars]![zec])) AS Valoare, Clienti.Denumire AS cDenumire, Iesiri_H.Expl,  …` · Latime: 10731 twips · sectiuni: 5 · controale: 41

### Trasabilitate schita raport BUConsum

Sursa: [Reports/BUConsum.txt](../../VERCON_ACCESS/Reports/BUConsum.txt) · imagine: [R_BUConsum.png](../R_BUConsum.png)

- Titlu (Caption): — · Sursa de date (RecordSource): `SELECT Day([databc]) AS Ziua, Month([databc]) AS Luna, Year([databc]) AS Anul, Iesiri_H.NumarBC AS NrFact, ProduseP.IdProdus, Produse.DenumireP, Sum(Iesiri.Cant) AS Cant, Produse.UM, Sum(Iesiri.Cant) AS SumOfCant, Round([produsep]![PUTVA],[tempvars]![zec]) AS Pret, Sum(Round([Iesiri]![cant]*[produsep]![putva],[tempvars]![zec])) AS Valoare, Clienti.Denumire AS cDenumire, Iesiri_H.Expl, Masini.Masin …` · Latime: 10755 twips · sectiuni: 5 · controale: 49

### Trasabilitate schita raport BUConsum_F

Sursa: [Reports/BUConsum_F.txt](../../VERCON_ACCESS/Reports/BUConsum_F.txt) · imagine: [R_BUConsum_F.png](../R_BUConsum_F.png)

- Titlu (Caption): — · Sursa de date (RecordSource): `SELECT Day([databc]) AS Ziua, Month([databc]) AS Luna, Year([databc]) AS Anul, Iesiri_H.NumarBC AS NrFact, Iesiri.Denumire, Sum(Iesiri.Cant) AS Cant, Iesiri.CodProdus, Iesiri.UM, Sum(Iesiri.Cant) AS SumOfCant, Round([PUTVA],[tempvars]![zec]) AS Pret, Sum(Round([Iesiri]![cant]*[putva],[tempvars]![zec])) AS Valoare, Clienti.Denumire AS cDenumire, Iesiri_H.Expl, Masini.Masina, Furnizori.Denumire AS F …` · Latime: 11189 twips · sectiuni: 5 · controale: 4

### Trasabilitate schita raport BUConsum_F1

Sursa: [Reports/BUConsum_F1.txt](../../VERCON_ACCESS/Reports/BUConsum_F1.txt) · imagine: [R_BUConsum_F1.png](../R_BUConsum_F1.png)

- Titlu (Caption): — · Sursa de date (RecordSource): `SELECT Iesiri_H.DataBC, Day([databc]) AS Ziua, Month([databc]) AS Luna, Year([databc]) AS Anul, Iesiri_H.NumarBC AS NrFact, Iesiri.Denumire, Sum(Iesiri.Cant) AS Cant, Iesiri.CodProdus, Iesiri.UM, Sum(Iesiri.Cant) AS SumOfCant, Round([PUTVA],[tempvars]![zec]) AS Pret, Sum(Round([Iesiri]![cant]*[putva],[tempvars]![zec])) AS Valoare, Clienti.Denumire AS cDenumire, Iesiri_H.Expl, Masini.Masina, Furniz …` · Latime: 11213 twips · sectiuni: 5 · controale: 50

## Secțiuni

| Secțiune | Bon_Consum | BonRetur | BUConsum | BUConsum_F | BUConsum_F1 |
|---|---|---|---|---|---|
| PageHeaderSection <a id="pageheadersection-inaltime-0-twips-linia-166"></a><a id="pageheadersection-inaltime-0-twips-linia-156"></a><a id="pageheadersection-inaltime-686-twips-linia-163"></a><a id="pageheadersection-inaltime-0-twips-linia-472"></a><a id="pageheadersection-inaltime-0-twips-linia-163"></a> | 1 / 0 / 166 | 1 / 0 / 156 | 1 / 686 / 163 | 1 / 0 / 472 | 1 / 0 / 163 |
| GroupHeader0 <a id="groupheader0-inaltime-2238-twips-linia-173"></a><a id="groupheader0-inaltime-1875-twips-linia-163"></a><a id="groupheader0-inaltime-1470-twips-linia-278"></a><a id="groupheader0-inaltime-1704-twips-linia-479"></a><a id="groupheader0-inaltime-1998-twips-linia-170"></a> | 2 / 2238 / 173 | 2 / 1875 / 163 | 2 / 1470 / 278 | 2 / 1704 / 479 | 2 / 1998 / 170 |
| Detail <a id="detail-inaltime-285-twips-linia-797"></a><a id="detail-inaltime-285-twips-linia-593"></a><a id="detail-inaltime-285-twips-linia-818"></a><a id="detail-inaltime-0-twips-linia-534"></a><a id="detail-inaltime-285-twips-linia-819"></a> | 3 / 285 / 797 | 3 / 285 / 593 | 3 / 285 / 818 | 3 / 0 / 534 | 3 / 285 / 819 |
| GroupFooter1 <a id="groupfooter1-inaltime-1017-twips-linia-986"></a><a id="groupfooter1-inaltime-795-twips-linia-757"></a><a id="groupfooter1-inaltime-1038-twips-linia-1023"></a><a id="groupfooter1-inaltime-1532-twips-linia-542"></a><a id="groupfooter1-inaltime-1269-twips-linia-1042"></a> | 4 / 1017 / 986 | 4 / 795 / 757 | 4 / 1038 / 1023 | 4 / 1532 / 542 | 4 / 1269 / 1042 |
| PageFooterSection <a id="pagefootersection-inaltime-510-twips-linia-1205"></a><a id="pagefootersection-inaltime-510-twips-linia-909"></a><a id="pagefootersection-inaltime-597-twips-linia-1294"></a><a id="pagefootersection-inaltime-0-twips-linia-571"></a><a id="pagefootersection-inaltime-477-twips-linia-1331"></a> | 5 / 510 / 1205 | 5 / 510 / 909 | 5 / 597 / 1294 | 5 / 0 / 571 | 5 / 477 / 1331 |

## Controale

| Secțiune | Control | Tip | Text / camp / sursa | Pozitie comună (twips) | Evenimente | Bon_Consum | BonRetur | BUConsum | BUConsum_F | BUConsum_F1 |
|---|---|---|---|---|---|---|---|---|---|---|
| PageHeaderSection | Text0 | TextBox | =DLookUp("FIrma","CaleTemp") | 0,0 5229x343 |  | — | — | 1:170 | — | — |
| PageHeaderSection | Text53 | TextBox | =DLookUp("Detalii","CaleTemp") | 0,343 5229x343 |  | — | — | 2:194 | — | — |
| PageHeaderSection | Expl | TextBox | Expl | 5490,343 5229x343 |  | — | — | 3:221 | — | — |
| PageHeaderSection | Text52 | TextBox | Masina | 5496,0 5229x343 |  | — | — | 4:249 | — | — |
| GroupHeader0 | Text0 | TextBox | =DLookUp("Firma","caleTemp") | 0,0 3459x510 |  | 1:181 | 1:171 / 0,0 3471x510 | — | — | — |
| GroupHeader0 | Label2 | Label | Produs, lucrare (comandă) | 3461,0 3465x793 |  | 2:202 | — | — | — | 2:198 / 3485,0 3465x1372 |
| GroupHeader0 | Label3 | Label | Număr document ....................... | 0,510 1247x853 |  | 3:222 | — | — | — | — |
| GroupHeader0 | Label5 | Label | Data | 1247,510 2214x283 |  | 4:242 | 4:217 / 1251,510 2214x283 | 22:771 / 1271,24 2319x268 | — | 5:267 / 1775,519 1710x283 |
| GroupHeader0 | Label6 | Label | Ziua | 1247,793 737x284 |  | 5:264 | 5:233 / 1254,793 737x284 | 2:310 / 1271,292 737x254 | — | 6:290 / 1778,803 569x284 |
| GroupHeader0 | Label7 | Label | Luna | 1984,793 737x284 |  | 6:286 | 6:249 / 1991,793 737x284 | 4:358 / 2008,292 737x254 | — | 7:313 / 2347,803 569x284 |
| GroupHeader0 | Label8 | Label | Anul | 2721,793 737x284 |  | 7:308 | 7:265 / 2728,793 737x284 | 6:406 / 2745,292 842x254 | — | 8:336 / 2916,803 569x284 |
| GroupHeader0 | Ziua | TextBox | Ziua | 1247,1077 737x285 |  | 8:330 | 8:281 / 1254,1077 737x285 | 1:286 / 1271,546 737x255 | — | 9:359 / 1778,1087 569x285 |
| GroupHeader0 | Text10 | TextBox | Luna | 1984,1077 737x285 |  | 9:356 | 9:301 / 1991,1077 737x285 | 3:333 / 2008,546 737x254 | — | 10:383 / 2347,1087 569x285 |
| GroupHeader0 | Text11 | TextBox | Anul | 2721,1077 737x285 |  | 10:382 | 10:321 / 2728,1077 737x285 | 5:381 / 2745,546 842x254 | — | 11:407 / 2916,1087 569x285 |
| GroupHeader0 | Label12 | Label | Predator | 3458,793 1155x285 |  | 11:408 | 11:341 / 3465,793 1155x285 | — | — | — |
| GroupHeader0 | Label13 | Label | Primitor | 4613,793 1185x285 |  | 12:430 | 12:357 / 4620,793 1185x285 | 10:501 / 4745,24 1755x268 | — | 13:454 / 4637,802 1101x285 |
| GroupHeader0 | Label14 | Label | Nr. comandă | 5798,793 1125x285 |  | 13:452 | — | 12:549 / 6500,24 1380x268 | — | 14:477 / 5738,802 1209x285 |
| GroupHeader0 | Text16 | TextBox |  | 3458,1077 1155x285 |  | 14:474 | — | 7:429 / 3587,292 1155x509 | — | 15:500 / 3482,1087 1155x285 |
| GroupHeader0 | Text17 | TextBox |  | 4613,1077 1185x285 |  | 15:499 | 15:409 / 4620,1077 1185x285 | — | — | 16:523 / 4637,1087 1101x285 |
| GroupHeader0 | Text18 | TextBox | NrFact | 5798,1077 1125x285 |  | 16:524 | 16:428 / 5805,1077 1125x285 | 11:524 / 6497,292 1380x509 | — | 17:546 / 5738,1087 1209x285 |
| GroupHeader0 | Label19 | Label | BON DE CONSUM ( COLECTIV ) | 6926,0 3783x1372 |  | 17:550 | — | 21:748 / 7880,24 2868x778 | — | 26:749 / 6950,0 4263x1372 |
| GroupHeader0 | Label20 | Label | Nr. crt. | 0,1716 630x510 |  | 18:571 | 18:464 / 0,1365 630x510 | 13:572 / 30,960 450x510 | — | 18:570 / 24,1488 354x510 |
| GroupHeader0 | Label21 | Label | Denumirea materialelor (inclusiv sort, marca, profil, dimensiune) | 630,1716 3135x510 |  | 19:594 | 19:479 / 630,1365 2835x510 | 14:594 / 480,960 3111x510 | — | 19:592 / 378,1488 3111x510 |
| GroupHeader0 | Label22 | Label | Cant. necesară | 3773,1716 990x510 |  | 20:619 | — | — | — | — |
| GroupHeader0 | Label23 | Label | Cod | 4763,1716 1470x510 |  | 21:644 | 21:511 / 4763,1365 1470x510 | — | — | — |
| GroupHeader0 | Label24 | Label | U/M | 6233,1716 690x510 |  | 22:669 | 22:527 / 6233,1365 690x510 | 17:660 / 3591,960 630x510 | — | 22:659 / 3489,1488 570x510 |
| GroupHeader0 | Label25 | Label | Cantitatea eliberată | 6923,1716 1185x510 |  | 23:694 | — | 18:682 / 6495,960 1380x510 | — | 23:682 / 8433,1488 822x510 |
| GroupHeader0 | Label26 | Label | Prețul unitar | 8108,1716 1185x510 |  | 24:719 | — | 19:704 / 7875,960 1380x510 | — | 24:704 / 9255,1488 822x510 |
| GroupHeader0 | Label27 | Label | Valoarea | 9293,1716 1425x510 |  | 25:744 | — | — | — | — |
| GroupHeader0 | Expl | TextBox | Expl | 3456,336 3471x285 |  | 26:769 | — | — | — | 1:178 / 3485,427 3465x375 |
| GroupHeader0 | Label2 | Label | Produs, lucrare (comanda) | 3465,0 3465x793 |  | — | 2:188 | — | — | — |
| GroupHeader0 | Label3 | Label | Numar document ....................... | 0,510 1247x853 |  | — | 3:202 | — | — | — |
| GroupHeader0 | Label14 | Label | Nr. comanda | 5805,795 1125x285 |  | — | 13:373 | — | — | — |
| GroupHeader0 | Text16 | TextBox | BCOLD | 3465,1077 1155x285 |  | — | 14:389 | — | — | — |
| GroupHeader0 | Label19 | Label | BON DE CONSUM ( RETUR ) | 6930,0 3783x1360 |  | — | 17:448 | — | — | — |
| GroupHeader0 | Label22 | Label | Cantitatea necesara | 3473,1365 1290x510 |  | — | 20:495 | — | — | — |
| GroupHeader0 | Label25 | Label | Cantitatea eliberata | 6923,1365 1185x510 |  | — | 23:543 | — | — | — |
| GroupHeader0 | Label26 | Label | Pretul unitar | 8108,1365 1185x510 |  | — | 24:559 | — | — | — |
| GroupHeader0 | Label27 | Label | Valoare | 9293,1365 1425x510 |  | — | 25:575 | 20:726 / 9255,960 1500x510 | — | 25:726 / 10077,1488 1113x510 |
| GroupHeader0 | Label12 | Label | Predător | 3590,24 1155x268 |  | — | — | 8:453 | — | 12:431 / 3482,802 1155x285 |
| GroupHeader0 | Text17 | TextBox | cDenumire | 4742,292 1755x509 |  | — | — | 9:476 | — | — |
| GroupHeader0 | Label22 | Label | Cantitatea necesară | 5286,960 1209x510 |  | — | — | 15:616 | — | 20:614 / 7611,1488 822x510 |
| GroupHeader0 | LCod | Label | Cod | 4221,960 1065x510 |  | — | — | 16:638 | — | 21:636 / 4059,1488 1122x510 |
| GroupHeader0 | Label3 | TextBox | ="Număr document " | 24,24 1247x778 |  | — | — | 23:792 | — | — |
| GroupHeader0 | BUConsum_F1 | Subform | Report.BUConsum_F1 | 0,0 11189x1340 |  | — | — | — | 1:488 | — |
| GroupHeader0 | Line59 | Line |  | 0,1536 11189x0 |  | — | — | — | 2:502 | — |
| GroupHeader0 | Label63 | Label |  | 0,1392 600x312 |  | — | — | — | 3:516 | — |
| GroupHeader0 | Text0 | TextBox | =DLookUp("FIrma","CaleTemp") | 24,0 3459x1372 |  | — | — | — | — | 3:221 |
| GroupHeader0 | Label3 | Label | Categoria | 24,519 1751x853 |  | — | — | — | — | 4:244 |
| GroupHeader0 | Label52 | Label | Furnizorul | 5181,1488 2430x510 |  | — | — | — | — | 27:769 |
| GroupHeader0 | Masina | TextBox | Masina | 24,802 1751x561 |  | — | — | — | — | 28:793 |
| Detail | Text29 | TextBox | =1 | 0,0 630x285 |  | 1:805 | 1:601 | 1:826 / 24,0 450x285 | — | 1:827 / 24,0 354x285 |
| Detail | Produse.Denumire | TextBox | DenumireP | 630,0 3135x285 |  | 2:823 | 2:617 / 630,0 2835x285 | — | — | — |
| Detail | Cant | TextBox | Cant | 3773,0 990x285 |  | 3:849 | 3:638 / 3473,0 1290x285 | 3:873 / 5280,0 1209x285 | — | 3:873 / 7617,0 822x285 |
| Detail | xCodProdus | TextBox | IdProdus | 4763,0 1461x285 |  | 4:875 | — | — | — | — |
| Detail | UM | TextBox | UM | 6224,0 696x285 |  | 5:895 | 5:677 | — | — | — |
| Detail | Text35 | TextBox | Cant | 6923,0 1185x285 |  | 6:915 | 6:695 | 6:943 / 6489,0 1380x285 | — | 6:943 / 8439,0 822x285 |
| Detail | Pret | TextBox | Pret | 8108,0 1185x285 |  | 7:938 | 7:715 | 7:969 / 7869,0 1380x285 | — | 7:968 / 9261,0 822x285 |
| Detail | Valoare | TextBox | Valoare | 9293,0 1425x285 |  | 8:961 | 8:735 | 8:995 / 9249,0 1500x285 | — | 8:993 / 10083,0 1113x285 |
| Detail | CodProdus | TextBox | IdProdus | 4763,0 1461x285 |  | — | 4:658 | — | — | — |
| Detail | Produse.Denumire | TextBox | =UCase([DenumireP]) | 474,0 3111x285 |  | — | — | 2:847 | — | — |
| Detail | TCod | TextBox | IdProdus | 4215,0 1065x285 |  | — | — | 4:899 | — | — |
| Detail | UMx | TextBox | =UCase(Nz([UM],"")) | 3585,0 636x285 |  | — | — | 5:921 | — | 5:921 / 3489,0 576x285 |
| Detail | Produse.Denumire | TextBox | =UCase([Denumire]) | 378,0 3111x285 |  | — | — | — | — | 2:848 |
| Detail | TCod | TextBox | =UCase(Left([CodProdus],10)) | 4065,0 1122x285 |  | — | — | — | — | 4:899 |
| Detail | tFurnizor | TextBox | =UCase([Furnizor]) | 5187,0 2430x285 |  | — | — | — | — | 9:1018 |
| GroupFooter1 | Text38 | TextBox | ="Data și semnătura" | 0,504 3471x510 |  | 1:995 | — | — | — | — |
| GroupFooter1 | Text39 | TextBox | ="Șef compartiment" | 3458,504 2436x510 |  | 2:1018 | — | — | — | — |
| GroupFooter1 | Text40 | TextBox | ="Gestionar" | 5896,504 2436x510 |  | 3:1044 | 3:804 / 5896,285 2436x510 | 3:1079 / 5931,408 2436x630 | — | — |
| GroupFooter1 | Text41 | TextBox | ="Primitor" | 8325,504 2391x510 |  | 4:1070 | 4:823 / 8325,285 2391x510 | 4:1102 / 8367,408 2379x630 | — | — |
| GroupFooter1 | Text42 | TextBox | Client | 5892,756 2436x261 |  | 5:1096 | — | — | — | — |
| GroupFooter1 | Text43 | TextBox | ="TOTAL BON CONSUM" | 630,0 2835x285 |  | 6:1121 | 6:864 | 6:1151 / 474,0 3111x285 | — | 1:1051 / 378,0 3111x285 |
| GroupFooter1 | Text46 | TextBox | =Sum([Valoare]) | 9285,0 1425x285 |  | 7:1146 | 7:885 | 7:1175 / 9249,0 1500x285 | — | 2:1075 / 9264,0 1929x285 |
| GroupFooter1 | Box47 | Rectangle |  | 3465,0 5820x285 |  | 8:1171 | — | — | — | — |
| GroupFooter1 | Box49 | Rectangle |  | 0,0 630x285 |  | 9:1188 | — | — | — | — |
| GroupFooter1 | Text38 | TextBox | ="Data si semnatura" | 0,285 3471x510 |  | — | 1:766 | 1:1032 / 24,408 3471x630 | — | — |
| GroupFooter1 | Text39 | TextBox | ="Sef compartiment" | 3458,285 2436x510 |  | — | 2:784 | 2:1055 / 3495,408 2436x630 | — | — |
| GroupFooter1 | Text42 | TextBox | cDenumire | 5895,510 2436x285 |  | — | 5:843 | — | — | — |
| GroupFooter1 | T1 | TextBox | =DLookUp("Gestionar","CaleTemp") | 5931,633 2436x405 |  | — | — | 5:1126 | — | — |
| GroupFooter1 | T2 | TextBox | cDenumire | 8367,633 2379x405 |  | — | — | 8:1201 | — | 9:1233 / 8367,633 2835x285 |
| GroupFooter1 | cDenumire | TextBox | cDenumire | 3912,624 1701x345 |  | — | — | 9:1226 | — | — |
| GroupFooter1 | Text54 | TextBox |  | 24,0 450x285 |  | — | — | 10:1244 | — | 4:1125 / 24,0 354x285 |
| GroupFooter1 | Text55 | TextBox |  | 3585,0 5670x285 |  | — | — | 11:1268 | — | — |
| GroupFooter1 | Child58 | Subform | Report.BUConsum_F1 | 0,192 11189x1340 |  | — | — | — | 1:553 | — |
| GroupFooter1 | Text53 | TextBox |  | 3489,0 5775x285 |  | — | — | — | — | 3:1101 |
| GroupFooter1 | Line51 | Line |  | 0,1032 11208x0 |  | — | — | — | — | 5:1149 |
| GroupFooter1 | DataBC | TextBox | DataBC | 24,633 3471x285 |  | — | — | — | — | 6:1162 |
| GroupFooter1 | Text38 | Label | DATA ȘI SEMNĂTURA | 24,408 3471x510 |  | — | — | — | — | 7:1187 |
| GroupFooter1 | Text39 | Label | ȘEF COMPARTIMENT | 3495,408 2436x510 |  | — | — | — | — | 8:1211 |
| GroupFooter1 | Text41 | Label | PRIMITOR | 8367,408 2835x510 |  | — | — | — | — | 10:1258 |
| GroupFooter1 | T1 | TextBox |  | 5931,633 2436x285 |  | — | — | — | — | 11:1282 |
| GroupFooter1 | Text40 | Label | GESTIONAR | 5931,408 2436x510 |  | — | — | — | — | 12:1305 |
| PageFooterSection | Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,0 2736x477 |  | 1:1212 | 1:916 | 3:1334 | — | 1:1338 |
| PageFooterSection | Text50 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 8346,24 2400x285 |  | — | — | 1:1301 | — | — |
| PageFooterSection | Line51 | Line |  | 0,0 10752x0 |  | — | — | 2:1324 | — | — |
