Option Strict On
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Slice 00EF-09 -- the view «Generale» of the invoice window: number, date, type, state, comments, order reference, total.
''' The page only OWNS its controls (all of them declared in its designer file); what they show and what the buttons do is the
''' window's (FacturiForm), which is where the invoice, the server and the edit mode live.
''' </summary>
Public Class VanzareGeneralePage
    Implements IThemedContainer

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            Dim k_palette As ThemePalette = k_scheme.Palette
            BackColor = k_palette.SurfaceAltColor
            tlyGenerale.BackColor = k_palette.SurfaceAltColor
            lblInfoFactura.ForeColor = k_palette.TextDimColor
        Catch ex As Exception
            GlobalErrorLog.Write("VanzareGeneralePage.ApplyTheme", ex)
        End Try
    End Sub

End Class
