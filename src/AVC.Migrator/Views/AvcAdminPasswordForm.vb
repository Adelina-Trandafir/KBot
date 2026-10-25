Imports KBot.Common

''' <summary>
''' Asks for the password of the server administrator when the registry is read: the connection itself uses the default host, port and
''' user (<see cref="AvcSettings"/>). The password is kept for this run only - never saved, never logged.
''' Every control is declared in the designer file; this class only wires behaviour.
''' </summary>
Public Class AvcAdminPasswordForm

    Private Const AppTitle As String = "Migrare Avacont"

    ''' <summary>What the operator typed; valid after the dialog returned <see cref="DialogResult.OK"/>.</summary>
    Public ReadOnly Property Password As String
        Get
            Return txtParola.Text
        End Get
    End Property

    ''' <param name="k_user">The administrator account, shown in the prompt.</param>
    ''' <param name="k_host">The server, shown in the prompt.</param>
    Friend Sub New(k_user As String, k_host As String)
        InitializeComponent()
        lblIntro.Text = $"Parola contului «{k_user}» de pe serverul {k_host}."
    End Sub

    Private Sub BtnOk_Click(sender As Object, e As EventArgs) Handles btnOk.Click
        Try
            If txtParola.Text.Length = 0 Then
                KBotMessage.Show("Introduceți parola de Admin a serverului.", AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                txtParola.Focus()
                Return
            End If
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("AvcAdminPasswordForm.Ok", ex)
        End Try
    End Sub

End Class
