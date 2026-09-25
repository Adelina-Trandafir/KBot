Option Strict On
Imports System.Security.Cryptography
Imports System.Text
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Asks for the password that switches the advanced settings pages on
''' (<see cref="AppSettings.AdvancedOptions"/>, operator, 24.09.2026). A wrong password keeps the
''' dialog open with a red line under the box; «Renunta» / Escape gives DialogResult.Cancel.
'''
''' <para>Only the SHA-256 of the password is in the build (<see cref="PasswordHash"/>), so the
''' text itself cannot be read out of the exe with a string search. It is a gate against an
''' operator wandering into pages they should not touch, not a security boundary: the setting
''' itself is a plain value in app_settings.json.</para>
''' </summary>
Public Class ParolaAvansataForm

    ''' <summary>SHA-256 (hex, lower case) of the advanced options password, UTF-8.</summary>
    Friend Const PasswordHash As String = "de10fa91101139e1273a2ad826928f41f97c933036950e97ea5c4f76c6a56b05"

    ''' <summary>True when <paramref name="text"/> is the password (case-sensitive, exact).</summary>
    Public Shared Function IsPassword(text As String) As Boolean
        If String.IsNullOrEmpty(text) Then Return False
        Dim hash As Byte() = SHA256.HashData(Encoding.UTF8.GetBytes(text))
        Return String.Equals(Convert.ToHexString(hash), PasswordHash, StringComparison.OrdinalIgnoreCase)
    End Function

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub ParolaAvansataForm_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        Try
            txtParola.Focus()
        Catch ex As Exception
            GlobalErrorLog.Write("ParolaAvansataForm.ParolaAvansataForm_Shown", ex)
        End Try
    End Sub

    Private Sub BtnConfirma_Click(sender As Object, e As EventArgs) Handles btnConfirma.Click
        Try
            If IsPassword(txtParola.Text) Then
                DialogResult = DialogResult.OK
                Close()
                Return
            End If
            lblEroare.Text = "Parola nu este corectă."
            txtParola.Text = String.Empty
            txtParola.Focus()
        Catch ex As Exception
            GlobalErrorLog.Write("ParolaAvansataForm.BtnConfirma_Click", ex)
        End Try
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim scheme As ThemeScheme = ThemeManager.Current
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            tlyMain.BackColor = p.SurfaceAltColor
            tlyCampuri.BackColor = p.SurfaceAltColor
            tlySubsol.BackColor = p.SurfaceAltColor
            For Each caption As Label In New Label() {lblIntro, lblParola}
                caption.ForeColor = p.TextDimColor
                caption.BackColor = Color.Transparent
            Next
            lblEroare.ForeColor = p.ErrorColor
            lblEroare.BackColor = Color.Transparent
            ButtonStyles.ApplySecondary(btnRenunta, scheme)
            ButtonStyles.ApplyPrimary(btnConfirma, scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("ParolaAvansataForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
