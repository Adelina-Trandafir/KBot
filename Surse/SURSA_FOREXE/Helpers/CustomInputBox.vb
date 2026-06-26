Imports System.Windows.Forms

Public Class CustomInputBox

    ' Configurația
    Public Property IsNumericOnly As Boolean = False

    ' [NOU] Limita de caractere pentru text
    Public Property FixedLength As Integer = 0

    ' Pentru intervale numerice (Valori, nu lungime)
    Public Property MinValue As Double? = Nothing
    Public Property MaxValue As Double? = Nothing

    Public ReadOnly Property UserInput As String
        Get
            Return txtInput.Text
        End Get
    End Property

    Public Sub New(title As String, prompt As String, Optional defaultValue As String = "")
        InitializeComponent()
        Me.Text = title
        Me.lblPrompt.Text = prompt
        Me.txtInput.Text = defaultValue
        Me.lblError.Visible = False
        KBotTheme.ApplyTheme(Me)
    End Sub

    Public Sub SetMask(mask As String)
        If Not String.IsNullOrEmpty(mask) Then
            txtInput.Mask = mask
            ' IncludeLiterals este important dacă masca are caractere fixe (ex: puncte)
            txtInput.TextMaskFormat = MaskFormat.IncludeLiterals
        End If
    End Sub

    ' [CRITIC] Aici aplicăm limitarea la tastare
    Private Sub CustomInputBox_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Dacă avem o lungime fixă setată prin LEN și NU avem o mască activă
        ' (MaskedTextBox gestionează propria lungime dacă are mască)
        If FixedLength > 0 AndAlso String.IsNullOrEmpty(txtInput.Mask) Then
            ' MaxLength blochează automat tastarea când se atinge limita
            ' Este echivalentul perfect și optimizat al verificării în OnKeyPress
            txtInput.MaxLength = FixedLength
        End If
    End Sub

    Private Sub btnOk_Click(sender As Object, e As EventArgs) Handles btnOk.Click
        If ValidateInput() Then
            Me.DialogResult = DialogResult.OK
            Me.Close()
        End If
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Function ValidateInput() As Boolean
        Dim val As String = txtInput.Text.Trim()

        ' 1. Validare NUMERICĂ (doar dacă Type=Numeric)
        If IsNumericOnly Then
            Dim n As Double
            ' Încercăm să parsăm valoarea
            If Not Double.TryParse(val, n) Then
                ShowError("Introduceți o valoare numerică validă.")
                Return False
            End If

            ' Validare valori (Min/Max numeric)
            If MinValue.HasValue AndAlso n < MinValue.Value Then
                ShowError($"Valoarea minimă este {MinValue.Value}.")
                Return False
            End If
            If MaxValue.HasValue AndAlso n > MaxValue.Value Then
                ShowError($"Valoarea maximă este {MaxValue.Value}.")
                Return False
            End If
        Else
            ' 2. Validare TEXT (Alfanumeric)
            ' Dacă avem Len setat, ne asigurăm că nu s-a depășit (paste protection)
            If FixedLength > 0 Then
                If val.Length > FixedLength Then
                    ShowError($"Textul este prea lung (Max {FixedLength} caractere).")
                    Return False
                End If
                ' Dacă vrei validare EXACTĂ (să nu fie nici mai puțin), decomentează:
                ' If val.Length <> FixedLength Then
                '     ShowError($"Trebuie să introduceți exact {FixedLength} caractere.")
                '     Return False
                ' End If
            End If
        End If

        lblError.Visible = False
        Return True
    End Function

    Private Sub ShowError(msg As String)
        lblError.Text = msg
        lblError.Visible = True
        txtInput.Focus()
        If String.IsNullOrEmpty(txtInput.Mask) Then txtInput.SelectAll()
    End Sub

    ' Blocare caractere non-numerice la tastare (doar pentru Numeric pur, fără mască)
    Private Sub txtInput_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtInput.KeyPress
        If IsNumericOnly AndAlso String.IsNullOrEmpty(txtInput.Mask) Then
            ' Permitem doar cifre, control (backspace) și separator zecimal
            If Not Char.IsDigit(e.KeyChar) AndAlso Not Char.IsControl(e.KeyChar) AndAlso e.KeyChar <> "."c AndAlso e.KeyChar <> ","c Then
                e.Handled = True
            End If
        ElseIf FixedLength Then

        End If
    End Sub

End Class