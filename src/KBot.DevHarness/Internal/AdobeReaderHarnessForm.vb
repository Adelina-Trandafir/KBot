Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.IO
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports Microsoft.Win32
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Banc de probă pentru încorporarea Adobe Reader/Acrobat DC într-un panou-gazdă, cu fiecare
''' switch de ascundere a chrome-ului (bare de instrumente/panouri) expus ca bifă. La orice
''' schimbare de bifă închidem instanța Adobe curentă și o redeschidem cu noul set de switch-uri
''' (cerința operatorului).
'''
''' Slice 0023 (first pass) added the four candidate levers against the right-hand Tools pane
''' (RHP), which the /A open parameters cannot touch (document chrome vs application chrome):
''' the child-window probe, geometry clipping, direct child hiding, keyboard toggles, HKCU
''' preferences and HKLM policies.
'''
''' Slice 0023 (config+layout pass) adds two things and NO new lever:
'''   * Scenario files (JSON, AppDir\Config) — load / run / save, so a combination found by hand
'''     becomes a file that can be repeated, sent to someone else, or tried on another machine.
'''     A scenario carries SETTINGS, never a document: loading one PRE-SETS the controls in the
'''     left panel, and the PDF the operator then picks with «Deschide PDF…» opens under those
'''     settings. The controls are the single source of truth from that point on — the scenario is
'''     never consulted again while launching, so what the operator sees ticked is what runs.
'''     Children are addressed by window TEXT, never by HWND: handles change on every launch
'''     (0x5083E -> 0x20B66) while the text (AVTaskPaneHostView) does not, and class names are
'''     useless because nearly everything is AVL_AVView.
'''   * A resizable docked layout (SplitContainer; the options are GroupBox sections stacked in a
'''     SCROLLING FlowLayoutPanel), so the Adobe area can be traded against the options area at
'''     runtime. Because the clip geometry is computed from pnlHost.ClientSize, it is re-applied on
'''     host resize and splitter moves — debounced, since Adobe repaints late and badly during a
'''     resize storm.
'''     NOTE: the options stack must NOT be a TableLayoutPanel. A TLP with AutoScroll plus a
'''     Percent filler row always reports that its content fits, so no scrollbar ever appears and
'''     every section past the fold is clipped — that defect shipped once and made both registry
'''     sections unreachable on screen.
'''
''' Detalii care au mușcat deja tiparul (vezi și <see cref="ReaderHostPreview"/> din KBot.App):
'''   * Reader e practic mono-instanță -> lansăm cu «/n» (instanță nouă) ca fereastra să ne
'''     aparțină și să nu fie predată unei instanțe deja deschise cu alte documente.
'''   * După SetParent + curățarea stilurilor, fereastra rămâne NEVĂZUTĂ până la o schimbare de
'''     layout. De aceea forțăm explicit o redesenare (NudgeRedraw).
'''   * La închiderea formularului (și la fiecare reîncorporare) OMORÂM forțat Adobe după PID.
''' </summary>
Public NotInheritable Class AdobeReaderHarnessForm

    ' Timeout la căutarea ferestrei Reader (ms) și pasul de polling.
    Private Const FIND_TIMEOUT_MS As Integer = 8000
    Private Const FIND_POLL_MS As Integer = 150
    ' Întârziere pentru a doua redesenare (Adobe își finalizează layout-ul după apariția ferestrei).
    Private Const REDRAW_DELAY_MS As Integer = 250
    ' Probe depth limit and the right-edge tolerance of the RHP heuristic (px).
    Private Const PROBE_MAX_DEPTH As Integer = 4
    Private Const RHP_EDGE_TOLERANCE As Integer = 8
    ' Dedicated log for this slice (house rule: harness output goes to Logs\test_*.log).
    Private Const RHP_LOG_NAME As String = "test_adobe_rhp.log"
    ' Scenario files live here, with no fixed names — the operator browses for one.
    Private Const CONFIG_DIR_NAME As String = "Config"

    Private ReadOnly _log As Action(Of String)
    Private ReadOnly _adobePath As String
    ' Registry seam + once-per-session HKCU snapshot (slice 0023).
    Private ReadOnly _regAccess As New WinRegistryAccess()
    Private ReadOnly _userSnapshot As RegistrySnapshotSet

    Private _loading As Boolean = True      ' cât e True, bifele nu declanșează relansarea
    Private _pdfPath As String = Nothing

    ' Fereastra Adobe găzduită acum + stilul ei original + PID-ul proprietar.
    Private _hostedWindow As IntPtr = IntPtr.Zero
    Private _originalStyle As IntPtr = IntPtr.Zero
    Private _hostedPid As Integer = 0
    ' Procesul Reader pe care l-am pornit noi (dacă l-am pornit).
    Private _readerProcess As Process
    ' Generația relansării curente: o relansare mai nouă o invalidează pe cea în curs.
    Private _generation As Integer = 0
    ' Between a scenario's `launch` and `waitForEmbed` steps.
    Private _pendingProcess As Process
    Private _pendingGeneration As Integer = 0

    ' Probe/child state: widest RHP candidate, the last probe's nodes, the handles we hid, and
    ' the TEXTS we hid (the durable identity across relaunches).
    Private _probeCandidateWidth As Integer = 0
    Private _probeCandidateClass As String = Nothing
    Private ReadOnly _lastProbe As New List(Of ChildWindowItem)()
    Private ReadOnly _hiddenChildren As New List(Of IntPtr)()
    Private ReadOnly _hiddenChildTexts As New List(Of String)()
    ' Status-block state: which HKCU values were applied, whether HKLM was applied this session.
    Private ReadOnly _userValuesApplied As New List(Of String)()
    Private _machinePolicyApplied As Boolean = False
    ' Generated .reg file paths (apply + revert), set by the last machine-policy apply.
    Private _applyRegPath As String = Nothing
    Private _revertRegPath As String = Nothing
    ' Last one-line action message, shown under the status block (see RefreshStatusBlock).
    Private _lastMessage As String = ""
    ' Loaded scenario + its file path; _scenarioRunning guards against re-entry.
    Private _scenario As HarnessScenario
    Private _scenarioPath As String
    Private _scenarioRunning As Boolean = False

    Public Sub New(log As Action(Of String))
        _log = log
        InitializeComponent()
        _userSnapshot = New RegistrySnapshotSet(_regAccess)
        PopulateRegistryCombos()
        _adobePath = ResolveAdobePath()
        If String.IsNullOrEmpty(_adobePath) Then
            SetControlsEnabled(False)
            ShowStatus("Adobe Reader/Acrobat nu a fost găsit pe această mașină.")
        Else
            ShowStatus("Adobe: " & _adobePath & Environment.NewLine & "Alege un PDF pentru a-l încorpora.")
        End If
        _loading = False
        UpdateCmdPreview()
        UpdateActionStates()
        SizeSections()
        RefreshPrefsGrid()
    End Sub

    ' Warn about an outstanding HKLM policy as soon as the bench is visible — a modal dialog in the
    ' constructor would fight the theming/Load path.
    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        Try
            WarnIfPolicyOutstanding()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.OnShown", ex)
        End Try
    End Sub

    ' Fills the hive/product combos ("auto" + explicit choices) and the hive-detection label.
    ' Detection failures are logged and shown as "?", never thrown out of the ctor.
    Private Sub PopulateRegistryCombos()
        cboHive.Items.AddRange(New Object() {
            "auto", AdobeRegistryConstants.AvGeneralReader, AdobeRegistryConstants.AvGeneralAcrobat})
        cboHive.SelectedIndex = 0
        cboProduct.Items.AddRange(New Object() {
            "auto", AdobeRegistryConstants.ProductReader, AdobeRegistryConstants.ProductAcrobat})
        cboProduct.SelectedIndex = 0
        PopulatePrefRows()
        Try
            Dim rEx As Boolean = _regAccess.KeyExists(AdobeRegistryConstants.AvGeneralReader)
            Dim aEx As Boolean = _regAccess.KeyExists(AdobeRegistryConstants.AvGeneralAcrobat)
            lblHive.Text = "AVGeneral găsit: Reader DC — " & If(rEx, "există", "lipsește") &
                           " · Adobe Acrobat — " & If(aEx, "există", "lipsește")
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.PopulateRegistryCombos", ex)
            lblHive.Text = "AVGeneral: detecție eșuată (vezi jurnalul de erori)."
        End Try
    End Sub

    ' Each HKCU preference gets a row whose value is EDITABLE, with «nu atinge» as the default and
    ' «șterge» as an explicit choice. The list entries are suggestions, not the whole alphabet: the
    ' combos are DropDown, so a scenario asking for a value nobody anticipated still shows up here
    ' as itself instead of being rounded to the nearest checkbox.
    Private Sub PopulatePrefRows()
        Dim flags As Object() = {PrefRowSelection.Untouched, "0", "1", PrefRowSelection.DeleteText}
        For Each c As ComboBox In New ComboBox() {cboExpandRhp, cboRhpSticky, cboEnableAv2}
            c.Items.AddRange(flags)
            c.SelectedIndex = 0
        Next
        cboRhpViewMode.Items.AddRange(New Object() {
            PrefRowSelection.Untouched, AdobeRegistryConstants.RhpViewModeCollapsed,
            "Expanded", PrefRowSelection.DeleteText})
        cboRhpViewMode.SelectedIndex = 0
        For Each c As ComboBox In New ComboBox() {cboExpandRhp, cboRhpSticky, cboRhpViewMode, cboEnableAv2}
            c.SelectionLength = 0
        Next
    End Sub

    ' Every row parsed, in panel order. Rows the operator left on «nu atinge» produce no intent at
    ' all — the state the old checkboxes could not express.
    Private Function ParsePrefRows() As List(Of PrefRowParse)
        Return New List(Of PrefRowParse) From {
            PrefRowSelection.ParseDword(AdobeRegistryConstants.ValExpandRhp, cboExpandRhp.Text),
            PrefRowSelection.ParseDword(AdobeRegistryConstants.ValRhpSticky, cboRhpSticky.Text),
            PrefRowSelection.ParseString(AdobeRegistryConstants.ValRhpViewMode, cboRhpViewMode.Text),
            PrefRowSelection.ParseDword(AdobeRegistryConstants.ValEnableAv2, cboEnableAv2.Text)}
    End Function

    ' A row that cannot be parsed must stop the run: silently skipping it would put the panel and
    ' the registry back into disagreement, which is the whole defect this pass exists to remove.
    Private Function InvalidPrefRows() As List(Of String)
        Return ParsePrefRows().Where(Function(p) p.Invalid).Select(Function(p) p.Message).ToList()
    End Function

    ' Any row change re-reads the machine so «Cerut vs Curent» never lags behind the panel.
    Private Sub PrefRow_Changed(sender As Object, e As EventArgs) _
        Handles cboExpandRhp.SelectedIndexChanged, cboExpandRhp.TextChanged,
                cboRhpSticky.SelectedIndexChanged, cboRhpSticky.TextChanged,
                cboRhpViewMode.SelectedIndexChanged, cboRhpViewMode.TextChanged,
                cboEnableAv2.SelectedIndexChanged, cboEnableAv2.TextChanged
        Try
            If _loading Then Return
            RefreshPrefsGrid()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.PrefRow_Changed", ex)
        End Try
    End Sub

    ' ── Evenimente UI (fiecare boundary UI: loghează și ÎNGHITE) ─────────────────
    Private Async Sub btnBrowse_Click(sender As Object, e As EventArgs) Handles btnBrowse.Click
        Try
            Using dlg As New System.Windows.Forms.OpenFileDialog()
                dlg.Filter = "Fișiere PDF (*.pdf)|*.pdf|Toate fișierele (*.*)|*.*"
                dlg.Title = "Alege un PDF pentru încorporare"
                If dlg.ShowDialog(Me) <> DialogResult.OK Then Return
                _pdfPath = dlg.FileName
            End Using
            lblFile.Text = _pdfPath
            UpdateCmdPreview()
            Await RelaunchAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnBrowse_Click", ex)
        End Try
    End Sub

    ' Orice bifă schimbată -> preview + (dacă avem PDF) închide Adobe și redeschide cu noul set.
    Private Async Sub Switch_CheckedChanged(sender As Object, e As EventArgs) _
        Handles chkNewInstance.CheckedChanged, chkNoSplash.CheckedChanged,
                chkToolbar.CheckedChanged, chkNavpanes.CheckedChanged,
                chkStatusbar.CheckedChanged, chkMessages.CheckedChanged,
                chkScrollbar.CheckedChanged, chkPagemodeNone.CheckedChanged
        Try
            If _loading Then Return
            UpdateCmdPreview()
            If Not String.IsNullOrEmpty(_pdfPath) Then Await RelaunchAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.Switch_CheckedChanged", ex)
        End Try
    End Sub

    Private Async Sub btnRelaunch_Click(sender As Object, e As EventArgs) Handles btnRelaunch.Click
        Try
            If String.IsNullOrEmpty(_pdfPath) Then
                ShowStatus("Alege întâi un PDF (Deschide PDF…).")
                Return
            End If
            Await RelaunchAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnRelaunch_Click", ex)
        End Try
    End Sub

    ' ── Layout: host resize + splitter, debounced ───────────────────────────────
    ' The clip geometry is computed from pnlHost.ClientSize, which now changes when the splitter
    ' moves and when the form resizes — without re-applying here, dragging the splitter would
    ' silently break the clip. Debounced (150 ms) because Adobe repaints late and badly during a
    ' resize storm: do NOT reposition on every Resize tick.
    Private Sub pnlHost_Resize(sender As Object, e As EventArgs) Handles pnlHost.Resize
        Try
            ScheduleLayoutRefresh()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.pnlHost_Resize", ex)
        End Try
    End Sub

    Private Sub splitMain_SplitterMoved(sender As Object, e As SplitterEventArgs) Handles splitMain.SplitterMoved
        Try
            ScheduleLayoutRefresh()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.splitMain_SplitterMoved", ex)
        End Try
    End Sub

    Private Sub ScheduleLayoutRefresh()
        ' Nothing hosted -> nothing to re-place (guard required by the layout rework).
        If _hostedWindow = IntPtr.Zero Then Return
        tmrLayout.Stop()
        tmrLayout.Start()
    End Sub

    Private Sub tmrLayout_Tick(sender As Object, e As EventArgs) Handles tmrLayout.Tick
        Try
            tmrLayout.Stop()
            If _hostedWindow = IntPtr.Zero Then Return
            LayoutHostedWindow()
            NudgeRedraw()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.tmrLayout_Tick", ex)
        End Try
    End Sub

    ' On close: invalidate any in-flight embed, force-kill Adobe (operator requirement), then
    ' restore the HKCU snapshot if the operator left "Restaurează la închidere" checked.
    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        Try
            Interlocked.Increment(_generation)
            tmrLayout.Stop()
            KillTracked()
            RestoreUserPrefsOnClose()
            RevertMachinePolicyOnClose()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.OnFormClosed", ex)
        End Try
        MyBase.OnFormClosed(e)
    End Sub

    ' ── Relansare / încorporare ─────────────────────────────────────────────────
    ' Închide Adobe curent, pornește o instanță nouă cu switch-urile bifate, îi găsește fereastra
    ' (pe fir de fundal, ca UI-ul să rămână responsiv) și o reparentează în pnlHost.
    Private Async Function RelaunchAsync() As Task
        Try
            Dim gen As Integer = StartFreshLaunch()
            If gen = 0 Then Return
            Dim proc As Process = _pendingProcess
            If proc Is Nothing Then Return
            Await CompleteEmbedAsync(gen, proc)
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.RelaunchAsync", ex)
            ShowStatus("Eroare la încorporare: " & ex.Message)
        End Try
    End Function

    ' Kills the current instance and starts a new one with the effective switch set. Returns the
    ' generation of this launch, or 0 when nothing was started. Shared by the button path and the
    ' scenario `launch` step (which then runs `waitForEmbed` separately).
    Private Function StartFreshLaunch() As Integer
        Dim gen As Integer = Interlocked.Increment(_generation)
        KillTracked()
        _pendingProcess = Nothing
        _pendingGeneration = 0

        Dim pdf As String = EffectivePdfPath()
        If String.IsNullOrEmpty(_adobePath) OrElse String.IsNullOrEmpty(pdf) Then Return 0

        Dim args As String = BuildArguments(pdf)
        ShowStatus("Pornesc Adobe…")
        Dim proc As Process = StartReader(args)
        If proc Is Nothing Then Return 0
        _pendingProcess = proc
        _pendingGeneration = gen
        Return gen
    End Function

    ' Waits for the launched window, hosts it, and (if a scenario asks) re-applies the hides.
    Private Async Function CompleteEmbedAsync(gen As Integer, proc As Process) As Task
        Dim pdf As String = EffectivePdfPath()
        Dim baseName As String = Path.GetFileNameWithoutExtension(pdf)
        Dim hwnd As IntPtr = Await Task.Run(Function() FindReaderWindow(baseName))

        ' O relansare mai nouă a preluat controlul cât timp căutam: curăț procesul propriu.
        If gen <> _generation Then
            SafeKill(proc)
            Return
        End If

        _readerProcess = proc
        _pendingProcess = Nothing
        If hwnd = IntPtr.Zero Then
            _hostedPid = SafePid(proc)
            ShowStatus("Adobe pornit, dar fereastra nu a apărut în " & (FIND_TIMEOUT_MS \ 1000).ToString() &
                       "s (fără încorporare).")
            Return
        End If

        _hostedWindow = hwnd
        Dim ownerPid As Integer = 0
        GetWindowThreadProcessId(hwnd, ownerPid)
        _hostedPid = ownerPid

        HostWindow(hwnd)
        ShowStatus("Încorporat. PID fereastră: " & ownerPid.ToString())
        _log("Adobe încorporat — " & Path.GetFileName(_adobePath) & " " & BuildArguments(pdf))
        UpdateActionStates()

        ' A doua redesenare, după ce Adobe își termină layout-ul (altfel rămâne nevăzut).
        Await Task.Delay(REDRAW_DELAY_MS)
        If gen <> _generation Then Return
        NudgeRedraw()

        ' Hidden children do not survive a relaunch (new HWNDs) — re-resolve by text.
        Dim hc As HideChildrenConfig = If(_scenario IsNot Nothing, _scenario.HideChildren, Nothing)
        If hc IsNot Nothing AndAlso hc.ReapplyOnRelaunch.GetValueOrDefault() AndAlso
           hc.ByText IsNot Nothing AndAlso hc.ByText.Count > 0 Then
            Await HideChildrenByTextAsync(hc)
        End If
    End Function

    Private Function StartReader(args As String) As Process
        Try
            Dim psi As New ProcessStartInfo(_adobePath, args) With {.UseShellExecute = False}
            Return Process.Start(psi)
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.StartReader", ex)
            ShowStatus("Nu am putut porni Adobe: " & ex.Message)
            Return Nothing
        End Try
    End Function

    ' Reparentează fereastra în pnlHost, curățând stilurile de fereastră de sine stătătoare.
    Private Sub HostWindow(hwnd As IntPtr)
        _originalStyle = GetWindowLongPtrSafe(hwnd, GWL_STYLE)

        Dim style As Long = _originalStyle.ToInt64()
        style = style And Not (WS_CAPTION Or WS_THICKFRAME Or WS_POPUP Or
                               WS_MINIMIZEBOX Or WS_MAXIMIZEBOX Or WS_SYSMENU)
        style = style Or WS_CHILD
        SetWindowLongPtrSafe(hwnd, GWL_STYLE, New IntPtr(style))

        SetParent(hwnd, pnlHost.Handle)
        NudgeRedraw()
    End Sub

    ' The bounds of the hosted window inside pnlHost. Without clipping it fills the panel; with
    ' clipping it is oversized and offset so the clipped bands (top toolbar strip / right pane
    ' strip) fall OUTSIDE the visible client area of pnlHost.
    Private Function HostedBounds() As Rectangle
        Dim w As Integer = pnlHost.ClientSize.Width
        Dim h As Integer = pnlHost.ClientSize.Height
        If chkClip.Checked Then
            Dim clipRight As Integer = CInt(numClipRight.Value)
            Dim clipTop As Integer = CInt(numClipTop.Value)
            Return New Rectangle(0, -clipTop, w + clipRight, h + clipTop)
        End If
        Return New Rectangle(0, 0, w, h)
    End Function

    Private Sub LayoutHostedWindow()
        If _hostedWindow = IntPtr.Zero Then Return
        Dim b As Rectangle = HostedBounds()
        If b.Width <= 0 OrElse b.Height <= 0 Then Return
        MoveWindow(_hostedWindow, b.X, b.Y, b.Width, b.Height, True)
    End Sub

    ' Forțează afișarea/redesenarea ferestrei reparentate (altfel rămâne nevăzută până la o
    ' schimbare de layout a formularului).
    Private Sub NudgeRedraw()
        If _hostedWindow = IntPtr.Zero Then Return
        Dim b As Rectangle = HostedBounds()
        If b.Width <= 0 OrElse b.Height <= 0 Then Return

        SetWindowPos(_hostedWindow, IntPtr.Zero, b.X, b.Y, b.Width, b.Height,
                     SWP_NOZORDER Or SWP_NOACTIVATE Or SWP_FRAMECHANGED Or SWP_SHOWWINDOW)
        MoveWindow(_hostedWindow, b.X, b.Y, Math.Max(1, b.Width - 1), b.Height, True)
        MoveWindow(_hostedWindow, b.X, b.Y, b.Width, b.Height, True)
        RedrawWindow(_hostedWindow, IntPtr.Zero, IntPtr.Zero,
                     RDW_INVALIDATE Or RDW_ALLCHILDREN Or RDW_UPDATENOW Or RDW_FRAME)
        pnlHost.Invalidate(True)
    End Sub

    ' ── Argumente / preview ─────────────────────────────────────────────────────
    ' Sintaxă Adobe: [/n] [/s] [/A "param1&param2&…"] "cale.pdf". Parametrii /A trebuie ÎNAINTE de fișier.
    Private Function BuildArguments(pdf As String) As String
        Dim sb As New StringBuilder()
        If chkNewInstance.Checked Then sb.Append("/n ")
        If chkNoSplash.Checked Then sb.Append("/s ")
        Dim op As String = BuildOpenParameters()
        If op.Length > 0 Then sb.Append("/A """).Append(op).Append(""" ")
        sb.Append(""""c).Append(pdf).Append(""""c)
        Return sb.ToString()
    End Function

    ' Parametrii de deschidere (/A) care ascund chrome-ul, în ordinea din panou. Read from the
    ' CONTROLS only: a loaded scenario has already written itself into them, so the panel is the
    ' single source of truth and the operator can still adjust before opening a file.
    Private Function BuildOpenParameters() As String
        Dim parts As New List(Of String)()
        If chkToolbar.Checked Then parts.Add("toolbar=0")
        If chkNavpanes.Checked Then parts.Add("navpanes=0")
        If chkStatusbar.Checked Then parts.Add("statusbar=0")
        If chkMessages.Checked Then parts.Add("messages=0")
        If chkScrollbar.Checked Then parts.Add("scrollbar=0")
        If chkPagemodeNone.Checked Then parts.Add("pagemode=none")
        Return String.Join("&", parts)
    End Function

    ' The document ALWAYS comes from «Deschide PDF…» — a scenario never carries one.
    Private Function EffectivePdfPath() As String
        Return _pdfPath
    End Function

    Private Sub UpdateCmdPreview()
        Dim exe As String = If(String.IsNullOrEmpty(_adobePath), "<Adobe negăsit>", Path.GetFileName(_adobePath))
        Dim pdf As String = _pdfPath
        If String.IsNullOrEmpty(pdf) Then pdf = "<alege un PDF>"
        txtCmd.Text = exe & " " & BuildArguments(pdf)
    End Sub

    ' ── Curățare Adobe ──────────────────────────────────────────────────────────
    ' Distruge forțat Adobe găzduit: întâi procesul PROPRIETAR al ferestrei (via PID), apoi
    ' procesul pornit de noi. Best-effort prin construcție.
    Private Sub KillTracked()
        Dim hostedPid As Integer = _hostedPid
        _hostedWindow = IntPtr.Zero
        _originalStyle = IntPtr.Zero
        _hostedPid = 0

        KillPid(hostedPid)

        Dim rp As Process = _readerProcess
        _readerProcess = Nothing
        SafeKill(rp)

        Dim pp As Process = _pendingProcess
        _pendingProcess = Nothing
        SafeKill(pp)

        ' Probe results and hidden-child HANDLES belong to the dead process. The hidden TEXTS are
        ' kept: they are the durable identity a scenario re-applies after the next embed.
        _lastProbe.Clear()
        _hiddenChildren.Clear()
        _probeCandidateWidth = 0
        _probeCandidateClass = Nothing
        If lstChildren IsNot Nothing AndAlso Not lstChildren.IsDisposed Then lstChildren.Items.Clear()
        UpdateActionStates()
    End Sub

    Private Shared Sub KillPid(pid As Integer)
        If pid <= 0 Then Return
        Try
            Dim p As Process = Process.GetProcessById(pid)
            Try
                If Not p.HasExited Then p.Kill(True)
            Finally
                p.Dispose()
            End Try
        Catch
            ' Best-effort: procesul poate să nu mai existe.
        End Try
    End Sub

    Private Shared Sub SafeKill(proc As Process)
        If proc Is Nothing Then Return
        Try
            If Not proc.HasExited Then proc.Kill(True)
        Catch
            ' Best-effort.
        End Try
        Try
            proc.Dispose()
        Catch
        End Try
    End Sub

    Private Shared Function SafePid(proc As Process) As Integer
        Try
            Return proc.Id
        Catch
            Return 0
        End Try
    End Function

    ' ── Căutarea ferestrei Reader (fir de fundal) ───────────────────────────────
    Private Function FindReaderWindow(baseName As String) As IntPtr
        Dim deadline As DateTime = DateTime.UtcNow.AddMilliseconds(FIND_TIMEOUT_MS)
        Do
            Dim found As IntPtr = IntPtr.Zero
            EnumWindows(
                Function(h, l)
                    If Not IsWindowVisible(h) Then Return True
                    Dim cls As String = GetClass(h)
                    If cls.IndexOf("Acrobat", StringComparison.OrdinalIgnoreCase) < 0 AndAlso
                       cls.IndexOf("AdobeAcrobat", StringComparison.OrdinalIgnoreCase) < 0 Then Return True
                    Dim title As String = GetTitle(h)
                    If title.IndexOf(baseName, StringComparison.OrdinalIgnoreCase) >= 0 Then
                        found = h
                        Return False   ' oprim enumerarea
                    End If
                    Return True
                End Function, IntPtr.Zero)

            If found <> IntPtr.Zero Then Return found
            Thread.Sleep(FIND_POLL_MS)
        Loop While DateTime.UtcNow < deadline
        Return IntPtr.Zero
    End Function

    ' ── Localizarea Adobe ───────────────────────────────────────────────────────
    ' NOTE: AdobeUtils.GetAdobeReaderPath (KBot.Xfa) resolves the .pdf HANDLER, but it is Private
    ' and KBot.Xfa is not referenced here — reusing it would mean touching KBot.Xfa, out of scope.
    Private Shared Function ResolveAdobePath() As String
        Try
            For Each exe As String In New String() {"Acrobat.exe", "AcroRd32.exe"}
                Dim p As String = ReadAppPath(exe)
                If Not String.IsNullOrEmpty(p) AndAlso File.Exists(p) Then Return p
            Next

            Dim candidates As String() = {
                "Adobe\Acrobat DC\Acrobat\Acrobat.exe",
                "Adobe\Acrobat Reader DC\Reader\AcroRd32.exe",
                "Adobe\Acrobat\Acrobat.exe"
            }
            For Each pf As String In New String() {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}
                If String.IsNullOrEmpty(pf) Then Continue For
                For Each c As String In candidates
                    Dim full As String = Path.Combine(pf, c)
                    If File.Exists(full) Then Return full
                Next
            Next
            Return Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.ResolveAdobePath", ex)
            Return Nothing
        End Try
    End Function

    Private Shared Function ReadAppPath(exeName As String) As String
        Try
            Using k As RegistryKey = Registry.LocalMachine.OpenSubKey(
                "SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" & exeName)
                If k IsNot Nothing Then Return TryCast(k.GetValue(Nothing), String)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.ReadAppPath", ex)
        End Try
        Return Nothing
    End Function

    ' ══ Diagnostic — child window probe ═════════════════════════════════════════
    Private Sub btnProbe_Click(sender As Object, e As EventArgs) Handles btnProbe.Click
        Try
            ProbeChildren()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnProbe_Click", ex)
        End Try
    End Sub

    ' Walks the hosted window's descendants (GW_CHILD/GW_HWNDNEXT recursion, depth-limited),
    ' logging one line per node and filling lstChildren + _lastProbe. The RHP candidate is a
    ' HEURISTIC: visible, flush against the host's right edge (± tolerance) and narrower than
    ' half the host width; the widest such child wins and feeds "Măsoară din probă".
    Private Sub ProbeChildren()
        If _hostedWindow = IntPtr.Zero Then
            ShowStatus("Nicio fereastră găzduită — încorporează întâi un PDF.")
            Return
        End If
        lstChildren.Items.Clear()
        _lastProbe.Clear()
        _probeCandidateWidth = 0
        _probeCandidateClass = Nothing

        Dim hostW As Integer = pnlHost.ClientSize.Width
        Dim total As Integer = 0
        RhpLog($"── Probă ferestre copil (hwnd gazdă 0x{_hostedWindow.ToInt64():X}, adâncime max {PROBE_MAX_DEPTH}; " &
               "EURISTIC candidat RHP = vizibil, lipit de marginea dreaptă ±" & RHP_EDGE_TOLERANCE.ToString() &
               "px, lățime < 1/2 gazdă) ──")
        WalkChildren(_hostedWindow, 1, hostW, total)

        ' The line that actually matters, per target the loaded scenario asks to hide — printed
        ' whether or not it matched, so it cannot be lost in the tree above.
        Dim targets As List(Of String) = ScenarioHideTargets()
        Dim zeroSizedTarget As Boolean = False
        For Each t As String In targets
            Dim hit As ChildWindowItem = _lastProbe.FirstOrDefault(
                Function(i) String.Equals(i.WindowText, t, StringComparison.OrdinalIgnoreCase))
            If hit Is Nothing Then
                RhpLog($"PANOU: {t} — NEGĂSIT")
            Else
                Dim vis As Boolean = IsWindowVisible(hit.Hwnd)
                ' Rectangles of invisible windows are unreliable — Adobe leaves stale geometry
                ' behind (e.g. a 27x913 child inside a 587-high host). Mark, never trust.
                Dim stale As String = If(vis, "", " (posibil învechit)")
                RhpLog($"PANOU: {t} — {hit.W}x{hit.H}{stale}, vis={If(vis, 1, 0)}")
                If hit.W <= 0 OrElse hit.H <= 0 Then zeroSizedTarget = True
            End If
        Next

        Dim summary As String
        If _probeCandidateWidth > 0 Then
            summary = $"{total} ferestre copil; candidat RHP (EURISTIC): {_probeCandidateClass}, lățime {_probeCandidateWidth}px."
        ElseIf zeroSizedTarget Then
            summary = $"{total} ferestre copil; niciun candidat RHP — dar ținta cerută EXISTĂ cu dimensiune ZERO " &
                      "(panoul e deja gol: probabil bază de pornire contaminată, nu un rezultat)."
        Else
            summary = $"{total} ferestre copil; niciun candidat RHP după euristic."
        End If
        RhpLog(summary)
        ShowStatus(summary)
        UpdateActionStates()
    End Sub

    ' The texts the loaded scenario wants hidden (empty when none is loaded).
    Private Function ScenarioHideTargets() As List(Of String)
        If _scenario Is Nothing OrElse _scenario.HideChildren Is Nothing OrElse
           _scenario.HideChildren.ByText Is Nothing Then Return New List(Of String)()
        Return _scenario.HideChildren.ByText.Where(Function(t) Not String.IsNullOrWhiteSpace(t)).ToList()
    End Function

    Private Sub WalkChildren(parent As IntPtr, depth As Integer, hostW As Integer, ByRef total As Integer)
        Dim child As IntPtr = GetWindow(parent, GW_CHILD)
        While child <> IntPtr.Zero
            total += 1
            Dim cls As String = GetClass(child)
            Dim fullText As String = GetTitle(child)
            Dim txt As String = fullText
            If txt.Length > 40 Then txt = txt.Substring(0, 40) & "…"
            Dim r As RECT
            GetWindowRect(child, r)
            Dim tl As Point = pnlHost.PointToClient(New Point(r.Left, r.Top))
            Dim w As Integer = r.Right - r.Left
            Dim h As Integer = r.Bottom - r.Top
            Dim vis As Boolean = IsWindowVisible(child)
            Dim style As Long = GetWindowLongPtrSafe(child, GWL_STYLE).ToInt64()
            Dim exStyle As Long = GetWindowLongPtrSafe(child, GWL_EXSTYLE).ToInt64()

            RhpLog($"  d={depth} hwnd=0x{child.ToInt64():X} cls={cls} text=""{txt}"" " &
                   $"x={tl.X} y={tl.Y} {w}x{h} vis={If(vis, 1, 0)} style=0x{style:X8} ex=0x{exStyle:X8}")
            Dim item As New ChildWindowItem(child, cls, fullText, w, h)
            lstChildren.Items.Add(item)
            _lastProbe.Add(item)

            ' RHP-candidate heuristic (labelled as such in the log header above).
            If vis AndAlso w > 0 AndAlso w < hostW \ 2 AndAlso
               Math.Abs((tl.X + w) - hostW) <= RHP_EDGE_TOLERANCE Then
                If w > _probeCandidateWidth Then
                    _probeCandidateWidth = w
                    _probeCandidateClass = cls
                End If
            End If

            If depth < PROBE_MAX_DEPTH Then WalkChildren(child, depth + 1, hostW, total)
            child = GetWindow(child, GW_HWNDNEXT)
        End While
    End Sub

    ' A probed child: text first, because the text is the durable identity across relaunches while
    ' the class is almost always AVL_AVView and the HWND changes every launch.
    Private NotInheritable Class ChildWindowItem
        Public ReadOnly Hwnd As IntPtr
        Public ReadOnly ClassName As String
        Public ReadOnly WindowText As String
        Public ReadOnly W As Integer
        Public ReadOnly H As Integer

        Public Sub New(hwnd As IntPtr, className As String, windowText As String, w As Integer, h As Integer)
            Me.Hwnd = hwnd
            Me.ClassName = className
            Me.WindowText = If(windowText, "")
            Me.W = w
            Me.H = h
        End Sub

        Public Overrides Function ToString() As String
            Dim t As String = If(String.IsNullOrEmpty(WindowText), "(fără text)", WindowText)
            Return $"{t} — {ClassName} ({W}x{H})"
        End Function
    End Class

    ' ══ Decupare — geometry clipping (live, no relaunch/kill) ═══════════════════
    Private Sub ClipSettingsChanged(sender As Object, e As EventArgs) _
        Handles chkClip.CheckedChanged, numClipRight.ValueChanged, numClipTop.ValueChanged
        Try
            If _loading Then Return
            LayoutHostedWindow()
            NudgeRedraw()
            RefreshStatusBlock()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.ClipSettingsChanged", ex)
        End Try
    End Sub

    Private Sub btnClipAuto_Click(sender As Object, e As EventArgs) Handles btnClipAuto.Click
        Try
            If _probeCandidateWidth <= 0 Then
                ShowStatus("Rulează întâi proba (Arborele de ferestre copil) — niciun candidat măsurat.")
                Return
            End If
            numClipRight.Value = Math.Min(numClipRight.Maximum, CDec(_probeCandidateWidth))
            If Not chkClip.Checked Then chkClip.Checked = True
            LayoutHostedWindow()
            NudgeRedraw()
            RhpLog($"Decupare setată din probă: dreapta={_probeCandidateWidth}px ({_probeCandidateClass}).")
            RefreshStatusBlock()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnClipAuto_Click", ex)
        End Try
    End Sub

    ' ══ Ferestre copil — hide/show a probed child directly ══════════════════════
    Private Sub btnHideChild_Click(sender As Object, e As EventArgs) Handles btnHideChild.Click
        Try
            Dim item As ChildWindowItem = SelectedChildAlive()
            If item Is Nothing Then Return
            HideChild(item)
            NudgeRedraw()
            UpdateActionStates()
            RefreshStatusBlock()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnHideChild_Click", ex)
        End Try
    End Sub

    ' Hides one child and remembers BOTH its handle (for exact restore now) and its text (so a
    ' saved scenario can find it again after the next launch).
    Private Sub HideChild(item As ChildWindowItem)
        ShowWindow(item.Hwnd, SW_HIDE)
        If Not _hiddenChildren.Contains(item.Hwnd) Then _hiddenChildren.Add(item.Hwnd)
        If Not String.IsNullOrEmpty(item.WindowText) AndAlso Not _hiddenChildTexts.Contains(item.WindowText) Then
            _hiddenChildTexts.Add(item.WindowText)
        End If
        RhpLog($"Ascuns copil: {item} (hwnd=0x{item.Hwnd.ToInt64():X})")
    End Sub

    Private Sub btnShowChild_Click(sender As Object, e As EventArgs) Handles btnShowChild.Click
        Try
            Dim item As ChildWindowItem = SelectedChildAlive()
            If item Is Nothing Then Return
            ShowWindow(item.Hwnd, SW_SHOW)
            _hiddenChildren.Remove(item.Hwnd)
            _hiddenChildTexts.Remove(item.WindowText)
            RhpLog($"Rearătat copil: {item} (hwnd=0x{item.Hwnd.ToInt64():X})")
            NudgeRedraw()
            UpdateActionStates()
            RefreshStatusBlock()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnShowChild_Click", ex)
        End Try
    End Sub

    Private Sub btnShowAllChildren_Click(sender As Object, e As EventArgs) Handles btnShowAllChildren.Click
        Try
            For Each h As IntPtr In _hiddenChildren.ToList()
                If IsWindow(h) Then
                    ShowWindow(h, SW_SHOW)
                    RhpLog($"Rearătat: 0x{h.ToInt64():X}")
                Else
                    ' Stale handle (Adobe destroys and recreates panels) — drop, do not throw.
                    RhpLog($"Handle mort eliminat din listă: 0x{h.ToInt64():X}")
                End If
            Next
            _hiddenChildren.Clear()
            _hiddenChildTexts.Clear()
            NudgeRedraw()
            UpdateActionStates()
            RefreshStatusBlock()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnShowAllChildren_Click", ex)
        End Try
    End Sub

    ' Selected list item whose HWND is still alive; stale handles are dropped with a log line.
    Private Function SelectedChildAlive() As ChildWindowItem
        Dim item As ChildWindowItem = TryCast(lstChildren.SelectedItem, ChildWindowItem)
        If item Is Nothing Then
            ShowStatus("Selectează întâi o fereastră din listă (rulează proba dacă lista e goală).")
            Return Nothing
        End If
        If Not IsWindow(item.Hwnd) Then
            RhpLog($"Handle mort (fereastra a fost distrusă între timp): {item} — eliminat din listă.")
            lstChildren.Items.Remove(item)
            _lastProbe.Remove(item)
            _hiddenChildren.Remove(item.Hwnd)
            UpdateActionStates()
            Return Nothing
        End If
        Return item
    End Function

    ' Re-probes and hides everything whose window text matches, retrying because Adobe creates the
    ' task pane host AFTER the main view: a single attempt right after embed often finds nothing.
    ' Every attempt logs "n of m found" — exactly the diagnostic needed when a scenario stops
    ' working on a different Adobe build.
    Private Async Function HideChildrenByTextAsync(cfg As HideChildrenConfig) As Task(Of Boolean)
        If cfg Is Nothing OrElse cfg.ByText Is Nothing OrElse cfg.ByText.Count = 0 Then
            RhpLog("hideChildren: lista «byText» este goală — nimic de ascuns.")
            Return True
        End If
        Dim attempts As Integer = cfg.EffectiveAttempts()
        Dim intervalMs As Integer = cfg.EffectiveIntervalMs()
        Dim wanted As List(Of String) = cfg.ByText.Where(Function(t) Not String.IsNullOrWhiteSpace(t)).ToList()
        Dim lastSummary As HideAttemptSummary = Nothing

        For attempt As Integer = 1 To attempts
            If _hostedWindow = IntPtr.Zero Then
                RhpLog("hideChildren: nicio fereastră găzduită — abandonez.")
                Return False
            End If
            ProbeChildren()

            Dim outcomes As New List(Of HideOutcome)()
            For Each text As String In wanted
                Dim matches As List(Of ChildWindowItem) = _lastProbe.
                    Where(Function(i) String.Equals(i.WindowText, text, StringComparison.OrdinalIgnoreCase)).ToList()
                If matches.Count = 0 Then
                    outcomes.Add(HideOutcome.NotFound)
                    RhpLog($"  {HideOutcomeClassifier.Label(HideOutcome.NotFound)}: «{text}»")
                    Continue For
                End If
                For Each m As ChildWindowItem In matches
                    If Not IsWindow(m.Hwnd) Then Continue For
                    ' Classify BEFORE touching it — afterwards everything looks hidden, which is
                    ' precisely how a no-op used to be logged as a success.
                    Dim outcome As HideOutcome = HideOutcomeClassifier.Classify(
                        found:=True, visible:=IsWindowVisible(m.Hwnd), width:=m.W, height:=m.H)
                    outcomes.Add(outcome)
                    RhpLog($"  {HideOutcomeClassifier.Label(outcome)}: «{m.WindowText}» " &
                           $"{m.W}x{m.H} vis={If(IsWindowVisible(m.Hwnd), 1, 0)} (hwnd=0x{m.Hwnd.ToInt64():X})")
                    If outcome = HideOutcome.Hidden Then HideChild(m)
                Next
            Next

            lastSummary = New HideAttemptSummary(wanted.Count, outcomes)
            RhpLog(lastSummary.SummaryLine(attempt, attempts))
            NudgeRedraw()
            UpdateActionStates()

            ' Stop early only on a REAL hide. A run that only ever sees «deja ascuns»/«zero» burns
            ' through every attempt and says so — that pattern is the signature of a contaminated
            ' baseline, not of success.
            If lastSummary.HiddenCount > 0 Then
                ShowStatus($"hideChildren: {lastSummary.HiddenCount} fereastră(e) ascunse efectiv.")
                Return True
            End If
            If attempt < attempts Then Await Task.Delay(intervalMs)
        Next

        If lastSummary IsNot Nothing AndAlso lastSummary.FoundCount > 0 Then
            Dim warn As String =
                $"ATENȚIE: după {attempts} încercări nu s-a ascuns NIMIC — ferestrele cerute erau deja " &
                "ascunse sau de dimensiune zero. Pasul nu a schimbat nimic, deci NU dovedește nimic " &
                "despre mecanismul de ascundere (bază de pornire contaminată?)."
            RhpLog(warn)
            ShowStatus(warn)
        Else
            RhpLog($"hideChildren: după {attempts} încercări niciun text cerut nu a fost găsit " &
                   "(rezultat valid de notat — panoul poate lipsi pe acest build Adobe).")
            ShowStatus("hideChildren: niciun text cerut nu a fost găsit în arborele de ferestre.")
        End If
        Return True
    End Function

    ' ══ Scurtături — keyboard toggles (experimental) ════════════════════════════
    Private Sub btnSendShiftF4_Click(sender As Object, e As EventArgs) Handles btnSendShiftF4.Click
        Try
            SendKeyToHosted("+{F4}", "Shift+F4")
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnSendShiftF4_Click", ex)
        End Try
    End Sub

    Private Sub btnSendF4_Click(sender As Object, e As EventArgs) Handles btnSendF4.Click
        Try
            SendKeyToHosted("{F4}", "F4")
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnSendF4_Click", ex)
        End Try
    End Sub

    ' Focus the hosted window, then send the keystroke. EXPERIMENTAL and version-dependent:
    ' keyboard across a process boundary into a reparented window does not behave like a native
    ' child. "Nothing happened" is a valid result to record — no retries, no workarounds.
    Private Sub SendKeyToHosted(keys As String, label As String)
        If _hostedWindow = IntPtr.Zero Then
            ShowStatus("Nicio fereastră găzduită — încorporează întâi un PDF.")
            Return
        End If
        Activate()
        SetFocus(_hostedWindow)
        SendKeys.SendWait(keys)
        RhpLog($"Trimis {label} către fereastra găzduită (EXPERIMENTAL, dependent de versiune; " &
               "tastatura peste granița de proces nu se comportă ca la un copil nativ). " &
               "Dacă nu se întâmplă nimic, notează asta ca rezultat valid.")
        ShowStatus($"Trimis {label}. Observă dacă panoul s-a comutat.")
    End Sub

    ' Translates a scenario key spec ("Shift+F4", "F4", "Ctrl+Shift+P") into SendKeys syntax.
    Private Shared Function ToSendKeysSyntax(spec As String) As String
        If String.IsNullOrWhiteSpace(spec) Then Return ""
        Dim prefix As New StringBuilder()
        Dim key As String = spec.Trim()
        Do
            Dim plus As Integer = key.IndexOf("+"c)
            If plus <= 0 Then Exit Do
            Dim modifier As String = key.Substring(0, plus).Trim().ToLowerInvariant()
            Select Case modifier
                Case "shift" : prefix.Append("+"c)
                Case "ctrl", "control" : prefix.Append("^"c)
                Case "alt" : prefix.Append("%"c)
                Case Else : Exit Do
            End Select
            key = key.Substring(plus + 1).Trim()
        Loop
        ' Function keys and named keys need braces; a single character does not.
        If key.Length > 1 AndAlso Not key.StartsWith("{", StringComparison.Ordinal) Then
            key = "{" & key.ToUpperInvariant() & "}"
        End If
        Return prefix.ToString() & key
    End Function

    ' ══ Preferințe Adobe (utilizator, HKCU) ═════════════════════════════════════
    Private Sub cboHive_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboHive.SelectedIndexChanged
        Try
            If _loading Then Return
            RefreshStatusBlock()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.cboHive_SelectedIndexChanged", ex)
        End Try
    End Sub

    ' The AVGeneral hive to write: explicit combo choice, or the pure resolver on "auto".
    ' Combo only — a loaded scenario has already selected it (see ApplyScenarioToControls).
    Private Function CurrentAvGeneralPath() As String
        Dim sel As String = TryCast(cboHive.SelectedItem, String)
        If sel = AdobeRegistryConstants.AvGeneralReader OrElse sel = AdobeRegistryConstants.AvGeneralAcrobat Then
            Return sel
        End If
        Dim res As AdobeHiveResolution = AdobeHiveResolver.Resolve(
            _regAccess.KeyExists(AdobeRegistryConstants.AvGeneralReader),
            _regAccess.KeyExists(AdobeRegistryConstants.AvGeneralAcrobat),
            _adobePath)
        Return res.AvGeneralPath
    End Function

    ' The HKCU values to write, as LITERAL intents.
    '
    ' The scenario wins: its values go to the registry exactly as written in the file (integer ->
    ' REG_DWORD, string -> REG_SZ, null -> delete), and the value NAMES are open — anything under
    ' the resolved AVGeneral hive can be driven from a file without a code change. The checkboxes
    ' are a manual shortcut for the common case and are merged in only for names the scenario does
    ' not mention (or for all of them when no scenario is loaded).
    '
    ' This replaces routing scenario values through the checkboxes, which clamped them: a file
    ' asking for `"bEnableAv2": 1` came out as 0 and the log recorded `0 (DWord) -> 0 (DWord)`.
    Private Function CollectUserPrefs() As List(Of UserPrefIntent)
        Dim fromScenario As New List(Of UserPrefIntent)()
        If _scenario IsNot Nothing AndAlso _scenario.UserPrefs IsNot Nothing Then
            Dim rejected As List(Of String) = Nothing
            fromScenario = UserPrefIntentFactory.FromValues(_scenario.UserPrefs.Values, rejected)
            If rejected IsNot Nothing Then
                For Each r As String In rejected
                    RhpLog(r)
                Next
            End If
        End If
        Return UserPrefIntentFactory.Merge(fromScenario, CollectPanelUserPrefs())
    End Function

    ' The four panel rows, each carrying whatever the operator typed or picked. A row on
    ' «nu atinge» contributes nothing; «șterge» contributes a deletion.
    Private Function CollectPanelUserPrefs() As List(Of UserPrefIntent)
        Return ParsePrefRows().Where(Function(p) p.Intent IsNot Nothing).
                               Select(Function(p) p.Intent).ToList()
    End Function

    ' Order: (1) snapshot ticked values ONCE per session (Capture is idempotent, so a second
    ' Apply cannot overwrite the true originals); (2) kill Adobe — ours by PID plus, with CONSENT
    ' ONLY, any foreign instance (Adobe rewrites its prefs on exit, the write is worthless while
    ' it runs); (3) write, logging old → new. Shared by the button and the scenario step.
    Private Function ApplyUserPrefsCore() As Boolean
        Dim bad As List(Of String) = InvalidPrefRows()
        If bad.Count > 0 Then
            For Each m As String In bad
                RhpLog("HKCU rând invalid: " & m)
            Next
            ShowStatus("Rând HKCU invalid — nu s-a scris nimic.")
            MessageBox.Show(Me,
                "Un rând din «Preferințe Adobe» nu poate fi interpretat:" & Environment.NewLine &
                Environment.NewLine & String.Join(Environment.NewLine, bad) & Environment.NewLine &
                Environment.NewLine &
                "Alege «nu atinge», «șterge» sau un număr întreg. Nu s-a scris nimic în registry.",
                "K-BOT — valoare HKCU invalidă", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End If

        Dim prefs As List(Of UserPrefIntent) = CollectUserPrefs()
        If prefs.Count = 0 Then
            ShowStatus("Toate rândurile HKCU sunt pe «nu atinge» — nimic de aplicat.")
            Return True
        End If
        Dim hive As String = CurrentAvGeneralPath()

        For Each p As UserPrefIntent In prefs
            _userSnapshot.Capture(hive, p.Name)
        Next

        If Not KillAdobeForRegistryWrite() Then Return False

        For Each p As UserPrefIntent In prefs
            Dim oldSnap As RegistryValueSnapshot = _regAccess.Read(hive, p.Name)
            If p.Action = UserPrefAction.Delete Then
                _regAccess.DeleteValue(hive, p.Name)
            Else
                _regAccess.Write(hive, p.Name, p.Kind, p.Value)
            End If

            ' READ BACK and compare. A preference that will not stick is a result worth stopping
            ' for — this is the check whose absence let four runs "pass" while testing nothing.
            Dim actual As RegistryValueSnapshot = _regAccess.Read(hive, p.Name)
            Dim v As WriteVerification = RegistryWriteVerifier.Verify(hive, p, actual)
            RhpLog($"HKCU: {oldSnap} => {v.Message}")
            If Not v.Matches Then
                RefreshPrefsGrid()
                ShowStatus("Scriere HKCU neconfirmată — vezi " & RHP_LOG_NAME & ".")
                MessageBox.Show(Me,
                    "Valoarea nu a rămas scrisă în registry:" & Environment.NewLine & Environment.NewLine &
                    v.Message & Environment.NewLine & Environment.NewLine &
                    "Scenariul se oprește aici — o preferință care nu se aplică ar face rezultatele " &
                    "neconcludente.",
                    "K-BOT — scriere registry neconfirmată", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
            End If
            Dim label As String = p.Name & "=" & p.RequestedText()
            If Not _userValuesApplied.Contains(label) Then _userValuesApplied.Add(label)
        Next
        RefreshPrefsGrid()
        Return True
    End Function

    Private Async Sub btnApplyUser_Click(sender As Object, e As EventArgs) Handles btnApplyUser.Click
        Try
            If Not ConfirmRegistryWrites(userPrefs:=True, machinePolicy:=False) Then Return
            If Not ApplyUserPrefsCore() Then Return
            If Not String.IsNullOrEmpty(EffectivePdfPath()) Then Await RelaunchAsync()
            ShowStatus("Valori HKCU aplicate; Adobe repornit cu setul curent de switch-uri.")
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnApplyUser_Click", ex)
            ShowStatus("Eroare la aplicarea valorilor HKCU: " & ex.Message)
        End Try
    End Sub

    Private Function RestoreUserPrefsCore() As Boolean
        If _userSnapshot.Count = 0 Then
            ShowStatus("Nimic de restaurat — nu s-a aplicat nicio valoare în această sesiune.")
            Return True
        End If
        If Not KillAdobeForRegistryWrite() Then Return False
        For Each s As RegistryValueSnapshot In _userSnapshot.Snapshots()
            RhpLog("HKCU restaurez la original: " & s.ToString())
        Next
        _userSnapshot.RestoreAll()
        _userValuesApplied.Clear()
        Return True
    End Function

    Private Async Sub btnRestoreUser_Click(sender As Object, e As EventArgs) Handles btnRestoreUser.Click
        Try
            If Not RestoreUserPrefsCore() Then Return
            If Not String.IsNullOrEmpty(EffectivePdfPath()) Then Await RelaunchAsync()
            ShowStatus("Valorile HKCU au fost restaurate la original.")
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnRestoreUser_Click", ex)
            ShowStatus("Eroare la restaurare: " & ex.Message)
        End Try
    End Sub

    ' Kills OUR hosted instance by PID (existing path), then asks — in Romanian, never silently —
    ' before killing foreign Adobe processes. Returns False when the operator declines.
    Private Function KillAdobeForRegistryWrite() As Boolean
        KillTracked()
        Dim foreign As New List(Of Process)()
        For Each name As String In New String() {"Acrobat", "AcroRd32"}
            foreign.AddRange(Process.GetProcessesByName(name))
        Next
        Try
            Dim alive As List(Of Process) = foreign.Where(Function(p) Not HasExitedSafe(p)).ToList()
            If alive.Count > 0 Then
                Dim r As DialogResult = MessageBox.Show(Me,
                    $"Există {alive.Count} proces(e) Adobe deschise în afara bancului de probă." & Environment.NewLine &
                    "Adobe își rescrie preferințele la ieșire, deci scrierea în registry ar fi suprascrisă." & Environment.NewLine &
                    "Le închid forțat acum? (Documentele nesalvate din acele ferestre se pierd.)",
                    "K-BOT — procese Adobe externe", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                If r <> DialogResult.Yes Then
                    ShowStatus("Operație anulată — au rămas procese Adobe deschise.")
                    RhpLog("Scriere registry anulată de operator: procese Adobe externe rămase deschise.")
                    Return False
                End If
                For Each p As Process In alive
                    Try
                        If Not p.HasExited Then p.Kill(True)
                        RhpLog($"Proces Adobe extern închis forțat: PID {p.Id}.")
                    Catch ex As Exception
                        GlobalErrorLog.Write("AdobeReaderHarnessForm.KillAdobeForRegistryWrite.Kill", ex)
                    End Try
                Next
            End If
            Return True
        Finally
            For Each p As Process In foreign
                Try
                    p.Dispose()
                Catch
                End Try
            Next
        End Try
    End Function

    Private Shared Function HasExitedSafe(p As Process) As Boolean
        Try
            Return p.HasExited
        Catch
            Return True
        End Try
    End Function

    ' On-close restore: bounded by construction — our Adobe is already killed and the restore is a
    ' handful of registry writes. DEVIATION: foreign Adobe processes are NOT killed here (no
    ' consent dialog on close — never silently); if any run, their exit may overwrite the restore,
    ' which is logged. On failure the operator is told (status + MessageBox, since the label alone
    ' would never be seen on a closing form) with the exact keys named for manual cleanup.
    Private Sub RestoreUserPrefsOnClose()
        If _userValuesApplied.Count = 0 Then Return
        If Not chkRestoreOnClose.Checked Then
            ' Deliberate: the operator wants the experiment to survive the bench closing. Say so,
            ' because the next session's baseline check will read exactly these values.
            RhpLog("Restaurare la închidere DEZACTIVATĂ — valorile HKCU rămân aplicate: " &
                   String.Join(", ", _userValuesApplied) &
                   ". Ele vor apărea ca stare de pornire la următoarea rulare.")
            Return
        End If
        Try
            Dim foreignCount As Integer =
                Process.GetProcessesByName("Acrobat").Length + Process.GetProcessesByName("AcroRd32").Length
            If foreignCount > 0 Then
                RhpLog($"ATENȚIE: {foreignCount} proces(e) Adobe încă deschise la închiderea bancului — " &
                       "restaurarea HKCU poate fi suprascrisă când acestea se închid.")
            End If
            _userSnapshot.RestoreAll()
            RhpLog($"Restaurare la închidere: {_userSnapshot.Count} valori HKCU readuse la original.")
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.RestoreUserPrefsOnClose", ex)
            Dim keys As String = String.Join(Environment.NewLine,
                _userSnapshot.Snapshots().Select(Function(s) s.Path & "\" & s.Name))
            Try
                lblStatus.Text = "Restaurarea automată a eșuat — curățare manuală necesară (vezi mesajul)."
                RhpLog("EȘEC restaurare la închidere: " & ex.Message & " — curățare manuală la: " & keys.Replace(Environment.NewLine, "; "))
            Catch logEx As Exception
                GlobalErrorLog.Write("AdobeReaderHarnessForm.RestoreUserPrefsOnClose.Log", logEx)
            End Try
            MessageBox.Show(Me,
                "Restaurarea automată a valorilor Adobe a eșuat. Curățare manuală necesară la:" &
                Environment.NewLine & keys,
                "K-BOT — restaurare registry", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ' ══ Politici Adobe (mașină, HKLM) — elevated reg.exe import ═════════════════
    Private Sub cboProduct_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboProduct.SelectedIndexChanged
        Try
            If _loading Then Return
            RefreshStatusBlock()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.cboProduct_SelectedIndexChanged", ex)
        End Try
    End Sub

    ' The FeatureLockDown <product>: explicit combo choice, else the pure resolver on "auto".
    ' Combo only — a loaded scenario has already selected it.
    Private Function CurrentPolicyProduct() As String
        Dim sel As String = TryCast(cboProduct.SelectedItem, String)
        If sel = AdobeRegistryConstants.ProductReader OrElse sel = AdobeRegistryConstants.ProductAcrobat Then
            Return sel
        End If
        Dim res As AdobeHiveResolution = AdobeHiveResolver.Resolve(
            _regAccess.KeyExists(AdobeRegistryConstants.AvGeneralReader),
            _regAccess.KeyExists(AdobeRegistryConstants.AvGeneralAcrobat),
            _adobePath)
        Return res.PolicyProduct
    End Function

    ' One HKLM policy value: its full section path, its name and the value to write.
    Private NotInheritable Class PolicyEntry
        Public ReadOnly SectionPath As String
        Public ReadOnly Name As String
        Public ReadOnly Value As UInteger

        Public Sub New(sectionPath As String, name As String, value As UInteger)
            Me.SectionPath = sectionPath
            Me.Name = name
            Me.Value = value
        End Sub
    End Class

    ' The policy values to write — from the CHECKBOXES only. A loaded scenario has already ticked
    ' them, so the panel is what runs.
    Private Function CollectPolicyEntries(product As String) As List(Of PolicyEntry)
        Dim fld As String = AdobeRegistryConstants.FeatureLockDownPath(product)
        Dim entries As New List(Of PolicyEntry)()
        If chkSuppressUpsell.Checked Then
            entries.Add(New PolicyEntry(fld, AdobeRegistryConstants.ValSuppressUpsell, 1UI))
        End If
        If chkDisableServices.Checked Then
            entries.Add(New PolicyEntry(AdobeRegistryConstants.CServicesPath(product),
                                        AdobeRegistryConstants.ValToggleServices, 1UI))
        End If
        Return entries
    End Function

    ' Generates the apply/revert .reg pair into AppDir\Logs\ (revert from PRE-apply reads:
    ' absent -> deletion line, present dword -> original value), then imports the apply file via
    ' elevated reg.exe. Reading HKLM for the snapshot needs no elevation; only writing does.
    Private Async Function ApplyMachinePolicyCoreAsync() As Task(Of Boolean)
        Dim product As String = CurrentPolicyProduct()
        Dim entries As List(Of PolicyEntry) = CollectPolicyEntries(product)
        If entries.Count = 0 Then
            ShowStatus("Nicio politică HKLM bifată — nimic de aplicat.")
            Return True
        End If

        Dim apply As New RegFileBuilder()
        Dim revert As New RegFileBuilder()
        ' Pre-apply snapshot, kept in the marker file so a revert can be reconstructed by hand if
        ' the .reg goes missing.
        Dim preApply As New List(Of String)()
        For Each en As PolicyEntry In entries
            preApply.Add(AddPolicyValue(apply, revert, en))
        Next

        Dim logsDir As String = Path.Combine(AppContext.BaseDirectory, "Logs")
        Directory.CreateDirectory(logsDir)
        _applyRegPath = Path.Combine(logsDir, "adobe_policy_apply.reg")
        _revertRegPath = Path.Combine(logsDir, "adobe_policy_revert.reg")
        ' reg.exe expects UTF-16 for "Version 5.00" files.
        File.WriteAllText(_applyRegPath, apply.Build(), Encoding.Unicode)
        File.WriteAllText(_revertRegPath, revert.Build(), Encoding.Unicode)
        RhpLog($"Fișiere .reg generate (produs «{product}»): {_applyRegPath} + {_revertRegPath}")

        Dim ok As Boolean = Await RunRegImportAsync(_applyRegPath)
        If ok Then
            _machinePolicyApplied = True
            ' Persist the fact of the apply. An elevated apply with nothing remembering it is how
            ' the machine stayed contaminated across a whole day.
            Try
                WriteMarker(New MachineStateMarker() With {
                    .PolicyApplied = True,
                    .Product = product,
                    .AppliedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    .RevertRegFile = _revertRegPath,
                    .PreApply = preApply})
                RhpLog($"Marcaj de stare scris: {MarkerPath()}")
            Catch ex As Exception
                GlobalErrorLog.Write("AdobeReaderHarnessForm.ApplyMachinePolicyCoreAsync.Marker", ex)
                RhpLog("ATENȚIE: marcajul de stare NU a putut fi scris — revocarea automată la " &
                       "închidere nu va ști că politica e aplicată.")
            End Try
            ShowStatus("Politica HKLM aplicată. Repornește Adobe (Reîncorporează) pentru efect.")
        End If
        RefreshStatusBlock()
        Return ok
    End Function

    Private Async Sub btnApplyMachine_Click(sender As Object, e As EventArgs) Handles btnApplyMachine.Click
        Try
            If Not ConfirmRegistryWrites(userPrefs:=False, machinePolicy:=True) Then Return
            Await ApplyMachinePolicyCoreAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnApplyMachine_Click", ex)
            ShowStatus("Eroare la aplicarea politicii HKLM: " & ex.Message)
        End Try
    End Sub

    Private Async Function RevertMachinePolicyCoreAsync() As Task(Of Boolean)
        ' Fall back to the path recorded in the marker: a revert may be needed in a LATER session,
        ' when _revertRegPath is empty because this run never applied anything.
        Dim regFile As String = _revertRegPath
        If String.IsNullOrEmpty(regFile) OrElse Not File.Exists(regFile) Then
            Dim r As MachineStateMarkerResult = ReadMarker()
            If r.Status = MarkerReadStatus.Present AndAlso r.Marker IsNot Nothing Then
                regFile = r.Marker.RevertRegFile
            End If
        End If
        If String.IsNullOrEmpty(regFile) OrElse Not File.Exists(regFile) Then
            ShowStatus("Nu există fișier de revocare — aplică întâi politica (el se generează atunci).")
            Return True
        End If
        Dim ok As Boolean = Await RunRegImportAsync(regFile)
        If ok Then
            _machinePolicyApplied = False
            ' Verify the revert actually landed before forgetting about it.
            Dim stillActive As Integer = Enumerable.Count(ReadPolicyState(), Function(p) p.Present)
            If stillActive = 0 Then
                ClearMarker()
                RhpLog("Politica HKLM revocată și verificată (nicio valoare rămasă). Marcaj șters.")
                ShowStatus("Politica HKLM a fost revocată (verificat: nicio valoare rămasă).")
            Else
                RhpLog($"ATENȚIE: după revocare au rămas {stillActive} valori de politică active — " &
                       "marcajul de stare rămâne pus.")
                ShowStatus($"Revocare incompletă: {stillActive} valori de politică încă active.")
            End If
        End If
        RefreshStatusBlock()
        Return ok
    End Function

    Private Async Sub btnRevertMachine_Click(sender As Object, e As EventArgs) Handles btnRevertMachine.Click
        Try
            Await RevertMachinePolicyCoreAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnRevertMachine_Click", ex)
            ShowStatus("Eroare la revocarea politicii HKLM: " & ex.Message)
        End Try
    End Sub

    ' Adds one policy value to the apply builder and its pre-apply original to the revert builder.
    ' Absent -> deletion line; present DWORD -> original value; present with another kind ->
    ' string when possible, otherwise logged as needing manual revert (never guessed).
    Private Function AddPolicyValue(apply As RegFileBuilder, revert As RegFileBuilder,
                                    entry As PolicyEntry) As String
        apply.AddDword(entry.SectionPath, entry.Name, entry.Value)
        Dim snap As RegistryValueSnapshot = _regAccess.Read(entry.SectionPath, entry.Name)
        RhpLog("HKLM citit (pentru revocare): " & snap.ToString())
        If snap.Presence = RegPresence.Absent Then
            revert.DeleteValue(entry.SectionPath, entry.Name)
        ElseIf snap.Kind = RegistryValueKind.DWord Then
            revert.AddDword(entry.SectionPath, entry.Name, DwordToUInt(snap.Value))
        ElseIf snap.Kind = RegistryValueKind.String Then
            revert.AddString(entry.SectionPath, entry.Name, CStr(snap.Value))
        Else
            RhpLog($"ATENȚIE: {entry.SectionPath}\{entry.Name} are tipul {snap.Kind}, nereprezentabil în " &
                   ".reg-ul de revocare — revocarea acestei valori va trebui făcută manual.")
        End If
        Return snap.ToString()
    End Function

    ' Registry DWORDs surface as Integer (possibly negative for high values) — normalize.
    Private Shared Function DwordToUInt(value As Object) As UInteger
        Return CUInt(CLng(CType(value, Integer)) And &HFFFFFFFFL)
    End Function

    ' Imports a .reg file through elevated reg.exe (Verb=runas, UseShellExecute=True — the
    ' harness itself is never elevated). UAC cancel (ERROR_CANCELLED 1223) is handled explicitly:
    ' Romanian message, full detail in the log, no stack trace in the operator's face.
    Private Async Function RunRegImportAsync(regFile As String) As Task(Of Boolean)
        Try
            Dim psi As New ProcessStartInfo("reg.exe", $"import ""{regFile}""") With {
                .Verb = "runas",
                .UseShellExecute = True
            }
            Using p As Process = Process.Start(psi)
                If p Is Nothing Then
                    RhpLog("reg.exe nu a pornit (Process.Start a întors Nothing).")
                    ShowStatus("reg.exe nu a pornit.")
                    Return False
                End If
                Dim exited As Boolean = Await Task.Run(Function() p.WaitForExit(60000))
                If Not exited Then
                    RhpLog($"reg.exe import «{Path.GetFileName(regFile)}» nu s-a terminat în 60s.")
                    ShowStatus("reg.exe nu s-a terminat în 60 de secunde — verifică manual.")
                    Return False
                End If
                RhpLog($"reg.exe import «{Path.GetFileName(regFile)}» -> cod ieșire {p.ExitCode}.")
                If p.ExitCode <> 0 Then ShowStatus($"reg.exe a eșuat (cod {p.ExitCode}) — vezi {RHP_LOG_NAME}.")
                Return p.ExitCode = 0
            End Using
        Catch wex As Win32Exception When wex.NativeErrorCode = 1223
            ShowStatus("Operația a fost anulată de utilizator.")
            RhpLog("UAC anulat de utilizator (ERROR_CANCELLED 1223).")
            GlobalErrorLog.Write("AdobeReaderHarnessForm.RunRegImportAsync.UAC", wex)
            Return False
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.RunRegImportAsync", ex)
            Throw
        End Try
    End Function

    ' ══ Scenariu — load / run / save ════════════════════════════════════════════
    Private Sub btnLoadScenario_Click(sender As Object, e As EventArgs) Handles btnLoadScenario.Click
        Try
            Dim dir As String = ConfigDir()
            Using dlg As New System.Windows.Forms.OpenFileDialog()
                dlg.Filter = "Scenarii K-BOT (*.json)|*.json|Toate fișierele (*.*)|*.*"
                dlg.Title = "Încarcă un scenariu"
                If Directory.Exists(dir) Then dlg.InitialDirectory = dir
                If dlg.ShowDialog(Me) <> DialogResult.OK Then Return
                LoadScenarioFile(dlg.FileName)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnLoadScenario_Click", ex)
            ShowStatus("Eroare la încărcarea scenariului: " & ex.Message)
        End Try
    End Sub

    ' Loading NEVER writes anything — only pressing Run does.
    ' NOTE: the parameter is deliberately NOT called `path` — VB identifiers are case-insensitive,
    ' so `path` would shadow System.IO.Path and turn `Path.GetFileName` into a compile error (the
    ' same shadowing trap that produced silent no-ops in slice 0010 and 0019).
    Private Sub LoadScenarioFile(filePath As String)
        Dim json As String = File.ReadAllText(filePath)
        Dim result As HarnessScenarioReadResult = HarnessScenarioReader.Read(json)

        For Each w As String In result.Warnings
            RhpLog("Scenariu (avertisment): " & w)
        Next

        If Not result.IsValid Then
            For Each errText As String In result.Errors
                RhpLog("Scenariu (eroare): " & errText)
            Next
            _scenario = Nothing
            _scenarioPath = Nothing
            lblScenario.Text = "(niciun scenariu)"
            btnRunScenario.Enabled = False
            MessageBox.Show(Me,
                "Scenariul nu a putut fi încărcat:" & Environment.NewLine & Environment.NewLine &
                String.Join(Environment.NewLine, result.Errors),
                "K-BOT — scenariu invalid", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ShowStatus("Scenariu invalid — vezi " & RHP_LOG_NAME & ".")
            Return
        End If

        _scenario = result.Scenario
        _scenarioPath = filePath
        lblScenario.Text = Path.GetFileName(filePath)
        btnRunScenario.Enabled = True
        RhpLog($"Scenariu încărcat: {filePath} — «{If(_scenario.Name, "(fără nume)")}»")

        ' A scenario's whole job is to SET the panel; loading always applies it.
        ApplyScenarioToControls()
        UpdateCmdPreview()
        Dim note As String = If(String.IsNullOrWhiteSpace(_scenario.Note), "", " — " & _scenario.Note)
        ShowStatus($"Scenariu încărcat: {If(_scenario.Name, Path.GetFileName(filePath))}{note}" &
                   Environment.NewLine & "Alege acum un PDF (Deschide PDF…) — se va deschide cu aceste setări.")
    End Sub

    ' Ticks the checkboxes and fills the spinners: this IS what loading a scenario does. The
    ' operator sees exactly what will be used and can still adjust before picking a PDF. The
    ' document is deliberately untouched — it always comes from «Deschide PDF…».
    Private Sub ApplyScenarioToControls()
        _loading = True
        Try
            If _scenario.Launch IsNot Nothing Then
                If _scenario.Launch.NewInstance.HasValue Then chkNewInstance.Checked = _scenario.Launch.NewInstance.Value
                If _scenario.Launch.NoSplash.HasValue Then chkNoSplash.Checked = _scenario.Launch.NoSplash.Value
            End If
            Dim op As OpenParametersConfig = _scenario.OpenParameters
            If op IsNot Nothing Then
                If op.Toolbar.HasValue Then chkToolbar.Checked = (op.Toolbar.Value = 0)
                If op.Navpanes.HasValue Then chkNavpanes.Checked = (op.Navpanes.Value = 0)
                If op.Statusbar.HasValue Then chkStatusbar.Checked = (op.Statusbar.Value = 0)
                If op.Messages.HasValue Then chkMessages.Checked = (op.Messages.Value = 0)
                If op.Scrollbar.HasValue Then chkScrollbar.Checked = (op.Scrollbar.Value = 0)
                If Not String.IsNullOrWhiteSpace(op.Pagemode) Then
                    chkPagemodeNone.Checked = String.Equals(op.Pagemode, "none", StringComparison.OrdinalIgnoreCase)
                End If
            End If
            If _scenario.Clip IsNot Nothing Then
                If _scenario.Clip.Enabled.HasValue Then chkClip.Checked = _scenario.Clip.Enabled.Value
                If _scenario.Clip.Right.HasValue Then numClipRight.Value = ClampToRange(numClipRight, _scenario.Clip.Right.Value)
                If _scenario.Clip.Top.HasValue Then numClipTop.Value = ClampToRange(numClipTop, _scenario.Clip.Top.Value)
            End If
            If _scenario.UserPrefs IsNot Nothing Then
                If _scenario.UserPrefs.RestoreOnClose.HasValue Then
                    chkRestoreOnClose.Checked = _scenario.UserPrefs.RestoreOnClose.Value
                End If
                SelectComboValue(cboHive, _scenario.UserPrefs.Hive)
            End If
            ' Always, even with no userPrefs section: a scenario silent about HKCU must clear rows
            ' left behind by the previous one, not inherit them.
            ApplyUserPrefValuesToRows(If(_scenario.UserPrefs Is Nothing, Nothing, _scenario.UserPrefs.Values))
            If _scenario.MachinePolicy IsNot Nothing Then
                SelectComboValue(cboProduct, _scenario.MachinePolicy.Product)
                ApplyPolicyValuesToChecks(_scenario.MachinePolicy.Values)
                chkRevertPolicyOnClose.Checked = _scenario.MachinePolicy.ShouldRevertOnClose()
            End If
        Finally
            _loading = False
        End Try
        ' The «Cerut vs Curent» table is the point of loading a scenario that touches HKCU.
        RefreshPrefsGrid()
        RefreshStatusBlock()
    End Sub

    Private Shared Function ClampToRange(n As NumericUpDown, value As Integer) As Decimal
        Dim v As Decimal = value
        If v < n.Minimum Then Return n.Minimum
        If v > n.Maximum Then Return n.Maximum
        Return v
    End Function

    Private Shared Sub SelectComboValue(combo As ComboBox, value As String)
        If String.IsNullOrWhiteSpace(value) Then Return
        For i As Integer = 0 To combo.Items.Count - 1
            If String.Equals(CStr(combo.Items(i)), value, StringComparison.OrdinalIgnoreCase) Then
                combo.SelectedIndex = i
                Return
            End If
        Next
    End Sub

    ' The panel has one ROW per COMMON HKCU value, and the row shows the scenario's value VERBATIM
    ' — 1 stays 1, "Expanded" stays "Expanded", null becomes «șterge». A value the file does not
    ' mention goes back to «nu atinge», which is not the same as writing 0. Values with no row are
    ' still applied literally (see CollectUserPrefs) and appear in the «Cerut vs Curent» grid.
    Private Sub ApplyUserPrefValuesToRows(values As Dictionary(Of String, JsonElement))
        Dim byName As New Dictionary(Of String, UserPrefIntent)(StringComparer.OrdinalIgnoreCase)
        For Each i As UserPrefIntent In UserPrefIntentFactory.FromValues(values)
            byName(i.Name) = i
        Next

        SetRowText(cboExpandRhp, byName, AdobeRegistryConstants.ValExpandRhp)
        SetRowText(cboRhpSticky, byName, AdobeRegistryConstants.ValRhpSticky)
        SetRowText(cboRhpViewMode, byName, AdobeRegistryConstants.ValRhpViewMode)
        SetRowText(cboEnableAv2, byName, AdobeRegistryConstants.ValEnableAv2)

        If values Is Nothing Then Return
        Dim known As String() = {AdobeRegistryConstants.ValExpandRhp, AdobeRegistryConstants.ValRhpSticky,
                                 AdobeRegistryConstants.ValRhpViewMode, AdobeRegistryConstants.ValEnableAv2}
        For Each k As String In values.Keys
            If Not known.Any(Function(n) String.Equals(n, k, StringComparison.OrdinalIgnoreCase)) Then
                RhpLog($"userPrefs.values.{k}: fără rând în panou — se aplică LITERAL din fișier " &
                       "(vizibilă în tabelul «Cerut vs Curent»).")
            End If
        Next
    End Sub

    Private Shared Sub SetRowText(row As ComboBox, byName As Dictionary(Of String, UserPrefIntent),
                                  name As String)
        Dim intent As UserPrefIntent = Nothing
        byName.TryGetValue(name, intent)
        ' TextFor(Nothing) is «nu atinge» — a file silent about a value leaves it alone.
        row.Text = PrefRowSelection.TextFor(intent)
        ' Setting .Text on an editable combo leaves the whole value selected, which reads as a
        ' highlighted (half-edited) row in a panel where nothing is being edited.
        row.SelectionLength = 0
    End Sub

    Private Sub ApplyPolicyValuesToChecks(values As Dictionary(Of String, JsonElement))
        If values Is Nothing Then Return
        chkSuppressUpsell.Checked = values.Keys.Any(
            Function(k) k.EndsWith(AdobeRegistryConstants.ValSuppressUpsell, StringComparison.OrdinalIgnoreCase))
        chkDisableServices.Checked = values.Keys.Any(
            Function(k) k.EndsWith(AdobeRegistryConstants.ValToggleServices, StringComparison.OrdinalIgnoreCase))

        For Each k As String In values.Keys
            If Not k.EndsWith(AdobeRegistryConstants.ValSuppressUpsell, StringComparison.OrdinalIgnoreCase) AndAlso
               Not k.EndsWith(AdobeRegistryConstants.ValToggleServices, StringComparison.OrdinalIgnoreCase) Then
                RhpLog($"machinePolicy.values.{k}: nu există un comutator în panou pentru această valoare — " &
                       "va fi IGNORATĂ.")
            End If
        Next
    End Sub

    Private Async Sub btnRunScenario_Click(sender As Object, e As EventArgs) Handles btnRunScenario.Click
        Try
            If _scenario Is Nothing Then
                ShowStatus("Încarcă întâi un scenariu.")
                Return
            End If
            If _scenarioRunning Then
                ShowStatus("Scenariul rulează deja.")
                Return
            End If
            _scenarioRunning = True
            btnRunScenario.Enabled = False
            Try
                Await RunScenarioAsync()
            Finally
                _scenarioRunning = False
                btnRunScenario.Enabled = True
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnRunScenario_Click", ex)
            ShowStatus("Eroare la rularea scenariului: " & ex.Message)
        End Try
    End Sub

    ' Runs the steps in order. A step that fails stops the run there, logged — no automatic
    ' rollback, because half-rolled-back state is harder to read than the failure.
    Private Async Function RunScenarioAsync() As Task
        Dim steps As List(Of String) = _scenario.Scenario
        If steps Is Nothing OrElse steps.Count = 0 Then
            ShowStatus("Scenariul nu conține niciun pas.")
            Return
        End If

        ' Every run starts by reading the machine, so a contaminated baseline is visible BEFORE the
        ' first step instead of being reconstructed from logs afterwards.
        LogMachineState("── Bază de pornire ──")
        Dim baseline As BaselineAssessment = BaselineEvaluator.Evaluate(
            ReadBaselineState(), _scenario.RequireCleanBaseline)
        Select Case baseline.Verdict
            Case BaselineVerdict.Block
                RhpLog("Scenariu REFUZAT: requireCleanBaseline = true și mașina nu e neutră." &
                       Environment.NewLine & baseline.Describe())
                MessageBox.Show(Me, baseline.BlockedText(), "K-BOT — bază de pornire contaminată",
                                MessageBoxButtons.OK, MessageBoxIcon.Error)
                ShowStatus($"Scenariu refuzat: bază contaminată ({baseline.Policies.Count} politici HKLM, " &
                           $"{baseline.Preferences.Count} preferințe HKCU).")
                Return
            Case BaselineVerdict.Warn
                RhpLog("ATENȚIE bază de pornire: mașina nu e neutră —" & Environment.NewLine & baseline.Describe())
                If MessageBox.Show(Me, baseline.WarningText(), "K-BOT — bază de pornire contaminată",
                                   MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) <> DialogResult.OK Then
                    RhpLog("Scenariu abandonat de operator la avertismentul de bază contaminată.")
                    ShowStatus("Rularea scenariului a fost abandonată.")
                    Return
                End If
        End Select

        ' Registry-touching runs are confirmed ONCE, up front, listing every path and value.
        Dim touchesUser As Boolean = steps.Contains(HarnessScenarioSteps.ApplyUserPrefs)
        Dim touchesMachine As Boolean = steps.Contains(HarnessScenarioSteps.ApplyMachinePolicy) AndAlso
                                        _scenario.MachinePolicy IsNot Nothing AndAlso _scenario.MachinePolicy.Apply
        If touchesUser OrElse touchesMachine Then
            If Not ConfirmRegistryWrites(touchesUser, touchesMachine) Then
                RhpLog("Scenariu abandonat de operator la confirmarea scrierilor în registry.")
                ShowStatus("Rularea scenariului a fost abandonată.")
                Return
            End If
        End If

        RhpLog($"══ Scenariu «{If(_scenario.Name, Path.GetFileName(If(_scenarioPath, "?")))}» — {steps.Count} pași ══")
        For Each stepName As String In steps
            RhpLog($"PAS start: {stepName}")
            Dim ok As Boolean
            Try
                ok = Await RunStepAsync(stepName)
            Catch ex As Exception
                GlobalErrorLog.Write("AdobeReaderHarnessForm.RunStepAsync." & stepName, ex)
                RhpLog($"PAS eșuat: {stepName} — {ex.Message}. Rularea se oprește; starea rămâne ca atare.")
                ShowStatus($"Scenariu oprit la pasul «{stepName}»: {ex.Message}")
                Return
            End Try
            If Not ok Then
                RhpLog($"PAS oprit: {stepName} — rularea se oprește; starea rămâne ca atare.")
                ShowStatus($"Scenariu oprit la pasul «{stepName}».")
                Return
            End If
            RhpLog($"PAS gata: {stepName}")
        Next
        RhpLog("══ Scenariu terminat ══")
        ShowStatus($"Scenariu terminat: {steps.Count} pași.")
    End Function

    ' Every step maps to the SAME code path as the corresponding button — no step contains new
    ' logic. An unrecognised name cannot reach here (the reader rejects the file), but it is
    ' still handled loudly rather than skipped.
    Private Async Function RunStepAsync(stepName As String) As Task(Of Boolean)
        Select Case stepName
            Case HarnessScenarioSteps.ApplyUserPrefs
                Return ApplyUserPrefsCore()

            Case HarnessScenarioSteps.ApplyMachinePolicy
                If _scenario.MachinePolicy Is Nothing OrElse Not _scenario.MachinePolicy.Apply Then
                    RhpLog("applyMachinePolicy: sărit (machinePolicy.apply = false).")
                    Return True
                End If
                Return Await ApplyMachinePolicyCoreAsync()

            Case HarnessScenarioSteps.Launch
                Dim gen As Integer = StartFreshLaunch()
                If gen = 0 Then
                    ShowStatus("Pasul «launch» nu a pornit Adobe (lipsește Adobe sau documentul).")
                    Return False
                End If
                Return True

            Case HarnessScenarioSteps.WaitForEmbed
                If _pendingProcess Is Nothing Then
                    RhpLog("waitForEmbed: nu există un proces pornit de pasul «launch».")
                    Return False
                End If
                Await CompleteEmbedAsync(_pendingGeneration, _pendingProcess)
                Return _hostedWindow <> IntPtr.Zero

            Case HarnessScenarioSteps.SendKeys
                Return Await SendScenarioKeysAsync()

            Case HarnessScenarioSteps.Probe
                ProbeChildren()
                Return True

            Case HarnessScenarioSteps.HideChildren
                Return Await HideChildrenByTextAsync(_scenario.HideChildren)

            Case HarnessScenarioSteps.ApplyClip
                ApplyScenarioClip()
                Return True

            Case HarnessScenarioSteps.RestoreUserPrefs
                Return RestoreUserPrefsCore()

            Case HarnessScenarioSteps.RevertMachinePolicy
                Return Await RevertMachinePolicyCoreAsync()

            Case Else
                RhpLog($"Pas necunoscut «{stepName}». Pași valizi: {HarnessScenarioSteps.AllAsText()}.")
                Return False
        End Select
    End Function

    Private Async Function SendScenarioKeysAsync() As Task(Of Boolean)
        If _scenario.Keys Is Nothing OrElse _scenario.Keys.Count = 0 Then
            RhpLog("sendKeys: lista «keys» este goală — nimic de trimis.")
            Return True
        End If
        For Each k As KeyStepConfig In _scenario.Keys
            If k Is Nothing OrElse String.IsNullOrWhiteSpace(k.Send) Then Continue For
            If k.DelayMsBefore.HasValue AndAlso k.DelayMsBefore.Value > 0 Then
                Await Task.Delay(k.DelayMsBefore.Value)
            End If
            SendKeyToHosted(ToSendKeysSyntax(k.Send), k.Send)
        Next
        Return True
    End Function

    ' Applies the clip geometry from the CURRENT spinner/checkbox values — the same path the
    ' spinners use, no relaunch. The scenario's clip values reached those controls when it was
    ' loaded, so there is nothing to re-read here.
    Private Sub ApplyScenarioClip()
        LayoutHostedWindow()
        NudgeRedraw()
        RhpLog($"applyClip: activă={chkClip.Checked}, dreapta={CInt(numClipRight.Value)}px, sus={CInt(numClipTop.Value)}px.")
        RefreshStatusBlock()
    End Sub

    ' Lists every registry path and value that will be written, current value beside the new one.
    ' Cancel abandons the whole run, not just the step.
    Private Function ConfirmRegistryWrites(userPrefs As Boolean, machinePolicy As Boolean) As Boolean
        Try
            Dim sb As New StringBuilder()
            If userPrefs Then
                Dim hive As String = CurrentAvGeneralPath()
                Dim prefs As List(Of UserPrefIntent) = CollectUserPrefs()
                If prefs.Count > 0 Then
                    sb.AppendLine("HKCU — preferințe utilizator:")
                    sb.AppendLine("  (hive · nume · valoare curentă · valoare nouă · tip)")
                    For Each p As UserPrefIntent In prefs
                        ' The TRUE intended value, straight from the scenario — never the value a
                        ' checkbox would have clamped it to.
                        Dim cur As RegistryValueSnapshot = _regAccess.Read(hive, p.Name)
                        Dim curText As String = If(cur.Presence = RegPresence.Absent, "(absent)",
                                                   Convert.ToString(cur.Value))
                        Dim newText As String = If(p.Action = UserPrefAction.Delete, "(șters)", p.RequestedText())
                        Dim kindText As String = If(p.Action = UserPrefAction.Delete, "—", p.Kind.ToString())
                        sb.AppendLine($"  {hive}\{p.Name}: {curText} -> {newText} ({kindText})")
                    Next
                End If
            End If
            If machinePolicy Then
                Dim product As String = CurrentPolicyProduct()
                Dim entries As List(Of PolicyEntry) = CollectPolicyEntries(product)
                If entries.Count > 0 Then
                    If sb.Length > 0 Then sb.AppendLine()
                    sb.AppendLine("HKLM — politici de mașină (cer elevare):")
                    For Each en As PolicyEntry In entries
                        Dim cur As RegistryValueSnapshot = _regAccess.Read(en.SectionPath, en.Name)
                        Dim curText As String = If(cur.Presence = RegPresence.Absent, "<absent>", CStr(cur.Value))
                        sb.AppendLine($"  {en.SectionPath}\{en.Name}: {curText} -> {en.Value}")
                    Next
                End If
            End If
            If sb.Length = 0 Then Return True

            Dim r As DialogResult = MessageBox.Show(Me,
                "Se vor scrie următoarele valori în registry:" & Environment.NewLine & Environment.NewLine &
                sb.ToString() & Environment.NewLine &
                "Continui?",
                "K-BOT — confirmare scriere registry", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning)
            If r <> DialogResult.OK Then
                RhpLog("Scriere registry refuzată de operator la confirmare.")
                Return False
            End If
            RhpLog("Operatorul a confirmat scrierile în registry:" & Environment.NewLine & sb.ToString())
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.ConfirmRegistryWrites", ex)
            Throw
        End Try
    End Function

    ' Writes the CURRENT state of every control back out in the same schema, including the text of
    ' any child windows hidden right now. This is the round trip that matters: find a combination
    ' by hand, save it, send it, re-run it, try it on another machine.
    Private Sub btnSaveScenario_Click(sender As Object, e As EventArgs) Handles btnSaveScenario.Click
        Try
            Dim dir As String = ConfigDir()
            Directory.CreateDirectory(dir)
            Using dlg As New System.Windows.Forms.SaveFileDialog()
                dlg.Filter = "Scenarii K-BOT (*.json)|*.json|Toate fișierele (*.*)|*.*"
                dlg.Title = "Salvează starea curentă ca scenariu"
                dlg.InitialDirectory = dir
                dlg.FileName = "scenariu_adobe.json"
                If dlg.ShowDialog(Me) <> DialogResult.OK Then Return
                Dim json As String = HarnessScenarioWriter.Write(BuildScenarioFromControls())
                File.WriteAllText(dlg.FileName, json, New UTF8Encoding(False))
                RhpLog($"Scenariu salvat: {dlg.FileName}")
                ShowStatus("Scenariu salvat: " & Path.GetFileName(dlg.FileName))
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnSaveScenario_Click", ex)
            ShowStatus("Eroare la salvarea scenariului: " & ex.Message)
        End Try
    End Sub

    Private Function BuildScenarioFromControls() As HarnessScenario
        ' Settings only — the document is NOT saved. A scenario is a recipe that any PDF can be
        ' opened under, so a path here would be dead weight on anyone else's machine.
        Dim s As New HarnessScenario() With {
            .Schema = HarnessScenarioReader.SupportedSchema,
            .Name = "Stare salvată din bancul de probă",
            .Note = "Generat automat din controalele bancului la " & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") &
                    ". Documentul se alege separat, cu «Deschide PDF…»."
        }

        s.Launch = New LaunchConfig() With {
            .NewInstance = chkNewInstance.Checked,
            .NoSplash = chkNoSplash.Checked}

        Dim op As New OpenParametersConfig()
        If chkToolbar.Checked Then op.Toolbar = 0
        If chkNavpanes.Checked Then op.Navpanes = 0
        If chkStatusbar.Checked Then op.Statusbar = 0
        If chkMessages.Checked Then op.Messages = 0
        If chkScrollbar.Checked Then op.Scrollbar = 0
        If chkPagemodeNone.Checked Then op.Pagemode = "none"
        s.OpenParameters = op

        s.Clip = New ClipConfig() With {
            .Enabled = chkClip.Checked,
            .Right = CInt(numClipRight.Value),
            .Top = CInt(numClipTop.Value)}

        If _hiddenChildTexts.Count > 0 Then
            s.HideChildren = New HideChildrenConfig() With {
                .ByText = New List(Of String)(_hiddenChildTexts),
                .ReapplyOnRelaunch = True,
                .ReapplyAttempts = HideChildrenConfig.DefaultReapplyAttempts,
                .ReapplyIntervalMs = HideChildrenConfig.DefaultReapplyIntervalMs}
        End If

        ' Save the LITERAL intents, so a saved file reproduces exactly what would be written —
        ' including a deletion, which round-trips as JSON null.
        Dim prefValues As New Dictionary(Of String, JsonElement)()
        For Each p As UserPrefIntent In CollectUserPrefs()
            If p.Action = UserPrefAction.Delete Then
                prefValues(p.Name) = JsonSerializer.SerializeToElement(Of Object)(Nothing)
            Else
                prefValues(p.Name) = ToJsonElement(p.Value)
            End If
        Next
        If prefValues.Count > 0 Then
            s.UserPrefs = New UserPrefsConfig() With {
                .Hive = CStr(If(cboHive.SelectedItem, "auto")),
                .Values = prefValues,
                .RestoreOnClose = chkRestoreOnClose.Checked}
        End If

        Dim policyValues As New Dictionary(Of String, JsonElement)()
        If chkSuppressUpsell.Checked Then
            policyValues(AdobeRegistryConstants.ValSuppressUpsell) = ToJsonElement(1)
        End If
        If chkDisableServices.Checked Then
            policyValues("cServices\" & AdobeRegistryConstants.ValToggleServices) = ToJsonElement(1)
        End If
        If policyValues.Count > 0 Then
            ' apply stays FALSE on save: a saved file must never write machine-wide policy just
            ' by being run somewhere else.
            s.MachinePolicy = New MachinePolicyConfig() With {
                .Product = CStr(If(cboProduct.SelectedItem, "auto")),
                .Apply = False,
                .Values = policyValues}
        End If

        s.Scenario = BuildDefaultStepList(s)
        Return s
    End Function

    ' The step list a saved file replays: exactly what the current state implies, in the order the
    ' operator would have clicked.
    Private Function BuildDefaultStepList(s As HarnessScenario) As List(Of String)
        Dim steps As New List(Of String)()
        steps.Add(HarnessScenarioSteps.Launch)
        steps.Add(HarnessScenarioSteps.WaitForEmbed)
        If s.HideChildren IsNot Nothing Then
            steps.Add(HarnessScenarioSteps.Probe)
            steps.Add(HarnessScenarioSteps.HideChildren)
        Else
            steps.Add(HarnessScenarioSteps.Probe)
        End If
        If chkClip.Checked Then steps.Add(HarnessScenarioSteps.ApplyClip)
        Return steps
    End Function

    Private Shared Function ToJsonElement(value As Object) As JsonElement
        Return JsonSerializer.SerializeToElement(value)
    End Function

    Private Shared Function ConfigDir() As String
        Return Path.Combine(AppContext.BaseDirectory, CONFIG_DIR_NAME)
    End Function

    ' ══ Starea mașinii (Adobe + registry) ══════════════════════════════════════
    Private Sub btnMachineState_Click(sender As Object, e As EventArgs) Handles btnMachineState.Click
        Try
            Dim summary As String = LogMachineState("── Starea mașinii ──")
            ShowStatus(summary)
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnMachineState_Click", ex)
            ShowStatus("Eroare la citirea stării mașinii: " & ex.Message)
        End Try
    End Sub

    ' Reads and logs the whole picture WITHOUT writing anything and without elevation (reading
    ' HKLM\SOFTWARE\Policies needs no rights). Returns a compact summary for lblStatus.
    Private Function LogMachineState(header As String) As String
        RhpLog(header)

        ' Adobe executable + version.
        If String.IsNullOrEmpty(_adobePath) Then
            RhpLog("  Adobe: NEGĂSIT")
        Else
            Dim ver As String = "?"
            Dim prod As String = "?"
            Try
                Dim fvi = FileVersionInfo.GetVersionInfo(_adobePath)
                ver = If(fvi.FileVersion, "?")
                prod = If(fvi.ProductName, "?")
            Catch ex As Exception
                GlobalErrorLog.Write("AdobeReaderHarnessForm.LogMachineState.Version", ex)
            End Try
            RhpLog($"  Adobe: {_adobePath}")
            RhpLog($"  Versiune: {ver}   Produs: {prod}")
        End If

        ' Both AVGeneral hives and the four RHP/viewer values in each.
        For Each hive As String In New String() {AdobeRegistryConstants.AvGeneralReader,
                                                 AdobeRegistryConstants.AvGeneralAcrobat}
            Dim exists As Boolean = _regAccess.KeyExists(hive)
            RhpLog($"  {hive} — {If(exists, "există", "lipsește")}")
            If Not exists Then Continue For
            For Each name As String In New String() {AdobeRegistryConstants.ValEnableAv2,
                                                     AdobeRegistryConstants.ValExpandRhp,
                                                     AdobeRegistryConstants.ValRhpSticky,
                                                     AdobeRegistryConstants.ValRhpViewMode}
                Dim s As RegistryValueSnapshot = _regAccess.Read(hive, name)
                RhpLog($"      {name} = " & If(s.Presence = RegPresence.Absent, "(absent)",
                                               Convert.ToString(s.Value) & $" ({s.Kind})"))
            Next
        Next

        ' Both HKLM policy products.
        Dim readings As List(Of PolicyReading) = ReadPolicyState()
        For Each r As PolicyReading In readings
            RhpLog("  " & r.ToString())
        Next

        Dim adobeProcs As Integer =
            Process.GetProcessesByName("Acrobat").Length + Process.GetProcessesByName("AcroRd32").Length
        RhpLog($"  Procese Adobe în execuție: {adobeProcs}")

        Dim active As Integer = Enumerable.Count(readings, Function(r) r.Present)
        ' The three RHP preferences count as contamination too — that is the whole point of pass 5.
        Dim prefsSet As Integer = Enumerable.Count(ReadUserRhpState(), Function(r) r.Present)
        Dim summaryText As String =
            $"Stare: Adobe {If(String.IsNullOrEmpty(_adobePath), "negăsit", "găsit")} · " &
            $"politici HKLM active: {active} · preferințe RHP în HKCU: {prefsSet} · " &
            $"procese Adobe: {adobeProcs}"
        If active > 0 OrElse prefsSet > 0 Then summaryText &= "  ⚠ bază de pornire CONTAMINATĂ"
        RhpLog("  " & summaryText)
        Return summaryText
    End Function

    ' The three HKCU values a NEUTRAL machine does not have at all. They are Adobe's memory of how
    ' the right-hand pane should look, and they survive everything: on 04.08 a baseline came back
    ' "curată" while bRHPSticky = 1 and aDefaultRHPViewMode_L = "Collapsed" were still set.
    '
    ' bEnableAv2 is deliberately NOT here: it selects the classic/modern viewer, which is the thing
    ' under test. Counting it as contamination would make every scenario that sets it block itself.
    Private Function ReadUserRhpState() As List(Of PolicyReading)
        Dim readings As New List(Of PolicyReading)()
        For Each hive As String In New String() {AdobeRegistryConstants.AvGeneralReader,
                                                 AdobeRegistryConstants.AvGeneralAcrobat}
            If Not _regAccess.KeyExists(hive) Then Continue For
            For Each name As String In New String() {AdobeRegistryConstants.ValExpandRhp,
                                                     AdobeRegistryConstants.ValRhpSticky,
                                                     AdobeRegistryConstants.ValRhpViewMode}
                Dim s As RegistryValueSnapshot = _regAccess.Read(hive, name)
                readings.Add(New PolicyReading(hive, name, s.Presence = RegPresence.Present, s.Value,
                                               BaselineOrigin.UserPreference))
            Next
        Next
        Return readings
    End Function

    ' What «bază de pornire curată» means: no HKLM policy AND no leftover HKCU pane preference.
    ' Kept separate from ReadPolicyState, which the revert path uses to verify that the ELEVATED
    ' import removed the policy — HKCU values must not make that verification fail.
    Private Function ReadBaselineState() As List(Of PolicyReading)
        Dim all As New List(Of PolicyReading)(ReadPolicyState())
        all.AddRange(ReadUserRhpState())
        Return all
    End Function

    ' Reads both products' policy values (read-only, no elevation).
    Private Function ReadPolicyState() As List(Of PolicyReading)
        Dim readings As New List(Of PolicyReading)()
        For Each product As String In New String() {AdobeRegistryConstants.ProductReader,
                                                    AdobeRegistryConstants.ProductAcrobat}
            Dim fld As String = AdobeRegistryConstants.FeatureLockDownPath(product)
            Dim cs As String = AdobeRegistryConstants.CServicesPath(product)
            readings.Add(ReadPolicyValue(fld, AdobeRegistryConstants.ValSuppressUpsell))
            readings.Add(ReadPolicyValue(cs, AdobeRegistryConstants.ValToggleServices))
        Next
        Return readings
    End Function

    Private Function ReadPolicyValue(path As String, name As String) As PolicyReading
        Dim s As RegistryValueSnapshot = _regAccess.Read(path, name)
        Return New PolicyReading(path, name, s.Presence = RegPresence.Present, s.Value)
    End Function

    ' Reverts an outstanding HKLM policy when the bench closes. Synchronous by necessity (the form
    ' is going away, there is nothing left to await on). If the operator cancels the UAC prompt the
    ' marker file is KEPT and the machine is reported as still modified, naming the exact keys —
    ' the silent-drift case this whole pass exists to prevent.
    Private Sub RevertMachinePolicyOnClose()
        If Not chkRevertPolicyOnClose.Checked Then Return
        Dim r As MachineStateMarkerResult = ReadMarker()
        If r.Status <> MarkerReadStatus.Present OrElse r.Marker Is Nothing OrElse Not r.Marker.PolicyApplied Then Return

        Dim regFile As String = If(Not String.IsNullOrEmpty(_revertRegPath) AndAlso File.Exists(_revertRegPath),
                                   _revertRegPath, r.Marker.RevertRegFile)
        If String.IsNullOrEmpty(regFile) OrElse Not File.Exists(regFile) Then
            ReportPolicyStillActive("fișierul de revocare lipsește (" & If(regFile, "—") & ")")
            Return
        End If

        Try
            Dim psi As New ProcessStartInfo("reg.exe", $"import ""{regFile}""") With {
                .Verb = "runas",
                .UseShellExecute = True
            }
            Using p As Process = Process.Start(psi)
                If p Is Nothing Then
                    ReportPolicyStillActive("reg.exe nu a pornit")
                    Return
                End If
                If Not p.WaitForExit(60000) Then
                    ReportPolicyStillActive("reg.exe nu s-a terminat în 60s")
                    Return
                End If
                If p.ExitCode <> 0 Then
                    ReportPolicyStillActive($"reg.exe a întors codul {p.ExitCode}")
                    Return
                End If
            End Using
            Dim stillActive As List(Of PolicyReading) = ReadPolicyState().Where(Function(x) x.Present).ToList()
            If stillActive.Count = 0 Then
                ClearMarker()
                RhpLog("Revocare la închidere: politica HKLM a fost revocată și verificată. Marcaj șters.")
            Else
                ReportPolicyStillActive($"{stillActive.Count} valori încă active după import")
            End If
        Catch wex As Win32Exception When wex.NativeErrorCode = 1223
            ReportPolicyStillActive("operatorul a anulat elevarea (UAC)")
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.RevertMachinePolicyOnClose", ex)
            ReportPolicyStillActive(ex.Message)
        End Try
    End Sub

    ' Names the exact keys that are still set, in the log AND to the operator's face — the form is
    ' closing, so lblStatus alone would never be read.
    Private Sub ReportPolicyStillActive(reason As String)
        Dim active As List(Of PolicyReading) = ReadPolicyState().Where(Function(p) p.Present).ToList()
        Dim keys As String = String.Join(Environment.NewLine, active.Select(Function(p) "  " & p.ToString()))
        Dim text As String =
            "Politica HKLM NU a fost revocată (" & reason & ")." & Environment.NewLine &
            "Mașina rămâne modificată la:" & Environment.NewLine & keys & Environment.NewLine & Environment.NewLine &
            "Revoc-o din banc («Revocă (cere elevare)») înainte de următoarea probă — altfel " &
            "rezultatele vor fi neconcludente."
        RhpLog("ATENȚIE la închidere: " & text.Replace(Environment.NewLine, " "))
        Try
            lblStatus.Text = "Politica HKLM a rămas ACTIVĂ — vezi mesajul."
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.ReportPolicyStillActive.Status", ex)
        End Try
        MessageBox.Show(Me, text, "K-BOT — mașina rămâne modificată", MessageBoxButtons.OK, MessageBoxIcon.Warning)
    End Sub

    ' ══ Marcaj de stare a mașinii (supraviețuiește închiderii bancului) ═════════
    Private Function MarkerPath() As String
        Return MachineStateMarkerStore.PathFor(ConfigDir())
    End Function

    Private Function ReadMarker() As MachineStateMarkerResult
        Try
            Dim p As String = MarkerPath()
            If Not File.Exists(p) Then Return New MachineStateMarkerResult(MarkerReadStatus.None, Nothing, Nothing)
            Return MachineStateMarkerStore.Parse(File.ReadAllText(p))
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.ReadMarker", ex)
            Return New MachineStateMarkerResult(MarkerReadStatus.Corrupt, Nothing, ex.Message)
        End Try
    End Function

    Private Sub WriteMarker(marker As MachineStateMarker)
        Try
            Directory.CreateDirectory(ConfigDir())
            File.WriteAllText(MarkerPath(), MachineStateMarkerStore.Serialize(marker), New UTF8Encoding(False))
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.WriteMarker", ex)
            Throw
        End Try
    End Sub

    Private Sub ClearMarker()
        Try
            Dim p As String = MarkerPath()
            If File.Exists(p) Then File.Delete(p)
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.ClearMarker", ex)
        End Try
    End Sub

    ' On open: if a policy is outstanding from a previous session, say so immediately — that is
    ' exactly the state that silently invalidated four runs.
    Private Sub WarnIfPolicyOutstanding()
        Dim r As MachineStateMarkerResult = ReadMarker()
        If Not r.NeedsWarning Then Return
        Dim text As String
        If r.Status = MarkerReadStatus.Corrupt Then
            text = "Fișierul de stare a mașinii nu a putut fi citit (" & If(r.[Error], "") & ")." &
                   Environment.NewLine &
                   "Nu pot ști dacă o politică HKLM a rămas aplicată — verifică cu «Starea mașinii»."
        Else
            text = "O politică HKLM aplicată de banc a rămas ACTIVĂ dintr-o sesiune anterioară" &
                   If(String.IsNullOrWhiteSpace(r.Marker.Product), "", $" (produs «{r.Marker.Product}»)") &
                   If(String.IsNullOrWhiteSpace(r.Marker.AppliedAt), "", $", aplicată la {r.Marker.AppliedAt}") & "." &
                   Environment.NewLine & Environment.NewLine &
                   "Ea suprimă serviciile Adobe și poate face panoul de instrumente gol sau de dimensiune " &
                   "zero. Revoc-o («Revocă (cere elevare)») înainte de a trage concluzii."
        End If
        RhpLog("ATENȚIE la pornire: " & text.Replace(Environment.NewLine, " "))
        MessageBox.Show(Me, text, "K-BOT — mașină modificată", MessageBoxButtons.OK, MessageBoxIcon.Warning)
    End Sub

    ' ── gridPrefs: cerut vs curent ──────────────────────────────────────────────
    ' Valoare · Cerut · Curent · Tip. This is where the operator SEES that a file asked for 1 and
    ' the machine holds 0 — the discrepancy that was previously invisible.
    Private Sub RefreshPrefsGrid()
        Try
            gridPrefs.Rows.Clear()
            If gridPrefs.Columns.Count = 0 Then
                gridPrefs.Columns.Add("colName", "Valoare")
                gridPrefs.Columns.Add("colWanted", "Cerut")
                gridPrefs.Columns.Add("colCurrent", "Curent")
                gridPrefs.Columns.Add("colKind", "Tip")
            End If

            Dim hive As String = CurrentAvGeneralPath()
            For Each p As UserPrefIntent In CollectUserPrefs()
                Dim cur As RegistryValueSnapshot = _regAccess.Read(hive, p.Name)
                Dim curText As String = If(cur.Presence = RegPresence.Absent, "(absent)",
                                           Convert.ToString(cur.Value))
                Dim kindText As String = If(p.Action = UserPrefAction.Delete, "—", p.Kind.ToString())
                Dim idx As Integer = gridPrefs.Rows.Add(p.Name, p.RequestedText(), curText, kindText)
                ' Highlight the rows where the machine does not (yet) hold what was asked for.
                Dim agrees As Boolean = RegistryWriteVerifier.Verify(hive, p, cur).Matches
                If Not agrees Then
                    gridPrefs.Rows(idx).DefaultCellStyle.ForeColor = ThemeManager.Current.Palette.ErrorColor
                End If
            Next
            ' No row selected: a highlighted row would paint over the mismatch colour.
            gridPrefs.ClearSelection()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.RefreshPrefsGrid", ex)
        End Try
    End Sub

    ' ── Stare / activare ────────────────────────────────────────────────────────
    ' lblStatus always shows the effective state of all four levers in one block, with the last
    ' action message underneath. Never throws (called from the ctor and from catch paths).
    Private Sub RefreshStatusBlock()
        Try
            Dim hive As String
            Try
                hive = CurrentAvGeneralPath()
            Catch ex As Exception
                GlobalErrorLog.Write("AdobeReaderHarnessForm.RefreshStatusBlock.Hive", ex)
                hive = "?"
            End Try
            Dim sb As New StringBuilder()
            sb.Append("Hive HKCU: ").Append(hive)
            sb.Append("  ·  Decupare: ").Append(If(chkClip.Checked,
                $"activă (dreapta={CInt(numClipRight.Value)}px, sus={CInt(numClipTop.Value)}px)", "inactivă"))
            sb.Append("  ·  Copii ascunși: ").Append(_hiddenChildren.Count.ToString())
            If _hiddenChildTexts.Count > 0 Then sb.Append(" [").Append(String.Join(", ", _hiddenChildTexts)).Append("]")
            sb.AppendLine()
            sb.Append("HKCU aplicat: ").Append(If(_userValuesApplied.Count > 0, String.Join(", ", _userValuesApplied), "—"))
            sb.Append("  ·  Politică HKLM în sesiune: ").Append(If(_machinePolicyApplied, "da", "nu"))
            sb.Append("  ·  Scenariu: ").Append(If(_scenario Is Nothing, "(niciunul)",
                If(_scenario.Name, Path.GetFileName(If(_scenarioPath, "?")))))
            If _lastMessage.Length > 0 Then
                sb.AppendLine()
                sb.Append(_lastMessage)
            End If
            lblStatus.Text = sb.ToString()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.RefreshStatusBlock", ex)
        End Try
    End Sub

    Private Sub ShowStatus(message As String)
        _lastMessage = message
        RefreshStatusBlock()
    End Sub

    ' Everything interactive lives in the options panel — disable it wholesale when Adobe is
    ' absent. Pass/Fail (pnlButtons) stay usable.
    Private Sub SetControlsEnabled(enabled As Boolean)
        flowOptions.Enabled = enabled
    End Sub

    ' Sections track the panel width. A FlowLayoutPanel ignores Dock on its children, so the width
    ' is set here instead: client width minus padding, and minus the vertical scrollbar when it is
    ' showing (otherwise the sections would be just wide enough to trigger a horizontal scrollbar
    ' too). Called on every resize of the panel, i.e. also on every splitter drag.
    Private Sub flowOptions_SizeChanged(sender As Object, e As EventArgs) Handles flowOptions.SizeChanged
        Try
            SizeSections()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.flowOptions_SizeChanged", ex)
        End Try
    End Sub

    Private Sub SizeSections()
        Dim usable As Integer = flowOptions.ClientSize.Width - flowOptions.Padding.Horizontal
        If flowOptions.VerticalScroll.Visible Then usable -= SystemInformation.VerticalScrollBarWidth
        If usable <= 0 Then Return
        For Each c As Control In flowOptions.Controls
            Dim w As Integer = usable - c.Margin.Horizontal
            If w <= 0 Then Continue For
            ' Setting .Width alone is futile on an AutoSize control — the layout engine recomputes
            ' it from the content and the section then overflows the panel sideways. Pinning
            ' Minimum and Maximum width to the same value leaves AutoSize governing the HEIGHT
            ' only, which is exactly what a stacked section needs. Height 0 = unconstrained.
            c.MinimumSize = New Size(w, 0)
            c.MaximumSize = New Size(w, 0)
            c.Width = w
        Next
    End Sub

    ' Hosted-window-dependent buttons + the probe-fed auto-measure.
    Private Sub UpdateActionStates()
        If String.IsNullOrEmpty(_adobePath) Then Return
        Dim hosted As Boolean = _hostedWindow <> IntPtr.Zero
        btnProbe.Enabled = hosted
        btnSendShiftF4.Enabled = hosted
        btnSendF4.Enabled = hosted
        btnHideChild.Enabled = hosted AndAlso lstChildren.Items.Count > 0
        btnShowChild.Enabled = hosted AndAlso lstChildren.Items.Count > 0
        btnShowAllChildren.Enabled = _hiddenChildren.Count > 0
        btnClipAuto.Enabled = _probeCandidateWidth > 0
        btnRunScenario.Enabled = _scenario IsNot Nothing AndAlso Not _scenarioRunning
    End Sub

    ' Timestamped tee: context log + AppDir\Logs\test_adobe_rhp.log (appended, never
    ' overwritten — the log is a deliverable of this slice).
    Private Sub RhpLog(line As String)
        _log(line)
        Dim dir As String = Path.Combine(AppContext.BaseDirectory, "Logs")
        Directory.CreateDirectory(dir)
        File.AppendAllText(Path.Combine(dir, RHP_LOG_NAME),
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") & "  " & line & Environment.NewLine)
    End Sub

    ' ── Win32 interop ───────────────────────────────────────────────────────────
    Private Const GWL_STYLE As Integer = -16
    Private Const GWL_EXSTYLE As Integer = -20
    Private Const WS_CHILD As Long = &H40000000L
    Private Const WS_POPUP As Long = &H80000000L
    Private Const WS_CAPTION As Long = &HC00000L
    Private Const WS_THICKFRAME As Long = &H40000L
    Private Const WS_SYSMENU As Long = &H80000L
    Private Const WS_MINIMIZEBOX As Long = &H20000L
    Private Const WS_MAXIMIZEBOX As Long = &H10000L

    Private Const SWP_NOZORDER As UInteger = &H4UI
    Private Const SWP_NOACTIVATE As UInteger = &H10UI
    Private Const SWP_FRAMECHANGED As UInteger = &H20UI
    Private Const SWP_SHOWWINDOW As UInteger = &H40UI

    Private Const RDW_INVALIDATE As UInteger = &H1UI
    Private Const RDW_UPDATENOW As UInteger = &H100UI
    Private Const RDW_ALLCHILDREN As UInteger = &H80UI
    Private Const RDW_FRAME As UInteger = &H400UI

    Private Const SW_HIDE As Integer = 0
    Private Const SW_SHOW As Integer = 5
    Private Const GW_HWNDNEXT As UInteger = 2UI
    Private Const GW_CHILD As UInteger = 5UI

    <StructLayout(LayoutKind.Sequential)>
    Private Structure RECT
        Public Left As Integer
        Public Top As Integer
        Public Right As Integer
        Public Bottom As Integer
    End Structure

    Private Delegate Function EnumWindowsProc(hWnd As IntPtr, lParam As IntPtr) As Boolean

    <DllImport("user32.dll")>
    Private Shared Function EnumWindows(callback As EnumWindowsProc, extra As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function IsWindowVisible(hWnd As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function IsWindow(hWnd As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetParent(hWndChild As IntPtr, hWndNewParent As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function MoveWindow(hWnd As IntPtr, x As Integer, y As Integer,
                                       w As Integer, h As Integer, repaint As Boolean) As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetWindowPos(hWnd As IntPtr, hWndInsertAfter As IntPtr,
                                         x As Integer, y As Integer, cx As Integer, cy As Integer,
                                         uFlags As UInteger) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function RedrawWindow(hWnd As IntPtr, lprcUpdate As IntPtr,
                                         hrgnUpdate As IntPtr, flags As UInteger) As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowThreadProcessId(hWnd As IntPtr, ByRef lpdwProcessId As Integer) As Integer
    End Function

    <DllImport("user32.dll")>
    Private Shared Function GetWindow(hWnd As IntPtr, uCmd As UInteger) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowRect(hWnd As IntPtr, ByRef lpRect As RECT) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function ShowWindow(hWnd As IntPtr, nCmdShow As Integer) As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetFocus(hWnd As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
    Private Shared Function GetClassName(hWnd As IntPtr, lpClassName As StringBuilder, nMaxCount As Integer) As Integer
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
    Private Shared Function GetWindowText(hWnd As IntPtr, lpString As StringBuilder, nMaxCount As Integer) As Integer
    End Function

    ' GetWindowLongPtr/SetWindowLongPtr nu există pe 32-bit; alegem varianta potrivită la rulare.
    <DllImport("user32.dll", EntryPoint:="GetWindowLongPtrW")>
    Private Shared Function GetWindowLongPtr64(hWnd As IntPtr, nIndex As Integer) As IntPtr
    End Function

    <DllImport("user32.dll", EntryPoint:="GetWindowLongW")>
    Private Shared Function GetWindowLong32(hWnd As IntPtr, nIndex As Integer) As Integer
    End Function

    <DllImport("user32.dll", EntryPoint:="SetWindowLongPtrW")>
    Private Shared Function SetWindowLongPtr64(hWnd As IntPtr, nIndex As Integer, dwNewLong As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", EntryPoint:="SetWindowLongW")>
    Private Shared Function SetWindowLong32(hWnd As IntPtr, nIndex As Integer, dwNewLong As Integer) As Integer
    End Function

    Private Shared Function GetWindowLongPtrSafe(hWnd As IntPtr, nIndex As Integer) As IntPtr
        If IntPtr.Size = 8 Then Return GetWindowLongPtr64(hWnd, nIndex)
        Return New IntPtr(GetWindowLong32(hWnd, nIndex))
    End Function

    Private Shared Function SetWindowLongPtrSafe(hWnd As IntPtr, nIndex As Integer, val As IntPtr) As IntPtr
        If IntPtr.Size = 8 Then Return SetWindowLongPtr64(hWnd, nIndex, val)
        Return New IntPtr(SetWindowLong32(hWnd, nIndex, val.ToInt32()))
    End Function

    Private Shared Function GetClass(hWnd As IntPtr) As String
        Dim sb As New StringBuilder(256)
        GetClassName(hWnd, sb, sb.Capacity)
        Return sb.ToString()
    End Function

    Private Shared Function GetTitle(hWnd As IntPtr) As String
        Dim sb As New StringBuilder(512)
        GetWindowText(hWnd, sb, sb.Capacity)
        Return sb.ToString()
    End Function

End Class
