Option Strict On

''' <summary>
''' One SNM bank statement, as the robot brings it back (slice 0057): the PDF's name, the
''' date the statement itself declares, and the XML that was embedded in the PDF.
''' </summary>
''' <remarks>
''' These three fields ARE the contract, and they are the same three the Access system
''' carried over the pipe as <c>PdfFisier</c> / <c>DataFisier</c> / <c>XmlContent</c>
''' (see <c>mdl_FX_Extrase.FX_Extrase_Prelucrare</c>). Two of them are hashed together into
''' the per-file HASH that stops a statement being imported twice, so their names and their
''' formatting cannot drift: <c>DataFisier</c> travels as <c>dd.MM.yyyy HH:mm:ss</c>,
''' invariant culture, exactly as it was written by the robot that Access read from.
'''
''' Nothing here is parsed. The robot downloads and unwraps; the XML is read on the SERVER,
''' by the import route, which is also the only side that can resolve a unit or a
''' classification. That is the same split Access had -- its robot pushed these three
''' fields and Access did the whole of the parsing.
''' </remarks>
Public NotInheritable Class ExtrasDescarcat

    ''' <summary>The PDF file name, as FOREXE names it. Part of the file HASH.</summary>
    Public Property PdfFisier As String = String.Empty

    ''' <summary>
    ''' The statement's own date, formatted dd.MM.yyyy HH:mm:ss (invariant). Read from the
    ''' «din data ...» tail of the FOREXE message description, NOT from the file name.
    ''' Part of the file HASH.
    ''' </summary>
    Public Property DataFisier As String = String.Empty

    ''' <summary>The XML embedded in the PDF, verbatim.</summary>
    Public Property XmlContent As String = String.Empty

    ''' <summary>Where the PDF was saved locally. Recorded, never hashed.</summary>
    Public Property CaleLocala As String = String.Empty
End Class
