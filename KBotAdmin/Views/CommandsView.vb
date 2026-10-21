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

    ' What to expect from each command, by the name the server gives it (verb «commands»). A command the server adds later and this list
    ' does not know falls back to the generic sentence below.
    Private Shared ReadOnly Expectations As New Dictionary(Of String, String)(StringComparer.Ordinal) From {
        {"ping", "Un singur rând pe bază: versiunea MariaDB, contul cu care s-a conectat serverul, charset-ul și collation-ul bazei și timpul de răspuns. Normal: charset utf8mb3, collation utf8mb3_general_ci, un timp de răspuns de câteva zeci de milisecunde. Altceva, sau sute de milisecunde, merită verificat."},
        {"tables", "Lista tuturor tabelelor bazei, cele mai mari primele, cu mărimea în MB, motorul și collation-ul. Numărul de rânduri e doar estimat de server, nu exact. Se citește, nu se schimbă nimic."},
        {"counts", "Numărul exact de rânduri al fiecărei tabele. Tabelele de peste 3 milioane de rânduri rămân pe estimare (nota de sub rezultat spune câte). Pe baze mari poate dura câteva secunde."},
        {"processlist", "Conexiunile deschise acum pe bază: cine, în ce stare, de câte secunde și ce comandă rulează. Normal: puține rânduri. Multe rânduri cu timp mare arată conexiuni rămase agățate."},
        {"journal", "Ultimele 50 de intrări din Jurnal pentru baza aleasă (cont, acțiune, țintă, rezultat), cele mai noi primele."},
        {"activity", "Pentru fiecare cont al bazei: ultima acțiune și numărul de acțiuni din Jurnal. Un cont care lipsește din listă nu a făcut nimic înregistrat."},
        {"charset", "Rezultatul bun este LISTA GOALĂ (nota spune că totul este utf8mb3_general_ci). Fiecare rând apărut este o coloană cu alt collation și trebuie corectată."},
        {"analyze", "Rulează ANALYZE TABLE pe fiecare tabelă: se reînnoiesc doar statisticile, datele rămân neschimbate. Fiecare tabelă ar trebui să răspundă «OK». Cere confirmare, fiindcă scrie în statistici; pe baze mari poate dura."},
        {"fisa_angajament", "Un tabel cu tot ce știe baza despre angajamentul scris: întâi rândurile DIAG (verificările calculate), apoi rândurile brute din toate tabelele. Cere o bază anume și codul angajamentului; nu merge pe <TOATE BAZELE>. Rândurile nu se trunchiază, ca rezultatul să poată fi copiat."}
    }

    Private Sub ShowTarget()
        Dim k_target As String = If(_context.IsAll, AdminContext.AllDatabasesCaption, _context.SelectedDb)
        Dim k_command As CommandInfo = CurrentCommand()
        lblDesc.Text = If(k_command Is Nothing, String.Empty, k_command.Description & "   Țintă: " & k_target)
        txtExplain.Text = If(k_command Is Nothing, String.Empty, ExplainCommand(k_command, k_target))
        ' the typed value exists only for a command that needs it; another choice in the combo makes the box disappear
        Dim k_needsParam As Boolean = k_command IsNot Nothing AndAlso k_command.Param <> String.Empty
        txtParam.Visible = k_needsParam
        If k_needsParam Then txtParam.PlaceholderText = k_command.Param
    End Sub

    Private Shared Function ExplainCommand(k_command As CommandInfo, k_target As String) As String
        Dim k_expect As String = Nothing
        If Not Expectations.TryGetValue(k_command.Name, k_expect) Then
            k_expect = "Rezultatul apare mai sus, câte un bloc pe fiecare bază; nota de sub fiecare bloc spune ce trebuie știut despre el."
        End If
        Dim k_sb As New StringBuilder()
        k_sb.Append("Ce face: ").AppendLine(k_command.Description)
        k_sb.Append("Ce te aștepți: ").AppendLine(k_expect)
        k_sb.Append("Țintă: ").Append(k_target)
        If k_command.Writes Then k_sb.Append("   (scrie în bază: cere confirmare)")
        If k_command.Param <> String.Empty Then k_sb.Append("   (cere: ").Append(k_command.Param.ToLowerInvariant()).Append(")")
        Return k_sb.ToString()
    End Function

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
            Dim k_param As String = String.Empty
            If k_command.Param <> String.Empty Then
                ' a command with a typed value runs on ONE database: refuse «all databases» before anything goes to the server
                If _context.RequireSingleDb("Comanda «" & k_command.Title & "»") = String.Empty Then Return
                k_param = txtParam.Text.Trim()
                If k_param = String.Empty Then
                    KBotMessage.Show("Scrieți " & k_command.Param.ToLowerInvariant() & ".", "Comenzi", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    txtParam.Focus()
                    Return
                End If
            End If
            Dim k_target As String = If(_context.IsAll, "TOATE bazele", "baza " & _context.SelectedDb)
            If k_command.Writes AndAlso Not UiRun.Confirm("Comanda «" & k_command.Title & "» scrie în " & k_target & "." & vbLf & "Continuați?", "Comenzi") Then Return
            Await UiRun.Guard("CommandsView.BtnRun_Click", _context,
                Async Function()
                    _context.Say("Se rulează «" & k_command.Title & "» pe " & k_target & "...")
                    Dim k_started As DateTime = DateTime.Now
                    Dim k_result As CommandResult = Await _context.Client.CallAsync(Of CommandResult)("command", "--name", k_command.Name, "--db", _context.DbArgument, "--param", k_param)
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
            If k_block.Raw Then
                ' one untruncated line per row (the result is meant to be copied and handed over)
                For Each k_row As List(Of String) In k_block.Rows
                    k_sb.AppendLine(String.Join(" | ", k_row))
                Next
                If k_block.Rows.Count = 0 Then k_sb.AppendLine("(niciun rând)")
                If k_block.Note <> String.Empty Then k_sb.AppendLine(k_block.Note)
                k_sb.AppendLine()
                Continue For
            End If
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
