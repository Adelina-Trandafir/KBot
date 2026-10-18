Option Strict On
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Drawing
Imports KBot.Common

''' <summary>
''' One button of a <see cref="KBotRibbon"/> group: a command with a key, a caption, an icon and a
''' tooltip. <see cref="Large"/> buttons stand alone (icon above the caption); small ones stack
''' three to a column. Authored in the designer through the stock collection editors
''' (Tabs -> Groups -> Buttons), like <see cref="KBotNavItem"/>.
''' </summary>
Public NotInheritable Class KBotRibbonButton

    ''' <summary>Parameterless constructor, required by the designer's collection editor.</summary>
    Public Sub New()
    End Sub

    ''' <summary>Convenience for code.</summary>
    Public Sub New(k_key As String, k_text As String, Optional k_image As Image = Nothing)
        ' «Me.» is mandatory: VB is case-insensitive and a same-named parameter would shadow the property.
        Me.Key = k_key
        Me.Text = If(k_text, String.Empty)
        Me.Image = k_image
    End Sub

    <Category("K-BOT")>
    <Description("Identifier raised with ItemClicked. Non-empty and unique in the whole ribbon.")>
    Public Property Key As String

    <Category("K-BOT")>
    <Description("The caption of the button.")>
    <DefaultValue("")>
    Public Property Text As String = String.Empty

    <Category("K-BOT")>
    <Description("The icon (drawn at 32 px on a large button, 16 px on a small one). Not owned by the button.")>
    Public Property Image As Image

    Private Function ShouldSerializeImage() As Boolean
        Return Image IsNot Nothing
    End Function

    Private Sub ResetImage()
        Image = Nothing
    End Sub

    <Category("K-BOT")>
    <Description("True = icon above the caption, standing alone. False = small: icon at the left, three to a column.")>
    <DefaultValue(True)>
    Public Property Large As Boolean = True

    <Category("K-BOT")>
    <Description("False = drawn dimmed and does not react.")>
    <DefaultValue(True)>
    Public Property Enabled As Boolean = True

    <Category("K-BOT")>
    <Description("False = takes no room and is not drawn.")>
    <DefaultValue(True)>
    Public Property Visible As Boolean = True

    <Category("K-BOT")>
    <Description("Tooltip title.")>
    <DefaultValue("")>
    Public Property ToolTipHeader As String = String.Empty

    <Category("K-BOT")>
    <Description("Tooltip body.")>
    <DefaultValue("")>
    Public Property ToolTipText As String = String.Empty

    <Category("K-BOT")>
    <Description("Caption colour. Empty = the theme's.")>
    Public Property ForeColor As Color = Color.Empty

    Private Function ShouldSerializeForeColor() As Boolean
        Return Not ForeColor.IsEmpty
    End Function

    Private Sub ResetForeColor()
        ForeColor = Color.Empty
    End Sub

    <Category("K-BOT")>
    <Description("Pinned buttons only: draws a thin separator line to the left of this button, while another pinned button is visible to its left. Alone, the line is not drawn.")>
    <DefaultValue(False)>
    Public Property SeparatorBefore As Boolean

    <Category("K-BOT")>
    <Description("Large button of a group: draws a thin separator line to the right of this button (the next buttons of the group follow after it). Alone at the end of the group, the line is not drawn. Slice KE12.")>
    <DefaultValue(False)>
    Public Property SeparatorAfter As Boolean

    ''' <summary>The slot computed by the last layout pass (Empty when hidden). Derived state: never serialized.</summary>
    Friend Property Bounds As Rectangle

    ''' <summary>X of the separator line drawn left of a pinned button by the last layout pass (-1 = none). Derived state.</summary>
    Friend Property SeparatorX As Integer = -1

    ''' <summary>True for a pinned Large button: icon and text are centred in the whole height of the panel. Derived state.</summary>
    Friend Property CenterV As Boolean

    ''' <summary>True while a Large button is drawn small (a pinned one, in the tab strip of a folded ribbon). Derived state.</summary>
    Friend Property DrawSmall As Boolean

    Public Overrides Function ToString() As String
        Return If(String.IsNullOrWhiteSpace(Key), "<no key>", Key) & " - """ & If(Text, String.Empty) & """" &
               If(Large, String.Empty, " (small)")
    End Function

End Class

''' <summary>What a <see cref="KBotRibbonField"/> is (slice 0114-01).</summary>
Public Enum KBotRibbonFieldKind
    ''' <summary>A list to pick from; the operator cannot type in it (KBotComboBox, not editable).</summary>
    Combo = 0

    ''' <summary>A one-line text box (KBotTextBox).</summary>
    Text = 1
End Enum

''' <summary>
''' A small input that stands in a group next to its buttons (slice 0114-01): a combo to pick from, or a text box. Authored
''' in the designer (Group -> Fields); the ribbon creates the real KBot control, places it in the group's panel and hides it with
''' the group. Small: it takes one row of the three of a column, like a small button. Its value is read from code
''' (<see cref="Text"/>, <see cref="SelectedIndex"/>) when a command needs it; there is no event.
''' </summary>
Public NotInheritable Class KBotRibbonField

    Private _text As String = String.Empty
    Private _selectedIndex As Integer
    Private ReadOnly _items As New Collection(Of String)()

    Public Sub New()
    End Sub

    <Category("K-BOT")>
    <Description("Identifier of the field. Non-empty and unique in the whole ribbon (also against the buttons).")>
    Public Property Key As String

    <Category("K-BOT")>
    <Description("Combo = pick from Items, no typing. Text = a text box.")>
    <DefaultValue(KBotRibbonFieldKind.Combo)>
    Public Property Kind As KBotRibbonFieldKind = KBotRibbonFieldKind.Combo

    <Category("K-BOT")>
    <Description("The rows of a Combo field.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property Items As Collection(Of String)
        Get
            Return _items
        End Get
    End Property

    <Category("K-BOT")>
    <Description("Width in pixels at 96 dpi (scaled with the screen).")>
    <DefaultValue(130)>
    Public Property Width As Integer = 130

    <Category("K-BOT")>
    <Description("False = takes no room and is not drawn. Call KBotRibbon.InvalidateLayout after changing it from code.")>
    <DefaultValue(True)>
    Public Property Visible As Boolean = True

    <Category("K-BOT")>
    <Description("False = dimmed and does not react.")>
    <DefaultValue(True)>
    Public Property Enabled As Boolean = True

    <Category("K-BOT")>
    <Description("Text kind: the grey hint shown while the box is empty. Combo kind: the pale text shown while no row is picked.")>
    <DefaultValue("")>
    Public Property PlaceholderText As String = String.Empty

    <Category("K-BOT")>
    <Description("Text kind: the longest text; 0 = no limit.")>
    <DefaultValue(0)>
    Public Property MaxLength As Integer

    <Category("K-BOT")>
    <Description("Text kind: only the digits 0-9 are kept (anything else typed or pasted is dropped).")>
    <DefaultValue(False)>
    Public Property DigitsOnly As Boolean

    <Category("K-BOT")>
    <Description("Tooltip title.")>
    <DefaultValue("")>
    Public Property ToolTipHeader As String = String.Empty

    <Category("K-BOT")>
    <Description("Tooltip body.")>
    <DefaultValue("")>
    Public Property ToolTipText As String = String.Empty

    ''' <summary>The row picked in a Combo field (-1 = none). Runtime state, never serialized.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectedIndex As Integer
        Get
            Dim k_combo As KBot.Controls.KBotComboBox = TryCast(Control, KBot.Controls.KBotComboBox)
            If k_combo IsNot Nothing Then Return k_combo.SelectedIndex
            Return _selectedIndex
        End Get
        Set(k_value As Integer)
            _selectedIndex = k_value
            Dim k_combo As KBot.Controls.KBotComboBox = TryCast(Control, KBot.Controls.KBotComboBox)
            If k_combo IsNot Nothing Then k_combo.SelectedIndex = k_value
        End Set
    End Property

    ''' <summary>The text of a Text field (what the operator typed; empty = nothing). Runtime state, never serialized.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Text As String
        Get
            Dim k_box As KBot.Controls.KBotTextBox = TryCast(Control, KBot.Controls.KBotTextBox)
            If k_box IsNot Nothing Then Return k_box.Text
            Return _text
        End Get
        Set(k_value As String)
            _text = If(k_value, String.Empty)
            Dim k_box As KBot.Controls.KBotTextBox = TryCast(Control, KBot.Controls.KBotTextBox)
            If k_box IsNot Nothing Then k_box.Text = _text
        End Set
    End Property

    ''' <summary>The real control the ribbon made for this field (Nothing at design time). Derived state.</summary>
    Friend Property Control As System.Windows.Forms.Control

    ''' <summary>The slot computed by the last layout pass (Empty when hidden). Derived state.</summary>
    Friend Property Bounds As Rectangle

    ''' <summary>The value kept while there is no control yet (applied when the ribbon creates it).</summary>
    Friend ReadOnly Property PendingText As String
        Get
            Return _text
        End Get
    End Property

    Friend ReadOnly Property PendingIndex As Integer
        Get
            Return _selectedIndex
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return If(String.IsNullOrWhiteSpace(Key), "<no key>", Key) & " (" & Kind.ToString() & ")"
    End Function

End Class

''' <summary>The fields of one group; the ribbon makes / drops their controls as they come and go.</summary>
Public NotInheritable Class KBotRibbonFieldCollection
    Inherits Collection(Of KBotRibbonField)

    Private _owner As KBotRibbon

    Friend Property Owner As KBotRibbon
        Get
            Return _owner
        End Get
        Set(k_value As KBotRibbon)
            _owner = k_value
            If k_value Is Nothing Then Return
            For Each k_field As KBotRibbonField In Me
                k_value.AttachField(k_field)
            Next
        End Set
    End Property

    Protected Overrides Sub InsertItem(k_index As Integer, k_item As KBotRibbonField)
        If k_item Is Nothing Then Throw New ArgumentNullException(NameOf(k_item))
        MyBase.InsertItem(k_index, k_item)
        _owner?.AttachField(k_item)
        _owner?.InvalidateLayout()
    End Sub

    Protected Overrides Sub RemoveItem(k_index As Integer)
        Dim k_field As KBotRibbonField = Me(k_index)
        MyBase.RemoveItem(k_index)
        _owner?.DetachField(k_field)
        _owner?.InvalidateLayout()
    End Sub

    Protected Overrides Sub ClearItems()
        Dim k_old As New List(Of KBotRibbonField)(Me)
        MyBase.ClearItems()
        For Each k_field As KBotRibbonField In k_old
            _owner?.DetachField(k_field)
        Next
        _owner?.InvalidateLayout()
    End Sub

    Protected Overrides Sub SetItem(k_index As Integer, k_item As KBotRibbonField)
        Throw New NotSupportedException("Replace a ribbon field by removing it and adding the new one.")
    End Sub
End Class

''' <summary>The buttons of one group.</summary>
Public NotInheritable Class KBotRibbonButtonCollection
    Inherits Collection(Of KBotRibbonButton)

    Friend Property Owner As KBotRibbon

    Protected Overrides Sub InsertItem(k_index As Integer, k_item As KBotRibbonButton)
        If k_item Is Nothing Then Throw New ArgumentNullException(NameOf(k_item))
        MyBase.InsertItem(k_index, k_item)
        Owner?.InvalidateLayout()
    End Sub

    Protected Overrides Sub SetItem(k_index As Integer, k_item As KBotRibbonButton)
        If k_item Is Nothing Then Throw New ArgumentNullException(NameOf(k_item))
        MyBase.SetItem(k_index, k_item)
        Owner?.InvalidateLayout()
    End Sub

    Protected Overrides Sub RemoveItem(k_index As Integer)
        MyBase.RemoveItem(k_index)
        Owner?.InvalidateLayout()
    End Sub

    Protected Overrides Sub ClearItems()
        MyBase.ClearItems()
        Owner?.InvalidateLayout()
    End Sub
End Class

''' <summary>A captioned set of buttons inside a tab.</summary>
Public NotInheritable Class KBotRibbonGroup

    Private ReadOnly _buttons As New KBotRibbonButtonCollection()
    Private ReadOnly _fields As New KBotRibbonFieldCollection()

    Public Sub New()
    End Sub

    Public Sub New(k_text As String)
        Me.Text = If(k_text, String.Empty)
    End Sub

    <Category("K-BOT")>
    <Description("The small inputs (combo / text box) of the group, placed in a column after its buttons (slice 0114-01).")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property Fields As KBotRibbonFieldCollection
        Get
            Return _fields
        End Get
    End Property

    <Category("K-BOT")>
    <Description("The caption under the group.")>
    <DefaultValue("")>
    Public Property Text As String = String.Empty

    <Category("K-BOT")>
    <Description("The buttons of the group.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property Buttons As KBotRibbonButtonCollection
        Get
            Return _buttons
        End Get
    End Property

    <Category("K-BOT")>
    <Description("False = the whole group is not shown.")>
    <DefaultValue(True)>
    Public Property Visible As Boolean = True

    Friend Property Bounds As Rectangle
    Friend Property CaptionBounds As Rectangle

    Friend Sub SetOwner(k_owner As KBotRibbon)
        _buttons.Owner = k_owner
        _fields.Owner = k_owner
    End Sub

    Public Overrides Function ToString() As String
        Return """" & If(Text, String.Empty) & """ (" & _buttons.Count.ToString(Globalization.CultureInfo.InvariantCulture) & ")"
    End Function
