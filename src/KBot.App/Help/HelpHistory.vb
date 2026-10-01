Option Strict On

''' <summary>
''' The pages the help window showed during this run of K-BOT (slice 0000-23). Owned by
''' <see cref="HelpService"/>, not by the window, so «Înapoi» / «Înainte» and the «Istoric» list
''' survive closing and reopening the help. Nothing is written to disk.
''' </summary>
Friend NotInheritable Class HelpHistory

    ''' <summary>How many different pages the «Istoric» list keeps.</summary>
    Public Const MaxVisited As Integer = 25

    Public ReadOnly Property Back As New Stack(Of String)()
    Public ReadOnly Property Forward As New Stack(Of String)()

    ''' <summary>The page on screen (or last on screen), as the help window names pages.</summary>
    Public Property Current As String = String.Empty

    Private ReadOnly _visited As New List(Of String)()

    ''' <summary>Every different page seen this run, the most recent first.</summary>
    Public ReadOnly Property Visited As IReadOnlyList(Of String)
        Get
            Return _visited
        End Get
    End Property

    ''' <summary>Puts <paramref name="page"/> at the top of <see cref="Visited"/>.</summary>
    Public Sub NoteVisit(page As String)
        If String.IsNullOrEmpty(page) Then Return
        _visited.Remove(page)
        _visited.Insert(0, page)
        If _visited.Count > MaxVisited Then _visited.RemoveAt(_visited.Count - 1)
    End Sub

End Class
