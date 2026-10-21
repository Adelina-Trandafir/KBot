Option Strict On

''' <summary>
''' Makes every button of a page show ALL its text (slice ME01-05). A <see cref="KBotButton"/> paints itself, so a
''' <see cref="KBotTableLayoutPanel"/> asks nothing for it (its fit skips self-painting controls) and a button
''' in a fixed column keeps the width authored in the designer, ending in «...» when the text is longer. This
''' widens such a column to what the button asks for (<c>GetPreferredSize</c>: text plus its own air) and never
''' narrows it. Widths are written through <see cref="KBotTableLayoutPanel.SetColumnWidth"/>, so they stay logical
''' and scale with the screen. Called each time a page is shown, which also covers a changed text size or screen.
''' </summary>
Public Module ButtonFit

    ''' <summary>Widens the fixed columns of every table under <paramref name="k_root"/> that hold a button wider than the column.</summary>
    Public Sub Apply(k_root As Control)
        Try
            If k_root Is Nothing Then Return
            Dim k_table As KBotTableLayoutPanel = TryCast(k_root, KBotTableLayoutPanel)
            If k_table IsNot Nothing Then FitTable(k_table)
            For Each k_child As Control In k_root.Controls
                Apply(k_child)
            Next
        Catch ex As Exception
            ' UI boundary: a page that could not be fitted still works, with the widths of its designer.
            GlobalErrorLog.Write("ButtonFit.Apply", ex)
        End Try
    End Sub

    Private Sub FitTable(k_table As KBotTableLayoutPanel)
        Dim k_scale As Single = Math.Max(0.1F, k_table.DpiScale)
        For k_col As Integer = 0 To k_table.ColumnStyles.Count - 1
            If k_table.ColumnStyles(k_col).SizeType <> SizeType.Absolute Then Continue For
            Dim k_need As Single = 0F   ' device pixels
            For k_row As Integer = 0 To k_table.RowStyles.Count - 1
                Dim k_button As KBotButton = TryCast(k_table.GetControlFromPosition(k_col, k_row), KBotButton)
                If k_button Is Nothing OrElse k_table.GetColumnSpan(k_button) <> 1 Then Continue For
                k_need = Math.Max(k_need, k_button.GetPreferredSize(Size.Empty).Width + k_button.Margin.Horizontal)
            Next
            If k_need <= 0F Then Continue For
            Dim k_logical As Single = CSng(Math.Ceiling(k_need / k_scale))
            If k_logical > k_table.DebugAuthoredColumn(k_col) Then k_table.SetColumnWidth(k_col, k_logical)
        Next
    End Sub

End Module
