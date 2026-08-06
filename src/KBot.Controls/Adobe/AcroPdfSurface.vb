Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>How an attempt to show a document in the ActiveX control ended.</summary>
Public Enum AcroPdfStatus
    ''' <summary>Loaded, laid out, and the panes are collapsed.</summary>
    Shown = 0
    ''' <summary>The AcroPDF control is not registered on this machine.</summary>
    NotRegistered = 1
    ''' <summary>The file is not on disk.</summary>
    FileMissing = 2
    ''' <summary>The control refused the document, or could not be created.</summary>
    Failed = 3
End Enum

''' <summary>The status, the Romanian sentence for the operator, and what the collapse achieved.</summary>
Public NotInheritable Class AcroPdfResult

    Public ReadOnly Property Status As AcroPdfStatus
    Public ReadOnly Property Message As String
    ''' <summary>True when the document view ended up filling the panel (panes collapsed).</summary>
    Public ReadOnly Property Collapsed As Boolean

    Public Sub New(status As AcroPdfStatus, message As String, Optional collapsed As Boolean = False)
        Me.Status = status
        Me.Message = If(message, "")
        Me.Collapsed = collapsed
    End Sub

    Public ReadOnly Property Succeeded As Boolean
        Get
            Return Status = AcroPdfStatus.Shown
        End Get
    End Property

End Class

