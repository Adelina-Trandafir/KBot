# Table: Platitori_L

## Fields

| # | Name | Type | Size | Required | Default | Validation | Description |
|---|------|------|------|----------|---------|------------|-------------|
| 0 | IDP | Long | 4 | True | 0 |  |  |
| 1 | IDF | Long | 4 | False | 0 |  |  |
| 2 | IDG | Long | 4 | False | 0 |  |  |
| 3 | IDV | Long | 4 | False | 0 |  |  |
| 4 | Nume | Text | 255 | False |  |  |  |
| 5 | Adresa | Text | 255 | False |  |  |  |
| 6 | CNP | Text | 255 | False |  |  |  |
| 7 | Frate | Yes/No | 1 | False | No |  |  |
| 8 | Plecat | Yes/No | 1 | False | No |  |  |
| 9 | Avans | Double | 8 | False | 0 |  |  |
| 10 | SI | Long | 4 | False | 0 |  |  |
| 11 | DataIntrare | Date/Time | 8 | False | =Now() |  |  |
| 12 | DataIesire | Date/Time | 8 | False |  |  |  |
| 13 | Sters | Yes/No | 1 | False | No |  |  |
| 14 | Editat | Yes/No | 1 | False | No |  |  |
| 15 | S | Yes/No | 1 | False | No |  |  |

## Indexes

- {F911206E-C6E8-4179-AE70-433C326B843E}: (IDG) FOREIGN
- IDF: (IDF)
- IDG: (IDG)
- IdOm: (IDP)
- IDV: (IDV)
- Nume: (Nume)
- PrimaryKey: (IDP) PRIMARY UNIQUE
