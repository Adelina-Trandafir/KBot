Option Strict On
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' The entry point of K-BOT's own message box (the window is <see cref="KBotMessageBoxForm"/>).
'''
''' <para><see cref="Present"/> has the shape of <see cref="KBotMessage.MessagePresenter"/>:
''' <c>KBotMessage.Presenter = AddressOf KBotMessageBox.Present</c> (done once, at start-up) turns
''' every <c>KBotMessage.Show</c> in the solution into this window.</para>
''' <para><see cref="Show"/> takes a <see cref="KBotMessageSpec"/> instead and can carry the extra
''' button; the debug message catalog uses it for its preview.</para>
''' </summary>
Public NotInheritable Class KBotMessageBox

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Installs the K-BOT window as the box behind every <c>KBotMessage.Show</c>. Idempotent.
    ''' </summary>
    Public Shared Sub Install()
        KBotMessage.Presenter = AddressOf Present
    End Sub

    ''' <summary>Drop-in for <c>MessageBox.Show</c>; see <see cref="KBotMessage.MessagePresenter"/>.</summary>
    Public Shared Function Present(k_owner As IWin32Window, k_text As String, k_caption As String,
                                   k_buttons As MessageBoxButtons, k_icon As MessageBoxIcon,
                                   k_defaultButton As MessageBoxDefaultButton,
                                   k_topMost As Boolean, k_extras As MessageExtras) As DialogResult
        Dim k_spec As KBotMessageSpec = KBotMessageSpec.FromWinForms(k_text, k_caption, k_buttons, k_icon, k_defaultButton)
        k_spec.TopMost = k_topMost
        If k_extras IsNot Nothing Then
            k_spec.ExtraButton = If(k_extras.ExtraButton, String.Empty)
            k_spec.Header = If(k_extras.Header, String.Empty)
            Dim k_close As KBotMsgClose
            If [Enum].TryParse(k_extras.CloseButton, True, k_close) Then k_spec.CloseButton = k_close
        End If
        Dim k_answer As KBotMessageResult = Show(k_owner, k_spec)
        KBotMessage.LastExtraClicked = k_answer.ExtraClicked
        Return k_answer.Result
    End Function

    ''' <summary>
    ''' Opens the box and waits. Safe from any thread: when the owner lives on another thread the
    ''' call is carried over to it, like the native box which does not care.
    ''' </summary>
    Public Shared Function Show(k_owner As IWin32Window, k_spec As KBotMessageSpec) As KBotMessageResult
        ArgumentNullException.ThrowIfNull(k_spec)
        Try
            Dim k_ctl As Control = TryCast(k_owner, Control)
            If k_ctl IsNot Nothing AndAlso k_ctl.IsHandleCreated AndAlso k_ctl.InvokeRequired Then
                Return DirectCast(k_ctl.Invoke(New Func(Of KBotMessageResult)(Function() ShowHere(k_owner, k_spec))), KBotMessageResult)
            End If
            Return ShowHere(k_owner, k_spec)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotMessageBox.Show", ex)
            Throw
        End Try
    End Function

    Private Shared Function ShowHere(k_owner As IWin32Window, k_spec As KBotMessageSpec) As KBotMessageResult
        Using k_form As New KBotMessageBoxForm(k_spec)
            Dim k_answer As DialogResult = If(k_owner Is Nothing, k_form.ShowDialog(), k_form.ShowDialog(k_owner))
            If k_form.ExtraClicked Then Return New KBotMessageResult(DialogResult.None, True)
            Return New KBotMessageResult(k_answer, False)
        End Using
    End Function

End Class
