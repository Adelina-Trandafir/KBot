''' <summary>What one table's write produced.</summary>
Public NotInheritable Class TableOutcome

    Public Sub New(targetTable As String)
        Me.TargetTable = targetTable
    End Sub

    Public ReadOnly Property TargetTable As String
    ''' <summary>Access rows read, before any filtering.</summary>
    Public Property RowsRead As Long
    ''' <summary>Rows skipped because they belong to another unit. Normal, not an error.</summary>
    Public Property RowsOtherUnit As Long
    ''' <summary>Rows skipped because their parent did not travel.</summary>
    Public Property RowsOrphanParent As Long
    ''' <summary>
    ''' Rows skipped because the document, order or statement file above them stayed behind.
    ''' </summary>
    ''' <remarks>
    ''' Kept apart from <see cref="RowsOtherUnit"/> deliberately. "Its document stayed
    ''' behind" and "it belongs to another unit" are different facts with different
    ''' remedies, and the operator has to be able to read which one happened - counting
    ''' them together is how FX_DDF came to report "altă unitate 14" for fourteen rows
    ''' that were really being dropped on a relic column.
    ''' </remarks>
    Public Property RowsSubtreeSkipped As Long
    ''' <summary>Rows actually sent to the server.</summary>
    Public Property RowsWritten As Long
    ''' <summary>Foreign-key values nulled because the parent was absent and the column allowed it.</summary>
    Public Property ValuesNulled As Long
    ''' <summary>Columns written per row.</summary>
    Public Property ColumnsWritten As Integer

    Public Overrides Function ToString() As String
        Return $"{TargetTable}: {RowsWritten} scrise din {RowsRead} citite"
    End Function

End Class

''' <summary>The outcome of a whole run.</summary>
Public NotInheritable Class TransferResult

    Public ReadOnly Property Tables As New List(Of TableOutcome)()
    Public Property Committed As Boolean
    Public Property Cancelled As Boolean
    Public Property [Error] As Exception
    ''' <summary>The dated journal folder this run wrote.</summary>
    Public Property JournalFolder As String

    Public ReadOnly Property TotalWritten As Long
        Get
            Return Tables.Sum(Function(t) t.RowsWritten)
        End Get
    End Property

    ''' <summary>Lines for the closing journal file and the operator's log.</summary>
    Public Function Totals() As List(Of String)
        Dim lines As New List(Of String)()
        For Each table In Tables
            Dim line = $"{table.TargetTable}: citite {table.RowsRead}, scrise {table.RowsWritten}"
            If table.RowsOtherUnit > 0 Then line &= $", altă unitate {table.RowsOtherUnit}"
            If table.RowsSubtreeSkipped > 0 Then line &= $", document rămas în urmă {table.RowsSubtreeSkipped}"
            If table.RowsOrphanParent > 0 Then line &= $", părinte netransferat {table.RowsOrphanParent}"
            If table.ValuesNulled > 0 Then line &= $", valori golite {table.ValuesNulled}"
            lines.Add(line)
        Next
        lines.Add($"TOTAL scrise: {TotalWritten}")
        Return lines
    End Function

End Class
