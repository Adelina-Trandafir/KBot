Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.Json

''' <summary>Where the history of one scope (an operator on a unit) is kept.</summary>
Public Interface IMachetaHistoryStore
    ''' <summary>The saved entries, newest first; an empty list when nothing was saved.</summary>
    Function Load(k_scope As String) As List(Of MachetaEntry)
    Sub Save(k_scope As String, k_entries As IReadOnlyList(Of MachetaEntry))
End Interface

''' <summary>
''' The history kept only in memory: it starts empty on every run of the application and is gone when it closes.
''' (The JSON store above is kept for a history that must survive a restart.)
''' </summary>
Public NotInheritable Class MemoryMachetaHistoryStore
    Implements IMachetaHistoryStore

    Public Function Load(k_scope As String) As List(Of MachetaEntry) Implements IMachetaHistoryStore.Load
        Return New List(Of MachetaEntry)()
    End Function

    Public Sub Save(k_scope As String, k_entries As IReadOnlyList(Of MachetaEntry)) Implements IMachetaHistoryStore.Save
    End Sub
End Class

''' <summary>
''' One JSON file per scope in <c>%APPDATA%\AVACONT\KBot\Machete\</c>. A small file, rewritten
''' whole on every change (ten lines): nothing to corrupt halfway worth the trouble.
''' </summary>
Public NotInheritable Class JsonMachetaHistoryStore
    Implements IMachetaHistoryStore

    Private Shared ReadOnly JsonOptions As New JsonSerializerOptions With {.WriteIndented = True, .PropertyNameCaseInsensitive = True}
    Private ReadOnly _dir As String

    Public Sub New(Optional k_dir As String = Nothing)
        _dir = If(String.IsNullOrEmpty(k_dir), Path.Combine(SetariFoldere.DirectorSetari(), "Machete"), k_dir)
    End Sub

    Private Function FilePathFor(k_scope As String) As String
        Dim k_name As New StringBuilder()
        For Each k_char As Char In k_scope.ToLowerInvariant()
            k_name.Append(If(Array.IndexOf(Path.GetInvalidFileNameChars(), k_char) >= 0 OrElse k_char = "@"c, "_"c, k_char))
        Next
        Return Path.Combine(_dir, k_name.ToString() & ".json")
    End Function

    Public Function Load(k_scope As String) As List(Of MachetaEntry) Implements IMachetaHistoryStore.Load
        Try
            Dim k_path As String = FilePathFor(k_scope)
            If Not File.Exists(k_path) Then Return New List(Of MachetaEntry)()
            Dim k_loaded As List(Of MachetaEntry) = JsonSerializer.Deserialize(Of List(Of MachetaEntry))(File.ReadAllText(k_path), JsonOptions)
            Return If(k_loaded, New List(Of MachetaEntry)())
        Catch ex As Exception
            GlobalErrorLog.Write("JsonMachetaHistoryStore.Load", ex)
            Throw
        End Try
    End Function

    Public Sub Save(k_scope As String, k_entries As IReadOnlyList(Of MachetaEntry)) Implements IMachetaHistoryStore.Save
        Try
            Directory.CreateDirectory(_dir)
            File.WriteAllText(FilePathFor(k_scope), JsonSerializer.Serialize(k_entries, JsonOptions), New UTF8Encoding(False))
        Catch ex As Exception
            GlobalErrorLog.Write("JsonMachetaHistoryStore.Save", ex)
            Throw
        End Try
    End Sub
End Class

