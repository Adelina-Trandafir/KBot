Option Strict On
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Dialogul care cere TEXTUL unui document justificativ (felia 0049, pasul 0049-02).
'''
''' <para>Pe randul sintetic «&lt; TOTI BENEFICIARII &gt;» grila e doar-citire: un rand de acolo
''' sta pe cate o copie la fiecare beneficiar, deci nu se poate scrie in el direct. Textul se
''' cere aici si abia apoi se imparte la toti. Pe un beneficiar anume grila ramane editabila,
''' dar randul se naste tot completat — un rand gol adaugat in grila nu se deosebea de unul
''' uitat, iar de la <c>btnSav</c> incolo un document fara text opreste salvarea.</para>
'''
''' <para><b>De ce nu are <c>AcceptButton</c>:</b> campul e multilinie, deci Enter trebuie sa
''' treaca la randul urmator, nu sa inchida dialogul.</para>
''' </summary>
Public Class OrdTextForm

    Private ReadOnly _pentruToti As Boolean

    ''' <summary>Textul scris de operator, fara spatiile de la capete.</summary>
    Public ReadOnly Property Textul As String
        Get
            Return txtDoc.Text.Trim()
        End Get
    End Property

    ''' <param name="pentruToti">True cand randul se adauga tuturor beneficiarilor (randul
    ''' sintetic) — atunci se si spune asta, fiindca urmarile sunt altele.</param>
    Public Sub New(pentruToti As Boolean)
        InitializeComponent()
        _pentruToti = pentruToti
    End Sub

    ' Boundary UI (Load): se logheaza si se inghite — un throw ar darama deschiderea.
    Private Sub OrdTextForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            If _pentruToti Then
                lblIntro.Text = "Scrieți textul documentului justificativ. Se adaugă la TOȚI " &
                                "beneficiarii ordonanțării, câte o copie fiecăruia."
            Else
                lblIntro.Text = "Scrieți textul documentului justificativ pentru beneficiarul selectat."
            End If
            ActiveControl = txtDoc
        Catch ex As Exception
            GlobalErrorLog.Write("OrdTextForm.OrdTextForm_Load", ex)
        End Try
    End Sub

    Private Sub BtnOk_Click(sender As Object, e As EventArgs) Handles btnOk.Click
        Try
            If Textul = "" Then
                MessageBox.Show(Me, "Scrieți textul documentului justificativ.",
                                "K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Information)
                ActiveControl = txtDoc
                Return
            End If
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("OrdTextForm.BtnOk_Click", ex)
        End Try
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim scheme As ThemeScheme = ThemeManager.Current
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            tlyMain.BackColor = p.SurfaceAltColor
            tlySubsol.BackColor = p.SurfaceAltColor
            lblIntro.ForeColor = p.TextDimColor
            lblIntro.BackColor = Color.Transparent
        Catch ex As Exception
            GlobalErrorLog.Write("OrdTextForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
