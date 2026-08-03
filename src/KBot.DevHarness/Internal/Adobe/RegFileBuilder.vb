Option Strict On
Imports System.Collections.Generic
Imports System.Text

' Pure .reg text builder (slice 0023, plan §3.F/§6). Emits a "Version 5.00" file with values
' grouped under their section headers in insertion order. dword values are 8-hex lowercase; a
' deletion is the `"name"=-` line; value names are escaped (\\ and \"). The caller writes the
' returned text to disk with the right encoding (reg.exe expects UTF-16 for v5 files) — encoding
' is an I/O concern, kept out of this pure type.
Public NotInheritable Class RegFileBuilder

    Public Const Header As String = "Windows Registry Editor Version 5.00"

    Private ReadOnly _sectionOrder As New List(Of String)()
    Private ReadOnly _lines As New Dictionary(Of String, List(Of String))(StringComparer.OrdinalIgnoreCase)

    Private Sub EnsureSection(section As String)
        If Not _lines.ContainsKey(section) Then
            _lines(section) = New List(Of String)()
            _sectionOrder.Add(section)
        End If
    End Sub

    ' Adds a REG_DWORD assignment under a section (the section is a full hive-prefixed path,
    ' without brackets — Build adds them).
    Public Function AddDword(section As String, name As String, value As UInteger) As RegFileBuilder
        EnsureSection(section)
        _lines(section).Add($"""{EscapeName(name)}""=dword:{value:x8}")
        Return Me
    End Function

    ' Adds a REG_SZ assignment (used when a revert must restore a present-but-string original).
    Public Function AddString(section As String, name As String, value As String) As RegFileBuilder
        EnsureSection(section)
        _lines(section).Add($"""{EscapeName(name)}""=""{EscapeName(value)}""")
        Return Me
    End Function

    ' Adds a value-deletion line (`"name"=-`) — used in the revert file for values that were absent.
    Public Function DeleteValue(section As String, name As String) As RegFileBuilder
        EnsureSection(section)
        _lines(section).Add($"""{EscapeName(name)}""=-")
        Return Me
    End Function

    Public Function Build() As String
        Dim sb As New StringBuilder()
        sb.AppendLine(Header)
        For Each section As String In _sectionOrder
            sb.AppendLine()
            sb.AppendLine($"[{section}]")
            For Each ln As String In _lines(section)
                sb.AppendLine(ln)
            Next
        Next
        Return sb.ToString()
    End Function

    ' .reg escaping for value names: backslash and double-quote.
    Private Shared Function EscapeName(name As String) As String
        Return name.Replace("\", "\\").Replace("""", "\""")
    End Function

End Class
