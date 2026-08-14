Option Strict On
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Pagina «Fișiere» a vederii DDF: <see cref="DdfFileBrowser"/>, adică PDF-urile
''' angajamentului curent găsite sub rădăcina configurată (<c>KBotPaths.DdfPdfRoot</c>).
'''
''' Rădăcina NU e o dependență injectată, ci aceeași citire statică de configurație pe care o
''' făcea <c>DdfView</c> înainte de felia 0032 — pagina rămâne fără DI, deci se instanțiază în
''' designer. Selectarea unui rând urcă neschimbată spre gazdă, care calculează ținta PDF și
''' comută pe pagina «Document».
''' </summary>
Public Class DdfFisierePage
    Implements IDdfPage, IThemedControl

    ' Pagina «Fișiere» nu are suprafață de generare -> nu ridică niciodată acest eveniment.
    ' Rămâne declarat ca gazda să se poată abona uniform la toate paginile.
    Public Event GenerateRequested As EventHandler Implements IDdfPage.GenerateRequested
    Public Event FileActivated As EventHandler(Of String) Implements IDdfPage.FileActivated

    Public Sub New()
        InitializeComponent()
        ' Browserul își are propria stare goală; eticheta de dedesubt e doar plasa de siguranță.
        lblFisiereGol.Visible = False
    End Sub

    Public ReadOnly Property PageKey As String Implements IDdfPage.PageKey
        Get
            Return "fisiere"
        End Get
    End Property

    ''' <summary>
    ''' Reîncarcă lista pentru CodAngajament-ul din context. <c>Nothing</c> (sau cod gol) -&gt;
    ''' browserul își arată singur starea goală (nu aruncă, nu creează foldere).
    ''' </summary>
    Public Sub SetContext(ctx As DdfPageContext) Implements IDdfPage.SetContext
        Try
            Dim cod As String = If(ctx Is Nothing, Nothing, ctx.Cod)
            If String.IsNullOrWhiteSpace(cod) Then
                browser.SetContext(Nothing, Nothing)
                Return
            End If
            browser.SetContext(KBotPaths.Current.DdfPdfRoot, cod)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfFisierePage.SetContext", ex)
            Throw
        End Try
    End Sub

    ' Trivial: urcă rândul ales spre gazdă (DdfView rutează spre pagina «Document»).
    Private Sub browser_FileActivated(pdfPath As String) Handles browser.FileActivated
        RaiseEvent FileActivated(Me, pdfPath)
    End Sub

    ''' <summary>Cascadă: fundalul paginii + browserul (care se auto-temează).</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            lblFisiereGol.ForeColor = p.TextDimColor
            lblFisiereGol.BackColor = p.SurfaceAltColor
            browser.ApplyTheme(scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfFisierePage.ApplyTheme", ex)
        End Try
    End Sub

End Class
