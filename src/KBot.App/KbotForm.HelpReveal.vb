Option Strict On
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

' Slice 0000-31 -- the MENIU menu as a guided tour shows it (step «reveal: menu»): opened under the
' button, with the two rows that appear only sometimes («Jurnal activitate», «(!) Operatiuni
' necorelate») shown, and held open (KBotPopupGuard) while the tour bubble takes the focus. The step
' ends -> HelpRevealEnd closes it and puts the rows back.
Partial Public Class KbotForm
    Implements IKBotHelpReveal

    Private _helpMenuReveal As Boolean
    Private _helpUncorrelatedWas As Boolean

    ''' <summary>
    ''' <c>reveal: menu</c> on <c>btnMeniu</c>: opens the menu for the tour. UI boundary (called from the
    ''' tour): logs and answers False on a failure, leaving nothing open.
    ''' </summary>
    Public Function HelpReveal(target As Control, part As String, ByRef area As Rectangle, ByRef note As String) As Boolean Implements IKBotHelpReveal.HelpReveal
        Try
            If Not ReferenceEquals(target, btnMeniu) OrElse Not String.Equals(part, "menu", StringComparison.Ordinal) Then Return False
            If _helpMenuReveal Then Return False
            _helpMenuReveal = True
            KBotPopupGuard.Hold()
            menuNou.ShowBelow(btnMeniu)
            area = menuNou.OpenBounds
            note = "Meniul e deschis de tur, cu rândurile care se văd doar uneori: «(!) Operațiuni necorelate» și «Jurnal activitate». Le vezi acum pe amândouă, chiar dacă în mod obișnuit lipsesc."
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.HelpReveal", ex)
            HelpRevealEnd()
            Return False
        End Try
    End Function

    Public Sub HelpRevealEnd() Implements IKBotHelpReveal.HelpRevealEnd
        Try
            If Not _helpMenuReveal Then Return
            _helpMenuReveal = False
            menuNou.Close()
            Dim item As KBotMenuItem = menuNou.Items.FirstOrDefault(Function(i) String.Equals(i.Key, UncorrelatedMenuKey, StringComparison.Ordinal))
            If item IsNot Nothing Then item.Visible = _helpUncorrelatedWas
            KBotPopupGuard.Release()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.HelpRevealEnd", ex)
        End Try
    End Sub

End Class
