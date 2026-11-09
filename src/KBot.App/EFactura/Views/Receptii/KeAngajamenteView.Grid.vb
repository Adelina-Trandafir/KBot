Option Strict On
Imports System.Globalization
Imports KBot.Common
Imports KBot.Controls

' SLICE-KE21-01: what the grid of angajamente raises.
Partial Public Class KeAngajamenteView

    ' The existing rows cannot be changed (a click opens no editor, the cell is not greyed); the empty row takes a code / description; its button is hidden.
    Private Sub GridAng_CellFormatting(sender As Object, e As KBotCellFormattingEventArgs) Handles gridAng.CellFormatting
        Try
            If e.Row Is Nothing Then Return
            Dim k_real As Boolean = TypeOf e.Row.Tag Is AngItem
            If String.Equals(e.ColumnKey, ColAng, StringComparison.Ordinal) Then
                If k_real Then e.ReadOnlyCell = True
            ElseIf String.Equals(e.ColumnKey, ColOpt, StringComparison.Ordinal) Then
                ' The standard delete button: not on the empty row, not on an angajament a reception is linked to, not while an invoice already covered is only looked at.
                ' Nor on the angajament a view of an angajament is for (slice 02EF-09): it is the one the view stands on.
                If Not k_real OrElse _viewMode OrElse CType(e.Row.Tag, AngItem).Linked OrElse
                   String.Equals(CType(e.Row.Tag, AngItem).Sursa, "fix", StringComparison.Ordinal) Then e.HideButton = True
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KeAngajamenteView.GridAng_CellFormatting", ex)
        End Try
    End Sub

    ' UI boundary: the current row changed: that angajament is the one of the reception.
    Private Sub GridAng_SelectionChanged(sender As Object, e As EventArgs) Handles gridAng.SelectionChanged
        Try
            If _angFilling OrElse _viewMode Then Return
            Dim k_item As AngItem = TryCast(gridAng.CurrentRow?.Tag, AngItem)
            If k_item IsNot Nothing Then BeginUseAngajament(k_item.Cod)
        Catch ex As Exception
            GlobalErrorLog.Write("KeAngajamenteView.GridAng_SelectionChanged", ex)
        End Try
    End Sub

    ' The empty row got a text: the angajament it names is added to the rows and becomes the current one.
    Private Sub GridAng_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles gridAng.CellValueChanged
        Try
            If _angFilling OrElse Not String.Equals(e.ColumnKey, ColAng, StringComparison.Ordinal) Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= gridAng.RowCount OrElse TypeOf gridAng.Rows(e.RowIndex).Tag Is AngItem Then Return
            Dim k_text As String = Convert.ToString(e.NewValue, CultureInfo.InvariantCulture)
            If String.IsNullOrWhiteSpace(k_text) Then Return
            ' Not inside the grid's own commit: the rows are rebuilt after it.
            BeginInvoke(New Action(Sub() BeginAddAngajament(k_text.Trim())))
        Catch ex As Exception
            GlobalErrorLog.Write("KeAngajamenteView.GridAng_CellValueChanged", ex)
        End Try
    End Sub

    ' The standard delete button of a row: takes the association off (after its question). It is not drawn, and a click on it is ignored, on the empty row, on an angajament
    ' a reception is linked to, and while an invoice already covered is only looked at.
    Private Sub GridAng_ButtonClick(sender As Object, e As KBotButtonClickEventArgs) Handles gridAng.ButtonClick
        Try
            If Not String.Equals(e.ColumnKey, ColOpt, StringComparison.Ordinal) Then Return
            If _viewMode OrElse e.RowIndex < 0 OrElse e.RowIndex >= gridAng.RowCount Then Return
            Dim k_item As AngItem = TryCast(gridAng.Rows(e.RowIndex).Tag, AngItem)
            If k_item Is Nothing OrElse k_item.Linked OrElse String.Equals(k_item.Sursa, "fix", StringComparison.Ordinal) Then Return
            BeginRemoveAssociation(k_item)
        Catch ex As Exception
            GlobalErrorLog.Write("KeAngajamenteView.GridAng_ButtonClick", ex)
        End Try
    End Sub

End Class
