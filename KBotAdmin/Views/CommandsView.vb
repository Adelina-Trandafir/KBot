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

    ' Repair buttons use the count in each check's summary, tied to the selected command/database.
    Private Const SignCheckName As String = "oi_semne"
    Private Const SignRepairName As String = "oi_semne_repara"
    Private Const SignBlockTitle As String = "Semne diferite"
    Private Const RepairableColumn As String = "cu cantitate negativa"
    Private Const StornoCheckName As String = "storno_preturi"
    Private Const StornoRepairName As String = "storno_preturi_repara"
    Private Const StornoBlockTitle As String = "Preturi storno si retur VERCON - sumar"

    Private _context As AdminContext
    Private _commands As New List(Of CommandInfo)()
    Private _repairCommand As String = String.Empty
    Private _repairDb As String = String.Empty

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
                _commands = k_result.Commands.OrderBy(Function(k_c) k_c.Title, StringComparer.Create(CultureInfo.GetCultureInfo("ro-RO"), True)).ToList()
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
        {StornoCheckName, "Verifică prețurile cu TVA ale tuturor liniilor storno și bonurilor de retur din baza selectată, în toate gestiunile și exercițiile. Arată sumarul pe gestiune și tip, apoi fiecare diferență cu produsul, documentul, prețul actual și cel propus. Sunt reparabile liniile migrate, nemodificate și active, la care prețul întreg corespunde rotunjirii Access a prețului variantei din aceeași gestiune. Celelalte diferențe rămân pentru analiză. Dacă există cazuri reparabile, apare butonul «Repară», care le corectează pe toate în baza selectată. Verificarea doar citește și cere o singură bază."},
        {StornoRepairName, "Repară toate cazurile eligibile de preț rotunjit la storno și bonuri de retur în baza selectată. Reface verificarea înainte să scrie, recuperează prețul cu TVA din varianta aceleiași gestiuni și actualizează reviziile liniilor și documentelor. Nu adaugă încă o dată TVA și păstrează cantitățile și valorile din antete. Corecțiile au audit cu prețul vechi/nou și o înregistrare în Jurnal; operația este într-o singură tranzacție pentru ambele tipuri. La sfârșit arată liniile corectate și diferențele rămase pentru analiză. Nu se rulează pe <TOATE BAZELE>."},
        {"ping", "Un singur rând pe bază: versiunea MariaDB, contul cu care s-a conectat serverul, charset-ul și collation-ul bazei și timpul de răspuns. Normal: charset utf8mb3, collation utf8mb3_general_ci, un timp de răspuns de câteva zeci de milisecunde. Altceva, sau sute de milisecunde, merită verificat."},
        {"tables", "Lista tuturor tabelelor bazei, cele mai mari primele, cu mărimea în MB, motorul și collation-ul. Numărul de rânduri e doar estimat de server, nu exact. Se citește, nu se schimbă nimic."},
        {"counts", "Numărul exact de rânduri al fiecărei tabele. Tabelele de peste 3 milioane de rânduri rămân pe estimare (nota de sub rezultat spune câte). Pe baze mari poate dura câteva secunde."},
        {"processlist", "Conexiunile deschise acum pe bază: cine, în ce stare, de câte secunde și ce comandă rulează. Normal: puține rânduri. Multe rânduri cu timp mare arată conexiuni rămase agățate."},
        {"journal", "Ultimele 50 de intrări din Jurnal pentru baza aleasă (cont, acțiune, țintă, rezultat), cele mai noi primele."},
        {"activity", "Pentru fiecare cont al bazei: ultima acțiune și numărul de acțiuni din Jurnal. Un cont care lipsește din listă nu a făcut nimic înregistrat."},
        {"charset", "Rezultatul bun este LISTA GOALĂ (nota spune că totul este utf8mb3_general_ci). Fiecare rând apărut este o coloană cu alt collation și trebuie corectată."},
        {"analyze", "Rulează ANALYZE TABLE pe fiecare tabelă: se reînnoiesc doar statisticile, datele rămân neschimbate. Fiecare tabelă ar trebui să răspundă «OK». Cere confirmare, fiindcă scrie în statistici; pe baze mari poate dura."},
        {"fisa_angajament", "Un tabel cu tot ce știe baza despre angajamentul scris: întâi rândurile DIAG (verificările calculate), apoi rândurile brute din toate tabelele. Cere o bază anume și codul angajamentului; nu merge pe <TOATE BAZELE>. Rândurile nu se trunchiază, ca rezultatul să poată fi copiat."},
        {"oi_semne_repara", "Corectează în registrul OI al bazei alese rândurile cu cantitate negativă și valoare pozitivă: valoarea devine negativă, ca cantitatea. Rândurile cu cantitate pozitivă și valoare negativă nu se ating, rămân în rezultat. SCRIE în baza aleasă, cere confirmare, notează în Jurnal și arată apoi ce a mai rămas. Merge doar pe o bază anume. În mod obișnuit nu se alege de aici: butonul «Repară» apare singur după «Semne greșite»."},
        {"oi_semne", "Caută în registrul OI (VC_OI_Produse) rândurile la care cantitatea și valoarea au semne diferite, de exemplu cantitate -1 și valoare +184,99. Rezultatul: pe fiecare gestiune, câte rânduri sunt, la câte produse, pe ce fel de bon (predare, transfer, casare) și primele numere IDOI. Lista goală înseamnă că nu e nimic de corectat. Se poate rula pe <TOATE BAZELE>; o bază fără tabelele VERCON o spune și trece mai departe. Se citește, nu se schimbă nimic."},
        {"profil_produs","Cere o bază anume și IdProdus (numărul variantei produsului din gestiune, nu codul din nomenclator). Rezultatul, de sus în jos: produsul (denumire, cod, preț, câte gestiuni îl au); în ce tabele VC_ apare și câte rânduri; apoi, pe fiecare gestiune, observațiile despre registrul OI (liniile de bon care nu-și găsesc contul în registru, adică exact cazul OI_ACCOUNT_ORIGIN), stocul OI pe deținător și, la urmă, fiecare rând din fiecare tabelă, cu antetul bonului sau NIR-ului lângă el. Se citește, nu se schimbă nimic. Rezultatul se copiază cu butonul de copiere și se poate lipi în conversație."}
    }

    Private Sub ShowTarget()
        btnRepair.Visible = False   ' a result belongs to the database and the command it was run for
        _repairCommand = String.Empty
        _repairDb = String.Empty
        Dim k_target As String = If(_context.IsAll, AdminContext.AllDatabasesCaption, _context.SelectedDb)
        Dim k_command As CommandInfo = CurrentCommand()
        'lblDesc.Text = If(k_command Is Nothing, String.Empty, k_command.Description & "   Țintă: " & k_target)
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
            If k_command.SingleDb AndAlso _context.RequireSingleDb("Comanda «" & k_command.Title & "»") = String.Empty Then Return
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
            Dim k_db As String = _context.DbArgument
            btnRepair.Visible = False
            _repairCommand = String.Empty
            _repairDb = String.Empty
            Await UiRun.Guard("CommandsView.BtnRun_Click", _context,
                Async Function()
                    _context.Say("Se rulează «" & k_command.Title & "» pe " & k_target & "...")
                    Dim k_started As DateTime = DateTime.Now
                    Dim k_result As CommandResult = Await _context.Client.CallAsync(Of CommandResult)("command", "--name", k_command.Name, "--db", k_db, "--param", k_param)
                    txtOutput.Text = RenderResult(k_result)
                    lblState.Text = k_result.Blocks.Count.ToString(CultureInfo.InvariantCulture) & " blocuri, " &
                                    (DateTime.Now - k_started).TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture) & " s."
                    ConfigureRepair(k_result, k_command.Name, k_db)
                    _context.Say("Gata.")
                End Function)
        Catch ex As Exception
            GlobalErrorLog.Write("CommandsView.BtnRun_Click", ex)
        End Try
    End Sub

    Private Sub ConfigureRepair(k_result As CommandResult, k_check As String, k_db As String)
        btnRepair.Visible = False
        _repairCommand = String.Empty
        _repairDb = String.Empty
        Dim k_current As CommandInfo = CurrentCommand()
        If _context.IsAll OrElse k_current Is Nothing OrElse k_current.Name <> k_check OrElse _context.SelectedDb <> k_db Then Return
        If k_result.Blocks.Any(Function(k_b) Not String.Equals(k_b.Db, k_db, StringComparison.OrdinalIgnoreCase)) Then Return
        Dim k_title As String
        Dim k_column As String
        Dim k_repair As String
        Select Case k_check
            Case SignCheckName
                k_title = SignBlockTitle
                k_column = RepairableColumn
                k_repair = SignRepairName
            Case StornoCheckName
                k_title = StornoBlockTitle
                k_column = "reparabile"
                k_repair = StornoRepairName
            Case Else
                Return
        End Select
        If Not HasRepairableRows(k_result, k_title, k_column) Then Return
        _repairCommand = k_repair
        _repairDb = k_db
        btnRepair.Visible = True
    End Sub

    ''' <summary>Uses the summary count, so detail truncation cannot limit the repair.</summary>
    Private Shared Function HasRepairableRows(k_result As CommandResult, k_title As String, k_columnName As String) As Boolean
        For Each k_block As CommandBlock In k_result.Blocks
            If Not k_block.Title.StartsWith(k_title, StringComparison.Ordinal) Then Continue For
            Dim k_column As Integer = k_block.Columns.IndexOf(k_columnName)
            If k_column < 0 Then Continue For
            For Each k_row As List(Of String) In k_block.Rows
                Dim k_count As Long
                If k_column < k_row.Count AndAlso Long.TryParse(k_row(k_column), NumberStyles.Integer, CultureInfo.InvariantCulture, k_count) AndAlso k_count > 0 Then Return True
            Next
        Next
        Return False
    End Function

    ''' <summary>Repairs what the selected check found in ONE database, then shows the remaining differences.</summary>
    Private Async Sub BtnRepair_Click(k_sender As Object, e As EventArgs) Handles btnRepair.Click
        Try
            Dim k_db As String = _context.RequireSingleDb("Repararea datelor verificate")
            If k_db = String.Empty Then Return
            Dim k_current As CommandInfo = CurrentCommand()
            Dim k_repair As String = _repairCommand
            If k_repair = String.Empty OrElse _repairDb <> k_db OrElse k_current Is Nothing Then Return
            Dim k_check As String = If(k_repair = StornoRepairName, StornoCheckName, SignCheckName)
            If k_current.Name <> k_check Then Return
            Dim k_explanation As String = If(k_repair = StornoRepairName,
                "Se repară toate prețurile storno și retur eligibile din baza " & k_db & ", în toate gestiunile și exercițiile: se recuperează zecimalele din prețul variantei aceleiași gestiuni. Cazurile de analizat rămân neschimbate.",
                "Se repară registrul OI al bazei " & k_db & ": rândurile cu cantitate negativă și valoare pozitivă primesc valoarea negativă.")
            If Not UiRun.Confirm(k_explanation & vbLf &
                                 "Se scrie în baza de date și rămâne în Jurnal. Continuați?", "Repară") Then Return
            btnRepair.Visible = False
            _repairCommand = String.Empty
            _repairDb = String.Empty
            Await UiRun.Guard("CommandsView.BtnRepair_Click", _context,
                Async Function()
                    _context.Say("Se repară datele verificate din baza " & k_db & "...")
                    Dim k_started As DateTime = DateTime.Now
                    Dim k_result As CommandResult = Await _context.Client.CallAsync(Of CommandResult)("command", "--name", k_repair, "--db", k_db, "--param", String.Empty)
                    txtOutput.Text = RenderResult(k_result)
                    lblState.Text = "Reparat. " & (DateTime.Now - k_started).TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture) & " s."
                    ConfigureRepair(k_result, k_check, k_db)
                    _context.Say("Gata.")
                End Function)
        Catch ex As Exception
            GlobalErrorLog.Write("CommandsView.BtnRepair_Click", ex)
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
