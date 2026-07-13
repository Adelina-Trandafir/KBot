' Identitatea de sesiune asa cum vine pe fir de la /api/auth/login (SessionContext).
' Doar identitate: in sistemul nou anul, SS-ul si CodProgram nu mai sunt fapte de
' login — se aleg la runtime pe MainForm (din catalogul /api/auth/periods).
' Mapat in KBot.Common.SessionContext.Populate.
Public NotInheritable Class SessionContextDto
    Public Property DbName As String
    Public Property CF As String
    Public Property NumeUnitate As String
    Public Property Role As String          ' Contabil / Administrator (store-only)
End Class
