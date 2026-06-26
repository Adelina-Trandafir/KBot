' Progres raportat de un test, per-test (distinct de bara de progres pe lot a formei).
Public NotInheritable Class HarnessProgressInfo
    Public ReadOnly Property Percent As Integer?
    Public ReadOnly Property Message As String

    Public Sub New(percent As Integer?, message As String)
        Me.Percent = percent
        Me.Message = message
    End Sub
End Class
