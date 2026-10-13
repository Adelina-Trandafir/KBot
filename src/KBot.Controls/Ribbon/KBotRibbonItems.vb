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

    ''' <summary>The slot computed by the last layout pass (Empty when hidden). Derived state: never serialized.</summary>
    Friend Property Bounds As Rectangle

    ''' <summary>True for a pinned Large button: icon and text are centred in the whole height of the panel. Derived state.</summary>
    Friend Property CenterV As Boolean

    ''' <summary>True while a Large button is drawn small (a pinned one, in the tab strip of a folded ribbon). Derived state.</summary>
    Friend Property DrawSmall As Boolean

    Public Overrides Function ToString() As String
        Return If(String.IsNullOrWhiteSpace(Key), "<no key>", Key) & " - """ & If(Text, String.Empty) & """" &
               If(Large, String.Empty, " (small)")
    End Function

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

    Public Sub New()
    End Sub

    Public Sub New(k_text As String)
        Me.Text = If(k_text, String.Empty)
    End Sub

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
