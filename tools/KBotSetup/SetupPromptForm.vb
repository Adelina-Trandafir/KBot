Imports System.Windows.Forms

''' <summary>
''' Dialog de configurare API afișat de installer O SINGURĂ DATĂ (când variabilele
''' de mediu KBOT_API_BASE_URL / KBOT_API_KEY lipsesc). Nu scrie niciun fișier —
''' valorile sunt persistate de installer în variabilele de mediu Windows.
''' Controalele sunt declarate în SetupPromptForm.Designer.vb (regula K-BOT: design-time).
''' </summary>
Public NotInheritable Class SetupPromptForm

    ''' <summary>URL-ul de bază validat (absolut http/https). Valid doar după DialogResult.OK.</summary>
    Public ReadOnly Property BaseUrl As String
        Get
            Return txtBaseUrl.Text.Trim()
        End Get
    End Property

    ''' <summary>Cheia API introdusă. Validă doar după DialogResult.OK.</summary>
    Public ReadOnly Property ApiKey As String
        Get
            Return txtApiKey.Text.Trim()
        End Get
    End Property

    ' btnOk NU are DialogResult în designer: validăm întâi, iar închiderea (DialogResult.OK)
    ' o setăm manual doar când datele sunt valide, ca dialogul să rămână deschis pe eroare.
    Private Sub btnOk_Click(sender As Object, e As EventArgs) Handles btnOk.Click
        Dim url As String = txtBaseUrl.Text.Trim()
        Dim key As String = txtApiKey.Text.Trim()

        Dim parsed As Uri = Nothing
        If Not Uri.TryCreate(url, UriKind.Absolute, parsed) OrElse
           (parsed.Scheme <> Uri.UriSchemeHttp AndAlso parsed.Scheme <> Uri.UriSchemeHttps) Then
            MessageBox.Show(Me,
                "URL-ul API nu este valid. Trebuie să fie absolut, cu http:// sau https:// (ex. http://server:5008/).",
                "K-BOT Setup", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtBaseUrl.Focus()
            Return
        End If

        If String.IsNullOrWhiteSpace(key) Then
            MessageBox.Show(Me, "Cheia API nu poate fi goală.",
                            "K-BOT Setup", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtApiKey.Focus()
            Return
        End If

        ' Valide — închidem cu OK (setarea DialogResult închide dialogul modal).
        Me.DialogResult = DialogResult.OK
    End Sub

End Class
