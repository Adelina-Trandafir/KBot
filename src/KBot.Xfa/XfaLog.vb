Option Strict On
Imports System.IO

' Jurnal de trasare al operațiilor XFA (INFO/DEBUG/WARN/ERROR) în C:\AVACONT\logs,
' un fișier per rulare. Portat din Logger-ul XFA_WRITTER, dar adaptat pentru librărie:
' NU mai alocă consolă și NU scrie în Console (rula ca exe CLI acolo).
'
' Este un SINK TERMINAL (ca GlobalErrorLog): nu aruncă niciodată — dacă scrierea eșuează,
' suprafațează pe Debug și continuă. Sink-ul de ERORI al soluției rămâne GlobalErrorLog;
' acesta e doar trasare operațională detaliată, moștenită din proiectul original.
Public Module XfaLog
    Private _logPath As String = Nothing
    Private ReadOnly _lock As New Object()

    ''' <summary>Deschide un fișier nou de log pentru tipul de document dat.</summary>
    Public Sub Init(tipDocument As String)
        Dim dir As String = "C:\AVACONT\logs"
        Try
            Directory.CreateDirectory(dir)
            Dim ts As String = DateTime.Now.ToString("yyyyMMdd_HHmmss")
            Dim tip As String = If(String.IsNullOrWhiteSpace(tipDocument), "UNKNOWN", tipDocument.ToUpper())
            _logPath = Path.Combine(dir, $"xfa_{tip}_{ts}.txt")
        Catch ex As Exception
            ' Nu putem crea directorul țintă — cădem pe temp, fără a masca cauza.
            System.Diagnostics.Debug.WriteLine(ex.ToString())
            _logPath = Path.Combine(Path.GetTempPath(), "xfa_fallback.txt")
        End Try

        Log("INFO", $"=== XFA_WRITTER start — tip: {tipDocument} ===")
    End Sub

    ''' <summary>Scrie o linie de log. Nu aruncă niciodată (sink terminal).</summary>
    Public Sub Log(level As String, msg As String)
        Dim line As String = $"[{DateTime.Now:HH:mm:ss.fff}] [{level,-5}] {msg}"
        Try
            SyncLock _lock
                Dim filePath As String = If(_logPath, Path.Combine(Path.GetTempPath(), "xfa_fallback.txt"))
                Using sw As New StreamWriter(filePath, append:=True)
                    sw.WriteLine(line)
                    sw.Flush()
                End Using
            End SyncLock
        Catch ex As Exception
            ' SINK TERMINAL: nu mai avem unde scrie. NU aruncăm — suprafațăm pe Debug.
            System.Diagnostics.Debug.WriteLine("XfaLog terminal failure: " & ex.Message & " | " & line)
        End Try
    End Sub

    ''' <summary>Scrie un antet de secțiune.</summary>
    Public Sub LogSection(title As String)
        Log("INFO", $"====== {title} ======")
    End Sub
End Module
