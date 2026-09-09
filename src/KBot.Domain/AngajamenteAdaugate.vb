' The result of POST /api/forexe/angajamente/upsert with `doar_noi = true` (slice 0057):
' the right-hand icon in the tree footer, which brings the current FOREXE list and leaves
' alone everything the server already has.
'
' Both figures come from the SERVER; they are not worked out here. Only the server knows
' what is in FX_Angajamente at the moment of the write, while the client sees just the
' tree of the chosen period (one year + one SS) -- an angajament belonging to another
' period would look new to it. The number the operator reads must be the database's.
Public Class AngajamenteAdaugate
    ''' <summary>How many rows the route received (empty-Cod ones included; it skips those).</summary>
    Public Property Primite As Integer

    ''' <summary>How many rows carried a non-empty Cod, so were candidates for the write.</summary>
    Public Property Candidate As Integer

    ''' <summary>How many NEW angajamente were inserted.</summary>
    Public Property Inserate As Integer

    ''' <summary>How many of the sent codes were already there and stayed untouched.</summary>
    Public Property Existente As Integer
End Class
