Option Strict On
Imports System.Globalization
Imports System.Text

''' <summary>
''' ADECHIT: moving one payment between children (list of payments, analysis, move).
''' </summary>
Partial Public Class CommandsView

    Private Sub HideMoveButton()
        btnMuta.Visible = False
        _moveParam = String.Empty
        _moveFrom = String.Empty
        _moveTo = String.Empty
    End Sub

    Private Sub HideMove()
        HideMoveButton()
        _fillingMove = True
        cmbPlata.Items.Clear()
        _fillingMove = False
        tlyPlata.Visible = False
        tlyMain.SetRowCollapsed(2, True)
        _moveDb = String.Empty
        _moveDest = String.Empty
    End Sub

    ''' <summary>Step 1 done: the list command returned the payments of the source child; they fill the choice list. Nothing is chosen yet and nothing is analysed.</summary>
    Private Sub ConfigureMovePayments(k_result As CommandResult, k_check As String, k_db As String, k_dest As String)
        HideMove()
        Dim k_current As CommandInfo = CurrentCommand()
        If k_check <> MoveListName OrElse _context.IsAll OrElse k_current Is Nothing OrElse k_current.Name <> k_check OrElse _context.SelectedDb <> k_db Then Return
        If k_result.Blocks.Count <> 1 OrElse Not String.Equals(k_result.Blocks(0).Db, k_db, StringComparison.OrdinalIgnoreCase) Then Return
        Dim k_block As CommandBlock = k_result.Blocks(0)
        If Not k_block.Title.StartsWith(MoveListTitle, StringComparison.Ordinal) OrElse k_block.Columns.Count < 9 OrElse k_block.Columns(0) <> "IDPL" Then Return
        _fillingMove = True
        For Each k_row As List(Of String) In k_block.Rows
            If k_row.Count < 9 OrElse k_row(0).Trim() = String.Empty Then Continue For
            cmbPlata.Items.Add(New PayChoice(k_row(0).Trim(), k_row(1) & " | " & k_row(2) & " | " & k_row(3) & " | " & k_row(4) & " lei | " & k_row(5) & " | " & k_row(6) &
                                             " | luna " & k_row(7) & If(k_row(8) = "da", " | din extras de bancă (nu se mută)", String.Empty)))
        Next
        cmbPlata.SelectedIndex = -1
        _fillingMove = False
        If cmbPlata.Items.Count = 0 Then Return
        _moveDb = k_db
        _moveDest = k_dest
        tlyPlata.Visible = True
        tlyMain.SetRowCollapsed(2, False)
        lblState.Text = cmbPlata.Items.Count.ToString(CultureInfo.InvariantCulture) & " plăți. Alegeți plata de mutat."
    End Sub

    ''' <summary>A payment was chosen: the analysis for it and the receiving child runs (writes nothing); «Mută plata» may appear.</summary>
    Private Async Sub CmbPlata_SelectedIndexChanged(k_sender As Object, e As EventArgs) Handles cmbPlata.SelectedIndexChanged
        Try
            If _fillingMove Then Return
            HideMoveButton()
            Dim k_choice As PayChoice = TryCast(cmbPlata.SelectedItem, PayChoice)
            If k_choice Is Nothing Then Return
            Dim k_db As String = _context.RequireSingleDb("Mutarea unei plăți între copii")
            If k_db = String.Empty OrElse _moveDb <> k_db OrElse _moveDest = String.Empty Then Return
            Dim k_param As String = k_choice.Key & ";" & _moveDest
            Await UiRun.Guard("CommandsView.CmbPlata_SelectedIndexChanged", _context,
                Async Function()
                    _context.Say("Se analizează plata " & k_choice.Key & "...")
                    Dim k_result As CommandResult = Await _context.Client.CallAsync(Of CommandResult)("command", "--name", MoveAnalysisName, "--db", k_db, "--param", k_param)
                    If cmbPlata.SelectedItem IsNot k_choice OrElse _moveDb <> k_db Then Return
                    txtOutput.Text = RenderResult(k_result)
                    ConfigureMove(k_result, k_db, k_param)
                    lblState.Text = If(btnMuta.Visible, "Verificați ce arată analiza și apăsați «Mută plata».", "Plata aleasă nu se poate muta: vezi motivele din rezultat.")
                    _context.Say("Gata.")
                End Function)
        Catch ex As Exception
            GlobalErrorLog.Write("CommandsView.CmbPlata_SelectedIndexChanged", ex)
        End Try
    End Sub

    ''' <summary>Step 1 done: when the analysis lists no «NU SE POATE MUTA» row, «Mută plata» appears, tied to this database and these exact two values. Changing either box hides it.</summary>
    Private Sub ConfigureMove(k_result As CommandResult, k_db As String, k_param As String)
        HideMoveButton()
        Dim k_current As CommandInfo = CurrentCommand()
        If _context.IsAll OrElse k_current Is Nothing OrElse k_current.Name <> MoveListName OrElse _context.SelectedDb <> k_db Then Return
        If k_result.Blocks.Count <> 1 OrElse Not String.Equals(k_result.Blocks(0).Db, k_db, StringComparison.OrdinalIgnoreCase) Then Return
        Dim k_block As CommandBlock = k_result.Blocks(0)
        If Not k_block.Title.StartsWith(MoveBlockTitle, StringComparison.Ordinal) OrElse k_block.Rows.Count = 0 Then Return
        If k_block.Rows.Any(Function(k_row) k_row.Count > 0 AndAlso k_row(0) = MoveBlockedMark) Then Return
        _moveDb = k_db
        _moveParam = k_param
        _moveFrom = MoveRowValue(k_block, "De la copilul")
        _moveTo = MoveRowValue(k_block, "La copilul")
        btnMuta.Visible = True
    End Sub

    Private Shared Function MoveRowValue(k_block As CommandBlock, k_label As String) As String
        For Each k_row As List(Of String) In k_block.Rows
            If k_row.Count > 1 AndAlso k_row(0) = k_label Then Return k_row(1)
        Next
        Return String.Empty
    End Function

    ''' <summary>Step 2: moves the analysed payment. The confirmation names both children; the values sent are the ones that were analysed, not whatever is in the boxes now.</summary>
    Private Async Sub BtnMuta_Click(k_sender As Object, e As EventArgs) Handles btnMuta.Click
        Try
            Dim k_db As String = _context.RequireSingleDb("Mutarea unei plăți între copii")
            If k_db = String.Empty OrElse _moveDb <> k_db OrElse _moveParam = String.Empty Then Return
            Dim k_param As String = _moveParam
            If Not UiRun.Confirm("În baza " & k_db & " se mută plata exact așa cum a arătat analiza:" & vbLf &
                                 "de la: " & _moveFrom & vbLf & "la: " & _moveTo & vbLf &
                                 "Chitanța sau documentul rămân neschimbate; dacă luna e închisă, se refac situațiile salvate ale ambilor copii. Totul într-o singură tranzacție, cu Jurnal. Continuați?", "Mutare plată") Then Return
            HideMove()
            Await UiRun.Guard("CommandsView.BtnMuta_Click", _context,
                Async Function()
                    _context.Say("Se mută plata în baza " & k_db & "...")
                    Dim k_started As DateTime = DateTime.Now
                    Dim k_result As CommandResult = Await _context.Client.CallAsync(Of CommandResult)("command", "--name", MoveName, "--db", k_db, "--param", k_param)
                    txtOutput.Text = RenderResult(k_result)
                    lblState.Text = "Plata a fost mutată. " & (DateTime.Now - k_started).TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture) & " s."
                    _context.Say("Gata.")
                End Function)
        Catch ex As Exception
            GlobalErrorLog.Write("CommandsView.BtnMuta_Click", ex)
        End Try
    End Sub

End Class
