Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.IO
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports Microsoft.Win32
Imports KBot.Common

''' <summary>
''' Banc de probă pentru încorporarea Adobe Reader/Acrobat DC într-un panou-gazdă, cu fiecare
''' switch de ascundere a chrome-ului (bare de instrumente/panouri) expus ca bifă. La orice
''' schimbare de bifă închidem instanța Adobe curentă și o redeschidem cu noul set de switch-uri
''' (cerința operatorului).
'''
''' Slice 0023 adds the four candidate levers against the right-hand Tools pane (RHP), which the
''' /A open parameters cannot touch (document chrome vs application chrome):
'''   §3.A a child-window probe (EnumChildWindows-style walk, depth-limited) whose output decides
'''        everything else — logged to Logs\test_adobe_rhp.log;
'''   §3.B geometry clipping (oversize + offset the hosted window so the clipped bands fall
'''        outside pnlHost) — live, no relaunch;
'''   §3.C direct ShowWindow(SW_HIDE) on a probed child HWND — live, no relaunch;
'''   §3.D experimental keyboard toggles (Shift+F4 / F4);
'''   §3.E HKCU AVGeneral preferences with a once-per-session snapshot and exact restore
'''        (absent restores to DELETION, never to 0);
'''   §3.F HKLM FeatureLockDown policies via generated .reg files imported by an elevated
'''        reg.exe (the harness itself is never elevated).
'''
''' Detalii care au mușcat deja tiparul (vezi și <see cref="ReaderHostPreview"/> din KBot.App):
'''   * Reader e practic mono-instanță -> lansăm cu «/n» (instanță nouă) ca fereastra să ne
'''     aparțină și să nu fie predată unei instanțe deja deschise cu alte documente. Fereastra
'''     o găsim cu EnumWindows (clasă Acrobat + titlu care conține numele fișierului), cu timeout.
'''   * După SetParent + curățarea stilurilor, fereastra rămâne NEVĂZUTĂ până la o schimbare de
'''     layout (redimensionare/mutare). De aceea forțăm explicit o redesenare (NudgeRedraw):
'''     SetWindowPos(FRAMECHANGED|SHOWWINDOW) + un mic „nudge" de dimensiune + RedrawWindow, o
'''     dată imediat și încă o dată după o scurtă întârziere (Adobe își termină layout-ul târziu).
'''   * La închiderea formularului (și la fiecare reîncorporare) OMORÂM forțat Adobe după PID —
'''     PID-ul ferestrei găzduite (proprietarul real, via GetWindowThreadProcessId) plus procesul
'''     pe care l-am pornit noi — ca să nu rămână un proces orfan.
''' </summary>
Public NotInheritable Class AdobeReaderHarnessForm

    ' Timeout la căutarea ferestrei Reader (ms) și pasul de polling.
    Private Const FIND_TIMEOUT_MS As Integer = 8000
    Private Const FIND_POLL_MS As Integer = 150
    ' Întârziere pentru a doua redesenare (Adobe își finalizează layout-ul după apariția ferestrei).
    Private Const REDRAW_DELAY_MS As Integer = 250
    ' §3.A probe depth limit and the right-edge tolerance of the RHP heuristic (px).
    Private Const PROBE_MAX_DEPTH As Integer = 4
    Private Const RHP_EDGE_TOLERANCE As Integer = 8
    ' Dedicated log for this slice (house rule: harness output goes to Logs\test_*.log).
    Private Const RHP_LOG_NAME As String = "test_adobe_rhp.log"

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

    ' §3.A/§3.C state: widest RHP candidate from the last probe, and the child handles we hid.
    Private _probeCandidateWidth As Integer = 0
    Private _probeCandidateClass As String = Nothing
    Private ReadOnly _hiddenChildren As New List(Of IntPtr)()
    ' §3.E/§3.F state for the status block: which HKCU values were applied this session, and
    ' whether the HKLM policy import succeeded this session.
    Private ReadOnly _userValuesApplied As New List(Of String)()
    Private _machinePolicyApplied As Boolean = False
    ' §3.F: generated .reg file paths (apply + revert), set by the last "Aplică (cere elevare)".
    Private _applyRegPath As String = Nothing
    Private _revertRegPath As String = Nothing
    ' Last one-line action message, shown under the status block (see RefreshStatusBlock).
    Private _lastMessage As String = ""

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

    ' La redimensionarea panoului, reașează fereastra găzduită pe tot panoul.
    Private Sub pnlHost_SizeChanged(sender As Object, e As EventArgs) Handles pnlHost.SizeChanged
        Try
            LayoutHostedWindow()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.pnlHost_SizeChanged", ex)
        End Try
    End Sub

    ' On close: invalidate any in-flight embed, force-kill Adobe (operator requirement), then
    ' restore the HKCU snapshot if the operator left "Restaurează la închidere" checked (§3.E).
    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        Try
            Interlocked.Increment(_generation)
            KillTracked()
            RestoreUserPrefsOnClose()
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
            Dim gen As Integer = Interlocked.Increment(_generation)
            KillTracked()   ' „close the current adobe if any"

            If String.IsNullOrEmpty(_adobePath) OrElse String.IsNullOrEmpty(_pdfPath) Then Return

            Dim args As String = BuildArguments(_pdfPath)
            ShowStatus("Pornesc Adobe…")
            Dim proc As Process = StartReader(args)
            If proc Is Nothing Then Return

            Dim baseName As String = Path.GetFileNameWithoutExtension(_pdfPath)
            Dim hwnd As IntPtr = Await Task.Run(Function() FindReaderWindow(baseName))

            ' O relansare mai nouă a preluat controlul cât timp căutam: curăț procesul propriu.
            If gen <> _generation Then
                SafeKill(proc)
                Return
            End If

            _readerProcess = proc
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
            ShowStatus("Încorporat. PID fereastră: " & ownerPid.ToString() & Environment.NewLine & args)
            _log("Adobe încorporat — " & Path.GetFileName(_adobePath) & " " & args)
            UpdateActionStates()

            ' A doua redesenare, după ce Adobe își termină layout-ul (altfel rămâne nevăzut).
            Await Task.Delay(REDRAW_DELAY_MS)
            If gen = _generation Then NudgeRedraw()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.RelaunchAsync", ex)
            ShowStatus("Eroare la încorporare: " & ex.Message)
        End Try
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
    ' Rulează pe firul UI (după await). Transitiv acoperit de Try/Catch-ul din RelaunchAsync.
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

    ' §3.B: the bounds of the hosted window inside pnlHost. Without clipping it fills the panel;
    ' with clipping it is oversized and offset so the clipped bands (top toolbar strip / right
    ' pane strip) fall OUTSIDE the visible client area of pnlHost.
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
    ' schimbare de layout a formularului). Reaplică frame-ul, face un mic „nudge" de dimensiune
    ' și cere o repictare completă a copilului. (Clip-aware since slice 0023: uses HostedBounds.)
    Private Sub NudgeRedraw()
        If _hostedWindow = IntPtr.Zero Then Return
        Dim b As Rectangle = HostedBounds()
        If b.Width <= 0 OrElse b.Height <= 0 Then Return

        ' 1) Reaplică frame-ul după schimbarea stilurilor și fă fereastra vizibilă.
        SetWindowPos(_hostedWindow, IntPtr.Zero, b.X, b.Y, b.Width, b.Height,
                     SWP_NOZORDER Or SWP_NOACTIVATE Or SWP_FRAMECHANGED Or SWP_SHOWWINDOW)
        ' 2) Mic „nudge" de dimensiune -> Adobe își reașează conținutul și pictează.
        MoveWindow(_hostedWindow, b.X, b.Y, Math.Max(1, b.Width - 1), b.Height, True)
        MoveWindow(_hostedWindow, b.X, b.Y, b.Width, b.Height, True)
        ' 3) Repictare completă a copilului + a panoului.
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

    ' Parametrii de deschidere (/A) care ascund chrome-ul, în ordinea din panou.
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

    Private Sub UpdateCmdPreview()
        Dim exe As String = If(String.IsNullOrEmpty(_adobePath), "<Adobe negăsit>", Path.GetFileName(_adobePath))
        Dim pdf As String = If(String.IsNullOrEmpty(_pdfPath), "document.pdf", _pdfPath)
        lblCmd.Text = exe & " " & BuildArguments(pdf)
    End Sub

    ' ── Curățare Adobe ──────────────────────────────────────────────────────────
    ' Distruge forțat Adobe găzduit: întâi procesul PROPRIETAR al ferestrei (via PID), apoi
    ' procesul pornit de noi. Best-effort prin construcție (fiecare pas înghite excepțiile).
    ' Since 0023 it also drops the probe/child state — those HWNDs die with the process.
    Private Sub KillTracked()
        Dim hostedPid As Integer = _hostedPid
        _hostedWindow = IntPtr.Zero
        _originalStyle = IntPtr.Zero
        _hostedPid = 0

        KillPid(hostedPid)

        Dim rp As Process = _readerProcess
        _readerProcess = Nothing
        SafeKill(rp)

        ' Probe results and hidden-child handles belong to the dead process.
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
    ' Fereastră de nivel superior a Reader-ului al cărei titlu conține numele fișierului, cu
    ' timeout mărginit. Zero dacă nu apare. Transitiv acoperit de Try/Catch-ul apelantului.
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
    ' Preferă App Paths (Acrobat.exe = Acrobat/Reader DC unificat, apoi AcroRd32.exe = Reader
    ' clasic), apoi cade pe căile uzuale de instalare.
    ' NOTE (0023): AdobeUtils.GetAdobeReaderPath (KBot.Xfa) resolves the .pdf HANDLER, but it is
    ' Private and KBot.Xfa is not referenced here — reusing it would mean touching KBot.Xfa,
    ' which is out of scope for this pass. Recorded in the worklog.
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

    ' ══ §3.A Diagnostic — child window probe ════════════════════════════════════
    Private Sub btnProbe_Click(sender As Object, e As EventArgs) Handles btnProbe.Click
        Try
            ProbeChildren()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnProbe_Click", ex)
        End Try
    End Sub

    ' Walks the hosted window's descendants (GW_CHILD/GW_HWNDNEXT recursion, depth-limited),
    ' logging one line per node and filling lstChildren. The RHP candidate is a HEURISTIC:
    ' visible, flush against the host's right edge (± tolerance) and narrower than half the
    ' host width; the widest such child wins and feeds "Măsoară din probă" (§3.B).
    Private Sub ProbeChildren()
        If _hostedWindow = IntPtr.Zero Then
            ShowStatus("Nicio fereastră găzduită — încorporează întâi un PDF.")
            Return
        End If
        lstChildren.Items.Clear()
        _probeCandidateWidth = 0
        _probeCandidateClass = Nothing

        Dim hostW As Integer = pnlHost.ClientSize.Width
        Dim total As Integer = 0
        RhpLog($"── Probă ferestre copil (hwnd gazdă 0x{_hostedWindow.ToInt64():X}, adâncime max {PROBE_MAX_DEPTH}; " &
               "EURISTIC candidat RHP = vizibil, lipit de marginea dreaptă ±" & RHP_EDGE_TOLERANCE.ToString() &
               "px, lățime < 1/2 gazdă) ──")
        WalkChildren(_hostedWindow, 1, hostW, total)

        Dim summary As String
        If _probeCandidateWidth > 0 Then
            summary = $"{total} ferestre copil; candidat RHP (EURISTIC): {_probeCandidateClass}, lățime {_probeCandidateWidth}px."
        Else
            summary = $"{total} ferestre copil; niciun candidat RHP după euristic."
        End If
        RhpLog(summary)
        ShowStatus(summary)
        UpdateActionStates()
    End Sub

    Private Sub WalkChildren(parent As IntPtr, depth As Integer, hostW As Integer, ByRef total As Integer)
        Dim child As IntPtr = GetWindow(parent, GW_CHILD)
        While child <> IntPtr.Zero
            total += 1
            Dim cls As String = GetClass(child)
            Dim txt As String = GetTitle(child)
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
            lstChildren.Items.Add(New ChildWindowItem(child, cls, w, h))

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

    ' A probed child in lstChildren: display class + size, carry the HWND.
    Private NotInheritable Class ChildWindowItem
        Public ReadOnly Hwnd As IntPtr
        Public ReadOnly ClassName As String
        Public ReadOnly W As Integer
        Public ReadOnly H As Integer

        Public Sub New(hwnd As IntPtr, className As String, w As Integer, h As Integer)
            Me.Hwnd = hwnd
            Me.ClassName = className
            Me.W = w
            Me.H = h
        End Sub

        Public Overrides Function ToString() As String
            Return $"{ClassName}  ({W}x{H})"
        End Function
    End Class

    ' ══ §3.B Decupare — geometry clipping (live, no relaunch/kill) ══════════════
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
            ' Value/checked changes already relaid out via ClipSettingsChanged; nudge once more
            ' explicitly in case neither actually changed.
            LayoutHostedWindow()
            NudgeRedraw()
            RhpLog($"Decupare setată din probă: dreapta={_probeCandidateWidth}px ({_probeCandidateClass}).")
            RefreshStatusBlock()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnClipAuto_Click", ex)
        End Try
    End Sub

    ' ══ §3.C Ferestre copil — hide/show a probed child directly ═════════════════
    Private Sub btnHideChild_Click(sender As Object, e As EventArgs) Handles btnHideChild.Click
        Try
            Dim item As ChildWindowItem = SelectedChildAlive()
            If item Is Nothing Then Return
            ShowWindow(item.Hwnd, SW_HIDE)
            If Not _hiddenChildren.Contains(item.Hwnd) Then _hiddenChildren.Add(item.Hwnd)
            RhpLog($"Ascuns copil: {item} (hwnd=0x{item.Hwnd.ToInt64():X})")
            NudgeRedraw()
            UpdateActionStates()
            RefreshStatusBlock()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnHideChild_Click", ex)
        End Try
    End Sub

    Private Sub btnShowChild_Click(sender As Object, e As EventArgs) Handles btnShowChild.Click
        Try
            Dim item As ChildWindowItem = SelectedChildAlive()
            If item Is Nothing Then Return
            ShowWindow(item.Hwnd, SW_SHOW)
            _hiddenChildren.Remove(item.Hwnd)
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
            NudgeRedraw()
            UpdateActionStates()
            RefreshStatusBlock()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnShowAllChildren_Click", ex)
        End Try
    End Sub

    ' Selected list item whose HWND is still alive; stale handles are dropped with a log line
    ' (IsWindow guard — Adobe destroys and recreates panels).
    Private Function SelectedChildAlive() As ChildWindowItem
        Dim item As ChildWindowItem = TryCast(lstChildren.SelectedItem, ChildWindowItem)
        If item Is Nothing Then
            ShowStatus("Selectează întâi o fereastră din listă (rulează proba dacă lista e goală).")
            Return Nothing
        End If
        If Not IsWindow(item.Hwnd) Then
            RhpLog($"Handle mort (fereastra a fost distrusă între timp): {item} — eliminat din listă.")
            lstChildren.Items.Remove(item)
            _hiddenChildren.Remove(item.Hwnd)
            UpdateActionStates()
            Return Nothing
        End If
        Return item
    End Function

    ' ══ §3.D Scurtături — keyboard toggles (experimental) ═══════════════════════
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
    ' child (documented in ReaderHostPreview). "Nothing happened" is a valid result to record —
    ' no retries, no workarounds (plan §3.D).
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

    ' ══ §3.E Preferințe Adobe (utilizator, HKCU) ════════════════════════════════
    Private Sub cboHive_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboHive.SelectedIndexChanged
        Try
            If _loading Then Return
            RefreshStatusBlock()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.cboHive_SelectedIndexChanged", ex)
        End Try
    End Sub

    ' The AVGeneral hive to write: explicit combo choice, or the pure resolver on "auto".
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

    ' One ticked HKCU value to write: registry name, kind, value, plus a short status label.
    Private NotInheritable Class UserPrefWrite
        Public ReadOnly Name As String
        Public ReadOnly Kind As RegistryValueKind
        Public ReadOnly Value As Object
        Public ReadOnly Label As String

        Public Sub New(name As String, kind As RegistryValueKind, value As Object, label As String)
            Me.Name = name
            Me.Kind = kind
            Me.Value = value
            Me.Label = label
        End Sub
    End Class

    Private Function CollectTickedUserPrefs() As List(Of UserPrefWrite)
        Dim prefs As New List(Of UserPrefWrite)()
        If chkExpandRhp.Checked Then prefs.Add(New UserPrefWrite(
            AdobeRegistryConstants.ValExpandRhp, RegistryValueKind.DWord, 0, "bExpandRHPInViewer=0"))
        If chkRhpSticky.Checked Then prefs.Add(New UserPrefWrite(
            AdobeRegistryConstants.ValRhpSticky, RegistryValueKind.DWord, 1, "bRHPSticky=1"))
        If chkRhpCollapsed.Checked Then prefs.Add(New UserPrefWrite(
            AdobeRegistryConstants.ValRhpViewMode, RegistryValueKind.String,
            AdobeRegistryConstants.RhpViewModeCollapsed, "aDefaultRHPViewMode_L=Collapsed"))
        If chkClassicViewer.Checked Then prefs.Add(New UserPrefWrite(
            AdobeRegistryConstants.ValEnableAv2, RegistryValueKind.DWord, 0, "bEnableAv2=0"))
        Return prefs
    End Function

    ' Plan §3.E order: (1) snapshot ticked values ONCE per session (Capture is idempotent, so a
    ' second Apply cannot overwrite the true originals); (2) kill Adobe — ours by PID plus, with
    ' CONSENT ONLY, any foreign instance (Adobe rewrites its prefs on exit, the write is
    ' worthless while it runs); (3) write, logging old → new; (4) relaunch.
    Private Async Sub btnApplyUser_Click(sender As Object, e As EventArgs) Handles btnApplyUser.Click
        Try
            Dim prefs As List(Of UserPrefWrite) = CollectTickedUserPrefs()
            If prefs.Count = 0 Then
                ShowStatus("Nicio valoare HKCU bifată — nimic de aplicat.")
                Return
            End If
            Dim hive As String = CurrentAvGeneralPath()

            For Each p As UserPrefWrite In prefs
                _userSnapshot.Capture(hive, p.Name)
            Next

            If Not KillAdobeForRegistryWrite() Then Return

            For Each p As UserPrefWrite In prefs
                Dim oldSnap As RegistryValueSnapshot = _regAccess.Read(hive, p.Name)
                _regAccess.Write(hive, p.Name, p.Kind, p.Value)
                RhpLog($"HKCU scris: {oldSnap} -> {p.Value} ({p.Kind})")
                If Not _userValuesApplied.Contains(p.Label) Then _userValuesApplied.Add(p.Label)
            Next

            If Not String.IsNullOrEmpty(_pdfPath) Then Await RelaunchAsync()
            ShowStatus("Valori HKCU aplicate; Adobe repornit cu setul curent de switch-uri.")
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnApplyUser_Click", ex)
            ShowStatus("Eroare la aplicarea valorilor HKCU: " & ex.Message)
        End Try
    End Sub

    Private Async Sub btnRestoreUser_Click(sender As Object, e As EventArgs) Handles btnRestoreUser.Click
        Try
            If _userSnapshot.Count = 0 Then
                ShowStatus("Nimic de restaurat — nu s-a aplicat nicio valoare în această sesiune.")
                Return
            End If
            If Not KillAdobeForRegistryWrite() Then Return
            For Each s As RegistryValueSnapshot In _userSnapshot.Snapshots()
                RhpLog("HKCU restaurez la original: " & s.ToString())
            Next
            _userSnapshot.RestoreAll()
            _userValuesApplied.Clear()
            If Not String.IsNullOrEmpty(_pdfPath) Then Await RelaunchAsync()
            ShowStatus("Valorile HKCU au fost restaurate la original.")
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnRestoreUser_Click", ex)
            ShowStatus("Eroare la restaurare: " & ex.Message)
        End Try
    End Sub

    ' Kills OUR hosted instance by PID (existing path), then asks — in Romanian, never silently —
    ' before killing foreign Adobe processes. Returns False when the operator declines (the
    ' registry write would be overwritten by Adobe on exit, so the operation aborts).
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

    ' On-close restore (§3.E): bounded by construction — our Adobe is already killed, and the
    ' restore is a handful of registry writes. DEVIATION from a literal reading of the plan:
    ' foreign Adobe processes are NOT killed here (no consent dialog on close — never silently);
    ' if any run, their exit may overwrite the restore, which is logged. On failure the operator
    ' is told (lblStatus + MessageBox — the form is closing, the label alone would never be seen)
    ' with the exact keys named for manual cleanup.
    Private Sub RestoreUserPrefsOnClose()
        If Not chkRestoreOnClose.Checked OrElse _userValuesApplied.Count = 0 Then Return
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

    ' ══ §3.F Politici Adobe (mașină, HKLM) — elevated reg.exe import ════════════
    Private Sub cboProduct_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboProduct.SelectedIndexChanged
        Try
            If _loading Then Return
            RefreshStatusBlock()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.cboProduct_SelectedIndexChanged", ex)
        End Try
    End Sub

    ' The FeatureLockDown <product>: explicit combo choice, or the pure resolver on "auto".
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

    ' Generates the apply/revert .reg pair into AppDir\Logs\ (revert from PRE-apply reads:
    ' absent -> deletion line, present dword -> original value), then imports the apply file via
    ' elevated reg.exe. Reading HKLM for the snapshot needs no elevation; only writing does.
    Private Async Sub btnApplyMachine_Click(sender As Object, e As EventArgs) Handles btnApplyMachine.Click
        Try
            If Not chkSuppressUpsell.Checked AndAlso Not chkDisableServices.Checked Then
                ShowStatus("Nicio politică HKLM bifată — nimic de aplicat.")
                Return
            End If
            Dim product As String = CurrentPolicyProduct()
            Dim fld As String = AdobeRegistryConstants.FeatureLockDownPath(product)
            Dim cs As String = AdobeRegistryConstants.CServicesPath(product)

            Dim apply As New RegFileBuilder()
            Dim revert As New RegFileBuilder()
            If chkSuppressUpsell.Checked Then
                AddPolicyValue(apply, revert, fld, AdobeRegistryConstants.ValSuppressUpsell)
            End If
            If chkDisableServices.Checked Then
                AddPolicyValue(apply, revert, cs, AdobeRegistryConstants.ValToggleServices)
            End If

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
                ShowStatus("Politica HKLM aplicată. Repornește Adobe (Reîncorporează) pentru efect.")
            End If
            RefreshStatusBlock()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnApplyMachine_Click", ex)
            ShowStatus("Eroare la aplicarea politicii HKLM: " & ex.Message)
        End Try
    End Sub

    Private Async Sub btnRevertMachine_Click(sender As Object, e As EventArgs) Handles btnRevertMachine.Click
        Try
            If String.IsNullOrEmpty(_revertRegPath) OrElse Not File.Exists(_revertRegPath) Then
                ShowStatus("Nu există fișier de revocare — aplică întâi politica (el se generează atunci).")
                Return
            End If
            Dim ok As Boolean = Await RunRegImportAsync(_revertRegPath)
            If ok Then
                _machinePolicyApplied = False
                ShowStatus("Politica HKLM a fost revocată (valorile originale reimportate).")
            End If
            RefreshStatusBlock()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHarnessForm.btnRevertMachine_Click", ex)
            ShowStatus("Eroare la revocarea politicii HKLM: " & ex.Message)
        End Try
    End Sub

    ' Adds one policy value to the apply builder and its pre-apply original to the revert
    ' builder. Absent -> deletion line; present DWORD -> original value; present with another
    ' kind -> string when possible, otherwise logged as needing manual revert (never guessed).
    Private Sub AddPolicyValue(apply As RegFileBuilder, revert As RegFileBuilder,
                               sectionPath As String, name As String)
        apply.AddDword(sectionPath, name, 1UI)
        Dim snap As RegistryValueSnapshot = _regAccess.Read(sectionPath, name)
        RhpLog("HKLM citit (pentru revocare): " & snap.ToString())
        If snap.Presence = RegPresence.Absent Then
            revert.DeleteValue(sectionPath, name)
        ElseIf snap.Kind = RegistryValueKind.DWord Then
            revert.AddDword(sectionPath, name, DwordToUInt(snap.Value))
        ElseIf snap.Kind = RegistryValueKind.String Then
            revert.AddString(sectionPath, name, CStr(snap.Value))
        Else
            ' Preserve honesty over completeness: an exotic original kind is not representable
            ' by this builder — record it instead of guessing.
            RhpLog($"ATENȚIE: {sectionPath}\{name} are tipul {snap.Kind}, nereprezentabil în .reg-ul " &
                   "de revocare — revocarea acestei valori va trebui făcută manual.")
        End If
    End Sub

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

    ' ── Stare / activare ────────────────────────────────────────────────────────
    ' §3.G: lblStatus always shows the effective state of all four levers in one block, with the
    ' last action message underneath. Never throws (called from ctor and catch paths).
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
            sb.AppendLine("Hive HKCU: " & hive)
            sb.AppendLine("Decupare: " & If(chkClip.Checked,
                $"activă (dreapta={CInt(numClipRight.Value)}px, sus={CInt(numClipTop.Value)}px)", "inactivă"))
            sb.AppendLine("Ferestre copil ascunse: " & _hiddenChildren.Count.ToString())
            sb.AppendLine("HKCU aplicat: " & If(_userValuesApplied.Count > 0,
                String.Join(", ", _userValuesApplied), "—"))
            sb.AppendLine("Politică HKLM aplicată în sesiune: " & If(_machinePolicyApplied, "da", "nu"))
            If _lastMessage.Length > 0 Then
                sb.AppendLine("―")
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

    ' Everything interactive lives in flowLeft — disable it wholesale when Adobe is absent
    ' (labels included, harmless). Pass/Fail (pnlButtons) stay usable.
    Private Sub SetControlsEnabled(enabled As Boolean)
        For Each c As Control In flowLeft.Controls
            c.Enabled = enabled
        Next
    End Sub

    ' Hosted-window-dependent buttons (§3.A/§3.C/§3.D) + the probe-fed auto-measure (§3.B).
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
    End Sub

    ' Timestamped tee: context log + AppDir\Logs\test_adobe_rhp.log (appended, never
    ' overwritten — the log is a deliverable of this slice). Failures propagate to the wrapped
    ' handler boundary (transitive coverage).
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
