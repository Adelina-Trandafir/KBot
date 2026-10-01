Option Strict On
Imports System.Diagnostics
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' The help window (slice 0000-01): contents tree and search on the left, the page on the right,
''' Back / Forward and the manual export button above it. One instance, owned by <see cref="HelpService"/>;
''' F1 and «?» reuse it and just turn the page.
'''
''' <para>Pages are HTML from <see cref="HelpHtml"/> in a <see cref="WebBrowser"/>. A click on a
''' <c>topic:</c> link is caught in <c>Navigating</c> and becomes a page change here; a web link
''' opens in the default browser, never inside the help.</para>
''' </summary>
Public Class HelpForm

    Private Const HomeKey As String = "home"
    Private Const TopicPrefix As String = "t:"

    Private ReadOnly _service As HelpService
    ' Slice 0000-23: Back / Forward / the page on screen live in HelpService (the whole run), so
    ' they outlive this window. See HelpHistory.
    Private ReadOnly _history As HelpHistory
    Private _syncingTree As Boolean
    Private _contextKeys As New List(Of String)()
    Private _shown As Boolean
    Private _pendingHtml As String
    Private _search As HelpSearchSession
    Private _pendingAnchor As String
    Private _scrollAnchor As String

    ''' <summary>Designer only.</summary>
    Public Sub New()
        Me.New(Nothing)
    End Sub

    Public Sub New(service As HelpService)
        _history = If(service IsNot Nothing, service.History, New HelpHistory())
        InitializeComponent()
        _service = service
        ' Slice 0000-20: the same search as the «?» popup; its list covers the contents while the
        ' box has text. Made here, not in Load: a hit from the popup is handed over before Show.
        If service IsNot Nothing Then
            _search = New HelpSearchSession(service, HelpSearchSession.WhereWindow, Nothing)
            pnlCautare.Source = _search
        End If
    End Sub

    ''' <summary>
    ''' The keys F1 / «?» tried, nearest first. Debug builds show them in the bar, so whoever
    ''' writes a topic can see which <c>screens</c> value would have matched.
    ''' </summary>
    Public Property ContextKeys As List(Of String)
        Get
            Return _contextKeys
        End Get
        Set(value As List(Of String))
            _contextKeys = If(value, New List(Of String)())
#If DEBUG Then
            lblContext.Text = "F1: " & String.Join("  ›  ", _contextKeys)
