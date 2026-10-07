Option Strict On
Imports System.Globalization
Imports System.Threading
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Slice 00EF-13 -- «Date Unitate»: the unit as the ISSUER of the E-Factura invoices (name, tax code, county, city, address, phone, e-mail,
''' series and first number of the invoices). It used to be the «Vânzător» view of <see cref="FacturiForm"/>; it is a window of its own now,
''' opened as a dialog from the button «Date unitate». The row is one per unit, kept in AVACONT_COMUN.Unitati_Detalii.
'''
''' <para>The series and the first number are written once: as soon as the unit has an issued invoice the server says so
''' (<see cref="EFacturaFurnizor.AreFacturi"/>) and they are shown read only (the server refuses a change anyway). The button «Preia de la
''' ANAF» takes the name and the address from ANAF, by the tax code the unit has in the list of units; it works ONCE
''' (<see cref="EFacturaFurnizor.AnafPreluat"/>). The unit's bank accounts have their own window, «Conturi Unitate», next to this one in the
''' menu of the invoice window's title bar.</para>
''' </summary>
Public Class DateUnitateForm

    Private ReadOnly _api As IEFacturaApi
    Private ReadOnly _gate As ReauthGate
    Private ReadOnly _cts As New CancellationTokenSource()
    Private _furnizor As EFacturaFurnizor
    Private _dirty As Boolean
    Private _working As Boolean
    Private _loading As Boolean

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _gate = ReauthGate.Direct
    End Sub

    ''' <param name="k_api">The issuer's routes; the invoice window passes the one it has.</param>
    ''' <param name="k_gate">The shell's re-login net.</param>
    ''' <param name="k_furnizor">The issuer as the invoice window last read it; Nothing = the unit has not filled it in yet.</param>
    Public Sub New(k_api As IEFacturaApi, k_gate As ReauthGate, k_furnizor As EFacturaFurnizor)
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_gate)
        InitializeComponent()
        _api = k_api
        _gate = k_gate
        _furnizor = k_furnizor
    End Sub

    ''' <summary>The issuer as the server last answered (after a save or the ANAF button); Nothing = still not filled in.</summary>
    <System.ComponentModel.Browsable(False)>
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property Furnizor As EFacturaFurnizor
        Get
            Return _furnizor
        End Get
    End Property

    Private Sub DateUnitateForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            If _api Is Nothing Then Return
            CountyList.Fill(cmbJud)
            ShowFurnizor(_furnizor)
            If _furnizor Is Nothing Then
                ntfMesaj.Show("Datele unității nu sunt completate. Completați denumirea și codul fiscal (sau apăsați «Preia de la ANAF»), apoi «Salvează»; " &
                              "fără ele nu se pot face facturi.", NoticeKind.Warning)
            End If
        Catch ex As Exception
            ' UI boundary (Load): log and swallow.
            GlobalErrorLog.Write("DateUnitateForm.DateUnitateForm_Load", ex)
        End Try
    End Sub

    Private Sub DateUnitateForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            If _dirty AndAlso Not _working Then
                Dim k_answer As DialogResult = KBotMessage.Show(Me, "Datele unității au modificări nesalvate. Le închideți fără să le salvați?", "Date Unitate",
                                                                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)
                If k_answer <> DialogResult.Yes Then e.Cancel = True
            End If
        Catch ex As Exception
            ' UI boundary (event handler): log and swallow.
            GlobalErrorLog.Write("DateUnitateForm.DateUnitateForm_FormClosing", ex)
        End Try
    End Sub

    Private Sub DateUnitateForm_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        Try
            _cts.Cancel()
            _cts.Dispose()
        Catch ex As Exception
            ' UI boundary (event handler): log and swallow.
            GlobalErrorLog.Write("DateUnitateForm.DateUnitateForm_FormClosed", ex)
        End Try
    End Sub

    ' ── Showing ─────────────────────────────────────────────────────────────────

    Private Sub ShowFurnizor(k_furnizor As EFacturaFurnizor)
        Dim k_was As Boolean = _loading
        _loading = True
        Try
            Dim k_f As EFacturaFurnizor = If(k_furnizor, New EFacturaFurnizor())
            txtDen.Text = k_f.Denumire
            txtCf.Text = k_f.CodFiscal
            CountyList.Choose(cmbJud, k_f.Judetul)
            txtOras.Text = k_f.Orasul
            txtAdresa.Text = k_f.Adresa
            txtTel.Text = k_f.Telefon
            txtMail.Text = k_f.Mail
            txtSerie.Text = k_f.SerieFactura
            txtNumar.Text = k_f.NumarInitial.ToString(CultureInfo.InvariantCulture)
            _dirty = False
        Finally
            _loading = k_was
        End Try
        ApplyState()
    End Sub

    Private Sub ApplyState()
        Dim k_locked As Boolean = _furnizor IsNot Nothing AndAlso _furnizor.AreFacturi
        txtSerie.ReadOnly = k_locked OrElse _working
        txtNumar.ReadOnly = k_locked OrElse _working
        btnSalveaza.Enabled = Not _working
        btnAnaf.Enabled = Not _working AndAlso Not (_furnizor IsNot Nothing AndAlso _furnizor.AnafPreluat)
    End Sub

    Private Sub SetBusy(k_on As Boolean)
        _working = k_on
        busy.Running = k_on
        UseWaitCursor = k_on
        ApplyState()
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            lblNota.ForeColor = ThemeManager.Current.Palette.WarningColor
        Catch ex As Exception
            GlobalErrorLog.Write("DateUnitateForm.OnThemeChanged", ex)
        End Try
    End Sub

    ' ── Editing ─────────────────────────────────────────────────────────────────

    Private Sub Field_Changed(sender As Object, e As EventArgs) _
        Handles txtDen.TextChanged, txtCf.TextChanged, cmbJud.SelectedIndexChanged, txtOras.TextChanged, txtAdresa.TextChanged,
                txtTel.TextChanged, txtMail.TextChanged, txtSerie.TextChanged, txtNumar.TextChanged
        Try
            If _loading Then Return
            _dirty = True
        Catch ex As Exception
            GlobalErrorLog.Write("DateUnitateForm.Field_Changed", ex)
        End Try
    End Sub

    Private Sub BtnIesire_Click(sender As Object, e As EventArgs) Handles btnIesire.Click
        Try
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("DateUnitateForm.BtnIesire_Click", ex)
        End Try
    End Sub

    ' ── ANAF ────────────────────────────────────────────────────────────────────

    ' UI boundary (async void): every failure is shown to the operator here.
    Private Async Sub BtnAnaf_Click(sender As Object, e As EventArgs) Handles btnAnaf.Click
        Try
            If _working Then Return
            Dim k_question As String = "Denumirea și adresa unității se iau de la ANAF, după codul fiscal din lista unităților, și se scriu imediat." & vbLf &
                                       "Se poate face o singură dată. Ce ați scris în fereastră și nu ați salvat se pierde." & vbLf & "Continuați?"
            If KBotMessage.Show(Me, k_question, "Preia de la ANAF", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then Return
            SetBusy(True)
            ntfMesaj.Clear()
            Dim k_taken As EFacturaFurnizor = Await _gate.RunAsync(
                Function() _api.TakeFurnizorFromAnafAsync(_cts.Token)).ConfigureAwait(True)
            If IsDisposed Then Return
            _furnizor = k_taken
            ShowFurnizor(k_taken)
            ntfMesaj.Show("Denumirea și adresa au fost preluate de la ANAF. Completați restul datelor și apăsați «Salvează».", NoticeKind.Success)
        Catch ex As OperationCanceledException
            ' The window was closed while the answer was on its way: nothing to show.
        Catch ex As ApiException
            If Not IsDisposed Then
                ' A second try (the first one was made from another place) closes the button too.
                If String.Equals(ex.Reason, "ANAF_DEJA_PRELUAT", StringComparison.Ordinal) AndAlso _furnizor IsNot Nothing Then _furnizor.AnafPreluat = True
                KBotMessage.Show(Me, ex.Message, "Preia de la ANAF", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("DateUnitateForm.BtnAnaf_Click", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Datele nu au putut fi preluate de la ANAF. Detalii în jurnalul de erori.", "Preia de la ANAF",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        Finally
            If Not IsDisposed Then SetBusy(False)
        End Try
    End Sub

    ' ── Saving ──────────────────────────────────────────────────────────────────

    ' UI boundary (async void): every failure is shown to the operator here.
    Private Async Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            If _working Then Return
            Dim k_number As Integer
            If txtDen.Text.Trim().Length = 0 Then
                Problem("Denumirea unității este obligatorie.", txtDen)
                Return
            End If
            If txtCf.Text.Trim().Length = 0 Then
                Problem("Codul fiscal al unității este obligatoriu.", txtCf)
                Return
            End If
            If txtTel.Text.Length > 0 AndAlso Not New KBotInputMask(txtTel.InputMask).IsComplete(txtTel.Text) Then
                Problem("Telefonul trebuie să aibă zece cifre sau să rămână gol.", txtTel)
                Return
            End If
            If Not Integer.TryParse(txtNumar.Text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, k_number) OrElse k_number < 1 Then
                Problem("Primul număr de factură trebuie să fie un număr întreg, cel puțin 1.", txtNumar)
                Return
            End If
            Dim k_request As New EFacturaFurnizor() With {
                .Denumire = txtDen.Text.Trim(),
                .CodFiscal = txtCf.Text.Replace(" ", String.Empty).Trim().ToUpperInvariant(),
                .Adresa = txtAdresa.Text.Trim(),
                .Orasul = txtOras.Text.Trim(),
                .Judetul = CountyList.CodeOf(cmbJud),
                .Mail = txtMail.Text.Trim(),
                .Telefon = txtTel.Text.Trim(),
                .SerieFactura = txtSerie.Text.Replace(" ", String.Empty).Trim().ToUpperInvariant(),
                .NumarInitial = k_number,
                .AfiseazaPrimiteNoi = If(_furnizor IsNot Nothing, _furnizor.AfiseazaPrimiteNoi, False)}
            SetBusy(True)
            ntfMesaj.Clear()
            Dim k_saved As EFacturaFurnizor = Await _gate.RunAsync(
                Function() _api.SaveFurnizorAsync(k_request, _cts.Token)).ConfigureAwait(True)
            If IsDisposed Then Return
            _furnizor = k_saved
            ShowFurnizor(k_saved)
            ntfMesaj.Show("Datele unității au fost salvate.", NoticeKind.Success)
        Catch ex As OperationCanceledException
            ' The window was closed during the save: nothing to show.
        Catch ex As ApiException
            If Not IsDisposed Then KBotMessage.Show(Me, ex.Message, "Date Unitate", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("DateUnitateForm.BtnSalveaza_Click", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Datele unității nu au putut fi salvate. Detalii în jurnalul de erori.", "Date Unitate",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        Finally
            If Not IsDisposed Then SetBusy(False)
        End Try
    End Sub

    Private Sub Problem(k_text As String, k_field As Control)
        KBotMessage.Show(Me, k_text, "Date Unitate", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        k_field.Focus()
    End Sub

End Class