End Class

''' <summary>The groups of one tab.</summary>
Public NotInheritable Class KBotRibbonGroupCollection
    Inherits Collection(Of KBotRibbonGroup)

    Friend Property Owner As KBotRibbon

    Protected Overrides Sub InsertItem(k_index As Integer, k_item As KBotRibbonGroup)
        If k_item Is Nothing Then Throw New ArgumentNullException(NameOf(k_item))
        MyBase.InsertItem(k_index, k_item)
        k_item.SetOwner(Owner)
        Owner?.InvalidateLayout()
    End Sub

    Protected Overrides Sub SetItem(k_index As Integer, k_item As KBotRibbonGroup)
        If k_item Is Nothing Then Throw New ArgumentNullException(NameOf(k_item))
        MyBase.SetItem(k_index, k_item)
        k_item.SetOwner(Owner)
        Owner?.InvalidateLayout()
    End Sub

    Protected Overrides Sub RemoveItem(k_index As Integer)
        MyBase.RemoveItem(k_index)
        Owner?.InvalidateLayout()
    End Sub

    Protected Overrides Sub ClearItems()
        MyBase.ClearItems()
        Owner?.InvalidateLayout()
    End Sub

    Friend Sub SetOwner(k_owner As KBotRibbon)
        Owner = k_owner
        For Each k_group As KBotRibbonGroup In Me
            k_group.SetOwner(k_owner)
        Next
    End Sub
End Class

''' <summary>One tab of the ribbon: a name in the tab strip and the groups shown under it.</summary>
Public NotInheritable Class KBotRibbonTab

    Private ReadOnly _groups As New KBotRibbonGroupCollection()

    Public Sub New()
    End Sub

    Public Sub New(k_text As String)
        Me.Text = If(k_text, String.Empty)
    End Sub

    <Category("K-BOT")>
    <Description("The name in the tab strip.")>
    <DefaultValue("")>
    Public Property Text As String = String.Empty

    <Category("K-BOT")>
    <Description("The groups of the tab.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property Groups As KBotRibbonGroupCollection
        Get
            Return _groups
        End Get
    End Property

    <Category("K-BOT")>
    <Description("False = the tab is not shown.")>
    <DefaultValue(True)>
    Public Property Visible As Boolean = True

    Friend Property Bounds As Rectangle

    Friend Sub SetOwner(k_owner As KBotRibbon)
        _groups.SetOwner(k_owner)
    End Sub

    Public Overrides Function ToString() As String
        Return """" & If(Text, String.Empty) & """ (" & _groups.Count.ToString(Globalization.CultureInfo.InvariantCulture) & " groups)"
    End Function
End Class

''' <summary>The tabs of the ribbon.</summary>
Public NotInheritable Class KBotRibbonTabCollection
    Inherits Collection(Of KBotRibbonTab)

    Friend Property Owner As KBotRibbon

    Protected Overrides Sub InsertItem(k_index As Integer, k_item As KBotRibbonTab)
        If k_item Is Nothing Then Throw New ArgumentNullException(NameOf(k_item))
        MyBase.InsertItem(k_index, k_item)
        k_item.SetOwner(Owner)
        Owner?.InvalidateLayout()
    End Sub

    Protected Overrides Sub SetItem(k_index As Integer, k_item As KBotRibbonTab)
        If k_item Is Nothing Then Throw New ArgumentNullException(NameOf(k_item))
        MyBase.SetItem(k_index, k_item)
        k_item.SetOwner(Owner)
        Owner?.InvalidateLayout()
    End Sub

    Protected Overrides Sub RemoveItem(k_index As Integer)
        MyBase.RemoveItem(k_index)
        Owner?.InvalidateLayout()
    End Sub

    Protected Overrides Sub ClearItems()
        MyBase.ClearItems()
        Owner?.InvalidateLayout()
    End Sub
End Class

''' <summary>Raised when a ribbon button is clicked.</summary>
Public NotInheritable Class KBotRibbonItemClickedEventArgs
    Inherits EventArgs

    Public Sub New(k_button As KBotRibbonButton)
        Button = k_button
    End Sub

    Public ReadOnly Property Button As KBotRibbonButton

    Public ReadOnly Property Key As String
        Get
            Return Button.Key
        End Get
    End Property
End Class
