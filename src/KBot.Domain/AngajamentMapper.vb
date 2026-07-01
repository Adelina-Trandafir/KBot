Option Strict On

' Mapper: rezultatul tabelar ListaAngajamente -> List(Of Angajament).
' Layer pur (fără JSON / fără dependențe): primește rândurile deja parsate
' (coloană -> valoare), pe care le produce ForexeRunner.RunJobAsync din
' JobResult.Tables("ListaAngajamente"). Sare peste rândurile cu Cod gol
' (mirror VBA IsEmpty(dRow("Cod"))).
'
' ATENȚIE — cheile per rând (KeyCod/KeyDescriere/KeyStare) NU sunt încă verificate.
' ScrapeTableExtract.js derivă cheile din textul antetelor <thead> ale tabelului
' "Listă angajamente", normalizat: diacritice șterse, non-alfanumeric -> "_"
' (ex. antet "Cod angajament" -> cheie "Cod_angajament", nu "Cod"). Valorile de mai
' jos sunt o presupunere; de confirmat dintr-un scrape live (JobResult.Tables are
' cheile reale) și de corectat DOAR aici.
Public NotInheritable Class AngajamentMapper

    ' Chei de coloană — de confirmat față de antetele reale (vezi nota de clasă).
    Public Const KeyCod As String = "Cod"
    Public Const KeyDescriere As String = "Descriere"
    Public Const KeyStare As String = "Stare"

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Mapează rândurile ListaAngajamente în List(Of Angajament).
    ''' Rândurile cu "Cod" gol/absent sunt ignorate.
    ''' </summary>
    Public Shared Function FromListaAngajamenteResult(rows As IEnumerable(Of IDictionary(Of String, String))) As List(Of Angajament)
        If rows Is Nothing Then Throw New ArgumentNullException(NameOf(rows))

        Dim result As New List(Of Angajament)()
        For Each r In rows
            If r Is Nothing Then Continue For

            Dim cod As String = Nothing
            If Not r.TryGetValue(KeyCod, cod) OrElse String.IsNullOrWhiteSpace(cod) Then Continue For

            Dim descriere As String = Nothing
            r.TryGetValue(KeyDescriere, descriere)

            Dim stare As String = Nothing
            r.TryGetValue(KeyStare, stare)

            result.Add(New Angajament() With {
                .CodAngajament = cod.Trim(),
                .Descriere = descriere,
                .Stare = stare
            })
        Next
        Return result
    End Function

End Class
