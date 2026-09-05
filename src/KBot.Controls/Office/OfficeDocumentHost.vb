Option Strict On
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms

''' <summary>Which Office program a document has to be opened with.</summary>
Public Enum OfficeDocumentKind
    ''' <summary>.xls / .xlsx / .xlsm / .xlsb / .csv</summary>
    Excel = 0
    ''' <summary>.doc / .docx / .docm / .rtf</summary>
    Word = 1
End Enum

''' <summary>
''' The two ways Excel's ribbon can be taken down. Both are kept, because they fail differently and
''' neither is the better one on every machine.
''' </summary>
Public Enum ExcelRibbonMode
    ''' <summary>Ask Excel to hide the ribbon itself, through the Excel 4 macro
    ''' <c>SHOW.TOOLBAR("Ribbon", False)</c>. Excel then re-lays its desk out, so the sheet starts at
    ''' the top of the frame with nothing left to correct. The cost is that it goes through the macro
    ''' engine, which is a policy setting: an Excel that refuses Excel 4 macros answers with a COM
    ''' error and keeps its ribbon. This is what has run so far.</summary>
    Excel4Macro = 0
    ''' <summary>Hide the ribbon's WINDOW, the way Word's is hidden — no macro, no application
    ''' setting, just <c>ShowWindow(SW_HIDE)</c>. Nothing is asked of Excel, so there is nothing for
    ''' it to refuse. In exchange Excel does not re-lay its desk out, and the band the hidden window
    ''' leaves is closed by <see cref="OfficeDocumentHost"/> placing <c>XLDESK</c> against the
    ''' frame's client area — which it does anyway.
    ''' <para>The window is Excel's own, not one of Word's docks — class <c>EXCEL2</c>, no title —
    ''' and it is found by shape rather than by name, since neither name is a promise Office makes.
    ''' See <see cref="OfficeDocumentHost.FindRibbonBand"/>.</para></summary>
    HideDockWindow = 1
End Enum

