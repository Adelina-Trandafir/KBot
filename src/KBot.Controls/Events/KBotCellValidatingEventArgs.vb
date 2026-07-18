Option Strict On

''' <summary>
''' Ridicat ÎNAINTE de a scrie în rând valoarea propusă de editor. Handler-ul poate:
''' <list type="bullet">
''' <item>să o RESPINGĂ (<c>Cancel = True</c>) — editorul rămâne deschis și focalizat;</item>
''' <item>să o CORECTEZE, rescriind <see cref="ProposedValue"/> (normalizare, conversie de tip,
''' trim) — valoarea scrisă în rând e cea de după handler.</item>
''' </list>
''' </summary>
Public NotInheritable Class KBotCellValidatingEventArgs
    Inherits EventArgs

    ''' <summary>Cheia coloanei editate.</summary>
    Public ReadOnly Property ColumnKey As String

    ''' <summary>Indexul rândului editat.</summary>
    Public ReadOnly Property RowIndex As Integer

    ''' <summary>Valoarea propusă de editor. Poate fi rescrisă de handler (coerciție).</summary>
    Public Property ProposedValue As Object

    ''' <summary>True => commit-ul se abandonează, editorul rămâne deschis.</summary>
    Public Property Cancel As Boolean

    Public Sub New(columnKey As String, rowIndex As Integer, proposedValue As Object)
        _ColumnKey = columnKey
        _RowIndex = rowIndex
        ' „Me.” e OBLIGATORIU: VB e case-insensitive, deci parametrul „proposedValue” ascunde
        ' proprietatea „ProposedValue”, iar o atribuire nekalificată ar fi un no-op.
        Me.ProposedValue = proposedValue
    End Sub

End Class
