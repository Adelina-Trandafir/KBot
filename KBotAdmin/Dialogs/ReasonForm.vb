Option Strict On

''' <summary>Asks for the reason of a rejection (5 to 2000 characters); the applicant receives it by e-mail (slice ME01).</summary>
Public Class ReasonForm

    Private _tooShortMessage As String = "Scrieți motivul (cel puțin 5 caractere): îl primește solicitantul."

    Public Sub New(k_prompt As String)
        InitializeComponent()
        Icon = AdminIcon.Value
        lblPrompt.Text = k_prompt
    End Sub

    ''' <summary>
    ''' The same box worded for another use (ME02-02: the reason of a backup). The Designer carries the wording of the
    ''' rejection of a request; this overload replaces every text that says so.
    ''' </summary>
    Public Sub New(k_prompt As String, k_title As String, k_okText As String, k_okTip As String, k_fieldTip As String, k_tooShortMessage As String)
        Me.New(k_prompt)
        Text = k_title
        btnOk.Text = k_okText
        tips.SetToolTipHeader(btnOk, k_okText)
        tips.SetToolTipText(btnOk, k_okTip)
        tips.SetToolTipText(txtReason, k_fieldTip)
        btnCancel.Text = "Renunță"
        tips.SetToolTipText(btnCancel, "Închide fereastra fără să facă nimic.")
        _tooShortMessage = k_tooShortMessage
        ' The backup prompt is longer than the one-line rejection prompt of the Designer: give the label room.
        tlyMain.RowStyles(0).Height = 104.0F
        ClientSize = New Size(ClientSize.Width, ClientSize.Height + 52)
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
                KBotMessage.Show(_tooShortMessage, "Motiv", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("ReasonForm.BtnOk_Click", ex)
        End Try
    End Sub

End Class
