# Table: IMPXLS

## Fields

| # | Name | Type | Size | Required | Default | Validation | Description |
|---|------|------|------|----------|---------|------------|-------------|
| 0 | ID | AutoNumber | 4 | False |  |  |  |
| 1 | CNP | Text | 255 | False |  |  |  |
| 2 | Nume | Text | 255 | False |  |  |  |
| 3 | IT | Text | 255 | False |  |  |  |
| 4 | Prenume | Text | 255 | False |  |  |  |
| 5 | Prenume_2 | Text | 255 | False |  |  |  |
| 6 | Prenume_3 | Text | 255 | False |  |  |  |
| 7 | SI | Double | 8 | True | 0 | Is Not Null |  |
| 8 | S | Yes/No | 1 | False | No |  |  |
| 9 | NumePrenume | Text | 255 | False |  |  |  |
| 10 | Adresa | Text | 255 | False |  |  |  |
| 11 | IDP | Long | 4 | False | 0 |  |  |

## Indexes

- ID: (ID) PRIMARY UNIQUE
- IDP: (IDP)
- Nume: (Nume)
- NumePrenume: (NumePrenume)
