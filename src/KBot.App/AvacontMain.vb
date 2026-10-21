Imports System.Globalization
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.EFactura
Imports KBot.Theming

''' <summary>
''' The application's main window. It owns the header (caption bar with the year / SS / unit selectors,
''' busy bar, the ribbon) and the side panel, and gives the rest of its area to the page it hosts: the
''' opening page, the shell (<see cref="KbotForm"/>, kept loaded and only shown or hidden) or any page
''' (a stand-alone form hosted as a page: several stay open, one on screen -- slice 0113-01). The windows the
''' operator must answer (editors, links, charts) are not pages: they open as modal windows of their own
''' (<see cref="ShowModalAsync"/>). The header controls of the shell are the same
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
                   k_registry As MachetaRegistry, k_chat As ChatService)
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
        ' The days list of «Sincronizeaza facturi primite» starts with no row picked (its hint «Perioada de adus» shows, as in the
        ' popup of the invoices tree); nothing picked and nothing typed = 7 days (see ReadSyncDays).
        Dim k_daysCombo As KBotRibbonField = ribbon.FindField(KeyPrimiteZileCombo)
        If k_daysCombo IsNot Nothing Then k_daysCombo.SelectedIndex = -1
        ribbon.Compact = AppSettings.Current.RibbonCompact
        ribbon.Collapsed = AppSettings.Current.RibbonCollapsed
        ribbon.RightText = k_shell.lblOperator.Text
        AddHandler ribbon.CollapsedChanged, AddressOf Ribbon_CollapsedChanged
        AddHandler k_shell.MenuStateChanged, AddressOf SyncRibbon
        AddHandler k_shell.ActiveViewChanged, AddressOf Shell_ActiveViewChanged
        AddHandler k_shell.lblOperator.TextChanged, AddressOf Operator_TextChanged
        AddHandler AppSettings.Changed, AddressOf AppSettings_Changed
        HookTutorialSignals()

        ' The opening page. The shell is loaded behind it (OnShown) and reached from the ribbon.
        _home = New AcasaPage(k_session, k_chat)
        PrepareHosted(_home)
        pnlHost.Controls.Add(_home)

        InitSidebar()
        ApplySidebarPlacement()
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
    ' Slice 0113-01: every open page is asked, the most recently used first, and each one is brought to the front
    ' first -- its own question about unsaved changes must be seen, not asked over a page that is hidden.
    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        MyBase.OnFormClosing(e)
        Try
            If e.Cancel OrElse _kbot.IsDisposed Then Return
            Do While _pages.Count > 0
                Dim k_open As Form = _pages(_pages.Count - 1)
                If k_open.IsDisposed Then
                    _pages.Remove(k_open)
                    Continue Do
                End If
                If Not ReferenceEquals(_page, k_open) Then MakeFront(k_open)
                k_open.Close()
                If Not k_open.IsDisposed Then
                    e.Cancel = True   ' the open page refused to close (unsaved changes)
                    Return
                End If
            Loop
            _closing = True
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
                Case "sumar_parteneri"
                    _kbot.OpenSumarPartners()
                Case "rezervari_repara", "rezervari_grafic", "rezervari_adauga", "receptii_reface", "receptii_asociere"
                    _kbot.RunViewRibbonButton(e.Key)
                Case "pagina_angajamente"
                    _kbot.OpenRegistered(KbotForm.MachetaAngajamente, Sub() ShowShell())
                Case "pagina_inchide"
                    CloseCurrent()
                Case "istoric_versiuni"
                    _kbot.OpenRegistered(KbotForm.MachetaIstoricVersiuni, AddressOf _kbot.DeschideIstoricVersiuni)
                Case "efactura_vanzare", "efactura_cumparare", "efactura_receptii", "efactura_vercon"
                    ' the ribbon key is the key of the macheta
                    _kbot.OpenRegistered(e.Key, Sub() _kbot.RunEFacturaCommand(e.Key))
                Case "ef_token"
                    _kbot.RunEFacturaCommand(e.Key)
                Case "ef_primite_sync"
                    RunPrimiteSync()
                Case KeyAccessTrimite, KeyAccessPreia
                    _kbot.RunPrimiteAccess(e.Key)
                Case KeyExtraseDescarca
                    _kbot.DescarcaExtraseDinRibbon()
                Case KeyGrupeEditeaza
                    _kbot.DeschideEditorulGrupe()
                Case "unitate_conturi", "unitate_date"
                    _kbot.RunUnitCommand(e.Key)
                Case Else
                    ' These entries work on the shell's own views and tree: it must be what is on screen.
                    If ShellEntries.Contains(e.Key) AndAlso Not ShowShell() Then Return
                    _kbot.RunMenuEntry(e.Key)
            End Select
            ' Slice 000T-13: a tutorial waits for the press; raised once the command has run, so the page it opens is on screen.
            RaiseEvent TutorialSignal(SignalRibbonPrefix & e.Key)
        Catch ex As Exception
            ' UI boundary (click handler): log and swallow; the entry told the operator what it had to.
            GlobalErrorLog.Write("AvacontMain.Ribbon_ItemClicked", ex)
        End Try
    End Sub

    ' ---- «Sincronizeaza facturi primite» (slice 0114-01 / 00EF-30) ----------------------------------

    Private Const KeyPrimiteSync As String = "ef_primite_sync"
    Private Const KeyPrimiteZileCombo As String = "ef_primite_zile_combo"
    Private Const KeyPrimiteZileText As String = "ef_primite_zile_text"
    ' Slice 00EF-43: the two buttons of the received invoices window that talk to the Access e-invoice store (Access clients only).
    Private Const KeyAccessTrimite As String = "ef_access_trimite"
    Private Const KeyAccessPreia As String = "ef_access_preia"

    ' The rows of the combo, in order (the combo shows them as «Ultimele N zile»); the first is the default.
    Private Shared ReadOnly SyncDayChoices As Integer() = {7, 15, 30, 45, 60}

    ' ---- The group of the FORM on screen (slice 0114-06 / 0114-07) ------------------------------------------------------
    ' Right to left on the ribbon: «Inchide», the group of the FORM (this one), the group of the VIEW («Sumar» ...). What stands in
    ' the form group depends on the form on screen, never on a view of it:
    '   - the shell (KbotMain, with all its views): «Editeaza grupe», «Descarca extrase»;
    '   - the «Extrase de cont» window: «Descarca extrase»;
    '   - the received invoices window: «Sincronizeaza facturi primite» and its two inputs, and -- only for a client that also runs the Access
'     application (slice 00EF-43) -- «Trimite in Access» and «Preia din Access».
    ' The group goes when none of them applies (the ribbon skips a group with nothing visible).

    Private Const KeyExtraseDescarca As String = "extrase_descarca"
    Private Const KeyGrupeEditeaza As String = "grupe_editeaza"

    Private Sub UpdateFormGroup()
        Dim k_page As Form = CurrentPage
        Dim k_shell As Boolean = k_page Is Nothing AndAlso _baseIsShell
        Dim k_extrase As Boolean = TypeOf k_page Is ExtraseForm
        Dim k_primite As Boolean = TypeOf k_page Is PrimiteForm
        Dim k_groups As KBotRibbonButton = ribbon.FindButton(KeyGrupeEditeaza)
        Dim k_download As KBotRibbonButton = ribbon.FindButton(KeyExtraseDescarca)
        Dim k_sync As KBotRibbonButton = ribbon.FindButton(KeyPrimiteSync)
        Dim k_combo As KBotRibbonField = ribbon.FindField(KeyPrimiteZileCombo)
        Dim k_text As KBotRibbonField = ribbon.FindField(KeyPrimiteZileText)
        Dim k_toAccess As KBotRibbonButton = ribbon.FindButton(KeyAccessTrimite)
        Dim k_fromAccess As KBotRibbonButton = ribbon.FindButton(KeyAccessPreia)
        If k_groups Is Nothing OrElse k_download Is Nothing OrElse k_sync Is Nothing OrElse k_combo Is Nothing OrElse k_text Is Nothing OrElse
           k_toAccess Is Nothing OrElse k_fromAccess Is Nothing Then Return
        Dim k_access As Boolean = k_primite AndAlso AccessFeature.Enabled
        Dim k_caption As String = If(k_shell, "Lista angajamentelor", If(k_extrase, "Extrase de cont", If(k_primite, "Facturi primite", String.Empty)))
        Dim k_showGroups As Boolean = k_shell
        Dim k_showDownload As Boolean = k_shell OrElse k_extrase
        If k_groups.Visible = k_showGroups AndAlso k_download.Visible = k_showDownload AndAlso k_sync.Visible = k_primite AndAlso
           k_combo.Visible = k_primite AndAlso k_text.Visible = k_primite AndAlso k_toAccess.Visible = k_access AndAlso k_fromAccess.Visible = k_access AndAlso
           String.Equals(ribbon.FormGroup.Text, k_caption, StringComparison.Ordinal) Then Return
        k_groups.Visible = k_showGroups
        k_download.Visible = k_showDownload
        k_sync.Visible = k_primite
        k_combo.Visible = k_primite
        k_text.Visible = k_primite
        k_toAccess.Visible = k_access
        k_fromAccess.Visible = k_access
        ribbon.FormGroup.Text = k_caption
        ribbon.InvalidateLayout()
    End Sub

    ' The days to bring: the number written in the box when there is one (1 to 60), else the row picked in the list, else 7.
    ' False = the number written is not valid (the operator was told).
    Private Function ReadSyncDays(ByRef k_days As Integer) As Boolean
        Dim k_text As KBotRibbonField = ribbon.FindField(KeyPrimiteZileText)
        Dim k_combo As KBotRibbonField = ribbon.FindField(KeyPrimiteZileCombo)
        Dim k_typed As String = If(k_text Is Nothing, String.Empty, k_text.Text.Trim())
        If k_typed.Length > 0 Then
            Dim k_number As Integer
            If Not Integer.TryParse(k_typed, NumberStyles.None, CultureInfo.InvariantCulture, k_number) OrElse k_number < 1 OrElse k_number > 60 Then
                KBotMessage.Show(Me, "Numărul de zile trebuie să fie între 1 și 60.",
                                 "Sincronizează facturi primite", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return False
            End If
            k_days = k_number
            Return True
        End If
        Dim k_index As Integer = If(k_combo Is Nothing, 0, k_combo.SelectedIndex)
        k_days = SyncDayChoices(Math.Max(0, Math.Min(SyncDayChoices.Length - 1, k_index)))
        Return True
    End Function

    Private Sub RunPrimiteSync()
        Dim k_days As Integer
        If Not ReadSyncDays(k_days) Then Return
        _kbot.SyncPrimite(k_days)
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
        Try
            If Not IsHandleCreated OrElse IsDisposed Then Return
            ' Slice 0114-24: the compact switch of Setari > Aplicatie takes effect at once (no-op when it did not change).
            ribbon.Compact = AppSettings.Current.RibbonCompact
            ApplySidebarPlacement()
            SyncRibbon(sender, e)
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.AppSettings_Changed", ex)
        End Try
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
        ApplySidebarPlacement()
        UpdateFormGroup()
        UpdateViewGroup()
        Dim k_button As KBotRibbonButton = ribbon.FindButton("pagina_inchide")
        If k_button Is Nothing Then Return
        Dim k_visible As Boolean = (_page IsNot Nothing AndAlso Not _page.IsDisposed) OrElse _baseIsShell
        If k_button.Visible = k_visible Then Return
        k_button.Visible = k_visible
        ribbon.InvalidateLayout()
    End Sub

    ' ---- The view group: what the view on screen offers (slice 0084-04 / 0114-03 / 0114-15) -------------------------
    ' The ribbon's RightGroup stands at the right of the panel, left of the form group. Its buttons (and fields) are made in the
    ' designer with Visible = False; this window shows them while their view is on screen and sets the group's caption. The «Sumar»
    ' view has its own button («Asociaza parteneri»); every other view that wants buttons implements IViewRibbon and answers which of
    ' them it wants, in what state, and what each one does. More buttons for the same or another view go into the same group the same way.

    Private Const KeySumarParteneri As String = "sumar_parteneri"

    ' Every button of the view group, with the view that owns it (the keys are the ones written in the designer).
    Private Shared ReadOnly ViewGroupKeys As String() = {KeySumarParteneri,
        "rezervari_repara", "rezervari_grafic", "rezervari_adauga", "receptii_reface", "receptii_asociere"}

    Private Sub UpdateViewGroup()
        Dim k_onShell As Boolean = CurrentPage Is Nothing AndAlso _baseIsShell
        Dim k_sumar As Boolean = k_onShell AndAlso String.Equals(_kbot.ActiveViewKey, "sumar", StringComparison.Ordinal)
        Dim k_view As IViewRibbon = If(k_onShell, _kbot.ActiveViewRibbon, Nothing)
        Dim k_changed As Boolean
        For Each k_key As String In ViewGroupKeys
            Dim k_button As KBotRibbonButton = ribbon.FindButton(k_key)
            If k_button Is Nothing Then Continue For
            Dim k_state As ViewButtonState = ViewButtonState.Hidden
            Dim k_text As String = String.Empty
            If String.Equals(k_key, KeySumarParteneri, StringComparison.Ordinal) Then
                If k_sumar Then k_state = If(_kbot.CanAssociatePartners, ViewButtonState.Enabled, ViewButtonState.Dimmed)
            ElseIf k_view IsNot Nothing AndAlso k_view.RibbonKeys.Contains(k_key) Then
                k_state = k_view.RibbonButtonState(k_key)
                k_text = If(k_view.RibbonButtonText(k_key), String.Empty)
            End If
            Dim k_show As Boolean = k_state <> ViewButtonState.Hidden
            Dim k_enabled As Boolean = k_state = ViewButtonState.Enabled
            If k_button.Visible <> k_show OrElse k_button.Enabled <> k_enabled Then
                k_button.Visible = k_show
                k_button.Enabled = k_enabled
                k_changed = True
            End If
            If k_text.Length > 0 AndAlso Not String.Equals(k_button.Text, k_text, StringComparison.Ordinal) Then
                k_button.Text = k_text
                k_changed = True
            End If
        Next
        Dim k_caption As String = If(k_sumar, "Sumar", If(k_view Is Nothing, String.Empty, k_view.RibbonCaption))
        If k_caption.Length > 0 AndAlso Not String.Equals(ribbon.RightGroup.Text, k_caption, StringComparison.Ordinal) Then
            ribbon.RightGroup.Text = k_caption
            k_changed = True
        End If
        If k_changed Then ribbon.InvalidateLayout()
    End Sub

    ' UI boundary (event of the shell): log and swallow.
    Private Sub Shell_ActiveViewChanged(sender As Object, e As EventArgs)
        Try
            If IsDisposed OrElse _kbot.IsDisposed Then Return
            UpdateViewGroup()
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.Shell_ActiveViewChanged", ex)
        End Try
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        RemoveHandler AppSettings.Changed, AddressOf AppSettings_Changed   ' static event: must end with the window
        TearDownSidebar()
        If ReferenceEquals(Current, Me) Then Current = Nothing
        MyBase.OnFormClosed(e)
    End Sub

    ' ---- Pages: stand-alone forms hosted in the work area; they stay open, one on screen -------------
    ' Slice 0113-01. A page that another page (or the base) covers is put away, not closed: it stays open, hidden, with
    ' what the operator typed in it, and comes back when it is asked for again. Modal windows are not pages (see ShowModal).

    ''' <summary>Every open page, the most recently used LAST (the one on screen, when a page is on screen).</summary>
    Private ReadOnly _pages As New List(Of Form)()
    Private ReadOnly _pageBars As New Dictionary(Of Form, KBotCaptionBar)()
    Private ReadOnly _pageRefs As New Dictionary(Of Form, MachetaRef)()   ' what the history knows each open page as
    Private Shared ReadOnly ShellRef As MachetaRef = MachetaRef.Create(KbotForm.MachetaAngajamente)
    Private _page As Form   ' the page on screen; Nothing = the base (the opening page or the shell)

    ''' <summary>The page shown in place of the shell now; Nothing = the shell.</summary>
    Friend ReadOnly Property CurrentPage As Form
        Get
            Return If(_page IsNot Nothing AndAlso Not _page.IsDisposed, _page, Nothing)
        End Get
    End Property
    Private _savedCaption As String

    ''' <summary>The open page of the given type -- on screen or put away -- or Nothing when there is none.</summary>
    Friend Function FindPage(Of T As Form)() As T
        For Each k_open As Form In _pages
            Dim k_found As T = TryCast(k_open, T)
            If k_found IsNot Nothing AndAlso Not k_open.IsDisposed Then Return k_found
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Shows a stand-alone form as a page of the work area. The page that was on screen (or the shell / the opening page)
    ''' is put away -- <see cref="PutAwayFront"/> first, so no Adobe stays open behind -- and stays open, hidden; a page
    ''' that is open already (put away) just comes back to the front. The page's own title bar goes: its title moves to
    ''' this window's caption bar (and follows it when it changes), and it closes the way it always did (the ribbon's
    ''' «Inchide», Escape, a cancel button). Always True (kept for the callers that tested a refusal to close).
    ''' </summary>
    Friend Function ShowPage(k_page As Form, Optional k_ref As MachetaRef = Nothing) As Boolean
        Try
            ArgumentNullException.ThrowIfNull(k_page)
            If Not ReferenceEquals(_page, k_page) Then
                If _pages.Contains(k_page) Then
                    MakeFront(k_page)   ' open and put away: it comes back
                Else
                    OpenNewPage(k_page)
                End If
            End If
            If k_ref IsNot Nothing Then NotePageOpen(k_page, k_ref)
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.ShowPage", ex)
            Throw
        End Try
    End Function

    ' The history hears that the page is open. Its place among the open machete is the one it got when it opened: asking for it
    ' again changes nothing there. A page that comes back under another reference (another year) closes the old line.
    Private Sub NotePageOpen(k_page As Form, k_ref As MachetaRef)
        Dim k_old As MachetaRef = Nothing
        If _pageRefs.TryGetValue(k_page, k_old) AndAlso Not k_old.Equals(k_ref) Then
            _registry.NoteClosed(k_old)
            DropSnapshot(k_old.Id)
        End If
        _pageRefs(k_page) = k_ref
        _registry.Note(k_ref, True)
    End Sub

    ' Hosts a form that was not open: the front is put away, the form becomes the front.
    Private Sub OpenNewPage(k_page As Form)
        PutFrontAway()
        PrepareHosted(k_page)
        Dim k_bar As KBotCaptionBar = HidePageTitleBar(k_page)
        _pageBars(k_page) = k_bar
        AddHandler k_page.FormClosed, AddressOf Page_FormClosed
        AddHandler k_page.TextChanged, AddressOf PageTitleChanged
        If k_bar IsNot Nothing Then AddHandler k_bar.TextChanged, AddressOf PageTitleChanged
        _pages.Add(k_page)
        _page = k_page
        pnlHost.Controls.Add(k_page)
        k_page.Show()
        k_page.BringToFront()
        UpdatePageCaption()   ' after Show: most forms write their title in Load
        UpdateCloseButton()
    End Sub

    ' Brings an open page (put away) to the front; the front is put away.
    Private Sub MakeFront(k_page As Form)
        If ReferenceEquals(_page, k_page) Then Return
        PutFrontAway()
        _page = k_page
        _pages.Remove(k_page)
        _pages.Add(k_page)   ' the most recently used goes last
        k_page.Visible = True
        k_page.BringToFront()
        UpdatePageCaption()
        UpdateCloseButton()
    End Sub

    ' What is on screen -- a page or the base -- is put away (its Adobe closed, see PutAwayFront) and hidden. A page stays
    ' open; the base keeps the caption it had, for when it comes back.
    Private Sub PutFrontAway()
        PutAwayFront()
        CaptureFront()   ' slice 0115-01: the picture of «Machete deschise», as the operator leaves it
        If _page IsNot Nothing Then
            _page.Visible = False
        Else
            _savedCaption = _kbot.capBar.Text
            SetBaseVisible(False)
        End If
    End Sub

    ''' <summary>
    ''' Slice 0113-01: what is on screen is about to be covered (another page, the other base, or a modal window): it moves to a
    ''' tab without a PDF -- its navigation bar stays shown -- and lets its documents go, so Adobe is closed while nobody looks
    ''' at it. Coming back, the operator is on that tab and picks the PDF tab to open the document again. A failure is logged and
    ''' does not stop the operator from moving on.
    ''' </summary>
    Friend Sub PutAwayFront()
        Try
            If _page IsNot Nothing AndAlso Not _page.IsDisposed Then
                TryCast(_page, IPutAway)?.PutAway()
            ElseIf _baseIsShell Then
                _kbot.PutAway()
            End If
        Catch ex As Exception
            ' UI boundary: closing Adobe is a courtesy to the next window; a failure here must not stop the navigation.
            GlobalErrorLog.Write("AvacontMain.PutAwayFront", ex)
        End Try
    End Sub

    ' ---- Modal windows ---------------------------------------------------------------------------------

    ''' <summary>
    ''' For code that opens a window the operator must answer before going on (the DDF / ORD editors, the reception links, the
    ''' uncorrelated operations, the reservations chart, the groups, the message catalog): a window of its own, with its own
    ''' caption bar, smaller than the work area, centred on the main window (it may cover the side panel), and returns when
    ''' it closed. With no main window (the Debug bench) it is the dialog it always was.
    ''' </summary>
    Friend Shared Function ShowModalAsync(k_owner As Control, k_form As KBotModalForm, k_ref As MachetaRef) As Task(Of Boolean)
        Dim k_main As AvacontMain = Current
        If k_main Is Nothing OrElse k_main.IsDisposed Then
            k_form.ShowDialog(If(k_owner Is Nothing, Nothing, k_owner.FindForm()))
            Return Task.FromResult(True)
        End If
        Return Task.FromResult(k_main.ShowModal(k_form, k_ref))
    End Function

    ' The page or shell on screen is put away first (its Adobe closed: the editors bring their own, and all the viewers
    ' share one Acrobat broker); the window then does not touch the pages, so they are as they were when it closes.
    Private Function ShowModal(k_form As KBotModalForm, k_ref As MachetaRef) As Boolean
        Try
            ArgumentNullException.ThrowIfNull(k_form)
            PutAwayFront()
            FitModal(k_form)
            k_form.ShowDialog(Me)   ' not recorded: a modal window is answered and gone, it is not a macheta to come back to
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.ShowModal", ex)
            Throw
        End Try
    End Function

    ' Smaller than the work area (a margin all round), centred on this window; a window the designer made bigger is cut down.
    ' Maximised, the window fills the area of the pages (under the ribbon, beside the side panel), not the whole screen:
    ' MaximizedBounds (relative to the monitor, as WM_GETMINMAXINFO wants it) says where, and MaximumSize is that area so
    ' Windows does not cut the maximised size back down to the opening size.
    Private Sub FitModal(k_form As KBotModalForm)
        Dim k_margin As Integer = LogicalToDeviceUnits(16)
        Dim k_host As Rectangle = pnlHost.RectangleToScreen(pnlHost.ClientRectangle)
        Dim k_cap As New Size(Math.Max(k_margin, k_host.Width - 2 * k_margin),
                              Math.Max(k_margin, k_host.Height - 2 * k_margin))
        k_form.StartPosition = FormStartPosition.CenterParent
        Dim k_max As Size = k_form.MaximumSize   ' 0 = no limit on that axis
        k_cap = New Size(If(k_max.Width > 0, Math.Min(k_max.Width, k_cap.Width), k_cap.Width),
                         If(k_max.Height > 0, Math.Min(k_max.Height, k_cap.Height), k_cap.Height))
        k_form.MinimumSize = New Size(Math.Min(k_form.MinimumSize.Width, k_cap.Width), Math.Min(k_form.MinimumSize.Height, k_cap.Height))
        k_form.MaximumSize = New Size(If(k_max.Width > 0, Math.Min(k_max.Width, k_host.Width), k_host.Width),
                                      If(k_max.Height > 0, Math.Min(k_max.Height, k_host.Height), k_host.Height))
        Dim k_origin As Point = Screen.FromControl(Me).Bounds.Location
        k_form.SetMaximizedBounds(New Rectangle(k_host.X - k_origin.X, k_host.Y - k_origin.Y, k_host.Width, k_host.Height))
        k_form.Size = New Size(Math.Min(k_form.Width, k_cap.Width), Math.Min(k_form.Height, k_cap.Height))
    End Sub

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

    ''' <summary>Brings the opening page to the front. The open pages stay open behind it (slice 0113-01). Always True.</summary>
    Friend Function ShowHome() As Boolean
        Return SwitchBase(False)
    End Function

    ''' <summary>Brings the shell (angajamente tree and views) to the front. The open pages stay open behind it (slice 0113-01). Always True.</summary>
    Friend Function ShowShell() As Boolean
        Return SwitchBase(True)
    End Function

    Private Function SwitchBase(k_shell As Boolean) As Boolean
        Try
            If _page IsNot Nothing AndAlso Not _page.IsDisposed Then
                ' Slice 0113-01: the page on screen is put away, not closed; the base comes back with the caption it had.
                PutAwayFront()
                CaptureFront()
                _page.Visible = False
                _page = Nothing
                _kbot.capBar.Text = _savedCaption
            ElseIf _baseIsShell AndAlso Not k_shell Then
                PutAwayFront()   ' the shell leaves the screen for the opening page
            End If
            _baseIsShell = k_shell
            ' The list of angajamente is a macheta too: open while it is the base, closed when the opening page takes its place.
            If k_shell Then
                _registry.Note(ShellRef, True)
            Else
                _registry.NoteClosed(ShellRef)
                DropSnapshot(ShellRef.Id)
            End If
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
        Dim k_bar As KBotCaptionBar = Nothing
        _pageBars.TryGetValue(_page, k_bar)
        Dim k_title As String = If(k_bar IsNot Nothing AndAlso k_bar.Text.Length > 0, k_bar.Text, _page.Text)
        If k_title.Length > 0 Then _kbot.capBar.Text = k_title
    End Sub

    Private Sub PageTitleChanged(sender As Object, e As EventArgs)
        Try
            UpdatePageCaption()
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.PageTitleChanged", ex)
        End Try
    End Sub

    ' The page closed (from the ribbon, Escape, or by itself). When it was the one on screen, the page used before it comes
    ' back (slice 0113-01), and with none left the base, with the caption it had. A page closed while put away changes nothing
    ' on screen.
    Private Sub Page_FormClosed(sender As Object, e As FormClosedEventArgs)
        Try
            Dim k_closed As Form = TryCast(sender, Form)
            If k_closed Is Nothing Then Return
            RemoveHandler k_closed.FormClosed, AddressOf Page_FormClosed
            RemoveHandler k_closed.TextChanged, AddressOf PageTitleChanged
            Dim k_bar As KBotCaptionBar = Nothing
            If _pageBars.TryGetValue(k_closed, k_bar) Then
                If k_bar IsNot Nothing Then RemoveHandler k_bar.TextChanged, AddressOf PageTitleChanged
                _pageBars.Remove(k_closed)
            End If
            _pages.Remove(k_closed)
            Dim k_ref As MachetaRef = Nothing
            If _pageRefs.TryGetValue(k_closed, k_ref) Then
                _pageRefs.Remove(k_closed)
                DropSnapshot(k_ref.Id)
                If Not _closing Then _registry.NoteClosed(k_ref)   ' the history: it goes to the end, in italics
            End If
            If Not ReferenceEquals(_page, k_closed) Then Return
            _page = Nothing
            If _closing OrElse IsDisposed OrElse Disposing OrElse _kbot.IsDisposed Then Return
            Dim k_next As Form = Nothing
            For k_index As Integer = _pages.Count - 1 To 0 Step -1
                If Not _pages(k_index).IsDisposed Then
                    k_next = _pages(k_index)
                    Exit For
                End If
            Next
            If k_next IsNot Nothing Then
                _page = k_next
                k_next.Visible = True
                k_next.BringToFront()
                UpdatePageCaption()
            Else
                _kbot.capBar.Text = _savedCaption
                SetBaseVisible(True)
            End If
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
