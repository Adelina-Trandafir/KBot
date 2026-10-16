Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' The ribbon: a tab strip over a panel of captioned groups of icon buttons, like Office's. It
''' replaces the single MENIU drop-down of the main window. The tabs, groups and buttons are drawn here; the only
''' child controls are the <see cref="KBotRibbonField"/> inputs (slice 0114-01), which are the KBot combo / text box,
''' placed in the group's panel and hidden with it. It themes through <see cref="IThemedControl"/>.
'''
''' <para><b>Model.</b> <see cref="Tabs"/> -> groups -> buttons, authored in the designer through
''' the stock collection editors (see <see cref="KBotRibbonButton"/>). A tab, group or button with
''' nothing visible under it is not shown, so a runtime <c>Visible = False</c> on the last button
''' of the ADMIN group removes the group, and then the tab.</para>
'''
''' <para><b>Height.</b> The control owns its height (tab strip, plus the panel unless
''' <see cref="Collapsed"/>), scaled like every other measure: <c>ThemeShapes.ScaleDpi</c> at layout
''' time, so nothing logical is ever stored in device pixels. A request to set another height is
''' overridden (<see cref="SetBoundsCore"/>).</para>
'''
''' <para><b>Collapsing.</b> Double click on a tab, or the chevron at the right, folds the ribbon
''' to its tab names; a click on a tab while it is folded unfolds it again. The owner remembers the
''' state through <see cref="CollapsedChanged"/>.</para>
'''
''' <para><b>Not done on purpose (first version):</b> keyboard navigation (Alt key tips, arrows),
''' drop-down buttons and a fly-out panel for the folded state.</para>
''' </summary>
<DefaultEvent("ItemClicked")>
Public NotInheritable Class KBotRibbon
    Inherits Control
    Implements IThemedControl

    ' Logical (96 dpi) measures.
    Private Const TabStripHeight As Integer = 28
    Private Const LargeIcon As Integer = 32
    Private Const SmallIcon As Integer = 16
    Private Const SmallRowHeight As Integer = 22
    Private Const GroupCaptionHeight As Integer = 18

    Private ReadOnly _tabs As New KBotRibbonTabCollection()
    Private ReadOnly _pinned As New KBotRibbonButtonCollection()
    Private ReadOnly _rightGroup As New KBotRibbonGroup()
    Private _selectedTab As Integer
    Private _collapsed As Boolean
    Private _rightText As String = String.Empty
    Private _tooltip As KBotToolTip

    Private _scheme As ThemeScheme
    Private _layoutValid As Boolean
    Private _layoutKey As Integer
    Private _heightGuard As Boolean

    Private _hotTab As KBotRibbonTab
    Private _hotButton As KBotRibbonButton
    Private _pressedButton As KBotRibbonButton
    Private _hotChevron As Boolean
    Private _chevronRect As Rectangle
    Private _rightTextRect As Rectangle
    Private _expandedByClickAt As Integer

    Private _boldFont As Font
    Private _captionFont As Font
    Private _rightFont As Font

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
        SetStyle(ControlStyles.Selectable, False)
        _tabs.Owner = Me
        _pinned.Owner = Me
        _rightGroup.SetOwner(Me)
    End Sub

    ' ---- Model ------------------------------------------------------------------------------

    <Category("K-BOT")>
    <Description("The tabs of the ribbon; each has groups, each group has buttons.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property Tabs As KBotRibbonTabCollection
        Get
            Return _tabs
        End Get
    End Property

    <Category("K-BOT")>
    <Description("Buttons pinned at the right end of the tab strip: always there, whatever tab is selected and even when the ribbon is folded. Small buttons.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property PinnedButtons As KBotRibbonButtonCollection
        Get
            Return _pinned
        End Get
    End Property

    <Category("K-BOT")>
    <Description("The group at the right end of the panel, outside the tabs and before the large pinned buttons (slice 0114-03): the same on every tab, shown only while it has a visible button or field, and not shown while the ribbon is folded. A host shows and hides its buttons / fields (and sets its caption) for the view on screen. Its right edge line is the separator before the pinned buttons.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    <TypeConverter(GetType(ExpandableObjectConverter))>
    Public ReadOnly Property RightGroup As KBotRibbonGroup
        Get
            Return _rightGroup
        End Get
    End Property

    <Category("K-BOT")>
    <Description("Index of the tab whose groups are shown.")>
    <DefaultValue(0)>
    Public Property SelectedTabIndex As Integer
        Get
            Return _selectedTab
        End Get
        Set(k_value As Integer)
            If k_value = _selectedTab Then Return
            _selectedTab = k_value
            InvalidateLayout()
        End Set
    End Property

    <Category("K-BOT")>
    <Description("True = folded to the tab names.")>
    <DefaultValue(False)>
    Public Property Collapsed As Boolean
        Get
            Return _collapsed
        End Get
        Set(k_value As Boolean)
            If k_value = _collapsed Then Return
            _collapsed = k_value
            InvalidateLayout()
            UpdateHeight()
            RaiseEvent CollapsedChanged(Me, EventArgs.Empty)
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Text at the right end of the tab strip (the operator's name).")>
    <DefaultValue("")>
    Public Property RightText As String
        Get
            Return _rightText
        End Get
        Set(k_value As String)
            _rightText = If(k_value, String.Empty)
            InvalidateLayout()
        End Set
    End Property

    <Category("K-BOT")>
    <Description("The tooltip component that shows the buttons' tooltips. Without it no tooltip is shown.")>
    Public Property Tooltip As KBotToolTip
        Get
            Return _tooltip
        End Get
        Set(k_value As KBotToolTip)
            _tooltip = k_value
            For Each k_group As KBotRibbonGroup In AllGroups()
                For Each k_field As KBotRibbonField In k_group.Fields
                    ApplyFieldTip(k_field)
                Next
            Next
        End Set
    End Property

    Private Function ShouldSerializeTooltip() As Boolean
        Return _tooltip IsNot Nothing
    End Function

    Private Sub ResetTooltip()
        _tooltip = Nothing
    End Sub

    <Category("K-BOT")>
    <Description("Raised when a button is clicked; the key is the button's.")>
    Public Event ItemClicked As EventHandler(Of KBotRibbonItemClickedEventArgs)

    <Category("K-BOT")>
    <Description("Raised when the ribbon is folded or unfolded.")>
    Public Event CollapsedChanged As EventHandler

    ''' <summary>The rectangle of the button with this key in SCREEN coordinates; Empty when it is not on show.</summary>
    Public Function ButtonScreenBounds(k_key As String) As Rectangle
        Dim k_button As KBotRibbonButton = FindButton(k_key)
        If k_button Is Nothing OrElse k_button.Bounds.IsEmpty Then Return Rectangle.Empty
        Return RectangleToScreen(k_button.Bounds)
    End Function

    ''' <summary>The button with this key anywhere in the ribbon; Nothing when there is none.</summary>
    Public Function FindButton(k_key As String) As KBotRibbonButton
        For Each k_group As KBotRibbonGroup In AllGroups()
            For Each k_button As KBotRibbonButton In k_group.Buttons
                If String.Equals(k_button.Key, k_key, StringComparison.Ordinal) Then Return k_button
            Next
        Next
        For Each k_button As KBotRibbonButton In _pinned
            If String.Equals(k_button.Key, k_key, StringComparison.Ordinal) Then Return k_button
        Next
        Return Nothing
    End Function

    ''' <summary>Every key must be non-empty and unique; throws otherwise (a clash would route a click to the wrong command).</summary>
    Public Sub ValidateKeys()
        Dim k_seen As New HashSet(Of String)(StringComparer.Ordinal)
        For Each k_group As KBotRibbonGroup In AllGroups()
            For Each k_button As KBotRibbonButton In k_group.Buttons
                If String.IsNullOrWhiteSpace(k_button.Key) Then
                    Throw New InvalidOperationException("A ribbon button has no key (" & k_button.Text & ").")
                End If
                If Not k_seen.Add(k_button.Key) Then
                    Throw New InvalidOperationException("Duplicate ribbon key '" & k_button.Key & "'.")
                End If
            Next
        Next
        For Each k_button As KBotRibbonButton In _pinned
            If String.IsNullOrWhiteSpace(k_button.Key) OrElse Not k_seen.Add(k_button.Key) Then
                Throw New InvalidOperationException("A pinned ribbon button has no key or a duplicate one (" & k_button.Text & ").")
            End If
        Next
        For Each k_group As KBotRibbonGroup In AllGroups()
            For Each k_field As KBotRibbonField In k_group.Fields
                If String.IsNullOrWhiteSpace(k_field.Key) OrElse Not k_seen.Add(k_field.Key) Then
                    Throw New InvalidOperationException("A ribbon field has no key or a duplicate one (" & k_field.ToString() & ").")
                End If
            Next
        Next
    End Sub

    ''' <summary>The field with this key anywhere in the ribbon; Nothing when there is none.</summary>
    Public Function FindField(k_key As String) As KBotRibbonField
        For Each k_group As KBotRibbonGroup In AllGroups()
            For Each k_field As KBotRibbonField In k_group.Fields
                If String.Equals(k_field.Key, k_key, StringComparison.Ordinal) Then Return k_field
            Next
        Next
        Return Nothing
    End Function

    ' Every group of the ribbon: those of every tab, then the right group (slice 0114-03).
    Private Iterator Function AllGroups() As IEnumerable(Of KBotRibbonGroup)
        For Each k_tab As KBotRibbonTab In _tabs
            For Each k_group As KBotRibbonGroup In k_tab.Groups
                Yield k_group
            Next
        Next
        Yield _rightGroup
    End Function

    ' ---- Fields (slice 0114-01) ---------------------------------------------------------------

    ''' <summary>Makes the real control of a field (not at design time: nothing is sited there) and puts it on the ribbon, hidden until placed.</summary>
    Friend Sub AttachField(k_field As KBotRibbonField)
        If k_field Is Nothing OrElse k_field.Control IsNot Nothing Then Return
        If KBotDesignTime.IsDesignTime(Me) Then Return
        Dim k_control As Control
        If k_field.Kind = KBotRibbonFieldKind.Text Then
            Dim k_box As New KBotTextBox() With {
                .PlaceholderText = k_field.PlaceholderText, .MaxLength = k_field.MaxLength, .Visible = False, .Text = k_field.PendingText}
            AddHandler k_box.TextChanged, Sub(k_s, k_e) FieldTextChanged(k_field, k_box)
            k_control = k_box
        Else
            Dim k_combo As New KBotComboBox() With {.Editable = False, .Visible = False}
            For Each k_row As String In k_field.Items
                k_combo.Items.Add(k_row)
            Next
            If k_field.PendingIndex >= 0 AndAlso k_field.PendingIndex < k_combo.Items.Count Then k_combo.SelectedIndex = k_field.PendingIndex
            k_control = k_combo
        End If
        k_control.Enabled = k_field.Enabled
        k_field.Control = k_control
        Controls.Add(k_control)
        Dim k_themed As IThemedControl = TryCast(k_control, IThemedControl)
        If k_themed IsNot Nothing AndAlso _scheme IsNot Nothing Then k_themed.ApplyTheme(_scheme)
        ApplyFieldTip(k_field)
        InvalidateLayout()
    End Sub

    Friend Sub DetachField(k_field As KBotRibbonField)
        If k_field Is Nothing OrElse k_field.Control Is Nothing Then Return
        Dim k_control As Control = k_field.Control
        k_field.Control = Nothing
        Controls.Remove(k_control)
        k_control.Dispose()
    End Sub

    ' DigitsOnly: what is not a digit is dropped as soon as it appears (typed or pasted).
    Private _fixingField As Boolean

    Private Sub FieldTextChanged(k_field As KBotRibbonField, k_box As KBotTextBox)
        Try
            If _fixingField OrElse Not k_field.DigitsOnly Then Return
            Dim k_clean As String = New String(k_box.Text.Where(Function(k_c) k_c >= "0"c AndAlso k_c <= "9"c).ToArray())
            If String.Equals(k_clean, k_box.Text, StringComparison.Ordinal) Then Return
            _fixingField = True
            Try
                k_box.Text = k_clean
            Finally
                _fixingField = False
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRibbon.FieldTextChanged", ex)
        End Try
    End Sub

    Private Sub ApplyFieldTip(k_field As KBotRibbonField)
        If _tooltip Is Nothing OrElse k_field.Control Is Nothing Then Return
        If String.IsNullOrEmpty(k_field.ToolTipText) AndAlso String.IsNullOrEmpty(k_field.ToolTipHeader) Then Return
        _tooltip.SetToolTipHeader(k_field.Control, k_field.ToolTipHeader)
        _tooltip.SetToolTipText(k_field.Control, k_field.ToolTipText)
    End Sub

    ''' <summary>Call after changing a button / group / tab from code (Visible, Text, Enabled, colour).</summary>
    Public Sub InvalidateLayout()
        _layoutValid = False
        If IsHandleCreated Then Invalidate()
    End Sub

    ' ---- Height -----------------------------------------------------------------------------

    Private Function Px(k_logical As Integer) As Integer
        Return ThemeShapes.ScaleDpi(Me, k_logical)
    End Function

    ' The panel holds a large button (icon, two lines of caption) or three small ones, plus the group caption;
    ' the caption lines are measured from the font, so a bigger text size or another scaling never clips them.
    Private Function PanelPx() As Integer
        Dim k_large As Integer = Px(4 + LargeIcon + 2 + 6) + 2 * Font.Height + Px(GroupCaptionHeight + 2)
        Dim k_small As Integer = Px(6 + 3 * SmallRowHeight + GroupCaptionHeight + 4)
        Return Math.Max(k_large, k_small)
    End Function

    Private Function DesiredHeight() As Integer
        Return Px(TabStripHeight) + If(_collapsed, 0, PanelPx()) + 1
    End Function

    Protected Overrides Sub SetBoundsCore(k_x As Integer, k_y As Integer, k_width As Integer, k_height As Integer, k_specified As BoundsSpecified)
        MyBase.SetBoundsCore(k_x, k_y, k_width, DesiredHeight(), k_specified)
    End Sub

    Private Sub UpdateHeight()
        If _heightGuard Then Return
        _heightGuard = True
        Try
            If Height <> DesiredHeight() Then SetBounds(Left, Top, Width, DesiredHeight(), BoundsSpecified.Height)
        Finally
            _heightGuard = False
        End Try
    End Sub

    Protected Overrides Sub OnHandleCreated(k_e As EventArgs)
        MyBase.OnHandleCreated(k_e)
        UpdateHeight()
    End Sub

    Protected Overrides Sub OnDpiChangedAfterParent(k_e As EventArgs)
        MyBase.OnDpiChangedAfterParent(k_e)
        InvalidateLayout()
        UpdateHeight()
    End Sub

    Protected Overrides Sub OnFontChanged(k_e As EventArgs)
        MyBase.OnFontChanged(k_e)
        _boldFont?.Dispose()
        _boldFont = Nothing
        _captionFont?.Dispose()
        _captionFont = Nothing
        _rightFont?.Dispose()
        _rightFont = Nothing
        InvalidateLayout()
    End Sub

    Protected Overrides Sub OnSizeChanged(k_e As EventArgs)
        MyBase.OnSizeChanged(k_e)
        _layoutValid = False
    End Sub

    Protected Overrides Sub Dispose(k_disposing As Boolean)
        If k_disposing Then
            _boldFont?.Dispose()
            _captionFont?.Dispose()
            _rightFont?.Dispose()
        End If
        MyBase.Dispose(k_disposing)
    End Sub

    ' ---- Theme ------------------------------------------------------------------------------

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        If k_scheme Is Nothing Then Return
        _scheme = k_scheme
        BackColor = k_scheme.Palette.SurfaceColor
        For Each k_group As KBotRibbonGroup In AllGroups()
            For Each k_field As KBotRibbonField In k_group.Fields
                Dim k_themed As IThemedControl = TryCast(k_field.Control, IThemedControl)
                If k_themed IsNot Nothing Then k_themed.ApplyTheme(k_scheme)
            Next
        Next
        InvalidateLayout()
        UpdateHeight()
    End Sub

    Private Function Palette() As ThemePalette
        Dim k_scheme As ThemeScheme = If(_scheme, ThemeManager.Current)
        Return k_scheme.Palette
    End Function

    Private Function CornerRadius() As Integer
        Dim k_scheme As ThemeScheme = If(_scheme, ThemeManager.Current)
        Return Math.Max(Px(3), Math.Min(Px(8), Px(k_scheme.Style.CornerRadius)))
    End Function

    Private Function BoldFont() As Font
        If _boldFont Is Nothing Then _boldFont = New Font(Font, FontStyle.Bold)
        Return _boldFont
    End Function

    Private Function CaptionFont() As Font
        If _captionFont Is Nothing Then _captionFont = New Font(Font.FontFamily, Math.Max(6.0F, Font.Size - 1.0F), FontStyle.Regular)
        Return _captionFont
    End Function

    Private Function RightFont() As Font
        If _rightFont Is Nothing Then _rightFont = New Font(Font, FontStyle.Bold Or FontStyle.Italic)
        Return _rightFont
    End Function

    ' ---- Layout -----------------------------------------------------------------------------

    Private Shared Function TabShown(k_tab As KBotRibbonTab) As Boolean
        If Not k_tab.Visible Then Return False
        For Each k_group As KBotRibbonGroup In k_tab.Groups
            If GroupShown(k_group) Then Return True
        Next
        Return False
    End Function

    Private Shared Function GroupShown(k_group As KBotRibbonGroup) As Boolean
        If Not k_group.Visible Then Return False
        For Each k_button As KBotRibbonButton In k_group.Buttons
            If k_button.Visible Then Return True
        Next
        For Each k_field As KBotRibbonField In k_group.Fields
            If k_field.Visible Then Return True
        Next
        Return False
    End Function

    Private Function ActiveTab() As KBotRibbonTab
        Dim k_first As KBotRibbonTab = Nothing
        For k_i As Integer = 0 To _tabs.Count - 1
            If Not TabShown(_tabs(k_i)) Then Continue For
            If k_first Is Nothing Then k_first = _tabs(k_i)
            If k_i = _selectedTab Then Return _tabs(k_i)
        Next
        Return k_first
    End Function

    Private Const MeasureFlags As TextFormatFlags = TextFormatFlags.NoPadding Or TextFormatFlags.SingleLine

    Private Function TextWidth(k_text As String, k_font As Font) As Integer
        If String.IsNullOrEmpty(k_text) Then Return 0
        Return TextRenderer.MeasureText(k_text, k_font, New Size(Integer.MaxValue, 0), MeasureFlags).Width
    End Function

    ' The caption of a Large button, cut into lines at the spaces only: a word is never split. Greedy; a word wider
    ' than the width stays alone on its line (the drawing cuts it with an ellipsis).
    Private Function GreedyLines(k_text As String, k_width As Integer) As List(Of String)
        Dim k_lines As New List(Of String)()
        If String.IsNullOrEmpty(k_text) Then Return k_lines
        Dim k_line As String = String.Empty
        For Each k_word As String In k_text.Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries)
            Dim k_try As String = If(k_line.Length = 0, k_word, k_line & " " & k_word)
            If k_line.Length > 0 AndAlso TextWidth(k_try, Font) > k_width Then
                k_lines.Add(k_line)
                k_line = k_word
            Else
                k_line = k_try
            End If
        Next
        If k_line.Length > 0 Then k_lines.Add(k_line)
        Return k_lines
    End Function

    ' At most k_maxLines lines: what does not fit is joined onto the last one, which the drawing ends with an ellipsis.
    Private Function WrapLines(k_text As String, k_width As Integer, k_maxLines As Integer) As List(Of String)
        Dim k_lines As List(Of String) = GreedyLines(k_text, k_width)
        If k_lines.Count > k_maxLines Then
            Dim k_rest As String = String.Join(" ", k_lines.Skip(k_maxLines - 1))
            k_lines.RemoveRange(k_maxLines - 1, k_lines.Count - (k_maxLines - 1))
            k_lines.Add(k_rest)
        End If
        Return k_lines
    End Function

    ' The narrowest width (within the limits) at which the caption takes at most two lines, every word whole.
    Private Function LargeWidth(k_text As String) As Integer
        Dim k_min As Integer = Px(64)
        Dim k_max As Integer = Px(124)
        Dim k_pad As Integer = Px(14)
        If String.IsNullOrEmpty(k_text) Then Return k_min
        If TextWidth(k_text, Font) + k_pad <= k_min Then Return k_min
        Dim k_longest As Integer = 0
        For Each k_part As String In k_text.Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries)
            k_longest = Math.Max(k_longest, TextWidth(k_part, Font))
        Next
        Dim k_width As Integer = Math.Max(k_min, k_longest + k_pad)
        Do While k_width < k_max AndAlso GreedyLines(k_text, k_width - k_pad).Count > 2
            k_width += Px(4)
        Loop
        Return Math.Min(k_width, k_max)
    End Function

    Private Function SmallWidth(k_text As String) As Integer
        Return Px(6) + Px(SmallIcon) + Px(6) + TextWidth(k_text, Font) + Px(8)
    End Function

    ' Puts the pending small buttons into one column (three at most) and moves k_x past it.
    Private Sub PlaceSmallColumn(k_column As List(Of KBotRibbonButton), ByRef k_x As Integer, k_top As Integer)
        If k_column.Count = 0 Then Return
        Dim k_width As Integer = 0
        For Each k_button As KBotRibbonButton In k_column
            k_width = Math.Max(k_width, SmallWidth(k_button.Text))
        Next
        For k_i As Integer = 0 To k_column.Count - 1
            k_column(k_i).Bounds = New Rectangle(k_x, k_top + k_i * Px(SmallRowHeight), k_width, Px(SmallRowHeight))
        Next
        k_x += k_width
        k_column.Clear()
    End Sub

    Private Sub EnsureLayout()
        Dim k_key As Integer = Px(1000) Xor (Width << 4) Xor If(_collapsed, 1, 0)
        If _layoutValid AndAlso k_key = _layoutKey Then Return
        _layoutKey = k_key
        _layoutValid = True
        ComputeLayout()
        ApplyFieldControls()
    End Sub

    ' The field controls follow the slots the layout gave them: placed and shown, or hidden (hidden tab, folded ribbon, hidden field).
    Private Sub ApplyFieldControls()
        For Each k_group As KBotRibbonGroup In AllGroups()
            For Each k_field As KBotRibbonField In k_group.Fields
                Dim k_control As Control = k_field.Control
                If k_control Is Nothing Then Continue For
                k_control.Enabled = k_field.Enabled
                If k_field.Bounds.IsEmpty Then
                    k_control.Visible = False
                Else
                    k_control.Bounds = k_field.Bounds
                    k_control.Visible = True
                End If
            Next
        Next
    End Sub

    ' The height the real control of a field wants at its font (0 while there is none, e.g. at design time).
    Private Function FieldHeight(k_field As KBotRibbonField) As Integer
        Dim k_combo As KBotComboBox = TryCast(k_field.Control, KBotComboBox)
        If k_combo IsNot Nothing Then Return k_combo.PreferredHeight
        If k_field.Control IsNot Nothing Then Return k_field.Control.GetPreferredSize(Size.Empty).Height
        Return 0
    End Function

    Private Sub ComputeLayout()
        ' Nothing is hit-testable until placed.
        For Each k_tab As KBotRibbonTab In _tabs
            k_tab.Bounds = Rectangle.Empty
        Next
        For Each k_group As KBotRibbonGroup In AllGroups()
            ClearGroupBounds(k_group)
        Next

        For Each k_pinnedButton As KBotRibbonButton In _pinned
            k_pinnedButton.Bounds = Rectangle.Empty
            k_pinnedButton.SeparatorX = -1
        Next

        Dim k_tabH As Integer = Px(TabStripHeight)
        _chevronRect = New Rectangle(Width - Px(30), 0, Px(26), k_tabH)
        ' The pinned buttons sit left of the chevron, the last one nearest to it.
        Dim k_pinX As Integer = _chevronRect.Left - Px(4)
        For k_p As Integer = _pinned.Count - 1 To 0 Step -1
            Dim k_pinned As KBotRibbonButton = _pinned(k_p)
            ' A Large pinned button stands in the panel, at the right; folded, it shrinks into the tab strip.
            k_pinned.DrawSmall = k_pinned.Large AndAlso _collapsed
            If Not k_pinned.Visible OrElse (k_pinned.Large AndAlso Not _collapsed) Then Continue For
            Dim k_pw As Integer = SmallWidth(k_pinned.Text)
            k_pinned.Bounds = New Rectangle(k_pinX - k_pw, Px(2), k_pw, k_tabH - Px(4))
            k_pinX -= k_pw + Px(2)
            If k_pinned.SeparatorBefore AndAlso PinnedVisibleBefore(k_p) Then
                k_pinned.SeparatorX = k_pinX - Px(3)
                k_pinX -= Px(6)
            End If
        Next
        Dim k_x As Integer = Px(8)
        For Each k_tab As KBotRibbonTab In _tabs
            If Not TabShown(k_tab) Then Continue For
            Dim k_w As Integer = TextWidth(k_tab.Text, BoldFont()) + Px(28)
            k_tab.Bounds = New Rectangle(k_x, 0, k_w, k_tabH)
            k_x += k_w
        Next
        Dim k_rightW As Integer = Math.Min(TextWidth(_rightText, RightFont()) + Px(12), Math.Max(0, k_pinX - k_x - Px(8)))
        _rightTextRect = New Rectangle(k_pinX - k_rightW, 0, k_rightW, k_tabH)

        If _collapsed Then Return
        Dim k_active As KBotRibbonTab = ActiveTab()
        If k_active Is Nothing Then Return

        Dim k_panelTop As Integer = k_tabH
        Dim k_gx As Integer = Px(4)
        For Each k_group As KBotRibbonGroup In k_active.Groups
            If Not GroupShown(k_group) Then Continue For
            k_gx += PlaceGroup(k_group, k_gx, k_panelTop) + 1
        Next

        ' The Large pinned buttons: right end of the panel, a thin line to their left.
        Dim k_panelRight As Integer = Width - Px(8)
        For k_p As Integer = _pinned.Count - 1 To 0 Step -1
            Dim k_pinned As KBotRibbonButton = _pinned(k_p)
            If Not k_pinned.Visible OrElse Not k_pinned.Large Then Continue For
            Dim k_pw As Integer = LargeWidth(k_pinned.Text)
            k_pinned.CenterV = True
            k_pinned.Bounds = New Rectangle(k_panelRight - k_pw, k_panelTop + Px(4), k_pw, PanelPx() - Px(8))
            k_panelRight -= k_pw + Px(2)
            If k_pinned.SeparatorBefore AndAlso PinnedVisibleBefore(k_p) Then
                k_pinned.SeparatorX = k_panelRight - Px(3)
                k_panelRight -= Px(6)
            End If
        Next

        ' The right group (slice 0114-03): the same on every tab, against the large pinned buttons; its right edge line
        ' is the separator before them.
        If GroupShown(_rightGroup) Then
            Dim k_rgW As Integer = PlaceGroup(_rightGroup, 0, k_panelTop)
            ShiftGroup(_rightGroup, k_panelRight - Px(2) - k_rgW)
        End If
    End Sub

    Private Shared Sub ClearGroupBounds(k_group As KBotRibbonGroup)
        k_group.Bounds = Rectangle.Empty
        k_group.CaptionBounds = Rectangle.Empty
        For Each k_button As KBotRibbonButton In k_group.Buttons
            k_button.Bounds = Rectangle.Empty
        Next
        For Each k_field As KBotRibbonField In k_group.Fields
            k_field.Bounds = Rectangle.Empty
        Next
    End Sub

    ' Moves a placed group (its box, caption, buttons and fields) k_dx to the right.
    Private Shared Sub ShiftGroup(k_group As KBotRibbonGroup, k_dx As Integer)
        k_group.Bounds = New Rectangle(k_group.Bounds.X + k_dx, k_group.Bounds.Y, k_group.Bounds.Width, k_group.Bounds.Height)
        k_group.CaptionBounds = New Rectangle(k_group.CaptionBounds.X + k_dx, k_group.CaptionBounds.Y, k_group.CaptionBounds.Width, k_group.CaptionBounds.Height)
        For Each k_button As KBotRibbonButton In k_group.Buttons
            If k_button.Bounds.IsEmpty Then Continue For
            k_button.Bounds = New Rectangle(k_button.Bounds.X + k_dx, k_button.Bounds.Y, k_button.Bounds.Width, k_button.Bounds.Height)
        Next
        For Each k_field As KBotRibbonField In k_group.Fields
            If k_field.Bounds.IsEmpty Then Continue For
            k_field.Bounds = New Rectangle(k_field.Bounds.X + k_dx, k_field.Bounds.Y, k_field.Bounds.Width, k_field.Bounds.Height)
        Next
    End Sub

    ' Places one group's buttons and fields with its left edge at k_gx and returns the group's width.
    Private Function PlaceGroup(k_group As KBotRibbonGroup, k_gx As Integer, k_panelTop As Integer) As Integer
        Dim k_captionH As Integer = Px(GroupCaptionHeight)
        Dim k_pad As Integer = Px(6)
        Dim k_cx As Integer = k_gx + k_pad
        Dim k_small As New List(Of KBotRibbonButton)()
        For Each k_button As KBotRibbonButton In k_group.Buttons
            If Not k_button.Visible Then Continue For
            If k_button.Large Then
                PlaceSmallColumn(k_small, k_cx, k_panelTop + Px(6))
                Dim k_w As Integer = LargeWidth(k_button.Text)
                k_button.Bounds = New Rectangle(k_cx, k_panelTop + Px(4), k_w, PanelPx() - k_captionH - Px(6))
                k_cx += k_w
            Else
                k_small.Add(k_button)
                If k_small.Count = 3 Then PlaceSmallColumn(k_small, k_cx, k_panelTop + Px(6))
            End If
        Next
        PlaceSmallColumn(k_small, k_cx, k_panelTop + Px(6))
        ' The fields: one column after the buttons, a row each (three at most), the widest sets the width.
        Dim k_fields As New List(Of KBotRibbonField)()
        For Each k_field As KBotRibbonField In k_group.Fields
            If k_field.Visible AndAlso k_fields.Count < 3 Then k_fields.Add(k_field)
        Next
        If k_fields.Count > 0 Then
            Dim k_fieldW As Integer = 0
            For Each k_field As KBotRibbonField In k_fields
                k_fieldW = Math.Max(k_fieldW, Px(Math.Max(40, k_field.Width)))
            Next
            ' The KBot combo / text box keep the height their font asks for (the combo forces it), so a row is that
            ' tall, never the 22 px of a small button: the rows stack with a small gap, centred above the group caption.
            Dim k_rowH As Integer = Px(SmallRowHeight) - Px(2)
            For Each k_field As KBotRibbonField In k_fields
                k_rowH = Math.Max(k_rowH, FieldHeight(k_field))
            Next
            Dim k_gap As Integer = Px(3)
            Dim k_block As Integer = k_fields.Count * k_rowH + (k_fields.Count - 1) * k_gap
            Dim k_area As Integer = PanelPx() - k_captionH - Px(6)
            Dim k_fieldTop As Integer = k_panelTop + Px(4) + Math.Max(0, (k_area - k_block) \ 2)
            k_cx += Px(4)
            For k_i As Integer = 0 To k_fields.Count - 1
                k_fields(k_i).Bounds = New Rectangle(k_cx, k_fieldTop + k_i * (k_rowH + k_gap), k_fieldW, k_rowH)
            Next
            k_cx += k_fieldW + Px(4)
        End If
        Dim k_groupW As Integer = Math.Max(k_cx - k_gx + k_pad, TextWidth(k_group.Text, CaptionFont()) + Px(16))
        k_group.Bounds = New Rectangle(k_gx, k_panelTop, k_groupW, PanelPx())
        k_group.CaptionBounds = New Rectangle(k_gx, k_panelTop + PanelPx() - k_captionH - Px(2), k_groupW, k_captionH)
        Return k_groupW
    End Function

    ' True when a pinned button earlier in the collection (to the left of the one at k_index) is visible.
    Private Function PinnedVisibleBefore(k_index As Integer) As Boolean
        For k_i As Integer = 0 To k_index - 1
            If _pinned(k_i).Visible Then Return True
        Next
        Return False
    End Function

    ' ---- Painting ---------------------------------------------------------------------------

    Protected Overrides Sub OnPaint(k_e As PaintEventArgs)
        Try
            If UpdateHeightIfNeeded() Then Return
            EnsureLayout()
            Dim k_g As Graphics = k_e.Graphics
            Dim k_p As ThemePalette = Palette()
            k_g.SmoothingMode = SmoothingMode.AntiAlias
            k_g.Clear(k_p.SurfaceColor)

            Dim k_tabH As Integer = Px(TabStripHeight)
            If Not _collapsed Then
                Dim k_panel As New Rectangle(0, k_tabH, Width, Height - k_tabH)
                Using k_brush As New SolidBrush(k_p.SurfaceAltColor)
                    k_g.FillRectangle(k_brush, k_panel)
                End Using
            End If
            Using k_pen As New Pen(k_p.BorderColor)
                k_g.DrawLine(k_pen, 0, Height - 1, Width, Height - 1)
                If Not _collapsed Then k_g.DrawLine(k_pen, 0, k_tabH - 1, Width, k_tabH - 1)
            End Using

            PaintTabs(k_g, k_p)
            For Each k_pinnedButton As KBotRibbonButton In _pinned
                If Not k_pinnedButton.Bounds.IsEmpty Then PaintButton(k_g, k_p, k_pinnedButton)
            Next
            ' The thin line left of a pinned button that asked for one (slice 0114-02).
            Using k_sepPen As New Pen(k_p.BorderColor)
                For Each k_pinnedButton As KBotRibbonButton In _pinned
                    If k_pinnedButton.SeparatorX < 0 OrElse k_pinnedButton.Bounds.IsEmpty Then Continue For
                    Dim k_y1 As Integer = If(_collapsed, Px(6), k_tabH + Px(10))
                    Dim k_y2 As Integer = If(_collapsed, k_tabH - Px(6), Height - Px(12))
                    k_g.DrawLine(k_sepPen, k_pinnedButton.SeparatorX, k_y1, k_pinnedButton.SeparatorX, k_y2)
                Next
            End Using
            If Not _collapsed Then PaintGroups(k_g, k_p)
        Catch ex As Exception
            ' UI boundary (OnPaint): log and swallow.
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotRibbon.OnPaint", ex)
        End Try
    End Sub

    ' The height is forced by SetBoundsCore; if something resized us past it, put it right and
    ' repaint on the next pass instead of drawing a stale frame.
    Private Function UpdateHeightIfNeeded() As Boolean
        If Height = DesiredHeight() Then Return False
        If Not IsHandleCreated Then Return False
        BeginInvoke(New Action(AddressOf UpdateHeight))
        Return False
    End Function

    Private Sub PaintTabs(k_g As Graphics, k_p As ThemePalette)
        Dim k_active As KBotRibbonTab = ActiveTab()
        For Each k_tab As KBotRibbonTab In _tabs
            If k_tab.Bounds.IsEmpty Then Continue For
            Dim k_isActive As Boolean = ReferenceEquals(k_tab, k_active)
            Dim k_rect As Rectangle = k_tab.Bounds
            If ReferenceEquals(k_tab, _hotTab) AndAlso Not k_isActive Then
                Using k_path As GraphicsPath = ThemeShapes.RoundedRect(Rectangle.Inflate(k_rect, -Px(2), -Px(3)), CornerRadius())
                    Using k_brush As New SolidBrush(k_p.ButtonHoverColor)
                        k_g.FillPath(k_brush, k_path)
                    End Using
                End Using
            End If
            TextRenderer.DrawText(k_g, k_tab.Text, If(k_isActive, BoldFont(), Font), k_rect,
                                  If(k_isActive, k_p.TextColor, k_p.TextDimColor),
                                  TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPadding Or TextFormatFlags.SingleLine)
            If k_isActive Then
                Using k_brush As New SolidBrush(k_p.AccentColor)
                    k_g.FillRectangle(k_brush, k_rect.Left + Px(10), k_rect.Bottom - Px(3), k_rect.Width - Px(20), Px(2))
                End Using
            End If
        Next

        If _rightTextRect.Width > 0 AndAlso Not String.IsNullOrEmpty(_rightText) Then
            TextRenderer.DrawText(k_g, _rightText, RightFont(), _rightTextRect, k_p.TextDimColor,
                                  TextFormatFlags.Right Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPadding Or
                                  TextFormatFlags.SingleLine Or TextFormatFlags.EndEllipsis)
        End If

        ' Chevron: points up when unfolded (click to fold), down when folded.
        If _hotChevron Then
            Using k_path As GraphicsPath = ThemeShapes.RoundedRect(Rectangle.Inflate(_chevronRect, -Px(2), -Px(3)), CornerRadius())
                Using k_brush As New SolidBrush(k_p.ButtonHoverColor)
                    k_g.FillPath(k_brush, k_path)
                End Using
            End Using
        End If
        Dim k_cx As Integer = _chevronRect.Left + _chevronRect.Width \ 2
        Dim k_cy As Integer = _chevronRect.Top + _chevronRect.Height \ 2
        Dim k_s As Integer = Px(4)
        Using k_pen As New Pen(k_p.TextDimColor, Math.Max(1.0F, Px(2) / 2.0F))
            If _collapsed Then
                k_g.DrawLines(k_pen, New Point() {New Point(k_cx - k_s, k_cy - k_s \ 2), New Point(k_cx, k_cy + k_s \ 2), New Point(k_cx + k_s, k_cy - k_s \ 2)})
            Else
                k_g.DrawLines(k_pen, New Point() {New Point(k_cx - k_s, k_cy + k_s \ 2), New Point(k_cx, k_cy - k_s \ 2), New Point(k_cx + k_s, k_cy + k_s \ 2)})
            End If
        End Using
    End Sub

    Private Sub PaintGroups(k_g As Graphics, k_p As ThemePalette)
        Dim k_active As KBotRibbonTab = ActiveTab()
        If k_active IsNot Nothing Then
            For Each k_group As KBotRibbonGroup In k_active.Groups
                PaintGroup(k_g, k_p, k_group)
            Next
        End If
        PaintGroup(k_g, k_p, _rightGroup)
    End Sub

    Private Sub PaintGroup(k_g As Graphics, k_p As ThemePalette, k_group As KBotRibbonGroup)
        If k_group.Bounds.IsEmpty Then Return
        For Each k_button As KBotRibbonButton In k_group.Buttons
            If Not k_button.Bounds.IsEmpty Then PaintButton(k_g, k_p, k_button)
        Next
        TextRenderer.DrawText(k_g, k_group.Text, CaptionFont(), k_group.CaptionBounds, k_p.TextDimColor,
                              TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPadding Or
                              TextFormatFlags.SingleLine Or TextFormatFlags.EndEllipsis)
        Using k_pen As New Pen(k_p.BorderColor)
            k_g.DrawLine(k_pen, k_group.Bounds.Right, k_group.Bounds.Top + Px(6), k_group.Bounds.Right, k_group.Bounds.Bottom - Px(6))
        End Using
    End Sub

    Private Sub PaintButton(k_g As Graphics, k_p As ThemePalette, k_button As KBotRibbonButton)
        Dim k_rect As Rectangle = k_button.Bounds
        Dim k_inner As Rectangle = Rectangle.Inflate(k_rect, -Px(1), -Px(1))
        If k_button.Enabled Then
            If ReferenceEquals(k_button, _pressedButton) AndAlso ReferenceEquals(k_button, _hotButton) Then
                FillRounded(k_g, k_inner, k_p.ButtonPressedColor)
            ElseIf ReferenceEquals(k_button, _hotButton) Then
                FillRounded(k_g, k_inner, k_p.ButtonHoverColor)
            End If
        End If

        Dim k_color As Color = If(k_button.Enabled, If(k_button.ForeColor.IsEmpty, k_p.TextColor, k_button.ForeColor), k_p.DisabledTextColor)
        If k_button.Large AndAlso Not k_button.DrawSmall Then
            Dim k_icon As Integer = Px(LargeIcon)
            Dim k_lines As List(Of String) = WrapLines(k_button.Text, k_rect.Width - Px(4), 2)
            Dim k_lineH As Integer = Font.Height
            ' A pinned button is centred in the whole height; the buttons of a group hang from the top (the group
            ' caption is below them).
            Dim k_top As Integer = k_rect.Top + Px(4)
            If k_button.CenterV Then
                Dim k_block As Integer = k_icon + Px(2) + k_lines.Count * k_lineH
                k_top = k_rect.Top + Math.Max(Px(2), (k_rect.Height - k_block) \ 2)
            End If
            Dim k_iconRect As New Rectangle(k_rect.Left + (k_rect.Width - k_icon) \ 2, k_top, k_icon, k_icon)
            DrawIcon(k_g, k_button, k_iconRect)
            Dim k_y As Integer = k_iconRect.Bottom + Px(2)
            For Each k_line As String In k_lines
                TextRenderer.DrawText(k_g, k_line, Font, New Rectangle(k_rect.Left + Px(2), k_y, k_rect.Width - Px(4), k_lineH), k_color,
                                      TextFormatFlags.HorizontalCenter Or TextFormatFlags.Top Or TextFormatFlags.NoPadding Or
                                      TextFormatFlags.SingleLine Or TextFormatFlags.EndEllipsis)
                k_y += k_lineH
            Next
        Else
            Dim k_icon As Integer = Px(SmallIcon)
            Dim k_iconRect As New Rectangle(k_rect.Left + Px(6), k_rect.Top + (k_rect.Height - k_icon) \ 2, k_icon, k_icon)
            DrawIcon(k_g, k_button, k_iconRect)
            Dim k_textRect As New Rectangle(k_iconRect.Right + Px(6), k_rect.Top, k_rect.Right - k_iconRect.Right - Px(8), k_rect.Height)
            TextRenderer.DrawText(k_g, k_button.Text, Font, k_textRect, k_color,
                                  TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPadding Or
                                  TextFormatFlags.SingleLine Or TextFormatFlags.EndEllipsis)
        End If
    End Sub

    Private Sub FillRounded(k_g As Graphics, k_rect As Rectangle, k_color As Color)
        Using k_path As GraphicsPath = ThemeShapes.RoundedRect(k_rect, CornerRadius())
            Using k_brush As New SolidBrush(k_color)
                k_g.FillPath(k_brush, k_path)
            End Using
        End Using
    End Sub

    Private Shared Sub DrawIcon(k_g As Graphics, k_button As KBotRibbonButton, k_rect As Rectangle)
        If k_button.Image Is Nothing Then Return
        Dim k_mode As InterpolationMode = k_g.InterpolationMode
        k_g.InterpolationMode = InterpolationMode.HighQualityBicubic
        k_g.PixelOffsetMode = PixelOffsetMode.HighQuality
        Try
            If k_button.Enabled Then
                k_g.DrawImage(k_button.Image, k_rect)
            Else
                Dim k_matrix As New ColorMatrix() With {.Matrix33 = 0.4F}
                Using k_attrs As New ImageAttributes()
                    k_attrs.SetColorMatrix(k_matrix)
                    k_g.DrawImage(k_button.Image, k_rect, 0, 0, k_button.Image.Width, k_button.Image.Height, GraphicsUnit.Pixel, k_attrs)
                End Using
            End If
        Finally
            k_g.InterpolationMode = k_mode
        End Try
    End Sub

    ' ---- Input ------------------------------------------------------------------------------

    Private Function ButtonAt(k_point As Point) As KBotRibbonButton
        For Each k_pinnedButton As KBotRibbonButton In _pinned
            If Not k_pinnedButton.Bounds.IsEmpty AndAlso k_pinnedButton.Bounds.Contains(k_point) Then Return k_pinnedButton
        Next
        If _collapsed Then Return Nothing
        Dim k_active As KBotRibbonTab = ActiveTab()
        If k_active IsNot Nothing Then
            For Each k_group As KBotRibbonGroup In k_active.Groups
                Dim k_found As KBotRibbonButton = ButtonInGroup(k_group, k_point)
                If k_found IsNot Nothing Then Return k_found
            Next
        End If
        Return ButtonInGroup(_rightGroup, k_point)
    End Function

    Private Shared Function ButtonInGroup(k_group As KBotRibbonGroup, k_point As Point) As KBotRibbonButton
        If k_group.Bounds.IsEmpty OrElse Not k_group.Bounds.Contains(k_point) Then Return Nothing
        For Each k_button As KBotRibbonButton In k_group.Buttons
            If Not k_button.Bounds.IsEmpty AndAlso k_button.Bounds.Contains(k_point) Then Return k_button
        Next
        Return Nothing
    End Function

    Private Function TabAt(k_point As Point) As KBotRibbonTab
        For Each k_tab As KBotRibbonTab In _tabs
            If Not k_tab.Bounds.IsEmpty AndAlso k_tab.Bounds.Contains(k_point) Then Return k_tab
        Next
        Return Nothing
    End Function

    Protected Overrides Sub OnMouseMove(k_e As MouseEventArgs)
        MyBase.OnMouseMove(k_e)
        Try
            If KBotDesignTime.IsDesignTime(Me) Then Return
            EnsureLayout()
            Dim k_button As KBotRibbonButton = ButtonAt(k_e.Location)
            If k_button IsNot Nothing AndAlso Not k_button.Enabled Then k_button = Nothing
            Dim k_tab As KBotRibbonTab = TabAt(k_e.Location)
            Dim k_chevron As Boolean = _chevronRect.Contains(k_e.Location)

            Dim k_changed As Boolean = Not ReferenceEquals(k_button, _hotButton) OrElse
                                       Not ReferenceEquals(k_tab, _hotTab) OrElse k_chevron <> _hotChevron
            If Not k_changed Then Return
            Dim k_buttonChanged As Boolean = Not ReferenceEquals(k_button, _hotButton)
            _hotButton = k_button
            _hotTab = k_tab
            _hotChevron = k_chevron
            Invalidate()
            If k_buttonChanged Then ShowButtonTip(k_button)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRibbon.OnMouseMove", ex)
        End Try
    End Sub

    Private Sub ShowButtonTip(k_button As KBotRibbonButton)
        If _tooltip Is Nothing Then Return
        _tooltip.HideNow()
        If k_button Is Nothing Then Return
        If String.IsNullOrEmpty(k_button.ToolTipText) AndAlso String.IsNullOrEmpty(k_button.ToolTipHeader) Then Return
        Dim k_content As New KBotToolTipContent() With {
            .Text = k_button.ToolTipText,
            .HeaderText = If(String.IsNullOrEmpty(k_button.ToolTipHeader), Nothing, k_button.ToolTipHeader)
        }
        _tooltip.ShowAt(Me, k_content, PointToScreen(New Point(k_button.Bounds.Left, k_button.Bounds.Bottom + Px(2))))
    End Sub

    Protected Overrides Sub OnMouseLeave(k_e As EventArgs)
        MyBase.OnMouseLeave(k_e)
        Try
            _hotButton = Nothing
            _hotTab = Nothing
            _hotChevron = False
            _pressedButton = Nothing
            _tooltip?.HideNow()
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRibbon.OnMouseLeave", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseDown(k_e As MouseEventArgs)
        MyBase.OnMouseDown(k_e)
        Try
            If KBotDesignTime.IsDesignTime(Me) OrElse k_e.Button <> MouseButtons.Left Then Return
            EnsureLayout()
            _tooltip?.HideNow()
            If _chevronRect.Contains(k_e.Location) Then
                Collapsed = Not _collapsed
                Return
            End If
            Dim k_tab As KBotRibbonTab = TabAt(k_e.Location)
            If k_tab IsNot Nothing Then
                SelectedTabIndex = _tabs.IndexOf(k_tab)
                If _collapsed Then
                    Collapsed = False
                    _expandedByClickAt = Environment.TickCount
                End If
                Return
            End If
            Dim k_button As KBotRibbonButton = ButtonAt(k_e.Location)
            If k_button IsNot Nothing AndAlso k_button.Enabled Then
                _pressedButton = k_button
                Invalidate()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRibbon.OnMouseDown", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseUp(k_e As MouseEventArgs)
        MyBase.OnMouseUp(k_e)
        Try
            If KBotDesignTime.IsDesignTime(Me) OrElse k_e.Button <> MouseButtons.Left Then Return
            Dim k_pressed As KBotRibbonButton = _pressedButton
            _pressedButton = Nothing
            Invalidate()
            If k_pressed Is Nothing Then Return
            If Not ReferenceEquals(ButtonAt(k_e.Location), k_pressed) Then Return
            RaiseEvent ItemClicked(Me, New KBotRibbonItemClickedEventArgs(k_pressed))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRibbon.OnMouseUp", ex)
        End Try
    End Sub

    ' Double click on a tab folds the ribbon -- unless this very click sequence has just unfolded it.
    Protected Overrides Sub OnMouseDoubleClick(k_e As MouseEventArgs)
        MyBase.OnMouseDoubleClick(k_e)
        Try
            If KBotDesignTime.IsDesignTime(Me) OrElse k_e.Button <> MouseButtons.Left Then Return
            If TabAt(k_e.Location) Is Nothing Then Return
            If Environment.TickCount - _expandedByClickAt < SystemInformation.DoubleClickTime Then Return
            Collapsed = Not _collapsed
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRibbon.OnMouseDoubleClick", ex)
        End Try
    End Sub

End Class
