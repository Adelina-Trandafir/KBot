Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

' Slice 00EF-08 -- the «Cumpărător» tab: the invoice's customer. The combo lists the unit's customers; the fields below it
' show the chosen one and edit it. A customer is saved on its own («Salvează clientul», straight to the database) and the
' invoice only keeps its id, as in Access; an invoice cannot be saved while the customer's fields hold unsaved changes.
Partial Public Class FacturiForm

    ''' <summary>One customer in the combo: «name — fiscal code».</summary>
    Private NotInheritable Class ClientChoice
        Public ReadOnly Client As EFacturaClient

        Public Sub New(k_client As EFacturaClient)
            Client = k_client
        End Sub

        Public Overrides Function ToString() As String
            Dim k_code As String = (Client.IndFiscal & Client.CodFiscal).Trim()
            Return If(k_code.Length = 0, Client.DenumireClient, $"{Client.DenumireClient} — {k_code}")
        End Function
    End Class

    Private _clients As New List(Of EFacturaClient)()
    ' The customer the fields show: 0 = a new one, not saved yet.
    Private _clientId As Integer
    Private _clientDirty As Boolean
    ' True from «Client nou» until another customer is shown: only then the customer fields can be typed in.
    Private _clientEditing As Boolean
    ' The tax code ANAF was last asked about (digits), so Tab + Enter on the same code ask once.
    Private _anafAsked As String = String.Empty

    Private Sub InitCounties()
        CountyList.Fill(cmbClientJud)
    End Sub

    Private Sub InitSectors()
        cmbClientSector.Items.Clear()
        cmbClientSector.Items.Add(String.Empty)
        For k_n As Integer = 1 To 6
            cmbClientSector.Items.Add("SECTOR" & k_n.ToString(Globalization.CultureInfo.InvariantCulture))
        Next
    End Sub

    ' ── Loading and showing ─────────────────────────────────────────────────────

    Private Async Function LoadClientsAsync() As Task
        Dim k_list As List(Of EFacturaClient) = Await _gate.RunAsync(
            Function() _api.GetClientiAsync(Nothing, _cts.Token)).ConfigureAwait(True)
        If IsDisposed Then Return
        _clients = k_list
        FillClientCombo()
    End Function

    Private Sub FillClientCombo()
        Dim k_was As Boolean = _loading
        _loading = True
        Try
            cmbClient.BeginUpdate()
            Try
                cmbClient.Items.Clear()
                For Each k_c As EFacturaClient In _clients
                    cmbClient.Items.Add(New ClientChoice(k_c))
                Next
            Finally
                cmbClient.EndUpdate()
            End Try
            SelectClientInCombo(_clientId)
        Finally
            _loading = k_was
        End Try
    End Sub

    Private Sub SelectClientInCombo(k_id As Integer)
        If k_id <= 0 Then
            cmbClient.SelectedIndex = -1
            cmbClient.Text = String.Empty
            Return
        End If
        For k_i As Integer = 0 To cmbClient.Items.Count - 1
            Dim k_choice As ClientChoice = TryCast(cmbClient.Items(k_i), ClientChoice)
            If k_choice IsNot Nothing AndAlso k_choice.Client.IdClient = k_id Then
                cmbClient.SelectedIndex = k_i
                Return
            End If
        Next
        cmbClient.SelectedIndex = -1
    End Sub

    ''' <summary>Puts a customer in the fields (Nothing = empty, no customer chosen).</summary>
    Private Sub ShowClient(k_client As EFacturaClient)
        Dim k_was As Boolean = _loading
        _loading = True
        Try
            Dim k_c As EFacturaClient = If(k_client, New EFacturaClient())
            _clientId = k_c.IdClient
            If k_c.IdClient > 0 AndAlso Not _clients.Any(Function(x) x.IdClient = k_c.IdClient) Then
                ' A customer the list did not carry (the server caps a list): add it so the combo can show it.
                _clients.Add(k_c)
                cmbClient.Items.Add(New ClientChoice(k_c))
            End If
            SelectClientInCombo(_clientId)
            chkCnp.Checked = k_c.Cnp
            txtClientInd.Text = k_c.IndFiscal
            txtClientCf.Text = k_c.CodFiscal
            txtClientDen.Text = k_c.DenumireClient
            CountyList.Choose(cmbClientJud, k_c.Judetul)
            SyncSector()
            txtClientOras.Text = k_c.Orasul
            Dim k_sector As Integer = cmbClientSector.FindStringExact(k_c.Sector)
            If cmbClientSector.Visible Then cmbClientSector.SelectedIndex = k_sector
            txtClientAdresa.Text = k_c.Adresa
            txtClientCont.Text = k_c.Cont
            txtClientBanca.Text = k_c.Banca
            _clientDirty = False
            _clientEditing = False
            _anafAsked = String.Empty
            ApplyClientMode(_mode <> EditMode.Viewing)
        Finally
            _loading = k_was
        End Try
    End Sub

    ''' <summary>The customer fields as an object (the id is the one shown; 0 = not saved yet).</summary>
    Private Function ReadClient() As EFacturaClient
        Return New EFacturaClient() With {
            .IdClient = _clientId,
            .DenumireClient = txtClientDen.Text.Trim(),
            .CodFiscal = txtClientCf.Text.Replace(" ", String.Empty).Trim(),
            .IndFiscal = txtClientInd.Text.Replace(" ", String.Empty).Trim().ToUpperInvariant(),
            .Cont = txtClientCont.Text.Replace(" ", String.Empty).Trim().ToUpperInvariant(),
            .Banca = txtClientBanca.Text.Trim(),
            .Adresa = txtClientAdresa.Text.Trim(),
            .Judetul = CountyList.CodeOf(cmbClientJud),
            .Orasul = txtClientOras.Text.Trim(),
            .Sector = If(IsBucharest(), cmbClientSector.Text.Trim(), String.Empty),
            .Cnp = chkCnp.Checked}
    End Function

    Private Function IsBucharest() As Boolean
        Return String.Equals(CountyList.CodeOf(cmbClientJud), "B", StringComparison.OrdinalIgnoreCase)
    End Function

    ''' <summary>The sector exists only for Bucharest: shown then, hidden and emptied for any other county.</summary>
    Private Sub SyncSector()
        Dim k_bucharest As Boolean = IsBucharest()
        pgCumparator.lblSectorT.Visible = k_bucharest
        cmbClientSector.Visible = k_bucharest
        If Not k_bucharest Then
            cmbClientSector.SelectedIndex = -1
            cmbClientSector.Text = String.Empty
        End If
    End Sub

    Private Sub CmbClientJud_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbClientJud.SelectedIndexChanged
        Try
            SyncSector()
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.CmbClientJud_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Sub ClientCleanState()
        _clientDirty = False
    End Sub

    ''' <summary>The customer controls follow the invoice: the combo and «Client nou» work while the invoice is edited; the fields
    ''' only after «Client nou».</summary>
    Private Sub ApplyClientMode(k_edit As Boolean)
        cmbClient.Enabled = k_edit
        btnClientNou.Enabled = k_edit
        For Each k_c As Control In New Control() {chkCnp, txtClientInd, txtClientCf, txtClientDen, cmbClientJud, txtClientOras,
                                    cmbClientSector, txtClientAdresa, txtClientCont, txtClientBanca}
            k_c.Enabled = k_edit AndAlso _clientEditing
        Next
        btnClientSalveaza.Enabled = k_edit AndAlso _clientEditing AndAlso Not _busy
        btnClientSterge.Enabled = k_edit AndAlso Not _busy AndAlso _clientId > 0
    End Sub

    ' ── Events ──────────────────────────────────────────────────────────────────

    Private Sub CmbClient_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbClient.SelectedIndexChanged
        Try
            If _loading Then Return
            Dim k_choice As ClientChoice = If(cmbClient.SelectedIndex >= 0, TryCast(cmbClient.Items(cmbClient.SelectedIndex), ClientChoice), Nothing)
            If k_choice Is Nothing Then Return
            ShowClient(k_choice.Client)
            SetDirty(True)
            ApplyClientMode(_mode <> EditMode.Viewing)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.CmbClient_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Sub ClientField_Changed(sender As Object, e As EventArgs) _
        Handles chkCnp.CheckedChanged, txtClientInd.TextChanged, txtClientCf.TextChanged, txtClientDen.TextChanged,
                cmbClientJud.SelectedIndexChanged, txtClientOras.TextChanged, cmbClientSector.SelectedIndexChanged,
                txtClientAdresa.TextChanged, txtClientCont.TextChanged, txtClientBanca.TextChanged
        Try
            If _loading OrElse _mode = EditMode.Viewing Then Return
            _clientDirty = True
            SetDirty(True)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.ClientField_Changed", ex)
        End Try
    End Sub

    Private Sub BtnClientNou_Click(sender As Object, e As EventArgs) Handles btnClientNou.Click
        Try
            If _busy OrElse _mode = EditMode.Viewing Then Return
            ShowClient(Nothing)
            _clientEditing = True
            SetDirty(True)
            ApplyClientMode(True)
            txtClientCf.Focus()
            SetStatus("Client nou: scrieți codul fiscal (datele se iau de la ANAF), completați restul și apăsați «Salvează clientul».")
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.BtnClientNou_Click", ex)
        End Try
    End Sub

    ' ── ANAF lookup of a typed tax code ─────────────────────────────────────────

    Private Sub TxtClientCf_Leave(sender As Object, e As EventArgs) Handles txtClientCf.Leave
        TakeClientFromAnafSafe()
    End Sub

    Private Sub TxtClientCf_FieldKeyDown(sender As Object, e As KeyEventArgs) Handles txtClientCf.FieldKeyDown
        If e.KeyCode = Keys.Enter Then TakeClientFromAnafSafe()
    End Sub

    ' UI boundary (async void): TakeClientFromAnafAsync shows its own failures.
    Private Async Sub TakeClientFromAnafSafe()
        Try
            Await TakeClientFromAnafAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.TakeClientFromAnafSafe", ex)
        End Try
    End Sub

    ''' <summary>Asks ANAF about the tax code typed for a new customer and fills name, VAT prefix, county, city and address.
    ''' Nothing is saved: the operator reads the fields and presses «Salvează clientul». A failure is a message; the typing goes on.</summary>
    Private Async Function TakeClientFromAnafAsync() As Task
        Try
            If _busy OrElse _loading OrElse Not _clientEditing OrElse _mode = EditMode.Viewing Then Return
            Dim k_typed As String = txtClientCf.Text.Replace(" ", String.Empty).Trim().ToUpperInvariant()
            If chkCnp.Checked OrElse k_typed.Length = 0 Then Return
            Dim k_digits As String = If(k_typed.StartsWith("RO", StringComparison.Ordinal), k_typed.Substring(2), k_typed)
            If k_digits.Length < 2 OrElse k_digits.Length > 10 OrElse Not k_digits.All(AddressOf Char.IsDigit) Then Return
            If String.Equals(k_digits, _anafAsked, StringComparison.Ordinal) Then Return
            _anafAsked = k_digits
            SetBusy(True, "Se caută clientul la ANAF…")
            Try
                Dim k_found As EFacturaClient = Await _gate.RunAsync(
                    Function() _api.TakeClientFromAnafAsync(k_digits, _cts.Token)).ConfigureAwait(True)
                If IsDisposed OrElse Not _clientEditing Then Return
                Dim k_was As Boolean = _loading
                _loading = True
                Try
                    txtClientCf.Text = k_found.CodFiscal
                    txtClientInd.Text = k_found.IndFiscal
                    txtClientDen.Text = k_found.DenumireClient
                    CountyList.Choose(cmbClientJud, k_found.Judetul)
                    SyncSector()
                    txtClientOras.Text = k_found.Orasul
                    If cmbClientSector.Visible Then cmbClientSector.SelectedIndex = cmbClientSector.FindStringExact(k_found.Sector)
                    txtClientAdresa.Text = k_found.Adresa
                Finally
                    _loading = k_was
                End Try
                _clientDirty = True
                SetDirty(True)
                SetStatus($"Datele clientului «{k_found.DenumireClient}» au fost luate de la ANAF. Verificați-le și apăsați «Salvează clientul».")
            Finally
                If Not IsDisposed Then SetBusy(False, Nothing)
            End Try
        Catch ex As OperationCanceledException
            ' The window was closed: nothing to show.
        Catch ex As ApiException
            If Not IsDisposed Then KBotMessage.Show(Me, ex.Message, "Preia de la ANAF", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.TakeClientFromAnafAsync", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Datele clientului nu au putut fi luate de la ANAF. Detalii în jurnalul de erori.", "Preia de la ANAF",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End Try
    End Function

    Private Async Sub BtnClientSalveaza_Click(sender As Object, e As EventArgs) Handles btnClientSalveaza.Click
        Try
            If _busy OrElse _mode = EditMode.Viewing Then Return
            Dim k_client As EFacturaClient = ReadClient()
            If k_client.DenumireClient.Length = 0 Then
                KBotMessage.Show(Me, "Denumirea clientului este obligatorie.", "Salvare client", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtClientDen.Focus()
                Return
            End If
            SetBusy(True, "Se salvează clientul…")
            Try
                Dim k_saved As EFacturaClient = Await _gate.RunAsync(
                    Function() _api.SaveClientAsync(k_client, _cts.Token)).ConfigureAwait(True)
                If IsDisposed Then Return
                Dim k_old As EFacturaClient = _clients.FirstOrDefault(Function(x) x.IdClient = k_saved.IdClient)
                If k_old IsNot Nothing Then _clients.Remove(k_old)
                _clients.Add(k_saved)
                _clients = _clients.OrderBy(Function(x) x.DenumireClient, StringComparer.CurrentCultureIgnoreCase).ToList()
                _clientId = k_saved.IdClient
                FillClientCombo()
                ShowClient(k_saved)
                SetDirty(True)
                SetStatus($"Clientul «{k_saved.DenumireClient}» a fost salvat și ales pentru factură.")
            Finally
                If Not IsDisposed Then SetBusy(False, Nothing)
            End Try
        Catch ex As OperationCanceledException
            ' The window was closed: nothing to show.
        Catch ex As ApiException
            GlobalErrorLog.Write("FacturiForm.BtnClientSalveaza_Click", ex)
            If Not IsDisposed Then KBotMessage.Show(Me, ex.Message, "Salvare client", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.BtnClientSalveaza_Click", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Clientul nu a putut fi salvat. Detalii în jurnalul de erori.", "Salvare client",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End Try
    End Sub

    Private Async Sub BtnClientSterge_Click(sender As Object, e As EventArgs) Handles btnClientSterge.Click
        Try
            If _busy OrElse _mode = EditMode.Viewing OrElse _clientId <= 0 Then Return
            If KBotMessage.Show(Me, $"Ștergeți clientul «{txtClientDen.Text.Trim()}»?" & vbLf &
                                "Un client care are facturi nu poate fi șters.", "Ștergere client",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then Return
            Dim k_id As Integer = _clientId
            SetBusy(True, "Se șterge clientul…")
            Try
                Await _gate.RunAsync(Function() _api.DeleteClientAsync(k_id, _cts.Token)).ConfigureAwait(True)
                If IsDisposed Then Return
                _clients.RemoveAll(Function(x) x.IdClient = k_id)
                ShowClient(Nothing)
                FillClientCombo()
                SetDirty(True)
                SetStatus("Clientul a fost șters.")
            Finally
                If Not IsDisposed Then SetBusy(False, Nothing)
            End Try
        Catch ex As OperationCanceledException
            ' The window was closed: nothing to show.
        Catch ex As ApiException
            GlobalErrorLog.Write("FacturiForm.BtnClientSterge_Click", ex)
            If Not IsDisposed Then KBotMessage.Show(Me, ex.Message, "Ștergere client", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.BtnClientSterge_Click", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Clientul nu a putut fi șters. Detalii în jurnalul de erori.", "Ștergere client",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End Try
    End Sub

End Class
