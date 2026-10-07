Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports System.Threading
Imports KBot.Common

''' <summary>
''' Slice 0078-05, signing bench: what happens ON DISK around a save, with times.
'''
''' In the hosted Adobe window the «Save As» comes FIRST and the signed file is written only
''' afterwards (after the token's PIN), so «the trap closed the dialog» is not «the signature is in
''' the file». This timeline shows the order: every file event in the document's folder (Adobe's
''' temporary files included), every change of the document's size / write time, and -- once the
''' file has stopped changing -- its signatures. Times are counted from the moment the trap reported
''' the dialog closed (<see cref="MarkDialogClosed"/>), so a line reads «+2 350 ms after the dialog».
'''
''' Created and used on the UI thread; the FileSystemWatcher is posted back to it.
''' </summary>
Friend NotInheritable Class PdfSaveTimeline
    Implements IDisposable

    Private Const PollMs As Integer = 250
    ' Two equal polls in a row = the file has stopped changing.
    Private Const StablePolls As Integer = 2

    Private ReadOnly _path As String
    Private ReadOnly _docType As String
    Private ReadOnly _log As Action(Of String)
    Private ReadOnly _ui As SynchronizationContext
    Private ReadOnly _timer As New System.Windows.Forms.Timer() With {.Interval = PollMs}
    Private _watcher As FileSystemWatcher
    Private _length As Long = -1
    Private _written As DateTime
    Private _changedPending As Boolean
    Private _stable As Integer
    Private _dialogClosed As DateTime?
    Private _disposed As Boolean

    Public Sub New(path As String, docType As String, log As Action(Of String))
        _path = path
        _docType = docType
        _log = log
        _ui = If(SynchronizationContext.Current, New SynchronizationContext())
        AddHandler _timer.Tick, AddressOf OnTick
    End Sub

    ''' <summary>Starts watching the folder and polling the file. Risky (file system): logs and rethrows.</summary>
    Public Sub Start()
        Try
            ReadState(_length, _written)
            Dim dir As String = Path.GetDirectoryName(_path)
            _watcher = New FileSystemWatcher(dir) With {
                .NotifyFilter = NotifyFilters.FileName Or NotifyFilters.LastWrite Or NotifyFilters.Size}
            AddHandler _watcher.Changed, AddressOf OnFsEvent
            AddHandler _watcher.Created, AddressOf OnFsEvent
            AddHandler _watcher.Deleted, AddressOf OnFsEvent
            AddHandler _watcher.Renamed, AddressOf OnFsRenamed
            _watcher.EnableRaisingEvents = True
            _timer.Start()
            Write($"Cronologie pornită: {Path.GetFileName(_path)}, {_length:N0} octeți, scris la {_written:HH:mm:ss.fff}. " &
                  $"Urmăresc tot folderul «{dir}».")
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSaveTimeline.Start", ex)
            Throw
        End Try
    End Sub

    ''' <summary>The trap reported the «Save As» closed: the zero of every later time.</summary>
    Public Sub MarkDialogClosed()
        _dialogClosed = DateTime.Now
        Dim length As Long
        Dim written As DateTime
        ReadState(length, written)
        Write($"«Salvare ca» ÎNCHIS. Fișierul acum: {length:N0} octeți, scris la {written:HH:mm:ss.fff}" &
              If(length = _length AndAlso written = _written, " — NESCHIMBAT: Adobe nu l-a scris încă.", "."))
    End Sub

    ''' <summary>«+1 234 ms după dialog», or "" before any dialog closed. For the bench's other lines.</summary>
    Public Function SinceDialog() As String
        If Not _dialogClosed.HasValue Then Return ""
        Return $" (+{(DateTime.Now - _dialogClosed.Value).TotalMilliseconds:N0} ms după închiderea dialogului)"
    End Function

    ' FileSystemWatcher thread -> UI thread. Never throws.
    Private Sub OnFsEvent(sender As Object, e As FileSystemEventArgs)
        Try
            If _disposed Then Return
            Dim line As String = $"Disc: {e.ChangeType} «{e.Name}»"
            _ui.Post(Sub(state) Write(line & SinceDialog()), Nothing)
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSaveTimeline.OnFsEvent", ex)
        End Try
    End Sub

    Private Sub OnFsRenamed(sender As Object, e As RenamedEventArgs)
        Try
            If _disposed Then Return
            Dim line As String = $"Disc: Renamed «{e.OldName}» -> «{e.Name}»"
            _ui.Post(Sub(state) Write(line & SinceDialog()), Nothing)
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSaveTimeline.OnFsRenamed", ex)
        End Try
    End Sub

    ' Timer: log and swallow.
    Private Sub OnTick(sender As Object, e As EventArgs)
        Try
            If _disposed Then Return
            Dim length As Long
            Dim written As DateTime
            ReadState(length, written)
            If length <> _length OrElse written <> _written Then
                Write($"Fișier: {_length:N0} -> {length:N0} octeți, scris la {written:HH:mm:ss.fff}" & SinceDialog())
                _length = length
                _written = written
                _changedPending = True
                _stable = 0
                Return
            End If
            If Not _changedPending Then Return
            _stable += 1
            If _stable < StablePolls Then Return
            _changedPending = False
            ReportSignatures()
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSaveTimeline.OnTick", ex)
        End Try
    End Sub

    ' The file stopped changing: what is in it now. Reached from OnTick (wrapped).
    Private Sub ReportSignatures()
        Dim bytes As Byte()
        Try
            bytes = SignedPdfFiles.ReadShared(_path)
        Catch ex As IOException
            GlobalErrorLog.Write("PdfSaveTimeline.ReportSignatures", ex)
            Write("Fișierul s-a oprit din schimbat, dar nu poate fi citit încă: " & ex.Message)
            _changedPending = True
            Return
        End Try
        Write("Fișierul s-a oprit din schimbat" & SinceDialog() & ":")
        For Each line As String In PdfSignatureReport.Lines("  Pe disc", bytes, _docType)
            Write(line)
        Next
    End Sub

    Private Sub ReadState(ByRef length As Long, ByRef written As DateTime)
        Dim fi As New FileInfo(_path)
        fi.Refresh()
        If fi.Exists Then
            length = fi.Length
            written = fi.LastWriteTime
        Else
            length = 0
            written = DateTime.MinValue
        End If
    End Sub

    Private Sub Write(line As String)
        If _disposed Then Return
        _log?.Invoke("[Disc] " & line)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Try
            _disposed = True
            _timer.Stop()
            _timer.Dispose()
            If _watcher IsNot Nothing Then
                _watcher.EnableRaisingEvents = False
                _watcher.Dispose()
                _watcher = Nothing
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSaveTimeline.Dispose", ex)
        End Try
    End Sub

End Class
