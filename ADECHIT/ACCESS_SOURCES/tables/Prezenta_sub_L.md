# Table: Prezenta_sub_L

## Fields

| # | Name | Type | Size | Required | Default | Validation | Description |
|---|------|------|------|----------|---------|------------|-------------|
| 0 | IDX | Long | 4 | True | 0 |  |  |
| 1 | IDZ | Long | 4 | False | 0 |  |  |
| 2 | IDL | Long | 4 | False | 0 |  |  |
| 3 | DI | Date/Time | 8 | False |  |  |  |
| 4 | DSF | Date/Time | 8 | False |  |  |  |
| 5 | NrZile | Long | 4 | False | 0 |  |  |
| 6 | Absent | Yes/No | 1 | False | No |  |  |
| 7 | Editat | Yes/No | 1 | False | No |  |  |
| 8 | Sters | Yes/No | 1 | False | No |  |  |
| 9 | Nou | Yes/No | 1 | False | No |  |  |

## Indexes

- IDX: (IDX)
- IDZ: (IDZ)
- Prezenta_LPrezenta_sub_L: (IDZ) FOREIGN
- PrimaryKey: (IDX) PRIMARY UNIQUE
