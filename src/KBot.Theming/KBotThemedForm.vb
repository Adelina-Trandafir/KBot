Imports System.ComponentModel
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Formular de bază care se auto-tematizează. Formularele noi îl moștenesc și primesc
''' tema gratis; cele existente migrează incremental (schimbă „Inherits Form” →
''' „Inherits KBot.Theming.KBotThemedForm” în .Designer.vb și șterg apelul propriu de
''' ApplyTheme din Load).
'''
''' La comutare live, difuzarea din ThemeManager.SetScheme deja a reaplicat tema pe
''' acest formular (e în lista de ținte), deci handler-ul de eveniment rulează DOAR
''' <see cref="OnThemeChanged"/> — fără dublu Apply.
'''
''' <para>Slice 0062 adds two behaviours, both switchable per form from the designer:</para>
''' <list type="bullet">
''' <item><see cref="AutoFitToTheme"/> -- the form never ends up smaller than its base size
''' or than what its themed content asks for (<see cref="ThemeFormFit"/>). The base is captured
''' at <see cref="OnCreateControl"/> (before <c>Load</c>), the fit runs at the tail of <see cref="OnLoad"/> (after the
''' theme) and again after every scheme or scaling change.</item>
''' <item>Slice 0066-02: the geometry is on ONE ruler. <c>AutoScaleMode.Dpi</c> gives
''' <c>DeviceDpi / designDpi</c> on both axes, and <see cref="AppScaling.ApplyZoom"/> (run from
''' <c>ThemeManager.Apply</c> in <see cref="OnLoad"/>, again on every broadcast, and from
''' <see cref="OnDpiChanged"/>) multiplies the whole form by the operator's zoom through
''' <c>Control.Scale</c>. Nothing here follows the font any more.</item>
''' <item><see cref="CenterOnScreen"/> -- a form that would be centred by WinForms' own rule
''' (<c>CenterScreen</c>, or <c>CenterParent</c> with nobody to centre on) is centred on the
''' application's screen (<see cref="AppScreen"/>) instead of the monitor under the mouse. Forms
''' that place themselves (<c>Manual</c>) are left alone.</item>
''' </list>
''' </summary>
Public Class KBotThemedForm
    Inherits Form

    Private _centerOnScreen As Boolean = True
    Private _autoFitToTheme As Boolean = True

    ''' <summary>
    ''' Puts the application's base font on the form BEFORE anything else (slice 0052).
    '''
    ''' <para>Since slice 0066-02 every form is <c>AutoScaleMode.Dpi</c>, so the font no longer
    ''' decides any geometry: WinForms scales the children by <c>DeviceDpi / AutoScaleDimensions</c>
    ''' alone, and a font written later moves nothing. (Under the old <c>AutoScaleMode.Font</c> the
    ''' form was measured in whatever font it wore during <c>InitializeComponent</c>, which is why
    ''' the base constructor had to dress it first; the timing is kept because the designer surface
    ''' still renders the base type, see below.)</para>
    '''
    ''' <para>It also fixes the designer surface: Visual Studio instantiates the BASE type to
    ''' render a derived form, so the designer now lays out in the same font the operator will
    ''' see. Designer and runtime finally measure the same thing — see <see cref="KBotFonts"/>
    ''' for what they used to measure instead.</para>
    ''' </summary>
    Public Sub New()
        Font = KBotFonts.Base
    End Sub

    ' ── Slice 0062: the two switches ──────────────────────────────────────────

    ''' <summary>
    ''' True (default): a form WinForms would centre on the mouse's monitor is centred on the
    ''' application's screen instead. False: the form is placed exactly as WinForms would.
    ''' Menus, flyouts and anything that positions itself must be False.
    ''' </summary>
    <Category("K-BOT")>
    <DefaultValue(True)>
    <Description("Centrează fereastra pe ecranul aplicației, nu pe monitorul de sub mouse.")>
    Public Property CenterOnScreen As Boolean
        Get
            Return _centerOnScreen
        End Get
        Set(value As Boolean)
            _centerOnScreen = value
        End Set
    End Property

    ''' <summary>
    ''' True (default): the form grows to its base size and to its themed content, at load and
    ''' after every scheme or scaling change (<see cref="ThemeFormFit"/>). False: the size is
    ''' left to the designer and the platform alone.
    ''' </summary>
    <Category("K-BOT")>
    <DefaultValue(True)>
    <Description("Fereastra crește la mărimea cerută de tema activă și nu scade sub baza ei.")>
    Public Property AutoFitToTheme As Boolean
        Get
            Return _autoFitToTheme
        End Get
        Set(value As Boolean)
            _autoFitToTheme = value
        End Set
    End Property

    ''' <summary>
    ''' The control whose preferred size IS the form's content demand. By default the single
    ''' child docked <c>Fill</c>; zero or several such children throw, because the answer would be
    ''' a guess -- a form laid out differently overrides this and names its root.
    '''
    ''' <para>Why not the form itself: <c>Form.GetPreferredSize</c> does not answer about
    ''' content, and a <c>TableLayoutPanel</c> clips any docked child to its cell (slice 0030 -- a
    ''' 56px button in a 40px cell reports 40).</para>
    ''' </summary>
    <Browsable(False)>
    Protected Overridable ReadOnly Property FitRoot As Control
        Get
            Dim found As Control = Nothing
            Dim count As Integer = 0
            For Each c As Control In Controls
                If c.Dock = DockStyle.Fill Then
                    count += 1
                    found = c
                End If
            Next
            If count <> 1 Then
                Throw New InvalidOperationException(
                    "Form «" & Name & "» has " & count & " children docked Fill; override FitRoot or set AutoFitToTheme = False.")
            End If
            Return found
        End Get
    End Property

    ''' <summary>
    ''' The base size is captured here: after <c>InitializeComponent</c> (so after the platform's
    ''' autoscale), before <see cref="OnLoad"/> (so before the theme grows anything). Idempotent.
    '''
    ''' <para>Not in <c>OnHandleCreated</c>, although that was the plan: measured, a borderless
    ''' form reports a TRANSIENT client size there (the window is created with the default frame
    ''' and WinForms puts the requested client size back a moment later -- 400x300 read as
    ''' 378x244). <c>Form.OnCreateControl</c> is the first hook after that fix-up, and it is the
    ''' one that raises <c>Load</c>, so "before MyBase" here is exactly "before the theme".</para>
    ''' </summary>
    Protected Overrides Sub OnCreateControl()
        Try
            If Not KBotDesignTime.IsDesignTime(Me) Then ThemeFormFit.Capture(Me)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotThemedForm.OnCreateControl", ex)
        End Try
        MyBase.OnCreateControl()
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        Try
            MyBase.OnLoad(e)
            ThemeManager.RegisterForm(Me)
            ThemeManager.Apply(Me)
            OnThemeChanged()
            AddHandler ThemeManager.ThemeChanged, AddressOf HandleThemeChanged
            AddHandler AppScaling.ScalingChanged, AddressOf HandleScalingChanged
        Catch ex As Exception
            ' Boundary UI (Load): un throw ar dărâma deschiderea formularului — logăm și înghițim.
            GlobalErrorLog.Write("KBotThemedForm.OnLoad", ex)
        End Try

        ' Size first, then place: the position depends on the fitted size. Its own Try so a
        ' misdeclared FitRoot cannot unhook the theme events above.
        Try
            If KBotDesignTime.IsDesignTime(Me) Then Return
            If _autoFitToTheme Then ThemeFormFit.Apply(Me, FitRoot)
            PlaceOnReferenceScreen()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotThemedForm.OnLoad(fit)", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Replaces WinForms' centring -- and only that. <c>CenterScreen</c> always goes through
    ''' <see cref="AppScreen"/>; <c>CenterParent</c> only when there is nobody to centre on
    ''' (no owner and no active form), which is exactly when WinForms falls back to the mouse's
    ''' monitor. <c>Manual</c> and the Windows defaults are left untouched. Runs before the first
    ''' paint, so there is no flicker.
    ''' </summary>
    Private Sub PlaceOnReferenceScreen()
        If Not _centerOnScreen OrElse WindowState <> FormWindowState.Normal Then Return
        Select Case StartPosition
            Case FormStartPosition.CenterScreen
                AppScreen.Center(Me)
            Case FormStartPosition.CenterParent
                If Owner Is Nothing AndAlso Parent Is Nothing AndAlso Form.ActiveForm Is Nothing Then
                    AppScreen.Center(Me)
                End If
        End Select
    End Sub

    ''' <summary>
    ''' Recomputes the size against the current base definition and the themed content, then
    ''' keeps the window on its screen. Called by the base after scheme and scaling changes and by
    ''' <see cref="ThemeFormFit.Baseline"/> when the operator changes what the base means.
    ''' </summary>
    Protected Friend Sub RefitToTheme()
        Try
            If IsDisposed OrElse Not IsHandleCreated OrElse Not _autoFitToTheme Then Return
            If KBotDesignTime.IsDesignTime(Me) Then Return
            If ThemeFormFit.Apply(Me, FitRoot) Then AppScreen.KeepOnScreen(Me)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotThemedForm.RefitToTheme", ex)
        End Try
    End Sub

    ' The platform just rescaled the form for the new monitor (bounds, fonts). Under Automatic our
    ' zoom does not depend on the DPI, so this is a no-op; under Fixed100 / Manual it is the mode's
    ' factor over the NEW screen that has to be re-applied. Fonts are left to the platform here.
    Protected Overrides Sub OnDpiChanged(e As DpiChangedEventArgs)
        MyBase.OnDpiChanged(e)
        Try
            If KBotDesignTime.IsDesignTime(Me) Then Return
            If AppScaling.ApplyZoom(Me) Then RefitToTheme()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotThemedForm.OnDpiChanged", ex)
        End Try
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        Try
            RemoveHandler ThemeManager.ThemeChanged, AddressOf HandleThemeChanged
            RemoveHandler AppScaling.ScalingChanged, AddressOf HandleScalingChanged
            ThemeManager.UnregisterForm(Me)
            ThemeFormFit.Forget(Me)
            AppScreen.ClearReference(Me)
            MyBase.OnFormClosed(e)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotThemedForm.OnFormClosed", ex)
        End Try
    End Sub

    Private Sub HandleThemeChanged(sender As Object, e As EventArgs)
        Try
            ' Apply s-a executat deja în difuzarea SetScheme; aici doar re-citim culorile semantice.
            OnThemeChanged()
            ' ...and the size: the scheme font just went through autoscale, ModernRenderer just
            ' grew the buttons -- whatever no longer fits is asked for now.
            RefitToTheme()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotThemedForm.HandleThemeChanged", ex)
        End Try
    End Sub

    ' AppScaling.Broadcast already rescaled fonts and our own metrics; the window follows.
    Private Sub HandleScalingChanged(sender As Object, e As EventArgs)
        Try
            RefitToTheme()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotThemedForm.HandleScalingChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Suprascrie ca să re-aplici culori semantice theme-aware după o comutare de schemă
    ''' (ex. WicketMonitorForm cu proprietățile sale Clr*).
    ''' </summary>
    Protected Overridable Sub OnThemeChanged()
    End Sub

End Class
