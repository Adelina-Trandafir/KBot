Option Strict On
Imports System.Collections.Generic

' Slice AV02-05 - «Modificare operatii» (the Access form ModificareNC1280): the list of the posted operations, the info panel, the delete check.
' Plain carriers; the rules live on the server (routes/avacont/modificare_date/operatii/).

''' <summary>The stage of the ordonantare of an operation (column «Ord»).</summary>
Public Enum OperatieStareOrd
    ''' <summary>No ordonantare.</summary>
    Niciuna = 0
    ''' <summary>The ordonantare exists (row in FX_ORD_TBL), nobody signed.</summary>
    Generat = 1
    ''' <summary>Signed by AB.</summary>
    SemnatAB = 2
    ''' <summary>Signed by CD.</summary>
    SemnatCD = 3
    ''' <summary>Signed by the Ordonator.</summary>
    Ordonator = 4
End Enum

''' <summary>Whether the invoices behind an operation have a reception (column «R»).</summary>
Public Enum OperatieStareReceptie
    ''' <summary>No invoice, or none of them has a reception.</summary>
    Niciuna = 0
    ''' <summary>Some of the invoices have a reception.</summary>
    Partiala = 1
    ''' <summary>All the invoices have a reception.</summary>
    Toate = 2
End Enum

''' <summary>One row of the list: one posted operation.</summary>
Public NotInheritable Class OperatieContabila
    Public Property IdOper As Integer
    Public Property IdDocument As Integer
    Public Property NumarNC As Integer
    Public Property NumarDocument As String = String.Empty
    Public Property DataDocument As Date?
    Public Property DataOperatie As Date?
    Public Property Clsf As String = String.Empty
    Public Property FelDocument As String = String.Empty
    ''' <summary>The kind of the document: E evidence, P payment, A miscellaneous, I closing, other codes as stored.</summary>
    Public Property Fel As String = String.Empty
    Public Property Jurnal As String = String.Empty
    Public Property Explicatie As String = String.Empty
    ''' <summary>The debit account as shown: «simbol» or «simbol.PCOD», «(+n)» when the side has more lines.</summary>
    Public Property Debit As String = String.Empty
    Public Property Credit As String = String.Empty
    Public Property Suma As Decimal
    Public Property Chelt As Boolean
    Public Property Cfpp As Boolean
    Public Property Ord As OperatieStareOrd
    Public Property Receptie As OperatieStareReceptie
    ''' <summary>Why the operation cannot be deleted (Romanian); empty when it can.</summary>
    Public Property Refuz As String = String.Empty
End Class

''' <summary>The operations of a subunit and year, and the date of the last closing.</summary>
Public NotInheritable Class OperatiiContabileLista
    Public Property Operatii As IReadOnlyList(Of OperatieContabila) = New List(Of OperatieContabila)()
    Public Property Inchidere As Date?
End Class

''' <summary>An invoice paid by a payment.</summary>
Public NotInheritable Class OperatieFacturaPlatita
    Public Property Numar As String = String.Empty
    Public Property Data As Date?
    Public Property Platit As Decimal
End Class

''' <summary>A partner balance: debit and credit totals.</summary>
Public NotInheritable Class OperatieSold
    Public Property Debit As Decimal
    Public Property Credit As Decimal

    ''' <summary>Debit minus credit.</summary>
    Public ReadOnly Property Sold As Decimal
        Get
            Return Debit - Credit
        End Get
    End Property
End Class

''' <summary>The info panel of one operation (the Access popup MNotaC11).</summary>
Public NotInheritable Class OperatieInfo
    Public Property Fel As String = String.Empty
    ''' <summary>INTEGRAL / PARTIAL for an evidence with payments, else empty.</summary>
    Public Property Factura As String = String.Empty
    Public Property NumarOP As Integer?
    Public Property DataOP As Date?
    Public Property Facturi As IReadOnlyList(Of OperatieFacturaPlatita) = New List(Of OperatieFacturaPlatita)()
    Public Property CodPartener As String = String.Empty
    Public Property DenumirePartener As String = String.Empty
    Public Property DataDocument As Date?
    ''' <summary>Opening balance + operations up to the date of the document; Nothing without a partner.</summary>
    Public Property SoldLaData As OperatieSold
    ''' <summary>Opening balance + all the operations of the year; Nothing without a partner.</summary>
    Public Property SoldAnual As OperatieSold
End Class

''' <summary>The answer of the delete check.</summary>
Public NotInheritable Class OperatieStergereVerificare
    ''' <summary>Why the delete is refused (Romanian); empty when it may go on.</summary>
    Public Property Refuz As String = String.Empty
    ''' <summary>The confirmation the operator answers.</summary>
    Public Property Intrebare As String = String.Empty
    Public Property Facturi As IReadOnlyList(Of OperatieFacturaPlatita) = New List(Of OperatieFacturaPlatita)()
End Class
