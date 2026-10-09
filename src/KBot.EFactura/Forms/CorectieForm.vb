Option Strict On

''' <summary>
''' Slice 00EF-16 -- «Corectează factura»: an invoice ANAF already accepted is corrected by sending it again as type 384. Only the
''' comment and the order reference change (the server refuses anything else); the lines, the customer and the date stay as they are.
''' The window only asks for the two texts; the send itself is FacturiForm's.
''' </summary>
Public Class CorectieForm

    ''' <summary>The comment as the operator left it.</summary>
    Public ReadOnly Property Comentarii As String
        Get
            Return txtComentarii.Text.Trim()
        End Get
    End Property

    ''' <summary>The order reference (BT-13) as the operator left it; may be empty.</summary>
    Public ReadOnly Property Referinta As String
        Get
            Return txtRef.Text.Trim()
        End Get
    End Property

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(k_label As String, k_comentarii As String, k_referinta As String)
        InitializeComponent()
        lblAntet.Text = $"Factura {k_label} a fost acceptată de ANAF. Corecția se trimite ca factură corectată (tip 384): " &
                        "se pot schimba doar comentariile și referința comenzii; pentru orice altceva, factura se stornează."
        txtComentarii.Text = If(k_comentarii, String.Empty)
        txtRef.Text = If(k_referinta, String.Empty)
    End Sub

End Class
