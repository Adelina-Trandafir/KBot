Imports System.Globalization
Imports System.IO
Imports System.Threading
Imports KBot.Common

''' <summary>
''' Slice 00EF-03: the «E-Factura» tab. Reads the unit's e-invoice data from Access and writes it to MariaDB (see
''' <see cref="EfImporter"/>). The server box, the DC and the cod fiscal come from the «Transfer» tab: this tab adds two
''' file paths and four ticks. The units of measure are NOT here: the operator writes EF_UM in AVACONT_COMUN on the server.
''' </summary>
''' <remarks>
''' Same two-step rule as «Transferă»: «Importă» is enabled only by a verification with no blocking finding, and only
''' while nothing the verification looked at has been changed since (<see cref="EfFingerprint"/>).
''' </remarks>
Partial Public Class MigratorForm

    ''' <summary>What the last CLEAN verification looked at; Nothing when there is none. «Importă» needs it to still match.</summary>
    Private _efVerified As String
    Private _efReport As EfImportReport

    ' ---- the tab and its defaults ---------------------------------------------------------------------------

    Private Sub tabPrincipal_SelectedIndexChanged(sender As Object, e As EventArgs) Handles tabPrincipal.SelectedIndexChanged
        Try
            If tabPrincipal.SelectedTab Is tabEFactura Then RefreshEfHeader(False)
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.tabPrincipal_SelectedIndexChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Shows which databases a run would write to and fills the file paths that can be known. Called when the tab is
    ''' shown and when the DC changes (<paramref name="k_dcChanged"/>, which also replaces the unit file: a path chosen
    ''' for one DC must not follow the operator to the next).
    ''' </summary>
    Private Sub RefreshEfHeader(k_dcChanged As Boolean)
        Dim dc = Convert.ToString(cboDc.SelectedItem, CultureInfo.InvariantCulture)
        Dim cui = txtCodFiscal.Text.Trim()
        If String.IsNullOrWhiteSpace(dc) Then
            lblEfBaza.Text = "Baza-țintă: (necunoscută). Citiți registrul și alegeți DC-ul în fila «Transfer»."
            Return
        End If
        lblEfBaza.Text = $"Baza unității: {dc}     ·     cod fiscal: {If(cui.Length = 0, "(lipsă)", cui)}"

        If k_dcChanged OrElse txtEfEmise.Text.Trim().Length = 0 Then
            Dim candidates = CaiRegistry.UnitsOf(_units, dc).Where(Function(u) u.HasUnitFile).Select(Function(u) u.UnitFilePath).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
            If candidates.Count > 0 Then
                txtEfEmise.Text = candidates(0)
                If candidates.Count > 1 Then
                    SayEf($"DC-ul are {candidates.Count} fișiere de unitate; s-a ales primul. Dacă facturile sunt în altul, alegeți-l cu «...».")
                End If
            ElseIf k_dcChanged Then
                txtEfEmise.Text = String.Empty
            End If
        End If

        If txtEfPrimite.Text.Trim().Length = 0 Then
            Dim registryFolder = Path.GetDirectoryName(txtRegistru.Text.Trim())
            If Not String.IsNullOrEmpty(registryFolder) Then
                Dim guess = Path.Combine(registryFolder, "EFACTURA", $"ef_{TableMaps.TransferYear}.accdb")
                If File.Exists(guess) Then txtEfPrimite.Text = guess
            End If
        End If
    End Sub

    ' ---- browsing -----------------------------------------------------------------------------------------------

    Private Sub btnEfEmise_Click(sender As Object, e As EventArgs) Handles btnEfEmise.Click
        Try
            BrowseAccess(txtEfEmise, "Alegeți fișierul unității (baza<an>.accdb)")
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.btnEfEmise_Click", ex)
        End Try
    End Sub

    Private Sub btnEfPrimite_Click(sender As Object, e As EventArgs) Handles btnEfPrimite.Click
        Try
            BrowseAccess(txtEfPrimite, "Alegeți magazinul de facturi (ef_<an>.accdb)")
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.btnEfPrimite_Click", ex)
        End Try
    End Sub

    Private Sub BrowseAccess(k_box As KBot.Controls.KBotTextField, k_title As String)
        Using dialog As New OpenFileDialog()
            dialog.Title = k_title
            dialog.Filter = "Baze Access (*.accdb)|*.accdb|Toate fișierele (*.*)|*.*"
            If File.Exists(k_box.Text.Trim()) Then dialog.FileName = k_box.Text.Trim()
            If dialog.ShowDialog(Me) = DialogResult.OK Then k_box.Text = dialog.FileName
        End Using
    End Sub

    ' ---- building the options -----------------------------------------------------------------------------------------

    Private Function BuildEfOptions() As EfImportOptions
        Dim server = BuildServer()
        If server Is Nothing Then Return Nothing

        Dim dc = Convert.ToString(cboDc.SelectedItem, CultureInfo.InvariantCulture)
        If String.IsNullOrWhiteSpace(dc) Then
            Warn("Nu a fost ales niciun DC. Citiți registrul și alegeți DC-ul în fila «Transfer».")
            Return Nothing
        End If

        Dim options As New EfImportOptions(server, dc, _settings.CommonDatabase) With {
            .AccessPassword = AccessPassword(),
            .IssuedFile = txtEfEmise.Text.Trim(),
            .ReceivedFile = txtEfPrimite.Text.Trim(),
            .UnitCui = txtCodFiscal.Text.Trim(),
            .DoFurnizor = chkEfFurnizor.Checked,
            .DoClienti = chkEfClienti.Checked,
            .DoEmise = chkEfEmise.Checked,
            .DoPrimite = chkEfPrimite.Checked
        }
        If Not options.AnythingSelected Then
            Warn("Nu a fost bifat nimic de importat.")
            Return Nothing
        End If
        Return options
    End Function

    ''' <summary>Everything a verification depends on, as text. No password.</summary>
    Private Shared Function EfFingerprint(k_options As EfImportOptions) As String
        Return String.Join("|", {
            k_options.Server.Describe(), k_options.UnitDatabase, k_options.CommonDatabase,
            k_options.IssuedFile, k_options.ReceivedFile, k_options.UnitCui,
            k_options.DoFurnizor.ToString(), k_options.DoClienti.ToString(),
            k_options.DoEmise.ToString(), k_options.DoPrimite.ToString()})
    End Function

    ' ---- verify -----------------------------------------------------------------------------------------------------------

    Private Async Sub btnEfVerifica_Click(sender As Object, e As EventArgs) Handles btnEfVerifica.Click
        Dim canImport As Boolean = False
        Try
            Dim options = BuildEfOptions()
            If options Is Nothing Then Return

            btnEfImporta.Enabled = False
            _efVerified = Nothing
            _efReport = Nothing
            rtbEfJurnal.Clear()
            SaveSettings()

            Dim transferWasEnabled = btnTransfera.Enabled
            SetBusy(True)
            BeginEfProgress()
            _cancellation = New CancellationTokenSource()
            Try
                Dim token = _cancellation.Token
                SayEf($"Verificare E-Factura pentru «{options.UnitDatabase}» (cod fiscal {options.UnitCui}).")
                Dim importer As New EfImporter(options, AddressOf SayEfFromWorker, AddressOf EfStepFromWorker)
                Dim report = Await Task.Run(Function() importer.Verify(token), token)
                ShowEfReport(report)

                If report.HasBlocking Then
                    Warn("Verificarea a găsit constatări BLOCANTE (în jurnal). Nu se poate importa până nu se rezolvă." &
                         Environment.NewLine & Environment.NewLine &
                         "Primele constatări sunt cele blocante; la fiecare scrie tabelul și rândul.")
                Else
                    _efReport = report
                    _efVerified = EfFingerprint(options)
                    canImport = True
                    SayEf("Verificare încheiată fără constatări blocante. «Importă» este activ.")
                End If
            Finally
                EndEfProgress()
                SetBusy(False)
                btnTransfera.Enabled = transferWasEnabled
                _cancellation?.Dispose()
                _cancellation = Nothing
            End Try
        Catch ex As OperationCanceledException
            SayEf("Verificare oprită.")
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.btnEfVerifica_Click", ex)
            Warn("Verificarea E-Factura a eșuat." & Environment.NewLine & Environment.NewLine & ex.Message)
        End Try
        btnEfImporta.Enabled = canImport
    End Sub

    Private Sub ShowEfReport(k_report As EfImportReport)
        SayEf("— Pe tabele —")
        For Each plan In k_report.Tables
            SayEf("   " & plan.Describe())
        Next

        Dim blocking = k_report.Findings.Where(Function(f) f.Severity = EfSeverity.Blocking).ToList()
        Dim warnings = k_report.Findings.Where(Function(f) f.Severity = EfSeverity.Warning).ToList()
        Dim infos = k_report.Findings.Where(Function(f) f.Severity = EfSeverity.Info).ToList()
        SayEf($"— Constatări: {blocking.Count} blocante, {warnings.Count} atenționări, {infos.Count} informări —")
        For Each finding In blocking.Concat(warnings).Concat(infos)
            SayEf("   " & finding.ToString())
        Next
    End Sub

    ' ---- import -----------------------------------------------------------------------------------------------------------------

    Private Async Sub btnEfImporta_Click(sender As Object, e As EventArgs) Handles btnEfImporta.Click
        Try
            Dim options = BuildEfOptions()
            If options Is Nothing Then Return

            If _efVerified Is Nothing OrElse Not String.Equals(_efVerified, EfFingerprint(options), StringComparison.Ordinal) Then
                btnEfImporta.Enabled = False
                Warn("Ceva s-a schimbat de la ultima verificare (o cale, o bifă, DC-ul sau codul fiscal). Rulați din nou «Verifică».")
                Return
            End If

            Dim toWrite = If(_efReport Is Nothing, 0L, _efReport.Tables.Sum(Function(t) t.RowsSelected))
            Dim confirmation = KBotMessage.Show(
                $"Se importă până la {toWrite} rânduri în baza «{options.UnitDatabase}»." &
                Environment.NewLine & Environment.NewLine &
                "Totul într-o tranzacție: orice eșec derulează tot înapoi. Rândurile care există deja în țintă rămân neschimbate." & Environment.NewLine & Environment.NewLine &
                "Continuați?",
                "Importă E-Factura", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If confirmation <> DialogResult.Yes Then Return

            btnEfImporta.Enabled = False
            Dim transferWasEnabled = btnTransfera.Enabled
            SetBusy(True)
            BeginEfProgress()
            _cancellation = New CancellationTokenSource()
            Try
                Dim token = _cancellation.Token
                SayEf("Import E-Factura: se scrie…")
                Dim importer As New EfImporter(options, AddressOf SayEfFromWorker, AddressOf EfStepFromWorker)
                Dim result = Await Task.Run(Function() importer.Run(token), token)

                SayEf("— Rezultat —")
                For Each plan In result.Tables
                    SayEf($"   {plan.TargetTable}: citite {plan.RowsRead}, adăugate {plan.RowsAdded}, deja existente {plan.RowsKept}, lăsate deoparte {plan.RowsLeftOut}.")
                Next
                SayEf($"Import încheiat cu COMMIT: {result.TotalAdded} rânduri adăugate.")
                _efVerified = Nothing
                _efReport = Nothing
                KBotMessage.Show($"Import E-Factura încheiat: {result.TotalAdded} rânduri adăugate." & Environment.NewLine &
                                 "Detaliile pe tabele sunt în jurnalul filei.",
                                 "Importă E-Factura", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Finally
                EndEfProgress()
                SetBusy(False)
                btnTransfera.Enabled = transferWasEnabled
                _cancellation?.Dispose()
                _cancellation = Nothing
            End Try
        Catch ex As OperationCanceledException
            SayEf("Import oprit. Tranzacția a fost derulată înapoi: nimic nu s-a scris.")
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.btnEfImporta_Click", ex)
            SayEf("Importul a eșuat și a fost derulat înapoi: " & ex.Message)
            Warn("Importul E-Factura a eșuat și a fost derulat înapoi. Nimic nu s-a scris." & Environment.NewLine & Environment.NewLine & ex.Message)
        End Try
    End Sub

    ' ---- log and progress ------------------------------------------------------------------------------------------------------

    Private Sub SayEf(k_message As String)
        Try
            rtbEfJurnal.AppendText($"[{DateTime.Now:HH:mm:ss}] {k_message}{Environment.NewLine}")
            rtbEfJurnal.SelectionStart = rtbEfJurnal.TextLength
            rtbEfJurnal.ScrollToCaret()
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.SayEf", ex)
        End Try
    End Sub

    Private Sub SayEfFromWorker(k_message As String)
        Try
            If InvokeRequired Then
                BeginInvoke(New Action(Of String)(AddressOf SayEf), k_message)
                Return
            End If
            SayEf(k_message)
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.SayEfFromWorker", ex)
        End Try
    End Sub

    Private Sub EfStepFromWorker(k_done As Integer, k_total As Integer, k_label As String)
        Try
            If InvokeRequired Then
                BeginInvoke(New Action(Of Integer, Integer, String)(AddressOf ShowEfStep), k_done, k_total, k_label)
                Return
            End If
            ShowEfStep(k_done, k_total, k_label)
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.EfStepFromWorker", ex)
        End Try
    End Sub

    Private Sub ShowEfStep(k_done As Integer, k_total As Integer, k_label As String)
        Try
            If k_total > 0 Then
                prgEf.Style = ProgressBarStyle.Blocks
                prgEf.Maximum = k_total
                prgEf.Value = Math.Max(0, Math.Min(k_done, k_total))
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.ShowEfStep", ex)
        End Try
    End Sub

    Private Sub BeginEfProgress()
        prgEf.Style = ProgressBarStyle.Marquee
    End Sub

    Private Sub EndEfProgress()
        prgEf.Style = ProgressBarStyle.Blocks
        prgEf.Minimum = 0
        prgEf.Maximum = 100
        prgEf.Value = 0
    End Sub

End Class
