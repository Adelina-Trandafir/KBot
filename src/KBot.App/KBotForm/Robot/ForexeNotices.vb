Option Strict On
Imports KBot.Theming

''' <summary>
''' Slice 0086-02 -- the one notice several FOREXE flows share. It lived in the form
''' (<c>KbotForm.AratEsecul</c>); the flows that left the form need it too, so it is a plain
''' shared helper and the form's method forwards here.
''' </summary>
Friend NotInheritable Class ForexeNotices

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Shows why the last thing asked of the robot came back empty -- and STAYS QUIET when the
    ''' operator is the one who gave up (slice 0057: a cancel leaves <c>LastFailure</c> empty,
    ''' deliberately).
    ''' </summary>
    Friend Shared Sub RobotFailure(k_owner As IWin32Window, k_controller As ForexeController, k_whatStarted As String)
        Dim k_reason As String = k_controller.LastFailure
        If String.IsNullOrWhiteSpace(k_reason) Then Return
        KBotMessage.Show(k_owner, k_whatStarted & " nu a adus nimic: " & k_reason,
                         "FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Warning)
    End Sub

End Class
