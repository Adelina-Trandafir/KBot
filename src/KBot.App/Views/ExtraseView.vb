Option Strict On
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' The Extrase view (slice 0080-02): the bank statements of the angajament selected in the main
''' tree -- the operations whose <c>FX_Extrase.CodContract</c> is its code, and their headers.
''' Data from GET /api/forexe/extrase/lista?cod=, always through the shell's re-login net; the
''' tree, the grids and the detail are <see cref="ExtrasePanel"/>.
'''
''' <para>The right icon of the tree footer downloads the SNM statements -- the same action as
''' the left icon of the main tree used to be (operator, 24.09.2026) -- and the view reloads
''' when the import wrote something.</para>
''' </summary>
Public Class ExtraseView
    Implements IAngajamentView, IThemedControl

    Private ReadOnly _apiClient As IApiClient
    Private ReadOnly _withReauth As Func(Of Func(Of Task(Of ExtraseInfo)), Task(Of ExtraseInfo))

    ''' <summary>
    ''' The shell's download-and-import of the statements. Returns True when something was
    ''' imported. Nothing = the host does not offer it, and the footer icon goes out.
    ''' </summary>
    Private ReadOnly _descarca As Func(Of Task(Of Boolean))

    ' The code asked for last -- the stale-guard of LoadAsync, as in every view.
    Private _requestedCod As String

    Public Sub New(apiClient As IApiClient,
                   withReauth As Func(Of Func(Of Task(Of ExtraseInfo)), Task(Of ExtraseInfo)),
                   Optional descarca As Func(Of Task(Of Boolean)) = Nothing)
        ArgumentNullException.ThrowIfNull(apiClient)
        ArgumentNullException.ThrowIfNull(withReauth)
        InitializeComponent()
        _apiClient = apiClient
        _withReauth = withReauth
        _descarca = descarca
        panel.Mode = ExtrasePanelMode.Angajament
        panel.ShowDownloadIcon = _descarca IsNot Nothing
        ShowEmpty("Selectați un angajament din arbore.")
    End Sub

    Public ReadOnly Property ViewKey As String Implements IAngajamentView.ViewKey
        Get
            Return "extrase"
        End Get
    End Property

    Public Sub SetContext(info As AngajamentTreeInfo) Implements IAngajamentView.SetContext
        Try
            Dim cod As String = info?.CodAngajament
            If String.IsNullOrWhiteSpace(cod) Then
                _requestedCod = Nothing
                panel.ClearData()
                ShowEmpty("Selectați un angajament din arbore.")
                Return
            End If
            _requestedCod = cod
            ShowEmpty("Se încarcă extrasele…")
            ' Fire-and-forget on purpose (synchronous shell handler): LoadAsync handles ALL of
            ' its errors itself, as in the other views.
            LoadAsync(cod)
        Catch ex As Exception
            GlobalErrorLog.Write("ExtraseView.SetContext", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Reloads the current angajament (after an import).</summary>
    Public Sub Reincarca()
        Try
            If String.IsNullOrWhiteSpace(_requestedCod) Then Return
            ShowEmpty("Se încarcă extrasele…")
            LoadAsync(_requestedCod)
        Catch ex As Exception
            GlobalErrorLog.Write("ExtraseView.Reincarca", ex)
            Throw
        End Try
    End Sub

    ' UI boundary: logs and SHOWS the error (started without await, nobody else can catch it).
    Private Async Sub LoadAsync(cod As String)
        Try
            Dim data As ExtraseInfo = Await _withReauth(
                Function() _apiClient.GetExtraseListaAsync(cod, CancellationToken.None)).ConfigureAwait(True)
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return

            If data Is Nothing OrElse (data.Antete.Count = 0 AndAlso data.Operatiuni.Count = 0) Then
                panel.ClearData()
                ShowEmpty("Angajamentul nu are extrase de cont.")
                Return
            End If
            panel.SetData(data)
            ShowContent()
        Catch ex As ApiException
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return
            GlobalErrorLog.Write("ExtraseView.LoadAsync", ex)
            panel.ClearData()
            ShowEmpty(ex.Message)   ' the server's Romanian «error» field
        Catch ex As Exception
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return
            GlobalErrorLog.Write("ExtraseView.LoadAsync", ex)
            panel.ClearData()
            ShowEmpty("Extrasele nu au putut fi încărcate. Detalii în jurnalul de erori.")
        End Try
    End Sub

    ' The footer icon: download + import through the shell, then reload if anything came in.
    Private Async Sub Panel_DescarcaCerut(sender As Object, e As EventArgs) Handles panel.DescarcaCerut
        Try
            If _descarca Is Nothing Then Return
            Dim importat As Boolean = Await _descarca().ConfigureAwait(True)
            If importat Then Reincarca()
        Catch ex As Exception
            ' UI boundary (async Sub): the shell already told the operator what failed.
            GlobalErrorLog.Write("ExtraseView.Panel_DescarcaCerut", ex)
        End Try
    End Sub

    Private Sub ShowEmpty(message As String)
        lblEmpty.Text = message
        lblEmpty.Visible = True
        panel.Visible = False
    End Sub

    Private Sub ShowContent()
        lblEmpty.Visible = False
        panel.Visible = True
    End Sub

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            panel.ApplyTheme(scheme)
            lblEmpty.ForeColor = p.TextDimColor
            lblEmpty.BackColor = p.SurfaceAltColor
        Catch ex As Exception
            GlobalErrorLog.Write("ExtraseView.ApplyTheme", ex)
        End Try
    End Sub

End Class
