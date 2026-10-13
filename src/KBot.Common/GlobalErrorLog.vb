Imports System.Diagnostics
Imports System.IO
Imports System.Text

' Sink-ul global de erori al soluției: scrie detaliul COMPLET al unei excepții
' (tip + mesaj + stack + inner-e, via ex.ToString()) în <AppDir>\Logs\harness_errors.log.
' Folosit de plasele globale din Program.vb și de HandleUiError din DevHarnessForm.
' Sink TERMINAL: dacă nici fișierul de erori nu se poate scrie, suprafațăm pe Trace
' și NU rearuncăm — rearuncarea ar masca eroarea originală.

''' <summary>One error written during THIS run of the program (the side panel of the opening page shows them).</summary>
Public NotInheritable Class SessionErrorEntry
    Public Sub New(k_moment As Date, k_source As String, k_message As String, k_detail As String)
        Moment = k_moment
        Source = k_source
        Message = k_message
        Detail = k_detail
    End Sub

    Public ReadOnly Property Moment As Date
    Public ReadOnly Property Source As String
    ''' <summary>The exception's own message (first line).</summary>
    Public ReadOnly Property Message As String
    ''' <summary>The full text written to the log file.</summary>
    Public ReadOnly Property Detail As String
End Class

Public Module GlobalErrorLog
    Private ReadOnly _gate As New Object()

    Private Const SessionErrorsKept As Integer = 100
    Private ReadOnly _recent As New List(Of SessionErrorEntry)()

    ''' <summary>Raised after an error was written; may come from any thread.</summary>
    Public Event Written As EventHandler(Of SessionErrorEntry)

    ''' <summary>The errors of this run, newest first (at most <paramref name="k_count"/>).</summary>
    Public Function RecentErrors(k_count As Integer) As IReadOnlyList(Of SessionErrorEntry)
        SyncLock _gate
            Dim k_list As New List(Of SessionErrorEntry)(_recent)
            k_list.Reverse()
            If k_count > 0 AndAlso k_list.Count > k_count Then k_list.RemoveRange(k_count, k_list.Count - k_count)
            Return k_list
        End SyncLock
    End Function

    Private Sub Remember(k_source As String, k_ex As Exception)
        Try
            Dim k_entry As New SessionErrorEntry(DateTime.Now, k_source, If(k_ex Is Nothing, String.Empty, k_ex.Message), If(k_ex IsNot Nothing, k_ex.ToString(), "<null exception>"))
            SyncLock _gate
                _recent.Add(k_entry)
                If _recent.Count > SessionErrorsKept Then _recent.RemoveAt(0)
            End SyncLock
            RaiseEvent Written(Nothing, k_entry)
        Catch k_inner As Exception
            ' Terminal sink: a failing listener must never mask the error being written.
            Trace.WriteLine("GlobalErrorLog.Remember failure: " & k_inner.Message)
        End Try
    End Sub

    ''' <summary>Numele fișierului, lângă executabil, sub <c>Logs\</c>.</summary>
    Public Const FileNameOnly As String = "harness_errors.log"

    ''' <summary>Scrie detaliul COMPLET al unei erori în &lt;AppDir&gt;\Logs\harness_errors.log.</summary>
    Public Sub Write(source As String, ex As Exception)
        Remember(source, ex)
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
