Option Strict On
Imports System.IO
Imports System.Threading.Tasks
Imports KBot.Common

''' <summary>
''' File I/O shared by the signing flow (slice 0078): reading a PDF that Adobe still holds open, and
''' writing the persistent cache the safe way (<c>.part</c> + move).
''' </summary>
Public NotInheritable Class SignedPdfFiles

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Reads a file that another process (Adobe) may hold open. FileShare.ReadWrite|Delete is what
    ''' lets us read while Adobe keeps its handle.
    ''' </summary>
    Public Shared Function ReadShared(path As String) As Byte()
        Try
            Using fs As New FileStream(path, FileMode.Open, FileAccess.Read,
                                       FileShare.ReadWrite Or FileShare.Delete)
                Dim buffer(CInt(fs.Length) - 1) As Byte
                Dim read As Integer = 0
                While read < buffer.Length
                    Dim n As Integer = fs.Read(buffer, read, buffer.Length - read)
                    If n <= 0 Then Exit While
                    read += n
                End While
                If read <> buffer.Length Then Throw New IOException($"Short read on {path}: {read}/{buffer.Length}.")
                Return buffer
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("SignedPdfFiles.ReadShared", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Waits until Adobe has finished writing: the same length twice, <paramref name="stepMs"/>
    ''' apart, and the file readable. Returns the bytes, or Nothing when it never settled within
    ''' <paramref name="timeoutMs"/>.
    ''' </summary>
    Public Shared Async Function ReadWhenSettledAsync(path As String, Optional timeoutMs As Integer = 10000,
                                                      Optional stepMs As Integer = 300) As Task(Of Byte())
        Try
            Dim limit As DateTime = DateTime.UtcNow.AddMilliseconds(timeoutMs)
            Dim lastLength As Long = -1
            While DateTime.UtcNow < limit
                Await Task.Delay(stepMs).ConfigureAwait(True)
                If Not File.Exists(path) Then Continue While
                Dim length As Long = New FileInfo(path).Length
                If length > 0 AndAlso length = lastLength Then
                    Try
                        Return ReadShared(path)
                    Catch ex As IOException
                        ' Still locked for reading (Adobe mid-write): try again on the next step.
                        GlobalErrorLog.Write("SignedPdfFiles.ReadWhenSettledAsync.Retry", ex)
                    End Try
                End If
                lastLength = length
            End While
            Return Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("SignedPdfFiles.ReadWhenSettledAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Writes a signed PDF into the persistent cache: <c>.part</c> first, then a move over the old
    ''' file, so an interruption never leaves a truncated file under a valid document name.
    ''' </summary>
    Public Shared Sub WriteCache(cachePath As String, bytes As Byte())
        Try
            If String.IsNullOrWhiteSpace(cachePath) Then Throw New ArgumentException("Empty cache path.", NameOf(cachePath))
            If bytes Is Nothing OrElse bytes.Length = 0 Then Throw New ArgumentException("Empty PDF.", NameOf(bytes))
            Directory.CreateDirectory(Path.GetDirectoryName(cachePath))
            Dim temp As String = cachePath & ".part"
            File.WriteAllBytes(temp, bytes)
            File.Move(temp, cachePath, overwrite:=True)
        Catch ex As Exception
            GlobalErrorLog.Write("SignedPdfFiles.WriteCache", ex)
            Throw
        End Try
    End Sub

End Class
