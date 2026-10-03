Option Strict On

''' <summary>
''' The one password shared by the AVACONT Access files (operator, 02.10.2026), used by the Migrator
''' (<c>AccessProvider.Open</c>) and by K-BOT («Trimite in Access»).
''' </summary>
''' <remarks>
''' Kept as bytes XOR-ed with a key so it is not a readable string in the binary. That only hides it from
''' a casual look: anyone who decompiles the application recovers it, so it is NOT protection.
''' </remarks>
Public NotInheritable Class AccessFilePassword

    Private Shared ReadOnly Mask As Byte() = {&H3A, &H71, &H16, &H5F, &H2D, &H2F}
    Private Shared ReadOnly Key As Byte() = {&H5B, &H1F, &H72, &H2D, &H48, &H66}

    Private Sub New()
    End Sub

    ''' <summary>The shared password, rebuilt on every call (never stored as a string field).</summary>
    Public Shared Function Value() As String
        Dim chars(Mask.Length - 1) As Char
        For i As Integer = 0 To Mask.Length - 1
            chars(i) = Convert.ToChar(Mask(i) Xor Key(i))
        Next
        Return New String(chars)
    End Function

End Class
