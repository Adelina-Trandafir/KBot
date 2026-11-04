"""SLICE-AD19-02: an additional opening component, separate from Platitori.SI."""


def carried_total(data, child_id, year):
    return sum(row['SOLD'] for row in data.get('CarriedBalances', [])
               if row['IDP'] == child_id and row['AN_START'] <= year)


def carried_entries(rows):
    """The effective date is the first day of the retained year."""
    return [{'date': f"{row['AN_START']:04d}-01-01", 'value': row['SOLD'],
             'text': f"Sold reportat din anii anteriori (început {row['AN_START']})"}
            for row in sorted(rows, key=lambda row: row['AN_START'])]
