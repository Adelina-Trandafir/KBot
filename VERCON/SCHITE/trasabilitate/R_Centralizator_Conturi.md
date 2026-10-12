# Trasabilitate schita raport Centralizator_Conturi

Sursa: [Reports/Centralizator_Conturi.txt](../../VERCON_ACCESS/Reports/Centralizator_Conturi.txt) · imagine: [R_Centralizator_Conturi.png](../R_Centralizator_Conturi.png)

- Titlu (Caption): Balanta produse
- Sursa de date (RecordSource): `Balanta22`
- Latime: 10227 twips · sectiuni: 6 · controale: 26

## ReportHeader (inaltime 624 twips, linia 212)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text71 | TextBox | =[Forms]![sfp]![di] | 4794,0 2499x459 | 220 |  |
| Label44 | Label | Centralizator conturi | 0,0 4794x459 | 242 |  |
| Text73 | TextBox | =[Forms]![sfp]![dsf] | 7770,0 2451x459 | 263 |  |
| Label74 | Label | / | 7293,0 477x459 | 286 |  |

## PageHeaderSection (inaltime 531 twips, linia 310)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Denumire_Label | Label | Denumire cont | 0,36 3987x495 | 317 |  |
| Valoare Initiala_Label | Label | Valoare Iniţiala | 3987,36 1560x495 | 337 |  |
| Valoare Intrata_Label | Label | Valoare Intrată | 5547,36 1560x495 | 359 |  |
| Valoare Iesita_Label | Label | Valoare Ieşita | 7107,36 1560x495 | 381 |  |
| Valoare Finala_Label | Label | Valoare Finală | 8667,36 1560x495 | 403 |  |

## Detail (inaltime 291 twips, linia 427)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Denumire | TextBox | DENP | 0,0 3987x285 | 436 |  |
| Valoare Initiala | TextBox | VI | 3987,3 1155x285 | 455 |  |
| Valoare Intrata | TextBox | VCIn | 5142,6 1155x285 | 481 |  |
| Valoare Iesita | TextBox | VCOut | 6297,6 1155x285 | 507 |  |
| Valoare Finala | TextBox | VF | 7452,0 1380x285 | 533 |  |

## SubsolGrup0 (inaltime 291 twips, linia 559)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text107 | TextBox | Masina | 0,0 3987x285 | 568 |  |
| Text108 | TextBox | =sum([VI]) | 3987,0 1560x285 | 587 |  |
| Text109 | TextBox | =sum([VCIn]) | 5547,0 1560x285 | 613 |  |
| Text110 | TextBox | =sum([VCOut]) | 7107,0 1560x285 | 639 |  |
| Text111 | TextBox | =sum([VF]) | 8667,0 1560x285 | 665 |  |

## PageFooterSection (inaltime 573 twips, linia 693)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text46 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 5184,72 5040x285 | 700 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,72 2736x477 | 724 |  |

## ReportFooter (inaltime 288 twips, linia 750)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label70 | Label | TOTAL General: | 0,0 3987x285 | 758 |  |
| Text100 | TextBox | =Sum([VI]) | 3987,0 1560x285 | 774 |  |
| Text102 | TextBox | =Sum([VCIn]) | 5547,3 1560x285 | 802 |  |
| Text104 | TextBox | =Sum([VCOut]) | 7107,3 1560x285 | 833 |  |
| Text106 | TextBox | =Sum([VF]) | 8667,3 1560x285 | 864 |  |

