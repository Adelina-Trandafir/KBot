Option Strict On
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Drawing

''' <summary>
''' English (slice 0025): one entry of a <see cref="KBotNavList"/> — either a button (key, text,
''' optional badge, Enabled, Visible) or a separator (<see cref="IsSeparator"/>), never both.
'''
''' ONE type carries both roles on purpose. Two types in one ordered collection would need a
''' custom collection editor, a custom editor needs a design-time assembly built against
''' Microsoft.WinForms.Designer.SDK (the net8.0-windows designer runs out of process), and this
''' slice deliberately buys none of that. The price is <see cref="IsSeparator"/> as a flag and a
''' <see cref="ToString"/> that makes the difference obvious in the collection dialog's list.
'''
''' Replaces the private nested <c>NavItem</c> that lived inside <see cref="KBotNavList"/>.
''' </summary>
Public NotInheritable Class KBotNavItem

    ''' <summary>Parameterless constructor — required by the designer's collection editor.</summary>
    Public Sub New()
    End Sub

    ''' <summary>Convenience for code: a button aligned at the start of the bar.</summary>
    Public Sub New(key As String, text As String)
        Me.New(key, text, KBotNavAlign.Near)
    End Sub

    ''' <summary>Convenience for code: a button with an explicit alignment.</summary>
    Public Sub New(key As String, text As String, align As KBotNavAlign)
        ' «Me.» is MANDATORY: VB is case-insensitive, so the parameters shadow the properties
        ' and an unqualified assignment would write the parameter (the 0010 / 0019 trap).
        Me.Key = key
        Me.Text = If(text, String.Empty)
        Me.Align = align
    End Sub

    <Category("K-BOT")>
    <Description("Identificatorul folosit de SelectedKey / SetItemVisible / SetItemEnabled / SetBadge. Trebuie să fie nevid și unic. Ignorat complet când elementul e separator.")>
    Public Property Key As String

    <Category("K-BOT")>
    <Description("Textul afișat pe buton.")>
    Public Property Text As String

    <Category("K-BOT")>
    <Description("Numărul din pastila din dreapta butonului. 0 = pastila nu se desenează.")>
    <DefaultValue(0)>
    Public Property Badge As Integer

    <Category("K-BOT")>
    <Description("False => butonul e desenat șters și nu se poate selecta (dar ocupă spațiu).")>
    <DefaultValue(True)>
    Public Property Enabled As Boolean = True

    <Category("K-BOT")>
    <Description("False => elementul nu ocupă spațiu, nu se pictează, nu se selectează și e sărit de navigarea cu tastatura.")>
    <DefaultValue(True)>
    Public Property Visible As Boolean = True

    <Category("K-BOT")>
    <Description("True => linie fină neselectabilă în locul unui buton. Cheia și textul sunt ignorate.")>
    <DefaultValue(False)>
    Public Property IsSeparator As Boolean

    <Category("K-BOT")>
    <Description("Near = ancorat la început (sus/stânga), Far = ancorat la capăt (jos/dreapta).")>
    <DefaultValue(KBotNavAlign.Near)>
    Public Property Align As KBotNavAlign = KBotNavAlign.Near

    ''' <summary>
    ''' The slot computed by <c>KBotNavList.RecalcLayout</c> (<see cref="Rectangle.Empty"/> when the
    ''' item is hidden). FRIEND, not Public: the designer must never see it and must never
    ''' serialize it — it is derived state, recomputed on every layout pass.
    ''' </summary>
    Friend Property Bounds As Rectangle

    ''' <summary>
    ''' What the collection dialog's left-hand list shows. This is the whole reason one item type
    ''' is bearable: a separator has to be recognisable at a glance among the buttons.
    ''' </summary>
    Public Overrides Function ToString() As String
        If IsSeparator Then
            Return "──────── separator (" & Align.ToString() & ") ────────"
        End If
        Dim shownKey As String = If(String.IsNullOrWhiteSpace(Key), "<fără cheie>", Key)
        Return shownKey & " — """ & If(Text, String.Empty) & """ (" & Align.ToString() & ")"
    End Function

End Class

''' <summary>
''' English (slice 0025): the ordered collection behind <see cref="KBotNavList.Items"/>. Every
''' mutation invalidates the owner's layout, so a designer edit (add / remove / reorder) repaints
''' by itself instead of waiting for a resize.
'''
''' Key validation deliberately does NOT live here: the collection editor inserts an item the
''' moment you press «Add», long before you have typed anything into it. Validation is
''' <c>KBotNavList.EndInit</c> (hard, at runtime) plus the existing runtime methods.
''' </summary>
Public NotInheritable Class KBotNavItemCollection
    Inherits Collection(Of KBotNavItem)

    ''' <summary>The bar that owns this collection (Nothing for a free-floating instance).</summary>
    Friend Property Owner As KBotNavList

    Protected Overrides Sub InsertItem(index As Integer, item As KBotNavItem)
        If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
        MyBase.InsertItem(index, item)
        Owner?.InvalidateLayout()
    End Sub

    Protected Overrides Sub SetItem(index As Integer, item As KBotNavItem)
        If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
        MyBase.SetItem(index, item)
        Owner?.InvalidateLayout()
    End Sub

    Protected Overrides Sub RemoveItem(index As Integer)
        MyBase.RemoveItem(index)
        Owner?.InvalidateLayout()
    End Sub

    Protected Overrides Sub ClearItems()
        MyBase.ClearItems()
        Owner?.InvalidateLayout()
    End Sub

End Class
