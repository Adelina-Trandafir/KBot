Option Strict On
Imports System.Globalization
Imports System.IO

''' <summary>Displays rendered repository Markdown and follows links between Markdown files.</summary>
Friend NotInheritable Class MdReaderControl
    Inherits KBotThemedUserControl

    Friend Const MinimumZoom As Integer = 75
    Friend Const MaximumZoom As Integer = 150
    Friend Const DefaultZoom As Integer = 100
    Private Const BoundLinksAttribute As String = "data-md-reader-links-bound"

    Private WithEvents browser As New WebBrowser With {
        .Dock = DockStyle.Fill,
        .AccessibleName = "Conținut Markdown",
        .AllowWebBrowserDrop = False,
        .IsWebBrowserContextMenuEnabled = False,
        .ScriptErrorsSuppressed = True
    }
    Private ReadOnly _history As New Stack(Of Uri)()
    Private _root As String
    Private _current As Uri
    Private _pendingHtml As String
    Private _pendingAnchor As String
    Private _scrollAnchor As String
    Private _zoomPercent As Integer = DefaultZoom

    Friend Event DocumentChanged(relativePath As String)

    Public Sub New()
        Controls.Add(browser)
    End Sub

    Friend ReadOnly Property CanGoBack As Boolean
        Get
            Return _history.Count > 0
        End Get
    End Property

    Friend Property ZoomPercent As Integer
        Get
            Return _zoomPercent
        End Get
        Set(k_value As Integer)
            Dim k_percent As Integer = Math.Clamp(k_value, MinimumZoom, MaximumZoom)
            If _zoomPercent = k_percent Then Return
            _zoomPercent = k_percent
            If browser.IsHandleCreated AndAlso Not browser.IsBusy Then ApplyZoom(k_keepPlace:=True)
        End Set
    End Property

    Friend Sub OpenProjectFile(k_root As String, k_relativePath As String)
        _root = Path.GetFullPath(k_root)
        _history.Clear()
        _current = Nothing
        LoadPage(MdReaderHtml.FileUri(Path.GetFullPath(k_relativePath, _root)))
    End Sub

    Friend Sub GoBack()
        If Not CanGoBack Then Return
        Try
            LoadPage(_history.Peek(), k_goBack:=True)
        Catch ex As Exception
            ShowError(ex)
        End Try
    End Sub

    Private Sub LoadPage(k_target As Uri, Optional k_goBack As Boolean = False)
        Dim k_local As String = MdReaderHtml.LocalPathInProject(_root, k_target)
        If Not MdReaderHtml.IsMarkdownPath(k_local) Then Throw New IOException("Fișierul Markdown nu se află în proiect.")
        Dim k_html As String = MdReaderHtml.Render(_root, k_target, ThemeManager.Current)
        SetHtml(k_html, k_target.Fragment)
        If k_goBack Then
            _history.Pop()
        ElseIf _current IsNot Nothing Then
            _history.Push(_current)
        End If
        _current = k_target
        RaiseEvent DocumentChanged(Path.GetRelativePath(_root, k_local))
    End Sub

    ' The ActiveX browser may still be completing its initial blank page or the previous one.
    Private Sub SetHtml(k_html As String, k_anchor As String)
        If browser.IsBusy Then
            _pendingHtml = k_html
            _pendingAnchor = k_anchor
            Return
        End If
        _pendingHtml = Nothing
        _pendingAnchor = Nothing
        _scrollAnchor = k_anchor
        browser.DocumentText = k_html
    End Sub

    Private Sub Browser_Navigating(k_sender As Object, e As WebBrowserNavigatingEventArgs) Handles browser.Navigating
        Dim k_target As Uri = e.Url
        If k_target IsNot Nothing AndAlso String.Equals(k_target.AbsoluteUri, "about:blank", StringComparison.OrdinalIgnoreCase) Then Return
        e.Cancel = True
        If k_target Is Nothing Then Return

        Try
            If k_target.AbsoluteUri.StartsWith("about:blank#", StringComparison.OrdinalIgnoreCase) Then
                ScrollToAnchor(k_target.Fragment)
            ElseIf MdReaderHtml.IsMarkdownPath(MdReaderHtml.LocalPathInProject(_root, k_target)) Then
                ' Finish cancelling the browser's navigation before assigning new HTML.
                BeginInvoke(New Action(Sub() FollowMarkdownLink(k_target)))
            ElseIf MdReaderHtml.IsWebLink(k_target) Then
                Process.Start(New ProcessStartInfo(k_target.AbsoluteUri) With {.UseShellExecute = True})?.Dispose()
            End If
        Catch ex As Exception
            ShowError(ex)
        End Try
    End Sub

    Private Sub FollowMarkdownLink(k_target As Uri)
        If IsDisposed OrElse Disposing Then Return
        Try
            If _current IsNot Nothing AndAlso String.Equals(k_target.LocalPath, _current.LocalPath, StringComparison.OrdinalIgnoreCase) Then
                ScrollToAnchor(k_target.Fragment)
            Else
                LoadPage(k_target)
            End If
        Catch ex As Exception
            ShowError(ex)
        End Try
    End Sub

    Private Sub Browser_NewWindow(k_sender As Object, e As ComponentModel.CancelEventArgs) Handles browser.NewWindow
        e.Cancel = True
    End Sub

    Private Sub Browser_DocumentCompleted(k_sender As Object, e As WebBrowserDocumentCompletedEventArgs) Handles browser.DocumentCompleted
        Try
            If _pendingHtml IsNot Nothing Then
                SetHtml(_pendingHtml, _pendingAnchor)
                Return
            End If
            Dim k_document As HtmlDocument = browser.Document
            Dim k_content As HtmlElement = k_document?.GetElementById(MdReaderHtml.ContentId)
            If k_content Is Nothing Then Return
            BindMarkdownLinks(k_document, k_content)
            ApplyZoom(k_keepPlace:=False)
            If String.IsNullOrEmpty(_scrollAnchor) Then Return
            Dim k_anchor As String = _scrollAnchor
            _scrollAnchor = Nothing
            ScrollToAnchor(k_anchor)
        Catch ex As Exception
            GlobalErrorLog.Write("MdReaderControl.Browser_DocumentCompleted", ex)
        End Try
    End Sub

    Private Sub BindMarkdownLinks(k_document As HtmlDocument, k_content As HtmlElement)
        If k_content.GetAttribute(BoundLinksAttribute) = "1" Then Return
        For Each k_anchor As HtmlElement In k_document.Links
            Dim k_target As Uri = Nothing
            If Not Uri.TryCreate(k_anchor.GetAttribute(MdReaderHtml.LocalLinkAttribute), UriKind.Absolute, k_target) Then Continue For
            Dim k_link As Uri = k_target
            AddHandler k_anchor.Click,
                Sub(k_sender As Object, e As HtmlElementEventArgs)
                    e.ReturnValue = False
                    BeginInvoke(New Action(Sub() FollowMarkdownLink(k_link)))
                End Sub
        Next
        k_content.SetAttribute(BoundLinksAttribute, "1")
    End Sub

    Private Sub ApplyZoom(k_keepPlace As Boolean)
        Dim k_document As HtmlDocument = browser.Document
        If k_document?.Body Is Nothing OrElse k_document.GetElementById(MdReaderHtml.ContentId) Is Nothing Then Return

        Dim k_readingPosition As HtmlElement = If(k_keepPlace, ElementAtTop(k_document), Nothing)
        Dim k_zoom As String = _zoomPercent.ToString(CultureInfo.InvariantCulture)
        ' Trident keeps percentage widths relative to the unzoomed viewport.
        Dim k_width As String = (10000.0 / _zoomPercent).ToString("0.####", CultureInfo.InvariantCulture)
        k_document.Body.Style = If(_zoomPercent = DefaultZoom, String.Empty, $"zoom:{k_zoom}%;width:{k_width}%")
        k_readingPosition?.ScrollIntoView(True)
    End Sub

    Private Function ElementAtTop(k_document As HtmlDocument) As HtmlElement
        Dim k_roots As HtmlElementCollection = k_document.GetElementsByTagName("html")
        If k_document.Body.ScrollTop = 0 AndAlso (k_roots.Count = 0 OrElse k_roots(0).ScrollTop = 0) Then Return Nothing

        Dim k_center As Integer = browser.ClientSize.Width \ 2
        For k_y As Integer = 8 To Math.Min(120, browser.ClientSize.Height - 1) Step 16
            Dim k_element As HtmlElement = k_document.GetElementFromPoint(New Point(k_center, k_y))
            If k_element Is Nothing Then Continue For
            If k_element.TagName = "HTML" OrElse k_element.TagName = "BODY" Then Continue For
            If k_element.Id = MdReaderHtml.ContentId OrElse k_element.Id = "document" Then Continue For
            Return k_element
        Next
        Return Nothing
    End Function

    Private Sub ScrollToAnchor(k_anchor As String)
        If String.IsNullOrEmpty(k_anchor) OrElse k_anchor.Length = 1 Then
            browser.Document?.Window?.ScrollTo(0, 0)
        Else
            Dim k_id As String = Uri.UnescapeDataString(k_anchor.Substring(1))
            browser.Document?.GetElementById(k_id)?.ScrollIntoView(True)
        End If
    End Sub

    Private Sub ShowError(k_error As Exception)
        GlobalErrorLog.Write("MdReaderControl", k_error)
        KBotMessage.Show(Me, "Nu am putut deschide legătura: " & k_error.Message, "Cititor Markdown",
                         MessageBoxButtons.OK, MessageBoxIcon.Warning)
    End Sub

End Class
