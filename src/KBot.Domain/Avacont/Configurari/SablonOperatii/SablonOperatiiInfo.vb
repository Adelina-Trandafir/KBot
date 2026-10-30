Option Strict On
Imports System.Collections.Generic

''' <summary>One line of an operation template (slice AV01-14, <c>TZ_SablonOperatiiLinie</c>): the BASE accounts and who the partner is on each side.</summary>
Public NotInheritable Class SablonLinieInfo
    ''' <summary>0 = a line not saved yet (the server replaces every line of a template on save anyway).</summary>
    Public Property IdLinie As Integer
    ''' <summary>Subchapter of the classification (04.02); empty = valid for every form.</summary>
    Public Property Forma As String = String.Empty
    ''' <summary>Base account, debit (<c>DefaCont.Simbol</c>); 0 = not chosen yet.</summary>
    Public Property ContDebit As Integer
    Public Property ContCredit As Integer
    ''' <summary>Analytic of the debit account (<c>TZ_PlanCont.Analitic</c>); empty = the account without analytic (slice AV01-17).</summary>
    Public Property AnaliticDebit As String = String.Empty
    Public Property AnaliticCredit As String = String.Empty
    ''' <summary>The classification of the line (<c>Clasificatii.IDClsf</c>); 0 = none. Type A: chosen per line. Type M: the server fills it from the template key + <see cref="Forma"/>.</summary>
    Public Property IdClsf As Integer
    ''' <summary>The partner on the debit side: <see cref="SablonOperatiiModuri.Nimic"/>, <see cref="SablonOperatiiModuri.Operatie"/> or <see cref="SablonOperatiiModuri.Fix"/>.</summary>
    Public Property PartDebitMod As String = SablonOperatiiModuri.Nimic
    ''' <summary>The fixed partner; 0 unless <see cref="PartDebitMod"/> is <see cref="SablonOperatiiModuri.Fix"/>.</summary>
    Public Property IdPartDebit As Integer
    Public Property PartCreditMod As String = SablonOperatiiModuri.Nimic
    Public Property IdPartCredit As Integer
    ''' <summary>Proposal + commitment (evidence only).</summary>
    Public Property PA As Boolean
    ''' <summary>Payment order line (payment only).</summary>
    Public Property Ordonantare As Boolean
    ''' <summary>Effective expense.</summary>
    Public Property Chelt As Boolean
End Class

''' <summary>The values of the partner mode of a template line (the wire values of <c>PartDebitMod</c> / <c>PartCreditMod</c>).</summary>
Public NotInheritable Class SablonOperatiiModuri
    Private Sub New()
    End Sub

    ''' <summary>No partner on that side.</summary>
    Public Const Nimic As String = "NIMIC"
    ''' <summary>The partner of the operation, chosen when the template is used (the Access <c>XXX</c> mark).</summary>
    Public Const Operatie As String = "OPERATIE"
    ''' <summary>A fixed partner, kept on the line.</summary>
    Public Const Fix As String = "FIX"
End Class

''' <summary>One operation template (slice AV01-14, <c>TZ_SablonOperatii</c>): the header and its lines.</summary>
Public NotInheritable Class SablonOperatieInfo
    ''' <summary>0 = a template not saved yet.</summary>
    Public Property IdSablon As Integer
    Public Property IdJurnal As Integer
    ''' <summary><c>M</c> = classification + partner on one side; <c>A</c> = other operations.</summary>
    Public Property Tip As String = "M"
    ''' <summary><c>E</c> evidence, <c>P</c> payment, <c>A</c> other operations.</summary>
    Public Property Fel As String = "E"
    Public Property Explicatie As String = String.Empty
    Public Property Compartiment As String = String.Empty
    ''' <summary>The text added to the payment order (payment only).</summary>
    Public Property FelOP As String = String.Empty
    ''' <summary>The classification key with XX.XX for the subchapter (required for type M).</summary>
    Public Property Cheie As String = String.Empty
    Public Property Linii As New List(Of SablonLinieInfo)()
End Class

''' <summary>A base account offered by the template lines (<c>AVACONT_COMUN.DefaCont</c>).</summary>
Public NotInheritable Class SablonContInfo
    Public Property Simbol As Integer
    Public Property Denumire As String = String.Empty
End Class

