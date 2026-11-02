Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Windows.Forms

''' <summary>
''' The editing surface of <see cref="KBotRichTextEditor"/>: a <c>RichTextBox</c> that tells its
''' host when the view may have moved and when the wheel turned.
'''
''' <para>The editor draws its own <see cref="KBotScrollBar"/> (themed, dark scheme included), so the
''' native vertical bar is switched off. A native edit without bars has no <c>WM_VSCROLL</c> to
''' listen to and ignores the wheel, so the host is told <see cref="ViewChanged"/> from the
''' messages that can move the view, and gets the wheel as <see cref="WheelTurned"/> (the message is
''' NOT passed on: the host scrolls). The scroll position is read and written in PIXELS
''' (<c>EM_GETSCROLLPOS</c> / <c>EM_SETSCROLLPOS</c>), which stays exact with mixed font sizes.</para>
''' </summary>
<ToolboxItem(False)>
Friend NotInheritable Class KBotRichEdit
    Inherits RichTextBox

    Private Const WM_KEYDOWN As Integer = &H100
    Private Const WM_KEYUP As Integer = &H101
    Private Const WM_CHAR As Integer = &H102
    Private Const WM_MOUSEMOVE As Integer = &H200
    Private Const WM_LBUTTONDOWN As Integer = &H201
    Private Const WM_LBUTTONUP As Integer = &H202
    Private Const WM_MOUSEWHEEL As Integer = &H20A
    Private Const WM_PASTE As Integer = &H302
    Private Const WM_CUT As Integer = &H300
    Private Const WM_SIZE As Integer = &H5
    Private Const MK_LBUTTON As Long = &H1L
    Private Const EM_LINESCROLL As Integer = &HB6
    Private Const EM_SCROLLCARET As Integer = &HB7
    Private Const EM_GETSCROLLPOS As Integer = &H4DD   ' WM_USER + 221
    Private Const EM_SETSCROLLPOS As Integer = &H4DE   ' WM_USER + 222

    <StructLayout(LayoutKind.Sequential)>
    Private Structure ScrollPoint
        Public X As Integer
        Public Y As Integer
    End Structure

    <DllImport("user32.dll", CharSet:=CharSet.Auto, SetLastError:=False)>
    Private Shared Function SendMessage(hWnd As IntPtr, msg As Integer, wParam As IntPtr,
                                        ByRef lParam As ScrollPoint) As IntPtr
    End Function

    ''' <summary>The view MAY have moved: the host re-reads it and puts its bar back on it.</summary>
    Public Event ViewChanged As EventHandler

    ''' <summary>The wheel turned over the box (<c>Delta</c> as in a normal mouse wheel event).</summary>
    Public Event WheelTurned As MouseEventHandler

    ''' <summary>The vertical scroll position in pixels (0 = top of the document).</summary>
    Friend Property ScrollY As Integer
        Get
            If Not IsHandleCreated Then Return 0
            Dim k_pt As New ScrollPoint()
            SendMessage(Handle, EM_GETSCROLLPOS, IntPtr.Zero, k_pt)
            Return k_pt.Y
        End Get
        Set(k_value As Integer)
            If Not IsHandleCreated Then Return
            Dim k_pt As New ScrollPoint()
            SendMessage(Handle, EM_GETSCROLLPOS, IntPtr.Zero, k_pt)
            k_pt.Y = Math.Max(0, k_value)
            SendMessage(Handle, EM_SETSCROLLPOS, IntPtr.Zero, k_pt)
        End Set
    End Property

    ' WndProc stays unwrapped (house rule: the window message contract is not broken; the net is
    ' Application.ThreadException).
    Protected Overrides Sub WndProc(ByRef m As Message)
        If m.Msg = WM_MOUSEWHEEL Then
            ' High word as a SIGNED 16-bit number (CShort throws on a negative one: wheel down).
            Dim k_delta As Integer = ((CInt((m.WParam.ToInt64() >> 16) And &HFFFFL) Xor &H8000) - &H8000)
            RaiseEvent WheelTurned(Me, New MouseEventArgs(MouseButtons.None, 0, 0, 0, k_delta))
            Return
        End If
        MyBase.WndProc(m)
        ' A mouse move moves the view only while selecting (left button held).
        If m.Msg = WM_MOUSEMOVE AndAlso (m.WParam.ToInt64() And MK_LBUTTON) = 0 Then Return
        Select Case m.Msg
            Case WM_KEYDOWN, WM_KEYUP, WM_CHAR, WM_LBUTTONDOWN, WM_LBUTTONUP, WM_MOUSEMOVE,
                 WM_PASTE, WM_CUT, WM_SIZE, EM_SCROLLCARET, EM_LINESCROLL, EM_SETSCROLLPOS
                RaiseEvent ViewChanged(Me, EventArgs.Empty)
        End Select
    End Sub

End Class
