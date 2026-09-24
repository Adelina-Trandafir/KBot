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
''' inclusiv în afara K-BOT. (Singura excepție, slice 0078: <c>AdobePrefs</c>, dialogul standard
''' de salvare -- vezi mai jos.)
'''
''' SIGNING (slice 0078, replaces the old «never sign while hosted» warning): the operator signs
''' INSIDE this surface. The host's Save As trap is ALWAYS on here, so a save can only overwrite the
''' document on screen, never land elsewhere. <see cref="Signing"/> (set by the page, may be
''' Nothing) is told about every trapped save and decides whether to upload.
''' </summary>
Public Class ReaderHostPreview
    Implements IDdfPreview, IThemedControl

    Public Event GenerateRequested As EventHandler Implements IDdfPreview.GenerateRequested

    ' Slice 0078: the Adobe «standard Save As dialog» preference is written once per process.
    Private Shared _savePrefDone As Boolean

    ''' <summary>
    ''' Slice 0078: the signing session of the document on screen (Nothing = nobody uploads, but the
    ''' Save As trap still keeps every save on the same path). Set by the page BEFORE ShowDocument.
    ''' </summary>
    <System.ComponentModel.Browsable(False),
     System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property Signing As PdfSigningSession

    ''' <summary>Slice 0078: a trapped save finished (argument = the document path). For the bench.</summary>
    Public Event DocumentSaved As Action(Of String)
    ''' <summary>Slice 0078: a save was cancelled by the trap (argument = Romanian reason). For the bench.</summary>
    Public Event SaveCancelled As Action(Of String)

    ' How the Adobe window is let go when the document changes (A kills the process we started,
    ' B closes the window and keeps the process warm) and whether the floating popup is hunted:
    ' until slice 0072 a compile-time constant, now the operator's choice in «Setări» ->
    ' Documente, read through AdobeHostSettings in ApplySettings (same place as the profile).

    Private ReadOnly _host As AdobeReaderHost
    ' Suprafața ActiveX, creată LENEȘ: dacă operatorul nu cere motorul «ActiveX», controlul COM nu
    ' se încarcă niciodată în proces.
    Private _acro As AcroPdfSurface
    Private _engine As AdobePreviewEngine = AdobePreviewEngine.WindowHost
    ' Documentul cerut ultima dată — gardă anti-răspuns depășit (același tipar ca vederile).
    Private _requestedPath As String

    Public Sub New()
        InitializeComponent()
        _host = New AdobeReaderHost(pnlHost, AddressOf AdobeHostLog.Write) With {
            .PopupWatchEnabled = True}
        ' Slice 0078: every save of the hosted Adobe goes back onto the document on screen.
        _host.SaveTrapEnabled = True
        AddHandler _host.DocumentSaved, AddressOf OnDocumentSaved
        AddHandler _host.SaveTrapFailed, AddressOf OnSaveTrapFailed
        ' În DESIGNER nu citim setările și nu scriem jurnal (0025-05, de când controlul e declarat
        ' în DdfView.Designer.vb și deci se construiește pe suprafața de design): `AppDir` e acolo
        ' folderul lui devenv.exe, deci `kbot_paths.json` lipsește oricum, iar singurul efect real
        ' ar fi un `adobe_preview.log` scris lângă Visual Studio, la fiecare redeschidere a vederii.
        ' Restul constructorului e inert prin construcție — AdobeReaderHost și cele patru obiecte
        ' ale lui doar își setează câmpurile; nici cronometrul de popup-uri, nici cârligul de
        ' ferestre nu pornesc până la ShowDocument.
        If Not KBotDesignTime.IsDesignTime(Me) Then ApplySettings()
        ShowMessage("Selectați o revizie din arbore.")
    End Sub

    ''' <summary>Motorul cerut acum. Public pentru vederea care oferă comutatorul.</summary>
    Public ReadOnly Property Engine As AdobePreviewEngine
        Get
            Return _engine
        End Get
    End Property

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
            Dim engine = AdobeViewerSettings.CurrentEngine()
            If mode.HasWarning Then AdobeHostLog.Write("ATENȚIE: " & mode.Warning)
            If newInstance.HasWarning Then AdobeHostLog.Write("ATENȚIE: " & newInstance.Warning)
            If engine.HasWarning Then AdobeHostLog.Write("ATENȚIE: " & engine.Warning)
            _host.Mode = mode.Value
            _host.NewInstanceMode = newInstance.Value
            _engine = engine.Value
            ' Detach mode + popup watch (slice 0072): the operator's, from app_settings.json.
            AdobeHostSettings.ApplyTo(_host, AddressOf AdobeHostLog.Write)
            AdobeHostLog.Write($"Setări gazdă Adobe: motor={AdobeViewerSettings.EngineLabel(engine.Value)}, " &
                               $"mod={AdobeViewerSettings.ModeLabel(mode.Value)}, " &
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
            ' Ambele motoare folosesc ACELAȘI panou, deci cel nefolosit trebuie să-l elibereze —
            ' altfel controlul ActiveX ar rămâne peste fereastra reparentată, sau invers.
            ReleaseUnusedEngine()
            _host.Detach()
            ' Setting "control Adobe nou la fiecare document": the ActiveX control of the previous
            ' document is closed and destroyed here, on the click; the load creates a new one. A
            ' reused control sometimes stayed empty after LoadFile (client trace 24.09.2026).
            If UsesActiveX() AndAlso AppSettings.Current.AcroPdfFreshControl Then _acro?.Clear()

            If String.IsNullOrWhiteSpace(pdfPath) Then
                ShowMessage("Selectați o revizie din arbore.")
                Return
            End If
            If Not exists Then
                ShowMissing()
                Return
            End If

            ' Slice 0078: before Adobe starts, so the preference is already read by it.
            If Not _savePrefDone Then
                _savePrefDone = True
                AdobeHostLog.Write(AdobePrefs.EnsureStandardSaveDialog())
            End If

            ' Starea de așteptare rămâne pe ecran până când fereastra e chiar încorporată. Panoul
            ' gazdă NU se arată acum: fereastra Adobe încă nu există, iar un dreptunghi gol nu spune
            ' nimic operatorului.
            ShowLoading()
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
            If UsesActiveX() Then
                Await EmbedWithActiveXAsync(pdfPath).ConfigureAwait(True)
                Return
            End If

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

    ''' <summary>
    ''' Calea ActiveX: încarcă documentul în controlul AcroPDF ȘI îl aduce în starea cerută —
    ''' aranjarea trezită și panourile colapsate, adică exact ce face butonul «Colapsează panourile»
    ''' de pe banc. Totul e în <see cref="AcroPdfSurface"/>, o singură implementare pentru amândouă.
    '''
    ''' Dacă AcroPDF nu e înregistrat pe mașină, NU cădem în tăcere pe cealaltă cale: operatorul a
    ''' cerut explicit acest motor, iar o comutare tăcută i-ar ascunde că setarea lui nu s-a aplicat.
    ''' </summary>
    Private Async Function EmbedWithActiveXAsync(pdfPath As String) As Task
        Dim surface As AcroPdfSurface = EnsureAcroSurface()
        If surface Is Nothing Then
            ShowMessage("Documentul nu a putut fi afișat. Detalii în jurnalul de erori.")
            Return
        End If

        ' Engine "ActiveX -- mod citire": toolbars hidden by Adobe's Read Mode (Ctrl+H) instead of
        ' the collapse / hide / header timers. Read at every load, so a change in the settings
        ' window applies to the next document.
        surface.ReadMode = (_engine = AdobePreviewEngine.ActiveXReadMode)
        Dim result As AcroPdfResult = Await surface.ShowDocumentAsync(pdfPath).ConfigureAwait(True)
        If Not String.Equals(_requestedPath, pdfPath, StringComparison.Ordinal) Then Return

        If result.Succeeded Then
            ShowHost()
            ' Nota discretă spune doar ce NU a mers: colapsarea e vizibilă, absența ei nu.
            If result.Collapsed Then
                lblNote.Visible = False
            Else
                ' The surface says WHY when it knows (e.g. Adobe showed nothing at all); otherwise
                ' the only thing missing is the collapse.
                lblNote.Text = If(result.Message.Length > 0, result.Message,
                                  "Panourile Adobe nu au putut fi colapsate — vezi jurnalul.")
                lblNote.Visible = True
            End If
        Else
            ShowMessage(result.Message)
        End If
    End Function

    ' Both ActiveX engines (old path and Read Mode) use the same AcroPDF surface.
    Private Function UsesActiveX() As Boolean
        Return _engine = AdobePreviewEngine.ActiveX OrElse _engine = AdobePreviewEngine.ActiveXReadMode
    End Function

    Private Function EnsureAcroSurface() As AcroPdfSurface
        Try
            If _acro Is Nothing Then
                _acro = New AcroPdfSurface(pnlHost, AddressOf AdobeHostLog.Write)
                ' Slice 0078: the ActiveX engine traps Save As exactly like the hosted window.
                _acro.SaveTrapEnabled = True
                AddHandler _acro.DocumentSaved, AddressOf OnDocumentSaved
                AddHandler _acro.SaveTrapFailed, AddressOf OnSaveTrapFailed
            End If
            Return _acro
        Catch ex As Exception
            GlobalErrorLog.Write("ReaderHostPreview.EnsureAcroSurface", ex)
            Return Nothing
        End Try
    End Function

    ' Slice 0078 -- host events, raised on the UI thread. UI boundary: log and swallow.
    Private Sub OnDocumentSaved(path As String)
        Try
            Signing?.NotifySaved(path)
            RaiseEvent DocumentSaved(path)
        Catch ex As Exception
            GlobalErrorLog.Write("ReaderHostPreview.OnDocumentSaved", ex)
        End Try
    End Sub

    Private Sub OnSaveTrapFailed(reason As String)
        Try
            RaiseEvent SaveCancelled(reason)
            If Signing IsNot Nothing Then
                Signing.NotifySaveCancelled(reason)
            Else
                KBotMessage.Show(FindForm(),
                                 "Salvarea documentului a fost oprită de K-BOT, ca fișierul să nu ajungă în alt loc." &
                                 Environment.NewLine & reason,
                                 "Salvare oprită", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("ReaderHostPreview.OnSaveTrapFailed", ex)
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
            DisposeAcro()
        Catch ex As Exception
            GlobalErrorLog.Write("ReaderHostPreview.DetachReader", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Eliberează panoul de motorul care NU e cerut acum. Cele două nu pot coexista: Adobe e practic
    ''' mono-instanță, iar controlul in-process e servit de același motor, deci același document nu
    ''' poate fi deschis simultan în amândouă (măsurat 05.08.2026 — a doua cerere dă un panou gri).
    ''' </summary>
    Private Sub ReleaseUnusedEngine()
        If UsesActiveX() Then
            _host.Detach()
        ElseIf _acro IsNot Nothing Then
            DisposeAcro()
        End If
    End Sub

    Private Sub DisposeAcro()
        If _acro Is Nothing Then Return
        RemoveHandler _acro.DocumentSaved, AddressOf OnDocumentSaved
        RemoveHandler _acro.SaveTrapFailed, AddressOf OnSaveTrapFailed
        _acro.Dispose()
        _acro = Nothing
    End Sub

    Public Sub Clear() Implements IDdfPreview.Clear
        Try
            _requestedPath = Nothing
            _host.Detach()
            _acro?.Clear()
            ShowMessage("Selectați o revizie din arbore.")
        Catch ex As Exception
            GlobalErrorLog.Write("ReaderHostPreview.Clear", ex)
        End Try
    End Sub

    Private Sub btnGenereaza_Click(sender As Object, e As EventArgs) Handles btnGenereaza.Click
        RaiseEvent GenerateRequested(Me, EventArgs.Empty)
    End Sub

    ' ── Stări ─────────────────────────────────────────────────────────────────
    ' Patru stări care se exclud reciproc. Fiecare metodă le setează pe TOATE, ca o stare nouă să nu
    ' poată lăsa în urmă un panou vechi vizibil peste ea.
    Private Sub ShowHost()
        pnlHost.Visible = True
        pnlLoading.Visible = False
        pnlMissing.Visible = False
        lblMessage.Visible = False
        pnlHost.BringToFront()
    End Sub

    ''' <summary>
    ''' Starea de așteptare. <c>pnlHost</c> rămâne VIZIBIL, doar acoperit de <c>pnlLoading</c> — nu
    ''' se ascunde. Fereastra Adobe se reparentează pe handle-ul lui cât timp mesajul e pe ecran, iar
    ''' un panou ascuns e un loc prost în care să muți o fereastră străină: WinForms poate recrea
    ''' handle-ul unui control la schimbări de vizibilitate, iar asta ar orfaniza fereastra deja
    ''' încorporată.
    ''' </summary>
    Private Sub ShowLoading()
        pnlHost.Visible = True
        pnlLoading.Visible = True
        pnlMissing.Visible = False
        lblMessage.Visible = False
        lblNote.Visible = False
        pnlLoading.BringToFront()
    End Sub

    Private Sub ShowMissing()
        pnlHost.Visible = False
        pnlLoading.Visible = False
        pnlMissing.Visible = True
        lblMessage.Visible = False
        lblNote.Visible = False
    End Sub

    Private Sub ShowMessage(message As String)
        lblMessage.Text = message
        pnlHost.Visible = False
        pnlLoading.Visible = False
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
            pnlLoading.BackColor = p.SurfaceAltColor
            lblLoading.ForeColor = p.TextDimColor
            lblLoading.BackColor = p.SurfaceAltColor
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
