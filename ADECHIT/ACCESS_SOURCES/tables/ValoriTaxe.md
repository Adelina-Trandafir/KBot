# Table: ValoriTaxe

Linked table. Source: `ValoriTaxe`  Connect: `;DATABASE=C:\avasit\baza2020_PP.mdb`

## Fields

| # | Name | Type | Size | Required | Default | Validation | Description |
|---|------|------|------|----------|---------|------------|-------------|
| 0 | IDV | Long | 4 | True | 0 |  |  |
| 1 | TaxaLunara | Double | 8 | False | 0 |  |  |
| 2 | TaxaZilnica | Double | 8 | False | 0 |  |  |
| 3 | TaxaMicDejun | Double | 8 | False | 0 |  |  |
| 4 | TaxaDejun | Long | 4 | False | 0 |  |  |
| 5 | Activ | Yes/No | 1 | False | No |  |  |
| 6 | Expl | Text | 255 | False |  |  |  |
| 7 | Murdar | Yes/No | 1 | False | No |  |  |
| 8 | ZileCon | Long | 4 | False | 0 |  |  |
| 9 | DisCon | Double | 8 | False | 0 |  |  |
| 10 | ZilePre | Long | 4 | False | 0 |  |  |
| 11 | DisPre | Double | 8 | False | 0 |  |  |
| 12 | DisBro | Double | 8 | False | 0 |  |  |
| 13 | DoarCalculZilnic | Yes/No | 1 | False | No |  |  |
| 14 | TaxaMancare | Double | 8 | False |  |  |  |

## Indexes

- IDV: (IDV)
- PrimaryKey: (IDV) PRIMARY UNIQUE
