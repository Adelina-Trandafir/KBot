Option Strict On
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Slice 00EF-09 -- the view «Conținut» of the invoice window: the lines of the invoice (the grid).
''' The page only OWNS its controls (all of them declared in its designer file); what they show and what the buttons do is the
''' window's (FacturiForm), which is where the invoice, the server and the edit mode live.
''' </summary>
Public Class VanzareContinutPage
    Implements IThemedContainer

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            Dim k_palette As ThemePalette = k_scheme.Palette
            BackColor = k_palette.SurfaceAltColor
            tlyContinut.BackColor = k_palette.SurfaceAltColor
            tlyLinii.BackColor = k_palette.SurfaceAltColor
            ButtonStyles.ApplySecondary(btnLinieNoua, k_scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("VanzareContinutPage.ApplyTheme", ex)
        End Try
    End Sub

End Class
