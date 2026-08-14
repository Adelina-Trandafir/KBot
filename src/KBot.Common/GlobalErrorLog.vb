Imports System.Diagnostics
Imports System.IO
Imports System.Text

' Sink-ul global de erori al soluției: scrie detaliul COMPLET al unei excepții
' (tip + mesaj + stack + inner-e, via ex.ToString()) în <AppDir>\Logs\harness_errors.log.
' Folosit de plasele globale din Program.vb și de HandleUiError din DevHarnessForm.
' Sink TERMINAL: dacă nici fișierul de erori nu se poate scrie, suprafațăm pe Trace
' și NU rearuncăm — rearuncarea ar masca eroarea originală.
Public Module GlobalErrorLog
    Private ReadOnly _gate As New Object()

    ''' <summary>Numele fișierului, lângă executabil, sub <c>Logs\</c>.</summary>
    Public Const FileNameOnly As String = "harness_errors.log"

    ''' <summary>Scrie detaliul COMPLET al unei erori în &lt;AppDir&gt;\Logs\harness_errors.log.</summary>
    Public Sub Write(source As String, ex As Exception)
        Try
            ' LogPaths: aceeași cale ca înainte (<AppDir>\Logs), calculată acum într-un singur loc.
            LogPaths.EnsureLogsDirectory()
            Dim filePath As String = LogPaths.Combine(FileNameOnly)
            Dim sb As New StringBuilder()
            sb.AppendLine("==== " & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") & "  [" & source & "] ====")
            sb.AppendLine(If(ex IsNot Nothing, ex.ToString(), "<null exception>"))
            sb.AppendLine()
            SyncLock _gate
                ' Rotația se cheamă ÎNAINTE de adăugare și nu aruncă niciodată: dacă eșuează,
                ' linia se scrie oricum în fișierul vechi. Vezi LogRotation.
                LogRotation.Roll(filePath)
                File.AppendAllText(filePath, sb.ToString(), New UTF8Encoding(True))
            End SyncLock
        Catch terminalEx As Exception
            ' SINK TERMINAL: nu mai există fișier unde să scriem (disc plin / fără drepturi).
            ' NU rearuncăm — ar masca eroarea ORIGINALĂ. Suprafațăm pe trace listener.
            Trace.WriteLine("GlobalErrorLog terminal failure (" & source & "): " & terminalEx.Message)
        End Try
    End Sub
End Module
