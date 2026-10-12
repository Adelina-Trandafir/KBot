Imports System.IO
Imports System.Text
Imports System.Threading.Tasks
Imports KBot.Common

''' <summary>
''' SLICE-VC05-01: choose the Access files of the subunit and read them (read-only), then show / save the profile report.
''' Nothing is written to Access or to MariaDB in this slice. The plan, test and migrate steps come with VC05-02/03.
''' </summary>
Public Class VrcMigratorForm

    Private _busy As Boolean
    Private _lastReport As String = String.Empty

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub VrcMigratorForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            lblStare.Text = "Adăugați bazele subunității și apăsați «Citește»."
            UpdateButtons()
        Catch ex As Exception
            GlobalErrorLog.Write("VrcMigratorForm.Load", ex)
        End Try
    End Sub

    Private Sub UpdateButtons()
        btnAdauga.Enabled = Not _busy
        btnElimina.Enabled = Not _busy AndAlso lstFisiere.SelectedItems.Count > 0
        btnCiteste.Enabled = Not _busy AndAlso lstFisiere.Items.Count > 0
        btnSalveaza.Enabled = Not _busy AndAlso _lastReport.Length > 0
    End Sub

    Private Sub btnAdauga_Click(sender As Object, e As EventArgs) Handles btnAdauga.Click
        Try
            If ofdBaze.ShowDialog(Me) <> DialogResult.OK Then Return
            For Each k_file In ofdBaze.FileNames
                If Not lstFisiere.Items.Contains(k_file) Then lstFisiere.Items.Add(k_file)
            Next
            _lastReport = String.Empty
            UpdateButtons()
        Catch ex As Exception
            Fail("VrcMigratorForm.Adauga", "Fișierele nu au putut fi adăugate.", ex)
        End Try
    End Sub

    Private Sub btnElimina_Click(sender As Object, e As EventArgs) Handles btnElimina.Click
        Try
            For Each k_item In lstFisiere.SelectedItems.Cast(Of Object)().ToList()
                lstFisiere.Items.Remove(k_item)
            Next
            _lastReport = String.Empty
            UpdateButtons()
        Catch ex As Exception
            Fail("VrcMigratorForm.Elimina", "Fișierele nu au putut fi eliminate.", ex)
        End Try
    End Sub

    Private Sub lstFisiere_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstFisiere.SelectedIndexChanged
        UpdateButtons()
    End Sub

    Private Async Sub btnCiteste_Click(sender As Object, e As EventArgs) Handles btnCiteste.Click
        Try
            _busy = True
            UpdateButtons()
            rtbRaport.Clear()
            Dim k_files = lstFisiere.Items.Cast(Of String)().ToList()
            Dim k_progress As Action(Of String) = Sub(k_text) BeginInvoke(Sub() lblStare.Text = k_text)
            Dim k_profiles = Await Task.Run(Function() k_files.Select(Function(k_f) VrcProfiler.Profile(k_f, k_progress)).ToList())
            _lastReport = VrcReport.ToMarkdown(k_profiles)
            rtbRaport.Text = _lastReport
            Dim k_bad As Integer = k_profiles.Where(Function(k_p) k_p.ReadError.Length > 0 OrElse Not k_p.Stable).Count()
            lblStare.Text = $"{k_profiles.Count} fișiere citite" & If(k_bad > 0, $"; {k_bad} cu probleme (vezi raportul).", ". Salvați raportul dacă doriți.")
        Catch ex As Exception
            Fail("VrcMigratorForm.Citeste", "Citirea a eșuat.", ex)
        Finally
            _busy = False
            UpdateButtons()
        End Try
    End Sub

    Private Sub btnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            If sfdRaport.ShowDialog(Me) <> DialogResult.OK Then Return
            File.WriteAllText(sfdRaport.FileName, _lastReport, New UTF8Encoding(False))
            lblStare.Text = "Raport salvat: " & sfdRaport.FileName
        Catch ex As Exception
            Fail("VrcMigratorForm.Salveaza", "Raportul nu a putut fi salvat.", ex)
        End Try
    End Sub

    Private Sub Fail(k_source As String, k_message As String, k_ex As Exception)
        GlobalErrorLog.Write(k_source, k_ex)
        lblStare.Text = k_message
        KBotMessage.Show(k_message & Environment.NewLine & k_ex.Message, "VRC.Migrator", MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub

    ''' <summary>Closing waits for a running read: the files and the Access driver must be released first.</summary>
    Private Sub VrcMigratorForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If _busy Then
            e.Cancel = True
            KBotMessage.Show("Citirea este în curs. Așteptați să se termine înainte de a închide.", "VRC.Migrator", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

End Class
