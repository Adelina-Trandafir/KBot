Option Strict On
Imports System.Globalization
Imports System.Text

''' <summary>
''' The schema difference between AVACONT_SURSA and ONE database (slice ME01). Read-only: the server builds
''' the plan in memory and prints it; nothing is written to a database or to schema_diff_log.
''' </summary>
Public Class DiffView
    Implements IAdminView

    Private _context As AdminContext
    Private _last As DiffResult

    Public Sub New()
        InitializeComponent()
        cmbMode.Items.Add("Complet (tot ce diferă)")
        cmbMode.Items.Add("Doar adăugări (ce lipsește)")
        cmbMode.SelectedIndex = 0
    End Sub

    Public Sub Attach(k_context As AdminContext) Implements IAdminView.Attach
        _context = k_context
    End Sub

    Public Function ActivatedAsync() As Task Implements IAdminView.ActivatedAsync
        ShowTarget()
        Return Task.CompletedTask
    End Function

    Public Sub SelectionChanged() Implements IAdminView.SelectionChanged
        ShowTarget()
    End Sub

    Private Sub ShowTarget()
        lblSummary.ForeColor = ThemeManager.Current.Palette.TextColor
        If _context.IsAll Then
            lblSummary.Text = "Alegeți o bază din listă: diferența se calculează pe o singură bază, nu pe toate."
        ElseIf _last Is Nothing OrElse Not String.Equals(_last.Db, _context.SelectedDb, StringComparison.OrdinalIgnoreCase) Then
            lblSummary.Text = "Baza " & _context.SelectedDb & ": apăsați «Calculează diferența»."
        End If
    End Sub

    Private Async Sub BtnCompute_Click(k_sender As Object, e As EventArgs) Handles btnCompute.Click
        Try
            Dim k_db As String = _context.RequireSingleDb("Diferența de schemă")
            If k_db = String.Empty Then Return
            Dim k_mode As String = If(cmbMode.SelectedIndex = 1, "SAFE", "FORCE")
            Await UiRun.Guard("DiffView.BtnCompute_Click", _context,
                Async Function()
                    _context.Say("Se compară " & k_db & " cu AVACONT_SURSA...")
                    _last = Await _context.Client.CallAsync(Of DiffResult)("diff", "--db", k_db, "--mode", k_mode)
                    Fill(_last)
                    _context.Say("Gata.")
                End Function)
        Catch ex As Exception
            GlobalErrorLog.Write("DiffView.BtnCompute_Click", ex)
        End Try
    End Sub

    Private Sub Fill(k_result As DiffResult)
        grid.BeginUpdate()
        Try
            grid.ClearRows()
            For Each k_statement As DiffStatement In k_result.Statements
                Dim k_row As KBotDataRow = grid.AddRow()
                k_row("table") = k_statement.Table
                k_row("object") = k_statement.ObjectName
                k_row("kind") = k_statement.Kind
                k_row("action") = k_statement.Action
                k_row("destructive") = If(k_statement.Destructive, "DA", String.Empty)
                k_row("error") = k_statement.ErrorText
                k_row.Tag = k_statement
            Next
        Finally
            grid.EndUpdate()
        End Try
        txtSql.Text = String.Empty
        If k_result.Count = 0 Then
            lblSummary.Text = k_result.Db & ": nicio diferență față de AVACONT_SURSA."
            lblSummary.ForeColor = ThemeManager.Current.Palette.SuccessColor
        Else
            lblSummary.Text = k_result.Db & ": " & k_result.Count.ToString(CultureInfo.InvariantCulture) & " instrucțiuni" &
                              If(k_result.Destructive > 0, ", din care " & k_result.Destructive.ToString(CultureInfo.InvariantCulture) & " DISTRUCTIVE.", ".")
            lblSummary.ForeColor = If(k_result.Destructive > 0, ThemeManager.Current.Palette.WarningColor, ThemeManager.Current.Palette.TextColor)
        End If
        lblState.Text = "Mod: " & If(k_result.Mode = "SAFE", "doar adăugări", "complet") & ". Nimic nu s-a scris pe server."
    End Sub

    Private Sub Grid_RowFormatting(k_sender As Object, e As KBotRowFormattingEventArgs) Handles grid.RowFormatting
        Try
            Dim k_statement As DiffStatement = TryCast(e.Row.Tag, DiffStatement)
            If k_statement Is Nothing Then Return
            If k_statement.ErrorText <> String.Empty Then
                e.ForeColor = ThemeManager.Current.Palette.ErrorColor
            ElseIf k_statement.Destructive Then
                e.ForeColor = ThemeManager.Current.Palette.WarningColor
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("DiffView.Grid_RowFormatting", ex)
        End Try
    End Sub

    Private Sub Grid_SelectionChanged(k_sender As Object, e As EventArgs) Handles grid.SelectionChanged
        Try
            Dim k_row As KBotDataRow = grid.CurrentRow
            Dim k_statement As DiffStatement = If(k_row Is Nothing, Nothing, TryCast(k_row.Tag, DiffStatement))
            txtSql.Text = If(k_statement Is Nothing, String.Empty, k_statement.Sql)
        Catch ex As Exception
            GlobalErrorLog.Write("DiffView.Grid_SelectionChanged", ex)
        End Try
    End Sub

    Private Sub BtnCopy_Click(k_sender As Object, e As EventArgs) Handles btnCopy.Click
        Try
            If _last Is Nothing OrElse _last.Statements.Count = 0 Then Return
            Dim k_sb As New StringBuilder()
            For Each k_statement As DiffStatement In _last.Statements
                k_sb.AppendLine(k_statement.Sql)
            Next
            Clipboard.SetText(k_sb.ToString())
            _context.Say("Instrucțiunile au fost copiate în clipboard.")
        Catch ex As Exception
            GlobalErrorLog.Write("DiffView.BtnCopy_Click", ex)
        End Try
    End Sub

End Class
