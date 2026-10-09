Option Strict On
Imports System.Windows.Forms

''' <summary>
''' Everything one message box needs, as plain data -- what the dialog shows
''' (<see cref="KBotMessageBox.Show"/>) and what the debug message catalog stores per message.
''' </summary>
Public NotInheritable Class KBotMessageSpec

    Public Property Kind As KBotMsgKind = KBotMsgKind.Info
    Public Property Buttons As KBotMsgButtons = KBotMsgButtons.OK
    Public Property Caption As String = String.Empty
    Public Property Text As String = String.Empty

    ''' <summary>A heading above the text (simple HTML allowed); empty = none.</summary>
    Public Property Header As String = String.Empty

    ''' <summary>When the X of the title bar shows; <c>Auto</c> = only when the message has a way out.</summary>
    Public Property CloseButton As KBotMsgClose = KBotMsgClose.Auto

    ''' <summary>Caption of one extra button, shown left of the standard ones; empty = none.</summary>
    Public Property ExtraButton As String = String.Empty

    ''' <summary>1-based, among the standard buttons (the extra button is never the default).</summary>
    Public Property DefaultButton As Integer = 1

    ''' <summary>Stays above other top-most windows (the interactive tutorials need it).</summary>
    Public Property TopMost As Boolean

    ''' <summary>The spec of a classic <c>MessageBox.Show</c> call.</summary>
    Public Shared Function FromWinForms(k_text As String, k_caption As String,
                                        k_buttons As MessageBoxButtons, k_icon As MessageBoxIcon,
                                        k_default As MessageBoxDefaultButton) As KBotMessageSpec
        Dim k_spec As New KBotMessageSpec() With {
            .Text = If(k_text, String.Empty),
            .Caption = If(k_caption, String.Empty),
            .Kind = KindOf(k_icon),
            .Buttons = ButtonsOf(k_buttons),
            .DefaultButton = If(k_default = MessageBoxDefaultButton.Button3, 3,
                                If(k_default = MessageBoxDefaultButton.Button2, 2, 1))
        }
        Return k_spec
    End Function

    Public Shared Function KindOf(k_icon As MessageBoxIcon) As KBotMsgKind
        ' Hand/Stop/Error share a value, so do Exclamation/Warning and Asterisk/Information.
        Select Case k_icon
            Case MessageBoxIcon.Error : Return KBotMsgKind.Error
            Case MessageBoxIcon.Warning : Return KBotMsgKind.Warning
            Case MessageBoxIcon.Information : Return KBotMsgKind.Info
            Case MessageBoxIcon.Question : Return KBotMsgKind.Question
            Case Else : Return KBotMsgKind.None
        End Select
    End Function

    Public Shared Function ButtonsOf(k_buttons As MessageBoxButtons) As KBotMsgButtons
        Select Case k_buttons
            Case MessageBoxButtons.OKCancel : Return KBotMsgButtons.OKCancel
            Case MessageBoxButtons.AbortRetryIgnore : Return KBotMsgButtons.AbortRetryIgnore
            Case MessageBoxButtons.YesNoCancel : Return KBotMsgButtons.YesNoCancel
            Case MessageBoxButtons.YesNo : Return KBotMsgButtons.YesNo
            Case MessageBoxButtons.RetryCancel : Return KBotMsgButtons.RetryCancel
            Case Else : Return KBotMsgButtons.OK
        End Select
    End Function

    Public Function Clone() As KBotMessageSpec
        Return DirectCast(MemberwiseClone(), KBotMessageSpec)
    End Function

End Class

''' <summary>What the operator did: the standard answer, or the extra button.</summary>
Public NotInheritable Class KBotMessageResult

    ''' <summary>The standard answer; <c>DialogResult.None</c> when the extra button was pressed.</summary>
    Public ReadOnly Property Result As DialogResult
    Public ReadOnly Property ExtraClicked As Boolean

    Public Sub New(k_result As DialogResult, k_extraClicked As Boolean)
        Result = k_result
        ExtraClicked = k_extraClicked
    End Sub

End Class
