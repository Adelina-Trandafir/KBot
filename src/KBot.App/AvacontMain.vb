Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' The application's main window. It owns the header (caption bar with the year / SS / unit selectors,
''' busy bar, the ribbon) and the side panel, and gives the rest of its area to the page it hosts: the
''' opening page, the shell (<see cref="KbotForm"/>, kept loaded and only shown or hidden) or any machete
''' (a stand-alone form shown as a page, one at a time). The header controls of the shell are the same
''' instances KbotForm declares (so its <c>Handles capBar...</c> keep working); they are only
''' re-parented here, which is why <c>KbotForm</c> still works on its own (the Debug bench).
''' </summary>
''' <remarks>
''' What the move changes for the hosted forms: none of them closes the application by itself (a
''' closed page gives the work area back to the page under it; this window listens to the shell's
''' FormClosed only for a failed start), the window state belongs to this window, and "fit to
''' theme" no longer grows the window with the content (the fit root is a host panel).
''' </remarks>
Public Class AvacontMain

    Private ReadOnly _kbot As KbotForm
    Private ReadOnly _home As AcasaPage
    Private _baseIsShell As Boolean   ' the page under the pages: the opening page (False) or the shell
    Private _closing As Boolean

    Private ReadOnly _registry As MachetaRegistry

    ''' <summary>The one main window of the running application (Nothing before it exists and after it closed).</summary>
    Friend Shared Property Current As AvacontMain

    Public Sub New(k_shell As KbotForm, k_session As SessionContext, k_controller As ForexeController,
                   k_registry As MachetaRegistry)
        InitializeComponent()
        If k_shell Is Nothing Then Throw New ArgumentNullException(NameOf(k_shell))
        _kbot = k_shell
        _registry = k_registry
        _session = k_session
        _controller = k_controller
        _tokenState = New Func(Of Task(Of EFacturaTokenState))(AddressOf k_shell.GetEFacturaTokenStateAsync)
        Current = Me
        k_shell.RegisterMachete(k_registry)
        Text = "K-BOT"
        Icon = k_shell.Icon

        ' Dock order: Fill first, then the Top bands; the last added docks first (top-most).
        Dim k_header As Control() = k_shell.TakeHeader()   ' busyBar, capBar (the ribbon is declared here)
        For Each k_band As Control In k_header
            Controls.Add(k_band)
        Next

        k_shell.PrepareForHosting(Me)
        pnlHost.Controls.Add(k_shell)
        AddHandler k_shell.FormClosed, AddressOf Shell_FormClosed

        ' The ribbon replaces the MENIU drop-down: a click goes to the same handler the menu used.
        ribbon.ValidateKeys()
        ribbon.Collapsed = AppSettings.Current.RibbonCollapsed
        ribbon.RightText = k_shell.lblOperator.Text
        AddHandler ribbon.CollapsedChanged, AddressOf Ribbon_CollapsedChanged
        AddHandler k_shell.MenuStateChanged, AddressOf SyncRibbon
        AddHandler k_shell.lblOperator.TextChanged, AddressOf Operator_TextChanged
        AddHandler AppSettings.Changed, AddressOf AppSettings_Changed

        ' The opening page. The shell is loaded behind it (OnShown) and reached from the ribbon.
        _home = New AcasaPage(k_session)
        PrepareHosted(_home)
        pnlHost.Controls.Add(_home)

        InitSidebar()
        UpdateCloseButton()
    End Sub

    ''' <summary>The hosted shell (for Program's re-login hook).</summary>
    Friend ReadOnly Property Shell As KbotForm
        Get
            Return _kbot
        End Get
    End Property

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        Try
            Dim k_settings As AppSettings = AppSettings.Current
            ' Once, the first time after this window exists: the application is shown maximized even if the
            ' box said no, so the operator sees how it looks. The box is theirs afterwards -- unticked again,
            ' it stays that way.
            If Not k_settings.FullScreenOffered Then
                k_settings.FullScreenOffered = True
                k_settings.StartMaximized = True
                k_settings.Save()
            End If
            If k_settings.StartMaximized Then WindowState = FormWindowState.Maximized
        Catch ex As Exception
            ' UI boundary (Load): log and swallow -- the window opens at its normal size.
            GlobalErrorLog.Write("AvacontMain.OnLoad", ex)
        End Try
    End Sub

    ' The page is shown only after this window has loaded: the theme and the zoom reach it
    ' through this window first (AppScaling marks the nested designed roots), then its own Load runs.
    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        Try
            _kbot.Show()   ' its Load runs now: session, periods, tree, FOREXE watchers; the opening page covers it
            _home.Show()
            ShowHome()
            SyncRibbon(Me, EventArgs.Empty)
            Activate()
        Catch ex As Exception
            ' UI boundary (Shown): log and swallow.
            GlobalErrorLog.Write("AvacontMain.OnShown", ex)
        End Try
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            ' The window's background IS its 1 px outline (as KbotForm and LoginForm do).
            BackColor = ThemeManager.Current.Palette.BorderColor
            ApplySidebarTones()
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.OnThemeChanged", ex)
        End Try
    End Sub

    ' A hosted form does not get FormClosing when its host closes: ask it to close here, so its
    ' own checks run (the browser leaves the view's panel before the window is destroyed).
    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        MyBase.OnFormClosing(e)
        Try
            If e.Cancel OrElse _kbot.IsDisposed Then Return
            _closing = True
            If _page IsNot Nothing AndAlso Not _page.IsDisposed Then _page.Close()
            If _page IsNot Nothing AndAlso Not _page.IsDisposed Then
                e.Cancel = True   ' the open page refused to close (unsaved changes)
                _closing = False
                Return
            End If
            _kbot.Close()
            If Not _kbot.IsDisposed Then
                e.Cancel = True   ' the shell refused to close
                _closing = False
            Else
                _home.Close()   ' lets go of what it listens to
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.OnFormClosing", ex)
            _closing = False
        End Try
    End Sub

    ' ---- Ribbon ---------------------------------------------------------------------------------

    Private Sub Ribbon_ItemClicked(sender As Object, e As KBotRibbonItemClickedEventArgs) Handles ribbon.ItemClicked
        Try
            Select Case e.Key
                Case "ajutor"
                    ShowRibbonHelp()
                Case "pagina_angajamente"
                    _kbot.OpenRegistered(KbotForm.MachetaAngajamente, Sub() ShowShell())
                Case "pagina_inchide"
                    CloseCurrent()
                Case "istoric_versiuni"
                    _kbot.OpenRegistered(KbotForm.MachetaIstoricVersiuni, AddressOf _kbot.DeschideIstoricVersiuni)
                Case "efactura_vanzare", "efactura_cumparare"
                    ' the ribbon key is the key of the macheta
                    _kbot.OpenRegistered(e.Key, Sub() _kbot.RunEFacturaCommand(e.Key))
                Case "ef_token", "ef_primite_sync"
                    _kbot.RunEFacturaCommand(e.Key)
                Case "unitate_conturi", "unitate_date"
                    _kbot.RunUnitCommand(e.Key)
                Case Else
                    ' These entries work on the shell's own views and tree: it must be what is on screen.
                    If ShellEntries.Contains(e.Key) AndAlso Not ShowShell() Then Return
                    _kbot.RunMenuEntry(e.Key)
            End Select
        Catch ex As Exception
            ' UI boundary (click handler): log and swallow; the entry told the operator what it had to.
            GlobalErrorLog.Write("AvacontMain.Ribbon_ItemClicked", ex)
        End Try
    End Sub

    Private Shared ReadOnly ShellEntries As New HashSet(Of String)(StringComparer.Ordinal) From {
        "angajament_nou", "angajament_forexe", "operatiuni_necorelate"}

    Private Sub Ribbon_CollapsedChanged(sender As Object, e As EventArgs)
        Try
            AppSettings.Current.RibbonCollapsed = ribbon.Collapsed
            AppSettings.Current.Save()
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.Ribbon_CollapsedChanged", ex)
        End Try
    End Sub

    ' The entries that appear only sometimes (uncorrelated operations, ADMIN tools, the log) follow the
    ' shell's own rules, which still live on its menu.
    Private Sub SyncRibbon(sender As Object, e As EventArgs)
        Try
            If IsDisposed OrElse _kbot.IsDisposed Then Return
            _kbot.MirrorMenuTo(ribbon)
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.SyncRibbon", ex)
        End Try
    End Sub

    Private Sub Operator_TextChanged(sender As Object, e As EventArgs)
        ribbon.RightText = _kbot.lblOperator.Text
    End Sub

    Private Sub AppSettings_Changed(sender As Object, e As EventArgs)
        If IsHandleCreated AndAlso Not IsDisposed Then SyncRibbon(sender, e)
    End Sub

    ''' <summary>
    ''' The «Inchide» button pinned at the right of the ribbon: it closes the macheta on screen -- a page, or the shell, which
    ''' goes back to the opening page (it stays loaded, hidden, with everything it holds). It exists only while there is a
    ''' macheta to close; the application closes from the X of the window. False = the macheta refused (unsaved changes).
    ''' </summary>
    Friend Function CloseCurrent() As Boolean
        If _page IsNot Nothing AndAlso Not _page.IsDisposed Then
            _page.Close()
            Return _page Is Nothing OrElse _page.IsDisposed
        End If
        If _baseIsShell Then Return ShowHome()
        Return False
    End Function

    ' «Inchide» is there while a macheta is on screen (a page, or the shell); on the opening page it is gone.
    Private Sub UpdateCloseButton()
        Dim k_button As KBotRibbonButton = ribbon.FindButton("pagina_inchide")
        If k_button Is Nothing Then Return
        Dim k_visible As Boolean = (_page IsNot Nothing AndAlso Not _page.IsDisposed) OrElse _baseIsShell
        If k_button.Visible = k_visible Then Return
        k_button.Visible = k_visible
        ribbon.InvalidateLayout()
    End Sub

    ' The help button of the ribbon. With a macheta on screen: the popup of the caption bar's «?» for it (search, the topic of
    ' the screen, its tours and tutorials). With none (the opening page): the help window itself, as a page, which the
    ' history of machete lists like any other.
    Private Sub ShowRibbonHelp()
        If Not KBotHelp.IsAvailable Then Return
        Dim k_front As Control = If(_page IsNot Nothing AndAlso Not _page.IsDisposed, CType(_page, Control), If(_baseIsShell, _kbot, Nothing))
        If k_front Is Nothing Then
            _kbot.OpenRegistered(KbotForm.MachetaAjutor, Sub() TryCast(KBotHelp.Provider, HelpService)?.ShowHelpPage(Me))
        Else
            KBotHelp.RequestMenu(k_front, ribbon.ButtonScreenBounds("ajutor"))
        End If
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        RemoveHandler AppSettings.Changed, AddressOf AppSettings_Changed   ' static event: must end with the window
        TearDownSidebar()
        If ReferenceEquals(Current, Me) Then Current = Nothing
        MyBase.OnFormClosed(e)
    End Sub

    ' ---- Pages: one stand-alone form at a time, in the work area ------------------------------------

    Private _page As Form
    Private _pageBar As KBotCaptionBar

    ''' <summary>The page shown in place of the shell now; Nothing = the shell.</summary>
    Friend ReadOnly Property CurrentPage As Form
        Get
            Return If(_page IsNot Nothing AndAlso Not _page.IsDisposed, _page, Nothing)
        End Get
    End Property
    Private _savedCaption As String

    ''' <summary>
    ''' Shows a stand-alone form as the page of the work area, in place of the shell (one page at a
    ''' time; the page under it stays behind, hidden, and comes back when the page closes). The page's own
    ''' title bar goes: its title moves to this window's caption bar (and follows it when it changes), and it
    ''' closes the way it always did (its own «Inchide» button, Escape, a cancel button). False = the page
    ''' already open refused to close (unsaved changes), so nothing was shown.
    ''' </summary>
    Friend Function ShowPage(k_page As Form, Optional k_ref As MachetaRef = Nothing) As Boolean
        Try
            ArgumentNullException.ThrowIfNull(k_page)
            If ReferenceEquals(_page, k_page) Then
                If k_ref IsNot Nothing Then _registry.Note(k_ref)   ' asked again: it comes back to the top of the history
                Return True
            End If
            If _page IsNot Nothing AndAlso Not _page.IsDisposed Then
                _page.Close()
                If _page IsNot Nothing AndAlso Not _page.IsDisposed Then Return False   ' the open page refused to close (unsaved changes)
            End If

            _page = k_page
            _savedCaption = _kbot.capBar.Text

            PrepareHosted(k_page)
            _pageBar = HidePageTitleBar(k_page)

            AddHandler k_page.FormClosed, AddressOf Page_FormClosed
            AddHandler k_page.TextChanged, AddressOf PageTitleChanged
            If _pageBar IsNot Nothing Then AddHandler _pageBar.TextChanged, AddressOf PageTitleChanged
            SetBaseVisible(False)
            pnlHost.Controls.Add(k_page)
            k_page.Show()
            k_page.BringToFront()
            UpdatePageCaption()   ' after Show: most forms write their title in Load
            If k_ref IsNot Nothing Then _registry.Note(k_ref)   ' the history of opened machete
            UpdateCloseButton()
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.ShowPage", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Shows a form as a page and returns when it has closed -- what <c>ShowDialog</c> used to do for the
    ''' code that opened it. False = the page already open refused to close, so the form was not shown.
    ''' </summary>
    Friend Async Function ShowPageAndWaitAsync(k_page As Form, k_ref As MachetaRef) As Task(Of Boolean)
        Dim k_closed As New TaskCompletionSource(Of Boolean)(TaskCreationOptions.RunContinuationsAsynchronously)
        AddHandler k_page.FormClosed, Sub(k_s, k_e) k_closed.TrySetResult(True)
        If Not ShowPage(k_page, k_ref) Then Return False
        Await k_closed.Task.ConfigureAwait(True)
        Return True
    End Function

    ''' <summary>
    ''' For code that opened a form modally: shows it as a page of the main window and waits for it to close;
    ''' with no main window (the Debug bench) it is the modal dialog it always was.
    ''' </summary>
    Friend Shared Async Function ShowMachetaAsync(k_owner As Control, k_form As Form, k_ref As MachetaRef) As Task(Of Boolean)
        Dim k_main As AvacontMain = Current
        If k_main Is Nothing OrElse k_main.IsDisposed Then
            k_form.ShowDialog(If(k_owner Is Nothing, Nothing, k_owner.FindForm()))
            Return True
        End If
        Return Await k_main.ShowPageAndWaitAsync(k_form, k_ref).ConfigureAwait(True)
    End Function

    ' A stand-alone form becomes a docked page: no frame, no size rules of its own, no own placement.
    Private Shared Sub PrepareHosted(k_page As Form)
        k_page.TopLevel = False
        k_page.FormBorderStyle = FormBorderStyle.None
        k_page.StartPosition = FormStartPosition.Manual
        k_page.ShowInTaskbar = False
        k_page.MinimumSize = Size.Empty
        k_page.Padding = Padding.Empty   ' the outline belongs to the main window
        k_page.Dock = DockStyle.Fill
        Dim k_themed As KBotThemedForm = TryCast(k_page, KBotThemedForm)
        If k_themed IsNot Nothing Then
            k_themed.AutoFitToTheme = False
            k_themed.CenterOnScreen = False
        End If
        HideCloseButtons(k_page)
        ' A modal dialog closed by its cancel button (a button with DialogResult.Cancel): a form shown
        ' without ShowDialog does not close on that, so the click closes it here.
        Dim k_cancel As Button = TryCast(k_page.CancelButton, Button)
        If k_cancel IsNot Nothing Then
            AddHandler k_cancel.Click, Sub(k_s, k_e)
                                           If Not k_page.IsDisposed AndAlso k_page.IsHandleCreated Then
                                               k_page.BeginInvoke(New Action(Sub()
                                                                                 If Not k_page.IsDisposed Then k_page.Close()
                                                                             End Sub))
                                           End If
                                       End Sub
        End If
    End Sub

    ' A macheta is closed from the ribbon (the «Inchide» button pinned at its right), so the machete lose their own
    ' close buttons. Only the buttons that merely close: «Renunta» of an editor (DDF, ORD, receptii links) closes too,
    ' but the «Renunta» of a nomenclature or an invoice cancels an edit and stays.
    Private Shared ReadOnly CloseButtonNames As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
        "btnInchide", "btnIesire", "btnClose"}

    Private Shared Sub HideCloseButtons(k_page As Form)
        Dim k_editor As Boolean = TypeOf k_page Is DdfEditForm OrElse TypeOf k_page Is OrdEditForm OrElse TypeOf k_page Is AsociereForm
        Dim k_hadCancel As Boolean = HideCloseButtonsIn(k_page, k_editor, TryCast(k_page.CancelButton, Button))
        ' Escape used to reach the form through its (now hidden) cancel button: it is heard on the form itself.
        If k_hadCancel Then
            k_page.KeyPreview = True
            AddHandler k_page.KeyDown, Sub(k_s, k_e)
                                           If k_e.KeyCode = Keys.Escape AndAlso Not k_page.IsDisposed Then
                                               k_e.Handled = True
                                               k_page.BeginInvoke(New Action(Sub()
                                                                                 If Not k_page.IsDisposed Then k_page.Close()
                                                                             End Sub))
                                           End If
                                       End Sub
        End If
    End Sub

    ' True when the form's cancel button was among the hidden ones.
    Private Shared Function HideCloseButtonsIn(k_parent As Control, k_editor As Boolean, k_cancel As Button) As Boolean
        Dim k_hidCancel As Boolean = False
        For Each k_child As Control In k_parent.Controls
            Dim k_button As Button = TryCast(k_child, Button)
            If k_button IsNot Nothing Then
                Dim k_hide As Boolean = CloseButtonNames.Contains(k_button.Name) OrElse
                                        (k_editor AndAlso String.Equals(k_button.Name, "btnRenunta", StringComparison.OrdinalIgnoreCase))
                If k_hide Then
                    k_button.Visible = False
                    If ReferenceEquals(k_button, k_cancel) Then k_hidCancel = True
                End If
            End If
            If k_child.HasChildren AndAlso HideCloseButtonsIn(k_child, k_editor, k_cancel) Then k_hidCancel = True
        Next
        Return k_hidCancel
    End Function

    ' The page under the pages -- the opening page or the shell -- shown or put away.
    Private Sub SetBaseVisible(k_visible As Boolean)
        If _baseIsShell Then
            _kbot.Visible = k_visible
            If k_visible Then _kbot.Activate()
        Else
            _home.Visible = k_visible
            If k_visible Then
                _home.BringToFront()
                _home.RefreshState()
                RefreshSidebar()
            End If
        End If
    End Sub

    ''' <summary>Brings the opening page to the front (closing the open page first). False = that page refused.</summary>
    Friend Function ShowHome() As Boolean
        Return SwitchBase(False)
    End Function

    ''' <summary>Brings the shell (angajamente tree and views) to the front. False = the open page refused to close.</summary>
    Friend Function ShowShell() As Boolean
        Return SwitchBase(True)
    End Function

    Private Function SwitchBase(k_shell As Boolean) As Boolean
        Try
            If _page IsNot Nothing AndAlso Not _page.IsDisposed Then
                _page.Close()
                If _page IsNot Nothing AndAlso Not _page.IsDisposed Then Return False
            End If
            _baseIsShell = k_shell
            _kbot.Visible = k_shell
            _home.Visible = Not k_shell
            If k_shell Then
                _kbot.BringToFront()
                _kbot.Activate()
            Else
                _home.BringToFront()
                _home.RefreshState()
                RefreshSidebar()
            End If
            UpdateCloseButton()
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.SwitchBase", ex)
            Throw
        End Try
    End Function

    ' The page's own caption bar is not shown here; its table row (AutoSize) collapses with it, a Fill
    ' row is given no height. The bar is returned: its text is the title the caption of this window follows.
    Private Shared Function HidePageTitleBar(k_page As Form) As KBotCaptionBar
        Dim k_first As KBotCaptionBar = Nothing
        For Each k_found As Control In k_page.Controls.Find("capBar", True)
            Dim k_bar As KBotCaptionBar = TryCast(k_found, KBotCaptionBar)
            If k_bar Is Nothing Then Continue For
            If k_first Is Nothing Then k_first = k_bar
            k_bar.Visible = False
            Dim k_table As TableLayoutPanel = TryCast(k_bar.Parent, TableLayoutPanel)
            If k_table IsNot Nothing Then
                Dim k_row As Integer = k_table.GetRow(k_bar)
                If k_row >= 0 AndAlso k_row < k_table.RowStyles.Count Then
                    k_table.RowStyles(k_row) = New RowStyle(SizeType.Absolute, 0.0F)
                End If
            End If
        Next
        Return k_first
    End Function

    ' The title of the page: the text of its own caption bar when it has one (most forms write their title
    ' only there), otherwise the form's text.
    Private Sub UpdatePageCaption()
        If _page Is Nothing OrElse _page.IsDisposed Then Return
        Dim k_title As String = If(_pageBar IsNot Nothing AndAlso _pageBar.Text.Length > 0, _pageBar.Text, _page.Text)
        If k_title.Length > 0 Then _kbot.capBar.Text = k_title
    End Sub

    Private Sub PageTitleChanged(sender As Object, e As EventArgs)
        Try
            UpdatePageCaption()
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.PageTitleChanged", ex)
        End Try
    End Sub

    ' The page closed: the page under it is back, with the caption it had.
    Private Sub Page_FormClosed(sender As Object, e As FormClosedEventArgs)
        Try
            Dim k_closed As Form = TryCast(sender, Form)
            If k_closed IsNot Nothing Then
                RemoveHandler k_closed.FormClosed, AddressOf Page_FormClosed
                RemoveHandler k_closed.TextChanged, AddressOf PageTitleChanged
            End If
            If Not ReferenceEquals(_page, k_closed) Then Return
            If _pageBar IsNot Nothing Then RemoveHandler _pageBar.TextChanged, AddressOf PageTitleChanged
            _pageBar = Nothing
            _page = Nothing
            If _closing OrElse IsDisposed OrElse Disposing OrElse _kbot.IsDisposed Then Return
            _kbot.capBar.Text = _savedCaption
            SetBaseVisible(True)
            UpdateCloseButton()
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.Page_FormClosed", ex)
        End Try
    End Sub

    ' The shell closed by itself (a failed start): the application goes with it.
    Private Sub Shell_FormClosed(sender As Object, e As FormClosedEventArgs)
        Try
            If _closing OrElse IsDisposed OrElse Disposing Then Return
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.Shell_FormClosed", ex)
        End Try
    End Sub
End Class
