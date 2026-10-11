Option Strict On
Imports System.Globalization
Imports System.Text

''' <summary>
''' Whitelisted server commands on the chosen database or on all of them (slice ME01). The list comes
''' from the server (verb <c>commands</c>); the program never sends free SQL.
''' </summary>
Public Class CommandsView
    Implements IAdminView

    Private Const MaxColumnWidth As Integer = 60

    Private _context As AdminContext
    Private _commands As New List(Of CommandInfo)()

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub Attach(k_context As AdminContext) Implements IAdminView.Attach
        _context = k_context
    End Sub

    Public Async Function ActivatedAsync() As Task Implements IAdminView.ActivatedAsync
        If _commands.Count > 0 Then
            ShowTarget()
            Return
        End If
        Await UiRun.Guard("CommandsView.ActivatedAsync", _context,
            Async Function()
                Dim k_result As CommandsResult = Await _context.Client.CallAsync(Of CommandsResult)("commands")
                _commands = k_result.Commands
                cmbCommand.Items.Clear()
                For Each k_command As CommandInfo In _commands
                    cmbCommand.Items.Add(k_command.Title & If(k_command.Writes, "  (scrie)", String.Empty))
                Next
                If cmbCommand.Items.Count > 0 Then cmbCommand.SelectedIndex = 0
                ShowTarget()
            End Function)
    End Function

    Public Sub SelectionChanged() Implements IAdminView.SelectionChanged
        ShowTarget()
    End Sub

    Private Sub ShowTarget()
        Dim k_target As String = If(_context.IsAll, AdminContext.AllDatabasesCaption, _context.SelectedDb)
        Dim k_command As CommandInfo = CurrentCommand()
        lblDesc.Text = If(k_command Is Nothing, String.Empty, k_command.Description & "   Țintă: " & k_target)
    End Sub

    Private Function CurrentCommand() As CommandInfo
        Dim k_index As Integer = cmbCommand.SelectedIndex
        Return If(k_index >= 0 AndAlso k_index < _commands.Count, _commands(k_index), Nothing)
    End Function

    Private Sub CmbCommand_SelectedIndexChanged(k_sender As Object, e As EventArgs) Handles cmbCommand.SelectedIndexChanged
        ShowTarget()
    End Sub

    Private Async Sub BtnRun_Click(k_sender As Object, e As EventArgs) Handles btnRun.Click
        Try
            Dim k_command As CommandInfo = CurrentCommand()
            If k_command Is Nothing Then Return
            Dim k_target As String = If(_context.IsAll, "TOATE bazele", "baza " & _context.SelectedDb)
            If k_command.Writes AndAlso Not UiRun.Confirm("Comanda «" & k_command.Title & "» scrie în " & k_target & "." & vbLf & "Continuați?", "Comenzi") Then Return
            Await UiRun.Guard("CommandsView.BtnRun_Click", _context,
                Async Function()
                    _context.Say("Se rulează «" & k_command.Title & "» pe " & k_target & "...")
                    Dim k_started As DateTime = DateTime.Now
                    Dim k_result As CommandResult = Await _context.Client.CallAsync(Of CommandResult)("command", "--name", k_command.Name, "--db", _context.DbArgument)
                    txtOutput.Text = RenderResult(k_result)
                    lblState.Text = k_result.Blocks.Count.ToString(CultureInfo.InvariantCulture) & " blocuri, " &
                                    (DateTime.Now - k_started).TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture) & " s."
                    _context.Say("Gata.")
                End Function)
        Catch ex As Exception
            GlobalErrorLog.Write("CommandsView.BtnRun_Click", ex)
        End Try
    End Sub

    Private Sub BtnCopy_Click(k_sender As Object, e As EventArgs) Handles btnCopy.Click
        Try
            If txtOutput.Text <> String.Empty Then Clipboard.SetText(txtOutput.Text)
            _context.Say("Rezultatul a fost copiat în clipboard.")
        Catch ex As Exception
            GlobalErrorLog.Write("CommandsView.BtnCopy_Click", ex)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' Formatting: aligned text, one block per database
    ' ------------------------------------------------------------------

    Private Shared Function RenderResult(k_result As CommandResult) As String
        Dim k_sb As New StringBuilder()
        For Each k_block As CommandBlock In k_result.Blocks
            k_sb.AppendLine("== " & k_block.Db & " - " & k_block.Title & " ==")
            Dim k_widths(k_block.Columns.Count - 1) As Integer
            For k_i As Integer = 0 To k_block.Columns.Count - 1
                k_widths(k_i) = Math.Min(MaxColumnWidth, k_block.Columns(k_i).Length)
            Next
            For Each k_row As List(Of String) In k_block.Rows
                For k_i As Integer = 0 To Math.Min(k_row.Count, k_widths.Length) - 1
                    k_widths(k_i) = Math.Max(k_widths(k_i), Math.Min(MaxColumnWidth, k_row(k_i).Length))
                Next
            Next
            AppendRow(k_sb, k_block.Columns, k_widths)
            k_sb.AppendLine(New String("-"c, Math.Max(1, k_widths.Sum() + 2 * Math.Max(0, k_widths.Length - 1))))
            For Each k_row As List(Of String) In k_block.Rows
                AppendRow(k_sb, k_row, k_widths)
            Next
            If k_block.Rows.Count = 0 Then k_sb.AppendLine("(niciun rând)")
            If k_block.Note <> String.Empty Then k_sb.AppendLine(k_block.Note)
            k_sb.AppendLine()
        Next
        Return k_sb.ToString()
    End Function

    Private Shared Sub AppendRow(k_sb As StringBuilder, k_cells As List(Of String), k_widths As Integer())
        For k_i As Integer = 0 To k_widths.Length - 1
            Dim k_text As String = If(k_i < k_cells.Count, k_cells(k_i), String.Empty).Replace(vbCr, " ").Replace(vbLf, " ")
            If k_text.Length > MaxColumnWidth Then k_text = k_text.Substring(0, MaxColumnWidth - 1) & "…"
            k_sb.Append(k_text.PadRight(k_widths(k_i)))
            If k_i < k_widths.Length - 1 Then k_sb.Append("  ")
        Next
        k_sb.AppendLine()
    End Sub

End Class
