Option Strict On

''' <summary>
''' The aggregate a <see cref="KBotDataColumn"/> contributes to the pinned totals row
''' (slice 0017-01). <see cref="KBotDataView.ShowTotalsRow"/> must be True for the row to
''' appear; a column with <see cref="KBotAggregate.None"/> renders an empty totals cell.
''' Aggregates are computed over ALL rows in the model (not only the visible ones), skipping
''' cells that are <c>Nothing</c> or non-numeric for <see cref="Sum"/> / <see cref="Average"/>.
''' </summary>
Public Enum KBotAggregate

    ''' <summary>No aggregate — the column's totals cell stays empty.</summary>
    None = 0

    ''' <summary>Sum of the numeric cells (non-numeric / Nothing cells are skipped).</summary>
    Sum = 1

    ''' <summary>Count of rows whose cell HAS a stored value (<see cref="KBotDataRow.HasValue"/>),
    ''' NOT the count of non-empty numeric cells. Always rendered as a plain integer.</summary>
    Count = 2

    ''' <summary>Mean of the numeric cells. Zero countable cells renders empty (never 0, never NaN).</summary>
    Average = 3

End Enum
