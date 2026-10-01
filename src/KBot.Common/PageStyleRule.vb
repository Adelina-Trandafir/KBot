Option Strict On
Imports System.Collections.Generic
Imports System.Linq

''' <summary>
''' One CSS rule K-BOT puts into every FOREXE page the operator sees (operator, 21.09.2026):
''' a selector and the declarations to force on it. The rules live in <see cref="AppSettings"/>,
''' are edited in «Setări» -> «Pagina FOREXE», and <c>ForexeWatch.js</c> writes them into a
''' stylesheet at every load and refresh, each declaration marked <c>!important</c> so it beats
''' the inline styles FOREXE's own markup carries. Never element ids from Wicket (they change
''' from render to render): tags, classes, names and static ids only.
''' </summary>
Public NotInheritable Class PageStyleRule

    ''' <summary>Off = kept in the list but not written into the page.</summary>
    Public Property Enabled As Boolean = True

    ''' <summary>The CSS selector, as the browser reads it (<c>:has()</c> included).</summary>
    Public Property Selector As String = String.Empty

    ''' <summary>Declarations, <c>prop: value; prop: value</c>. <c>!important</c> is added by the page.</summary>
    Public Property Css As String = String.Empty

    ''' <summary>What the rule is for, in the operator's words.</summary>
    Public Property Note As String = String.Empty

    ''' <summary>
    ''' The one page the rule holds on, or empty for every page (operator, 21.09.2026: «hidden
    ''' ONLY when the current page is …/CABWeb/contract»). Compared by the page script without
    ''' the query string: a full address matches origin + path, a value starting with «/»
    ''' matches the path, anything else matches the last segment of the path («contract»).
    ''' </summary>
    Public Property Page As String = String.Empty

    Public Sub New()
    End Sub

    Public Sub New(note As String, selector As String, css As String)
        Me.Note = If(note, String.Empty)
        Me.Selector = If(selector, String.Empty)
        Me.Css = If(css, String.Empty)
    End Sub

    Public Sub New(note As String, selector As String, css As String, page As String)
        Me.New(note, selector, css)
        Me.Page = If(page, String.Empty)
    End Sub

    Public Function Clone() As PageStyleRule
        Return New PageStyleRule(Note, Selector, Css, Page) With {.Enabled = Enabled}
    End Function

    ''' <summary>
    ''' The rules the operator asked for on 21.09.2026. The first two hold only while an
    ''' angajament is open (the header with its code is on the page), so the list and home
    ''' pages keep FOREXE's own menu - the robot's flows start from that menu.
    ''' </summary>
    Public Shared Function Defaults() As List(Of PageStyleRule)
        Const codOpen As String = "body:has(.well.well-small h4 span:nth-child(2)) "
        Dim rules As New List(Of PageStyleRule) From {
            New PageStyleRule("Meniul lateral FOREXE ascuns cât timp un angajament e deschis",
                              codOpen & "[class*='col-lg-2']:has(.bs-sidebar)",
                              "visibility: hidden"),
            New PageStyleRule("Conținutul pe toată lățimea cât timp un angajament e deschis",
                              codOpen & "[class*='col-lg-2']:has(.bs-sidebar) + [class*='col-lg-10']",
                              "width: 100%")
        }
        rules.AddRange(NewFormRules())
        rules.AddRange(New List(Of PageStyleRule) From {
            New PageStyleRule("Pagina folosește 90% din lățimea ferestrei",
                              "#main.container",
                              "max-width: 90%"),
            New PageStyleRule("Fără mărirea textului paginii (105%)",
                              "body",
                              "font-size: 100%"),
            New PageStyleRule("Butonul «Înapoi» din bara de file ascuns pe pagina angajamentului",
                              "span.nav.nav-tabs ~ button.btn.btn-default",
                              "display: none",
                              "https://forexe.mfinante.gov.ro/CABWeb/contract")
        })
        Return rules
    End Function

    ' The empty «Angajament nou» form has no code header yet, so the two rules above never hold on
    ' it and FOREXE's side menu stayed in view (operator, 01.10.2026). The form is told apart by its
    ' «descriere» field.
    Private Const NewFormOpen As String = "body:has(textarea[name='descriere']) "

    Private Shared Function NewFormRules() As List(Of PageStyleRule)
        Return New List(Of PageStyleRule) From {
            New PageStyleRule("Meniul lateral FOREXE ascuns pe formularul de angajament nou",
                              NewFormOpen & "[class*='col-lg-2']:has(.bs-sidebar)",
                              "visibility: hidden"),
            New PageStyleRule("Conținutul pe toată lățimea pe formularul de angajament nou",
                              NewFormOpen & "[class*='col-lg-2']:has(.bs-sidebar) + [class*='col-lg-10']",
                              "width: 100%")
        }
    End Function

    ''' <summary>
    ''' Settings saved before 01.10.2026 hold the first rules of <see cref="Defaults"/> but not the
    ''' ones for the «Angajament nou» form. Adds those, once: only when the list still has the
    ''' «angajament deschis» menu rule (an operator who deleted that one does not want them either)
    ''' and has no rule on the new form yet. Returns True when it changed the list.
    ''' </summary>
    Public Shared Function AddNewFormRulesIfMissing(rules As List(Of PageStyleRule)) As Boolean
        If rules Is Nothing Then Return False
        Dim hasMenuRule As Boolean = rules.Any(Function(r) r IsNot Nothing AndAlso
            r.Selector.Contains(".bs-sidebar") AndAlso r.Selector.Contains("h4 span:nth-child(2)"))
        Dim hasNewFormRule As Boolean = rules.Any(Function(r) r IsNot Nothing AndAlso
            r.Selector.Contains("textarea[name='descriere']"))
        If Not hasMenuRule OrElse hasNewFormRule Then Return False
        rules.AddRange(NewFormRules())
        Return True
    End Function

    ''' <summary>Deep copy of a list; Nothing gives an empty list.</summary>
    Public Shared Function CloneList(rules As IEnumerable(Of PageStyleRule)) As List(Of PageStyleRule)
        Dim copy As New List(Of PageStyleRule)()
        If rules Is Nothing Then Return copy
        For Each r As PageStyleRule In rules
            If r IsNot Nothing Then copy.Add(r.Clone())
        Next
        Return copy
    End Function

End Class