#End If
        End Set
    End Property

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        Try
            If _service Is Nothing Then Return
            FoldSearch()
            BuildTree()
            ' HelpService picks the first page before Show(); it is drawn now that there is a handle.
            If _history.Current.Length = 0 Then _history.Current = HomeKey
            _history.NoteVisit(_history.Current)
            Render(_history.Current)
            SelectInTree(_history.Current)
            UpdateButtons()
        Catch ex As Exception
            ' UI boundary (Load).
            GlobalErrorLog.Write("HelpForm.OnLoad", ex)
        End Try
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim p As ThemePalette = ThemeManager.Current.Palette
            BackColor = p.BorderColor   ' the form background is the 1px outline (Padding 1)
            tvCuprins.BackColor = p.SurfaceAltColor
            tvCuprins.ForeColor = p.TextColor
            lblContext.ForeColor = p.TextDimColor
            ' The page CSS comes from the palette, so the page is redrawn with the new scheme.
            If _service IsNot Nothing AndAlso _history.Current.Length > 0 Then Render(_history.Current)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.OnThemeChanged", ex)
        End Try
    End Sub

    ' ── Public page changes (HelpService) ─────────────────────────────────────────

    Public Sub ShowHome()
        Try
            Go(HomeKey, remember:=True)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.ShowHome", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Opens a topic; with <paramref name="sectionAnchor"/>, scrolled to that section (slice 0000-18).</summary>
    Public Sub ShowTopic(id As String, Optional sectionAnchor As String = Nothing)
        Try
            Go(TopicPrefix & id & If(String.IsNullOrEmpty(sectionAnchor), String.Empty, "#" & sectionAnchor), remember:=True)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.ShowTopic", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Draws the current page again (a picture was taken, the topics were re-read).</summary>
    Public Sub RefreshPage()
        Try
            If _history.Current.Length > 0 Then Render(_history.Current)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.RefreshPage", ex)
            Throw
        End Try
    End Sub

    ' Slice 0000-02: «Capturi...» shows only in capture mode (checked each time the window activates).
    Protected Overrides Sub OnActivated(e As EventArgs)
        MyBase.OnActivated(e)
        Try
            btnCapturi.Visible = KbotForm.HelpCaptureModeOn
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.OnActivated", ex)
        End Try
    End Sub

    Private Sub BtnCapturi_Click(sender As Object, e As EventArgs) Handles btnCapturi.Click
        Try
            HelpCaptureForm.ShowFor(Me, _service)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.BtnCapturi_Click", ex)
        End Try
    End Sub

    ' ── Navigation ────────────────────────────────────────────────────────────────

    ' Every page change goes through here: history, render, tree selection, buttons.
    Private Sub Go(page As String, remember As Boolean)
        If String.Equals(page, _history.Current, StringComparison.Ordinal) Then Return
        If remember AndAlso _history.Current.Length > 0 Then
            _history.Back.Push(_history.Current)
            _history.Forward.Clear()
        End If
        _history.Current = page
        _history.NoteVisit(page)
        Render(page)
        SelectInTree(page)
        UpdateButtons()
    End Sub

    ' ── Slice 0000-23: «Istoric» — every page seen this run ──────────────────────

    Private Sub BtnIstoric_Click(sender As Object, e As EventArgs) Handles btnIstoric.Click
        Try
            mnuIstoric.Items.Clear()
            For Each page As String In _history.Visited
                Dim item As New KBotMenuItem(page, PageTitle(page)) With {.Tag = page}
                If String.Equals(page, _history.Current, StringComparison.Ordinal) Then item.ShortcutText = "pe ecran"
                mnuIstoric.Items.Add(item)
            Next
            If mnuIstoric.Items.Count = 0 Then
                mnuIstoric.Items.Add(New KBotMenuItem("none", "Nicio pagină deschisă încă") With {.Enabled = False})
            End If
            mnuIstoric.ShowBelow(btnIstoric)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.BtnIstoric_Click", ex)
        End Try
    End Sub

    Private Sub MnuIstoric_ItemClicked(sender As Object, e As KBotMenuItemClickedEventArgs) Handles mnuIstoric.ItemClicked
        Try
            Dim page As String = TryCast(e.Item.Tag, String)
            If Not String.IsNullOrEmpty(page) Then Go(page, remember:=True)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.MnuIstoric_ItemClicked", ex)
        End Try
    End Sub

    ' What the «Istoric» list says for a page: the topic's title (and «› section» for a page opened
    ' at a section by the search), or the start page.
    Private Function PageTitle(page As String) As String
        If Not page.StartsWith(TopicPrefix, StringComparison.Ordinal) Then Return "Pagina de start"
        Dim id As String = page.Substring(TopicPrefix.Length)
        Dim hash As Integer = id.IndexOf("#"c)
        If hash >= 0 Then id = id.Substring(0, hash)
        Dim topic As HelpTopic = _service.Library.Find(id)
        Return If(topic Is Nothing, id, topic.Title) & If(hash >= 0, " › secțiune", String.Empty)
    End Function

    ' ── Slice 0000-23: text size («A−» / «A+», kept in the settings) ─────────────

    Private Sub BtnTextMic_Click(sender As Object, e As EventArgs) Handles btnTextMic.Click
        Try
            StepTextSize(-1)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.BtnTextMic_Click", ex)
        End Try
    End Sub

    Private Sub BtnTextMare_Click(sender As Object, e As EventArgs) Handles btnTextMare.Click
        Try
            StepTextSize(1)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.BtnTextMare_Click", ex)
        End Try
    End Sub

    ' One step through AppSettings.HelpTextPercentChoices; saved, and the page on screen
    ' redrawn at the new size (the scroll position is kept by changing the body's style in place).
    Private Sub StepTextSize(direction As Integer)
        Dim choices As IReadOnlyList(Of Integer) = AppSettings.HelpTextPercentChoices
        Dim at As Integer = choices.ToList().IndexOf(AppSettings.Current.HelpTextPercent)
        If at < 0 Then at = choices.ToList().IndexOf(100)
        Dim nextAt As Integer = Math.Max(0, Math.Min(choices.Count - 1, at + direction))
        If nextAt = at Then Return
        Dim copy As AppSettings = AppSettings.Current.Clone()
        copy.HelpTextPercent = choices(nextAt)
        copy.Save()
        ApplyTextSizeInPlace()
        UpdateButtons()
    End Sub

    Private Sub ApplyTextSizeInPlace()
        If web.Document?.Body Is Nothing Then
            If _history.Current.Length > 0 Then Render(_history.Current)
            Return
        End If
        Dim pt As Double = 10.5 * AppScaling.TextScale * HelpHtml.HelpTextFactor()
        web.Document.Body.Style = "font-size:" & pt.ToString("0.0", Globalization.CultureInfo.InvariantCulture) & "pt"
    End Sub

    ' ── Slice 0000-23: always on top, and out of the way of other windows' dialogs ───
    ' The help window is TopMost (designer), so nothing of K-BOT's covers it. A modal dialog of
    ' another window disables it (Windows' EnableWindow, WM_ENABLE) -- and a disabled window on top
    ' of that dialog would hide it and could not be moved or closed. So when that happens the help
    ' window closes itself (the pages seen stay in HelpHistory). Its own dialogs (the manual's
    ' «Save as», the capture tool) are owned by it and do not count; nor does a minimized help
    ' window (a tour or a capture put it there), which covers nothing. The check waits 150 ms:
    ' at WM_ENABLE the dialog does not exist yet, so its owner cannot be seen.

    Private Const WM_ENABLE As Integer = &HA

    ' Left to Application.ThreadException, like every WndProc override in K-BOT (house rule).
    Protected Overrides Sub WndProc(ByRef m As Message)
        MyBase.WndProc(m)
        If m.Msg = WM_ENABLE AndAlso m.WParam = IntPtr.Zero AndAlso _service IsNot Nothing Then tmrModal.Start()
    End Sub

    Private Sub TmrModal_Tick(sender As Object, e As EventArgs) Handles tmrModal.Tick
        Try
            tmrModal.Stop()
            If IsDisposed OrElse Not IsHandleCreated OrElse HelpWindowNative.IsEnabled(Handle) Then Return
            If WindowState = FormWindowState.Minimized Then Return
            If HelpWindowNative.OwnsEnabledWindow(Handle) Then Return
            Close()
        Catch ex As Exception
            ' UI boundary (timer).
            GlobalErrorLog.Write("HelpForm.TmrModal_Tick", ex)
        End Try
    End Sub

    Private Sub Render(page As String)
        Dim library As HelpLibrary = _service.Library
        Dim parts As List(Of HelpPart) = _service.VisibleParts()
        Dim html As String
        Dim anchor As String = Nothing
        If page.StartsWith(TopicPrefix, StringComparison.Ordinal) Then
            Dim topicId As String = page.Substring(TopicPrefix.Length)
            Dim hash As Integer = topicId.IndexOf("#"c)
            If hash >= 0 Then
                anchor = topicId.Substring(hash + 1)
                topicId = topicId.Substring(0, hash)
            End If
            Dim topic As HelpTopic = library.Find(topicId)
            If topic Is Nothing Then
                html = HelpHtml.MessagePage("Pagina nu există", {"Pagina de ajutor cerută nu se găsește. Folosește cuprinsul din stânga."})
            ElseIf Not parts.Contains(topic.Part) Then
                html = HelpHtml.MessagePage(topic.Title, {"Pagina aceasta face parte din «" & HelpTopic.PartTitle(topic.Part) & "», care nu e disponibilă pentru contul tău."})
            Else
                html = HelpHtml.TopicPage(library, topic, parts)
            End If
        Else
            html = HelpHtml.HomePage(library, parts)
        End If
        SetHtml(html, anchor)
    End Sub

    ' The WebBrowser drops a DocumentText set before the window is shown or while a previous page
    ' is still loading (seen on screen: F1 opened the window with a blank page). Such a page waits
    ' here and is drawn from OnShown / DocumentCompleted. Only the newest one is kept.
    ' Slice 0000-18: the section to scroll to travels with its page and is used once it has loaded.
    Private Sub SetHtml(html As String, anchor As String)
        If Not _shown OrElse web.IsBusy Then
            _pendingHtml = html
            _pendingAnchor = anchor
            Return
        End If
        _pendingHtml = Nothing
        _pendingAnchor = Nothing
        _scrollAnchor = anchor
        web.DocumentText = html
    End Sub

    Private Sub ScrollToAnchor()
        Dim a As String = _scrollAnchor
        _scrollAnchor = Nothing
        If String.IsNullOrEmpty(a) OrElse web.Document Is Nothing Then Return
        web.Document.GetElementById(a)?.ScrollIntoView(True)
    End Sub

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        Try
            _shown = True
            If _pendingHtml IsNot Nothing Then SetHtml(_pendingHtml, _pendingAnchor)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.OnShown", ex)
        End Try
    End Sub

    Private Sub Web_DocumentCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs) Handles web.DocumentCompleted
        Try
            If _pendingHtml IsNot Nothing Then
                SetHtml(_pendingHtml, _pendingAnchor)
            Else
                ScrollToAnchor()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.Web_DocumentCompleted", ex)
        End Try
    End Sub

    Private Sub UpdateButtons()
        btnInapoi.Enabled = _history.Back.Count > 0
        btnInainte.Enabled = _history.Forward.Count > 0
        Dim percent As Integer = AppSettings.Current.HelpTextPercent
        Dim choices As IReadOnlyList(Of Integer) = AppSettings.HelpTextPercentChoices
        btnTextMic.Enabled = percent > choices(0)
        btnTextMare.Enabled = percent < choices(choices.Count - 1)
    End Sub

    Private Sub BtnInapoi_Click(sender As Object, e As EventArgs) Handles btnInapoi.Click
        Try
            If _history.Back.Count = 0 Then Return
            _history.Forward.Push(_history.Current)
            Dim page As String = _history.Back.Pop()
            _history.Current = String.Empty
            Go(page, remember:=False)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.BtnInapoi_Click", ex)
        End Try
    End Sub

    Private Sub BtnInainte_Click(sender As Object, e As EventArgs) Handles btnInainte.Click
        Try
            If _history.Forward.Count = 0 Then Return
            _history.Back.Push(_history.Current)
            Dim page As String = _history.Forward.Pop()
            _history.Current = String.Empty
            Go(page, remember:=False)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.BtnInainte_Click", ex)
        End Try
    End Sub

    Private Sub BtnManual_Click(sender As Object, e As EventArgs) Handles btnManual.Click
        Try
            _service.ExportManual(Me)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.BtnManual_Click", ex)
            KBotMessage.Show(Me, "Manualul nu a putut fi salvat: " & ex.Message, "Ajutor K-BOT",
                             MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ' A topic link becomes a page change here; a web link goes to the default browser.
    Private Sub Web_Navigating(sender As Object, e As WebBrowserNavigatingEventArgs) Handles web.Navigating
        Try
            If e.Url Is Nothing Then Return
            Dim url As String = e.Url.OriginalString
            If url.StartsWith(HelpHtml.TourScheme, StringComparison.OrdinalIgnoreCase) Then
                ' Slice 0000-04: a guided tour link.
                e.Cancel = True
                _service.StartTour(Uri.UnescapeDataString(url.Substring(HelpHtml.TourScheme.Length)))
            ElseIf url.StartsWith(HelpHtml.TopicScheme, StringComparison.OrdinalIgnoreCase) Then
                e.Cancel = True
                Go(TopicPrefix & Uri.UnescapeDataString(url.Substring(HelpHtml.TopicScheme.Length)), remember:=True)
            ElseIf e.Url.Scheme = Uri.UriSchemeHttp OrElse e.Url.Scheme = Uri.UriSchemeHttps Then
                e.Cancel = True
                Process.Start(New ProcessStartInfo(url) With {.UseShellExecute = True})?.Dispose()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.Web_Navigating", ex)
        End Try
    End Sub

    ' ── Search (slice 0000-20) ────────────────────────────────────────────────────

    ''' <summary>A question asked in the «?» popup, carried on here with its results.</summary>
    Friend Sub TakeOverSearch(from As HelpSearchSession)
        Try
            ' Slice 0000-21: the same question (same id), so a rating here updates its row.
            _search?.EndQuestion()
            _search?.Adopt(from)
            pnlCautare.SetQuery(from.Query)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.TakeOverSearch", ex)
            Throw
        End Try
    End Sub

    ' Results on screen: the list takes the whole left side; none: back to the box over the contents.
    Private Sub FoldSearch()
        If pnlCautare.HasRows Then
            tvCuprins.Visible = False
            pnlCautare.Dock = DockStyle.Fill
        Else
            pnlCautare.Dock = DockStyle.Top
            pnlCautare.Height = pnlCautare.CollapsedHeight
            tvCuprins.Visible = True
        End If
    End Sub

    Private Sub PnlCautare_RowsVisibleChanged(sender As Object, e As EventArgs) Handles pnlCautare.RowsVisibleChanged
        Try
            FoldSearch()
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.PnlCautare_RowsVisibleChanged", ex)
        End Try
    End Sub

    ' Esc in the box empties it: the contents come back.
    Private Sub PnlCautare_EscapePressed(sender As Object, e As EventArgs) Handles pnlCautare.EscapePressed
        Try
            pnlCautare.SetQuery(String.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.PnlCautare_EscapePressed", ex)
        End Try
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        Try
            pnlCautare.EndQuestion()
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.OnFormClosed", ex)
        End Try
        MyBase.OnFormClosed(e)
    End Sub

    ' ── Contents tree ─────────────────────────────────────────────────────────────

    Private Sub BuildTree()
        tvCuprins.BeginUpdate()
        Try
            tvCuprins.Nodes.Clear()
            Dim library As HelpLibrary = _service.Library
            For Each part As HelpPart In _service.VisibleParts()
                Dim partNode As New TreeNode(HelpTopic.PartTitle(part)) With {.Tag = HomeKey}
                partNode.NodeFont = New Font(tvCuprins.Font, FontStyle.Bold)
                AddChildren(library, part, String.Empty, partNode.Nodes)
                tvCuprins.Nodes.Add(partNode)
                partNode.ExpandAll()
            Next
        Finally
            tvCuprins.EndUpdate()
        End Try
    End Sub

    Private Shared Sub AddChildren(library As HelpLibrary, part As HelpPart, parentId As String, into As TreeNodeCollection)
        For Each t As HelpTopic In library.Children(part, parentId)
            Dim n As New TreeNode(t.Title) With {.Tag = TopicPrefix & t.Id}
            AddChildren(library, part, t.Id, n.Nodes)
            into.Add(n)
        Next
    End Sub

    Private Sub SelectInTree(page As String)
        If Not page.StartsWith(TopicPrefix, StringComparison.Ordinal) Then Return
        Dim hash As Integer = page.IndexOf("#"c)
        If hash >= 0 Then page = page.Substring(0, hash)
        Dim node As TreeNode = FindNode(tvCuprins.Nodes, page)
        If node Is Nothing Then Return
        _syncingTree = True
        Try
            tvCuprins.SelectedNode = node
            node.EnsureVisible()
        Finally
            _syncingTree = False
        End Try
    End Sub

    Private Shared Function FindNode(nodes As TreeNodeCollection, page As String) As TreeNode
        For Each n As TreeNode In nodes
            If String.Equals(TryCast(n.Tag, String), page, StringComparison.OrdinalIgnoreCase) Then Return n
            Dim hit As TreeNode = FindNode(n.Nodes, page)
            If hit IsNot Nothing Then Return hit
        Next
        Return Nothing
    End Function

    Private Sub TvCuprins_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles tvCuprins.AfterSelect
        Try
            If _syncingTree OrElse e.Node Is Nothing Then Return
            Dim page As String = TryCast(e.Node.Tag, String)
            If String.IsNullOrEmpty(page) Then Return
            Go(page, remember:=True)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.TvCuprins_AfterSelect", ex)
        End Try
    End Sub

End Class
