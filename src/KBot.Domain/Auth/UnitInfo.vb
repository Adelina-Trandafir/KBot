' Unitate accesibila operatorului, asa cum o intoarce /api/auth/units.
' In sistemul nou o baza de date se identifica prin NUME (DC), nu prin IdUnitate.
' Picker-ul de login arata NumeUnitate (Display); valoarea din spate e DC.
Public NotInheritable Class UnitInfo
    Public Property DC As String
    Public Property NumeUnitate As String

    ' Ce afiseaza combo-ul (niciodata DC-ul brut).
    Public ReadOnly Property Display As String
        Get
            Return NumeUnitate
        End Get
    End Property
End Class
