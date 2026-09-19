Option Strict On
Imports System
Imports System.IO
Imports System.Text

''' <summary>
''' One append-only text log for the updater: <c>&lt;target&gt;\Logs\updater.log</c>, the
''' folder the update never overwrites, so the story of every update sits next to the
''' app's other journals. When even that cannot be written (the very case the updater
''' exists for may be a permissions problem) it falls back to <c>%TEMP%\KBot\updater.log</c>.
''' </summary>
Friend NotInheritable Class UpdaterLog

    Private ReadOnly _gate As New Object()
    Private ReadOnly _path As String

    Public ReadOnly Property Path As String
        Get
            Return _path
        End Get
    End Property

    Public Sub New(targetDir As String)
        _path = ChoosePath(targetDir)
    End Sub

    Private Shared Function ChoosePath(targetDir As String) As String
        Dim preferred As String = IO.Path.Combine(targetDir, "Logs", "updater.log")
        Try
            Directory.CreateDirectory(IO.Path.GetDirectoryName(preferred))
            Using New FileStream(preferred, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)
            End Using
            Return preferred
        Catch ex As Exception
            Dim fallback As String = IO.Path.Combine(IO.Path.GetTempPath(), "KBot", "updater.log")
            Directory.CreateDirectory(IO.Path.GetDirectoryName(fallback))
            Return fallback
        End Try
    End Function

    Public Sub Write(text As String)
        Dim line As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") & "  " & text & Environment.NewLine
        SyncLock _gate
            Try
                File.AppendAllText(_path, line, Encoding.UTF8)
            Catch ex As Exception
                ' The log is the last resort; failing to write it must not stop the update.
                Diagnostics.Trace.WriteLine("updater.log write failed: " & ex.Message)
            End Try
        End SyncLock
    End Sub

    Public Sub Write(text As String, ex As Exception)
        Write(text & Environment.NewLine & ex.ToString())
    End Sub
End Class
