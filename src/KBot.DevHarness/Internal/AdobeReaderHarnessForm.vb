Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
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

    Private ReadOnly _log As Action(Of String)
    Private ReadOnly _adobePath As String

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

    Public Sub New(log As Action(Of String))
        _log = log
        InitializeComponent()
        _adobePath = ResolveAdobePath()
        If String.IsNullOrEmpty(_adobePath) Then
            SetControlsEnabled(False)
            ShowStatus("Adobe Reader/Acrobat nu a fost găsit pe această mașină.")
        Else
            ShowStatus("Adobe: " & _adobePath & Environment.NewLine & "Alege un PDF pentru a-l încorpora.")
        End If
        _loading = False
        UpdateCmdPreview()
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

    ' La închidere OMOARĂ Adobe (cerința operatorului): invalidează orice încorporare în curs,
    ' apoi distruge forțat procesul găzduit + cel pornit de noi.
    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        Try
            Interlocked.Increment(_generation)
            KillTracked()
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

    Private Sub LayoutHostedWindow()
        If _hostedWindow = IntPtr.Zero Then Return
        Dim w As Integer = pnlHost.ClientSize.Width
        Dim h As Integer = pnlHost.ClientSize.Height
        If w <= 0 OrElse h <= 0 Then Return
        MoveWindow(_hostedWindow, 0, 0, w, h, True)
    End Sub

    ' Forțează afișarea/redesenarea ferestrei reparentate (altfel rămâne nevăzută până la o
    ' schimbare de layout a formularului). Reaplică frame-ul, face un mic „nudge" de dimensiune
    ' și cere o repictare completă a copilului.
    Private Sub NudgeRedraw()
        If _hostedWindow = IntPtr.Zero Then Return
        Dim w As Integer = pnlHost.ClientSize.Width
        Dim h As Integer = pnlHost.ClientSize.Height
        If w <= 0 OrElse h <= 0 Then Return

        ' 1) Reaplică frame-ul după schimbarea stilurilor și fă fereastra vizibilă.
        SetWindowPos(_hostedWindow, IntPtr.Zero, 0, 0, w, h,
                     SWP_NOZORDER Or SWP_NOACTIVATE Or SWP_FRAMECHANGED Or SWP_SHOWWINDOW)
        ' 2) Mic „nudge" de dimensiune -> Adobe își reașează conținutul și pictează.
        MoveWindow(_hostedWindow, 0, 0, Math.Max(1, w - 1), h, True)
        MoveWindow(_hostedWindow, 0, 0, w, h, True)
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
    Private Sub KillTracked()
        Dim hostedPid As Integer = _hostedPid
        _hostedWindow = IntPtr.Zero
        _originalStyle = IntPtr.Zero
        _hostedPid = 0

        KillPid(hostedPid)

        Dim rp As Process = _readerProcess
        _readerProcess = Nothing
        SafeKill(rp)
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

    ' ── Stare / activare ────────────────────────────────────────────────────────
    Private Sub ShowStatus(message As String)
        lblStatus.Text = message
    End Sub

    Private Sub SetControlsEnabled(enabled As Boolean)
        btnBrowse.Enabled = enabled
        btnRelaunch.Enabled = enabled
        chkNewInstance.Enabled = enabled
        chkNoSplash.Enabled = enabled
        chkToolbar.Enabled = enabled
        chkNavpanes.Enabled = enabled
        chkStatusbar.Enabled = enabled
        chkMessages.Enabled = enabled
        chkScrollbar.Enabled = enabled
        chkPagemodeNone.Enabled = enabled
    End Sub

    ' ── Win32 interop ───────────────────────────────────────────────────────────
    Private Const GWL_STYLE As Integer = -16
    Private Const WS_CHILD As Long = &H40000000L
    Private Const WS_POPUP As Long = &H80000000L
    Private Const WS_CAPTION As Long = &HC00000L
    Private Const WS_THICKFRAME As Long = &H40000L
    Private Const WS_SYSMENU As Long = &H80000L
    Private Const WS_MINIMIZEBOX As Long = &H20000L
    Private Const WS_MAXIMIZEBOX As Long = &H10000L

    Private Const SWP_NOSIZE As UInteger = &H1UI
    Private Const SWP_NOMOVE As UInteger = &H2UI
    Private Const SWP_NOZORDER As UInteger = &H4UI
    Private Const SWP_NOACTIVATE As UInteger = &H10UI
    Private Const SWP_FRAMECHANGED As UInteger = &H20UI
    Private Const SWP_SHOWWINDOW As UInteger = &H40UI

    Private Const RDW_INVALIDATE As UInteger = &H1UI
    Private Const RDW_UPDATENOW As UInteger = &H100UI
    Private Const RDW_ALLCHILDREN As UInteger = &H80UI
    Private Const RDW_FRAME As UInteger = &H400UI

    Private Delegate Function EnumWindowsProc(hWnd As IntPtr, lParam As IntPtr) As Boolean

    <DllImport("user32.dll")>
    Private Shared Function EnumWindows(callback As EnumWindowsProc, extra As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function IsWindowVisible(hWnd As IntPtr) As Boolean
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
