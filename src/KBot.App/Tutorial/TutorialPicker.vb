Option Strict On
Imports System.Drawing
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>What the picker found under the mouse (slice 000T): what a step needs to point there.</summary>
Friend NotInheritable Class TutorialPick

    ''' <summary><c>TypeName.controlName</c> (or just a window type when no named control is there).</summary>
    Public Property Target As String = String.Empty

    ''' <summary>A painted piece of the target (<c>footer.left</c>, <c>item:rezervari</c>...); empty = the whole control.</summary>
    Public Property Part As String = String.Empty

    ''' <summary>The wait that fits what was picked (a button is clicked, a nav item is selected...).</summary>
    Public Property SuggestedWait As TutorialWaitKind = TutorialWaitKind.Manual

    Public Property SuggestedWaitArg As String = String.Empty

    ''' <summary>False = the control has no usable name, so a tutorial cannot point at it by name.</summary>
    Public Property HasStableName As Boolean

    ''' <summary>Screen rectangle of the picked control or part.</summary>
    Public Property Bounds As Rectangle

    ''' <summary>The control that was picked (the nearest named one).</summary>
    Public Property Control As Control

End Class

''' <summary>
''' «Alege pe ecran» of the tutorial designer (slice 000T): a see-through layer over the whole screen
''' that catches the mouse. The control (or painted part) under the mouse gets a ring and a small
''' label that says what the step would write; a left click takes it, Esc or a right click gives up.
''' Resolution reuses what the help already uses: <see cref="HelpService.ScreenKeys"/> for the name,
''' <see cref="IKBotHelpParts"/> for the painted pieces. Only K-BOT's own windows are read.
''' </summary>
Friend NotInheritable Class TutorialPicker
    Inherits Form

    Private ReadOnly _frame As New HelpTourFrame()
    Private ReadOnly _info As New InfoBox()
    Private _hover As TutorialPick

    ' What the user clicked; Nothing when they gave up.
    Private _result As TutorialPick

    Private Sub New()
        FormBorderStyle = FormBorderStyle.None
        StartPosition = FormStartPosition.Manual
        ShowInTaskbar = False
        TopMost = True
        AutoScaleMode = AutoScaleMode.None
        Bounds = SystemInformation.VirtualScreen
        BackColor = Color.Black
        Opacity = 0.02
        Cursor = Cursors.Cross
        KeyPreview = True
    End Sub

    ''' <summary>
    ''' Runs the picker; returns what the user clicked, or Nothing. The caller hides its own windows
    ''' first (a window of the designer under the mouse would be picked instead of the screen).
    ''' </summary>
    Public Shared Function Pick() As TutorialPick
        Try
            Using picker As New TutorialPicker()
                picker.ShowDialog()
                Return picker._result
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialPicker.Pick", ex)
            Throw
        End Try
    End Function

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Try
            Dim found As TutorialPick = Resolve(Cursor.Position)
            _hover = found
            If found Is Nothing OrElse found.Bounds.IsEmpty Then
                _frame.Hide()
                _info.Hide()
                Return
            End If
            _frame.Surround(found.Bounds)
            _info.ShowFor(found, Cursor.Position)
        Catch ex As Exception
            ' UI boundary (mouse handler): log and swallow; the picker stays usable.
            GlobalErrorLog.Write("TutorialPicker.OnMouseMove", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        Try
            If e.Button = MouseButtons.Left AndAlso _hover IsNot Nothing Then
                _result = _hover
                DialogResult = DialogResult.OK
            ElseIf e.Button = MouseButtons.Right Then
                DialogResult = DialogResult.Cancel
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialPicker.OnMouseDown", ex)
        End Try
    End Sub

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        If e.KeyCode = Keys.Escape Then DialogResult = DialogResult.Cancel
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        MyBase.OnFormClosed(e)
        If Not _frame.IsDisposed Then _frame.Close()
        If Not _info.IsDisposed Then _info.Close()
    End Sub

    ' ── what is under the mouse ──────────────────────────────────────────────────

    Private Function Resolve(k_screenPoint As Point) As TutorialPick
        Dim skip As New List(Of IntPtr) From {Handle}
        If _frame.IsHandleCreated Then skip.Add(_frame.Handle)
        If _info.IsHandleCreated Then skip.Add(_info.Handle)
        Return ResolveAt(k_screenPoint, skip)
    End Function

    ''' <summary>
    ''' What is under <paramref name="k_screenPoint"/> among K-BOT's own windows, topmost first, except the
    ''' windows in <paramref name="k_skip"/>; Nothing when it is not a K-BOT window (the recorder also uses it).
    ''' </summary>
    Friend Shared Function ResolveAt(k_screenPoint As Point, k_skip As ICollection(Of IntPtr)) As TutorialPick
        For Each box As HelpCaptureNative.WindowBox In HelpCaptureNative.TopLevelWindows(k_skip)
            If Not box.Bounds.Contains(k_screenPoint) Then Continue For
            Dim window As Form = TryCast(Control.FromHandle(box.Handle), Form)
            ' A window that is not K-BOT's, or one of the tutorial's own, is not a target: look below it.
            If window Is Nothing Then Return Nothing
            If TypeOf window Is TutorialDim OrElse TypeOf window Is HelpTourFrame OrElse TypeOf window Is HelpTourBubble Then Continue For
            Return ResolveIn(window, k_screenPoint)
        Next
        Return Nothing
    End Function

    Private Shared Function ResolveIn(k_window As Form, k_screenPoint As Point) As TutorialPick
        Dim c As Control = k_window
        Do
            Dim child As Control = c.GetChildAtPoint(c.PointToClient(k_screenPoint), GetChildAtPointSkip.Invisible Or GetChildAtPointSkip.Transparent)
            If child Is Nothing Then Exit Do
            c = child
        Loop
        Return ForControl(NearestNamed(c, k_window), k_screenPoint, True)
    End Function

    ''' <summary>The nearest control that has a name: an inner text box of a field is not what a step points at.</summary>
    Friend Shared Function NearestNamed(k_control As Control, k_window As Form) As Control
        Dim named As Control = k_control
        While named IsNot Nothing AndAlso Not TypeOf named Is Form AndAlso named.Name.Length = 0
            named = named.Parent
        End While
        Return If(named, k_window)
    End Function

    ''' <summary>
    ''' What a step needs to point at <paramref name="named"/>: its stable name, the painted part under
    ''' <paramref name="k_screenPoint"/> (when <paramref name="k_withPart"/>) and the wait that fits it.
    ''' </summary>
    Friend Shared Function ForControl(named As Control, k_screenPoint As Point, k_withPart As Boolean) As TutorialPick
        Dim pick As New TutorialPick With {.Bounds = named.RectangleToScreen(named.ClientRectangle), .Control = named}
        Dim keys As List(Of String) = HelpService.ScreenKeys(named)
        Dim withControl As String = keys.FirstOrDefault(Function(k) k.Contains("."c))
        If TypeOf named Is Form Then
            pick.Target = named.GetType().Name
            pick.HasStableName = True
        ElseIf withControl IsNot Nothing Then
            pick.Target = withControl
            pick.HasStableName = True
        Else
            pick.Target = If(keys.FirstOrDefault(), named.GetType().Name)
        End If

        Dim part As String = If(k_withPart, PartAt(named, k_screenPoint), String.Empty)
        pick.Part = part
        If part.Length > 0 Then
            Dim r As Rectangle = PartRect(named, part)
            If Not r.IsEmpty Then pick.Bounds = r
        End If
        SuggestWait(named, part, pick)
        Return pick
    End Function

    ' ── painted parts ────────────────────────────────────────────────────────────

    ' Most specific first, so «header.search» wins over «header». Mirrors $PartsByType in Check-Help.ps1.
    Private Shared ReadOnly TreeParts As String() = {"header.search", "header.right", "footer.left", "footer.right", "footer.collapse", "columns", "header", "footer"}
    Private Shared ReadOnly GridParts As String() = {"header.filter", "footer.left", "footer.right", "footer.collapse", "header", "footer"}
    Private Shared ReadOnly CaptionParts As String() = {"icon", "title", "unit", "year", "ss", "options", "theme", "help", "minimize", "maximize", "close"}

    Private Shared Function PartAt(k_control As Control, k_screenPoint As Point) As String
        Dim local As Point = k_control.PointToClient(k_screenPoint)
        Dim nav As KBotNavList = TryCast(k_control, KBotNavList)
        If nav IsNot Nothing Then
            Dim key As String = nav.ItemKeyAt(local)
            Return If(key.Length > 0, "item:" & key, String.Empty)
        End If
        Dim tree As AdvancedTreeControl = TryCast(k_control, AdvancedTreeControl)
        If tree IsNot Nothing AndAlso tree.RightIconRectsOnScreen().Any(Function(r) r.Contains(local)) Then Return "node.icon"
        Dim names As String() = Nothing
        If tree IsNot Nothing Then names = TreeParts
        If TypeOf k_control Is KBotDataView Then names = GridParts
        If TypeOf k_control Is KBotCaptionBar Then names = CaptionParts
        Dim owner As IKBotHelpParts = TryCast(k_control, IKBotHelpParts)
        If names Is Nothing OrElse owner Is Nothing Then Return String.Empty
        For Each name As String In names
            Try
                If owner.HelpPartBounds(name).Contains(local) Then Return name
            Catch ex As ArgumentException
                ' A part this control family does not have: not a candidate.
            End Try
        Next
        Return String.Empty
    End Function

    Private Shared Function PartRect(k_control As Control, k_part As String) As Rectangle
        If k_part = "node.icon" Then
            Dim tree As AdvancedTreeControl = TryCast(k_control, AdvancedTreeControl)
            Dim local As Point = k_control.PointToClient(Cursor.Position)
            Dim hit As Rectangle = If(tree Is Nothing, Rectangle.Empty, tree.RightIconRectsOnScreen().FirstOrDefault(Function(r) r.Contains(local)))
            Return If(hit.IsEmpty, Rectangle.Empty, k_control.RectangleToScreen(hit))
        End If
        Dim owner As IKBotHelpParts = TryCast(k_control, IKBotHelpParts)
        If owner Is Nothing Then Return Rectangle.Empty
        Dim r2 As Rectangle = owner.HelpPartBounds(k_part)
        Return If(r2.IsEmpty, Rectangle.Empty, k_control.RectangleToScreen(r2))
    End Function

    ' The wait that fits the control: what the user does to it.
    Private Shared Sub SuggestWait(k_control As Control, k_part As String, k_pick As TutorialPick)
        If TypeOf k_control Is KBotNavList AndAlso k_part.StartsWith("item:", StringComparison.Ordinal) Then
            k_pick.SuggestedWait = TutorialWaitKind.Tab
            k_pick.SuggestedWaitArg = k_part.Substring(5)
        ElseIf TypeOf k_control Is AdvancedTreeControl Then
            ' A row is selected; the row button and every painted button of the header / footer (the footer's
            ' icons, the collapse button, the search icon) are CLICKED -- the runner watches the press inside the part
            ' (slice 000T-09). The empty bands themselves ("header", "footer") do nothing.
            If k_part.Length = 0 Then
                k_pick.SuggestedWait = TutorialWaitKind.Select
            ElseIf k_part = "header" OrElse k_part = "footer" Then
                k_pick.SuggestedWait = TutorialWaitKind.Manual
            Else
                k_pick.SuggestedWait = TutorialWaitKind.Click
            End If
        ElseIf TypeOf k_control Is KBotDataView Then
            ' Same for the grid: its header (sort), filter icon and footer buttons; not the TOTAL band, not the cells.
            k_pick.SuggestedWait = If(k_part.Length > 0 AndAlso k_part <> "footer", TutorialWaitKind.Click, TutorialWaitKind.Manual)
        ElseIf TypeOf k_control Is CheckBox Then
            k_pick.SuggestedWait = TutorialWaitKind.Checked
        ElseIf TypeOf k_control Is ButtonBase Then
            k_pick.SuggestedWait = TutorialWaitKind.Click
        ElseIf TypeOf k_control Is KBotComboBox OrElse TypeOf k_control Is KBotTextField OrElse TypeOf k_control Is TextBoxBase OrElse TypeOf k_control Is ComboBox Then
            k_pick.SuggestedWait = TutorialWaitKind.Changed
        Else
            k_pick.SuggestedWait = TutorialWaitKind.Manual
        End If
    End Sub

    ' ── the little label that follows the mouse ──────────────────────────────────

    Private NotInheritable Class InfoBox
        Inherits Form

        Private Const WS_EX_TOOLWINDOW As Integer = &H80
        Private Const WS_EX_NOACTIVATE As Integer = &H8000000
        Private Const WS_EX_TRANSPARENT As Integer = &H20
        Private ReadOnly _label As New Label()

        Public Sub New()
            FormBorderStyle = FormBorderStyle.None
            StartPosition = FormStartPosition.Manual
            ShowInTaskbar = False
            TopMost = True
            AutoScaleMode = AutoScaleMode.None
            _label.AutoSize = True
            _label.Padding = New Padding(6, 4, 6, 4)
            _label.Font = New Font("Segoe UI", 9.0F)
            Controls.Add(_label)
            AutoSize = True
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        End Sub

        Protected Overrides ReadOnly Property ShowWithoutActivation As Boolean
            Get
                Return True
            End Get
        End Property

        Protected Overrides ReadOnly Property CreateParams As CreateParams
            Get
                Dim cp As CreateParams = MyBase.CreateParams
                cp.ExStyle = cp.ExStyle Or WS_EX_TOOLWINDOW Or WS_EX_NOACTIVATE Or WS_EX_TRANSPARENT
                Return cp
            End Get
        End Property

        Public Sub ShowFor(k_pick As TutorialPick, k_mouse As Point)
            Dim p As ThemePalette = ThemeManager.Current.Palette
            BackColor = p.AccentColor
            _label.BackColor = p.AccentColor
            _label.ForeColor = Color.White
            Dim text As String = "target: " & k_pick.Target
            If k_pick.Part.Length > 0 Then text &= vbLf & "part: " & k_pick.Part
            text &= vbLf & "wait: " & k_pick.SuggestedWait.ToString().ToLowerInvariant() &
                    If(k_pick.SuggestedWaitArg.Length > 0, ":" & k_pick.SuggestedWaitArg, String.Empty)
            If Not k_pick.HasStableName Then text &= vbLf & "(fara nume stabil: nu poate fi tinta)"
            _label.Text = text
            Location = New Point(k_mouse.X + 18, k_mouse.Y + 22)
            If Not Visible Then Show()
        End Sub

    End Class

End Class
