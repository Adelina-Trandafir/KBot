Option Strict On
Imports System.IO
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' The tutorial designer (slice 000T-03), an operator tool shown only in capture mode: the steps on
''' the left, the step being edited on the right (a property grid + the bubble text), «Alege pe ecran...»
''' to fill the target from the live application, «Salveaza», and «Testeaza de la pasul» that runs the
''' tutorial as it is now. Reads and writes the same files the help reads (<see cref="TutorialStore"/>).
''' It is NOT described in the help.
''' </summary>
Public Class TutorialDesignerForm

    Private Const NewFlowItem As String = "(tutorial nou)"

    Private ReadOnly _service As HelpService
    Private _flow As TutorialFlow
    Private _loading As Boolean
    Private _dirty As Boolean

    Private Shared _instance As TutorialDesignerForm

    Public Sub New(k_service As HelpService)
        InitializeComponent()
        _service = k_service
        Try
            _colorResetting = True
            cmbHtmlColor.SelectedIndex = 0
            _colorResetting = False
            RefreshFlowList()
            NewFlow()
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialDesignerForm.New", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Shows the one designer window (a second call brings the first to the front).</summary>
    Public Shared Sub ShowFor(k_owner As IWin32Window, k_service As HelpService)
        If _instance Is Nothing OrElse _instance.IsDisposed Then _instance = New TutorialDesignerForm(k_service)
        If Not _instance.Visible Then _instance.Show(k_owner)
        If _instance.WindowState = FormWindowState.Minimized Then _instance.WindowState = FormWindowState.Normal
        _instance.BringToFront()
    End Sub

    ' ── flows ────────────────────────────────────────────────────────────────────

    Private Sub RefreshFlowList()
        _loading = True
        Try
            cmbFlows.Items.Clear()
            cmbFlows.Items.Add(NewFlowItem)
            Dim folder As String = TutorialStore.RuntimeFolder()
            If Directory.Exists(folder) Then
                For Each k_file As String In Directory.EnumerateFiles(folder, "*.md").OrderBy(Function(f) f, StringComparer.OrdinalIgnoreCase)
                    cmbFlows.Items.Add(Path.GetFileNameWithoutExtension(k_file))
                Next
            End If
            cmbFlows.SelectedIndex = 0
        Finally
            _loading = False
        End Try
    End Sub

    Private Sub NewFlow()
        _flow = New TutorialFlow With {.Id = "tutorial-nou", .Title = "Tutorial nou", .Starts = "KbotForm"}
        _flow.Steps.Add(New TutorialStep With {.Title = "Primul pas"})
        _dirty = False
        BindFlow()
    End Sub

    Private Sub BindFlow()
        pgFlow.SelectedObject = _flow
        FillSteps(0)
    End Sub

    Private Sub CmbFlows_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbFlows.SelectedIndexChanged
        Try
            If _loading OrElse cmbFlows.SelectedIndex < 0 Then Return
            If _dirty AndAlso KBotMessage.Show(Me, "Ai schimbări nesalvate în tutorialul de acum. Le pierzi dacă treci la altul. Continui?",
                                               "Designer tutoriale", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
                _loading = True
                cmbFlows.SelectedItem = If(cmbFlows.Items.Contains(_flow.Id), CObj(_flow.Id), CObj(NewFlowItem))
                _loading = False
                Return
            End If
            Dim k_name As String = cmbFlows.SelectedItem.ToString()
            If k_name = NewFlowItem Then
                NewFlow()
            Else
                _flow = TutorialFlow.ParseFile(Path.Combine(TutorialStore.RuntimeFolder(), k_name & ".md"))
                _dirty = False
                BindFlow()
            End If
        Catch ex As Exception
            ' UI boundary (event handler): log and tell the operator; the old tutorial stays.
            GlobalErrorLog.Write("TutorialDesignerForm.CmbFlows_SelectedIndexChanged", ex)
            KBotMessage.Show(Me, "Tutorialul nu a putut fi citit: " & ex.Message, "Designer tutoriale", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ' ── steps ────────────────────────────────────────────────────────────────────

    Private ReadOnly Property CurrentStep As TutorialStep
        Get
            Return If(lstSteps.SelectedIndex >= 0 AndAlso lstSteps.SelectedIndex < _flow.Steps.Count, _flow.Steps(lstSteps.SelectedIndex), Nothing)
        End Get
    End Property

    Private Shared Function LineFor(k_index As Integer, k_step As TutorialStep) As String
        Return (k_index + 1) & ". " & k_step.Title & If(k_step.IsOptional, "  (opțional)", String.Empty)
    End Function

    Private Sub FillSteps(k_select As Integer)
        _loading = True
        Try
            lstSteps.Items.Clear()
            For i As Integer = 0 To _flow.Steps.Count - 1
                lstSteps.Items.Add(LineFor(i, _flow.Steps(i)))
            Next
            If _flow.Steps.Count > 0 Then lstSteps.SelectedIndex = Math.Max(0, Math.Min(k_select, _flow.Steps.Count - 1))
        Finally
            _loading = False
        End Try
        ShowStep()
    End Sub

    Private Sub ShowStep()
        _loading = True
        Try
            Dim st As TutorialStep = CurrentStep
            pgStep.SelectedObject = st
            txtText.Text = If(st Is Nothing, String.Empty, st.Text.Replace(vbLf, vbCrLf))
            txtText.Enabled = st IsNot Nothing
            tlpHtmlTools.Enabled = st IsNot Nothing
            btnPick.Enabled = st IsNot Nothing
        Finally
            _loading = False
        End Try
    End Sub

    Private Sub LstSteps_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstSteps.SelectedIndexChanged
        If Not _loading Then ShowStep()
    End Sub

    Private Sub PgStep_PropertyValueChanged(s As Object, e As PropertyValueChangedEventArgs) Handles pgStep.PropertyValueChanged
        Try
            _dirty = True
            Dim at As Integer = lstSteps.SelectedIndex
            If at >= 0 AndAlso CurrentStep IsNot Nothing Then
                _loading = True
                lstSteps.Items(at) = LineFor(at, CurrentStep)
                _loading = False
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialDesignerForm.PgStep_PropertyValueChanged", ex)
        End Try
    End Sub

    Private Sub PgFlow_PropertyValueChanged(s As Object, e As PropertyValueChangedEventArgs) Handles pgFlow.PropertyValueChanged
        _dirty = True
    End Sub

    Private Sub TxtText_TextChanged(sender As Object, e As EventArgs) Handles txtText.TextChanged
        Try
            ' The preview follows every change, the load of a step included.
            lblPreview.Html = txtText.Text
            If _loading OrElse CurrentStep Is Nothing Then Return
            CurrentStep.Text = txtText.Text.Replace(vbCrLf, vbLf)
            _dirty = True
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialDesignerForm.TxtText_TextChanged", ex)
        End Try
    End Sub

    ' ── the HTML helpers (slice 000T-06) ─────────────────────────────────────────

    ' Same order as the items of cmbHtmlColor. The values are text written INTO the tutorial (a theme word follows the
    ' scheme on the user's screen; a fixed colour is content, not a colour of this window).
    Private Shared ReadOnly HtmlColorValues As String() = {String.Empty, "accent", "dim", "warning", "error", "success", "#c00000", "#1a7f37", "#0b5cad", "#c25e00"}
    Private _colorResetting As Boolean

    ' The selection goes between the two tags and stays selected (no selection: the caret lands between them).
    Private Sub WrapSelection(k_open As String, k_close As String)
        Dim k_start As Integer = txtText.SelectionStart
        Dim k_selected As String = txtText.SelectedText
        txtText.SelectedText = k_open & k_selected & k_close
        txtText.Select(k_start + k_open.Length, k_selected.Length)
        txtText.Focus()
    End Sub

    Private Sub BtnHtmlBold_Click(sender As Object, e As EventArgs) Handles btnHtmlBold.Click
        WrapSelection("<b>", "</b>")
    End Sub

    Private Sub BtnHtmlItalic_Click(sender As Object, e As EventArgs) Handles btnHtmlItalic.Click
        WrapSelection("<i>", "</i>")
    End Sub

    Private Sub BtnHtmlUnderline_Click(sender As Object, e As EventArgs) Handles btnHtmlUnderline.Click
        WrapSelection("<u>", "</u>")
    End Sub

    Private Sub BtnHtmlMark_Click(sender As Object, e As EventArgs) Handles btnHtmlMark.Click
        WrapSelection("<mark>", "</mark>")
    End Sub

    Private Sub BtnHtmlHeading_Click(sender As Object, e As EventArgs) Handles btnHtmlHeading.Click
        WrapSelection("<h3>", "</h3>")
    End Sub

    Private Sub BtnHtmlBreak_Click(sender As Object, e As EventArgs) Handles btnHtmlBreak.Click
        txtText.SelectedText = "<br>"
        txtText.Focus()
    End Sub

    ' Every non-empty line of the selection becomes an item; nothing selected: a list with one empty item.
    Private Sub BtnHtmlList_Click(sender As Object, e As EventArgs) Handles btnHtmlList.Click
        Dim k_items As List(Of String) = txtText.SelectedText.Replace(vbCrLf, vbLf).Split(ChrW(10)).
            Select(Function(k_line) k_line.Trim()).Where(Function(k_line) k_line.Length > 0).ToList()
        If k_items.Count = 0 Then
            Dim k_start As Integer = txtText.SelectionStart
            txtText.SelectedText = "<ul><li></li></ul>"
            txtText.Select(k_start + "<ul><li>".Length, 0)
        Else
            txtText.SelectedText = "<ul>" & vbCrLf & String.Join(vbCrLf, k_items.Select(Function(k_item) "<li>" & k_item & "</li>")) & vbCrLf & "</ul>"
        End If
        txtText.Focus()
    End Sub

    Private Sub CmbHtmlColor_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbHtmlColor.SelectedIndexChanged
        Try
            If _colorResetting OrElse cmbHtmlColor.SelectedIndex <= 0 Then Return
            WrapSelection("<span style=""color:" & HtmlColorValues(cmbHtmlColor.SelectedIndex) & """>", "</span>")
            ' Back to the «Culoare...» heading: the box is a menu, not a state.
            _colorResetting = True
            cmbHtmlColor.SelectedIndex = 0
            _colorResetting = False
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialDesignerForm.CmbHtmlColor_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Sub BtnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click
        Dim at As Integer = Math.Min(_flow.Steps.Count, lstSteps.SelectedIndex + 1)
        _flow.Steps.Insert(at, New TutorialStep With {.Title = "Pas nou"})
        _dirty = True
        FillSteps(at)
    End Sub

    Private Sub BtnDel_Click(sender As Object, e As EventArgs) Handles btnDel.Click
        Dim at As Integer = lstSteps.SelectedIndex
        If at < 0 Then Return
        If KBotMessage.Show(Me, "Ștergi pasul «" & _flow.Steps(at).Title & "»?", "Designer tutoriale",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
        _flow.Steps.RemoveAt(at)
        _dirty = True
        FillSteps(at)
    End Sub

    Private Sub BtnUp_Click(sender As Object, e As EventArgs) Handles btnUp.Click
        MoveStep(-1)
    End Sub

    Private Sub BtnDown_Click(sender As Object, e As EventArgs) Handles btnDown.Click
        MoveStep(1)
    End Sub

    Private Sub MoveStep(k_by As Integer)
        Dim at As Integer = lstSteps.SelectedIndex
        Dim k_to As Integer = at + k_by
        If at < 0 OrElse k_to < 0 OrElse k_to >= _flow.Steps.Count Then Return
        Dim st As TutorialStep = _flow.Steps(at)
        _flow.Steps.RemoveAt(at)
        _flow.Steps.Insert(k_to, st)
        _dirty = True
        FillSteps(k_to)
    End Sub

    Private Sub BtnDup_Click(sender As Object, e As EventArgs) Handles btnDup.Click
        Dim at As Integer = lstSteps.SelectedIndex
        If at < 0 Then Return
        Dim src As TutorialStep = _flow.Steps(at)
        Dim copy As New TutorialStep With {
            .Title = src.Title & " (copie)", .Target = src.Target, .Part = src.Part, .Anchor = src.Anchor,
            .WaitKind = src.WaitKind, .WaitArg = src.WaitArg, .WhenKind = src.WhenKind, .WhenArg = src.WhenArg,
            .IsOptional = src.IsOptional, .Merge = src.Merge, .Why = src.Why, .DimRest = src.DimRest, .Text = src.Text}
        copy.Allow.AddRange(src.Allow)
        _flow.Steps.Insert(at + 1, copy)
        _dirty = True
        FillSteps(at + 1)
    End Sub

    ' ── the picker ───────────────────────────────────────────────────────────────

    Private Sub BtnPick_Click(sender As Object, e As EventArgs) Handles btnPick.Click
        Dim st As TutorialStep = CurrentStep
        If st Is Nothing Then Return
        Dim pick As TutorialPick = Nothing
        Try
            ' The designer steps out of the way: it would be picked instead of the application.
            Hide()
            pick = TutorialPicker.Pick()
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialDesignerForm.BtnPick_Click", ex)
        Finally
            Show()
            Activate()
        End Try
        If pick Is Nothing Then Return
        If Not pick.HasStableName Then
            KBotMessage.Show(Me, "Controlul ales nu are un nume stabil (" & pick.Target & "): un tutorial nu poate arăta spre el după nume. Alege o zonă mai mare sau dă-i un nume în designer.",
                             "Designer tutoriale", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        st.Target = pick.Target
        st.Part = pick.Part
        If st.WaitKind = TutorialWaitKind.Manual Then
            st.WaitKind = pick.SuggestedWait
            st.WaitArg = pick.SuggestedWaitArg
        End If
        _dirty = True
        pgStep.Refresh()
    End Sub

    ' ── the recorder ─────────────────────────────────────────────────────────────

    Private Sub BtnInregistreaza_Click(sender As Object, e As EventArgs) Handles btnInregistreaza.Click
        Try
            WindowState = FormWindowState.Minimized
            TutorialRecorder.Start(AddressOf RecordingDone)
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialDesignerForm.BtnInregistreaza_Click", ex)
            WindowState = FormWindowState.Normal
            KBotMessage.Show(Me, "Înregistrarea nu a putut porni. Detalii în jurnalul de erori.", "Designer tutoriale", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' «Opreste» on the bar: the recorded steps go in after the chosen step, ready to be given texts.
    Private Sub RecordingDone(k_steps As List(Of TutorialStep))
        Try
            If IsDisposed Then Return
            WindowState = FormWindowState.Normal
            Activate()
            If k_steps.Count = 0 Then
                KBotMessage.Show(Me, "Nu s-a înregistrat niciun pas.", "Designer tutoriale", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Dim at As Integer = Math.Min(_flow.Steps.Count, lstSteps.SelectedIndex + 1)
            _flow.Steps.InsertRange(at, k_steps)
            _dirty = True
            FillSteps(at)
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialDesignerForm.RecordingDone", ex)
        End Try
    End Sub

    ' ── save and test ────────────────────────────────────────────────────────────

    ' A tutorial the parser would refuse is told here, not at a user's screen.
    Private Function Check() As Boolean
        Try
            TutorialFlow.Parse(_flow.ToMarkdown())
            Return True
        Catch ex As ArgumentException
            KBotMessage.Show(Me, "Tutorialul nu e în regulă: " & ex.Message, "Designer tutoriale", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End Try
    End Function

    Private Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            If Not Check() Then Return
            Dim written As List(Of String) = TutorialStore.Save(_flow)
            _service.ReloadLibrary()
            _dirty = False
            Dim keep As String = _flow.Id
            RefreshFlowList()
            _loading = True
            cmbFlows.SelectedItem = keep
            _loading = False
            KBotMessage.Show(Me, "Salvat:" & vbLf & String.Join(vbLf, written), "Designer tutoriale", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialDesignerForm.BtnSalveaza_Click", ex)
            KBotMessage.Show(Me, "Tutorialul nu a putut fi salvat. Detalii în jurnalul de erori.", "Designer tutoriale", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Deletes the tutorial open now, from every folder it was saved to (Recycle Bin). A tutorial that was never saved
    ' has no file: the button then only drops what is on screen.
    Private Sub BtnSterge_Click(sender As Object, e As EventArgs) Handles btnSterge.Click
        Try
            ' The file chosen in the list, not _flow.Id: the id may have been edited in the grid and then names another tutorial.
            Dim k_id As String = If(cmbFlows.SelectedIndex > 0, cmbFlows.SelectedItem.ToString(), String.Empty)
            Dim k_files As List(Of String) = If(k_id.Length > 0, TutorialStore.FilesOf(k_id), New List(Of String)())
            Dim k_ask As String
            If k_files.Count = 0 Then
                k_ask = "Tutorialul «" & _flow.Title & "» nu e salvat nicăieri. Renunți la ce ai pe ecran?"
            Else
                k_ask = "Ștergi tutorialul «" & _flow.Title & "»?" & vbLf & vbLf & String.Join(vbLf, k_files) & vbLf & vbLf &
                        "Fișierele merg în Coșul de reciclare."
            End If
            If KBotMessage.Show(Me, k_ask, "Designer tutoriale", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Return
            If k_files.Count > 0 Then
                ' Other tutorials pointing here would be left with a dead link: tell, and ask again.
                Dim k_links As List(Of String) = TutorialStore.LinksTo(k_id)
                If k_links.Count > 0 AndAlso KBotMessage.Show(Me,
                        "Aceste tutoriale au o legătură către «" & _flow.Title & "»:" & vbLf & vbLf & String.Join(vbLf, k_links) & vbLf & vbLf &
                        "După ștergere, legătura rămâne în text și un clic pe ea nu mai pornește nimic. Ștergi tutorialul oricum?",
                        "Designer tutoriale", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Return
                TutorialStore.Delete(k_id)
                _service.ReloadLibrary()
            End If
            RefreshFlowList()
            NewFlow()
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialDesignerForm.BtnSterge_Click", ex)
            KBotMessage.Show(Me, "Tutorialul nu a putut fi șters. Detalii în jurnalul de erori.", "Designer tutoriale", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub BtnTesteaza_Click(sender As Object, e As EventArgs) Handles btnTesteaza.Click
        Try
            If Not Check() Then Return
            Dim k_from As Integer = Math.Max(0, lstSteps.SelectedIndex)
            ' What is tested is what is on screen now (unsaved), read back through the real parser.
            Dim k_copy As TutorialFlow = TutorialFlow.Parse(_flow.ToMarkdown())
            k_copy.Mandatory = False   ' a test run must always be possible to leave
            WindowState = FormWindowState.Minimized
            TutorialRunner.Start(_service, k_copy,
                                 Sub()
                                     If Not IsDisposed Then WindowState = FormWindowState.Normal
                                 End Sub, k_from)
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialDesignerForm.BtnTesteaza_Click", ex)
            WindowState = FormWindowState.Normal
            KBotMessage.Show(Me, "Tutorialul nu a putut porni. Detalii în jurnalul de erori.", "Designer tutoriale", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub BtnInchide_Click(sender As Object, e As EventArgs) Handles btnInchide.Click
        Close()
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        MyBase.OnFormClosing(e)
        Try
            If _dirty AndAlso Not e.Cancel AndAlso Visible AndAlso
               KBotMessage.Show(Me, "Ai schimbări nesalvate. Închizi fără să le salvezi?", "Designer tutoriale",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
                e.Cancel = True
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialDesignerForm.OnFormClosing", ex)
        End Try
    End Sub

End Class
