Option Strict On
Imports System.Diagnostics
Imports KBot.Common
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
    Private ReadOnly _back As New Stack(Of String)()
    Private ReadOnly _forward As New Stack(Of String)()
    Private _current As String = String.Empty
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
            If _current.Length = 0 Then _current = HomeKey
            Render(_current)
            SelectInTree(_current)
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
            If _current.Length > 0 AndAlso _service IsNot Nothing Then Render(_current)
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
            If _current.Length > 0 Then Render(_current)
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
        If String.Equals(page, _current, StringComparison.Ordinal) Then Return
        If remember AndAlso _current.Length > 0 Then
            _back.Push(_current)
            _forward.Clear()
        End If
        _current = page
        Render(page)
        SelectInTree(page)
        UpdateButtons()
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
        btnInapoi.Enabled = _back.Count > 0
        btnInainte.Enabled = _forward.Count > 0
    End Sub

    Private Sub BtnInapoi_Click(sender As Object, e As EventArgs) Handles btnInapoi.Click
        Try
            If _back.Count = 0 Then Return
            _forward.Push(_current)
            Dim page As String = _back.Pop()
            _current = String.Empty
            Go(page, remember:=False)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.BtnInapoi_Click", ex)
        End Try
    End Sub

    Private Sub BtnInainte_Click(sender As Object, e As EventArgs) Handles btnInainte.Click
        Try
            If _forward.Count = 0 Then Return
            _back.Push(_current)
            Dim page As String = _forward.Pop()
            _current = String.Empty
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
