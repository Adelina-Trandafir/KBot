Option Strict On
Imports System.Globalization
Imports System.Text

''' <summary>
''' The result of a command as aligned text, one block per database.
''' </summary>
Partial Public Class CommandsView

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