''' <summary>One concrete classification of the unit (<c>Clasificatii</c>, slice AV01-17): what the classification combo of a line of type A offers.</summary>
Public NotInheritable Class SablonClsfInfo
    Public Property IdClsf As Integer
    ''' <summary>The concrete key: 65.01.04.02.57.02.02.</summary>
    Public Property Clsf As String = String.Empty
    ''' <summary>Functional part (chapter + form, 650402).</summary>
    Public Property ClsfF As String = String.Empty
    ''' <summary>Economic part (article + paragraph, 570202).</summary>
    Public Property ClsfE As String = String.Empty
    Public Property SursaSector As String = String.Empty
    Public Property Capitol As String = String.Empty
    Public Property Articol As String = String.Empty
    Public Property Alineat As String = String.Empty
    Public Property Denumire As String = String.Empty
End Class

''' <summary>An account of the chart of accounts of the unit (<c>TZ_PlanCont</c>, slice AV01-17): what the account combos of a line offer.</summary>
Public NotInheritable Class SablonContPlanInfo
    ''' <summary>Sector + source of the account (01A ...).</summary>
    Public Property SursaSector As String = String.Empty
    ''' <summary>The full symbol: base + sector/source + [functional] + [economic] + [.analytic].</summary>
    Public Property Simbol As String = String.Empty
    ''' <summary>The base account (<c>DefaCont.Simbol</c>).</summary>
    Public Property Cont As Integer
    Public Property Analitic As String = String.Empty
    ''' <summary>Functional part (chapter + form, e.g. 650402); empty = the account is not tied to a functional classification.</summary>
    Public Property ClsfF As String = String.Empty
    ''' <summary>Economic part (article + paragraph, e.g. 200101); empty = not tied to an economic classification.</summary>
    Public Property ClsfE As String = String.Empty
    Public Property Denumire As String = String.Empty
End Class

''' <summary>A partner offered as a fixed partner of a template line.</summary>
Public NotInheritable Class SablonPartenerInfo
    Public Property IdPartener As Integer
    Public Property Cod As String = String.Empty
    Public Property Denumire As String = String.Empty
End Class

''' <summary>A classification key the tree of type M hangs templates on (<c>Clasificatii.ClsfX</c>: capitol.XX.XX.articol.alineat) with its forms (subchapters).</summary>
Public NotInheritable Class SablonClasificatieInfo
    Public Property Cheie As String = String.Empty
    ''' <summary>Sector + source of the classification (01A ...).</summary>
    Public Property SursaSector As String = String.Empty
    ''' <summary>The subchapters (04.02 ...) the key has: one default line per form.</summary>
    Public Property Forme As New List(Of String)()
End Class

''' <summary>The templates of the unit database and the lists the window's combos need.</summary>
Public NotInheritable Class SabloaneOperatiiCatalog
    Public Property Sabloane As IReadOnlyList(Of SablonOperatieInfo) = New List(Of SablonOperatieInfo)()
    Public Property Jurnale As IReadOnlyList(Of JurnalInfo) = New List(Of JurnalInfo)()
    Public Property Conturi As IReadOnlyList(Of SablonContInfo) = New List(Of SablonContInfo)()
    ''' <summary>The chart of accounts of the unit, one row per symbol and sector+source (the account combos of the lines).</summary>
    ''' <summary>Every concrete classification of the unit (the classification combo of a line of type A; <c>IdClsf</c> of a line points at one).</summary>
    Public Property ClasificatiiLinii As IReadOnlyList(Of SablonClsfInfo) = New List(Of SablonClsfInfo)()
    Public Property ConturiPlan As IReadOnlyList(Of SablonContPlanInfo) = New List(Of SablonContPlanInfo)()
    Public Property Parteneri As IReadOnlyList(Of SablonPartenerInfo) = New List(Of SablonPartenerInfo)()
    ''' <summary>The classification keys of the unit (the tree of type M hangs the templates on them).</summary>
    Public Property Clasificatii As IReadOnlyList(Of SablonClasificatieInfo) = New List(Of SablonClasificatieInfo)()
    ''' <summary>The names of the economic titles, articles and paragraphs (<c>DefaClsfE</c>): code (six digits) to name.</summary>
    Public Property NumeClsfE As IReadOnlyDictionary(Of String, String) = New Dictionary(Of String, String)()
End Class
