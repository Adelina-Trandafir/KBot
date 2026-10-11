Option Strict On
Imports System.IO
Imports System.Net.Http
Imports KBot.Common

' Descarcă/cache-uiește macheta XFA (DDF/ORD) de pe rutele bearer ale API-ului (vezi Configs).
' Portat din XFA_WRITTER. Cache pe disc în CACHE_DIR; HEAD întâi (evită re-download
' dacă fișierul e deja în cache), apoi GET complet.
<System.Runtime.Versioning.SupportedOSPlatform("windows")>
Public Class TemplateDownloader

    ''' <summary>Returns the live bearer token of the signed-in session; set once by the application at start-up.</summary>
    Public Shared Property TokenProvider As Func(Of String)

    ''' <summary>
    ''' Called when the server answers 401 (session closed or expired): the application re-signs the
    ''' operator in and returns True when the session has a fresh token, False when it has not. Without it a 401 is
    ''' reported as it is. Runs on the calling (worker) thread, so the implementation must marshal to the UI itself.
    ''' </summary>
    Public Shared Property ReauthenticateHook As Func(Of Boolean)

    ' A 401 from the template route, told apart from every other failure so one re-login and one retry can follow.
    Private NotInheritable Class TemplateUnauthorizedException
        Inherits Exception

        Public Sub New(k_message As String, k_inner As Exception)
            MyBase.New(k_message, k_inner)
        End Sub
    End Class

    ''' <summary>
    ''' Întoarce calea locală a machetei pentru tipul de document (o descarcă dacă lipsește).
    ''' Metodă de graniță (I/O + HTTP): loghează în GlobalErrorLog și rearuncă.
    ''' </summary>
    Public Shared Function GetTemplatePath(docType As String) As String
        Try
            Try
                Return GetTemplatePathOnce(docType)
            Catch ex As TemplateUnauthorizedException
                If ReauthenticateHook Is Nothing OrElse Not ReauthenticateHook.Invoke() Then
                    Throw New Exception("Sesiunea a expirat sau a fost închisă. Autentificați-vă din nou.", ex)
                End If
            End Try
            ' One retry with the fresh token; a second 401 is reported, not looped on.
            Try
                Return GetTemplatePathOnce(docType)
            Catch ex As TemplateUnauthorizedException
                Throw New Exception("Serverul a refuzat sesiunea imediat după autentificare. Contactați administratorul.", ex)
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("TemplateDownloader.GetTemplatePath", ex)
            Throw
        End Try
    End Function

    Private Shared Function GetTemplatePathOnce(docType As String) As String
        Try
            Dim templateUrl As String = GetUrlForDocType(docType)

            If Not Directory.Exists(CACHE_DIR) Then
                Directory.CreateDirectory(CACHE_DIR)
            End If

            Using client As New HttpClient()
                client.Timeout = TimeSpan.FromSeconds(60)
                Dim k_token As String = If(TokenProvider Is Nothing, String.Empty, TokenProvider.Invoke())
                If String.IsNullOrEmpty(k_token) Then
                    Throw New InvalidOperationException("Nu exista o sesiune autentificata pentru descarcarea machetei.")
                End If
                client.DefaultRequestHeaders.Authorization = New System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", k_token)

                Dim headRequest As New HttpRequestMessage(HttpMethod.Head, BASE_URL & templateUrl)
                Dim headResponse As HttpResponseMessage

                Try
                    headResponse = client.SendAsync(headRequest).Result
                    headResponse.EnsureSuccessStatusCode()
                Catch ex As Exception
                    XfaLog.Log("WARN", $"GetTemplatePath — HEAD a eșuat, trec pe download complet: {ex.Message}")
                    Return DownloadFull(client, templateUrl, docType)
                End Try

                Dim remoteFileName As String = GetFileNameFromResponse(headResponse)

                If String.IsNullOrEmpty(remoteFileName) Then
                    Return DownloadFull(client, templateUrl, docType)
                End If

                Dim localPath As String = Path.Combine(CACHE_DIR, remoteFileName)
                If File.Exists(localPath) Then
                    XfaLog.Log("INFO", $"GetTemplatePath — machetă din cache: {localPath}")
                    Return localPath
                End If

                Return DownloadFull(client, templateUrl, docType)
            End Using
        Catch ex As TemplateUnauthorizedException
            Throw   ' control flow (re-login + one retry in GetTemplatePath): not logged here
        Catch ex As Exception
            GlobalErrorLog.Write("TemplateDownloader.GetTemplatePath", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Mapează docType → URL din Configs. Adaugă aici orice tip nou de document.
    ''' </summary>
    Private Shared Function GetUrlForDocType(docType As String) As String
        Select Case If(docType, "").ToUpper()
            Case "DDF" : Return DDF_URL
            Case "ORD" : Return ORD_URL
            Case Else
                Throw New Exception($"Tip document necunoscut: «{docType}». Tipuri acceptate: DDF, ORD.")
        End Select
    End Function

    Private Shared Function DownloadFull(client As HttpClient, templateUrl As String, docType As String) As String
        Dim response As HttpResponseMessage

        Try
            response = client.GetAsync(BASE_URL & templateUrl).Result
            response.EnsureSuccessStatusCode()
        Catch ex As HttpRequestException When ex.StatusCode = System.Net.HttpStatusCode.Unauthorized
            Throw New TemplateUnauthorizedException($"Macheta {docType}: serverul a răspuns 401.", ex)
        Catch ex As Exception
            Throw New Exception($"Nu s-a putut descărca macheta {docType} de pe server: {ex.Message}", ex)
        End Try

        Dim remoteFileName As String = GetFileNameFromResponse(response)
        If String.IsNullOrEmpty(remoteFileName) Then
            remoteFileName = $"{docType}_template_{DateTime.Now:yyyyMMdd}.pdf"
        End If

        Dim localPath As String = Path.Combine(CACHE_DIR, remoteFileName)
        Dim pdfBytes As Byte() = response.Content.ReadAsByteArrayAsync().Result
        File.WriteAllBytes(localPath, pdfBytes)

        XfaLog.Log("INFO", $"DownloadFull — machetă descărcată: {localPath} ({pdfBytes.Length} octeți)")
        Return localPath
    End Function

    Private Shared Function GetFileNameFromResponse(response As HttpResponseMessage) As String
        Dim cd = response.Content?.Headers?.ContentDisposition
        If cd IsNot Nothing AndAlso Not String.IsNullOrEmpty(cd.FileName) Then
            Return cd.FileName.Trim(""""c)
        End If
        Return Nothing
    End Function

End Class
