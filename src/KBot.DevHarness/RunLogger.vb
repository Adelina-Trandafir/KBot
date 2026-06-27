Imports System
Imports System.IO
Imports System.Text

' Fișierul de rezultate per rulare: <AppDir>\Logs\test_{yyyyMMdd_HHmmss_fff}.log.
' Deschidere STRICTĂ (dacă nu se poate scrie, constructorul aruncă → RunTestsAsync prinde
' și NU pornește rularea, fiindcă rezultatele TREBUIE să ajungă în fișier).
' StreamWriter UTF-8 cu AutoFlush → rezultatele parțiale supraviețuiesc unui crash.
Public NotInheritable Class RunLogger
    Implements IDisposable

    Private ReadOnly _writer As StreamWriter
    Public ReadOnly Property FilePath As String

    Public Sub New(filePath As String)
        Me.FilePath = filePath
        Dim dir As String = Path.GetDirectoryName(filePath)
        If Not String.IsNullOrEmpty(dir) Then Directory.CreateDirectory(dir)
        _writer = New StreamWriter(filePath, append:=False,
                                   encoding:=New UTF8Encoding(encoderShouldEmitUTF8Identifier:=True)) With {.AutoFlush = True}
    End Sub

    Public Sub WriteLine(text As String)
        _writer.WriteLine(text)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Try
            _writer.Flush()
        Finally
            _writer.Dispose()
        End Try
    End Sub
End Class
