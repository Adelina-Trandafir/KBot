Option Strict On
Imports System.Collections.Generic

''' <summary>The kinds of line under a salary-template article (the wire values of <c>TZ_SablonSalariiLinie.Tip</c>).</summary>
Public NotInheritable Class SablonSalariiTipuri
    Private Sub New()
    End Sub

    ''' <summary>An amount kept from the salary (its debit is the credit of the article).</summary>
    Public Const Retinere As String = "RETINERE"
    ''' <summary>The payment of what was kept, to the third party.</summary>
    Public Const Virament As String = "VIRAMENT"
    ''' <summary>Salaries paid in cash.</summary>
    Public Const Numerar As String = "NUMERAR"
End Class

''' <summary>One line under an article (slice AV01-15, <c>TZ_SablonSalariiLinie</c>): base accounts and the partner of each side.</summary>
Public NotInheritable Class SablonSalariiLinieInfo
    ''' <summary>0 = a line not saved yet.</summary>
    Public Property IdLinie As Integer
    ''' <summary>The number the window gives the line so that a virament can name the retention it pays even when that one is not saved yet (existing line: its id; new line: negative).</summary>
    Public Property Ref As Integer
    ''' <summary>The kind: <see cref="SablonSalariiTipuri"/>.</summary>
    Public Property Tip As String = SablonSalariiTipuri.Retinere
    ''' <summary>For a virament: the <see cref="Ref"/> of the retention it pays; 0 = none.</summary>
    Public Property Parinte As Integer
    Public Property Explicatie As String = String.Empty
    Public Property ContDebit As Integer
    Public Property ContCredit As Integer
    Public Property PartDebitMod As String = SablonOperatiiModuri.Nimic
    Public Property IdPartDebit As Integer
    Public Property PartCreditMod As String = SablonOperatiiModuri.Nimic
    Public Property IdPartCredit As Integer
    ''' <summary>Medical leave (retentions and viraments only; one per article and kind).</summary>
    Public Property EsteCM As Boolean
End Class

''' <summary>One article of the salary template (slice AV01-15, <c>TZ_SablonSalarii</c>): a classification of the pay roll with its commitment operation and its lines.</summary>
Public NotInheritable Class SablonSalariiArticolInfo
    ''' <summary>0 = an article not saved yet.</summary>
    Public Property IdArticol As Integer
    Public Property IdSubunitate As Integer
    ''' <summary>Article + paragraph of the pay roll, NN.NN.NN.</summary>
    Public Property Clasificatie As String = String.Empty
    Public Property Explicatie As String = String.Empty
    Public Property ContDebit As Integer
    Public Property ContCredit As Integer
    Public Property PartDebitMod As String = SablonOperatiiModuri.Nimic
    Public Property IdPartDebit As Integer
    Public Property PartCreditMod As String = SablonOperatiiModuri.Nimic
    Public Property IdPartCredit As Integer
    Public Property Linii As New List(Of SablonSalariiLinieInfo)()
End Class

''' <summary>A subunit the salary templates hang on (its chapter decides the articles).</summary>
Public NotInheritable Class SablonSubunitInfo
    Public Property IdSubunitate As Integer
    Public Property Capitol As String = String.Empty
    ''' <summary>Sector + source (01A ...).</summary>
    Public Property SursaSector As String = String.Empty
    Public Property Denumire As String = String.Empty
End Class

''' <summary>The articles of the unit database and the lists the window's combos need.</summary>
Public NotInheritable Class SabloaneSalariiCatalog
    Public Property Articole As IReadOnlyList(Of SablonSalariiArticolInfo) = New List(Of SablonSalariiArticolInfo)()
    Public Property Subunitati As IReadOnlyList(Of SablonSubunitInfo) = New List(Of SablonSubunitInfo)()
    Public Property Conturi As IReadOnlyList(Of SablonContInfo) = New List(Of SablonContInfo)()
    Public Property Parteneri As IReadOnlyList(Of SablonPartenerInfo) = New List(Of SablonPartenerInfo)()
End Class
