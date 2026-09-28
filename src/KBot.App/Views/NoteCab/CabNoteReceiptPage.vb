Option Strict On
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' «Recipisă» of the «Note corecție» view (slice 0088-04): the FOREXE receipt of the note's upload,
''' in the same <see cref="ReaderHostPreview"/> and with the same lazy mount as
''' <see cref="CabNoteDocumentPage"/>.
'''
''' <para>Operator (28.09.2026): a note with a receipt shows the receipt itself -- no button. A note
''' without one shows the «document lipsă» surface with «Validează documentul»; the click goes up as
''' <see cref="GenerateRequested"/> and <c>NoteCabView</c> opens <see cref="CabNoteReceiptForm"/>.
''' A receipt is never signed, so the page has no signing session.</para>
''' </summary>
Public Class CabNoteReceiptPage
    Implements ICabNotePage, IThemedControl

    Private _pendingPath As String
    Private _pendingExists As Boolean
    Private _shownPath As String
    Private _shownExists As Boolean

    ' The path shown for a note that has no receipt: never a real file, so the preview always
    ' shows its «document lipsă» surface for it.
    Private Const NoReceiptPath As String = "?recipisa-lipsa"
    ' The receipt is on the server and on its way here: a line, never the «Validează» button.
    Private Const DownloadingPath As String = "?recipisa-descarcare"

    Public Event GenerateRequested As EventHandler Implements ICabNotePage.GenerateRequested

    Public Sub New()
        InitializeComponent()
        previewPdf.SetMissingTexts(
            "Recipisa FOREXE a acestei note nu este încă în K-BOT.",
            "Validează documentul",
            "Validează documentul",
            "Caută în FOREXE recipisa încărcării notei, o descarcă și o păstrează pe server.")
    End Sub

    Public ReadOnly Property PageKey As String Implements ICabNotePage.PageKey
        Get
            Return "recipisa"
        End Get
    End Property

    Public Sub SetContext(ctx As CabNotePageContext) Implements ICabNotePage.SetContext
        Try
            If ctx Is Nothing OrElse ctx.Note Is Nothing Then
                _pendingPath = Nothing
                _pendingExists = False
            ElseIf ctx.Note.Receipt Is Nothing Then
                _pendingPath = NoReceiptPath
                _pendingExists = False
            ElseIf String.IsNullOrEmpty(ctx.ReceiptPath) OrElse Not ctx.ReceiptExists Then
                ' On the server, not on this computer yet: NoteCabView is downloading it.
                _pendingPath = DownloadingPath
                _pendingExists = False
            Else
                _pendingPath = ctx.ReceiptPath
                _pendingExists = ctx.ReceiptExists
            End If
            MountIfVisible()
        Catch ex As Exception
            GlobalErrorLog.Write("CabNoteReceiptPage.SetContext", ex)
            Throw
        End Try
    End Sub

    ' UI boundary: log and swallow. Here -- and ONLY here -- Adobe may start.
    Private Sub CabNoteReceiptPage_VisibleChanged(sender As Object, e As EventArgs) Handles Me.VisibleChanged
        Try
            MountIfVisible()
        Catch ex As Exception
            GlobalErrorLog.Write("CabNoteReceiptPage.VisibleChanged", ex)
        End Try
    End Sub

    Private Sub MountIfVisible()
        If Not Visible Then Return
        If String.Equals(_shownPath, _pendingPath, StringComparison.Ordinal) AndAlso
           _shownExists = _pendingExists Then Return

        _shownPath = _pendingPath
        _shownExists = _pendingExists

        If String.IsNullOrEmpty(_pendingPath) Then
            previewPdf.Clear()
        ElseIf _pendingPath = DownloadingPath Then
            previewPdf.ShowNotice("Se descarcă recipisa de pe server…")
        Else
            previewPdf.ShowDocument(_pendingPath, _pendingExists)
        End If
    End Sub

    ' Trivial: hands «Validează documentul» up to the host (NoteCabView).
    Private Sub previewPdf_GenerateRequested(sender As Object, e As EventArgs) Handles previewPdf.GenerateRequested
        RaiseEvent GenerateRequested(Me, EventArgs.Empty)
    End Sub

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            lblEmpty.ForeColor = p.TextDimColor
            lblEmpty.BackColor = p.SurfaceAltColor
            previewPdf.ApplyTheme(scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("CabNoteReceiptPage.ApplyTheme", ex)
        End Try
    End Sub

End Class
