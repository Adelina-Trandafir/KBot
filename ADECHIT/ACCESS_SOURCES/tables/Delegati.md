# Table: Delegati

Linked table. Source: `Delegati`  Connect: `;DATABASE=C:\avasit\baza2020_PP.mdb`

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

## Indexes

- IdDelegat: (IDD)
- IDS: (IDS)
- Nume: (Nume)
- Platitori_subDelegati: (IDS) FOREIGN
- PrimaryKey: (IDD) PRIMARY UNIQUE
