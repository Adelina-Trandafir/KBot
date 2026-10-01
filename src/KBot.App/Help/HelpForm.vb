Option Strict On
Imports System.Diagnostics
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' The help window (slice 0000-01): contents tree and search on the left, the page on the right,
''' Back / Forward, print and export above it. One instance, owned by <see cref="HelpService"/>;
''' F1 and «?» reuse it and just turn the page.
'''
''' <para><b>One document, many places</b> (slice 0000-32). The <see cref="WebBrowser"/> holds the
''' whole help (<see cref="HelpHtml.Book"/>): opening a topic scrolls to its section, and scrolling
''' by hand walks from one topic into the next while the contents tree follows. A page is still
''' named as before (<c>home</c>, <c>t:&lt;topic id&gt;</c>, <c>t:&lt;id&gt;#&lt;section&gt;</c>, and
''' now <c>p:&lt;part&gt;</c>); only a topic this login may not read replaces the document with a
''' message page.</para>
'''
''' <para>A click on a <c>topic:</c> link is caught in <c>Navigating</c> and becomes a page change
''' here; a web link opens in the default browser, never inside the help.</para>
''' </summary>
Public Class HelpForm

    Private Const HomeKey As String = "home"
    Private Const TopicPrefix As String = "t:"
    ' Slice 0000-32: a part's heading, e.g. "p:contabil".
    Private Const PartPrefix As String = "p:"
    Private Const MessageCaption As String = "Ajutor K-BOT"

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

    ' Slice 0000-32: the book. _bookLoading = handed to the browser (waiting or loading);
    ' _bookReady = on screen, so a page change is a scroll. _anchors: section id -> page.
    Private _bookLoading As Boolean
    Private _bookReady As Boolean
    Private ReadOnly _anchors As New Dictionary(Of String, String)(StringComparer.Ordinal)
    ' Where the page stood when the tree was last brought in line with it (-1 = never).
    Private _lastScrollY As Integer = -1

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
            ' The page CSS comes from the palette, so the page is built again with the new scheme.
            ' Not before the window is shown: Load draws the first page (the base calls this from Load).
            If _service IsNot Nothing AndAlso _shown AndAlso _history.Current.Length > 0 Then Render(_history.Current, reload:=True)
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

    ''' <summary>Builds the page again (a picture was taken, the topics were re-read), at the same place.</summary>
    Public Sub RefreshPage()
        Try
            If _history.Current.Length > 0 Then Render(_history.Current, reload:=True)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.RefreshPage", ex)
            Throw
        End Try
    End Sub

    ' ── Navigation ────────────────────────────────────────────────────────────────

    ' Every page change goes through here: history, render, tree selection, buttons.
    Private Sub Go(page As String, remember As Boolean)
        If String.Equals(page, _history.Current, StringComparison.Ordinal) Then
            ' Slice 0000-32: the place it is at, asked for again -- the reader may have scrolled
            ' away inside it, so go back to its start. Nothing new for the history.
            Render(page)
            Return
        End If
        If remember AndAlso _history.Current.Length > 0 Then
            ' The place being left may have been reached by scrolling: it counts as seen.
            _history.NoteVisit(_history.Current)
            _history.Back.Push(_history.Current)
            _history.Forward.Clear()
        End If
        _history.Current = page
        _history.NoteVisit(page)
        Render(page)
        SelectInTree(page)
        UpdateButtons()
    End Sub

    ' The topic id of a page name, without its section; empty for the start page or a part.
    Private Shared Function TopicIdOf(page As String) As String
        If String.IsNullOrEmpty(page) OrElse Not page.StartsWith(TopicPrefix, StringComparison.Ordinal) Then Return String.Empty
        Dim id As String = page.Substring(TopicPrefix.Length)
        Dim hash As Integer = id.IndexOf("#"c)
        Return If(hash >= 0, id.Substring(0, hash), id)
    End Function

    ' A page name without its section.
    Private Shared Function WithoutSection(page As String) As String
        Dim hash As Integer = page.IndexOf("#"c)
        Return If(hash >= 0, page.Substring(0, hash), page)
    End Function

    Private Shared Function PartPage(part As HelpPart) As String
        Return PartPrefix & part.ToString().ToLowerInvariant()
    End Function

    ' The part a "p:..." page names; False when it names none.
    Private Shared Function TryPartOf(page As String, ByRef part As HelpPart) As Boolean
        If Not page.StartsWith(PartPrefix, StringComparison.Ordinal) Then Return False
        For Each p As HelpPart In [Enum].GetValues(Of HelpPart)()
            If String.Equals(PartPage(p), page, StringComparison.OrdinalIgnoreCase) Then
                part = p
                Return True
            End If
        Next
        Return False
    End Function

    ' ── Slice 0000-23: «Istoric» — every page seen this run ──────────────────────

    Private Sub BtnIstoric_Click(sender As Object, e As EventArgs) Handles btnIstoric.Click
        Try
            mnuIstoric.Items.Clear()
            Dim onScreen As String = WithoutSection(_history.Current)
            For Each page As String In _history.Visited
                Dim item As New KBotMenuItem(page, PageTitle(page)) With {.Tag = page}
                If String.Equals(WithoutSection(page), onScreen, StringComparison.Ordinal) Then item.ShortcutText = "pe ecran"
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
    ' at a section by the search), a part's name, or the start page.
    Private Function PageTitle(page As String) As String
        Dim part As HelpPart
        If TryPartOf(page, part) Then Return HelpTopic.PartTitle(part)
        Dim id As String = TopicIdOf(page)
        If id.Length = 0 Then Return "Pagina de start"
        Dim topic As HelpTopic = _service.Library.Find(id)
        Return If(topic Is Nothing, id, topic.Title) & If(page.Contains("#"c), " › secțiune", String.Empty)
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
    ' redrawn at the new size, in place.
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

    ' The body's style is changed in place (no reload). Slice 0000-32: the page is long, so the
    ' same pixel offset would be another topic at the new size -- what was at the top of the
    ' view is brought back there.
    Private Sub ApplyTextSizeInPlace()
        If web.Document?.Body Is Nothing Then
            If _history.Current.Length > 0 Then Render(_history.Current)
            Return
        End If
        Dim atTop As HtmlElement = TextAtTop()
        Dim pt As Double = 10.5 * AppScaling.TextScale * HelpHtml.HelpTextFactor()
        web.Document.Body.Style = "font-size:" & pt.ToString("0.0", Globalization.CultureInfo.InvariantCulture) & "pt"
        atTop?.ScrollIntoView(True)
        _lastScrollY = ScrollY()
    End Sub

    ' The piece of text at the top of the view (a paragraph, a heading, a picture). Between two
    ' paragraphs the point falls on the section itself, so a few points further down are tried
    ' first; a whole section is the answer only when nothing smaller is found.
    Private Function TextAtTop() As HtmlElement
        Dim doc As HtmlDocument = web.Document
        Dim probe As Point = ProbePoint()
        Dim section As HtmlElement = Nothing
        For y As Integer = probe.Y To probe.Y + 120 Step 20
            Dim el As HtmlElement = doc.GetElementFromPoint(New Point(probe.X, y))
            If el Is Nothing Then Continue For
            Dim tag As String = If(el.TagName, String.Empty)
            If String.Equals(tag, "BODY", StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(tag, "HTML", StringComparison.OrdinalIgnoreCase) Then Continue For
            Dim id As String = el.Id
            If Not String.IsNullOrEmpty(id) AndAlso _anchors.ContainsKey(id) Then
                If section Is Nothing Then section = el
                Continue For
            End If
            Return el
        Next
        Return section
    End Function

    ' ── Slice 0000-23: always on top, and out of the way of other windows' dialogs ───
    ' The help window is TopMost (designer), so nothing of K-BOT's covers it. A modal dialog of
    ' another window disables it (Windows' EnableWindow, WM_ENABLE) -- and a disabled window on top
    ' of that dialog would hide it and could not be moved or closed. So when that happens the help
    ' window closes itself (the pages seen stay in HelpHistory). Its own dialogs are owned by it and
    ' do not count; nor does a minimized help window (a tour or a capture put it there), which
    ' covers nothing. The check waits 150 ms: at WM_ENABLE the dialog does not exist yet, so its
    ' owner cannot be seen.
    '
    ' Slice 0000-32: 150 ms is not always enough. A dialog that is slow to appear (the «Save as»
    ' of the export, the first time, or under a debugger) was still missing at the check, the
    ' window took it for somebody else's and closed -- under its own dialog, which was then left
    ' without an owner while every other K-BOT window stayed disabled: K-BOT froze. So now:
    '   - the window's own dialogs are opened inside OwnDialog(), and no check runs meanwhile;
    '   - a check that finds no dialog on screen yet looks again instead of closing.

    Private Const WM_ENABLE As Integer = &HA
    ' After one of its own dialogs returns, how long a disabled help window is still taken for its own.
    Private Const OwnDialogGraceMs As Integer = 2000

    Private _ownDialogs As Integer
    Private _ownDialogGraceUntil As Long

    ' Every dialog the help window opens itself (message, «Save as», print) goes inside
    ' «Using OwnDialog()».
    Private Function OwnDialog() As IDisposable
        Return New OwnDialogScope(Me)
    End Function

    Private NotInheritable Class OwnDialogScope
        Implements IDisposable

        Private ReadOnly _form As HelpForm
        Private _done As Boolean

        Public Sub New(form As HelpForm)
            _form = form
            form._ownDialogs += 1
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If _done Then Return
            _done = True
            _form._ownDialogs -= 1
            _form._ownDialogGraceUntil = Environment.TickCount64 + OwnDialogGraceMs
        End Sub
    End Class

    ' Left to Application.ThreadException, like every WndProc override in K-BOT (house rule).
    Protected Overrides Sub WndProc(ByRef m As Message)
        MyBase.WndProc(m)
        If m.Msg = WM_ENABLE AndAlso m.WParam = IntPtr.Zero AndAlso _service IsNot Nothing AndAlso _ownDialogs = 0 Then tmrModal.Start()
    End Sub

    Private Sub TmrModal_Tick(sender As Object, e As EventArgs) Handles tmrModal.Tick
        Try
            tmrModal.Stop()
            If IsDisposed OrElse Not IsHandleCreated OrElse HelpWindowNative.IsEnabled(Handle) Then Return
            If WindowState = FormWindowState.Minimized Then Return
            If _ownDialogs > 0 OrElse HelpWindowNative.OwnsEnabledWindow(Handle) Then Return
            ' Nothing of this window's is on screen. Either the dialog is somebody else's, or it
            ' has not appeared yet (no enabled window at all / one of ours has just returned):
            ' then look again, do not close under it.
            If Environment.TickCount64 < _ownDialogGraceUntil OrElse Not HelpWindowNative.AnyEnabledWindow(Handle) Then
                tmrModal.Start()
                Return
            End If
            Close()
        Catch ex As Exception
            ' UI boundary (timer).
            GlobalErrorLog.Write("HelpForm.TmrModal_Tick", ex)
        End Try
    End Sub

    ' ── The page (slice 0000-32: one document, scrolled) ─────────────────────────

    ' Puts <page> on screen. With the book loaded it is a scroll; else the book is loaded and
    ' opens there. reload = build the document again (new theme, new picture, topics re-read).
    Private Sub Render(page As String, Optional reload As Boolean = False)
        Dim message As String = Nothing
        Dim anchor As String = AnchorOf(page, message)
        If anchor Is Nothing Then
            ' Not in the book (a topic this login may not read, a topic that is gone).
            _bookLoading = False
            _bookReady = False
            SetHtml(message, Nothing)
            Return
        End If
        If _bookReady AndAlso Not reload Then
            ScrollTo(anchor)
            Return
        End If
        If _bookLoading AndAlso Not reload Then
            ' The book is on its way: only the place it opens at changes.
            If _pendingHtml IsNot Nothing Then _pendingAnchor = anchor Else _scrollAnchor = anchor
            Return
        End If

        Dim library As HelpLibrary = _service.Library
        Dim parts As List(Of HelpPart) = _service.VisibleParts()
        _anchors.Clear()
        _anchors(HelpHtml.HomeAnchor) = HomeKey
        For Each part As HelpPart In parts
            _anchors(HelpHtml.PartAnchor(part)) = PartPage(part)
            For Each t As HelpTopic In library.InReadingOrder(part)
                _anchors(HelpHtml.TopicAnchor(t.Id)) = TopicPrefix & t.Id
            Next
        Next
        _bookReady = False
        _bookLoading = True
        SetHtml(HelpHtml.Book(library, parts), anchor)
    End Sub

    ' The id of the section a page name stands for; Nothing when the page is not in the book,
    ' and then <messageHtml> is what to show instead.
    Private Function AnchorOf(page As String, ByRef messageHtml As String) As String
        Dim parts As List(Of HelpPart) = _service.VisibleParts()
        Dim part As HelpPart
        If TryPartOf(page, part) Then Return If(parts.Contains(part), HelpHtml.PartAnchor(part), HelpHtml.HomeAnchor)
        Dim topicId As String = TopicIdOf(page)
        If topicId.Length = 0 Then Return HelpHtml.HomeAnchor

        Dim topic As HelpTopic = _service.Library.Find(topicId)
        If topic Is Nothing Then
            messageHtml = HelpHtml.MessagePage("Pagina nu există", {"Pagina de ajutor cerută nu se găsește. Folosește cuprinsul din stânga."})
            Return Nothing
        End If
        If Not parts.Contains(topic.Part) Then
            messageHtml = HelpHtml.MessagePage(topic.Title, {"Pagina aceasta face parte din «" & HelpTopic.PartTitle(topic.Part) & "», care nu e disponibilă pentru contul tău."})
            Return Nothing
        End If
        Dim hash As Integer = page.IndexOf("#"c)
        Return If(hash < 0 OrElse hash = page.Length - 1, HelpHtml.TopicAnchor(topic.Id),
                  HelpHtml.SectionAnchor(topic.Id, page.Substring(hash + 1)))
    End Function

    ' The WebBrowser drops a DocumentText set before the window is shown or while a previous page
    ' is still loading (seen on screen: F1 opened the window with a blank page). Such a page waits
    ' here and is drawn from OnShown / DocumentCompleted. Only the newest one is kept.
    ' Slice 0000-18: the place to scroll to travels with its page and is used once it has loaded.
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

    ' Brings a section of the loaded book to the top of the view.
    Private Sub ScrollTo(anchor As String)
        Dim doc As HtmlDocument = web.Document
        If doc Is Nothing Then Return
        If String.Equals(anchor, HelpHtml.HomeAnchor, StringComparison.Ordinal) Then
            doc.Window?.ScrollTo(0, 0)
        Else
            Dim el As HtmlElement = doc.GetElementById(anchor)
            If el Is Nothing Then
                ' A section the text no longer has (an old history entry): its topic.
                Dim cut As Integer = anchor.IndexOf(HelpHtml.SectionSeparator, StringComparison.Ordinal)
                If cut > 0 Then el = doc.GetElementById(anchor.Substring(0, cut))
            End If
            el?.ScrollIntoView(True)
        End If
        ' The tree already shows this page; it follows again only once the reader scrolls.
        _lastScrollY = ScrollY()
    End Sub

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        Try
            _shown = True
            If _pendingHtml IsNot Nothing Then SetHtml(_pendingHtml, _pendingAnchor)
            If _service IsNot Nothing Then tmrScroll.Start()
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.OnShown", ex)
        End Try
    End Sub

    Private Sub Web_DocumentCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs) Handles web.DocumentCompleted
        Try
            If _pendingHtml IsNot Nothing Then
                SetHtml(_pendingHtml, _pendingAnchor)
                Return
            End If
            ' The book counts as loaded only when its start section is really there.
            If _bookLoading AndAlso web.Document?.GetElementById(HelpHtml.HomeAnchor) IsNot Nothing Then
                _bookLoading = False
                _bookReady = True
            End If
            Dim anchor As String = _scrollAnchor
            _scrollAnchor = Nothing
            If _bookReady AndAlso Not String.IsNullOrEmpty(anchor) Then
                ScrollTo(anchor)
            Else
                _lastScrollY = ScrollY()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.Web_DocumentCompleted", ex)
        End Try
    End Sub

    ' ── Slice 0000-32: the contents tree follows the scroll ──────────────────────
    ' A timer, not the page's scroll event: it needs nothing wired into each loaded document,
    ' and it asks the page one thing (what is at the top of the view) only after it has moved.

    ' A point just inside the top of the view, in the text column.
    Private Function ProbePoint() As Point
        Return New Point(Math.Max(8, web.ClientSize.Width \ 2), 6)
    End Function

    ' How far down the page is scrolled (the document or its body holds it, by rendering mode).
    Private Function ScrollY() As Integer
        Dim doc As HtmlDocument = web.Document
        If doc Is Nothing Then Return 0
        Dim y As Integer = 0
        If doc.Body IsNot Nothing Then y = doc.Body.ScrollTop
        Dim roots As HtmlElementCollection = doc.GetElementsByTagName("html")
        If roots.Count > 0 Then y = Math.Max(y, roots(0).ScrollTop)
        Return y
    End Function

    ' The page whose section is at the top of the view, or Nothing.
    Private Function PageAtTop() As String
        Dim el As HtmlElement = web.Document?.GetElementFromPoint(ProbePoint())
        Dim hops As Integer = 0
        While el IsNot Nothing AndAlso hops < 64
            Dim id As String = el.Id
            Dim page As String = Nothing
            If Not String.IsNullOrEmpty(id) AndAlso _anchors.TryGetValue(id, page) Then Return page
            el = el.Parent
            hops += 1
        End While
        Return Nothing
    End Function

    Private Sub TmrScroll_Tick(sender As Object, e As EventArgs) Handles tmrScroll.Tick
        Try
            If Not _bookReady OrElse web.IsBusy OrElse WindowState = FormWindowState.Minimized Then Return
            Dim y As Integer = ScrollY()
            If y = _lastScrollY Then Return
            _lastScrollY = y
            Dim page As String = PageAtTop()
            If page Is Nothing OrElse String.Equals(page, WithoutSection(_history.Current), StringComparison.Ordinal) Then Return
            ' Reached by scrolling: it becomes the page on screen (so Back returns here after
            ' a jump elsewhere), without a line in the history for every topic passed through.
            _history.Current = page
            SelectInTree(page)
        Catch ex As Exception
            ' UI boundary (timer). Stopped, so a page that cannot be read does not fill the log
            ' four times a second; the tree then follows only clicks, as before.
            tmrScroll.Stop()
            GlobalErrorLog.Write("HelpForm.TmrScroll_Tick", ex)
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

    ' ── Slice 0000-32: print and export, each with a menu of how much ────────────

    ' The topic on screen, when it is one this login reads; Nothing on the start page or a part.
    Private Function CurrentTopic() As HelpTopic
        Dim topic As HelpTopic = _service.Library.Find(TopicIdOf(_history.Current))
        If topic Is Nothing OrElse Not _service.VisibleParts().Contains(topic.Part) Then Return Nothing
        Return topic
    End Function

    ' Everything, then -- when a topic is on screen -- that topic, the topic with what is under
    ' it (if it has anything) and its parent with everything under it (if it has a parent).
    ' Print takes what this window shows; the exported manual keeps its own parts (ManualParts).
    Private Sub FillScopeMenu(menu As KBotDropDownMenu, forPrint As Boolean)
        menu.Items.Clear()
        Dim library As HelpLibrary = _service.Library
        Dim rows As New List(Of HelpScope) From {
            HelpScope.All(library, If(forPrint, _service.VisibleParts(), _service.ManualParts()))}
        Dim topic As HelpTopic = CurrentTopic()
        If topic IsNot Nothing Then
            rows.Add(HelpScope.One(topic))
            If library.Children(topic.Part, topic.Id).Count > 0 Then rows.Add(HelpScope.Under(library, topic, asChapter:=False))
            Dim parent As HelpTopic = library.Find(topic.Parent)
            If parent IsNot Nothing Then rows.Add(HelpScope.Under(library, parent, asChapter:=True))
        End If
        For Each scope As HelpScope In rows
            menu.Items.Add(New KBotMenuItem(scope.Kind.ToString(), scope.MenuText(forPrint)) With {.Tag = scope})
            If scope.Kind = HelpScopeKind.All Then menu.Items.Add(KBotMenuItem.Separator())
        Next
        If topic Is Nothing Then
            menu.Items.Add(New KBotMenuItem("none", "Subiectul curent") With {.Enabled = False, .ShortcutText = "deschide întâi un subiect"})
        End If
    End Sub

    Private Sub BtnPrint_Click(sender As Object, e As EventArgs) Handles btnPrint.Click
        Try
            FillScopeMenu(mnuPrint, forPrint:=True)
            mnuPrint.ShowBelow(btnPrint)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.BtnPrint_Click", ex)
        End Try
    End Sub

    Private Sub MnuPrint_ItemClicked(sender As Object, e As KBotMenuItemClickedEventArgs) Handles mnuPrint.ItemClicked
        Try
            Dim scope As HelpScope = TryCast(e.Item.Tag, HelpScope)
            If scope IsNot Nothing Then PrintScope(scope)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.MnuPrint_ItemClicked", ex)
            Using OwnDialog()
                KBotMessage.Show(Me, "Tipărirea nu a putut porni: " & ex.Message, MessageCaption,
                                 MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Using
        End Try
    End Sub

    ' Prints straight from the page on screen: the sections outside <scope> are marked as left
    ' out (a class that only the print rules read, so nothing changes on screen), then Windows'
    ' print dialog is shown. A large scope is asked about first.
    Private Sub PrintScope(scope As HelpScope)
        If Not _bookReady OrElse web.Document Is Nothing Then
            Using OwnDialog()
                KBotMessage.Show(Me, "Ajutorul încă se încarcă. Încearcă din nou peste o clipă.", MessageCaption,
                                 MessageBoxButtons.OK, MessageBoxIcon.Information)
            End Using
            Return
        End If
        If scope.IsLarge Then
            Dim answer As DialogResult
            Using OwnDialog()
                answer = KBotMessage.Show(Me,
                    "Vrei să tipărești " & scope.SpokenName & "? Sunt " & scope.Topics.Count & " subiecte, cu imagini: foarte multe pagini." &
                    vbCrLf & vbCrLf &
                    "Recomandarea K-BOT: nu tipări atât. Alege din același meniu doar subiectul sau capitolul de care ai nevoie." &
                    vbCrLf & vbCrLf & "Tipărești totuși?",
                    MessageCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2)
            End Using
            If answer <> DialogResult.Yes Then Return
        End If
        MarkForPrint(scope)
        Using OwnDialog()
            web.ShowPrintDialog()
        End Using
    End Sub

    ' Gives every section of the book its classes again, with HelpHtml.NoPrintClass on the ones
    ' <scope> leaves out. The start section is never printed (the page's own print rules).
    Private Sub MarkForPrint(scope As HelpScope)
        Dim whole As Boolean = scope.Kind = HelpScopeKind.All
        Dim keep As New HashSet(Of String)(scope.Topics.Select(Function(t) HelpHtml.TopicAnchor(t.Id)), StringComparer.Ordinal)
        Dim firstPart As String = HelpHtml.PartAnchor(_service.VisibleParts()(0))
        For Each pair As KeyValuePair(Of String, String) In _anchors
            If pair.Value.StartsWith(TopicPrefix, StringComparison.Ordinal) Then
                SetSectionClass(pair.Key, HelpHtml.TopicClass, printed:=keep.Contains(pair.Key))
            ElseIf pair.Value.StartsWith(PartPrefix, StringComparison.Ordinal) Then
                ' A part's heading goes on paper only with the whole help.
                SetSectionClass(pair.Key, HelpHtml.PartClass(String.Equals(pair.Key, firstPart, StringComparison.Ordinal)), printed:=whole)
            End If
        Next
    End Sub

    ' Both names of the attribute are written: which one the page listens to depends on the
    ' mode it is rendered in, and the other is an attribute nobody reads.
    Private Sub SetSectionClass(anchor As String, classes As String, printed As Boolean)
        Dim el As HtmlElement = web.Document.GetElementById(anchor)
        If el Is Nothing Then Return
        Dim value As String = If(printed, classes, classes & " " & HelpHtml.NoPrintClass)
        el.SetAttribute("className", value)
        el.SetAttribute("class", value)
    End Sub

    Private Sub BtnManual_Click(sender As Object, e As EventArgs) Handles btnManual.Click
        Try
            FillScopeMenu(mnuExport, forPrint:=False)
            mnuExport.ShowBelow(btnManual)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.BtnManual_Click", ex)
        End Try
    End Sub

    Private Sub MnuExport_ItemClicked(sender As Object, e As KBotMenuItemClickedEventArgs) Handles mnuExport.ItemClicked
        Try
            Dim scope As HelpScope = TryCast(e.Item.Tag, HelpScope)
            If scope Is Nothing Then Return
            Using OwnDialog()
                _service.Export(Me, scope)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.MnuExport_ItemClicked", ex)
            Using OwnDialog()
                KBotMessage.Show(Me, "Fișierul nu a putut fi salvat: " & ex.Message, MessageCaption,
                                 MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Using
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
            tmrScroll.Stop()
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
                Dim partNode As New TreeNode(HelpTopic.PartTitle(part)) With {.Tag = PartPage(part)}
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

    ' The node of a page becomes the selected one, without a page change. The start page has no
    ' node: nothing stays selected, so a click on any node is a new selection.
    Private Sub SelectInTree(page As String)
        page = WithoutSection(page)
        Dim isHome As Boolean = String.Equals(page, HomeKey, StringComparison.Ordinal)
        Dim node As TreeNode = If(isHome, Nothing, FindNode(tvCuprins.Nodes, page))
        If node Is Nothing AndAlso Not isHome Then Return
        _syncingTree = True
        Try
            tvCuprins.SelectedNode = node
            node?.EnsureVisible()
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

    ' Slice 0000-32: a click on the node that is already selected (no AfterSelect) goes back to
    ' the start of its topic -- the reader may be further down in it.
    Private Sub TvCuprins_NodeMouseClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles tvCuprins.NodeMouseClick
        Try
            If e.Button <> MouseButtons.Left OrElse e.Node Is Nothing OrElse e.Node IsNot tvCuprins.SelectedNode Then Return
            Dim page As String = TryCast(e.Node.Tag, String)
            If Not String.IsNullOrEmpty(page) Then Go(page, remember:=True)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpForm.TvCuprins_NodeMouseClick", ex)
        End Try
    End Sub

End Class
