Option Strict On
Imports System.Globalization
Imports KBot.Common

''' <summary>
''' English (slice 0017-01): the pinned totals-row COMPUTATION for <see cref="KBotDataView"/>.
''' Aggregates run over ALL rows in the model, not just the visible ones. The formatted text is
''' cached per column and recomputed only when the model changes (AddRow / ClearRows / EndUpdate
''' / a committed edit / a column added / the row toggled on), so paint never re-aggregates and
''' a big grid does not pay per-repaint. Geometry lives in <c>.Layout</c>, painting in
''' <c>.Painting</c>.
''' </summary>
Partial Class KBotDataView

    ''' <summary>
    ''' Recompute the cached aggregate text for every column. Deferred while inside a
    ''' BeginUpdate/EndUpdate batch (guards against a recompute storm — EndUpdate runs it once).
    ''' Boundary: log + swallow — a totals glitch must never blow up a data load or an edit
    ''' commit; the band simply keeps its previous text.
    ''' </summary>
    Private Sub RecomputeTotals()
        Try
            If _updateDepth <> 0 Then Return
            _totalsText.Clear()
            If Not _showTotalsRow Then Return
            For Each col In _columns
                _totalsText(col.Key) = ComputeAggregateText(col)
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.RecomputeTotals", ex)
        End Try
    End Sub

    ' The cached aggregate text for a column, computed lazily if the cache does not hold it yet
    ' (e.g. a column read before the first recompute). Used by the painter and by auto-size.
    Private Function TotalsTextFor(col As KBotDataColumn) As String
        Dim t As String = Nothing
        If _totalsText.TryGetValue(col.Key, t) Then Return t
        Return ComputeAggregateText(col)
    End Function

    ' Compute + format one column's aggregate over ALL rows.
    Private Function ComputeAggregateText(col As KBotDataColumn) As String
        Select Case col.Aggregate
            Case KBotAggregate.Sum
                Dim s As Double = 0
                Dim d As Double
                For Each r In _rows
                    If TryNumeric(r(col.Key), d) Then s += d
                Next
                Return FormatAggregate(col, s, forceInteger:=False)

            Case KBotAggregate.Average
                Dim s As Double = 0
                Dim cnt As Integer = 0
                Dim d As Double
                For Each r In _rows
                    If TryNumeric(r(col.Key), d) Then
                        s += d
                        cnt += 1
                    End If
                Next
                ' No countable cells -> empty, never 0 and never NaN.
                If cnt = 0 Then Return String.Empty
                Return FormatAggregate(col, s / cnt, forceInteger:=False)

            Case KBotAggregate.Count
                ' Count of ROWS that HAVE a stored value for this column (state present), NOT the
                ' count of non-empty numeric cells — a row with a stored Nothing still counts.
                Dim n As Integer = 0
                For Each r In _rows
                    If r.HasValue(col.Key) Then n += 1
                Next
                Return FormatAggregate(col, n, forceInteger:=True)

            Case Else
                Return String.Empty
        End Select
    End Function

    ' Format an aggregate value. Count always renders a plain integer (ignores every format
    ' string). Sum/Average use AggregateFormatString if set, else the column's FormatString,
    ' else a plain ToString — always in CurrentCulture, matching the body's FormatValue.
    Private Shared Function FormatAggregate(col As KBotDataColumn, value As Double, forceInteger As Boolean) As String
        If forceInteger Then
            Return CInt(Math.Round(value)).ToString(CultureInfo.CurrentCulture)
        End If
        Dim fmt As String = If(Not String.IsNullOrEmpty(col.AggregateFormatString),
                               col.AggregateFormatString, col.FormatString)
        If String.IsNullOrEmpty(fmt) Then Return value.ToString(CultureInfo.CurrentCulture)
        Return value.ToString(fmt, CultureInfo.CurrentCulture)
    End Function

    ' Numeric coercion for Sum/Average: real numeric primitives count; a numeric STRING parses;
    ' anything else (Nothing, Boolean, a non-numeric string, an object) is skipped. Returns
    ' whether the value contributed, so "skipped" and "contributed 0" stay distinct.
    Private Shared Function TryNumeric(value As Object, ByRef result As Double) As Boolean
        result = 0
        If value Is Nothing Then Return False
        Select Case True
            Case TypeOf value Is Double, TypeOf value Is Single, TypeOf value Is Decimal,
                 TypeOf value Is Integer, TypeOf value Is Long, TypeOf value Is Short,
                 TypeOf value Is Byte
                result = Convert.ToDouble(value, CultureInfo.InvariantCulture)
                Return True
        End Select
        Dim s As String = TryCast(value, String)
        If s IsNot Nothing Then
            Return Double.TryParse(s.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, result)
        End If
        Return False
    End Function

    ''' <summary>
    ''' Friend test hook: the cached totals text for a column key (headless — no paint needed).
    ''' Empty string when the totals row is off or the column has no aggregate.
    ''' </summary>
    Friend Function DebugTotalsText(colKey As String) As String
        Dim t As String = Nothing
        If _totalsText.TryGetValue(colKey, t) Then Return If(t, String.Empty)
        Return String.Empty
    End Function

End Class
