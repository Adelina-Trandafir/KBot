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
''' Fără PDF pe disc arătăm eticheta noastră, NU suprafața „document lipsă" a
''' <c>ReaderHostPreview</c>: aceea poartă un buton «Generează», iar generarea ORD e o felie
''' ulterioară — un buton care nu face nimic e mai rău decât niciun buton.
''' </summary>
Public Class OrdDocumentPage
    Implements IOrdPage, IThemedControl

    ' Ținta cerută (reținută) și ce e efectiv încorporat acum — perechea (cale, existență).
    Private _pendingPath As String
    Private _pendingExists As Boolean
    Private _shownPath As String
    Private _shownExists As Boolean
    ' Mesajul stării goale, ales la fiecare context (nicio selecție vs. PDF inexistent).
    Private _mesajGol As String = "Selectați o ordonanțare din arbore."

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
            If ctx Is Nothing OrElse String.IsNullOrEmpty(ctx.PdfPath) Then
                _pendingPath = Nothing
                _pendingExists = False
                _mesajGol = "Selectați o ordonanțare din arbore."
            Else
                _pendingPath = ctx.PdfPath
                _pendingExists = ctx.PdfExists
                If Not ctx.PdfExists Then
                    _mesajGol = "Nu există PDF generat pentru această ordonanțare."
                End If
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
        If String.Equals(_shownPath, _pendingPath, StringComparison.Ordinal) AndAlso
           _shownExists = _pendingExists Then Return

        _shownPath = _pendingPath
        _shownExists = _pendingExists

        If String.IsNullOrEmpty(_pendingPath) OrElse Not _pendingExists Then
            ' Eliberăm fereastra găzduită înainte de a ascunde suprafața: altfel o fereastră
            ' Adobe reparentată ar rămâne agățată de un panou invizibil.
            previewPdf.Clear()
            previewPdf.Visible = False
            lblEmpty.Text = _mesajGol
            lblEmpty.Visible = True
            Return
        End If

        lblEmpty.Visible = False
        previewPdf.Visible = True
        previewPdf.ShowDocument(_pendingPath, True)
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
