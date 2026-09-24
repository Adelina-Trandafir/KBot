Option Strict On
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Pagina «Document» a vederii ORD (felia 0033): PDF-ul REAL al ordonanțării selectate, în
''' aceeași suprafață <see cref="ReaderHostPreview"/> pe care o folosește DDF-ul.
'''
''' ÎNCĂRCARE LENEȘĂ, gardă pe perechea (cale, existență) — copiată din
''' <c>DdfDocumentPage</c>: <see cref="SetContext"/> doar REȚINE ținta; încorporarea —
''' singurul loc de unde poate porni Adobe — se face abia când pagina devine vizibilă. Garda
''' nu poate fi pe cale singură: existența se poate schimba sub aceeași cale (azi doar prin
''' generare din afara K-BOT, mâine prin felia de generare ORD), iar o gardă pe cale ar sări
''' exact re-încorporarea care trebuia făcută.
'''
''' Without a PDF on disk, <c>ReaderHostPreview</c> shows its "document lipsa" surface with
''' the "Genereaza" button; the click is raised up as <see cref="GenerateRequested"/> and
''' <c>OrdView.OnGenerateRequested</c> builds the PDF, exactly as DDF does.
''' </summary>
Public Class OrdDocumentPage
    Implements IOrdPage, IThemedControl

    ' Ținta cerută (reținută) și ce e efectiv încorporat acum — perechea (cale, existență).
    Private _pendingPath As String
    Private _pendingExists As Boolean
    Private _shownPath As String
    Private _shownExists As Boolean
    ' Slice 0078: the signing session that goes with the pending target.
    Private _pendingSigning As PdfSigningSession
    ' Mesajul stării goale, ales la fiecare context (nicio selecție vs. PDF inexistent).
    'Private _mesajGol As String = "Selectați o ordonanțare din arbore."

    Public Event GenerateRequested As EventHandler Implements IOrdPage.GenerateRequested

    ' Pagina «Document» nu listează fișiere -> nu ridică niciodată acest eveniment. Rămâne
    ' declarat ca gazda să se poată abona uniform la toate paginile.
    Public Event FileActivated As EventHandler(Of String) Implements IOrdPage.FileActivated
    Public Sub New()
        InitializeComponent()
    End Sub

    Public ReadOnly Property PageKey As String Implements IOrdPage.PageKey
        Get
            Return "document"
        End Get
    End Property

    ''' <summary>
    ''' Reține ținta PDF a nodului curent. Nimic selectat / o rădăcină de lună -&gt; ținta se
    ''' golește (părintele nu compune cale decât pentru o frunză). NU încorporează nimic cât
    ''' timp pagina e ascunsă (vezi nota clasei).
    ''' </summary>
    Public Sub SetContext(ctx As OrdPageContext) Implements IOrdPage.SetContext
        Try
            ' O rădăcină de lună ajunge aici cu PdfPath gol — părintele nu compune cale decât
            ' pentru o frunză (sau pentru fișierul ales din listă), deci nu mai verificăm IsRoot.
            If ctx Is Nothing OrElse String.IsNullOrEmpty(ctx.PdfPath) Then
                _pendingPath = Nothing
                _pendingExists = False
                _pendingSigning = Nothing
            Else
                _pendingPath = ctx.PdfPath
                _pendingExists = ctx.PdfExists
                _pendingSigning = ctx.Signing
            End If
            MountIfVisible()
        Catch ex As Exception
            GlobalErrorLog.Write("OrdDocumentPage.SetContext", ex)
            Throw
        End Try
    End Sub

    ' Graniță UI: loghează și înghite. Aici — și DOAR aici — poate porni Adobe.
    Private Sub OrdDocumentPage_VisibleChanged(sender As Object, e As EventArgs) Handles Me.VisibleChanged
        Try
            MountIfVisible()
        Catch ex As Exception
            GlobalErrorLog.Write("OrdDocumentPage.VisibleChanged", ex)
        End Try
    End Sub

    ' Încorporează ținta reținută, dar numai dacă pagina e pe ecran ȘI perechea (cale,
    ' existență) s-a schimbat față de ce e afișat — ca să nu relansăm Adobe la fiecare
    ' comutare de pagină.
    Private Sub MountIfVisible()
        If Not Visible Then Return
        ' Slice 0078: outside the (path, exists) guard -- a new session for the same file (after an
        ' upload, or a view reload) must reach the preview without re-launching Adobe.
        previewPdf.Signing = _pendingSigning
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

    ' Trivial: hands the generate request up to the host (OrdView).
    Private Sub previewPdf_GenerateRequested(sender As Object, e As EventArgs) _
        Handles previewPdf.GenerateRequested
        RaiseEvent GenerateRequested(Me, EventArgs.Empty)
    End Sub

    ''' <summary>Cascadă: fundalul paginii + starea goală; suprafața PDF se auto-temează.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette

            BackColor = p.SurfaceAltColor
            lblEmpty.ForeColor = p.TextDimColor
            lblEmpty.BackColor = p.SurfaceAltColor

            previewPdf.ApplyTheme(scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("OrdDocumentPage.ApplyTheme", ex)
        End Try
    End Sub

End Class
