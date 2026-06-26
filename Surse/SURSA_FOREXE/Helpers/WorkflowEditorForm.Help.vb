Imports System.Drawing
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Windows.Forms
Imports Markdig

' ┌─────────────────────────────────────────────────────────────────────────┐
' │  WorkflowEditorForm — Help                                              │
' │                                                                         │
' │  Panel2 al SplitContainer-ului conține acum un sistem custom de tab-uri │
' │  (Panel + Button-uri flat, fără TabControl) cu două "tab-uri":          │
' │    "🗺 Hartă"  → DocMapTree (existent, mutat la runtime)                │
' │    "❓ Ajutor" → WebBrowser cu HTML generat din wfl_help.md via Markdig │
' │                + slider zoom 75–200% (COM cu fallback CSS)              │
' │                                                                         │
' │  Integrare în fișierele existente (2 linii):                            │
' │    WorkflowEditorForm.vb — Constructor, după InitializeComponent():     │
' │        InitHelpPanel()                                                  │
' │                                                                         │
' │    WorkflowEditorForm.vb — RtbEditor_SelectionChanged, după             │
' │    SyncDocMapSelection(curLineIdx):                                      │
' │        UpdateHelpForLine(curLineIdx)                                    │
' └─────────────────────────────────────────────────────────────────────────┘
Partial Public Class WorkflowEditorForm

    ' =========================================================================
    ' CONTROALE — tab system custom
    ' =========================================================================
    Private _tabContainer As Panel     ' outer container → Panel2 (DockStyle.Fill)
    Private _tabHeader As Panel     ' bara de tab-uri (DockStyle.Top, 32px)
    Private _btnTabHarta As Button    ' buton "🗺 Hartă"
    Private _btnTabAjutor As Button    ' buton "❓ Ajutor"
    Private _tabContent As Panel     ' zona de conținut (DockStyle.Fill)
    Private _panelHarta As Panel     ' conține DocMapTree
    Private _panelAjutor As Panel     ' conține _helpBrowser + _zoomPanel

    Private _helpBrowser As WebBrowser
    Private _zoomPanel As Panel
    Private _zoomSlider As TrackBar
    Private _zoomLabel As Label

    ' =========================================================================
    ' DATE HELP
    ' =========================================================================
    Private Shared _helpDict As Dictionary(Of String, String)
    Private _currentHelpTag As String = Nothing
    Private _isAjutorActive As Boolean = False

    ' =========================================================================
    ' ZOOM STATE
    ' =========================================================================
    Private _zoomPercent As Integer = 150
    Private _comZoomOk As Boolean? = Nothing   ' Nothing=neîncercat, True=COM, False=CSS
    Private _suppressZoom As Boolean = False

    ' =========================================================================
    ' CULORI tab system (mutable — se schimbă cu tema dark/light)
    ' =========================================================================
    Private Shared CLR_TAB_BG As Color = Color.FromArgb(37, 37, 38)
    Private Shared CLR_TAB_ACTIVE As Color = Color.FromArgb(30, 30, 30)
    Private Shared CLR_TAB_INACTIVE As Color = Color.FromArgb(50, 50, 52)
    Private Shared CLR_TAB_BORDER As Color = Color.FromArgb(0, 122, 204)
    Private Shared CLR_TAB_TEXT_ON As Color = Color.FromArgb(255, 255, 255)
    Private Shared CLR_TAB_TEXT_OFF As Color = Color.FromArgb(160, 160, 160)

    ' =========================================================================
    ' INIT — apelat din constructor, după InitializeComponent()
    ' =========================================================================
    Friend Sub InitHelpPanel()

        ' ── Container outer ──────────────────────────────────────────────────
        _tabContainer = New Panel() With {
            .Dock = DockStyle.Fill,
            .BackColor = CLR_TAB_BG,
            .Padding = New Padding(0)
        }

        ' ── Header cu butoane flat ───────────────────────────────────────────
        _tabHeader = New Panel() With {
            .Dock = DockStyle.Top,
            .Height = 32,
            .BackColor = CLR_TAB_BG,
            .Padding = New Padding(0)
        }

        _btnTabHarta = MakeTabButton("🗺 Hartă")
        _btnTabHarta.Dock = DockStyle.Left
        _btnTabHarta.Width = 100

        _btnTabAjutor = MakeTabButton("❓ Ajutor")
        _btnTabAjutor.Dock = DockStyle.Left
        _btnTabAjutor.Width = 100

        AddHandler _btnTabHarta.Click, AddressOf OnTabHartaClick
        AddHandler _btnTabAjutor.Click, AddressOf OnTabAjutorClick
        AddHandler _btnTabHarta.Paint, AddressOf OnTabButtonPaint
        AddHandler _btnTabAjutor.Paint, AddressOf OnTabButtonPaint

        ' Adăugare în header (stânga→dreapta: Hartă, Ajutor)
        _tabHeader.Controls.Add(_btnTabAjutor)  ' adăugat primul → apare la dreapta cu Dock=Left
        _tabHeader.Controls.Add(_btnTabHarta)   ' adăugat al doilea → apare la stânga

        ' ── Zona de conținut ────────────────────────────────────────────────
        _tabContent = New Panel() With {
            .Dock = DockStyle.Fill,
            .BackColor = CLR_TAB_BG,
            .Padding = New Padding(0)
        }

        ' ── Panel Hartă (conține DocMapTree) ────────────────────────────────
        _panelHarta = New Panel() With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.FromArgb(37, 37, 38),
            .Padding = New Padding(0)
        }
        SplitContainer1.Panel2.Controls.Remove(DocMapTree)
        SplitContainer1.Panel2.HorizontalScroll.Enabled = False
        DocMapTree.Dock = DockStyle.Fill
        _panelHarta.Controls.Add(DocMapTree)

        ' ── Panel Ajutor (conține browser + zoom strip) ──────────────────────
        _panelAjutor = New Panel() With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.FromArgb(30, 30, 30),
            .Padding = New Padding(0)
        }

        _helpBrowser = New WebBrowser() With {
            .Dock = DockStyle.Fill,
            .IsWebBrowserContextMenuEnabled = False,
            .WebBrowserShortcutsEnabled = False,
            .AllowNavigation = True,
            .AllowWebBrowserDrop = False
        }

        ' Blochează navigările externe
        AddHandler _helpBrowser.Navigating,
            Sub(s As Object, ev As WebBrowserNavigatingEventArgs)
                Dim url = ev.Url?.ToString()
                If url IsNot Nothing AndAlso
                   Not url.StartsWith("about:") AndAlso
                   Not url.StartsWith("res:") Then
                    ev.Cancel = True
                End If
            End Sub

        ' La fiecare DocumentCompleted aplică zoom (COM persistent sau CSS la fiecare load)
        AddHandler _helpBrowser.DocumentCompleted,
            Sub(s As Object, ev As WebBrowserDocumentCompletedEventArgs)
                If _comZoomOk <> False Then
                    Dim ok = TryApplyComZoom(_zoomPercent)
                    If _comZoomOk Is Nothing Then _comZoomOk = ok
                    If Not ok Then _comZoomOk = False
                End If
                If _comZoomOk = False Then ApplyZoomCss(_zoomPercent)
            End Sub

        ' ── Zoom strip (Dock=Bottom în _panelAjutor) ─────────────────────────
        _zoomPanel = New Panel() With {
            .Dock = DockStyle.Bottom,
            .Height = 28,
            .BackColor = Color.FromArgb(45, 45, 48),
            .Padding = New Padding(8, 0, 8, 0)
        }

        _zoomLabel = New Label() With {
            .Text = "🔎 150%",
            .Dock = DockStyle.Right,
            .Width = 72,
            .ForeColor = Color.White,
            .Font = New Font("Segoe UI", 8.5F),
            .TextAlign = ContentAlignment.MiddleLeft
        }

        _zoomSlider = New TrackBar() With {
            .Dock = DockStyle.Fill,
            .Minimum = 75,
            .Maximum = 200,
            .Value = 100,
            .TickFrequency = 25,
            .SmallChange = 5,
            .LargeChange = 25,
            .BackColor = Color.FromArgb(45, 45, 48),
            .AutoSize = False
        }

        AddHandler _zoomSlider.ValueChanged, AddressOf OnZoomChanged

        _zoomPanel.Controls.Add(_zoomSlider)    ' Dock=Fill
        _zoomPanel.Controls.Add(_zoomLabel)     ' Dock=Right (procesat primul → dreapta)

        ' Panelul Ajutor: Fill primul, Bottom ultimul
        ' (WinForms procesează dock invers ordinii de adăugare)
        _panelAjutor.Controls.Add(_helpBrowser) ' Dock=Fill
        _panelAjutor.Controls.Add(_zoomPanel)   ' Dock=Bottom

        ' Navigare inițială obligatorie
        _helpBrowser.Navigate("about:blank")

        ' ── Asamblare finală ─────────────────────────────────────────────────
        ' _tabContent va afișa _panelHarta sau _panelAjutor, nu amândouă
        _tabContent.Controls.Add(_panelHarta)   ' primul tab vizibil implicit
        _tabContent.Controls.Add(_panelAjutor)

        ' Adaugă în container (Fill înainte de Top din perspectiva dock-ului,
        ' dar ordinea Controls.Add trebuie: Top primul, Fill ultimul)
        _tabContainer.Controls.Add(_tabContent)  ' Dock=Fill  — adăugat ultimul
        _tabContainer.Controls.Add(_tabHeader)   ' Dock=Top   — adăugat primul

        SplitContainer1.Panel2.Controls.Add(_tabContainer)

        ' Activează tab-ul Hartă implicit
        ActivateTab(isAjutor:=False)

        ' Încarcă fișierul de help
        Dim helpPath = Path.Combine(Application.StartupPath, "Helpers", "wfl_help.md")
        LoadHelpFile(helpPath)

        ShowWelcome()
    End Sub

    ' =========================================================================
    ' FACTORY — buton tab flat
    ' =========================================================================
    Private Shared Function MakeTabButton(text As String) As Button

        Dim btn As New Button() With {
        .Text = text,
        .FlatStyle = FlatStyle.Flat,
        .Font = New Font("Segoe UI", 8.5F),
        .ForeColor = CLR_TAB_TEXT_OFF,
        .BackColor = CLR_TAB_INACTIVE,
        .Height = 32,
        .TabStop = False
    }

        btn.FlatAppearance.BorderSize = 0
        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 60, 64)
        btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 100, 170)

        Return btn
    End Function

    ' =========================================================================
    ' PAINT — linie albastră sub tab-ul activ
    ' =========================================================================
    Private Sub OnTabButtonPaint(sender As Object, e As PaintEventArgs)
        Dim btn = DirectCast(sender, Button)
        Dim isActive = (btn Is _btnTabHarta AndAlso Not _isAjutorActive) OrElse
                       (btn Is _btnTabAjutor AndAlso _isAjutorActive)
        If isActive Then
            Using p As New Pen(CLR_TAB_BORDER, 2)
                e.Graphics.DrawLine(p, 0, btn.Height - 2, btn.Width - 1, btn.Height - 2)
            End Using
        End If
    End Sub

    ' =========================================================================
    ' ACTIVARE TAB
    ' =========================================================================
    Private Sub ActivateTab(isAjutor As Boolean)
        _isAjutorActive = isAjutor

        ' Culori butoane
        _btnTabHarta.BackColor = If(Not isAjutor, CLR_TAB_ACTIVE, CLR_TAB_INACTIVE)
        _btnTabHarta.ForeColor = If(Not isAjutor, CLR_TAB_TEXT_ON, CLR_TAB_TEXT_OFF)
        _btnTabAjutor.BackColor = If(isAjutor, CLR_TAB_ACTIVE, CLR_TAB_INACTIVE)
        _btnTabAjutor.ForeColor = If(isAjutor, CLR_TAB_TEXT_ON, CLR_TAB_TEXT_OFF)
        _btnTabHarta.Invalidate()
        _btnTabAjutor.Invalidate()

        ' Swap conținut
        _panelHarta.Visible = Not isAjutor
        _panelAjutor.Visible = isAjutor

        ' Zoom strip vizibil doar pe Ajutor
        If _zoomPanel IsNot Nothing Then _zoomPanel.Visible = isAjutor
    End Sub

    ' =========================================================================
    ' CLICK EVENTS
    ' =========================================================================
    Private Sub OnTabHartaClick(sender As Object, e As EventArgs)
        If Not _isAjutorActive Then Return
        ActivateTab(isAjutor:=False)
    End Sub

    Private Sub OnTabAjutorClick(sender As Object, e As EventArgs)
        If _isAjutorActive Then Return
        ActivateTab(isAjutor:=True)

        ' Afișăm help-ul pentru tagul curent când userul deschide tab-ul
        Dim lineIdx = rtbEditor.GetLineFromCharIndex(rtbEditor.SelectionStart)
        UpdateHelpForLine(lineIdx)
    End Sub

    ' =========================================================================
    ' UPDATE — apelat din RtbEditor_SelectionChanged
    ' =========================================================================
    Friend Sub UpdateHelpForLine(lineIdx As Integer)
        If Not _isAjutorActive Then Return

        Dim tagName As String = ""
        If lineIdx >= 0 AndAlso lineIdx < _lineCache.Count Then
            tagName = If(_lineCache(lineIdx)?.TagName, "")
        End If

        If String.Equals(tagName, _currentHelpTag, StringComparison.OrdinalIgnoreCase) Then Return
        _currentHelpTag = tagName

        If String.IsNullOrEmpty(tagName) Then
            ShowWelcome()
            Return
        End If

        Dim md As String = Nothing
        If _helpDict Is Nothing OrElse Not _helpDict.TryGetValue(tagName, md) Then
            ShowUndocumented(tagName)
            Return
        End If

        RenderMarkdown(tagName, md)
    End Sub

    ' =========================================================================
    ' RENDER
    ' =========================================================================
    Private Sub RenderMarkdown(tagName As String, markdownBody As String)
        Try
            Dim pipeline = New MarkdownPipelineBuilder().
                               UseAdvancedExtensions().
                               Build()
            Dim htmlBody = Markdig.Markdown.ToHtml(markdownBody, pipeline)
            _helpBrowser.DocumentText = BuildPage(
                $"<h2 class='tag-title'>&lt;{tagName}&gt;</h2>" & htmlBody)
        Catch ex As Exception
            _helpBrowser.DocumentText = BuildPage(
                $"<h2 class='tag-title'>&lt;{tagName}&gt;</h2><pre>{Net.WebUtility.HtmlEncode(markdownBody)}</pre>")
        End Try
    End Sub

    Private Sub ShowWelcome()
        _currentHelpTag = Nothing
        _helpBrowser.DocumentText = BuildPage(
            "<p class='hint'>Plasează cursorul pe un tag în editor " &
            "pentru a vedea documentația aferentă.</p>")
    End Sub

    Private Sub ShowUndocumented(tagName As String)
        _helpBrowser.DocumentText = BuildPage(
            $"<h2 class='tag-title'>&lt;{tagName}&gt;</h2>" &
            "<p class='hint'>Nicio documentație disponibilă pentru acest tag.</p>")
    End Sub

    ' =========================================================================
    ' HTML SHELL — temă adaptivă dark / light
    ' =========================================================================
    Private Shared Function BuildPage(bodyContent As String) As String
        Dim css As String

        If KBotTheme.IsDark Then
            css =
                "* { box-sizing: border-box; }" &
                "html { -ms-overflow-style: -ms-autohiding-scrollbar; overflow-x: hidden; }" &
                "body{font-family:'Segoe UI',Arial,sans-serif;font-size:15px;" &
                "background:#1e1e1e;color:#d4d4d4;margin:10px 14px;line-height:1.5;" &
                "scrollbar-face-color:#111;scrollbar-arrow-color:#666;" &
                "scrollbar-track-color:#000;scrollbar-shadow-color:#000;" &
                "scrollbar-highlight-color:#222;scrollbar-3dlight-color:#111;" &
                "scrollbar-darkshadow-color:#000}" &
                "h1,h2,h3{margin-top:14px;margin-bottom:6px}" &
                "h2{color:#4ec9b0;font-size:15px;border-bottom:1px solid #3c3c3c;padding-bottom:4px}" &
                "h3{color:#9cdcfe;font-size:15px}" &
                ".tag-title{color:#4ec9b0;font-size:16px;font-weight:bold;" &
                "border-bottom:2px solid #2e75b6;padding-bottom:6px;margin-top:0}" &
                "code{background:#2d2d2d;color:#ce9178;padding:1px 5px;" &
                "border-radius:3px;font-family:Consolas,monospace;font-size:14px}" &
                "pre{background:#252526;border-left:3px solid #2e75b6;" &
                "padding:8px 12px;border-radius:0 3px 3px 0;margin:8px 0;" &
                "white-space:pre-wrap;word-wrap:break-word;" &
                "scrollbar-face-color:#111;scrollbar-arrow-color:#666;" &
                "scrollbar-track-color:#000;scrollbar-shadow-color:#000;" &
                "scrollbar-highlight-color:#222;scrollbar-3dlight-color:#111;" &
                "scrollbar-darkshadow-color:#000}" &
                "pre code{background:none;color:#9cdcfe;padding:0}" &
                "table{border-collapse:collapse;width:100%;table-layout:fixed;margin:8px 0;font-size:13px;}" &
                "th{background:#2e75b6;color:#fff;padding:6px 10px;text-align:left;font-weight:600;}" &
                "td{border:1px solid #3c3c3c;padding:6px 10px;vertical-align:top;word-wrap:break-word;overflow-wrap:break-word;}" &
                "tr:nth-child(even) td{background:#252526}" &
                "th:first-child,td:first-child{width:18%}" &
                "th:nth-child(2),td:nth-child(2){width:9%}" &
                "th:nth-child(3),td:nth-child(3){width:9%}" &
                "blockquote{border-left:3px solid #2e75b6;margin:8px 0;padding:4px 12px;" &
                "color:#9cdcfe;background:#252526;border-radius:0 3px 3px 0}" &
                "hr{border:none;border-top:1px solid #3c3c3c;margin:12px 0}" &
                ".hint{color:#666;font-style:italic;margin-top:30px;text-align:center}" &
                "a{color:#4ec9b0;text-decoration:none}"
        Else
            css =
                "* { box-sizing: border-box; }" &
                "html { -ms-overflow-style: -ms-autohiding-scrollbar; overflow-x: hidden; }" &
                "body{font-family:'Segoe UI',Arial,sans-serif;font-size:15px;" &
                "background:#ffffff;color:#1e1e1e;margin:10px 14px;line-height:1.5;}" &
                "h1,h2,h3{margin-top:14px;margin-bottom:6px}" &
                "h2{color:#006050;font-size:15px;border-bottom:1px solid #cccccc;padding-bottom:4px}" &
                "h3{color:#1e75b6;font-size:15px}" &
                ".tag-title{color:#006050;font-size:16px;font-weight:bold;" &
                "border-bottom:2px solid #2e75b6;padding-bottom:6px;margin-top:0}" &
                "code{background:#f0f0f0;color:#a31515;padding:1px 5px;" &
                "border-radius:3px;font-family:Consolas,monospace;font-size:14px}" &
                "pre{background:#f5f5f5;border-left:3px solid #2e75b6;" &
                "padding:8px 12px;border-radius:0 3px 3px 0;margin:8px 0;" &
                "white-space:pre-wrap;word-wrap:break-word;}" &
                "pre code{background:none;color:#1e75b6;padding:0}" &
                "table{border-collapse:collapse;width:100%;table-layout:fixed;margin:8px 0;font-size:13px;}" &
                "th{background:#2e75b6;color:#fff;padding:6px 10px;text-align:left;font-weight:600;}" &
                "td{border:1px solid #cccccc;padding:6px 10px;vertical-align:top;word-wrap:break-word;overflow-wrap:break-word;}" &
                "tr:nth-child(even) td{background:#f0f0f0}" &
                "th:first-child,td:first-child{width:18%}" &
                "th:nth-child(2),td:nth-child(2){width:9%}" &
                "th:nth-child(3),td:nth-child(3){width:9%}" &
                "blockquote{border-left:3px solid #2e75b6;margin:8px 0;padding:4px 12px;" &
                "color:#1e75b6;background:#e8f0f8;border-radius:0 3px 3px 0}" &
                "hr{border:none;border-top:1px solid #cccccc;margin:12px 0}" &
                ".hint{color:#888;font-style:italic;margin-top:30px;text-align:center}" &
                "a{color:#006050;text-decoration:none}"
        End If

        Return "<!DOCTYPE html><html><head><meta charset='utf-8'>" &
               "<meta http-equiv='X-UA-Compatible' content='IE=edge'>" &
               "<style>" & css & "</style></head><body>" & bodyContent & "</body></html>"
    End Function

    ' =========================================================================
    ' PARSARE FIȘIER HELP
    ' =========================================================================
    Private Shared Sub LoadHelpFile(filePath As String)
        _helpDict = New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        If Not File.Exists(filePath) Then Return

        Dim content As String
        Try
            content = File.ReadAllText(filePath, System.Text.Encoding.UTF8)
        Catch
            Return
        End Try

        Dim parts = Regex.Split(content, "(?m)^## ", RegexOptions.Multiline)
        For Each part In parts
            If String.IsNullOrWhiteSpace(part) Then Continue For
            Dim nl = part.IndexOfAny({ControlChars.Cr, ControlChars.Lf})
            If nl < 0 Then Continue For
            Dim tag As String = part.Substring(0, nl).Trim()
            Dim body As String = part.Substring(nl).Trim()
            If Not String.IsNullOrEmpty(tag) Then _helpDict(tag) = body
        Next
    End Sub

    ' =========================================================================
    ' ZOOM — COM (OLECMDID_OPTICAL_ZOOM=63) cu fallback CSS
    ' =========================================================================

    ''' <summary>
    ''' Aplică zoom-ul la procentul dat. Încearcă COM via late binding
    ''' (fără SHDocVw.dll); la eșec → CSS fallback permanent.
    ''' </summary>
    Friend Sub ApplyZoom(percent As Integer)
        _zoomPercent = Math.Max(75, Math.Min(200, percent))

        If _zoomLabel IsNot Nothing Then _zoomLabel.Text = $"🔎 {_zoomPercent}%"

        If _zoomSlider IsNot Nothing AndAlso _zoomSlider.Value <> _zoomPercent Then
            _suppressZoom = True
            _zoomSlider.Value = _zoomPercent
            _suppressZoom = False
        End If

        If _comZoomOk <> False Then
            Dim ok = TryApplyComZoom(_zoomPercent)
            If _comZoomOk Is Nothing Then _comZoomOk = ok
            If ok Then Return
            _comZoomOk = False
        End If
        ApplyZoomCss(_zoomPercent)
    End Sub

    ''' <summary>
    ''' Zoom via COM — late binding pe ActiveXInstance (nu necesită SHDocVw.dll).
    ''' OLECMDID_OPTICAL_ZOOM = 63, MSOCMDEXECOPT_DONTPROMPTUSER = 2.
    ''' </summary>
    Private Function TryApplyComZoom(percent As Integer) As Boolean
        Try
            Dim ax As Object = _helpBrowser.ActiveXInstance
            If ax Is Nothing Then Return False
            ax.ExecWB(63, 2, percent, Nothing)
            Return True
        Catch
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Zoom via CSS — setează style pe Document.Body.
    ''' Apelat la fiecare DocumentCompleted când COM nu e disponibil.
    ''' </summary>
    Private Sub ApplyZoomCss(percent As Integer)
        Try
            If _helpBrowser.Document?.Body Is Nothing Then Return
            ' zoom: X% scalează vizual body-ul, dar width rămâne relativ la layout.
            ' Compensăm: body width = 100/factor% → după zoom rezultă exact 100% din viewport.
            ' Ex: zoom=150% → width=66.67% → 66.67% * 1.5 = 100% ✓
            Dim invWidth As String = (10000.0 / percent).ToString("F2", Globalization.CultureInfo.InvariantCulture)
            _helpBrowser.Document.Body.Style =
            $"zoom: {percent}%; width: {invWidth}%; margin: 0; padding: 8px;"
        Catch
        End Try
    End Sub

    Private Sub OnZoomChanged(sender As Object, e As EventArgs)
        If _suppressZoom Then Return
        ApplyZoom(_zoomSlider.Value)
    End Sub

    ' =========================================================================
    ' TEMĂ — re-aplică culorile panourilor și butoanelor de tab la switch dark/light
    ' =========================================================================

    ''' <summary>
    ''' Comută paleta sistemului de tab-uri și a panourilor de conținut între
    ''' dark și light. Re-randează pagina de ajutor curentă cu CSS-ul nou.
    ''' Apelat din ApplyEditorTheme().
    ''' </summary>
    Friend Sub ApplyHelpPanelTheme()
        If _tabContainer Is Nothing Then Return   ' InitHelpPanel() nu a fost apelat încă

        Dim isDark = KBotTheme.IsDark

        ' ── Actualizăm paleta de culori ──────────────────────────────────────
        CLR_TAB_BG = If(isDark, Color.FromArgb(37, 37, 38), Color.FromArgb(240, 240, 240))
        CLR_TAB_ACTIVE = If(isDark, Color.FromArgb(30, 30, 30), Color.FromArgb(255, 255, 255))
        CLR_TAB_INACTIVE = If(isDark, Color.FromArgb(50, 50, 52), Color.FromArgb(220, 220, 220))
        CLR_TAB_BORDER = Color.FromArgb(0, 122, 204)   ' albastru accent — identic în ambele teme
        CLR_TAB_TEXT_ON = If(isDark, Color.FromArgb(255, 255, 255), Color.FromArgb(0, 0, 0))
        CLR_TAB_TEXT_OFF = If(isDark, Color.FromArgb(160, 160, 160), Color.FromArgb(80, 80, 80))

        ' ── Actualizăm controalele container ─────────────────────────────────
        _tabContainer.BackColor = CLR_TAB_BG
        _tabHeader.BackColor = CLR_TAB_BG
        _tabContent.BackColor = CLR_TAB_BG
        _panelHarta.BackColor = CLR_TAB_BG
        _panelAjutor.BackColor = CLR_TAB_ACTIVE

        ' ── Zoom strip ───────────────────────────────────────────────────────
        If _zoomPanel IsNot Nothing Then
            Dim zoomBg = If(isDark, Color.FromArgb(45, 45, 48), Color.FromArgb(230, 230, 230))
            _zoomPanel.BackColor = zoomBg
            _zoomSlider.BackColor = zoomBg
            _zoomLabel.ForeColor = If(isDark, Color.White, Color.FromArgb(30, 30, 30))
        End If

        ' ── Re-aplică culorile butoanelor de tab (activ/inactiv) ─────────────
        ActivateTab(_isAjutorActive)

        ' ── Re-randăm pagina de help cu noul CSS ─────────────────────────────
        If _isAjutorActive Then
            If String.IsNullOrEmpty(_currentHelpTag) Then
                ShowWelcome()
            ElseIf _helpDict IsNot Nothing Then
                Dim md As String = Nothing
                If _helpDict.TryGetValue(_currentHelpTag, md) Then
                    RenderMarkdown(_currentHelpTag, md)
                Else
                    ShowUndocumented(_currentHelpTag)
                End If
            End If
        End If
    End Sub

End Class