Option Strict On

''' <summary>
''' Cele TREI rezultate posibile ale unei descărcări de PDF semnat (felia 0041). Un tip
''' explicit, nu un <c>Byte()</c> care poate fi <c>Nothing</c> din două motive diferite:
''' „serverul n-are documentul" și „cache-ul meu e deja bun" cer acțiuni opuse pe client, iar
''' un <c>Nothing</c> le-ar amesteca.
''' </summary>
Public Enum PdfDownloadStatus
    ''' <summary>Serverul are documentul, iar octeții sunt în <see cref="PdfDownloadResult.Bytes"/>.</summary>
    Content = 0
    ''' <summary>Cache-ul local are deja exact acest conținut (304) — se folosește fișierul de pe disc.</summary>
    NotModified = 1
    ''' <summary>Nu există PDF semnat pentru documentul cerut (404) — se cade pe regenerare.</summary>
    NotFound = 2
End Enum

''' <summary>
''' Rezultatul lui <c>DownloadDdfPdfAsync</c> / <c>DownloadOrdPdfAsync</c>. POCO -&gt; fără
''' Try/Catch. <see cref="Bytes"/> și <see cref="Sha256"/> sunt populate DOAR pe
''' <see cref="PdfDownloadStatus.Content"/>, iar octeții au trecut deja verificarea de sumă
''' din <c>ApiClient</c> — dacă ajung aici, se potrivesc cu <c>ETag</c>-ul serverului.
''' </summary>
Public NotInheritable Class PdfDownloadResult

    Public ReadOnly Property Status As PdfDownloadStatus
    ''' <summary>Octeții PDF-ului, VERBATIM. Nothing pe NotModified / NotFound.</summary>
    Public ReadOnly Property Bytes As Byte()
    ''' <summary>Suma de control verificată a octeților de mai sus. Gol pe NotModified / NotFound.</summary>
    Public ReadOnly Property Sha256 As String

    Private Sub New(status As PdfDownloadStatus, bytes As Byte(), sha As String)
        Me.Status = status
        Me.Bytes = bytes
        Me.Sha256 = If(sha, String.Empty)
    End Sub

    Public Shared Function FromContent(bytes As Byte(), sha As String) As PdfDownloadResult
        Return New PdfDownloadResult(PdfDownloadStatus.Content, bytes, sha)
    End Function

    Public Shared Function NotModified() As PdfDownloadResult
        Return New PdfDownloadResult(PdfDownloadStatus.NotModified, Nothing, Nothing)
    End Function

    Public Shared Function NotFound() As PdfDownloadResult
        Return New PdfDownloadResult(PdfDownloadStatus.NotFound, Nothing, Nothing)
    End Function

End Class

''' <summary>
''' Răspunsul serverului la o încărcare reușită (<c>PUT …/pdf/…</c>). Numele proprietăților
''' SUNT cheile JSON (PropertyNamingPolicy=Nothing). POCO -&gt; fără Try/Catch.
''' </summary>
Public NotInheritable Class PutPdfResponse
    Public Property sha256 As String
    ''' <summary>Numele derivat pe SERVER — sursa unică; clientul nu îl propune niciodată.</summary>
    Public Property nume_fisier As String
    Public Property dimensiune As Integer
End Class
