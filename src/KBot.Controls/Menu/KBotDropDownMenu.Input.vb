Option Strict On
Imports System.Windows.Forms
Imports KBot.Common

' The keyboard and the clicks outside the menu (slice 0087). The menu windows never take the focus,
' so while the menu is open an application message filter reads the keys meant for the form
' underneath and routes them to the deepest open window, and closes the menu on a click anywhere
' that is not one of its windows.
Partial Class KBotDropDownMenu

    Private Const WM_KEYDOWN As Integer = &H100
    Private Const WM_CHAR As Integer = &H102
    Private Const WM_SYSKEYDOWN As Integer = &H104
    Private Const WM_SYSCHAR As Integer = &H106
    Private Const WM_LBUTTONDOWN As Integer = &H201
    Private Const WM_RBUTTONDOWN As Integer = &H204
    Private Const WM_MBUTTONDOWN As Integer = &H207
    Private Const WM_NCLBUTTONDOWN As Integer = &HA1
    Private Const WM_NCRBUTTONDOWN As Integer = &HA4
    Private Const WM_NCMBUTTONDOWN As Integer = &HA7

    ''' <summary>
    ''' One key while the menu is open. Every key is consumed (True): the form underneath must not
    ''' react to arrows or Enter meant for the menu.
    ''' </summary>
    Friend Function HandleKey(key As Keys) As Boolean
        If _windows.Count = 0 Then Return False
        Dim deepest As KBotMenuWindow = _windows(_windows.Count - 1)
        Select Case key
            Case Keys.Escape
                CloseFrom(If(_windows.Count > 1, _windows.Count - 1, 0))
            Case Keys.Up
                deepest.MoveHighlight(-1)
            Case Keys.Down
                deepest.MoveHighlight(1)
            Case Keys.Home
                deepest.HighlightEdge(first:=True)
            Case Keys.End
                deepest.HighlightEdge(first:=False)
            Case Keys.Right
                deepest.OpenHighlightedSubmenu(selectFirst:=True)
            Case Keys.Left
                If _windows.Count > 1 Then CloseFrom(_windows.Count - 1)
            Case Keys.Enter, Keys.Space
                deepest.ActivateHighlighted()
            Case Keys.Menu, Keys.LMenu, Keys.RMenu, Keys.Tab
                CloseFrom(0)
        End Select
        Return True
    End Function

    ''' <summary>Reads the thread's messages while the menu is open. Installed / removed by the menu.</summary>
    Private NotInheritable Class MenuMessageFilter
        Implements IMessageFilter

        Private ReadOnly _menu As KBotDropDownMenu

        Public Sub New(menu As KBotDropDownMenu)
            _menu = menu
        End Sub

        Public Function PreFilterMessage(ByRef m As Message) As Boolean Implements IMessageFilter.PreFilterMessage
            Try
                Select Case m.Msg
                    Case WM_KEYDOWN, WM_SYSKEYDOWN
                        ' Slice 0000-31: a held menu is a still picture; the keys belong to whoever has the focus.
                        If KBot.Theming.KBotPopupGuard.KeepOpen Then Return False
                        Return _menu.HandleKey(CType(CInt(m.WParam.ToInt64() And &HFFFF), Keys))
                    Case WM_CHAR, WM_SYSCHAR
                        Return _menu.IsOpen AndAlso Not KBot.Theming.KBotPopupGuard.KeepOpen
                    Case WM_LBUTTONDOWN, WM_RBUTTONDOWN, WM_MBUTTONDOWN,
                         WM_NCLBUTTONDOWN, WM_NCRBUTTONDOWN, WM_NCMBUTTONDOWN
                        If _menu.OwnsWindow(m.HWnd) Then Return False
                        ' Slice 0000-31: a click away does not close a held menu.
                        If KBot.Theming.KBotPopupGuard.KeepOpen Then
                            KBot.Theming.KBotPopupGuard.CloseOnRelease(AddressOf _menu.CloseAll)
                            Return False
                        End If
                        ' A press on the button that opened the menu only closes it: letting the
                        ' click through would open it again at once.
                        Dim onButton As Boolean = _menu.IsDropDownButton(m.HWnd)
                        _menu.CloseFrom(0)
                        Return onButton
                End Select
                Return False
            Catch ex As Exception
                ' UI boundary (message pump): log and let the message through.
                GlobalErrorLog.Write("KBotDropDownMenu.MenuMessageFilter.PreFilterMessage", ex)
                Return False
            End Try
        End Function
    End Class

End Class
