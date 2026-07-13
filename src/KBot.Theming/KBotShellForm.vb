Option Strict On
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Formular de bază pentru shell-uri fără chenar REDIMENSIONABILE (ex. MainForm).
''' Adaugă exact cele trei comportamente native pierdute de FormBorderStyle.None:
'''  1. banda de redimensionare de 8px logici pe margini/colțuri (WM_NCHITTEST);
'''  2. maximizarea limitată la zona de lucru a monitorului (WM_GETMINMAXINFO) —
'''     altfel fereastra maximizată ar acoperi taskbar-ul;
'''  3. drag / Aero snap — deja rezolvate prin NativeMethods.DragMove (KBotCaptionBar).
''' Tematizarea și colțurile rotunjite vin din KBotThemedForm + ThemeManager.Apply.
'''
''' Detaliu de mecanică: WM_NCHITTEST ajunge la CEL MAI ADÂNC control de sub cursor,
''' nu la formular — iar copiii andocați acoperă complet banda de margine. De aceea
''' fiecare control descendent e subclasat (NativeWindow) ca să întoarcă HTTRANSPARENT
''' în bandă; hit-test-ul cade astfel înapoi pe formular, care răspunde HT* corect.
''' Totul e conținut aici — formularele derivate nu au nimic de făcut.
''' </summary>
Public Class KBotShellForm
    Inherits KBotThemedForm

    ' Lățimea benzii de redimensionare, în px logici (@96dpi).
    Private Const ResizeBandLogical As Integer = 8

    ' Filtrele instalate pe descendenți (un NativeWindow per control cu handle viu).
    Private ReadOnly _edgeFilters As New Dictionary(Of Control, EdgeHitTestFilter)()

    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        Try
            HookTree(Me)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotShellForm.OnHandleCreated", ex)
        End Try
    End Sub

    'add ignore flag from step by step debugging
    <DebuggerStepThrough>
    Protected Overrides Sub WndProc(ByRef m As Message)
        If FormBorderStyle = FormBorderStyle.None Then
            If m.Msg = NativeMethods.WM_GETMINMAXINFO Then
                ' Întâi WinForms (setează ptMinTrackSize din MinimumSize), apoi noi
                ' suprascriem DOAR câmpurile de maximizare cu zona de lucru.
                MyBase.WndProc(m)
                NativeMethods.ApplyMinMaxInfo(m.LParam, Me)
                Return
            End If

            If m.Msg = NativeMethods.WM_NCHITTEST AndAlso WindowState = FormWindowState.Normal Then
                MyBase.WndProc(m)
                If m.Result.ToInt64() = NativeMethods.HTCLIENT Then
                    Dim hit As Integer = HitTestResizeBand(PointFromLParam(m.LParam))
                    If hit <> 0 Then m.Result = New IntPtr(hit)
                End If
                Return
            End If
        End If
        MyBase.WndProc(m)
    End Sub

    ' Punctul ecran (semnat, pentru multi-monitor cu coordonate negative) din lParam.
    Private Shared Function PointFromLParam(lParam As IntPtr) As Point
        Dim value As Long = lParam.ToInt64()
        Dim x As Integer = CShort(value And &HFFFF&)
        Dim y As Integer = CShort((value >> 16) And &HFFFF&)
        Return New Point(x, y)
    End Function

    ' True dacă punctul-ecran cade în banda de redimensionare a acestui formular.
    Friend Function IsInResizeBand(screenPoint As Point) As Boolean
        If FormBorderStyle <> FormBorderStyle.None OrElse WindowState <> FormWindowState.Normal Then
            Return False
        End If
        Return HitTestResizeBand(screenPoint) <> 0
    End Function

    ' Codul HT* pentru un punct-ecran aflat în bandă; 0 dacă e în afara ei.
    ' Colțurile au prioritate față de margini (banda orizontală ∩ banda verticală).
    Private Function HitTestResizeBand(screenPoint As Point) As Integer
        Dim pt As Point = PointToClient(screenPoint)
        Dim band As Integer = ThemeShapes.ScaleDpi(Me, ResizeBandLogical)

        Dim onLeft As Boolean = pt.X < band
        Dim onRight As Boolean = pt.X >= ClientSize.Width - band
        Dim onTop As Boolean = pt.Y < band
        Dim onBottom As Boolean = pt.Y >= ClientSize.Height - band

        If onTop AndAlso onLeft Then Return NativeMethods.HTTOPLEFT
        If onTop AndAlso onRight Then Return NativeMethods.HTTOPRIGHT
        If onBottom AndAlso onLeft Then Return NativeMethods.HTBOTTOMLEFT
        If onBottom AndAlso onRight Then Return NativeMethods.HTBOTTOMRIGHT
        If onLeft Then Return NativeMethods.HTLEFT
        If onRight Then Return NativeMethods.HTRIGHT
        If onTop Then Return NativeMethods.HTTOP
        If onBottom Then Return NativeMethods.HTBOTTOM
        Return 0
    End Function

    ' ── Instalarea filtrelor pe descendenți (recursiv + dinamic) ──────────────
    ' Controalele adăugate ulterior (ex. vederile lazy) sunt prinse prin ControlAdded.
    Private Sub HookTree(root As Control)
        For Each child As Control In root.Controls
            HookControl(child)
            HookTree(child)
        Next
        AddHandler root.ControlAdded, AddressOf OnDescendantAdded
    End Sub

    Private Sub HookControl(ctrl As Control)
        If _edgeFilters.ContainsKey(ctrl) Then Return
        Dim filter As New EdgeHitTestFilter(Me)
        _edgeFilters(ctrl) = filter
        If ctrl.IsHandleCreated Then filter.AssignHandle(ctrl.Handle)
        AddHandler ctrl.HandleCreated, AddressOf OnDescendantHandleCreated
        AddHandler ctrl.HandleDestroyed, AddressOf OnDescendantHandleDestroyed
        AddHandler ctrl.Disposed, AddressOf OnDescendantDisposed
    End Sub

    Private Sub OnDescendantAdded(sender As Object, e As ControlEventArgs)
        Try
            HookControl(e.Control)
            HookTree(e.Control)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotShellForm.OnDescendantAdded", ex)
        End Try
    End Sub

    Private Sub OnDescendantHandleCreated(sender As Object, e As EventArgs)
        Dim ctrl As Control = TryCast(sender, Control)
        Dim filter As EdgeHitTestFilter = Nothing
        If ctrl IsNot Nothing AndAlso _edgeFilters.TryGetValue(ctrl, filter) Then
            filter.AssignHandle(ctrl.Handle)
        End If
    End Sub

    Private Sub OnDescendantHandleDestroyed(sender As Object, e As EventArgs)
        Dim ctrl As Control = TryCast(sender, Control)
        Dim filter As EdgeHitTestFilter = Nothing
        If ctrl IsNot Nothing AndAlso _edgeFilters.TryGetValue(ctrl, filter) Then
            filter.ReleaseHandle()
        End If
    End Sub

    Private Sub OnDescendantDisposed(sender As Object, e As EventArgs)
        Dim ctrl As Control = TryCast(sender, Control)
        If ctrl IsNot Nothing Then _edgeFilters.Remove(ctrl)
    End Sub

    ''' <summary>
    ''' Subclasare per-control: în banda de redimensionare răspunde HTTRANSPARENT,
    ''' astfel hit-test-ul trece de copil și ajunge la formular (care întoarce HT*).
    ''' În afara benzii, comportament neschimbat.
    ''' </summary>
    Private NotInheritable Class EdgeHitTestFilter
        Inherits NativeWindow

        Private ReadOnly _shell As KBotShellForm

        Public Sub New(shell As KBotShellForm)
            _shell = shell
        End Sub

        Protected Overrides Sub WndProc(ByRef m As Message)
            If m.Msg = NativeMethods.WM_NCHITTEST Then
                Try
                    If _shell.IsInResizeBand(PointFromLParam(m.LParam)) Then
                        m.Result = New IntPtr(NativeMethods.HTTRANSPARENT)
                        Return
                    End If
                Catch ex As Exception
                    GlobalErrorLog.Write("KBotShellForm.EdgeHitTestFilter", ex)
                End Try
            End If
            MyBase.WndProc(m)
        End Sub
    End Class

End Class
