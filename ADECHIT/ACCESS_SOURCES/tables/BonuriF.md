# Table: BonuriF

Linked table. Source: `BonuriF`  Connect: `;DATABASE=C:\avasit\baza2020_PP.mdb`

## Fields

| # | Name | Type | Size | Required | Default | Validation | Description |
|---|------|------|------|----------|---------|------------|-------------|
| 0 | IDB | Long | 4 | True | 0 |  |  |
| 1 | IDPL | Long | 4 | False | 0 |  |  |
| 2 | NrBF | Long | 4 | False | 0 |  |  |
| 3 | DataBF | Date/Time | 8 | False |  |  |  |
| 4 | OraBF | Date/Time | 8 | False |  |  |  |
| 5 | Valoare | Double | 8 | False | 0 |  |  |
| 7 | Listat | Yes/No | 1 | False | No |  |  |
| 8 | Corect | Yes/No | 1 | False | No |  |  |
| 9 | MESAJ | Text | 255 | False |  |  |  |
| 10 | Anulat | Yes/No | 1 | False | No |  |  |
| 11 | Continut | Text | 255 | False |  |  |  |

## Indexes

- IdBonFiscal: (IDB)
- IDPL: (IDPL)
- PlatiBonuriF: (IDPL) FOREIGN
- PrimaryKey: (IDB) PRIMARY UNIQUE
