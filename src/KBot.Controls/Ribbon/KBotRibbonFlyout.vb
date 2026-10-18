Option Strict On
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' The temporary panel of a folded <see cref="KBotRibbon"/> (slice 0114-24): a click on a tab while the ribbon is folded shows that
''' tab's groups here, over the page under the ribbon, like Office's collapsed ribbon. It is a child of the form, outside the
''' docking, so the page below does not move. The ribbon owns everything: the layout, the drawing, the hot / pressed button and the
''' field controls (which stand in this panel while it is open); this class only forwards paint and mouse to the ribbon.
''' </summary>
Friend NotInheritable Class KBotRibbonFlyout
    Inherits Control

    Private ReadOnly _owner As KBotRibbon

    Friend Sub New(k_owner As KBotRibbon)
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
        SetStyle(ControlStyles.Selectable, False)
        _owner = k_owner
        Visible = False
    End Sub

    Protected Overrides Sub OnPaint(k_e As PaintEventArgs)
        Try
            _owner.PaintFlyoutPanel(k_e.Graphics, ClientSize)
        Catch ex As Exception
            ' UI boundary (OnPaint): log and swallow.
            GlobalErrorLog.Write("KBotRibbonFlyout.OnPaint", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseMove(k_e As MouseEventArgs)
        MyBase.OnMouseMove(k_e)
        _owner.FlyoutMouseMove(k_e.Location)
    End Sub

    Protected Overrides Sub OnMouseLeave(k_e As EventArgs)
        MyBase.OnMouseLeave(k_e)
        _owner.FlyoutMouseLeave()
    End Sub

    Protected Overrides Sub OnMouseDown(k_e As MouseEventArgs)
        MyBase.OnMouseDown(k_e)
        If k_e.Button = MouseButtons.Left Then _owner.FlyoutMouseDown(k_e.Location)
    End Sub

    Protected Overrides Sub OnMouseUp(k_e As MouseEventArgs)
        MyBase.OnMouseUp(k_e)
        If k_e.Button = MouseButtons.Left Then _owner.FlyoutMouseUp(k_e.Location)
    End Sub

    ''' <summary>
    ''' Watches the thread's mouse presses and the Escape key while the fly-out is open. A press is never eaten (the ribbon's tab
    ''' strip, or whatever was clicked, still gets it); Escape is eaten when it closed the fly-out, so it does not also close a page.
    ''' </summary>
    Friend NotInheritable Class OutsideClickFilter
        Implements IMessageFilter

        Private Const WM_KEYDOWN As Integer = &H100
        Private Const WM_LBUTTONDOWN As Integer = &H201
        Private Const WM_RBUTTONDOWN As Integer = &H204
        Private Const WM_MBUTTONDOWN As Integer = &H207
        Private Const WM_NCLBUTTONDOWN As Integer = &HA1
        Private Const WM_NCRBUTTONDOWN As Integer = &HA4
        Private Const WM_NCMBUTTONDOWN As Integer = &HA7
        Private Const VK_ESCAPE As Integer = &H1B

        Private ReadOnly _owner As KBotRibbon

        Friend Sub New(k_owner As KBotRibbon)
            _owner = k_owner
        End Sub

        Public Function PreFilterMessage(ByRef k_m As Message) As Boolean Implements IMessageFilter.PreFilterMessage
            Select Case k_m.Msg
                Case WM_LBUTTONDOWN, WM_RBUTTONDOWN, WM_MBUTTONDOWN, WM_NCLBUTTONDOWN, WM_NCRBUTTONDOWN, WM_NCMBUTTONDOWN
                    _owner.FlyoutOutsideClick(k_m.HWnd)
                Case WM_KEYDOWN
                    If k_m.WParam.ToInt32() = VK_ESCAPE Then Return _owner.FlyoutEscape()
            End Select
            Return False
        End Function
    End Class
End Class
