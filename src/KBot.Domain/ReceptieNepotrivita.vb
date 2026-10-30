''' <summary>
''' One day on which the receptions of an angajament do not add up to its reservations (slice 0122-10): the sum
''' of <c>FX_Receptii.DIF</c> up to that day differs from the cumulative value the reservation ended the day on
''' (<c>FX_Rezervari.R_Definitiva</c>). A reception version put on the wrong header gives exactly this, silently,
''' and the ordonantari that add <c>DIF</c> up to a payment date then see wrong receptions. Comes from the tree
''' route (<c>ReceptiiNepotrivite</c>) so the main tree can say it in the tooltip.
''' </summary>
Public NotInheritable Class ReceptieNepotrivita
    ''' <summary>The day of the reservation.</summary>
    Public Property Zi As Date

    ''' <summary>The indicator code, e.g. «AAB».</summary>
    Public Property CodIndicator As String = String.Empty

    ''' <summary>What the reservations say was received up to that day (<c>R_Definitiva</c>).</summary>
    Public Property Rezervat As Double

    ''' <summary>What the receptions add up to up to that day (sum of <c>DIF</c>).</summary>
    Public Property Receptionat As Double
End Class
