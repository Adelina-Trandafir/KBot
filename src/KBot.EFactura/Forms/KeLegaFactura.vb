Option Strict On
Imports System.Collections.Generic
Imports KBot.Domain

''' <summary>
''' Slice KE11 -- what one ticked invoice asks of the reception, as <see cref="KeLegaForm"/> hands it back: the request to the server
''' (angajament, kind, reception, the amounts per classification), the classification of every line as chosen in the window, and whether any
''' choice differs from what the line had. Plain data, no logic.
''' </summary>
Public NotInheritable Class KeLegaFactura

    ''' <summary>The invoice as the list holds it.</summary>
    Public Property Factura As EFacturaPrimita

    ''' <summary>«number (supplier)»: how a message names the invoice.</summary>
    Public Property Eticheta As String = String.Empty

    ''' <summary>The request for THIS invoice (the amounts are the sums of its lines' «Recepționată» per classification).</summary>
    Public Property Cerere As EFacturaKeCerere

    ''' <summary>The classification of every line of the invoice (<c>IdClsf</c> Nothing = not chosen).</summary>
    Public ReadOnly Property Clasificari As New List(Of EFacturaKeLinie)()

    ''' <summary>The server's check of the request (filled by the page before anything is written).</summary>
    Public Property Verificare As EFacturaKeVerificare

    ''' <summary>At least one line got a classification different from the one it had when the window opened.</summary>
    Public Property ClasificariSchimbate As Boolean

End Class
