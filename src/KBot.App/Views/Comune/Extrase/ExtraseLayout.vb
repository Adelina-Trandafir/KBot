Option Strict On
Imports System.Linq
Imports KBot.Controls

''' <summary>
''' Applies an operator's column choice (slice 0080-02) to a grid whose columns are ALL authored
''' in the designer: the chosen keys become visible, in the chosen order; every other column
''' stays in the grid, hidden, after them. Nothing is created or dropped, so the formats, sums
''' and filter icons set in the designer stay exactly as authored.
''' </summary>
Friend NotInheritable Class ExtraseLayout

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Reorders and shows / hides. The grid's rows are cleared first (the caller refills them):
    ''' the collection is rebuilt, and a rebuild under live rows is a second source of truth
    ''' nobody needs. A key the grid does not have is an error -- the catalogue and the
    ''' designer are one list, and a drift between them must be seen.
    ''' </summary>
    Public Shared Sub Apply(grid As KBotDataView, keys As IList(Of String))
        ArgumentNullException.ThrowIfNull(grid)
        ArgumentNullException.ThrowIfNull(keys)
        Dim all As List(Of KBotDataColumn) = grid.Columns.ToList()
        Dim byKey As Dictionary(Of String, KBotDataColumn) =
            all.ToDictionary(Function(c) c.Key, StringComparer.Ordinal)
        For Each k As String In keys
            If Not byKey.ContainsKey(k) Then
                Throw New ArgumentException($"Coloana «{k}» nu există în grila «{grid.Name}».", NameOf(keys))
            End If
        Next

        Dim chosen As New HashSet(Of String)(keys, StringComparer.Ordinal)
        Dim ordered As New List(Of KBotDataColumn)()
        ordered.AddRange(keys.Select(Function(k) byKey(k)))
        ordered.AddRange(all.Where(Function(c) Not chosen.Contains(c.Key)))

        grid.BeginUpdate()
        Try
            grid.ClearRows()
            grid.Columns.Clear()
            For Each c As KBotDataColumn In ordered
                c.Visible = If(chosen.Contains(c.Key), KBotColumnVisibility.Visible, KBotColumnVisibility.Hidden)
                grid.Columns.Add(c)
            Next
        Finally
            grid.EndUpdate()
        End Try
    End Sub

End Class
