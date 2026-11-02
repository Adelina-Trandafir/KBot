Option Strict On

''' <summary>
''' Slice 01EF-03 -- a received invoice in the archive (table <c>EF_PrimiteArhiva</c>). <see cref="DataArhivare"/> and <see cref="Un"/> are
''' Nothing for the invoices migrated from Access (neither who nor when was kept there).
''' </summary>
Public NotInheritable Class EFacturaArhivaRand
    Public Property IdPrimita As Integer
    Public Property DataArhivare As Date?
    Public Property Un As String = String.Empty
    Public Property Motiv As String = String.Empty
End Class
