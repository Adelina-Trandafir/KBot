Option Strict On

''' <summary>
''' Slice 0000-31: lets the popups of the application (menus, filter menus, calendars, drop-down
''' lists) stay open when the operator clicks away or another window takes the focus. Held while the
''' help capture tool is on (the operator sets up an open menu, then presses «Captureaza») and while a
''' guided tour shows a menu. Every popup asks <see cref="KeepOpen"/> where it would close itself, and
''' when it must stay it leaves a closer with <see cref="CloseOnRelease"/>: once the guard is released
''' the popups that were kept open close, as they would have the moment the focus left them.
''' Counted, so two holders can overlap.
''' </summary>
Public NotInheritable Class KBotPopupGuard

    Private Shared _holds As Integer
    Private Shared ReadOnly _closers As New List(Of Action)()

    Private Sub New()
    End Sub

    ''' <summary>True while at least one holder keeps the popups open.</summary>
    Public Shared ReadOnly Property KeepOpen As Boolean
        Get
            Return _holds > 0
        End Get
    End Property

    ''' <summary>Starts keeping popups open. Pair with <see cref="Release"/>.</summary>
    Public Shared Sub Hold()
        _holds += 1
    End Sub

    ''' <summary>
    ''' Ends one hold. When the last one ends, every popup that stayed open because of it is closed.
    ''' A release without a hold is ignored.
    ''' </summary>
    Public Shared Sub Release()
        If _holds = 0 Then Return
        _holds -= 1
        If _holds > 0 Then Return
        Dim pending As List(Of Action) = _closers.ToList()
        _closers.Clear()
        For Each closer As Action In pending
            Try
                closer()
            Catch ex As Exception
                ' A popup that cannot close itself must not stop the others.
                KBot.Common.GlobalErrorLog.Write("KBotPopupGuard.Release", ex)
            End Try
        Next
    End Sub

    ''' <summary>
    ''' A popup that stayed open because of the guard leaves here what closes it later (it must be
    ''' safe to call when the popup is already closed or disposed). The same closer is kept once.
    ''' </summary>
    Public Shared Sub CloseOnRelease(closer As Action)
        ArgumentNullException.ThrowIfNull(closer)
        If Not _closers.Contains(closer) Then _closers.Add(closer)
    End Sub

End Class
