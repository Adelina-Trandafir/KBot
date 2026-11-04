Imports System.IO
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Migrator

''' <summary>Separate preview/apply workflow; never calls the full migration writer.</summary>
Public Class AdeOpeningBalanceForm
    Private ReadOnly _server As TargetServer
    Private ReadOnly _dc As String
    Private ReadOnly _subunit As String
    Private ReadOnly _file As String
    Private _preview As AdeOpeningBalancePreview
    Private _busy As Boolean

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(server As TargetServer, dc As String, subunit As String, file As String)
        Me.New()
        _server = server
        _dc = dc
        _subunit = subunit
        _file = file
        lblContext.Text = $"Destinație: {dc} / {subunit}. MDB complet: {Path.GetFileName(file)}." & Environment.NewLine &
            "Copiii din 2020 care continuă în 2021. SI existent se păstrează; soldul reportat se salvează separat. Pozitiv = debit; negativ = credit."
    End Sub

    Private Async Sub AdeOpeningBalanceForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If _server IsNot Nothing Then Await ReadPreviewAsync()
    End Sub

    Private Async Sub BtnPreview_Click(sender As Object, e As EventArgs) Handles btnPreview.Click
        Await ReadPreviewAsync()
    End Sub

    Private Async Function ReadPreviewAsync() As Task
        If _busy Then Return
        _preview = Nothing
        SetBusy(True)
        lblStatus.Text = "Se citesc MDB-ul complet și soldurile din destinație; nu se scrie nimic."
        Try
            _preview = Await Task.Run(Function() AdeOpeningBalanceTransfer.Preview(_server, _dc, _subunit, _file))
            ShowRows()
            lblStatus.Text = PreviewSummary()
        Catch ex As Exception
            ShowError("AdeOpeningBalanceForm.Preview", ex)
        Finally
            SetBusy(False)
        End Try
    End Function

    Private Sub ShowRows()
        grid.BeginUpdate()
        Try
            grid.ClearRows()
            For Each item In _preview.Rows
                If chkUnresolved.Checked AndAlso item.Resolved Then Continue For
                Dim row = grid.AddRow()
                row("name") = item.Name
                row("initial") = If(item.ExistingInitial.HasValue, CType(item.ExistingInitial.Value, Object), Nothing)
                row("old") = If(item.OldValue.HasValue, CType(item.OldValue.Value, Object), Nothing)
                row("new") = If(item.HasProposed, CType(item.NewValue, Object), Nothing)
                row("difference") = If(item.OldValue.HasValue AndAlso item.HasProposed,
                    CType(CLng(item.NewValue) - item.OldValue.GetValueOrDefault(), Object), Nothing)
                row("origin") = item.Origin
                row("state") = item.State
                row("combined") = If(item.HasProposed, CType(item.CombinedInitial, Object), Nothing)
            Next
        Finally
            grid.EndUpdate()
        End Try
    End Sub

    Private Function PreviewSummary() As String
        Dim unresolved = _preview.Rows.Where(Function(r) Not r.Resolved).Count()
        Dim applicable = _preview.Rows.Where(Function(r) r.CanApply).Count()
        Dim visible = If(chkUnresolved.Checked, unresolved, _preview.Rows.Count)
        Return $"Afișați: {visible} din {_preview.Rows.Count} copii; nerezolvați: {unresolved} (de aplicat: {applicable}; omiși sau blocați: {unresolved - applicable}). Consultați Stare / motiv."
    End Function

    Private Sub ChkUnresolved_CheckedChanged(sender As Object, e As EventArgs) Handles chkUnresolved.CheckedChanged
        If _busy OrElse _preview Is Nothing Then Return
        Try
            ShowRows()
            lblStatus.Text = PreviewSummary()
        Catch ex As Exception
            ShowError("AdeOpeningBalanceForm.Filter", ex)
        End Try
    End Sub

    Private Async Sub BtnApply_Click(sender As Object, e As EventArgs) Handles btnApply.Click
        If _busy OrElse _preview Is Nothing Then Return
        Dim count = _preview.Rows.Where(Function(r) r.CanApply).Count()
        If count = 0 Then Return
        If KBotMessage.Show($"Se salvează separat soldul reportat pentru {count} copii în {_dc} / {_subunit}." &
            Environment.NewLine & "Se aplică rândurile «De aplicat». SI existent, situațiile salvate și documentele rămân nemodificate." &
            Environment.NewLine & Environment.NewLine & "Salvați soldurile reportate afișate?", "Solduri reportate",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
        Dim approved = _preview
        _preview = Nothing
        SetBusy(True)
        lblStatus.Text = "Se salvează soldurile reportate separat, într-o singură tranzacție..."
        Try
            Dim progress As IProgress(Of String) = New Progress(Of String)(Sub(message) lblStatus.Text = message)
            Dim written = Await Task.Run(Function() AdeOpeningBalanceTransfer.Apply(_server, _dc, _subunit, approved,
                Path.Combine(AppContext.BaseDirectory, "Jurnale", "ADE-SI"), AddressOf progress.Report))
            For Each item In approved.Rows.Where(Function(r) r.CanApply)
                item.State = "Aplicat"
                item.CanApply = False
                item.Resolved = True
            Next
            _preview = approved
            ShowRows()
            lblStatus.Text = $"COMMIT confirmat: {written} solduri reportate salvate separat. " & PreviewSummary() & " Pentru recitire: «Previzualizează»."
        Catch ex As Exception
            ShowError("AdeOpeningBalanceForm.Apply", ex)
        Finally
            SetBusy(False)
        End Try
    End Sub

    Private Sub SetBusy(value As Boolean)
        _busy = value
        btnPreview.Enabled = Not value
        chkUnresolved.Enabled = Not value
        btnApply.Enabled = Not value AndAlso _preview IsNot Nothing AndAlso _preview.Rows.Any(Function(r) r.CanApply)
        busyBar.Running = value
        UseWaitCursor = value
    End Sub

    Private Sub ShowError(context As String, error1 As Exception)
        GlobalErrorLog.Write(context, error1)
        lblStatus.Text = error1.Message
        KBotMessage.Show(error1.Message, "Solduri reportate", MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub

    Private Sub AdeOpeningBalanceForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If Not _busy Then Return
        e.Cancel = True
        lblStatus.Text = "Așteptați terminarea operației înainte de închiderea ferestrei."
    End Sub
End Class
