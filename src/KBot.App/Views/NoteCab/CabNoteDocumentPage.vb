Option Strict On
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' «Document» of the «Note corecție» view (slice 0088): the note's PDF, in the same
''' <see cref="ReaderHostPreview"/> as the DDF / ORD «Document» pages and with the same lazy mount
''' (copied from <c>OrdDocumentPage</c>): <see cref="SetContext"/> only remembers the target; Adobe
''' is started only when the page is visible and the (path, exists) pair changed.
'''
''' <para>No file = the preview's «document lipsa» surface with its «Genereaza» button; the click
''' goes up as <see cref="GenerateRequested"/>, and <c>NoteCabView</c> generates the unsigned note.
''' A SIGNED note is never generated: the view downloads the server's copy instead.</para>
''' </summary>
Public Class CabNoteDocumentPage
    Implements ICabNotePage, IThemedControl

    Private _pendingPath As String
    Private _pendingExists As Boolean
    Private _shownPath As String
    Private _shownExists As Boolean
    Private _pendingSigning As PdfSigningSession
    ' Slice 0099: the print-count target that goes with the pending target.
    Private _pendingPrint As PdfPrintTarget

    Public Event GenerateRequested As EventHandler Implements ICabNotePage.GenerateRequested

    Public Sub New()
        InitializeComponent()
    End Sub

    Public ReadOnly Property PageKey As String Implements ICabNotePage.PageKey
        Get
            Return "document"
        End Get
    End Property

    Public Sub SetContext(ctx As CabNotePageContext) Implements ICabNotePage.SetContext
        Try
            If ctx Is Nothing OrElse String.IsNullOrEmpty(ctx.PdfPath) Then
                _pendingPath = Nothing
                _pendingExists = False
                _pendingSigning = Nothing
                _pendingPrint = Nothing
            Else
                _pendingPath = ctx.PdfPath
                _pendingExists = ctx.PdfExists
                _pendingSigning = ctx.Signing
                _pendingPrint = ctx.PrintTarget
            End If
            MountIfVisible()
        Catch ex As Exception
            GlobalErrorLog.Write("CabNoteDocumentPage.SetContext", ex)
            Throw
        End Try
    End Sub

    ' UI boundary: log and swallow. Here -- and ONLY here -- Adobe may start.
    Private Sub CabNoteDocumentPage_VisibleChanged(sender As Object, e As EventArgs) Handles Me.VisibleChanged
        Try
            MountIfVisible()
        Catch ex As Exception
            GlobalErrorLog.Write("CabNoteDocumentPage.VisibleChanged", ex)
        End Try
    End Sub

    Private Sub MountIfVisible()
        If Not Visible Then Return
        previewPdf.Signing = _pendingSigning
        ' Slice 0099: before ShowDocument -- the document shown takes the target it finds.
        previewPdf.PrintTarget = _pendingPrint
        If String.Equals(_shownPath, _pendingPath, StringComparison.Ordinal) AndAlso
           _shownExists = _pendingExists Then Return

        _shownPath = _pendingPath
        _shownExists = _pendingExists

        If String.IsNullOrEmpty(_pendingPath) Then
            previewPdf.Clear()
        Else
            previewPdf.ShowDocument(_pendingPath, _pendingExists)
        End If
    End Sub

    ' Trivial: hands the generate request up to the host (NoteCabView).
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
            GlobalErrorLog.Write("CabNoteDocumentPage.ApplyTheme", ex)
        End Try
    End Sub

End Class
