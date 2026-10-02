''' <summary>
''' One reception of an angajament whose chain does not close (slice 0101): the LAST snapshot
''' of the chain is not a deletion row and its total differs from the reception's current value.
''' Comes from the tree route (<c>LantNeinchis</c>) so the main tree can show it without opening
''' the association form.
''' </summary>
Public NotInheritable Class ReceptieNeinchisa
    ''' <summary>The reception's date (<c>FX_Receptii_R.DataR</c>).</summary>
    Public Property DataReceptie As Date

    ''' <summary>Total of the chain's last snapshot (<c>FX_Receptii_H.Total</c>).</summary>
    Public Property TotalUltimInstantaneu As Double

    ''' <summary>The reception's current value (<c>FX_Receptii_R.SumaAntet</c>).</summary>
    Public Property ValoareReceptie As Double
End Class
