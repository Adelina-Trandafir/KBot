Option Strict On
Imports System.Globalization
Imports System.Text

''' <summary>
''' Whitelisted server commands on the chosen database or on all of them (slice ME01). The list comes
''' from the server (verb <c>commands</c>); the program never sends free SQL.
''' </summary>
Public Class CommandsView
    Implements IAdminView


    Private _context As AdminContext
    Private _commands As New List(Of CommandInfo)()
    Private _repairCommand As String = String.Empty
    Private _repairDb As String = String.Empty
    Private _explainDb As String = String.Empty
    Private _fixDb As String = String.Empty
    Private _fixCode As String = String.Empty
    Private _lanturiDb As String = String.Empty
    Private _delDb As String = String.Empty
    Private _delCode As String = String.Empty
    Private _delName As String = String.Empty
    Private _bonDb As String = String.Empty
    Private _bonParam As String = String.Empty
    Private _moveDb As String = String.Empty
    Private _moveParam As String = String.Empty
    Private _moveFrom As String = String.Empty
    Private _moveTo As String = String.Empty
    Private _moveDest As String = String.Empty
    Private _fillingMove As Boolean
    Private _partnerDb As String = String.Empty
    Private _runText As String = String.Empty
    Private _oiDb As String = String.Empty
    Private _oiParam As String = String.Empty
    Private _oiToken As String = String.Empty
    Private _oiLabel As String = String.Empty

    Public Sub New()
        InitializeComponent()
        _runText = btnRun.Text
        HideBon()
        HideMove()
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
                ' _commands keeps the order of the combo's Items (SelectedIndex indexes both): by application, then by title.
                _commands = k_result.Commands.Where(Function(k_c) Not HiddenCommands.Contains(k_c.Name)).
                    OrderBy(Function(k_c) AppIndex(AppOf(k_c.Name))).
                    ThenBy(Function(k_c) k_c.Title, StringComparer.Create(CultureInfo.GetCultureInfo("ro-RO"), True)).ToList()
                cmbCommand.Items.Clear()
                For Each k_command As CommandInfo In _commands
                    cmbCommand.Items.Add(New KBotComboGroupedItem(k_command.Title & If(k_command.Writes, "  (scrie)", String.Empty), AppOf(k_command.Name), String.Empty, k_command))
                Next
                If cmbCommand.Items.Count > 0 Then cmbCommand.SelectedIndex = 0
                ShowTarget()
            End Function)
    End Function

    Private Shared Function AppOf(k_name As String) As String
        Dim k_app As String = Nothing
        Return If(CommandApps.TryGetValue(k_name, k_app), k_app, NoAppRoot)
    End Function

    Private Shared Function AppIndex(k_app As String) As Integer
        Return Array.IndexOf(AppRoots, k_app)
    End Function

    Public Sub SelectionChanged() Implements IAdminView.SelectionChanged
        ShowTarget()
    End Sub

    Private Sub ShowTarget()
        btnRepair.Visible = False   ' a result belongs to the database and the command it was run for
        _repairCommand = String.Empty
        _repairDb = String.Empty
        btnExplain.Visible = False
        _explainDb = String.Empty
        HideLanturi()
        HideFix()
        HideDelete()
        HideMove()
        HideBon()
        HideOiWipe()
        HidePartner()
        Dim k_target As String = If(_context.IsAll, AdminContext.AllDatabasesCaption, _context.SelectedDb)
        Dim k_command As CommandInfo = CurrentCommand()
        'lblDesc.Text = If(k_command Is Nothing, String.Empty, k_command.Description & "   Țintă: " & k_target)
        txtExplain.Text = If(k_command Is Nothing, String.Empty, ExplainCommand(k_command, k_target))
        ' the typed value exists only for a command that needs it; another choice in the combo makes the box disappear
        Dim k_needsParam As Boolean = k_command IsNot Nothing AndAlso k_command.Param <> String.Empty
        Dim k_choosesPartner As Boolean = k_command IsNot Nothing AndAlso k_command.Name = PartnerInvoicesName
        txtParam.Visible = k_needsParam AndAlso Not k_choosesPartner
        If k_needsParam Then txtParam.PlaceholderText = k_command.Param
        ShowSecondParam(k_command IsNot Nothing AndAlso k_command.Name = MoveListName)
        If k_choosesPartner Then ShowPartner()
    End Sub


    ''' <summary>The second typed value (the child who receives the payment) exists only for the payment move; without it the first box takes the whole width.</summary>
    Private Sub ShowSecondParam(k_visible As Boolean)
        txtParam2.Visible = k_visible
        If Not k_visible Then txtParam2.Text = String.Empty
        tlyParam.ColumnStyles(0).SizeType = SizeType.Percent
        tlyParam.ColumnStyles(0).Width = If(k_visible, 50.0F, 100.0F)
        tlyParam.ColumnStyles(1).SizeType = If(k_visible, SizeType.Percent, SizeType.Absolute)
        tlyParam.ColumnStyles(1).Width = If(k_visible, 50.0F, 0.0F)
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
            Dim k_moveDest As String = txtParam2.Text.Trim()
            If k_command.Param <> String.Empty Then
                ' a command with a typed value runs on ONE database: refuse «all databases» before anything goes to the server
                If _context.RequireSingleDb("Comanda «" & k_command.Title & "»") = String.Empty Then Return
                If k_command.Name = PartnerInvoicesName Then
                    Dim k_partner As PartnerChoice = TryCast(cmbPartener.SelectedItem, PartnerChoice)
                    If k_partner Is Nothing OrElse Not String.Equals(_partnerDb, _context.SelectedDb, StringComparison.OrdinalIgnoreCase) Then
                        KBotMessage.Show("Alegeți furnizorul din listă.", "Comenzi", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        cmbPartener.Focus()
                        Return
                    End If
                    k_param = k_partner.Key
                Else
                    k_param = txtParam.Text.Trim()
                    If k_param = String.Empty Then
                        KBotMessage.Show("Scrieți " & k_command.Param.ToLowerInvariant() & ".", "Comenzi", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        txtParam.Focus()
                        Return
                    End If
                End If
                If k_command.Name = MoveListName AndAlso k_moveDest = String.Empty Then
                    KBotMessage.Show("Scrieți CNP-ul sau ID-ul copilului care primește plata.", "Comenzi", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    txtParam2.Focus()
                    Return
                End If
            End If
            If k_command.Name = DeleteBcName Then
                Await FindBonsAsync(k_command, _context.SelectedDb, k_param)
                Return
            End If
            If k_command.Name = OiWipeAnalysisName AndAlso _oiToken <> String.Empty Then
                ' the analysis for exactly this database and this typed value found nothing against the deletion: the button now deletes
                If String.Equals(_oiDb, _context.SelectedDb, StringComparison.OrdinalIgnoreCase) AndAlso String.Equals(_oiParam, k_param, StringComparison.Ordinal) Then
                    Await RunOiWipeAsync(k_command)
                    Return
                End If
                HideOiWipe()
            End If
            Dim k_target As String = If(_context.IsAll, "TOATE bazele", "baza " & _context.SelectedDb)
            If k_command.Name = WipeCommandName Then
                ' the typed name must be the selected database, and the confirmation says plainly that nothing can be undone
                If Not String.Equals(k_param, _context.SelectedDb, StringComparison.OrdinalIgnoreCase) Then
                    KBotMessage.Show("Pentru golire scrieți în casetă exact numele bazei alese: " & _context.SelectedDb & ".", "Golire VERCON", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    txtParam.Focus()
                    Return
                End If
                If Not UiRun.Confirm("Se șterg TOATE datele VERCON din baza " & _context.SelectedDb & ": toate tabelele VC_ (documente, produse, gestiuni, exerciții, subunități, setări, numerotare) și trimiterile facturilor primite spre Vercon." & vbLf &
                                     "Partenerii, FX_, AD_ și TZ_ nu se ating. Operația NU se poate anula și rămâne în Jurnal. Continuați?", "Golire VERCON") Then Return
            ElseIf k_command.Name = AvacontWipeCommandName Then
                If Not String.Equals(k_param, _context.SelectedDb, StringComparison.OrdinalIgnoreCase) Then
                    KBotMessage.Show("Pentru golire scrieți în casetă exact numele bazei alese: " & _context.SelectedDb & ".", "Golire date contabile", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    txtParam.Focus()
                    Return
                End If
                If Not UiRun.Confirm("Se șterg TOATE datele contabile din baza " & _context.SelectedDb & ": toate tabelele TZ_ (note, documente, operații, plan de conturi, solduri, jurnale, șabloane, subunități, OP-uri, setări) și legăturile facturilor primite cu operațiile (EF_PrimiteOperatii)." & vbLf &
                                     "Partenerii, clasificațiile, FX_, AD_, VC_ și facturile primite nu se ating. Operația NU se poate anula și rămâne în Jurnal. Continuați?", "Golire date contabile") Then Return
            ElseIf k_command.Writes Then
                If Not UiRun.Confirm("Comanda «" & k_command.Title & "» scrie în " & k_target & "." & vbLf & "Continuați?", "Comenzi") Then Return
            End If
            Dim k_db As String = _context.DbArgument
            btnRepair.Visible = False
            _repairCommand = String.Empty
            _repairDb = String.Empty
            btnExplain.Visible = False
            _explainDb = String.Empty
            HideLanturi()
            HideFix()
            HideDelete()
            HideMove()
            HideOiWipe()
            txtParam.Visible = k_command.Param <> String.Empty AndAlso k_command.Name <> PartnerInvoicesName
            Await UiRun.Guard("CommandsView.BtnRun_Click", _context,
                Async Function()
                    _context.Say("Se rulează «" & k_command.Title & "» pe " & k_target & "...")
                    Dim k_started As DateTime = DateTime.Now
                    Dim k_result As CommandResult = Await _context.Client.CallAsync(Of CommandResult)("command", "--name", k_command.Name, "--db", k_db, "--param", k_param)
                    txtOutput.Text = RenderResult(k_result)
                    lblState.Text = k_result.Blocks.Count.ToString(CultureInfo.InvariantCulture) & " blocuri, " &
                                    (DateTime.Now - k_started).TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture) & " s."
                    ConfigureRepair(k_result, k_command.Name, k_db)
                    ConfigureExplain(k_result, k_command.Name, k_db)
                    ConfigureLanturi(k_result, k_command.Name, k_db)
                    ConfigureDelete(k_result, k_command.Name, k_db, k_param)
                    ConfigureMovePayments(k_result, k_command.Name, k_db, k_moveDest)
                    ConfigureOiWipe(k_result, k_command.Name, k_db, k_param)
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

End Class
