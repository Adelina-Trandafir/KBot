# Table: @COMP

## Fields

| # | Name | Type | Size | Required | Default | Validation | Description |
|---|------|------|------|----------|---------|------------|-------------|
| 0 | IDCOMP | AutoNumber | 4 | False |  |  |  |
| 1 | IDP_D | Long | 4 | False | 0 |  |  |
| 2 | IDP_C | Long | 4 | False | 0 |  |  |
| 3 | SUMA | Double | 8 | False | 0 |  |  |

## Indexes

- ID: (IDCOMP)
- IDCC: (IDP_C)
- IDCD: (IDP_D)
- PrimaryKey: (IDCOMP) PRIMARY UNIQUE
