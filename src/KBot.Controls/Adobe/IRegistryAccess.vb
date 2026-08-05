Option Strict On
Imports Microsoft.Win32

' I/O seam for registry access (slice 0023, plan §6: "No registry I/O inside the pure types;
' put I/O behind a small interface so the tests never touch the real registry").
' Paths are full, hive-prefixed strings, e.g. "HKEY_CURRENT_USER\Software\Adobe\...".
Public Interface IRegistryAccess

    ' Reads a single value. Missing key OR missing value -> an ABSENT snapshot (never throws for
    ' "not found"; a real I/O failure still surfaces).
    Function Read(path As String, name As String) As RegistryValueSnapshot

    ' Writes a value with an explicit kind, creating the key if needed.
    Sub Write(path As String, name As String, kind As RegistryValueKind, value As Object)

    ' Deletes a value if present; a missing value is a no-op (used to restore an ABSENT snapshot).
    Sub DeleteValue(path As String, name As String)

    ' True if the key itself exists (used to detect which AVGeneral hive is present).
    Function KeyExists(path As String) As Boolean

End Interface
