Option Strict On
Imports System.Globalization
Imports System.IO
Imports System.Text

''' <summary>
''' The content of one cell of the page «Tabele», read-only, in a modal window: the whole text of a long cell, or a blob as a hex dump
''' (the first <see cref="DumpBytes"/> bytes; the file saved with «Salvează ca fișier» holds all the bytes the server sent).
''' </summary>
Public Class CellContentForm

    ''' <summary>How many bytes of a blob the hex dump shows (the dump is text: a long one would freeze the box).</summary>
    Public Const DumpBytes As Integer = 65536

    Private ReadOnly _bytes As Byte()
    Private ReadOnly _text As String
    Private ReadOnly _fileName As String

    ''' <summary>A text cell: <paramref name="k_text"/> is shown as it is and saved as UTF-8.</summary>
    Public Sub New(k_title As String, k_info As String, k_text As String, k_fileName As String)
        InitializeComponent()
        Icon = AdminIcon.Value
        Text = k_title
        _text = k_text
        _fileName = k_fileName
        lblInfo.Text = k_info
        txtContent.Text = k_text.Replace(vbCr, String.Empty).Replace(vbLf, vbCrLf)
        txtContent.WordWrap = True
    End Sub

    ''' <summary>A blob cell: the hex dump of its first bytes is shown, all of <paramref name="k_bytes"/> is saved.</summary>
    Public Sub New(k_title As String, k_info As String, k_bytes As Byte(), k_fileName As String)
        InitializeComponent()
        Icon = AdminIcon.Value
        Text = k_title
        _bytes = k_bytes
        _fileName = k_fileName
        lblInfo.Text = k_info
        txtContent.Text = HexDump(k_bytes, DumpBytes)
    End Sub

    ''' <summary>Offset, sixteen bytes in hex, and the same bytes as characters (a dot for what cannot be read).</summary>
    Public Shared Function HexDump(k_bytes As Byte(), k_max As Integer) As String
        Dim k_count As Integer = Math.Min(k_bytes.Length, k_max)
        Dim k_sb As New StringBuilder(k_count * 4 + 64)
        For k_start As Integer = 0 To k_count - 1 Step 16
            k_sb.Append(k_start.ToString("X8", CultureInfo.InvariantCulture)).Append("  ")
            Dim k_ascii As New StringBuilder(16)
            For k_i As Integer = 0 To 15
                If k_start + k_i < k_count Then
                    Dim k_b As Byte = k_bytes(k_start + k_i)
                    k_sb.Append(k_b.ToString("X2", CultureInfo.InvariantCulture)).Append(" "c)
                    k_ascii.Append(If(k_b >= 32 AndAlso k_b < 127, ChrW(k_b), "."c))
                Else
                    k_sb.Append("   ")
                End If
                If k_i = 7 Then k_sb.Append(" "c)
            Next
            k_sb.Append(" |").Append(k_ascii.ToString()).AppendLine("|")
        Next
        Return k_sb.ToString()
    End Function

    Private Sub BtnCopy_Click(k_sender As Object, e As EventArgs) Handles btnCopy.Click
        Try
            If txtContent.Text <> String.Empty Then Clipboard.SetText(txtContent.Text)
            lblInfo.Text = "Copiat în clipboard (" & txtContent.Text.Length.ToString("N0", CultureInfo.CurrentCulture) & " caractere)."
        Catch ex As Exception
            GlobalErrorLog.Write("CellContentForm.BtnCopy_Click", ex)
        End Try
    End Sub

    Private Sub BtnSave_Click(k_sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            Using k_dialog As New SaveFileDialog With {.FileName = _fileName, .Title = "Salvează conținutul celulei"}
                If k_dialog.ShowDialog(Me) <> DialogResult.OK Then Return
                If _bytes IsNot Nothing Then
                    File.WriteAllBytes(k_dialog.FileName, _bytes)
                Else
                    File.WriteAllText(k_dialog.FileName, _text, New UTF8Encoding(False))
                End If
                lblInfo.Text = "Salvat în " & k_dialog.FileName
            End Using
        Catch ex As Exception
            ' UI boundary: shown and logged, never rethrown.
            GlobalErrorLog.Write("CellContentForm.BtnSave_Click", ex)
            KBotMessage.Show("Fișierul nu a putut fi salvat: " & ex.Message, "Conținut celulă", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class
