' Înlocuiește globalele VBA (globANL, globCF, globNumeUnit, globCodProgram, globSectorSursa...).
' Se încarcă o singură dată după login, din API.
Public Class SessionContext
    Public Property OperatorName As String = String.Empty
    Public Property CF As String = String.Empty
    Public Property NumeUnitate As String = String.Empty
    Public Property An As Integer
    Public Property CodProgram As String = String.Empty
    Public Property SectorSursa As String = String.Empty
    Public ReadOnly Property IsLoaded As Boolean
        Get
            Return Not String.IsNullOrEmpty(CF)
        End Get
    End Property
End Class
