# Table: Delegati_L

## Fields

| # | Name | Type | Size | Required | Default | Validation | Description |
|---|------|------|------|----------|---------|------------|-------------|
| 0 | IDD | Long | 4 | True | 0 |  |  |
| 1 | IDS | Long | 4 | False | 0 |  |  |
| 2 | Nume | Text | 50 | False |  |  |  |
| 3 | CNP | Text | 50 | False |  |  |  |
| 4 | DATEBI | Text | 50 | False |  |  |  |
| 5 | AUTO | Text | 50 | False |  |  |  |
| 6 | Activ | Yes/No | 1 | False | No |  |  |
| 7 | Editat | Yes/No | 1 | False | No |  |  |
| 8 | Sters | Yes/No | 1 | False |  |  |  |
| 9 | Nou | Yes/No | 1 | False | No |  |  |

## Indexes

- {81E82274-4CD9-4C44-BFE0-20A8F1AAE851}: (IDS) FOREIGN
- IdDelegat: (IDD)
- IDS: (IDS)
- Nume: (Nume)
- PrimaryKey: (IDD) PRIMARY UNIQUE
