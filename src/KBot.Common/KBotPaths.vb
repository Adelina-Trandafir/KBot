Option Strict On
Imports System.IO
Imports System.Text.Json

''' <summary>
''' Locațiile din sistemul de fișiere LOCAL folosite de K-BOT (felia 0020-04). Momentan o
''' singură proprietate — <see cref="DdfPdfRoot"/> — cu valoarea implicită
''' «C:\AVACONT\FOREXE\PDF\DDF\». Backing store: un fișier JSON lângă executabil
''' (<c>&lt;AppDir&gt;\kbot_paths.json</c>). Un fișier lipsă sau stricat CADE pe implicit și
''' loghează; NU aruncă la pornire.
'''
''' ATENȚIE: aceasta NU este o reînviere a lui <c>AppConfig</c>. `AppConfig` a fost retras
''' fiindcă ținea ADRESA SERVERULUI, iar adresa serverului rămâne hardcodată în
''' <c>ApiOptions</c>. `KBotPaths` ține DOAR locații din sistemul de fișiere local. Formularul
''' de configurare care va edita aceste valori este o felie ulterioară, în afara acestui domeniu.
''' </summary>
Public NotInheritable Class KBotPaths

    ''' <summary>Valoarea implicită a rădăcinii PDF-urilor DDF (planul, decizia 13).</summary>
    Public Const DefaultDdfPdfRoot As String = "C:\AVACONT\FOREXE\PDF\DDF\"

    ''' <summary>Numele fișierului de configurare, lângă executabil.</summary>
    Public Const FileName As String = "kbot_paths.json"

    ''' <summary>Rădăcina în care se caută PDF-urile DDF (recursiv). Nu e niciodată gol.</summary>
    Public Property DdfPdfRoot As String = DefaultDdfPdfRoot

    Private Shared ReadOnly _gate As New Object()
    Private Shared _current As KBotPaths

    ''' <summary>
    ''' Instanța curentă, încărcată o singură dată din <c>&lt;AppDir&gt;\kbot_paths.json</c>.
    ''' Thread-safe; cade pe implicit dacă fișierul lipsește sau e stricat.
    ''' </summary>
    Public Shared ReadOnly Property Current As KBotPaths
        Get
            If _current Is Nothing Then
                SyncLock _gate
                    If _current Is Nothing Then _current = Load()
                End SyncLock
            End If
            Return _current
        End Get
    End Property

    ''' <summary>
    ''' Încarcă din directorul dat (implicit <see cref="AppContext.BaseDirectory"/>). Fișier
    ''' lipsă/gol/stricat -&gt; valori implicite (+ log pe stricat). NU aruncă.
    ''' </summary>
    Public Shared Function Load(Optional dir As String = Nothing) As KBotPaths
        Dim baseDir As String = If(String.IsNullOrEmpty(dir), AppContext.BaseDirectory, dir)
        Dim result As New KBotPaths()
        Dim filePath As String = Path.Combine(baseDir, FileName)

        Try
            If Not File.Exists(filePath) Then Return result   ' lipsă -> implicit, fără log

            Dim json As String = File.ReadAllText(filePath)
            If String.IsNullOrWhiteSpace(json) Then Return result   ' gol -> implicit

            Dim dto As KBotPathsDto = JsonSerializer.Deserialize(Of KBotPathsDto)(json)
            If dto IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(dto.DdfPdfRoot) Then
                result.DdfPdfRoot = dto.DdfPdfRoot.Trim()
            End If
            Return result
        Catch ex As Exception
            ' Stricat (JSON nevalid / drepturi) -> implicit + log; niciodată o excepție la pornire.
            GlobalErrorLog.Write("KBotPaths.Load", ex)
            Return New KBotPaths()
        End Try
    End Function

End Class

''' <summary>DTO de fir pentru JSON. Numele proprietății E cheia JSON. POCO -> fără Try/Catch.</summary>
Friend NotInheritable Class KBotPathsDto
    Public Property DdfPdfRoot As String
End Class
