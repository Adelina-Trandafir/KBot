Option Strict On
Imports Microsoft.Win32
Imports KBot.DevHarness
' IRegistryAccess / RegistryValueSnapshot au trecut în KBot.Controls (felia 0024-03).
Imports KBot.Controls

' In-memory IRegistryAccess for the pure-helper tests (slice 0023, plan §6): the tests never
' touch the real registry. Stores (kind, value) per path|name, case-insensitively, and tracks
' key existence separately so the hive resolver can be probed too.
Public NotInheritable Class FakeRegistryAccess
    Implements IRegistryAccess

    Private NotInheritable Class Entry
        Public ReadOnly Kind As RegistryValueKind
        Public ReadOnly Value As Object

        Public Sub New(kind As RegistryValueKind, value As Object)
            Me.Kind = kind
            Me.Value = value
        End Sub
    End Class

    Private ReadOnly _values As New Dictionary(Of String, Entry)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _keys As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

    Private Shared Function K(path As String, name As String) As String
        Return path & "|" & name
    End Function

    ' Test setup: seed a value (and its key) directly.
    Public Sub Seed(path As String, name As String, kind As RegistryValueKind, value As Object)
        _values(K(path, name)) = New Entry(kind, value)
        _keys.Add(path)
    End Sub

    Public Sub SeedKey(path As String)
        _keys.Add(path)
    End Sub

    Public Function Read(path As String, name As String) As RegistryValueSnapshot Implements IRegistryAccess.Read
        Dim e As Entry = Nothing
        If _values.TryGetValue(K(path, name), e) Then
            Return RegistryValueSnapshot.PresentSnap(path, name, e.Kind, e.Value)
        End If
        Return RegistryValueSnapshot.AbsentSnap(path, name)
    End Function

    Public Sub Write(path As String, name As String, kind As RegistryValueKind, value As Object) Implements IRegistryAccess.Write
        _values(K(path, name)) = New Entry(kind, value)
        _keys.Add(path)
    End Sub

    Public Sub DeleteValue(path As String, name As String) Implements IRegistryAccess.DeleteValue
        _values.Remove(K(path, name))
    End Sub

    Public Function KeyExists(path As String) As Boolean Implements IRegistryAccess.KeyExists
        Return _keys.Contains(path)
    End Function

    Public Function ValueNames(path As String) As IReadOnlyList(Of String) Implements IRegistryAccess.ValueNames
        Dim names As New List(Of String)()
        Dim prefix As String = path & "|"
        For Each key As String In _values.Keys
            If key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) Then
                names.Add(key.Substring(prefix.Length))
            End If
        Next
        names.Sort(StringComparer.OrdinalIgnoreCase)
        Return names
    End Function

End Class
