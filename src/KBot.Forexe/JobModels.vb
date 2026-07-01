Imports System.Collections.Generic

Namespace KBot.Forexe
    Public Class JobRequest
        Public Property WorkflowName As String = String.Empty
        Public Property WflPath As String = String.Empty
        Public Property Parameters As New Dictionary(Of String, String)
    End Class

    Public Class JobResult
        Public Property Success As Boolean
        Public Property Message As String = String.Empty
        ' Variabilele plate ale executorului la finalul job-ului (nume -> valoare).
        ' Consumatorii existenți (dicționar plat) rămân neatinși.
        Public Property Data As New Dictionary(Of String, String)
        ' Îmbogățire aditivă: rezultatele tabelare (ex. ScrapeTable) sparte pe
        ' variabilă -> listă de rânduri (coloană -> valoare). Populat de RunJobAsync
        ' pentru orice variabilă care conține un JSON array de obiecte.
        Public Property Tables As New Dictionary(Of String, List(Of Dictionary(Of String, String)))
    End Class
End Namespace
