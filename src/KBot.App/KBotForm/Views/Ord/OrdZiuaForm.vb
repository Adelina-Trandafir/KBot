Option Strict On
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Dialogul care cere ZIUA pentru care se genereaza o ordonantare noua (felia 0049).
'''
''' <para>In Access, data venea din randul de plata pe care statea cursorul in
''' <c>frmFX_MAIN</c>, iar <c>FX_Adaugare_ORD_Din_Plati</c> o primea ca parametru. Punctul de
''' intrare de aici e arborele vederii ORD, care nu poarta plati — deci ziua se cere explicit.
''' Un camp in plus, dar in locul unei date ghicite.</para>
''' </summary>
Public Class OrdZiuaForm

    Private ReadOnly _cod As String

    ''' <summary>Ziua aleasa de operator.</summary>
    Public ReadOnly Property Ziua As Date
        Get
            Return dtpZiua.Value.Date
        End Get
    End Property

    Public Sub New(cod As String)
        InitializeComponent()
        _cod = If(cod, String.Empty)
    End Sub

    ' Boundary UI (Load): se logheaza si se inghite — un throw ar darama deschiderea.
    Private Sub OrdZiuaForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            capBar.Text = $"K-BOT — Ordonanțare nouă · {_cod}"
            Text = capBar.Text
            dtpZiua.Value = Date.Today
        Catch ex As Exception
            GlobalErrorLog.Write("OrdZiuaForm.OrdZiuaForm_Load", ex)
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
            GlobalErrorLog.Write("OrdZiuaForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
