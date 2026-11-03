Option Strict On
Imports System.Text.Json.Nodes

Public Class VerconReportDesignerForm
    Private ReadOnly _value As JsonNode
    Private ReadOnly _unitMode As Boolean
    Private ReadOnly _nodes As New List(Of JsonNode)()
    Private ReadOnly _hits As New List(Of RectangleF)()
    Private _painting As Boolean
    Private _page As RectangleF
    Private _scale As Single
    Private _drag As Boolean
    Private _resize As Boolean
    Private _origin As Point
    Private _startX As Double
    Private _startY As Double
    Private _startW As Double
    Private _startTotal As Double

    Public Sub New(k_value As JsonNode, k_unitMode As Boolean)
        InitializeComponent()
        Icon = AdminIcon.Value
        _value = k_value.DeepClone()
        _unitMode = k_unitMode
        If _unitMode Then
            cmbBand.Items.Remove("before")
            cmbBand.Items.Remove("after")
        End If
        cmbSection.Items.Add("Antet / Subsol / Texte")
        If Not _unitMode Then
            cmbSection.Items.Add("Coloane")
            cmbSection.Items.Add("Titlu / Pagină")
            If _value("sections") IsNot Nothing Then
                For Each k_pair In _value("sections").AsObject()
                    cmbSection.Items.Add("Tabel: " & k_pair.Key)
                Next
            End If
        End If
        For Each k_token In New String() {"{UNITATE}", "{CUI}", "{ADRESA}", "{GESTIUNE}", "{PERIOADA}", "{RAPORT}", "{ANTET}", "{SUBSOL}", "{PAGINA}", "{PAGINI}"}
            cmbBinding.Items.Add(k_token)
        Next
        If _value("fields") IsNot Nothing Then
            For Each k_field In _value("fields").AsArray()
                cmbBinding.Items.Add(k_field.GetValue(Of String)())
            Next
        End If
        cmbSection.SelectedIndex = 0
    End Sub

    Public ReadOnly Property Value As JsonNode
        Get
            Return _value
        End Get
    End Property

    Private Function IsColumns() As Boolean
        Return cmbSection.SelectedIndex = 1 OrElse cmbSection.SelectedIndex >= 3
    End Function

    Private Function CurrentArray() As JsonArray
        Dim k_key = If(IsColumns(), "columns", "elements")
        Dim k_parent = _value
        If cmbSection.SelectedIndex >= 3 Then
            k_parent = _value("sections")(cmbSection.Text.Substring(7))
        End If
        If k_parent(k_key) Is Nothing Then k_parent(k_key) = New JsonArray()
        Return k_parent(k_key).AsArray()
    End Function

    Private Function Selected() As JsonNode
        Dim k_index = cmbItem.SelectedIndex
        Return If(k_index >= 0 AndAlso k_index < _nodes.Count, _nodes(k_index), Nothing)
    End Function

    Private Shared Function TextOf(k_node As JsonNode, k_key As String, Optional k_default As String = "") As String
        Return If(k_node(k_key)?.GetValue(Of String)(), k_default)
    End Function

    Private Shared Function NumberOf(k_node As JsonNode, k_key As String, k_default As Double) As Double
        Return If(k_node(k_key) Is Nothing, k_default, Double.Parse(k_node(k_key).ToJsonString(), Globalization.CultureInfo.InvariantCulture))
    End Function

    Private Sub RefreshItems(Optional k_index As Integer = 0)
        _painting = True
        _nodes.Clear()
        cmbItem.Items.Clear()
        If cmbSection.SelectedIndex = 2 Then
            _nodes.Add(_value)
            cmbItem.Items.Add("Titlu raport / Pagină")
        Else
            For Each k_node In CurrentArray()
                _nodes.Add(k_node)
                cmbItem.Items.Add(If(IsColumns(), TextOf(k_node, "label"), TextOf(k_node, "text")))
            Next
        End If
        If _nodes.Count > 0 Then cmbItem.SelectedIndex = Math.Min(k_index, _nodes.Count - 1)
        _painting = False
        PaintProperties()
    End Sub

    Private Sub SectionChanged(k_sender As Object, e As EventArgs) Handles cmbSection.SelectedIndexChanged
        If Not _painting Then RefreshItems()
    End Sub

    Private Sub ItemChanged(k_sender As Object, e As EventArgs) Handles cmbItem.SelectedIndexChanged
        If Not _painting Then PaintProperties()
    End Sub

    Private Sub PaintProperties()
        _painting = True
        Try
            Dim k_node = Selected()
            Dim k_title = cmbSection.SelectedIndex = 2
            Dim k_columns = IsColumns()
            txtCaption.Enabled = k_node IsNot Nothing
            cmbBinding.Enabled = k_node IsNot Nothing AndAlso Not k_title
            cmbType.Enabled = k_node IsNot Nothing AndAlso k_columns
            cmbBand.Enabled = k_node IsNot Nothing AndAlso Not k_columns AndAlso Not k_title
            numWidth.Enabled = k_node IsNot Nothing AndAlso Not k_title
            numWidth.Maximum = If(k_columns, 20D, 1D)
            numWidth.Minimum = If(k_columns, 0.1D, 0.01D)
            numFont.Enabled = k_node IsNot Nothing
            numFont.Minimum = If(k_title, 8D, 5D)
            numFont.Maximum = If(k_title, 40D, 36D)
            chkBold.Enabled = k_node IsNot Nothing AndAlso Not k_title
            chkLandscape.Enabled = Not _unitMode
            btnAddField.Enabled = Not _unitMode AndAlso k_columns AndAlso
                (cmbSection.SelectedIndex >= 3 OrElse _value("sections") Is Nothing OrElse _value("sections").AsObject().Count = 0)
            btnAddText.Enabled = Not k_columns AndAlso Not k_title
            btnDelete.Enabled = k_node IsNot Nothing AndAlso Not k_title
            btnLeft.Enabled = k_columns AndAlso cmbItem.SelectedIndex > 0
            btnRight.Enabled = k_columns AndAlso cmbItem.SelectedIndex < _nodes.Count - 1
            If k_node IsNot Nothing Then
                txtCaption.Text = TextOf(k_node, If(k_title, "title", If(k_columns, "label", "text")))
                cmbBinding.Text = If(k_columns, TextOf(k_node, "key"), "")
                cmbType.Text = TextOf(k_node, "type", "text")
                cmbBand.Text = TextOf(k_node, "band", "header")
                numFont.Value = CDec(NumberOf(k_node, If(k_title, "title_size", "size"), 8))
                numWidth.Value = CDec(NumberOf(k_node, If(k_columns, "width", "w"), 1))
                chkBold.Checked = If(k_node("bold")?.GetValue(Of Boolean)(), False)
            End If
            chkLandscape.Checked = If(_value("landscape")?.GetValue(Of Boolean)(), False)
        Finally
            _painting = False
            pnlCanvas.Invalidate()
        End Try
    End Sub

    Private Sub PropertyChanged(k_sender As Object, e As EventArgs) Handles txtCaption.TextChanged, cmbBinding.TextChanged,
        cmbType.SelectedIndexChanged, cmbBand.SelectedIndexChanged, numFont.ValueChanged, numWidth.ValueChanged,
        chkBold.CheckedChanged, chkLandscape.CheckedChanged
        If _painting Then Return
        Dim k_node = Selected()
        If k_node Is Nothing Then Return
        Dim k_title = cmbSection.SelectedIndex = 2
        k_node(If(k_title, "title", If(IsColumns(), "label", "text"))) = JsonValue.Create(txtCaption.Text)
        k_node(If(k_title, "title_size", "size")) = JsonValue.Create(numFont.Value)
        If Not k_title Then
            k_node("bold") = JsonValue.Create(chkBold.Checked)
            If IsColumns() Then
                k_node("key") = JsonValue.Create(cmbBinding.Text)
                k_node("type") = JsonValue.Create(cmbType.Text)
                k_node("width") = JsonValue.Create(numWidth.Value)
            Else
                k_node("band") = JsonValue.Create(cmbBand.Text)
                k_node("w") = JsonValue.Create(Math.Min(1 - NumberOf(k_node, "x", 0), CDbl(numWidth.Value)))
                If k_sender Is cmbBinding AndAlso cmbBinding.Text <> "" Then
                    _painting = True
                    txtCaption.InnerTextBox.SelectedText = cmbBinding.Text
                    _painting = False
                    k_node("text") = JsonValue.Create(txtCaption.Text)
                End If
            End If
        End If
        If Not _unitMode Then _value("landscape") = JsonValue.Create(chkLandscape.Checked)
        pnlCanvas.Invalidate()
    End Sub

    Private Sub AddField_Click(k_sender As Object, e As EventArgs) Handles btnAddField.Click
        CurrentArray().Add(New JsonObject From {{"key", JsonValue.Create("name")}, {"label", JsonValue.Create("Denumire")},
            {"type", JsonValue.Create("text")}, {"width", JsonValue.Create(1)}})
        ResetHeaders()
        RefreshItems(CurrentArray().Count - 1)
    End Sub

    Private Sub AddText_Click(k_sender As Object, e As EventArgs) Handles btnAddText.Click
        CurrentArray().Add(New JsonObject From {{"id", JsonValue.Create(Guid.NewGuid().ToString("N"))},
            {"band", JsonValue.Create("header")}, {"text", JsonValue.Create("Text nou")},
            {"x", JsonValue.Create(0)}, {"y", JsonValue.Create(20)}, {"w", JsonValue.Create(0.3)},
            {"h", JsonValue.Create(14)}, {"size", JsonValue.Create(9)}})
        RefreshItems(CurrentArray().Count - 1)
    End Sub

    Private Sub ResetHeaders()
        If Not IsColumns() Then Return
        Dim k_parent = If(cmbSection.SelectedIndex >= 3, _value("sections")(cmbSection.Text.Substring(7)), _value)
        k_parent("headers") = New JsonArray()
    End Sub

    Private Sub Delete_Click(k_sender As Object, e As EventArgs) Handles btnDelete.Click
        Dim k_index = cmbItem.SelectedIndex
        If k_index < 0 Then Return
        If IsColumns() AndAlso CurrentArray().Count = 1 Then Return
        CurrentArray().RemoveAt(k_index)
        ResetHeaders()
        RefreshItems(Math.Max(0, k_index - 1))
    End Sub

    Private Sub Move_Click(k_sender As Object, e As EventArgs) Handles btnLeft.Click, btnRight.Click
        Dim k_index = cmbItem.SelectedIndex
        Dim k_target = k_index + If(k_sender Is btnLeft, -1, 1)
        If k_target < 0 OrElse k_target >= CurrentArray().Count Then Return
        Dim k_node = CurrentArray()(k_index)
        CurrentArray().RemoveAt(k_index)
        CurrentArray().Insert(k_target, k_node)
        ResetHeaders()
        RefreshItems(k_target)
    End Sub

    Private Sub Canvas_Paint(k_sender As Object, e As PaintEventArgs) Handles pnlCanvas.Paint
        Dim k_landscape = Not _unitMode AndAlso If(_value("landscape")?.GetValue(Of Boolean)(), False)
        Dim k_width As Single = If(k_landscape, 842F, 595F)
        Dim k_height As Single = If(k_landscape, 595F, 842F)
        _scale = Math.Min((pnlCanvas.Width - 30) / k_width, (pnlCanvas.Height - 30) / k_height)
        If _scale <= 0 Then Return
        _page = New RectangleF((pnlCanvas.Width - k_width * _scale) / 2, 10, k_width * _scale, k_height * _scale)
        e.Graphics.FillRectangle(Brushes.White, _page)
        e.Graphics.DrawRectangle(Pens.Gray, Rectangle.Round(_page))
        _hits.Clear()
        Dim k_total = If(IsColumns(), _nodes.Sum(Function(k_node) NumberOf(k_node, "width", 1)), 1)
        Dim k_offset As Single = 0
        Dim k_usable = _page.Width - 52 * _scale
        For k_index As Integer = 0 To _nodes.Count - 1
            Dim k_node = _nodes(k_index)
            Dim k_rect As RectangleF
            If IsColumns() Then
                Dim k_w = CSng(NumberOf(k_node, "width", 1) / k_total) * k_usable
                k_rect = New RectangleF(_page.Left + 26 * _scale + k_offset, _page.Top + 150 * _scale, k_w, 54 * _scale)
                k_offset += k_w
            ElseIf cmbSection.SelectedIndex = 2 Then
                k_rect = New RectangleF(_page.Left + 26 * _scale, _page.Top + 115 * _scale, k_usable, 32 * _scale)
            Else
                Dim k_band = TextOf(k_node, "band", "header")
                Dim k_y As Single = If(k_band = "footer", k_height - 125, If(k_band = "before", 145, If(k_band = "after", k_height - 245, 26)))
                k_rect = New RectangleF(_page.Left + 26 * _scale + CSng(NumberOf(k_node, "x", 0)) * k_usable,
                    _page.Top + (k_y + CSng(NumberOf(k_node, "y", 0))) * _scale,
                    CSng(NumberOf(k_node, "w", 1)) * k_usable, Math.Max(16, CSng(NumberOf(k_node, "h", 14))) * _scale)
            End If
            _hits.Add(k_rect)
            e.Graphics.FillRectangle(If(k_index = cmbItem.SelectedIndex, Brushes.LightBlue, Brushes.WhiteSmoke), k_rect)
            e.Graphics.DrawRectangle(Pens.Gray, Rectangle.Round(k_rect))
            Dim k_text = TextOf(k_node, If(IsColumns(), "label", If(cmbSection.SelectedIndex = 2, "title", "text")))
            Using k_font As New Font("Arial", Math.Max(6, CSng(NumberOf(k_node, If(cmbSection.SelectedIndex = 2, "title_size", "size"), 8)) * _scale),
                If(If(k_node("bold")?.GetValue(Of Boolean)(), False), FontStyle.Bold, FontStyle.Regular), GraphicsUnit.Pixel)
                e.Graphics.DrawString(k_text, k_font, Brushes.Black, k_rect)
            End Using
        Next
    End Sub

    Private Sub Canvas_SizeChanged(k_sender As Object, e As EventArgs) Handles pnlCanvas.SizeChanged
        pnlCanvas.Invalidate()
    End Sub

    Private Sub Canvas_MouseDown(k_sender As Object, e As MouseEventArgs) Handles pnlCanvas.MouseDown
        If e.Button <> MouseButtons.Left OrElse cmbSection.SelectedIndex = 2 Then Return
        For k_index As Integer = _hits.Count - 1 To 0 Step -1
            If Not _hits(k_index).Contains(e.Location) Then Continue For
            cmbItem.SelectedIndex = k_index
            Dim k_node = Selected()
            _resize = Math.Abs(e.X - _hits(k_index).Right) < 9
            _drag = Not IsColumns() OrElse _resize
            _origin = e.Location
            _startX = NumberOf(k_node, "x", 0)
            _startY = NumberOf(k_node, "y", 0)
            _startW = NumberOf(k_node, If(IsColumns(), "width", "w"), 1)
            _startTotal = If(IsColumns(), _nodes.Sum(Function(k_item) NumberOf(k_item, "width", 1)), 1)
            pnlCanvas.Capture = _drag
            Exit For
        Next
    End Sub

    Private Sub Canvas_MouseMove(k_sender As Object, e As MouseEventArgs) Handles pnlCanvas.MouseMove
        If Not _drag OrElse _scale <= 0 Then Return
        Dim k_node = Selected()
        If _resize Then
            Dim k_delta = (e.X - _origin.X) / (_page.Width - 52 * _scale)
            Dim k_limit = If(IsColumns(), 20, 1 - NumberOf(k_node, "x", 0))
            k_node(If(IsColumns(), "width", "w")) = JsonValue.Create(Math.Max(0.1, Math.Min(k_limit, _startW + k_delta * _startTotal)))
        Else
            k_node("x") = JsonValue.Create(Math.Max(0, Math.Min(1 - NumberOf(k_node, "w", 1), _startX + (e.X - _origin.X) / (_page.Width - 52 * _scale))))
            k_node("y") = JsonValue.Create(Math.Max(0, Math.Min(100, _startY + (e.Y - _origin.Y) / _scale)))
        End If
        pnlCanvas.Invalidate()
    End Sub

    Private Sub Canvas_MouseUp(k_sender As Object, e As MouseEventArgs) Handles pnlCanvas.MouseUp
        _drag = False
        pnlCanvas.Capture = False
        PaintProperties()
    End Sub

    Private Sub Ok_Click(k_sender As Object, e As EventArgs) Handles btnOk.Click
        DialogResult = DialogResult.OK
        Close()
    End Sub
End Class
