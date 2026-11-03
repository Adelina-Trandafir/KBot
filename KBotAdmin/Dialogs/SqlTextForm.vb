Option Strict On
Imports System.Globalization

''' <summary>The text of one backup or schema plan file, read-only, in a modal window (slice ME01).</summary>
Public Class SqlTextForm

    Public Sub New(k_name As String, k_size As Long, k_text As String, k_shownBytes As Long, k_truncated As Boolean)
        InitializeComponent()
        Icon = AdminIcon.Value
        Text = k_name
        txtSql.Text = k_text.Replace(vbCr, String.Empty).Replace(vbLf, vbCrLf)
        lblInfo.Text = k_name & "   -   " & UiRun.FormatBytes(k_size) &
                       If(k_truncated, "   -   se vede doar începutul (primii " & UiRun.FormatBytes(k_shownBytes) & ")", "   -   text complet")
    End Sub

    Private Sub BtnCopy_Click(k_sender As Object, e As EventArgs) Handles btnCopy.Click
        Try
            If txtSql.Text <> String.Empty Then Clipboard.SetText(txtSql.Text)
            lblInfo.Text = "Textul a fost copiat în clipboard (" & txtSql.Text.Length.ToString("N0", CultureInfo.InvariantCulture) & " caractere)."
        Catch ex As Exception
            GlobalErrorLog.Write("SqlTextForm.BtnCopy_Click", ex)
        End Try
    End Sub

End Class
