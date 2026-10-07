Option Strict On
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Slice 00EF-09 -- the view «Atașamente» of the invoice window: whether the original invoice goes to ANAF as an attachment.
''' The page only OWNS its controls (all of them declared in its designer file); what they show and what the buttons do is the
''' window's (FacturiForm), which is where the invoice, the server and the edit mode live.
''' </summary>
Public Class VanzareAtasamentePage
    Implements IThemedContainer

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            Dim k_palette As ThemePalette = k_scheme.Palette
            BackColor = k_palette.SurfaceAltColor
            tlyAtasamente.BackColor = k_palette.SurfaceAltColor
            lblAtasHint.ForeColor = k_palette.TextDimColor
        Catch ex As Exception
            GlobalErrorLog.Write("VanzareAtasamentePage.ApplyTheme", ex)
        End Try
    End Sub

End Class
