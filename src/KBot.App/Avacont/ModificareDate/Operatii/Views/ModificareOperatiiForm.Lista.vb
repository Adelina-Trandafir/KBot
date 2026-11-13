Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

' Slice AV02-05 - «Modificare operatii»: filling the list, the icons of the columns «Tip», «Ord» and «R», the letter keys of the Access list and the
' sort remembered per user. The filters are the grid's own (one per column, all applied together).
Partial Public Class ModificareOperatiiForm

    ' Set while the page itself sorts the grid (restoring the remembered sort): that is not a choice of the operator to remember.
    Private _applyingSort As Boolean

    ' The Access keys of the list (MNotaC1.Form_KeyDown): the letter opens the filter of its column.
    Private Shared ReadOnly FilterKeys As New Dictionary(Of Keys, String) From {
        {Keys.O, "data"}, {Keys.C, "debit"}, {Keys.E, "explicatie"}, {Keys.F, "fel"}, {Keys.J, "jurnal"},
        {Keys.L, "clsf"}, {Keys.N, "nc"}, {Keys.D, "doc"}, {Keys.S, "suma"}}

    ''' <summary>The text behind the icon of the column «Tip» (the filter offers these words).</summary>
    Friend Shared Function TipText(k_fel As String) As String
        Select Case If(k_fel, String.Empty).ToUpperInvariant()
            Case "E" : Return "Evidențiere"
            Case "P" : Return "Plată"
            Case "A" : Return "Diverse"
            Case "I" : Return "Închidere"
            Case "" : Return "Manuală"
            Case Else : Return k_fel
        End Select
    End Function

    ''' <summary>The words behind the icon of the column «Ord».</summary>
    Friend Shared Function OrdText(k_state As OperatieStareOrd) As String
        Select Case k_state
            Case OperatieStareOrd.Generat : Return "Generată"
            Case OperatieStareOrd.SemnatAB : Return "Semnată AB"
            Case OperatieStareOrd.SemnatCD : Return "Semnată CD"
            Case OperatieStareOrd.Ordonator : Return "Semnată de ordonator"
            Case Else : Return String.Empty
        End Select
    End Function

    ''' <summary>The words behind the icon of the column «R».</summary>
    Friend Shared Function ReceptieText(k_state As OperatieStareReceptie) As String
        Select Case k_state
            Case OperatieStareReceptie.Toate : Return "Toate facturile cu recepție"
            Case OperatieStareReceptie.Partiala : Return "Doar unele facturi cu recepție"
            Case Else : Return String.Empty
        End Select
    End Function

    ' The operations in the default order of Access (note, date, document), then the remembered sort; the operation that was selected stays selected,
    ' or, when it is gone (deleted), the row at the same place.
    Private Sub FillGrid(k_ops As IEnumerable(Of OperatieContabila), k_keepId As Integer)
        Dim k_oldPos As Integer = gridOperatii.CurrentRowIndex
        gridOperatii.BeginUpdate()
        Try
            gridOperatii.ClearRows()
            For Each k_op As OperatieContabila In k_ops.OrderBy(Function(k_o) k_o.NumarNC).
                                                       ThenBy(Function(k_o) If(k_o.DataDocument, Date.MinValue)).
                                                       ThenBy(Function(k_o) k_o.NumarDocument, StringComparer.CurrentCultureIgnoreCase).
                                                       ThenBy(Function(k_o) k_o.IdOper)
                Dim k_row As KBotDataRow = gridOperatii.AddRow()
                k_row.Tag = k_op
                k_row("tip") = TipText(k_op.Fel)
                k_row("nc") = k_op.NumarNC
                k_row("doc") = k_op.NumarDocument
                k_row("data") = If(k_op.DataDocument.HasValue, CObj(k_op.DataDocument.Value), Nothing)
                k_row("clsf") = k_op.Clsf
                k_row("fel") = k_op.FelDocument
                k_row("jurnal") = k_op.Jurnal
                k_row("explicatie") = k_op.Explicatie
                k_row("debit") = k_op.Debit
                k_row("credit") = k_op.Credit
                k_row("suma") = k_op.Suma
                k_row("chelt") = k_op.Chelt
                k_row("cfpp") = k_op.Cfpp
                k_row("ord") = OrdText(k_op.Ord)
                k_row("rec") = ReceptieText(k_op.Receptie)
            Next
        Finally
            gridOperatii.EndUpdate()
        End Try
        ApplySavedSort()

        Dim k_index As Integer = -1
        If k_keepId > 0 Then
            For k_i As Integer = 0 To gridOperatii.RowCount - 1
                Dim k_op As OperatieContabila = TryCast(gridOperatii.Rows(k_i).Tag, OperatieContabila)
                If k_op IsNot Nothing AndAlso k_op.IdOper = k_keepId Then
                    k_index = k_i
                    Exit For
                End If
            Next
        End If
        If k_index < 0 AndAlso k_keepId > 0 AndAlso gridOperatii.RowCount > 0 Then k_index = Math.Min(Math.Max(k_oldPos, 0), gridOperatii.RowCount - 1)
        gridOperatii.CurrentRowIndex = k_index
        RefreshButtons()
        LoadInfo()
    End Sub

    ''' <summary>The operation of the selected row, or Nothing.</summary>
    Private Function SelectedOperatie() As OperatieContabila
        Dim k_row As KBotDataRow = gridOperatii.CurrentRow
        Return If(k_row Is Nothing, Nothing, TryCast(k_row.Tag, OperatieContabila))
    End Function

    Private Function SelectedIdOper() As Integer
        Dim k_op As OperatieContabila = SelectedOperatie()
        Return If(k_op Is Nothing, 0, k_op.IdOper)
    End Function

    ' ── Sort remembered per user ────────────────────────────────────────────────

    Private Sub ApplySavedSort()
        _applyingSort = True
        Try
            Dim k_key As String = AppSettings.Current.ModificareOperatiiSortKey
            If String.IsNullOrEmpty(k_key) OrElse Not gridOperatii.Columns.Any(Function(k_c) String.Equals(k_c.Key, k_key, StringComparison.Ordinal)) Then
                gridOperatii.ApplySort(Nothing, KBotSortDirection.None)
            Else
                gridOperatii.ApplySort(k_key, If(AppSettings.Current.ModificareOperatiiSortDesc, KBotSortDirection.Descending, KBotSortDirection.Ascending))
            End If
        Finally
            _applyingSort = False
        End Try
    End Sub

    Private Sub GridOperatii_SortChanged(sender As Object, e As EventArgs) Handles gridOperatii.SortChanged
        Try
            If _applyingSort Then Return
            Dim k_settings As AppSettings = AppSettings.Current
            k_settings.ModificareOperatiiSortKey = If(gridOperatii.SortDirection = KBotSortDirection.None, String.Empty, If(gridOperatii.SortColumnKey, String.Empty))
            k_settings.ModificareOperatiiSortDesc = (gridOperatii.SortDirection = KBotSortDirection.Descending)
            k_settings.Save()
        Catch ex As Exception
            GlobalErrorLog.Write("ModificareOperatiiForm.GridOperatii_SortChanged", ex)
        End Try
    End Sub

    ''' <summary>«Ordonare implicita»: back to note, date, document (and remembered so).</summary>
    Private Sub OrdonareImplicita()
        gridOperatii.ApplySort(Nothing, KBotSortDirection.None)
    End Sub

    ''' <summary>«Curata»: every filter and the sort off.</summary>
    Private Sub CurataFiltre()
        gridOperatii.ClearAllFilters()
        OrdonareImplicita()
    End Sub

    ' ── Painting and keys ───────────────────────────────────────────────────────

    Private Function IconSize() As Integer
        Return Math.Max(12, CInt(Math.Round(16 * DeviceDpi / 96.0)))
    End Function

    Private Sub GridOperatii_CellFormatting(sender As Object, e As KBotCellFormattingEventArgs) Handles gridOperatii.CellFormatting
        Try
            Dim k_op As OperatieContabila = TryCast(e.Row?.Tag, OperatieContabila)
            If k_op Is Nothing Then Return
            Dim k_palette As ThemePalette = ThemeManager.Current.Palette
            Dim k_back As Color = ThemeManager.Current.PanelBack()
            Select Case e.ColumnKey
                Case "tip"
                    e.CellImage = ModificareOperatiiIcons.TipIcon(k_op.Fel, k_palette.AccentColor, IconSize())
                Case "ord"
                    e.CellImage = ModificareOperatiiIcons.OrdIcon(k_op.Ord, k_palette.AccentColor, k_back, IconSize())
                Case "rec"
                    e.CellImage = ModificareOperatiiIcons.ReceptieIcon(k_op.Receptie, k_palette.SuccessColor, k_back, IconSize())
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("ModificareOperatiiForm.GridOperatii_CellFormatting", ex)
        End Try
    End Sub

    Private Sub GridOperatii_SelectionChanged(sender As Object, e As EventArgs) Handles gridOperatii.SelectionChanged
        Try
            RefreshButtons()
            LoadInfo()
        Catch ex As Exception
            GlobalErrorLog.Write("ModificareOperatiiForm.GridOperatii_SelectionChanged", ex)
        End Try
    End Sub

    Private Sub GridOperatii_KeyDown(sender As Object, e As KeyEventArgs) Handles gridOperatii.KeyDown
        Try
            If e.Control OrElse e.Alt Then Return
            Dim k_column As String = Nothing
            If Not e.Shift AndAlso FilterKeys.TryGetValue(e.KeyCode, k_column) Then
                e.Handled = True
                e.SuppressKeyPress = True
                gridOperatii.ShowColumnFilterMenu(k_column)
            ElseIf e.KeyCode = Keys.Delete AndAlso Not e.Shift Then
                e.Handled = True
                StergeOperatie()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("ModificareOperatiiForm.GridOperatii_KeyDown", ex)
        End Try
    End Sub

End Class
