Imports Xunit
Imports KBot.Domain

Public Class AngajamentTests
    <Fact>
    Public Sub Defaults_StringuriGoale()
        Dim a As New Angajament()
        Assert.Equal(String.Empty, a.CodAngajament)
        Assert.Equal(0, a.Id)
    End Sub
End Class