''' <summary>
''' Shows a PDF in the AcroPDF ActiveX control, and puts it into the state the operator actually
''' wants: document filling the panel, Adobe's panes collapsed.
'''
''' WHY THIS EXISTS AS A SHARED CLASS. Everything here was learned on the bench between 05 and
''' 06.08.2026, and every step is a measurement rather than a guess. Re-deriving it in KBot.App
''' would mean a second copy of hard-won behaviour that would drift — the mistake slice 0024-01 was
''' written to stop. The bench and the DDF preview drive THIS class.
'''
''' THE SEQUENCE, AND WHY EACH STEP IS THERE:
'''
'''  1. <b>Load.</b> <c>LoadFile</c> by reflection — <c>Option Strict On</c> forbids late binding,
'''     and there is deliberately no interop assembly (see <see cref="AcroPdfHost"/>).
'''  2. <b>Wake.</b> Adobe postpones its FIRST layout until the control receives input: right after
'''     the first load, 26 of 28 child windows are zero-sized and the panel is grey until someone
'''     clicks in it. A focus call plus a one-pixel resize fixes it — and the SIZE CHANGE is the part
'''     that works («Adobe recomputes its layout on a size CHANGE, not on a repaint»).
'''  3. <b>Collapse.</b> Pressing Adobe's OWN collapse button. Hiding the panes with
'''     <c>ShowWindow</c> does NOT work: hiding leaves the WIDTH, so Adobe never re-lays-out and the
'''     document stays inset by 67px. Adobe's own collapse sets the width to zero AND moves the
'''     siblings. And because the button toggles, an already-collapsed strip is toggled TWICE — the
'''     floating bar follows the collapse ACTION, not the collapsed STATE.
'''
''' WHAT IS NOT CLAIMED: the chrome API (<c>setShowToolbar</c> and friends) does nothing on this
''' build — all five calls return OK and the bars stay — so it is not used here at all.
''' </summary>
Public NotInheritable Class AcroPdfSurface
    Implements IDisposable

    ' Deep enough for Adobe's pane tree, which was measured at 11 levels.
    Private Const ProbeDepth As Integer = 14
    ' Adobe re-lays-out asynchronously; these are the waits that let it.
    Private Const LayoutSettleMs As Integer = 250
    Private Const CollapseSettleMs As Integer = 600

    Private Const TabStripText As String = "AVDockableTabStripView"
    Private Const CollapseButtonText As String = "AVExpandCollapseButtonView"
    Private Const PageViewText As String = "AVSplitationPageView"
    Private Const DocumentViewText As String = "AVDocumentMainView"

    Private ReadOnly _panel As Control
    Private ReadOnly _log As Action(Of String)
    Private _host As AcroPdfHost
    Private _clsid As String

    Public Sub New(hostPanel As Control, log As Action(Of String))
        If hostPanel Is Nothing Then Throw New ArgumentNullException(NameOf(hostPanel))
        _panel = hostPanel
        _log = log
        _clsid = AcroPdfDetector.NormaliseClsid(AcroPdfDetector.ResolveClsid())
    End Sub

    ''' <summary>False when AcroPDF is not registered — the caller must then fall back or say so.</summary>
    Public ReadOnly Property IsAvailable As Boolean
        Get
            Return Not String.IsNullOrEmpty(_clsid)
        End Get
    End Property

    ''' <summary>The control's own version string, or Nothing. Available only after a first load.</summary>
    Public Function TryReadVersion() As String
        Return If(_host Is Nothing, Nothing, _host.TryReadVersion())
    End Function

    ''' <summary>
    ''' Loads the document and leaves it in the wanted state. Never throws — every failure path
    ''' returns a Romanian sentence the caller can put on screen.
    ''' </summary>
    Public Async Function ShowDocumentAsync(pdfPath As String) As Task(Of AcroPdfResult)
        Try
            If Not IsAvailable Then
                Return New AcroPdfResult(AcroPdfStatus.NotRegistered,
                                         "Controlul Adobe (AcroPDF) nu este înregistrat pe această mașină.")
            End If
            If String.IsNullOrWhiteSpace(pdfPath) OrElse Not File.Exists(pdfPath) Then
                Return New AcroPdfResult(AcroPdfStatus.FileMissing, "Documentul nu există pe disc.")
            End If

            Dim host As AcroPdfHost = EnsureHost()
            If host Is Nothing Then
                Return New AcroPdfResult(AcroPdfStatus.Failed,
                                         "Controlul Adobe nu a putut fi creat. Detalii în jurnalul de erori.")
            End If

            Report($"AcroPDF: încarc «{Path.GetFileName(pdfPath)}».")
            host.LoadFile(pdfPath)

            Await WakeLayoutAsync().ConfigureAwait(True)
            Dim collapsed As Boolean = Await CollapsePanesAsync().ConfigureAwait(True)

            Return New AcroPdfResult(AcroPdfStatus.Shown, "", collapsed)
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfSurface.ShowDocumentAsync", ex)
            Return New AcroPdfResult(AcroPdfStatus.Failed,
                                     "Documentul nu a putut fi afișat. Detalii în jurnalul de erori.")
        End Try
    End Function

    ''' <summary>
    ''' Empties the surface by DESTROYING the control; the next load recreates it. Setting <c>src</c>
    ''' to an empty string returns success and changes nothing on this build.
    ''' </summary>
    Public Sub Clear()
        Try
            Dim host As AcroPdfHost = _host
            _host = Nothing
            If host Is Nothing Then Return
            _panel.Controls.Remove(host)
            host.Dispose()
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfSurface.Clear", ex)
        End Try
    End Sub

    ' Creates the control on first use. AxHost needs a handle before GetOcx() returns anything.
    Private Function EnsureHost() As AcroPdfHost
        If _host IsNot Nothing Then Return _host
        Try
            Dim host As New AcroPdfHost(_clsid) With {.Dock = DockStyle.Fill, .Name = "axAcroPdf"}
            _panel.Controls.Add(host)
            Dim unused As IntPtr = host.Handle
            _host = host
            Return host
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfSurface.EnsureHost", ex)
            Return Nothing
        End Try
    End Function

    ' ── Step 2: the first layout ────────────────────────────────────────────────
    Private Async Function WakeLayoutAsync() As Task
        If Not IsLayoutDegenerate() Then Return
        AdobeWindowHosting.FocusWindow(_host.Handle)
        If Not IsLayoutDegenerate() Then
            Report("AcroPDF: aranjarea s-a făcut la focus.")
            Return
        End If
        ' The size change is the step that actually works (measured 06.08.2026).
        AdobeWindowHosting.NudgeRedraw(_host.Handle,
                                       New Rectangle(0, 0, _panel.ClientSize.Width, _panel.ClientSize.Height))
        Await Task.Delay(LayoutSettleMs).ConfigureAwait(True)
        If IsLayoutDegenerate() Then
            Report("AcroPDF: ATENȚIE — aranjarea a rămas degenerată; panoul poate arăta gol.")
        Else
            Report("AcroPDF: aranjarea s-a făcut la schimbarea de dimensiune.")
        End If
    End Function

    ' «Not laid out» has an unambiguous marker: the document view has no size.
    Private Function IsLayoutDegenerate() As Boolean
        Dim page As AdobeWindowNode = FindNode(Walk(), PageViewText)
        Return page Is Nothing OrElse page.Width <= 0 OrElse page.Height <= 0
    End Function

    ' ── Step 3: Adobe's own collapse ────────────────────────────────────────────
    Private Async Function CollapsePanesAsync() As Task(Of Boolean)
        Dim nodes As List(Of AdobeWindowNode) = Walk()
        Dim strip As AdobeWindowNode = FindNode(nodes, TabStripText)
        If strip Is Nothing Then
            Report("AcroPDF: banda de file nu există în arbore — nu am ce colapsa.")
            Return False
        End If

        ' Zero on BOTH axes is «not laid out», not «collapsed».
        If strip.Height <= 0 Then
            Report($"AcroPDF: banda e {strip.Width}x{strip.Height} — Adobe nu a aranjat încă; nu colapsez.")
            Return False
        End If

        If strip.Width > 0 Then
            If Not Await ClickCollapseAsync(nodes, strip).ConfigureAwait(True) Then Return False
        Else
            ' Already collapsed. The floating bar follows the collapse ACTION, not the STATE, so
            ' re-open and close again to make Adobe perform it in this session.
            Report("AcroPDF: deja colapsat — comut de două ori, ca acțiunea să se producă acum.")
            If Not Await ClickCollapseAsync(nodes, strip).ConfigureAwait(True) Then Return False
            Dim reopened As List(Of AdobeWindowNode) = Walk()
            Dim s2 As AdobeWindowNode = FindNode(reopened, TabStripText)
            If s2 Is Nothing Then Return False
            If Not Await ClickCollapseAsync(reopened, s2).ConfigureAwait(True) Then Return False
        End If

        ' The objective signal: the document view reaches the left edge and fills the panel.
        Dim page As AdobeWindowNode = FindNode(Walk(), PageViewText)
        Dim ok As Boolean = page IsNot Nothing AndAlso page.Bounds.X = 0 AndAlso page.Width > 0
        If ok Then
            Report($"AcroPDF: panouri colapsate — documentul ocupă {page.Width}x{page.Height} de la x=0.")
        Else
            Report("AcroPDF: ATENȚIE — după colapsare documentul NU a ajuns la marginea stângă. " &
                   "Un click sintetic peste graniță de proces nu e garantat onorat.")
        End If
        Return ok
    End Function

    Private Async Function ClickCollapseAsync(nodes As List(Of AdobeWindowNode),
                                              strip As AdobeWindowNode) As Task(Of Boolean)
        Dim button As AdobeWindowNode = nodes.
            Where(Function(n) String.Equals(n.Text, CollapseButtonText, StringComparison.OrdinalIgnoreCase)).
            OrderBy(Function(n) Math.Abs(n.Bounds.X - (strip.Bounds.X + strip.Width))).
            FirstOrDefault()
        If button Is Nothing Then
            Report("AcroPDF: butonul de colapsare nu există în arbore.")
            Return False
        End If
        AdobeWindowHosting.ClickCentre(button.Hwnd)
        Await Task.Delay(CollapseSettleMs).ConfigureAwait(True)
        Return True
    End Function

    ' ── Helpers ─────────────────────────────────────────────────────────────────
    Private Function Walk() As List(Of AdobeWindowNode)
        If _host Is Nothing OrElse Not _host.IsHandleCreated Then Return New List(Of AdobeWindowNode)()
        Return AdobeWindowProbe.Walk(_host.Handle, _panel.Handle, ProbeDepth)
    End Function

    Private Shared Function FindNode(nodes As List(Of AdobeWindowNode), text As String) As AdobeWindowNode
        Return nodes.FirstOrDefault(Function(n) String.Equals(n.Text, text, StringComparison.OrdinalIgnoreCase))
    End Function

    Private Sub Report(line As String)
        _log?.Invoke(line)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Clear()
    End Sub

End Class
