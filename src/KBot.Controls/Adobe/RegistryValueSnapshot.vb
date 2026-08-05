Option Strict On
Imports Microsoft.Win32

' Three-state snapshot of a single registry value (slice 0023, plan §3.E/§6).
' The distinction that matters: an ABSENT value is not zero. Restoring an absent value means
' DELETING it, never writing 0 — that is the classic trap that leaves Adobe in a state the
' operator did not have before. "Wrong type" is just Present with a Kind other than expected;
' it is preserved verbatim (same Kind + Value written back) rather than coerced.
Public Enum RegPresence
    Absent
    Present
End Enum

Public NotInheritable Class RegistryValueSnapshot

    Public ReadOnly Property Path As String
    Public ReadOnly Property Name As String
    Public ReadOnly Property Presence As RegPresence
    Public ReadOnly Property Kind As RegistryValueKind
    Public ReadOnly Property Value As Object

    Public Sub New(path As String, name As String, presence As RegPresence,
                   kind As RegistryValueKind, value As Object)
        Me.Path = path
        Me.Name = name
        Me.Presence = presence
        Me.Kind = kind
        Me.Value = value
    End Sub

    ' Value was not present under the key (or the key itself was missing).
    Public Shared Function AbsentSnap(path As String, name As String) As RegistryValueSnapshot
        Return New RegistryValueSnapshot(path, name, RegPresence.Absent, RegistryValueKind.Unknown, Nothing)
    End Function

    ' Value was present, captured with its exact kind and value.
    Public Shared Function PresentSnap(path As String, name As String,
                                       kind As RegistryValueKind, value As Object) As RegistryValueSnapshot
        Return New RegistryValueSnapshot(path, name, RegPresence.Present, kind, value)
    End Function

    ' One-line description for the log (the log is a deliverable of this slice).
    Public Overrides Function ToString() As String
        If Presence = RegPresence.Absent Then
            Return $"{Path}\{Name} = <absent>"
        End If
        Return $"{Path}\{Name} = {Value} ({Kind})"
    End Function

End Class
