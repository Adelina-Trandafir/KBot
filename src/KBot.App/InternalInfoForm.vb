Option Strict On
Imports System.Text
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Fereastră-instrument NEMODALĂ care arată toate câmpurile unui rând de arbore
''' (AngajamentTreeInfo), inclusiv cele nouă flag-uri Are*, pentru angajamentul selectat.
''' MainForm o deschide cu Show (nu ShowDialog) și îi împinge contextul la fiecare
''' selecție din arbore; butonul «Reîmprospătează» re-citește selecția curentă prin
''' provider-ul primit la construcție. Închiderea/tragerea sunt pe KBotCaptionBar.
''' </summary>
Public Class InternalInfoForm

    ' Lățimea etichetei în coloana câmp/valoare (monospațiat) — «Dată definitivare» = 17.
    Private Const LABEL_WIDTH As Integer = 18

    ' Sursă a selecției curente din MainForm; butonul de refresh o cheamă din nou.
    Private ReadOnly _provider As Func(Of AngajamentTreeInfo)

    Public Sub New(provider As Func(Of AngajamentTreeInfo))
        InitializeComponent()
        _provider = provider
        Try
            capBar.IconImage = My.Resources.kbot_64
        Catch ex As Exception
            ' Iconița e cosmetică — o absență nu trebuie să împiedice deschiderea.
            GlobalErrorLog.Write("InternalInfoForm.New", ex)
        End Try
    End Sub

    ''' <summary>Reîncarcă afișajul pentru un context de arbore (Nothing = fără selecție).</summary>
    Public Sub ShowInfo(info As AngajamentTreeInfo)
        Try
            If info Is Nothing OrElse String.IsNullOrEmpty(info.CodAngajament) Then
                lblHeader.Text = "Niciun angajament selectat"
                txtInfo.Text = "Selectați un angajament în arbore pentru a-i vedea datele interne."
                Return
            End If

            lblHeader.Text = info.CodAngajament &
                             If(String.IsNullOrEmpty(info.Descriere), String.Empty, "  —  " & info.Descriere)
            txtInfo.Text = Compose(info)
            txtInfo.SelectionStart = 0
            txtInfo.SelectionLength = 0
        Catch ex As Exception
            ' Boundary UI: nu rearuncăm dintr-o fereastră deschisă — logăm și înghițim.
            GlobalErrorLog.Write("InternalInfoForm.ShowInfo", ex)
        End Try
    End Sub

    ' Compune blocul câmp/valoare. Booleeni -> Da/Nu; date -> yyyy-MM-dd; lipsă -> «—».
    Private Shared Function Compose(info As AngajamentTreeInfo) As String
        Dim sb As New StringBuilder()
        sb.AppendLine(Line("Stare", If(String.IsNullOrEmpty(info.Stare), "—", info.Stare)))
        sb.AppendLine(Line("IDDF", If(info.IDDF.HasValue, info.IDDF.Value.ToString(), "—")))
        sb.AppendLine(Line("Dată creare", DateOrDash(info.DataCreare)))
        sb.AppendLine(Line("Dată definitivare", DateOrDash(info.DataDefinitivare)))
        sb.AppendLine(Line("Încărcat", DaNu(info.EIncarcat)))
        sb.AppendLine(Line("Preluat", DaNu(info.EPreluat)))
        sb.AppendLine(Line("Salarii", DaNu(info.Salarii)))
        sb.AppendLine(Line("Ascuns", DaNu(info.Ascuns)))
        sb.AppendLine(Line("Surse", If(String.IsNullOrEmpty(info.Surse), "—", info.Surse)))
        sb.AppendLine()
        sb.AppendLine("── Flag-uri vedere (Are*) ──")
        sb.AppendLine(Line("Indicatori", DaNu(info.AreIndicatori)))
        sb.AppendLine(Line("Istoric", DaNu(info.AreIstoric)))
        sb.AppendLine(Line("Revizii", DaNu(info.AreRevizii)))
        sb.AppendLine(Line("Rezervări", DaNu(info.AreRezervari)))
        sb.AppendLine(Line("Partener", DaNu(info.ArePartener)))
        sb.AppendLine(Line("Recepții", DaNu(info.AreReceptii)))
        sb.AppendLine(Line("Plăți", DaNu(info.ArePlati)))
        sb.AppendLine(Line("DDF", DaNu(info.AreDDF)))
        sb.AppendLine(Line("ORD", DaNu(info.AreORD)))
        Return sb.ToString()
    End Function

    Private Shared Function Line(label As String, value As String) As String
        Return label.PadRight(LABEL_WIDTH) & ": " & value
    End Function

    Private Shared Function DaNu(b As Boolean) As String
        Return If(b, "Da", "Nu")
    End Function

    Private Shared Function DateOrDash(d As Date?) As String
        Return If(d.HasValue, d.Value.ToString("yyyy-MM-dd"), "—")
    End Function

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        Try
            ShowInfo(If(_provider IsNot Nothing, _provider(), Nothing))
        Catch ex As Exception
            GlobalErrorLog.Write("InternalInfoForm.btnRefresh_Click", ex)
        End Try
    End Sub

    ' Culorile semantice (după ThemeManager.Apply și la comutare live).
    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim scheme = ThemeManager.Current
            Dim p = scheme.Palette

            ' Fundalul formularului ESTE conturul de 1px (ca LoginForm / MainForm).
            BackColor = p.BorderColor

            lblHeader.ForeColor = p.TextColor
            lblHeader.BackColor = p.SurfaceAltColor
            txtInfo.BackColor = p.SurfaceAltColor
            txtInfo.ForeColor = p.TextColor

            ButtonStyles.ApplySecondary(btnRefresh, scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("InternalInfoForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