''' <summary>
''' The machete of the operator as a STACK, kept per scope (operator + unit, given by <c>k_scopeProvider</c> and read on every
''' call, so a re-login or a change of unit moves to another list by itself). Slice 0113-01:
''' <list type="bullet">
''' <item>the OPEN machete come first, in the order they were opened; an open one never moves while it stays open (asking for it
''' again changes nothing); a new one takes the place after the last open one;</item>
''' <item>a macheta that closes goes to the END of the list (<see cref="MarkClosed"/>) and is written in italics;</item>
''' <item>the list holds at most <c>k_max</c> lines (ten): when it is full, the closed machete leave, the one closed longest ago
''' first, so the open ones stay visible as long as possible (more than <c>k_max</c> open ones are all kept);</item>
''' <item>a macheta opened again after it closed leaves the end and takes its place among the open ones.</item>
''' </list>
''' An empty scope (nobody is signed in) records nothing.
''' </summary>
Public NotInheritable Class MachetaHistory

    Private ReadOnly _store As IMachetaHistoryStore
    Private ReadOnly _scopeProvider As Func(Of String)
    Private ReadOnly _max As Integer
    Private ReadOnly _gate As New Object()
    Private _scope As String = String.Empty
    Private _entries As New List(Of MachetaEntry)()

    Public Sub New(k_store As IMachetaHistoryStore, k_scopeProvider As Func(Of String), Optional k_max As Integer = 10)
        ArgumentNullException.ThrowIfNull(k_store)
        ArgumentNullException.ThrowIfNull(k_scopeProvider)
        _store = k_store
        _scopeProvider = k_scopeProvider
        _max = Math.Max(1, k_max)
    End Sub

    ''' <summary>Raised after the list changed; may come from any thread.</summary>
    Public Event Changed As EventHandler

    ' Moves to the list of the current scope (loading it) when the scope changed. Caller holds the gate.
    Private Sub EnsureScope()
        Dim k_scope As String = If(_scopeProvider(), String.Empty)
        If String.Equals(k_scope, _scope, StringComparison.Ordinal) Then Return
        _scope = k_scope
        _entries = New List(Of MachetaEntry)()
        If k_scope.Length = 0 Then Return
        Try
            _entries = _store.Load(k_scope)
            For Each k_loaded As MachetaEntry In _entries
                k_loaded.IsOpen = False   ' nothing is open when the application starts
            Next
        Catch ex As Exception
            ' Already logged by the store; a history that cannot be read starts empty rather than blocking the shell.
            _entries = New List(Of MachetaEntry)()
        End Try
    End Sub

    ''' <summary>The entries of the current scope: the open ones first, in opening order, then the closed ones, the last closed at the end.</summary>
    Public Function Recent() As IReadOnlyList(Of MachetaEntry)
        SyncLock _gate
            EnsureScope()
            Return _entries.ToList()
        End SyncLock
    End Function

    ''' <summary>
    ''' Puts a reference in the list and saves (a repeat is moved, not doubled). <paramref name="k_open"/> = it is open now: it takes
    ''' the place after the last open one -- unless it is open already, when it keeps its place. Not open (a view of the shell, a
    ''' shortcut): it goes to the end, closed, as if it had just closed; an entry that is open is left alone.
    ''' </summary>
    Public Sub Record(k_ref As MachetaRef, k_title As String, Optional k_open As Boolean = False)
        ArgumentNullException.ThrowIfNull(k_ref)
        Try
            SyncLock _gate
                EnsureScope()
                If _scope.Length = 0 Then Return
                Dim k_text As String = If(k_title, k_ref.TypeKey)
                Dim k_at As Integer = _entries.FindIndex(Function(k_e) String.Equals(k_e.Id, k_ref.Id, StringComparison.Ordinal))
                If k_at >= 0 AndAlso _entries(k_at).IsOpen Then
                    _entries(k_at).Title = k_text   ' open already: its place stays
                Else
                    If k_at >= 0 Then _entries.RemoveAt(k_at)
                    Dim k_entry As New MachetaEntry With {
                        .Id = k_ref.Id, .TypeKey = k_ref.TypeKey, .Title = k_text, .IsOpen = k_open,
                        .Args = k_ref.Args.ToDictionary(Function(k_p) k_p.Key, Function(k_p) k_p.Value), .Moment = Date.Now}
                    If k_open Then
                        _entries.Insert(_entries.Where(Function(k_e) k_e.IsOpen).Count(), k_entry)
                    Else
                        _entries.Add(k_entry)
                    End If
                    TrimClosed()
                End If
                _store.Save(_scope, _entries)
            End SyncLock
            RaiseEvent Changed(Me, EventArgs.Empty)
        Catch ex As Exception
            ' Recording is a convenience: a failure (already logged by the store) never stops the macheta opening.
            Return
        End Try
    End Sub

    ''' <summary>An open macheta closed: it leaves its place and goes to the end of the list, closed. Nothing happens for one that is not open.</summary>
    Public Sub MarkClosed(k_id As String)
        Try
            SyncLock _gate
                EnsureScope()
                If _scope.Length = 0 Then Return
                Dim k_at As Integer = _entries.FindIndex(Function(k_e) k_e.IsOpen AndAlso String.Equals(k_e.Id, k_id, StringComparison.Ordinal))
                If k_at < 0 Then Return
                Dim k_entry As MachetaEntry = _entries(k_at)
                _entries.RemoveAt(k_at)
                k_entry.IsOpen = False
                k_entry.Moment = Date.Now
                _entries.Add(k_entry)
                _store.Save(_scope, _entries)
            End SyncLock
            RaiseEvent Changed(Me, EventArgs.Empty)
        Catch ex As Exception
            ' Same as Record: a convenience that never stops the macheta from closing.
            Return
        End Try
    End Sub

    ' Over the limit, the closed machete leave -- the one closed longest ago first (the first closed line). Open ones always stay.
    ' Caller holds the gate.
    Private Sub TrimClosed()
        Do While _entries.Count > _max
            Dim k_first As Integer = _entries.FindIndex(Function(k_e) Not k_e.IsOpen)
            If k_first < 0 Then Exit Do
            _entries.RemoveAt(k_first)
        Loop
    End Sub
End Class
