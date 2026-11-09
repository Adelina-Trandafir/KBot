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
            BackColor = k_scheme.PanelBack()
            tlyClient.BackColor = k_scheme.PanelBack()
            tlyLoc.BackColor = k_scheme.PanelBack()
            lblOrasClientT.BackColor = k_scheme.PanelBack()
            lblSectorT.BackColor = k_scheme.PanelBack()
            tlyCf.BackColor = k_scheme.PanelBack()
            lblIndT.BackColor = k_scheme.PanelBack()
            tlyBtn.BackColor = k_scheme.PanelBack()
            For Each k_b As Button In New Button() {btnClientNou, btnClientSalveaza, btnClientSterge}
                ButtonStyles.ApplySecondary(k_b, k_scheme)
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("VanzareCumparatorPage.ApplyTheme", ex)
        End Try
    End Sub

End Class
