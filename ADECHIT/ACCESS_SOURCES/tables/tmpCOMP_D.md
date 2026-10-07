# Table: tmpCOMP_D

## Fields

| # | Name | Type | Size | Required | Default | Validation | Description |
|---|------|------|------|----------|---------|------------|-------------|
| 0 | IDCD | AutoNumber | 4 | False |  |  |  |
| 1 | D_IDP | Long | 4 | False | 0 |  |  |
| 2 | D_IDG | Long | 4 | False | 0 |  |  |
| 3 | D_GRUPA | Text | 255 | False |  |  |  |
| 4 | D_NUME | Text | 255 | False |  |  |  |
| 5 | D_SOLD | Double | 8 | False | 0 |  |  |
| 6 | D_REST | Double | 8 | False | 0 |  |  |

## Indexes

- ID: (IDCD)
- IDP_D: (D_IDP)
- PrimaryKey: (IDCD) PRIMARY UNIQUE
