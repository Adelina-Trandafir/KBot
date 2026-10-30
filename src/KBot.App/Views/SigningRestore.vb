Option Strict On
Imports System.IO
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Controls

''' <summary>
''' After a signature K-BOT refused (<see cref="PdfSigningStatus.Refused"/>, slice DIR1-03) the document has to go back to what
''' the server holds, without the operator doing anything: the view lets the document go (Adobe closes it), this deletes the
''' local copies that carry the refused signature, and the view loads the document again -- which downloads the original.
''' </summary>
Friend NotInheritable Class SigningRestore

    Private Sub New()
    End Sub

    ' Adobe frees the file a moment after the document is let go.
    Private Const DeleteAttempts As Integer = 20
    Private Const DeleteStepMs As Integer = 250

    ''' <summary>
    ''' Deletes the file the session watched and its copy in the persistent cache (when it is another file). A file Adobe still
    ''' holds is tried again for a few seconds; one that cannot be deleted is logged and left (the next open compares it with
    ''' the server's and replaces it). Never throws.
    ''' </summary>
    Friend Shared Async Function DeleteLocalCopiesAsync(k_session As PdfSigningSession) As Task
        Try
            If k_session Is Nothing Then Return
            Dim k_paths As New List(Of String)()
            If Not String.IsNullOrWhiteSpace(k_session.DisplayedPath) Then k_paths.Add(k_session.DisplayedPath)
            If Not String.IsNullOrWhiteSpace(k_session.CachePath) AndAlso
               Not String.Equals(k_session.CachePath, k_session.DisplayedPath, StringComparison.OrdinalIgnoreCase) Then
                k_paths.Add(k_session.CachePath)
            End If
            For Each k_path As String In k_paths
                Await DeleteWhenFreeAsync(k_path).ConfigureAwait(True)
            Next
        Catch ex As Exception
            ' UI boundary (async, called without await by the views): log and swallow.
            GlobalErrorLog.Write("SigningRestore.DeleteLocalCopiesAsync", ex)
        End Try
    End Function

    Private Shared Async Function DeleteWhenFreeAsync(k_path As String) As Task
        Dim k_last As Exception = Nothing
        For k_attempt As Integer = 1 To DeleteAttempts
            Try
                If Not File.Exists(k_path) Then
                    AdobeHostLog.Write($"Semnătură refuzată: fișierul local șters, se descarcă originalul: {k_path}.")
                    Return
                End If
                File.Delete(k_path)
            Catch ex As IOException
                k_last = ex   ' still held by Adobe: next step
            Catch ex As UnauthorizedAccessException
                k_last = ex
            End Try
            Await Task.Delay(DeleteStepMs).ConfigureAwait(True)
        Next
        If k_last IsNot Nothing Then GlobalErrorLog.Write("SigningRestore.DeleteWhenFreeAsync", k_last)
        AdobeHostLog.Write($"Semnătură refuzată: fișierul local n-a putut fi șters (încă folosit): {k_path}.")
    End Function
End Class
