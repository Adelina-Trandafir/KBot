Option Strict On
Imports System.ComponentModel

''' <summary>
''' The help popup of the «?» caption-bar button (slice 0000-20): a search box with its results,
''' the topic of the screen and the guided tours of the visible windows (all rows come from the
''' application through <see cref="IKBotHelpSearchSource"/>), and a last line «Deschide ajutorul
''' complet».
'''
''' <para><b>A window, shown modeless under the button</b> (<see cref="ShowUnder"/>). It
''' activates, because the search box needs the keyboard, and closes itself on Esc, on a click
''' elsewhere (<c>Deactivate</c>) and when a row hands over to a page, a screen or a tour. WinForms
''' disposes it on close: never wrap it in a <c>Using</c>. The close is deferred
''' (<c>BeginInvoke</c>) so the handler that caused it finishes on a live window.</para>
''' </summary>
<ToolboxItem(False)>
<DesignerCategory("Form")>
Partial Public Class KBotHelpPopup
    Implements IThemedControl

    Private Const WS_EX_TOOLWINDOW As Integer = &H80

    Private _anchor As Rectangle = Rectangle.Empty
    Private _closing As Boolean

    ''' <summary>«Deschide ajutorul complet» was clicked (the popup closes by itself).</summary>
    Public Event FullHelpRequested As EventHandler

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
    End Sub

    ''' <summary>A popup searching in <paramref name="source"/>.</summary>
    Public Sub New(source As IKBotHelpSearchSource)
        Me.New()
        If source Is Nothing Then Throw New ArgumentNullException(NameOf(source))
        pnlCautare.Source = source
    End Sub

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or WS_EX_TOOLWINDOW   ' no taskbar / Alt+Tab entry
            Return cp
        End Get
    End Property

    ''' <summary>
    ''' Shows the popup under <paramref name="anchorScreenRect"/> (right edges aligned, the «?» is
    ''' at the right of the caption bar), above it when there is no room below, always on the
    ''' anchor's screen. <paramref name="owner"/> keeps it above the window that asked.
    ''' </summary>
    Public Sub ShowUnder(anchorScreenRect As Rectangle, owner As Form)
        Try
            If anchorScreenRect.IsEmpty Then Throw New ArgumentException("The anchor rectangle is empty.", NameOf(anchorScreenRect))
            _anchor = anchorScreenRect
            Place()
            If owner IsNot Nothing AndAlso Not owner.IsDisposed Then
                Show(owner)
            Else
                Show()
            End If
            Activate()
            pnlCautare.FocusSearch()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotHelpPopup.ShowUnder", ex)
            Throw
        End Try
    End Sub

    ' The size is final only after Load (theme zoom), so the place is worked out again there.
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        Try
            Place()
        Catch ex As Exception
            ' UI boundary (Load).
            GlobalErrorLog.Write("KBotHelpPopup.OnLoad", ex)
        End Try
    End Sub

    Private Sub Place()
        If _anchor.IsEmpty Then Return
        Dim area As Rectangle = Screen.FromRectangle(_anchor).WorkingArea
        Dim x As Integer = _anchor.Right - Width
        Dim y As Integer = _anchor.Bottom
        If y + Height > area.Bottom AndAlso _anchor.Top - Height >= area.Top Then y = _anchor.Top - Height
        x = Math.Max(area.Left, Math.Min(x, area.Right - Width))
        y = Math.Max(area.Top, Math.Min(y, area.Bottom - Height))
        Location = New Point(x, y)
    End Sub

    ' ── Closing ───────────────────────────────────────────────────────────────────

    ''' <summary>
    ''' Slice 000T-08: True while a tutorial points at the popup: losing the focus, Esc and the rows do not close it
    ''' (any window the tutorial shows takes the focus for a moment, which used to close the popup under it). The
    ''' tutorial closes it with <see cref="CloseNow"/> when it moves on or ends.
    ''' </summary>
    Public Property KeepOpen As Boolean

    ''' <summary>Closes the popup now, whatever <see cref="KeepOpen"/> says.</summary>
    Public Sub CloseNow()
        KeepOpen = False
        CloseSoon()
    End Sub

    Private Sub CloseSoon()
        If KeepOpen OrElse _closing OrElse IsDisposed Then Return
        _closing = True
        If IsHandleCreated Then
            BeginInvoke(New MethodInvoker(AddressOf Close))
        Else
            Close()
        End If
    End Sub

    Protected Overrides Sub OnDeactivate(e As EventArgs)
        MyBase.OnDeactivate(e)
        Try
            CloseSoon()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotHelpPopup.OnDeactivate", ex)
        End Try
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        Try
            pnlCautare.EndQuestion()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotHelpPopup.OnFormClosed", ex)
        End Try
        MyBase.OnFormClosed(e)
    End Sub

    Private Sub PnlCautare_CloseRequested(sender As Object, e As EventArgs) Handles pnlCautare.CloseRequested
        CloseSoon()
    End Sub

    Private Sub PnlCautare_EscapePressed(sender As Object, e As EventArgs) Handles pnlCautare.EscapePressed
        CloseSoon()
    End Sub

    Private Sub BtnAjutorComplet_Click(sender As Object, e As EventArgs) Handles btnAjutorComplet.Click
        Try
            CloseSoon()
            RaiseEvent FullHelpRequested(Me, EventArgs.Empty)
        Catch ex As Exception
            ' UI boundary (click handler).
            GlobalErrorLog.Write("KBotHelpPopup.BtnAjutorComplet_Click", ex)
        End Try
    End Sub

    ' ── Theme ─────────────────────────────────────────────────────────────────────

    ''' <summary>
    ''' The 1px outline (the form's own background under its Padding) and the last line; the
    ''' search panel is themed as a nested control.
    ''' </summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.BorderColor
            btnAjutorComplet.BackColor = p.SurfaceColor
            btnAjutorComplet.ForeColor = p.AccentColor
            btnAjutorComplet.FlatAppearance.BorderSize = 0
            btnAjutorComplet.FlatAppearance.MouseOverBackColor = p.ButtonHoverColor
            btnAjutorComplet.FlatAppearance.MouseDownBackColor = p.ButtonPressedColor
        Catch ex As Exception
            GlobalErrorLog.Write("KBotHelpPopup.ApplyTheme", ex)
        End Try
    End Sub

End Class
