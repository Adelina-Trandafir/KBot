Option Strict On
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' The «Browser FOREXE» view of the shell (slice 0074): the live FOREXE page, docked into
''' this control's panel, with the floating K-BOT menu inside it - and nothing else. No
''' recording, no toolbars: it is the operator's own window on the site, shown only while a
''' session is connected (the shell hides the nav entry otherwise).
'''
''' <para><b>Tree to page.</b> <see cref="SetContext"/> only RECORDS the tree's selection.
''' The robot is sent after it («adlop - Deschide Angajament.wfl»: list, search, the row's
''' menu, «Modificare», then stop) on exactly two occasions: the operator clicked a node
''' while this view is open (<see cref="DeschideSelectia"/>, from the shell's click handler)
''' and the view was just activated with a node selected. Never on a SetContext that comes
''' from a tree reload: the shell reloads the tree in the middle of the watcher's download,
''' and an open started there would collide with the download on the one browser - and
''' would also drag the operator off the angajament they had just made by hand.</para>
'''
''' <para><b>Page to tree.</b> The menu in the page reports which angajament is on screen
''' (a navigation, a search made by hand, a Wicket re-render); the shell selects that node
''' in its tree and tells the view through <see cref="NoteazaCodulPaginii"/>. The page's
''' code is the truth the selection is compared against, so a node already on the page is
''' never opened twice.</para>
'''
''' <para><b>Docking follows visibility.</b> The browser window is a child of
''' <c>pnlBrowser</c> only while this view is on screen: shown, it docks; hidden (another
''' view was picked), it is released and parked off screen again. A browser taken over by
''' the recorder is not fought over - the view says where it went and offers «Adu browserul
''' aici». On shell close the shell releases it before its handle dies (a Chromium window
''' destroyed together with its host panel takes the session with it).</para>
''' </summary>
Public Class BrowserView
    Implements IAngajamentView, IThemedControl

    Private ReadOnly _controller As ForexeController

    ' What the PAGE shows, as the watcher last reported it; empty = no open angajament.
    Private _codPagina As String = String.Empty
    ' What the TREE has selected, as the shell last told us; empty = nothing.
    Private _codSelectat As String = String.Empty
    ' A dock is in flight (VisibleChanged and the button can both ask within a few ms).
    Private _andocare As Boolean
    ' An open is in flight - a second request for another code waits its turn (the
    ' coordinator would refuse it anyway, being busy) and is retried through the tree.
    Private _deschidere As Boolean
    ' Why the last dock failed, shown in the panel until the next attempt; empty = it did not.
    Private _eroareAndocare As String = String.Empty

    Public Sub New(controller As ForexeController)
        ArgumentNullException.ThrowIfNull(controller)
        InitializeComponent()
        _controller = controller
        AddHandler _controller.StateChanged, AddressOf Controller_StateChanged
        AddHandler Me.Disposed, AddressOf BrowserView_Disposed
        ActualizeazaStarea()
    End Sub

    Public ReadOnly Property ViewKey As String Implements IAngajamentView.ViewKey
        Get
            Return "browser"
        End Get
    End Property

    ''' <summary>The panel the browser is docked into; the shell releases it before closing.</summary>
    Public ReadOnly Property BrowserHost As Control
        Get
            Return pnlBrowser
        End Get
    End Property

    ''' <summary>True while the browser is docked in THIS view.</summary>
    Public ReadOnly Property BrowserEsteAici As Boolean
        Get
            Return _controller.BrowserHost Is pnlBrowser
        End Get
    End Property

    ' ── Tree to page ──────────────────────────────────────────────────────

    ''' <summary>
    ''' The tree's selection - recorded, nothing more (see the header: a reload must not
    ''' start the robot). Nothing = no selection.
    ''' </summary>
    Public Sub SetContext(info As AngajamentTreeInfo) Implements IAngajamentView.SetContext
        Try
            _codSelectat = If(info Is Nothing, String.Empty, If(info.CodAngajament, String.Empty).Trim())
            ActualizeazaStarea()
        Catch ex As Exception
            GlobalErrorLog.Write("BrowserView.SetContext", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' The operator CLICKED a node while this view is open (the shell calls it right after
    ''' SetContext from its click handler): open that angajament in the page, unless the
    ''' page already shows it. Clicking the node the page has left (a search by hand, a
    ''' download that ended on another page) brings it back.
    ''' </summary>
    Public Sub DeschideSelectia()
        Try
            DeschideDacaDifera()
        Catch ex As Exception
            GlobalErrorLog.Write("BrowserView.DeschideSelectia", ex)
        End Try
    End Sub

    ''' <summary>
    ''' The page told the shell which angajament it shows (empty = none). Kept as the truth
    ''' the selection is compared against.
    ''' </summary>
    Public Sub NoteazaCodulPaginii(cod As String)
        Try
            _codPagina = If(cod, String.Empty).Trim()
            ActualizeazaStarea()
        Catch ex As Exception
            GlobalErrorLog.Write("BrowserView.NoteazaCodulPaginii", ex)
        End Try
    End Sub

    ' Sends the robot after the selected node when the page does not show it already.
    Private Sub DeschideDacaDifera()
        If String.IsNullOrEmpty(_codSelectat) Then Return
        If String.Equals(_codSelectat, _codPagina, StringComparison.OrdinalIgnoreCase) Then Return
        If Not _controller.IsConnected OrElse Not BrowserEsteAici Then
            ActualizeazaStarea()
            Return
        End If
        PornesteDeschiderea(_codSelectat)
    End Sub

    ' UI boundary (async Sub): log and tell, never rethrow.
    Private Async Sub PornesteDeschiderea(cod As String)
        If _deschidere Then
            lblStare.Text = $"Se deschide deja un angajament — «{cod}» rămâne pe rând (alegeți-l din nou)."
            Return
        End If
        _deschidere = True
        busy.Running = True
        lblStare.Text = $"Deschid «{cod}» în FOREXE..."
        Try
            Dim ok As Boolean = Await _controller.DeschideAngajamentAsync(cod)
            If ok Then
                ' The watcher reports the page the robot left behind the moment it is handed
                ' back (setSuspended(false) -> reportPage), which sets _codPagina for real.
                lblStare.Text = $"Angajamentul «{cod}» e deschis în FOREXE (Modificare)."
            Else
                lblStare.Text = If(String.IsNullOrEmpty(_controller.LastFailure),
                                   $"«{cod}» nu s-a putut deschide în FOREXE.",
                                   _controller.LastFailure)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("BrowserView.PornesteDeschiderea", ex)
            lblStare.Text = $"«{cod}» nu s-a putut deschide în FOREXE: {ex.Message}"
        Finally
            _deschidere = False
            busy.Running = False
        End Try
    End Sub

    ' ── Docking follows visibility ───────────────────────────────────────

    Private Async Sub BrowserView_VisibleChanged(sender As Object, e As EventArgs) Handles Me.VisibleChanged
        Try
            If Visible Then
                Await AndocheazaAsync()
            Else
                Await ElibereazaAsync()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("BrowserView.VisibleChanged", ex)
        End Try
    End Sub

    ''' <summary>Docks the browser here unless it already is; failures are shown in the view.</summary>
    Private Async Function AndocheazaAsync() As Task
        If IsDisposed OrElse Not Visible Then Return
        If _andocare Then Return
        ' The executor reads the host's handle and refuses a panel without one; a view that
        ' has just been shown for the first time may not have created it yet.
        If Not pnlBrowser.IsHandleCreated Then pnlBrowser.CreateControl()
        If Not pnlBrowser.IsHandleCreated Then Return
        If Not _controller.IsConnected Then
            ActualizeazaStarea()
            Return
        End If
        If BrowserEsteAici Then
            ' Still docked here (the view was hidden and shown without losing it): the
            ' window is re-fitted at once, before the operator can see a misplaced toolbar.
            Await _controller.SyncBrowserBoundsAsync()
            ActualizeazaStarea()
            Return
        End If
        _andocare = True
        _eroareAndocare = String.Empty
        busy.Running = True
        ActualizeazaStarea()
        Try
            Await _controller.DockBrowserAsync(pnlBrowser)
        Catch ex As Exception
            GlobalErrorLog.Write("BrowserView.AndocheazaAsync", ex)
            _eroareAndocare = ex.Message
        Finally
            _andocare = False
            busy.Running = False
            ActualizeazaStarea()
        End Try
        ' The view was opened with a node selected: the page follows it (the second of the
        ' two occasions in the header). The page is asked what it shows FIRST - the "page"
        ' event of the watcher's resume would arrive after this decision - so a page already
        ' on that angajament stays put.
        If BrowserEsteAici Then
            _codPagina = Await _controller.CitesteCodulPaginiiAsync()
            ActualizeazaStarea()
            DeschideDacaDifera()
        End If
    End Function

    ''' <summary>Parks the browser off screen if this view holds it; quiet otherwise.</summary>
    Private Async Function ElibereazaAsync() As Task
        If IsDisposed Then Return
        If Not BrowserEsteAici Then Return
        Try
            Await _controller.ReleaseBrowserAsync(pnlBrowser)
        Catch ex As Exception
            GlobalErrorLog.Write("BrowserView.ElibereazaAsync", ex)
        End Try
    End Function

    Private Async Sub BtnAdu_Click(sender As Object, e As EventArgs) Handles btnAdu.Click
        Try
            Await AndocheazaAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("BrowserView.BtnAdu_Click", ex)
        End Try
    End Sub

    ' The host panel changed size (splitter, window resize): re-fit the docked window.
    Private Sub PnlBrowser_Resize(sender As Object, e As EventArgs) Handles pnlBrowser.Resize
        Try
            If Not BrowserEsteAici Then Return
            tmrResync.Stop()
            tmrResync.Start()
        Catch ex As Exception
            GlobalErrorLog.Write("BrowserView.PnlBrowser_Resize", ex)
        End Try
    End Sub

    Private Async Sub TmrResync_Tick(sender As Object, e As EventArgs) Handles tmrResync.Tick
        tmrResync.Stop()
        Try
            If Not BrowserEsteAici Then Return
            Await _controller.SyncBrowserBoundsAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("BrowserView.TmrResync_Tick", ex)
        End Try
    End Sub

    ' ── State ─────────────────────────────────────────────────────────────

    ' May come from the robot's thread: onto the UI thread first.
    Private Sub Controller_StateChanged(sender As Object, e As EventArgs)
        Try
            If IsDisposed OrElse Disposing OrElse Not IsHandleCreated Then Return
            If InvokeRequired Then
                BeginInvoke(New Action(AddressOf ActualizeazaStarea))
            Else
                ActualizeazaStarea()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("BrowserView.Controller_StateChanged", ex)
        End Try
    End Sub

    ''' <summary>Everything that depends on where the browser is, in one place.</summary>
    Private Sub ActualizeazaStarea()
        Try
            Dim conectat As Boolean = _controller.IsConnected
            Dim aici As Boolean = conectat AndAlso BrowserEsteAici

            lblGol.Visible = Not aici
            btnAdu.Visible = conectat AndAlso Not aici AndAlso Not _andocare

            If Not conectat Then
                lblGol.Text = "Nu există o sesiune FOREXE. Conectați-vă din banda de jos."
                lblStare.Text = "Browser FOREXE — neconectat"
                _codPagina = String.Empty
            ElseIf aici Then
                If Not _deschidere Then
                    lblStare.Text = If(String.IsNullOrEmpty(_codPagina),
                                       "Browser FOREXE — alegeți un angajament din listă ca să-l deschidă aici.",
                                       $"Browser FOREXE — angajamentul «{_codPagina}» e în pagină.")
                End If
            Else
                If _andocare Then
                    lblGol.Text = "Se andochează browserul..."
                ElseIf Not String.IsNullOrEmpty(_eroareAndocare) Then
                    lblGol.Text = "Browserul nu a putut fi andocat: " & _eroareAndocare
                Else
                    lblGol.Text = "Browserul FOREXE e afișat în altă fereastră (Recorder) sau e ascuns."
                End If
                If Not _deschidere Then lblStare.Text = "Browser FOREXE — browserul nu e în această vedere."
            End If
        Catch ex As Exception
            ' UI boundary (called from events): log and swallow.
            GlobalErrorLog.Write("BrowserView.ActualizeazaStarea", ex)
        End Try
    End Sub

    ' The coordinator is a singleton: a subscription left behind would keep the view alive.
    Private Sub BrowserView_Disposed(sender As Object, e As EventArgs)
        Try
            RemoveHandler _controller.StateChanged, AddressOf Controller_StateChanged
        Catch ex As Exception
            GlobalErrorLog.Write("BrowserView.BrowserView_Disposed", ex)
        End Try
    End Sub

    ' ── Theme ─────────────────────────────────────────────────────────────

    ''' <summary>
    ''' The view sits on the shell's card (SurfaceAlt). Being IThemedControl, ThemeManager
    ''' does not recurse into the children, so each one is dressed here.
    ''' </summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p = scheme.Palette
            BackColor = p.SurfaceAltColor
            pnlAntet.BackColor = p.SurfaceAltColor
            pnlBrowser.BackColor = p.SurfaceAltColor
            lblStare.BackColor = p.SurfaceAltColor
            lblStare.ForeColor = p.TextColor
            lblGol.BackColor = p.SurfaceAltColor
            lblGol.ForeColor = p.TextDimColor
            ButtonStyles.ApplySecondary(btnAdu, scheme)
            busy.ApplyTheme(scheme)
        Catch ex As Exception
            ' UI boundary (theme cascade): log and swallow.
            GlobalErrorLog.Write("BrowserView.ApplyTheme", ex)
        End Try
    End Sub

End Class
