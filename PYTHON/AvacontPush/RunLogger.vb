Imports System.IO
Imports System.Text

' UTF-8, AutoFlush log for one operation (scan or push).
' If the log file cannot be created, the run is aborted (constructor throws).
Public NotInheritable Class RunLogger
    Implements IDisposable

    Private ReadOnly _writer As StreamWriter
    Private _disposed As Boolean

    Public Sub New()
        Dim logDir = Path.Combine(AppContext.BaseDirectory, "Logs")
        Try
            Directory.CreateDirectory(logDir)
            Dim fileName = $"push_{DateTime.Now:yyyyMMdd_HHmmss}.log"
            Dim fullPath = Path.Combine(logDir, fileName)
            _writer = New StreamWriter(fullPath, append:=False, encoding:=New UTF8Encoding(True)) With {.AutoFlush = True}
        Catch ex As IOException
            Throw New ApplicationException("Nu s-a putut crea fișierul jurnal în folderul Logs.", ex)
        Catch ex As UnauthorizedAccessException
            Throw New ApplicationException("Acces refuzat la crearea fișierului jurnal în folderul Logs.", ex)
        End Try
    End Sub

    Public Sub Write(line As String)
        _writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  {line}")
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        If _disposed Then Return
        _disposed = True
        _writer.Flush()
        _writer.Dispose()
    End Sub

End Class
