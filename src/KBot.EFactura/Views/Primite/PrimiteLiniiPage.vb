Option Strict On
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Slice 00EF-18 -- the view «Linii» of a received invoice: the lines the supplier wrote in the XML and, under them, the VAT per rate.
''' The page only OWNS its controls (declared in its designer file); what they show is the work of <see cref="PrimiteView"/>,
''' which holds the invoice and the server.
''' </summary>
Public Class PrimiteLiniiPage
    Implements IThemedContainer

    Public Sub New()
        InitializeComponent()
    End Sub

    ' Slice 00EF-27: a line of an invoice read with the CPV column and without a CPV code is painted in the theme's error
    ' colour (dark red); a line stored before the column existed (Cpv = Nothing) is painted as usual.
    Private Sub GridLinii_RowFormatting(sender As Object, e As KBotRowFormattingEventArgs) Handles gridLinii.RowFormatting
        Try
            Dim k_line As EFacturaPrimitaLinie = TryCast(e.Row?.Tag, EFacturaPrimitaLinie)
            If k_line IsNot Nothing AndAlso k_line.CpvLipsa Then e.ForeColor = ThemeManager.Current.Palette.ErrorColor
        Catch ex As Exception
            ' Paint boundary: the row is painted as it is.
            GlobalErrorLog.Write("PrimiteLiniiPage.GridLinii_RowFormatting", ex)
        End Try
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            BackColor = k_scheme.Palette.SurfaceAltColor
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteLiniiPage.ApplyTheme", ex)
        End Try
    End Sub

End Class
