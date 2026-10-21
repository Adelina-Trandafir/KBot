Imports System.Globalization
Imports KBot.Common

''' <summary>
''' The receipts that share their series and number, in a grid where the number is edited by hand. After every change the whole list
''' is checked again, so a number that is already taken shows up at once. The migration plan is rebuilt by the caller on OK.
''' Every control is declared in the designer file; this class only wires behaviour.
''' </summary>
Public Class AdeReceiptsForm

    Private ReadOnly _receipts As List(Of AdeReceipt)
    Private ReadOnly _shown As List(Of AdeReceipt)
    Private ReadOnly _edits As Dictionary(Of Integer, Integer)

    Public Sub New(k_receipts As IEnumerable(Of AdeReceipt), k_edits As IDictionary(Of Integer, Integer))
        InitializeComponent()
        _receipts = k_receipts.ToList()
        _shown = AdeReceiptNumbers.Duplicated(_receipts)
        _edits = New Dictionary(Of Integer, Integer)(k_edits)
    End Sub

    ''' <summary>The numbers changed so far, per IDC in Access; the caller reads it after OK.</summary>
    Public ReadOnly Property Edits As Dictionary(Of Integer, Integer)
        Get
            Return _edits
        End Get
    End Property

    Private Sub AdeReceiptsForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            dgvChitante.BeginUpdate()
            Try
                dgvChitante.ClearRows()
                For Each k_receipt In _shown
                    Dim k_row = dgvChitante.AddRow()
                    k_row.Tag = k_receipt
                    k_row("idc") = k_receipt.Id.ToString(CultureInfo.InvariantCulture)
                    k_row("idpl") = If(k_receipt.PaymentId.HasValue, k_receipt.PaymentId.Value.ToString(CultureInfo.InvariantCulture), String.Empty)
                    k_row("data") = If(k_receipt.IssuedOn.HasValue, k_receipt.IssuedOn.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), String.Empty)
                    k_row("serie") = k_receipt.Series
                    k_row("anulata") = If(k_receipt.Cancelled, "da", "nu")
                Next
            Finally
                dgvChitante.EndUpdate()
            End Try
            ShowState(String.Empty)
        Catch ex As Exception
            Fail("AdeReceiptsForm.Load", "Lista chitanțelor nu a putut fi afișată.", ex)
        End Try
    End Sub

    ''' <summary>Writes the current numbers and the status of every row, and says how many are still doubled.</summary>
    Private Sub ShowState(k_prefix As String)
        Dim k_groups = AdeReceiptNumbers.Groups(_receipts, _edits)
        For k_index As Integer = 0 To dgvChitante.Rows.Count - 1
            Dim k_receipt = TryCast(dgvChitante.Rows(k_index).Tag, AdeReceipt)
            If k_receipt Is Nothing Then Continue For
            Dim k_number = AdeReceiptNumbers.Current(k_receipt, _edits)
            dgvChitante.Item("numar", k_index) = If(k_number.HasValue, k_number.Value.ToString(CultureInfo.InvariantCulture), String.Empty)
            dgvChitante.Item("stare", k_index) = AdeReceiptNumbers.Status(k_receipt, k_groups, _edits)
        Next
        Dim k_still = _shown.Where(Function(k_r)
                                       Dim k_n = AdeReceiptNumbers.Current(k_r, _edits)
                                       Return k_r.Series IsNot Nothing AndAlso k_n.HasValue AndAlso k_groups(AdeReceiptNumbers.GroupKey(k_r.Series, k_n.Value)).Count > 1
                                   End Function).Count()
        Dim k_summary = If(k_still = 0, "Nicio dublură rămasă; apăsați «Gata».", $"{k_still} chitanțe sunt încă în dublu.") &
                        $" Numere schimbate: {_edits.Count}."
        lblStare.Text = If(k_prefix.Length > 0, k_prefix & " ", String.Empty) & k_summary
    End Sub

    Private Sub DgvChitante_CellValueChanged(sender As Object, e As KBot.Controls.KBotCellValueEventArgs) Handles dgvChitante.CellValueChanged
        Try
            If e.ColumnKey <> "numar" Then Return
            Dim k_receipt = TryCast(dgvChitante.Rows(e.RowIndex).Tag, AdeReceipt)
            If k_receipt Is Nothing Then Return
            Dim k_number As Integer
            Dim k_text As String = Convert.ToString(e.NewValue, CultureInfo.InvariantCulture)
            If k_text Is Nothing OrElse Not Integer.TryParse(k_text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, k_number) OrElse k_number <= 0 Then
                ' The old number comes back; a suffix such as «.1» does not fit the number column of the database.
                ShowState("Numărul trebuie să fie un număr întreg pozitiv, fără zecimale sau puncte.")
                Return
            End If
            If k_receipt.Number.HasValue AndAlso k_number = k_receipt.Number.Value Then
                _edits.Remove(k_receipt.Id)
            Else
                _edits(k_receipt.Id) = k_number
            End If
            ShowState(String.Empty)
        Catch ex As Exception
            Fail("AdeReceiptsForm.CellValueChanged", "Numărul nu a putut fi verificat.", ex)
        End Try
    End Sub

    Private Sub BtnRegula_Click(sender As Object, e As EventArgs) Handles btnRegula.Click
        Try
            Dim k_result = AdeReceiptNumbers.ApplyOlderRule(_receipts, _edits)
            ShowState($"Regula: {k_result.Changed} numere schimbate, {k_result.Skipped} chitanțe nu s-au putut schimba (numărul fără cifra miilor este deja luat sau numărul are sub 1000).")
        Catch ex As Exception
            Fail("AdeReceiptsForm.Rule", "Regula nu a putut fi aplicată.", ex)
        End Try
    End Sub

    Private Sub BtnReset_Click(sender As Object, e As EventArgs) Handles btnReset.Click
        Try
            _edits.Clear()
            ShowState("Numerele din Access au fost puse la loc.")
        Catch ex As Exception
            Fail("AdeReceiptsForm.Reset", "Schimbările nu au putut fi anulate.", ex)
        End Try
    End Sub

    Private Sub Fail(k_context As String, k_message As String, k_error As Exception)
        GlobalErrorLog.Write(k_context, k_error)
        KBotMessage.Show(k_message & Environment.NewLine & Environment.NewLine & k_error.Message,
                         "Migrare ADE", MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub

End Class
