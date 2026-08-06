Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports Microsoft.Win32
Imports KBot.Common
' Slice 0024: the hosting primitives (geometry, probe, reparenting, hide/show, Adobe lookup,
' command line) live in KBot.Controls now and are SHARED with ReaderHostPreview in KBot.App. The
' bench keeps only what is bench-specific: scenarios, the registry levers and the panel.
Imports KBot.Controls
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

    ' Timeouts, the probe depth limit and the RHP edge tolerance now live with the hosting code in
    ' KBot.Controls (slice 0024) — the bench and the shipping preview wait exactly as long as each
    ' other and probe exactly as deep as each other.
    Private Const FIND_TIMEOUT_MS As Integer = AdobeWindowHosting.FindTimeoutMs
    Private Const REDRAW_DELAY_MS As Integer = AdobeWindowHosting.RedrawDelayMs
    Private Const PROBE_MAX_DEPTH As Integer = AdobeWindowProbe.DefaultMaxDepth
    Private Const RHP_EDGE_TOLERANCE As Integer = AdobeWindowProbe.RhpEdgeTolerance
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
    Private ReadOnly _lastProbe As New List(Of AdobeWindowNode)()
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

    ' ── Felia 0024-03 ───────────────────────────────────────────────────────────
    ' Captura și desprinderea folosesc ACUM aceleași primitive ca previzualizarea livrată. Bancul
    ' își păstrează orchestrarea (pași de scenariu separați, spinnere live) — vezi worklog-ul
    ' 0024-01 §3 — dar niciuna dintre primitive nu mai are a doua copie.
    Private ReadOnly _capture As New AdobeWindowCapture()
    Private ReadOnly _teardown As New AdobeWindowTeardown()
    Private ReadOnly _creationHook As AdobeCreationHook
    ' Procesele pornite de BANC. Nimic din afara acestei mulțimi nu e omorât vreodată.
    Private ReadOnly _launchedPids As New HashSet(Of Integer)()
    ' Ultima măsurătoare lansare → încorporare, în ms (−1 = încă niciuna).
    Private _lastEmbedMs As Integer = -1
    ' Controlul ActiveX creat la rulare (Nothing când AcroPDF nu e înregistrat pe mașina asta).
    Private _acroHost As AcroPdfHost
    Private _acroClsid As String
    Private _acroSecondPath As String

    Public Sub New(log As Action(Of String))
        _log = log
        InitializeComponent()
        _creationHook = New AdobeCreationHook(AddressOf RhpLog)
        _userSnapshot = New RegistrySnapshotSet(_regAccess)
        PopulateRegistryCombos()
        InitAcroPdfSection()
        _adobePath = AdobeWindowHosting.ResolveAdobePath()
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
        ApplyTabSections()
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
            ' UnhookWinEvent pe TOATE căile, inclusiv cele de eșec (felia 0024-03 §4).
            _creationHook.Dispose()
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
            ' Înainte de ORICE lansare, dacă operatorul a cerut-o: bEnableAv2 = 0. Trebuie să fie
            ' aici, nu după pornire — Adobe citește preferința la startul lui.
            If Not ApplyClassicUiIfRequested() Then
                ShowStatus("Nu am putut aplica bEnableAv2 = 0 — lansarea a fost oprită.")
                Return
            End If
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
        Dim launchedPid As Integer = SafePid(proc)
        If launchedPid > 0 Then _launchedPids.Add(launchedPid)

        ' Felia 0024-03: căutarea NU cere fereastra să fie deja vizibilă — o prinde cât e încă ascunsă
        ' și o ascunde pe loc. Identificarea se face după NUMELE DOCUMENTULUI; PID-ul doar preferă
        ' fereastra pornită de noi și etichetează potrivirea. Nu se condiționează pe «/n»: jurnalul
        ' operatorului arată că Adobe predă documentul unei instanțe existente la FIECARE lansare,
        ' inclusiv cu «/n», deci o căutare strictă pe PID nu găsea nimic și lăsa fereastra reală
        ' plutind pe ecran, cu buton în bara de activități.
        Dim opts As AdobeHostOptions = CurrentHostOptions()
        If opts.UseCreationHook Then _creationHook.Install(launchedPid)

        Dim caught As AdobeCaptureResult =
            Await Task.Run(Function() _capture.Find(launchedPid, baseName, opts))

        If opts.UseCreationHook Then _creationHook.Remove()

        ' O relansare mai nouă a preluat controlul cât timp căutam: curăț procesul propriu.
        If gen <> _generation Then
            SafeKill(proc)
            Return
        End If

        _readerProcess = proc
        _pendingProcess = Nothing
        Dim hwnd As IntPtr = caught.Window
        If hwnd = IntPtr.Zero Then
            _hostedPid = launchedPid
            _lastEmbedMs = -1
            RefreshEmbedTiming()
            ShowStatus("Adobe pornit, dar fereastra nu a apărut în " & (FIND_TIMEOUT_MS \ 1000).ToString() &
                       "s (fără încorporare).")
            Return
        End If

        _hostedWindow = hwnd
        Dim ownerPid As Integer = caught.OwnerPid
        _hostedPid = ownerPid
        _lastEmbedMs = caught.ElapsedMs
        RefreshEmbedTiming()
        RhpLog($"Fereastră prinsă în {caught.ElapsedMs} ms " &
               $"({If(caught.Match = AdobeCaptureMatch.ByPid, "după PID", "după titlu")}), PID {ownerPid}.")
        If caught.Match = AdobeCaptureMatch.ByTitle Then
            RhpLog($"ATENȚIE: fereastra (PID {ownerPid}) NU a fost creată de banc (am pornit PID " &
                   $"{launchedPid}). Nu va fi omorâtă la desprindere.")
        End If

        HostWindow(hwnd)
        ' Abia acum devine vizibilă — după ce a fost făcută copil și așezată.
        _capture.Reveal(hwnd)
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
    ' Mecanica e în AdobeWindowHosting (KBot.Controls) din felia 0024 — bancul și previzualizarea
    ' DDF curăță ACELEAȘI stiluri; înainte, cele două copii divergeau.
    Private Sub HostWindow(hwnd As IntPtr)
        _originalStyle = AdobeWindowHosting.AttachAsChild(hwnd, pnlHost.Handle)
        NudgeRedraw()
    End Sub

    ' The bounds of the hosted window inside pnlHost. Without clipping it fills the panel; with
    ' clipping it is oversized and offset so the clipped bands (top toolbar strip / right pane
    ' strip) fall OUTSIDE the visible client area of pnlHost.
    '
    ' The dx/dy/dw/dh deltas apply to the SAME window and compose on top of the clip — they are the
    ' general form of what clip right/top do in two fixed directions. Because everything goes
    ' through here, they are re-applied for free by LayoutHostedWindow, NudgeRedraw, the resize /
    ' splitter debounce and every relaunch: there is nothing to keep re-imposing on a timer.
    ' The arithmetic itself is AdobeHostGeometry.Compute (KBot.Controls, slice 0024): the bench reads
    ' the numbers off its spinners, the DDF preview reads them off a profile, and both then place the
    ' window through the SAME function.
    Private Function HostedBounds() As Rectangle
        Return AdobeHostGeometry.Compute(pnlHost.ClientSize.Width, pnlHost.ClientSize.Height,
                                         chkClip.Checked, CInt(numClipRight.Value), CInt(numClipTop.Value),
                                         CInt(numDx.Value), CInt(numDy.Value),
                                         CInt(numDw.Value), CInt(numDh.Value))
    End Function

    Private Sub LayoutHostedWindow()
        If _hostedWindow = IntPtr.Zero Then Return
        AdobeWindowHosting.Place(_hostedWindow, HostedBounds())
    End Sub

    ' Forțează afișarea/redesenarea ferestrei reparentate (altfel rămâne nevăzută până la o
    ' schimbare de layout a formularului).
    Private Sub NudgeRedraw()
        If _hostedWindow = IntPtr.Zero Then Return
        AdobeWindowHosting.NudgeRedraw(_hostedWindow, HostedBounds())
        pnlHost.Invalidate(True)
    End Sub

    ' ── Argumente / preview ─────────────────────────────────────────────────────
    ' Sintaxă Adobe: [/n] [/s] [/A "param1&param2&…"] "cale.pdf". Parametrii /A trebuie ÎNAINTE de
    ' fișier — regula e codificată o singură dată, în AdobeWindowHosting.BuildArguments (felia 0024).
    Private Function BuildArguments(pdf As String) As String
        Return AdobeWindowHosting.BuildArguments(chkNewInstance.Checked, chkNoSplash.Checked,
                                                 BuildOpenParameters(), pdf)
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
        Dim hostedWindow As IntPtr = _hostedWindow
        _hostedWindow = IntPtr.Zero
        _originalStyle = IntPtr.Zero
        _hostedPid = 0

        ' Felia 0024-03: desprinderea trece prin AdobeWindowTeardown, în modul ales de operator.
        ' NICIODATĂ nu se repune stilul original și nu se re-parentează — asta lăsa în urmă o
        ' fereastră Adobe vie, cu buton în bara de activități, la fiecare schimbare de document.
        ' Un PID pe care bancul nu l-a pornit nu e omorât nici aici.
        If hostedWindow <> IntPtr.Zero OrElse hostedPid > 0 Then
            Dim outcome As AdobeTeardownOutcome = _teardown.Run(
                hostedWindow, hostedPid, _launchedPids, CurrentDetachMode(), CurrentCloseGraceMs())
            If outcome.Message.Length > 0 Then RhpLog(outcome.Message)
            If outcome.Action = AdobeTeardownAction.Killed OrElse
               outcome.Action = AdobeTeardownAction.ClosedThenKilled Then
                _launchedPids.Remove(hostedPid)
            End If
        End If

        _creationHook.Remove()

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

        ' The dx/dy/dw/dh deltas deliberately SURVIVE here: they describe where the next hosted
        ' window should go, not where this dead one was, and HostedBounds re-applies them the moment
        ' something is hosted again — the same way the clip settings survive a relaunch.
        UpdateActionStates()
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

    ' ── Căutarea ferestrei Reader / localizarea Adobe ───────────────────────────
    ' Ambele au trecut în AdobeWindowHosting (KBot.Controls, felia 0024). Căutarea rulează pe fir de
    ' fundal, la fel ca înainte — apelantul e cel care o pune pe Task.Run.
    Private Shared Function FindReaderWindow(baseName As String) As IntPtr
        Return AdobeWindowHosting.FindReaderWindow(baseName)
    End Function

    ' ══ Închidere / captură (felia 0024-03) ════════════════════════════════════
    ' Cele patru manete care fac reproductibilă comparația A vs B. Toate citesc DIN CONTROALE, ca
    ' orice altceva în banc: un scenariu încărcat s-a scris deja în ele.
    Private Function CurrentDetachMode() As AdobeDetachMode
        If rdoDetachClose.Checked Then Return AdobeDetachMode.CloseWindow
        Return AdobeDetachMode.KillProcess
    End Function

    Private Function CurrentCloseGraceMs() As Integer
        Return CInt(numCloseGrace.Value)
    End Function

    Private Function CurrentHostOptions() As AdobeHostOptions
        Return New AdobeHostOptions() With {
            .DetachMode = CurrentDetachMode(),
            .UseCreationHook = chkCreationHook.Checked,
            .CaptureDelayMs = CInt(numCaptureDelay.Value),
            .CloseGraceMs = CurrentCloseGraceMs(),
            .FindTimeoutMs = FIND_TIMEOUT_MS}
    End Function

    ' Numărul cu care se compară A și B. Fără el, «care mod e mai bun» rămâne o impresie.
    Private Sub RefreshEmbedTiming()
        If lblEmbedTiming Is Nothing OrElse lblEmbedTiming.IsDisposed Then Return
        Dim mode As String = If(CurrentDetachMode() = AdobeDetachMode.KillProcess, "A", "B")
        If _lastEmbedMs < 0 Then
            lblEmbedTiming.Text = $"Timp lansare → încorporare: — (mod {mode})"
        Else
            lblEmbedTiming.Text = $"Timp lansare → încorporare: {_lastEmbedMs} ms (mod {mode})"
        End If
    End Sub

    Private Sub HostingSettingsChanged(sender As Object, e As EventArgs) _
        Handles rdoDetachKill.CheckedChanged, rdoDetachClose.CheckedChanged,
                numCaptureDelay.ValueChanged, numCloseGrace.ValueChanged,
                chkCreationHook.CheckedChanged
        Try
            If _loading Then Return
            RefreshEmbedTiming()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.HostingSettingsChanged", ex)
        End Try
    End Sub

    ' «Aplică bEnableAv2 = 0 înainte de fiecare lansare». Reia EXACT calea de aplicare existentă
    ' (instantaneu o dată pe sesiune + dialogul de consimțământ înainte de a omorî un Adobe străin)
    ' — nicio mașinărie nouă de registry, doar rândul din panou pus pe 0 și aplicat.
    Private Function ApplyClassicUiIfRequested() As Boolean
        If Not chkForceClassicUi.Checked Then Return True
        Try
            RhpLog("Forțez interfața clasică înainte de lansare: bEnableAv2 = 0.")
            _loading = True
            Try
                cboEnableAv2.Text = "0"
                cboEnableAv2.SelectionLength = 0
            Finally
                _loading = False
            End Try
            RefreshPrefsGrid()
            Return ApplyUserPrefsCore()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.ApplyClassicUiIfRequested", ex)
            Return False
        End Try
    End Function

    ' ══ Filele suprafeței de document ═══════════════════════════════════════════
    '
    ' Sunt DOUĂ suprafețe care fac același lucru pe căi diferite: fereastra Adobe găzduită și
    ' controlul ActiveX. Până acum una ocupa panoul din dreapta, cealaltă stătea strivită într-o
    ' secțiune din panoul de OPȚIUNI, iar manetele amândurora erau amestecate în aceeași listă — nu
    ' se puteau nici compara, nici judeca separat. Acum fiecare are fila ei, la aceeași dimensiune,
    ' iar panoul din stânga arată DOAR manetele suprafeței selectate.

    ' Secțiunile care aparțin exclusiv suprafeței ActiveX.
    Private Shared ReadOnly ActiveXSections As String() = {"grpActiveX"}

    ' Secțiunile valabile pentru AMBELE: alegerea documentului și cele două zone de registry
    ' (preferințele Adobe afectează deopotrivă fereastra găzduită și controlul in-process).
    Private Shared ReadOnly SharedSections As String() = {"grpFile", "grpUser", "grpMachine"}

    Private Sub tabsMain_SelectedIndexChanged(sender As Object, e As EventArgs) Handles tabsMain.SelectedIndexChanged
        Try
            ApplyTabSections()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.tabsMain_SelectedIndexChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Shows only the option sections that belong to the selected surface. Sections are HIDDEN, not
    ''' removed, so their order in the flow — which a layout test pins — never changes.
    ''' </summary>
    Private Sub ApplyTabSections()
        Dim onActiveX As Boolean = tabsMain.SelectedTab Is tabActiveX
        For Each g As GroupBox In flowOptions.Controls.OfType(Of GroupBox)()
            If SharedSections.Contains(g.Name) Then
                g.Visible = True
            ElseIf ActiveXSections.Contains(g.Name) Then
                g.Visible = onActiveX
            Else
                g.Visible = Not onActiveX
            End If
        Next
        ShowStatus(If(onActiveX,
                      "Suprafață: ActiveX (AcroPDF). Manetele din stânga sunt cele ale controlului.",
                      "Suprafață: fereastra Adobe găzduită. Manetele din stânga sunt cele ale ferestrei."))
    End Sub

    ' ══ ActiveX (AcroPDF) — evaluare, felia 0024-03 §8 ══════════════════════════
    '
    ' ÎNTREBAREA la care există secțiunea asta, și singura care contează: se randează DDF-ul XFA real
    ' în controlul ActiveX, sau apare substitutul Adobe («Please wait… if this message is not
    ' eventually replaced»)? XFA e tot motivul pentru care există suprafața asta. Un «nu» aici
    ' înseamnă că găzduirea de ferestre rămâne singura cale; un «da» deschide discuția.
    '
    ' Nimic din KBot.App nu depinde de asta în felia curentă.
    Private Sub InitAcroPdfSection()
        Try
            Dim raw As String = AcroPdfDetector.ResolveClsid()
            _acroClsid = AcroPdfDetector.NormaliseClsid(raw)
            If String.IsNullOrEmpty(_acroClsid) Then
                lblAcroStatus.Text = "Controlul AcroPDF nu este înregistrat pe această mașină."
                SetAcroControlsEnabled(False)
                RhpLog($"AcroPDF: ProgID «{AcroPdfDetector.ProgId}» nu are CLSID în HKCR — " &
                       "controlul nu e înregistrat. ACESTA E UN REZULTAT VALID de consemnat.")
                Return
            End If
            lblAcroStatus.Text = $"AcroPDF înregistrat, CLSID {raw}. Alege un PDF cu «Deschide PDF…», apoi încarcă-l."
            RhpLog($"AcroPDF: CLSID citit din HKCR\{AcroPdfDetector.ProgId}\CLSID = {raw}.")
            SetAcroControlsEnabled(True)
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.InitAcroPdfSection", ex)
            lblAcroStatus.Text = "AcroPDF: detecție eșuată (vezi jurnalul de erori)."
            SetAcroControlsEnabled(False)
        End Try
    End Sub

    Private Sub SetAcroControlsEnabled(enabled As Boolean)
        btnAcroLoad.Enabled = enabled
        btnAcroSecond.Enabled = enabled
        btnAcroClear.Enabled = enabled
        pnlAcroHost.Enabled = enabled
    End Sub

    ' Creează controlul la prima folosire. AxHost are nevoie de un handle înainte ca GetOcx() să
    ' întoarcă ceva, deci îl adăugăm în panou și abia apoi îl folosim.
    Private Function EnsureAcroHost() As AcroPdfHost
        If _acroHost IsNot Nothing Then Return _acroHost
        If String.IsNullOrEmpty(_acroClsid) Then Return Nothing
        Dim host As New AcroPdfHost(_acroClsid) With {.Dock = DockStyle.Fill, .Name = "axAcroPdf"}
        pnlAcroHost.Controls.Add(host)
        ' Forțează crearea ferestrei; fără asta GetOcx() întoarce Nothing.
        Dim unused As IntPtr = host.Handle
        _acroHost = host
        Dim version As String = host.TryReadVersion()
        If String.IsNullOrEmpty(version) Then
            RhpLog("AcroPDF: controlul nu expune o proprietate de versiune pe acest build.")
        Else
            RhpLog("AcroPDF versiune: " & version)
        End If
        Return host
    End Function

    Private Sub btnAcroLoad_Click(sender As Object, e As EventArgs) Handles btnAcroLoad.Click
        Try
            LoadIntoAcro(_pdfPath, "documentul curent")
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnAcroLoad_Click", ex)
            RhpLog("AcroPDF: încărcare eșuată — " & ex.Message)
            ShowStatus("AcroPDF: încărcare eșuată (vezi jurnalul).")
        End Try
    End Sub

    ' Al DOILEA document în ACELAȘI control — schimbarea de document e tot rostul comparației.
    Private Sub btnAcroSecond_Click(sender As Object, e As EventArgs) Handles btnAcroSecond.Click
        Try
            Using dlg As New System.Windows.Forms.OpenFileDialog()
                dlg.Filter = "Fișiere PDF (*.pdf)|*.pdf|Toate fișierele (*.*)|*.*"
                dlg.Title = "Al doilea document pentru controlul ActiveX"
                If dlg.ShowDialog(Me) <> DialogResult.OK Then Return
                _acroSecondPath = dlg.FileName
            End Using
            LoadIntoAcro(_acroSecondPath, "al doilea document")
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnAcroSecond_Click", ex)
            RhpLog("AcroPDF: încărcarea celui de-al doilea document a eșuat — " & ex.Message)
        End Try
    End Sub

    ' Depth 4 is enough for the hosted window; the ActiveX control nests its panes deeper, and a tree
    ' cut off above the pane that matters answers nothing.
    Private Const ACRO_PROBE_DEPTH As Integer = 7

    ''' <summary>
    ''' Depth used ONLY by the floating-bar watch. Deliberately far past what the pane tree needs.
    '''
    ''' The watch of 09:43 found no transient child at depth 7, which points at «the bar is not a
    ''' window» — but a bar nested deeper than 7 would look exactly the same from here, and that
    ''' alternative has to be ruled out before the conclusion is worth anything. The watch also
    ''' reports the DEEPEST node it actually reached: if that comes back below this limit, the tree
    ''' was exhausted and depth is eliminated as an explanation rather than merely not observed.
    ''' </summary>
    Private Const ACRO_WATCH_DEPTH As Integer = 14

    ' The floating bar's remembered position, and the class that identified it. Nothing is persisted
    ' to disk yet — first the window has to be identified beyond doubt.
    Private _hudRect As Rectangle = Rectangle.Empty
    Private _hudClass As String = Nothing
    Private _hudText As String = Nothing

    ''' <summary>
    ''' Finds the floating bar (Adobe's HUD) and remembers where it is.
    '''
    ''' WHY THE OTHER PROBE CANNOT SEE IT. <see cref="AdobeWindowProbe"/> walks the CHILDREN of the
    ''' ActiveX control; the floating bar is a TOP-LEVEL popup, so it is not in that tree at all.
    '''
    ''' WHY THIS IS EASIER THAN IT LOOKS. AcroPDF is an IN-PROCESS COM server — the control runs
    ''' inside this very process, not inside a separate Acrobat. So the popup belongs to OUR process
    ''' id, and none of the cross-process caveats that dog the hosted window apply here.
    '''
    ''' The position is NOT in the registry: the operator moved the bar between two AVGeneral
    ''' snapshots and not one of the 106 values changed (`iNumUserDockUndockHUD` is a dock/undock
    ''' COUNTER, still 0). So it lives in memory, which means the only way to reproduce it is to move
    ''' the window — hence <see cref="btnAcroHudApply_Click"/>.
    ''' </summary>
    Private Sub btnAcroHud_Click(sender As Object, e As EventArgs) Handles btnAcroHud.Click
        Try
            Dim myPid As Integer = Process.GetCurrentProcess().Id
            Dim win As INativeWindows = Win32Windows.Instance
            Dim ours As New HashSet(Of IntPtr)()
            For Each f As Form In Application.OpenForms
                If f.IsHandleCreated Then ours.Add(f.Handle)
            Next

            RhpLog($"── Sondă bara plutitoare (ferestre de nivel superior ale procesului {myPid}) ──")
            Dim best As IntPtr = IntPtr.Zero
            Dim bestArea As Long = Long.MaxValue
            Dim shown As Integer = 0

            For Each h As IntPtr In win.EnumTopLevelWindows()
                If win.OwnerPid(h) <> myPid Then Continue For
                If ours.Contains(h) Then Continue For            ' our own forms
                Dim cls As String = win.GetClass(h)
                Dim txt As String = win.GetTitle(h)
                Dim r As Rectangle = AdobeWindowHosting.RectInParent(h)
                Dim vis As Boolean = win.IsWindowVisible(h)
                If r.Width <= 0 OrElse r.Height <= 0 Then Continue For
                shown += 1
                RhpLog($"    cls={cls} text=«{txt}» {r.X},{r.Y} {r.Width}x{r.Height} vis={If(vis, 1, 0)}")

                ' The bar is small, visible and floating. Smallest visible non-form window wins.
                Dim area As Long = CLng(r.Width) * r.Height
                If vis AndAlso area < bestArea Then
                    bestArea = area
                    best = h
                End If
            Next

            If shown = 0 Then
                RhpLog("  Nicio fereastră de nivel superior în afară de formularele noastre. " &
                       "Bara plutitoare nu era pe ecran în acest moment — arat-o întâi (mișcă mouse-ul " &
                       "peste document), apoi sondează.")
                ShowStatus("Bara plutitoare nu e vizibilă acum.")
                Return
            End If

            If best = IntPtr.Zero Then
                RhpLog("  Niciun candidat VIZIBIL — nu rețin nimic.")
                Return
            End If

            _hudRect = AdobeWindowHosting.RectInParent(best)
            _hudClass = win.GetClass(best)
            _hudText = win.GetTitle(best)
            RhpLog($"  REȚINUT ca bară plutitoare: cls={_hudClass} text=«{_hudText}» " &
                   $"la {_hudRect.X},{_hudRect.Y} {_hudRect.Width}x{_hudRect.Height}.")
            RhpLog("  EURISTIC (cea mai mică fereastră vizibilă a procesului, în afara formularelor " &
                   "noastre) — confirmă din listă că e chiar ea înainte de a te baza pe asta.")
            ShowStatus($"Bară plutitoare reținută la {_hudRect.X},{_hudRect.Y}.")
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnAcroHud_Click", ex)
            RhpLog("Sonda barei plutitoare a eșuat — " & ex.Message)
        End Try
    End Sub

    ' Sampling state for the floating-bar watch.
    Private ReadOnly _hudSeen As New Dictionary(Of IntPtr, String)()
    Private _hudWatchTicks As Integer = 0
    ' Deepest node the walk actually reached — the difference between «nothing there» and «I stopped».
    Private _hudDeepest As Integer = 0
    ' 0 = idle, 1 = we re-opened the panes and still owe the closing click that produces the bar.
    Private _acroToggleStage As Integer = 0

    ''' <summary>
    ''' Watches for the floating bar for ten seconds, sampling five times a second.
    '''
    ''' WHY A SNAPSHOT CANNOT WORK (measured 06.08.2026). The one-shot probe reported «niciun candidat
    ''' VIZIBIL» and listed nothing but <c>vis=0</c> windows — because the bar only exists while the
    ''' mouse is over the document, and pressing a button moves the mouse away and dismisses it. The
    ''' bar is transient by design, so it has to be sampled over time while the operator keeps the
    ''' mouse where it belongs.
    '''
    ''' Everything visible is recorded once, with its rectangle, so the bar names itself.
    ''' </summary>
    Private Sub btnAcroHudWatch_Click(sender As Object, e As EventArgs) Handles btnAcroHudWatch.Click
        Try
            _hudSeen.Clear()
            _hudWatchTicks = 0
            _hudDeepest = 0
            RhpLog("── Urmăresc bara plutitoare 10 s ──")
            RhpLog("  MIȘCĂ ACUM mouse-ul peste document și ține-l acolo. Bara există doar cât timp " &
                   "e hover; de asta o sondă instantanee nu o prinde niciodată.")
            tmrHudWatch.Start()
            ShowStatus("Urmăresc bara plutitoare 10 s — mișcă mouse-ul peste document.")
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnAcroHudWatch_Click", ex)
        End Try
    End Sub

    Private Sub tmrHudWatch_Tick(sender As Object, e As EventArgs) Handles tmrHudWatch.Tick
        Try
            _hudWatchTicks += 1
            Dim myPid As Integer = Process.GetCurrentProcess().Id
            Dim win As INativeWindows = Win32Windows.Instance
            Dim ours As New HashSet(Of IntPtr)()
            For Each f As Form In Application.OpenForms
                If f.IsHandleCreated Then ours.Add(f.Handle)
            Next

            ' (a) TOP-LEVEL windows of this process.
            For Each h As IntPtr In win.EnumTopLevelWindows()
                If win.OwnerPid(h) <> myPid Then Continue For
                If ours.Contains(h) OrElse _hudSeen.ContainsKey(h) Then Continue For
                If Not win.IsWindowVisible(h) Then Continue For
                Dim r As Rectangle = AdobeWindowHosting.RectInParent(h)
                If r.Width <= 0 OrElse r.Height <= 0 Then Continue For
                Record(h, win.GetClass(h), win.GetTitle(h), r, "nivel superior")
            Next

            ' (b) CHILDREN of the control. THE FIRST WATCH MISSED THIS, and that is why it found
            ' nothing: it only enumerated top-level windows, so a bar that is a transient CHILD of
            ' the ActiveX control was invisible to the instrument, not absent from the screen.
            If _acroHost IsNot Nothing AndAlso _acroHost.IsHandleCreated Then
                For Each n As AdobeWindowNode In AdobeWindowProbe.Walk(_acroHost.Handle, pnlAcroHost.Handle, ACRO_WATCH_DEPTH)
                    If n.Depth > _hudDeepest Then _hudDeepest = n.Depth
                    If _hudSeen.ContainsKey(n.Hwnd) Then Continue For
                    If Not n.Visible OrElse n.Width <= 0 OrElse n.Height <= 0 Then Continue For
                    ' The panes that are always there are not news; only report the newcomers.
                    If IsAlwaysPresentPane(n.Text) Then Continue For
                    Record(n.Hwnd, n.ClassName, n.Text, n.Bounds, $"copil d={n.Depth}")
                Next
            End If

            If _hudWatchTicks < 50 Then Return      ' 50 x 200 ms = 10 s
            tmrHudWatch.Stop()

            ' The line that decides whether «found nothing» is a conclusion or just a short walk.
            Dim exhausted As Boolean = _hudDeepest < ACRO_WATCH_DEPTH
            RhpLog($"  Adâncime maximă atinsă în arbore: {_hudDeepest} (limita era {ACRO_WATCH_DEPTH}). " &
                   If(exhausted,
                      "Arborele S-A TERMINAT înainte de limită, deci adâncimea NU mai e o explicație posibilă.",
                      "ATENȚIE: s-a atins limita, deci pot exista ferestre mai adânci NEVĂZUTE — " &
                      "ridică limita înainte de a trage vreo concluzie."))

            If _hudSeen.Count = 0 Then
                If exhausted Then
                    RhpLog("  CONCLUZIE: bara plutitoare NU e o fereastră. Zece secunde de eșantionare " &
                           "peste ferestrele de nivel superior ALE procesului ȘI peste tot arborele de " &
                           "copii, care s-a epuizat, nu au surprins nimic nou vizibil. E desenată direct " &
                           "în vederea documentului, deci nu poate fi găsită, mutată sau reținută ca " &
                           "fereastră — ideea de a-i reproduce poziția se închide aici.")
                Else
                    RhpLog("  Nimic nou vizibil, DAR arborele a fost tăiat la limită — încă nu e o concluzie.")
                End If
                ShowStatus("Bara nu a apărut ca fereastră — vezi jurnalul.")
            Else
                RhpLog($"  {_hudSeen.Count} fereastră(e) noi surprinse; reținut cls={_hudClass} " &
                       $"la {_hudRect.X},{_hudRect.Y} {_hudRect.Width}x{_hudRect.Height}.")
                ShowStatus($"Bară plutitoare reținută: {_hudClass}.")
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.tmrHudWatch_Tick", ex)
            tmrHudWatch.Stop()
        End Try
    End Sub

    ' The structural panes appear in every probe; reporting them would bury the one newcomer that
    ' matters under thirty lines of noise.
    Private Shared Function IsAlwaysPresentPane(text As String) As Boolean
        If String.IsNullOrEmpty(text) Then Return False
        Return text.StartsWith("AV", StringComparison.Ordinal) AndAlso
               (text.EndsWith("View", StringComparison.Ordinal) OrElse
                text.EndsWith("ViewForDocs", StringComparison.Ordinal))
    End Function

    Private Sub Record(hwnd As IntPtr, cls As String, text As String, r As Rectangle, where As String)
        _hudSeen(hwnd) = cls
        RhpLog($"    VĂZUT ({where}): cls={cls} text=«{text}» {r.X},{r.Y} {r.Width}x{r.Height}")
        If _hudRect.IsEmpty OrElse CLng(r.Width) * r.Height < CLng(_hudRect.Width) * _hudRect.Height Then
            _hudRect = r
            _hudClass = cls
            _hudText = text
        End If
    End Sub

    ''' <summary>
    ''' Puts the floating bar back where it was remembered. Matched by CLASS + TEXT, never by handle:
    ''' Adobe destroys and recreates this popup, so a handle recorded a moment ago is worthless —
    ''' the same rule slice 0023 established for the hosted window's children.
    ''' </summary>
    Private Sub btnAcroHudApply_Click(sender As Object, e As EventArgs) Handles btnAcroHudApply.Click
        Try
            If _hudRect.IsEmpty OrElse String.IsNullOrEmpty(_hudClass) Then
                ShowStatus("Sondează întâi bara plutitoare, ca să am ce reaplica.")
                Return
            End If

            Dim myPid As Integer = Process.GetCurrentProcess().Id
            Dim win As INativeWindows = Win32Windows.Instance
            Dim target As IntPtr = IntPtr.Zero
            For Each h As IntPtr In win.EnumTopLevelWindows()
                If win.OwnerPid(h) <> myPid Then Continue For
                If Not String.Equals(win.GetClass(h), _hudClass, StringComparison.OrdinalIgnoreCase) Then Continue For
                If Not String.Equals(If(win.GetTitle(h), ""), If(_hudText, ""), StringComparison.OrdinalIgnoreCase) Then Continue For
                target = h
                Exit For
            Next

            If target = IntPtr.Zero Then
                RhpLog($"  Bara plutitoare (cls={_hudClass}) nu e pe ecran acum — nimic de mutat.")
                ShowStatus("Bara plutitoare nu e vizibilă acum.")
                Return
            End If

            Dim before As Rectangle = AdobeWindowHosting.RectInParent(target)
            AdobeWindowHosting.Place(target, _hudRect)
            Dim after As Rectangle = AdobeWindowHosting.RectInParent(target)
            RhpLog($"  Bară plutitoare: cerut {_hudRect.X},{_hudRect.Y} {_hudRect.Width}x{_hudRect.Height} — " &
                   $"{before.X},{before.Y} -> {after.X},{after.Y}.")
            If after.Location <> _hudRect.Location Then
                RhpLog("  ATENȚIE: nu a ajuns unde s-a cerut (Adobe a refuzat sau o repoziționează singur).")
            End If
            ShowStatus($"Bară plutitoare mutată la {after.X},{after.Y}.")
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnAcroHudApply_Click", ex)
            RhpLog("Reaplicarea poziției barei a eșuat — " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Makes Adobe perform its FIRST layout, which it otherwise postpones until the control gets
    ''' some input.
    '''
    ''' THE SYMPTOM THIS FIXES (measured 05.08.2026): on the first document of a session the panel
    ''' stays grey until the operator clicks somewhere inside it; afterwards it never happens again.
    ''' The probe shows exactly that — right after the first <c>LoadFile</c>, 26 of 28 child windows
    ''' are zero-sized and the tab strip is <c>0x0</c> (zero on BOTH axes = nothing laid out). After
    ''' a click it drops to 18 and the panes have real rectangles.
    '''
    ''' Escalates, cheapest first, and reports which step did it — that is the useful outcome, since
    ''' the cheap steps are the ones safe to run on every load.
    ''' </summary>
    Private Sub WakeAcroLayout()
        Try
            If _acroHost Is Nothing OrElse Not _acroHost.IsHandleCreated Then Return

            RhpLog("── Trezesc aranjarea ActiveX ──")
            RhpLog($"  înainte: {DegenerateReport()}")

            ' 1. Focus. The cheapest thing that resembles «the operator clicked in it».
            AdobeWindowHosting.FocusWindow(_acroHost.Handle)
            If Not IsAcroLayoutDegenerate() Then
                RhpLog("  REZOLVAT doar cu focus — " & DegenerateReport())
                ShowStatus("Aranjarea Adobe s-a făcut la focus.")
                Return
            End If

            ' 2. A size change. Adobe recomputes its layout on a size CHANGE, not on a repaint.
            Dim client As New Rectangle(0, 0, pnlAcroHost.ClientSize.Width, pnlAcroHost.ClientSize.Height)
            AdobeWindowHosting.NudgeRedraw(_acroHost.Handle, client)
            If Not IsAcroLayoutDegenerate() Then
                RhpLog("  REZOLVAT cu schimbarea de dimensiune — " & DegenerateReport())
                ShowStatus("Aranjarea Adobe s-a făcut la schimbarea de dimensiune.")
                Return
            End If

            ' 3. A synthetic click into the document view — what the operator does by hand. Last,
            ' because a click lands IN the document and could select something.
            Dim nodes As List(Of AdobeWindowNode) =
                AdobeWindowProbe.Walk(_acroHost.Handle, pnlAcroHost.Handle, ACRO_PROBE_DEPTH)
            Dim target As AdobeWindowNode = nodes.FirstOrDefault(
                Function(n) String.Equals(n.Text, "AVDocumentMainView", StringComparison.OrdinalIgnoreCase))
            If target Is Nothing Then
                RhpLog("  AVDocumentMainView negăsit — nu am unde să dau click.")
                Return
            End If
            AdobeWindowHosting.ClickCentre(target.Hwnd)
            RhpLog("  Am dat click sintetic în AVDocumentMainView (ultima treaptă). " & DegenerateReport())
            ShowStatus("Am trezit controlul cu un click sintetic — vezi jurnalul.")
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.WakeAcroLayout", ex)
            RhpLog("Trezirea aranjării ActiveX a eșuat — " & ex.Message)
        End Try
    End Sub

    ' «Not laid out» is not a guess: the document view having no height is the unambiguous marker.
    Private Function IsAcroLayoutDegenerate() As Boolean
        If _acroHost Is Nothing OrElse Not _acroHost.IsHandleCreated Then Return False
        Dim nodes As List(Of AdobeWindowNode) =
            AdobeWindowProbe.Walk(_acroHost.Handle, pnlAcroHost.Handle, ACRO_PROBE_DEPTH)
        Dim page As AdobeWindowNode = nodes.FirstOrDefault(
            Function(n) String.Equals(n.Text, "AVSplitationPageView", StringComparison.OrdinalIgnoreCase))
        Return page Is Nothing OrElse page.Height <= 0 OrElse page.Width <= 0
    End Function

    Private Function DegenerateReport() As String
        Dim nodes As List(Of AdobeWindowNode) =
            AdobeWindowProbe.Walk(_acroHost.Handle, pnlAcroHost.Handle, ACRO_PROBE_DEPTH)
        Dim zero As Integer = nodes.Where(Function(n) n.Width <= 0 OrElse n.Height <= 0).Count()
        Dim page As AdobeWindowNode = nodes.FirstOrDefault(
            Function(n) String.Equals(n.Text, "AVSplitationPageView", StringComparison.OrdinalIgnoreCase))
        Dim pageText As String = If(page Is Nothing, "absent", $"{page.Width}x{page.Height}")
        Return $"{nodes.Count} ferestre, {zero} de dimensiune zero, document {pageText}"
    End Function

    ''' <summary>
    ''' Presses Adobe's OWN collapse button, so Adobe performs its own re-layout.
    '''
    ''' THE CORRECTION THAT MADE THIS NECESSARY (measured 05.08.2026). «Ascunde chrome-ul» reported
    ''' three windows ASCUNS and changed nothing useful: hiding leaves the WIDTH, so Adobe never
    ''' reflows. The probe is unambiguous —
    '''
    '''   after ShowWindow(SW_HIDE):  AVDockableTabStripView 67x697 vis=0, document at x=67 w=792
    '''   the state actually wanted:  AVDockableTabStripView  0x697 vis=0, document at x=0  w=859
    '''
    ''' Adobe's collapse sets the width to ZERO and moves the siblings. Visibility was a consequence
    ''' of that, not the mechanism, and reading the correlation as the cause is what produced the
    ''' useless button. So: press Adobe's button and let Adobe do the layout.
    '''
    ''' The button TOGGLES, so this only clicks when the strip is actually expanded — otherwise it
    ''' would re-open the pane it is supposed to close.
    ''' </summary>
    Private Sub btnAcroCollapse_Click(sender As Object, e As EventArgs) Handles btnAcroCollapse.Click
        Try
            If _acroHost Is Nothing OrElse Not _acroHost.IsHandleCreated Then
                ShowStatus("Încarcă întâi un document în ActiveX.")
                Return
            End If

            Dim nodes As List(Of AdobeWindowNode) =
                AdobeWindowProbe.Walk(_acroHost.Handle, pnlAcroHost.Handle, ACRO_PROBE_DEPTH)
            Dim strip As AdobeWindowNode = nodes.FirstOrDefault(
                Function(n) String.Equals(n.Text, "AVDockableTabStripView", StringComparison.OrdinalIgnoreCase))

            RhpLog("── Colapsez panourile prin butonul lui Adobe ──")
            If strip Is Nothing Then
                RhpLog("  AVDockableTabStripView NEGĂSIT — nimic de colapsat.")
                ShowStatus("Banda de file nu există în arbore.")
                Return
            End If
            ' ZERO LĂȚIME ȘI ZERO ÎNĂLȚIME NU ÎNSEAMNĂ „COLAPSAT" — înseamnă „Adobe încă nu a
            ' aranjat nimic". Distincția a costat o rulare: la 21:18:03 butonul ăsta a văzut 0x0, a
            ' zis «deja colapsată» și a refuzat să apese, deși starea reală era „nearanjat".
            ' Colapsat cu adevărat = lățime 0 dar ÎNĂLȚIME ÎNTREAGĂ (0x697).
            If strip.Height <= 0 Then
                RhpLog($"  AVDockableTabStripView e {strip.Width}x{strip.Height} — Adobe NU și-a " &
                       "făcut încă aranjarea (zero pe AMBELE axe nu e «colapsat»). Trezesc controlul.")
                WakeAcroLayout()
                Return
            End If
            ' DEJA COLAPSAT NU ÎNSEAMNĂ „NIMIC DE FĂCUT" — și asta a fost greșeala.
            '
            ' Bara plutitoare apare ca URMARE A ACȚIUNII de colapsare, nu ca urmare a STĂRII de
            ' colapsat. Starea persistă între sesiuni (măsurat: la 09:35:18 banda era deja 0x697
            ' înainte de orice click, după repornirea aplicației), deci la pornire Adobe e deja
            ' colapsat, butonul ăsta zicea «nu apăs», nicio colapsare nu se mai întâmpla în sesiune
            ' — și bara nu mai apărea deloc. Nu o fereastră ascunsă a stricat-o, ci acțiunea care
            ' lipsea.
            '
            ' Deci: dacă e deja colapsat, COMUTĂM DE DOUĂ ORI — deschidem și închidem la loc — ca
            ' Adobe să execute chiar acțiunea de colapsare în sesiunea curentă.
            If strip.Width <= 0 Then
                Dim reopen As AdobeWindowNode = NearestCollapseButton(nodes, strip)
                If reopen Is Nothing Then
                    RhpLog("  Deja colapsat, dar nu găsesc butonul ca să comut — nu pot forța acțiunea.")
                    Return
                End If
                RhpLog($"  Deja colapsat ({strip.Width}x{strip.Height}). Bara plutitoare apare la " &
                       "ACȚIUNEA de colapsare, nu la starea de colapsat — deci deschid și închid la " &
                       "loc, ca Adobe să execute acțiunea acum.")
                AdobeWindowHosting.ClickCentre(reopen.Hwnd)
                _acroToggleStage = 1
                tmrAcroVerify.Stop()
                tmrAcroVerify.Start()
                ShowStatus("Comut panourile ca să reapară bara plutitoare…")
                Return
            End If

            ' The collapse button that belongs to the strip sits immediately to its right.
            Dim button As AdobeWindowNode = NearestCollapseButton(nodes, strip)
            If button Is Nothing Then
                RhpLog("  AVExpandCollapseButtonView NEGĂSIT — nu am pe ce apăsa.")
                Return
            End If

            Dim before As String = $"bandă {strip.Width}x{strip.Height} la x={strip.Bounds.X}"
            RhpLog($"  Apăs butonul de la x={button.Bounds.X} ({button.Width}x{button.Height}, " &
                   $"hwnd=0x{button.Hwnd.ToInt64():X}); înainte: {before}.")
            AdobeWindowHosting.ClickCentre(button.Hwnd)

            ' Adobe re-lays-out asynchronously; re-probe after a beat and report the number that
            ' decides it — where the DOCUMENT view ends up.
            tmrAcroVerify.Stop()
            tmrAcroVerify.Start()
            ShowStatus("Am apăsat butonul de colapsare; verific în 600 ms.")
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnAcroCollapse_Click", ex)
            RhpLog("Colapsarea prin buton a eșuat — " & ex.Message)
        End Try
    End Sub

    ' Re-probes after the click and states plainly whether the document view reflowed. A click Adobe
    ' ignored must not read like a success — the same rule as everywhere else in this bench.
    ''' <summary>The collapse button belonging to a strip: the nearest one to its right edge.</summary>
    Private Shared Function NearestCollapseButton(nodes As List(Of AdobeWindowNode),
                                                  strip As AdobeWindowNode) As AdobeWindowNode
        Return nodes.
            Where(Function(n) String.Equals(n.Text, "AVExpandCollapseButtonView", StringComparison.OrdinalIgnoreCase)).
            OrderBy(Function(n) Math.Abs(n.Bounds.X - (strip.Bounds.X + strip.Width))).
            FirstOrDefault()
    End Function

    Private Sub tmrAcroVerify_Tick(sender As Object, e As EventArgs) Handles tmrAcroVerify.Tick
        Try
            tmrAcroVerify.Stop()
            If _acroHost Is Nothing OrElse Not _acroHost.IsHandleCreated Then Return

            Dim nodes As List(Of AdobeWindowNode) =
                AdobeWindowProbe.Walk(_acroHost.Handle, pnlAcroHost.Handle, ACRO_PROBE_DEPTH)

            ' Second half of the «already collapsed» toggle: it is open again now, so close it — and
            ' THAT closing is the action that makes the floating bar appear.
            If _acroToggleStage = 1 Then
                _acroToggleStage = 0
                Dim s As AdobeWindowNode = nodes.FirstOrDefault(
                    Function(n) String.Equals(n.Text, "AVDockableTabStripView", StringComparison.OrdinalIgnoreCase))
                If s Is Nothing Then
                    RhpLog("  Comutare: banda a dispărut din arbore între cele două clickuri.")
                    Return
                End If
                RhpLog($"  Comutare, pasul 2: banda e acum {s.Width}x{s.Height} — o închid la loc.")
                Dim b As AdobeWindowNode = NearestCollapseButton(nodes, s)
                If b Is Nothing Then
                    RhpLog("  Comutare: butonul a dispărut — nu pot închide la loc.")
                    Return
                End If
                AdobeWindowHosting.ClickCentre(b.Hwnd)
                tmrAcroVerify.Start()      ' verify on the next tick
                Return
            End If
            Dim strip As AdobeWindowNode = nodes.FirstOrDefault(
                Function(n) String.Equals(n.Text, "AVDockableTabStripView", StringComparison.OrdinalIgnoreCase))
            Dim page As AdobeWindowNode = nodes.FirstOrDefault(
                Function(n) String.Equals(n.Text, "AVSplitationPageView", StringComparison.OrdinalIgnoreCase))

            If strip Is Nothing OrElse page Is Nothing Then
                RhpLog("  Verificare: arborele nu mai conține banda sau vederea documentului.")
                Return
            End If

            Dim collapsed As Boolean = strip.Width <= 0
            Dim flushLeft As Boolean = page.Bounds.X = 0
            RhpLog($"  După click: bandă {strip.Width}x{strip.Height}, document la x={page.Bounds.X} " &
                   $"lățime {page.Width} (panou {pnlAcroHost.ClientSize.Width}).")
            If collapsed AndAlso flushLeft Then
                RhpLog("  REUȘIT: banda e la lățime zero ȘI documentul s-a lățit la marginea stângă. " &
                       "Asta e starea cerută, iar ea PERSISTĂ la documentele următoare din același " &
                       "proces Adobe (măsurat: supraviețuiește a două încărcări).")
                ShowStatus("Colapsare reușită — documentul ocupă tot panoul.")
            Else
                RhpLog("  FĂRĂ EFECT: Adobe nu a colapsat la click. Un click sintetic peste graniță " &
                       "de proces nu e garantat onorat — rezultat valid de consemnat, nu un bug de " &
                       "ocolit prin redimensionări din afară (acelea nu declanșează re-aranjarea).")
                ShowStatus("Click ignorat de Adobe — vezi jurnalul.")
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.tmrAcroVerify_Tick", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Dumps the window tree INSIDE the ActiveX control.
    '''
    ''' This exists for the pane behaviour reported on 05.08.2026: which of Adobe's bars appear,
    ''' which collapse/expand buttons are present, whether the floating bar shows, and whether the
    ''' document view has any size at all — ALL of it depends on the state left behind by the
    ''' previous document, and all of it is window structure. Run it after each step of a sequence
    ''' and diff the dumps; that is the difference between a description and a diagnosis.
    '''
    ''' Same probe as the hosted window (<see cref="AdobeWindowProbe"/>), so a node that looks odd
    ''' here can be compared directly with the embedded-window case.
    ''' </summary>
    Private Sub btnAcroProbe_Click(sender As Object, e As EventArgs) Handles btnAcroProbe.Click
        Try
            If _acroHost Is Nothing OrElse Not _acroHost.IsHandleCreated Then
                ShowStatus("Încarcă întâi un document în ActiveX.")
                RhpLog("Sondă ActiveX: controlul nu e creat încă — nimic de sondat.")
                Return
            End If

            Dim nodes As List(Of AdobeWindowNode) =
                AdobeWindowProbe.Walk(_acroHost.Handle, pnlAcroHost.Handle, ACRO_PROBE_DEPTH)
            RhpLog($"── Sondă ActiveX (hwnd control 0x{_acroHost.Handle.ToInt64():X}, " &
                   $"panou {pnlAcroHost.ClientSize.Width}x{pnlAcroHost.ClientSize.Height}, " &
                   $"adâncime max {ACRO_PROBE_DEPTH}) ──")
            For Each n As AdobeWindowNode In nodes
                RhpLog(AdobeWindowProbe.DescribeNode(n))
            Next

            ' The one number that decides whether «nu se vede nimic» means «no document» or «the
            ' document view was squeezed to nothing by the panes».
            ' Where().Count(), not Count(predicate): List(Of T).Count is a PROPERTY and shadows the
            ' LINQ overload, so the predicate form does not compile here.
            Dim zeroSized As Integer =
                nodes.Where(Function(n) n.Width <= 0 OrElse n.Height <= 0).Count()
            RhpLog($"Sondă ActiveX: {nodes.Count} ferestre copil, dintre care {zeroSized} de " &
                   "dimensiune ZERO. O vedere de document cu dimensiune zero explică un panou gol " &
                   "fără ca documentul să lipsească.")
            ShowStatus($"Sondă ActiveX: {nodes.Count} ferestre copil ({zeroSized} de dimensiune zero).")
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnAcroProbe_Click", ex)
            RhpLog("Sondă ActiveX: eșec — " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' The window texts that stand between the ActiveX control and the state the operator wants.
    '''
    ''' MEASURED ON 05.08.2026, NOT PROPOSED. Comparing the probe of the «perfect» state against a
    ''' bad one, the whole difference is these windows being invisible:
    '''
    '''   <c>AVDockableTabStripView</c>          67x297 vis=1  ->  0x297 vis=0
    '''   <c>AVExpandCollapseButtonView</c>      27x297 vis=1  ->  27x227 vis=0
    '''   <c>AVSplitationPageView</c> (document) x=67 w=1805    ->  x=0  w=1899  (fills the panel)
    '''
    ''' The document view widens to the full panel BY ITSELF once they are gone — nothing has to be
    ''' resized. <c>AVTaskPaneHostView</c> is included because it is the right-hand Tools pane, the
    ''' original target of slice 0023.
    ''' </summary>
    ''' <summary>
    ''' NOTE THE ABSENCE OF <c>AVExpandCollapseButtonView</c>. Hiding it BREAKS the floating bar,
    ''' measured 06.08.2026: at 09:28:25 that window was vis=1 and the bar worked; «Ascunde chrome-ul»
    ''' ran at 09:28:49 and hid it («1 ascunse»); from 09:28:51 it is vis=0 and the bar never appeared
    ''' again. It is the 27px strip at the right edge that the floating bar hovers out of — hiding it
    ''' removes the hover target.
    '''
    ''' That was also this button's ONLY remaining effect: the same run logs «1 ascunse, 3 deja
    ''' ascuns/zero», i.e. everything else was already gone. So the button was doing nothing except
    ''' the one harmful thing.
    ''' </summary>
    Private Shared ReadOnly AcroChromeTexts As String() = {
        "AVDockableTabStripView", "AVTaskPaneHostView"}

    ''' <summary>The window that must stay visible, and is re-shown if an earlier run hid it.</summary>
    Private Const AcroFloatingBarHost As String = "AVExpandCollapseButtonView"

    ''' <summary>
    ''' Hides the ActiveX control's chrome by window TEXT — the lever that actually works.
    '''
    ''' The control's own API does NOT: `setShowToolbar`, `setShowScrollbars`, `setPageMode`,
    ''' `setLayoutMode` and `setView` all returned OK on every load of 05.08.2026 and the bars stayed
    ''' put. That is the same split slice 0023 hit with the `/A` open parameters — they address
    ''' DOCUMENT chrome, while the tab strip and the Tools pane are APPLICATION chrome.
    '''
    ''' Each window is classified BEFORE being touched, so «already hidden» and «zero-sized» cannot
    ''' be reported as a success — the lesson from pass 4 of slice 0023, applied here before the same
    ''' mistake could be made a second time.
    ''' </summary>
    Private Sub btnAcroHideChrome_Click(sender As Object, e As EventArgs) Handles btnAcroHideChrome.Click
        Try
            If _acroHost Is Nothing OrElse Not _acroHost.IsHandleCreated Then
                ShowStatus("Încarcă întâi un document în ActiveX.")
                Return
            End If

            Dim nodes As List(Of AdobeWindowNode) =
                AdobeWindowProbe.Walk(_acroHost.Handle, pnlAcroHost.Handle, ACRO_PROBE_DEPTH)
            RhpLog("── Ascund chrome-ul ActiveX (după text) ──")

            Dim outcomes As New List(Of HideOutcome)()
            For Each text As String In AcroChromeTexts
                Dim matches As List(Of AdobeWindowNode) = nodes.
                    Where(Function(n) String.Equals(n.Text, text, StringComparison.OrdinalIgnoreCase)).ToList()
                If matches.Count = 0 Then
                    outcomes.Add(HideOutcome.NotFound)
                    RhpLog($"  {HideOutcomeClassifier.Label(HideOutcome.NotFound)}: «{text}»")
                    Continue For
                End If
                For Each m As AdobeWindowNode In matches
                    If Not AdobeWindowHosting.IsAlive(m.Hwnd) Then Continue For
                    ' Classify FIRST — afterwards everything looks hidden, which is exactly how a
                    ' no-op gets logged as a success.
                    Dim outcome As HideOutcome = HideOutcomeClassifier.Classify(
                        found:=True, visible:=AdobeWindowHosting.IsVisible(m.Hwnd),
                        width:=m.Width, height:=m.Height)
                    outcomes.Add(outcome)
                    RhpLog($"  {HideOutcomeClassifier.Label(outcome)}: «{m.Text}» {m.Width}x{m.Height} " &
                           $"(hwnd=0x{m.Hwnd.ToInt64():X})")
                    If outcome = HideOutcome.Hidden Then AdobeWindowHosting.Hide(m.Hwnd)
                Next
            Next

            ' REPARĂ ce a stricat o rulare anterioară: dacă strip-ul barei plutitoare a fost ascuns
            ' de versiunea veche a acestui buton, îl arătăm la loc. Fără asta, bara rămâne dispărută
            ' până la repornirea Adobe și nimic nu spune de ce.
            For Each n As AdobeWindowNode In nodes.
                Where(Function(x) String.Equals(x.Text, AcroFloatingBarHost, StringComparison.OrdinalIgnoreCase))
                If AdobeWindowHosting.IsAlive(n.Hwnd) AndAlso Not AdobeWindowHosting.IsVisible(n.Hwnd) Then
                    AdobeWindowHosting.Show(n.Hwnd)
                    RhpLog($"  REARĂTAT «{n.Text}» (hwnd=0x{n.Hwnd.ToInt64():X}) — el produce bara " &
                           "plutitoare, iar o rulare anterioară îl ascunsese.")
                End If
            Next

            Dim summary As New HideAttemptSummary(AcroChromeTexts.Length, outcomes)
            RhpLog(summary.SummaryLine(1, 1))

            ' SUGESTIA OPERATORULUI, corectată: «forțăm o redesenare după ascundere?»
            ' O REDESENARE nu mută nimic — dreptunghiurile din sondă sunt geometrie reală, nu pictură
            ' veche; documentul chiar STĂ la x=67. Ce poate declanșa re-aranjarea e o SCHIMBARE DE
            ' DIMENSIUNE, iar NudgeRedraw face exact asta (un pixel mai îngust și înapoi). Comentariul
            ' lui din felia 0024 spune de ce, și e răspunsul la întrebare: «Adobe recalculează
            ' layout-ul la o schimbare de DIMENSIUNE, nu la o repictare».
            Dim client As New Rectangle(0, 0, pnlAcroHost.ClientSize.Width, pnlAcroHost.ClientSize.Height)
            AdobeWindowHosting.NudgeRedraw(_acroHost.Handle, client)
            RhpLog("  Am forțat o SCHIMBARE DE DIMENSIUNE pe control (un pixel și înapoi), ca Adobe " &
                   "să reia aranjarea — o simplă repictare nu ar muta nimic.")
            tmrAcroVerify.Stop()
            tmrAcroVerify.Start()
            ShowStatus($"Chrome ActiveX: {summary.HiddenCount} ascunse; verific aranjarea în 600 ms.")
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnAcroHideChrome_Click", ex)
            RhpLog("Ascunderea chrome-ului ActiveX a eșuat — " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Dumps EVERY value under both AVGeneral hives, naming nothing in advance.
    '''
    ''' The pane state survives from one document to the next, so Adobe writes it somewhere. Rather
    ''' than guess a key name — the house rule in <see cref="AdobeRegistryConstants"/> is that an
    ''' invented name is worse than an absent one — enumerate the whole key before and after an
    ''' action and let the value that changed name itself.
    ''' </summary>
    Private Sub btnAcroPrefs_Click(sender As Object, e As EventArgs) Handles btnAcroPrefs.Click
        Try
            RhpLog("── Instantaneu AVGeneral (toate valorile, în ambele hive-uri) ──")
            Dim total As Integer = 0
            For Each hive As String In New String() {
                AdobeRegistryConstants.AvGeneralReader, AdobeRegistryConstants.AvGeneralAcrobat}

                If Not _regAccess.KeyExists(hive) Then
                    RhpLog($"  {hive} — cheia LIPSEȘTE")
                    Continue For
                End If
                Dim names As IReadOnlyList(Of String) = _regAccess.ValueNames(hive)
                RhpLog($"  {hive} — {names.Count} valori")
                For Each n As String In names
                    Dim snap As RegistryValueSnapshot = _regAccess.Read(hive, n)
                    RhpLog($"    {n} = {Convert.ToString(snap.Value)} ({snap.Kind})")
                    total += 1
                Next
            Next
            RhpLog($"Instantaneu AVGeneral: {total} valori. Ia unul ÎNAINTE și unul DUPĂ acțiune — " &
                   "valoarea care s-a schimbat e cea care ține starea panourilor.")
            ShowStatus($"Instantaneu AVGeneral: {total} valori în jurnal.")
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnAcroPrefs_Click", ex)
            RhpLog("Instantaneu AVGeneral: eșec — " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Empties the control by DESTROYING it; the next load recreates it.
    '''
    ''' <c>src = ""</c> does not work on this build — measured 06.08.2026: the call returns without
    ''' error, the log said «control golit», and the document stayed on screen. A method that reports
    ''' success while changing nothing is worse than one that fails, so it is gone.
    ''' </summary>
    Private Sub btnAcroClear_Click(sender As Object, e As EventArgs) Handles btnAcroClear.Click
        Try
            If _acroHost Is Nothing Then
                ShowStatus("Nu e nimic de golit — controlul nu e creat.")
                Return
            End If
            Dim host As AcroPdfHost = _acroHost
            _acroHost = Nothing
            pnlAcroHost.Controls.Remove(host)
            host.Dispose()
            RhpLog("AcroPDF: control DISTRUS (golirea prin «src» nu face nimic pe acest build — " &
                   "întorcea succes și lăsa documentul pe ecran). Următoarea încărcare îl recreează.")
            ShowStatus("AcroPDF golit (controlul a fost distrus).")
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnAcroClear_Click", ex)
            RhpLog("AcroPDF: golirea a eșuat — " & ex.Message)
        End Try
    End Sub

    ' Parametrul NU se numește `path`: VB e insensibil la majuscule, deci `path` ar umbri
    ' System.IO.Path și fiecare `Path.GetFileName` de mai jos ar deveni o căutare de membru pe String.
    Private Sub LoadIntoAcro(pdf As String, what As String)
        If String.IsNullOrEmpty(pdf) Then
            ShowStatus("Alege întâi un PDF (Deschide PDF…).")
            Return
        End If
        Dim host As AcroPdfHost = EnsureAcroHost()
        If host Is Nothing Then
            RhpLog("AcroPDF: controlul nu poate fi creat (CLSID absent).")
            Return
        End If

        ' MĂSURAT PE 05.08.2026, nu presupus: același document NU poate fi deschis simultan în
        ' fereastra găzduită ȘI în controlul ActiveX. Adobe e practic mono-instanță, iar controlul e
        ' servit de ACELAȘI motor Acrobat care ține deja documentul deschis în fereastra încorporată
        ' — a doua cerere nu întoarce nimic și panoul rămâne gri. `LoadFile` întoarce oricum True,
        ' deci fără linia asta simptomul arată ca „ActiveX-ul nu randează XFA", ceea ce e FALS:
        ' documentele DIFERITE se randează corect. O oră s-a pierdut pe teorii despre calea cu spații
        ' și despre XFA înainte ca operatorul să vadă coliziunea.
        If IsHostedDocument(pdf) Then
            Dim warning As String =
                "ATENȚIE: «" & Path.GetFileName(pdf) & "» e deja deschis în fereastra GĂZDUITĂ. " &
                "Același document nu poate fi afișat simultan și în ActiveX — controlul va rămâne " &
                "GRI, iar LoadFile va întoarce oricum True. Desprinde întâi fereastra (relansează " &
                "sau închide) ori alege alt document."
            RhpLog(warning)
            ShowStatus(warning)
        End If

        ' Barele se sting ÎNAINTE și DIN NOU DUPĂ încărcare, iar ambele treceri se loghează separat.
        ' Documentația Adobe și practica nu sunt de acord care ordine ține pe ce build, iar bancul e
        ' exact locul unde întrebarea asta se rezolvă prin măsurare, nu prin citit de forumuri.
        If chkAcroChrome.Checked Then
            RhpLog("AcroPDF: ascund barele ÎNAINTE de încărcare —")
            For Each line As String In host.ApplyChrome()
                RhpLog(line)
            Next
        End If

        Dim ok As Boolean = host.LoadFile(pdf)
        RhpLog($"AcroPDF: LoadFile({what}) = {ok} — «{pdf}».")

        If chkAcroChrome.Checked Then
            RhpLog("AcroPDF: ascund barele DUPĂ încărcare —")
            For Each line As String In host.ApplyChrome()
                RhpLog(line)
            Next
            RhpLog("AcroPDF: DE CONSEMNAT — au dispărut barele? Dacă da, API-ul ăsta înlocuiește " &
                   "decuparea, ascunderea ferestrelor copil și cheile de registry din felia 0023.")
        End If

        ' Prima încărcare a sesiunii rămâne GRI până când operatorul dă click în panou: Adobe își
        ' amână prima aranjare până primește ceva input (măsurat — 26 din 28 de ferestre copil erau
        ' de dimensiune zero imediat după LoadFile). Trezirea e ieftină și se face de fiecare dată;
        ' când aranjarea e deja bună, IsAcroLayoutDegenerate iese imediat și nu se atinge nimic.
        If IsAcroLayoutDegenerate() Then
            RhpLog("AcroPDF: aranjarea e degenerată imediat după încărcare — trezesc controlul.")
            WakeAcroLayout()
        End If

        RhpLog("AcroPDF: VERDICTUL DE CONSEMNAT — se vede documentul XFA randat, sau substitutul " &
               "Adobe («Please wait…»)? Notează care dintre ele, e singura întrebare a acestei secțiuni.")
        ShowStatus($"AcroPDF: LoadFile a întors {ok}. Verifică pe ecran ce s-a randat.")
    End Sub

    ''' <summary>
    ''' Adevărat când <paramref name="path"/> e chiar documentul ținut acum în fereastra găzduită.
    ''' Comparație pe cale completă, insensibilă la majuscule (Windows).
    ''' </summary>
    Private Function IsHostedDocument(pdf As String) As Boolean
        If _hostedWindow = IntPtr.Zero OrElse String.IsNullOrEmpty(pdf) Then Return False
        If String.IsNullOrEmpty(_pdfPath) Then Return False
        Return String.Equals(Path.GetFullPath(pdf), Path.GetFullPath(_pdfPath),
                             StringComparison.OrdinalIgnoreCase)
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
        RhpLog($"── Probă ferestre copil (hwnd gazdă 0x{_hostedWindow.ToInt64():X}, adâncime max {PROBE_MAX_DEPTH}; " &
               "EURISTIC candidat RHP = vizibil, lipit de marginea dreaptă ±" & RHP_EDGE_TOLERANCE.ToString() &
               "px, lățime < 1/2 gazdă) ──")

        ' The walk itself is AdobeWindowProbe (KBot.Controls, slice 0024) — the SAME tree the DDF
        ' preview reads to decide modern vs classic, so a detection bug is one bug, not two.
        Dim nodes As List(Of AdobeWindowNode) = AdobeWindowProbe.Walk(_hostedWindow, pnlHost.Handle, PROBE_MAX_DEPTH)
        For Each n As AdobeWindowNode In nodes
            RhpLog(AdobeWindowProbe.DescribeNode(n))
            lstChildren.Items.Add(n)
            _lastProbe.Add(n)
        Next
        Dim total As Integer = nodes.Count

        Dim candidate As AdobeWindowNode = AdobeWindowProbe.RhpCandidate(nodes, hostW)
        If candidate IsNot Nothing Then
            _probeCandidateWidth = candidate.Width
            _probeCandidateClass = candidate.ClassName
        End If

        ' The viewer generation, read off the same tree. The bench does not ACT on it — the shipping
        ' preview does — but printing it here is how a new Adobe build gets diagnosed on the bench.
        Dim detection As AdobeUiDetection = AdobeUiDetector.Detect(nodes)
        RhpLog(detection.Describe())

        ' The line that actually matters, per target the loaded scenario asks to hide — printed
        ' whether or not it matched, so it cannot be lost in the tree above.
        Dim targets As List(Of String) = ScenarioHideTargets()
        Dim zeroSizedTarget As Boolean = False
        For Each t As String In targets
            Dim hit As AdobeWindowNode = _lastProbe.FirstOrDefault(
                Function(i) String.Equals(i.Text, t, StringComparison.OrdinalIgnoreCase))
            If hit Is Nothing Then
                RhpLog($"PANOU: {t} — NEGĂSIT")
            Else
                Dim vis As Boolean = AdobeWindowHosting.IsVisible(hit.Hwnd)
                ' Rectangles of invisible windows are unreliable — Adobe leaves stale geometry
                ' behind (e.g. a 27x913 child inside a 587-high host). Mark, never trust.
                Dim stale As String = If(vis, "", " (posibil învechit)")
                RhpLog($"PANOU: {t} — {hit.Width}x{hit.Height}{stale}, vis={If(vis, 1, 0)}")
                If hit.Width <= 0 OrElse hit.Height <= 0 Then zeroSizedTarget = True
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

    ' WalkChildren and ChildWindowItem are GONE (slice 0024): AdobeWindowProbe.Walk returns
    ' AdobeWindowNode, whose ToString() produces the same list entry the bench showed before.

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

    ' ══ Poziția ferestrei Adobe — dx/dy/dw/dh on the HOSTED window ══════════════
    '
    ' The deltas drive the SAME window as clip right/top; they are its general form (clip right is
    ' dw, clip top is dy + dh). Everything flows through HostedBounds, so a delta is re-applied by
    ' the existing layout paths — no timer, no re-probing, no per-child SetWindowPos, and the page
    ' scale is whatever the resulting window size implies, exactly as with clipping.
    Private Sub MoveSettingsChanged(sender As Object, e As EventArgs) _
        Handles numDx.ValueChanged, numDy.ValueChanged, numDw.ValueChanged, numDh.ValueChanged
        Try
            If _loading Then Return
            ' Enable «Readu la zero» even with nothing hosted: the deltas are set BEFORE a PDF is
            ' opened just as often as after, and ApplyHostedGeometry returns early in that case.
            UpdateActionStates()
            ApplyHostedGeometry("Poziție")
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.MoveSettingsChanged", ex)
        End Try
    End Sub

    Private Sub btnResetMove_Click(sender As Object, e As EventArgs) Handles btnResetMove.Click
        Try
            ' Reset is exact by construction: the deltas ARE the whole displacement, so zeroing them
            ' restores the untouched geometry. Nothing needs to be recorded beforehand.
            _loading = True
            Try
                numDx.Value = 0
                numDy.Value = 0
                numDw.Value = 0
                numDh.Value = 0
            Finally
                _loading = False
            End Try
            ApplyHostedGeometry("Poziție readusă la zero")
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnResetMove_Click", ex)
        End Try
    End Sub

    ' Re-places the hosted window and REPORTS WHAT ACTUALLY HAPPENED: the rectangle asked for next
    ' to the rectangle the window ended up with. Adobe can refuse or clamp a size, and a request
    ' that was silently ignored must not read like a success — the hideChildren lesson from pass 4.
    Private Sub ApplyHostedGeometry(what As String)
        If _hostedWindow = IntPtr.Zero Then
            RefreshStatusBlock()
            Return
        End If
        Dim before As Rectangle = ChildRectInParent(_hostedWindow)
        Dim wanted As Rectangle = HostedBounds()
        LayoutHostedWindow()
        NudgeRedraw()
        Dim after As Rectangle = ChildRectInParent(_hostedWindow)

        Dim outcome As MoveOutcome = MoveOutcomeClassifier.Classify(
            found:=True, apiSucceeded:=True, before:=before, after:=after)
        RhpLog($"{what}: cerut {MoveOutcomeClassifier.Describe(wanted)} — " &
               $"{MoveOutcomeClassifier.Label(outcome)} {MoveOutcomeClassifier.Describe(before)} -> " &
               MoveOutcomeClassifier.Describe(after))
        If after <> wanted Then
            RhpLog("  ATENȚIE: fereastra nu a ajuns la dreptunghiul cerut (Adobe a refuzat sau a limitat).")
        End If
        UpdateActionStates()
        RefreshStatusBlock()
    End Sub

    ' The hosted window's rectangle in pnlHost's CLIENT coordinates. GetWindowRect returns SCREEN
    ' coordinates; mixing the two is the one way this reads plausible numbers and means nothing, so
    ' the conversion happens once — now inside AdobeWindowHosting (slice 0024).
    Private Shared Function ChildRectInParent(hwnd As IntPtr) As Rectangle
        Return AdobeWindowHosting.RectInParent(hwnd)
    End Function

    ' ══ Ferestre copil — hide/show a probed child directly ══════════════════════
    Private Sub btnHideChild_Click(sender As Object, e As EventArgs) Handles btnHideChild.Click
        Try
            Dim item As AdobeWindowNode = SelectedChildAlive()
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
    Private Sub HideChild(item As AdobeWindowNode)
        AdobeWindowHosting.Hide(item.Hwnd)
        If Not _hiddenChildren.Contains(item.Hwnd) Then _hiddenChildren.Add(item.Hwnd)
        If Not String.IsNullOrEmpty(item.Text) AndAlso Not _hiddenChildTexts.Contains(item.Text) Then
            _hiddenChildTexts.Add(item.Text)
        End If
        RhpLog($"Ascuns copil: {item} (hwnd=0x{item.Hwnd.ToInt64():X})")
    End Sub

    Private Sub btnShowChild_Click(sender As Object, e As EventArgs) Handles btnShowChild.Click
        Try
            Dim item As AdobeWindowNode = SelectedChildAlive()
            If item Is Nothing Then Return
            AdobeWindowHosting.Show(item.Hwnd)
            _hiddenChildren.Remove(item.Hwnd)
            _hiddenChildTexts.Remove(item.Text)
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
                If AdobeWindowHosting.IsAlive(h) Then
                    AdobeWindowHosting.Show(h)
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
    Private Function SelectedChildAlive() As AdobeWindowNode
        Dim item As AdobeWindowNode = TryCast(lstChildren.SelectedItem, AdobeWindowNode)
        If item Is Nothing Then
            ShowStatus("Selectează întâi o fereastră din listă (rulează proba dacă lista e goală).")
            Return Nothing
        End If
        If Not AdobeWindowHosting.IsAlive(item.Hwnd) Then
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
                Dim matches As List(Of AdobeWindowNode) = _lastProbe.
                    Where(Function(i) String.Equals(i.Text, text, StringComparison.OrdinalIgnoreCase)).ToList()
                If matches.Count = 0 Then
                    outcomes.Add(HideOutcome.NotFound)
                    RhpLog($"  {HideOutcomeClassifier.Label(HideOutcome.NotFound)}: «{text}»")
                    Continue For
                End If
                For Each m As AdobeWindowNode In matches
                    If Not AdobeWindowHosting.IsAlive(m.Hwnd) Then Continue For
                    ' Classify BEFORE touching it — afterwards everything looks hidden, which is
                    ' precisely how a no-op used to be logged as a success.
                    Dim outcome As HideOutcome = HideOutcomeClassifier.Classify(
                        found:=True, visible:=AdobeWindowHosting.IsVisible(m.Hwnd), width:=m.Width, height:=m.Height)
                    outcomes.Add(outcome)
                    RhpLog($"  {HideOutcomeClassifier.Label(outcome)}: «{m.Text}» " &
                           $"{m.Width}x{m.Height} vis={If(AdobeWindowHosting.IsVisible(m.Hwnd), 1, 0)} (hwnd=0x{m.Hwnd.ToInt64():X})")
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
        AdobeWindowHosting.FocusWindow(_hostedWindow)
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
            ' The move section pre-sets the spinners exactly like the clip section does.
            If _scenario.Move IsNot Nothing Then
                numDx.Value = ClampToRange(numDx, _scenario.Move.EffectiveDx())
                numDy.Value = ClampToRange(numDy, _scenario.Move.EffectiveDy())
                numDw.Value = ClampToRange(numDw, _scenario.Move.EffectiveDw())
                numDh.Value = ClampToRange(numDh, _scenario.Move.EffectiveDh())
            End If
            ' Captura și desprinderea (felia 0024-03). O secțiune absentă lasă controalele exact cum
            ' erau — aceeași regulă ca peste tot: absent ≠ „pune pe implicit".
            If _scenario.Hosting IsNot Nothing Then
                Dim warning As String = _scenario.Hosting.DetachModeWarning()
                If Not String.IsNullOrEmpty(warning) Then RhpLog("ATENȚIE: " & warning)
                If Not String.IsNullOrWhiteSpace(_scenario.Hosting.DetachMode) Then
                    rdoDetachClose.Checked = _scenario.Hosting.WantsCloseWindow()
                    rdoDetachKill.Checked = Not rdoDetachClose.Checked
                End If
                If _scenario.Hosting.UseCreationHook.HasValue Then
                    chkCreationHook.Checked = _scenario.Hosting.UseCreationHook.Value
                End If
                If _scenario.Hosting.CaptureDelayMs.HasValue Then
                    numCaptureDelay.Value = ClampToRange(numCaptureDelay, _scenario.Hosting.CaptureDelayMs.Value)
                End If
                If _scenario.Hosting.CloseGraceMs.HasValue Then
                    numCloseGrace.Value = ClampToRange(numCloseGrace, _scenario.Hosting.CloseGraceMs.Value)
                End If
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
        ' …and the buttons must match what was just loaded: «Readu la zero» is enabled by a non-zero
        ' delta arriving from a FILE exactly as much as by one typed into the spinner.
        UpdateActionStates()
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

            Case HarnessScenarioSteps.ApplyMove
                ApplyHostedGeometry("Poziție (scenariu)")
                Return True

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

        ' The window placement, saved only when it would actually do something — an all-zero `move`
        ' section in a file is noise.
        Dim mv As New MoveConfig() With {.Dx = CInt(numDx.Value), .Dy = CInt(numDy.Value),
                                         .Dw = CInt(numDw.Value), .Dh = CInt(numDh.Value)}
        If Not mv.IsNoOp() Then s.Move = mv

        ' Captura și desprinderea se salvează ÎNTOTDEAUNA: fără ele, o stare salvată nu poate
        ' reproduce comparația A vs B, care e tot rostul feliei 0024-03.
        s.Hosting = New HostingConfig() With {
            .DetachMode = If(rdoDetachClose.Checked, HostingConfig.DetachClose, HostingConfig.DetachKill),
            .UseCreationHook = chkCreationHook.Checked,
            .CaptureDelayMs = CInt(numCaptureDelay.Value),
            .CloseGraceMs = CInt(numCloseGrace.Value)}

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
        steps.Add(HarnessScenarioSteps.Probe)
        If s.HideChildren IsNot Nothing Then steps.Add(HarnessScenarioSteps.HideChildren)
        If chkClip.Checked Then steps.Add(HarnessScenarioSteps.ApplyClip)
        If s.Move IsNot Nothing Then steps.Add(HarnessScenarioSteps.ApplyMove)
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
            sb.Append("  ·  Poziție: ").Append(
                If(HostedWindowGeometry.IsNeutral(CInt(numDx.Value), CInt(numDy.Value),
                                                  CInt(numDw.Value), CInt(numDh.Value)),
                   "neutră",
                   $"dx={CInt(numDx.Value)} dy={CInt(numDy.Value)} dw={CInt(numDw.Value)} dh={CInt(numDh.Value)}"))
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
        btnResetMove.Enabled = Not HostedWindowGeometry.IsNeutral(
            CInt(numDx.Value), CInt(numDy.Value), CInt(numDw.Value), CInt(numDh.Value))
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
    ' GONE IN SLICE 0024. Every declaration that used to live here (styles, SetParent, MoveWindow,
    ' SetWindowPos, RedrawWindow, GetWindow, MapWindowPoints, GetClassName/GetWindowText, the 32/64
    ' bit GetWindowLongPtr pair, the RECT struct and the EnumWindows delegate) is now in
    ' AdobeNativeMethods / AdobeWindowHosting in KBot.Controls, shared with ReaderHostPreview.
    ' The bench reaches them ONLY through AdobeWindowHosting — there is no second copy to drift.

End Class
