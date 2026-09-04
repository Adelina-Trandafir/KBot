' One unit database offered to the operator, as reported by
' "schema_sync --list-targets".
Public NotInheritable Class SchemaTarget

    Public Property Name As String = ""

    ' True for a database that is really there. The list is read from the
    ' server, so this is False only for a name AVACONT_COMUN.CAI claims and the
    ' server does not have. Such a row is shown but cannot be ticked: it is a
    ' broken registry entry, worth seeing and impossible to sync.
    Public Property Exists As Boolean = True

    ' Whether CAI lists it. False means the database is on the server but the
    ' registry does not know about it - also shown rather than hidden.
    Public Property InCai As Boolean

    ' The CheckedListBox renders items through ToString.
    Public Overrides Function ToString() As String
        If Not Exists Then Return Name & "   (în CAI, nu există pe server)"
        If Not InCai Then Return Name & "   (lipsă din CAI)"
        Return Name
    End Function

End Class
