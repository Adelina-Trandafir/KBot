Option Strict On
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Slice 00EF-18 -- the view «Atașamente» of a received invoice: the files embedded in the XML. The page only OWNS its controls
''' (declared in its designer file); what they show is the work of <see cref="PrimiteView"/>.
''' </summary>
Public Class PrimiteAtasamentePage
    Implements IThemedContainer

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            BackColor = k_scheme.Palette.SurfaceAltColor
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteAtasamentePage.ApplyTheme", ex)
        End Try
    End Sub

End Class
