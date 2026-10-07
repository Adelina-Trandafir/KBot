# Table: _TBLS_

## Fields

| # | Name | Type | Size | Required | Default | Validation | Description |
|---|------|------|------|----------|---------|------------|-------------|
| 0 | NumTbl | Text | 50 | False |  |  |  |
| 1 | Typ | Byte | 1 | False | 0 |  | 1 - Local  2 - Baza de date  3 - Cale.mdb |
| 2 | Struct | Long | 4 | True |  |  |  |
| 3 | Mdi | Yes/No | 1 | False | 0 |  |  |
| 4 | Bz | Text | 255 | False |  |  |  |

## Indexes

- NumTbl: (NumTbl)
- PrimaryKey: (Struct) PRIMARY UNIQUE
- Struct: (Struct)
