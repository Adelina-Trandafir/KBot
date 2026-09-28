Option Strict On
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Drawing
Imports KBot.Common

''' <summary>
''' One row of a <see cref="KBotDropDownMenu"/> (slice 0087): a command, a separator
''' (<see cref="IsSeparator"/>) or a submenu (a row whose <see cref="Items"/> is not empty).
''' Edited in the property grid through the stock collection editor, like
''' <see cref="KBotNavItem"/>; <see cref="Items"/> opens a nested editor for the submenu.
'''
''' <para><b>Text with format.</b> <see cref="Text"/> accepts the K-BOT rich-text markup
''' (<c>&lt;b&gt; &lt;i&gt; &lt;u&gt; &lt;color=#RRGGBB&gt; &lt;back=#RRGGBB&gt;</c>, see
''' <see cref="KBotRichText"/>); <see cref="Font"/> and <see cref="ForeColor"/> set the base look of
''' the row, Nothing / Empty = the menu's.</para>
''' </summary>
Public NotInheritable Class KBotMenuItem

    Private ReadOnly _items As New KBotMenuItemCollection()

    ''' <summary>Parameterless constructor, required by the designer's collection editor.</summary>
    Public Sub New()
    End Sub

    ''' <summary>Convenience for code: a command row.</summary>
    Public Sub New(key As String, text As String, Optional image As Image = Nothing)
        ' «Me.» is mandatory: VB is case-insensitive and the parameters shadow the properties.
        Me.Key = key
        Me.Text = If(text, String.Empty)
        Me.Image = image
    End Sub

    ''' <summary>A separator row.</summary>
    Public Shared Function Separator() As KBotMenuItem
        Return New KBotMenuItem() With {.IsSeparator = True}
    End Function

    <Category("K-BOT")>
    <Description("Identifier raised with ItemClicked. Non-empty and unique in the whole menu; ignored on separators.")>
    Public Property Key As String

    <Category("K-BOT")>
    <Description("Row text. Accepts <b> <i> <u> <color=#RRGGBB> <back=#RRGGBB> markup.")>
    <DefaultValue("")>
    Public Property Text As String = String.Empty

    <Category("K-BOT")>
    <Description("Dim text at the right end of the row (e.g. a shortcut). Not shown on submenu rows.")>
    <DefaultValue("")>
    Public Property ShortcutText As String = String.Empty

    <Category("K-BOT")>
    <Description("Icon drawn in the left bar. Not owned by the item.")>
    Public Property Image As Image

    Private Function ShouldSerializeImage() As Boolean
        Return Image IsNot Nothing
    End Function

    Private Sub ResetImage()
        Image = Nothing
    End Sub

    <Category("K-BOT")>
    <Description("Row height in logical pixels (96 dpi). 0 = the menu's ItemHeight.")>
    <DefaultValue(0)>
    Public Property Height As Integer

    <Category("K-BOT")>
    <Description("Base font of the row text. Empty = the menu's font.")>
    Public Property Font As Font

    Private Function ShouldSerializeFont() As Boolean
        Return Font IsNot Nothing
    End Function

    Private Sub ResetFont()
        Font = Nothing
    End Sub

    <Category("K-BOT")>
    <Description("Base colour of the row text. Empty = from the theme.")>
    Public Property ForeColor As Color = Color.Empty

    Private Function ShouldSerializeForeColor() As Boolean
        Return ForeColor <> Color.Empty
    End Function

    Private Sub ResetForeColor()
        ForeColor = Color.Empty
    End Sub

    <Category("K-BOT")>
    <Description("False = drawn dimmed, cannot be clicked or opened.")>
    <DefaultValue(True)>
    Public Property Enabled As Boolean = True

    <Category("K-BOT")>
    <Description("False = the row takes no space and is skipped by the keyboard.")>
    <DefaultValue(True)>
    Public Property Visible As Boolean = True

    <Category("K-BOT")>
    <Description("True = a thin line instead of a command; key, text and icon are ignored.")>
    <DefaultValue(False)>
    Public Property IsSeparator As Boolean

    ''' <summary>The submenu. Not empty = the row opens it instead of raising ItemClicked.</summary>
    <Category("K-BOT")>
    <Description("Submenu rows. When not empty the row opens a submenu instead of raising ItemClicked.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property Items As KBotMenuItemCollection
        Get
            Return _items
        End Get
    End Property

    ''' <summary>Anything the host wants to carry; never read by the menu.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Tag As Object

    ''' <summary>True when the row opens a submenu (it has at least one visible row).</summary>
    <Browsable(False)>
    Public ReadOnly Property HasSubmenu As Boolean
        Get
            If IsSeparator Then Return False
            For Each child As KBotMenuItem In _items
                If child.Visible Then Return True
            Next
            Return False
        End Get
    End Property

    ''' <summary>What the collection editor lists: separators must be recognisable at a glance.</summary>
    Public Overrides Function ToString() As String
        If IsSeparator Then Return "──────── separator ────────"
        Dim shownKey As String = If(String.IsNullOrWhiteSpace(Key), "<no key>", Key)
        Dim suffix As String = If(_items.Count > 0, $" ▸ ({_items.Count})", String.Empty)
        Return shownKey & " — """ & If(Text, String.Empty) & """" & suffix
    End Function

End Class

''' <summary>
''' The ordered rows of a menu or of a submenu. Validation (non-empty unique keys) is not done
''' here: the collection editor inserts a row before anything is typed into it. The menu checks
''' the whole tree when it opens (<see cref="KBotDropDownMenu.ValidateItems"/>).
''' </summary>
Public NotInheritable Class KBotMenuItemCollection
    Inherits Collection(Of KBotMenuItem)

    Protected Overrides Sub InsertItem(index As Integer, item As KBotMenuItem)
        Try
            ArgumentNullException.ThrowIfNull(item)
            MyBase.InsertItem(index, item)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotMenuItemCollection.InsertItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub SetItem(index As Integer, item As KBotMenuItem)
        Try
            ArgumentNullException.ThrowIfNull(item)
            MyBase.SetItem(index, item)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotMenuItemCollection.SetItem", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Adds several rows at once (the designer writes a list of <c>Add</c> calls).</summary>
    Public Sub AddRange(items As IEnumerable(Of KBotMenuItem))
        ArgumentNullException.ThrowIfNull(items)
        For Each it As KBotMenuItem In items
            Add(it)
        Next
    End Sub

    ''' <summary>The first row with this key, searched in this list and every submenu; Nothing if none.</summary>
    Public Function Find(key As String) As KBotMenuItem
        If String.IsNullOrEmpty(key) Then Return Nothing
        For Each it As KBotMenuItem In Me
            If Not it.IsSeparator AndAlso String.Equals(it.Key, key, StringComparison.Ordinal) Then Return it
            Dim inner As KBotMenuItem = it.Items.Find(key)
            If inner IsNot Nothing Then Return inner
        Next
        Return Nothing
    End Function

End Class

''' <summary>A command row of a <see cref="KBotDropDownMenu"/> was clicked (or chosen with Enter).</summary>
Public NotInheritable Class KBotMenuItemClickedEventArgs
    Inherits EventArgs

    Public Sub New(item As KBotMenuItem)
        ArgumentNullException.ThrowIfNull(item)
        Me.Item = item
    End Sub

    Public ReadOnly Property Item As KBotMenuItem

    Public ReadOnly Property Key As String
        Get
            Return Item.Key
        End Get
    End Property
End Class
