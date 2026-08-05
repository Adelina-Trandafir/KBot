Option Strict On
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' Suprafața «Document» a vederii DDF: PDF-ul REAL, deschis cu Adobe și găzduit în panou
''' (felia 0020-03, opțiunea A a planului §6; rescrisă în felia 0024).
'''
''' TOATĂ mecanica Win32 (căutarea ferestrei, reparentarea, curățarea stilurilor, așezarea,
''' redesenarea, detașarea) a fost mutată în <see cref="AdobeReaderHost"/> din KBot.Controls,
''' împreună cu bancul de probă (<c>AdobeReaderHarnessForm</c>). Existau DOUĂ copii ale acelui cod
''' și DEJA divergeau — bancul curăța WS_SYSMENU/WS_MINIMIZEBOX/WS_MAXIMIZEBOX și forța redesenarea,
''' această clasă nu — deci același PDF se comporta diferit în cele două locuri. Ce a rămas aici e
''' doar UI: cele trei stări ale suprafeței, tema și butonul de generare.
'''
''' CE FACE FELIA 0024 ÎN PLUS: găzduirea rulează sub un PROFIL (modern / clasic), ales de setarea
''' «Mod vizualizator Adobe» și, pe «Automat», de generația detectată din arborele de ferestre.
''' Profilul aduce parametrii de lansare, decuparea și poziția măsurate pe banc — vezi
''' <c>docs\SETARI_UTILIZATOR.md</c>.
'''
''' NU SE SCRIE NIMIC ÎN REGISTRY. Bancul scrie <c>bEnableAv2</c> ca să FORȚEZE o generație; aici
''' nu se scrie, fiindcă acea valoare schimbă Adobe-ul operatorului pentru ORICE PDF ar deschide,
''' inclusiv în afara K-BOT.
'''
''' AVERTISMENT (planul §8 ter): NU invoca semnarea (Adobe) cât timp această suprafață ține o
''' fereastră găzduită — lansarea unui mod de semnare peste același proces cere probleme.
''' </summary>
Public Class ReaderHostPreview
    Implements IDdfPreview, IThemedControl

    Public Event GenerateRequested As EventHandler Implements IDdfPreview.GenerateRequested

    Private ReadOnly _host As AdobeReaderHost
    ' Documentul cerut ultima dată — gardă anti-răspuns depășit (același tipar ca vederile).
    Private _requestedPath As String

    Public Sub New()
        InitializeComponent()
        _host = New AdobeReaderHost(pnlHost, AddressOf AdobeHostLog.Write) With {
            .PopupWatchEnabled = True}
        ApplySettings()
        ShowMessage("Selectați o revizie din arbore.")
    End Sub

    Public ReadOnly Property Surface As Control Implements IDdfPreview.Surface
        Get
            Return Me
        End Get
    End Property

    ''' <summary>
    ''' Reia setările operatorului (profil + «instanță nouă») din <c>kbot_paths.json</c>. O valoare
    ''' lipsă sau nerecunoscută cade pe «Automat» ȘI se scrie în jurnal — o setare stricată nu are
    ''' voie să oprească deschiderea unui document, dar nici să dispară în tăcere.
    ''' </summary>
    Public Sub ApplySettings()
        Try
            Dim mode = AdobeViewerSettings.CurrentMode()
            Dim newInstance = AdobeViewerSettings.CurrentNewInstance()
            If mode.HasWarning Then AdobeHostLog.Write("ATENȚIE: " & mode.Warning)
            If newInstance.HasWarning Then AdobeHostLog.Write("ATENȚIE: " & newInstance.Warning)
            _host.Mode = mode.Value
            _host.NewInstanceMode = newInstance.Value
            AdobeHostLog.Write($"Setări gazdă Adobe: mod={AdobeViewerSettings.ModeLabel(mode.Value)}, " &
                               $"instanță nouă={AdobeViewerSettings.NewInstanceLabel(newInstance.Value)}.")
        Catch ex As Exception
            GlobalErrorLog.Write("ReaderHostPreview.ApplySettings", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Aplică setările CURENTE și le reflectă imediat pe documentul deja afișat, dacă există:
    ''' geometria se reaplică pe loc, parametrii de lansare abia la documentul următor (nu se poate
    ''' schimba «/n» al unui proces care rulează deja).
    ''' </summary>
    Public Sub ReapplySettings()
        Try
            ApplySettings()
            _host.ReapplyProfile()
        Catch ex As Exception
            GlobalErrorLog.Write("ReaderHostPreview.ReapplySettings", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Afișează documentul. Fișier lipsă -&gt; suprafața „document lipsă" (contract IDdfPreview,
    ''' niciodată o excepție). La orice document nou detașăm mai întâi fereastra găzduită curent.
    ''' </summary>
    Public Sub ShowDocument(pdfPath As String, exists As Boolean) Implements IDdfPreview.ShowDocument
        Try
            _requestedPath = pdfPath
            _host.Detach()

            If String.IsNullOrWhiteSpace(pdfPath) Then
                ShowMessage("Selectați o revizie din arbore.")
                Return
            End If
            If Not exists Then
                ShowMissing()
                Return
            End If

            ShowHost()
            ' Fire-and-forget deliberat: metoda își tratează singură TOATE erorile (același tipar ca
            ' LoadAsync din vederi — apelantul e un handler sincron, nu există cine să aștepte).
            EmbedAsync(pdfPath)
        Catch ex As Exception
            GlobalErrorLog.Write("ReaderHostPreview.ShowDocument", ex)
            ShowMessage("Documentul nu a putut fi afișat. Detalii în jurnalul de erori.")
        End Try
    End Sub

    ' Graniță UI asincronă: loghează și ÎNGHITE. Fiecare cale de eșec ajunge într-un mesaj românesc
    ' pe ecran — §6: o previzualizare care arată în tăcere un dreptunghi gri e cel mai prost final.
    Private Async Sub EmbedAsync(pdfPath As String)
        Try
            Dim result As AdobeHostResult = Await _host.ShowDocumentAsync(pdfPath).ConfigureAwait(True)

            ' Între timp operatorul a ales altă revizie: răspunsul e depășit, îl aruncăm.
            If Not String.Equals(_requestedPath, pdfPath, StringComparison.Ordinal) Then Return

            Select Case result.Status
                Case AdobeHostStatus.Hosted
                    ShowHost()
                    ' Notă discretă doar când versiunea Adobe nu a fost recunoscută.
                    lblNote.Text = result.Message
                    lblNote.Visible = result.Message.Length > 0
                Case AdobeHostStatus.Superseded
                    ' Nimic de arătat: o cerere mai nouă a preluat controlul.
                Case Else
                    ShowMessage(result.Message)
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("ReaderHostPreview.EmbedAsync", ex)
            ShowMessage("Documentul nu a putut fi afișat. Detalii în jurnalul de erori.")
        End Try
    End Sub

    Private Sub pnlHost_SizeChanged(sender As Object, e As EventArgs) Handles pnlHost.SizeChanged
        Try
            _host.Relayout()
        Catch ex As Exception
            GlobalErrorLog.Write("ReaderHostPreview.pnlHost_SizeChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Eliberează fereastra Adobe găzduită. Apelat din Dispose (vezi Designer) și la fiecare
    ''' schimbare de document.
    ''' </summary>
    Friend Sub DetachReader()
        Try
            _host?.Dispose()
        Catch ex As Exception
            GlobalErrorLog.Write("ReaderHostPreview.DetachReader", ex)
        End Try
    End Sub

    Public Sub Clear() Implements IDdfPreview.Clear
        Try
            _requestedPath = Nothing
            _host.Detach()
            ShowMessage("Selectați o revizie din arbore.")
        Catch ex As Exception
            GlobalErrorLog.Write("ReaderHostPreview.Clear", ex)
        End Try
    End Sub

    Private Sub btnGenereaza_Click(sender As Object, e As EventArgs) Handles btnGenereaza.Click
        RaiseEvent GenerateRequested(Me, EventArgs.Empty)
    End Sub

    ' ── Stări ─────────────────────────────────────────────────────────────────
    Private Sub ShowHost()
        pnlHost.Visible = True
        pnlMissing.Visible = False
        lblMessage.Visible = False
    End Sub

    Private Sub ShowMissing()
        pnlHost.Visible = False
        pnlMissing.Visible = True
        lblMessage.Visible = False
        lblNote.Visible = False
    End Sub

    Private Sub ShowMessage(message As String)
        lblMessage.Text = message
        pnlHost.Visible = False
        pnlMissing.Visible = False
        lblMessage.Visible = True
        lblNote.Visible = False
    End Sub

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            pnlHost.BackColor = p.SurfaceColor
            pnlMissing.BackColor = p.SurfaceAltColor
            tblMissing.BackColor = p.SurfaceAltColor
            lblMissing.ForeColor = p.TextDimColor
            lblMissing.BackColor = Color.Transparent
            lblMessage.ForeColor = p.TextDimColor
            lblMessage.BackColor = p.SurfaceAltColor
            lblNote.ForeColor = p.TextDimColor
            lblNote.BackColor = p.SurfaceAltColor
            btnGenereaza.BackColor = p.AccentColor
            btnGenereaza.ForeColor = p.AccentTextColor
            btnGenereaza.FlatAppearance.BorderColor = p.AccentColor
        Catch ex As Exception
            GlobalErrorLog.Write("ReaderHostPreview.ApplyTheme", ex)
        End Try
    End Sub

End Class
