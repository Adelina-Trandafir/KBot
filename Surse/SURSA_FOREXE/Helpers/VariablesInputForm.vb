Imports System.Drawing
Imports System.Windows.Forms
Imports WorkflowModels
Imports System.Linq
Imports Newtonsoft.Json.Linq

''' <summary>
''' Form pentru introducerea variabilelor unui workflow înainte de execuție.
''' Înlocuiește logica cu CustomInputBox iterativ din LstWorkflows_ItemCheck.
''' 
''' Utilizare:
'''   Dim f As New VariablesInputForm(item.Variables)
'''   If f.ShowDialog() = DialogResult.OK Then
'''       ' item.Variables a fost deja actualizat in-place
'''   End If
''' </summary>
Public Class VariablesInputForm
    Inherits Form

    ' -------------------------------------------------------------------------
    ' Constante vizuale
    ' -------------------------------------------------------------------------
    Private Const ICON_OK As String = "✓"
    Private Const ICON_WARN As String = "⚠"
    Private Const COLOR_OK As String = "#2E7D32"    ' verde închis
    Private Const COLOR_WARN As String = "#E65100"  ' portocaliu

    ' -------------------------------------------------------------------------
    ' Date
    ' -------------------------------------------------------------------------
    Private ReadOnly _variables As Dictionary(Of String, WorkflowVariable)
    ' Variabila curent selectată
    Private _currentVar As WorkflowVariable = Nothing

    ' -------------------------------------------------------------------------
    ' Controale UI (declarate explicit — nu folosim Designer)
    ' -------------------------------------------------------------------------
    'Private WithEvents lstVars As ListBox
    'Private pnlRight As Panel
    'Private lblVarName As Label
    'Private lblDescription As Label
    'Private lblTypeInfo As Label
    'Private lblConstraints As Label
    'Private pnlInputWrapper As Panel
    'Private WithEvents txtValue As TextBox
    'Private WithEvents mskValue As MaskedTextBox
    'Private lblError As Label
    'Private WithEvents btnSave As Button       ' Salvează valoarea în variabilă
    'Private WithEvents btnContinue As Button   ' DialogResult.OK
    'Private WithEvents btnCancel As Button     ' DialogResult.Cancel
    'Private WithEvents btnSaveVariablesToJSON As Button ' Salvează toate variabilele într-un fișier JSON (pentru testare)
    'Private WithEvents btnLoadVariablesFromJSON As Button ' Încarcă variabilele dintr-un fișier JSON (pentru testare)
    'Private lblAllStatus As Label

    Public Sub New(variables As Dictionary(Of String, WorkflowVariable))
        If variables Is Nothing Then Throw New ArgumentNullException(NameOf(variables))
        _variables = variables
        'BuildUI()
        InitializeComponent()
        PopulateList()
        UpdateContinueButton()

        ' Selectăm prima variabilă necomplete, sau prima dacă toate sunt complete
        Dim firstIncomplete = _variables.Values.FirstOrDefault(Function(v) String.IsNullOrEmpty(v.Value))
        If firstIncomplete IsNot Nothing Then
            lstVars.SelectedIndex = _variables.Values.ToList().IndexOf(firstIncomplete)
        ElseIf lstVars.Items.Count > 0 Then
            lstVars.SelectedIndex = 0
        End If

        KBotTheme.ApplyTheme(Me)
    End Sub

    ' =========================================================================
    ' CONSTRUIRE UI
    ' =========================================================================
    'Private Sub BuildUI()
    '    Me.Text = "Parametri workflow"
    '    Me.Size = New Size(720, 460)
    '    Me.MinimumSize = New Size(560, 380)
    '    Me.StartPosition = FormStartPosition.CenterParent
    '    Me.FormBorderStyle = FormBorderStyle.Sizable
    '    Me.Font = New Font("Segoe UI", 9.5F)
    '    Me.BackColor = Color.FromArgb(245, 245, 245)

    '    ' ----- Panou stânga: lista variabilelor -----
    '    Dim pnlLeft As New Panel With {
    '        .Dock = DockStyle.Left,
    '        .Width = 240,
    '        .Padding = New Padding(8)
    '    }

    '    Dim lblListTitle As New Label With {
    '        .Text = "Variabile",
    '        .Dock = DockStyle.Top,
    '        .Height = 26,
    '        .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold),
    '        .ForeColor = Color.FromArgb(55, 71, 79)
    '    }

    '    lstVars = New ListBox With {
    '        .Dock = DockStyle.Fill,
    '        .DrawMode = DrawMode.OwnerDrawFixed,
    '        .ItemHeight = 28,
    '        .BorderStyle = BorderStyle.FixedSingle,
    '        .Font = New Font("Segoe UI", 9.5F),
    '        .BackColor = Color.White
    '    }
    '    AddHandler lstVars.DrawItem, AddressOf LstVars_DrawItem

    '    pnlLeft.Controls.Add(lstVars)
    '    pnlLeft.Controls.Add(lblListTitle)

    '    ' ----- Separator vertical -----
    '    Dim splitter As New Panel With {
    '        .Dock = DockStyle.Left,
    '        .Width = 1,
    '        .BackColor = Color.FromArgb(200, 200, 200)
    '    }

    '    ' ----- Panou dreapta: editor -----
    '    pnlRight = New Panel With {
    '        .Dock = DockStyle.Fill,
    '        .Padding = New Padding(16, 12, 16, 12)
    '    }

    '    lblVarName = New Label With {
    '        .Dock = DockStyle.Top,
    '        .Height = 30,
    '        .Font = New Font("Segoe UI", 12.0F, FontStyle.Bold),
    '        .ForeColor = Color.FromArgb(33, 33, 33),
    '        .Text = "Selectați o variabilă"
    '    }

    '    lblDescription = New Label With {
    '        .Dock = DockStyle.Top,
    '        .Height = 22,
    '        .Font = New Font("Segoe UI", 9.0F, FontStyle.Italic),
    '        .ForeColor = Color.FromArgb(100, 100, 100),
    '        .Text = ""
    '    }

    '    lblTypeInfo = New Label With {
    '        .Dock = DockStyle.Top,
    '        .Height = 20,
    '        .Font = New Font("Segoe UI", 8.5F),
    '        .ForeColor = Color.FromArgb(80, 80, 120),
    '        .Text = ""
    '    }

    '    lblConstraints = New Label With {
    '        .Dock = DockStyle.Top,
    '        .Height = 20,
    '        .Font = New Font("Segoe UI", 8.5F),
    '        .ForeColor = Color.FromArgb(80, 80, 80),
    '        .Text = ""
    '    }

    '    ' Wrapper pentru input (conține fie TextBox fie MaskedTextBox)
    '    pnlInputWrapper = New Panel With {
    '        .Dock = DockStyle.Top,
    '        .Height = 36,
    '        .Padding = New Padding(0, 4, 0, 0)
    '    }

    '    txtValue = New TextBox With {
    '        .Dock = DockStyle.Fill,
    '        .Font = New Font("Segoe UI", 10.0F),
    '        .Visible = False
    '    }

    '    mskValue = New MaskedTextBox With {
    '        .Dock = DockStyle.Fill,
    '        .Font = New Font("Segoe UI", 10.0F),
    '        .Visible = False
    '    }

    '    pnlInputWrapper.Controls.Add(txtValue)
    '    pnlInputWrapper.Controls.Add(mskValue)

    '    lblError = New Label With {
    '        .Dock = DockStyle.Top,
    '        .Height = 20,
    '        .Font = New Font("Segoe UI", 8.5F),
    '        .ForeColor = Color.FromArgb(198, 40, 40),
    '        .Text = "",
    '        .Visible = False
    '    }

    '    ' Buton Salvează
    '    btnSave = New Button With {
    '        .Dock = DockStyle.Top,
    '        .Height = 32,
    '        .Text = "Salvează valoarea  ↩",
    '        .BackColor = Color.FromArgb(25, 118, 210),
    '        .ForeColor = Color.White,
    '        .FlatStyle = FlatStyle.Flat,
    '        .Enabled = False,
    '        .Margin = New Padding(0, 8, 0, 0)
    '    }
    '    btnSave.FlatAppearance.BorderSize = 0

    '    ' Buton Salvează toate variabilele într-un fișier JSON (pentru testare)
    '    btnSaveVariablesToJSON = New Button With {
    '        .Dock = DockStyle.Top,
    '        .Height = 32,
    '        .Text = "Salvează toate variabilele într-un fișier JSON (pentru testare)",
    '        .BackColor = Color.FromArgb(100, 181, 246),
    '        .ForeColor = Color.White,
    '        .FlatStyle = FlatStyle.Flat,
    '        .Margin = New Padding(0, 8, 0, 0)
    '    }
    '    btnSaveVariablesToJSON.FlatAppearance.BorderSize = 0
    '    AddHandler btnSaveVariablesToJSON.Click, Sub(s, e)
    '                                                 Dim sfd As New SaveFileDialog With {
    '                                                     .Filter = "Fișier JSON|*.json",
    '                                                     .Title = "Salvează variabilele într-un fișier JSON"
    '                                                 }
    '                                                 Dim jsonObj As New JObject()

    '                                                 If sfd.ShowDialog() = DialogResult.OK Then
    '                                                     Try
    '                                                         For Each kvp In _variables
    '                                                             jsonObj(kvp.Key) = kvp.Value.Value
    '                                                         Next

    '                                                     Catch ex As LogException
    '                                                         MessageBox.Show("Eroare la serializarea variabilelor în JSON: " & ex.Message, "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '                                                     End Try

    '                                                     System.IO.File.WriteAllText(sfd.FileName, jsonObj.ToString())
    '                                                     MessageBox.Show("Variabilele au fost salvate cu succes!", "Salvare reușită", MessageBoxButtons.OK, MessageBoxIcon.Information)
    '                                                 End If
    '                                             End Sub

    '    btnLoadVariablesFromJSON = New Button With {
    '        .Dock = DockStyle.Top,
    '        .Height = 32,
    '        .Text = "Încarcă variabilele dintr-un fișier JSON (pentru testare)",
    '        .BackColor = Color.FromArgb(100, 181, 246),
    '        .ForeColor = Color.White,
    '        .FlatStyle = FlatStyle.Flat,
    '        .Margin = New Padding(0, 8, 0, 0)
    '    }
    '    btnLoadVariablesFromJSON.FlatAppearance.BorderSize = 0

    '    AddHandler btnLoadVariablesFromJSON.Click , Sub(s, e)
    '                                                 Dim ofd As New OpenFileDialog With {
    '                                                     .Filter = "Fișier JSON|*.json",
    '                                                     .Title = "Încarcă variabilele dintr-un fișier JSON"
    '                                                 }
    '                                                 If ofd.ShowDialog() = DialogResult.OK Then
    '                                                     Try
    '                                                         Dim jsonText = System.IO.File.ReadAllText(ofd.FileName)
    '                                                         Dim jsonObj = JObject.Parse(jsonText)
    '                                                         For Each prop As JProperty In jsonObj.Properties()
    '                                                             If _variables.ContainsKey(prop.Name) Then
    '                                                                 _variables(prop.Name).Value = prop.Value.ToString()
    '                                                             End If
    '                                                         Next
    '                                                         PopulateList()
    '                                                         UpdateContinueButton()
    '                                                         MessageBox.Show("Variabilele au fost încărcate cu succes!", "Încărcare reușită", MessageBoxButtons.OK, MessageBoxIcon.Information)
    '                                                     Catch ex As LogException
    '                                                         MessageBox.Show("Eroare la încărcarea fișierului JSON: " & ex.Message, "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '                                                     End Try
    '                                                 End If
    '                                             End Sub
    '    ' Spacer flexibil
    '    Dim spacer As New Panel With {.Dock = DockStyle.Fill}

    '    ' ----- Bara de jos cu butoane -----
    '    Dim pnlBottom As New Panel With {
    '        .Dock = DockStyle.Bottom,
    '        .Height = 48,
    '        .Padding = New Padding(8, 8, 8, 8)
    '    }

    '    lblAllStatus = New Label With {
    '        .Dock = DockStyle.Left,
    '        .AutoSize = False,
    '        .Width = 200,
    '        .TextAlign = ContentAlignment.MiddleLeft,
    '        .Font = New Font("Segoe UI", 8.5F),
    '        .ForeColor = Color.FromArgb(100, 100, 100)
    '    }

    '    btnCancel = New Button With {
    '        .Text = "Renunță",
    '        .Dock = DockStyle.Right,
    '        .Width = 90,
    '        .FlatStyle = FlatStyle.Flat,
    '        .BackColor = Color.FromArgb(240, 240, 240),
    '        .ForeColor = Color.FromArgb(60, 60, 60)
    '    }
    '    btnCancel.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180)

    '    btnContinue = New Button With {
    '        .Text = "Continuă  ▶",
    '        .Dock = DockStyle.Right,
    '        .Width = 100,
    '        .FlatStyle = FlatStyle.Flat,
    '        .BackColor = Color.FromArgb(46, 125, 50),
    '        .ForeColor = Color.White,
    '        .Enabled = False
    '    }
    '    btnContinue.FlatAppearance.BorderSize = 0

    '    pnlBottom.Controls.Add(lblAllStatus)
    '    pnlBottom.Controls.Add(btnCancel)
    '    pnlBottom.Controls.Add(btnContinue)

    '    ' Adaugă controalele în pnlRight (Dock Top se stivuiește de sus în jos)
    '    ' Ordinea adăugării e inversă față de ordinea vizuală cu Dock=Top
    '    pnlRight.Controls.Add(spacer)
    '    pnlRight.Controls.Add(btnSave)
    '    pnlRight.Controls.Add(lblError)
    '    pnlRight.Controls.Add(pnlInputWrapper)
    '    pnlRight.Controls.Add(lblConstraints)
    '    pnlRight.Controls.Add(lblTypeInfo)
    '    pnlRight.Controls.Add(lblDescription)
    '    pnlRight.Controls.Add(lblVarName)

    '    Me.Controls.Add(pnlBottom)
    '    Me.Controls.Add(pnlRight)
    '    Me.Controls.Add(splitter)
    '    Me.Controls.Add(pnlLeft)

    '    Me.AcceptButton = btnSave
    '    Me.CancelButton = btnCancel
    'End Sub

    ' =========================================================================
    ' POPULARE LISTBOX
    ' =========================================================================
    Private Sub PopulateList()
        lstVars.Items.Clear()
        For Each kvp In _variables
            lstVars.Items.Add(kvp.Value)  ' stocăm WorkflowVariable ca item
        Next
    End Sub

    ' =========================================================================
    ' DESENARE CUSTOM LISTBOX (iconiță + nume + stare)
    ' =========================================================================
    Private Sub LstVars_DrawItem(sender As Object, e As DrawItemEventArgs) Handles lstVars.DrawItem
        If e.Index < 0 OrElse e.Index >= lstVars.Items.Count Then Return

        Dim v = DirectCast(lstVars.Items(e.Index), WorkflowVariable)
        Dim isSelected = (e.State And DrawItemState.Selected) <> 0
        Dim isComplete = Not String.IsNullOrEmpty(v.Value)

        ' Fundal
        Dim bgColor As Color
        If isSelected Then
            bgColor = Color.FromArgb(227, 242, 253)
        Else
            bgColor = If(e.Index Mod 2 = 0, Color.White, Color.FromArgb(250, 250, 250))
        End If
        e.Graphics.FillRectangle(New SolidBrush(bgColor), e.Bounds)

        ' Bara laterală colorată (stare)
        Dim stateColor = If(isComplete,
                            Color.FromArgb(46, 125, 50),                                     ' verde = completat
                            If(v.IsRequired, Color.FromArgb(198, 40, 40),                    ' roșu = obligatoriu gol
                                             Color.FromArgb(150, 150, 150)))                 ' gri = opțional gol
        e.Graphics.FillRectangle(New SolidBrush(stateColor), New Rectangle(e.Bounds.X, e.Bounds.Y + 2, 3, e.Bounds.Height - 4))

        ' Iconiță stare
        Dim icon = If(isComplete, ICON_OK, ICON_WARN)
        Dim iconColor = If(isComplete, Color.FromArgb(46, 125, 50), Color.FromArgb(230, 81, 0))
        Using iconFont = New Font("Segoe UI", 9.0F)
            Using iconBrush = New SolidBrush(iconColor)
                e.Graphics.DrawString(icon, iconFont, iconBrush, New PointF(e.Bounds.X + 8, e.Bounds.Y + 6))
            End Using
        End Using

        ' Nume variabilă
        Dim textColor = If(isSelected, Color.FromArgb(13, 71, 161), Color.FromArgb(33, 33, 33))
        Using nameBrush = New SolidBrush(textColor)
            e.Graphics.DrawString(v.Name, lstVars.Font, nameBrush, New PointF(e.Bounds.X + 26, e.Bounds.Y + 5))
        End Using

        ' Valoare curentă (mică, gri)
        If isComplete Then
            Dim previewText = $"= {v.Value}"
            If previewText.Length > 24 Then previewText = previewText.Substring(0, 22) & "…"
            Using previewFont = New Font("Segoe UI", 7.5F)
                Using previewBrush = New SolidBrush(Color.FromArgb(120, 120, 120))
                    Dim nameSize = e.Graphics.MeasureString(v.Name, lstVars.Font)
                    e.Graphics.DrawString(previewText, previewFont, previewBrush,
                                          New PointF(e.Bounds.X + 26 + nameSize.Width + 4, e.Bounds.Y + 8))
                End Using
            End Using
        End If

        e.DrawFocusRectangle()
    End Sub

    ' =========================================================================
    ' SELECTARE VARIABILĂ DIN LISTĂ
    ' =========================================================================
    Private Sub LstVars_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstVars.SelectedIndexChanged
        If lstVars.SelectedIndex < 0 Then
            _currentVar = Nothing
            ClearEditor()
            Return
        End If

        _currentVar = DirectCast(lstVars.SelectedItem, WorkflowVariable)
        LoadVariableInEditor(_currentVar)
    End Sub

    Private Sub ClearEditor()
        lblVarName.Text = "Selectați o variabilă"
        lblDescription.Text = ""
        lblTypeInfo.Text = ""
        lblConstraints.Text = ""
        txtValue.Visible = False
        mskValue.Visible = False
        lblError.Visible = False
        btnSave.Enabled = False
    End Sub

    Private Sub LoadVariableInEditor(v As WorkflowVariable)
        lblVarName.Text = v.Name
        lblDescription.Text = If(String.IsNullOrEmpty(v.Description), "", v.Description)
        lblError.Text = ""
        lblError.Visible = False

        ' Tip și constrângeri
        Dim typeText = $"Tip: {If(v.VarType = "Numeric", "Numeric", "Text")}"
        If v.Length > 0 Then typeText &= $"  |  Lungime: {v.Length}"
        lblTypeInfo.Text = typeText

        Dim constraints As New List(Of String)
        If Not String.IsNullOrEmpty(v.Mask) Then constraints.Add($"Mască: {v.Mask}")
        If v.Min.HasValue Then constraints.Add($"Min: {v.Min.Value}")
        If v.Max.HasValue Then constraints.Add($"Max: {v.Max.Value}")
        lblConstraints.Text = If(constraints.Count > 0, String.Join("   ", constraints), "")

        ' Input: mască sau text simplu
        If Not String.IsNullOrEmpty(v.Mask) Then
            mskValue.Mask = v.Mask
            mskValue.TextMaskFormat = MaskFormat.IncludeLiterals
            mskValue.Text = v.Value
            mskValue.Visible = True
            txtValue.Visible = False

        ElseIf v.VarType.Equals("JSON", StringComparison.OrdinalIgnoreCase) Then
            ' TextBox multiline — mai ușor de editat JSON
            txtValue.Multiline = True
            txtValue.AcceptsReturn = True ' Adauga linia asta pt a permite Enter in text
            txtValue.ScrollBars = ScrollBars.Vertical
            txtValue.Height = 120
            txtValue.Font = New Font("Courier New", 8.5F)
            txtValue.Text = If(String.IsNullOrEmpty(v.Value),
                               "[" & Environment.NewLine &
                               "  { ""cod"": """", ""valoare"": """" }" & Environment.NewLine &
                               "]",
                               v.Value)
            pnlInputWrapper.Height = 130
            txtValue.Visible = True
            mskValue.Visible = False
        Else
            txtValue.Text = v.Value
            If v.VarType.Equals("Numeric", StringComparison.OrdinalIgnoreCase) Then
                txtValue.ImeMode = ImeMode.Disable
            End If
            If v.Length > 0 AndAlso String.IsNullOrEmpty(v.Mask) Then
                txtValue.MaxLength = v.Length
            Else
                txtValue.MaxLength = 32767
            End If
            txtValue.Visible = True
            mskValue.Visible = False
            txtValue.Multiline = False
            txtValue.AcceptsReturn = False
            txtValue.ScrollBars = ScrollBars.None
            txtValue.Height = 34
            pnlInputWrapper.Height = 44
        End If

        btnSave.Enabled = True

        ' Focus pe input activ
        If mskValue.Visible Then mskValue.Focus() Else txtValue.Focus()
    End Sub

    ' =========================================================================
    ' BUTON SALVEAZĂ
    ' =========================================================================
    Private Sub BtnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click

        If _currentVar Is Nothing Then Return
        Dim idx = lstVars.SelectedIndex
        If idx < 0 Then Return

        Dim inputValue As String
        If mskValue.Visible Then
            inputValue = mskValue.Text.Trim
        Else
            inputValue = txtValue.Text.Trim
        End If

        If _currentVar.VarType.Equals("JSON", StringComparison.OrdinalIgnoreCase) Then
            Try
                JArray.Parse(inputValue)
            Catch ex As Exception
                lblError.Text = "JSON invalid: " & ex.Message
                lblError.Visible = True
                Return
            End Try
        End If

        ' Validare
        Dim errMsg = ValidateValue(_currentVar, inputValue)
        If Not String.IsNullOrEmpty(errMsg) Then
            lblError.Text = errMsg
            lblError.Visible = True
            If mskValue.Visible Then mskValue.Focus() Else txtValue.Focus()
            Return
        End If

        ' Salvăm
        lblError.Visible = False
        _currentVar.Value = inputValue

        ' Refresh lista (re-desenare item)
        lstVars.Invalidate(lstVars.GetItemRectangle(idx))

        UpdateContinueButton()

        ' Auto-avansăm la următoarea variabilă necompletă
        Dim nextIncomplete = _variables.Values.Skip(idx + 1).
                             FirstOrDefault(Function(v) String.IsNullOrEmpty(v.Value))
        If nextIncomplete IsNot Nothing Then
            lstVars.SelectedIndex = _variables.Values.ToList.IndexOf(nextIncomplete)
        End If
    End Sub

    ' =========================================================================
    ' VALIDARE
    ' =========================================================================
    Private Function ValidateValue(v As WorkflowVariable, value As String) As String
        If String.IsNullOrEmpty(value) Then Return "Câmpul nu poate fi gol."

        If v.VarType.Equals("Numeric", StringComparison.OrdinalIgnoreCase) Then
            Dim n As Double
            If Not Double.TryParse(value.Replace(",", "."),
                                   Globalization.NumberStyles.Any,
                                   Globalization.CultureInfo.InvariantCulture, n) Then
                Return "Introduceți o valoare numerică validă."
            End If
            If v.Min.HasValue AndAlso n < v.Min.Value Then
                Return $"Valoarea minimă acceptată este {v.Min.Value}."
            End If
            If v.Max.HasValue AndAlso n > v.Max.Value Then
                Return $"Valoarea maximă acceptată este {v.Max.Value}."
            End If
        Else
            If v.Length > 0 AndAlso value.Length > v.Length Then
                Return $"Textul depășește lungimea maximă de {v.Length} caractere."
            End If
        End If

        Return String.Empty
    End Function

    ' =========================================================================
    ' ACTUALIZARE BUTON CONTINUĂ
    ' =========================================================================
    Private Sub UpdateContinueButton()
        'Dim allFilled = _variables.Values.All(Function(v) Not String.IsNullOrEmpty(v.Value))
        Dim allFilled = _variables.Values.All(Function(v) Not v.IsRequired OrElse Not String.IsNullOrEmpty(v.Value))

        btnContinue.Enabled = allFilled

        Dim filled = System.Linq.Enumerable.Count(
                 _variables.Values,
                 Function(v) Not String.IsNullOrEmpty(v.Value))

        Dim total = _variables.Count
        lblAllStatus.Text = $"{filled}/{total} parametri completați"
        lblAllStatus.ForeColor = If(allFilled, Color.FromArgb(46, 125, 50), Color.FromArgb(150, 80, 0))
    End Sub

    ' =========================================================================
    ' BUTOANE CONTINUĂ / RENUNȚĂ
    ' =========================================================================
    Private Sub BtnContinue_Click(sender As Object, e As EventArgs) Handles btnContinue.Click
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub BtnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    ' =========================================================================
    ' TASTE RAPIDE: Enter în input => Salvează, Tab => variabila următoare
    ' =========================================================================
    Private Sub TxtValue_KeyDown(sender As Object, e As KeyEventArgs) Handles txtValue.KeyDown
        If e.KeyCode = Keys.Enter AndAlso _currentVar.VarType <> "JSON" Then
            BtnSave_Click(Nothing, EventArgs.Empty)
            e.SuppressKeyPress = True
        ElseIf e.KeyCode = Keys.Enter AndAlso _currentVar.VarType = "JSON" Then
            ' Pentru JSON permitem Enter (multiline), dar dacă se apasă Ctrl+Enter atunci salvăm
            If e.Control Then
                BtnSave_Click(Nothing, EventArgs.Empty)
                e.SuppressKeyPress = True
            End If
        End If
    End Sub

    Private Sub MskValue_KeyDown(sender As Object, e As KeyEventArgs) Handles mskValue.KeyDown
        If e.KeyCode = Keys.Enter AndAlso _currentVar.VarType <> "JSON" Then
            BtnSave_Click(Nothing, EventArgs.Empty)
            e.SuppressKeyPress = True
        ElseIf e.KeyCode = Keys.Enter AndAlso _currentVar.VarType = "JSON" Then
            ' Pentru JSON permitem Enter (multiline), dar dacă se apasă Ctrl+Enter atunci salvăm
            If e.Control Then
                BtnSave_Click(Nothing, EventArgs.Empty)
                e.SuppressKeyPress = True
            End If
        End If
    End Sub

    ' Blocare caractere non-numerice pentru câmpuri Numeric fără mască
    Private Sub TxtValue_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtValue.KeyPress
        If _currentVar Is Nothing Then Return
        If _currentVar.VarType.Equals("Numeric", StringComparison.OrdinalIgnoreCase) AndAlso
           String.IsNullOrEmpty(_currentVar.Mask) Then
            If Not Char.IsDigit(e.KeyChar) AndAlso Not Char.IsControl(e.KeyChar) AndAlso
               e.KeyChar <> "."c AndAlso e.KeyChar <> ","c Then
                e.Handled = True
            End If
        End If
    End Sub

    Private Sub BtnLoadVariablesFromJSON_Click() Handles btnLoadVariablesFromJSON.Click
        Dim ofd As New OpenFileDialog With {
                                            .Filter = "Fișier JSON|*.json",
                                                .Title = "Încarcă variabilele dintr-un fișier JSON"
                                            }
        If ofd.ShowDialog = DialogResult.OK Then
            Try
                Dim jsonText = IO.File.ReadAllText(ofd.FileName)
                Dim jsonObj = JObject.Parse(jsonText)
                For Each prop In jsonObj.Properties
                    If _variables.ContainsKey(prop.Name) Then
                        _variables(prop.Name).Value = prop.Value.ToString
                    End If
                Next
                PopulateList()
                UpdateContinueButton()

                'MessageBox.Show("Variabilele au fost încărcate cu succes!", "Încărcare reușită", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Catch ex As Exception
                MessageBox.Show("Eroare la încărcarea fișierului JSON: " & ex.Message, "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End If
    End Sub

    Private Sub BtnSaveVariablesToJSON_Click() Handles btnSaveVariablesToJSON.Click
        Dim sfd As New SaveFileDialog With {
            .Filter = "Fișier JSON|*.json",
            .Title = "Salvează variabilele într-un fișier JSON"
        }
        Dim jsonObj As New JObject

        If sfd.ShowDialog = DialogResult.OK Then
            Try
                For Each kvp In _variables
                    jsonObj(kvp.Key) = kvp.Value.Value
                Next

            Catch ex As Exception
                MessageBox.Show("Eroare la serializarea variabilelor în JSON: " & ex.Message, "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try

            IO.File.WriteAllText(sfd.FileName, jsonObj.ToString)
            MessageBox.Show("Variabilele au fost salvate cu succes!", "Salvare reușită", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub
End Class