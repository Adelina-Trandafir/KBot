Option Strict On
Imports System.Globalization
Imports System.Text

''' <summary>
''' VERCON: the bon of consum deletion (choice list of bons) and the OI/MF emptying with its confirmation token.
''' </summary>
Partial Public Class CommandsView

    Private Sub HideOiWipe()
        _oiDb = String.Empty
        _oiParam = String.Empty
        _oiToken = String.Empty
        _oiLabel = String.Empty
        If _runText <> String.Empty Then btnRun.Text = _runText
    End Sub

    ''' <summary>After the analysis of an OI/MF gestiune: when the server returned a confirmation token (nothing against the deletion), «Rulează» becomes «Rulează ștergerea», tied to this database and this typed value.</summary>
    Private Sub ConfigureOiWipe(k_result As CommandResult, k_check As String, k_db As String, k_param As String)
        HideOiWipe()
        Dim k_current As CommandInfo = CurrentCommand()
        If k_check <> OiWipeAnalysisName OrElse _context.IsAll OrElse k_current Is Nothing OrElse k_current.Name <> k_check OrElse _context.SelectedDb <> k_db Then Return
        If k_result.Blocks.Any(Function(k_b) Not String.Equals(k_b.Db, k_db, StringComparison.OrdinalIgnoreCase)) Then Return
        Dim k_block As CommandBlock = k_result.Blocks.FirstOrDefault(Function(k_b) Not String.IsNullOrEmpty(k_b.ConfirmationToken))
        If k_block Is Nothing Then Return
        _oiDb = k_db
        _oiParam = k_param
        _oiToken = k_block.ConfirmationToken
        _oiLabel = k_block.ConfirmationLabel
        btnRun.Text = OiWipeRunText
        lblState.Text = "Analiza e în regulă. Apăsați «" & OiWipeRunText & "» ca să ștergeți definitiv."
    End Sub

    ''' <summary>«Rulează ștergerea»: confirms with what the analysis said it would delete, then sends the analysis token to the deletion; ONE database, ONE gestiune.</summary>
    Private Async Function RunOiWipeAsync(k_command As CommandInfo) As Task
        Dim k_db As String = _oiDb
        Dim k_token As String = _oiToken
        If Not UiRun.Confirm("Se șterg DEFINITIV, exact ce a arătat analiza: " & _oiLabel & vbLf &
                             "Bonurile PTC (BP, BT, BS) cu registrul OI, toate consumurile (BC) și toate NIR-urile, cu pozițiile și legăturile lor (inclusiv cele cu E-Factura)." & vbLf &
                             "Facturile, nomenclatoarele și Auditul nu se ating. Operația NU se poate anula și rămâne în Jurnal. Ați făcut o copie a bazei? Continuați?", k_command.Title) Then Return
        HideOiWipe()
        Await UiRun.Guard("CommandsView.RunOiWipeAsync", _context,
            Async Function()
                _context.Say("Se șterge în baza " & k_db & "...")
                Dim k_started As DateTime = DateTime.Now
                Dim k_result As CommandResult = Await _context.Client.CallAsync(Of CommandResult)("command", "--name", OiWipeName, "--db", k_db, "--param", k_token)
                txtOutput.Text = RenderResult(k_result)
                lblState.Text = "Gestiunea a fost golită. " & (DateTime.Now - k_started).TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture) & " s. Rulați din nou analiza ca să vedeți ce a rămas."
                _context.Say("Gata.")
            End Function)
    End Function

    ''' <summary>«Rulează» on the bon deletion: finds the bons for the typed value and lists them in <c>cmbBon</c>. Deletes NOTHING; the chosen bon is deleted by «Șterge bonul».</summary>
    Private Async Function FindBonsAsync(k_command As CommandInfo, k_db As String, k_param As String) As Task
        btnRun.Enabled = False
        Try
            HideBon()
            Await UiRun.Guard("CommandsView.FindBonsAsync", _context,
                Async Function()
                    _context.Say("Se caută bonul de consum…")
                    txtOutput.Text = String.Empty
                    Dim k_preview As CommandResult = Await _context.Client.CallAsync(Of CommandResult)("command", "--name", PreviewDeleteBcName, "--db", k_db, "--param=" & k_param)
                    If CurrentCommand() IsNot k_command OrElse _context.SelectedDb <> k_db OrElse txtParam.Text.Trim() <> k_param Then Return
                    txtOutput.Text = RenderResult(k_preview)
                    Dim k_found As List(Of BonChoice) = ReadBons(k_preview, k_db)
                    If k_found.Count = 0 Then
                        lblState.Text = "Nu s-a găsit niciun bon de consum cu această valoare."
                    Else
                        ShowBons(k_found, k_db, k_param)
                        lblState.Text = If(k_found.Count = 1, "1 bon găsit. Verificați-l și apăsați «Șterge bonul».",
                                           k_found.Count.ToString(CultureInfo.InvariantCulture) & " bonuri găsite. Alegeți bonul de șters și apăsați «Șterge bonul».")
                    End If
                    _context.Say(lblState.Text)
                End Function)
        Finally
            btnRun.Enabled = True
        End Try
    End Function

    ''' <summary>The bons of the analysis result: the list block («identificator», «bon») when several matched, the single-bon block when one did. Rows without an identifier are skipped.</summary>
    Private Shared Function ReadBons(k_result As CommandResult, k_db As String) As List(Of BonChoice)
        Dim k_found As New List(Of BonChoice)()
        For Each k_block As CommandBlock In k_result.Blocks
            If Not String.Equals(k_block.Db, k_db, StringComparison.OrdinalIgnoreCase) Then Continue For
            Dim k_isList As Boolean = k_block.Columns.Count > 0 AndAlso k_block.Columns(0) = "identificator"
            For Each k_row As List(Of String) In k_block.Rows
                If k_row.Count = 0 Then Continue For
                Dim k_m As System.Text.RegularExpressions.Match = System.Text.RegularExpressions.Regex.Match(k_row(0).Trim(), "ID\s+(\d{1,10}/\d{1,10}/\d{1,10}/\d{1,10})$")
                If Not k_m.Success Then Continue For
                Dim k_caption As String = If(k_isList AndAlso k_row.Count > 1, k_row(1) & " | " & k_row(0).Trim(), k_row(0).Trim())
                k_found.Add(New BonChoice(k_m.Groups(1).Value, k_caption))
            Next
        Next
        Return k_found
    End Function

    Private Sub ShowBons(k_found As List(Of BonChoice), k_db As String, k_param As String)
        _bonDb = k_db
        _bonParam = k_param
        cmbBon.Items.Clear()
        For Each k_choice As BonChoice In k_found
            cmbBon.Items.Add(k_choice)
        Next
        ' several bons: nothing is preselected, the operator must pick the one to delete
        cmbBon.SelectedIndex = If(k_found.Count = 1, 0, -1)
        tlyBon.Visible = True
        tlyMain.SetRowCollapsed(1, False)
    End Sub

    Private Sub HideBon()
        tlyBon.Visible = False
        tlyMain.SetRowCollapsed(1, True)
        cmbBon.Items.Clear()
        _bonDb = String.Empty
        _bonParam = String.Empty
    End Sub

    ''' <summary>Deletes ONLY the bon chosen in <c>cmbBon</c>: the server analysis is repeated for that exact identifier, and the confirmation token it returns must belong to that bon.</summary>
    Private Async Sub BtnDeleteBon_Click(k_sender As Object, e As EventArgs) Handles btnDeleteBon.Click
        Try
            Dim k_db As String = _context.RequireSingleDb("Ștergerea unui bon de consum")
            If k_db = String.Empty OrElse Not String.Equals(_bonDb, k_db, StringComparison.OrdinalIgnoreCase) Then Return
            Dim k_command As CommandInfo = CurrentCommand()
            If k_command Is Nothing OrElse k_command.Name <> DeleteBcName Then Return
            Dim k_choice As BonChoice = TryCast(cmbBon.SelectedItem, BonChoice)
            If k_choice Is Nothing Then
                KBotMessage.Show("Alegeți din listă bonul de șters.", k_command.Title, MessageBoxButtons.OK, MessageBoxIcon.Information)
                cmbBon.Focus()
                Return
            End If
            btnDeleteBon.Enabled = False
            Try
                Await UiRun.Guard("CommandsView.BtnDeleteBon_Click", _context,
                    Async Function()
                        _context.Say("Se verifică bonul ales…")
                        Dim k_preview As CommandResult = Await _context.Client.CallAsync(Of CommandResult)("command", "--name", PreviewDeleteBcName, "--db", k_db, "--param=" & k_choice.Selector)
                        If CurrentCommand() IsNot k_command OrElse _context.SelectedDb <> k_db OrElse cmbBon.SelectedItem IsNot k_choice Then Return
                        txtOutput.Text = RenderResult(k_preview)
                        If k_preview.Blocks.Count <> 1 Then Return
                        Dim k_match As CommandBlock = k_preview.Blocks(0)
                        If Not String.Equals(k_match.Db, k_db, StringComparison.OrdinalIgnoreCase) Then Return
                        If String.IsNullOrEmpty(k_match.ConfirmationToken) Then
                            lblState.Text = "Bonul ales nu poate fi șters."
                            KBotMessage.Show(If(k_match.Note <> String.Empty, k_match.Note, "Bonul ales nu a mai fost găsit. Rulați din nou comanda."), k_command.Title, MessageBoxButtons.OK, MessageBoxIcon.Warning)
                            Return
                        End If
                        ' the token is built from the identifier of the bon the server found: it must be the chosen one
                        If Not k_match.ConfirmationToken.StartsWith(k_choice.Key & "/", StringComparison.Ordinal) Then
                            lblState.Text = "Serverul a găsit alt bon decât cel ales; nu s-a șters nimic."
                            Return
                        End If
                        If Not UiRun.Confirm("Se șterge definitiv bonul din baza " & k_db & "." & vbLf &
                                             k_match.ConfirmationLabel & vbLf & vbLf &
                                             "Operația este ireversibilă. Continuați?", k_command.Title) Then Return
                        Dim k_result As CommandResult = Await _context.Client.CallAsync(Of CommandResult)("command", "--name", DeleteBcName, "--db", k_db, "--param", k_match.ConfirmationToken)
                        txtOutput.Text = RenderResult(k_result)
                        Dim k_deleted As Boolean = k_result.Blocks.Any(Function(k_block) k_block.Title = DeletedBcTitle)
                        lblState.Text = If(k_deleted, DeletedBcTitle, "Ștergerea nu a fost confirmată de server.")
                        _context.Say(lblState.Text)
                        If k_deleted Then
                            ' the deleted bon leaves the list; the others stay, untouched
                            cmbBon.Items.Remove(k_choice)
                            If cmbBon.Items.Count = 0 Then HideBon() Else cmbBon.SelectedIndex = -1
                        End If
                    End Function)
            Finally
                btnDeleteBon.Enabled = True
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("CommandsView.BtnDeleteBon_Click", ex)
        End Try
    End Sub

End Class
