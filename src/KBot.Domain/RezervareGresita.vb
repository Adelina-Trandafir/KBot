''' <summary>
''' One reservation of an angajament whose chain is broken (operator, 12.10.2026): its
''' <c>R_Anterioara</c> is not what the previous reservation of the same indicator ended on
''' (<c>R_Definitiva</c>), so every figure computed from it is wrong. Comes from the tree route
''' (<c>RezervariGresite</c>) so the main tree can mark the row without opening the Rezervari view.
''' </summary>
Public NotInheritable Class RezervareGresita
    ''' <summary>The reservation's day (<c>FX_Rezervari.DataRezervare</c>).</summary>
    Public Property DataRezervare As Date

    ''' <summary>The indicator code, e.g. «AAB».</summary>
    Public Property CodIndicator As String = String.Empty

    ''' <summary>What the reservation holds as its previous value.</summary>
    Public Property Anterioara As Double

    ''' <summary>What it should hold: the end value of the previous reservation (0 when none).</summary>
    Public Property Asteptata As Double
End Class
