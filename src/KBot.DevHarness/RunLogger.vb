Imports System
Imports System.IO
Imports System.Text
Imports KBot.Common

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
        ' Garda de mărime, ca la ceilalți scriitori. ATENȚIE la ce face DE FAPT: apelantul de azi
        ' (DevHarnessForm.RunTestsAsync) compune un nume UNIC pe rulare, cu milisecunde, și
        ' deschide cu append:=False — deci fișierul nu există încă și rotația nu are ce roti.
        ' Rămâne pusă fiindcă RunLogger primește o cale ARBITRARĂ: un apelant care dă o cale fixă
        ' e protejat. Pentru o rulare scăpată de sub control, garda utilă e cea din LogRotation
        ' apelată la scriere, nu aici — vezi worklogul feliei 0031-01.
        LogRotation.Roll(filePath)
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
