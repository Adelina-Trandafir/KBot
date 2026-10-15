Option Strict On
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Slice 00EF-18 / 00EF-25 -- the view «Mesaje» of a received invoice: the messages of the E-Factura system about it (the ones exchanged
''' through ANAF), and under them a box to write a new one for the supplier. The notes written in the XML are NOT here, they are in the
''' view «Observații» (<see cref="PrimiteObservatiiPage"/>). The page only OWNS its controls (declared in its designer file); what they show
''' and the sending are the work of <see cref="PrimiteView"/>, which the page tells through <see cref="SendRequested"/>.
''' </summary>
Public Class PrimiteMesajePage
    Implements IThemedContainer

    ''' <summary>The operator pressed «Trimite» with a message written (<paramref name="k_text"/> is trimmed, never empty).</summary>
    Public Event SendRequested(k_text As String)

    Public Sub New()
        InitializeComponent()
    End Sub

    ''' <summary>Empties the box of the new message (after it was sent).</summary>
    Public Sub ClearInput()
        txtMesaj.Text = String.Empty
    End Sub

    ''' <summary>The box and the button are off while a message is on its way, so it cannot be sent twice.</summary>
    Public Sub SetBusy(k_busy As Boolean)
        txtMesaj.Enabled = Not k_busy
        btnTrimite.Enabled = Not k_busy
    End Sub

    ''' <summary>UI boundary: logs and swallows.</summary>
    Private Sub BtnTrimite_Click(sender As Object, e As EventArgs) Handles btnTrimite.Click
        Try
            Dim k_text As String = txtMesaj.Text.Trim()
            If k_text.Length = 0 Then Return
            RaiseEvent SendRequested(k_text)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteMesajePage.BtnTrimite_Click", ex)
        End Try
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            BackColor = k_scheme.Palette.SurfaceAltColor
            pnlTrimite.BackColor = k_scheme.Palette.SurfaceAltColor
            lblTrimite.ForeColor = k_scheme.Palette.TextDimColor
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteMesajePage.ApplyTheme", ex)
        End Try
    End Sub

End Class
