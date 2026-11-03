Option Strict On

''' <summary>
''' Asks for the passphrase of the local backup (slice ME02). When <c>k_create</c> is True it is a NEW store: the passphrase is
''' typed twice and the dialog says plainly that a lost passphrase loses the copies. Otherwise it only unlocks.
''' </summary>
Public Class PassphraseForm

    Private ReadOnly _create As Boolean

    Public Sub New(k_prompt As String, k_create As Boolean)
        InitializeComponent()
        Icon = AdminIcon.Value
        _create = k_create
        lblPrompt.Text = k_prompt
        If k_create Then
            lblWarn.Text = "Fără parolă copiile locale nu se mai pot citi, iar parola nu se poate recupera. Păstrați-o în managerul de parole înainte să continuați."
            btnOk.Text = "Creează"
        Else
            lblWarn.Text = String.Empty
            lblConfirm.Visible = False
            txtConfirm.Visible = False
            tlyMain.RowStyles(2).Height = 0.0F
            btnOk.Text = "Deschide"
        End If
        chkRemember.Checked = True
    End Sub

    Public ReadOnly Property Passphrase As String
        Get
            Return txtPass.Text
        End Get
    End Property

    Public ReadOnly Property RememberKey As Boolean
        Get
            Return chkRemember.Checked
        End Get
    End Property

    Private Sub BtnOk_Click(k_sender As Object, e As EventArgs) Handles btnOk.Click
        Try
            If txtPass.Text.Length < 8 Then
                KBotMessage.Show("Parola trebuie să aibă cel puțin 8 caractere.", "Parola copiilor locale", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            If _create AndAlso txtPass.Text <> txtConfirm.Text Then
                KBotMessage.Show("Cele două parole nu sunt la fel.", "Parola copiilor locale", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("PassphraseForm.BtnOk_Click", ex)
        End Try
    End Sub

End Class
