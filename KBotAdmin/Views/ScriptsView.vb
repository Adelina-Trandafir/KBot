Option Strict On
Imports System.Diagnostics
Imports System.Globalization
Imports System.IO

''' <summary>
''' The «Scripturi» page of «Publicare»: the push-*.ps1 and publish-*.ps1 scripts of the repository. On the left, the scripts (grouped under
''' Push and Publish) with what each does and a «Rulează» button that fills its cell; on the right, the switches of the chosen script, one per
''' row with a tick (the version parts are four ticks of which only one can be on), and under them, behind a bar the operator drags to give
''' more room to the one or the other: for the scripts that make a version, the NOUTATI.md text of the version that comes out (the current
''' one, or the one raised by the ticked part); for the other scripts that have it, the text of <c>-Notes</c>. «Rulează» starts the script
''' in its own PowerShell window, so its questions and the sftp password prompt work as when typed by hand; when the script stops with an
''' error, that error is also written to the log of the page. Nothing starts without the operator pressing «Rulează» and answering the
''' confirmation. publish-admin.ps1 replaces this very program: it is started detached, waits for the program to end, and the program
''' closes (<see cref="CloseProgramRequested"/>).
''' </summary>
Public Class ScriptsView
    Implements IAdminView

    Private Const ColRun As String = "run"
    Private Const ColOn As String = "on"

    Private _context As AdminContext
    Private ReadOnly _log As LogSink
    Private ReadOnly _ticked As New Dictionary(Of ScriptDef, HashSet(Of String))()
    Private ReadOnly _notes As New Dictionary(Of ScriptDef, String)()
    Private _current As ScriptDef
    Private _filling As Boolean

    ''' <summary>Asked before a detached script starts: the host may refuse (an operation is still running on the server).</summary>
    Public Event CloseProgramQuery(ByRef k_allowed As Boolean)

    ''' <summary>The detached script has been started: the program must end now so that the script can run.</summary>
    Public Event CloseProgramRequested()

    Public Sub New()
        InitializeComponent()
        _log = New LogSink(logBox)
    End Sub

    Public Sub Attach(k_context As AdminContext) Implements IAdminView.Attach
        _context = k_context
    End Sub

    Public Function ActivatedAsync() As Task Implements IAdminView.ActivatedAsync
        If gridScripts.RowCount = 0 Then FillScripts()
        ShowRoot()
        ShowNotesPane()   ' NOUTATI.md may have been written since the page was last on screen
        ShowCommand()
        Return Task.CompletedTask
    End Function

    Public Sub SelectionChanged() Implements IAdminView.SelectionChanged
        ' The scripts do not depend on the chosen database.
    End Sub

    Private Sub ShowRoot()
        Dim k_root As String = ScriptCatalog.FindRepoRoot()
        lblState.Text = If(k_root = String.Empty,
                           "Nu găsesc KBot.sln deasupra programului: scripturile nu pot porni de aici.",
                           "Scripturile sunt în " & k_root & ". Fiecare pornește într-o fereastră PowerShell proprie.")
    End Sub

    Private Sub FillScripts()
        gridScripts.BeginUpdate()
        Try
            gridScripts.ClearRows()
            For Each k_def As ScriptDef In ScriptCatalog.All()
                Dim k_row As KBotDataRow = gridScripts.AddRow()
                k_row.Tag = k_def
                k_row("grup") = k_def.Group
                k_row("script") = k_def.Title
                k_row("desc") = k_def.Description
                k_row(ColRun) = String.Empty
                _ticked(k_def) = RememberedTicks(k_def)
                _notes(k_def) = String.Empty
            Next
        Finally
            gridScripts.EndUpdate()
        End Try
        gridScripts.CurrentRowIndex = gridScripts.FirstDisplayedRowIndex()
        ShowSwitches(TryCast(gridScripts.CurrentRow?.Tag, ScriptDef))
    End Sub

    ''' <summary>The switches ticked for this script the last time (admin_settings.json); a switch the script no longer has is dropped.</summary>
    Private Function RememberedTicks(k_def As ScriptDef) As HashSet(Of String)
        Dim k_set As New HashSet(Of String)(StringComparer.Ordinal)
        Dim k_saved As List(Of String) = Nothing
        If _context.Client.Settings.ScriptSwitches.TryGetValue(k_def.Key, k_saved) AndAlso k_saved IsNot Nothing Then
            For Each k_flag As String In k_saved
                If k_def.Flags.ContainsKey(k_flag) Then k_set.Add(k_flag)
            Next
        End If
        Return k_set
    End Function

    ''' <summary>Writes the ticks of every script to admin_settings.json, so they survive closing the program and publishing a new one.</summary>
    Private Sub SaveTicks()
        Try
            Dim k_all As Dictionary(Of String, List(Of String)) = _context.Client.Settings.ScriptSwitches
            k_all.Clear()
            For Each k_pair As KeyValuePair(Of ScriptDef, HashSet(Of String)) In _ticked
                If k_pair.Value.Count > 0 Then k_all(k_pair.Key.Key) = k_pair.Key.Flags.Keys.Where(Function(k_f) k_pair.Value.Contains(k_f)).ToList()
            Next
            AdminSettingsStore.Save(_context.Client.Settings)
        Catch ex As Exception
            ' UI boundary: the ticks stay on screen, only the keeping failed.
            GlobalErrorLog.Write("ScriptsView.SaveTicks", ex)
            _context.Say("Bifele nu s-au putut salva: " & ex.Message)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' The switches of the chosen script
    ' ------------------------------------------------------------------

    Private Sub GridScripts_SelectionChanged(k_sender As Object, e As EventArgs) Handles gridScripts.SelectionChanged
        Try
            Dim k_row As KBotDataRow = gridScripts.CurrentRow
            Dim k_def As ScriptDef = If(k_row Is Nothing, Nothing, TryCast(k_row.Tag, ScriptDef))
            If k_def IsNot Nothing AndAlso k_def IsNot _current Then ShowSwitches(k_def)   ' a group band keeps the list of the last script
        Catch ex As Exception
            GlobalErrorLog.Write("ScriptsView.GridScripts_SelectionChanged", ex)
        End Try
    End Sub

    ''' <summary>Fills the switch list with the switches of this script (with the ticks it had), and the notes box.</summary>
    Private Sub ShowSwitches(k_def As ScriptDef)
        If k_def Is Nothing Then Return
        _filling = True
        gridSwitches.BeginUpdate()
        Try
            _current = k_def
            gridSwitches.ClearRows()
            For Each k_key As String In k_def.Flags.Keys
                Dim k_row As KBotDataRow = gridSwitches.AddRow()
                k_row.Tag = k_key
                k_row(ColOn) = _ticked(k_def).Contains(k_key)
                k_row("switch") = ScriptCatalog.SwitchText(k_def, k_key)
                k_row("desc") = ScriptCatalog.FlagDescription(k_key)
            Next
            txtNotes.Text = _notes(k_def)
        Finally
            gridSwitches.EndUpdate()
            _filling = False
        End Try
        ShowNotesPane()
        ShowCommand()
    End Sub

    ''' <summary>
    ''' What is under the switch list: the NOUTATI text of the version that comes out (scripts with a version bump), the <c>-Notes</c> box
    ''' (the other scripts that take it), or nothing at all (the lower part of the bar is folded away).
    ''' </summary>
    Private Sub ShowNotesPane()
        Try
            If _current Is Nothing Then Return
            Dim k_release As Boolean = ScriptCatalog.HasBump(_current)
            splSide.Panel2Collapsed = Not (k_release OrElse _current.TakesNotes)
            lblNotesHead.Visible = k_release
            rtbNotes.Visible = k_release
            txtNotes.Visible = Not k_release AndAlso _current.TakesNotes
            If k_release Then ShowReleaseNotes()
        Catch ex As Exception
            GlobalErrorLog.Write("ScriptsView.ShowNotesPane", ex)
        End Try
    End Sub

    ''' <summary>The index (0 Major .. 3 Revision) of the version part that is ticked for this script, -1 when none.</summary>
    Private Function TickedBumpPart(k_def As ScriptDef) As Integer
        Return Array.FindIndex(ScriptCatalog.BumpFlags, Function(k_flag) _ticked(k_def).Contains(k_flag))
    End Function

    ''' <summary>Which version the script is about to make, and what NOUTATI.md says about it (read again each time: an assistant may have written it meanwhile).</summary>
    Private Sub ShowReleaseNotes()
        Dim k_root As String = ScriptCatalog.FindRepoRoot()
        If k_root = String.Empty Then
            lblNotesHead.Text = "Nu găsesc KBot.sln deasupra programului."
            rtbNotes.ShowText(String.Empty)
            Return
        End If
        Try
            Dim k_part As Integer = TickedBumpPart(_current)
            Dim k_now As Version = ReleasePreview.CurrentVersion(k_root)
            Dim k_target As Version = If(k_part < 0, k_now, ReleasePreview.Bumped(k_now, k_part))
            Dim k_section As ReleaseNotesSection = ReleasePreview.NotesFor(k_root, k_target)
            Dim k_head As String = If(k_part < 0,
                                      "Versiunea rămâne " & k_now.ToString(4),
                                      "Versiunea " & k_now.ToString(4) & " devine " & k_target.ToString(4) & " (" & PartName(k_part) & ")")
            If k_section.IsWritten Then
                k_head &= If(k_section.IsBaseline, " - începutul jurnalului NOUTATI.", " - NOUTATI: " & k_section.Lines.Count.ToString(CultureInfo.InvariantCulture) & " rânduri.")
                rtbNotes.ShowText(ReleasePreview.SectionText(k_section))
            Else
                k_head &= " - NOUTATI: nimic scris încă."
                rtbNotes.ShowText("În docs\release-notes\NOUTATI.md nu e text pentru " & k_target.ToString(4) & "." & vbCrLf & vbCrLf &
                                  If(_ticked(_current).Contains(ScriptCatalog.FlagNoNotes),
                                     "Bifa «Fără NOUTATI» e pusă: scriptul merge mai departe fără text și nu întreabă.",
                                     "Dacă porniți așa, scriptul pune cererea pe clipboard și așteaptă textul (Enter = caută din nou, S = fără text). Bifați «Fără NOUTATI» ca să nu mai întrebe."))
            End If
            lblNotesHead.Text = k_head
        Catch ex As Exception
            ' UI boundary: the preview is only a help, the script itself does not depend on it.
            GlobalErrorLog.Write("ScriptsView.ShowReleaseNotes", ex)
            lblNotesHead.Text = "NOUTATI nu s-au putut citi."
            rtbNotes.ShowText(ex.Message)
        End Try
    End Sub

    Private Shared Function PartName(k_part As Integer) As String
        Select Case k_part
            Case 0 : Return "Major"
            Case 1 : Return "Minor"
            Case 2 : Return "Build"
            Case Else : Return "Revision"
        End Select
    End Function

    Private Sub GridSwitches_CellValueChanged(k_sender As Object, e As KBotCellValueEventArgs) Handles gridSwitches.CellValueChanged
        Try
            If _filling OrElse _current Is Nothing OrElse e.ColumnKey <> ColOn Then Return
            Dim k_key As String = Convert.ToString(gridSwitches.Rows(e.RowIndex).Tag, CultureInfo.InvariantCulture)
            If Convert.ToBoolean(e.NewValue, CultureInfo.InvariantCulture) Then
                _ticked(_current).Add(k_key)
                ' The values of one parameter (or opposites; the four version parts): ticking one clears the others of its group.
                For Each k_group As String() In ScriptCatalog.Exclusive
                    If Not k_group.Contains(k_key) Then Continue For
                    For Each k_other As String In k_group
                        If k_other = k_key OrElse Not _ticked(_current).Remove(k_other) Then Continue For
                        _filling = True
                        Try
                            For k_i As Integer = 0 To gridSwitches.RowCount - 1
                                If String.Equals(Convert.ToString(gridSwitches.Rows(k_i).Tag, CultureInfo.InvariantCulture), k_other, StringComparison.Ordinal) Then
                                    gridSwitches(ColOn, k_i) = False
                                End If
                            Next
                        Finally
                            _filling = False
                        End Try
                    Next
                Next
            Else
                _ticked(_current).Remove(k_key)
            End If
            SaveTicks()
            ShowNotesPane()
            ShowCommand()
        Catch ex As Exception
            GlobalErrorLog.Write("ScriptsView.GridSwitches_CellValueChanged", ex)
        End Try
    End Sub

    Private Sub TxtNotes_TextChanged(k_sender As Object, e As EventArgs) Handles txtNotes.TextChanged
        Try
            If _filling OrElse _current Is Nothing OrElse Not _current.TakesNotes Then Return
            _notes(_current) = txtNotes.Text
            ShowCommand()
        Catch ex As Exception
            GlobalErrorLog.Write("ScriptsView.TxtNotes_TextChanged", ex)
        End Try
    End Sub

    ''' <summary>The call of the chosen script, written out as it would be typed.</summary>
    Private Sub ShowCommand()
        Try
            If _current Is Nothing Then
                lblCommand.Text = String.Empty
                Return
            End If
            lblCommand.Text = _current.Description & vbLf & ScriptCatalog.DisplayCommand(_current, ArgumentsOf(_current))
        Catch ex As Exception
            GlobalErrorLog.Write("ScriptsView.ShowCommand", ex)
        End Try
    End Sub

    Private Function ArgumentsOf(k_def As ScriptDef) As List(Of String)
        Return ScriptCatalog.ScriptArguments(k_def, _ticked(k_def), _notes(k_def))
    End Function

    ' ------------------------------------------------------------------
    ' Run
    ' ------------------------------------------------------------------

    Private Sub GridScripts_ButtonClick(k_sender As Object, e As KBotButtonClickEventArgs) Handles gridScripts.ButtonClick
        Try
            If e.ColumnKey <> ColRun Then Return
            Dim k_def As ScriptDef = TryCast(gridScripts.Rows(e.RowIndex).Tag, ScriptDef)
            If k_def Is Nothing Then Return
            RunScript(k_def, ArgumentsOf(k_def))
        Catch ex As Exception
            GlobalErrorLog.Write("ScriptsView.GridScripts_ButtonClick", ex)
            _log.Add(KBotLogLevel.Error, "Scriptul nu a putut porni: " & ex.Message)
            KBotMessage.Show("Scriptul nu a putut porni: " & ex.Message, "Scripturi", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub RunScript(k_def As ScriptDef, k_args As List(Of String))
        Dim k_root As String = ScriptCatalog.FindRepoRoot()
        If k_root = String.Empty Then
            KBotMessage.Show("Nu găsesc KBot.sln deasupra programului, deci nu știu unde sunt scripturile.", "Scripturi", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        Dim k_command As String = ScriptCatalog.DisplayCommand(k_def, k_args)
        Dim k_question As String = "Pornesc într-o fereastră PowerShell nouă, în " & k_root & ":" & vbLf & vbLf & k_command & vbLf & vbLf
        If k_def.Detached Then
            k_question &= "KBotAdmin se închide acum; scriptul pornește după ce programul s-a oprit, iar dacă se termină cu bine, KBotAdmin pornește singur din nou din _Admin\KBotAdmin.exe." & vbLf & vbLf
        End If
        If Not UiRun.Confirm(k_question & "Continuați?", "Scripturi") Then Return
        If k_def.Detached Then
            Dim k_allowed As Boolean = True
            RaiseEvent CloseProgramQuery(k_allowed)
            If Not k_allowed Then Return
        End If
        RunLog.Write("Scripturi: pornesc " & k_command)
        ' The script's window writes the text of an error here when the script stops with one; OnScriptExited shows it in the log.
        Dim k_errorFile As String = Path.Combine(Path.GetTempPath(), "kbotadmin_script_" & Guid.NewGuid().ToString("N") & ".err")
        Dim k_process As Process = ScriptCatalog.Start(k_def, k_root, k_args, k_errorFile)
        _log.Add(KBotLogLevel.Info, "Pornit: " & k_command)
        _context.Say("A pornit " & k_def.Title & ".")
        If k_def.Detached Then
            _log.Add(KBotLogLevel.Info, "KBotAdmin se închide; scriptul așteaptă să se oprească programul.")
            RaiseEvent CloseProgramRequested()
            Return
        End If
        k_process.EnableRaisingEvents = True
        AddHandler k_process.Exited, Sub(k_s, k_e) OnScriptExited(k_def.Title, k_process, k_errorFile)
    End Sub

    ''' <summary>
    ''' The PowerShell window was closed: the error the script stopped with (when it did) and its exit code (that of the script) go to the
    ''' page's log. Runs on a pool thread.
    ''' </summary>
    Private Sub OnScriptExited(k_title As String, k_process As Process, k_errorFile As String)
        Try
            ShowScriptError(k_title, k_errorFile)
            Dim k_code As Integer = k_process.ExitCode
            Dim k_text As String = k_title & If(k_code = 0, " s-a terminat cu succes.", " s-a terminat cu eroare (cod " & k_code.ToString(CultureInfo.InvariantCulture) & ").")
            _log.Add(If(k_code = 0, KBotLogLevel.Info, KBotLogLevel.Error), k_text)
            RunLog.Write("Scripturi: " & k_text)
            k_process.Dispose()
        Catch ex As Exception
            GlobalErrorLog.Write("ScriptsView.OnScriptExited", ex)
        End Try
    End Sub

    ''' <summary>The text the script's window kept in <paramref name="k_errorFile"/> (an error it stopped with), line by line in the log as errors; the file is removed.</summary>
    Private Sub ShowScriptError(k_title As String, k_errorFile As String)
        Try
            If Not File.Exists(k_errorFile) Then Return
            Dim k_text As String = File.ReadAllText(k_errorFile).Trim()
            File.Delete(k_errorFile)
            If k_text = String.Empty Then Return
            RunLog.Write("Scripturi: " & k_title & " a oprit cu eroare: " & k_text)
            _log.Add(KBotLogLevel.Error, k_title & " a oprit cu eroare:")
            For Each k_line As String In k_text.Replace(vbCrLf, vbLf).Split(ControlChars.Lf)
                If Not String.IsNullOrWhiteSpace(k_line) Then _log.Add(KBotLogLevel.Error, "    " & k_line.TrimEnd())
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("ScriptsView.ShowScriptError", ex)
        End Try
    End Sub

End Class
