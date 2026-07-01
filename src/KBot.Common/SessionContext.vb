' Înlocuiește globalele VBA (globANL, globCF, globNumeUnit, globCodProgram, globSectorSursa...).
' Se încarcă o singură dată după login, din API.
Public Class SessionContext
    Public Property OperatorName As String = String.Empty
    Public Property CF As String = String.Empty
    Public Property NumeUnitate As String = String.Empty
    Public Property An As Integer
    Public Property CodProgram As String = String.Empty
    Public Property SectorSursa As String = String.Empty
    ' Numele bazei de date a unității (ex. "018_GRRS"). Devine țintă a upsert-ului
    ' (db_name) și valoare DC în FX_Angajamente. Populat la login (felia login);
    ' până atunci se setează manual pentru rularea de probă.
    Public Property DbName As String = String.Empty
    Public ReadOnly Property IsLoaded As Boolean
        Get
            Return Not String.IsNullOrEmpty(CF)
        End Get
    End Property
End Class
