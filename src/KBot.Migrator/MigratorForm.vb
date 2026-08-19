Imports System.Collections.Generic
Imports System.Data
Imports System.IO
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Ecranul utilitarului, cu cele patru regiuni din §5.3:
''' <list type="number">
''' <item><b>Surse</b> — folderul de artefacte, cheia API și DC-urile rezolvate din Cai.json,
''' fiecare cu bifă. Nimic nu pornește până nu e bifat cel puțin unul.</item>
''' <item><b>Verificare</b> — pașii 1–5, care nu scriu nimic. E butonul implicit.</item>
''' <item><b>Transfer</b> — pașii 6–7. Stă dezactivat până când verificarea a rulat curat,
''' sau până e forțat explicit prin bifa alăturată.</item>
''' <item><b>Jurnal</b> — coada live.</item>
''' </list>
'''
''' Cheia API nu e în plan, dar rutele de seed sunt păzite cu <c>X-Api-Key</c> și utilitarul
''' n-ar putea vorbi cu serverul fără ea. Se preîncarcă din variabila de mediu
''' <c>KBOT_SEED_API_KEY</c> dacă există, altfel o tastează operatorul; nu se scrie nicăieri.
''' </summary>
Public Class MigratorForm

    ''' <summary>Variabila de mediu din care se preia cheia, dacă e pusă.</summary>
    Public Const ApiKeyEnvVar As String = "KBOT_SEED_API_KEY"

    Private _verification As VerificationResult
    Private _verifiedFolder As String
    Private _busy As Boolean

    Public Sub New()
        InitializeComponent()

        Try
            Dim key As String = Environment.GetEnvironmentVariable(ApiKeyEnvVar)
            If Not String.IsNullOrWhiteSpace(key) Then txtCheie.Text = key

            ' Sugestia de folder: lângă executabil, dacă exportul a fost copiat acolo.
            Dim guess As String = Path.Combine(AppContext.BaseDirectory, "VBA_ARTEFACTS")
            If Directory.Exists(guess) Then txtFolder.Text = guess

            AcceptButton = btnVerifica
        Catch ex As Exception
            ' Graniță UI (constructor de formular): un throw ar împiedica deschiderea.
            GlobalErrorLog.Write("MigratorForm.New", ex)
        End Try
    End Sub

    ' =========================================================================
    ' Regiunea 1 — surse
    ' =========================================================================

    Private Sub btnRasfoire_Click(sender As Object, e As EventArgs) Handles btnRasfoire.Click
        Try
            If Directory.Exists(txtFolder.Text) Then dlgFolder.SelectedPath = txtFolder.Text
            If dlgFolder.ShowDialog(Me) = DialogResult.OK Then
                txtFolder.Text = dlgFolder.SelectedPath
                ResetVerification("Folder schimbat — rulează din nou verificarea.")
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.btnRasfoire_Click", ex)
            ShowError(ex)
        End Try
    End Sub

    ''' <summary>
    ''' Citește DOAR Cai.json și listează DC-urile. Nu atinge serverul și nu citește tabelele —
    ''' e pasul minim ca operatorul să aibă ce bifa.
    ''' </summary>
    Private Sub btnIncarca_Click(sender As Object, e As EventArgs) Handles btnIncarca.Click
        Try
            If Not ValidateFolder() Then Return

            Dim reader As New ArtifactReader(txtFolder.Text)
            Dim manifest As ExportManifest = reader.ReadManifest()
            Dim builderLog As New List(Of String)()
            Dim maps As New RoutingMaps()

            ' Doar harta [Cai]: restul se construiește la verificare.
            Dim chunk As ChunkFile = reader.ReadChunk(manifest.CaiFile)
            Dim iUnit As Integer = chunk.IndexOfColumn("IdUnitate")
            Dim iDc As Integer = chunk.IndexOfColumn("DC")
            If iUnit < 0 OrElse iDc < 0 Then
                Throw New InvalidOperationException(
                    "«" & manifest.CaiFile & "» nu are coloanele IdUnitate și DC.")
            End If

            Dim dcs As New SortedSet(Of String)(StringComparer.OrdinalIgnoreCase)
            For Each row In chunk.Rows
                Dim dc As String = JsonValues.AsText(row(iDc))
                If Not String.IsNullOrWhiteSpace(dc) Then dcs.Add(dc)
            Next

            clbDc.Items.Clear()
            For Each dc As String In dcs
                clbDc.Items.Add(dc)
            Next

            ResetVerification("S-au găsit " & dcs.Count.ToString() & " DC-uri în [Cai] (" &
                              manifest.CaiRows.ToString() & " unități). Bifează și verifică.")
            AppendLog("Artefacte din «" & txtFolder.Text & "», export din " & manifest.Exported & ".")
            If manifest.UnexpectedTables.Count > 0 Then
                AppendLog("ATENȚIE: tabele în afara domeniului există în Access: " &
                          String.Join(", ", manifest.UnexpectedTables) & " (nu au fost exportate).")
            End If

        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.btnIncarca_Click", ex)
            ShowError(ex)
        End Try
    End Sub

    ' =========================================================================
    ' Regiunea 2 — verificare (nu scrie nimic)
    ' =========================================================================

    Private Async Sub btnVerifica_Click(sender As Object, e As EventArgs) Handles btnVerifica.Click
        Try
            If _busy Then Return
            Dim dcs As List(Of String) = SelectedDcs()
            If dcs Is Nothing Then Return
            If Not ValidateKey() Then Return

            SetBusy(True, "Verificare în curs…")
            txtJurnal.Clear()
            _verification = Nothing
            _verifiedFolder = Nothing

            Using log As New RunLog(dcs)
                AddHandler log.Line, AddressOf OnLogLine
                Try
                    Using api As New SeedApiClient(txtCheie.Text)
                        Dim reader As New ArtifactReader(txtFolder.Text)
                        Dim runner As New MigrationRunner(reader, api, log, dcs)
                        _verification = Await runner.VerifyAsync()
                        _verifiedFolder = txtFolder.Text
                    End Using
                Finally
                    RemoveHandler log.Line, AddressOf OnLogLine
                End Try
            End Using

            FillGrid(_verification)
            UpdateTransferButton()
            lblStare.Text = If(_verification.IsClean,
                               "Verificare curată — transferul poate porni.",
                               "Verificare cu opriri — vezi jurnalul.")

        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.btnVerifica_Click", ex)
            _verification = Nothing
            UpdateTransferButton()
            lblStare.Text = "Verificarea s-a oprit."
            ShowError(ex)
        Finally
            SetBusy(False, Nothing)
        End Try
    End Sub

    ' =========================================================================
    ' Regiunea 3 — transfer
    ' =========================================================================

    Private Async Sub btnTransfera_Click(sender As Object, e As EventArgs) Handles btnTransfera.Click
        Try
            If _busy Then Return
            If _verification Is Nothing Then Return
            If Not String.Equals(_verifiedFolder, txtFolder.Text, StringComparison.OrdinalIgnoreCase) Then
                MessageBox.Show(Me, "Folderul s-a schimbat după verificare. Rulează verificarea din nou.",
                                "Migrare FX", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim dcs As List(Of String) = _verification.CleanDcs
            If dcs.Count = 0 Then
                MessageBox.Show(Me, "Niciun DC nu a trecut verificarea.",
                                "Migrare FX", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim confirm As DialogResult = MessageBox.Show(
                Me,
                "Se scriu rândurile în " & dcs.Count.ToString() & " baze: " & String.Join(", ", dcs) & "." &
                Environment.NewLine & Environment.NewLine &
                "Rândurile deja existente rămân neatinse. Continui?",
                "Migrare FX", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If confirm <> DialogResult.Yes Then Return

            SetBusy(True, "Transfer în curs…")

            Using log As New RunLog(dcs)
                AddHandler log.Line, AddressOf OnLogLine
                Try
                    Using api As New SeedApiClient(txtCheie.Text)
                        Dim reader As New ArtifactReader(txtFolder.Text)
                        Dim runner As New MigrationRunner(reader, api, log, dcs)
                        Await runner.TransferAsync(_verification)
                    End Using
                Finally
                    RemoveHandler log.Line, AddressOf OnLogLine
                End Try
            End Using

            FillGrid(_verification)
            lblStare.Text = "Transfer încheiat — vezi jurnalul din folderul Logs."

        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.btnTransfera_Click", ex)
            lblStare.Text = "Transferul s-a oprit."
            ShowError(ex)
        Finally
            SetBusy(False, Nothing)
        End Try
    End Sub

    Private Sub chkForteaza_CheckedChanged(sender As Object, e As EventArgs) Handles chkForteaza.CheckedChanged
        Try
            UpdateTransferButton()
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.chkForteaza_CheckedChanged", ex)
        End Try
    End Sub

    Private Sub clbDc_ItemCheck(sender As Object, e As ItemCheckEventArgs) Handles clbDc.ItemCheck
        Try
            ' Selecția s-a schimbat: verificarea de dinainte nu mai acoperă ce e bifat acum.
            ResetVerification("Selecția s-a schimbat — rulează din nou verificarea.")
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.clbDc_ItemCheck", ex)
        End Try
    End Sub

    ' =========================================================================
    ' Regiunea 4 — jurnal, plus ajutoare de ecran
    ' =========================================================================

    Private Sub OnLogLine(text As String)
        Try
            If txtJurnal.InvokeRequired Then
                txtJurnal.BeginInvoke(New Action(Of String)(AddressOf OnLogLine), text)
                Return
            End If
            AppendLog(text)
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.OnLogLine", ex)
        End Try
    End Sub

    Private Sub AppendLog(text As String)
        txtJurnal.AppendText(text & Environment.NewLine)
    End Sub

    Private Sub FillGrid(v As VerificationResult)
        Dim dt As New DataTable()
        dt.Columns.Add("DC", GetType(String))
        dt.Columns.Add("Tabel", GetType(String))
        dt.Columns.Add("Citite", GetType(Integer))
        dt.Columns.Add("Rutate", GetType(Integer))
        dt.Columns.Add("Inserate", GetType(Integer))
        dt.Columns.Add("Deja existente", GetType(Integer))
        dt.Columns.Add("Respinse", GetType(Integer))

        If v IsNot Nothing Then
            For Each st In SeedTables.All()
                Dim s As TableStats = v.StatsFor(st.Name)
                For Each dc As String In v.CleanDcs
                    Dim d As DcTableStats = s.For1(dc)
                    dt.Rows.Add(dc, st.Name, s.Read, d.Routed, d.Inserted, d.Skipped, s.Rejected)
                Next
            Next
        End If

        dgvRezultate.DataSource = dt
    End Sub

    ''' <summary>DC-urile bifate, sau Nothing (cu mesaj) dacă nu e bifat niciunul.</summary>
    Private Function SelectedDcs() As List(Of String)
        If Not ValidateFolder() Then Return Nothing
        Dim dcs As New List(Of String)()
        For Each item As Object In clbDc.CheckedItems
            dcs.Add(CStr(item))
        Next
        If dcs.Count = 0 Then
            MessageBox.Show(Me, "Bifează cel puțin o bază de date (DC).",
                            "Migrare FX", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return Nothing
        End If
        Return dcs
    End Function

    Private Function ValidateFolder() As Boolean
        If String.IsNullOrWhiteSpace(txtFolder.Text) OrElse Not Directory.Exists(txtFolder.Text) Then
            MessageBox.Show(Me, "Alege folderul cu artefactele exportate din Access (VBA_ARTEFACTE).",
                            "Migrare FX", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return False
        End If
        Return True
    End Function

    Private Function ValidateKey() As Boolean
        If String.IsNullOrWhiteSpace(txtCheie.Text) Then
            MessageBox.Show(Me, "Completează cheia API (X-Api-Key). Rutele de seed nu folosesc token bearer.",
                            "Migrare FX", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return False
        End If
        Return True
    End Function

    Private Sub ResetVerification(message As String)
        _verification = Nothing
        _verifiedFolder = Nothing
        UpdateTransferButton()
        If message IsNot Nothing Then lblStare.Text = message
    End Sub

    ''' <summary>
    ''' Transferul rămâne dezactivat până când verificarea a rulat CURAT — sau până când
    ''' operatorul cere explicit forțarea.
    ''' </summary>
    Private Sub UpdateTransferButton()
        btnTransfera.Enabled = Not _busy AndAlso _verification IsNot Nothing AndAlso
                               _verification.CleanDcs.Count > 0 AndAlso
                               (_verification.IsClean OrElse chkForteaza.Checked)
    End Sub

    Private Sub SetBusy(busy As Boolean, message As String)
        _busy = busy
        btnVerifica.Enabled = Not busy
        btnRasfoire.Enabled = Not busy
        btnIncarca.Enabled = Not busy
        clbDc.Enabled = Not busy
        UpdateTransferButton()
        If message IsNot Nothing Then lblStare.Text = message
        Cursor = If(busy, Cursors.WaitCursor, Cursors.Default)
    End Sub

    Private Sub ShowError(ex As Exception)
        MessageBox.Show(Me, ex.Message, "Migrare FX", MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub

End Class
