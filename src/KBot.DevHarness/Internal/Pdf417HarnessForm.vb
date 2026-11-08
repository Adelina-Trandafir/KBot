Imports System.Linq
Imports System.Text
Imports KBot.Common.Avacont
Imports ZXing
Imports ZXing.Common
Imports ZXing.PDF417
Imports ZXing.PDF417.Internal

Friend NotInheritable Class Pdf417HarnessForm
    Inherits Form

    ' Start and stop include the two-module horizontal margins in Code PDF417.
    Private Const SymbolWidth As Integer = 175
    Private Const RowHeight As Integer = 3
    Private Const ZxingMargin As Integer = 2

    Private ReadOnly _ansi As Encoding = GetAccessEncoding()
    Private ReadOnly _encoder As New ComboBox With {
        .DropDownStyle = ComboBoxStyle.DropDownList,
        .AccessibleName = "Encoder PDF417",
        .Width = 160
    }
    Private ReadOnly _input As New TextBox With {
        .Dock = DockStyle.Fill,
        .Multiline = True,
        .AcceptsReturn = True,
        .ScrollBars = ScrollBars.Vertical,
        .AccessibleName = "Text de codificat",
        .Text = "PROBA 12345678 PDF417"
    }
    Private ReadOnly _preview As New PictureBox With {
        .Dock = DockStyle.Fill,
        .BackColor = Color.White,
        .AccessibleName = "Cod de bare PDF417"
    }
    Private ReadOnly _status As New Label With {.AutoSize = True, .Margin = New Padding(3, 6, 3, 6)}
    Private ReadOnly _passed As New Button With {.AutoSize = True, .Text = "Citit corect", .DialogResult = DialogResult.OK}
    Private ReadOnly _failed As New Button With {.AutoSize = True, .Text = "Citire nereușită", .DialogResult = DialogResult.No}
    Private _rows As String() = Array.Empty(Of String)()
    Private _zxingMatrix As BitMatrix

    Public Sub New()
        Text = "PDF417 — probă AVACONT"
        StartPosition = FormStartPosition.CenterParent
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(800, 580)
        MinimumSize = New Size(580, 570)

        Dim layout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .Padding = New Padding(12),
            .ColumnCount = 1,
            .RowCount = 6
        }
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 56))
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))

        Dim instruction As New Label With {
            .AutoSize = True,
            .Text = "Tastează textul, apoi scanează codul cu un cititor PDF417 de pe telefon."
        }
        Dim encoderPanel As New FlowLayoutPanel With {.AutoSize = True, .Dock = DockStyle.Fill, .WrapContents = False}
        _encoder.Items.AddRange(New Object() {"VBA / Access", "ZXing.Net"})
        _encoder.SelectedIndex = 0
        encoderPanel.Controls.Add(New Label With {.AutoSize = True, .Text = "Encoder", .Margin = New Padding(3, 7, 6, 3)})
        encoderPanel.Controls.Add(_encoder)
        Dim buttons As New FlowLayoutPanel With {
            .AutoSize = True,
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.RightToLeft,
            .WrapContents = False
        }
        buttons.Controls.AddRange(New Control() {_failed, _passed})
        layout.Controls.Add(instruction, 0, 0)
        layout.Controls.Add(encoderPanel, 0, 1)
        layout.Controls.Add(_input, 0, 2)
        layout.Controls.Add(_status, 0, 3)
        layout.Controls.Add(_preview, 0, 4)
        layout.Controls.Add(buttons, 0, 5)
        Controls.Add(layout)

        AddHandler _input.TextChanged, AddressOf RefreshBarcode
        AddHandler _encoder.SelectedIndexChanged, AddressOf RefreshBarcode
        AddHandler _preview.Paint, AddressOf DrawBarcode
        AddHandler _preview.Resize, Sub(sender, e) _preview.Invalidate()
        RefreshBarcode(Me, EventArgs.Empty)
        ActiveControl = _input
        _input.SelectAll()
    End Sub

    Private Shared Function GetAccessEncoding() As Encoding
        Dim provider = CodePagesEncodingProvider.Instance
        Dim encoding = provider.GetEncoding(0)
        If encoding Is Nothing OrElse Not encoding.IsSingleByte Then encoding = provider.GetEncoding(1250)
        Return provider.GetEncoding(encoding.CodePage, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback)
    End Function

    Private Sub RefreshBarcode(sender As Object, e As EventArgs)
        _rows = Array.Empty(Of String)()
        _zxingMatrix = Nothing
        Try
            If _input.TextLength = 0 Then
                _status.Text = "Introdu textul de codificat."
            ElseIf _encoder.SelectedIndex = 1 Then
                RefreshZxingBarcode(_input.Text)
            Else
                Dim glyphs = Pdf417VbaEncoder.Encode(_input.Text, _ansi)
                _rows = glyphs.Split(vbCrLf, StringSplitOptions.RemoveEmptyEntries)
                _status.Text = $"{_rows.Length} rânduri · 6 coloane · corecție 3 · Windows ANSI {_ansi.CodePage}"
            End If
        Catch ex As EncoderFallbackException
            _status.Text = $"Textul conține un caracter care nu poate fi codificat în Windows ANSI {_ansi.CodePage}."
        Catch ex As WriterException
            _status.Text = "ZXing.Net nu a putut genera codul PDF417. Încearcă un text mai scurt."
        Catch ex As ArgumentException
            _status.Text = ex.Message
        End Try
        _passed.Enabled = _rows.Length > 0 OrElse _zxingMatrix IsNot Nothing
        _failed.Enabled = _passed.Enabled
        _preview.Invalidate()
    End Sub

    Private Sub RefreshZxingBarcode(text As String)
        Dim useUtf8 = text.Any(Function(character) Convert.ToInt32(character) > 127)
        Dim options As New PDF417EncodingOptions With {
            .Dimensions = New Dimensions(6, 6, 3, 90),
            .ErrorCorrection = PDF417ErrorCorrectionLevel.L3,
            .AspectRatio = PDF417AspectRatio.A3,
            .Margin = ZxingMargin
        }
        If useUtf8 Then options.CharacterSet = "UTF-8"

        Dim writer As New PDF417Writer()
        Dim matrix = writer.encode(text, BarcodeFormat.PDF_417, 0, 0, options.Hints)
        ' ZXing turns tall symbols sideways when asked for a landscape matrix.
        If matrix.Height = SymbolWidth AndAlso matrix.Width > SymbolWidth Then matrix.rotate(270)
        _zxingMatrix = matrix

        Dim rowCount = (matrix.Height - 2 * ZxingMargin) \ RowHeight
        Dim charset = If(useUtf8, "UTF-8 cu ECI", "ASCII")
        _status.Text = $"{rowCount} rânduri · 6 coloane · corecție 3 · {charset}"
    End Sub

    Private Sub DrawBarcode(sender As Object, e As PaintEventArgs)
        If _zxingMatrix IsNot Nothing Then
            DrawZxingBarcode(e.Graphics, _zxingMatrix)
            Return
        End If
        If _rows.Length = 0 Then Return
        Dim area = _preview.ClientSize
        Dim heightInModules = _rows.Length * RowHeight
        Dim scale = Math.Min((area.Width - 8) \ SymbolWidth, (area.Height - 8) \ (heightInModules + 4))
        If scale < 1 Then Return

        Dim left = (area.Width - SymbolWidth * scale) \ 2
        Dim top = (area.Height - heightInModules * scale) \ 2
        For row = 0 To _rows.Length - 1
            Dim x = left
            Dim y = top + row * RowHeight * scale
            For Each glyph In _rows(row)
                Dim width As Integer
                Dim bits As Integer
                Select Case glyph
                    Case "+"c
                        width = 18
                        bits = &B1111111101010100
                    Case "*"c
                        width = 2
                        bits = &B01
                    Case "-"c
                        width = 19
                        bits = &B1111110100010100100
                    Case "A"c To "F"c
                        width = 5
                        bits = AscW(glyph) - AscW("A"c)
                    Case Else
                        width = 5
                        bits = AscW(glyph) - AscW("a"c) + 6
                End Select
                For bit = width - 1 To 0 Step -1
                    If (bits And (1 << bit)) <> 0 Then
                        e.Graphics.FillRectangle(Brushes.Black, x, y, scale, RowHeight * scale)
                    End If
                    x += scale
                Next
            Next
        Next
    End Sub

    Private Sub DrawZxingBarcode(graphics As Graphics, matrix As BitMatrix)
        Dim area = _preview.ClientSize
        Dim scale = Math.Min((area.Width - 8) \ matrix.Width, (area.Height - 8) \ matrix.Height)
        If scale < 1 Then Return

        Dim left = (area.Width - matrix.Width * scale) \ 2
        Dim top = (area.Height - matrix.Height * scale) \ 2
        For row = 0 To matrix.Height - 1
            Dim column = 0
            While column < matrix.Width
                If Not matrix(column, row) Then
                    column += 1
                    Continue While
                End If
                Dim start = column
                Do
                    column += 1
                Loop While column < matrix.Width AndAlso matrix(column, row)
                graphics.FillRectangle(Brushes.Black, left + start * scale, top + row * scale, (column - start) * scale, scale)
            End While
        Next
    End Sub
End Class
