Option Strict On
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Pagina «Vizualizare» a vederii DDF: reconstrucția documentului din XML-ul XFA, adică
''' suprafața aleasă LA COMPILARE de <see cref="DdfPreviewFactory"/> (implicit
''' <see cref="XfaXmlPreview"/>, rezervă <see cref="ReaderHostPreview"/>).
'''
''' Pagina e proastă: nu știe nici de rețea, nici de generare. Primește calea PDF-ului și
''' flag-ul de existență prin <see cref="SetContext"/> și le dă mai departe suprafeței;
''' butonul «Generează documentul» al suprafeței urcă neschimbat spre <c>DdfView</c>, singurul
''' care are cu ce genera.
''' </summary>
Public Class DdfVizualizarePage
    Implements IDdfPage, IThemedControl

    ' Suprafața de previzualizare aleasă la compilare. Pe calea implicită E chiar `previewXfa`
    ' (instanța designerului); pe calea de rezervă e o instanță nouă, montată în constructor.
    Private ReadOnly _preview As IDdfPreview

    Public Event GenerateRequested As EventHandler Implements IDdfPage.GenerateRequested
    ' Pagina «Vizualizare» nu listează fișiere -> nu ridică niciodată acest eveniment. Rămâne
    ' declarat ca gazda să se poată abona uniform la toate paginile.
    Public Event FileActivated As EventHandler(Of String) Implements IDdfPage.FileActivated

    Public Sub New()
        InitializeComponent()
        _preview = DdfPreviewFactory.Create(previewXfa)
        MountPreview()
    End Sub

    Public ReadOnly Property PageKey As String Implements IDdfPage.PageKey
        Get
            Return "previzualizare"
        End Get
    End Property

    ' Montează suprafața aleasă la compilare și se abonează la butonul «Generează documentul».
    Private Sub MountPreview()
        Try
            Dim surface As Control = _preview.Surface
            ' Calea implicită: suprafața E `previewXfa`, deja creată și așezată de designer — nu
            ' mai e nimic de montat. Calea de rezervă (constanta de compilare pe AdobeReader) aduce
            ' o instanță nouă, neparentată: o montăm acum și ascundem suprafața din designer, ca să
            ' nu rămână două una peste alta.
            If surface.Parent Is Nothing Then
                surface.Dock = DockStyle.Fill
                ' Suprafața acoperă pagina; eticheta goală rămâne dedesubt ca plasă.
                Controls.Add(surface)
                surface.BringToFront()
                previewXfa.Visible = False
            End If
            AddHandler _preview.GenerateRequested, AddressOf Preview_GenerateRequested
        Catch ex As Exception
            GlobalErrorLog.Write("DdfVizualizarePage.MountPreview", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Nimic selectat sau o rădăcină de lună -&gt; suprafața se golește (o lună nu are UN singur
    ''' document). O frunză -&gt; i se dă calea așteptată și flag-ul de existență, iar suprafața
    ''' decide singură între randare și starea „document lipsă" (contractul IDdfPreview).
    ''' </summary>
    Public Sub SetContext(ctx As DdfPageContext) Implements IDdfPage.SetContext
        Try
            ' O rădăcină de lună ajunge aici cu PdfPath gol — părintele nu compune cale decât
            ' pentru o frunză (sau pentru fișierul ales din listă), deci nu mai verificăm IsRoot.
            If ctx Is Nothing OrElse String.IsNullOrEmpty(ctx.PdfPath) Then
                _preview.Clear()
                Return
            End If
            _preview.ShowDocument(ctx.PdfPath, ctx.PdfExists)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfVizualizarePage.SetContext", ex)
            Throw
        End Try
    End Sub

    ' Trivial: ridică mai departe cererea de generare spre gazdă (DdfView).
    Private Sub Preview_GenerateRequested(sender As Object, e As EventArgs)
        RaiseEvent GenerateRequested(Me, EventArgs.Empty)
    End Sub

    ''' <summary>Cascadă: fundalul paginii + suprafața găzduită (care se auto-temează).</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            lblPreviewGol.ForeColor = p.TextDimColor
            lblPreviewGol.BackColor = p.SurfaceAltColor
            Dim themed As IThemedControl = TryCast(_preview, IThemedControl)
            themed?.ApplyTheme(scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfVizualizarePage.ApplyTheme", ex)
        End Try
    End Sub

End Class
