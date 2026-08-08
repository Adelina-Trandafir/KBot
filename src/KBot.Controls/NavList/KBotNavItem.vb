Option Strict On
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Drawing
Imports KBot.Common

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

    ''' <summary>
    ''' English (slice 0025-02): the icon drawn at the LEFT of the caption — the equivalent of
    ''' <c>Button.Image</c>, and editable with the same stock image editor in the property grid
    ''' (the designer stores it in the form's <c>.resx</c>).
    '''
    ''' There is deliberately NO <c>ImageAlign</c>: a nav entry is a left-icon-then-caption row by
    ''' design, and an alignment nobody asked for is one more thing that can be set wrong.
    ''' Ignored when <see cref="IsSeparator"/> is True. The item does NOT own the image and never
    ''' disposes it — it belongs to the caller or to the form's resources, exactly like
    ''' <c>KBotCaptionBar.IconImage</c>.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Pictograma desenată la stânga textului (ca Image-ul unui buton). Ignorată pe separatori.")>
    Public Property Image As System.Drawing.Image

    ' English (slice 0025-03): without these two the designer writes «KBotNavItemN.Image = Nothing»
    ' for EVERY item that has no icon — four dead lines in DdfView, eight in MainForm, and it grows
    ' with the bar. A reference type has no usable <DefaultValue> in VB (the attribute takes a
    ' converted string, and Nothing is not one), so ShouldSerialize/Reset is how «unset» is said.
    ' Private on purpose: TypeDescriptor looks them up by name including non-public members, and
    ' they are not part of the item's API.
    Private Function ShouldSerializeImage() As Boolean
        Return Image IsNot Nothing
    End Function

    Private Sub ResetImage()
        Image = Nothing
    End Sub

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

    ''' <summary>
    ''' English (slice 0025-04): when True, THIS button sizes itself to its content — padding +
    ''' <see cref="Image"/> + the measured caption + the badge pill — and ignores the bar's
    ''' <c>KBotNavList.ItemWidth</c>. Default False, so a bar behaves exactly as before.
    '''
    ''' Precedence is: <c>AutoSize</c> on the item, then <c>ItemWidth</c> on the bar, then the
    ''' bar's own default (fill the width on a vertical bar, measure the content on a horizontal
    ''' one). On a vertical bar the button can never grow past the bar's usable width.
    '''
    ''' Ignored on a separator: it has no content to fit.
    ''' </summary>
    <Category("K-BOT")>
    <Description("True => butonul se redimensionează ca să încapă textul și pictograma, ignorând ItemWidth-ul barei.")>
    <DefaultValue(False)>
    Public Property AutoSize As Boolean

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

    ' English (slice 0025-03): the four mutators carry their own Try/Catch because they are ENTRY
    ' POINTS — the designer calls them from InitializeComponent and callers call them from code, so
    ' there is no already-wrapped boundary above them to log at (the house rule's transitive
    ' coverage does not apply). Classification is «boundary»: log and RE-THROW, never swallow — a
    ' bar that silently drops an item is the failure mode this whole slice exists to prevent.
    ' The log is skipped inside Visual Studio, like every other sink in these controls.
    Protected Overrides Sub InsertItem(index As Integer, item As KBotNavItem)
        Try
            If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
            MyBase.InsertItem(index, item)
            Owner?.InvalidateLayout()
        Catch ex As Exception
            LogUnlessDesignTime("KBotNavItemCollection.InsertItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub SetItem(index As Integer, item As KBotNavItem)
        Try
            If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
            MyBase.SetItem(index, item)
            Owner?.InvalidateLayout()
        Catch ex As Exception
            LogUnlessDesignTime("KBotNavItemCollection.SetItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub RemoveItem(index As Integer)
        Try
            MyBase.RemoveItem(index)
            Owner?.InvalidateLayout()
        Catch ex As Exception
            LogUnlessDesignTime("KBotNavItemCollection.RemoveItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub ClearItems()
        Try
            MyBase.ClearItems()
            Owner?.InvalidateLayout()
        Catch ex As Exception
            LogUnlessDesignTime("KBotNavItemCollection.ClearItems", ex)
            Throw
        End Try
    End Sub

    ' Writing a log file from inside devenv.exe is noise at best; see KBotDesignTime.
    Private Sub LogUnlessDesignTime(source As String, ex As Exception)
        If KBotDesignTime.IsDesignTime(Owner) Then Return
        GlobalErrorLog.Write(source, ex)
    End Sub

End Class
