Option Strict On
Imports System.Collections.Generic

' Slice 00EF-18 -- what the received-invoice routes of the server answer (PYTHON/routes/efactura/primite_routes.py, slice 00EF-17).
' Plain data, no logic. Amounts are as the supplier wrote them (a credit note may be negative or carry Semn = -1).

''' <summary>A received e-invoice (<c>EF_Primite</c>): the header, as listed and as detailed.</summary>
Public NotInheritable Class EFacturaPrimita

    Public Property IdPrimita As Integer

    ''' <summary>ANAF's request number of the message (the supplier's upload number); the key of the message.</summary>
    Public Property IdSol As String = String.Empty

    Public Property NrFact As String = String.Empty
    Public Property DataFact As Date?
    Public Property DataScad As Date?

    ''' <summary>The VAT rate of the biggest taxable amount (a summary; the rates are in <see cref="EFacturaPrimitaDetaliu.Cote"/>).</summary>
    Public Property CotaTva As Decimal

    Public Property Tva As Decimal

    ''' <summary>The taxable amount.</summary>
    Public Property Valoare As Decimal

    ''' <summary>The payable amount.</summary>
    Public Property Total As Decimal

    ''' <summary>The supplier's tax code as written (the «RO» kept).</summary>
    Public Property Cui As String = String.Empty

    ''' <summary>The tax code without «RO», spaces and dots: what the DDF partners are compared with.</summary>
    Public Property CuiNormalizat As String = String.Empty

    Public Property Furnizor As String = String.Empty
    Public Property Adresa As String = String.Empty

    ''' <summary>Name of the first file embedded in the XML; empty when there is none.</summary>
    Public Property Atasament As String = String.Empty

    ''' <summary>«FC» invoice, «NC» credit note.</summary>
    Public Property Tip As String = "FC"

    Public Property Semn As Integer = 1
    Public Property Ref As String = String.Empty
    Public Property IdPrimitaRef As Integer?

    ''' <summary>The message was never opened.</summary>
    Public Property Nou As Boolean

    ''' <summary>In a list asked for a DDF: «auto» (the supplier is a partner of the DDF) or «manual» (the operator linked it); else empty.</summary>
    Public Property Legatura As String = String.Empty

    Public ReadOnly Property Eticheta As String
        Get
            Return $"{NrFact}"
        End Get
    End Property

End Class

Public NotInheritable Class EFacturaPrimitaLinie
    Public Property NrLinie As String = String.Empty
    Public Property Denumire As String = String.Empty
    Public Property Explicatie As String = String.Empty
    Public Property Unit As String = String.Empty
    Public Property Cant As Decimal
    Public Property Pret As Decimal
    Public Property Valoare As Decimal
End Class

''' <summary>One VAT rate of an invoice (one <c>TaxSubtotal</c> of the XML).</summary>
Public NotInheritable Class EFacturaPrimitaCota
    Public Property Categorie As String = String.Empty
    Public Property CotaTva As Decimal
    Public Property Baza As Decimal
    Public Property Tva As Decimal
End Class

Public NotInheritable Class EFacturaPrimitaMesaj
    Public Property IdMsg As Integer
    Public Property IdMesajAnaf As String = String.Empty
    Public Property Mesaj As String = String.Empty
    Public Property DataMesaj As Date?
End Class

''' <summary>A file embedded in the XML of the invoice.</summary>
Public NotInheritable Class EFacturaPrimitaAtasament
    ''' <summary>Position among the embedded files (0-based): what the server's file route takes.</summary>
    Public Property Index As Integer

    Public Property Nume As String = String.Empty
    Public Property Mime As String = String.Empty
    Public Property Octeti As Long
End Class

''' <summary>A DDF an invoice is linked to.</summary>
Public NotInheritable Class EFacturaPrimitaDdf
    Public Property IdDdf As Integer
    Public Property CodAngajament As String = String.Empty
End Class

''' <summary>A received invoice with everything the views show.</summary>
Public NotInheritable Class EFacturaPrimitaDetaliu
    Public Property Factura As New EFacturaPrimita()
    Public ReadOnly Property Linii As New List(Of EFacturaPrimitaLinie)()
    Public ReadOnly Property Cote As New List(Of EFacturaPrimitaCota)()
    Public ReadOnly Property Note As New List(Of String)()
    Public ReadOnly Property Mesaje As New List(Of EFacturaPrimitaMesaj)()
    Public ReadOnly Property Atasamente As New List(Of EFacturaPrimitaAtasament)()
    Public ReadOnly Property LegaturiAuto As New List(Of EFacturaPrimitaDdf)()
    Public ReadOnly Property LegaturiManuale As New List(Of EFacturaPrimitaDdf)()
End Class

''' <summary>What one call of the synchronisation did.</summary>
Public NotInheritable Class EFacturaSincronizare
    Public Property Cui As String = String.Empty
    Public Property Zile As Integer

    ''' <summary>Received messages ANAF lists for the period.</summary>
    Public Property Gasite As Integer

    Public Property Adaugate As Integer

    ''' <summary>Messages already in the database.</summary>
    Public Property Sarite As Integer

    ''' <summary>New messages this call left for the next one (it takes a batch at a time).</summary>
    Public Property Ramase As Integer

    Public ReadOnly Property Erori As New List(Of String)()
End Class

''' <summary>A DDF the operator can pick to link a received invoice to (slice 00EF-20).</summary>
Public NotInheritable Class EFacturaDdfAlegere
    Public Property IdDdf As Integer
    Public Property CodAngajament As String = String.Empty
    Public Property Obiect As String = String.Empty
    Public Property Partener As String = String.Empty
    Public Property CodFiscal As String = String.Empty
End Class
