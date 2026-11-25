Option Strict On
Imports System.Globalization
Imports System.Text

''' <summary>
''' AVACONT: the choice of the partner (CodFiscal) of the invoices and payments command (slice ME06-01).
''' </summary>
Partial Public Class CommandsView

    ''' <summary>The command for one CodFiscal takes it from a choice box, not from the typed box: the row is shown and filled with the partners of the selected database.</summary>
    Private Async Sub ShowPartner()
        Try
            tlyPartener.Visible = True
            tlyMain.SetRowCollapsed(3, False)
            If _context.IsAll OrElse String.IsNullOrEmpty(_context.SelectedDb) Then
                lblState.Text = "Alegeți o bază în arbore: lista furnizorilor se citește din baza aleasă."
                Return
            End If
            Dim k_db As String = _context.SelectedDb
            Dim k_result As CommandResult = Await _context.Client.CallAsync(Of CommandResult)("command", "--name", PartnerListName, "--db", k_db, "--param", String.Empty).ConfigureAwait(True)
            Dim k_current As CommandInfo = CurrentCommand()
            If IsDisposed OrElse k_current Is Nothing OrElse k_current.Name <> PartnerInvoicesName OrElse _context.IsAll OrElse _context.SelectedDb <> k_db Then Return
            If k_result.Blocks.Count <> 1 OrElse Not k_result.Blocks(0).Title.StartsWith(PartnerListTitle, StringComparison.Ordinal) OrElse k_result.Blocks(0).Columns.Count < 4 Then Return
            cmbPartener.Items.Clear()
            For Each k_row As List(Of String) In k_result.Blocks(0).Rows
                If k_row.Count < 4 OrElse k_row(0).Trim() = String.Empty Then Continue For
                cmbPartener.Items.Add(New PartnerChoice(k_row(0).Trim(), k_row(1) & " | " & k_row(0).Trim() & If(k_row(3) = String.Empty, String.Empty, " | SS " & k_row(3))))
            Next
            cmbPartener.SelectedIndex = -1
            _partnerDb = k_db
            lblState.Text = cmbPartener.Items.Count.ToString(CultureInfo.InvariantCulture) & " furnizori în baza " & k_db & ". Alegeți furnizorul."
        Catch ex As Exception
            GlobalErrorLog.Write("CommandsView.ShowPartner", ex)
            lblState.Text = "Lista furnizorilor nu a putut fi citită. Detalii în jurnalul de erori."
        End Try
    End Sub

    Private Sub HidePartner()
        tlyPartener.Visible = False
        tlyMain.SetRowCollapsed(3, True)
        cmbPartener.Items.Clear()
        _partnerDb = String.Empty
    End Sub

End Class
