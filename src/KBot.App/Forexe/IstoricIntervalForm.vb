Option Strict On
Imports System.Globalization
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' The history of ONE angajament, cut to the minutes the operator spent on it in the FOREXE
''' browser (slice 0073). Opened by the shell right after an operation the floating K-BOT
''' menu reported (a new angajament, a reservation row, a reception) was downloaded and
''' saved: it shows what FOREXE wrote in that window, nothing else, so the operator can check
''' at once that the site recorded what they meant.
'''
''' <para><b>The rows.</b> The real <see cref="IstoricView"/> - the same one the «Istoric» page
''' of the shell uses - loaded through <see cref="IstoricView.SetContextInterval"/>. The
''' interval sits in its date filter, so «TOATE» on the date menu, «Reset», or the
''' «Tot istoricul» button below widen the view to the whole history without another
''' window.</para>
'''
''' <para><b>The interval is padded</b> by <see cref="MarjaMinute"/> on each side: the
''' instants come from the operator's machine, <c>DataFX</c> from the FOREXE server, and the
''' two clocks are not the same clock. The label in the footer says exactly what was used.</para>
'''
''' <para>Modeless and owned by the shell; one window per operation, disposed on close.</para>
''' </summary>
Public Class IstoricIntervalForm

    ''' <summary>Minutes added on each side of the interval - two clocks, see the header.</summary>
    Public Const MarjaMinute As Integer = 2

    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    Private ReadOnly _view As IstoricView
    Private ReadOnly _cod As String
    Private ReadOnly _deLa As Date
    Private ReadOnly _panaLa As Date
    Private ReadOnly _eticheta As String

    ''' <param name="apiClient">The HTTP client of the shell.</param>
    ''' <param name="withReauth">The shell's re-login net, specialised on IstoricInfo.</param>
    ''' <param name="cod">Angajament code.</param>
    ''' <param name="deLa">When the operator started the operation (local clock).</param>
    ''' <param name="panaLa">When the save was confirmed (local clock).</param>
    ''' <param name="eticheta">Operator-facing name of the operation, for the caption.</param>
    Public Sub New(apiClient As IApiClient,
                   withReauth As Func(Of Func(Of Task(Of IstoricInfo)), Task(Of IstoricInfo)),
                   cod As String, deLa As Date, panaLa As Date, eticheta As String)
        ArgumentNullException.ThrowIfNull(apiClient)
        ArgumentNullException.ThrowIfNull(withReauth)
        If String.IsNullOrWhiteSpace(cod) Then
            Throw New ArgumentException("Codul angajamentului este obligatoriu.", NameOf(cod))
        End If
        InitializeComponent()
        Try
            _cod = cod.Trim()
            _deLa = deLa.AddMinutes(-MarjaMinute)
            _panaLa = panaLa.AddMinutes(MarjaMinute)
            _eticheta = If(eticheta, String.Empty)

            capBar.IconImage = My.Resources.kbot_64
            capBar.Text = If(String.IsNullOrEmpty(_eticheta),
                             $"Istoric «{_cod}»",
                             $"Istoric «{_cod}» — {_eticheta}")
            Me.Text = capBar.Text
            lblInterval.Text = $"Rândurile scrise de FOREXE între {_deLa.ToString("dd.MM.yyyy HH:mm", _roCulture)} " &
                               $"și {_panaLa.ToString("dd.MM.yyyy HH:mm", _roCulture)} " &
                               $"(intervalul lucrului în browser, cu ±{MarjaMinute} min)."

            _view = New IstoricView(apiClient, withReauth) With {.Dock = DockStyle.Fill}
            pnlContinut.Controls.Add(_view)
        Catch ex As Exception
            GlobalErrorLog.Write("IstoricIntervalForm.New", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Loads the rows. Called from Load so the view has a handle and a real size.</summary>
    Private Sub IstoricIntervalForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            _view.SetContextInterval(_cod, _deLa, _panaLa)
        Catch ex As Exception
            GlobalErrorLog.Write("IstoricIntervalForm.IstoricIntervalForm_Load", ex)
            KBotMessage.Show(Me, "Istoricul nu a putut fi încărcat: " & ex.Message, Me.Text,
                             MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ''' <summary>Drops the time limit: the whole history of the angajament, in the same window.</summary>
    Private Sub BtnTotIstoricul_Click(sender As Object, e As EventArgs) Handles btnTotIstoricul.Click
        Try
            _view.SetContext(New AngajamentTreeInfo With {.CodAngajament = _cod})
            lblInterval.Text = "Tot istoricul angajamentului (fără limită de timp)."
            btnTotIstoricul.Enabled = False
        Catch ex As Exception
            GlobalErrorLog.Write("IstoricIntervalForm.BtnTotIstoricul_Click", ex)
            KBotMessage.Show(Me, "Istoricul nu a putut fi reîncărcat: " & ex.Message, Me.Text,
                             MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub BtnInchide_Click(sender As Object, e As EventArgs) Handles btnInchide.Click
        Close()
    End Sub

    ' -- Theme -----------------------------------------------------------------

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim schema = ThemeManager.Current
            Dim p = schema.Palette

            ' The form's background IS the 1px outline of the window (see ForexeHistoryForm).
            BackColor = p.BorderColor
            lblInterval.ForeColor = p.TextDimColor

            ButtonStyles.ApplySecondary(btnTotIstoricul, schema)
            ButtonStyles.ApplyPrimary(btnInchide, schema)
        Catch ex As Exception
            ' UI boundary (theme cascade): log and swallow.
            GlobalErrorLog.Write("IstoricIntervalForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
