Option Strict On
Imports System.Diagnostics
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' Debug bench for every message box of K-BOT. The grid (left) lists the catalog
''' (<c>Config\mesaje_catalog.json</c>, produced by <c>tools\MessageCatalog\scan.js</c>): type,
''' buttons, extra button, caption, text and the FUNCTION the call lives in. The panel on the
''' right edits the selected message -- type, button set, one extra button with its own text,
''' caption and the message itself -- and «Previzualizează» opens the real
''' <see cref="KBotMessageBoxForm"/> with exactly that. «Salvează în JSON» writes the file back.
'''
''' <para>The catalog is an inventory and an editing surface: the calls in the code still carry
''' their own text. Texts with {parts} are built at run time; keep the braces when editing.</para>
'''
''' <para>Every handler is a UI boundary: log and swallow.</para>
''' </summary>
Public NotInheritable Class MessageCatalogForm

    Private ReadOnly _path As String
    Private _catalog As MessageCatalog
    Private _current As MessageEntry
    Private _loading As Boolean
    Private _dirty As Boolean

    Private Shared ReadOnly EXTRA_SUGGESTIONS As String() = {
        "Deschide folderul", "Copiază mesajul", "Detalii", "Încearcă din nou", "Deschide jurnalul"}

    Public Sub New()
        InitializeComponent()
        _path = MessageCatalog.ResolvePath()
        FillCombos()
        LoadCatalog()
    End Sub

    ' ---------------- loading ----------------

    Private Sub FillCombos()
        _loading = True
        For Each k_name As String In [Enum].GetNames(GetType(KBotMsgKind))
            cboType.Items.Add(k_name)
        Next
        For Each k_name As String In [Enum].GetNames(GetType(KBotMsgButtons))
            cboButtons.Items.Add(k_name)
        Next
        For Each k_name As String In [Enum].GetNames(GetType(KBotMsgClose))
            cboClose.Items.Add(k_name)
        Next
        For Each k_name As String In EXTRA_SUGGESTIONS
            cboExtra.Items.Add(k_name)
        Next
        _loading = False
    End Sub

    Private Sub LoadCatalog()
        Try
            If Not IO.File.Exists(_path) Then
                _catalog = New MessageCatalog()
                SetStatus("Lipseste " & _path & " - ruleaza: node tools\MessageCatalog\scan.js")
            Else
                _catalog = MessageCatalog.Load(_path)
                SetStatus(_catalog.Messages.Count & " mesaje - " & _path)
            End If
            FillGrid()
            ShowSelected()
        Catch ex As Exception
            GlobalErrorLog.Write("MessageCatalogForm.LoadCatalog", ex)
            KBotMessage.Show(Me, "Catalogul nu a putut fi citit: " & ex.Message, "Mesaje",
                             MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub FillGrid()
        grd.BeginUpdate()
        Try
            grd.ClearRows()
            For Each k_entry As MessageEntry In _catalog.Messages
                Dim k_row As KBotDataRow = grd.AddRow()
                k_row.Tag = k_entry
                PaintRow(k_row, k_entry)
            Next
        Finally
            grd.EndUpdate()
        End Try
    End Sub

    Private Shared Sub PaintRow(k_row As KBotDataRow, k_entry As MessageEntry)
        k_row("stare") = If(k_entry.IsNew, "NOU", If(k_entry.IsEdited(), "MODIFICAT", String.Empty))
        k_row("tip") = k_entry.Type
        k_row("butoane") = k_entry.Buttons
        k_row("extra") = k_entry.ExtraButton
        k_row("titlu") = k_entry.Caption
        k_row("mesaj") = k_entry.Text.Replace(vbCrLf, " | ").Replace(vbLf, " | ")
        k_row("functia") = k_entry.FunctionName & ":" & k_entry.Line
    End Sub

    ' ---------------- selection -> editor ----------------

    Private Sub grd_SelectionChanged(sender As Object, e As EventArgs) Handles grd.SelectionChanged
        Try
            ShowSelected()
        Catch ex As Exception
            GlobalErrorLog.Write("MessageCatalogForm.grd_SelectionChanged", ex)
        End Try
    End Sub

    Private Sub ShowSelected()
        Dim k_row As KBotDataRow = grd.CurrentRow
        _current = If(k_row Is Nothing, Nothing, TryCast(k_row.Tag, MessageEntry))
        Dim k_has As Boolean = _current IsNot Nothing
        cboType.Enabled = k_has
        cboButtons.Enabled = k_has
        cboExtra.Enabled = k_has
        txtCaption.Enabled = k_has
        txtHeader.Enabled = k_has
        cboClose.Enabled = k_has
        rtbText.Editabil = k_has
        btnPreview.Enabled = k_has
        _loading = True
        Try
            If Not k_has Then
                lblFn.Text = "-"
                lblNote.Text = String.Empty
                cboType.Text = String.Empty
                cboButtons.Text = String.Empty
                cboExtra.Text = String.Empty
                txtCaption.Text = String.Empty
                txtHeader.Text = String.Empty
                cboClose.Text = String.Empty
                rtbText.Rtf = String.Empty
                Return
            End If
            lblFn.Text = _current.FunctionName & vbLf & _current.File & ":" & _current.Line
            cboType.Text = _current.Type
            cboButtons.Text = _current.Buttons
            cboExtra.Text = _current.ExtraButton
            txtCaption.Text = _current.Caption
            txtHeader.Text = _current.Header
            cboClose.Text = _current.CloseButton
            rtbText.Rtf = _current.Text
            lblNote.Text = NoteFor(_current)
        Finally
            _loading = False
        End Try
    End Sub

    Private Shared Function NoteFor(k_entry As MessageEntry) As String
        Dim k_note As New System.Text.StringBuilder()
        If k_entry.IsNew Then k_note.Append("NOU - gasit la ultima actualizare. ")
        If k_entry.IsEdited() Then
            k_note.Append("Modificat fata de cod: se aplica la rulare. ")
            If Not String.Equals(k_entry.Buttons, k_entry.OrigButtons, StringComparison.OrdinalIgnoreCase) Then
                k_note.Append("ATENTIE: codul asteapta butoanele " & k_entry.OrigButtons & ". ")
            End If
            If k_entry.ExtraButton.Trim().Length > 0 Then
                k_note.Append("Butonul extra intoarce 'None' si KBotMessage.LastExtraClicked = True. ")
            End If
        Else
            k_note.Append("La fel ca in cod (nu se aplica nimic). ")
        End If
        If k_entry.Dynamic OrElse k_entry.FromVariable Then
            k_note.Append("Partile {intre acolade} se calculeaza la rulare: pastreaza-le (acolada literala: {{ }}).")
        End If
        k_note.Append(" Textul poate avea HTML simplu: <b> <i> <u> <br> <div> <font color>.")
        Return k_note.ToString().Trim()
    End Function

    ' ---------------- editor -> model ----------------

    Private Sub Edited(sender As Object, e As EventArgs) Handles cboType.SelectedIndexChanged,
            cboButtons.SelectedIndexChanged, cboExtra.TextChanged, cboExtra.SelectedIndexChanged,
            txtCaption.TextChanged, txtHeader.TextChanged, cboClose.SelectedIndexChanged, rtbText.ContinutModificat
        Try
            If _loading OrElse _current Is Nothing Then Return
            ' Only the field the operator touched is read back, so a change of type never rewrites the text.
            If sender Is cboType AndAlso cboType.Text.Length > 0 Then _current.Type = cboType.Text
            If sender Is cboButtons AndAlso cboButtons.Text.Length > 0 Then _current.Buttons = cboButtons.Text
            If sender Is cboExtra Then _current.ExtraButton = cboExtra.Text.Trim()
            If sender Is txtCaption Then _current.Caption = txtCaption.Text
            If sender Is txtHeader Then _current.Header = txtHeader.Text
            If sender Is cboClose AndAlso cboClose.Text.Length > 0 Then _current.CloseButton = cboClose.Text
            If sender Is rtbText Then _current.Text = rtbText.TextSimplu.Replace(vbCr, String.Empty)
            _current.IsNew = False
            Dim k_row As KBotDataRow = grd.CurrentRow
            If k_row IsNot Nothing Then
                PaintRow(k_row, _current)
                grd.InvalidateRow(grd.CurrentRowIndex)
            End If
            lblNote.Text = NoteFor(_current)
            MarkDirty()
        Catch ex As Exception
            GlobalErrorLog.Write("MessageCatalogForm.Edited", ex)
        End Try
    End Sub

    Private Sub MarkDirty()
        _dirty = True
        SetStatus("Modificat, nesalvat - " & _path)
    End Sub

    Private Sub SetStatus(k_text As String)
        lblStatus.Text = k_text
    End Sub

    ' ---------------- preview, save, close ----------------

    Private Sub btnPreview_Click(sender As Object, e As EventArgs) Handles btnPreview.Click
        Try
            If _current Is Nothing Then Return
            Dim k_answer As KBotMessageResult = KBotMessageBox.Show(Me, _current.ToSpec())
            SetStatus("Previzualizare: " & If(k_answer.ExtraClicked, "butonul extra", k_answer.Result.ToString()) &
                      If(_dirty, " (modificari nesalvate)", String.Empty))
        Catch ex As Exception
            GlobalErrorLog.Write("MessageCatalogForm.btnPreview_Click", ex)
        End Try
    End Sub

    Private Sub btnReset_Click(sender As Object, e As EventArgs) Handles btnReset.Click
        Try
            If _current Is Nothing Then Return
            _current.ResetToCode()
            Dim k_row As KBotDataRow = grd.CurrentRow
            If k_row IsNot Nothing Then PaintRow(k_row, _current)
            ShowSelected()
            MarkDirty()
        Catch ex As Exception
            GlobalErrorLog.Write("MessageCatalogForm.btnReset_Click", ex)
        End Try
    End Sub

    ' «Actualizează»: runs the scanner (toolsMessageCatalogscan.js --merge) over the source, which adds the
    ' message boxes written since the last scan (marked NOU), refreshes lines and the code's own wording and
    ' keeps every edit; then the file is read again.
    Private Async Sub btnScan_Click(sender As Object, e As EventArgs) Handles btnScan.Click
        Try
            If _dirty AndAlso Not SaveCatalog() Then Return
            Dim k_root As String = MessageCatalog.FindRepositoryRoot()
            If k_root Is Nothing Then
                KBotMessage.Show(Me, "Nu gasesc folderul proiectului (toolsMessageCatalogscan.js) deasupra executabilului.",
                                 "Mesaje", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            btnScan.Enabled = False
            SetStatus("Se cauta mesaje noi in cod...")
            Dim k_output As String = Await Task.Run(Function() RunScanner(k_root)).ConfigureAwait(True)
            If IsDisposed Then Return
            LoadCatalog()
            Dim k_new As Integer = _catalog.Messages.Where(Function(k_m) k_m.IsNew).Count()
            SetStatus(_catalog.Messages.Count & " mesaje, " & k_new & " noi - " & k_output.Replace(vbCr, String.Empty).Replace(vbLf, "  "))
        Catch ex As Exception
            GlobalErrorLog.Write("MessageCatalogForm.btnScan_Click", ex)
            KBotMessage.Show(Me, "Actualizarea a esuat: " & ex.Message & vbLf & vbLf &
                             "Are nevoie de Node.js in PATH (comanda 'node').", "Mesaje", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If Not IsDisposed Then btnScan.Enabled = True
        End Try
    End Sub

    ' Process I/O (risky boundary): logs and rethrows; the handler above tells the operator.
    Private Shared Function RunScanner(k_root As String) As String
        Try
            Dim k_info As New ProcessStartInfo("node", """" & IO.Path.Combine(k_root, "tools", "MessageCatalog", "scan.js") & """ --merge") With {
                .WorkingDirectory = k_root,
                .UseShellExecute = False,
                .CreateNoWindow = True,
                .RedirectStandardOutput = True,
                .RedirectStandardError = True,
                .StandardOutputEncoding = System.Text.Encoding.UTF8
            }
            Using k_proc As Process = Process.Start(k_info)
                Dim k_out As String = k_proc.StandardOutput.ReadToEnd()
                Dim k_err As String = k_proc.StandardError.ReadToEnd()
                k_proc.WaitForExit()
                If k_proc.ExitCode <> 0 Then Throw New InvalidOperationException("scan.js a iesit cu codul " & k_proc.ExitCode & ": " & k_err)
                Return k_out.Trim()
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("MessageCatalogForm.RunScanner", ex)
            Throw
        End Try
    End Function

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            SaveCatalog()
        Catch ex As Exception
            GlobalErrorLog.Write("MessageCatalogForm.btnSave_Click", ex)
        End Try
    End Sub

    Private Function SaveCatalog() As Boolean
        Try
            _catalog.Generated = Date.Now.ToString("yyyy-MM-dd")
            For Each k_m As MessageEntry In _catalog.Messages
                k_m.IsNew = False      ' saved = seen
            Next
            _catalog.Save(_path)
            ' The running application reads the copy next to the executable: bring it up to date too.
            Dim k_runtime As String = IO.Path.Combine(AppContext.BaseDirectory, "Config", MessageCatalog.FileName)
            If Not String.Equals(IO.Path.GetFullPath(k_runtime), IO.Path.GetFullPath(_path), StringComparison.OrdinalIgnoreCase) Then
                IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(k_runtime))
                IO.File.Copy(_path, k_runtime, True)
            End If
            KBotMessage.ReloadCatalog()
            _dirty = False
            SetStatus("Salvat - " & _path)
            Return True
        Catch ex As Exception
            KBotMessage.Show(Me, "Nu s-a putut salva: " & ex.Message, "Mesaje", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        Try
            If _dirty Then
                Dim k_answer As DialogResult = KBotMessage.Show(Me, "Salvezi modificarile din catalogul de mesaje?", "Mesaje",
                                                                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
                                                                MessageBoxDefaultButton.Button1)
                If k_answer = DialogResult.Cancel OrElse (k_answer = DialogResult.Yes AndAlso Not SaveCatalog()) Then
                    e.Cancel = True
                    Return
                End If
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MessageCatalogForm.OnFormClosing", ex)
        End Try
        MyBase.OnFormClosing(e)
    End Sub

End Class
