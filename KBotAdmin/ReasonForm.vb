Option Strict On

''' <summary>Asks for the reason of a rejection (5 to 2000 characters); the applicant receives it by e-mail (slice ME01).</summary>
Public Class ReasonForm

    Public Sub New(k_prompt As String)
        InitializeComponent()
        Icon = AdminIcon.Value
        lblPrompt.Text = k_prompt
    End Sub

    ''' <summary>The reason typed, whitespace folded to single spaces.</summary>
    Public ReadOnly Property Reason As String
        Get
            Return String.Join(" ", txtReason.Text.Split(New Char() {" "c, ControlChars.Cr, ControlChars.Lf, ControlChars.Tab},
                                                          StringSplitOptions.RemoveEmptyEntries))
        End Get
    End Property

    Private Sub BtnOk_Click(k_sender As Object, e As EventArgs) Handles btnOk.Click
        Try
            If Reason.Length < 5 Then
                KBotMessage.Show("Scrieți motivul (cel puțin 5 caractere): îl primește solicitantul.", "Motiv", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("ReasonForm.BtnOk_Click", ex)
        End Try
    End Sub

End Class
