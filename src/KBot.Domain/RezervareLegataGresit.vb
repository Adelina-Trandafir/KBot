''' <summary>
''' One reservation linked to a DDF revision that does not cover it (slice 0114-26): the revision holds another
''' day or another kind of operation, and its lines do not add up with this reservation. The reservation is then
''' counted as already having a revision and never gets the «+» of its own. Comes from the tree route
''' (<c>LegaturiGresite</c>) and from the repair route, so the main tree can mark the row and the repair can say
''' what it frees.
''' </summary>
Public NotInheritable Class RezervareLegataGresit
    ''' <summary>The reservation's day (<c>FX_Rezervari.DataRezervare</c>).</summary>
    Public Property DataRezervare As Date

    ''' <summary>The indicator code, e.g. «AAB».</summary>
    Public Property CodIndicator As String = String.Empty

    ''' <summary>The revision it is linked to by mistake (<c>FX_DDF_REV.IDREV</c>).</summary>
    Public Property IdRevizie As Integer

    ''' <summary>The value the reservation brings (positive = increase, negative = decrease).</summary>
    Public Property Valoare As Double

    ''' <summary>The date of that revision; <c>Nothing</c> when it has none.</summary>
    Public Property DataRevizie As Date?
End Class
