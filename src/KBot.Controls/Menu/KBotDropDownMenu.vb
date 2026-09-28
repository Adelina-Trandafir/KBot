Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' The K-BOT drop-down menu (slice 0087): the classic Windows 10 desktop menu look -- a coloured
''' bar down the whole left side that holds the row icons, rows with formatted text, separators
''' that start after the bar, and cascading submenus. A component: dropped on a form it sits in the
''' designer tray, its rows are edited in the property grid (<see cref="Items"/>, nested for
''' submenus), and <see cref="DropDownButton"/> ties it to the button that opens it.
'''
''' <para><b>Why not <see cref="CustomPopup"/>.</b> CustomPopup is a flat, activating context menu
''' built in code, used in many places; submenus need several windows open at once, which only
''' works when none of them takes the focus. This menu's windows never activate
''' (<c>WS_EX_NOACTIVATE</c>): the form underneath keeps its active title bar, and the keyboard is
''' read by a message filter installed while the menu is open.</para>
'''
''' <para>Colours follow the house contract: <c>Color.Empty</c> = from the active theme, an
''' explicit colour wins. Sizes are logical pixels at 96 dpi, scaled when the menu opens.</para>
''' </summary>
<ToolboxItem(True)>
<DesignerCategory("Code")>
<DefaultEvent("ItemClicked")>
<DefaultProperty("Items")>
Public Class KBotDropDownMenu
    Inherits Component

    Private ReadOnly _items As New KBotMenuItemCollection()
    Private _dropDownButton As Control
    Private _font As Font
    Private _itemHeight As Integer = 30
    Private _imageSize As Integer = 20
    Private _iconBarWidth As Integer = 38
    Private _minimumWidth As Integer = 200
    Private _maximumWidth As Integer = 480
    Private _submenuDelay As Integer = 300
    Private _backColor As Color = Color.Empty
    Private _iconBarColor As Color = Color.Empty
    Private _foreColor As Color = Color.Empty
    Private _borderColor As Color = Color.Empty
    Private _highlightBackColor As Color = Color.Empty
    Private _highlightBorderColor As Color = Color.Empty
    Private _disabledForeColor As Color = Color.Empty
    Private _separatorColor As Color = Color.Empty

    ' Open state: the chain of windows, root first. Empty = closed.
    Private ReadOnly _windows As New List(Of KBotMenuWindow)()
    Private _filter As MenuMessageFilter
    Private _hostForm As Form
    Private _anchor As Control

    ''' <summary>A command row was clicked or chosen with Enter. The menu is already closed.</summary>
    Public Event ItemClicked As EventHandler(Of KBotMenuItemClickedEventArgs)

    ''' <summary>Raised before the menu opens; set Cancel to keep it closed (e.g. to disable rows first).</summary>
    Public Event Opening As CancelEventHandler

    ''' <summary>Raised after the last menu window closed, whatever closed it.</summary>
    Public Event Closed As EventHandler

    Public Sub New()
    End Sub

    Public Sub New(container As IContainer)
        Me.New()
        container?.Add(Me)
    End Sub

    ' ── Rows ────────────────────────────────────────────────────────────────────

    <Category("K-BOT")>
    <Description("Menu rows. A row with its own Items opens a submenu.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property Items As KBotMenuItemCollection
        Get
            Return _items
        End Get
    End Property

    ''' <summary>
    ''' The button that opens the menu under itself on click; a click on it while the menu is open
    ''' closes it. Nothing = the host calls <see cref="ShowBelow"/> / <see cref="ShowAt"/> itself.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Button that opens the menu below itself when clicked. Empty = open it from code.")>
    <DefaultValue(GetType(Control), Nothing)>
    Public Property DropDownButton As Control
        Get
            Return _dropDownButton
        End Get
        Set(value As Control)
            If value Is _dropDownButton Then Return
            If _dropDownButton IsNot Nothing Then RemoveHandler _dropDownButton.Click, AddressOf DropDownButton_Click
            _dropDownButton = value
            If _dropDownButton IsNot Nothing AndAlso Not IsInDesigner() Then
                AddHandler _dropDownButton.Click, AddressOf DropDownButton_Click
            End If
        End Set
    End Property

    ' ── Look ────────────────────────────────────────────────────────────────────

    <Category("K-BOT: Look")>
    <Description("Base font of the rows. Empty = the font of the form that opens the menu.")>
    Public Property Font As Font
        Get
            Return _font
        End Get
        Set(value As Font)
            _font = value
        End Set
    End Property

    Private Function ShouldSerializeFont() As Boolean
        Return _font IsNot Nothing
    End Function

    Private Sub ResetFont()
        _font = Nothing
    End Sub

    <Category("K-BOT: Look")>
    <Description("Default row height in logical pixels (96 dpi); a row's own Height wins.")>
    <DefaultValue(30)>
    Public Property ItemHeight As Integer
        Get
            Return _itemHeight
        End Get
        Set(value As Integer)
            _itemHeight = Math.Max(16, Math.Min(96, value))
        End Set
    End Property

    <Category("K-BOT: Look")>
    <Description("Side of the row icons in logical pixels.")>
    <DefaultValue(20)>
    Public Property ImageSize As Integer
        Get
            Return _imageSize
        End Get
        Set(value As Integer)
            _imageSize = Math.Max(8, Math.Min(64, value))
        End Set
    End Property

    <Category("K-BOT: Look")>
    <Description("Width of the icon bar down the left side, in logical pixels.")>
    <DefaultValue(38)>
    Public Property IconBarWidth As Integer
        Get
            Return _iconBarWidth
        End Get
        Set(value As Integer)
            _iconBarWidth = Math.Max(16, Math.Min(96, value))
        End Set
    End Property

    <Category("K-BOT: Look")>
    <Description("Minimum width of a menu window, in logical pixels.")>
    <DefaultValue(200)>
    Public Property MinimumWidth As Integer
        Get
            Return _minimumWidth
        End Get
        Set(value As Integer)
            _minimumWidth = Math.Max(60, value)
        End Set
    End Property

    <Category("K-BOT: Look")>
    <Description("Maximum width of a menu window, in logical pixels; longer text is cut with an ellipsis.")>
    <DefaultValue(480)>
    Public Property MaximumWidth As Integer
        Get
            Return _maximumWidth
        End Get
        Set(value As Integer)
            _maximumWidth = Math.Max(_minimumWidth, value)
        End Set
    End Property

    <Category("K-BOT: Behaviour")>
    <Description("Milliseconds the mouse rests on a submenu row before the submenu opens.")>
    <DefaultValue(300)>
    Public Property SubmenuDelay As Integer
        Get
            Return _submenuDelay
        End Get
        Set(value As Integer)
            _submenuDelay = Math.Max(0, Math.Min(2000, value))
        End Set
    End Property

    <Category("K-BOT: Colours")>
    <Description("Background of the rows. Empty = from the theme.")>
    Public Property BackColor As Color
        Get
            Return _backColor
        End Get
        Set(value As Color)
            _backColor = value
        End Set
    End Property

    <Category("K-BOT: Colours")>
    <Description("Colour of the icon bar down the left side. Empty = from the theme.")>
    Public Property IconBarColor As Color
        Get
            Return _iconBarColor
        End Get
        Set(value As Color)
            _iconBarColor = value
        End Set
    End Property

    <Category("K-BOT: Colours")>
    <Description("Row text colour. Empty = from the theme.")>
    Public Property ForeColor As Color
        Get
            Return _foreColor
        End Get
        Set(value As Color)
            _foreColor = value
        End Set
    End Property

    <Category("K-BOT: Colours")>
    <Description("Window outline. Empty = from the theme.")>
    Public Property BorderColor As Color
        Get
            Return _borderColor
        End Get
        Set(value As Color)
            _borderColor = value
        End Set
    End Property

    <Category("K-BOT: Colours")>
    <Description("Fill of the highlighted row. Empty = from the theme accent.")>
    Public Property HighlightBackColor As Color
        Get
            Return _highlightBackColor
        End Get
        Set(value As Color)
            _highlightBackColor = value
        End Set
    End Property

    <Category("K-BOT: Colours")>
    <Description("Outline of the highlighted row. Empty = from the theme accent.")>
    Public Property HighlightBorderColor As Color
        Get
            Return _highlightBorderColor
        End Get
        Set(value As Color)
            _highlightBorderColor = value
        End Set
    End Property

    <Category("K-BOT: Colours")>
    <Description("Text colour of disabled rows. Empty = from the theme.")>
    Public Property DisabledForeColor As Color
        Get
            Return _disabledForeColor
        End Get
        Set(value As Color)
            _disabledForeColor = value
        End Set
    End Property

    <Category("K-BOT: Colours")>
    <Description("Separator line colour. Empty = from the theme.")>
    Public Property SeparatorColor As Color
        Get
            Return _separatorColor
        End Get
        Set(value As Color)
            _separatorColor = value
        End Set
    End Property

    Private Function ShouldSerializeBackColor() As Boolean
        Return _backColor <> Color.Empty
    End Function
    Private Sub ResetBackColor()
        _backColor = Color.Empty
    End Sub
    Private Function ShouldSerializeIconBarColor() As Boolean
        Return _iconBarColor <> Color.Empty
    End Function
    Private Sub ResetIconBarColor()
        _iconBarColor = Color.Empty
    End Sub
    Private Function ShouldSerializeForeColor() As Boolean
        Return _foreColor <> Color.Empty
    End Function
    Private Sub ResetForeColor()
        _foreColor = Color.Empty
    End Sub
    Private Function ShouldSerializeBorderColor() As Boolean
        Return _borderColor <> Color.Empty
    End Function
    Private Sub ResetBorderColor()
        _borderColor = Color.Empty
    End Sub
    Private Function ShouldSerializeHighlightBackColor() As Boolean
        Return _highlightBackColor <> Color.Empty
    End Function
    Private Sub ResetHighlightBackColor()
        _highlightBackColor = Color.Empty
    End Sub
    Private Function ShouldSerializeHighlightBorderColor() As Boolean
        Return _highlightBorderColor <> Color.Empty
    End Function
    Private Sub ResetHighlightBorderColor()
        _highlightBorderColor = Color.Empty
    End Sub
    Private Function ShouldSerializeDisabledForeColor() As Boolean
        Return _disabledForeColor <> Color.Empty
    End Function
    Private Sub ResetDisabledForeColor()
        _disabledForeColor = Color.Empty
    End Sub
    Private Function ShouldSerializeSeparatorColor() As Boolean
        Return _separatorColor <> Color.Empty
    End Function
    Private Sub ResetSeparatorColor()
        _separatorColor = Color.Empty
    End Sub

    ''' <summary>The colours actually painted, resolved against the active theme when the menu opens.</summary>
    Friend Function ResolveColors() As KBotMenuColors
        Dim p As ThemePalette = ThemeManager.Current.Palette
        Dim back As Color = If(_backColor.IsEmpty, p.SurfaceAltColor, _backColor)
        Return New KBotMenuColors() With {
            .Back = back,
            .IconBar = If(_iconBarColor.IsEmpty, ThemeShapes.Blend(p.SurfaceColor, p.BorderColor, 0.18), _iconBarColor),
            .Fore = If(_foreColor.IsEmpty, p.TextColor, _foreColor),
            .Border = If(_borderColor.IsEmpty, p.BorderColor, _borderColor),
            .HighlightBack = If(_highlightBackColor.IsEmpty, ThemeShapes.Blend(p.AccentColor, back, 0.8), _highlightBackColor),
            .HighlightBorder = If(_highlightBorderColor.IsEmpty, ThemeShapes.Blend(p.AccentColor, back, 0.35), _highlightBorderColor),
            .Disabled = If(_disabledForeColor.IsEmpty, p.DisabledTextColor, _disabledForeColor),
            .Separator = If(_separatorColor.IsEmpty, ThemeShapes.Blend(p.BorderColor, back, 0.3), _separatorColor)}
    End Function

    ' ── Open / close ────────────────────────────────────────────────────────────

    ''' <summary>True while at least one menu window is open.</summary>
    <Browsable(False)>
    Public ReadOnly Property IsOpen As Boolean
        Get
            Return _windows.Count > 0
        End Get
    End Property

    ''' <summary>Opens the menu under <paramref name="anchor"/> (flipped above when there is no room below).</summary>
    Public Sub ShowBelow(anchor As Control)
        Try
            ArgumentNullException.ThrowIfNull(anchor)
            Dim screenRect As Rectangle = anchor.RectangleToScreen(anchor.ClientRectangle)
            ShowCore(anchor, New Point(screenRect.Left, screenRect.Bottom), screenRect.Top)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDropDownMenu.ShowBelow", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Opens the menu with its top-left corner at a screen point.</summary>
    Public Sub ShowAt(anchor As Control, screenPoint As Point)
        Try
            ArgumentNullException.ThrowIfNull(anchor)
            ShowCore(anchor, screenPoint, screenPoint.Y)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDropDownMenu.ShowAt", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Closes every open menu window. Nothing happens when the menu is closed.</summary>
    Public Sub Close()
        Try
            CloseFrom(0)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDropDownMenu.Close", ex)
            Throw
        End Try
    End Sub

    Private Sub ShowCore(anchor As Control, topLeft As Point, flipBottom As Integer)
        If IsOpen Then CloseFrom(0)
        Dim cancel As New CancelEventArgs()
        RaiseEvent Opening(Me, cancel)
        If cancel.Cancel Then Return
        ValidateItems()
        If Not _items.Any(Function(i) i.Visible) Then
            Throw New InvalidOperationException("The menu has no visible rows.")
        End If

        _anchor = anchor
        _hostForm = anchor.FindForm()
        Dim root As New KBotMenuWindow(Me, _items, Nothing, anchor)
        _windows.Add(root)
        root.ShowAt(topLeft, flipBottom, flipsLeft:=False)

        _filter = New MenuMessageFilter(Me)
        Application.AddMessageFilter(_filter)
        If _hostForm IsNot Nothing Then AddHandler _hostForm.Deactivate, AddressOf HostForm_Deactivate
        TryCast(anchor, IPopupAnchor)?.SetPopupOpen(True)
    End Sub

    ''' <summary>
    ''' Every command row (not separator) needs a non-empty key, unique across the whole tree --
    ''' checked when the menu opens, because the collection editor adds rows before keys are typed.
    ''' </summary>
    Friend Sub ValidateItems()
        Dim seen As New HashSet(Of String)(StringComparer.Ordinal)
        ValidateLevel(_items, seen)
    End Sub

    Private Shared Sub ValidateLevel(items As KBotMenuItemCollection, seen As HashSet(Of String))
        For i As Integer = 0 To items.Count - 1
            Dim it As KBotMenuItem = items(i)
            If it.IsSeparator Then Continue For
            If String.IsNullOrWhiteSpace(it.Key) Then
                Throw New ArgumentException($"Menu row {i} («{it.Text}») has no key.", NameOf(Items))
            End If
            If Not seen.Add(it.Key) Then
                Throw New ArgumentException($"Duplicate menu key '{it.Key}'.", NameOf(Items))
            End If
            ValidateLevel(it.Items, seen)
        Next
    End Sub

    ' ── Called by the windows ───────────────────────────────────────────────────

    ''' <summary>The windows open right now, root first.</summary>
    Friend ReadOnly Property OpenWindows As IReadOnlyList(Of KBotMenuWindow)
        Get
            Return _windows
        End Get
    End Property

    ''' <summary>Opens the submenu of <paramref name="row"/> next to <paramref name="parent"/>.</summary>
    Friend Function OpenSubmenu(parent As KBotMenuWindow, row As KBotMenuItem, rowScreenRect As Rectangle) As KBotMenuWindow
        Dim level As Integer = _windows.IndexOf(parent)
        If level < 0 Then Return Nothing
        CloseFrom(level + 1)
        Dim child As New KBotMenuWindow(Me, row.Items, parent, _anchor)
        _windows.Add(child)
        child.ShowNextTo(rowScreenRect)
        Return child
    End Function

    ''' <summary>Closes the windows from <paramref name="level"/> down (0 = everything).</summary>
    Friend Sub CloseFrom(level As Integer)
        If level < 0 Then level = 0
        For i As Integer = _windows.Count - 1 To level Step -1
            Dim w As KBotMenuWindow = _windows(i)
            _windows.RemoveAt(i)
            w.CloseQuietly()
        Next
        If level = 0 Then TearDown()
    End Sub

    ''' <summary>A command row was chosen: close everything, then tell the host.</summary>
    Friend Sub Choose(item As KBotMenuItem)
        CloseFrom(0)
        RaiseEvent ItemClicked(Me, New KBotMenuItemClickedEventArgs(item))
    End Sub

    Private Sub TearDown()
        Dim wasOpen As Boolean = _filter IsNot Nothing
        If _filter IsNot Nothing Then
            Application.RemoveMessageFilter(_filter)
            _filter = Nothing
        End If
        If _hostForm IsNot Nothing Then
            RemoveHandler _hostForm.Deactivate, AddressOf HostForm_Deactivate
            _hostForm = Nothing
        End If
        If _anchor IsNot Nothing Then
            TryCast(_anchor, IPopupAnchor)?.SetPopupOpen(False)
            _anchor = Nothing
        End If
        If wasOpen Then RaiseEvent Closed(Me, EventArgs.Empty)
    End Sub

    ''' <summary>True when the handle belongs to one of the open menu windows.</summary>
    Friend Function OwnsWindow(hwnd As IntPtr) As Boolean
        For Each w As KBotMenuWindow In _windows
            If w.IsHandleCreated AndAlso w.Handle = hwnd Then Return True
        Next
        Return False
    End Function

    ''' <summary>True when the handle is the drop-down button or one of its children.</summary>
    Friend Function IsDropDownButton(hwnd As IntPtr) As Boolean
        If _dropDownButton Is Nothing OrElse Not _dropDownButton.IsHandleCreated Then Return False
        Dim c As Control = Control.FromChildHandle(hwnd)
        While c IsNot Nothing
            If c Is _dropDownButton Then Return True
            c = c.Parent
        End While
        Return False
    End Function

    ' ── Handlers (UI boundaries: log and swallow) ───────────────────────────────

    Private Sub DropDownButton_Click(sender As Object, e As EventArgs)
        Try
            If IsOpen Then
                CloseFrom(0)
                Return
            End If
            ShowBelow(_dropDownButton)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDropDownMenu.DropDownButton_Click", ex)
        End Try
    End Sub

    Private Sub HostForm_Deactivate(sender As Object, e As EventArgs)
        Try
            CloseFrom(0)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDropDownMenu.HostForm_Deactivate", ex)
        End Try
    End Sub

    Private Function IsInDesigner() As Boolean
        Return DesignMode OrElse LicenseManager.UsageMode = LicenseUsageMode.Designtime
    End Function

    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing Then
                CloseFrom(0)
                DropDownButton = Nothing
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

End Class

''' <summary>The colours a menu window paints with, resolved once per opening.</summary>
Friend NotInheritable Class KBotMenuColors
    Public Back As Color
    Public IconBar As Color
    Public Fore As Color
    Public Border As Color
    Public HighlightBack As Color
    Public HighlightBorder As Color
    Public Disabled As Color
    Public Separator As Color
End Class
