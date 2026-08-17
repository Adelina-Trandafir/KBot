Option Strict On
Imports System.IO
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common

''' <summary>
''' Cache-ul local al PDF-urilor SEMNATE (felia 0041), comun vederilor DDF și ORD.
'''
''' Discul nu mai este SURSA documentului semnat — este doar un cache al lui. Sursa e serverul
''' (<c>FX_DDF_PDF</c> / <c>FX_ORD_PDF</c>), iar fișierele stau mai departe unde stăteau, sub
''' rădăcinile din <see cref="KBotPaths"/>, cu numele compuse de <c>DdfPdfLocator</c> /
''' <c>OrdPdfLocator</c> — nimic din convenția de cale nu se schimbă.
'''
''' VALIDARE PRIN SUMĂ, NU ȘTERGERE ÎN BLOC: fișierul local se rehash-uiește și suma pleacă
''' spre server ca <c>If-None-Match</c>. Un 304 înseamnă „ce ai pe disc e exact ce am eu" și
''' nu se transferă niciun octet; abia la nepotrivire se descarcă și se rescrie fișierul.
''' Regula „șterge la fiecare deschidere" trăiește exclusiv în zona nesemnată
''' (<see cref="TempPdfStore"/>) — un PDF semnat nu se aruncă niciodată orbește.
'''
''' Octeții primiți au trecut deja verificarea SHA-256 în <c>ApiClient</c> (comparație cu
''' <c>ETag</c>-ul): dacă ajung aici, se pot scrie pe disc.
''' </summary>
Public NotInheritable Class PdfCache

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Aduce cache-ul local la zi pentru un document SEMNAT și spune dacă fișierul e gata de
    ''' arătat.
    '''
    ''' <paramref name="serverSha"/> gol înseamnă „serverul nu are PDF semnat" — atunci nu se
    ''' face niciun apel și se întoarce <see cref="PdfCacheResult.Nesemnat"/>, iar apelantul
    ''' cade pe regenerarea locală. <paramref name="descarca"/> primește suma fișierului local
    ''' (gol când nu există) și întoarce rezultatul rutei de octeți.
    '''
    ''' Nu aruncă pentru erorile așteptate: o cădere de rețea sau un răspuns de eroare devine
    ''' <see cref="PdfCacheStatus.Eroare"/> cu mesaj românesc, fiindcă apelantul este o vedere
    ''' care trebuie să spună ceva operatorului, nu să se prăbușească.
    ''' </summary>
    Public Shared Async Function EnsureAsync(cachePath As String,
                                              serverSha As String,
                                              descarca As Func(Of String, Task(Of PdfDownloadResult))) _
        As Task(Of PdfCacheResult)
        Try
            If String.IsNullOrWhiteSpace(cachePath) Then Return PdfCacheResult.Nesemnat()
            If String.IsNullOrWhiteSpace(serverSha) Then Return PdfCacheResult.Nesemnat()
            If descarca Is Nothing Then Throw New ArgumentNullException(NameOf(descarca))

            ' Suma fișierului de pe disc, dacă există. Nothing = „n-am cache local", stare
            ' normală: atunci nu se trimite If-None-Match și serverul răspunde cu octeți.
            Dim shaLocal As String = PdfHash.ComputeFile(cachePath)

            ' Scurtătură fără rețea: suma locală e deja cea anunțată de ruta de listă. Nu se mai
            ' cere nimic — asta e cazul obișnuit după prima descărcare.
            If PdfHash.AreEqual(shaLocal, serverSha) Then
                Return PdfCacheResult.Gata(cachePath)
            End If

            Dim rezultat As PdfDownloadResult = Await descarca(If(shaLocal, String.Empty)).ConfigureAwait(True)
            If rezultat Is Nothing Then Return PdfCacheResult.Nesemnat()

            Select Case rezultat.Status
                Case PdfDownloadStatus.NotModified
                    ' Serverul spune că fișierul local e bun (suma din lista de revizii era
                    ' învechită) — se folosește ce e pe disc.
                    Return PdfCacheResult.Gata(cachePath)

                Case PdfDownloadStatus.NotFound
                    ' Cursă: între citirea listei și acum, rândul a dispărut de pe server.
                    ' Nu e o eroare — documentul se regenerează local, ca oricare nesemnat.
                    Return PdfCacheResult.Nesemnat()

                Case Else
                    Scrie(cachePath, rezultat.Bytes)
                    Return PdfCacheResult.Gata(cachePath)
            End Select
        Catch ex As ApiException
            ' Mesajul e deja românesc (câmpul «error» al serverului sau motivul SHA_MISMATCH).
            GlobalErrorLog.Write("PdfCache.EnsureAsync", ex)
            Return PdfCacheResult.Eroare(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("PdfCache.EnsureAsync", ex)
            Return PdfCacheResult.Eroare(
                "Documentul semnat nu a putut fi adus de pe server. Detalii în jurnalul de erori.")
        End Try
    End Function

    ' Scrierea în cache. Se scrie ÎNTÂI într-un fișier temporar alăturat și abia apoi se mută
    ' peste cel vechi: o întrerupere la jumătate ar lăsa altfel în cache un PDF trunchiat, cu
    ' numele unui document semnat valid — exact minciuna pe care felia o previne.
    Private Shared Sub Scrie(cachePath As String, octeti As Byte())
        If octeti Is Nothing OrElse octeti.Length = 0 Then
            Throw New InvalidOperationException("Serverul a răspuns cu conținut gol.")
        End If
        Directory.CreateDirectory(Path.GetDirectoryName(cachePath))
        Dim temp As String = cachePath & ".part"
        File.WriteAllBytes(temp, octeti)
        ' `overwrite:=True` — o revizie re-semnată își înlocuiește propriul fișier.
        File.Move(temp, cachePath, overwrite:=True)
    End Sub

End Class

''' <summary>Ce s-a întâmplat cu cache-ul unui PDF semnat.</summary>
Public Enum PdfCacheStatus
    ''' <summary>Fișierul local e la zi și se poate arăta.</summary>
    Gata = 0
    ''' <summary>Serverul nu are PDF semnat — documentul se regenerează local.</summary>
    Nesemnat = 1
    ''' <summary>N-a mers: <see cref="PdfCacheResult.Mesaj"/> spune de ce, în română.</summary>
    Eroare = 2
End Enum

''' <summary>Rezultatul lui <see cref="PdfCache.EnsureAsync"/>. POCO -&gt; fără Try/Catch.</summary>
Public NotInheritable Class PdfCacheResult
    Public ReadOnly Property Status As PdfCacheStatus
    ''' <summary>Calea fișierului local, populată doar pe <see cref="PdfCacheStatus.Gata"/>.</summary>
    Public ReadOnly Property Cale As String
    ''' <summary>Mesaj românesc pentru operator, populat doar pe <see cref="PdfCacheStatus.Eroare"/>.</summary>
    Public ReadOnly Property Mesaj As String

    Private Sub New(status As PdfCacheStatus, cale As String, mesaj As String)
        Me.Status = status
        Me.Cale = If(cale, String.Empty)
        Me.Mesaj = If(mesaj, String.Empty)
    End Sub

    Public Shared Function Gata(cale As String) As PdfCacheResult
        Return New PdfCacheResult(PdfCacheStatus.Gata, cale, Nothing)
    End Function

    Public Shared Function Nesemnat() As PdfCacheResult
        Return New PdfCacheResult(PdfCacheStatus.Nesemnat, Nothing, Nothing)
    End Function

    Public Shared Function Eroare(mesaj As String) As PdfCacheResult
        Return New PdfCacheResult(PdfCacheStatus.Eroare, Nothing, mesaj)
    End Function
End Class
