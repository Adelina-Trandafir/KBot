Option Strict On
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Drawing

''' <summary>How the rows of one <see cref="KBotSidePanelSection"/> are laid out.</summary>
Public Enum KBotSidePanelRowStyle
    ''' <summary>One line: the text at the left, the detail (a time, a count) dim at the right.</summary>
    Line = 0
    ''' <summary>Two lines: the detail small and dim on top (a caption), the text under it (the value).</summary>
    Stacked = 1
End Enum

''' <summary>One row of a <see cref="KBotSidePanelSection"/>. Filled from code at run time; authored in the designer only for fixed content.</summary>
Public NotInheritable Class KBotSidePanelRow

    Public Sub New()
    End Sub

    Public Sub New(k_text As String, Optional k_detail As String = "", Optional k_key As String = Nothing)
        ' «Me.» is mandatory: VB is case-insensitive and a same-named parameter would shadow the property.
        Me.Text = If(k_text, String.Empty)
        Me.DetailText = If(k_detail, String.Empty)
        Me.Key = k_key
    End Sub

    <Category("K-BOT")>
    <Description("Identifier raised with RowClicked. Empty = the row is not clickable.")>
    Public Property Key As String

    <Category("K-BOT")>
    <Description("The main text of the row.")>
    <DefaultValue("")>
    Public Property Text As String = String.Empty

    <Category("K-BOT")>
    <Description("The secondary text: a time at the right (Line style) or a caption on top (Stacked style).")>
    <DefaultValue("")>
    Public Property DetailText As String = String.Empty

    <Category("K-BOT")>
    <Description("Tooltip shown over the row.")>
    <DefaultValue("")>
    Public Property ToolTipText As String = String.Empty

    <Category("K-BOT")>
    <Description("Title of the tooltip shown over the row. Empty = none.")>
    <DefaultValue("")>
    Public Property ToolTipHeader As String = String.Empty

    ''' <summary>
    ''' Asked for each time the tooltip of the row is about to open: the picture shown under its text (Nothing = no picture).
    ''' The picture belongs to the caller; the tooltip never disposes it. Never serialized.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property ToolTipImageProvider As Func(Of Image)

    <Category("K-BOT")>
    <Description("Colour of the main text. Empty = the theme's.")>
    Public Property ForeColor As Color = Color.Empty

    Private Function ShouldSerializeForeColor() As Boolean
        Return Not ForeColor.IsEmpty
    End Function

    Private Sub ResetForeColor()
        ForeColor = Color.Empty
    End Sub

    <Category("K-BOT")>
    <Description("True = the main text is written in italics (Line style).")>
    <DefaultValue(False)>
    Public Property Italic As Boolean

    ''' <summary>Whatever the caller wants back with the click (never serialized).</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Tag As Object

    ''' <summary>The slot computed by the last layout pass, in content coordinates. Derived state.</summary>
    Friend Property Bounds As Rectangle

    Public Overrides Function ToString() As String
        Return If(Text, String.Empty)
    End Function
End Class

''' <summary>The rows of one section.</summary>
Public NotInheritable Class KBotSidePanelRowCollection
    Inherits Collection(Of KBotSidePanelRow)

    Friend Property Owner As KBotSidePanel

    Protected Overrides Sub InsertItem(k_index As Integer, k_item As KBotSidePanelRow)
        If k_item Is Nothing Then Throw New ArgumentNullException(NameOf(k_item))
        MyBase.InsertItem(k_index, k_item)
        Owner?.InvalidateLayout()
    End Sub

    Protected Overrides Sub SetItem(k_index As Integer, k_item As KBotSidePanelRow)
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

''' <summary>A titled, foldable list of rows inside a <see cref="KBotSidePanel"/>.</summary>
Public NotInheritable Class KBotSidePanelSection

    Private ReadOnly _rows As New KBotSidePanelRowCollection()
    Private _owner As KBotSidePanel
    Private _collapsed As Boolean

    Public Sub New()
    End Sub

    Public Sub New(k_key As String, k_title As String)
        Me.Key = k_key
        Me.Title = If(k_title, String.Empty)
    End Sub

    <Category("K-BOT")>
    <Description("Identifier of the section; unique in the panel.")>
    Public Property Key As String

    <Category("K-BOT")>
    <Description("The title in the header (shown in capitals).")>
    <DefaultValue("")>
    Public Property Title As String = String.Empty

    <Category("K-BOT")>
    <Description("The icon in the header, and the one left in the narrow strip when the panel is folded. Not owned by the section.")>
    Public Property Icon As Image

    Private Function ShouldSerializeIcon() As Boolean
        Return Icon IsNot Nothing
    End Function

    Private Sub ResetIcon()
        Icon = Nothing
    End Sub

    <Category("K-BOT")>
    <Description("True = only the header is shown.")>
    <DefaultValue(False)>
    Public Property Collapsed As Boolean
        Get
            Return _collapsed
        End Get
        Set(k_value As Boolean)
            If k_value = _collapsed Then Return
            _collapsed = k_value
            _owner?.InvalidateLayout()
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Line = text and a dim detail on one line; Stacked = a caption over the value.")>
    <DefaultValue(KBotSidePanelRowStyle.Line)>
    Public Property RowStyle As KBotSidePanelRowStyle = KBotSidePanelRowStyle.Line

    <Category("K-BOT")>
    <Description("Shown, dim, when the section has no rows.")>
    <DefaultValue("")>
    Public Property EmptyText As String = String.Empty

    <Category("K-BOT")>
    <Description("False = the section is not shown.")>
    <DefaultValue(True)>
    Public Property Visible As Boolean = True

    <Category("K-BOT")>
    <Description("The rows of the section.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property Rows As KBotSidePanelRowCollection
        Get
            Return _rows
        End Get
    End Property

    Friend Property HeaderBounds As Rectangle

    Friend Sub SetOwner(k_owner As KBotSidePanel)
        _owner = k_owner
        _rows.Owner = k_owner
    End Sub

    Public Overrides Function ToString() As String
        Return If(String.IsNullOrWhiteSpace(Key), "<no key>", Key) & " - """ & If(Title, String.Empty) & """"
    End Function
End Class

''' <summary>The sections of the panel.</summary>
Public NotInheritable Class KBotSidePanelSectionCollection
    Inherits Collection(Of KBotSidePanelSection)

    Friend Property Owner As KBotSidePanel

    Protected Overrides Sub InsertItem(k_index As Integer, k_item As KBotSidePanelSection)
        If k_item Is Nothing Then Throw New ArgumentNullException(NameOf(k_item))
        MyBase.InsertItem(k_index, k_item)
        k_item.SetOwner(Owner)
        Owner?.InvalidateLayout()
    End Sub

    Protected Overrides Sub SetItem(k_index As Integer, k_item As KBotSidePanelSection)
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

''' <summary>Raised when a clickable row of a <see cref="KBotSidePanel"/> is clicked.</summary>
Public NotInheritable Class KBotSidePanelRowClickedEventArgs
    Inherits EventArgs

    Public Sub New(k_section As KBotSidePanelSection, k_row As KBotSidePanelRow)
        Section = k_section
        Row = k_row
    End Sub

    Public ReadOnly Property Section As KBotSidePanelSection
    Public ReadOnly Property Row As KBotSidePanelRow
End Class
