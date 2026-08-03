Option Strict On
Imports System.Collections.Generic

' Session snapshot of registry values, with once-per-session capture and exact restore
' (slice 0023, plan §3.E/§6).
' Capture is idempotent per (path,name): the FIRST capture of a value wins and a later capture
' is ignored, so a second "Apply" cannot overwrite the operator's true original with a value the
' harness itself just wrote. RestoreAll deletes what was absent and rewrites what was present,
' preserving the original kind (including a wrong-type original).
Public NotInheritable Class RegistrySnapshotSet

    Private ReadOnly _reg As IRegistryAccess
    ' Keyed case-insensitively by path|name; preserves nothing about order (order is irrelevant
    ' to restore correctness). Values are the immutable snapshots.
    Private ReadOnly _snaps As New Dictionary(Of String, RegistryValueSnapshot)(StringComparer.OrdinalIgnoreCase)

    Public Sub New(reg As IRegistryAccess)
        If reg Is Nothing Then Throw New ArgumentNullException(NameOf(reg))
        _reg = reg
    End Sub

    Private Shared Function KeyOf(path As String, name As String) As String
        Return path & "|" & name
    End Function

    Public Function IsCaptured(path As String, name As String) As Boolean
        Return _snaps.ContainsKey(KeyOf(path, name))
    End Function

    Public ReadOnly Property Count As Integer
        Get
            Return _snaps.Count
        End Get
    End Property

    ' Captures the current state of a value, ONCE. A repeat capture of the same value is ignored.
    Public Sub Capture(path As String, name As String)
        Dim k As String = KeyOf(path, name)
        If _snaps.ContainsKey(k) Then Return
        _snaps(k) = _reg.Read(path, name)
    End Sub

    ' The captured snapshots (for logging / status).
    Public Function Snapshots() As IReadOnlyList(Of RegistryValueSnapshot)
        Return New List(Of RegistryValueSnapshot)(_snaps.Values)
    End Function

    ' Restores every captured value to exactly what it was: absent -> delete, present -> write
    ' back with the original kind and value.
    Public Sub RestoreAll()
        For Each s As RegistryValueSnapshot In _snaps.Values
            If s.Presence = RegPresence.Absent Then
                _reg.DeleteValue(s.Path, s.Name)
            Else
                _reg.Write(s.Path, s.Name, s.Kind, s.Value)
            End If
        Next
    End Sub

End Class
