Option Strict On
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' Pagina «Document» a vederii DDF: PDF-ul REAL (<see cref="ReaderHostPreview"/>), distinct de
''' «Vizualizare» (reconstrucția din XML XFA). Aduce cu ea banda de setări Adobe (felia 0024) și
''' banda de butoane de jos.
'''
''' ÎNCĂRCARE LENEȘĂ, gardă pe perechea (cale, existență): <see cref="SetContext"/> doar REȚINE
''' ținta; încorporarea — singurul loc de unde poate porni Adobe — se face abia când pagina
''' devine vizibilă. De aceea garda nu poate fi pe cale singură: după o generare, calea rămâne
''' aceeași și doar existența se întoarce din False în True, iar o gardă pe cale ar sări exact
''' re-încorporarea care trebuia făcută.
''' </summary>
Public Class DdfDocumentPage
    Implements IDdfPage, IThemedControl

    ' Ținta cerută (reținută) și ce e efectiv încorporat acum — perechea (cale, existență).
    Private _pendingPath As String
    Private _pendingExists As Boolean
    Private _shownPath As String
    Private _shownExists As Boolean
    ' Se populează combo-urile de setări chiar acum? Atunci o selecție programatică nu are voie
    ' să declanșeze o salvare.
    ' Private _suppressAdobeComboEvent As Boolean

    Public Event GenerateRequested As EventHandler Implements IDdfPage.GenerateRequested

    ' Pagina «Document» nu listează fișiere -> nu ridică niciodată acest eveniment. Rămâne
    ' declarat ca gazda să se poată abona uniform la toate paginile.
    Public Event FileActivated As EventHandler(Of String) Implements IDdfPage.FileActivated

    Public Sub New()
        InitializeComponent()
        'BuildAdobeCombos()
    End Sub

    Public ReadOnly Property PageKey As String Implements IDdfPage.PageKey
        Get
            Return "document"
        End Get
    End Property

    ''' <summary>
    ''' Reține ținta PDF a nodului curent. Nimic selectat / o rădăcină de lună -&gt; ținta se
    ''' golește. NU încorporează nimic cât timp pagina e ascunsă (vezi nota clasei).
    ''' </summary>
    Public Sub SetContext(ctx As DdfPageContext) Implements IDdfPage.SetContext
        Try
            ' O rădăcină de lună ajunge aici cu PdfPath gol — părintele nu compune cale decât
            ' pentru o frunză (sau pentru fișierul ales din listă), deci nu mai verificăm IsRoot.
            If ctx Is Nothing OrElse String.IsNullOrEmpty(ctx.PdfPath) Then
                _pendingPath = Nothing
                _pendingExists = False
            Else
                _pendingPath = ctx.PdfPath
                _pendingExists = ctx.PdfExists
            End If
            MountIfVisible()
        Catch ex As Exception
            GlobalErrorLog.Write("DdfDocumentPage.SetContext", ex)
            Throw
        End Try
    End Sub

    ' Graniță UI: loghează și înghite. Aici — și DOAR aici — poate porni Adobe.
    Private Sub DdfDocumentPage_VisibleChanged(sender As Object, e As EventArgs) Handles Me.VisibleChanged
        Try
            MountIfVisible()
        Catch ex As Exception
            GlobalErrorLog.Write("DdfDocumentPage.VisibleChanged", ex)
        End Try
    End Sub

    ' Încorporează ținta reținută, dar numai dacă pagina e pe ecran ȘI perechea (cale, existență)
    ' s-a schimbat față de ce e afișat — ca să nu re-încorporăm (și să nu relansăm Adobe) la
    ' fiecare comutare de pagină.
    Private Sub MountIfVisible()
        If Not Visible Then Return
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

    ' Trivial: ridică mai departe cererea de generare spre gazdă (DdfView).
    Private Sub previewPdf_GenerateRequested(sender As Object, e As EventArgs) _
        Handles previewPdf.GenerateRequested
        RaiseEvent GenerateRequested(Me, EventArgs.Empty)
    End Sub

    ''' <summary>Cascadă: chrome-ul paginii + suprafața PDF (care se auto-temează).</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette

            BackColor = p.SurfaceAltColor
            pnlBottomButtons.BackColor = p.SurfaceAltColor

            previewPdf.ApplyTheme(scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfDocumentPage.ApplyTheme", ex)
        End Try
    End Sub

End Class

