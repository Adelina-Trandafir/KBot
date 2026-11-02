Option Strict On
Imports System.Globalization
Imports System.IO
Imports System.Text

''' <summary>
''' The log of the walk through linked tables on the page «Tabele» (ME01-08): <c>&lt;AppDir&gt;\Logs\tabele_navigare.log</c>. It exists so the
''' operator can read what the page did and check it against what they expected: every click on a link cell and the links found, the
''' choice in the popup, every value asked for a composite key (typed text, server call or memory), every view opened (from memory or from
''' the server, the arguments sent, the rows that came), the back / forward lists offered and the one taken, and after each step the whole
''' history tree with the current view marked. It is OFF until the operator ticks «Jurnal navigare» in the ribbon group of the page «Tabele»
''' (<see cref="SetEnabled"/>). Nothing here changes what the page does; a failure to write is sent to the error log and never reaches the page. A log over 2 MB is moved to <c>tabele_navigare.old.log</c> when the program starts writing.
''' </summary>
Public Module TableNavLog

    Private Const MaxBytes As Long = 2L * 1024L * 1024L
    Private ReadOnly _lock As New Object()
    Private _started As Boolean

    Private _enabled As Boolean

    Public ReadOnly Property LogPath As String
        Get
            Return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "tabele_navigare.log")
        End Get
    End Property

    ''' <summary>
    ''' Is the log on? OFF at every start: the operator ticks «Jurnal navigare» in the ribbon group of the page «Tabele» when they want it. While it is off
    ''' <see cref="Write"/> does nothing and no file is touched.
    ''' </summary>
    Public ReadOnly Property Enabled As Boolean
        Get
            Return _enabled
        End Get
    End Property

    ''' <summary>Turns the log on or off; the switch itself is written (it is the first and the last line of what was logged).</summary>
    Public Sub SetEnabled(k_on As Boolean)
        If k_on = _enabled Then Return
        If k_on Then
            _enabled = True
            Write("LOG", "switched ON")
        Else
            Write("LOG", "switched OFF")
            _enabled = False
        End If
    End Sub

    ''' <summary>One line: the time to the millisecond, the area in brackets, the text. Does nothing while the log is off. Never throws: a diagnostic must not break what it observes.</summary>
    Public Sub Write(k_area As String, k_text As String)
        If Not _enabled Then Return
        Try
            SyncLock _lock
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath))
                If Not _started Then
                    _started = True
                    Dim k_info As New FileInfo(LogPath)
                    If k_info.Exists AndAlso k_info.Length > MaxBytes Then
                        Dim k_old As String = Path.Combine(Path.GetDirectoryName(LogPath), "tabele_navigare.old.log")
                        File.Move(LogPath, k_old, True)
                    End If
                    File.AppendAllText(LogPath, Environment.NewLine & "===== KBotAdmin pornit " & Stamp() & Environment.NewLine, Encoding.UTF8)
                End If
                File.AppendAllText(LogPath, Stamp() & "  [" & k_area.PadRight(8) & "] " & k_text & Environment.NewLine, Encoding.UTF8)
            End SyncLock
        Catch ex As Exception
            GlobalErrorLog.Write("TableNavLog.Write", ex)
        End Try
    End Sub

    Private Function Stamp() As String
        Return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)
    End Function

End Module
