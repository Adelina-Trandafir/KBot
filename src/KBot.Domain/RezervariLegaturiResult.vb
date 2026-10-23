' POCO for POST /api/forexe/rezervari/repara-legaturi (slice 0114-26): the reservations of one angajament that
' are linked to a revision which does not cover them, and what freeing them did. Same shape for the dry run
' (Applied = False, Repaired = 0) and the real run.
' Pure model (no I/O) -> no Try/Catch (house rule: simple POCOs).

''' <summary>Result of freeing the wrongly linked reservations of one angajament.</summary>
Public NotInheritable Class RezervariLegaturiResult
    Public Property Cod As String = String.Empty

    ''' <summary>False = dry run: nothing was written.</summary>
    Public Property Applied As Boolean

    ''' <summary>How many reservations the repair frees.</summary>
    Public Property ToRepair As Integer

    ''' <summary>How many it freed (0 on a dry run).</summary>
    Public Property Repaired As Integer

    ''' <summary>The pictures of those reservations taken off the wrong revision (0 on a dry run).</summary>
    Public Property PicturesRemoved As Integer

    ''' <summary>The reservations (the server sends the first 60).</summary>
    Public Property Details As New List(Of RezervareLegataGresit)()

    Public Property Warnings As New List(Of String)()
End Class
