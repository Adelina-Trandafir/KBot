' One SNM statement on its way to the server (slice 0057).
'
' The robot has its own class for the same three fields (KBot.Forexe.ExtrasDescarcat).
' They are NOT merged: KBot.Forexe does not reference KBot.Domain, deliberately -- the
' robot returns what it scraped and the mapping into domain shapes happens above it, the
' same way JobResult tables become Angajament through AngajamentMapper. The shell copies
' one into the other in a handful of lines.
'
' PdfFisier and DataFisier are hashed together on the server into the per-file HASH, so
' neither may be reformatted on the way through: DataFisier stays dd.MM.yyyy HH:mm:ss.
Public Class ExtrasPentruImport
    Public Property PdfFisier As String = String.Empty
    Public Property DataFisier As String = String.Empty
    Public Property XmlContent As String = String.Empty
    Public Property CaleLocala As String = String.Empty
End Class
