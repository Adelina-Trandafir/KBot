# Table: DatePickerDate

## Fields

| # | Name | Type | Size | Required | Default | Validation | Description |
|---|------|------|------|----------|---------|------------|-------------|
| 0 | ID | AutoNumber | 4 | False |  |  |  |
| 1 | DI | Date/Time | 8 | False |  |  |  |
| 2 | DSF | Date/Time | 8 | False |  |  |  |
| 3 | IDZ | Long | 4 | False | 0 |  |  |
| 4 | Absent | Yes/No | 1 | False | No |  |  |
| 5 | NrZile | Long | 4 | False | 0 |  |  |

## Indexes

- IDP: (IDZ)
- PrimaryKey: (ID) PRIMARY UNIQUE
