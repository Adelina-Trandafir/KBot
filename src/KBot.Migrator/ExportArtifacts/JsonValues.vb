Imports System.Globalization
Imports System.Text.Json

''' <summary>
''' Citiri mici din valorile unui rând. Exportatorul VBA scrie NULL ca <c>null</c> (niciodată
''' ca șir gol) și numerele neîncadrate în ghilimele, cu punct zecimal — deci aici nu se
''' ghicește nimic despre locale.
'''
''' Metode pure, fără I/O: intră la „foarte sigure" din regulile casei, deci fără Try/Catch.
''' </summary>
Public Module JsonValues

    ''' <summary>True dacă valoarea lipsește sau e <c>null</c>.</summary>
    Public Function IsNull(v As JsonElement) As Boolean
        Return v.ValueKind = JsonValueKind.Undefined OrElse v.ValueKind = JsonValueKind.Null
    End Function

    ''' <summary>
    ''' Valoarea ca text, sau Nothing dacă e NULL. Un șir GOL rămâne șir gol: exportatorul
    ''' păstrează diferența dintre „" și NULL, iar rutarea o folosește (un DC gol trimite pe
    ''' ramura de retragere, un DC NULL la fel — dar niciuna nu e „lipsă de rând").
    ''' </summary>
    Public Function AsText(v As JsonElement) As String
        If IsNull(v) Then Return Nothing
        If v.ValueKind = JsonValueKind.String Then Return v.GetString()
        Return v.ToString()
    End Function

    ''' <summary>
    ''' Valoarea ca întreg. Întoarce False dacă e NULL sau nu e un întreg — apelantul
    ''' decide dacă asta e un orfan sau o eroare.
    ''' </summary>
    Public Function TryAsLong(v As JsonElement, ByRef value As Long) As Boolean
        value = 0
        If IsNull(v) Then Return False
        If v.ValueKind = JsonValueKind.Number Then Return v.TryGetInt64(value)
        If v.ValueKind = JsonValueKind.String Then
            Return Long.TryParse(v.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, value)
        End If
        Return False
    End Function

End Module
