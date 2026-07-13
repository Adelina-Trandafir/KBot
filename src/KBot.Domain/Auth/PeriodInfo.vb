' Un rand din catalogul an / SS / CodProgram al unei baze de date.
' Intors de /api/auth/periods; alimenteaza combo-urile An si SS de pe MainForm.
' SS este 1:1 cu CodProgram (CodProgram calatoreste odata cu SS-ul ales).
Public NotInheritable Class PeriodInfo
    Public Property AN As Integer
    Public Property SS As String
    Public Property CodProgram As String
End Class
