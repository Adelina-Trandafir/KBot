Option Strict On
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Slice 00EF-09 -- the view «Cumpărător» of the invoice window: the customer of the invoice (choose, add, change, delete).
''' The page only OWNS its controls (all of them declared in its designer file); what they show and what the buttons do is the
''' window's (FacturiForm), which is where the invoice, the server and the edit mode live.
''' </summary>
Public Class VanzareCumparatorPage
    Implements IThemedContainer

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            Dim k_palette As ThemePalette = k_scheme.Palette
            BackColor = k_palette.SurfaceAltColor
            tlyClient.BackColor = k_palette.SurfaceAltColor
            tlyLoc.BackColor = k_palette.SurfaceAltColor
            lblOrasClientT.BackColor = k_palette.SurfaceAltColor
            lblSectorT.BackColor = k_palette.SurfaceAltColor
            tlyCf.BackColor = k_palette.SurfaceAltColor
            lblIndT.BackColor = k_palette.SurfaceAltColor
            tlyBtn.BackColor = k_palette.SurfaceAltColor
            For Each k_b As Button In New Button() {btnClientNou, btnClientSalveaza, btnClientSterge}
                ButtonStyles.ApplySecondary(k_b, k_scheme)
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("VanzareCumparatorPage.ApplyTheme", ex)
        End Try
    End Sub

End Class
