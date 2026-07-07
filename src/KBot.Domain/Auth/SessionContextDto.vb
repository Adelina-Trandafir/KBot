' Contextul de sesiune asa cum vine pe fir de la /api/auth/login (SessionContext).
' Este mapat in KBot.Common.SessionContext.Populate (ANL -> An).
Public NotInheritable Class SessionContextDto
    Public Property DbName As String
    Public Property IdUnitate As Integer
    Public Property ANL As Integer
    Public Property CodProgram As String
    Public Property SectorSursa As String
    Public Property CF As String
    Public Property NumeUnitate As String
    Public Property Role As String          ' Contabil / Administrator (store-only)
End Class
