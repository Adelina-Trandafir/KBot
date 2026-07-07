' Unitate accesibila operatorului (subsetul din CAI folosit de picker-ul de login).
' Intoarsa de /api/auth/units.
Public NotInheritable Class UnitInfo
    Public Property IdUnitate As Integer
    Public Property DbName As String
    Public Property NumeUnitate As String
    Public Property AlteDetalii As String
    Public Property Sursa As String
    Public Property AnDate As Integer?
    Public Property DC As String

    ' "AlteDetalii - NumeUnitate", curatat de separatorii cu o parte goala.
    Public ReadOnly Property Display As String
        Get
            Dim left = If(AlteDetalii, String.Empty).Trim()
            Dim right = If(NumeUnitate, String.Empty).Trim()
            If left.Length = 0 Then Return right
            If right.Length = 0 Then Return left
            Return $"{left} - {right}"
        End Get
    End Property
End Class