''' <summary>What one <see cref="OfficeDocumentHost.ShowDocument"/> ended in. The message is
''' operator-visible, so it is Romanian and it is never empty on a failure.</summary>
Public NotInheritable Class OfficeHostResult

    Public ReadOnly Property Succeeded As Boolean
    Public ReadOnly Property Message As String
    ''' <summary>How long the whole open-and-embed took, for the working log.</summary>
    Public ReadOnly Property ElapsedMs As Integer

    Public Sub New(succeeded As Boolean, message As String, elapsedMs As Integer)
        Me.Succeeded = succeeded
        Me.Message = If(message, String.Empty)
        Me.ElapsedMs = elapsedMs
    End Sub

    Public Shared Function Ok(elapsedMs As Integer) As OfficeHostResult
        Return New OfficeHostResult(True, String.Empty, elapsedMs)
    End Function

    Public Shared Function Fail(message As String) As OfficeHostResult
        Return New OfficeHostResult(False, message, 0)
    End Function

End Class

''' <summary>
''' Hosts an Excel or Word document inside a WinForms panel, showing THE PAGE and not the program:
''' the ribbon, the formula bar and the status bar are taken down before the window is reparented.
''' This is the port of the technique proved in <c>Surse\Prelucrare-Excel-Razvan</c>.
'''
''' <para><b>A PRIVATE instance, every time.</b> The server is created through its ProgID, which
''' gives a fresh <c>EXCEL.EXE</c> / <c>WINWORD.EXE</c> rather than attaching to whatever the
''' operator has open. Stripping the chrome off the operator's own Word — or closing it on teardown —
''' would be a spectacular way to lose somebody's unsaved work.</para>
'''
''' <para><b>The chrome is put back before we quit.</b> <c>DisplayFormulaBar</c> and
''' <c>DisplayStatusBar</c> are APPLICATION settings that Office persists to the user profile, so an
''' instance that quits with them off can leave the operator's next Excel without a formula bar.
''' Every value this class writes is recorded first and replayed on the way out.</para>
'''
''' <para><b>Excel's ribbon has two methods, and the caller picks.</b> <see cref="ExcelRibbon"/>
''' chooses between asking Excel to hide it (the Excel 4 macro, which is what has run so far) and
''' hiding the ribbon's WINDOW the way Word's is hidden. The second asks Excel for nothing at all, so
''' there is nothing it can refuse and nothing written to the user profile; see
''' <see cref="ExcelRibbonMode"/> for what each one costs.</para>
'''
''' <para><b>The window mechanics are not re-implemented here.</b> Reparenting, the style mask,
''' placing and the redraw nudge all go through <see cref="AdobeWindowHosting"/> — one
''' implementation, however Adobe-flavoured its name is. Two copies of that code existed once in
''' this solution and had already diverged; the second copy is not coming back for Office.</para>
'''
''' <para><b>Teardown is deliberate, not hopeful.</b> The hosted window is a child of a panel that
''' WinForms will destroy; an Office process whose window vanishes underneath it stays alive with no
''' way to reach it. So <see cref="Detach"/> closes the document, quits the application, releases
''' every COM reference and then — if the process is still there — kills it.</para>
''' </summary>
Public NotInheritable Class OfficeDocumentHost
    Implements IDisposable

    ''' <summary>How long to wait for the document window chain to appear, and the polling step.</summary>
    Private Const EmbedTimeoutMs As Integer = 8000
    Private Const PollStepMs As Integer = 100
    ''' <summary>How long a quit is given before the process is killed.</summary>
    Private Const QuitGraceMs As Integer = 3000
    ''' <summary>How often the hosted document is told its application is still the active one.
    ''' See <see cref="PulseActivation"/>.</summary>
    Private Const ActivationPulseMs As Integer = 300

    ''' <summary>Excel's window chain, outermost first. The main window is the one hosted; the
    ''' inner two only have to EXIST, which is how we know the workbook is really laid out.</summary>
    Private Shared ReadOnly ExcelChain As String() = {"XLDESK", "EXCEL7"}
    ''' <summary>Word's chain under <c>OpusApp</c>.</summary>
    Private Shared ReadOnly WordChain As String() = {"_WwF", "_WwB", "_WwG"}

    ''' <summary>The class every WORD command-bar dock shares. There are four of them under
    ''' <c>OpusApp</c> — top, left, right and bottom — so the class on its own is not enough to pick
    ''' one out. Excel does NOT use this class for its ribbon: see <see cref="FindRibbonBand"/>.</summary>
    Private Const WordDockClass As String = "MsoCommandBarDock"
    ''' <summary>The title of the Word dock that holds the ribbon. Not operator-visible: it is the
    ''' name Word gives its own furniture.</summary>
    Private Const WordTopDockText As String = "MsoDockTop"

    ''' <summary>The class Excel gives the band that carries the ribbon — untitled, and NOT one of
    ''' Word's docks. Excel hands the same class to several other children of the frame, so this
    ''' narrows the search and does not settle it: see <see cref="FindRibbonBand"/>.</summary>
    Private Const ExcelRibbonClass As String = "EXCEL2"

    Private ReadOnly _surface As Control
    Private ReadOnly _log As Action(Of String)

    Private _app As Object
    Private _doc As Object
    Private _hosted As IntPtr = IntPtr.Zero
    ''' <summary>Which program is open now. Read on teardown, where Excel-only calls must not
    ''' be sent to Word.</summary>
    Private _kind As OfficeDocumentKind
    Private _hostedPid As Integer
    ''' <summary>The height of the dead band the hidden ribbon dock leaves at the top of the hosted
    ''' frame. Recomputed on every <see cref="Relayout"/>; always zero for Excel.</summary>
    Private _topInset As Integer
    ''' <summary>The form the surface sits on, while a document is hosted. See <see cref="HookHostForm"/>.</summary>
    Private _hostForm As Form
    ''' <summary>True once <see cref="FillInnerChain"/> has reported what it corrected, so it says
    ''' it once per document rather than once per resize.</summary>
    Private _fillLogged As Boolean
    ''' <summary>Keeps the hosted document accepting mouse buttons. See <see cref="PulseActivation"/>.</summary>
    Private ReadOnly _pulse As New System.Windows.Forms.Timer()
    ''' <summary>What the hosted application was last told about being active, so the "no longer
    ''' active" message is sent once and not three times a second.</summary>
    Private _toldActive As Boolean
    ''' <summary>True once <c>SHOW.TOOLBAR(…, False)</c> has actually gone out, so teardown only puts
    ''' back a ribbon this class really took down. See <see cref="RestoreChrome"/>.</summary>
    Private _ribbonMacroSent As Boolean
    ''' <summary>True once a ribbon band has actually been hidden for the document on screen, so the
    ''' second attempt in <see cref="ShowDocument"/> is a RETRY and not a repeat. Without it a
    ''' document that was dealt with on the first attempt writes its «not found» explanation to the
    ''' log a second time, from a frame whose desk has since been stretched over the gap — a line
    ''' that reads like a failure and is nothing of the kind.</summary>
    Private _ribbonBandHidden As Boolean

    ''' <summary>
    ''' Which of the two Excel ribbon methods to use. Read when the workbook is opened, so it has to
    ''' be set before <see cref="ShowDocument"/> — changing it afterwards does nothing to the document
    ''' already on screen. Ignored for Word, whose ribbon has only ever had the one method.
    ''' </summary>
    Public Property ExcelRibbon As ExcelRibbonMode = ExcelRibbonMode.Excel4Macro

    ''' <summary>Every application setting this class overwrote, with the value it had before.
    ''' Replayed in reverse on teardown — see the class remarks.</summary>
    Private ReadOnly _chromeRestore As New List(Of ChromeSetting)()

    Public Sub New(surface As Control, log As Action(Of String))
        If surface Is Nothing Then Throw New ArgumentNullException(NameOf(surface))
        _surface = surface
        _log = If(log, New Action(Of String)(Sub(s) Trace.WriteLine(s)))
        _pulse.Interval = ActivationPulseMs
        AddHandler _pulse.Tick, AddressOf PulseActivation
    End Sub

    ''' <summary>True while a document window is embedded in the surface.</summary>
    Public ReadOnly Property IsHosting As Boolean
        Get
            Return _hosted <> IntPtr.Zero AndAlso AdobeWindowHosting.IsAlive(_hosted)
        End Get
    End Property

    ' ══════════════════════════════════════════════════════════════════════════
    ' Showing
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Opens <paramref name="filePath"/> read-only in a private Office instance and embeds its window
    ''' in the surface. Never throws: every failure comes back as a Romanian sentence the caller can
    ''' put on screen.
    ''' </summary>
    Public Function ShowDocument(filePath As String, kind As OfficeDocumentKind) As OfficeHostResult
        Dim started As DateTime = DateTime.UtcNow
        Try
            Detach()

            If String.IsNullOrWhiteSpace(filePath) OrElse Not File.Exists(filePath) Then
                Return OfficeHostResult.Fail("Fișierul nu mai există pe disc.")
            End If

            _kind = kind
            Dim progId As String = If(kind = OfficeDocumentKind.Excel, "Excel.Application", "Word.Application")
            _app = OfficeLateBound.CreateFromProgId(progId)
            If _app Is Nothing Then
                _log($"ProgID {progId} is not registered on this machine.")
                Return OfficeHostResult.Fail(If(kind = OfficeDocumentKind.Excel,
                                                "Microsoft Excel nu este instalat pe acest calculator, așa că fișierul nu poate fi afișat aici. Îl poți salva pe disc.",
                                                "Microsoft Word nu este instalat pe acest calculator, așa că fișierul nu poate fi afișat aici. Îl poți salva pe disc."))
            End If

            OfficeLateBound.TrySetProp(_app, "Visible", False)
            OfficeLateBound.TrySetProp(_app, "DisplayAlerts", False)
            OfficeLateBound.TrySetProp(_app, "ScreenUpdating", False)

            If kind = OfficeDocumentKind.Excel Then
                OpenExcel(filePath)
            Else
                OpenWord(filePath)
            End If

            ' THE ORDER BELOW IS THE PROVEN ONE (Surse\Prelucrare-Excel-Razvan): show first, find the
            ' window, reparent last. Reparenting a still-hidden frame looks tidier on paper and is
            ' not what was measured to work, so it is not what runs here.

            ' The flicker that order would cause is dealt with by parking the frame off-screen
            ' first, so what appears on the monitor is never the standalone program, only the pane
            ' it ends up in. This is done with MoveWindow and NOT with Excel's own Left/Top: those
            ' throw on a hidden application ("Property Left could not be written" was the whole of
            ' what the first version achieved), while the window handle is valid long before the
            ' window is shown.
            ParkOffScreen(ResolveMainWindow(kind, filePath), _surface.ClientSize)

            OfficeLateBound.TrySetProp(_app, "Visible", True)
            OfficeLateBound.TrySetProp(_app, "ScreenUpdating", True)
            ' Word only reliably takes its chrome down once it is showing; Excel accepts it either
            ' way. Cheap enough to repeat for both.
            StripChromeAfterShow(kind)

            Dim mainWindow As IntPtr = ResolveMainWindow(kind, filePath)
            If mainWindow = IntPtr.Zero Then
                _log("The Office main window could not be found.")
                Detach()
                Return OfficeHostResult.Fail("Fereastra documentului nu a putut fi găsită. Detalii în jurnalul de lucru.")
            End If

            ' The chain is a readiness probe, nothing is done with the handles: while the inner
            ' document window does not exist yet, the reparented frame is a grey rectangle.
            Dim chain As String() = If(kind = OfficeDocumentKind.Excel, ExcelChain, WordChain)
            If Not WaitForChain(mainWindow, chain) Then
                _log("The inner document window never appeared; hosting the frame anyway.")
            End If

            _hosted = mainWindow
            _hostedPid = AdobeWindowHosting.OwnerPid(mainWindow)
            Dim originalStyle As IntPtr = AdobeWindowHosting.AttachAsChild(mainWindow, _surface.Handle)
            _log($"Hosted window 0x{mainWindow.ToInt64():X} (pid {_hostedPid}), original style 0x{originalStyle.ToInt64():X}.")

            ' The ribbon only really goes away when its WINDOW does: MinimizeRibbon collapses it to
            ' its tab strip and Word has no equivalent of Excel's SHOW.TOOLBAR macro. Done AFTER the
            ' reparent on purpose -- see HideTopDock: a frame Office has not laid out yet measures
            ' every candidate at nothing and the shape rule cannot pick a winner.
            ' Excel comes through here too when ExcelRibbon says to hide the window rather than ask.
            HideTopDock(mainWindow, kind)

            ' From here on the document must be let go BEFORE the form takes its windows down.
            HookHostForm()

            Relayout()

            ' A RETRY, and only that: HideTopDock returns at once when the band above is already
            ' down. It is here for the frame that had not finished laying itself out at the first
            ' attempt -- Relayout has since squared it with the panel, which is the point at which
            ' Office gives real measurements. It cannot hide a second window, and it cannot write a
            ' second «not found» line for a document that was dealt with.
            HideTopDock(mainWindow, kind)

            ' Without this the sheet is a picture: it scrolls, and it ignores every click.
            TellOfficeItIsActive(True)
            _pulse.Start()

            Dim elapsed As Integer = CInt(Math.Min(Integer.MaxValue, (DateTime.UtcNow - started).TotalMilliseconds))
            _log($"Document embedded in {elapsed} ms: {Path.GetFileName(filePath)}")
            Return OfficeHostResult.Ok(elapsed)
        Catch ex As Exception
            GlobalErrorLog.Write("OfficeDocumentHost.ShowDocument", ex)
            ' Describe, not ex.Message: the reflection wrapper's own message says nothing at all.
            ' The file's own details go with it — an Open that Office refuses is nearly always about
            ' the file, and «which file, how big, still there?» is the first question anyone asks.
            _log("Embedding failed: " & OfficeHostLog.Describe(ex))
            _log("   the file was: " & DescribeFile(filePath))
            Detach()
            Return OfficeHostResult.Fail("Documentul nu a putut fi deschis. Detalii în jurnalul de lucru.")
        End Try
    End Function

    ''' <summary>
    ''' What can be said about a file without opening it: full path, size, and whether it is still
    ''' there at all. Never throws — it is only ever called while reporting another failure.
    ''' </summary>
    Private Shared Function DescribeFile(filePath As String) As String
        Try
            If String.IsNullOrWhiteSpace(filePath) Then Return "<no path>"
            Dim info As New FileInfo(filePath)
            If Not info.Exists Then Return $"{filePath} (NOT on disk any more)"
            Return $"{filePath} ({info.Length} bytes, written {info.LastWriteTime:yyyy-MM-dd HH:mm:ss})"
        Catch ex As Exception
            Return filePath & " (its details could not be read: " & ex.Message & ")"
        End Try
    End Function

    ''' <summary>
    ''' Opens the workbook read-only and takes the program's furniture down: the formula bar, the
    ''' status bar, and — when <see cref="ExcelRibbon"/> asks for it — the ribbon, through the Excel 4
    ''' macro. Under <see cref="ExcelRibbonMode.HideDockWindow"/> the ribbon is not mentioned to Excel
    ''' at all: its window is hidden later, next to Word's, in <see cref="HideTopDock"/>.
    ''' </summary>
    Private Sub OpenExcel(filePath As String)
        ' Filename, UpdateLinks (0 = none), ReadOnly. Positional, because a late-bound call cannot
        ' name arguments; the rest of Open's twelve parameters keep their defaults.
        Dim books As Object = OfficeLateBound.GetProp(_app, "Workbooks")
        Try
            _doc = OfficeLateBound.Invoke(books, "Open", filePath, 0, True)
        Finally
            OfficeLateBound.Release(books)
        End Try

        RememberAndSet(_app, "DisplayFormulaBar", False)
        RememberAndSet(_app, "DisplayStatusBar", False)

        If ExcelRibbon = ExcelRibbonMode.Excel4Macro Then
            ' SHOW.TOOLBAR is an Excel 4 macro and looks like an antique because it is one — it is
            ' also the only call that asks Excel to take the whole ribbon down rather than collapse
            ' it to its tabs. It is a request, though, and a request can be refused.
            _ribbonMacroSent = OfficeLateBound.TryInvoke(_app, "ExecuteExcel4Macro", "SHOW.TOOLBAR(""Ribbon"",False)")
            _log($"Excel ribbon: SHOW.TOOLBAR sent, {If(_ribbonMacroSent, "accepted", "REFUSED — the dock window is not hidden either, so the ribbon stays up")}.")
        Else
            _log("Excel ribbon: nothing asked of Excel; the dock window is hidden instead.")
        End If
    End Sub

    ''' <summary>
    ''' Opens the document read-only in print layout and takes down the status bar and the rulers.
    ''' The ribbon is MINIMIZED rather than removed: Word has no equivalent of Excel's macro, and
    ''' <c>MinimizeRibbon</c> is a toggle, so it is only sent when the ribbon is actually expanded.
    ''' </summary>
    Private Sub OpenWord(filePath As String)
        ' FileName, ConfirmConversions, ReadOnly, AddToRecentFiles.
        Dim docs As Object = OfficeLateBound.GetProp(_app, "Documents")
        Try
            _doc = OfficeLateBound.Invoke(docs, "Open", filePath, False, True, False)
        Finally
            OfficeLateBound.Release(docs)
        End Try

        RememberAndSet(_app, "DisplayStatusBar", False)

        Dim window As Object = OfficeLateBound.TryGetProp(_app, "ActiveWindow")
        If window IsNot Nothing Then
            Try
                OfficeLateBound.TrySetProp(window, "DisplayRulers", False)
                OfficeLateBound.TrySetProp(window, "DisplayVerticalRuler", False)
                Dim view As Object = OfficeLateBound.TryGetProp(window, "View")
                If view IsNot Nothing Then
                    Try
                        ' wdPrintView = 3. Spelled as a literal because there is no interop
                        ' assembly here to take the constant from.
                        OfficeLateBound.TrySetProp(view, "Type", 3)
                    Finally
                        OfficeLateBound.Release(view)
                    End Try
                End If
            Finally
                OfficeLateBound.Release(window)
            End Try
        End If

    End Sub

    ''' <summary>
    ''' Moves a window far off the desktop before it is allowed to show itself. Nothing is read back
    ''' from it — the window is about to be reparented and placed properly anyway.
    ''' </summary>
    ''' <param name="size">The size to park it AT — the panel's, not an arbitrary one. Office lays
    ''' its inner windows out for the size the frame has when the document opens, and it does not
    ''' necessarily lay them out again when the frame is later resized: a workbook opened in an
    ''' 800×600 frame kept its desk at 778×544 inside a 1140×813 panel, with the rest of the pane
    ''' empty. Starting at the right size is most of that problem gone; <see cref="FillInnerChain"/>
    ''' deals with the remainder.</param>
    Private Shared Sub ParkOffScreen(hwnd As IntPtr, size As Size)
        If hwnd = IntPtr.Zero OrElse Not AdobeNativeMethods.IsWindow(hwnd) Then Return
        AdobeNativeMethods.SetWindowPos(hwnd, IntPtr.Zero, -32000, -32000,
                                        Math.Max(400, size.Width), Math.Max(300, size.Height),
                                        AdobeNativeMethods.SWP_NOZORDER Or AdobeNativeMethods.SWP_NOACTIVATE)
    End Sub

    ''' <summary>
    ''' The chrome that only comes off once the program is visible. Word ignores
    ''' <c>DisplayStatusBar</c> written at a hidden application and reports its ribbon as zero pixels
    ''' high, so both are done again here, where the answers are real.
    '''
    ''' <para>The status bar is re-written with a plain <c>TrySetProp</c>, not
    ''' <see cref="RememberAndSet"/>: the operator's original value was captured on the first
    ''' attempt, and recording it a second time would save OUR value as the one to restore.</para>
    ''' </summary>
    ''' <para>Word's ribbon is NOT minimised here any more. <c>ExecuteMso("MinimizeRibbon")</c> is a
    ''' toggle whose result Office keeps in the USER PROFILE: two preview runs left the operator's
    ''' own Word opening with a collapsed ribbon, and nothing put it back. Since the whole dock
    ''' window is hidden anyway (<see cref="HideTopDock"/>), collapsing it first bought nothing
    ''' but that side effect.</para>
    Private Sub StripChromeAfterShow(kind As OfficeDocumentKind)
        OfficeLateBound.TrySetProp(_app, "DisplayStatusBar", False)
    End Sub

    ''' <summary>
    ''' The top-level window to host. Excel hands its own out (<c>Application.Hwnd</c>); Word does
    ''' not, so the document window is asked for its handle and the tree is walked up to the frame.
    ''' When neither answers, the last resort is the visible top-level window of the right class
    ''' whose title carries the file name — the same "match on the title" rule the Adobe host learned.
    ''' </summary>
    Private Function ResolveMainWindow(kind As OfficeDocumentKind, filePath As String) As IntPtr
        If kind = OfficeDocumentKind.Excel Then
            Dim h As IntPtr = New IntPtr(OfficeLateBound.AsInt64(OfficeLateBound.TryGetProp(_app, "Hwnd")))
            If h <> IntPtr.Zero AndAlso AdobeWindowHosting.IsAlive(h) Then Return h
            Return FindTopLevelByTitle("XLMAIN", filePath)
        End If

        Dim window As Object = OfficeLateBound.TryGetProp(_app, "ActiveWindow")
        If window IsNot Nothing Then
            Try
                Dim inner As IntPtr = New IntPtr(OfficeLateBound.AsInt64(OfficeLateBound.TryGetProp(window, "Hwnd")))
                Dim root As IntPtr = TopLevelOf(inner)
                If root <> IntPtr.Zero Then Return root
            Finally
                OfficeLateBound.Release(window)
            End Try
        End If
        Return FindTopLevelByTitle("OpusApp", filePath)
    End Function

    ''' <summary>Walks up from a child window to the window that has no parent.</summary>
    Private Shared Function TopLevelOf(hwnd As IntPtr) As IntPtr
        If hwnd = IntPtr.Zero OrElse Not AdobeNativeMethods.IsWindow(hwnd) Then Return IntPtr.Zero
        Dim current As IntPtr = hwnd
        ' Bounded: a corrupt parent chain must not spin here forever.
        For i As Integer = 0 To 15
            Dim parent As IntPtr = AdobeNativeMethods.GetParent(current)
            If parent = IntPtr.Zero Then Return current
            current = parent
        Next
        Return current
    End Function

    ''' <summary>The visible top-level window of <paramref name="className"/> whose caption
    ''' contains the file's base name, or zero.</summary>
    Private Shared Function FindTopLevelByTitle(className As String, filePath As String) As IntPtr
        Dim wanted As String = Path.GetFileNameWithoutExtension(filePath)
        Dim found As IntPtr = IntPtr.Zero
        AdobeNativeMethods.EnumWindows(
            Function(hwnd As IntPtr, extra As IntPtr) As Boolean
                If Not AdobeNativeMethods.IsWindowVisible(hwnd) Then Return True
                If Not String.Equals(AdobeNativeMethods.GetClass(hwnd), className, StringComparison.OrdinalIgnoreCase) Then Return True
                If wanted.Length > 0 AndAlso
                   AdobeNativeMethods.GetTitle(hwnd).IndexOf(wanted, StringComparison.OrdinalIgnoreCase) < 0 Then Return True
                found = hwnd
                Return False
            End Function, IntPtr.Zero)
        Return found
    End Function

    ''' <summary>
    ''' Waits until the whole class chain exists under <paramref name="root"/>.
    '''
    ''' <para>It sleeps rather than pumping messages. <c>Application.DoEvents</c> would keep the pane
    ''' responsive, and that is exactly the danger: the operator clicking the next row would re-enter
    ''' <see cref="ShowDocument"/> on top of the one still running. Office is out of process and lays
    ''' its own windows out without our pump, so there is nothing to gain for the risk.</para>
    ''' </summary>
    Private Shared Function WaitForChain(root As IntPtr, chain As String()) As Boolean
        Dim deadline As DateTime = DateTime.UtcNow.AddMilliseconds(EmbedTimeoutMs)
        Do
            Dim current As IntPtr = root
            Dim complete As Boolean = True
            For Each className As String In chain
                current = FindChildByClass(current, className)
                If current = IntPtr.Zero Then
                    complete = False
                    Exit For
                End If
            Next
            If complete Then Return True
            Threading.Thread.Sleep(PollStepMs)
        Loop While DateTime.UtcNow < deadline
        Return False
    End Function

    ''' <summary>
    ''' Hides the band that carries the ribbon. Always for Word, which has no other way; for Excel
    ''' only under <see cref="ExcelRibbonMode.HideDockWindow"/>.
    '''
    ''' <para>Nothing is put back afterwards. This is our own private instance and it is killed on
    ''' teardown, so there is no profile setting here to corrupt — unlike <c>DisplayFormulaBar</c>
    ''' and friends, this is a window, not a preference. That is the whole appeal of doing Excel this
    ''' way: the macro engine is never touched, and neither is anything Office writes to the user
    ''' profile.</para>
    '''
    ''' <para>What is hidden is always written to the log with its class, its title and its height,
    ''' and a miss lists the frame's children instead. See <see cref="FindRibbonBand"/> for why that
    ''' matters more here than anywhere else in this class.</para>
    ''' </summary>
    Private Sub HideTopDock(mainWindow As IntPtr, kind As OfficeDocumentKind)
        If mainWindow = IntPtr.Zero Then Return
        If _ribbonBandHidden Then Return
        If kind = OfficeDocumentKind.Excel AndAlso ExcelRibbon <> ExcelRibbonMode.HideDockWindow Then Return

        Dim band As IntPtr = FindRibbonBand(mainWindow, kind)
        If band = IntPtr.Zero Then
            _log("The ribbon band was not found under the frame; the ribbon stays up.")
            _log("   the frame's own children were: " & DescribeChildren(mainWindow))
            Return
        End If
        Dim area As Rectangle = AdobeNativeMethods.RectInParent(band)
        AdobeNativeMethods.ShowWindow(band, AdobeNativeMethods.SW_HIDE)
        _ribbonBandHidden = True
        _log($"Ribbon band 0x{band.ToInt64():X} hidden: {AdobeNativeMethods.GetClass(band)} ""{AdobeNativeMethods.GetTitle(band)}"", {area.Height} px tall.")
    End Sub

    ''' <summary>
    ''' The window that carries the ribbon, found by name where the name is known and by SHAPE where
    ''' it is not.
    '''
    ''' <para><b>The two programs do not name it the same thing, and neither is a promise.</b> Word's
    ''' is class <c>MsoCommandBarDock</c> titled <c>MsoDockTop</c> — matched on class AND title,
    ''' because the left, right and bottom docks share that class and must stay: hiding all four
    ''' would take the scroll bars and the view controls with them. Excel's is class <c>EXCEL2</c>
    ''' with no title at all — 267 px tall over a 1118 px frame, read out of a live
    ''' <c>Office16\EXCEL.EXE</c> with Window Detective. It is emphatically NOT an
    ''' <c>MsoCommandBarDock</c>, which is what this code assumed before the window was actually
    ''' looked at.</para>
    '''
    ''' <para><b>So the names are a fast path, not the answer.</b> Both are private Office details
    ''' that carry no compatibility promise, and <c>EXCEL2</c> is the weaker of the two: it is a bare
    ''' Excel class with an EMPTY title, and Excel gives it to SEVERAL children of the same frame.
    ''' Measured 04.09.2026 by walking a live <c>XLMAIN</c>: four separate <c>EXCEL2</c> children,
    ''' all untitled. So the class narrows the field and the geometry picks the winner out of it.</para>
    '''
    ''' <para><b>Excel does not lay its frame out the way Word does.</b> This is what the first
    ''' attempt got wrong. Under Word, <c>_WwF</c> starts BELOW the dock, so "the band above the
    ''' document" identifies the ribbon. Under Excel it identifies nothing: <c>XLDESK</c> sits at
    ''' 0,0 and fills the entire client area — measured 745×505 in a 745×504 frame — with the bars
    ''' as siblings ON TOP of it rather than above it. The old rule read a document top of 0, decided
    ''' there was nothing above it, and returned empty every single time. That is why the ribbon
    ''' stayed up.</para>
    '''
    ''' <para>An Office that renames its windows still gets its ribbon hidden, because the class is
    ''' only ever preferred and never required, and the log line names what was actually hidden — so
    ''' the day this does go wrong the answer is already in the working log.</para>
    ''' </summary>
    Private Shared Function FindRibbonBand(frame As IntPtr, kind As OfficeDocumentKind) As IntPtr
        If kind = OfficeDocumentKind.Word Then
            Dim dock As IntPtr = FindChildByClassAndText(frame, WordDockClass, WordTopDockText)
            If dock <> IntPtr.Zero Then Return dock
        End If

        Dim documentClass As String = If(kind = OfficeDocumentKind.Excel, ExcelChain(0), WordChain(0))
        ' Excel's class first, since it rules out the desk and the formula bar outright; then the
        ' same search with no class at all, for an Office that has renamed it.
        If kind = OfficeDocumentKind.Excel Then
            Dim named As IntPtr = FindTopBand(frame, documentClass, ExcelRibbonClass)
            If named <> IntPtr.Zero Then Return named
        End If

        ' THE CLASS-LESS PASS ONLY RUNS WHEN SOMETHING REALLY IS ABOVE THE DOCUMENT. It is a net for
        ' an Office that has renamed its ribbon window, and a net that wide needs a floor under it:
        ' an Excel whose ribbon is genuinely not there leaves the desk at the top of the frame and
        ' the formula bar (372x57 measured) as the tallest band left, which this would then hide --
        ' the cell editor gone, and nothing gained. A document window sitting at the top of the frame
        ' is Office saying there is no band to take down, so we take it at its word.
        '
        ' The named pass above is NOT gated this way: a class we measured is an identification, not a
        ' guess, and it stays trusted whatever the desk is doing.
        Dim document As IntPtr = FindChildByClass(frame, documentClass)
        If document <> IntPtr.Zero AndAlso AdobeNativeMethods.RectInParent(document).Y <= 0 Then Return IntPtr.Zero

        Return FindTopBand(frame, documentClass, Nothing)
    End Function

    ''' <summary>
    ''' The tallest visible direct child of <paramref name="frame"/> that looks like a bar across the
    ''' top of it: nearly the frame's full width, starting in its upper half, and not covering the
    ''' whole of it.
    ''' </summary>
    ''' <param name="documentClass">The class of the window the document is drawn in. Skipped by
    ''' HANDLE, whatever its shape — under Excel the desk is a full-width child starting at 0,0, so
    ''' it fits the bar description perfectly and hiding it would hide the sheet.</param>
    ''' <param name="requiredClass">The class to restrict the search to, or <c>Nothing</c> to look at
    ''' every child. Excel's ribbon class is passed here rather than matched directly, because it is
    ''' shared with siblings that are not the ribbon.</param>
    ''' <remarks>
    ''' The three shape guards are what keep this from hiding something that matters. <b>Nearly the
    ''' full width</b> rules out floating scraps parked high up; <b>starting in the upper half</b>
    ''' rules out the status bar and anything else along the bottom; <b>not the full height</b> rules
    ''' out a desk or a pane that covers the frame. Tallest-wins then picks the ribbon over the
    ''' formula bar and the quick-access strip. The Office 16 ribbon clears all three with room to
    ''' spare: 1118 px wide, 267 px tall, at the top of the frame.
    '''
    ''' <para><b>Visibility is asked of the child ITSELF, never of its ancestors</b>
    ''' (<see cref="AdobeNativeMethods.IsVisibleStyleSet"/>). This is the whole of what made the
    ''' ribbon survive in the DDF preview while the DevHarness hid it every time: the harness keeps
    ''' its host panel on screen, and the preview pane hides <c>pnlGazda</c> to show
    ''' «Se deschide documentul…» while the document opens. The frame is reparented into that hidden
    ''' panel, so <c>IsWindowVisible</c> — which is False when ANY ancestor is hidden — answered False
    ''' for every child of the frame, this loop rejected all of them, and the search came back empty.
    ''' The log said so in as many words at every miss: «EXCEL2 "" 0,0 535x267 hidden» for the very
    ''' band that was about to be on screen. Word never noticed, because its dock is found by class
    ''' and title above and is never asked whether it is showing.</para>
    ''' </remarks>
    Private Shared Function FindTopBand(frame As IntPtr, documentClass As String, requiredClass As String) As IntPtr
        Dim client As Size = AdobeNativeMethods.ClientSize(frame)
        If client.Width <= 0 OrElse client.Height <= 0 Then Return IntPtr.Zero
        Dim document As IntPtr = FindChildByClass(frame, documentClass)

        Dim best As IntPtr = IntPtr.Zero
        Dim bestHeight As Integer = 0
        Dim child As IntPtr = AdobeNativeMethods.GetWindow(frame, AdobeNativeMethods.GW_CHILD)
        Do While child <> IntPtr.Zero
            If child <> document AndAlso AdobeNativeMethods.IsVisibleStyleSet(child) AndAlso
               (requiredClass Is Nothing OrElse
                String.Equals(AdobeNativeMethods.GetClass(child), requiredClass, StringComparison.OrdinalIgnoreCase)) Then
                Dim r As Rectangle = AdobeNativeMethods.RectInParent(child)
                If r.Height > bestHeight AndAlso
                   r.Width * 100 >= client.Width * 80 AndAlso
                   r.Y * 2 < client.Height AndAlso
                   r.Height < client.Height Then
                    best = child
                    bestHeight = r.Height
                End If
            End If
            child = AdobeNativeMethods.GetWindow(child, AdobeNativeMethods.GW_HWNDNEXT)
        Loop
        Return best
    End Function

    ''' <summary>Every direct child of a window as <c>class "title" x,y wxh vis</c>, for the one log
    ''' line that has to explain why a window was not found. The geometry is there on purpose: with
    ''' four identically-named <c>EXCEL2</c> children, the rectangle is the only thing that tells them
    ''' apart, and a miss that prints only names cannot be diagnosed. Bounded — this goes in a log.</summary>
    Private Shared Function DescribeChildren(parent As IntPtr) As String
        If parent = IntPtr.Zero Then Return "<no frame>"
        Dim seen As New List(Of String)()
        Dim child As IntPtr = AdobeNativeMethods.GetWindow(parent, AdobeNativeMethods.GW_CHILD)
        Do While child <> IntPtr.Zero AndAlso seen.Count < 24
            Dim r As Rectangle = AdobeNativeMethods.RectInParent(child)
            seen.Add($"{AdobeNativeMethods.GetClass(child)} ""{AdobeNativeMethods.GetTitle(child)}"" {r.X},{r.Y} {r.Width}x{r.Height} {If(AdobeNativeMethods.IsVisibleStyleSet(child), "vis", "hidden")}")
            child = AdobeNativeMethods.GetWindow(child, AdobeNativeMethods.GW_HWNDNEXT)
        Loop
        If seen.Count = 0 Then Return "<none>"
        Return String.Join(", ", seen)
    End Function

    ''' <summary>
    ''' Makes Excel's own inner windows fill the frame we placed.
    '''
    ''' <para>The frame (<c>XLMAIN</c>) follows the panel exactly — it is the window we move. What it
    ''' contains does not: the desk (<c>XLDESK</c>) and the sheet (<c>EXCEL7</c>) keep whatever size
    ''' they were laid out at, so a workbook that opened in a small frame sits in the top-left corner
    ''' of the pane with empty grey to the right and below it. Measured 04.09.2026: <c>XLMAIN</c>
    ''' 1140×813, <c>XLDESK</c> 778×544, both at the same origin.</para>
    '''
    ''' <para>Each window in the chain is therefore placed against its parent's CLIENT area, which is
    ''' also what Excel itself does for a maximized workbook — this is the geometry it would have,
    ''' not a layout of our invention. Only sizes that are actually wrong are written, so a chain
    ''' Excel has already got right costs nothing but the measuring.</para>
    '''
    ''' <para>Word is not touched. Its band is handled by moving the frame instead
    ''' (<see cref="TopInset"/>), and its document window is <b>not</b> a plain fill: the page has
    ''' margins around it that Word draws and manages itself.</para>
    ''' </summary>
    Private Sub FillInnerChain()
        If _kind <> OfficeDocumentKind.Excel OrElse _hosted = IntPtr.Zero Then Return
        Dim parent As IntPtr = _hosted
        For Each className As String In ExcelChain
            Dim child As IntPtr = FindChildByClass(parent, className)
            If child = IntPtr.Zero Then Return
            Dim area As Size = AdobeNativeMethods.ClientSize(parent)
            If area.Width <= 0 OrElse area.Height <= 0 Then Return
            Dim current As Rectangle = AdobeNativeMethods.RectInParent(child)
            If current.X <> 0 OrElse current.Y <> 0 OrElse
               current.Width <> area.Width OrElse current.Height <> area.Height Then
                AdobeNativeMethods.MoveWindow(child, 0, 0, area.Width, area.Height, True)
                ' Once per document. Relayout runs on every WM_SIZE, and an Excel that never
                ' reflows its desk would otherwise write a line for every pixel of a drag.
                If Not _fillLogged Then
                    _log($"{className} resized from {current.Width}x{current.Height} to {area.Width}x{area.Height}.")
                End If
            End If
            parent = child
        Next
        _fillLogged = True
    End Sub

    ''' <summary>
    ''' The height of the dead band at the top of the hosted frame: where the document window starts,
    ''' in the frame's own coordinates.
    '''
    ''' <para>Word does not close the gap a hidden dock leaves — the document window stays exactly
    ''' where it was, with an empty strip above it — so the gap is measured instead of assumed. When
    ''' Word does re-layout (or the dock was never there) this reads zero and the placement below is
    ''' the plain one.</para>
    '''
    ''' <para><b>Excel is never asked, under either ribbon method.</b> With the macro it re-lays its
    ''' desk out itself and the sheet already starts at the top; with the dock window hidden it does
    ''' NOT, but <see cref="FillInnerChain"/> then places <c>XLDESK</c> at the frame's own origin,
    ''' which closes the same gap from the inside. Measuring it here as well would move the frame up
    ''' by a band that is no longer there, and the bottom of the sheet would fall off the panel.</para>
    ''' </summary>
    Private Function TopInset() As Integer
        If _kind <> OfficeDocumentKind.Word OrElse _hosted = IntPtr.Zero Then Return 0
        Dim document As IntPtr = FindChildByClass(_hosted, WordChain(0))
        If document = IntPtr.Zero Then Return 0
        Return Math.Max(0, AdobeNativeMethods.RectInParent(document).Y)
    End Function

    ''' <summary>The first direct child of <paramref name="parent"/> with that window class.</summary>
    Private Shared Function FindChildByClass(parent As IntPtr, className As String) As IntPtr
        If parent = IntPtr.Zero Then Return IntPtr.Zero
        Dim child As IntPtr = AdobeNativeMethods.GetWindow(parent, AdobeNativeMethods.GW_CHILD)
        Do While child <> IntPtr.Zero
            If String.Equals(AdobeNativeMethods.GetClass(child), className, StringComparison.OrdinalIgnoreCase) Then Return child
            child = AdobeNativeMethods.GetWindow(child, AdobeNativeMethods.GW_HWNDNEXT)
        Loop
        Return IntPtr.Zero
    End Function

    ''' <summary>The first direct child of <paramref name="parent"/> with that class AND that window
    ''' title. Word's four command-bar docks only differ by title.</summary>
    Private Shared Function FindChildByClassAndText(parent As IntPtr, className As String, text As String) As IntPtr
        If parent = IntPtr.Zero Then Return IntPtr.Zero
        Dim child As IntPtr = AdobeNativeMethods.GetWindow(parent, AdobeNativeMethods.GW_CHILD)
        Do While child <> IntPtr.Zero
            If String.Equals(AdobeNativeMethods.GetClass(child), className, StringComparison.OrdinalIgnoreCase) AndAlso
               String.Equals(AdobeNativeMethods.GetTitle(child), text, StringComparison.OrdinalIgnoreCase) Then Return child
            child = AdobeNativeMethods.GetWindow(child, AdobeNativeMethods.GW_HWNDNEXT)
        Loop
        Return IntPtr.Zero
    End Function

    ''' <summary>
    ''' Fills the surface with the hosted window. Safe to call at any time — a resize handler runs
    ''' whether or not anything is embedded.
    '''
    ''' <para><b>The band where the ribbon used to be.</b> Hiding a docked bar does not make Word
    ''' close the gap it left. Rather than nag the frame into a re-layout it has already declined to
    ''' do, the frame is moved UP by the height of that gap and made exactly that much taller: the
    ''' empty strip goes above the panel's top edge, where the panel clips it, the page starts at the
    ''' top edge, and the bottom still lands where it should. It costs one extra
    ''' <c>MoveWindow</c> — and only when there is a gap to hide.</para>
    ''' </summary>
    Public Sub Relayout()
        Try
            If _hosted = IntPtr.Zero OrElse Not AdobeWindowHosting.IsAlive(_hosted) Then Return
            Dim bounds As New Rectangle(0, 0, _surface.ClientSize.Width, _surface.ClientSize.Height)
            If bounds.Width <= 0 OrElse bounds.Height <= 0 Then Return

            ' Square with the panel FIRST: the frame gets its real width, and whatever Word still
            ' wants to lay out at that width, it lays out now. Only then is the gap worth measuring.
            AdobeWindowHosting.Place(_hosted, bounds)

            _topInset = TopInset()
            If _topInset > 0 Then
                bounds = New Rectangle(bounds.X, bounds.Y - _topInset,
                                       bounds.Width, bounds.Height + _topInset)
                AdobeWindowHosting.Place(_hosted, bounds)
            End If

            FillInnerChain()
            AdobeWindowHosting.NudgeRedraw(_hosted, bounds)
        Catch ex As Exception
            GlobalErrorLog.Write("OfficeDocumentHost.Relayout", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Teardown
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Lets the hosted document go: settings back, document closed without saving, application
    ''' quit, COM released, and the process killed if it outlived all of that. Never throws.
    ''' </summary>
    Public Sub Detach()
        Try
            _pulse.Stop()
            _toldActive = False
            If _app Is Nothing AndAlso _hosted = IntPtr.Zero Then Return
            UnhookHostForm()

            ' NOTHING IS SAID TO OFFICE ONCE ITS WINDOW HAS GONE. The hosted frame is a child of the
            ' caller's panel, and when WinForms destroys that panel Windows destroys the frame with
            ' it — across the process boundary, without asking. What is left is an Office process
            ' with no main window, and the FIRST property written to it kills it: measured
            ' 04.09.2026, «Property DisplayStatusBar could not be written: Exception has been thrown
            ' by the target of an invocation», then RPC_E_DISCONNECTED for every call after it, and a
            ' fault dialog on the operator's screen. The stack from that crash is Excel's own menu
            ' machinery re-laying out a window that no longer exists.
            '
            ' So when the window is gone, so is the chance to be polite: release the references and
            ' end the process by pid. A zero handle means the frame was never hosted (a failure part
            ' way through ShowDocument) — that Office is intact and is asked properly.
            Dim reachable As Boolean = _hosted = IntPtr.Zero OrElse AdobeWindowHosting.IsAlive(_hosted)
            If Not reachable Then
                _log($"The hosted window is already destroyed; process {_hostedPid} is ended without being spoken to.")
            End If

            If reachable Then RestoreChrome()

            ' Close(False) / Close(0): read-only or not, an unsaved-changes prompt inside a hosted
            ' window is a dialog nobody can reach.
            If _doc IsNot Nothing Then
                If reachable Then OfficeLateBound.TryInvoke(_doc, "Close", False)
                OfficeLateBound.Release(_doc)
            End If

            If _app IsNot Nothing Then
                If reachable Then OfficeLateBound.TryInvoke(_app, "Quit")
                OfficeLateBound.Release(_app)
            End If

            ' NO GC.Collect HERE, deliberately. The usual Office teardown recipe ends with two
            ' collections because it drops its wrappers and hopes the finalizer gets round to
            ' releasing them. This class does not hope: OfficeLateBound.Release calls
            ' FinalReleaseComObject on every reference it took, which drops the COM count to zero
            ' there and then, and whatever survives that is dealt with below by name and by pid.
            KillIfStillRunning(reachable)
        Catch ex As Exception
            GlobalErrorLog.Write("OfficeDocumentHost.Detach", ex)
        Finally
            _doc = Nothing
            _app = Nothing
            _hosted = IntPtr.Zero
            _hostedPid = 0
            _topInset = 0
            _fillLogged = False
            _ribbonMacroSent = False
            _ribbonBandHidden = False
            _chromeRestore.Clear()
        End Try
    End Sub

    ''' <summary>
    ''' Quitting is a request, not a guarantee: an Office process whose window we reparented can sit
    ''' there with no UI and no owner. Killing it is the only honest end — this is OUR instance,
    ''' opened read-only, with nothing in it the operator could lose.
    ''' </summary>
    ''' <param name="wasAskedToQuit">False when the process was never sent <c>Quit</c> because its
    ''' window had already been destroyed. There is then nothing to wait for — waiting the full
    ''' grace period would only hold the closing form still for three seconds.</param>
    Private Sub KillIfStillRunning(wasAskedToQuit As Boolean)
        If _hostedPid <= 0 Then Return
        Try
            Dim p As Process = Process.GetProcessById(_hostedPid)
            Using p
                If wasAskedToQuit AndAlso p.WaitForExit(QuitGraceMs) Then
                    _log($"Office process {_hostedPid} exited on its own.")
                    Return
                End If
                _log(If(wasAskedToQuit,
                        $"Office process {_hostedPid} did not quit within {QuitGraceMs} ms; killing it.",
                        $"Office process {_hostedPid} was never asked to quit; killing it."))
                p.Kill()
                p.WaitForExit(QuitGraceMs)
            End Using
        Catch ex As ArgumentException
            ' No such process any more: it quit between the call and here. The good outcome.
            _log($"Office process {_hostedPid} was already gone ({ex.Message}).")
        Catch ex As Exception
            GlobalErrorLog.Write("OfficeDocumentHost.KillIfStillRunning", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Keeping the document clickable
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Tells the hosted document, three times a second, that its application is the one the
    ''' operator is working in.
    '''
    ''' <para><b>Why this exists.</b> A reparented Office window takes the mouse WHEEL but ignores
    ''' mouse BUTTONS: no cell selection, no context menu. Measured 04.09.2026 by driving real input
    ''' at a hosted sheet and reading <c>ActiveCell</c> back out of Excel — with the frame left
    ''' top-level, every click landed; inside the panel, not one did, and it made no difference
    ''' whether the ribbon had been stripped, whether the threads shared an input queue
    ''' (<c>AttachThreadInput</c>) or where the focus was. The one message that changed the answer
    ''' was <c>WM_ACTIVATEAPP</c>.</para>
    '''
    ''' <para>Office decides from that message whether its application is in the foreground, and it
    ''' refuses mouse buttons when it believes it is not. Once its window is a child of ours, Windows
    ''' never sends it again: activation belongs to our top-level window, which is a different
    ''' process. So we send it ourselves, and we keep sending it — one message did not hold for more
    ''' than a moment, while a pulse held through everything an operator does: clicking in our own
    ''' controls, switching to another program and back, and sitting idle.</para>
    '''
    ''' <para>It is POSTED, never sent: a synchronous call into an Office that is busy opening a
    ''' document would hang the operator's form. It is only sent while our own window really is in
    ''' the foreground, and the matching "no longer active" goes out once when it is not — Office is
    ''' never told something the operator can see is untrue.</para>
    '''
    ''' <para><b>And the hosted process's OWN foreground counts as ours.</b> This is not a detail:
    ''' without it the pulse closed every Word context menu it made possible. A menu is a top-level
    ''' popup owned by Office, so opening one moves the foreground off our form; the next tick read
    ''' that as the operator having left and posted the deactivation, on which Word cancelled menu
    ''' tracking. The menu lasted until the next tick and no longer — about a third of a second,
    ''' which is what «the right-click menu disappears» looks like from the outside.</para>
    ''' </summary>
    Private Sub PulseActivation(sender As Object, e As EventArgs)
        Try
            ' A disposed surface would throw here three times a second and fill the error log with
            ' the same line; there is nothing left to keep alive anyway.
            If _surface.IsDisposed Then
                _pulse.Stop()
                Return
            End If
            If _hosted = IntPtr.Zero OrElse Not AdobeWindowHosting.IsAlive(_hosted) Then Return
            Dim foreground As IntPtr = AdobeNativeMethods.GetForegroundWindow()

            ' NOTHING IS SAID WHILE OFFICE ITSELF HOLDS THE FOREGROUND. A context menu is a
            ' top-level POPUP owned by the Office process, and opening one takes the foreground away
            ' from our form. Without this guard the next tick sees a foreground that is not ours,
            ' concludes the operator has left, and posts WM_ACTIVATEAPP(FALSE) — on which Word
            ' cancels menu tracking and the menu the operator just opened disappears, roughly a
            ' third of a second after it appeared. The message that makes right-click work at all is
            ' the same one that was then killing the menu it opened.
            '
            ' So a foreground window belonging to the hosted process counts as ours, and gets told
            ' nothing whatsoever: not FALSE, which closes the menu, and not TRUE either, since an
            ' activation message posted into an open menu is not worth the risk when Office already
            ' knows it is in front.
            Dim foregroundPid As Integer = 0
            If foreground <> IntPtr.Zero Then
                AdobeNativeMethods.GetWindowThreadProcessId(foreground, foregroundPid)
            End If
            If _hostedPid > 0 AndAlso foregroundPid = _hostedPid Then Return

            Dim top As Control = _surface.TopLevelControl
            Dim ours As Boolean = top IsNot Nothing AndAlso foreground = top.Handle
            If ours Then
                TellOfficeItIsActive(True)
            ElseIf _toldActive Then
                TellOfficeItIsActive(False)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("OfficeDocumentHost.PulseActivation", ex)
        End Try
    End Sub

    ''' <param name="active">True while our window has the foreground. The lParam is the thread the
    ''' activation is coming from or going to, which is ours in both directions.</param>
    Private Sub TellOfficeItIsActive(active As Boolean)
        Dim pid As Integer = 0
        Dim threadId As Integer = AdobeNativeMethods.GetWindowThreadProcessId(_surface.Handle, pid)
        AdobeNativeMethods.PostMessage(_hosted, AdobeNativeMethods.WM_ACTIVATEAPP,
                                       New IntPtr(If(active, 1, 0)), New IntPtr(threadId))
        _toldActive = active
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Getting out before the panel does
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Listens for the form holding the surface to start closing, and lets the document go there.
    '''
    ''' <para>WITHOUT THIS THE TEARDOWN IS ALWAYS TOO LATE. A form destroys its window before it
    ''' disposes its controls, so by the time <c>Dispose</c> reaches the panel — and the panel
    ''' reaches this class — the hosted frame has already been destroyed along with it, and Office
    ''' is a process with no window that faults at the first thing said to it. <c>FormClosing</c> is
    ''' the last moment at which everything is still alive.</para>
    '''
    ''' <para>A close that another handler then CANCELS leaves the pane empty until the operator
    ''' picks a row again. That is the harmless direction to be wrong in.</para>
    ''' </summary>
    Private Sub HookHostForm()
        UnhookHostForm()
        _hostForm = _surface.FindForm()
        If _hostForm Is Nothing Then Return
        AddHandler _hostForm.FormClosing, AddressOf HostFormClosing
    End Sub

    Private Sub UnhookHostForm()
        If _hostForm Is Nothing Then Return
        RemoveHandler _hostForm.FormClosing, AddressOf HostFormClosing
        _hostForm = Nothing
    End Sub

    ''' <summary>UI boundary: logs and swallows. A failure here must not stop a form from closing.</summary>
    Private Sub HostFormClosing(sender As Object, e As FormClosingEventArgs)
        Try
            Detach()
        Catch ex As Exception
            GlobalErrorLog.Write("OfficeDocumentHost.HostFormClosing", ex)
        End Try
    End Sub

    ''' <summary>Records a setting's current value, then writes the new one.</summary>
    Private Sub RememberAndSet(target As Object, name As String, value As Object)
        Dim original As Object = OfficeLateBound.TryGetProp(target, name)
        If OfficeLateBound.TrySetProp(target, name, value) AndAlso original IsNot Nothing Then
            _chromeRestore.Add(New ChromeSetting(target, name, original))
        End If
    End Sub

    ''' <summary>Replays the recorded settings, newest first. See the class remarks for why.</summary>
    Private Sub RestoreChrome()
        For i As Integer = _chromeRestore.Count - 1 To 0 Step -1
            Dim s As ChromeSetting = _chromeRestore(i)
            OfficeLateBound.TrySetProp(s.Target, s.Name, s.OriginalValue)
        Next
        ' Excel only, and only when the macro really went out. Word has no such member, and sending
        ' it there wrote a DISP_E_UNKNOWNNAME line into the working log at every single teardown --
        ' noise that would eventually make somebody stop reading the log, which is the one thing it
        ' must not do. The same goes for an Excel hosted with its dock window hidden: nothing was
        ' asked of the ribbon, so there is nothing to put back.
        If _kind = OfficeDocumentKind.Excel AndAlso _ribbonMacroSent Then
            OfficeLateBound.TryInvoke(_app, "ExecuteExcel4Macro", "SHOW.TOOLBAR(""Ribbon"",True)")
        End If
        _chromeRestore.Clear()
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Detach()
        RemoveHandler _pulse.Tick, AddressOf PulseActivation
        _pulse.Dispose()
    End Sub

    ''' <summary>One application setting, with the value it had before this class touched it.</summary>
    Private NotInheritable Class ChromeSetting
        Public ReadOnly Property Target As Object
        Public ReadOnly Property Name As String
        Public ReadOnly Property OriginalValue As Object

        Public Sub New(target As Object, name As String, originalValue As Object)
            Me.Target = target
            Me.Name = name
            Me.OriginalValue = originalValue
        End Sub
    End Class

End Class
