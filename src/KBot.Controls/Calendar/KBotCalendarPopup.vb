Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' The window a <see cref="KBotDatePicker"/> drops: a borderless form whose whole client area is
''' one <see cref="KBotCalendar"/>.
'''
''' <para><b>It is a WINDOW, not a control you drop on a form.</b> Same family as
''' <c>CustomPopup</c>: built in code, opened with <see cref="ShowBelow"/>, and it closes itself —
''' on a chosen day, on Esc, or on <c>Deactivate</c> (the operator clicked somewhere else). Being
''' shown modeless, WinForms disposes it for us on close: <b>never put it in a <c>Using</c></b>,
''' or it is destroyed before anyone sees it.</para>
'''
''' <para><b>It activates.</b> Like <c>CustomPopup</c> and unlike the flyouts, it takes the
''' keyboard focus — a calendar you cannot walk with the arrow keys is half a calendar. The price
''' is the same one: the title bar underneath reads as inactive while the calendar is open.</para>
'''
''' <para>The placement rule is not duplicated here: <c>CustomPopup.FitToWorkArea</c> already
''' decides how a drop-down flips when it does not fit, and it is the tested one.</para>
''' </summary>
<ToolboxItem(False)>
<DesignerCategory("Code")>
<DefaultEvent("DateCommitted")>
Public NotInheritable Class KBotCalendarPopup
    Inherits Form
    Implements IThemedControl

    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const CS_DROPSHADOW As Integer = &H20000

    Private ReadOnly _calendar As New KBotCalendar()
    Private _closing As Boolean = False

    ''' <summary>Raised when the operator CHOSE a day. Not raised when the popup is dismissed.</summary>
    Public Event DateCommitted As EventHandler(Of KBotDateSelectedEventArgs)

    Public Sub New()
        FormBorderStyle = FormBorderStyle.None
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        ControlBox = False
        MinimizeBox = False
        MaximizeBox = False
        Text = String.Empty
        ' No autoscaling: the size comes from KBotCalendar.NaturalSize, which is already in scaled
        ' pixels, and a second pass of the form's own scaling would stretch it a second time.
        AutoScaleMode = AutoScaleMode.None

        _calendar.Dock = DockStyle.Fill
        _calendar.ShowToday = True
        AddHandler _calendar.DateSelected, AddressOf OnCalendarDateSelected
        Controls.Add(_calendar)

        ' Self-theming, for the same reason CustomPopup is: a standalone window is not reached by
        ' the host's theme traversal, and an owner who must remember to theme it is exactly how a
        ' white window ends up inside a dark scheme.
        ApplyTheme(ThemeManager.Current)
    End Sub

    ''' <summary>The calendar inside — set <c>Value</c>, <c>MinDate</c>, … before opening.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property Calendar As KBotCalendar
        Get
            Return _calendar
        End Get
    End Property

    ''' <summary>Reapplies the scheme to the window and to the calendar it carries.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            _calendar.ApplyTheme(scheme)
            MyBase.BackColor = _calendar.BackColor
        Catch ex As Exception
            GlobalErrorLog.Write("KBotCalendarPopup.ApplyTheme", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Opens the calendar under a rectangle INSIDE <paramref name="anchor"/> (client coordinates)
    ''' — the drop-down button of the field, which is painted, not a control of its own. When it
    ''' does not fit below, it flips above the field; when it does not fit to the right, its right
    ''' edge lines up with the field's.
    ''' </summary>
    Public Sub ShowBelow(anchor As Control, anchorRect As Rectangle)
        Try
            ArgumentNullException.ThrowIfNull(anchor)
            If anchorRect.IsEmpty Then
                Throw New ArgumentException(
                    "Dreptunghiul de ancorare e gol — câmpul sub care s-ar deschide calendarul nu e vizibil.",
                    NameOf(anchorRect))
            End If

            ' The ambient font of the host: a standalone window does not inherit it, and a calendar
            ' in a different font from the form it hangs off is visible at once.
            If anchor.Font IsNot Nothing Then
                MyBase.Font = anchor.Font
                _calendar.ResetFont()
            End If

            Dim sus As Point = anchor.PointToScreen(New Point(anchorRect.Left, anchorRect.Top))
            Dim dorit As Size = _calendar.NaturalSize
            Dim wa As Rectangle = Screen.FromPoint(sus).WorkingArea

            Bounds = CustomPopup.FitToWorkArea(dorit,
                                               New Point(sus.X, sus.Y + anchorRect.Height),
                                               sus.X + anchorRect.Width, sus.Y, wa)

            Owner = anchor.FindForm()
            Show()
            Activate()
            _calendar.Focus()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotCalendarPopup.ShowBelow", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Closes with a result. <paramref name="picked"/> Nothing = dismissed (Esc, click outside).
    ''' Guards against re-entry: <c>Close</c> raises <c>Deactivate</c>, which would come back here.
    ''' </summary>
    Private Sub CloseWith(picked As Date?)
        If _closing Then Return
        _closing = True
        Try
            _lastClosedAt = DateTime.UtcNow
            If picked.HasValue Then
                RaiseEvent DateCommitted(Me, New KBotDateSelectedEventArgs(picked.Value))
            End If
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotCalendarPopup.CloseWith", ex)
        End Try
    End Sub

    Private Sub OnCalendarDateSelected(sender As Object, e As KBotDateSelectedEventArgs)
        CloseWith(e.Value)
    End Sub

    ''' <summary>A click anywhere else dismisses the calendar — what every drop-down does.</summary>
    Protected Overrides Sub OnDeactivate(e As EventArgs)
        MyBase.OnDeactivate(e)
        CloseWith(Nothing)
    End Sub

    ''' <summary>Esc dismisses. The rest of the keyboard belongs to the calendar.</summary>
    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If keyData = Keys.Escape Then
            CloseWith(Nothing)
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

    ' When the popup was closed. STATIC, and that is fine: it closes the moment it loses activation,
    ' so two of them can never be open at once.
    Private Shared _lastClosedAt As DateTime = DateTime.MinValue

    ''' <summary>
    ''' "A calendar has just closed" — the answer to the SECOND click on the button that opened it.
    ''' That press does two things, in this order: it activates the window underneath (so the popup
    ''' closes itself through <c>Deactivate</c>) and only then reaches the button. A button that
    ''' merely opens would reopen it instantly, and the operator would see a calendar that refuses
    ''' to close. The field asks this first and stands down. Same problem, same 250 ms answer as
    ''' <c>CustomPopup.ClosedJustNow</c> — below the double-click threshold, so it cannot swallow a
    ''' second deliberate opening.
    ''' </summary>
    Public Shared ReadOnly Property ClosedJustNow As Boolean
        Get
            Return (DateTime.UtcNow - _lastClosedAt).TotalMilliseconds < 250
        End Get
    End Property

    ''' <summary>
    ''' The stamp is set in two places because neither covers both: <see cref="CloseWith"/> is our
    ''' own path and is the only one that runs on a popup which never reached the screen
    ''' (<c>Form.Close</c> on a handleless form only disposes, without raising <c>FormClosed</c>),
    ''' while this catches the closings that do not come from us (Alt+F4, the host closing the
    ''' window). Both are idempotent — it is a timestamp.
    ''' </summary>
    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        _lastClosedAt = DateTime.UtcNow
        MyBase.OnFormClosed(e)
    End Sub

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            ' TOOLWINDOW: no taskbar button, no place in Alt+Tab. NOACTIVATE is deliberately
            ' ABSENT — without activation there is no keyboard focus. CS_DROPSHADOW is the shadow
            ' every system drop-down has.
            cp.ExStyle = cp.ExStyle Or WS_EX_TOOLWINDOW
            cp.ClassStyle = cp.ClassStyle Or CS_DROPSHADOW
            Return cp
        End Get
    End Property

End Class
