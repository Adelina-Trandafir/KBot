Option Strict On
Imports System.IO

''' <summary>
''' A plain trace of what the Publicare page did (slice ME01), one line per step, appended to
''' <c>&lt;AppDir&gt;\Logs\publicare.log</c>. It exists so that a push that "does nothing" can be followed step by
''' step. A trace must never break the work it traces, so a failed write is dropped.
''' </summary>
Public Module RunLog

    Private ReadOnly Gate As New Object()

    Public Sub Write(k_text As String)
        Try
            SyncLock Gate
                Dim k_dir As String = Path.Combine(AppContext.BaseDirectory, "Logs")
                Directory.CreateDirectory(k_dir)
                File.AppendAllText(Path.Combine(k_dir, "publicare.log"),
                                   DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", Globalization.CultureInfo.InvariantCulture) & "  " & k_text & Environment.NewLine)
            End SyncLock
        Catch ex As IOException
            ' Dropped on purpose: see the summary.
        Catch ex As UnauthorizedAccessException
            ' Dropped on purpose: see the summary.
        End Try
    End Sub

End Module
