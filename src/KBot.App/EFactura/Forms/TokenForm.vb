Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Threading
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Slice 00EF-05 -- «E-Factura — tokenul ANAF»: the first window of the E-Factura menu entry. It shows whether the unit
''' has an ANAF token and until when it works, warns 7 days before the refresh token stops (the server computes the
''' date), and runs the certificate step (<see cref="TokenAuthorizer"/>) from «Autorizează» / «Reînnoiește tokenul».
'''
''' <para>The window never sees a token: the server keeps both tokens and answers only dates and labels (slice 00EF-04).
''' It is modeless; the shell keeps one at a time.</para>
''' </summary>
Public Class TokenForm

    Private ReadOnly _authorizer As TokenAuthorizer
    Private ReadOnly _unitName As String
    Private ReadOnly _cts As New CancellationTokenSource()
    Private _state As EFacturaTokenState
    Private _working As Boolean

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _unitName = String.Empty
    End Sub

    ''' <param name="k_authorizer">The steps against the server; built by the shell with its API client and re-login net.</param>
    ''' <param name="k_unitName">The open unit's name, shown for orientation.</param>
    Public Sub New(k_authorizer As TokenAuthorizer, k_unitName As String)
        ArgumentNullException.ThrowIfNull(k_authorizer)
        InitializeComponent()
        _authorizer = k_authorizer
        _unitName = If(k_unitName, String.Empty)
    End Sub

    Private Sub TokenForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            If _authorizer Is Nothing Then Return
            lblUnitate.Text = If(_unitName.Length = 0, String.Empty, "Unitate: " & _unitName)
            LoadState()
        Catch ex As Exception
            ' UI boundary (Load): log and swallow.
            GlobalErrorLog.Write("TokenForm.TokenForm_Load", ex)
        End Try
    End Sub

    Private Sub TokenForm_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        Try
            _cts.Cancel()
            _cts.Dispose()
        Catch ex As Exception
            ' UI boundary (event handler): log and swallow.
            GlobalErrorLog.Write("TokenForm.TokenForm_FormClosed", ex)
        End Try
    End Sub

    ' ── State ───────────────────────────────────────────────────────────────────

    ' UI boundary: logs and shows the error; started without await.
    Private Async Sub LoadState()
        Try
            SetBusy(True, "Se citește starea tokenului…")
            Dim k_state As EFacturaTokenState = Await _authorizer.GetStateAsync(_cts.Token).ConfigureAwait(True)
            If IsDisposed Then Return
            ShowState(k_state)
        Catch ex As OperationCanceledException
            ' The window was closed while the answer was on its way: nothing to show.
        Catch ex As ApiException
            If Not IsDisposed Then ShowFailure(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("TokenForm.LoadState", ex)
            If Not IsDisposed Then ShowFailure("Starea tokenului nu a putut fi citită. Detalii în jurnalul de erori.")
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
    End Sub

    Private Sub ShowFailure(k_text As String)
        lblStare.Text = "Starea tokenului nu se poate citi."
        ntfMesaj.Show(k_text, NoticeKind.Error)
        ClearDetails()
    End Sub

    Private Sub ClearDetails()
        lblCui.Text = String.Empty
        lblCertificat.Text = String.Empty
        lblAutorizat.Text = String.Empty
        lblEroare.Text = String.Empty
    End Sub

    Private Sub ShowState(k_state As EFacturaTokenState)
        _state = k_state
        ntfMesaj.Clear()

        If Not k_state.Configured Then
            lblStare.Text = "Serverul nu este pregătit pentru E-Factura."
            ntfMesaj.Show("Datele aplicației ANAF nu sunt puse încă pe server. Contactați administratorul K-BOT.", NoticeKind.Error)
        ElseIf Not k_state.Exists Then
            lblStare.Text = "Unitatea nu are încă un token ANAF. Apăsați «Autorizează» și alegeți certificatul calificat."
            btnAutorizeaza.Text = "Autorizează"
        ElseIf k_state.MustRenew Then
            lblStare.Text = "Tokenul ANAF al unității a expirat sau ANAF nu l-a mai primit. Până îl reînnoiți, nu se pot face apeluri către ANAF."
            ntfMesaj.Show("Reînnoiți tokenul cu certificatul calificat.", NoticeKind.Warning)
            btnAutorizeaza.Text = "Reînnoiește tokenul"
        Else
            Dim k_until As String = DateText(k_state.ValidUntilUtc, "dd.MM.yyyy")
            Dim k_days As Integer = If(k_state.DaysLeft, 0)
            lblStare.Text = $"Tokenul ANAF este valabil până la {k_until} ({k_days} zile)."
            If k_state.ShouldWarn Then
                ntfMesaj.Show($"Tokenul expiră în {k_days} zile. Reînnoiți-l din timp, cu certificatul calificat.", NoticeKind.Warning)
            End If
            btnAutorizeaza.Text = "Reînnoiește tokenul"
        End If

        lblCui.Text = If(k_state.Cui.Length = 0, String.Empty, "Cod fiscal (CUI): " & k_state.Cui)
        lblCertificat.Text = If(k_state.CertificateLabel.Length = 0, String.Empty, "Certificat folosit: " & k_state.CertificateLabel)
        lblAutorizat.Text = If(k_state.AuthorizedBy.Length = 0, String.Empty,
                               "Autorizat de " & k_state.AuthorizedBy & " la " & DateText(k_state.AuthorizedAtUtc, "dd.MM.yyyy HH:mm"))
        lblEroare.Text = If(k_state.LastError.Length = 0, String.Empty,
                            "Ultima eroare: " & k_state.LastError & " (" & DateText(k_state.LastErrorAtUtc, "dd.MM.yyyy HH:mm") & ")")
        ApplyButtons()
    End Sub

    ' UTC from the server, shown in the PC's time.
    Private Shared Function DateText(k_moment As Date?, k_format As String) As String
        If Not k_moment.HasValue Then Return "—"
        Return k_moment.Value.ToLocalTime().ToString(k_format, CultureInfo.InvariantCulture)
    End Function

    Private Sub SetBusy(k_on As Boolean, k_text As String)
        _working = k_on
        busy.Running = k_on
        If k_text IsNot Nothing Then lblStare.Text = k_text
        ApplyButtons()
    End Sub

    Private Sub ApplyButtons()
        btnReimprospateaza.Enabled = Not _working
        btnAutorizeaza.Enabled = Not _working AndAlso _state IsNot Nothing AndAlso _state.Configured
    End Sub

    ' ── Buttons ─────────────────────────────────────────────────────────────────

    Private Sub btnReimprospateaza_Click(sender As Object, e As EventArgs) Handles btnReimprospateaza.Click
        Try
            If Not _working Then LoadState()
        Catch ex As Exception
            ' UI boundary (handler): log and swallow.
            GlobalErrorLog.Write("TokenForm.btnReimprospateaza_Click", ex)
        End Try
    End Sub

    Private Sub btnInchide_Click(sender As Object, e As EventArgs) Handles btnInchide.Click
        Try
            Close()
        Catch ex As Exception
            ' UI boundary (handler): log and swallow.
            GlobalErrorLog.Write("TokenForm.btnInchide_Click", ex)
        End Try
    End Sub

    ' UI boundary (async void): every failure is shown to the operator here.
    Private Async Sub btnAutorizeaza_Click(sender As Object, e As EventArgs) Handles btnAutorizeaza.Click
        Dim k_reload As Boolean = False
        Try
            If _working Then Return
            SetBusy(True, "Autorizare ANAF…")
            ntfMesaj.Clear()
            Dim k_new As EFacturaTokenState = Await _authorizer.AuthorizeAsync(
                AddressOf PickCertificate, AddressOf ShowProgress, _cts.Token).ConfigureAwait(True)
            If IsDisposed Then Return
            If k_new Is Nothing Then
                k_reload = True                     ' the operator gave up at the certificate list: back to what was shown
            Else
                ShowState(k_new)
                ntfMesaj.Show("Tokenul ANAF a fost obținut și este păstrat pe server.", NoticeKind.Success)
            End If
        Catch ex As OperationCanceledException
            ' The window was closed during the step: nothing to show.
        Catch ex As ApiException
            If Not IsDisposed Then
                KBotMessage.Show(Me, ex.Message, "E-Factura", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                k_reload = True                     ' the server may have noted the failure: show its state
            End If
        Catch ex As EFacturaException
            If Not IsDisposed Then
                KBotMessage.Show(Me, ex.Message, "E-Factura", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                k_reload = True
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("TokenForm.btnAutorizeaza_Click", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Autorizarea nu a reușit. Detalii în jurnalul de erori.", "E-Factura",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error)
                k_reload = True
            End If
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
        If k_reload AndAlso Not IsDisposed Then LoadState()
    End Sub

    Private Function PickCertificate(k_certs As IReadOnlyList(Of AnafCertificate)) As AnafCertificate
        Using k_dialog As New CertificatePickerForm(k_certs)
            If k_dialog.ShowDialog(Me) = DialogResult.OK Then Return k_dialog.Selected
        End Using
        Return Nothing
    End Function

    Private Sub ShowProgress(k_text As String)
        lblStare.Text = k_text
    End Sub

End Class
