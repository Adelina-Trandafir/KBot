Option Strict On
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' «Extrase de cont» (slice 0080-03, operator 24.09.2026): every bank statement of the
''' database -- headers with or without an angajament, operations with CodContract, with only
''' CodContract, or with neither (those stand for something other than an angajament). Opened
''' modal from the left icon of the main tree's footer, in place of the direct download that
''' icon used to run; the download is the button in this window's footer.
'''
''' <para>Data from GET /api/forexe/extrase/lista (no <c>cod</c>), through the shell's re-login
''' net. The body is <see cref="ExtrasePanel"/> in <see cref="ExtrasePanelMode.Toate"/> mode,
''' so the grids use the window's own column layouts from «Setări → Extrase».</para>
''' </summary>
Public Class ExtraseForm

    Private ReadOnly _apiClient As IApiClient
    Private ReadOnly _withReauth As Func(Of Func(Of Task(Of ExtraseInfo)), Task(Of ExtraseInfo))
    Private ReadOnly _descarca As Func(Of IWin32Window, Task(Of Boolean))
    Private _loading As Boolean

    Public Sub New(apiClient As IApiClient,
                   withReauth As Func(Of Func(Of Task(Of ExtraseInfo)), Task(Of ExtraseInfo)),
                   descarca As Func(Of IWin32Window, Task(Of Boolean)))
        ArgumentNullException.ThrowIfNull(apiClient)
        ArgumentNullException.ThrowIfNull(withReauth)
        ArgumentNullException.ThrowIfNull(descarca)
        InitializeComponent()
        _apiClient = apiClient
        _withReauth = withReauth
        _descarca = descarca
    End Sub

    Private Sub ExtraseForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            Me.KeyPreview = True   ' Escape closes: there is no native X on a borderless window
            LoadAsync()
        Catch ex As Exception
            ' UI boundary (Load): log and swallow.
            GlobalErrorLog.Write("ExtraseForm.ExtraseForm_Load", ex)
        End Try
    End Sub

    ' UI boundary: logs and SHOWS the error; started without await.
    Private Async Sub LoadAsync()
        If _loading Then Return
        _loading = True
        Try
            ShowEmpty("Se încarcă extrasele…")
            SetBusy(True)
            Dim data As ExtraseInfo = Await _withReauth(
                Function() _apiClient.GetExtraseListaAsync(Nothing, CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed Then Return
            If data Is Nothing OrElse (data.Antete.Count = 0 AndAlso data.Operatiuni.Count = 0) Then
                panel.ClearData()
                ShowEmpty("Baza nu are extrase de cont. «Descarcă extrasele din FOREXE» le aduce.")
                SetStatus(String.Empty)
                Return
            End If
            panel.SetData(data)
            ShowContent()
            SetStatus($"{data.Antete.Count} antete, {data.Operatiuni.Count} operațiuni.")
        Catch ex As ApiException
            GlobalErrorLog.Write("ExtraseForm.LoadAsync", ex)
            If IsDisposed Then Return
            panel.ClearData()
            ShowEmpty(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("ExtraseForm.LoadAsync", ex)
            If IsDisposed Then Return
            panel.ClearData()
            ShowEmpty("Extrasele nu au putut fi încărcate. Detalii în jurnalul de erori.")
        Finally
            _loading = False
            If Not IsDisposed Then SetBusy(False)
        End Try
    End Sub

    ' The real download button: the shell's download + import, then reload when anything came in.
    Private Async Sub BtnDescarca_Click(sender As Object, e As EventArgs) Handles btnDescarca.Click
        Try
            SetBusy(True)
            SetStatus("Se descarcă extrasele din FOREXE…")
            Dim importat As Boolean
            Try
                importat = Await _descarca(Me).ConfigureAwait(True)
            Finally
                If Not IsDisposed Then SetBusy(False)
            End Try
            If IsDisposed Then Return
            SetStatus(String.Empty)
            If importat Then LoadAsync()
        Catch ex As Exception
            ' UI boundary (async Sub): the shell already told the operator what failed.
            GlobalErrorLog.Write("ExtraseForm.BtnDescarca_Click", ex)
        End Try
    End Sub

    Private Sub BtnInchide_Click(sender As Object, e As EventArgs) Handles btnInchide.Click
        Close()
    End Sub

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        Try
            MyBase.OnKeyDown(e)
            If e.KeyCode = Keys.Escape Then Close()
        Catch ex As Exception
            GlobalErrorLog.Write("ExtraseForm.OnKeyDown", ex)
        End Try
    End Sub

    Private Sub SetBusy(busy As Boolean)
        btnDescarca.Enabled = Not busy
        UseWaitCursor = busy
    End Sub

    Private Sub SetStatus(text As String)
        lblStare.Text = If(text, String.Empty)
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

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim scheme As ThemeScheme = ThemeManager.Current
            Dim p As ThemePalette = scheme.Palette
            ' The form background IS the 1px outline of the window (Padding(1)).
            BackColor = p.BorderColor
            tlyMain.BackColor = p.SurfaceAltColor
            pnlCard.BackColor = p.SurfaceAltColor
            tlySubsol.BackColor = p.SurfaceAltColor
            lblEmpty.ForeColor = p.TextDimColor
            lblEmpty.BackColor = p.SurfaceAltColor
            lblStare.ForeColor = p.TextDimColor
            ButtonStyles.ApplyPrimary(btnDescarca, scheme)
            ButtonStyles.ApplySecondary(btnInchide, scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("ExtraseForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
