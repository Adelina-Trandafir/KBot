Option Strict On
Imports KBot.Common
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

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            BackColor = k_scheme.Palette.SurfaceAltColor
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteLiniiPage.ApplyTheme", ex)
        End Try
    End Sub

End Class
