Option Strict On
Imports System.Text
Imports System.Text.Json.Nodes

Public Class VerconReportsView
    Implements IAdminView

    Private _context As AdminContext
    Private ReadOnly _items As New List(Of ConfigItem)()
    Private _loaded As ConfigItem
    Private _unit As String = String.Empty
    Private _version As String = String.Empty
    Private _value As JsonNode
    Private _painting As Boolean
    Private _dirty As Boolean
    Private _busy As Boolean

    Private Class ConfigItem
        Public Property Part As String
        Public Property Key As String
        Public Property Label As String
    End Class

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub Attach(k_context As AdminContext) Implements IAdminView.Attach
        _context = k_context
    End Sub

    Public Function ActivatedAsync() As Task Implements IAdminView.ActivatedAsync
        ShowContext()
        Return Task.CompletedTask
    End Function

    Public Sub SelectionChanged() Implements IAdminView.SelectionChanged
        ShowContext()
    End Sub

    Private Sub ShowContext()
        lblState.Text = If(_loaded Is Nothing, "Alegeți unitatea, apoi Citește lista.",
            "Se editează: " & _unit & " / " & _loaded.Label & If(_dirty, " — modificări nesalvate", String.Empty))
        If Not String.Equals(_unit, _context.SelectedDb, StringComparison.OrdinalIgnoreCase) AndAlso _loaded IsNot Nothing Then
            lblState.Text &= "   Unitatea selectată s-a schimbat; Citește lista pentru noua unitate."
        End If
    End Sub

    Private Function CanDiscard() As Boolean
        Return Not _dirty OrElse UiRun.Confirm("Configurarea are modificări nesalvate. Renunțați la ele?", "Rapoarte VERCON")
    End Function

    Private Async Sub BtnList_Click(k_sender As Object, e As EventArgs) Handles btnList.Click
        If _busy OrElse Not CanDiscard() Then Return
        If _context.IsAll Then
            KBotMessage.Show("Alegeți o singură unitate din arbore.", "Rapoarte VERCON", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        Dim k_db As String = _context.SelectedDb
        Await RunAsync("VerconReportsView.List", Async Function()
            Dim k_result = Await _context.Client.CallRawAsync("vercon-reports-list", "--db", k_db)
            _items.Clear()
            _items.Add(New ConfigItem With {.Part = "unit", .Key = String.Empty, .Label = "Antet / Subsol — unitate"})
            For Each k_query In k_result.GetProperty("queries").EnumerateArray()
                _items.Add(New ConfigItem With {.Part = "query", .Key = k_query.GetString(), .Label = "SQL — " & k_query.GetString() & " (sursă comună)"})
            Next
            For Each k_report In k_result.GetProperty("reports").EnumerateArray()
                _items.Add(New ConfigItem With {.Part = "layout", .Key = k_report.GetProperty("id").GetString(),
                                               .Label = "Machetă — " & k_report.GetProperty("label").GetString()})
            Next
            cmbDefinition.Items.Clear()
            For Each k_item In _items
                cmbDefinition.Items.Add(k_item.Label)
            Next
            _unit = k_result.GetProperty("unit").GetString()
            _loaded = Nothing
            _value = Nothing
            _dirty = False
            txtSql.Text = String.Empty
            pnlUnit.Visible = False
            btnSave.Enabled = False
            btnDesigner.Enabled = False
            If _items.Count > 0 Then cmbDefinition.SelectedIndex = 0
            ShowContext()
        End Function)
    End Sub

    Private Async Sub BtnRead_Click(k_sender As Object, e As EventArgs) Handles btnRead.Click
        If _busy OrElse _unit = String.Empty OrElse cmbDefinition.SelectedIndex < 0 OrElse Not CanDiscard() Then Return
        Dim k_item = _items(cmbDefinition.SelectedIndex)
        Await RunAsync("VerconReportsView.Read", Async Function()
            Dim k_result = Await _context.Client.CallRawAsync("vercon-reports-get", "--db", _unit, "--part", k_item.Part, "--key", k_item.Key)
            _version = k_result.GetProperty("version").GetString()
            _value = JsonNode.Parse(k_result.GetProperty("value").GetRawText())
            _loaded = k_item
            PaintValue()
        End Function)
    End Sub

    Private Sub PaintValue()
        _painting = True
        Try
            pnlUnit.Visible = _loaded.Part = "unit"
            txtSql.Visible = _loaded.Part <> "unit"
            txtSql.ReadOnly = _loaded.Part <> "query"
            If _loaded.Part = "query" Then
                txtSql.Text = _value.GetValue(Of String)().Replace(vbLf, vbCrLf)
                lblHelp.Text = "SELECT editabil. Tabele: {{TABLE:VC_Intrari}}, {{TABLE:VC_NIR}} etc. Parametri: %(start)s, %(end)s, %(document)s, %(variant)s, %(category)s, %(supplier)s, %(receiver)s. Contextul este aplicat de server. Salvarea nu execută SQL."
            ElseIf _loaded.Part = "unit" Then
                txtName.Text = NodeText("name")
                txtFiscal.Text = NodeText("fiscal_code")
                txtAddress.Text = NodeText("address")
                txtManager.Text = NodeText("manager")
                txtHeader.Text = NodeText("header")
                txtFooter.Text = NodeText("footer")
                lblHelp.Text = "Comun tuturor rapoartelor unității. Câmpuri disponibile: {UNITATE}, {CUI}, {ADRESA}, {GESTIUNE}, {PERIOADA}, {RAPORT}, {PAGINA}, {PAGINI}. Editorul vizual mută elementele antetului și subsolului."
            Else
                txtSql.Text = "Machetă editabilă vizual: " & NodeText("title") & vbCrLf & "Folosiți Editor vizual pentru coloane, texte, dimensiuni și poziții."
                lblHelp.Text = "Modificările machetei se salvează numai în unitatea afișată, fără modificarea programului."
            End If
            btnSave.Enabled = True
            btnDesigner.Enabled = _loaded.Part <> "query"
            _dirty = False
            ShowContext()
        Finally
            _painting = False
        End Try
    End Sub

    Private Function NodeText(k_key As String) As String
        Return If(_value(k_key)?.GetValue(Of String)(), String.Empty)
    End Function

    Private Sub ReadUnitFields()
        If _loaded.Part <> "unit" Then Return
        _value("name") = JsonValue.Create(txtName.Text)
        _value("fiscal_code") = JsonValue.Create(txtFiscal.Text)
        _value("address") = JsonValue.Create(txtAddress.Text)
        _value("manager") = JsonValue.Create(txtManager.Text)
        _value("header") = JsonValue.Create(txtHeader.Text)
        _value("footer") = JsonValue.Create(txtFooter.Text)
    End Sub

    Private Sub ValueChanged(k_sender As Object, e As EventArgs) Handles txtSql.TextChanged, txtName.TextChanged,
        txtFiscal.TextChanged, txtAddress.TextChanged, txtManager.TextChanged, txtHeader.TextChanged, txtFooter.TextChanged
        If _painting OrElse _loaded Is Nothing Then Return
        _dirty = True
        ShowContext()
    End Sub

    Private Sub BtnDesigner_Click(k_sender As Object, e As EventArgs) Handles btnDesigner.Click
        Try
            If _loaded Is Nothing OrElse _value Is Nothing OrElse _busy Then Return
            ReadUnitFields()
            Using k_form As New VerconReportDesignerForm(_value, _loaded.Part = "unit")
                If k_form.ShowDialog(Me) <> DialogResult.OK Then Return
                _value = k_form.Value.DeepClone()
                PaintValue()
                _dirty = True
                ShowContext()
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("VerconReportsView.Designer", ex)
            KBotMessage.Show(ex.Message, "Editor raport", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Async Sub BtnSave_Click(k_sender As Object, e As EventArgs) Handles btnSave.Click
        If _busy OrElse _loaded Is Nothing Then Return
        ReadUnitFields()
        If _loaded.Part = "query" Then _value = JsonValue.Create(txtSql.Text.Replace(vbCr, String.Empty))
        Dim k_payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(_value.ToJsonString()))
        Await RunAsync("VerconReportsView.Save", Async Function()
            Dim k_result = Await _context.Client.CallRawAsync("vercon-reports-save", "--db", _unit, "--part", _loaded.Part,
                "--key", _loaded.Key, "--version", _version, "--payload", k_payload)
            _version = k_result.GetProperty("version").GetString()
            _dirty = False
            ShowContext()
            _context.Say("Configurare salvată pentru " & _unit & ". Versiunea anterioară este păstrată.")
        End Function)
    End Sub

    Private Async Function RunAsync(k_name As String, k_action As Func(Of Task)) As Task
        _busy = True
        tlyBar.Enabled = False
        pnlEditor.Enabled = False
        Try
            Await UiRun.Guard(k_name, _context, k_action)
        Finally
            _busy = False
            tlyBar.Enabled = True
            pnlEditor.Enabled = True
        End Try
    End Function
End Class
